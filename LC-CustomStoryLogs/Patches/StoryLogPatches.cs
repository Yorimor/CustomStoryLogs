using HarmonyLib;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(StoryLog))]
public class StoryLogPatches
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void HideLogPatch(StoryLog __instance)
    {
        if (CustomStoryLogs.HideVanillaLogs.Value)
        {
            CustomStoryLogs.Logger.LogDebug($"Hiding vanilla story log [{__instance.storyLogID}]");
            __instance.RemoveLogCollectible();
        }
    }
}