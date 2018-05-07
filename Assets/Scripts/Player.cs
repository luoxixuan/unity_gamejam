using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace GameJam
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class Player : MovingObject
    {
        public AudioClip m_moveSound1;              //1 of 2 Audio clips to play when player moves.
        public AudioClip m_moveSound2;              //2 of 2 Audio clips to play when player moves.
        public AudioClip m_gameOverSound;             //Audio clip to play when player dies.
        public Text m_foodText;                     //UI Text to display current player food total.
        public int m_speed = 1;                     //UI Text to display current player food total.

        private Animator m_animator;                    //Used to store a reference to the Player's animator component.
        private int m_playFood;                         //Used to store player food points total during level.
        private int m_lastHorizental;                   //上次走的横轴面向
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif


        //Start overrides the Start function of MovingObject
        protected override void Start()
        {
            //Get a component reference to the Player's animator component
            m_animator = GetComponent<Animator>();

            //Get the current food point total stored in GameManager.instance between levels.
            m_playFood = GameManager.instance.playerFoodPoints;

            //Set the foodText to reflect the current player food total.
            m_foodText.text = "Food: " + m_playFood;

            //Call the Start function of the MovingObject base class.
            base.Start();
        }


        //This function is called when the behaviour becomes disabled or inactive.
        private void OnDisable()
        {
            //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
            GameManager.instance.playerFoodPoints = m_playFood;
        }


        private void CheckPlayerMove()
        {
            //If it's not the player's turn, exit the function.
            if (!GameManager.instance.playersTurn || GameManager.instance.playerInDialog) return;

            int horizontal = 0;     //Used to store the horizontal move direction.
            int vertical = 0;       //Used to store the vertical move direction.

            //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER

            //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            //Check if moving horizontally, if so set vertical to zero.
            if (horizontal != 0)
            {
                vertical = 0;
            }
            //Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            //Check if Input has registered more than zero touches
            if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
                    //If so, set touchOrigin to the position of that touch,之前是滑一下走一下，现在是点一下走一下
                    //touchOrigin = myTouch.position;
                    touchOrigin = transform.position;

                }

                //If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
                //else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
                else if (myTouch.phase == TouchPhase.Ended)
                {
                    //Set touchEnd to equal the position of this touch
                    //Vector2 touchEnd = myTouch.position;
                    Vector2 touchEnd = Camera.main.ScreenToWorldPoint(myTouch.position);

                    //Calculate the difference between the beginning and end of the touch on the x axis.
                    float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					//touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
                        horizontal = Mathf.Abs(x) > 1? horizontal : 0;
                    }
					else
                    {
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
                        vertical = Mathf.Abs(y) > 1? vertical : 0;
                    }
                    
                    m_foodText.text = touchEnd.x + "," + touchEnd.y;

                }
			}
			
#endif //End of mobile platform dependendent compilation section started above with #elif
            //Check if we have a non-zero value for horizontal or vertical
            if (horizontal != 0 || vertical != 0)
            {
                //Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
                //Pass in horizontal and vertical as parameters to specify the direction to move Player in.
                // 调整角色面向
                if (m_lastHorizental != horizontal && horizontal != 0) {
                    float ratationY = horizontal < 0 ? -180 : 0;
                    gameObject.transform.rotation = new Quaternion(0, ratationY, 0, 0);
                    m_lastHorizental = horizontal;
                }
                AttemptMove<Wall>(horizontal * m_speed, vertical); //只有横轴的速度可以调。。主要是为了同步走路的动画
            }
        }


        private void Update()
        {
            CheckPlayerMove();
        }

        //AttemptMove overrides the AttemptMove function in the base class MovingObject
        //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //Every time player moves, subtract from food points total.
            m_playFood--;

            //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
            base.AttemptMove<T>(xDir, yDir);

            //Hit allows us to reference the result of the Linecast done in Move.
            RaycastHit2D hit;

            //If Move returns true, meaning Player was able to move into an empty space.
            if (Move(xDir, yDir, out hit))
            {
                //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
                SoundManager.instance.RandomizeSfx(m_moveSound1, m_moveSound2);
                m_animator.SetTrigger("playerMove");
                //Debug.Log("player move once");
            }

            //Set the playersTurn boolean of GameManager to false now that players turn is over.
            GameManager.instance.playersTurn = false;

            //Since the player has moved and lost food points, check if the game has ended.
            CheckIfGameOver();
        }


        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
            //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
            //m_animator.SetTrigger ("playerChop");
        }


        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            /*
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit")
			{
				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay); //这个Invoke得研究一下
				
				//Disable the player object since level is over.
				enabled = false;
			}
			*/
        }


        //Restart reloads the scene when called.
        private void Restart()
        {
            //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }


        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (m_playFood <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                SoundManager.instance.PlaySingle(m_gameOverSound);

                //Stop the background music.
                SoundManager.instance.musicSource.Stop();

                //Call the GameOver function of GameManager.
                GameManager.instance.GameOver();
            }
        }
    }
}

