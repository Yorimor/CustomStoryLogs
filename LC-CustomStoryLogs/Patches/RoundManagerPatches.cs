using HarmonyLib;

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
            CustomStoryLogs.SpawnLogsServer.SendAllClients(planetName);
        }
    }
}