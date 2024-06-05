using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameNetcodeStuff;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomStoryLogs.Tools;

public class LogPlacementTool: MonoBehaviour
{
    public InputAction activateToolAction;
    
    private GameObject dummyLogPrefab;
    private GameObject dummyLog = null;
    
    private GameObject canvasPrefab;
    
    private void Start()
    {
        activateToolAction = KeyBindManager.CreateInputAction("ActivateLogPositionTool", "<Keyboard>/p", "press");
        
        dummyLogPrefab = CustomStoryLogs.MyAssets.LoadAsset<GameObject>("assets/yorimor/customstorylogs/logpreview.prefab");
        canvasPrefab = CustomStoryLogs.MyAssets.LoadAsset<GameObject>("assets/yorimor/customstorylogs/canvas.prefab");
    }

    void Update()
    {
        if (activateToolAction.triggered)
        {
            ActivateTool();
        }
    }

    void ActivateTool()
    {
        CustomStoryLogs.Logger.LogInfo("ToggleTool");
    }
}