using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Projectiles
{
	public class ProjectileMovementSystem : SystemBase
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
			EntityCommandBuffer.ParallelWriter commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

			EntityQuery projectileQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<ProjectileMovementData>());

			ProjectileMovementJob projectileMovementJob = new ProjectileMovementJob
			{
				entityTypeHandle = GetEntityTypeHandle(),
				translationTypeHandle = GetComponentTypeHandle<Translation>(false),
				projectileMoveTypeHandle = GetComponentTypeHandle<ProjectileMovementData>(true),
				deltaTime = Time.DeltaTime,
				projectileBounds = _cameraBounds,
				commandBuffer = commandBuffer
			};
			Dependency = projectileMovementJob.ScheduleParallel(projectileQuery, 1, Dependency);
			_endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
		}

		private void RefreshCameraBounds()
		{
			if (_mainCamera == null || _mainCameraTransform == null)
			{
				_mainCamera = Camera.main;
				if (_mainCamera != null)
				{
					_mainCameraTransform = _mainCamera.transform;
					_cameraHalfSize = new Vector3(_mainCamera.orthographicSize * 2.0f, _mainCamera.orthographicSize / _mainCamera.aspect * 2.0f, 0);
				}
				else
				{
					return;
				}
			}
			_cameraBounds.min = _mainCameraTransform.position - _cameraHalfSize;
			_cameraBounds.max = _mainCameraTransform.position + _cameraHalfSize;
		}
	}

	public struct ProjectileMovementJob : IJobEntityBatchWithIndex
	{
		[ReadOnly] public EntityTypeHandle entityTypeHandle;

		public ComponentTypeHandle<Translation> translationTypeHandle;

		[ReadOnly] public ComponentTypeHandle<ProjectileMovementData> projectileMoveTypeHandle;

		[ReadOnly] public float deltaTime;

		[ReadOnly] public Rect projectileBounds;

		public EntityCommandBuffer.ParallelWriter commandBuffer;

		[BurstCompile]
		public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
		{
			var entityChunk = batchInChunk.GetNativeArray(entityTypeHandle);
			var translationArray = batchInChunk.GetNativeArray(translationTypeHandle);
			var projectileMoveDataArray = batchInChunk.GetNativeArray(projectileMoveTypeHandle);
			for (int i = 0; i < batchInChunk.Count; ++i)
			{
				Translation translation = translationArray[i];
				ProjectileMovementData projectileMoveData = projectileMoveDataArray[i];
				float2 delta = (projectileMoveData.speed * deltaTime) * projectileMoveData.direction;
				translation.Value.x += delta.x;
				translation.Value.y += delta.y;
				if (!projectileBounds.Contains(translation.Value))
				{
					int entityIndex = indexOfFirstEntityInQuery + i;
					commandBuffer.DestroyEntity(entityIndex, entityChunk[i]);
				}
				else
				{
					translationArray[i] = translation;
				}
			}
		}
	}
}