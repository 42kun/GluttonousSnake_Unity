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
    // ��ʼ�߳���
    public int initSnakeLength = 2;
    //������
    public float snakeBodyDis = .3f;
    // ������
    public float speed = 100;
    // ��ת���ٶ�
    public float turnSpeed = 18;
    // ��������ٱ���
    public float speedUpRate = 1.5f;
    // ����ƫ�Ƽ��,�Լ��offset�ľ��봫���ٶȱ仯
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
    /// ��ʼ������
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
    /// ��ͷ���ƣ�ͬʱ������ͷ״̬
    /// Ϊ�������ʼ״̬
    /// </summary>
    public void MoveSnakeHead()
    {
        if (first_move)
        {
            //����Ҫ�������в�����һ����ʼ�ٶ�,ͬʱ����״̬����
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
            //�����ٶ�
            snakeHeadRigid.velocity = speed * speedUpRate * Time.fixedDeltaTime * turnVector;
            //���½Ƕ�
            float absAngle = -VectorAngle(new Vector2(1, 0), turnVector);
            //Debug.Log(absAngle);
            absAngle = absAngle > 0 ? absAngle : absAngle + 360;
            absAngle = (absAngle + 90) % 360;
            snakeHead.obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, absAngle));
            snakeHead.Update();
        }
    }
    //ÿdistanceBetween����һ��״̬���飬����ʱ���ƶ�����

    /// <summary>
    /// ��������״̬��ͬʱ�ƶ�����
    /// </summary>
    void MoveSnakeBodies()
    {
        SnakeObject p = snakeHead.next;
        //�ٶ�״̬����䶯һ��
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
    /// ����һ��
    /// �ȼ���Ӧ�����ɵ�λ��
    /// �ٸ�������
    /// </summary>
    public void snakeGrow()
    {
        //��ȡβ����β��ǰһ�ڵľ���
        float dis = (snakeBodyTail.pre.obj.transform.position - snakeBodyTail.obj.transform.position).magnitude;
        //β���ٶȹ�һ��
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
    /// ɵ��Э�̣����ɹ�
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
    /// ����һ��Vector2��ָ������תָ���ǶȺ����õ���������
    /// </summary>
    /// <param name="source">��תǰ��ԴVector3</param>
    /// <param name="axis">��ת��</param>
    /// <param name="angle">��ת�Ƕ�</param>
    /// <returns>��ת��õ�����Vector3</returns>
    public Vector2 Rotate(Vector2 source, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);// ��תϵ��
        return q * source;// ����Ŀ������
    }
    /// <summary>
    /// �������ģ���ʱ��Ϊ�����Ļ�ȡ�����Ƕ�
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
