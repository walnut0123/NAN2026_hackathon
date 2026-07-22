using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int capacity = 20;

    [Header("TEMP - Step 2 manual verification only (remove once UI exists)")]
    [SerializeField] private ItemData testItemToAdd;
    [SerializeField] private KeyCode testAddKey = KeyCode.I;

    [Header("TEMP - Step 4 manual verification only (remove once UI exists)")]
    [SerializeField] private KeyCode testDropKey = KeyCode.O;
    [SerializeField] private int testDropSlotIndex = 0;

    public Inventory Inventory { get; private set; }

    private void Awake()
    {
        Inventory = new Inventory(capacity);
    }

    private void Update()
    {
        if (testItemToAdd != null && Input.GetKeyDown(testAddKey))
        {
            int leftover = TryAddItem(testItemToAdd, 1);
            Debug.Log($"[PlayerInventory] TryAddItem({testItemToAdd.itemName}) leftover={leftover}");
            LogSlots();
        }

        if (Input.GetKeyDown(testDropKey))
        {
            bool dropped = TryDropItem(testDropSlotIndex, 1);
            Debug.Log($"[PlayerInventory] TryDropItem(slot {testDropSlotIndex}) success={dropped}");
            LogSlots();
        }
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


    private void LogSlots()
    {
        for (int i = 0; i < Inventory.Slots.Count; i++)
        {
            var slot = Inventory.Slots[i];
            if (!slot.IsEmpty)
                Debug.Log($"  Slot {i}: {slot.item.itemName} x{slot.count}");
        }
    }
}
