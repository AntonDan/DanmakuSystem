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
	public partial class EntityMovementSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

		protected override void OnCreate()
		{
			_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			EntityQuery projectileQuery = GetEntityQuery(ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation2D>(), ComponentType.ReadOnly<MovementComponent>());

			EntityMovementJob projectileMovementJob = new EntityMovementJob
			{
				translationTypeHandle = GetComponentTypeHandle<Translation>(false),
				rotationTypeHandle = GetComponentTypeHandle<Rotation2D>(false),
				projectileMoveTypeHandle = GetComponentTypeHandle<MovementComponent>(true),
				deltaTime = Time.DeltaTime,
			};
			Dependency = projectileMovementJob.ScheduleParallel(projectileQuery, Dependency);

			// Dependency.Complete();
			_endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
		}
	}

	[BurstCompile]
	public struct EntityMovementJob : IJobEntityBatch
	{
		public ComponentTypeHandle<Translation> translationTypeHandle;

		public ComponentTypeHandle<Rotation2D> rotationTypeHandle;

		[ReadOnly] public ComponentTypeHandle<MovementComponent> projectileMoveTypeHandle;

		[ReadOnly] public float deltaTime;

		public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
		{
			NativeArray<Translation> translationArray = batchInChunk.GetNativeArray(translationTypeHandle);
			NativeArray<Rotation2D> rotationArray = batchInChunk.GetNativeArray(rotationTypeHandle);
			NativeArray<MovementComponent> entityMovementArray = batchInChunk.GetNativeArray(projectileMoveTypeHandle);
			for (int i = 0; i < batchInChunk.Count; ++i)
			{
				Translation translation = translationArray[i];
				MovementComponent projectileMoveData = entityMovementArray[i];
				float2 delta = (projectileMoveData.movementSpeed * deltaTime) * (projectileMoveData.shouldUseEntityRotationInstead ? rotationArray[i].direction : projectileMoveData.movementDirection);
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