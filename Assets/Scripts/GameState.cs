using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public enum GameStateEnum
{
    Starting,
    Running,
    Ending
};
public class GameState : NetworkBehaviour
{
    [SerializeField] NetworkObject playerPrefab;
    [Networked] public GameStateEnum gameState { get; set; }
    [Networked] TickTimer timer { get; set; }
    [Networked] int numberOfPlayer{get;set;}
    public override void Spawned()
    {
        if (gameState != GameStateEnum.Starting || numberOfPlayer>=4)
        {
            Runner.Shutdown();
            FindObjectOfType<GameUI>().EndGameImmidiate();
            return;
        }
        Rpc_PlayerJoined();
        if (HasStateAuthority)
        {
            gameState = GameStateEnum.Starting;
            timer=TickTimer.CreateFromSeconds(Runner, 10);
        }
        NetworkObject playerObject = Runner.Spawn(playerPrefab);
        Runner.SetPlayerObject(Runner.LocalPlayer,playerObject);
    }
    public override void FixedUpdateNetwork()
    {
        if (timer.Expired(Runner) && HasStateAuthority)
        {
            if (numberOfPlayer < 2)
            {
                FindObjectOfType<GameUI>().EndGame();
                Runner.Shutdown();
                return;
            }
            gameState = GameStateEnum.Running;
            timer = default;
            FindAnyObjectByType<MainGame>().StartGame();
        }
    }
    [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
    public void Rpc_PlayerJoined()
    {
        numberOfPlayer++;
    }
}
