using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TypeOfCard
{
    Club,
    Diamonds,
    Hearts,
    Spade
}
public class Card 
{
    public int number;
    public Sprite sprite;
    public TypeOfCard typeOfCard;
    public Card(int number,Sprite sprite, TypeOfCard typeOfCard)
    {
        this.number = number;
        this.sprite = sprite;
        this.typeOfCard = typeOfCard;
    }
    public bool CheckEquals(Card card)
    {
        return this.number == card.number;
    }
}
