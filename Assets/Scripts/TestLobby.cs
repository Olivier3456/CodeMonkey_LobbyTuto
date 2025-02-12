using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // (J'appelle cette fonction ici juste pour le test.)
        await CreateLobby();
    }


    private async Task CreateLobby()
    {
        // Attention, les opérations liées aux lobbies peuvent créer des erreurs.
        // Pour que ça ne casse pas tout le programme, on les fait en try-catch.
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            Debug.Log($"Created Lobby {lobbyName}, max players: {maxPlayers}.");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
