﻿using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class InputWrapper : MonoBehaviour
{
	// Fields
	GamePadState xboxPadState;
	PlayerIndex xboxPadIndex;
	InputType type = InputType.None;
	string typeSuffix;

	// Enums
	public enum InputType
	{
		None,
		UnityPad,
		XboxPad
	}
	
	// Properties
	public int LocalPlayerIndex;
	public Vector2 LeftStick;
	public Vector2 RightStick;
	public float Jump;
	public float Drop;
	public float Aim;
	public float Fire;
	public float SwitchLeft;
	public float SwitchRight;

	public float Dash;
	public Vector2 DashVector;

	// Startup function to be called manually by instantiator
	public void Init()
	{
		// Get the joystick's devicename strings
		string[] joystickNames = Input.GetJoystickNames();

		// If there are enough joysticks
		if( joystickNames.Length > LocalPlayerIndex )
		{
			// Determine if we should use XInput or Unity Input
			switch( Input.GetJoystickNames()[ LocalPlayerIndex ] )
			{
				case "Controller (XBOX 360 For Windows)":
					// Set the appropriate type and instantiate the XInput pad index
					type = InputType.XboxPad;
					xboxPadIndex = (PlayerIndex)LocalPlayerIndex;
					break;
				default:
					// Set the appropriate type and create an axis name suffix for this joystick index
					type = InputType.UnityPad;
					typeSuffix = " (Joy " + ( LocalPlayerIndex + 1 ) + ")";
					break;
			}
		}
		else if( joystickNames.Length == LocalPlayerIndex )
		{
			// If there are no joysticks left, use the keyboard
			type = InputType.UnityPad;
			typeSuffix = " (Keyboard)";
		}
		else
		{
			// If there are no joysticks left and the keyboard is taken, log a warning message
			Debug.LogWarning( "InputWrapper #" + LocalPlayerIndex + " unable to bind joystick" );
		}
	}

	// Unity Methods
	void Update()
	{
		// Set values from Unity/XInput based on the appropriate axis + suffix
		switch( type )
		{
			case InputType.UnityPad:
				LeftStick.x = Input.GetAxisRaw( "Move Horizontal" + typeSuffix );
				LeftStick.y = Input.GetAxisRaw( "Move Vertical" + typeSuffix );

				if( typeSuffix != " (Keyboard)" )
				{
					RightStick.x = Input.GetAxisRaw( "Camera Horizontal" + typeSuffix );
					RightStick.y = Input.GetAxisRaw( "Camera Vertical" + typeSuffix );
				}
				else
				{
					RightStick.x = Input.GetAxisRaw( "Camera Horizontal" + typeSuffix );
					RightStick.y = Input.GetAxisRaw( "Camera Vertical" + typeSuffix );
				}

				Jump = Input.GetAxis( "Jump" + typeSuffix );
				Drop = Input.GetAxis( "Drop" + typeSuffix );
				Aim = Input.GetAxis( "Aim" + typeSuffix );
				Fire = Input.GetAxis( "Fire" + typeSuffix );
				SwitchLeft = Input.GetAxis( "Switch Left" + typeSuffix );
				SwitchRight = Input.GetAxis( "Switch Right" + typeSuffix );
				break;
			case InputType.XboxPad:
				xboxPadState = GamePad.GetState( xboxPadIndex );

				LeftStick.x = xboxPadState.ThumbSticks.Left.X;
				LeftStick.y = xboxPadState.ThumbSticks.Left.Y;

				RightStick.x = xboxPadState.ThumbSticks.Right.X;
				RightStick.y = xboxPadState.ThumbSticks.Right.Y;

				Jump = xboxPadState.Buttons.LeftShoulder == ButtonState.Pressed ? 1f : 0f;
				Drop = ( xboxPadState.Buttons.RightShoulder == ButtonState.Pressed ? 1f : 0f );
				Aim = ( xboxPadState.Triggers.Left >= .25f ? 1f : 0f );
				Fire = ( xboxPadState.Triggers.Right >= .25f ? 1f : 0f );
				SwitchLeft = ( xboxPadState.Buttons.X == ButtonState.Pressed ? 1f : 0f );
				SwitchRight = ( xboxPadState.Buttons.B == ButtonState.Pressed ? 1f : 0f );
				break;
		}

		// Dash detection
		if( typeSuffix != " (Keyboard)" )
		{
			CheckJoyDashInput();
		}
		else
		{
			CheckKeyDashInput();
		}
	}

	/* Dash states:
	 * 0 - Stick outside outer zone
	 * 1 - Stick within inner zone
	 * 2 - Stick passed out of inner zone and into outer zone
	 * 3 - Stick passed back into inner zone
	 */
	int dashInputState = 1;
	float innerZoneBoundary = 0.25f;
	float outerZoneBoundary = 0.75f;
	float dashTimer = 0f;
	float dashTimeout = .2f;

	void CheckJoyDashInput()
	{
		if( dashTimer > 0f )
		{
			dashTimer -= Time.deltaTime;
		}

		if( dashTimer <= 0f )
		{
			if( dashInputState != 0 )
			{
				dashInputState = 0;
			}
		}

		if( dashInputState == 4 )
		{
			Dash = 0f;
			dashInputState = 0;
			dashTimer = 0f;
		}

		if( dashInputState == 0 && LeftStick.magnitude < innerZoneBoundary )
		{
			dashInputState = 1;
			dashTimer = dashTimeout;
		}
		if( dashInputState == 1 && LeftStick.magnitude > outerZoneBoundary )
		{
			dashInputState = 2;
			dashTimer = dashTimeout;
			DashVector = LeftStick.normalized;
		}
		if( dashInputState == 2 && LeftStick.magnitude < innerZoneBoundary )
		{
			dashInputState = 3;
			dashTimer = dashTimeout;
		}
		if( dashInputState == 3 )
		{
			Dash = 1f;
			dashInputState = 4;
			dashTimer = dashTimeout;
		}
	}

	float prevDash = 0f;
	void CheckKeyDashInput()
	{
		if( Dash == 1f )
		{
			Dash = 0f;
		}

		if( prevDash == 0f && Input.GetKey( KeyCode.LeftShift ) )
		{
			DashVector = LeftStick.normalized;
			Dash = 1f;
		}
		prevDash = Input.GetAxis( "Dash" + typeSuffix );
	}
}
