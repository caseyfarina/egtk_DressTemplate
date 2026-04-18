using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

public static class SaveScene
{
    public static void Execute()
    {
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log($"[Save] path={scene.path}");
        EditorSceneManager.SaveScene(scene, scene.path);
        Debug.Log("[Save] Done.");
    }
}
