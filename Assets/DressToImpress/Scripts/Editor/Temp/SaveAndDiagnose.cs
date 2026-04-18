using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SaveAndDiagnose
{
    public static void Execute()
    {
        // Save to the correct original path
        Scene active = SceneManager.GetActiveScene();
        Debug.Log($"[Diagnose] Active scene: {active.name} path: {active.path}");

        bool saved = EditorSceneManager.SaveScene(active, active.path);
        Debug.Log($"[Diagnose] Scene saved: {saved}");

        // Check CharacterRoot child layers
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot == null) { Debug.LogError("[Diagnose] [CharacterRoot] not found."); return; }

        Debug.Log($"[Diagnose] [CharacterRoot] position: {charRoot.transform.position}");
        Debug.Log($"[Diagnose] [CharacterRoot] child count: {charRoot.transform.childCount}");

        for (int i = 0; i < charRoot.transform.childCount; i++)
        {
            Transform child = charRoot.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
                Debug.Log($"[Diagnose]   Layer '{child.name}': sprite={(sr.sprite ? sr.sprite.name : "NULL")} pos={child.localPosition}");
        }

        // Check CharacterDisplay arrays
        CharacterDisplay cd = charRoot.GetComponent<CharacterDisplay>();
        if (cd != null)
        {
            SerializedObject so = new SerializedObject(cd);
            var bodyProp = so.FindProperty("bodyTypes");
            Debug.Log($"[Diagnose] CharacterDisplay bodyTypes array size: {bodyProp?.arraySize}");
        }

        // Check Camera
        Camera[] cams = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in cams)
            Debug.Log($"[Diagnose] Camera: '{cam.gameObject.name}' ortho={cam.orthographic} size={cam.orthographicSize} depth={cam.depth} pos={cam.transform.position}");
    }
}
