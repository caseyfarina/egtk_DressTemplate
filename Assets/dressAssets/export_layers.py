# Style Em — Krita Batch Layer Exporter
# How to run: in Krita, go to Tools > Scripts > Run Script and select this file
# OR paste into Tools > Scripter and click Run.
#
# Exports every clothing/hair/body item as cropped PNGs with embedded
# canvas-coordinate offsets in the filename:
#   {itemName}_color{n}_x{X}_y{Y}.png
#
# Requires: Krita 5+ (built-in Python + PyQt5). No external libraries.

import os
import re
from krita import Krita
from PyQt5.QtCore import QRect
from PyQt5.QtWidgets import QFileDialog, QApplication

# ---------------------------------------------------------------------------
# Configuration — UNITY_ROOT is chosen at runtime via folder dialogue
# ---------------------------------------------------------------------------

UNITY_ROOT = None  # set by run_export() via folder picker
CANVAS_W, CANVAS_H = 2048, 2048

# Mapping: (top-level group name, optional sub-group name) -> relative output path
# Keys use lowercase for case-insensitive matching.
# Sub-group key of "" means "match the top-level group directly".
FOLDER_MAP = {
    ("clothes", "outwear"):      "Assets/Art/Clothing/Outerwear",
    ("clothes", "tops"):         "Assets/Art/Clothing/Tops",
    ("clothes", "inners"):       "Assets/Art/Clothing/Tops",
    ("clothes", "trousers"):     "Assets/Art/Clothing/Bottoms",
    ("clothes", "skirts"):       "Assets/Art/Clothing/Skirts",
    ("clothes", "dresses"):      "Assets/Art/Clothing/Dresses",
    ("clothes", "shoes"):        "Assets/Art/Clothing/Shoes",
    ("clothes", "socks"):        "Assets/Art/Clothing/SocksLeggings",
    ("clothes", "accessories"):  "Assets/Art/Clothing/Accessories",
    ("clothes", "hats"):         "Assets/Art/Clothing/Hats",
    ("hair",    "front"):        "Assets/Art/Hair/Front",
    ("hair",    "back"):         "Assets/Art/Hair/Back",
    ("brows",   "brows"):             "Assets/Art/FacialFeatures/Eyebrows",
    ("body",    "eyes"):         "Assets/Art/FacialFeatures/Eyes",
    ("body",    "ears"):         "Assets/Art/FacialFeatures/Ears",
    ("body",    "mouths"):       "Assets/Art/FacialFeatures/Mouths",
    ("body",    "nose"):         "Assets/Art/FacialFeatures/Nose",
    # body/bases and body/skinToen are handled by export_body_types() — NOT in this map
}

# Top-level group names to skip entirely (lowercase).
SKIP_TOP_LEVEL = {"body"}   # body is handled separately by export_body_types()
SKIP_SUB_GROUPS = set()  # nothing to skip via the generic walker

# ---------------------------------------------------------------------------
# Visibility helpers
# ---------------------------------------------------------------------------

def _collect_visibility(node, state: dict):
    """Recursively record current visibility of every node in the subtree."""
    state[node.uniqueId()] = node.visible()
    for child in node.childNodes():
        _collect_visibility(child, state)


def _restore_visibility(node, state: dict):
    """Recursively restore visibility from a previously saved state dict."""
    uid = node.uniqueId()
    if uid in state:
        node.setVisible(state[uid])
    for child in node.childNodes():
        _restore_visibility(child, state)


def _hide_tree(node):
    """Recursively hide every node in the subtree (children first)."""
    for child in node.childNodes():
        _hide_tree(child)
    node.setVisible(False)


def _show_node_and_ancestors(node, root_node):
    """
    Make *node* visible and ensure every ancestor up to (but not including)
    the document root is also visible so the node actually renders.
    """
    node.setVisible(True)
    parent = node.parentNode()
    # parentNode() returns None at the document root level
    while parent is not None and parent != root_node:
        parent.setVisible(True)
        parent = parent.parentNode()


# ---------------------------------------------------------------------------
# Bounds helpers
# ---------------------------------------------------------------------------

def _union_bounds(rects):
    """Return the union QRect of a list of QRects, ignoring empty/null ones."""
    valid = [r for r in rects if not r.isEmpty()]
    if not valid:
        return QRect()
    result = valid[0]
    for r in valid[1:]:
        result = result.united(r)
    return result


def _clamp_rect(rect):
    """Clamp a QRect to the canvas dimensions."""
    x = max(0, rect.x())
    y = max(0, rect.y())
    x2 = min(CANVAS_W, rect.right() + 1)
    y2 = min(CANVAS_H, rect.bottom() + 1)
    if x2 <= x or y2 <= y:
        return QRect()
    return QRect(x, y, x2 - x, y2 - y)


