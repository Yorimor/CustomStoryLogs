using Unity.Netcode;
using UnityEngine;

namespace CustomStoryLogs;

public class CustomLogInteract: NetworkBehaviour
{
    public LogCollectableData data;
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
        if (this.gameObject.GetComponent<InteractTrigger>() != null)
        {
            this.gameObject.GetComponent<InteractTrigger>().interactable = false;
        }
        
        foreach (UnityEngine.MeshRenderer meshChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
            if (meshChild != null)
            {
                meshChild.enabled = false;
            }

        foreach (Collider colliderChild in this.GetComponentsInChildren<Collider>())
            if (colliderChild != null)
            {
                colliderChild.enabled = false;
            }
    }
}
