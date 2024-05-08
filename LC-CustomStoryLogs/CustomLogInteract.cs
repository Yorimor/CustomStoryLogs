using Unity.Netcode;
using UnityEngine;
namespace CustomStoryLogs;

public class CustomLogInteract: MonoBehaviour
{
    public int storyLogID = -1;
    private bool collected;

    public void CollectLog()
    {
        if (NetworkManager.Singleton == null || GameNetworkManager.Instance == null || this.collected || this.storyLogID == -1)
            return;
        
        this.collected = true;
        this.RemoveLogCollectible();

        if (CustomStoryLogs.UnlockedNetVar.Value.Contains(this.storyLogID))
        {
            CustomStoryLogs.Logger.LogError($"Tried to unlock custom log {this.storyLogID} which has not been added!");
            return;
        }

        CustomStoryLogs.UnlockStoryLogOnServer(storyLogID);
    }

    private void Start()
    {
        if (!CustomStoryLogs.UnlockedNetVar.Value.Contains(this.storyLogID))
            return;
        this.RemoveLogCollectible();
    }

    private void RemoveLogCollectible()
    {
        foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
            componentsInChild.enabled = false;
        this.gameObject.GetComponent<InteractTrigger>().interactable = false;
        foreach (Collider componentsInChild in this.GetComponentsInChildren<Collider>())
            componentsInChild.enabled = false;
    }
}
