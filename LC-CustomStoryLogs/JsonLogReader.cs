using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using BepInEx;
using Newtonsoft.Json;

namespace CustomStoryLogs;

public struct JsonLogsFile
{
    public string username;
    public string modname;
    public string version;
    public Dictionary<string, JsonLogData> logs;
}

public struct JsonLogData
{
    public string name;
    public string text;
    public string moon;
    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 rotation;
}

public class JsonLogReader
{
    public static void LoadLogsFromUserFiles()
    {
        foreach (string logsFolder in Directory.GetDirectories(Paths.PluginPath, "CustomStoryLogs", SearchOption.AllDirectories).ToList())
        {
            string path = Path.Combine(logsFolder, "logs");
            
            if (!Directory.Exists(path)) continue;
            
            string[] files = Directory.GetFiles(path);
            foreach (string text in files)
            {
                if (Path.GetExtension(text) == ".json" || Path.GetExtension(text) == ".txt")
                {
                    LoadLogsFromJsonFile(text);
                }
            }
        }
    }
    
    public static void LoadLogsFromJsonFile(string jsonPath)
    {
        CustomStoryLogs.Logger.LogInfo($"Loading logs from {jsonPath.Split("BepInEx").Last()}");
        string content = File.ReadAllText(jsonPath);
        JsonLogsFile logsFile = new JsonLogsFile();
        
        try
        {
            logsFile = JsonConvert.DeserializeObject<JsonLogsFile>(content);
        }
        catch (Exception e)
        {
            CustomStoryLogs.Logger.LogError($"There was a problem loading the file {jsonPath}");
            CustomStoryLogs.Logger.LogError(e.GetBaseException());
            return;
        }

        string modGuid = $"{logsFile.username}-{logsFile.modname}-{logsFile.version}";

        foreach (JsonLogData logData in logsFile.logs.Values)
        {
            CustomStoryLogs.Logger.LogDebug($"(Json log) Adding {logData.name} to {logData.moon} @ {logData.position}");

            var logID = CustomStoryLogs.RegisterCustomLog(modGuid, logData.name, logData.text);
            CustomStoryLogs.RegisterCustomLogCollectable(modGuid, logID, logData.moon, logData.position, logData.rotation);
        }
    }
}