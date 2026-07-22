using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int capacity = 20;

    public Inventory Inventory { get; private set; }

    private void Awake()
    {
        Inventory = new Inventory(capacity);
    }

    public int TryAddItem(ItemData item, int count)
    {
        return Inventory.AddItem(item, count);
    }

    public bool TryRemoveItem(ItemData item, int count)
    {
        return Inventory.RemoveItem(item, count);
    }

public bool TryDropItem(int slotIndex, int count)
    {
        if (slotIndex < 0 || slotIndex >= Inventory.Slots.Count)
            return false;

        var slot = Inventory.Slots[slotIndex];
        if (slot.IsEmpty || slot.count < count)
            return false;

        var item = slot.item;
        if (item.worldPrefab == null)
        {
            Debug.LogWarning($"[PlayerInventory] {item.itemName} has no worldPrefab assigned; cannot drop.");
            return false;
        }

        if (!Inventory.RemoveItem(item, count))
            return false;

        Vector3 spawnPos = GetDropPosition();
        var dropped = Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);
        dropped.AddComponent<DroppedItemMarker>();

        var pickup = dropped.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.itemData = item;
            pickup.count = count;
        }

        Debug.Log($"[PlayerInventory] Dropped {item.itemName} x{count} at {spawnPos}");
        return true;
    }

    private const float DropHeightOffset = 0.4f;

    private Vector3 GetDropPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
        Vector3 basePos = transform.position + transform.forward * 1.5f + new Vector3(randomOffset.x, DropHeightOffset, randomOffset.y);

        if (Physics.Raycast(basePos + Vector3.up * 5f, Vector3.down, out var hit, 20f, ~0, QueryTriggerInteraction.Ignore))
            basePos.y = hit.point.y + DropHeightOffset;

        return basePos;
    }
}
