using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class ScenePlayerJoinManager : MonoBehaviour
{
    [SerializeField] private PlayerInput[] scenePlayers;

    private void Awake()
    {
        foreach (var player in scenePlayers)
        {
            player.DeactivateInput();
            player.user.UnpairDevices();
        }
    }

    private void OnEnable()
    {
        InputUser.listenForUnpairedDeviceActivity++;
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
    }

    private void OnDisable()
    {
        InputUser.listenForUnpairedDeviceActivity--;
        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
    }

    private void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        if (control.device is not Gamepad gamepad)
            return;

        if (control != gamepad.buttonSouth)
            return;

        if (IsGamepadAlreadyAssigned(gamepad))
            return;

        PlayerInput freePlayer = GetFirstFreePlayer();

        if (freePlayer == null)
        {
            Debug.Log("No Empty Space.");
            return;
        }

        freePlayer.SwitchCurrentControlScheme("Gamepad", gamepad);
        freePlayer.ActivateInput();

        Debug.Log($"{gamepad.displayName} -> {freePlayer.gameObject.name} connected");
    }

    private bool IsGamepadAlreadyAssigned(Gamepad gamepad)
    {
        foreach (var player in scenePlayers)
        {
            if (player.devices.Contains(gamepad))
                return true;
        }

        return false;
    }

    private PlayerInput GetFirstFreePlayer()
    {
        foreach (var player in scenePlayers)
        {
            if (player.devices.Count == 0)
                return player;
        }

        return null;
    }
}