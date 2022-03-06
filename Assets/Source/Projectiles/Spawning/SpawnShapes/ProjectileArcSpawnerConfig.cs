using Unity.Entities;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct ProjectileArcSpawnerConfig : IProjectileSpawnerConfig
	{
		public Entity entityToSpawn;
		public int burstAmount;
		public float firingCyclePeriod;
		public int projectileCountPerCycle;

		public float angle;
		public float radius;

		public Entity GetEntityToSpawn()
		{
			return entityToSpawn;
		}

		public void SetEntityToSpawn(Entity newEntity)
		{
			entityToSpawn = newEntity;
		}
	}
}
