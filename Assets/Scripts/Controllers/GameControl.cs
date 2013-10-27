﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameControl : MonoBehaviour
{
	// Fields

	// Properties
	public ObjectPool ObjectPool;
	public GameObject PlayerPrefab;
	public GameObject PickupPrefab;
	public GameObject RocketDronePrefab;
	public GameObject MortarDronePrefab;
	public GameObject SeekerDronePrefab;
	public int LocalPlayerCount = 1;
	public List<GameObject> Players;

	// Unity Methods
	void Awake()
	{
		// Persist this object between scenes
		GameObject.DontDestroyOnLoad( this );

		// Instantiate player list
		Players = new List<GameObject>();

		// Make sure the local player count is valid
		LocalPlayerCount = Mathf.Clamp( LocalPlayerCount, 1, 4 );
	}

	void Start()
	{
		// Spawn players
		List<GameObject> spawnPoints = GameObject.FindGameObjectsWithTag( "PlayerSpawn" ).ToList<GameObject>();
		for( int i = 0; i < LocalPlayerCount; ++i )
		{
			int random = Random.Range( 0, spawnPoints.Count );
			SpawnLocalPlayer( spawnPoints[ random ].transform, i );
			spawnPoints.RemoveAt( random );
		}

		// Spawn pickups
		Terrain terrain = GameObject.Find( "Terrain" ).GetComponent<Terrain>();
		Vector3 bounds = terrain.terrainData.size;

		// Rocket Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, bounds.x ) - bounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, bounds.z ) - bounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Rocket, randomPosition, randomRotation );
		}

		// Mortar Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, bounds.x ) - bounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, bounds.z ) - bounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Mortar, randomPosition, randomRotation );
		}

		// Seeker Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, bounds.x ) - bounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, bounds.z ) - bounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Seeker, randomPosition, randomRotation );
		}
	}

	void Update()
	{
		// Grab the cursor when the window is clicked
		if( !Screen.lockCursor && Input.GetMouseButtonDown( 0 ) )
		{
			Screen.lockCursor = true;
		}

		// Un-grab it when escape is pressed
		if( Screen.lockCursor && Input.GetKeyDown( "escape" ) )
		{
			Screen.lockCursor = false;
		}
	}

	// Utility Methods
	void SpawnLocalPlayer( Transform spawnPoint, int idx )
	{
		// Instantiate a new player object
		GameObject player = (GameObject)Instantiate( PlayerPrefab, spawnPoint.position, spawnPoint.rotation );

		// Setup necessary script properties
		InputWrapper inputScript = player.GetComponent<InputWrapper>();
		inputScript.LocalPlayerIndex = Players.Count;
		inputScript.Init();

		Transform camera = player.transform.Find( "Camera" );
		camera.camera.rect = CalculateViewport( idx );
		Camera cameraComponent = camera.GetComponent<Camera>();
		int cameraMask = 0;
		for( int i = 0; i < 4; ++i )
		{
			if( i == idx )
				continue;

			int layerMask = 1 << LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
			cameraMask |= layerMask;
		}
		cameraComponent.cullingMask = ~cameraMask;

		PlayerOverlay playerOverlay = player.transform.Find( "Overlay" ).GetComponent<PlayerOverlay>();
		playerOverlay.GameControl = this;
		playerOverlay.ObjectPool = ObjectPool;

		// Add to the player list
		Players.Add( player );
	}

	void SpawnPickup( PickupInfo.Type type, Vector3 position, Quaternion rotation )
	{
		// Get a blank pickup from the object pool, spawn the appropriate graphic and attach
		GameObject pickup = ObjectPool.Spawn( PickupPrefab );
		GameObject pickupMesh = null;
		switch( type )
		{
			case PickupInfo.Type.Rocket:
				pickupMesh = ObjectPool.Spawn( RocketDronePrefab );
				break;
			case PickupInfo.Type.Mortar:
				pickupMesh = ObjectPool.Spawn( MortarDronePrefab );
				break;
			case PickupInfo.Type.Seeker:
				pickupMesh = ObjectPool.Spawn( SeekerDronePrefab );
				break;
			default:
				break;
		}
		pickupMesh.transform.parent = pickup.transform;

		// Spawn and configure the pickup glow billboard
		GameObject pickupGlow = pickup.transform.Find( "Overlay" ).gameObject;
		Billboard pickupBillboard = pickupGlow.GetComponent<Billboard>();
		pickupBillboard.GameControl = this;
		pickupBillboard.ObjectPool = ObjectPool;

		// Setup transform
		pickup.transform.position = position;
		pickup.transform.rotation = rotation;

		// Set script properties
		PickupInstance pickupScript = pickup.GetComponent<PickupInstance>();
		pickupScript.ObjectPool = ObjectPool;
		pickupScript.Type = type;

		// Start the script
		pickupScript.Start();
	}

	// Setup camera rect based on player count and index
	Rect CalculateViewport( int idx )
	{
		Rect cameraRect = new Rect( 0f, 0f, 1f, 1f );

		switch( LocalPlayerCount )
		{
			case 1:
				cameraRect = new Rect( 0f, 0f, 1f, 1f );
				break;
			case 2:
				switch( idx )
				{
					case 0:
						cameraRect = new Rect( 0f, .5f, 1f, .5f );
						break;
					case 1:
						cameraRect = new Rect( 0f, 0f, 1f, .5f );
						break;
					default:
						break;
				}
				break;
			case 3:
				switch( idx )
				{
					case 0:
						cameraRect = new Rect( 0f, .5f, 1f, .5f );
						break;
					case 1:
						cameraRect = new Rect( 0f, 0f, .5f, .5f );
						break;
					case 2:
						cameraRect = new Rect( .5f, 0f, .5f, .5f );
						break;
					default:
						break;
				}
				break;
			case 4:
				switch( idx )
				{
					case 0:
						cameraRect = new Rect( 0f, .5f, .5f, .5f );
						break;
					case 1:
						cameraRect = new Rect( .5f, .5f, .5f, .5f );
						break;
					case 2:
						cameraRect = new Rect( 0f, 0f, .5f, .5f );
						break;
					case 3:
						cameraRect = new Rect( .5f, 0f, .5f, .5f );
						break;
					default:
						break;
				}
				break;
		}

		return cameraRect;
	}
}
