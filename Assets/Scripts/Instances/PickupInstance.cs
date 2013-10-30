﻿using UnityEngine;
using System.Collections;

public class PickupInstance : MonoBehaviour
{
	// Fields
	int terrainMask;

	PickupInfo.Type initType;
	bool initFalling;
	Vector3 initVelocity;

	// Enums

	// Properties
	public PickupInfo.Type Type = PickupInfo.Type.None;
	public bool Falling = true;
	public Vector3 Velocity;

	// Unity Methods
	public void Awake()
	{
		// Store pickup-related init state
		initType = Type;
		initFalling = Falling;
		initVelocity = Velocity;
	}

	public void OnEnable()
	{
		// Restore pickup-related init state
		Type = initType;
		Falling = initFalling;
		Velocity = initVelocity;
	}

	public void Start()
	{
		// Store the terrain layer mask
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
	}

	void Update()
	{
		// Rotate at a fixed rate
		transform.rotation *= Quaternion.AngleAxis( PickupInfo.Properties.RotatePerSecond * Time.deltaTime, Vector3.up );

		if( Falling )
		{
			// If still in midair, increment velocity and check for collision with the terrain
			Velocity += -Vector3.up * PickupInfo.Properties.Gravity * Time.deltaTime;

			if( Physics.CheckSphere( transform.position, PickupInfo.Properties.TerrainCollisionRadius, terrainMask ) )
			{
				Falling = false;
				Velocity = Vector3.zero;
			}
		}
		else
		{
			// Otherwise, check if embedded in the terrain and push out if so
			while( Physics.CheckSphere( transform.position, PickupInfo.Properties.TerrainCollisionRadius, terrainMask ) )
			{
				transform.position += new Vector3( 0f, PickupInfo.Properties.SmallValue, 0f );
			}
		}

		// Move down relative to velocity
		transform.position += Velocity * Time.deltaTime;
	}

	void OnDrawGizmos()
	{
		// Draw the debug pickup radius
		if( GameInfo.Properties.Debug )
		{
			Gizmos.DrawWireSphere( transform.position, PickupInfo.Properties.TerrainCollisionRadius );
		}
	}

	void OnTriggerEnter( Collider col )
	{
		// check if a player has grabbed this pickup
		if( col.tag == "Player" )
		{
			// Collider is a Marble, get player as parent of parent
			GameObject player = col.transform.parent.parent.gameObject;
			PlayerInstance playerScript = player.transform.Find( "Avatar" ).GetComponent<PlayerInstance>();

			// If the player has an empty drone slot

			if( playerScript.Drones.Count < 3 )
			{

				// Spawn a drone
				GameObject drone = GameControl.DronePool.Spawn( "Drone" );
				DroneInfo.Type droneType = new DroneInfo.Type();

				// Set type
				switch( Type )
				{
					case PickupInfo.Type.Rocket:
						droneType = DroneInfo.Type.Rocket;
						break;
					case PickupInfo.Type.Mortar:
						droneType = DroneInfo.Type.Mortar;
						break;
					case PickupInfo.Type.Seeker:
						droneType = DroneInfo.Type.Seeker;
						break;
					default:
						break;
				}

				// Position and rotate the drone to match this pickup
				drone.transform.position = transform.position;
				drone.transform.rotation = transform.rotation;

				// Setup the drone's scripts
				Drone droneScript = drone.GetComponent<Drone>();
				droneScript.Player = player;
				droneScript.Type = droneType;
				droneScript.PooledStart();

				KinematicHover droneHover = drone.GetComponent<KinematicHover>();
				droneHover.Target = playerScript.DroneAnchors[ playerScript.Drones.Count ];
				playerScript.Drones.Add( drone );

				// Assign type-specific ammo
				switch( Type )
				{
					case PickupInfo.Type.Rocket:
						droneScript.Ammo = 3;
						break;
					case PickupInfo.Type.Mortar:
						droneScript.Ammo = 2;
						break;
					case PickupInfo.Type.Seeker:
						droneScript.Ammo = 1;
						break;
					default:
						break;
				}

				GameControl.PickupPool.Despawn( gameObject );
			}
		}
	}
}
