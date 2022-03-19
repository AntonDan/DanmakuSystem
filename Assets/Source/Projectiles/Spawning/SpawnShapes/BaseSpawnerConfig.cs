using Unity.Entities;

namespace Projectiles
{

	public struct BaseSpawnerConfig<T> : IProjectileSpawnerConfig where T : struct
	{
		public Entity entityToSpawn;
		public int burstAmount;
		public float firingCyclePeriod;
		public int projectileCountPerCycle;

		public T specializedConfig;

		public BaseSpawnerConfig(Entity entityToSpawn, int burstAmount, float firingCyclePeriod, int projectileCountPerCycle)
		{
			this.entityToSpawn = entityToSpawn;
			this.burstAmount = burstAmount;
			this.firingCyclePeriod = firingCyclePeriod;
			this.projectileCountPerCycle = projectileCountPerCycle;
			specializedConfig = default;
		}

		public BaseSpawnerConfig(Entity entityToSpawn, int burstAmount, float firingCyclePeriod, int projectileCountPerCycle, T specializedConfig)
		{
			this.entityToSpawn = entityToSpawn;
			this.burstAmount = burstAmount;
			this.firingCyclePeriod = firingCyclePeriod;
			this.projectileCountPerCycle = projectileCountPerCycle;
			this.specializedConfig = specializedConfig;
		}

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
