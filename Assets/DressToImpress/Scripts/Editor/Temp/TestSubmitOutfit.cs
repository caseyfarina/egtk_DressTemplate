using UnityEngine;

public static class TestSubmitOutfit
{
    public static void Execute()
    {
        StylingRoomManager srm = GameObject.FindFirstObjectByType<StylingRoomManager>();
        if (srm == null) { Debug.LogError("[Test] StylingRoomManager not found"); return; }

        OutfitScorer scorer = GameObject.FindFirstObjectByType<OutfitScorer>();
        if (scorer != null)
            Debug.Log($"[Test] PreviewScore before submit: {scorer.PreviewScore()}");

        Debug.Log("[Test] Calling OnSubmitOutfit()...");
        srm.OnSubmitOutfit();
        Debug.Log("[Test] OnSubmitOutfit() returned.");
    }
}
