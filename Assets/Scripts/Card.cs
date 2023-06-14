using Fusion;
public enum TypeOfCard
{
    Club,
    Diamond,
    Heart,
    Spade
}
public struct Card : INetworkStruct
{
    public int number;
    public TypeOfCard typeOfCard;
    public Card(int number,TypeOfCard typeOfCard)
    {
        this.number = number;
        this.typeOfCard = typeOfCard;
    }
    public bool CheckEquals(Card card)
    {
        return this.number == card.number;
    }
}
