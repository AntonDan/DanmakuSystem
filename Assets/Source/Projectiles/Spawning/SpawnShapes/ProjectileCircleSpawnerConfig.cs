using Unity.Collections;
using Unity.Entities;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct ProjectileCircleSpawnerConfig : IProjectileSpawnerConfig
	{
		public Entity entityToSpawn;
		public int burstAmount;
		public float firingCyclePeriod;
		public int projectileCountPerCycle;

		public float radius;


		public ProjectileCircleSpawnerConfig(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle, float radius)
		{
			this.entityToSpawn = Entity.Null;
			this.burstAmount = burstAmount;
			this.firingCyclePeriod = firingCyclePeriod;
			this.projectileCountPerCycle = projectileCountPerCycle;
			this.radius = radius;
		}

		public ProjectileCircleSpawnerConfig(Entity entityToSpawn, int burstAmount, float firingCyclePeriod, int projectileCountPerCycle, float radius)
		{
			this.entityToSpawn = entityToSpawn;
			this.burstAmount = burstAmount;
			this.firingCyclePeriod = firingCyclePeriod;
			this.projectileCountPerCycle = projectileCountPerCycle;
			this.radius = radius;
		}

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
