using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Projectiles
{
	[BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(BeginPresentationEntityCommandBufferSystem))]
	public partial class EntityDestructionSystem : SystemBase
	{
		public static NativeQueue<Entity>.ParallelWriter spawnersToDestroyParallel;
		public static NativeQueue<Entity> spawnersToDestroy;

		protected override void OnCreate()
		{
			base.OnCreate();
			spawnersToDestroy = new NativeQueue<Entity>(Allocator.Persistent);
			spawnersToDestroyParallel = spawnersToDestroy.AsParallelWriter();
		}

		protected override void OnUpdate()
		{
			NativeArray<Entity> TempArray = spawnersToDestroy.ToArray(Allocator.Temp);
			EntityManager.DestroyEntity(TempArray);
			spawnersToDestroy.Clear();
			TempArray.Dispose();
		}

		protected override void OnDestroy()
		{
			spawnersToDestroy.Dispose();
			base.OnDestroy();
		}

	}
}