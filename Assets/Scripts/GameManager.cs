using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace Completed
{
    using System.Collections.Generic;		//Allows us to use Lists. 

    struct PlayerItem
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
    }

    public class GameManager : MonoBehaviour
    {
        public float turnDelay = 0.1f;                          //Delay between each Player turn.
        public int playerFoodPoints = 1000;						//Starting value for Player food points.
        public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
        [HideInInspector]
        public bool playersTurn = true;       //Boolean to check if it's players turn, hidden in inspector but public.
        [HideInInspector]
        public bool playerInDialog = false;   //Boolean to check if players is in dialog, hidden in inspector but public.
        [HideInInspector]
        public bool doingSomething = false;   //Boolean to check if players is in dialog, hidden in inspector but public.

        private int level = 1;                                  //Current level number, expressed in game as "Day 1".
        private bool doingSetup = false;						//Boolean to check if we're setting up board, prevent Player from moving during setup.

        private List<PlayerItem> m_allItems;                      //All items, load from config.
        private List<PlayerItem> m_playersItems;                  //player's items.

        private string m_itemsConfigPath = "Data/itemsConfig.txt";

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

        //Awake is always called before any Start functions
        void Awake()
        {
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            //Call the InitGame function to initialize the first level 
            InitGame();
        }

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }


        void loadGameConfig()
        {
            m_allItems = LoadJsonConfigs<PlayerItem>(m_itemsConfigPath);
        }

        //Initializes the game for each level.
        void InitGame()
        {
            loadGameConfig();


        }

        //Update is called every frame.
        void Update()
        {
            //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if (playersTurn || doingSomething || doingSetup || playerInDialog)

                //If any of these are true, return and do not start MoveEnemies.
                return;

            //Start moving enemies.
            StartCoroutine(DoSomeThing());
        }

        void AddItemToPlayer(int itemID)
        {
            foreach (PlayerItem item in m_allItems)
            {
                if (item.itemID == itemID)
                {
                    m_playersItems.Add(item);
                    break;
                }
            }
        }

        bool PlayHasItem(int itemID)
        {
            bool flag = false;
            foreach (PlayerItem item in m_playersItems)
            {
                if (item.itemID == itemID)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        //GameOver is called when the player reaches 0 food points
        public void GameOver()
        {
            //Disable this GameManager.
            enabled = false;
        }

        //Coroutine to move enemies in sequence.
        IEnumerator DoSomeThing()
        {
            doingSomething = true;
            //Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);
            yield return new WaitForSeconds(turnDelay);

            //Once Enemies are done moving, set playersTurn to true so player can move.
            doingSomething = false;
            playersTurn = true;
        }
    }
}

