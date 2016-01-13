﻿using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
    public bool inverse;//Control inverse flag
    private PlayerController _player; //Player controller script
    private SmoothFollow _camera; //Main camera controller script

    void Start()
    {
        //Get player controller
        _player = GetComponent<PlayerController>();
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SmoothFollow>();
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
                //Rotate camera
                _camera.Rotate(_player.Direction);
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
                //Rotate camera
                _camera.Rotate(_player.Direction);
            }
        }
        else
        //Reset player direction and camera position
        {
            //Move straight
            _player.Direction = PlayerController.MoveDirection.STRAGHT;
            //Rotate camera
            _camera.Rotate(_player.Direction);
        }
    }
}
