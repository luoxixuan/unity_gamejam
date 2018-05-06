﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace GameJam
{
    // 装饰后续List读取类型
    public enum DecorateType
    {
        DecType_None,   // 无
        DecType_Random, // 在List中随机一个替换
        DecType_Order,  // 按照List顺序依次替换
        DecType_Talk,   // 触发对话
        DecType_GetItem,// 获得物品
        DecType_GameOver,// 游戏结束
    }

    // 事件
    [System.Serializable]
    public struct GameEvent
    {
        public int eventID { get; set; }           //事件id
        public DecorateType eventType { get; set; } //事件类型：0无，1随机倒地，2顺序，3触发对话，4获得物品
        public int eventTriger { get; set; }     //触发事件的前置条件，暂时就是物品id吧，0就是没条件，大于0的id表示有这个物品才能触发，小于0的表示没有这物品才能触发
        public int eventResult { get; set; }     //如果是获得物品的事件那就是物品id，如果是对话，暂时就是对话的名字，如果是物品落地之类的；
        public int isRepeat { get; set; }        //0不能重复触发，1可以重复触发；
        public int canTrigger { get; set; }      //0当前游戏状态不能触发，1可以触发，这个状态在配置里就配成1就行
    }

    //物品
    [System.Serializable]
    public struct GameItem
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
    }

    //单句对话
    [System.Serializable]
    public class DialogItem
    {
        public string Name { get; set; }
        public string DialogText { get; set; }
        public string IconPath { get; set; }
    }

    //一段对话
    [System.Serializable]
    public class DialogNode
    {
        public int DialogID { get; set; }
        public string DialogName { get; set; }
        //public DialogItem[] DialogArr { get; set; }
        public List<DialogItem> DialogArr { get; set; }
    }

    public class GameConfig : MonoBehaviour
    {
        [HideInInspector]
        public List<GameItem> itemConfigs { get; set; } //物品配置
        private string itemConfigsPath = "Data/itemConfigs.txt";

        [HideInInspector]
        public List<GameEvent> eventConfigs { get; set; } //事件配置
        private string eventConfigsPath = "Data/eventConfigs.txt";

        [HideInInspector]
        public List<DialogNode> dialogConfigs { get; set; } //对话配置
        private string dialogConfigsPath = "Data/dialogText.txt";
        

        public static GameConfig instance = null;

        // 一个模板函数，用来加载放在Assets\StreamingAssets\Data目录下的json配置
        public static List<T> LoadJsonConfigs<T>(string configPath)
        {
            List<T> configItems = null;

#if UNITY_STANDALONE || UNITY_IPHONE
            configPath = Application.streamingAssetsPath + "/" + configPath;
            if (false == File.Exists(configPath))
            {
                Debug.Log("DialogManage::LoadDialogs File Not Exists! " + configPath);
                return null;
            }

            StreamReader sr = new StreamReader(configPath);
            JsonSerializer serializer = new JsonSerializer();
            List<T> nodes = (List<T>)serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            configItems = nodes;
            sr.Dispose();
#elif UNITY_ANDROID
            string strJson = AndroidAssetLoadSDK.LoadTextFile(configPath);
            List<T> nodes = JsonConvert.DeserializeObject<List<T>>(strJson);
            configItems = nodes;
#endif

            return configItems;
        }

        void LoadGameConfigs()
        {
            itemConfigs = LoadJsonConfigs<GameItem>(itemConfigsPath);
            eventConfigs = LoadJsonConfigs<GameEvent>(eventConfigsPath);
            dialogConfigs = LoadJsonConfigs<DialogNode>(dialogConfigsPath);
        }
        void Awake() {
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);

            LoadGameConfigs();
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        
    }
}