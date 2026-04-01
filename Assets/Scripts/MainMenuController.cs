using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject buttonsRoot;
    [SerializeField] private GameObject backgroundToHide;
    [SerializeField] private GameObject controlsPanel;

    [Header("First Selection")]
    [SerializeField] private Selectable startButton;

    [Header("Players")]
    [SerializeField] private PlayerInput[] playerInputs;

    private bool controlsOpen;
    private bool startingGame;

    public bool IsMainMenuOpen => mainMenuPanel != null && mainMenuPanel.activeSelf;

    private void Start()
    {
        OpenMainMenu();
    }

    private void Update()
    {
        if (controlsOpen && AnyCancelPressed())
        {
            BackFromControls();
        }
    }

    public void OpenMainMenu()
    {
        RefreshPlayers();

        Time.timeScale = 0f;

        SetPlayersInputActive(false);
        BlockAllPlayersJumpUntilRelease();

        mainMenuPanel.SetActive(true);
        buttonsRoot.SetActive(true);

        if (backgroundToHide != null)
            backgroundToHide.SetActive(true);

        controlsPanel.SetActive(false);
        controlsOpen = false;

        StartCoroutine(SelectStartButtonNextFrame());
    }

    public void StartGame()
    {
        if (startingGame) return;
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        startingGame = true;

        mainMenuPanel.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        BlockAllPlayersJumpUntilRelease();

        yield return null;

        while (AnySubmitHeld())
            yield return null;

        yield return null;

        Time.timeScale = 1f;
        SetPlayersInputActive(true);

        startingGame = false;
    }

    public void OpenControls()
    {
        buttonsRoot.SetActive(false);

        if (backgroundToHide != null)
            backgroundToHide.SetActive(false);

        controlsPanel.SetActive(true);
        controlsOpen = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void BackFromControls()
    {
        controlsPanel.SetActive(false);

        if (backgroundToHide != null)
            backgroundToHide.SetActive(true);

        buttonsRoot.SetActive(true);
        controlsOpen = false;

        StartCoroutine(SelectStartButtonNextFrame());
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator SelectStartButtonNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (EventSystem.current != null && startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            startButton.Select();
        }
    }

    private void SetPlayersInputActive(bool active)
    {
        RefreshPlayers();

        foreach (var player in playerInputs)
        {
            if (player == null) continue;

            if (active)
            {
                player.ActivateInput();
                player.SwitchCurrentActionMap("Player");
            }
            else
            {
                player.DeactivateInput();
            }
        }
    }

    private void RefreshPlayers()
    {
        if (playerInputs == null || playerInputs.Length == 0)
        {
            playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        }
    }

    private bool AnySubmitHeld()
    {
        foreach (var pad in Gamepad.all)
        {
            if (pad != null && pad.buttonSouth.isPressed)
                return true;
        }
        return false;
    }

    private bool AnyCancelPressed()
    {
        foreach (var pad in Gamepad.all)
        {
            if (pad != null && pad.buttonEast.wasPressedThisFrame)
                return true;
        }
        return false;
    }

    private void BlockAllPlayersJumpUntilRelease()
    {
        RefreshPlayers();

        foreach (var player in playerInputs)
        {
            if (player == null) continue;

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.BlockJumpUntilRelease();
            }
        }
    }
}