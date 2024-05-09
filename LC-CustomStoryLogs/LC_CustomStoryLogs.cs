using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace CustomStoryLogs;

public struct CustomLogData
{
    public string ModGUID;
    public int LogID;
    public bool Unlocked;
    public bool Hidden;
    public string LogName;
    public string LogText;
    public string Keyword;
}

public struct CustomCollectableData
{
    public string ModGUID;
    public Vector3 Position;
    public Vector3 Rotation;
    public int LogID;
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
[BepInDependency(LethalNetworkAPI.MyPluginInfo.PLUGIN_GUID)]
public class CustomStoryLogs : BaseUnityPlugin
{
    public static CustomStoryLogs Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private static List<string> UsedKeywords = new List<string>();
    
    public static Dictionary<int, CustomLogData> RegisteredLogs = new Dictionary<int, CustomLogData>();

    public static Dictionary<string, List<CustomCollectableData>> RegisteredCollectables =
        new Dictionary<string, List<CustomCollectableData>>();
    
    public static string UnlockedSaveKey = $"{LethalNetworkAPI.MyPluginInfo.PLUGIN_GUID}-Unlocked";

    public static LethalNetworkVariable<List<int>> UnlockedNetVar = new LethalNetworkVariable<List<int>>(identifier: $"{MyPluginInfo.PLUGIN_GUID}-Unlocked");
    
    public static LethalServerMessage<string> SpawnLogsServer = new LethalServerMessage<string>(identifier: $"{MyPluginInfo.PLUGIN_GUID}-SpawnLogs");
    LethalClientMessage<string> SpawnLogsClient = new LethalClientMessage<string>(identifier: $"{MyPluginInfo.PLUGIN_GUID}-SpawnLogs");
    
    LethalServerMessage<int> UnlockLogServer = new LethalServerMessage<int>(identifier: $"{MyPluginInfo.PLUGIN_GUID}-UnlockLog");
    LethalClientMessage<int> UnlockLogClient = new LethalClientMessage<int>(identifier: $"{MyPluginInfo.PLUGIN_GUID}-UnlockLog");
    
