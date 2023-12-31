
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{



    private Lobby createdLobby;
    private float hearbeatTimer = 15;
    private float heartbeatCountdown = 15;
    private float lobbyUpdateTimer = 1.1f;
    private float lobbyUpdateCountdown = 1.1f;
    private int maxNumberOfPlayers = 2;


    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in");
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        if (createdLobby != null)
        {
            heartbeatCountdown -= Time.deltaTime;
            lobbyUpdateCountdown -= Time.deltaTime;
            
            if (heartbeatCountdown <= 0)
            {
                SendHeartbeat();
            }

            if (lobbyUpdateCountdown <= 0)
            {
                UpdateLobby();
            }
        }
    }

    private async void SendHeartbeat()
    {
        heartbeatCountdown = hearbeatTimer;
        await LobbyService.Instance.SendHeartbeatPingAsync(createdLobby.Id);
    }

    private async void UpdateLobby()
    {
        lobbyUpdateCountdown = lobbyUpdateTimer;
        try
        {
            createdLobby = await LobbyService.Instance.GetLobbyAsync(createdLobby.Id);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void CreateLobby(string lobbyName)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxNumberOfPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort) allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            NetworkManager.Singleton.StartHost();

            string playerName = Utils.GetPlayerName();
            CreateLobbyOptions option = new CreateLobbyOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { Utils.DATA_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    { Utils.DATA_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };
            createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxNumberOfPlayers, option);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void CloseLobby()
    {
        try
        {
            NetworkManager.Singleton.Shutdown();
            await LobbyService.Instance.DeleteLobbyAsync(createdLobby.Id);
            createdLobby = null;
            Debug.Log("Lobby Closed");

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async Task<QueryResponse> GetLobbies()
    {
        return await Lobbies.Instance.QueryLobbiesAsync();
    }
    
    public async void JoinLobby(LobbyItem lobbyItem)
    {
        try
        {
            string playerName = Utils.GetPlayerName();
            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { Utils.DATA_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                    }
                }
            };
            
            createdLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyItem.GetLobby().Id, joinLobbyByCodeOptions); 
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(lobbyItem.GetJoinCode());
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort) joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);
            
            NetworkManager.Singleton.StartClient();
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public Lobby GetCurrentLobby()
    {
        return createdLobby;
    }




}
