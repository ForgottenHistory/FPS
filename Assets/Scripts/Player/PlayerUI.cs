using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerUI : MonoBehaviour, IInitialize
{
    /////////////////////////////////////////////////////////////////////////////////////
    // PUBLIC FIELDS 
    /////////////////////////////////////////////////////////////////////////////////////
    
    public GameObject debugUI;
    public TextMeshProUGUI debugSpeedText;
    public TextMeshProUGUI debugVelocityText;
    public TextMeshProUGUI debugVerticalVelocityText;

    public bool isActive { get; set; }

    /////////////////////////////////////////////////////////////////////////////////////
    // PRIVATE FIELDS
    /////////////////////////////////////////////////////////////////////////////////////

    CharacterController playerController;

    /////////////////////////////////////////////////////////////////////////////////////
    
    public void Initialize()
    {
        playerController = GetComponent<CharacterController>();

        isActive = true;
    }

    public void Deinitialize()
    {
        isActive = false;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void SetDebugUI(bool state)
    {
        debugUI.SetActive(state);
        isActive = state;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    
    public void SetDebugSpeedText(float speed)
    {
        debugSpeedText.text = "Speed: " + speed.ToString("F2");
    }

    /////////////////////////////////////////////////////////////////////////////////////
    
    public void SetDebugVelocityText(Vector3 velocity)
    {
        debugVelocityText.text = "Velocity: " + velocity.ToString("F2");
    }

    /////////////////////////////////////////////////////////////////////////////////////
    
    public void SetDebugVerticalVelocityText(float verticalVelocity)
    {
        debugVerticalVelocityText.text = "Vertical Velocity: " + verticalVelocity.ToString("F2");
    }

    /////////////////////////////////////////////////////////////////////////////////////
}
