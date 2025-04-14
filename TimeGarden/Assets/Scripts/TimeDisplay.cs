using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    void Update()
    {
        text.text = InputHandler.Time.ToString("F2");
    }
}
