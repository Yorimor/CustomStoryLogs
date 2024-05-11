using Unity.Netcode;
using UnityEngine;

namespace CustomStoryLogs;

public class CustomLogInteract: NetworkBehaviour
{
    public int storyLogID = -1;
    private bool collected;

    public void CollectLog()
    {
        if (NetworkManager.Singleton == null || GameNetworkManager.Instance == null || this.collected || storyLogID == -1)
            return;

        if (CustomStoryLogs.GetUnlockedList().Contains(this.storyLogID)) return;

        CustomStoryLogs.UnlockStoryLogOnServer(storyLogID);
    }

    public void LocalCollectLog()
    {
        this.collected = true;
        this.RemoveLogCollectible();
    }

    private void Start()
    {
        name = "CustomStoryLog." + storyLogID.ToString();
        if (CustomStoryLogs.GetUnlockedList().Contains(this.storyLogID))
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
