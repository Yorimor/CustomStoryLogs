using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
public class GameNetPatches
{
    [HarmonyPatch("SaveGameValues")]
    [HarmonyPostfix]
    private static void SaveUnlockedLogs(GameNetworkManager __instance)
    {
        if (!__instance.isHostingGame) return;

        try
        {
            ES3.Save(CustomStoryLogs.UnlockedSaveKey, CustomStoryLogs.UnlockedNetVar.Value, __instance.currentSaveFileName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while trying to save unlocked custom logs: {ex}");
        }
    }
    
    [HarmonyPatch("ResetSavedGameValues")]
    [HarmonyPostfix]
    private static void ResetUnlockedLogs(GameNetworkManager __instance)
    {
        if (!__instance.isHostingGame) return;
        
        ES3.Save(CustomStoryLogs.UnlockedSaveKey, new List<int>(), __instance.currentSaveFileName);
    }
}