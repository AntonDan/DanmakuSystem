using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(Projectiles.BaseSpawnerConfig<Projectiles.ProjectileCircleSpawnerConfig>))]

namespace Projectiles
{
	public struct ProjectileCircleSpawnerConfig
	{
		public float radius;

		public ProjectileCircleSpawnerConfig(float radius)
		{
			this.radius = radius;
		}
	}
}
