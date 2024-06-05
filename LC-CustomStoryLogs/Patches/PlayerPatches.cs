using System;
using System.Collections.Generic;
using CustomStoryLogs.Tools;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CustomStoryLogs.Patches;


[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatch
{
    [HarmonyPatch("ConnectClientToPlayerObject")]
    [HarmonyPostfix]
    public static void ConnectClientToPlayerObjectPatch(ref PlayerControllerB __instance)
    {
        if(__instance.IsOwner)
        {
            __instance.gameObject.AddComponent<LogPlacementTool>();
        }
    }
}
