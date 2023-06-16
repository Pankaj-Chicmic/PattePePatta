using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class Player : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnPlayerNameChanged))] public string playerName { get; set; }
    private PlayerPanel playerPanel;
    private NetworkedPlayers networkPlayer;
    public override void Spawned()
    {
        playerPanel=FindAnyObjectByType<GameUI>().GetPlayerPanel(Object.StateAuthority);
        playerPanel.gameObject.SetActive(true);
        playerPanel.player = this;
        networkPlayer = FindObjectOfType<GameUI>().GetNetworkPlayer(Object.StateAuthority);
        networkPlayer.player = this;
        if (HasStateAuthority)
        {
            playerName = FindObjectOfType<PlayersData>().playerName;
        }
    }
    public void SetNumberOfCards(int numberOfCards)
    {
        playerPanel.ChangeNumberOfCards(numberOfCards);
    }
    public static void OnPlayerNameChanged(Changed<Player> playerInfo)
    {
        playerInfo.Behaviour.playerPanel.SetName(playerInfo.Behaviour.playerName);
    }
    public void PlacedClicked()
    {
        FindObjectOfType<MainGame>().Rpc_PlaceCard(Object.StateAuthority);
    }
    public void SetNetworkedRank(int rank)
    {
        networkPlayer.rank = rank;
    }
    public void SetRank(int rank)
    {
        playerPanel.SetRank(rank);
    }
    public void AddCard(Card card)
    {
        networkPlayer.AddCard(card);
    }
    public Card GetCard()
    {
        return networkPlayer.GetCard();
    }
    public bool IfLost(int remainingPlayers)
    {
        return networkPlayer.IfLost(remainingPlayers);
    }
    public void PlayerWonRound(MainGame mainGame)
    {
        networkPlayer.PlayerWonRound(mainGame);
    }
    public void SetButtonStatus(bool status)
    {
        playerPanel.SetButtonStatus(status, HasStateAuthority);
    }
    public void ChangeNetworkedStatusForButton(bool status)
    {
        networkPlayer.status = status ? 1 : 0;
    }
}
