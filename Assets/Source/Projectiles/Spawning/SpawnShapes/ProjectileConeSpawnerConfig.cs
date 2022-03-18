using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(Projectiles.BaseSpawnerConfig<Projectiles.ProjectileConeSpawnerConfig>))]

namespace Projectiles
{
	public struct ProjectileConeSpawnerConfig
	{
		public float angle;

		public ProjectileConeSpawnerConfig(float angle)
		{
			this.angle = angle;
		}
	}
}
