using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class Player : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnPlayerNameChanged))] public string playerName { get; set; }
    //[Networked] public int lastCardIndex { get; set; }
    private PlayerPanel playerPanel;
    public override void Spawned()
    {
        //Runner.SetPlayerObject(Object.StateAuthority,Object);
        //lastCardIndex = -1;
        playerPanel=FindAnyObjectByType<GameUI>().GetPlayerPanel(Object.StateAuthority);
        playerPanel.gameObject.SetActive(true);
        playerPanel.player = this;
        if (HasStateAuthority)
        {
            //Debug.Log("Called");
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
    [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
    public void Rpc_SetButtonTrue(RpcInfo info=default)
    {
        playerPanel.changePlaceButtonStatus(true);
    }
    public void PlacedClicked()
    {
        playerPanel.changePlaceButtonStatus(false);
        FindObjectOfType<MainGame>().Rpc_PlaceCard(Object.StateAuthority);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_SetRank(int rank,RpcInfo info = default)
    {
        playerPanel.SetRank(rank);
    }
}
