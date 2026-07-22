using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int count = 1;

    // Final behavior, not a dev-only stub: a fully picked-up item is removed from the
    // field. Kept as a toggle (default on) so a specific pickup can be debugged without
    // disappearing, e.g. to repeatedly test Interact() without re-dropping it each time.
    public bool destroyOnFullPickup = true;

    public void Interact(PlayerInventory inventory)
    {
        if (itemData == null || inventory == null)
            return;

        int leftover = inventory.TryAddItem(itemData, count);
        int picked = count - leftover;

        if (picked <= 0)
        {
            Debug.Log($"[ItemPickup] Inventory full, could not pick up {itemData.itemName}");
            return;
        }

        Debug.Log($"[ItemPickup] Picked up {itemData.itemName} x{picked}");

        if (leftover > 0)
        {
            count = leftover;
        }
        else if (destroyOnFullPickup)
        {
            var entity = GetComponent<PersistentWorldEntity>();
            if (entity != null)
                GameManager.Instance?.MarkWorldObjectRemoved(entity.Id);

            Destroy(gameObject);
        }
    }
}
