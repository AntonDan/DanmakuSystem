using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Projectiles
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class ProjectileCollisionSystem : SystemBase
	{
		private Transform _mainCameraTransform;
		private Camera _mainCamera;
		private Rect _cameraBounds;
		private Vector3 _cameraHalfSize;
		private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

		protected override void OnCreate()
		{
			_cameraBounds = new Rect(Vector2.zero, Vector2.one);
			_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			RefreshCameraBounds();
			EntityQuery projectileQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<ProjectileCollider>());

			ProjectileCollisionJob projectileCollisionJob = new ProjectileCollisionJob
			{
				entitiesToDestroy = EntityDestructionSystem.spawnersToDestroyParallel,
				entityTypeHandle = GetEntityTypeHandle(),
				translationTypeHandle = GetComponentTypeHandle<Translation>(true),
				projectileCollisionHandle = GetComponentTypeHandle<ProjectileCollider>(true),
				projectileBounds = _cameraBounds,
				playerPosition = PlayerMovement.playerPosition
			};
			Dependency = projectileCollisionJob.ScheduleParallel(projectileQuery, 1, Dependency);

			_endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
		}

		private void RefreshCameraBounds()
		{
			if (_mainCamera == null || _mainCameraTransform == null)
			{
				_mainCamera = Camera.main;
				if (_mainCamera == null) return;

				_mainCameraTransform = _mainCamera.transform;
				_cameraHalfSize = new Vector3(_mainCamera.orthographicSize * _mainCamera.aspect * 2.0f, _mainCamera.orthographicSize * 2.0f, 0);
			}
			_cameraBounds.min = _mainCameraTransform.position - _cameraHalfSize;
			_cameraBounds.max = _mainCameraTransform.position + _cameraHalfSize;
		}

		[BurstCompile]
		public struct ProjectileCollisionJob : IJobEntityBatch
		{
			public NativeQueue<Entity>.ParallelWriter entitiesToDestroy;
			[ReadOnly] public EntityTypeHandle entityTypeHandle;
			[ReadOnly] public ComponentTypeHandle<Translation> translationTypeHandle;
			[ReadOnly] public ComponentTypeHandle<ProjectileCollider> projectileCollisionHandle;
			[ReadOnly] public Rect projectileBounds;
			[ReadOnly] public Vector2 playerPosition;

			public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
			{
				NativeArray<Entity> entityArray = batchInChunk.GetNativeArray(entityTypeHandle);
				NativeArray<Translation> translationArray = batchInChunk.GetNativeArray(translationTypeHandle);
				NativeArray<ProjectileCollider> projectileCollisionArray = batchInChunk.GetNativeArray(projectileCollisionHandle);
				Vector2 projectileToPlayerVector = Vector2.zero;

				for (int i = 0; i < batchInChunk.Count; ++i)
				{
					Translation translation = translationArray[i];

					projectileToPlayerVector.x = translation.Value.x - playerPosition.x;
					projectileToPlayerVector.y = translation.Value.y - playerPosition.y;

					if (!projectileBounds.Contains(translation.Value) || projectileToPlayerVector.sqrMagnitude < projectileCollisionArray[i].squareRadius)
					{
						entitiesToDestroy.Enqueue(entityArray[i]);
					}
				}
			}
		}
	}
}

