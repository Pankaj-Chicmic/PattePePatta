using UnityEngine;
using TMPro;
using Fusion;
public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject playerDataPrefab;
    [SerializeField] GameObject networkRunnerPrefab;
    [SerializeField] TMP_InputField playerNamesInputField;
    [SerializeField] TMP_InputField roomNameInputField;
    private NetworkRunner networkRunner;
    private PlayersData playerDataObject;
    private void Start()
    {
        playerDataObject = FindObjectOfType<PlayersData>();
        if (playerDataObject == null) playerDataObject=Instantiate(playerDataPrefab).GetComponent<PlayersData>();
        playerNamesInputField.text = RanndomName();
    }
   
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }
    private string RanndomName()
    {
        return "Player " + Random.Range(1000, 9999);
    }
    public void StartGame()
    {
        playerDataObject.playerName = playerNamesInputField.text;
        StartSharedMode(GameMode.Shared, roomNameInputField.text, "SampleScene");
    }
    private async void StartSharedMode(GameMode mode, string roomName, string sceneName)
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab).GetComponent<NetworkRunner>();
        }
        networkRunner.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
        };
        await networkRunner.StartGame(startGameArgs);
        networkRunner.SetActiveScene(sceneName);
    }
}
