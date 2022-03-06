using Unity.Entities;

namespace Projectiles
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class ProjectileSpawnerControlSystem : SystemBase
	{
		private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;

		protected override void OnCreate()
		{
			_endInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			// EntityCommandBuffer commandBuffer = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();
			// EntityQuery spawnerQuery = GetEntityQuery(ComponentType.ReadOnly<ProjectileCircleSpawnerConfig>());
			// commandBuffer.AddComponent<Disabled>(spawnerQuery);
		}
	}
}