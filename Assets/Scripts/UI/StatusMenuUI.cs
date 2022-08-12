using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class StatusMenuUI : MonoBehaviour
{
    public GameObject yesPanel;
    public GameObject noPanel;
    public StatusMenuState statusMenuState;
    PlayerInput playerInput;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    public void Start() 
    {
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    void EnablePlayerInput()
    {
        playerInput.Enable();
    }

    void DisablePlayerInput()
    {
        playerInput.Disable();
    }

    public bool HandleKeyPress()
    {
        bool keyPressed = false;

        if (playerInput.Controls.InputCancel.triggered || playerInput.Controls.InputMenu.triggered)
        {
            stateHandler.ChangeState(StateHandler.State.CombatState);
            keyPressed = true;
        }

        if (playerInput.Controls.InputJoystick.ReadValue<Vector2>().x != 0)
        {
            shouldQuit = !shouldQuit;
            int alpha = 0;
            if (shouldQuit) alpha = 1;
            yesPanel.GetComponent<CanvasGroup>().alpha = alpha;
            noPanel.GetComponent<CanvasGroup>().alpha = 1 - alpha;
            keyPressed = true;
        }

        if (playerInput.Controls.InputSubmit.triggered)
        {
            Debug.Log(shouldQuit);
            if (shouldQuit)
            {
                // UnityEditor.EditorApplication.isPlaying = false;
                Application.Quit();
            }
            else stateHandler.ChangeState(StateHandler.State.CombatState);
        }
        return keyPressed;
    }

    public void DisplayMenu(bool visible)
    {
        int alpha = 1;
        if (!visible) alpha = 0;
        GetComponentInParent<GraphicRaycaster>().enabled = visible;
        gameObject.GetComponent<CanvasGroup>().alpha = alpha;
        yesPanel.GetComponent<CanvasGroup>().alpha = alpha;
        noPanel.GetComponent<CanvasGroup>().alpha = 0;
        shouldQuit = true;
    }
}
