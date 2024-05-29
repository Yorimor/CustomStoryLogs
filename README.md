# CustomStoryLogs

---

A Mod Library for use with Lethal Company.

Allows you to add custom story logs to the terminal, and in world interactables to unlock them.

This is my first mod intended for use by other people as a library, so the code is inconsistent and messy. Any feed back and help is very welcome!

Logs can be added with code, or through a text/json file created with my [Log Builder](https://yorimor.github.io/CustomStoryLogsJsonBuilder/) tool!

See the **Useful Links** section below, for tools to help with coordinates.

---

### Guides

<details>
<summary>Using the Log Builder</summary>

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

In your own mods files create the following folders `<Your Mod Folder>/BepInEx/plugins/CustomStoryLogs/logs/`.
Please see my [Example Mod](https://thunderstore.io/c/lethal-company/p/Yorimor/CustomStoryLogsExample/) as an example of this.

In the `logs` folder create a new .txt or .json file (name it anything you like) and paste the json data into the file and save it.

And that's it! My mod will automatically load the logs and place them in game for you!

</details>

<details>
<summary>Add Logs With Code</summary>

Add a story log:
```csharp
// public static int RegisterCustomLog(string modGUID, string logName, string text, bool unlocked=false, bool hidden=false)
int myLogID = CustomStoryLogs.CustomStoryLogs.RegisterCustomLog(YOUR_MOD_GUID, "Log Name - May 09", "test log\n\n:)");
```

The function returns an integer which is used to uniquely identify the new log. It is created from a hash of your mods GUID and the first word in the log name.

The first word in the log name is also used as the keyword for opening the log, e.g. to open `Bridge collapse - Mar 19` you would type `view bridge`.

The log ID is used below to create an interactable pickup in world. Alternatively you can call `CustomStoryLogs.CustomStoryLogs.UnlockStoryLogOnServer(logID)` to unlock the log through code instead of a pickup.

You can also define if the log is already unlocked or hidden the terminal list.

Add an interactable:
```csharp
// public static void RegisterCustomLogCollectable(string modGUID, int logID, string planetName, Vector3 position, Vector3 rotation, int modelID=0)
CustomStoryLogs.CustomStoryLogs.RegisterCustomLogCollectable(YOUR_MOD_GUID, myLogID, "71 Gordion", new Vector3(-28,-2,-15), Vector3.zero);
```

This spawns the interactable on the Company moon just in front of the ship. Interactable objects are removed when leaving the moon.

Planet name checks for the moons display name and scene name.

</details>


<details>
<summary>Adding Custom Models</summary>

You can register a prefab as a new model, and use it for collectables. Any custom models will require a **CollisionBox** component to be attached, as this will be used for the interaction.

[Follow the asset bundling guide on lethal.wiki to make and load your own assets](https://lethal.wiki/dev/intermediate/asset-bundling)

```csharp
// public static int RegisterCustomLogModel(GameObject customModel)
int modelID = CustomStoryLogs.CustomStoryLogs.RegisterCustomLogModel(myModel);

// Then use this ID for any of the collectables you want to use the new model
CustomStoryLogs.CustomStoryLogs.RegisterCustomLogCollectable(YOUR_MOD_GUID, "71 Gordion", new Vector3(-28,-2,-15), Vector3.zero, modelID);
```

</details>

<details>
<summary>Log Unlocked Events</summary>

There are two events for when logs are unlocked; One for a specific logs, and one for when any log is unlocked.

#### Your event method

This will need to be somewhere in your plugins code, and will be the code you want to run whenever the event is triggered.

```csharp
// Both events provide the unlocked logs ID
public static void MyEvent(int logID)
{
    // Your code here
}
```

#### Any Log Event
Gets called when any Custom Story Log is unlocked

```csharp
CustomStoryLogs.CustomStoryLogs.AnyLogCollectEvent += MyEvent;
```

#### Specific Log Event
Gets called when the specific Custom Story Log is unlocked.

Use this line **after** you have added your log using the **Add logs with code** section above.

```csharp
CustomStoryLogs.CustomStoryLogs.RegisteredLogs[logID].Event += MyEvent;
```


</details>

<details>
<summary>Miscellaneous</summary>

#### Update Log Text
Using the below code, you can modify the text of one of your logs.
```csharp
CustomStoryLogs.CustomStoryLogs.RegisteredLogs[logID].UpdateText("New Text");
```
</details>

---

#### Useful links

Imperium is a very straight forward way to get coordinates in a moon, the other two require some setup/learning.

- [Imperium](https://thunderstore.io/c/lethal-company/p/giosuel/Imperium/) 
- [Unity Explorer](https://thunderstore.io/c/lethal-company/p/Noop/UnityExplorer/)
- [Asset Ripper](https://github.com/nomnomab/lc-project-patcher)

---

#### Planned Features

- [ ] Add custom log views, instead of having it all under `sigurd`
- [ ] In game tool for placing logs
- [ ] Add to nuget
- [x] Events for when a log is collected
- [x] Ability to use custom model/game objects for the pickup
- [x] Add logs from text/json files

---

#### Credits

- Noah E. for helping so much in creating the Log Builder
- Xilophor's LethalNetworkAPI and Project Template
- Evaisa's LethaLib
- Model used: https://elbolilloduro.itch.io/exploration-objects