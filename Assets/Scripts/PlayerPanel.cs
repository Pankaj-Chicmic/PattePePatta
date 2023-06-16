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
    public Player player;
    private void Start()
    {
        PlaceButton.onClick.AddListener(PlaceButtonClicked);
    }
    public void SetName(string playerName)
    {
        nameText.text = playerName.ToString();
    }
    public void ChangeNumberOfCards(int NumberOfCards)
    {
        numberOfCardsText.text = NumberOfCards.ToString();
    }
    public void SetButtonStatus(bool status,bool interactable)
    {
        PlaceButton.gameObject.SetActive(status);
        PlaceButton.interactable = interactable;
    }
    public void SetRank(int rank)
    {
        remainingCardTextGameObject.SetActive(false);
        rankText.text = "Rank: "+rank.ToString();
        rankText.gameObject.SetActive(true);
    }
    public void SetCardBackActive(bool status)
    {
        cardBack.gameObject.SetActive(status);
    }
    public void SetPlaceButtonIntractable()
    {
        PlaceButton.interactable = true;
    }
    private void PlaceButtonClicked()
    {
        player.PlacedClicked();
    }
}
