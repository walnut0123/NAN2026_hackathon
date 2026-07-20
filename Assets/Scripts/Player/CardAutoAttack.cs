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

    [Header("탐색 설정")]
    [Tooltip("적 감지 주기 (초 단위, 성능 최적화를 위해 매 프레임 탐색하지 않음)")]
    public float searchInterval = 0.2f;

    // 현재 타겟으로 지정된 적
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    // Physics.OverlapSphereNonAlloc 최적화용 버퍼 (최대 20 마리 탐색)
    private Collider[] hitColliders = new Collider[20];
    private float searchTimer = 0f;

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

        // 지정한 주기(searchInterval)마다 주변 적 탐색 실행
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            FindNearestTarget();
        }
    }

    /// <summary>
    /// 사거리 내 'Enemy' 태그를 가진 가장 가까운 적을 찾습니다.
    /// </summary>
    private void FindNearestTarget()
    {
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders);
        
        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < numFound; i++)
        {
            Collider col = hitColliders[i];

            // 'Enemy' 태그 확인
            if (col != null && col.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector3.Distance(transform.position, col.transform.position);
                
                // 가장 가까운 적 업데이트
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = col.transform;
                }
            }
        }

        // 타겟이 변경되었을 때만 콘솔창에 적 이름 출력
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

    // 공격 사거리 및 현재 타겟 시각화 기즈모
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