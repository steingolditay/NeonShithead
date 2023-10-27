

using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{

    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private LobbyManager lobbyManager;
    
    [SerializeField] private GameObject lobbyItem;
    [SerializeField] private Transform lobbyList;

    private LobbyItem selectedLobbyItem;

    void Start()
    {
        backButton.onClick.AddListener(mainMenu.ExitLobbyMenu);
        joinButton.onClick.AddListener(JoinLobby);
        refreshButton.onClick.AddListener(RefreshLobbyList);
    }
    
    public void SetLobbyItem(LobbyItem item)
    {
        selectedLobbyItem = item;
        ToggleJoinButton(selectedLobbyItem != null);
    }


    private void ToggleJoinButton(bool state)
    {
        joinButton.interactable = state;
    }

    public async void RefreshLobbyList()
    {
        ClearLobbyList();
        QueryResponse lobbiesQuery = await lobbyManager.GetLobbies();
        
        foreach (Lobby lobby in lobbiesQuery.Results)
        {
            string joinCode = lobby.Data[Utils.DATA_JOIN_CODE].Value;
            GameObject lobbyObject = Instantiate(lobbyItem, lobbyList);
            LobbyItem item = lobbyObject.GetComponent<LobbyItem>();
            item.SetName(lobby.Name);
            item.SetLobby(lobby, joinCode);
        }
    }

    public void ClearLobbyList()
    {
        foreach (Transform item in lobbyList)
        {
            Destroy(item.gameObject);
        }
    }

    private void JoinLobby()
    {
        lobbyManager.JoinLobby(selectedLobbyItem);
    }
}
