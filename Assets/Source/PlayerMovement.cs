using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public static Vector2 playerPosition;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		Vector2 direction;
		direction.x = Input.GetAxisRaw("Horizontal");
		direction.y = Input.GetAxisRaw("Vertical");
		if (direction.magnitude < 0.1f) return;
		direction.Normalize();

		transform.Translate(direction * 2.0f * Time.deltaTime, Space.World);
		// playerPosition = transform.position;
	}
}
