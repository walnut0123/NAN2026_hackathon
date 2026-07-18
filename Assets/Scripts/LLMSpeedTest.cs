using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;

public class LLMSpeedTest : MonoBehaviour
{
    // Gist에 저장된 "최신 리비전" 고정 주소 (커밋 해시 없는 버전 - 항상 최신 내용을 반환함)
    private const string GIST_RAW_URL = "https://gist.githubusercontent.com/walnut0123/a0e016f97ecd1ac3588749b710584140/raw/tunnel_url";

    private string serverUrl = ""; // 앱 시작 시 Gist에서 자동으로 채워짐
    private bool serverUrlReady = false;


    private string inputText = "카드게임 규칙을 한 문장으로 설명해줘";
    private string lastReply = "";
    private string statusText = "대기 중";
    private bool isWaiting = false;

    private GUIStyle labelStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle buttonStyle;
    private bool stylesInitialized = false;

    // 화면 크기 기준(1080x2340, S24)으로 비율 계산 후 실제 Screen.width/height에 맞춰 스케일링
    private void InitStyles()
    {
        float scaleFactor = Screen.width / 1080f; // 기준 해상도 대비 배율

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(48 * scaleFactor)
        };
        labelStyle.normal.textColor = Color.white;

        textFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = Mathf.RoundToInt(48 * scaleFactor)
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.RoundToInt(52 * scaleFactor)
        };

        stylesInitialized = true;
    }

    void Start()
    {
        StartCoroutine(FetchServerUrl());
    }

    // 앱 시작 시 Gist에서 최신 서버 주소를 읽어옴 (캐시 방지를 위해 타임스탬프 쿼리 파라미터 부착)
    IEnumerator FetchServerUrl()
    {
        statusText = "서버 주소 조회 중...";

        string cacheBustUrl = GIST_RAW_URL + "?t=" + System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        UnityWebRequest req = UnityWebRequest.Get(cacheBustUrl);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string url = req.downloadHandler.text.Trim();
            if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
            {
                serverUrl = url;
                serverUrlReady = true;
                statusText = "준비 완료";
                Debug.Log($"[서버 주소 로드 완료] {serverUrl}");
            }
            else
            {
                statusText = "서버 주소 형식 오류 (Gist 내용 확인 필요)";
                Debug.LogError($"[Gist 내용 이상] '{url}'");
            }
        }
        else
        {
            statusText = "서버 주소 조회 실패: " + req.error;
            Debug.LogError($"[Gist 조회 실패] {req.error}");
        }
    }

    // 화면에 입력창 + 버튼 그리기 (테스트 전용, 가장 빠르게 만들 수 있는 방식)
    void OnGUI()
    {
        if (!stylesInitialized) InitStyles();

        float w = Screen.width;
        float h = Screen.height;

        // 여백 및 요소 크기를 화면 비율 기준으로 계산 (1080x2340 기준 설계)
        float margin = w * 0.03f;           // 좌우 여백
        float labelHeight = h * 0.035f;      // 라벨 한 줄 높이
        float fieldHeight = h * 0.06f;       // 입력창 높이
        float buttonHeight = h * 0.07f;      // 버튼 높이
        float spacing = h * 0.015f;          // 요소 간 간격

        float y = margin;

        GUI.Label(new Rect(margin, y, w - margin * 2, labelHeight), "메시지 입력:", labelStyle);
        y += labelHeight + spacing;

        inputText = GUI.TextField(new Rect(margin, y, w - margin * 2, fieldHeight), inputText, textFieldStyle);
        y += fieldHeight + spacing;

        GUI.enabled = !isWaiting && serverUrlReady;
        if (GUI.Button(new Rect(margin, y, w * 0.35f, buttonHeight), "전송", buttonStyle))
        {
            StartCoroutine(SendMessageAndMeasure(inputText));
        }
        GUI.enabled = true;
        y += buttonHeight + spacing * 1.5f;

        GUI.Label(new Rect(margin, y, w - margin * 2, labelHeight * 1.5f), "상태: " + statusText, labelStyle);
        y += labelHeight * 1.5f + spacing;

        GUI.Label(new Rect(margin, y, w - margin * 2, h * 0.3f), "응답: " + lastReply, labelStyle);
    }

    IEnumerator SendMessageAndMeasure(string message)
    {
        isWaiting = true;
        statusText = "요청 전송 중...";

        var payload = new
        {
            model = "default",
            messages = new[]
            {
                new { role = "user", content = message }
            },
            temperature = 0.7
        };

        string jsonBody = JsonConvert.SerializeObject(payload);
        var request = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 시간 측정 시작
        Stopwatch stopwatch = Stopwatch.StartNew();

        yield return request.SendWebRequest();

        stopwatch.Stop();
        long elapsedMs = stopwatch.ElapsedMilliseconds;

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JObject.Parse(request.downloadHandler.text);
            string reply = response["choices"][0]["message"]["content"].ToString();

            lastReply = reply;
            statusText = $"성공 | 응답 시간: {elapsedMs} ms ({elapsedMs / 1000.0:F2}초)";

            Debug.Log($"<color=lime>[속도 테스트]</color> {elapsedMs} ms 소요 | 응답: {reply}");
        }
        else
        {
            statusText = $"실패 | 경과 시간: {elapsedMs} ms | 에러: {request.error}";
            Debug.LogError($"[속도 테스트 실패] {elapsedMs} ms 경과 후 실패: {request.error}");
        }

        isWaiting = false;
    }
}