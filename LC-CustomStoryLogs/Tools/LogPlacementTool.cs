using System.IO;
using BepInEx;
using GameNetcodeStuff;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CustomStoryLogs.Tools;

public class LogPlacementTool: MonoBehaviour
{
    public InputAction modifierKey;
    public InputAction activateToolAction;
    public InputAction activateUIAction;
    public InputAction escKey;
    
    private GameObject dummyLogPrefab;
    private GameObject dummyLog;
    
    private GameObject canvasPrefab;
    private Transform canvas;
    private LogPlacementUI canvasUI;
    
    private LayerMask layerMask;

    private Camera cam;
    private PlayerControllerB playerController;
    
    private bool isToolActive = false;
    private bool isUIActive = false;
    private void Start()
    {
        escKey = KeyBindManager.CreateInputAction("EscKey", "<Keyboard>/escape", "press");
        modifierKey = KeyBindManager.CreateInputAction("ModifierKey", "<Keyboard>/alt", "press");
        activateToolAction = KeyBindManager.CreateInputAction("ActivateLogPositionTool", "<Keyboard>/l", "press");
        activateUIAction = KeyBindManager.CreateInputAction("ActivateUITool", "<Keyboard>/k", "press");
        
        dummyLogPrefab = CustomStoryLogs.MyAssets.LoadAsset<GameObject>("assets/yorimor/customstorylogs/logpreview.prefab");
        canvasPrefab = CustomStoryLogs.MyAssets.LoadAsset<GameObject>("Assets/Yorimor/CustomStoryLogs/LogToolUI.prefab");
        
        cam = StartOfRound.Instance.activeCamera;
        playerController = StartOfRound.Instance.localPlayerController;

        //Add Default, Room and Colliders LayerMask
        layerMask |= (1 << 0);
        layerMask |= (1 << 8);
        layerMask |= (1 << 11);
        
        CustomStoryLogs.Logger.LogInfo("Log Placement tool initialised. Alt+L to Open Tool");
    }

    void Update()
    {
        if (modifierKey.IsPressed()) // TODO: fix null ref here if exit save and enter another
        // InvalidOperationException: Cannot add action with duplicate name 'EscKey' to set 'CustomStoryLogs'
        {
            if (activateToolAction.triggered)
            {
                ActivateTool();
            }

            if (activateUIAction.triggered)
            {
                ActivateUI();
            }
        }

        if (escKey.triggered)
        {
            if (isToolActive)
            {
                if (isUIActive) ActivateUI();
                ActivateTool();
            }
        }

        if (dummyLog == null || cam == null) return;
        
        
        RaycastHit hit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            
        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            dummyLog.transform.position = hit.point;
            dummyLog.transform.up = hit.normal;

            dummyLog.transform.position += dummyLog.transform.up.normalized * 0.04f;
            dummyLog.transform.Rotate(0, 0, -90);
        }
    }

    void ActivateTool()
    {
        if(dummyLogPrefab == null || canvasPrefab == null) return;
        isToolActive = !isToolActive;

        if (isToolActive)
        {
            CustomStoryLogs.Logger.LogDebug("Activate Log Placement Tool");
            dummyLog = Instantiate(dummyLogPrefab, Vector3.zero, Quaternion.identity);
            canvas = Instantiate(canvasPrefab, Vector3.zero, Quaternion.identity).transform;

            canvasUI = canvas.GetComponent<LogPlacementUI>();
            canvasUI.LogTransform = dummyLog.transform;
        }
        else
        {
            CustomStoryLogs.Logger.LogDebug("Deactivate Log Placement Tool");
            Destroy(dummyLog);
            Destroy(canvas.gameObject);
            isUIActive = false;
            LockPlayerCamera(false);
        }
    }

    void ActivateUI()
    {
        if (!isToolActive || canvas == null || canvasUI == null) return;
        isUIActive = !isUIActive;
        
        if (isUIActive)
        {
            CustomStoryLogs.Logger.LogDebug("Enable Log Placement Tool Edit Mode");
            canvasUI.TweakModeOn = true;
            LockPlayerCamera(true);
        }
        else
        {
            CustomStoryLogs.Logger.LogDebug("Disable Log Placement Tool Edit Mode");
            canvasUI.TweakModeOn = false;
            LockPlayerCamera(false);
        }
    }
    
    void LockPlayerCamera(bool _lock)
    {
        if(playerController == null) return;

        if(_lock)
        {
            playerController.disableLookInput = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            playerController.disableLookInput = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}