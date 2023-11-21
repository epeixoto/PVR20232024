using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    //Controlar o nosso UI
    public TMP_InputField playerNameInput, lobbyCodeInput;
    public GameObject introLobby, lobbyPanel;
    public TMP_Text[] playername;
    public TMP_Text lobbyCodeText;

    //Criar o Lobby e o host para sabar quem
    //é o servidor o o respetivo Lobby Code
    Lobby hostLobby, joinnedLobby;

    // Start is called before the first frame update
    async void Start()
    {
        //Iniciar os serviços da Unity, permitindo estar a espera que os utlizadores entrem no Lobby
        await UnityServices.InitializeAsync();
    }

    async Task Authenticate()
    {
        //Se tiver sign o utlizador não precisa de autenticação
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        //Limpar as sessões de utilizadores que estão loggeds na mesma máquina
        AuthenticationService.Instance.ClearSessionToken();
        
        //Login
        await AuthenticationService.
            Instance.SignInAnonymouslyAsync();

        Debug.Log("Utilizador Logged com: " + 
            AuthenticationService.Instance.PlayerId);

    }

    async public void CreateLobby()
    {
        //Para autenticar o utlizar no Lobby assim que cria o Lobby
        await Authenticate();

        //Enviar os atributos para o Lobby
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = GetPlayer()
        };
        //guardar a instancia do nosso lobby
        hostLobby = await Lobbies.Instance.CreateLobbyAsync("lobby",4, options);
        Debug.Log("Criou um Lobby: " + hostLobby.LobbyCode);
        joinnedLobby = hostLobby;
        //Mostrar os utilizadores que foram para o Lobby
        InvokeRepeating("SendLobbyHeartBeart", 10, 10); //Atualiza o hostLobby
        ShowPlayer();
        lobbyCodeText.text = joinnedLobby.LobbyCode;
        introLobby.SetActive(false);
        lobbyPanel.SetActive(true);


    }

    //Juntar a uma sessão lobby através do Code
    async public void JoinLobbyByCode() { 
        await Authenticate();

        //Enviar os atributos para o Lobby
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer()
        };

        joinnedLobby = await Lobbies.Instance.
            JoinLobbyByCodeAsync(lobbyCodeInput.text, options);

        Debug.Log("Entrou no Lobby com : " + joinnedLobby.LobbyCode);
        
        //Mostrar os utilizadores que foram para o Lobby
        ShowPlayer();
        lobbyCodeText.text = joinnedLobby.LobbyCode;
        introLobby.SetActive(false);
        lobbyPanel.SetActive(true);


    }

    //Mostrar a lista de utilizador no Lobby:
    void ShowPlayer()
    {
        for(int i = 0; i < joinnedLobby.Players.Count; i++)
        {
            //Debug.Log("Jogador: " + joinnedLobby.Players[i].Id);
            Debug.Log("Jogador: " + joinnedLobby.Players[i].
                Data["name"].Value);
            playername[i].text = joinnedLobby.Players[i].
                Data["name"].Value;
        }
    }

    //Para atualizar a instancia do nosso Lobby,
    //tendo em conta que ao longo do tempo esta vai abaixo
    async void SendLobbyHeartBeart()
    {
        if (hostLobby == null)
            return;

        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        Debug.Log("Atualiza o Lobby");

        //Obter o lobby host 
        UpdateLobby();
        //Players
        ShowPlayer();
    }

    async void UpdateLobby()
    {
        if (joinnedLobby == null)
            return;

        joinnedLobby = await LobbyService.Instance.
            GetLobbyAsync(joinnedLobby.Id);
    }

    //Para mostrar o nome do utlizador,
    //vamos criar uma estrutura para o player

    Player GetPlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "name", new PlayerDataObject(PlayerDataObject.
                VisibilityOptions.Public, playerNameInput.text) }
            }
        };
        return player;
    }
}
