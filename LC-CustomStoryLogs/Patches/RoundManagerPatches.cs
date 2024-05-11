using HarmonyLib;
using UnityEngine.Yoga;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatches
{
    [HarmonyPatch("LoadNewLevel")]
    [HarmonyPostfix]
    private static void AddInLevelStoryLogs(RoundManager __instance, int randomSeed, SelectableLevel newLevel)
    {
        string planetName = newLevel.PlanetName;

        if (CustomStoryLogs.RegisteredCollectables.ContainsKey(planetName) && __instance.IsHost)
        {
            CustomStoryLogs.Logger.LogDebug("Telling clients to despawn log objects");
            CustomStoryLogs.SpawnLogsServer.SendAllClients(planetName);
        }
    }
}