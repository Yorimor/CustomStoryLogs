# CustomStoryLogs

---

A Mod Library for use with Lethal Company.

Allows you to add custom story logs to the terminal, and in world interactables to unlock them.

This is my first mod intended for use by other people as a library, so the code is inconsistent and messy. Any feed back and help is very welcome!

---

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
- Add logs from text/json files
- Add custom log views, instead of having it all under `sigurd`
- Ability to use custom model/game objects for the pickup

---

### Credits

- Xilophor's LethalNetworkAPI and Project Template
- Evaisa's LethaLib
- Model used: https://elbolilloduro.itch.io/exploration-objects