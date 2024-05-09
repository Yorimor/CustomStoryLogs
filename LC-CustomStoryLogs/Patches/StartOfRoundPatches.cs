using System.Linq;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatches
{
    [HarmonyPatch("StartGame")]
    [HarmonyPostfix]
    private static void AddInLevelStoryLogs(StartOfRound __instance)
    {
        string planetName = __instance.currentLevel.PlanetName;

        if (CustomStoryLogs.RegisteredCollectables.ContainsKey(planetName) && __instance.IsHost)
        {
            CustomStoryLogs.SpawnLogsServer.SendAllClients(planetName);
        }
    }

    [HarmonyPatch("ShipLeave")]
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