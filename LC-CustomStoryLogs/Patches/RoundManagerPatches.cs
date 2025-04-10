using HarmonyLib;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatches
{
    [HarmonyPatch("LoadNewLevel")]
    [HarmonyPostfix]
    private static void AddInLevelStoryLogs(RoundManager __instance, int randomSeed, SelectableLevel newLevel)
    {
        if (__instance.IsHost)
        {
            LoadLogsForLevel(newLevel.PlanetName);
            LoadLogsForLevel(newLevel.sceneName);
        }
    }

    private static void LoadLogsForLevel(string planetName)
    {
        if (CustomStoryLogs.PlanetCollectables.ContainsKey(planetName))
        {
            CustomStoryLogs.Logger.LogDebug("Telling clients to spawn log objects");
            CustomStoryLogs.SpawnLogs.SendClients(planetName);
        }
    }
}