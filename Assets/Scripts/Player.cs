using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<Card> cards;
    private string playerName;
    private PlayerPanel playerPanel;
    public void SetPlayer(string playerName,PlayerPanel playerPanel)
    {
        this.cards = new List<Card>();
        this.playerName = playerName;
        this.playerPanel = playerPanel;
        this.playerPanel.SetName(playerName);
        this.playerPanel.gameObject.SetActive(true);
    }
    public void AddCard(Card card)
    {
        cards.Add(card);
        playerPanel.ChangeNumberOfCards(cards.Count);
    }
    public Card PlaceCard()
    {
        if (cards.Count == 0) return null;
        playerPanel.ChangeNumberOfCards(cards.Count-1);
        Card toReturn = cards[Random.Range(0, cards.Count - 1)];
        cards.Remove(toReturn);
        return toReturn;
    }
    public void AddWonCards(List<Card> wonCards)
    {
        playerPanel.ChangeNumberOfCards(cards.Count + wonCards.Count);
        foreach (Card wonCard in wonCards)
        {
            cards.Add(wonCard);
        }
    }
    public void SetButton(bool status)
    {
        playerPanel.changePlaceButtonStatus(status);
    }
    public void SetRank(int rank)
    {
        playerPanel.SetRank(rank);
    }
    public void SetCardBackStatus(bool status)
    {
        playerPanel.SetCardBackActive(status);
    }
    public bool CardEnded()
    {
        return cards.Count == 0;
    }
    public string GetPlayerName()
    {
        return playerName;
    }
    public void ShuffleCard()
    {
        for(int i = 0; i < cards.Count / 2; i++)
        {
            int first = Random.Range(0, cards.Count);
            int second = Random.Range(0, cards.Count);
            Card firstCard = cards[first];
            Card secondCard = cards[second];
            cards[first] = secondCard;
            cards[second] = firstCard;
        }
    }
}
