using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundWonByText;
    [SerializeField] private TextMeshProUGUI countOfCardsOnTableText;
    [SerializeField] private List<PlayerPanel> playerPanels;
    [SerializeField] private List<Image> images;
    public void SetImage(Sprite sprite)
    {
        for (int i = 0; i < images.Count-1; i++)
        {
            images[i].sprite = images[i + 1].sprite;
        }
        images[images.Count - 1].sprite = sprite;
        SetImagesFalseOrTrue();
    }
    public void clearSprites()
    {
        foreach(Image image in images)
        {
            image.sprite = null;
        }
        SetImagesFalseOrTrue();
    }
    private void SetImagesFalseOrTrue()
    {
        foreach(Image image in images)
        {
            if (image.sprite == null) image.gameObject.SetActive(false);
            else image.gameObject.SetActive(true);
        }
    }
    public void ChangeCardsOnTable(int cardsOnTable)
    {
        countOfCardsOnTableText.text = "Cards On Table  " + cardsOnTable.ToString();
    }
    public void SetRoundWOnByText(string playerName)
    {
        roundWonByText.text = "Round Won By " + playerName.ToString();
    }
    public void ReSetRoundWOnByText()
    {
        roundWonByText.text = "";
    }
    public void EndGame()
    {
        Invoke("LoadMainMenu", 10);
    }
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) LoadMainMenu();
    }
    public PlayerPanel GetPlayerPanel(PlayerRef playerInstance)
    {
        return playerPanels[playerInstance];
    }
}
