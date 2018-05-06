using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam
{
    using System.Collections.Generic;		//Allows us to use Lists. 

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
        public bool doingSomething = false;   //Boolean to check if players is doSomething, hidden in inspector but public.

        private int level = 1;                                  //Current level number, expressed in game as "Day 1".
        private bool doingSetup = false;						//Boolean to check if we're setting up board, prevent Player from moving during setup.

        private List<GameItem> m_allItems;                      //All items, load from config.
        private List<GameItem> m_playersItems;                  //player's items.
        

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
            m_allItems = GameConfig.instance.itemConfigs;
        }

        //Initializes the game for each level.
        void InitGame()
        {
            loadGameConfig();

            m_playersItems = new List<GameItem>();
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

        public void AddItemToPlayer(int itemID)
        {
            foreach (GameItem item in m_allItems)
            {
                if (item.itemID == itemID)
                {
                    m_playersItems.Add(item);
                    Debug.Log("获得物品：" + item.itemName);
                    break;
                }
            }
        }

        public bool PlayHasItem(int itemID)
        {
            bool flag = false;
            foreach (GameItem item in m_playersItems)
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

