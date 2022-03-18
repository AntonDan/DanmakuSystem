using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class EnemyHealth : MonoBehaviour, IDamageable
{
	[SerializeField]
	private float _health;

	[SerializeField]
	private float _maxHealth;

	Color originalColor;

	private void Start()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		originalColor = renderer.color;
	}

	public void Damage(float amount)
	{
		if (amount <= 0) return;

		_health = Mathf.Max(_health - amount, 0);
		if (_health == 0)
		{
			Destroy(gameObject);
			return;
		}
		Timing.RunCoroutineSingleton(FlashRoutine().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
	}

	IEnumerator<float> FlashRoutine()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();

		renderer.color = Color.white;
		yield return Timing.WaitForSeconds(0.05f);
		renderer.color = originalColor;
	}
}
