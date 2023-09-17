using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////
    // PUBLIC FIELDS 
    /////////////////////////////////////////////////////////////////////////////////////

    public GameObject player;
    public CameraController playerCamera;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI winText;

    /////////////////////////////////////////////////////////////////////////////////////
    // PRIVATE FIELDS
    /////////////////////////////////////////////////////////////////////////////////////

    PlayerMovement playerMovement;
    PlayerUI playerUI;
    PlayerInput playerInput;

    float coinsOnMap = 0;
    float coinsCollected = 0;

    /////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        // Find player components
        playerMovement = player.GetComponent<PlayerMovement>();
        playerUI = player.GetComponent<PlayerUI>();
        playerInput = player.GetComponent<PlayerInput>();

        // Initialize player components
        playerCamera.Initialize();
        playerMovement.Initialize();
        playerInput.Initialize();
        playerUI.Initialize();

        playerUI.SetDebugUI(true);

        // Find all coins on map and set their game manager
        coinsOnMap = GameObject.FindGameObjectsWithTag("Coin").Length;
        foreach (GameObject coin in GameObject.FindGameObjectsWithTag("Coin"))
        {
            coin.GetComponent<Coin>().gameManager = this;
        }

        // Update coins text
        coinsText.text = "Coins: " + coinsCollected + "/" + coinsOnMap;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    public void CollectCoin()
    {
        // Update coins text
        coinsCollected++;
        coinsText.text = "Coins: " + coinsCollected + "/" + coinsOnMap;

        // Win state
        if (coinsCollected == coinsOnMap)
        {
            Win();
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void Win()
    {
        Destroy(coinsText.gameObject);
        winText.gameObject.SetActive(true);
    }
    
    /////////////////////////////////////////////////////////////////////////////////////
}
