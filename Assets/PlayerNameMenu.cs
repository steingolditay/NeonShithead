using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameMenu : MonoBehaviour
{

    [SerializeField] private MainMenu mainMenu;
    
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button setButton;

    private string playerName = "";
    
    // Start is called before the first frame update
    void Start()
    {
        setButton.onClick.AddListener(SetName);

        inputField.characterLimit = 12;
        inputField.onValueChanged.AddListener(ValidateInput);
        setButton.interactable = false;
    }

    private void ValidateInput(string input)
    {
        playerName = input;
        setButton.interactable = input.Length >= 2;
    }

    private void SetName()
    {
        Utils.SetPlayerName(playerName);
        mainMenu.OnPlayerNameSet();
    }

}
