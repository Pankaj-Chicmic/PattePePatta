using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerPanel : MonoBehaviour
{
    [SerializeField] private GameObject remainingCardTextGameObject;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI numberOfCardsText;
    [SerializeField] private TextMeshProUGUI  rankText;
    [SerializeField] private Button PlaceButton;
    [SerializeField] private Image cardBack;
    public void SetName(string playerName)
    {
        nameText.text = playerName.ToString();
    }
    public void ChangeNumberOfCards(int NumberOfCards)
    {
        numberOfCardsText.text = NumberOfCards.ToString();
    }
    public void changePlaceButtonStatus(bool status)
    {
        PlaceButton.interactable = status;
        if (!status) PlaceButton.gameObject.SetActive(false);
        else PlaceButton.gameObject.SetActive(true);
    }
    public void SetRank(int rank)
    {
        remainingCardTextGameObject.SetActive(false);
        PlaceButton.gameObject.SetActive(false);
        rankText.text = "Rank: "+rank.ToString();
        rankText.gameObject.SetActive(true);
    }
    public void SetCardBackActive(bool status)
    {
        cardBack.gameObject.SetActive(status);
    }
}
