using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Start()
    {
        // 생성될 때 메인 카메라의 회전값과 일치시켜 정면을 보게 만듭니다.
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}