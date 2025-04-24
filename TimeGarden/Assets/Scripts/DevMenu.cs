using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DevMenu : MonoBehaviour
{
    private InputAction action;
    [SerializeField] private GameObject devMenu;
    [SerializeField] private TMP_Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        action = InputSystem.actions.FindActionMap("Player").FindAction("Toggle Dev");
    }
    private void OnEnable()
    {
        action.performed += OnToggle;
    }
    private void OnDisable()
    {
        action.performed -= OnToggle;
    }
    void OnToggle(InputAction.CallbackContext context)
    {
        devMenu.SetActive(!devMenu.activeInHierarchy);
    }
    public void SetMultiplier(float multiplier)
    {
        InputHandler.SetMultiplier(multiplier);
        text.text = multiplier.ToString("F2");
    }
}
