using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDecorateItem : MonoBehaviour {

    // 装饰后续List读取类型
    public enum DecorateType
    {
        DecType_None,   // 无
        DecType_Random, // 在List中随机一个替换
        DecType_Order,  // 按照List顺序依次替换
        DecType_Talk,   // 触发对话
    }

    public GameObject TouchItemPrefab;
    public DecorateType m_eDecType; // 触发 List读取类型

    public List<Sprite> m_lDecList;
    public bool m_bIsRepeat;    // 是否可重复触发
    public bool m_bNeedFall;    // 是否需要倒下
    public string m_sDialogName;    // 如果是对话 对话名

    private SpriteRenderer m_spRenderer;
    private PolygonCollider2D m_collider;
    private Vector3 m_vEulerRotation;

    private GameObject m_gColliderGo;
    private GameObject m_touchItem;

    private bool isOnTrigger = false; //不是立即触发东西，加个状态标记下，当前有没有在屏幕上显示触发开关

	// Use this for initialization
	void Start () {
        m_spRenderer = this.GetComponent<SpriteRenderer>();
        m_collider = this.GetComponent<PolygonCollider2D>();
        if (null == TouchItemPrefab) //如果场景中预设了这边就不设置了
        {
            TouchItemPrefab = (GameObject)Resources.Load("Prefabs/TouchMe");

        }
    }
	
	// Update is called once per frame
	void Update () {
	}

    //这个可能会被外部对象调用
    public void doTrigger()
    {
        Debug.Log("SceneDecorateItem::doTrigger: " + m_eDecType);
        if (m_eDecType == DecorateType.DecType_Random)
        {
            if (null != m_lDecList && m_lDecList.Count > 0)
            {
                int randInt = Random.Range(0, m_lDecList.Count - 1);
                m_spRenderer.sprite = m_lDecList[randInt];
                if (true == m_bNeedFall)
                {
                    if (m_gColliderGo.transform.position.x <= transform.position.x)
                    {
                        m_vEulerRotation = this.transform.localEulerAngles;
                        m_vEulerRotation.z = -90.0f;
                        this.transform.localEulerAngles = m_vEulerRotation;
                    }
                    else
                    {
                        m_vEulerRotation = this.transform.localEulerAngles;
                        m_vEulerRotation.z = 90.0f;
                        this.transform.localEulerAngles = m_vEulerRotation;
                    }
                }

                m_collider.isTrigger = false;
            }
        }
        else if (m_eDecType == DecorateType.DecType_Talk)
        {
            if (null == DialogManage.m_me)
            {
                return;
            }
            if (null != m_sDialogName && m_sDialogName.Length > 0)
            {
                DialogManage.m_me.StartDialogByName(m_sDialogName);
            }
            m_collider.isTrigger = false;
        }
        destroyTouchItem(); //用完就删，拔吊无情
    }

    // 创建一个可以点击的叹号
    private void createTouchItem() {
        if (null == m_touchItem)
        {
            m_touchItem = Instantiate(TouchItemPrefab);
            m_touchItem.transform.parent = gameObject.transform.parent;
            m_touchItem.transform.position = gameObject.transform.position;
            TouchItem touchItem = m_touchItem.GetComponent<TouchItem>();
            touchItem.decorateItem = this;
            isOnTrigger = true;
            //Completed.GameManager.instance.playerInDialog = true; //干脆有东西就不让走好了。。
        }
    }

    private void destroyTouchItem() {
        if (m_touchItem)
        {
            GameObject.Destroy(m_touchItem);
            m_touchItem = null;
            isOnTrigger = false; //销毁后把这个状态置空
            //Completed.GameManager.instance.playerInDialog = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_gColliderGo = collision.gameObject;
        //Debug.Log("SceneDecorateItem::OnTriggerEnter2D: "+m_gColliderGo.name);
        if (m_bIsRepeat && !isOnTrigger) //可以重复触发的东西，再次进来的时候激活下，已经触发的就不再继续触发
            //m_collider.isTrigger = true;
            createTouchItem();
            //doTrigger();
        if (!m_bIsRepeat && m_collider.isTrigger) //不能重复触发的东西，进来直接做动作
            doTrigger();
            //createTouchItem();
    }

    // 对话重复触发不知道怎么做=-=
    private void OnTriggerExit2D(Collider2D collision)
    {
        //Debug.Log("SceneDecorateItem::OnTriggerExit2D: " + m_gColliderGo.name);
        if (true == m_bIsRepeat)
        {
            //m_collider.isTrigger = true;
        }
        destroyTouchItem();
    }

    // 碰撞时每帧检测
    private void OnTriggerStay2D(Collider2D collision)
    {
        //Debug.Log("SceneDecorateItem::OnTriggerStay2D: " + m_gColliderGo.name);
        if (isOnTrigger)
        {
            //createTouchItem();
            if (Input.GetKeyUp(KeyCode.Backspace))
            {
                doTrigger(); //pc上也可以用键盘触发
            }
        }
    }

    // 设置Trigger
    public void SetTrigger(bool value)
    {
        m_collider.isTrigger = value;
    }
}
