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
    //游戏分数
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
        //以下代码只会执行一次
        //TODO:关于帧率还是要学习研究！
        Application.targetFrameRate = 60;
        //当再次返回这个脚本所在的场景时，这个脚本所在的物体会重新初始化，所以也会再次执行Awake
        DontDestroyOnLoad(this.gameObject);

        //boardManager = new BoardManager();
        //报错：Instantiate a class that derives from MonoBehaviour
        //对于继承了MonoBehaviour的脚本，我们都应该采取unity内部的实例化方法
        //我们可以这么写：
        //boardManager = gameObject.AddComponent<BoardManager>();
        //但是因为BoardManager需要挂载一些手动导入的对象
        //所以还是采取标准写法
    }

    void InitGame()
    {
        boardManager = GetComponent<BoardManager>();
        uiManagement = GameObject.Find("UI").GetComponent<UIManagement>();
        // 第一次激活
        if (gameState == GameState.None)
        {
            uiManagement.StartGame();
            gameState = GameState.StartGame;
        }
        // 重启进入的
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
        // 生成地图
        if (canMapGenerate)
            boardManager.BuildMap();
        // 生成蛇
        GameObject snake = Instantiate(snakePrefab, new Vector3(5, 3, 0), Quaternion.identity);
        snakeManager = snake.GetComponent<SnakeManager>();
        // 开启食物生成
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
        //任意键启动游戏
        if (gameState==GameState.StartGame && Input.anyKey)
        {
            GenerateGameObject();
            uiManagement.EnterGame();
            gameState = GameState.InGame;
        }
        // 重启
        if (gameState!=GameState.StartGame && Input.GetKeyDown(KeyCode.R))
        {
            //Debug.Log("Restart");
            SceneManager.LoadScene(0);
        }
        // 暂停
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

        // 失败
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
