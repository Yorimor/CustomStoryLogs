using HarmonyLib;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatches
{

    [HarmonyPatch("ShipHasLeft")]
    [HarmonyPostfix]
    private static void CleanupLevel(StartOfRound __instance)
    {
        string planetName = __instance.currentLevel.PlanetName;

        if (CustomStoryLogs.RegisteredCollectables.ContainsKey(planetName))
        {
            CustomStoryLogs.DespawnLogsLocally(planetName);
        }
    }
}