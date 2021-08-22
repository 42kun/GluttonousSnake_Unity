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
