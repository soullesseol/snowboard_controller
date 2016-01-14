using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
    public bool inverse;//Control inverse flag
    private PlayerController _player; //Player controller script
    
    void Start()
    {
        //Get player controller
        _player = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.position.x < Screen.width / 2)
            {
                if (!inverse)
                {
                    //Move left
                    _player.Direction = PlayerController.MoveDirection.LEFT;
                }
                else
                {
                    //Move right
                    _player.Direction = PlayerController.MoveDirection.RIGHT;
                }
            }
            else if (touch.position.x > Screen.width / 2)
            {
                if (!inverse)
                {
                    //Move right
                    _player.Direction = PlayerController.MoveDirection.RIGHT;
                }
                else
                {
                    //Move left
                    _player.Direction = PlayerController.MoveDirection.LEFT;
                }
            }
        }
        else
        //Reset player direction and camera position
        {
            //Move straight
            _player.Direction = PlayerController.MoveDirection.STRAGHT;
        }
    }
}
