using UnityEngine;

public class CardProjectile : MonoBehaviour
{
    // 이동 속도 및 판정 범위
    private float speed = 15.0f;
    private float hitThreshold = 0.2f;

    // 타겟 Y축 높이 보정
    private float targetHeightOffset = 1.0f;

    private Transform targetTransform;
    private Vector3 targetPosition;
    private bool isInitialized = false;

    /// <summary>
    /// 카드 발사 초기화 함수
    /// </summary>
    public void Initialize(Transform target)
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        targetTransform = target;
        UpdateTargetPosition();
        
        // 초기 생성 시 회전 설정
        RotateToTarget();

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // 1. 타겟 위치 최신화 (Y축 고정)
        UpdateTargetPosition();

        // 2. 진행 방향(적 방향)으로 회전
        RotateToTarget();

        // 3. 적을 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 4. 도착 여부 확인
        if (Vector3.Distance(transform.position, targetPosition) <= hitThreshold)
        {
            OnHitTarget();
        }
    }

    private void UpdateTargetPosition()
    {
        if (targetTransform != null)
        {
            // 타겟 Y축 높이는 카드의 현재 Y축 높이로 고정 (평면 이동)
            Vector3 rawTargetPos = targetTransform.position + new Vector3(0, targetHeightOffset, 0);
            targetPosition = new Vector3(rawTargetPos.x, transform.position.y, rawTargetPos.z);
        }
    }

    /// <summary>
    /// 빌보드를 제외하고, 카드가 진행 방향(적 방향)을 곧바르게 바라보도록 Y축만 회전시킵니다.
    /// </summary>
    private void RotateToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            // 이동 방향(X, Z)에 따른 Y축 회전각만 계산 (기존 +90도 오프셋 유지)
            float targetYAngle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + 90.0f;

            // X축, Z축 회전은 0으로 고정하여 평면 방향만 바라보게 설정
            transform.rotation = Quaternion.Euler(90, targetYAngle, 0);
        }
    }

private void OnHitTarget()
    {
        if (targetTransform != null)
        {
            var damageable = targetTransform.GetComponent<IDamageable>();
            damageable?.TakeDamage(1);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 에디터 뷰포트 창에서 카드와 적 사이의 추적 경로를 노란색 선으로 표시합니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!isInitialized || targetTransform == null) return;

        // 노란색 선 설정
        Gizmos.color = Color.yellow;
        
        // 카드의 현재 위치에서 고정된 타겟 목표 지점까지 선을 그림
        Gizmos.DrawLine(transform.position, targetPosition);
        
        // 목표 지점에 작은 구체를 그려 가시성 확보
        Gizmos.DrawWireSphere(targetPosition, 0.1f);
    }
}