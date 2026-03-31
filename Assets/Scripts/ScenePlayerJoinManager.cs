using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenePlayerJoinManager : MonoBehaviour
{
    [SerializeField] private PlayerInput[] scenePlayers;

    private IEnumerator Start()
    {
        yield return null;

        ValidateScenePlayers();
        RebuildAssignments();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad)
            return;

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Reconnected:
                RebuildAssignments();
                break;
        }
    }

    [ContextMenu("Rebuild Assignments")]
    private void RebuildAssignments()
    {
        foreach (var player in scenePlayers)
        {
            if (player == null)
                continue;

            player.neverAutoSwitchControlSchemes = true;
            player.DeactivateInput();

            if (player.user.valid)
                player.user.UnpairDevices();
        }

        int count = Mathf.Min(Gamepad.all.Count, scenePlayers.Length);

        for (int i = 0; i < count; i++)
        {
            var player = scenePlayers[i];
            var gamepad = Gamepad.all[i];

            if (player == null || gamepad == null)
                continue;

            player.ActivateInput();
            player.SwitchCurrentControlScheme("Gamepad", gamepad);

            Debug.Log($"{gamepad.displayName} -> {player.gameObject.name}");
        }

        for (int i = count; i < scenePlayers.Length; i++)
        {
            if (scenePlayers[i] != null)
                Debug.Log($"{scenePlayers[i].gameObject.name} is empty");
        }
    }

    private void ValidateScenePlayers()
    {
        HashSet<int> ids = new HashSet<int>();

        for (int i = 0; i < scenePlayers.Length; i++)
        {
            var player = scenePlayers[i];
            int id = player.GetInstanceID();

        }
    }
}