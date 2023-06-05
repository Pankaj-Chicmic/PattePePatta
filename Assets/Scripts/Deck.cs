//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Deck : MonoBehaviour
//{
//    private List<Card> cards=new List<Card>();
//    public void CreateAndShuffle(Queue<Player> players)
//    {
//        Object[] cardSpritesTemp= Resources.LoadAll("Icons", typeof(Sprite));
//        List<Sprite> cardSprites = new List<Sprite>();
//        foreach(var card in cardSpritesTemp)
//        {
//            cardSprites.Add((Sprite)card);
//        }
//        for (int i = 1; i <= 4; i++)
//        {
//            for(int j = 1; j <= 13; j++)
//            {
//                Card curr = new Card(j,cardSprites[(i-1)*13+j-1], (TypeOfCard)(i-1));
//                cards.Add(curr);
//            }
//        }
//        Shuffle();
//        for(int i = 0; i < 52 && 52-i>=players.Count;)
//        {
//            foreach(Player player in players)
//            {
//                player.AddCard(cards[i]);
//                i++;
//            }
//        }
//    }
//    private void Shuffle()
//    {
//        for(int i = 0; i < 26; i++)
//        {
//            int first = Random.Range(0, 52);
//            int second = Random.Range(0, 52);
//            Card firstCard = cards[first];
//            Card secondCard = cards[second];
//            cards[first] = secondCard;
//            cards[second] = firstCard;
//        }
//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Deck : MonoBehaviour
{
    private Card[] cards = new Card[52];

    public void CreateAndShuffle(Queue<Player> players)
    {
        Sprite[] cardSpritesTemp = Resources.LoadAll<Sprite>("Icons");
        List<Sprite> cardSprites = cardSpritesTemp.Cast<Sprite>().ToList();

        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Card curr = new Card(j + 1, cardSprites[(i * 13) + j], (TypeOfCard)i);
                cards[index] = curr;
                index++;
            }
        }

        Shuffle();

        int playerCount = players.Count;
        for (int i = 0; i < 52 && 52 - i >= playerCount;)
        {
            foreach (Player player in players)
            {
                player.AddCard(cards[i]);
                i++;
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
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }
}
