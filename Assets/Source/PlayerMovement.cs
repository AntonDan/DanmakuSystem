using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public static Vector2 playerPosition;

	public GameObject sword;

	private void Start()
	{
		StartCoroutine(SwordSlash());
	}

	void Update()
	{
		Vector2 direction;
		direction.x = Input.GetAxisRaw("Horizontal");
		direction.y = Input.GetAxisRaw("Vertical");
		if (direction.magnitude < 0.1f) return;
		direction.Normalize();

		transform.Translate(direction * 2.0f * Time.deltaTime, Space.World);
		playerPosition = transform.position;
	}

	IEnumerator SwordSlash()
	{
		Transform swordTransform = sword.transform;
		Transform playerTransform = transform;
		yield return new WaitForSeconds(2.0f);

		while (true)
		{
			Material mat = sword.GetComponent<Renderer>().material;
			float swingDuration = 0.15f;
			for (float time = 0; time < swingDuration; time += Time.deltaTime)
			{
				float progression = time / swingDuration;
				mat.SetTextureOffset("_MainTex", new Vector2(Mathf.Lerp(1, 0.2f, progression), 0f));
				yield return null;
			}

			float fadeDuration = 0.4f;
			for (float time = 0; time < fadeDuration; time += Time.deltaTime)
			{
				float progression = time / fadeDuration;
				mat.SetTextureOffset("_MainTex", new Vector2(Mathf.Lerp(0.2f, -1, progression), 0f));
				yield return null;
			}
			yield return new WaitForSeconds(0.25f);
		}
		// for ()
		// swordTransform.position = 
	}
}
