using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private Deck deck;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<PlayerPanel> playerPanels;
    [SerializeField] private int numberOfPlayer=4;
    private PlayersData playersData;
    private Player currChancePlayer=null;
    private List<Card> cardsOnTable;
    private Queue<Player> playerList=new Queue<Player>();
    private void Start()
    {
        playersData = FindObjectOfType<PlayersData>();
        numberOfPlayer = playersData.numberOfPlayers;
        cardsOnTable = new List<Card>();
        for(int i = 0; i < Mathf.Min(numberOfPlayer,4); i++)
        {
            Player currPlayer= Instantiate(playerPrefab).GetComponent<Player>();  
            currPlayer.SetPlayer(playersData.playerNames[i], playerPanels[i]);
            playerList.Enqueue(currPlayer);
        }
        deck.CreateAndShuffle(playerList);
        currChancePlayer = playerList.Peek();
        currChancePlayer.SetButton(true); 
    }
    public void Game()
    {
        currChancePlayer.SetButton(false);
        Card newCardOnTable = currChancePlayer.PlaceCard();
        if (newCardOnTable == null)
        {
            currChancePlayer.SetRank(playerList.Count);
            currChancePlayer.SetCardBackStatus(false);
            playerList.Dequeue();
        }
        else
        {
            gameUI.SetImage(newCardOnTable.sprite);
            Debug.Log(newCardOnTable.number+" "+newCardOnTable.typeOfCard);
            if (cardsOnTable.Count > 0 && cardsOnTable[cardsOnTable.Count - 1].CheckEquals(newCardOnTable))
            {
                StartCoroutine(Won(newCardOnTable));
                return;
            }
            else
            {
                cardsOnTable.Add(newCardOnTable);
                gameUI.ChangeCardsOnTable(cardsOnTable.Count);
                if (currChancePlayer.CardEnded())
                {
                    currChancePlayer.SetRank(playerList.Count);
                    currChancePlayer.SetCardBackStatus(false);
                    playerList.Dequeue();
                    if (playerList.Count == 1)
                    {
                        currChancePlayer=playerList.Dequeue();
                        currChancePlayer.SetRank(1);
                        currChancePlayer.SetCardBackStatus(true);
                        currChancePlayer.SetButton(false);
                        gameUI.EndGame();
                        return;
                    }
                }
                else
                {
                    playerList.Enqueue(playerList.Dequeue());
                }
            }
        }
        currChancePlayer = playerList.Peek();
        currChancePlayer.SetButton(true);
    }
    private IEnumerator Won(Card newCardOnTable)
    {
        gameUI.SetRoundWOnByText(currChancePlayer.GetPlayerName());
        cardsOnTable.Add(newCardOnTable);
        gameUI.ChangeCardsOnTable(cardsOnTable.Count);
        yield return new WaitForSecondsRealtime(3); 
        currChancePlayer.AddWonCards(cardsOnTable);
        cardsOnTable.Clear();
        gameUI.ChangeCardsOnTable(cardsOnTable.Count);
        gameUI.clearSprites(); 
        currChancePlayer = playerList.Peek();
        currChancePlayer.SetButton(true);
        gameUI.ReSetRoundWOnByText();
        ReshuffleCardsOfAllPlayer();
    }
    private void ReshuffleCardsOfAllPlayer()
    {
        foreach(Player player in playerList)
        {
            player.ShuffleCard();
        }
    }
}
