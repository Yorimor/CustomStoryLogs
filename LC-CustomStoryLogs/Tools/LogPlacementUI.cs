using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace CustomStoryLogs.Tools;

public class LogPlacementUI: MonoBehaviour
{
    public TMP_InputField MoonName;
    
    public XYZButtonGroup Position;
    public XYZButtonGroup Rotation;

    public Button[] SaveButtons;

    public Transform LogTransform = null;
    public bool TweakModeOn = false;
    
    public void OnSave(int btnID)
    {
        CustomStoryLogs.Logger.LogInfo($"Saving Tool Data {btnID}!");
        PlacementToolData data = new PlacementToolData();
        data.moon = StartOfRound.Instance.currentLevel.PlanetName;

        data.position = new PlacementToolVector();
        data.position.x = LogTransform.position.x;
        data.position.y = LogTransform.position.y;
        data.position.z = LogTransform.position.z;
        
        data.rotation = new PlacementToolVector();
        data.rotation.x = LogTransform.rotation.eulerAngles.x;
        data.rotation.y = LogTransform.rotation.eulerAngles.y;
        data.rotation.z = LogTransform.rotation.eulerAngles.z;
        
        string path = Path.Combine(Paths.PluginPath, "Yorimor-CustomStoryLogs", "tool_data");
        Directory.CreateDirectory(path);
        path = Path.Combine(path, $"position-{btnID}.json");
        
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        System.IO.File.WriteAllText(path, jsonString);
        CustomStoryLogs.Logger.LogInfo($"Log position saved to {path}");
    }

    public void Update()
    {
        if (LogTransform == null) return;

        if (StartOfRound.Instance.currentLevel)
        {
            MoonName.text = $"\"{StartOfRound.Instance.currentLevel.PlanetName}\"";
        }
        else
        {
            MoonName.text = "Not on a moon!";
        }
        
        if (TweakModeOn)
        {
            LogTransform.position = Position.vector;
            LogTransform.rotation = Quaternion.Euler(Rotation.vector);
        }
        else
        {
            Position.NewVector(LogTransform.position);
            Rotation.NewVector(LogTransform.rotation.eulerAngles);
        }
    }
}

public class XYZButton: MonoBehaviour
{
    public TMP_InputField Value;
    public Button Increment;
    public Button Decrement;

    public float SmallChange = 0.01f;
    public float LargeChange = 0.1f;

    public bool IsRotation;
    
    public void OnIncrement()
    {
        Value.text = FixValue(GetFloat() + LargeChange).ToString("0.00");
    }
    
    public void OnDecrement()
    {
        Value.text = FixValue(GetFloat() - LargeChange).ToString("0.00");
    }
    
    public float GetFloat()
    {
        float result;
        if (float.TryParse(Value.text, out result))
        {
            return result;
        }
        else
        {
            return 0.0f;
        }
    }
    
    public float FixValue(float _value)
    {
        if (IsRotation)
        {
            if (_value < 0f)
            {
                _value = 360f - ((_value * -1) % 360f);
            }

            if (_value > 360f)
            {
                _value = _value % 360f;
            }
        }

        return _value;
    }
}

public class XYZButtonGroup : MonoBehaviour
{
    public XYZButton X;
    public XYZButton Y;
    public XYZButton Z;

    public Vector3 vector;
    
    public void UpdateVector()
    {
        vector = new Vector3(X.GetFloat(), Y.GetFloat(), Z.GetFloat());
    }

    public void NewVector(Vector3 _vector)
    {
        vector = _vector;
        X.Value.text = vector.x.ToString("0.00");
        Y.Value.text = vector.y.ToString("0.00");
        Z.Value.text = vector.z.ToString("0.00");
    }
}

public class PlacementToolData
{
    public string moon;
    public PlacementToolVector position;
    public PlacementToolVector rotation;
}

public class PlacementToolVector
{
    public float x;
    public float y;
    public float z;
}