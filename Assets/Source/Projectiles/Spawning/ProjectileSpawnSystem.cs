using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(Projectiles.ProjectileSpawnSystem.ProjectileSpawningJob<Projectiles.BaseSpawnerConfig<Projectiles.ProjectileArcSpawnerConfig>, Projectiles.ProjectileArcSpawner>))]
[assembly: RegisterGenericJobType(typeof(Projectiles.ProjectileSpawnSystem.ProjectileSpawningJob<Projectiles.BaseSpawnerConfig<Projectiles.ProjectileCircleSpawnerConfig>, Projectiles.ProjectileCircleSpawner>))]
[assembly: RegisterGenericJobType(typeof(Projectiles.ProjectileSpawnSystem.ProjectileSpawningJob<Projectiles.BaseSpawnerConfig<Projectiles.ProjectileConeSpawnerConfig>, Projectiles.ProjectileConeSpawner>))]

namespace Projectiles
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class ProjectileSpawnSystem : SystemBase
	{
		private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;
		private Unity.Mathematics.Random _rand;

		protected override void OnCreate()
		{
			TypeManager.Initialize();
			_beginSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
			_rand = new Unity.Mathematics.Random((uint)DateTime.UtcNow.Millisecond);
		}

		protected override void OnUpdate()
		{
			ScheduleSpawnerJob<BaseSpawnerConfig<ProjectileArcSpawnerConfig>, ProjectileArcSpawner>();
			ScheduleSpawnerJob<BaseSpawnerConfig<ProjectileCircleSpawnerConfig>, ProjectileCircleSpawner>();
			ScheduleSpawnerJob<BaseSpawnerConfig<ProjectileConeSpawnerConfig>, ProjectileConeSpawner>();

			// Dependency.Complete();
			_beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
		}

		protected void ScheduleSpawnerJob<SpawnerConfigType, SpawnerBehaviorType>() where SpawnerConfigType : struct, IProjectileSpawnerConfig where SpawnerBehaviorType : IProjectileSpawner<SpawnerConfigType>
		{
			EntityQuery projectileSpawnerQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Rotation2D>(), ComponentType.ReadOnly<SpawnerConfigType>(), ComponentType.ReadWrite<ProjectileSpawnerState>());
			if (projectileSpawnerQuery.IsEmpty) return;

			EntityCommandBuffer.ParallelWriter commandBuffer = _beginSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
			ProjectileSpawningJob<SpawnerConfigType, SpawnerBehaviorType> projectileSpawnJob = new ProjectileSpawningJob<SpawnerConfigType, SpawnerBehaviorType>
			{
				spawnersToDestroy = EntityDestructionSystem.spawnersToDestroyParallel,
				entityTypeHandle = GetEntityTypeHandle(),
				translationTypeHandle = GetComponentTypeHandle<Translation>(true),
				rotationTypeHandle = GetComponentTypeHandle<Rotation2D>(true),
				spawnerDataTypeHandle = GetComponentTypeHandle<SpawnerConfigType>(true),
				spawnerStateTypeHandle = GetComponentTypeHandle<ProjectileSpawnerState>(false),
				currentTime = Time.ElapsedTime,
				deltaTime = Time.DeltaTime,
				commandBuffer = commandBuffer,
				seed = (uint)(UnityEngine.Random.Range(0, int.MaxValue))
			};

			Dependency = projectileSpawnJob.ScheduleParallel(projectileSpawnerQuery, 1, Dependency);
		}

		[BurstCompile]
		public struct ProjectileSpawningJob<SpawnerConfigType, SpawnerBehaviorType> : IJobEntityBatchWithIndex where SpawnerConfigType : struct, IProjectileSpawnerConfig where SpawnerBehaviorType : IProjectileSpawner<SpawnerConfigType>
		{
			public NativeQueue<Entity>.ParallelWriter spawnersToDestroy;
			[ReadOnly] public EntityTypeHandle entityTypeHandle;
			[ReadOnly] public ComponentTypeHandle<Translation> translationTypeHandle;
			[ReadOnly] public ComponentTypeHandle<Rotation2D> rotationTypeHandle;
			[ReadOnly] public ComponentTypeHandle<SpawnerConfigType> spawnerDataTypeHandle;
			public ComponentTypeHandle<ProjectileSpawnerState> spawnerStateTypeHandle;

			[ReadOnly] public double currentTime;
			[ReadOnly] public float deltaTime;

			public EntityCommandBuffer.ParallelWriter commandBuffer;
			public uint seed;

			private Unity.Mathematics.Random rand;
			[ReadOnly] private SpawnerBehaviorType spawnerBehavior;


			public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
			{
				NativeArray<Entity> entityArray = batchInChunk.GetNativeArray(entityTypeHandle);
				NativeArray<Translation> translationArray = batchInChunk.GetNativeArray(translationTypeHandle);
				NativeArray<Rotation2D> rotationArray = batchInChunk.GetNativeArray(rotationTypeHandle);
				NativeArray<SpawnerConfigType> spawnerDataArray = batchInChunk.GetNativeArray(spawnerDataTypeHandle);
				NativeArray<ProjectileSpawnerState> spawnerStateArray = batchInChunk.GetNativeArray(spawnerStateTypeHandle);
				rand = new Unity.Mathematics.Random(seed + (uint)indexOfFirstEntityInQuery);

				for (int i = 0; i < batchInChunk.Count; ++i)
				{
					ProjectileSpawnerState spawnerState = spawnerStateArray[i];
					if (spawnerState.nextSpawnTime < currentTime && !spawnerState.shouldBeDestroyed)
					{
						spawnerStateArray[i] = spawnerBehavior.Fire(commandBuffer, indexOfFirstEntityInQuery + i, translationArray[i], rotationArray[i], spawnerDataArray[i], spawnerState, currentTime, deltaTime, rand);
						if (spawnerStateArray[i].shouldBeDestroyed)
						{
							spawnersToDestroy.Enqueue(entityArray[i]);
						}
					}
				}
			}

		}
	}
}