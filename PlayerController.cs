using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(InputManager), typeof(AnimationManager))]
public class PlayerController : MonoBehaviour
{
    public enum MoveDirection
    {
        STRAGHT,
        RIGHT,
        LEFT
    }

    public enum PowerUp
    {
        NONE,
        SHIELD,
        MULT
    }

    public float speed;//Rider default speed
    public float handling;//Craft default handling
    [Range(0,1f)]
    public float handlingSmooth;
    //Rider rotation modifiers (roll & turn)
    public float rollSpeed;
    public float rollLimit;
    public float turnSpeed;
    public float turnLimit;
    public float energyIncrease;//Amount of energy added to a total player energy amount after collision with energy gate
    public float invinsibleTime;//Player invinsibility time
    public float shieldTime;//Shield effect time
	public GameObject board; //Player rider board object
	public LayerMask groundCollisions;//Layer of all ground collisions which should adjust rider rotation
	public float groundFriction;//Ground friction amount
	[Range(0,1f)]
	public float angleSmooth;//Player speed change based on angle smoothing

    private bool _hasCrashed;//Player crash state flag
    private bool _hasEnergy;
    private float _powerUpTimer;//Power up active timer
    private float _currentSpeed;//In-game player speed value
    private float _currentHandling;//In-game player handling value
    private float _currentAltitude;//In-game player altitude value
    private float _currentRoll;//Rider current roll value
    private float _currentTurn;//Rider current turn value
	private float _currentAngle;//Current mountain angle
	private string _centerBoardTag;//Board center collided object tag
	private string _frontBoardTag;//Board front side collided object tag
	private string _backBoardTag;//Board end side collided object tag
	private float _terrainAngle;//Current mountain angle

    private MoveDirection _dir; //Player movement direction (Required for input manager)
	private AnimationManager _anim;//Player animation manager
    private PowerUp _powerUp;//Collected power up
    private GameManager _gm;//Game central manager
    private Rigidbody _rigidbody;//Player rigidbody component

