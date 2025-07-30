using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using UnityEngine;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace CustomStoryLogs;

public delegate void LogCollected(int logID);

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
[BepInDependency(LethalNetworkAPI.MyPluginInfo.PLUGIN_GUID)]
public class CustomStoryLogs : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "Yorimor.CustomStoryLogs";
    public const string PLUGIN_NAME = "CustomStoryLogs";
    public const string PLUGIN_VERSION = "1.5.2";
    
    public static CustomStoryLogs Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private static List<string> UsedKeywords = new List<string>();
    
    public static Dictionary<int, CustomLogData> RegisteredLogs = new Dictionary<int, CustomLogData>();

    public static Dictionary<string, List<int>> PlanetCollectables = new Dictionary<string, List<int>>();
    public static Dictionary<int, LogCollectableData> Collectables = new Dictionary<int, LogCollectableData>();
    
    public static string UnlockedSaveKey = $"{MyPluginInfo.PLUGIN_GUID}-Unlocked";

    // public static LNetworkVariable<List<int>> UnlockedNetwork = new LNetworkVariable<List<int>>(identifier: "UnlockedList");
    public static LNetworkVariable<List<int>?> UnlockedNetwork = LNetworkVariable<List<int>?>.Connect(identifier: "UnlockedList");
    public static List<int>? UnlockedLocal = new List<int>();
    
    // https://github.com/Xilophor/lethal-network-api-docs/blob/rework/docs/api/LethalNetworkAPI.LNetworkMessage.md
    
    // public static LethalServerMessage<string> SpawnLogsServer = new LethalServerMessage<string>(identifier: "SpawnLogs");
    // public static LethalClientMessage<string> SpawnLogsClient = new LethalClientMessage<string>(identifier: "SpawnLogs");
    public static LNetworkMessage<string> SpawnLogs = LNetworkMessage<string>.Connect(identifier: "SpawnLogs", null, SpawnLogsLocally);
    
    // public static LethalServerMessage<int> UnlockLogServer = new LethalServerMessage<int>(identifier: "UnlockLog");
    // public static LethalClientMessage<int> UnlockLogClient = new LethalClientMessage<int>(identifier: "UnlockLog");
    public static LNetworkMessage<int> UnlockLogMsg = LNetworkMessage<int>.Connect(identifier: "UnlockLog", ReceiveUnlockFromClient, ReceiveUnlockMsgFromServer);
    
    public static AssetBundle MyAssets;
    public static GameObject CustomLogObj;

    public static List<GameObject> CustomModels = new List<GameObject>();

    public static LogCollected AnyLogCollectEvent;

    public string AssemblyLocation;
    
    public static ConfigEntry<bool> ToolEnabled;
    public static ConfigEntry<bool> HideVanillaLogs;
    
    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;
        
        AssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MyAssets = AssetBundle.LoadFromFile(Path.Combine(AssemblyLocation, "yorimor.customlogs"));
        if (MyAssets == null) {
            Logger.LogError("Failed to load custom assets.");
            return;
        }
        
        // https://elbolilloduro.itch.io/exploration-objects
        CustomLogObj = MyAssets.LoadAsset<GameObject>("Assets/Yorimor/CustomStoryLogs/CustomStoryModel.prefab");
        NetworkPrefabs.RegisterNetworkPrefab(CustomLogObj);

        UnlockedNetwork.Value = new List<int>();

        // SpawnLogsClient.OnReceived += SpawnLogsLocally;
        // UnlockLogServer.OnReceived += ReceiveUnlockFromClient;
        // UnlockLogClient.OnReceived += ReceiveUnlockMsgFromServer;

        Patch();

        ToolEnabled = Config.Bind("PlacementTool", "ToolEnabled", false,"Enable/Disable the in game placement tool");
        Logger.LogInfo($"Placement Tool Enabled: {ToolEnabled.Value}");

        HideVanillaLogs = Config.Bind("VanillaLogs", "HideVanillaLogs", false,"Show/Hide the vanilla story logs");
        Logger.LogInfo($"Hide Vanilla Logs: {HideVanillaLogs.Value}");
        
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

        AnyLogCollectEvent += EmptyOnCollect;
        
        JsonLogReader.LoadLogsFromUserFiles();

        Logger.LogInfo($"{PLUGIN_GUID} v{PLUGIN_VERSION} loaded!");
        
        // AddTestLogs();
    }

    private static void AddTestLogs()
    {
        int modelID = RegisterCustomLogModel(MyAssets.LoadAsset<GameObject>("Assets/Yorimor/CustomStoryLogs/Cube.prefab"));
        int logID = RegisterCustomLog(PLUGIN_GUID, "Test - Test", "Model Test\n\n/\\\n\\/");
        RegisterCustomLogCollectable(PLUGIN_GUID, logID, "71 Gordion", new Vector3(-28,-2,-15), Vector3.zero);
        RegisteredLogs[logID].Event += TestEvent;
        RegisteredLogs[logID].UpdateText("New text");

        AnyLogCollectEvent += TestEventAny;
    }

    private static void TestEvent(int logID)
    {
        Logger.LogInfo($"TEST EVENT {logID}");
        RegisteredLogs[logID].UpdateText("new text :P");
    }

    private static void TestEventAny(int logID)
    {
        Logger.LogInfo($"TEST EVENT ANY {logID}");
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

    public static int RegisterCustomLog(string modGUID, string logName, string text, bool unlocked=false, bool hidden=false)
    {
        CustomLogData newLog = new CustomLogData();
        String[] split = logName.Trim().Split("-")[0].Trim().Split(" ");
        newLog.Keyword = split[0].ToLower();

        if (UsedKeywords.Contains(newLog.Keyword))
        {
            CustomStoryLogs.Logger.LogError($"Unable to add story log [{logName}], keyword [{newLog.Keyword}] already in use!");
            return -1;
        }
        UsedKeywords.Add(newLog.Keyword);
        
        newLog.LogName = logName;
        newLog.LogText = text + "\n\n\n\n\n\n";
        newLog.Unlocked = unlocked;
        newLog.Hidden = hidden;
        newLog.ModGUID = modGUID;

        newLog.LogID = (modGUID + newLog.Keyword).GetHashCode();

        RegisteredLogs[newLog.LogID] = newLog;
        
        Logger.LogDebug($"Added new log [{modGUID}/{newLog.Keyword}] with ID [{newLog.LogID}]");
        return newLog.LogID;
    }

    public static void RegisterCustomLogCollectable(string modGUID, int logID, string planetName, Vector3 position, Vector3 rotation, int modelID=0)
    {
        LogCollectableData collectableData = new LogCollectableData();
        if (!RegisteredLogs.ContainsKey(logID))
        {
            Logger.LogError($"Custom log not found with ID {logID} for collectable added by {modGUID}");
            return;
        }
        
        collectableData.ModGUID = modGUID;
        collectableData.LogID = logID;
        collectableData.Position = position;
        collectableData.Rotation = rotation;

        if (modelID > CustomModels.Count || modelID < 0)
        {
            Logger.LogError($"[{modGUID}/{logID}] Custom ModelID {modelID} invalid! Setting to default/zero.");
            modelID = 0;
        }
        
        collectableData.ModelID = modelID;

        if (!PlanetCollectables.ContainsKey(planetName))
        {
            PlanetCollectables[planetName] = new List<int>();
        }

        Collectables[collectableData.LogID] = collectableData;
        PlanetCollectables[planetName].Add(collectableData.LogID);
        
        return;
    }

    public static int RegisterCustomLogModel(GameObject customModel)
    {
        CustomModels.Add(customModel);
        int modelID = CustomModels.Count;
        
        CustomStoryLogs.Logger.LogInfo($"Added model {customModel.name} with ID {modelID}");
        return modelID;
    }

    private static void ReceiveUnlockFromClient(int logID, ulong client)
    {
        if (!RegisteredLogs.ContainsKey(logID))
        {
            CustomStoryLogs.Logger.LogError($"Log {logID} unlocked by {client} but it does not exist!");
            return;
        }
        CustomStoryLogs.Logger.LogInfo($"Log {logID} unlocked by {client}");
        UnlockedNetwork.Value.Add(logID);
        
        CustomStoryLogs.Logger.LogDebug($"Telling clients to unlock log {logID}");
        UnlockLogMsg.SendClients(logID);
    }

    public static List<int> GetUnlockedList()
    {
        UnlockedLocal ??= [];
        
        // thanks to sciencebird
        if (CustomStoryLogs.UnlockedNetwork.Value == null)
        {
            return UnlockedLocal;
        }
        
        foreach (int id in UnlockedNetwork.Value)
        {
            if (!UnlockedLocal.Contains(id))
            {
                UnlockedLocal.Add(id);
            }
        }

        return UnlockedLocal;
    }

    public static void UnlockStoryLogOnServer(int logID)
    {
        CustomStoryLogs.Logger.LogDebug($"Telling server to unlock log {logID}");
        UnlockLogMsg.SendServer(logID);
    }

    private static void ReceiveUnlockMsgFromServer(int logID)
    {
        CustomStoryLogs.Logger.LogDebug($"Unlocking log {logID}");
        HUDManager.Instance.DisplayGlobalNotification($"Found journal entry: '{RegisteredLogs[logID].LogName}'");
        GameObject.Find("CustomStoryLog." + logID.ToString())?.GetComponent<CustomLogInteract>()?.LocalCollectLog();
        UnlockedLocal.Add(logID);
        
        RegisteredLogs[logID].Event?.Invoke(logID);
        AnyLogCollectEvent?.Invoke(logID);
    }

    private static void SpawnLogsLocally(string planetName)
    {
        CustomStoryLogs.Logger.LogInfo($"Loading logs for: {planetName}");
        foreach (int index in CustomStoryLogs.PlanetCollectables[planetName])
        {
            LogCollectableData collectableData = CustomStoryLogs.Collectables[index];
            
            string objName = "CustomStoryLog." + collectableData.LogID.ToString();
            
            if (GameObject.Find(objName)) continue;
            
            CustomStoryLogs.Logger.LogInfo($"Spawning collectable log {collectableData.LogID}");
            GameObject obj = CustomStoryLogs.Instantiate(CustomLogObj);
            
            if (collectableData.ModelID > 0)
            {
                GameObject model = CustomStoryLogs.Instantiate(CustomModels[collectableData.ModelID - 1], obj.transform);
                obj.GetComponent<MeshRenderer>().enabled = false;

                BoxCollider oldBox = obj.GetComponent<BoxCollider>();
                BoxCollider newBox = model.GetComponent<BoxCollider>();
                if (newBox)
                {
                    newBox.enabled = false;
                    oldBox.size = newBox.size;
                    oldBox.center = newBox.center;
                }
            }

            CustomLogInteract interact = obj.GetComponent<CustomLogInteract>();
            interact.storyLogID = collectableData.LogID;
            interact.data = collectableData;
            
            obj.name = objName;
            obj.transform.position = collectableData.Position;
            obj.transform.rotation = Quaternion.Euler(collectableData.Rotation);
            
            CustomStoryLogs.Logger.LogDebug($"Spawned log {objName} @ {obj.transform.position}");
        }
    }

    public static void DespawnLogsLocally(string planetName)
    {
        CustomStoryLogs.Logger.LogInfo($"Removing logs for: {planetName}");
        foreach (int index in CustomStoryLogs.PlanetCollectables[planetName])
        {   
            LogCollectableData collectableData = CustomStoryLogs.Collectables[index];
            string objName = "CustomStoryLog." + collectableData.LogID.ToString();
            
            GameObject obj = GameObject.Find(objName);
            if (obj)
            {
                Destroy(obj);
            }
        }
    }

    public static void EmptyOnCollect(int logID)
    {
        // Intentionally empty
    }

    public static bool IsLogUnlocked(int logID)
    {
        try
        {
            return GetUnlockedList().Contains(logID);
        }
        catch (NullReferenceException e)
        {
            CustomStoryLogs.Logger.LogError($"{e}");
            CustomStoryLogs.Logger.LogError($"Error in IsLogUnlocked() for LogID {logID}, returning false");
            
            CustomStoryLogs.Logger.LogDebug($"UnlockedNetwork.Value -> {UnlockedNetwork.Value?.ToString()}");
            CustomStoryLogs.Logger.LogDebug($"UnlockedLocal -> {UnlockedLocal?.ToString()}");
            
            // Make lists not null if needed after logging error
            UnlockedNetwork.Value ??= [];
            UnlockedLocal ??= [];
        }

        return false;
    }
}