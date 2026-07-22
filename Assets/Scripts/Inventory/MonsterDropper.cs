using UnityEngine;

public class MonsterDropper : MonoBehaviour
{
    [SerializeField] private LootTable lootTable;
    [SerializeField] private float scatterRadius = 0.5f;
    [SerializeField] private float popForce = 2f;

    [Header("TEMP - Step 5 manual verification only (remove once real combat/Health exists)")]
    [SerializeField] private KeyCode testSpawnDropsKey = KeyCode.K;

    private void Awake()
    {
        // No real combat system yet. Once a Health-style component implementing
        // IDamageable exists, this auto-wires SpawnDrops() to its death event -
        // no changes needed here when combat lands.
        var damageable = GetComponent<IDamageable>();
        if (damageable != null)
            damageable.OnDeath += SpawnDrops;
    }

    private void Update()
    {
        if (Input.GetKeyDown(testSpawnDropsKey))
            SpawnDrops();
    }

    public void SpawnDrops()
    {
        if (lootTable == null)
        {
            Debug.LogWarning("[MonsterDropper] No LootTable assigned.");
            return;
        }

        foreach (var (item, count) in lootTable.GetDrops())
        {
            if (item.worldPrefab == null)
            {
                Debug.LogWarning($"[MonsterDropper] {item.itemName} has no worldPrefab; skipping drop.");
                continue;
            }

            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0.4f, offset.y);

            var dropped = Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);
            dropped.AddComponent<DroppedItemMarker>();

            var pickup = dropped.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemData = item;
                pickup.count = count;
            }

            var rb = dropped.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(Vector3.up * popForce, ForceMode.Impulse);

            Debug.Log($"[MonsterDropper] Dropped {item.itemName} x{count} at {spawnPos}");
        }
    }
}
