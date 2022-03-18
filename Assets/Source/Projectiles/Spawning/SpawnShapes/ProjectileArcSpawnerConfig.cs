using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(Projectiles.BaseSpawnerConfig<Projectiles.ProjectileArcSpawnerConfig>))]

namespace Projectiles
{
	public struct ProjectileArcSpawnerConfig
	{
		public float angle;
		public float radius;

		public ProjectileArcSpawnerConfig(float angle, float radius)
		{
			this.angle = angle;
			this.radius = radius;
		}
	}
}
