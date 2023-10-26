
using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{

    private Color32 originalColor = new Color32(67, 190, 186, 255);
    private Color32 selectedColor = new Color32(190, 110, 67, 255);

    [SerializeField] private Button lobbyButton;
    [SerializeField] private TextMeshProUGUI lobbyName;

    private Lobby lobby;
    private string joinCode;
    
    private JoinLobbyMenu joinLobbyMenu;
    private Image background;
    private bool isSelected = false;

    private void Awake()
    {
        background = transform.GetComponent<Image>();
        background.color = originalColor;
        
        lobbyButton.onClick.AddListener(ToggleSelection);
        joinLobbyMenu = GameObject.Find("JoinLobbyMenu").GetComponent<JoinLobbyMenu>();
    }

    public void SetName(string lobbyName)
    {
        this.lobbyName.text = lobbyName;
    }

    public void SetLobby(Lobby lobby, string joinCode)
    {
        this.lobby = lobby;
        this.joinCode = joinCode;
    }

    public string GetJoinCode()
    {
        return joinCode;
    }

    public Lobby GetLobby()
    {
        return lobby;
    }

    private void ToggleSelection()
    {
        isSelected = !isSelected;
        background.color = isSelected ? selectedColor : originalColor;
        
        joinLobbyMenu.SetLobbyItem(isSelected ? this : null);

    }
}
