using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;
using Unity.Entities;

public partial class EnemyBehavior : MonoBehaviour
{
	[SerializeField]
	private GameObject _projectilePrefab;
	[SerializeField]
	private GameObject _playerObject;

	private EntityManager _entityManager;
	private BlobAssetStore _blobAssetStore;
	private Entity _projectileEntity;

	private EnemyBehaviorStateBase currentState;

	void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_blobAssetStore = new BlobAssetStore();
		GameObjectConversionSettings conversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
		_projectileEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_projectilePrefab, conversionSettings);

		Timing.RunCoroutine(BehaviorRoutine().CancelWith(gameObject));
	}

	private void OnDestroy()
	{
		_blobAssetStore.Dispose();
	}

	IEnumerator<float> BehaviorRoutine()
	{
		currentState = new DiveAndEncirclePlayer(gameObject, _playerObject, _projectileEntity);
		while (true)
		{
			yield return Timing.WaitUntilDone(currentState.Run().CancelWith(gameObject));
			currentState.OnDestroy();
			currentState = currentState.GetNextState();
			yield return Timing.WaitForSeconds(0.05f);
		}
	}



}
