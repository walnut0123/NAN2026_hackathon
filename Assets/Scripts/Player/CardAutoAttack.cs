using System.Collections.Generic;
using UnityEngine;

public class CardAutoAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [Tooltip("자동 공격 기능 On/Off 스위치")]
    public bool isAutoAttackEnabled = true;

    [Tooltip("적 인식 및 공격 가능 거리 (Scene 뷰에서 기즈모로 확인 가능)")]
    public float attackRange = 10.0f;

    [Tooltip("공격 속도 (초 단위)")]
    public float attackCooldown = 1.0f;

    [Header("투사체 설정")]
    [Tooltip("발사할 카드 프리팹 리스트 (등록된 52종 카드 프리팹)")]
    public List<GameObject> cardPrefabs = new List<GameObject>();

    [Tooltip("카드가 생성될 위치 (지정하지 않으면 플레이어 위치)")]
    public Transform firePoint;

    [Tooltip("카드가 지면에 파묻히지 않도록 올려줄 Y축 높이 오프셋")]
    public float spawnHeightOffset = 1.0f;

    [Header("탐색 설정")]
    [Tooltip("적 감지 주기 (초 단위, 성능 최적화를 위해 매 프레임 탐색하지 않음)")]
    public float searchInterval = 0.2f;

    // 현재 타겟으로 지정된 적
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    // Physics.OverlapSphereNonAlloc 최적화용 버퍼
    private Collider[] hitColliders = new Collider[20];
    private float searchTimer = 0f;
    private float attackTimer = 0f;

    private void Update()
    {
        if (!isAutoAttackEnabled)
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                Debug.Log("[CardAutoAttack] 자동 공격 비활성화 - 타겟 해제됨");
            }
            return;
        }

        // 1. 지정한 주기마다 주변 적 탐색
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            FindNearestTarget();
        }

        // 2. 타겟이 존재할 경우 공격 쿨타임 계산 및 발사
        if (currentTarget != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                ShootCard();
            }
        }
        else
        {
            attackTimer = 0f;
        }
    }

    private void FindNearestTarget()
    {
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders);
        
        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < numFound; i++)
        {
            Collider col = hitColliders[i];

            if (col != null && col.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector3.Distance(transform.position, col.transform.position);
                
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = col.transform;
                }
            }
        }

        if (currentTarget != nearestEnemy)
        {
            currentTarget = nearestEnemy;

            if (currentTarget != null)
            {
                Debug.Log($"[CardAutoAttack] 현재 감지된 적: {currentTarget.name} (거리: {shortestDistance:F2}m)");
            }
            else
            {
                Debug.Log("[CardAutoAttack] 범위 내에 적이 없습니다.");
            }
        }
    }

    /// <summary>
    /// 등록된 카드 프리팹 리스트 중 하나를 랜덤으로 선택해 발사합니다.
    /// </summary>
    private void ShootCard()
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0)
        {
            Debug.LogWarning("[CardAutoAttack] Card Prefabs 리스트가 비어 있거나 할당되지 않았습니다.");
            return;
        }

        int randomIndex = Random.Range(0, cardPrefabs.Count);
        GameObject selectedPrefab = cardPrefabs[randomIndex];

        if (selectedPrefab == null) return;

        // 지면에 파묻히지 않도록 Y축 오프셋을 더해 생성 위치 설정
        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        spawnPosition.y += spawnHeightOffset;

        GameObject cardInstance = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        CardProjectile projectile = cardInstance.GetComponent<CardProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(currentTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}