# ---------------------------------------------------------------------------
# Export primitives
# ---------------------------------------------------------------------------

def _export_qimage(doc, bounds, out_path):
    """
    Project the merged visible canvas within *bounds* and save as PNG.
    Returns True on success, False if bounds are empty or save fails.
    """
    if bounds.isEmpty():
        return False
    b = _clamp_rect(bounds)
    if b.isEmpty():
        return False
    img = doc.projection(b.x(), b.y(), b.width(), b.height())
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    ok = img.save(out_path, "PNG")
    if ok:
        print(f"  Saved: {out_path}")
    else:
        print(f"  ERROR saving: {out_path}")
    return ok


def _build_filename(item_name, color_index, bounds):
    """
    Construct the output filename following the convention:
      {itemName}_color{n}_x{X}_y{Y}.png
    """
    return f"{item_name}_color{color_index}_x{bounds.x()}_y{bounds.y()}.png"


# ---------------------------------------------------------------------------
# Layer-naming convention helpers
# ---------------------------------------------------------------------------

# Patterns for _line and _color / _Color layers
_LINE_RE  = re.compile(r'_line$', re.IGNORECASE)
_COLOR_RE = re.compile(r'[_]?[Cc]olor(\d+)$')


def _is_line_layer(name):
    return bool(_LINE_RE.search(name))


def _color_index(name):
    """Return the integer color index if name ends with color{n}, else None."""
    m = _COLOR_RE.search(name)
    return int(m.group(1)) if m else None


def _paint_layers(group_node):
    """Return direct child paint layers of a group (type == 'paintlayer')."""
    return [c for c in group_node.childNodes() if c.type() == 'paintlayer']


# ---------------------------------------------------------------------------
# Core export logic
# ---------------------------------------------------------------------------

def export_item_group(doc, root_node, item_group, out_dir, visibility_state):
    """
    Export one item group (e.g. beret1/, eye1/, thighHighs1/).

    Convention inside the group:
      - Layers whose name ends with _line  → linework (shared across all variants)
      - Layers whose name ends with _color{n} or _Color{n} → nth color variant
      - All other paint layers → treated as a single color variant (index 1)

    For each color variant we:
      1. Hide the entire document tree.
      2. Show the line layer (if present) + the color layer.
      3. Compute union bounds of their node.bounds().
      4. Project + save PNG.
      5. Restore visibility from saved state.
    """
    children = item_group.childNodes()
    if not children:
        return

    item_name = item_group.name().strip()
    line_layers = [c for c in children if _is_line_layer(c.name())]
    color_layers = {}  # index -> node
    plain_layers  = []  # layers with no special suffix

    for child in children:
        if _is_line_layer(child.name()):
            continue  # already captured
        idx = _color_index(child.name())
        if idx is not None:
            color_layers[idx] = child
        else:
            plain_layers.append(child)

    def _do_export(color_idx, layers_to_show):
        """Hide tree, show chosen layers, compute bounds, export."""
        # 1. Hide everything
        _hide_tree(root_node)

        # 2. Show chosen layers + their ancestors
        for layer in layers_to_show:
            _show_node_and_ancestors(layer, root_node)

        # 3. Refresh so projection picks up visibility changes
        doc.refreshProjection()

        # 4. Compute bounds as union of individual layer bounds
        all_bounds = [l.bounds() for l in layers_to_show]
        bounds = _union_bounds(all_bounds)
        if bounds.isEmpty():
            print(f"  SKIP (empty bounds): {item_name} color{color_idx}")
            _restore_visibility(root_node, visibility_state)
            return

        # 5. Build path and export
        fname = _build_filename(item_name, color_idx, bounds)
        out_path = os.path.join(out_dir, fname)
        _export_qimage(doc, bounds, out_path)

        # 6. Restore original visibility
        _restore_visibility(root_node, visibility_state)

    if color_layers:
        for idx in sorted(color_layers.keys()):
            layers_to_show = line_layers + [color_layers[idx]]
            _do_export(idx, layers_to_show)
    elif plain_layers:
        # No color suffix — export each plain layer as a separate color variant
        # OR if there is only one, export as color1.
        for i, plain in enumerate(plain_layers, start=1):
            layers_to_show = line_layers + [plain]
            _do_export(i, layers_to_show)
    elif line_layers:
        # Only a line layer exists — export it alone as color1
        _do_export(1, line_layers)


