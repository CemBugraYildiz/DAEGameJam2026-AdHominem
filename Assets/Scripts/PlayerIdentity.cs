using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerColor
{
    Red,
    Blue,
    Green,
    Yellow
}

public class PlayerIdentity : MonoBehaviour
{
    [field: SerializeField] public PlayerColor Color { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    public bool IsJoined
    {
        get
        {
            if (playerInput == null)
                return gameObject.activeInHierarchy;

            return playerInput.inputIsActive && playerInput.devices.Count > 0;
        }
    }

    private void Reset()
    {
        playerInput = GetComponent<PlayerInput>();
    }
}