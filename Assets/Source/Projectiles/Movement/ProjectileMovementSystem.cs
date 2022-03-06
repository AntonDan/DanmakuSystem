using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
			EntityQuery projectileQuery = GetEntityQuery(ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation2D>(), ComponentType.ReadOnly<MovementComponent>());

			EntityCommandBuffer.ParallelWriter commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
			EntityMovementJob projectileMovementJob = new EntityMovementJob
			{
				entitiesToDestroy = EntityDestructionSystem.spawnersToDestroyParallel,
				entityTypeHandle = GetEntityTypeHandle(),
				translationTypeHandle = GetComponentTypeHandle<Translation>(false),
				rotationTypeHandle = GetComponentTypeHandle<Rotation2D>(false),
				projectileMoveTypeHandle = GetComponentTypeHandle<MovementComponent>(true),
				deltaTime = Time.DeltaTime,
				projectileBounds = _cameraBounds,
				commandBuffer = commandBuffer
			};
			Dependency = projectileMovementJob.ScheduleParallel(projectileQuery, 1, Dependency);

			// Dependency.Complete();
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
	}

	[BurstCompile]
	public struct EntityMovementJob : IJobEntityBatchWithIndex
	{
		public NativeQueue<Entity>.ParallelWriter entitiesToDestroy;

		[ReadOnly] public EntityTypeHandle entityTypeHandle;

		public ComponentTypeHandle<Translation> translationTypeHandle;

		public ComponentTypeHandle<Rotation2D> rotationTypeHandle;

		[ReadOnly] public ComponentTypeHandle<MovementComponent> projectileMoveTypeHandle;

		[ReadOnly] public float deltaTime;

		[ReadOnly] public Rect projectileBounds;

		public EntityCommandBuffer.ParallelWriter commandBuffer;

		public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
		{
			NativeArray<Entity> entityChunk = batchInChunk.GetNativeArray(entityTypeHandle);
			NativeArray<Translation> translationArray = batchInChunk.GetNativeArray(translationTypeHandle);
			NativeArray<Rotation2D> rotationArray = batchInChunk.GetNativeArray(rotationTypeHandle);
			NativeArray<MovementComponent> projectileMoveDataArray = batchInChunk.GetNativeArray(projectileMoveTypeHandle);
			for (int i = 0; i < batchInChunk.Count; ++i)
			{
				Translation translation = translationArray[i];
				if (!projectileBounds.Contains(translation.Value))
				{
					entitiesToDestroy.Enqueue(entityChunk[i]);
				}
				else
				{
					MovementComponent projectileMoveData = projectileMoveDataArray[i];
					float2 delta = (projectileMoveData.movementSpeed * deltaTime) * rotationArray[i].direction;
					translation.Value.x += delta.x;
					translation.Value.y += delta.y;
					translation.Value.z = translation.Value.y + 0.25f * translation.Value.x;
					translationArray[i] = translation;

					Rotation2D rotation = rotationArray[i];
					rotation.RotateBy(projectileMoveData.rotationSpeed * deltaTime);
					rotationArray[i] = rotation;
				}
			}
		}
	}
}