def export_plain_paint_layer(doc, root_node, layer, out_dir, visibility_state):
    """
    Export a bare paint layer (not inside an item sub-group) as a single
    color variant (color1).
    """
    item_name = layer.name().strip()

    _hide_tree(root_node)
    _show_node_and_ancestors(layer, root_node)
    doc.refreshProjection()

    bounds = layer.bounds()
    if bounds.isEmpty():
        print(f"  SKIP (empty bounds): {item_name}")
        _restore_visibility(root_node, visibility_state)
        return

    fname = _build_filename(item_name, 1, bounds)
    out_path = os.path.join(out_dir, fname)
    _export_qimage(doc, bounds, out_path)
    _restore_visibility(root_node, visibility_state)


# ---------------------------------------------------------------------------
# Tree walker
# ---------------------------------------------------------------------------

def _norm(name):
    """Normalise a layer name for map lookups (strip, lowercase)."""
    return name.strip().lower()


def walk_category_group(doc, root_node, category_group, out_dir, visibility_state):
    """
    Walk one category group (e.g. Tops/, Dresses/, beret1/) and export all
    item sub-groups or plain paint layers found inside.

    Items are recognised as:
      - A group node whose children are paint layers (= leaf item group)
      - A paint layer directly inside a category group (= plain item)
    """
    for child in category_group.childNodes():
        ctype = child.type()

        if ctype == 'grouplayer':
            # Check if this group contains further sub-groups (nested categories
            # like Dresses/Jumpsuit1/) or is itself a leaf item group.
            sub_children = child.childNodes()
            has_sub_groups = any(c.type() == 'grouplayer' for c in sub_children)

            if has_sub_groups:
                # Recurse: this is a sub-category (e.g. Dresses/, thighHighs/)
                walk_category_group(doc, root_node, child, out_dir, visibility_state)
            else:
                # Leaf item group — export it
                print(f"  Exporting item group: {child.name()}")
                export_item_group(doc, root_node, child, out_dir, visibility_state)

        elif ctype == 'paintlayer':
            # Bare paint layer directly in a category group
            print(f"  Exporting plain layer: {child.name()}")
            export_plain_paint_layer(doc, root_node, child, out_dir, visibility_state)


def export_body_types(doc, root_node, body_group, out_dir, visibility_state):
    """
    Special handler for the body group.

    Layer structure expected inside body_group:
      bases/      — sub-group containing the outline layer(s) (base1, base2 …)
                    Each child may be a paint layer or a group whose first
                    paint layer is the outline.
      skinToen/   — sub-group (spelling varies) containing skin-tone fills:
                    skin1, skin2, skin3 …

    For every base outline × every skin tone we:
      1. Hide the whole document.
      2. Show the base outline layer + the skin-tone fill layer.
      3. Compute union bounds, export as:
            {base_name}_color{skin_index}_x{X}_y{Y}.png
    """
    sub = {_norm(n.name()): n for n in body_group.childNodes()}

    # --- locate bases group ------------------------------------------------
    bases_node = sub.get("bases")
    if bases_node is None:
        print("WARNING [export_body_types] 'bases' sub-group not found. Skipping body export.")
        return

    # --- locate skin-tone group (handles typo "skintoen" or "skintone") ----
    skin_group = None
    for key in sub:
        if key.startswith("skin"):
            skin_group = sub[key]
            break
    if skin_group is None:
        print("WARNING [export_body_types] skin tone sub-group not found. Skipping body export.")
        return

    # --- collect skin-tone layers, sorted by their number ------------------
    _SKIN_RE = re.compile(r'skin\s*(\d+)', re.IGNORECASE)
    skin_layers = []
    for child in skin_group.childNodes():
        m = _SKIN_RE.search(child.name())
        if m:
            skin_layers.append((int(m.group(1)), child))
    skin_layers.sort(key=lambda x: x[0])

    if not skin_layers:
        print("WARNING [export_body_types] No skin layers (skin1/skin2/…) found.")
        return

    print(f"  Body: found {len(skin_layers)} skin tone(s): "
          + ", ".join(n.name() for _, n in skin_layers))

    # --- for each base outline, export combined with each skin tone ---------
    for base_child in bases_node.childNodes():
        base_name = base_child.name().strip()

        # Resolve the actual paint-layer(s) that represent the outline.
        # If base_child is a group, grab all its paint layers; otherwise use it directly.
        if base_child.type() == 'grouplayer':
            outline_layers = _paint_layers(base_child)
            if not outline_layers:
                print(f"  SKIP (empty group): {base_name}")
                continue
        else:
            outline_layers = [base_child]

        print(f"\n  Exporting body base: {base_name}")
        for skin_idx, skin_layer in skin_layers:
            layers_to_show = outline_layers + [skin_layer]

            # 1. Hide everything
            _hide_tree(root_node)

            # 2. Show outline + skin tone
            for layer in layers_to_show:
                _show_node_and_ancestors(layer, root_node)

            # 3. Refresh
            doc.refreshProjection()

            # 4. Bounds = union of all shown layers
            bounds = _union_bounds([l.bounds() for l in layers_to_show])
            if bounds.isEmpty():
                print(f"  SKIP (empty bounds): {base_name} skin{skin_idx}")
                _restore_visibility(root_node, visibility_state)
                continue

            # 5. Export
            fname = _build_filename(base_name, skin_idx, bounds)
            out_path = os.path.join(out_dir, fname)
            _export_qimage(doc, bounds, out_path)

            # 6. Restore
            _restore_visibility(root_node, visibility_state)


