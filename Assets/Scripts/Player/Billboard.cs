using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    // 유니티 기본 Plane의 Y축 세움 보정을 위한 90도 회전 오프셋
    private readonly Quaternion rotationOffset = Quaternion.Euler(90f, 0f, 0f);

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // 카메라 회전값에 90도 오프셋을 곱해 Plane을 세워서 카메라를 바라보게 함
            transform.rotation = mainCameraTransform.rotation * rotationOffset;
        }
    }
}