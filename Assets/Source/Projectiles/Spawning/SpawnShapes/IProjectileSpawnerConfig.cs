using Unity.Entities;

namespace Projectiles
{
	public interface IProjectileSpawnerConfig : IComponentData
	{
		Entity GetEntityToSpawn();
		void SetEntityToSpawn(Entity newEntity);
	}
}
