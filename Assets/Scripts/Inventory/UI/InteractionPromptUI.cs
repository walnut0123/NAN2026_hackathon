using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private string interactKeyLabel = "E";

    // Discovered automatically instead of serialized, so nothing can be mis-wired
    // by dragging the wrong object in the Inspector.
    private InteractionDetector detector;
    private Text promptText;

    private void Awake()
    {
        promptText = GetComponentInChildren<Text>(true);
    }

    private void Start()
    {
        detector = FindObjectOfType<InteractionDetector>();
        detector.OnPromptChanged += HandlePromptChanged;
        gameObject.SetActive(false);

        UIManager.Register("Interaction Prompt", gameObject);
    }

    private void OnDestroy()
    {
        if (detector != null)
            detector.OnPromptChanged -= HandlePromptChanged;

        UIManager.Unregister("Interaction Prompt");
    }

    private void HandlePromptChanged(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        promptText.text = $"[{interactKeyLabel}] {label} 줍기";
    }
}
