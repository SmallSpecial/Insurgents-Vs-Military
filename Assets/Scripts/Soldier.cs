﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Author: Israel Anthony
 * Purpose: Calculate steering forces for soldiers.
 * Caveats: None
 */ 
public class Soldier : MovementForces 
{
	private GameObject leader;

	// Use this for initialization
	void Start () 
	{
		// Error checking
		GameObject sceneMngr = GameObject.Find("SceneManager");
		if(null == sceneMngr)
		{
			Debug.Log("Error in " + gameObject.name + 
				": Requires a SceneManager object in the scene.");
			Debug.Break();
		}

		behaviourMngr = sceneMngr.GetComponent<BehaviourManager>();

		mass = 1.0f;
		maxSpeed = 15.0f;
		maxForce = 25.0f;
		safeZone = new Vector3(206.0f, 0.0f, 486.0f);

		position = gameObject.transform.position;
	}


	// Update is called once per frame
	void Update () 
	{
		UpdatePosition ();
		ReturnCenter ();
		SetTransform ();
	}


	public override void SetTarget()
	{
		if (targetList.Count > 0) 
		{
			float d = Vector3.Magnitude (position - targetList [0].transform.position);
			int closestIndex = 0;

			// Find the closest target and set that as the target
			for (int i = 0; i < targetList.Count; i++) 
			{
				if (Vector3.Magnitude (position - targetList [i].transform.position) < d) 
				{
					closestIndex = i;
					d = Vector3.Magnitude (position - targetList [i].transform.position);
				}
			}

			if (d < 25.0f) 
			{
				target = targetList [closestIndex];
			} 
			else 
			{
				target = null;
			}
		}
		else 
		{
			target = null;
		}
	}


	void UpdatePosition()
	{
		if (target == null) 
		{
			//Step 0: update position to current tranform
			position = gameObject.transform.position;

			Vector3 followForce = Seek (leader.transform.position - leader.GetComponent<MovementForces> ().direction * 3.0f);
			ApplyForce (followForce);

			Arrive (leader.transform.position);

			AvoidObstacle();

			//Step 1: Add Acceleration to Velocity * Time
			velocity += acceleration * Time.deltaTime;
			//Step 2: Add vel to position * Time
			position += velocity * Time.deltaTime;
			//Step 3: Reset Acceleration vector
			acceleration = Vector3.zero;
			//Step 4: Calculate direction (to know where we are facing)
			direction = velocity.normalized;
		} 
		else 
		{
			//Step 0: update position to current tranform
			position = gameObject.transform.position;

			//Step 0.5: Evade the target
			Vector3 pursuingForce = Pursue(target.transform.position);
			ApplyForce (pursuingForce);

			AvoidObstacle();

			//Step 1: Add Acceleration to Velocity * Time
			velocity += acceleration * Time.deltaTime;
			//Step 2: Add vel to position * Time
			position += velocity * Time.deltaTime;
			//Step 3: Reset Acceleration vector
			acceleration = Vector3.zero;
			//Step 4: Calculate direction (to know where we are facing)
			direction = velocity.normalized;
		}
	}


	/* Soldier specific methods */

	/// <summary>
	/// Sets the leader.
	/// </summary>
	/// <param name="general">General.</param>
	public void SetLeader(GameObject general)
	{
		leader = general;
	}


	/// <summary>
	/// Arrive the specified target.
	/// </summary>
	/// <param name="target">Target.</param>
	void Arrive(Vector3 target)
	{
		Vector3 desiredVelocity = target - position;

		float d = desiredVelocity.magnitude;
		desiredVelocity.Normalize ();

		if (d < 100.0f) 
		{
			float magnitude = Map(d, 0.0f, 100.0f, 0.0f, maxSpeed);
			desiredVelocity *= magnitude;
		} 
		else 
		{
			desiredVelocity *= maxSpeed;
		}

		Vector3 steeringForce = desiredVelocity - velocity;
		steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
		ApplyForce(steeringForce);
	}


	/// <summary>
	/// Map the specified oldValue from an old range to a newValue in a new range.
	/// </summary>
	/// <param name="oldValue">Old value.</param>
	/// <param name="oldMin">Old minimum.</param>
	/// <param name="oldMax">Old maximum.</param>
	/// <param name="newMin">New minimum.</param>
	/// <param name="newMax">New maximum.</param>
	float Map(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
	{
		return newMin + (newMax - newMin) * ((oldValue - oldMin) / (oldMax - oldMin));
	}


	/// <summary>
	/// Raises the render object event. Draws debugging lines.
	/// </summary>
	void OnRenderObject()
	{
		if (debugging) 
		{
			// Forward vector
			GL.PushMatrix ();
			behaviourMngr.matBlue.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + gameObject.transform.forward * 3);
			GL.End ();

			// Right vector
			behaviourMngr.matGreen.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + gameObject.transform.right * 3);
			GL.End ();

			// Future Position vector
			behaviourMngr.matGreen.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + velocity);
			GL.End ();

			// Black line to target
			if (null != targetList) 
			{
				foreach (GameObject v in targetList) 
				{
					if (v == target) 
					{
						behaviourMngr.matBlack.SetPass (0);
						GL.Begin (GL.LINES);
						GL.Vertex (position);
						GL.Vertex (v.transform.position);
						GL.End ();
					}
				}
			}
			GL.PopMatrix ();
		}
	}
}
