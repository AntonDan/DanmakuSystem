using Unity.Entities;
using UnityEngine;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct ProjectileSpawnerState : IComponentData
	{
		[HideInInspector]
		public double nextSpawnTime;

		[HideInInspector]
		public int burstCount;

		[HideInInspector]
		public bool shouldBeDestroyed;

		public void UpdateState(int burstAmount, float firingCyclePeriod, double currentTime)
		{
			++burstCount;
			shouldBeDestroyed = burstCount >= burstAmount;
			nextSpawnTime = currentTime + firingCyclePeriod;
		}
	}
}
