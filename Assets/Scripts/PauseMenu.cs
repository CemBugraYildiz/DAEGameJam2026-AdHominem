using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private PlayerInput[] playerInputs;
    [SerializeField] private MainMenuController mainMenuController;

    private bool isPaused;
    private bool isResuming;

    private void Start()
    {
        pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (mainMenuController != null && mainMenuController.IsMainMenuOpen)
            return;

        if (!isPaused && AnyPlayerPressedPause())
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        SetGameplayInput(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void ResumeGame()
    {
        if (!isPaused || isResuming) return;
        StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine()
    {
        isResuming = true;

        pausePanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);

        yield return null;

        while (AnySouthButtonHeld())
            yield return null;

        Time.timeScale = 1f;
        SetGameplayInput(true);

        isPaused = false;
        isResuming = false;
    }

    private void SetGameplayInput(bool enabled)
    {
        foreach (var player in playerInputs)
        {
            if (player == null) continue;

            var gameplayMap = player.actions.FindActionMap("Player", true);
            if (gameplayMap == null) continue;

            if (enabled)
                gameplayMap.Enable();
            else
                gameplayMap.Disable();
        }
    }

    private bool AnySouthButtonHeld()
    {
        foreach (var pad in Gamepad.all)
        {
            if (pad != null && pad.buttonSouth.isPressed)
                return true;
        }

        return false;
    }
    private bool AnyPlayerPressedPause()
    {
        foreach (Gamepad pad in Gamepad.all)
        {
            if (pad == null) continue;

            if (pad.selectButton.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }
}