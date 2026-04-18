using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class FixCharacterPosition
{
    // The body sprite center is at local (-4.08, 3.86) relative to CharacterRoot.
    // Camera orthographicSize=5.5, centered at world (0,0,-10) → visible Y [-5.5..5.5].
    // To center the body at world (0, 0): CharacterRoot = (+4.08, -3.86, 0).
    public static void Execute()
    {
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot == null) { Debug.LogError("[FixPos] [CharacterRoot] not found"); return; }

        // Shift CharacterRoot so body center lands at world origin
        // Body local center: (-4.08, 3.86) → CharacterRoot world offset to cancel:
        charRoot.transform.position = new Vector3(4.08f, -3.86f, 0f);
        Debug.Log($"[FixPos] CharacterRoot moved to {charRoot.transform.position}");

        // Verify body position
        Transform bodyLayer = charRoot.transform.Find("Layer_BodyBase");
        if (bodyLayer != null)
            Debug.Log($"[FixPos] Body world pos now: {bodyLayer.position}");

        // Save scene
        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scene.path);
        Debug.Log("[FixPos] Scene saved.");
    }
}
