using System.Threading.Tasks;
using UnityEngine;

public class LobbyButtons : MonoBehaviour
{
    [SerializeField] private TestLobby testLobby;

    public async void CreateLobbyButton() => await testLobby.CreateLobby();
    public void ListLobbiesButton() => testLobby.ListLobbies();
    public void JoinLobbyButton(string lobbyCode) => testLobby.JoinLobbyByCode(lobbyCode);
    public void QuickJoinLobbyButton() => testLobby.QuickJoinLobby();
    public void PrintPlayersButton() => testLobby.PrintPlayers();
}