def run_export():
    """Entry point: walk the top-level layer structure and export everything."""
    global UNITY_ROOT

    # Prompt user to select the Unity project root folder
    chosen = QFileDialog.getExistingDirectory(
        None,
        "Select Unity Project Root (the folder containing Assets/)",
        "",
        QFileDialog.ShowDirsOnly | QFileDialog.DontResolveSymlinks
    )
    if not chosen:
        print("Export cancelled — no folder selected.")
        return
    UNITY_ROOT = chosen
    print(f"Exporting to: {UNITY_ROOT}")

    app = Krita.instance()
    doc = app.activeDocument()

    if doc is None:
        print("ERROR: No active document. Open mainBase.kra first.")
        return

    root_node = doc.rootNode()

    # Save the entire document visibility state upfront so we can restore it
    # after every individual export.
    visibility_state = {}
    _collect_visibility(root_node, visibility_state)

    top_level = {_norm(n.name()): n for n in root_node.childNodes()}

    # -------------------------------------------------------------------
    # Process each mapping entry
    # -------------------------------------------------------------------
    # We group FOLDER_MAP entries by (top_key, sub_key) and resolve the
    # actual nodes, then dispatch to the walker.

    processed_sub = set()   # track (top_key, sub_key) already handled

    for (top_key, sub_key), rel_out in FOLDER_MAP.items():
        out_dir = os.path.join(UNITY_ROOT, rel_out)

        top_node = top_level.get(top_key)
        if top_node is None:
            # Try case-insensitive search
            for name, node in top_level.items():
                if name == top_key:
                    top_node = node
                    break
            if top_node is None:
                print(f"WARNING: Top-level group '{top_key}' not found. Skipping.")
                continue

        if sub_key == "":
            # The top-level group IS the category (e.g. brows/)
            print(f"\n=== Exporting: {top_node.name()} -> {rel_out} ===")
            walk_category_group(doc, root_node, top_node, out_dir, visibility_state)
        else:
            # Find the sub-group inside the top-level group
            sub_nodes = {_norm(n.name()): n for n in top_node.childNodes()}
            sub_node = sub_nodes.get(sub_key)
            if sub_node is None:
                print(f"WARNING: Sub-group '{sub_key}' not found inside '{top_key}'. Skipping.")
                continue

            pair = (top_key, sub_key)
            if pair in SKIP_SUB_GROUPS:
                print(f"  Skipping {top_key}/{sub_key} (intentional skip).")
                continue

            if pair in processed_sub:
                continue  # e.g. Tops and Inners both map to Clothing/Tops — already walked
            # Mark all pairs that share this output directory so we don't double-export
            # Actually we WANT to export both Tops and Inners into the same folder —
            # just don't process the same (top, sub) pair twice.
            processed_sub.add(pair)

            print(f"\n=== Exporting: {top_node.name()}/{sub_node.name()} -> {rel_out} ===")
            walk_category_group(doc, root_node, sub_node, out_dir, visibility_state)

    # --- Special: body types (outline + skin tone compositing) ---------------
    body_node = top_level.get("body")
    if body_node is not None:
        body_out = os.path.join(UNITY_ROOT, "Assets/Art/BodyTypes")
        print(f"\n=== Exporting: body types (outline + skin compositing) -> Assets/Art/BodyTypes ===")
        export_body_types(doc, root_node, body_node, body_out, visibility_state)
    else:
        print("WARNING: 'body' top-level group not found — skipping body type export.")

    # Final restore of original visibility
    _restore_visibility(root_node, visibility_state)
    doc.refreshProjection()
    print("\nExport complete.")


# ---------------------------------------------------------------------------
# Run immediately when executed via Scripter or Run Script
# ---------------------------------------------------------------------------
run_export()
