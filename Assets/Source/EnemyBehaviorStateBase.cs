using System;
using System.Collections.Generic;
using MEC;
using Unity.Entities;
using UnityEngine;

abstract class EnemyBehaviorStateBase
{
	protected GameObject gameObject;
	protected Transform transform;
	protected SpriteRenderer renderer;
	protected GameObject playerObject;
	protected Transform playerTransform;

	protected EntityManager entityManager;

	public EnemyBehaviorStateBase(GameObject gameObject, GameObject playerObject)
	{
		SetGameObject(gameObject);
		this.playerObject = playerObject;
		if (this.playerObject != null)
		{
			playerTransform = this.playerObject.transform;
		}
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	abstract public IEnumerator<float> Run();

	abstract public EnemyBehaviorStateBase GetNextState();

	abstract public void OnDestroy();

	protected void SetGameObject(GameObject gameObject)
	{
		this.gameObject = gameObject;
		transform = this.gameObject.transform;
		renderer = this.gameObject.GetComponent<SpriteRenderer>();
	}

	static protected IEnumerator<float> RunFor(float duration, Func<float, bool> Action)
	{
		for (float time = 0; time < duration; time += Time.deltaTime)
		{
			float progression = time / duration;
			if (!Action(progression))
				yield break;
			yield return Timing.WaitForOneFrame;
		}
	}

	protected IEnumerator<float> MoveTo(Vector2 from, Vector2 to, float duration)
	{
		Func<float, bool> MoveAction = (prog) => { return ActionMoveTo(from, to, prog); };
		yield return Timing.WaitUntilDone(RunFor(duration, MoveAction).CancelWith(gameObject));
		transform.position = to;
	}

	protected IEnumerator<float> MoveTo(Vector2 from, Transform to, float duration)
	{
		Func<float, bool> MoveAction = (prog) => { return ActionMoveTo(from, to, prog); };
		yield return Timing.WaitUntilDone(RunFor(duration, MoveAction).CancelWith(gameObject, to.gameObject));
		transform.position = to.position;
	}

	protected IEnumerator<float> RotateTo(float from, float to, float duration)
	{
		Func<float, bool> RotateAction = (prog) => { return ActionRotateTo(from, to, prog); };
		yield return Timing.WaitUntilDone(RunFor(duration, RotateAction).CancelWith(gameObject));
	}

	protected IEnumerator<float> ScaleTo(Vector2 from, Vector2 to, float duration)
	{
		Func<float, bool> ScaleAction = (prog) => { return ActionScaleTo(from, to, prog); };
		yield return Timing.WaitUntilDone(RunFor(duration, ScaleAction).CancelWith(gameObject));
		transform.localScale = to;
	}

	protected IEnumerator<float> ColorTo(Color from, Color to, float duration)
	{
		Func<float, bool> ColorAction = (prog) => { return ActionColorTo(from, to, prog); };
		yield return Timing.WaitUntilDone(RunFor(duration, ColorAction).CancelWith(gameObject));
		renderer.color = to;
	}

	private bool ActionMoveTo(Vector2 from, Vector2 to, float progression)
	{
		if (transform == null) return false;
		transform.position = Vector2.Lerp(from, to, progression);
		return true;
	}

	private bool ActionMoveTo(Vector2 from, Transform to, float progression)
	{
		if (transform == null) return false;
		transform.position = Vector2.Lerp(from, to.position, progression);
		return true;
	}

	private bool ActionRotateTo(float from, float to, float progression)
	{
		if (transform == null) return false;
		float newRotation = Mathf.Lerp(from, to, progression);
		throw new NotImplementedException();
		//return true;
	}

	private bool ActionScaleTo(Vector2 from, Vector2 to, float progression)
	{
		if (transform == null) return false;
		transform.localScale = Vector2.Lerp(from, to, progression);
		return true;
	}

	private bool ActionColorTo(Color from, Color to, float progression)
	{
		if (transform == null) return false;
		renderer.color = Color.Lerp(from, to, progression);
		return true;
	}
}