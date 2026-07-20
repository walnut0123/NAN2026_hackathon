using UnityEngine;

public enum ItemType
{
    Material,   // 재료
    Consumable, // 소비품
    Equipment   // 장비
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;                   // 아이템 식별용 Unique ID (예: item_wood)
    public string itemName;             // 표시될 아이템 이름
    [TextArea(3, 5)]
    public string description;          // 아이템 설명
    public Sprite icon;                 // UI에 표시될 아이콘 이미지

    [Header("Properties")]
    public ItemType itemType;           // 아이템 종류
    public int maxStackCount = 99;      // 슬롯 당 최대 중첩 수
    public GameObject worldPrefab;      // 필드 드롭/습득용 프리팹 (3~4단계에서 사용)
}