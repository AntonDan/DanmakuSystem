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

		public int GetBurstAmount()
		{
			return burstAmount;
		}

		public void SetBurstAmount(int newBurstAmount)
		{
			burstAmount = newBurstAmount;
		}

		public float GetFiringCyclePeriod()
		{
			return firingCyclePeriod;
		}

		public void SetFiringCyclePeriod(float newFiringCyclePeriod)
		{
			firingCyclePeriod = newFiringCyclePeriod;
		}

		public int GetProjectileCountPerCycle()
		{
			return projectileCountPerCycle;
		}


		public void SetProjectileCountPerCycle(int newProjectileCountPerCycle)
		{
			projectileCountPerCycle = newProjectileCountPerCycle;
		}
	}
}
