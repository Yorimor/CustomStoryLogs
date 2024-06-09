﻿using UnityEngine.InputSystem;

namespace CustomStoryLogs.Tools;

// Credit to: https://github.com/Kitseru/StarshipDeliveryMod

delegate void SimpleMethodDelegate();

internal class KeyBindManager
{
    public static InputActionMap? customActionMap;

    public static InputAction CreateInputAction(string _inputName, string _binding, string _interactions)
    {
        if(customActionMap == null)
        {
            customActionMap = new InputActionMap("CustomStoryLogs");
        }
        customActionMap.Disable();

        InputAction newInputAction = customActionMap.AddAction(_inputName);
        newInputAction.AddBinding(_binding).WithInteraction(_interactions);
        
        customActionMap.Enable();

        return newInputAction;
    }
}