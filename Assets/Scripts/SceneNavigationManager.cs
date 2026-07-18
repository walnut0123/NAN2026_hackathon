using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneNavigationManager : MonoBehaviour
{
    private static SceneNavigationManager instance;

    private Canvas mainCanvas;
    private GameObject btnGoToLLMTestObj;
    private GameObject btnGoToMainObj;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CreateNavigationCanvas();
        CreateNavigationButtons();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateButtonVisibility(scene.name);
    }

    void CreateNavigationCanvas()
    {
        GameObject canvasObj = new GameObject("SceneNavigationCanvas");
        canvasObj.transform.SetParent(this.transform);

        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 999; 

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CreateNavigationButtons()
    {
        // 1. LLMTest 씬으로 가는 버튼 생성
        btnGoToLLMTestObj = CreateButton(
            "Btn_GoToLLMTest", 
            "Go to LLMTest", 
            () => SceneManager.LoadScene("LLMTest")
        );

        // 2. Main 씬으로 가는 버튼 생성
        btnGoToMainObj = CreateButton(
            "Btn_GoToMain", 
            "Go to Main", 
            () => SceneManager.LoadScene("Main")
        );

        // [핵심 수정] 두 버튼 모두 좌측 상단 완전히 동일한 위치(X: 40, Y: -40)에 겹쳐서 배치
        // 어차피 한 번에 하나만 켜지므로 위치가 정확히 일치해야 전환 시 이질감이 없습니다.
        Vector2 buttonSize = new Vector2(300, 100);
        SetupButtonTransform(btnGoToLLMTestObj.GetComponent<RectTransform>(), buttonSize, 40f, -40f);
        SetupButtonTransform(btnGoToMainObj.GetComponent<RectTransform>(), buttonSize, 40f, -40f);

        // 초기화 시점 가시성 설정
        UpdateButtonVisibility(SceneManager.GetActiveScene().name);
    }

    void UpdateButtonVisibility(string sceneName)
    {
        if (btnGoToLLMTestObj == null || btnGoToMainObj == null) return;

        // Main 씬에서는 LLMTest로 가는 버튼만 활성화
        if (sceneName == "Main")
        {
            btnGoToLLMTestObj.SetActive(true);
            btnGoToMainObj.SetActive(false);
        }
        // LLMTest 씬에서는 Main으로 가는 버튼만 활성화
        else if (sceneName == "LLMTest")
        {
            btnGoToLLMTestObj.SetActive(false);
            btnGoToMainObj.SetActive(true);
        }
        else
        {
            // 그 외 예외적인 씬 세팅일 경우 둘 다 꺼둠
            btnGoToLLMTestObj.SetActive(false);
            btnGoToMainObj.SetActive(false);
        }
    }

    GameObject CreateButton(string objName, string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject btnObj = new GameObject(objName);
        btnObj.transform.SetParent(mainCanvas.transform, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); 

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClickAction);

        // 텍스트 생성
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        Text txt = textObj.AddComponent<Text>();
        txt.text = buttonText;
        
        // 폰트 에러 안전 버퍼 (null 설정 시 기본 내장 Arial 자동 매핑)
        if (txt.font == null)
        {
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        }
        
        txt.fontSize = 32; 
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white; 

        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        return btnObj;
    }

    void SetupButtonTransform(RectTransform rt, Vector2 size, float xOffset, float yOffset)
    {
        // 최상단 좌측(Top-Left) 고정 앵커 및 피벗 설정
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(xOffset, yOffset);
    }
}