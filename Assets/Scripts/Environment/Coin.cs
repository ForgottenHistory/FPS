using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Coin : MonoBehaviour
{   
    public GameManager gameManager { get; set; } = null;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            gameManager.CollectCoin();
            Destroy(gameObject);
        }
    }
}
