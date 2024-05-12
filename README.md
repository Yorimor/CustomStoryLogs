# CustomStoryLogs

---

A Mod Library for use with Lethal Company.

Allows you to add custom story logs to the terminal, and in world interactables to unlock them.

This is my first mod intended for use by other people as a library, so the code is inconsistent and messy. Any feed back and help is very welcome!

Logs can be added with code, or through a text/json file created with my [Log Builder](https://yorimor.github.io/CustomStoryLogsJsonBuilder/) tool!

---

### Using the Log Builder

Fill out all the box in the [Log Builder](https://yorimor.github.io/CustomStoryLogsJsonBuilder/), add as many logs as you need.

Once you have everything filled out, click export to generate the json data (it will give you something similar to the below example), and then click copy text.

```json
{
    "username": "Yorimor",
    "modname": "ExampleLog",
    "version": "1.2.3",
    "logs": {
        "0": {
            "name": "Example - May 12",
            "text": "This is an example log, created with the log builder!\n\n:D",
            "moon": "71 Gordion",
            "position": {
                "X": "-28",
                "Y": "-2",
                "Z": "-15"
            },
            "rotation": {
                "X": "0",
                "Y": "0",
                "Z": "0"
            }
        }
    }
}
```

In your own mods files create the following folders `BepInEx/plugins/CustomStoryLogs/logs/`

In the `logs` folder create a new .txt or .json file (name it anything you like) and paste the json data into the file and save it.

And that's it! My mod will automatically load the logs and place them in game for you!

```

---

### Using the library through code

Add a story log:
```csharp
// public int RegisterCustomLog(string modGUID, string logName, string text, bool unlocked=false, bool hidden=false)
int myLogID = CustomStoryLogs.CustomStoryLogs.RegisterCustomLog(YOUR_MOD_GUID, "Log Name - May 09", "test\nunlocked\nnot hidden");
```

The function returns an integer which is used to uniquely identify the new log. It is created from a hash of your mods GUID and the first word in the log name.

The first word in the log name is also used as the keyword for opening the log, e.g. to open `Bridge collapse - Mar 19` you would type `view bridge`.

The log ID is used below to create an interactable pickup in world. Alternatively you can call `CustomStoryLogs.UnlockStoryLogOnServer(logID)` to unlock the log through code instead of a pickup.

You can also define if the log is already unlocked or hidden the terminal list.

Add an interactable:
```csharp
// public void RegisterCustomLogCollectable(string modGUID, int logID, string planetName, Vector3 position, Vector3 rotation)
CustomStoryLogs.CustomStoryLogs.RegisterCustomLogCollectable(YOUR_MOD_GUID, "71 Gordion", new Vector3(-28,-2,-15), Vector3.zero);
```

This spawns the interactable on the Company moon just in front of the ship. Interactable objects are removed when leaving the moon.

---

### Planned Features

- Improve docs
- Add custom log views, instead of having it all under `sigurd`
- Ability to use custom model/game objects for the pickup

- Add logs from text/json files (Done!)

---

### Credits

- Noah E. for helping so much in creating the Log Builder
- Xilophor's LethalNetworkAPI and Project Template
- Evaisa's LethaLib
- Model used: https://elbolilloduro.itch.io/exploration-objects