using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//记录区块信息，为传播式地图生成提供参考
public struct ChunkInfo
{
    public bool isWall;
    public bool floorHasCheck;
    public bool isBuilding;
    public ChunkInfo(bool isWall = false, bool isBuilding = false, bool floorHasCheck = false)
    {
        this.isWall = isWall;
        this.isBuilding = isBuilding;
        this.floorHasCheck = floorHasCheck;
    }
}

public class BoardManager : MonoBehaviour
{ 
    const int side = 101;
    const float uint_scale = 0.1f;
    const int building_size = 17;

    private GameObject board = null;
    private Transform boardHolder;
    //记录地图方块坐标，仅用于地图生成，不用于游戏逻辑
    private Dictionary<Vector2Int, ChunkInfo> chunkInfo;

    private Transform wallTransform;
    //外墙中部生成
    public GameObject[] wallMidTile;
    public float[] wallMidTileProb;

    //外墙外部生成
    public GameObject[] wallSideTile;
    public float[] wallSideTileProb;

    //外墙直边最小长度
    public float MinLenOfEdgeRate;
    //外墙直角边最小长度
    public float MinLenOfLedRate;
    private List<Vector2Int> vertexs;

    private Transform buildingTransform;
    //建筑生成数
    public GameObject building1Object;
    public GameObject building2Object;
    public int building1GenerateNum = 2;
    public int building2GenerateNum = 2;
    //最小建筑曼哈顿距离
    public int buildingMinDis = 50;
    //建筑尝试生成次数
    public int buildingGenerateRetryTime = 10;

    //石砖替换候选Sprite
    public Sprite brickSprite;
    public Sprite[] brickSprites;
    public float[] brickSpritesProb;
    //娜迦石替换候选Sprite
    public Sprite nagaStoneSprite;
    public Sprite[] nagaStoneSprites;
    public float[] nagaStoneSpritesProb;
    //沙土替换候选Sprite
    public Sprite redSandSprite;
    public Sprite[] redSandSprites;
    public float[] redSandSpritesProb;

    private Transform floorTransform;
    public GameObject floorObject;
    public GameObject floorSandObject;


    public GameObject apple;
    public GameObject goldenApple;

    public float appleExistTime = 10;
    public float goldenAppleGap = 20;

    bool foodReadySpawn = false;
    List<Vector2Int> avalFoodPosition = new List<Vector2Int>();
    GameObject nowFoodObject = null;
    public Vector2Int nowFoodPosition;
    int hasSpawnApple = 0;
    float foodExistTime = 0;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!foodReadySpawn)
            return;
        if (nowFoodObject == null)
        {
            nowFoodPosition =  GenerateOneFood();
        }
        else
        {
            foodExistTime += Time.deltaTime;
            if (foodExistTime > appleExistTime)
                DestoryFood();
        }
    }



    /// <summary>
    /// 开启食物生成
    /// </summary>
    public void StartFoodSpawn()
    {
        foreach (Vector2Int p in chunkInfo.Keys)
        {
            if (!chunkInfo[p].isWall)
                avalFoodPosition.Add(p);
        }
        foodReadySpawn = true;
    }

    /// <summary>
    /// 生成一个新食物
    /// </summary>
    /// <returns></returns>
    Vector2Int GenerateOneFood()
    {
        Vector2Int pos = avalFoodPosition[Random.Range(0, avalFoodPosition.Count)];
        //Debug.Log(string.Format("food generate,pos={0}", pos));
        hasSpawnApple++;
        foodExistTime = 0;
        if (hasSpawnApple % goldenAppleGap == 0)
            nowFoodObject = Instantiate(goldenApple, new Vector3(pos.x * uint_scale, pos.y * uint_scale, 0), Quaternion.identity);
        else
            nowFoodObject = Instantiate(apple, new Vector3(pos.x * uint_scale, pos.y * uint_scale, 0), Quaternion.identity);
        return pos;
    }

    /// <summary>
    /// 销毁食物，在任何情况下调用都不会产生逻辑错误
    /// </summary>
    public void DestoryFood()
    {
        Destroy(nowFoodObject);
        nowFoodObject = null;
        foodExistTime = 0;
    }


    public void BuildMap()
    {
        if (board != null)
        {
            ClearMap();
            Destroy(board);
            board = null;
        }
        board = new GameObject("Board");
        boardHolder = board.transform;
        chunkInfo = new Dictionary<Vector2Int, ChunkInfo>();

        wallTransform = new GameObject("Walls").transform;
        wallTransform.SetParent(boardHolder);
        buildingTransform = new GameObject("Buildings").transform;
        buildingTransform.SetParent(boardHolder);
        floorTransform = new GameObject("Floors").transform;
        floorTransform.SetParent(boardHolder);

        List<Vector2Int> breakPoints = GenerateBreakPoints();
        //生成外墙顶点集合
        vertexs = GenerateVertexList(breakPoints);
        GenerateWalls();
        GenerateBuildings();
        GenerateFloors();
        RandomChangeBlocksSprite();
        hasSpawnApple = 0;
        foodExistTime = 0;
    }

    public void ClearMap()
    {

    }

    #region test
