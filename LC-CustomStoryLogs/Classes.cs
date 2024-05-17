namespace CustomStoryLogs;
using UnityEngine;

public class CustomLogData
{
    public string ModGUID;
    public int LogID;
    public bool Unlocked;
    public bool Hidden;
    public string LogName;
    public string LogText;
    public string Keyword;
    public LogCollected Event;

    public void UpdateText(string newText)
    {
        LogText = newText;

        Terminal terminal = GameObject.FindObjectOfType<Terminal>();
        if (terminal)
        {
            foreach (TerminalNode node in terminal.logEntryFiles)
            {
                if (node.storyLogFileID != LogID) continue;

                node.displayText = newText;
                break;
            }
        }
    }
}

public class LogCollectableData
{
    public string ModGUID;
    public Vector3 Position;
    public Vector3 Rotation;
    public int LogID;
    public int ModelID;
}