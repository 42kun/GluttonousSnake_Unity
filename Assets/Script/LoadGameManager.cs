using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameManager : MonoBehaviour
{
    public GameObject gameManager;
    public bool canGameStart = false;
    private void Awake()
    {
        if (GameManager.instance == null && canGameStart)
        {
            Instantiate(gameManager);
        }
    }
}
