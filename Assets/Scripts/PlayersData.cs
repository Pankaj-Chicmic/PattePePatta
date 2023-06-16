using UnityEngine;
public class PlayersData : MonoBehaviour
{
    [HideInInspector]public string playerName;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
