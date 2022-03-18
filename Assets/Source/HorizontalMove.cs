using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;
using Projectiles;
using Unity.Entities;

class DiveAndEncirclePlayer : EnemyBehaviorStateBase
{
	private TrailRenderer _trailRenderer;
	private Entity _projectile;

	public DiveAndEncirclePlayer(GameObject gameObject, GameObject playerObject, Entity projectile) : base(gameObject, playerObject)
	{
		_projectile = projectile;
		_trailRenderer = base.gameObject.GetComponent<TrailRenderer>();
	}

	public override EnemyBehaviorStateBase GetNextState()
	{
		return new GappedCircleBurst(gameObject, playerObject, _projectile);
	}

	public override void OnDestroy()
	{
	}

	public override IEnumerator<float> Run()
	{
		float diveDuration = 0.5f;
		Color originalColor = renderer.color;
		Color newColor = originalColor;
		newColor.a = 0.5f;

		Vector3 originalScale = transform.localScale;
		Vector3 newScale = transform.localScale * 0.75f;

		yield return Timing.WaitUntilDone(ScaleTo(originalScale, newScale, diveDuration).Superimpose(ColorTo(originalColor, newColor, diveDuration)));
		_trailRenderer.enabled = true;


		float encirclementDistance = 3.0f;
		Vector2 direction = Vector2.zero;
		direction.x = Mathf.Cos(0);
		direction.y = Mathf.Sin(0);
		Vector2 currentPosition = transform.position;

		Func<float, bool> MoveAction = (prog) => { return ActionMoveTo(currentPosition, playerTransform, direction * encirclementDistance, prog); };
		yield return Timing.WaitUntilDone(RunFor(1.0f, MoveAction));


		float encircleDuration = 3.0f;
		for (float time = 0.0f; time < encircleDuration; time += Time.deltaTime)
		{
			float progression = time / encircleDuration;
			float angle = -2 * Mathf.PI * progression;
			direction.x = Mathf.Cos(angle);
			direction.y = Mathf.Sin(angle);

			transform.position = (Vector2)playerTransform.position + direction * encirclementDistance;
			yield return Timing.WaitForOneFrame;
		}

		_trailRenderer.enabled = false;
		yield return Timing.WaitUntilDone(ScaleTo(transform.localScale, originalScale, diveDuration).Superimpose(ColorTo(renderer.color, originalColor, diveDuration)));

		yield return Timing.WaitForSeconds(1.0f);
	}

	private bool ActionMoveTo(Vector2 from, Transform to, Vector2 offset, float progression)
	{
		if (transform == null) return false;
		transform.position = Vector2.Lerp(from, (Vector2)to.position + offset, progression);
		return true;
	}
}

class GappedCircleBurst : EnemyBehaviorStateBase
{
	private Entity _projectile;
	private CircularProjectileEmitter _emitter;
	private ArcProjectileEmitter _subemitter;
	private Entity _e1;

	public GappedCircleBurst(GameObject gameObject, GameObject playerObject, Entity projectile) : base(gameObject, playerObject)
	{
		_projectile = projectile;
	}

	public override EnemyBehaviorStateBase GetNextState()
	{
		return new MirrorredSprayShot(gameObject, playerObject, _projectile);
	}

	public override void OnDestroy()
	{
		if (_e1 != Entity.Null)
		{
			entityManager.DestroyEntity(_e1);
		}
	}

	public override IEnumerator<float> Run()
	{
		float circleFiringCyclePeriod = 0.25f;
		int circleBurstAmount = 6;
		int circleProjectileCount = 3;
		float circleRadius = 0.2f;
		float circleRotationSpeed = 25.0f;

		float arcSize = 100.0f;
		int arcProjectileCount = 24;

		float emissionDuration = circleBurstAmount * circleFiringCyclePeriod;
		float rotationAmount = circleRotationSpeed * emissionDuration;

		Vector2 currentPosition = transform.position;
		_emitter = new CircularProjectileEmitter(currentPosition, 0, 0, 0, null, new ProjectileCircleSpawnerConfig(circleRadius));
		_subemitter = new ArcProjectileEmitter(currentPosition, 0, 0, 0, null, new ProjectileArcSpawnerConfig(arcSize, circleRadius));
		_emitter.SetSubemitter(_subemitter.WithFiringParameters(1, 1, arcProjectileCount).Of(_projectile));

		for (int i = 0; i < 5; ++i)
		{
			int direction = i % 2 == 0 ? 1 : -1;
			float currentRotation = i % 2 == 0 ? 0 : rotationAmount;

			_e1 = _emitter
			.WithFiringParameters(circleBurstAmount, circleFiringCyclePeriod, circleProjectileCount)
			.WithTransform(currentPosition, currentRotation)
			.WithMovement(0, direction * circleRotationSpeed).Fire();
			yield return Timing.WaitForSeconds(emissionDuration);
			yield return Timing.WaitForSeconds(circleFiringCyclePeriod); // Padding
		}

		yield return Timing.WaitForSeconds(1.0f);
	}
}

class MirrorredSprayShot : EnemyBehaviorStateBase
{
	private Entity _projectile;
	private ConeProjectileEmitter _emitter;

	private Entity _e1;
	private Entity _e2;

	public MirrorredSprayShot(GameObject gameObject, GameObject playerObject, Entity projectile) : base(gameObject, playerObject)
	{
		_projectile = projectile;
	}

	public override EnemyBehaviorStateBase GetNextState()
	{
		return new DiveAndEncirclePlayer(gameObject, playerObject, _projectile);
	}

	public override void OnDestroy()
	{
		if (_e1 != Entity.Null)
		{
			entityManager.DestroyEntity(_e1);
		}
		if (_e2 != Entity.Null)
		{
			entityManager.DestroyEntity(_e2);
		}
	}

	public override IEnumerator<float> Run()
	{
		float coneFiringCyclePeriod = 0.12f;
		int coneBurstAmount = 50;
		float coneAngle = 30f;

		float emissionDuration = coneBurstAmount * coneFiringCyclePeriod;

		Vector2 currentPosition = transform.position;
		Vector2 emitter1Position = currentPosition + Vector2.right;
		Vector2 emitter2Position = currentPosition - Vector2.right;

		_emitter = new ConeProjectileEmitter(currentPosition, 0, 0, 0, null, new ProjectileConeSpawnerConfig(coneAngle));

		_e1 = _emitter
		.WithFiringParameters(coneBurstAmount, coneFiringCyclePeriod, 1)
		.WithTransform(emitter1Position, 0)
		.WithMovement(0, -30).Of(_projectile).Fire();

		_e2 = _emitter
		.WithFiringParameters(coneBurstAmount, coneFiringCyclePeriod, 1)
		.WithTransform(emitter2Position, 180)
		.WithMovement(0, 30).Of(_projectile).Fire();

		yield return Timing.WaitForSeconds(emissionDuration);

		yield return Timing.WaitForSeconds(0.5f);
	}
}