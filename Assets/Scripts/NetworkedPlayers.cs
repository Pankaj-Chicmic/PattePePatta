using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class NetworkedPlayers : NetworkBehaviour
{
    [HideInInspector] public Player player;
    [Networked] [Capacity(52)] NetworkArray<Card> playerCards { get; }
    [Networked(OnChanged = nameof(OnCardChangedPlayer))] public int playerCardsNumber { get; private set; }
    [Networked(OnChanged = nameof(OnButtonStatusChanged))] public int status { get; set; }
    [Networked(OnChanged = nameof(OnRankChanged))] public int rank { get; set; }
    private void Shuffle()
    {
        for (int i = playerCardsNumber - 1; i > 0; i--)
        {
            int j = Random.Range(0, playerCardsNumber - 1);
            Card temp = playerCards[i];
            playerCards.Set(i, playerCards[j]);
            playerCards.Set(j, temp);
        }
    }
    private void SetButtonStatus(bool status)
    {
        player.SetButtonStatus(status);
    }
    public void AddCard(Card card)
    {
        playerCards.Set(playerCardsNumber, card);
        playerCardsNumber++;
    }
    public Card GetCard()
    {
        if (playerCardsNumber > 0)
        {
            playerCardsNumber--;
            return playerCards[playerCardsNumber];
        }
        Card card = new Card();
        card.number = -1;
        return card;
    }
    public bool IfLost(int remainingPlayers)
    {
        if (playerCardsNumber == 0)
        {
            player.SetNetworkedRank(remainingPlayers);
        }
        return playerCardsNumber == 0;
    }
    public void PlayerWonRound(MainGame mainGame)
    {
        int cardsOnTableTempNumber = mainGame.cardsOntableNumber;
        for (int i = 0; i < cardsOnTableTempNumber; i++)
        {
            AddCard(mainGame.cardsOnTable[i]);
        }
        Shuffle();
    }
    private static void OnButtonStatusChanged(Changed<NetworkedPlayers> playerInfo)
    {
        bool status = playerInfo.Behaviour.status == 1 ? true : false;
        playerInfo.Behaviour.SetButtonStatus(status);
    }
    private static void OnCardChangedPlayer(Changed<NetworkedPlayers> playerInfo)
    {
        playerInfo.Behaviour.player.SetNumberOfCards(playerInfo.Behaviour.playerCardsNumber);
    }
    private static void OnRankChanged(Changed<NetworkedPlayers> playerInfo)
    {
        playerInfo.Behaviour.player.SetRank(playerInfo.Behaviour.rank);
    }
}
