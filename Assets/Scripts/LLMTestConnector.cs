using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class LLMTestConnector : MonoBehaviour
{
    // mirrored networking mode 적용 후의 실제 Wi-Fi IP + SGLang 포트
    //private string serverUrl = "http://192.168.0.31:30000/v1/chat/completions";
    private string serverUrl = "https://glass-voted-connecting-powered.trycloudflare.com/v1/chat/completions"; //quicktunnel클라우드 서버 연동

    void Start()
    {
        StartCoroutine(SendTestMessage());
    }

    IEnumerator SendTestMessage()
    {
        var payload = new
        {
            model = "default",
            messages = new[]
            {
                new { role = "user", content = "안녕! 너는 카드게임 NPC야. 짧게 한 문장으로 인사해줘." }
            },
            temperature = 0.7
        };

        string jsonBody = JsonConvert.SerializeObject(payload);
        Debug.Log("[요청 보냄] " + jsonBody);

        var request = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[서버 원본 응답] " + request.downloadHandler.text);

            var response = JObject.Parse(request.downloadHandler.text);
            string reply = response["choices"][0]["message"]["content"].ToString();

            Debug.Log("<color=cyan>[NPC 응답]</color> " + reply);
        }
        else
        {
            Debug.LogError("[통신 실패] " + request.error);
            Debug.LogError("[상세] " + request.downloadHandler.text);
        }
    }
}
