using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersData : MonoBehaviour
{
    public List<string> playerNames=new List<string>();
    public int numberOfPlayers;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
