using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //WASD 조작용 패키지


public class Player : MonoBehaviour
{
    // Start is called before the first frame update

    //commit test
    private AgentMover AgentMover; //플레이어 이동코드를 별도로 관리.

    private Vector3 pointerInput;
    private Vector2 movementInput;

    public Vector3 PointerInput => pointerInput;
    //private WeaponParent weaponParent;

    [SerializeField]
    private InputActionReference movement, shoot, pointerPosition;

    void Awake()
    {
        //weaponParent = GetComponentinChildren<AgentAnimations>();
        AgentMover = GetComponent<AgentMover>();
    }

    // Update is called once per frame
    void Update()
    {
        pointerInput = GetPointerInput();
        movementInput = movement.action.ReadValue<Vector2>();

        Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        AgentMover.MovementInput = moveVector;
    }
    

    private Vector3 GetPointerInput()
    {
        // 화면의 마우스 2D 좌표 읽기
        Vector2 mouseScreenPos = pointerPosition.action.ReadValue<Vector2>();
        
        // 마우스 위치에서 3D 공간으로 나아가는 레이(Ray) 생성
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        
        // 레이가 3D 공간의 물체(예: 바닥)와 부딪혔는지 검사
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
        {
            // 부딪힌 지점의 3D 월드 좌표 반환
            return hitInfo.point;
        }

        // 부딪힌 곳이 없다면 기본 Vector3.zero 반환 (혹은 이전 값 유지)
        return Vector3.zero;
    }



    
}


