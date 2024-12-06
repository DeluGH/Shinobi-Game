using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeybindManager : MonoBehaviour
{
    public InputActionAsset inputActions; // Reference to your InputActionAsset

    private const string KeybindsSaveKey = "Keybinds";

    private void Awake()
    {
        LoadKeybinds();
    }

    // Save current keybinds to PlayerPrefs
    public void SaveKeybinds()
    {
        if (inputActions == null) return;

        string keybindsJson = inputActions.ToJson();
        PlayerPrefs.SetString(KeybindsSaveKey, keybindsJson);
        PlayerPrefs.Save();
        Debug.Log("Keybinds saved.");
    }

    // Load keybinds from PlayerPrefs
    public void LoadKeybinds()
    {
        if (inputActions == null) return;

        string keybindsJson = PlayerPrefs.GetString(KeybindsSaveKey, string.Empty);
        if (!string.IsNullOrEmpty(keybindsJson))
        {
            inputActions.LoadFromJson(keybindsJson);
            Debug.Log("Keybinds loaded.");
        }
        else
        {
            Debug.LogWarning("No saved keybinds found.");
        }
    }

    // Reset keybinds to default
    public void ResetKeybinds()
    {
        if (inputActions == null) return;

        inputActions.RemoveAllBindingOverrides();
        Debug.Log("Keybinds reset to default.");
    }

    // Start the rebinding process for a specific action and binding index
    public void StartRebinding(InputAction action, Button button)
    {
        if (action == null || button == null) return;

        button.GetComponentInChildren<Text>().text = "Press a key...";

        action.PerformInteractiveRebinding()
            .OnComplete(callback =>
            {
                int bindingIndex = action.GetBindingIndexForControl(callback.selectedControl); // Get the binding index
                Debug.Log($"Rebinding complete: {action.bindings[bindingIndex].path}");
                button.GetComponentInChildren<Text>().text = action.GetBindingDisplayString(bindingIndex);
                SaveKeybinds(); // Save changes after rebinding
                callback.Dispose();
            })
            .Start();
    }
}

//using UnityEditor.Timeline.Actions;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerAttack : MonoBehaviour
//{
//    public InputActionMap map;

//    public InputAction attackAction;

//    void Start()
//    {
//        // Ensure the InputActionMap is enabled
//        if (map != null)
//        {
//            map.Enable();
//            attackAction = map.FindAction("Attack"); // Find the "Attack" action
//        }

//        if (attackAction == null)
//        {
//            Debug.LogError("Attack action not found in the assigned InputActionMap!");
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // Check if the attack action is triggered
//        if (attackAction != null && attackAction.triggered)
//        {
//            PerformAttack();
//        }
//    }

//    private void PerformAttack()
//    {
//        Debug.Log("Attack performed!");
//        // Add attack logic here (e.g., animations, damage application)
//    }
//}
