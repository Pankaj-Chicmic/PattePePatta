using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
public class MainGame : NetworkBehaviour,IPlayerLeft
{
    [SerializeField] private Deck deck;
    [Networked] [Capacity(52)] public NetworkArray<Card> cardsOnTable { get; }
    [Networked] [Capacity(4)] public NetworkArray<PlayerRef> playersInstances { get; }
    [Networked] [Capacity(4)] NetworkDictionary<PlayerRef, bool> dict { get; }
    [Networked] public int currentPlayerIndex { get; private set; }
    [Networked] public int totalPlayerNumber { get; private set; }
    [Networked] public int remainingPlayers { get; private set; }
    [Networked(OnChanged = nameof(OnCardsOnTableChanged))] public int cardsOntableNumber { get; private set; }
    [Networked(OnChanged = nameof(OnNewCardOnTableChanged))] public Card newCardOnTable { get; private set; }
    [Networked] public TickTimer timer { get; set; }
    private NetworkRunner networkRunner;
    private GameUI gameUI;
    private GameState gameState;
    public override void Spawned()
    {
        gameUI = FindObjectOfType<GameUI>();
        gameState = FindObjectOfType<GameState>();
    }
    public void StartGame()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
        int index = 0;
        foreach (PlayerRef playerInstance in Runner.ActivePlayers)
        {
            if (index >= 4) return;
            totalPlayerNumber++;
            remainingPlayers++;
            playersInstances.Set(index, playerInstance);
            dict.Add(playerInstance, true);
            index++;
        }
        currentPlayerIndex = -1;
        deck.DivideCards(FindObjectsOfType<Player>());
        SetNextPlayer();
    }
    private void RemovePlayer(PlayerRef playerInstance)
    {
        dict.Remove(playerInstance);
        remainingPlayers--;
    }
    private void SetNextPlayer()
    {
        if (SinglePlayerLeft())
        {
            MatchWon();
        }
        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayerNumber;
        while (!dict.ContainsKey(playersInstances[currentPlayerIndex]))
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayerNumber;
        }
        NetworkObject tempObject = Runner.GetPlayerObject(playersInstances[currentPlayerIndex]);
        tempObject.GetBehaviour<Player>().ChangeNetworkedStatusForButton(true);
    }
    private void SetNextPlayerAfterWinning()
    {
        if (SinglePlayerLeft())
        {
            MatchWon();
        }
        currentPlayerIndex--;
        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayerNumber;
        NetworkObject tempObject = Runner.GetPlayerObject(playersInstances[currentPlayerIndex]);
        tempObject.GetBehaviour<Player>().ChangeNetworkedStatusForButton(true);
    }
    private void ResetTable()
    {
        cardsOntableNumber = 0;
        Card card=new Card();
        card.number = -1;
        newCardOnTable = card;
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_PlaceCard(PlayerRef playerinstance)
    {
        Player currentPlayer = Runner.GetPlayerObject(playerinstance).GetBehaviour<Player>();
        currentPlayer.ChangeNetworkedStatusForButton(false);
        Card card = currentPlayer.GetCard();
        if (card.number != -1)
        {
            newCardOnTable = card;
            cardsOnTable.Set(cardsOntableNumber, card);
            cardsOntableNumber++;
            if (cardsOntableNumber > 1)
            {
                if (card.number == cardsOnTable[cardsOntableNumber - 2].number)
                {
                    timer=TickTimer.CreateFromSeconds(Runner, 5);
                }
                else
                {
                    if (currentPlayer.IfLost(remainingPlayers))
                    {
                        RemovePlayer(playerinstance);
                        if (SinglePlayerLeft())
                        {
                            MatchWon();
                        }
                        else
                        {
                            SetNextPlayer();
                        }
                    }
                    else
                    {
                        SetNextPlayer();
                    }
                }
            }
            else
            {
                if (currentPlayer.IfLost(remainingPlayers))
                {
                    RemovePlayer(playerinstance);
                    if (SinglePlayerLeft())
                    {
                        MatchWon();
                    }
                    else
                    {
                        SetNextPlayer();
                    }
                }
                else
                {
                    SetNextPlayer();
                }
            }
        }
        else
        {
            SetNextPlayer();
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (timer.IsRunning)
        {
            gameUI.SetRoundWOnByText(Runner.GetPlayerObject(playersInstances[currentPlayerIndex]).GetBehaviour<Player>().playerName);
        }
        if (timer.Expired(Runner))
        {
            gameUI.ReSetRoundWOnByText();
            if (!HasStateAuthority) return;
            Runner.GetPlayerObject(playersInstances[currentPlayerIndex]).GetBehaviour<Player>().PlayerWonRound(this);
            ResetTable();
            SetNextPlayerAfterWinning();
            timer = default;
        }
    }
    private bool SinglePlayerLeft()
    {
        return remainingPlayers == 1;
    }
    private void MatchWon()
    {
        foreach(PlayerRef playerInstance in playersInstances)
        {
            if (dict.ContainsKey(playerInstance))
            {
                Player player = Runner.GetPlayerObject(playerInstance).GetBehaviour<Player>();
                player.Rpc_SetRank(remainingPlayers);
                return;
            }
        }
        Rpc_ShutDownGame();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ShutDownGame()
    {
        gameUI.EndGame();
        Runner.Shutdown();
    }
    public static void OnCardsOnTableChanged(Changed<MainGame> playerInfo)
    {
        playerInfo.Behaviour.gameUI.ChangeCardsOnTable(playerInfo.Behaviour.cardsOntableNumber);
    }
    public static void OnNewCardOnTableChanged(Changed<MainGame> playerInfo)
    {
        MainGame mainGame = playerInfo.Behaviour;
        if (mainGame.newCardOnTable.number == -1) mainGame.gameUI.clearSprites();
        else
        { 
            Sprite sprite = Deck.GetCardSprite(mainGame.newCardOnTable.typeOfCard, mainGame.newCardOnTable.number);
            mainGame.gameUI.SetImage(sprite);
        } 
    }
    public void PlayerLeft(PlayerRef player)
    {
        if (gameState.gameState == GameStateEnum.Running)
        {
            if (HasStateAuthority)
            {
                RemovePlayer(player);
                if (playersInstances[currentPlayerIndex] == player) SetNextPlayer();
                remainingPlayers--;
            }
        }
    }
}