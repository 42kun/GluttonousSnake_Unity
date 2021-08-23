using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnakeTrigger : MonoBehaviour
{
    protected virtual void Awake()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "wall":
                GameManager.instance.gameState = GameManager.GameState.Fail;
                break;
            case "apple":
                GameManager.instance.snakeManager.snakeGrow();
                GameManager.instance.boardManager.DestoryFood();
                GameManager.instance.score += Apple.score;
                GameManager.instance.uiManagement.UpdateScore(GameManager.instance.score);
                SoundManager.instance.playClip(SoundManager.instance.eatClip);
                break;
            case "golden_apple":
                GameManager.instance.snakeManager.snakeGrow();
                GameManager.instance.boardManager.DestoryFood();
                GameManager.instance.score += GoldebApple.score;
                GameManager.instance.uiManagement.UpdateScore(GameManager.instance.score);
                GameManager.instance.gameState = GameManager.GameState.Win;
                break;
        }
    }
}
