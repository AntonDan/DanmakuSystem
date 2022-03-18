using Unity.Entities;
using Unity.Transforms;

namespace Projectiles
{
	public interface IProjectileSpawner<SpawnerConfigType> where SpawnerConfigType : struct, IProjectileSpawnerConfig
	{
		ProjectileSpawnerState Fire(in EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, Translation spawnerTranslation, Rotation2D spawnerRotation, SpawnerConfigType spawnerConfig, ProjectileSpawnerState spawnerState, double currentTime, float deltaTime, Unity.Mathematics.Random rand);
	}
}