#if TEST
    /// <summary>
    /// 第一版测试墙体生成
    /// 三层墙体中间，随机生成石砖，裂石砖，苔石砖，暗色原石
    /// </summary>
    private void TestGenerateWall()
    {
        float[] wallMidTileProb_ = convertProbArray(wallMidTileProb);

        List<Vector2Int> wallPosition = new List<Vector2Int>();
        for (int i= 0; i < side; i++){
            if (i == 0 || i == side-1)
            {
                for(int j = 0; j < side; j++)
                {
                    wallPosition.Add(new Vector2Int(j, i));
                }
            }
            else
            {
                wallPosition.Add(new Vector2Int(0, i));
                wallPosition.Add(new Vector2Int(side-1, i));
            }
        }
        foreach(Vector2Int p in wallPosition)
        {
            GameObject t_obj = RandomGetObjectByProb(wallMidTile,wallMidTileProb_);
            GameObject t_real = Instantiate(t_obj, new Vector3(0.1f*p.x, 0.1f*p.y, 0), Quaternion.identity);

            //获取小方块的宽度
            //float sprite_width = t_obj.GetComponent<SpriteRenderer>().bounds.size.x;
            //Debug.Log(sprite_width);
            //output:0.16,大约1/6，在scale为1的时候
            t_real.transform.SetParent(boardHolder);
        }
    }

    private List<Vector2Int> TestGenerateWall2(GameObject[] objects,float[] probs,int innerDis,List<Vector2Int> breakPoints)
    {
        //线框顶点序列 3*4
        List<Vector2Int> vertexs = new List<Vector2Int>();
        Vector2Int[] VertexOffset = { new Vector2Int(1,-1), new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) };
        for(int i = 0; i < 4; i++)
        {
            vertexs.Add(breakPoints[2*i]);
            if (i % 2 == 0) {
                vertexs.Add(new Vector2Int(
                breakPoints[2 * ((i + 1) % 4) + 1].x,
                breakPoints[2 * i].y
                ));
            }
            else
            {
                vertexs.Add(new Vector2Int(
                    breakPoints[2 * i].x,
                    breakPoints[2 * ((i + 1) % 4) + 1].y
                ));
            }
            vertexs.Add(breakPoints[2 * ((i + 1) % 4) + 1]);
            for(int f = 0; f < innerDis; f++)
            {
                for(int fi = 1; fi <= 3; fi++)
                {
                    vertexs[vertexs.Count - fi] += VertexOffset[i];
                }
            }
        }
        PrintVector2IntListTool(breakPoints);
        PrintVector2IntListTool(vertexs);
        return vertexs;
    }

    private void SpriteChangeTest(Transform rootTransorm)
    {
        SpriteRenderer[] allSprites = rootTransorm.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer sr in allSprites)
        {
            Debug.Log(sr.sprite.name);
        }
    }
