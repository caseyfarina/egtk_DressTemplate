using UnityEngine;
using UnityEngine.UI;

public static class TestButtons
{
    // Reset to index 0, then click Prev (should wrap to last index)
    public static void Execute()
    {
        CharacterCreator creator = GameObject.FindFirstObjectByType<CharacterCreator>();
        if (creator == null) { Debug.LogError("[Test] CharacterCreator not found"); return; }

        // Force back to index 0 first
        creator.SelectBodyType(0);
        creator.SelectEyes(0);
        Debug.Log("[Test] Reset to index 0");

        // Now click Prev — should wrap to last
        ClickPrev("[Row_Skin Tone]");
        ClickPrev("[Row_Eyes]");
        Debug.Log("[Test] Clicked Prev from 0 — should show last index");
    }

    private static void ClickPrev(string rowName)
    {
        GameObject row = GameObject.Find(rowName);
        if (row == null) { Debug.LogError($"[Test] Row not found: {rowName}"); return; }
        Button btn = row.transform.Find("BtnPrev")?.GetComponent<Button>();
        if (btn == null) { Debug.LogError($"[Test] BtnPrev not found in {rowName}"); return; }
        btn.onClick.Invoke();
        Debug.Log($"[Test] Invoked Prev on {rowName}");
    }
}