    //Player collision detection
    void OnCollisionEnter(Collision col)
    {
        switch(col.transform.tag)
        {
            case "Obstacle":
			Debug.Log ("Obstacle collision");
                //Change crashed state flag status
                _hasCrashed = true;
                //Change game manager state
                _gm.State = GameManager.GameState.SUMMARY;
                //Stop player
                _rigidbody.velocity = Vector3.zero;
                break;

            case "Shield":
                _powerUp = PowerUp.SHIELD;
                _powerUpTimer = shieldTime;
                break;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        switch (col.transform.tag)
        {
            case "Gate_Energy":
                //Increment current scroe multiplyer
                _gm.ScoreManager.Energy += energyIncrease;
                break;

            case "Gate_Multiplier":
                //Increment current scroe multiplyer
                _gm.ScoreManager.Multiplier += 1;
               break;

            case "Gate_Slow":
                //Check if current speed is bigger than default speed amount
                if(_currentSpeed > speed)
                {
                    //Decrease current speed amount
                    _currentSpeed -= _currentAngle;
                    //Decrease camera smooth
                    _gm.TerrainManager.CameraController.DecreaseSmooth(1f);
                }
               break;
        }
    }

    // Use this for initialization
    void Start()
    {
        _gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        //Add rotation contsraints
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        //Get box collider reference
        _rigidbody = GetComponent<Rigidbody>();
		//Get player animation manager
		_anim = GetComponent<AnimationManager>();
        //Player not crashed
        _hasCrashed = false;
        //Player has energy
        _hasEnergy = true;
        //Reset collected power up
        _powerUp = PowerUp.NONE;
        //Set initial speed
        _currentSpeed = speed;
        //Set initial altitude
        _currentAltitude = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		//3 main board ray origin points positions
		Vector3 boardFront = new Vector3(board.transform.position.x, board.transform.position.y, board.transform.position.z + board.transform.localScale.z / 2);
		Vector3 boardCenter = board.transform.position;
		Vector3 boardBack = new Vector3(board.transform.position.x, board.transform.position.y, board.transform.position.z - board.transform.localScale.z / 2);

		//Checking board collision objects
		RaycastHit frontHit = BoardCollision(boardFront, -board.transform.up, 1f, groundCollisions, _frontBoardTag);//Front side
		RaycastHit centerHit = BoardCollision(boardCenter, -board.transform.up, 1f, groundCollisions, _centerBoardTag);//Center
		RaycastHit backHit = BoardCollision(boardBack, -board.transform.up, 1f, groundCollisions, _backBoardTag);//Back side

		//Draw board rays
		Debug.DrawRay(boardFront, -board.transform.up, Color.blue);
		Debug.DrawRay(boardCenter, -board.transform.up, Color.green);
		Debug.DrawRay(boardBack, -board.transform.up, Color.red);

		Debug.Log ("Front tag: " + _frontBoardTag);
		Debug.Log ("Center tag: " + _centerBoardTag);
		Debug.Log ("Back tag: " + _backBoardTag);

        if (!_hasCrashed && _hasEnergy)
        {
            //PowerUp effects
            switch (_powerUp)
            {
                case PowerUp.SHIELD:
                    //Reset shield
                    if (_powerUpTimer <= 0)
                    {
                        _powerUp = PowerUp.NONE;
                    }
                    break;
            }

            //Count powerup timer
            if (_powerUpTimer > 0)
            {
                //Count timer
                _powerUpTimer -= Time.deltaTime;
            }

            //Check player direction
            switch (_dir)
            {
                case MoveDirection.STRAGHT:
                    //Reset roll
                    if(_currentRoll > 0)
                    {
                        _currentRoll = Mathf.Lerp(_currentRoll, 0, _currentRoll / 10f * Time.fixedDeltaTime);
                    }
                    else if (_currentRoll < 0)
                    {
                        _currentRoll = Mathf.Lerp(_currentRoll, 0, -_currentRoll / 10f * Time.fixedDeltaTime);
                    }

                    //Reset turn
                    if (_currentTurn > 0)
                    {
                        _currentTurn = Mathf.Lerp(_currentTurn, 0, _currentTurn / 10f * Time.fixedDeltaTime);
                    }
                    else if (_currentTurn < 0)
                    {
                        _currentTurn = Mathf.Lerp(_currentTurn, 0, -_currentTurn / 10f * Time.fixedDeltaTime);
                    }

                    //Assign reset roll rotation to player prefab
                    transform.localEulerAngles = new Vector3(0, _currentTurn, _currentRoll);
                    break;

                case MoveDirection.RIGHT:
                    //Turn player
                    Turn();
                    //Roll player
                    Roll();
                    break;

                case MoveDirection.LEFT:
                    //Turn player
                    Turn();
                    //Roll player
                    Roll();
                    break;
            }

			//Adjust snowboard rider mesh y axis according to the board collided mesh normal direction
			Quaternion adjustRot = Quaternion .FromToRotation (transform.up , frontHit.normal) * transform.rotation;
			transform.rotation = Quaternion.Slerp (transform.rotation , adjustRot , 1f);
			
			//transform.rotation = Quaternion.Slerp(transform.up, centerBoardHit.normal, 0.1f);
			Vector3 currentAngle = transform.eulerAngles;
			
			//float groundDot = Vector3.Dot (-board.up, centerBoardHit.normal);
			
			//Calculating slope angle value
			float mountainSlope = (backHit.point.y - frontHit.point.y) / GetComponent<BoxCollider>().size.z ;
			
			float mountainAngle = Mathf.Atan(mountainSlope) * Mathf.Rad2Deg + _terrainAngle;
			
			_currentSpeed = Mathf.Lerp (_currentSpeed, mountainAngle / groundFriction, angleSmooth);

			Debug.Log ("Mountain angle: " + mountainAngle);
			Debug.Log ("Current speed: " + _currentSpeed);
			
			Vector3 SBForce = transform.forward * _currentSpeed * speed * Time.fixedDeltaTime;
			//Debug.Log("Force: " + _SBForce);
			_rigidbody.MovePosition ( transform.position + SBForce);
			//Drawing player forward direction ray
			Debug.DrawRay (transform.position, transform.forward , Color.green );

			//Check if user most board has passed ramp
			if(backHit.transform.tag == "RampTop")
			{
				//Play player score trick animation TBI

				//Increase current score

			} 
        }
    }

    public void ResetController(Vector3 pos)
    {
        //Reset player position
        transform.position = pos;
        //Reset player rotation
        transform.localEulerAngles = Vector3.zero;
        //Reset collected power up
        _powerUp = PowerUp.NONE;
        //Reset crash flag
        _hasCrashed = false;
        //Player has energy
        _hasEnergy = true;
        //Set initial speed
        _currentSpeed = speed;
        //Set initial handling
        _currentHandling = 0;
        //Set initial altitude
        _currentAltitude = 0;
    }

    public void Roll()
    {
        switch (_dir)
        {
            case MoveDirection.LEFT:

                _currentRoll += rollSpeed * 10f * Time.deltaTime;
                break;

            case MoveDirection.RIGHT:

                _currentRoll -= rollSpeed * 10f * Time.deltaTime;
                break;
        }
        //Clamp roll value
        _currentRoll = Mathf.Clamp(_currentRoll, -rollLimit, rollLimit);
        //Assign rotation amount
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _currentRoll);
    }

    public void Turn()
    {
        switch (_dir)
        {
            case MoveDirection.LEFT:

                _currentTurn -= turnSpeed * 10f * Time.deltaTime;
                break;

            case MoveDirection.RIGHT:

                _currentTurn += turnSpeed * 10f * Time.deltaTime;
                break;
        }
        //Clamp roll value
        _currentTurn = Mathf.Clamp(_currentTurn, -turnLimit, turnLimit);
        //Assign rotation amount
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, _currentTurn, transform.localEulerAngles.z);
    }

	private RaycastHit BoardCollision(Vector3 castPos, Vector3 direction, float length, LayerMask layer, string tag)
	{
		//Hit data output
		RaycastHit hit;

		if(Physics.Raycast(castPos, direction, out hit, length, layer))
		{

			tag = hit.transform.tag;
		}
		else
		{
			tag = null;
		}

		return hit;
	}

	//Mountain angle setter method
	public float MountainAngle
	{
		set { _terrainAngle = value; }
	}

    //Player direction get/set method
    public MoveDirection Direction
    {
        get { return _dir; }
        set { _dir = value; }
    }
}
