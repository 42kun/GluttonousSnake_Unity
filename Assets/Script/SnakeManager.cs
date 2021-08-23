using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnakeManager : SnakeTrigger
{
    private SnakeObject snakeHead;
    private SnakeObject snakeBodyTail;
    private Rigidbody2D snakeHeadRigid;
    public GameObject snakeBody;
    Transform snakeHolder;
    // 初始蛇长度
    public int initSnakeLength = 2;
    //蛇身间隔
    public float snakeBodyDis = .3f;
    // 蛇移速
    public float speed = 100;
    // 蛇转向速度
    public float turnSpeed = 18;
    // 方向键加速倍率
    public float speedUpRate = 1.5f;
    // 长度偏移间隔,以间隔offset的距离传播速度变化
    public int offset = 5;

    bool first_move = true;


    List<Coroutine> snakeCoroutines;

    public class SnakeObject
    {
        public Queue<Vector2> velocity;
        public Queue<Quaternion> rotation;
        public GameObject obj;
        public Rigidbody2D rb2D;
        public SnakeObject pre;
        public SnakeObject next;
        public SnakeObject(GameObject _obj)
        {
            obj = _obj;
            rb2D = obj.GetComponent<Rigidbody2D>();
            velocity = new Queue<Vector2>();
            rotation = new Queue<Quaternion>();
            pre = null;
            next = null;
        }

        public void Update()
        {
            if (velocity.Count != 0)
            {
                velocity.Dequeue();
                rotation.Dequeue();
            }
            velocity.Enqueue(rb2D.velocity);
            rotation.Enqueue(obj.transform.rotation);
        }

        public void EnqueState(Vector2 v,Quaternion q)
        {
            velocity.Enqueue(v);
            rotation.Enqueue(q);
        }

        public void DequeState(out Vector2 v,out Quaternion q)
        {
            v = velocity.Dequeue();
            q = rotation.Dequeue();
        }
        
        public void PeekState(out Vector2 v, out Quaternion q)
        {
            v = velocity.Peek();
            q = rotation.Peek();
        }

        public void ApplyState()
        {
            obj.transform.rotation = rotation.Peek();
            rb2D.velocity = velocity.Peek();
        }
    }
    protected override void Awake()
    {
        base.Awake();
        snakeHolder = new GameObject("najia").transform;
        snakeHead = new SnakeObject(this.gameObject);
        snakeHeadRigid = GetComponent<Rigidbody2D>();
        snakeHead.obj.transform.SetParent(snakeHolder);
        GenerateSnakeBodies();
    }
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        MoveSnakeHead();
        MoveSnakeBodies();
        DebugDrawSnake();
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Debug.Log(collision);
    //}
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    Debug.Log(collision.gameObject.tag);
    //}

    /// <summary>
    /// 初始化蛇身
    /// </summary>
    void GenerateSnakeBodies()
    {
        Vector2 headPos = snakeHead.obj.transform.position;
        SnakeObject pre = snakeHead;
        SnakeObject s_obj = null;
        for(int i = 1; i < initSnakeLength; i++)
        {
            GameObject t_obj = Instantiate(snakeBody, new Vector3(headPos.x - i * snakeBodyDis, headPos.y, 0), Quaternion.identity);
            t_obj.transform.SetParent(snakeHolder);
            s_obj = new SnakeObject(t_obj);
            pre.next = s_obj;
            s_obj.pre = pre;
            pre = s_obj;
        }
        snakeBodyTail = s_obj;
    }

    /// <summary>
    /// 蛇头控制，同时更新蛇头状态
    /// 为蛇身赋予初始状态
    /// </summary>
    public void MoveSnakeHead()
    {
        if (first_move)
        {
            //必须要给蛇所有部件有一个初始速度,同时填满状态数组
            SnakeObject p = snakeHead;
            while (p != null)
            {
                GameObject obj = p.obj;
                p.obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                p.rb2D.velocity = -snakeHead.obj.transform.up * speed * Time.fixedDeltaTime;
                if (p==snakeHead)
                {
                    p.rotation.Enqueue(p.obj.transform.rotation);
                    p.velocity.Enqueue(p.rb2D.velocity);
                }
                else
                {
                    for(int i = 0; i < offset; i++)
                    {
                        p.EnqueState(p.rb2D.velocity, p.obj.transform.rotation);
                    }
                }
                p = p.next;
            }
            first_move = false;
            return;
        }
        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");
        //Debug.Log(string.Format("input=({0},{1})", horizontal, vertical));
        if (horizontal == 0 && vertical == 0)
        {
            snakeHeadRigid.velocity = snakeHeadRigid.velocity.normalized * speed * Time.fixedDeltaTime;
            snakeHead.Update();
        }
        else
        {
            Vector2 expectVector = new Vector2(horizontal, vertical).normalized;
            Vector2 nowVector = snakeHeadRigid.velocity.normalized;
            float angle = VectorAngle(nowVector, expectVector);
            float turnAngle = Mathf.Abs(angle) < turnSpeed * Time.fixedDeltaTime ? Mathf.Abs(angle) : turnSpeed * Time.fixedDeltaTime;
            float sign = Mathf.Sign(angle);
            Vector2 turnVector = Rotate(nowVector, new Vector3(0,0,1), -turnAngle * sign);
            //更新速度
            snakeHeadRigid.velocity = speed * speedUpRate * Time.fixedDeltaTime * turnVector;
            //更新角度
            float absAngle = -VectorAngle(new Vector2(1, 0), turnVector);
            //Debug.Log(absAngle);
            absAngle = absAngle > 0 ? absAngle : absAngle + 360;
            absAngle = (absAngle + 90) % 360;
            snakeHead.obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, absAngle));
            snakeHead.Update();
        }
    }
    //每distanceBetween更新一次状态数组，其余时间移动即可

    /// <summary>
    /// 更新蛇身状态，同时移动蛇身
    /// </summary>
    void MoveSnakeBodies()
    {
        SnakeObject p = snakeHead.next;
        //速度状态向后蠕动一格
        while (p != null)
        {
            Vector2 v;
            Quaternion q;
            p.pre.DequeState(out v, out q);
            p.EnqueState(v, q);
            if (p.next == null)
            {
                p.DequeState(out v, out q);
            }
            p = p.next;
        }
        p = snakeHead.next;
        while (p != null)
        {
            p.ApplyState();
            p = p.next;
        }
    }

    /// <summary>
    /// 增长一节
    /// 先计算应该生成的位置
    /// 再更新链表
    /// </summary>
    public void snakeGrow()
    {
        //获取尾巴与尾巴前一节的距离
        float dis = (snakeBodyTail.pre.obj.transform.position - snakeBodyTail.obj.transform.position).magnitude;
        //尾巴速度归一化
        Vector2 vel = snakeBodyTail.velocity.Peek().normalized;
        Vector2 pos = (Vector2)snakeBodyTail.obj.transform.position - vel * dis;

        GameObject t_obj = Instantiate(snakeBody, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        t_obj.transform.SetParent(snakeHolder);

        SnakeObject s_obj = new SnakeObject(t_obj);
        for(int i = 0; i < offset; i++)
        {
            s_obj.velocity.Enqueue(snakeBodyTail.velocity.Peek());
            s_obj.rotation.Enqueue(snakeBodyTail.rotation.Peek());
        }
        s_obj.ApplyState();

        s_obj.pre = snakeBodyTail;
        snakeBodyTail.next = s_obj;

        snakeBodyTail = s_obj;
    }

    /// <summary>
    /// 傻逼协程，卡成狗
    /// </summary>
    //protected IEnumerator SmoothMovement(SnakeObject p,float moveTime)
    //{
    //    float time = 0;
    //    float inverseMoveTime = 1 / moveTime;
    //    //Debug.Log("start");
    //    Vector2 oldPosition = p.obj.transform.position;
    //    Quaternion oldPRotation = p.obj.transform.rotation;
    //    const int lerp = 16;
    //    while (time < moveTime)
    //    {
    //        for(int i = 0; i < lerp; i++)
    //        {
    //            Vector2 newPosition = Vector2.Lerp(oldPosition, p.position, time * inverseMoveTime);
    //            Quaternion newRotation = Quaternion.Slerp(oldPRotation, p.rotation, time * inverseMoveTime);
    //            p.obj.transform.position = newPosition;
    //            p.obj.transform.rotation = newRotation;
    //            time += Time.deltaTime/4;
    //            yield return null;
    //        }
    //        //Vector2 newPosition = Vector2.Lerp(oldPosition,p.position, time*inverseMoveTime);
    //        //Quaternion newRotation = Quaternion.Slerp(oldPRotation, p.rotation, time * inverseMoveTime);
    //        //Debug.Log(string.Format("old position ({0},{1}), new position ({2},{3}),dis = {4},deltatime={5},v={6},total_time={7}",
    //        //    oldPosition[0],
    //        //    oldPosition[1],
    //        //    newPosition[0],
    //        //    newPosition[1], 
    //        //    -oldPosition[0]+ newPosition[0],
    //        //    Time.deltaTime,
    //        //    (-oldPosition[0] + newPosition[0])/time,
    //        //    time));
    //        //time += Time.deltaTime;
    //        //yield return null;
    //    }
    //    p.rb2D.MovePosition(p.position);
    //}

    #region OtherTools
    /// <summary>
    /// 计算一个Vector2绕指定轴旋转指定角度后所得到的向量。
    /// </summary>
    /// <param name="source">旋转前的源Vector3</param>
    /// <param name="axis">旋转轴</param>
    /// <param name="angle">旋转角度</param>
    /// <returns>旋转后得到的新Vector3</returns>
    public Vector2 Rotate(Vector2 source, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);// 旋转系数
        return q * source;// 返回目标向量
    }
    /// <summary>
    /// 带正负的（逆时针为正）的获取向量角度
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    float VectorAngle(Vector2 from, Vector2 to)
    {
        float angle;

        Vector3 cross = Vector3.Cross(from, to);
        angle = Vector2.Angle(from, to);
        return cross.z > 0 ? -angle : angle;
    }


    void DebugDrawSnake()
    {
        SnakeObject p = snakeHead.next;
        while (p != null)
        {
            Debug.DrawLine(p.obj.transform.position, (Vector2)p.obj.transform.position + p.velocity.Peek());
            p = p.next;
        }
    }
    #endregion
}
