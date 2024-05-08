using HarmonyLib;
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
        CustomStoryLogs.Logger.LogInfo(planetName);

        if (CustomStoryLogs.RegisteredCollectables.ContainsKey(planetName))
        {
            CustomStoryLogs.Logger.LogInfo(CustomStoryLogs.RegisteredCollectables[planetName]);
            foreach (CollectableData collectableData in CustomStoryLogs.RegisteredCollectables[planetName])
            {
                CustomStoryLogs.Logger.LogInfo(collectableData.LogID);
                GameObject obj = GameObject.Instantiate(CustomStoryLogs.CustomLogObj);
                obj.transform.position = collectableData.Position;
            }
        }
    }
}