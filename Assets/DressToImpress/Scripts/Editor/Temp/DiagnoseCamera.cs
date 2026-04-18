using UnityEngine;

public static class DiagnoseCamera
{
    public static void Execute()
    {
        Camera[] cams = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"[Cam] Total cameras: {cams.Length}");
        foreach (var cam in cams)
        {
            Debug.Log($"[Cam] '{cam.gameObject.name}': pos={cam.transform.position} " +
                $"ortho={cam.orthographic} size={cam.orthographicSize} depth={cam.depth} " +
                $"active={cam.gameObject.activeInHierarchy} cullingMask={cam.cullingMask}");
        }

        // Check what's visible to the main camera
        Camera main = Camera.main;
        if (main != null)
        {
            float halfH = main.orthographicSize;
            float halfW = halfH * main.aspect;
            Vector3 p = main.transform.position;
            Debug.Log($"[Cam] Main cam visible world rect: X[{p.x-halfW:F2}..{p.x+halfW:F2}] Y[{p.y-halfH:F2}..{p.y+halfH:F2}]");
        }

        // Check CharacterRoot position relative to camera
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot != null)
        {
            Debug.Log($"[Cam] CharacterRoot world pos: {charRoot.transform.position} scale: {charRoot.transform.localScale}");
            // Log each sprite renderer's world position
            SpriteRenderer[] srs = charRoot.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in srs)
            {
                if (sr.sprite == null) continue;
                Debug.Log($"[Cam]   SR '{sr.gameObject.name}': worldPos={sr.transform.position} bounds={sr.bounds.min}..{sr.bounds.max}");
            }
        }
    }
}
