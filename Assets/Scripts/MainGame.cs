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
    [Networked] [Capacity(52)] NetworkArray<Card> player0Cards { get; }
    [Networked] [Capacity(52)] NetworkArray<Card> player1Cards { get; }
    [Networked] [Capacity(52)] NetworkArray<Card> player2Cards { get; }
    [Networked] [Capacity(52)] NetworkArray<Card> player3Cards { get; }
    [Networked(OnChanged = nameof(OnCardChangedPlayer0))] public int player0CardsNumber { get; private set; }
    [Networked(OnChanged = nameof(OnCardChangedPlayer1))] public int player1CardsNumber { get; private set; }
    [Networked(OnChanged = nameof(OnCardChangedPlayer2))] public int player2CardsNumber { get; private set; }
    [Networked(OnChanged = nameof(OnCardChangedPlayer3))] public int player3CardsNumber { get; private set; }
    [Networked] [Capacity(4)] NetworkDictionary<PlayerRef, int> dict { get; }
    [Networked] public int nextPlayerIndex { get; private set; }
    [Networked] public int totalPlayerNumber { get; private set; }
    [Networked] public int remainingPlayers { get; private set; }
    [Networked(OnChanged = nameof(OnCardsOnTableChanged))] public int cardsOntableNumber { get; private set; }
    [Networked(OnChanged = nameof(OnNewCardOnTableChanged))] public Card newCardOnTable { get; private set; }
    [Networked] public TickTimer timer { get; set; }
    private NetworkRunner networkRunner;
    public void StartGame()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
        player0CardsNumber = 0;
        player1CardsNumber = 0;
        player2CardsNumber = 0;
        player3CardsNumber = 0;
        cardsOntableNumber = 0;
        int index = 0;
        foreach (PlayerRef playerInstance in Runner.ActivePlayers)
        {
            dict.Add(playerInstance, index);
            totalPlayerNumber++;
            remainingPlayers++;
            playersInstances.Set(index, playerInstance);
            index++;
        }
        nextPlayerIndex = -1;
        deck.DivideCards(totalPlayerNumber);
        SetNextPlayer();
    }
    private void SetNextPlayer()
    {
        Debug.Log("Called Next Player");
        nextPlayerIndex = (nextPlayerIndex + 1) % totalPlayerNumber;
        //
        while (!dict.ContainsKey(playersInstances[nextPlayerIndex]))
        {
            //Debug.Log("called" + playersInstances[nextPlayerIndex]);
            nextPlayerIndex = (nextPlayerIndex + 1) % totalPlayerNumber;
            //Debug.Log("OK");
        }
        //Debug.Log("OKAY");
        //
        if (remainingPlayers == 1)
        {
            PlayerWonMatch();
        }
        NetworkObject tempObject = Runner.GetPlayerObject(playersInstances[nextPlayerIndex]);
        tempObject.GetBehaviour<Player>().Rpc_SetButtonTrue();
    }
    private void SetNextPlayerAfterWinning()
    {
        nextPlayerIndex--;
        nextPlayerIndex = (nextPlayerIndex + 1) % totalPlayerNumber;
        NetworkObject tempObject = Runner.GetPlayerObject(playersInstances[nextPlayerIndex]);
        tempObject.GetBehaviour<Player>().Rpc_SetButtonTrue();
        if (remainingPlayers == 1)
        {
            PlayerWonMatch();
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_PlaceCard(PlayerRef playerinstance)
    {
        Card card = GetCard(playerinstance);
        Debug.Log(card.number+ "Table");
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
                    if(IfLost(playerinstance))
                    {
                        if (SinglePlayerLeft())
                        {
                            SetNextPlayer();
                            PlayerWonMatch();
                        }
                        else
                        {
                            SetNextPlayer();
                        }
                    }
                    else
                    {
                        Debug.Log("OUT");
                        SetNextPlayer();
                    }
                }
            }
            else
            {
                if (IfLost(playerinstance))
                {
                    if (SinglePlayerLeft())
                    {
                        SetNextPlayer();
                        PlayerWonMatch();
                    }
                    else
                    {
                        Debug.Log("OUT");
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
            Debug.Log("OUT1");
            SetNextPlayer();
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (timer.IsRunning)
        {
            FindObjectOfType<GameUI>().SetRoundWOnByText(Runner.GetPlayerObject(playersInstances[nextPlayerIndex]).GetBehaviour<Player>().playerName);
        }
        if (timer.Expired(Runner))
        {
            FindObjectOfType<GameUI>().ReSetRoundWOnByText();
            if (!HasStateAuthority) return;
            PlayerWonRound(playersInstances[nextPlayerIndex]);
            timer = default;
        }
    }
    private bool SinglePlayerLeft()
    {
        return remainingPlayers== 1;
    }
    private void PlayerWonMatch()
    {
        NetworkObject playerObject = Runner.GetPlayerObject(playersInstances[nextPlayerIndex]);
        playerObject.GetBehaviour<Player>().Rpc_SetRank(remainingPlayers);
        Rpc_ShutDownGame();   
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ShutDownGame()
    {
        FindObjectOfType<GameUI>().EndGame();
        Runner.Shutdown();
    }
    private void PlayerWonRound(PlayerRef playerInstance)
    {
        Card card=new Card();
        card.number = -1;
        newCardOnTable = card;
        NetworkObject playerObject = Runner.GetPlayerObject(playerInstance);
        Player player = playerObject.GetBehaviour<Player>();
        int cardsOnTableTempNumber = cardsOntableNumber;
        for (int i = 0; i < cardsOnTableTempNumber; i++)
        {
            AddCard(cardsOnTable[i], dict[playerInstance]);
            cardsOnTable.Set(i, default);
            cardsOntableNumber--;
        }
        Shuffle(dict[playerInstance]);
        SetNextPlayerAfterWinning();
    }
    private bool IfLost(PlayerRef playerInstance)
    {
        int index = dict[playerInstance];
        switch (index)
        {
            case 0:
                if (player0CardsNumber == 0)
                {
                    dict.Remove(playerInstance);
                    NetworkObject playerObject = Runner.GetPlayerObject(playerInstance);
                    playerObject.GetBehaviour<Player>().Rpc_SetRank(remainingPlayers);
                    remainingPlayers--;
                    return true;
                }
                else return false;
            case 1:
                if (player1CardsNumber == 0)
                {
                    dict.Remove(playerInstance);
                    NetworkObject playerObject = Runner.GetPlayerObject(playerInstance);
                    playerObject.GetBehaviour<Player>().Rpc_SetRank(remainingPlayers);
                    remainingPlayers--;
                    return true;
                }
                else return false; 
            case 2:
                if (player2CardsNumber == 0)
                {
                    dict.Remove(playerInstance);
                    NetworkObject playerObject = Runner.GetPlayerObject(playerInstance);
                    playerObject.GetBehaviour<Player>().Rpc_SetRank(remainingPlayers);
                    remainingPlayers--;
                    return true;
                }
                else return false;
            case 3:
                if (player3CardsNumber == 0)
                {
                    dict.Remove(playerInstance);
                    NetworkObject playerObject = Runner.GetPlayerObject(playerInstance);
                    playerObject.GetBehaviour<Player>().Rpc_SetRank(remainingPlayers);
                    remainingPlayers--;
                    return true;
                }
                else return false;
        }
        return false;
    }
    public static void OnCardsOnTableChanged(Changed<MainGame> playerInfo)
    {
        FindObjectOfType<GameUI>().ChangeCardsOnTable(playerInfo.Behaviour.cardsOntableNumber);
    }
    public void AddCard(Card card, int index)
    {
        switch (index)
        {
            case 0:
                player0Cards.Set(player0CardsNumber, card);
                player0CardsNumber++;
                break;
            case 1:
                player1Cards.Set(player1CardsNumber, card);
                player1CardsNumber++;
                break;
            case 2:
                player2Cards.Set(player2CardsNumber, card);
                player2CardsNumber++;
                break;
            case 3:
                player3Cards.Set(player3CardsNumber, card);
                player3CardsNumber++;
                break;
        }

    }
    public Card GetCard(PlayerRef player)
    {
        int index = dict[player];
        switch (index)
        {
            case 0:
                if (player0CardsNumber > 0)
                {
                    player0CardsNumber--;
                    return player0Cards[player0CardsNumber];
                }
                else
                {
                    Card card1 = new Card();
                    card1.number = -1;
                    return card1;
                }
            case 1:
                if (player1CardsNumber > 0)
                {
                    player1CardsNumber--;
                    return player1Cards[player1CardsNumber];
                }
                else
                {
                    Card card1 = new Card();
                    card1.number = -1;
                    return card1;
                }
            case 2:
                if (player2CardsNumber > 0)
                {
                    player2CardsNumber--;
                    return player2Cards[player2CardsNumber];
                }
                else
                {
                    Card card1 = new Card();
                    card1.number = -1;
                    return card1;
                }
            case 3:
                if (player3CardsNumber > 0)
                {
                    player3CardsNumber--;
                    return player3Cards[player3CardsNumber];
                }
                else
                {
                    Card card1 = new Card();
                    card1.number = -1;
                    return card1;
                }
        }
        Card card = new Card();
        card.number = -1;
        return card;
    }
    public static void OnCardChangedPlayer0(Changed<MainGame> playerInfo)
    {
        NetworkObject playerObject = playerInfo.Behaviour.Runner.GetPlayerObject(playerInfo.Behaviour.playersInstances[0]);
        Player player = playerObject.GetBehaviour<Player>();
        player.SetNumberOfCards(playerInfo.Behaviour.player0CardsNumber);
    }
    public static void OnCardChangedPlayer1(Changed<MainGame> playerInfo)
    {
        NetworkObject playerObject = playerInfo.Behaviour.Runner.GetPlayerObject(playerInfo.Behaviour.playersInstances[1]);
        Player player = playerObject.GetBehaviour<Player>();
        player.SetNumberOfCards(playerInfo.Behaviour.player1CardsNumber);
    }

    public static void OnCardChangedPlayer2(Changed<MainGame> playerInfo)
    {
        NetworkObject playerObject = playerInfo.Behaviour.Runner.GetPlayerObject(playerInfo.Behaviour.playersInstances[2]);
        Player player = playerObject.GetBehaviour<Player>();
        player.SetNumberOfCards(playerInfo.Behaviour.player2CardsNumber);
    }
    public static void OnCardChangedPlayer3(Changed<MainGame> playerInfo)
    {
        NetworkObject playerObject = playerInfo.Behaviour.Runner.GetPlayerObject(playerInfo.Behaviour.playersInstances[3]);
        Player player = playerObject.GetBehaviour<Player>();
        player.SetNumberOfCards(playerInfo.Behaviour.player3CardsNumber);
    }
    public static void OnNewCardOnTableChanged(Changed<MainGame> playerInfo)
    {
        Debug.Log(playerInfo.Behaviour.newCardOnTable.number);
        if (playerInfo.Behaviour.newCardOnTable.number == -1) FindObjectOfType<GameUI>().clearSprites();
        else
        { 
            Sprite sprite = Deck.GetCardSprite(playerInfo.Behaviour.newCardOnTable.typeOfCard, playerInfo.Behaviour.newCardOnTable.number);
            FindObjectOfType<GameUI>().SetImage(sprite);
        } 
    }
    private void Shuffle(int index)
    {
        switch (index)
        {
            case 0:
                for (int i = player0CardsNumber - 1; i > 0; i--)
                {
                    int j = Random.Range(0, player0CardsNumber - 1);
                    Card temp = player0Cards[i];
                    player0Cards.Set(i, player0Cards[j]);
                    player0Cards.Set(j, temp);
                }
                break;
            case 1:
                for (int i = player1CardsNumber - 1; i > 0; i--)
                {
                    int j = Random.Range(0, player1CardsNumber - 1);
                    Card temp = player1Cards[i];
                    player1Cards.Set(i, player1Cards[j]);
                    player1Cards.Set(j, temp);
                }
                break;
            case 2:
                for (int i = player2CardsNumber - 1; i > 0; i--)
                {
                    int j = Random.Range(0, player2CardsNumber - 1);
                    Card temp = player2Cards[i];
                    player2Cards.Set(i, player2Cards[j]);
                    player2Cards.Set(j, temp);
                }
                break;
            case 3:
                for (int i = player3CardsNumber - 1; i > 0; i--)
                {
                    int j = Random.Range(0, player3CardsNumber - 1);
                    Card temp = player3Cards[i];
                    player3Cards.Set(i, player3Cards[j]);
                    player3Cards.Set(j, temp);
                }
                break;
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (FindObjectOfType<GameState>().gameState == GameStateEnum.Running)
        {
            if (HasStateAuthority)
            {
                dict.Remove(player);
                if (playersInstances[nextPlayerIndex] == player) SetNextPlayer();
                remainingPlayers--;
            }
        }
    }
}