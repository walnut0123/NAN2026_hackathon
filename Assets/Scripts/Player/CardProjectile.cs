using UnityEngine;

public class CardProjectile : MonoBehaviour
{
    [Header("투사체 속성")]
    [Tooltip("카드 이동 속도")]
    public float speed = 15.0f;

    [Tooltip("굶지마 스타일 포탄 궤적 높이 (곡선 연출)")]
    public float arcHeight = 2.0f;

    [Tooltip("목표 지점 도착 인정 거리")]
    public float hitThreshold = 0.2f;

    private Transform targetTransform;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float totalDistance;
    private float progress = 0.0f;
    private bool isInitialized = false;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

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
        startPosition = transform.position;
        targetPosition = targetTransform.position;
        
        totalDistance = Vector3.Distance(startPosition, targetPosition);
        progress = 0.0f;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // 1. 빌보드(Billboard) 처리: 카드가 항상 카메라를 정면으로 바라보게 유지
        AlignToCamera();

        // 2. 타겟 위치 최신화 (적이 이동 중일 수 있으므로)
        if (targetTransform != null)
        {
            targetPosition = targetTransform.position;
        }

        // 3. 진행도 계산
        progress += (speed / Mathf.Max(totalDistance, 0.1f)) * Time.deltaTime;

        if (progress >= 1.0f || Vector3.Distance(transform.position, targetPosition) <= hitThreshold)
        {
            OnHitTarget();
            return;
        }

        // 4. 호(Arc)를 그리는 포탄 궤적 이동 계산
        Vector3 currentLinearPos = Vector3.Lerp(startPosition, targetPosition, progress);
        
        // Sin 곡선을 이용하여 중앙에서 최고 높이(arcHeight)에 도달하는 Y축 오프셋 계산
        float arcOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
        
        transform.position = new Vector3(currentLinearPos.x, currentLinearPos.y + arcOffset, currentLinearPos.z);
    }

    /// <summary>
    /// 카드가 항상 카메라를 정면으로 바라보도록 회전 (2.5D 비주얼 보장)
    /// </summary>
    private void AlignToCamera()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    /// <summary>
    /// 적에 적중했을 때 처리
    /// </summary>
    private void OnHitTarget()
    {
        Debug.Log($"[CardProjectile] 카드 적중! 타겟: {(targetTransform != null ? targetTransform.name : "파괴된 적")}");
        
        // 우선은 시각적 확인을 위해 Destroy 처리 (추후 오브젝트 풀링으로 전환 예정)
        Destroy(gameObject);
    }
}