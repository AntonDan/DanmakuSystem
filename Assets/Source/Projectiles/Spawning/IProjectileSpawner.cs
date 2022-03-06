using Unity.Entities;
using Unity.Transforms;

namespace Projectiles
{
	public interface IProjectileSpawner<SpawnerConfigType>
	{
		ProjectileSpawnerState Fire(in EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, Translation spawnerTranslation, Rotation2D spawnerRotation, SpawnerConfigType spawnerConfig, ProjectileSpawnerState spawnerState, double currentTime, float deltaTime);
	}
}