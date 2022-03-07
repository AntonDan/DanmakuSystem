using Unity.Entities;

namespace Projectiles
{
	public interface IProjectileSpawnerConfig : IComponentData
	{
		Entity GetEntityToSpawn();
		void SetEntityToSpawn(Entity newEntity);

		int GetBurstAmount();
		void SetBurstAmount(int newBurstAmount);

		float GetFiringCyclePeriod();
		void SetFiringCyclePeriod(float newFiringCyclePeriod);

		int GetProjectileCountPerCycle();
		void SetProjectileCountPerCycle(int newProjectileCountPerCycle);
	}
}
