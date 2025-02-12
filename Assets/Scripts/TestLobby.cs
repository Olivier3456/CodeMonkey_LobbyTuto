using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartBeatTimer = 0f;
    private string playerName;


    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Le Joueur Pro " + Random.Range(0, 9999);
        Debug.Log($"Player Name: {playerName}.");
    }


    public async Task CreateLobby()
    {
        // Attention, les opérations liées aux lobbies peuvent créer des erreurs.
        // Pour que ça ne casse pas tout le programme, on les fait toujours en try-catch.
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {                    
                    // La visibilité doit être publique pour pouvoir être vue de l'extérieur du lobby.
                    {"Game Mode", new DataObject(DataObject.VisibilityOptions.Public, "Capture The Flag")}
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;

            Debug.Log($"Created Lobby {lobbyName} with game mode: {lobby.Data["Game Mode"].Value}. Max players: {maxPlayers}. Available slots left: {hostLobby.AvailableSlots}. Lobby Id is: {lobby.Id}. Lobby Code is: {lobby.LobbyCode}.");
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        // Member = le joueur sera vu par les membres du lobby
                        // playerName est une string qu'on a ajoutée au début de notre classe
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                    }
        };
    }


    public async void ListLobbies()
    {
        try
        {
            // Voir les classes QueryLobbiesOptions et QueryFilter pour avoir plus de détails
            // sur les options possibles.
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                // nombre de résultats à retourner
                Count = 25,

                Filters = new List<QueryFilter>
                {
                    // GT = greater than, donc notre filtre = plus que 0 places disponibles
                    // (la valeur (ici 0) doit être au format string)
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,
                                    "0",
                                    QueryFilter.OpOptions.GT),
                },

                Order = new List<QueryOrder>
                {
                    // false pour ascending, Created pour date de création
                    // donc ici, on classe par ordre descendant de création
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
                Debug.Log($"Lobby name: {lobby.Name} with game mode: {lobby.Data["Game Mode"].Value}.");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            Debug.Log($"Joined Lobby with code {lobbyCode}.");
            PrintPlayers(joinedLobby);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            Lobby joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            // (on peut ajouter à cette fonction des QuickJoinLobbyOptions)

            Debug.Log("Lobby Quick Joined.");
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in Lobby {lobby.Name} with game mode: {lobby.Data["Game Mode"].Value}.");
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
    public void PrintPlayers()
    {
        if (hostLobby == null)
        {
            Debug.Log("Can't print players, hostLobby is null!");
            return;
        }
        PrintPlayers(hostLobby);
    }


    private void Update()
    {
        HandlerLobbyHeartBeat();
    }
    private async void HandlerLobbyHeartBeat()
    {
        if (hostLobby == null)
        {
            return;
        }

        heartBeatTimer -= Time.deltaTime;

        if (heartBeatTimer < 0f)
        {
            float heartBeatTimerMax = 15f;
            heartBeatTimer = heartBeatTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }
}
