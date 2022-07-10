using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class CarUIManager : NetworkBehaviour
{
    public static event Action<string, ulong> NameChanged;

    [SerializeField] private InputManager input;
    [SerializeField] CarController carController;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Toggle transmissionToggle;
    [SerializeField] private GameObject dashboardUI;
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPauseMenuActive = false;

    public override void OnNetworkSpawn()
    {
        input.controls.Main.PauseMenu.performed += ChangeUIStateInput;
        transmissionToggle.isOn = carController.isAutomatic;
    }

    private void OnDisable()
    {
        input.controls.Main.PauseMenu.performed -= ChangeUIStateInput;
    }

    public override void OnNetworkDespawn()
    {
        input.controls.Main.PauseMenu.performed -= ChangeUIStateInput;
    }

    private void ChangeUIStateInput(InputAction.CallbackContext ctx)
    {
        ChangeUIState();
    }

    public void ChangeUIState()
    {
        if (!isPauseMenuActive)
        {
            isPauseMenuActive = true;
            dashboardUI.SetActive(false);
            pauseMenuUI.SetActive(true);
        }
        else if (isPauseMenuActive)
        {
            isPauseMenuActive = false;
            pauseMenuUI.SetActive(false);
            dashboardUI.SetActive(true);
        }
    }

    public void ChangeTransmission(bool input)
    {
        carController.isAutomatic = input;
    }

    public void SetName()
    {
        NameChanged?.Invoke(nameInput.text, OwnerClientId);
    }
}
