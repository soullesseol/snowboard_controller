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

    public GameObject board; //Player rider board object
    public LayerMask groundCollisions;//Layer of all ground collisions which should adjust rider rotation
    public float speed;//Rider default speed
    public float handling;//Craft default handling
    [Range(0,1f)]
    public float handlingSmooth;
    public float rollSpeed;//Rider rotation modifiers (roll & turn)
    public float rollLimit;
    public float turnSpeed;
    public float turnLimit;
	public float groundFriction;//Ground friction amount
	[Range(0,1f)]
	public float angleSmooth;//Player speed change based on angle smoothing

    float _currentSpeed;//In-game player speed value
    float _currentHandling;//In-game player handling value
    float _currentAltitude;//In-game player altitude value
    float _currentRoll;//Rider current roll value
    float _currentTurn;//Rider current turn value
	float _currentAngle;//Current mountain angle
	string _centerBoardTag;//Board center collided object tag
	string _frontBoardTag;//Board front side collided object tag
	string _backBoardTag;//Board end side collided object tag
	float _terrainAngle;//Current mountain angle

    MoveDirection _dir; //Player movement direction (Required for input manager)
    Rigidbody _rigidbody;//Player rigidbody component

    // Use this for initialization
    void Start()
    {
        //Add rotation contsraints
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        //Get box collider reference
        _rigidbody = GetComponent<Rigidbody>();
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
                _currentTurn += turnSpeed * 10f * Time.deltaTime;
                //Clamp roll value
                _currentTurn = Mathf.Clamp(_currentTurn, -turnLimit, turnLimit);
                //Assign rotation amount
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, _currentTurn, transform.localEulerAngles.z);

                //Roll player
                _currentRoll += rollSpeed * 10f * Time.deltaTime;
                //Clamp roll value
                _currentRoll = Mathf.Clamp(_currentRoll, -rollLimit, rollLimit);
                //Assign rotation amount
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _currentRoll);
                break;

            case MoveDirection.LEFT:
                //Turn player
                _currentTurn -= turnSpeed * 10f * Time.deltaTime;
                //Clamp roll value
                _currentTurn = Mathf.Clamp(_currentTurn, -turnLimit, turnLimit);
                //Assign rotation amount
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, _currentTurn, transform.localEulerAngles.z);

                //Roll player
                _currentRoll -= rollSpeed * 10f * Time.deltaTime;
                //Clamp roll value
                _currentRoll = Mathf.Clamp(_currentRoll, -rollLimit, rollLimit);
                //Assign rotation amount
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _currentRoll);
                break;
        }

        //Adjust snowboard rider mesh y axis according to the board collided mesh normal direction
        Quaternion adjustRot = Quaternion .FromToRotation (transform.up , frontHit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp (transform.rotation , adjustRot , 1f);
            
        Vector3 currentAngle = transform.eulerAngles;
                        
        //Calculating slope angle value
        float mountainSlope = (backHit.point.y - frontHit.point.y) / GetComponent<BoxCollider>().size.z ;
            
        float mountainAngle = Mathf.Atan(mountainSlope) * Mathf.Rad2Deg + _terrainAngle;
            
        _currentSpeed = Mathf.Lerp (_currentSpeed, mountainAngle / groundFriction, angleSmooth);
            
        Vector3 SBForce = transform.forward * _currentSpeed * speed * Time.fixedDeltaTime;
        //Debug.Log("Force: " + _SBForce);
        _rigidbody.MovePosition ( transform.position + SBForce);

        /*
        *DEBUG SECTION. REMOVE IN PRODUCTION
        **/

        //Output debug parameters
        Debug.Log ("Mountain angle: " + mountainAngle);
        Debug.Log ("Current speed: " + _currentSpeed);
        Debug.Log ("Front tag: " + _frontBoardTag);
        Debug.Log ("Center tag: " + _centerBoardTag);
        Debug.Log ("Back tag: " + _backBoardTag);

        //Draw board rays
        Debug.DrawRay(boardFront, -board.transform.up, Color.blue);
        Debug.DrawRay(boardCenter, -board.transform.up, Color.green);
        Debug.DrawRay(boardBack, -board.transform.up, Color.red);

		//Drawing player forward direction ray
		Debug.DrawRay (transform.position, transform.forward , Color.green );
    }

	RaycastHit BoardCollision(Vector3 castPos, Vector3 direction, float length, LayerMask layer, string tag)
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

    //Player direction get/set method
    public MoveDirection Direction
    {
        get { return _dir; }
        set { _dir = value; }
    }
}
