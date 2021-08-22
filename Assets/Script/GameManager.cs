using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public BoardManager boardManager;
    [HideInInspector]
    public bool canMapGenerate = true;

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
        boardManager = GetComponent<BoardManager>();
        InitGame();
    }

    void InitGame()
    {
        if(canMapGenerate)
            boardManager.BuildMap();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (reGenerateMap)
        {
            boardManager.BuildMap();
            reGenerateMap = false;
        }
    }
}
