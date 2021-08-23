using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public BoardManager boardManager;
    public SnakeManager snakeManager;

    public GameObject snakePrefab;

    public UIManagement uiManagement;

    public AudioClip eatFoodSound;
    public AudioClip backgroundSound;
    public AudioClip WinSound;
    public AudioClip FailSound;

    [HideInInspector]
    public bool canMapGenerate = true;

    public enum GameState
    {
        None,
        StartGame,
        InGame,
        Pause,
        Win,
        Fail,
        End
    }

    public GameState gameState = GameState.None;

    [HideInInspector]
    //��Ϸ����
    public int score = 0;


#if TEST
    public bool reGenerateMap  = false;
#endif

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        //���´���ֻ��ִ��һ��
        //TODO:����֡�ʻ���Ҫѧϰ�о���
        Application.targetFrameRate = 60;
        //���ٴη�������ű����ڵĳ���ʱ������ű����ڵ���������³�ʼ��������Ҳ���ٴ�ִ��Awake
        DontDestroyOnLoad(this.gameObject);

        //boardManager = new BoardManager();
        //����Instantiate a class that derives from MonoBehaviour
        //���ڼ̳���MonoBehaviour�Ľű������Ƕ�Ӧ�ò�ȡunity�ڲ���ʵ��������
        //���ǿ�����ôд��
        //boardManager = gameObject.AddComponent<BoardManager>();
        //������ΪBoardManager��Ҫ����һЩ�ֶ�����Ķ���
        //���Ի��ǲ�ȡ��׼д��
    }

    void InitGame()
    {
        boardManager = GetComponent<BoardManager>();
        uiManagement = GameObject.Find("UI").GetComponent<UIManagement>();
        // ��һ�μ���
        if (gameState == GameState.None)
        {
            uiManagement.StartGame();
            gameState = GameState.StartGame;
        }
        // ���������
        else
        {
            gameState = GameState.InGame;
            uiManagement.EnterGame();
        }
        score = 0;
        Time.timeScale = 1;
        SoundManager.instance.StartBackground();
    }

    void GenerateGameObject()
    {
        // ���ɵ�ͼ
        if (canMapGenerate)
            boardManager.BuildMap();
        // ������
        GameObject snake = Instantiate(snakePrefab, new Vector3(5, 3, 0), Quaternion.identity);
        snakeManager = snake.GetComponent<SnakeManager>();
        // ����ʳ������
        boardManager.StartFoodSpawn();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitGame();
    }

    private void OnLevelWasLoaded(int level)
    {
        InitGame();
        GenerateGameObject();
    }

    // Update is called once per frame
    void Update()
    {
#if TEST
        if (reGenerateMap)
        {
            boardManager.BuildMap();
            reGenerateMap = false;
        }
#endif
        //�����������Ϸ
        if (gameState==GameState.StartGame && Input.anyKey)
        {
            GenerateGameObject();
            uiManagement.EnterGame();
            gameState = GameState.InGame;
        }
        // ����
        if (gameState!=GameState.StartGame && Input.GetKeyDown(KeyCode.R))
        {
            //Debug.Log("Restart");
            SceneManager.LoadScene(0);
        }
        // ��ͣ
        if ((gameState==GameState.InGame||gameState==GameState.Pause) && Input.GetKeyDown(KeyCode.P))
        {
            //Debug.Log("Pause");
            if (gameState==GameState.Pause)
            {
                Time.timeScale = 1;
                gameState = GameState.InGame;
                uiManagement.ContinueGame();
            }
            else
            {
                Time.timeScale = 0;
                gameState = GameState.Pause;
                uiManagement.PauseGame();
            }
        }

        // ʧ��
        if (gameState == GameState.Fail)
        {
            Time.timeScale = 0;
            uiManagement.Fail();
            SoundManager.instance.StopBackground();
            SoundManager.instance.playClip(SoundManager.instance.failClip);
            gameState = GameState.End;
        }
        if(gameState == GameState.Win)
        {
            Time.timeScale = 0;
            uiManagement.Win();
            SoundManager.instance.StopBackground();
            SoundManager.instance.playClip(SoundManager.instance.winClip);
            gameState = GameState.End;
        }
    }
}
