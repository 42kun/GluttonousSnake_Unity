using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameManager : MonoBehaviour
{
    public GameObject gameManager;
    public bool canMapGenerate = true;
    private void Awake()
    {
        if (GameManager.instance == null)
        {
            Instantiate(gameManager);
            gameManager.GetComponent<GameManager>().canMapGenerate = canMapGenerate;
        }
    }
}
