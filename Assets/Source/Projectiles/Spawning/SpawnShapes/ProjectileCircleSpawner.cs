using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Projectiles
{
	[BurstCompile]
	public struct ProjectileCircleSpawner : IProjectileSpawner<BaseSpawnerConfig<ProjectileCircleSpawnerConfig>>
	{
		public ProjectileSpawnerState Fire(in EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, Translation spawnerTranslation, Rotation2D spawnerRotation, BaseSpawnerConfig<ProjectileCircleSpawnerConfig> spawnerConfig, ProjectileSpawnerState spawnerState, double currentTime, float deltaTime, Unity.Mathematics.Random rand)
		{
			Vector2 circleCenter = new Vector2(spawnerTranslation.Value.x, spawnerTranslation.Value.y);
			Vector2 spawnPosition;
			Vector2 spawnDirection;

			float radius = spawnerConfig.specializedConfig.radius;
			int projectileCount = spawnerConfig.projectileCountPerCycle;
			float minAngle = Mathf.Deg2Rad * spawnerRotation.angle;
			float maxAngle = 2 * Mathf.PI + minAngle;
			float angleStep = 2 * Mathf.PI / projectileCount;

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

	public struct ProjectileSpawnerSharedMethods
	{
		public static void SpawnEntity(EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, Entity prefab, Translation translation, Rotation2D rotation)
		{
			Entity newEntity = commandBuffer.Instantiate(entityInQueryIndex, prefab);
			commandBuffer.SetComponent(entityInQueryIndex, newEntity, translation);
			commandBuffer.SetComponent(entityInQueryIndex, newEntity, rotation);
		}
	}
}