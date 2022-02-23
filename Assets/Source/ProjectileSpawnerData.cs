using Unity.Entities;
using UnityEngine;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct ProjectileSpawnerData : IComponentData
	{
		public Entity prefab;
		public float spawnDelay;
		public int particlesPerCycle;
		public float angleStep;
		public float offset;

		[HideInInspector]
		public double nextSpawnTime;
	}
}
