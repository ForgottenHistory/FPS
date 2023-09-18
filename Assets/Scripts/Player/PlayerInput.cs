using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour, IInitialize
{
    /////////////////////////////////////////////////////////////////////////////////////
    // PUBLIC FIELDS 
    /////////////////////////////////////////////////////////////////////////////////////

    public bool isActive { get; set; }

    /////////////////////////////////////////////////////////////////////////////////////
    // PRIVATE FIELDS
    /////////////////////////////////////////////////////////////////////////////////////

    PlayerMovement playerMovement;
    PlayerUI playerUI;

    /////////////////////////////////////////////////////////////////////////////////////
    // INITIALIZATION & DEINITIALIZATION
    /////////////////////////////////////////////////////////////////////////////////////

    public void Initialize()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerUI = GetComponent<PlayerUI>();

        isActive = true;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void Deinitialize()
    {
        isActive = false;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // UPDATES
    /////////////////////////////////////////////////////////////////////////////////////

    void Update()
    {
        if (isActive == false) return;

        MovementInput();
        DebugInput();
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // MOVEMENT INPUTS
    /////////////////////////////////////////////////////////////////////////////////////

    void MovementInput()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && playerMovement.CanJump())
        {
            playerMovement.Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            playerMovement.Crouch();
        }

        playerMovement.isSprinting = Input.GetKey(KeyCode.LeftShift);

        playerMovement.CheckWallJumpTimer();
        x = Mathf.Clamp(x, -playerMovement.leftInputMultiplier, playerMovement.rightInputMultiplier);
        playerMovement.MovePlayer(x, z);
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // DEBUG INPUTS
    /////////////////////////////////////////////////////////////////////////////////////

    void DebugInput()
    {
        ToggleDebugUI();
        ResetPosition();
    }

    void ToggleDebugUI()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            playerUI.SetDebugUI(!playerUI.isActive);
        }
    }

    void ResetPosition()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerMovement.Reset();
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////
}