    public static AssetBundle MyAssets;
    public static GameObject CustomLogObj;
    
    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;
        
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MyAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "yorimor.customlogs"));
        if (MyAssets == null) {
            Logger.LogError("Failed to load custom assets.");
            return;
        }
        
        // https://elbolilloduro.itch.io/exploration-objects
        CustomLogObj = MyAssets.LoadAsset<GameObject>("Assets/Yorimor/CustomStoryLogs/CustomStoryModel.prefab");
        NetworkPrefabs.RegisterNetworkPrefab(CustomLogObj);

        UnlockedNetVar.Value = new List<int>();

        SpawnLogsClient.OnReceived += SpawnLogsLocally;
        UnlockLogServer.OnReceived += ReceiveUnlockFromClient;
        UnlockLogClient.OnReceived += ReceiveUnlockMsgFromServer;

        Patch();

        UsedKeywords =
        [
            "buy", "pro flashlight", "money", "confirm", "deny", "help", "info", "store", "pro flashlight",
            "survival kit", "flashlight", "lockpicker", "mapper", "shovel", "jetpack", "boombox", "bestiary", "stun",
            "reset credits", "view", "inside cam", "moons", "vow", "experimentation", "assurance", "offense",
            "adamance", "route", "television", "teleporter", "rend", "march", "dine", "titan", "artifice", "embrion",
            "company", "walkie-talkie", "spray paint", "brackens", "forest keeper", "earth leviathan", "lasso",
            "spore lizards", "snare fleas", "eyeless dogs", "hoarding bugs", "bunker spiders", "hygroderes",
            "coil-heads", "manticoils", "baboon hawks", "nutcracker", "old birds", "butler", "circuit bees", "locusts",
            "thumpers", "jester", "decor", "upgrades", "tzp", "green suit", "hazard suit", "pajama suit", "cozy lights",
            "sigurd", "signal", "toilet", "record", "shower", "table", "romantic table", "file cabinet", "cupboard",
            "bunkbeds", "storage", "other", "scan", "b3", "c1", "c2", "c7", "d6", "f2", "h5", "i1", "j6", "k9", "l0",
            "m6", "m9", "o5", "p1", "r2", "r4", "t2", "u2", "u9", "v0", "x8", "y9", "z3", "first", "smells", "swing",
            "shady", "sound", "goodbye", "screams", "golden", "idea", "nonsense", "hiding", "desmond", "real",
            "zap gun", "monitor", "switch", "loud horn", "extension ladder", "inverse teleporter", "jackolantern",
            "radar", "eject", "welcome mat", "goldfish", "plushie pajama man", "purple suit", "purple", "bee", "bunny",
            "disco", "tulip"
        ];

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loaded!");
        
        AddTestLogs();
    }

    private void AddTestLogs()
    {
        RegisterCustomLog("test", "test", "ytest", unlocked:true);
        RegisterCustomLog("test", "test one", "log testeraidaioja", unlocked:true);
        RegisterCustomLog("test", "test one two", "asdads a asdfa a", unlocked:true);
        RegisterCustomLog("test", "test two three - asjsjsd", "jkukjhkhkh a", unlocked:true);
        RegisterCustomLog("test", "test two - five", "zcxzczcxcz", unlocked:true);
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
        Harmony.PatchAll();
    }

    internal static void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    public int RegisterCustomLog(string modGUID, string logName, string text, bool unlocked=false, bool hidden=false)
    {
        CustomLogData newLog = new CustomLogData();
        String[] split = logName.Trim().Split("-")[0].Trim().Split(" ");
        newLog.Keyword = split[0].ToLower();
        if (split.Length >= 2) newLog.Keyword += split[1];
        if (split.Length >= 3) newLog.Keyword += split[2];

        if (UsedKeywords.Contains(newLog.Keyword))
        {
            CustomStoryLogs.Logger.LogError($"Unable to add story log [{logName}], keyword [{newLog.Keyword}] already in use!");
            throw new Exception($"Unable to add story log [{modGUID}.{logName}], keyword [{newLog.Keyword}] already in use!");
        }
        UsedKeywords.Add(newLog.Keyword);
        
        newLog.LogName = logName;
        newLog.LogText = text + "\n\n\n\n\n\n";
        newLog.Unlocked = unlocked;
        newLog.Hidden = hidden;
        newLog.ModGUID = modGUID;

        newLog.LogID = (modGUID + newLog.Keyword).GetHashCode();

        RegisteredLogs[newLog.LogID] = newLog;

        return newLog.LogID;
    }

    public void RegisterCustomLogCollectable(string modGUID, int logID, string planetName, Vector3 position, Vector3 rotation)
    {
        if (!RegisteredLogs.ContainsKey(logID))
        {
            Logger.LogError($"Custom log not found with ID {logID} for collectable added by {modGUID}");
            return;
        }
        
        CustomCollectableData collectableData = new CustomCollectableData();
        collectableData.ModGUID = modGUID;
        collectableData.LogID = logID;
        collectableData.Position = position;
        collectableData.Rotation = rotation;

        if (!RegisteredCollectables.ContainsKey(planetName))
        {
            RegisteredCollectables[planetName] = new List<CustomCollectableData>();
        }

        RegisteredCollectables[planetName].Add(collectableData);
    }

    private void ReceiveUnlockFromClient(int logID, ulong client)
    {
        if (!RegisteredLogs.ContainsKey(logID))
        {
            CustomStoryLogs.Logger.LogError($"Log {logID} unlocked by {client} but it does not exist!");
            return;
        }
        CustomStoryLogs.Logger.LogInfo($"Log {logID} unlocked by {client}");
        UnlockedNetVar.Value.Add(logID);
        UnlockLogServer.SendAllClients(logID);
    }

    public static void UnlockStoryLogOnServer(int logID)
    {
        Instance.UnlockLogClient.SendServer(logID);
    }

    private void ReceiveUnlockMsgFromServer(int logID)
    {
        HUDManager.Instance.DisplayGlobalNotification($"Found journal entry: '{RegisteredLogs[logID].LogName}'");
        GameObject.Find("CustomStoryLog." + logID.ToString())?.GetComponent<CustomLogInteract>().LocalCollectLog();
    }

    private void SpawnLogsLocally(string planetName)
    {
        CustomStoryLogs.Logger.LogInfo($"Loading logs for: {planetName}");
        foreach (CustomCollectableData collectableData in CustomStoryLogs.RegisteredCollectables[planetName])
        {   
            string objName = "CustomStoryLog." + collectableData.LogID.ToString();
            
            if (GameObject.Find(objName)) continue;
            
            CustomStoryLogs.Logger.LogInfo($"Spawning collectable log {collectableData.LogID}");
            GameObject obj = CustomStoryLogs.Instantiate(CustomStoryLogs.CustomLogObj);
                
            obj.GetComponent<CustomLogInteract>().storyLogID = collectableData.LogID;
            obj.name = objName;
            obj.transform.position = collectableData.Position;
            obj.transform.rotation = Quaternion.Euler(collectableData.Rotation);
        }
    }

    public static void DespawnLogsLocally(string planetName)
    {
        CustomStoryLogs.Logger.LogInfo($"Removing logs for: {planetName}");
        foreach (CustomCollectableData collectableData in CustomStoryLogs.RegisteredCollectables[planetName])
        {   
            string objName = "CustomStoryLog." + collectableData.LogID.ToString();
            
            GameObject obj = GameObject.Find(objName);
            if (obj)
            {
                Destroy(obj);
            }
        }
    }
}