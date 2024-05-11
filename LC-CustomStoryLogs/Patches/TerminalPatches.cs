using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace CustomStoryLogs.Patches;

[HarmonyPatch(typeof(Terminal))]
public class TerminalPatches
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void AddCustomLogs(Terminal __instance)
    {
        if (__instance.IsServer)
        {
            CustomStoryLogs.Logger.LogInfo("Loading unlocked logs");
            CustomStoryLogs.UnlockedNetwork.Value = ES3.Load<List<int>>(CustomStoryLogs.UnlockedSaveKey,
                GameNetworkManager.Instance.currentSaveFileName, new List<int>());
        }

        CustomStoryLogs.UnlockedLocal = CustomStoryLogs.UnlockedNetwork.Value;

        CustomStoryLogs.Logger.LogInfo("Adding logs");
        TerminalKeyword view = null;
        foreach (TerminalKeyword keyword in __instance.terminalNodes.allKeywords)
        {
            // Plugin.Logger.LogInfo(keyword.word);
            if (keyword.word == "view")
            {
                view = keyword;
            }
        }
        if (!view) return;
        
        foreach (CustomLogData data in CustomStoryLogs.RegisteredLogs.Values)
        {
            TerminalNode newNode = ScriptableObject.CreateInstance<TerminalNode>();
            newNode.name = data.LogName;
            newNode.displayText = data.LogText;
            newNode.storyLogFileID = data.LogID;
            newNode.clearPreviousText = true;
        
            TerminalKeyword newKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            newKeyword.word = data.Keyword;
        
            CompatibleNoun logNoun1 = new CompatibleNoun();
            logNoun1.result = newNode;
            logNoun1.noun = newKeyword;

            if (data.Unlocked && !CustomStoryLogs.GetUnlockedList().Contains(data.LogID))
            {
                CustomStoryLogs.UnlockedNetwork.Value.Add(data.LogID);
                CustomStoryLogs.UnlockedLocal.Add(data.LogID);
            }
        
            view.compatibleNouns = view.compatibleNouns.Append(logNoun1).ToArray();
            __instance.logEntryFiles.Add(newNode);
            __instance.terminalNodes.allKeywords = __instance.terminalNodes.allKeywords.Append(newKeyword).ToArray();
        }
    }

    [HarmonyPatch("TextPostProcess")]
    [HarmonyPrefix]
    private static void FixInvalidUnlockedLogIDs(Terminal __instance, string modifiedDisplayText, TerminalNode node, ref string __result)
    {
        if (node.name.Contains("LogsHub"))
        {
            List<int> fixedLogs = new List<int>();
            foreach (int logID in __instance.unlockedStoryLogs)
            {
                if (logID < __instance.logEntryFiles.Count)
                {
                    fixedLogs.Add(logID);
                }
            }
            __instance.unlockedStoryLogs = fixedLogs;
            
            List<int> fixedNewLogs = new List<int>();
            foreach (int logID in __instance.newlyUnlockedStoryLogs)
            {
                if (logID < __instance.logEntryFiles.Count)
                {
                    fixedLogs.Add(logID);
                }
            }
            __instance.newlyUnlockedStoryLogs = fixedNewLogs;
        }
    }

    [HarmonyPatch("TextPostProcess")]
    [HarmonyPostfix]
    private static void AddCustomLogsToTerminalDisplay(Terminal __instance, string modifiedDisplayText, TerminalNode node, ref string __result)
    {
        if (node.name.Contains("LogsHub"))
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n");
            
            foreach (int logID in CustomStoryLogs.GetUnlockedList())
            {
                if (!CustomStoryLogs.RegisteredLogs.ContainsKey(logID)) continue;

                CustomLogData log = CustomStoryLogs.RegisteredLogs[logID];
                if (!log.Hidden)
                {
                    stringBuilder.Append("\n" + log.LogName);
                }
            }
            
            stringBuilder.Append("\n\n\n\n");

            __result = __result.Replace("[currentUnlockedLogsList]", "");
            __result = __result.TrimEnd() + stringBuilder.ToString();
        }
    }

    [HarmonyPatch("AttemptLoadStoryLogFileNode")]
    [HarmonyPostfix]
    private static void LoadCustomLog(Terminal __instance, TerminalNode node)
    {
        int logID = node.storyLogFileID;
        if (CustomStoryLogs.RegisteredLogs.ContainsKey(logID) && CustomStoryLogs.GetUnlockedList().Contains(logID))
        {
            __instance.LoadNewNode(node);
        }
    }
}