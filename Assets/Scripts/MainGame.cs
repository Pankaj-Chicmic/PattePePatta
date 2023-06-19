using UnityEngine;
using Fusion;
public class MainGame : NetworkBehaviour,IPlayerLeft
{
    [SerializeField] private Deck deck;
    private NetworkRunner networkRunner;
    private GameUI gameUI;
    private GameState gameState;
    [Networked] [Capacity(52)] public NetworkArray<Card> cardsOnTable { get; }
    [Networked] [Capacity(4)] public NetworkArray<PlayerRef> playersInstances { get; }
    [Networked] [Capacity(4)] NetworkDictionary<PlayerRef, bool> dict { get; }
    [Networked(OnChanged = nameof(OnNewCardOnTableChanged))] public Card newCardOnTable { get; private set; }
    [Networked(OnChanged = nameof(OnCardsOnTableChanged))] public int cardsOntableNumber { get; private set; }
    [Networked] public int currentPlayerIndex { get; private set; }
    [Networked] public int totalPlayerNumber { get; private set; }
    [Networked] public int remainingPlayers { get; private set; }
    [Networked] public TickTimer timer { get; set; }
    private void SetNextPlayer()
    {
        if (SinglePlayerLeft())
        {
            MatchWon();
            return;
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
            return;
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
    private void RemovePlayer(PlayerRef playerInstance)
    {
        Debug.Log("Called to Removed Player");
        if (dict.ContainsKey(playerInstance))
        {
            Debug.Log("Removing Player");
            dict.Remove(playerInstance);
            remainingPlayers--;
        }
    }
    public override void Spawned()
    {
        gameUI = FindObjectOfType<GameUI>();
        gameState = FindObjectOfType<GameState>();
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
                    timer=TickTimer.CreateFromSeconds(Runner,5);
                }
                else
                {
                    if (currentPlayer.IfLost(remainingPlayers))
                    {
                        Debug.Log("Calling To Remove Player");
                        RemovePlayer(playerinstance);
                        if (SinglePlayerLeft())
                        {
                            Debug.Log("Single Player Left Calling MatchWon()");
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
    private bool SinglePlayerLeft()
    {
        return remainingPlayers == 1;
    }
    private void MatchWon()
    {
        Debug.Log("Called MatchWon");
        foreach(PlayerRef playerInstance in playersInstances)
        {
            if (dict.ContainsKey(playerInstance))
            {
                Debug.Log("Found Won Player Setting It");
                Player player = Runner.GetPlayerObject(playerInstance).GetBehaviour<Player>();
                player.SetNetworkedRank(remainingPlayers);
            }
        }
        Debug.Log("Calling Shutdown Rpc");
        Rpc_ShutDownGame();
    }
    public void PlayerLeft(PlayerRef player)
    {
        gameUI.DisablePanel(player);
        if (gameState.gameState == GameStateEnum.Running)
        {
            if (HasStateAuthority)
            {
                RemovePlayer(player);
                if (playersInstances[currentPlayerIndex] == player) SetNextPlayer();
            }
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_ShutDownGame()
    {
        Debug.Log("Shutdown Rpc Called");
        gameUI.EndGame();
        Invoke(nameof(ShutDownRunner),5);
    }
    private void ShutDownRunner()
    {
        Runner.Shutdown();
    }
    private static void OnCardsOnTableChanged(Changed<MainGame> playerInfo)
    {
        playerInfo.Behaviour.gameUI.ChangeCardsOnTable(playerInfo.Behaviour.cardsOntableNumber);
    }
    private static void OnNewCardOnTableChanged(Changed<MainGame> playerInfo)
    {
        MainGame mainGame = playerInfo.Behaviour;
        if (mainGame.newCardOnTable.number == -1) mainGame.gameUI.clearSprites();
        else
        { 
            Sprite sprite = Deck.GetCardSprite(mainGame.newCardOnTable.typeOfCard, mainGame.newCardOnTable.number);
            mainGame.gameUI.SetImage(sprite);
        } 
    }
}