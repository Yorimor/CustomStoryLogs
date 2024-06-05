using UnityEngine.InputSystem;

namespace CustomStoryLogs.Tools;

// Credit to: https://github.com/Kitseru/StarshipDeliveryMod

delegate void SimpleMethodDelegate();
internal class KeyBindManager
{
    public static InputActionMap? customActionMap;
    public static void BindKeyToMethod(SimpleMethodDelegate _method, string _inputName, string _key)
    {
        if(customActionMap == null)
        {
            customActionMap = new InputActionMap("CustomStoryLogs");
        }
        customActionMap.Disable();

        InputAction newAction = customActionMap.AddAction(_inputName);
        newAction.AddBinding("<Keyboard>/" + _key);
        customActionMap.Enable();
        newAction.performed += ctx => _method();

        CustomStoryLogs.Logger.LogInfo($"New Keybind created, {_inputName} bind to {_key}");
    }

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