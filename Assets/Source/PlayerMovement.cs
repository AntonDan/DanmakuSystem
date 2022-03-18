using System.Collections;
using UnityEngine;
using MEC;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
	public static Vector2 playerPosition;

	public GameObject sword;

	private Transform _swordTransform;
	private Transform _playerTransform;
	private Material _swordMaterial;
	private CoroutineHandle _handle;

	private void Start()
	{
		playerPosition = transform.position;
		_swordTransform = sword.transform;
		_playerTransform = transform;
		_swordMaterial = sword.GetComponent<Renderer>().material;
	}

	void Update()
	{
		Vector2 direction;
		direction.x = Input.GetAxisRaw("Horizontal");
		direction.y = Input.GetAxisRaw("Vertical");
		if (direction.magnitude > 0.0f)
		{
			direction.Normalize();
			transform.Translate(direction * 2.5f * Time.deltaTime, Space.World);
			playerPosition = transform.position;
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			_handle = Timing.RunCoroutineSingleton(SwordSlash(), _handle, SingletonBehavior.Abort);
		}
	}

	IEnumerator<float> SwordSlash()
	{
		float swingArcAngle = 180.0f;
		float swingRadius = 0.5f;
		float swingDuration = 0.1f;
		float fadeDuration = 0.3f;

		RaycastHit2D Hit = Physics2D.CircleCast(_swordTransform.position, swingRadius, _playerTransform.forward);
		if (Hit)
		{
			IDamageable target = Hit.transform.GetComponent<IDamageable>();
			if (target != null)
			{
				Vector2 hitDirection = Hit.point - (Vector2)_swordTransform.position;
				float angle = Vector2.Angle(_playerTransform.up, hitDirection);
				if (angle < swingArcAngle / 2.0f)
				{
					Debug.DrawLine(_swordTransform.position, Hit.point, Color.red, 0.1f);
					target.Damage(0.5f);
				}
			}
		}

		for (float time = 0; time < swingDuration; time += Time.deltaTime)
		{
			float progression = time / swingDuration;
			_swordMaterial.SetTextureOffset("_MainTex", new Vector2(Mathf.Lerp(1, 0.2f, progression), 0f));
			yield return Timing.WaitForOneFrame;
		}

		for (float time = 0; time < fadeDuration; time += Time.deltaTime)
		{
			float progression = time / fadeDuration;
			_swordMaterial.SetTextureOffset("_MainTex", new Vector2(Mathf.Lerp(0.2f, -1, progression), 0f));
			yield return Timing.WaitForOneFrame;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(sword.transform.position, 0.5f);
	}
}
