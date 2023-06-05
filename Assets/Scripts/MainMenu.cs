using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject playersDataPrefab;
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] List<TMP_InputField> playerNamesInputField;
    private PlayersData playersDataObject;
    int numberOfPlayers=2;
    void Start()
    {
        List<TMP_Dropdown.OptionData> options=new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData("2 Players"));
        options.Add(new TMP_Dropdown.OptionData("3 Players"));
        options.Add(new TMP_Dropdown.OptionData("4 Players"));
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(ChangeNumberOfPlayer);
        PlayersData playersDataObject = FindObjectOfType<PlayersData>();
        foreach(TMP_InputField playerNameInputField in playerNamesInputField)
        {
            playerNameInputField.text = RanndomName();
        }
    }
    private void ChangeNumberOfPlayer(int index)
    {
        numberOfPlayers = int.Parse(dropdown.options[index].text[0]+"");
        SetPlayerNameInputFieldActive();
    }
    public void StartGame()
    {
        if (playersDataObject == null)
        {
            playersDataObject = Instantiate(playersDataPrefab).GetComponent<PlayersData>();
        }
        playersDataObject.numberOfPlayers = numberOfPlayers;
        playersDataObject.playerNames.Clear();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            playersDataObject.playerNames.Add(playerNamesInputField[i].text);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void SetPlayerNameInputFieldActive()
    {
        for(int i=0;i<playerNamesInputField.Count;i++)
        {
            if (i < numberOfPlayers)
            {
                playerNamesInputField[i].gameObject.SetActive(true);
            }
            else
            {
                playerNamesInputField[i].gameObject.SetActive(false);
            }
        }
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }
    private string RanndomName()
    {
        return "Player " + Random.Range(1000, 9999);
    }
}
