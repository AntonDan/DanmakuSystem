using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Projectiles
{
	public class SpawnProjectilesSystem : SystemBase
	{
		private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;

		protected override void OnCreate()
		{
			_beginSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			EntityCommandBuffer.ParallelWriter commandBuffer = _beginSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
			double currentTime = Time.ElapsedTime;

			Entities.WithAll<ProjectileSpawnerData>().ForEach((Entity e, int entityInQueryIndex, ref ProjectileSpawnerData spawnerData, in Translation translation) =>
			{
				if (spawnerData.nextSpawnTime < currentTime)
				{
					spawnerData.nextSpawnTime += spawnerData.spawnDelay;
					SpawnProjectilesCirclePattern(commandBuffer, entityInQueryIndex, translation, spawnerData);
					spawnerData.offset += spawnerData.angleStep;
				}
			}).ScheduleParallel();
			_beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
		}



		static void SpawnProjectilesCirclePattern(in EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, in Translation spawnerTranslation, in ProjectileSpawnerData spawnerData)
		{
			Vector2 circleCenter = new Vector2(spawnerTranslation.Value.x, spawnerTranslation.Value.y);
			Vector2 particlePosition;
			Vector2 spawnDirection;

			float radius = 2.0f;
			int projectileCount = spawnerData.particlesPerCycle;
			float maxAngle = 2 * Mathf.PI + spawnerData.offset;
			float angleStep = 2 * Mathf.PI / projectileCount;

			Translation projectileTranslation = new Translation();
			Rotation projectileRotation = new Rotation();
			ProjectileMovementData projectileMovement = new ProjectileMovementData();
			projectileMovement.speed = 2.0f;


			for (float angle = spawnerData.offset; angle < maxAngle; angle += angleStep)
			{
				spawnDirection.x = Mathf.Cos(angle);
				spawnDirection.y = Mathf.Sin(angle);
				particlePosition = circleCenter + radius * spawnDirection;
				projectileTranslation.Value.x = particlePosition.x;
				projectileTranslation.Value.y = particlePosition.y;
				projectileRotation.Value = quaternion.Euler(0, 0, angle);
				projectileMovement.direction = spawnDirection;

				Entity newEntity = commandBuffer.Instantiate(entityInQueryIndex, spawnerData.prefab);
				commandBuffer.SetComponent(entityInQueryIndex, newEntity, projectileTranslation);
				commandBuffer.SetComponent(entityInQueryIndex, newEntity, projectileRotation);
				commandBuffer.SetComponent(entityInQueryIndex, newEntity, projectileMovement);
			}
		}

	}
}