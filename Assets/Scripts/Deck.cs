using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
public class Deck : NetworkBehaviour
{
    static private Dictionary<TypeOfCard, List<Sprite>> sprites = new Dictionary<TypeOfCard, List<Sprite>>();
    private MainGame mainGame;
    [Networked] [Capacity(52)] NetworkArray<Card> cards { get; }
    public override void Spawned()
    {
        CreateSpritesDictionary();
        if (HasStateAuthority)
        {
            CreateCards();
            Shuffle();
        }
    }
    private void CreateSpritesDictionary()
    {
        Sprite[] cardSpritesTemp = Resources.LoadAll<Sprite>("Icons");
        List<Sprite> cardSprites = cardSpritesTemp.Cast<Sprite>().ToList();
        for (int i = 0; i < 4; i++)
        {
            sprites.Add((TypeOfCard)i, new List<Sprite>());
            for (int j = 0; j < 13; j++)
            {
                sprites[(TypeOfCard)i].Add(cardSprites[i * 13 + j]);
            }
        }
    }
    private void CreateCards()
    {
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Card curr = new Card(j + 1, (TypeOfCard)i);
                cards.Set(index, curr);
                index++;
            }
        }
    }
    
    private void Shuffle()
    {
        System.Random random = new System.Random();
        int n = cards.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            Card temp = cards[i];
            cards.Set(i, cards[j]);
            cards.Set(j, temp);
        }
    }
    public void DivideCards(Player[] players)
    {
        int totalNumberOfPlayers = players.Length;
        for (int i = 0; i < 52 && 52 - i >=totalNumberOfPlayers;)
        {
           for(int j = 0; j < totalNumberOfPlayers; j++)
           {
                players[j].AddCard(cards[i]);
                i++;
           }
        }
    }
    public static Sprite GetCardSprite(TypeOfCard typeOfCard, int number)
    {
        return sprites[typeOfCard][number - 1];
    }
}
