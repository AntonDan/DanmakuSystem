using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Projectiles
{
	[BurstCompile]
	public struct ProjectileArcSpawner : IProjectileSpawner<BaseSpawnerConfig<ProjectileArcSpawnerConfig>>
	{
		public ProjectileSpawnerState Fire(in EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, Translation spawnerTranslation, Rotation2D spawnerRotation, BaseSpawnerConfig<ProjectileArcSpawnerConfig> spawnerConfig, ProjectileSpawnerState spawnerState, double currentTime, float deltaTime, Unity.Mathematics.Random rand)
		{
			Vector2 circleCenter = new Vector2(spawnerTranslation.Value.x, spawnerTranslation.Value.y);
			Vector2 spawnPosition;
			Vector2 spawnDirection;

			float radius = spawnerConfig.specializedConfig.radius;
			int projectileCount = spawnerConfig.projectileCountPerCycle;
			if (projectileCount == 0) return spawnerState;

			float arcAngle = Mathf.Deg2Rad * spawnerConfig.specializedConfig.angle;
			float minAngle = Mathf.Deg2Rad * spawnerRotation.angle - arcAngle / 2;
			float maxAngle = minAngle + arcAngle;
			float angleStep = arcAngle / projectileCount;

			Translation entityTranslation = new Translation();
			Rotation2D entityRotation = new Rotation2D();

			for (float angle = minAngle; angle < maxAngle; angle += angleStep)
			{
				spawnDirection.x = Mathf.Cos(angle);
				spawnDirection.y = Mathf.Sin(angle);
				spawnPosition = circleCenter + radius * spawnDirection;
				entityTranslation.Value.x = spawnPosition.x;
				entityTranslation.Value.y = spawnPosition.y;
				entityRotation.SetAngle(Mathf.Rad2Deg * angle);

				ProjectileSpawnerSharedMethods.SpawnEntity(commandBuffer, entityInQueryIndex, spawnerConfig.entityToSpawn, entityTranslation, entityRotation);
			}

			spawnerState.UpdateState(spawnerConfig.burstAmount, spawnerConfig.firingCyclePeriod, currentTime);
			return spawnerState;
		}
	}


	[BurstCompile]
	public struct ProjectileConeSpawner : IProjectileSpawner<BaseSpawnerConfig<ProjectileConeSpawnerConfig>>
	{

		public ProjectileSpawnerState Fire(in EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, Translation spawnerTranslation, Rotation2D spawnerRotation, BaseSpawnerConfig<ProjectileConeSpawnerConfig> spawnerConfig, ProjectileSpawnerState spawnerState, double currentTime, float deltaTime, Unity.Mathematics.Random rand)
		{
			Vector2 spawnPosition = new Vector2(spawnerTranslation.Value.x, spawnerTranslation.Value.y);
			Vector2 spawnDirection;

			float coneAngle = spawnerConfig.specializedConfig.angle;
			int projectileCount = spawnerConfig.projectileCountPerCycle;
			if (projectileCount == 0) return spawnerState;

			float arcAngle = Mathf.Deg2Rad * spawnerConfig.specializedConfig.angle;
			float minAngle = Mathf.Deg2Rad * spawnerRotation.angle - arcAngle / 2;
			float maxAngle = minAngle + arcAngle;

			Translation entityTranslation = new Translation()
			{
				Value = new float3(spawnPosition.x, spawnPosition.y, 0)
			};
			Rotation2D entityRotation = new Rotation2D();

			for (int i = 0; i < projectileCount; ++i)
			{
				float spawnAngle = rand.NextFloat(minAngle, maxAngle);
				spawnDirection.x = Mathf.Cos(spawnAngle);
				spawnDirection.y = Mathf.Sin(spawnAngle);
				entityRotation.SetAngle(Mathf.Rad2Deg * spawnAngle);

				ProjectileSpawnerSharedMethods.SpawnEntity(commandBuffer, entityInQueryIndex, spawnerConfig.entityToSpawn, entityTranslation, entityRotation);
			}

			spawnerState.UpdateState(spawnerConfig.burstAmount, spawnerConfig.firingCyclePeriod, currentTime);
			return spawnerState;
		}
	}
}