#endif
    #endregion

    /// <summary>
    /// 生成外墙
    /// </summary>
    private void GenerateWalls()  
    {
        Debug.Assert(vertexs != null);

        Vector2Int[] vertexOffset = { new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) };
        //外墙瓷砖累进概率
        float[] wallMidTileProb_ = convertProbArray(wallMidTileProb);
        float[] wallSideTileProb_ = convertProbArray(wallSideTileProb);

        //为了外墙遮罩生成，需要先退一格
        for (int vi = 0; vi < vertexs.Count; vi++)
        {
            vertexs[vi] += -1*vertexOffset[(int)(vi / 3)];
        }

        for (int f = -1; f < 3; f++)
        {
            //坐标集
            List<Vector2Int> wallPoints = new List<Vector2Int>();
            for(int vi = 0; vi < vertexs.Count; vi++)
            {
                Vector2Int v1 = vertexs[vi];
                Vector2Int v2 = vertexs[(vi + 1) % vertexs.Count];
                if (v1.x == v2.x)
                {
                    int y1 = (int)v1.y, y2 = (int)v2.y;
                    if (y1 > y2)
                    {
                        int swap_t = y1;
                        y1 = y2;
                        y2 = swap_t;
                    }
                    for(int i = y1 + 1; i < y2; i++)
                    {
                        wallPoints.Add(new Vector2Int(v1.x,i));
                    }
                }
                else
                {
                    Debug.Assert(v1.y == v2.y);
                    int x1 = (int)v1.x, x2 = (int)v2.x;
                    if (x1 > x2)
                    {
                        int swap_t = x1;
                        x1 = x2;
                        x2 = swap_t;
                    }
                    for (int i = x1 + 1; i < x2; i++)
                    {
                        wallPoints.Add(new Vector2Int(i, v1.y));
                    }
                }
                wallPoints.Add(v1);
            }
            foreach(Vector2Int vt in wallPoints)
            {
                if (f != -1)
                {
                    GameObject t_obj = Instantiate(
                    f == 1 ? RandomGetObjectByProb(wallMidTile, wallMidTileProb_) : RandomGetObjectByProb(wallSideTile, wallSideTileProb_),
                    new Vector2(vt.x * uint_scale, vt.y * uint_scale),
                    Quaternion.identity);
                    t_obj.transform.SetParent(wallTransform);
                }
                chunkInfo[vt] = new ChunkInfo(isWall:true);
            }
            //偏移坐标
            if (f == 2)
                break;

            for(int vi = 0; vi < vertexs.Count; vi++)
            {
                vertexs[vi] += vertexOffset[(int)(vi / 3)];
            }

        }
        
    }

    /// <summary>
    /// 生成建筑
    /// </summary>
    private void GenerateBuildings()
    {
        List<Vector2Int> building_center_list = new List<Vector2Int>();
        int try_times;
        for (int i = 0; i < building1GenerateNum; i++)
        {
            try_times = 0;
            while (!GenerateBuilding(building1Object, building_center_list) && try_times < buildingGenerateRetryTime)
                try_times++;
        }
        for (int i = 0; i < building2GenerateNum; i++)
        {
            try_times = 0;
            while (!GenerateBuilding(building2Object, building_center_list,false) && try_times < buildingGenerateRetryTime)
                try_times++;
        }
    }

    /// <summary>
    /// 生成单个建筑
    /// </summary>
    /// <param name="buildings"></param>
    /// <param name="building_center_list"></param>
    /// <returns></returns>
    private bool GenerateBuilding(GameObject buildings,List<Vector2Int> building_center_list,bool randomRotation = true)
    {
        int edge = 4;
        int min_p = building_size / 2 + edge;
        //开区间
        int max_p = side - edge -1 - min_p;
        bool generateSuccess = false;
        for(int t = 0; t < buildingGenerateRetryTime&&!generateSuccess; t++)
        {
            bool pos_aval = true;
            Vector2Int bc = new Vector2Int
            {
                x = Random.Range(min_p, max_p),
                y = Random.Range(min_p, max_p)
            };
            bc.x = GetXMOD3E0Num(bc.x) + 2;
            bc.y = GetXMOD3E0Num(bc.y) + 2;
            foreach(Vector2Int vertex in vertexs)
            {
                if (ManhattanDistance(vertex, bc) < building_size)
                    pos_aval = false;
            }
            foreach(Vector2Int bcp in building_center_list)
            {
                if (ManhattanDistance(bcp, bc) < buildingMinDis)
                    pos_aval = false;
            }
            if (pos_aval)
            {
                building_center_list.Add(bc);
                GameObject t_obj = Instantiate(
                    buildings,
                    new Vector3(bc.x*uint_scale,bc.y*uint_scale,0),
                    Quaternion.identity
                    );
                if(randomRotation)
                    t_obj.transform.Rotate(new Vector3(0, 0, Random.Range(0, 4) * 90));
                t_obj.transform.SetParent(buildingTransform);
                generateSuccess = true;

                //在chunkInfo中记录
                for(int i = bc.x - building_size / 2; i<= bc.x + building_size / 2; i++)
                {
                    for(int j = bc.y - building_size / 2; j <= bc.y + building_size / 2; j++)
                    {
                        Vector2Int vt = new Vector2Int(i, j);
                        chunkInfo[vt] = new ChunkInfo(isBuilding: true);
                    }
                }
            }
        }
        return generateSuccess;
    }

    /// <summary>
    /// 生成地板
    /// </summary>
    private void GenerateFloors()
    {
        const int maxFloorNum = 10000;
        int generateFloorNum = 0;
        Vector2Int root = new Vector2Int(side / 2+1, side / 2-1);

        Queue<Vector2Int> floorQueue = new Queue<Vector2Int>();
        floorQueue.Enqueue(root);

        while (floorQueue.Count != 0)
        {
            Vector2Int f = floorQueue.Dequeue();

            //如果这个位置已经在字典里-》两种可能，墙体建筑/floor添加的
            if (chunkInfo.ContainsKey(f))
            {
                if (chunkInfo[f].floorHasCheck)
                    continue;
                //如果是墙体，检查一下九宫格是否满，若不满填满九宫格
                //跳出循环
                if (chunkInfo[f].isWall)
                {
                    addFloorSandIfEmpty(f);
                    continue;
                }
                //建筑则尝试填满，同时添加上下左右四个坐标
                if (chunkInfo[f].isBuilding)
                {
                    addFloorSandIfEmpty(f);
                    add4DirectionPos(f, floorQueue);
                }
                else
                    Debug.LogError("非法情况");
                ChunkInfo t = chunkInfo[f];
                t.floorHasCheck = true;
                chunkInfo[f] = t;
            }
            //空白方格，生成实例，传播坐标
            else
            {
                chunkInfo[new Vector2Int(f.x,f.y)] = new ChunkInfo(floorHasCheck: true);
                chunkInfo[new Vector2Int(f.x+1, f.y)] = new ChunkInfo(floorHasCheck: true);
                chunkInfo[new Vector2Int(f.x+1, f.y-1)] = new ChunkInfo(floorHasCheck: true);
                chunkInfo[new Vector2Int(f.x, f.y-1)] = new ChunkInfo(floorHasCheck: true);

                add4DirectionPos(f, floorQueue);
                GameObject obj = Instantiate(
                    floorObject,
                    new Vector3(f.x * uint_scale, f.y * uint_scale, 0),
                    Quaternion.identity
                );
                obj.transform.SetParent(floorTransform);
                addFloorSandIfEmpty(f);
                generateFloorNum++;
                if (generateFloorNum > maxFloorNum)
                {
                    Debug.LogError("广度优先崩溃");
                    break;
                }
            }
            
        }
    }

    void RandomChangeBlocksSprite()
    {
        RandomChangeBlocksSprite(floorTransform, nagaStoneSprite, nagaStoneSprites, nagaStoneSpritesProb);
        RandomChangeBlocksSprite(floorTransform, redSandSprite, redSandSprites, redSandSpritesProb);
        RandomChangeBlocksSprite(buildingTransform, brickSprite, brickSprites, brickSpritesProb);
        RandomChangeBlocksSprite(buildingTransform, nagaStoneSprite, nagaStoneSprites, nagaStoneSpritesProb);
        RandomChangeBlocksSprite(buildingTransform, redSandSprite, redSandSprites, redSandSpritesProb);
    }

    #region tools
    //tools
    //-----------------------------------------------------------//
    /// <summary>
    /// 将概率数组转换为累进概率数组
    /// </summary>
    /// <param name="prob">原始概率数组</param>
    /// <returns></returns>
    private float[] convertProbArray(float[] prob)
    {
        float[] prob_ = new float[prob.Length];
        prob.CopyTo(prob_,0);
        Debug.Assert(prob_.Length >= 1);
        for(int i = 1; i < prob_.Length; i++)
        {
            prob_[i] += prob_[i - 1];
        }
        return prob_;

    }

    /// <summary>
    /// 生成弯折点在正方形边框上的坐标
    /// 按照l，u，r，d顺序生成
    /// 左侧点生成顺序为先上后下，然后顺时针旋转
    /// </summary>
    /// <returns></returns>
    private List<Vector2Int> GenerateBreakPoints()
    {
        List<Vector2Int> breakPoints = new List<Vector2Int>();
        int em = (int)(side * MinLenOfEdgeRate);
        int sm = (int)(side * MinLenOfLedRate);
        for (int i = 0; i < 4; i++)
        {
            int el = Random.Range(em, side - 2 * sm);
            el = GetXMOD3E0Num(el)+2;
            int p1 = side - (el+sm);
            p1 = Random.Range(sm, p1);
            p1 = GetXMOD3E0Num(p1);
            int p2 = side - el - p1;
            switch (i)
            {
                case 0:
                    breakPoints.Add(new Vector2Int(0, side - 1 - p1));
                    breakPoints.Add(new Vector2Int(0, p2 - 1));
                    break;
                case 1:
                    breakPoints.Add(new Vector2Int(side - 1 - p1, side - 1));
                    breakPoints.Add(new Vector2Int(p2 - 1, side - 1));
                    break;
                case 2:
                    breakPoints.Add(new Vector2Int(side - 1, p1 - 1));
                    breakPoints.Add(new Vector2Int(side - 1, side - 1 - p2));
                    break;
                case 3:
                    breakPoints.Add(new Vector2Int(p1 - 1, 0));
                    breakPoints.Add(new Vector2Int(side - 1 - p2, 0));
                    break;
            }
        }
        return breakPoints;
    }

    /// <summary>
    /// 生成线框顶点，从左上开始，顺时针生成
    /// </summary>
    /// <param name="breakPoints">边界转折点</param>
    /// <returns></returns>
    private List<Vector2Int> GenerateVertexList(List<Vector2Int> breakPoints)
    {
        List<Vector2Int> vertexs = new List<Vector2Int>();
        for (int i = 0; i < 4; i++)
        {
            vertexs.Add(breakPoints[2 * i]);
            if (i % 2 == 0)
            {
                vertexs.Add(new Vector2Int(
                breakPoints[2 * ((i + 1) % 4) + 1].x,
                breakPoints[2 * i].y
                ));
            }
            else
            {
                vertexs.Add(new Vector2Int(
                    breakPoints[2 * i].x,
                    breakPoints[2 * ((i + 1) % 4) + 1].y
                ));
            }
            vertexs.Add(breakPoints[2 * ((i + 1) % 4) + 1]);
        }
        return vertexs;
    }


    /// <summary>
    /// 根据累进概率数组随机返回对象
    /// </summary>
    /// <param name="obj">对象数组</param>
    /// <param name="prob">累进概率数组</param>
    /// <returns></returns>
    private T RandomGetObjectByProb<T>(T[] obj,float[] prob, float rand=-1)
    {
        if(rand==-1)
            rand = Random.Range(0f, 1f);
        for(int i = 0; i < obj.Length; i++)
        {
            if (prob[i] > rand)
                return obj[i];
        }
        return default;
    }

    private void PrintVector2IntListTool(List<Vector2Int> vec,string info = "Print Vector2Int Tool")
    {
        Debug.Log(string.Format("-------------{0}-------------", info));
        foreach(Vector2Int t in vec)
        {
            Debug.Log(string.Format("Tool:Vector2Int x={0},y={1}", t.x, t.y));
        }
        Debug.Log("-------------end-------------");
    }

    /// <summary>
    /// 根据顶点列表画线框的工具
    /// 需要放在update里面执行
    /// </summary>
    /// <param name="vec">顶点列表</param>
    private void ConnectVector2IntPointListTool(List<Vector2Int> vec)
    {
        for(int i = 0; i < vec.Count; i++)
        {
            Debug.DrawLine(
                new Vector2(vec[i].x,vec[i].y)*uint_scale,
                new Vector2(vec[(i + 1) % vec.Count].x, vec[(i + 1) % vec.Count].y) * uint_scale,
               Color.blue);
        }
    }
    
    /// <summary>
    /// 计算两个VectorInt类型之间的曼哈顿距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int ManhattanDistance(Vector2Int a,Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private int GetXMOD3E0Num(int x)
    {
        if (x > side / 2)
        {
            x -= 3;
        }
        x += (3 - x % 3);
        Debug.Assert(x % 3 == 0);
        return x;
    }

    /// <summary>
    /// floor生成辅助函数，将中心上下左右四个坐标添加到队列中
    /// </summary>
    /// <param name="p"></param>
    /// <param name="q"></param>
    private void add4DirectionPos(Vector2Int p,Queue<Vector2Int> q)
    {
        q.Enqueue(new Vector2Int(p.x - 3, p.y));
        q.Enqueue(new Vector2Int(p.x + 3, p.y));
        q.Enqueue(new Vector2Int(p.x, p.y - 3));
        q.Enqueue(new Vector2Int(p.x, p.y + 3));
    }

    /// <summary>
    /// floor 生成辅助函数，添加红沙地面
    /// </summary>
    /// <param name="center"></param>
    private void addFloorSandIfEmpty(Vector2Int center)
    {
        int[] offset =new int[] { -1, 0, 1 };
        foreach(int x_off in offset)
        {
            foreach(int y_off in offset)
            {
                Vector2Int pos_t = new Vector2Int(center.x + x_off, center.y + y_off);
                if (!chunkInfo.ContainsKey(pos_t))
                {
                    chunkInfo[pos_t] = new ChunkInfo(floorHasCheck: true);
                    GameObject t_obj = Instantiate(floorSandObject,
                        new Vector3(pos_t.x * uint_scale, pos_t.y * uint_scale, 0),
                        Quaternion.identity);
                    t_obj.transform.SetParent(floorTransform);
                }
            }
        }
    }

    /// <summary>
    /// 根据父组件随即替换某个Sprite为其候选Sprites
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="targetSprite"></param>
    /// <param name="replaceSprites"></param>
    /// <param name="probs"></param>
    private void RandomChangeBlocksSprite(Transform transform, Sprite targetSprite, Sprite[] replaceSprites, float[] probs)
    {
        probs = convertProbArray(probs);
        SpriteRenderer[] sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        //Debug.Log(sprites.Length);
        foreach(SpriteRenderer spr in sprites)
        {
            //Debug.Log(string.Format("{0},{1}",spr.sprite.name, targetSprite.name));
            if (spr.sprite == targetSprite)
            {
                float rand = Random.Range(0f, 1f);
                if (rand < probs[probs.Length - 1])
                {
                    Sprite rps = RandomGetObjectByProb(replaceSprites, probs, rand);
                    spr.sprite = rps;
                }
            }
        }
    }
    #endregion

}
