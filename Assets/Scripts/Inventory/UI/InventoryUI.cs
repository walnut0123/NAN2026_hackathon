using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    // No serialized cross-references: everything below is discovered automatically
    // so nothing can be mis-wired by dragging the wrong object in the Inspector.
    private PlayerInventory playerInventory;
    private CombinationManager combinationManager;
    private Transform slotContainer;
    private GameObject slotUIPrefab;

    private int? selectedSlotIndex;
    private ItemData selectedItem;

    private void Awake()
    {
        slotContainer = transform.Find("SlotContainer");
        slotUIPrefab = Resources.Load<GameObject>("UI/InventorySlotUI_Row");
    }

    private void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        combinationManager = FindObjectOfType<CombinationManager>();

        playerInventory.Inventory.OnInventoryChanged += Refresh;
        Refresh();

        UIManager.Register("Inventory", gameObject);
    }

    private void OnDestroy()
    {
        if (playerInventory != null && playerInventory.Inventory != null)
            playerInventory.Inventory.OnInventoryChanged -= Refresh;

        UIManager.Unregister("Inventory");
    }

    private void Refresh()
    {
        selectedSlotIndex = null;
        selectedItem = null;

        // DestroyImmediate (not Destroy) because Refresh can run more than once per
        // frame (e.g. two TryAddItem calls in a row each fire OnInventoryChanged) -
        // Destroy defers removal to end-of-frame, so a second Refresh would still see
        // the "old" rows and duplicate them.
        for (int i = slotContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(slotContainer.GetChild(i).gameObject);

        var slots = playerInventory.Inventory.Slots;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
                continue;

            var rowGO = Instantiate(slotUIPrefab, slotContainer);
            var rowUI = rowGO.GetComponent<InventorySlotUI>();
            rowUI.Setup(i, slots[i], playerInventory, this);
        }
    }

    public void OnSlotCombineClicked(int slotIndex, ItemData item)
    {
        if (selectedSlotIndex == null)
        {
            selectedSlotIndex = slotIndex;
            selectedItem = item;
            Debug.Log($"[InventoryUI] Selected {item.itemName} for combination. Pick a second item.");
            return;
        }

        if (selectedSlotIndex == slotIndex)
        {
            // Clicked the same slot again - cancel selection.
            selectedSlotIndex = null;
            selectedItem = null;
            return;
        }

        var a = selectedItem;
        var b = item;
        selectedSlotIndex = null;
        selectedItem = null;

        combinationManager.TryCombine(playerInventory, a, b);
    }
}
