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
    public TextMeshProUGUI debugTextOne;
    public TextMeshProUGUI debugTextTwo;

    public bool isActive { get; set; }

    /////////////////////////////////////////////////////////////////////////////////////
    // PRIVATE FIELDS
    /////////////////////////////////////////////////////////////////////////////////////

    CharacterController playerController;

    Vector2 offsetBetweenTexts;
    Vector2 startPosition;

    Dictionary<string, TextMeshProUGUI> debugTexts = new Dictionary<string, TextMeshProUGUI>();

    /////////////////////////////////////////////////////////////////////////////////////
    
    public void Initialize()
    {
        playerController = GetComponent<CharacterController>();

        // Get the offset between the two texts
        offsetBetweenTexts = debugTextTwo.rectTransform.anchoredPosition - debugTextOne.rectTransform.anchoredPosition;
        startPosition = debugTextOne.rectTransform.anchoredPosition;

        // Disable the texts
        debugTextOne.gameObject.SetActive(false);
        debugTextTwo.gameObject.SetActive(false);

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

    public void SetDebugText(string key, string value)
    {
        if (!debugTexts.ContainsKey(key))
        {
            // Create a new text
            TextMeshProUGUI newText = Instantiate(debugTextOne, debugTextOne.transform.parent);
            newText.rectTransform.anchoredPosition = startPosition + offsetBetweenTexts * debugTexts.Count;
            newText.text = key + ": " + value;
            newText.gameObject.SetActive(true);

            // Add it to the dictionary
            debugTexts.Add(key, newText);
        }
        else
        {
            // Update the text
            debugTexts[key].text = key + ": " + value;
        }
    }
}
