using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
    public class SceneDecorateItem : MonoBehaviour
    {
        public GameObject TouchItemPrefab;
        
        public List<Sprite> m_lDecList; //
        public bool m_bNeedFall;        // 是否需要倒下

        [Tooltip("物体能够触发的事件的id数组")]
        public List<int> m_eventIDs = null;   //事件的id数组

        private List<GameEvent> m_eventList = null; //可触发事件的列表,在运行时根据在unity编辑器里配置的IDs数组赋值
        private SpriteRenderer m_spRenderer;
        private PolygonCollider2D m_collider;
        private Vector3 m_vEulerRotation;

        private GameObject m_gColliderGo;
        private GameObject m_touchItem;

        private bool m_onTrigger = false; //不是立即触发东西，加个状态标记下，当前有没有在屏幕上显示触发开关

        // Use this for initialization
        void Start()
        {
            m_spRenderer = this.GetComponent<SpriteRenderer>();
            m_collider = this.GetComponent<PolygonCollider2D>();

            if (null == TouchItemPrefab) //如果场景中没有预设这边就用交互物品
            {
                TouchItemPrefab = (GameObject)Resources.Load("Prefabs/TouchMe");
            }
            m_eventList = new List<GameEvent>();
            //添加事件，好像效率有点低，需要循环两个表
            if (m_eventIDs != null)
            {
                foreach (int id in m_eventIDs)
                {
                    foreach (GameEvent e in GameConfig.instance.eventConfigs)
                    {
                        if (e.eventID == id)
                        {
                            m_eventList.Add(e);
                        }
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void doDecorateItemEvent() {
            bool deleteEventTrigger = false;
            if (null != m_eventList && m_eventList.Count > 0) {
                for (int i = 0; i < m_eventList.Count; )
                {
                    GameEvent e = m_eventList[i];
                    deleteEventTrigger = doEventTrigger(e);
                    if (deleteEventTrigger)
                    {
                        m_eventList.Remove(e); //成功触发就删除这个事件
                    }
                    else
                    {
                        i++;
                    }

                }
            }

            if (null != m_eventList && m_eventList.Count == 0) {
                m_collider.isTrigger = true; //事件全部触发完成，标记置为true，后面就不再出交互的选项了
            }
            else
                m_collider.isTrigger = false; //否则就是没有全部触发完
        }

        //加个返回值判断下事件触发了需不要删除
        bool doEventTrigger(GameEvent gEvent)
        {
            bool deleteEventTrigger = true; //默认触发了就删掉, 除非触发失败或者事件可以多次触发
            //如果有触发条件但是玩家没有对应的物品那就不触发
            if (gEvent.eventTriger != 0 && !GameManager.instance.PlayHasItem(gEvent.eventTriger)) 
            {
                return false;
            }

            DecorateType eventType = gEvent.eventType;
            //Debug.Log("SceneDecorateItem::doTrigger: " + m_eDecType);
            if (eventType == DecorateType.DecType_Random)
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

                }
            }
            else if (eventType == DecorateType.DecType_Talk)
            {
                if (null == DialogManage.m_me)
                {
                    return false;
                }
                else if (gEvent.eventResult > 0)
                {
                    DialogManage.m_me.StartDialogByID(gEvent.eventResult);
                }
            }
            else if (eventType == DecorateType.DecType_GetItem)
            {
                if (null == GameManager.instance)
                {
                    return false;
                }
                else if (gEvent.eventResult > 0)
                {
                    GameManager.instance.AddItemToPlayer(gEvent.eventResult);
                }
            }
            
            destroyTouchItem();
            if (deleteEventTrigger) {
                deleteEventTrigger = gEvent.isRepeat == 0; //可以重复触发的事件不能删除，不能重复触发的触发完删除
            }

            return deleteEventTrigger;
        }

        // 创建一个可以点击的叹号
        private void createTouchItem()
        {
            if (null == m_touchItem && m_eventList != null && m_eventList.Count > 0)
            {
                m_touchItem = Instantiate(TouchItemPrefab);
                m_touchItem.transform.parent = gameObject.transform.parent;
                m_touchItem.transform.position = gameObject.transform.position;
                TouchItem touchItem = m_touchItem.GetComponent<TouchItem>();
                touchItem.decorateItem = this;
                m_onTrigger = true;
                //Completed.GameManager.instance.playerInDialog = true; //干脆有东西就不让走好了。。
            }
        }

        private void destroyTouchItem()
        {
            if (m_touchItem)
            {
                GameObject.Destroy(m_touchItem);
                m_touchItem = null;
                m_onTrigger = false; //销毁后把这个状态置空
                //Completed.GameManager.instance.playerInDialog = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            m_gColliderGo = collision.gameObject;
            //Debug.Log("SceneDecorateItem::OnTriggerEnter2D: "+m_gColliderGo.name);
            if (!m_onTrigger) //可以重复触发的东西，再次进来的时候激活下，已经触发的就不再继续触发
                createTouchItem();
        }

        // 出去之后销毁
        private void OnTriggerExit2D(Collider2D collision)
        {
            //Debug.Log("SceneDecorateItem::OnTriggerExit2D: " + m_gColliderGo.name);
            destroyTouchItem();
        }

        // 碰撞时每帧检测
        private void OnTriggerStay2D(Collider2D collision)
        {
            //Debug.Log("SceneDecorateItem::OnTriggerStay2D: " + m_gColliderGo.name);
            if (m_onTrigger)
            {
                if (Input.GetKeyUp(KeyCode.Backspace))
                {
                    doDecorateItemEvent(); //pc上也可以用键盘触发
                }
            }
        }

        // 设置Trigger
        public void SetTrigger(bool value)
        {
            m_collider.isTrigger = value;
        }
    }
}