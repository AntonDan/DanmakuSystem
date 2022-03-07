
using Unity.Entities;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct ProjectileCollider : IComponentData
	{
		public float squareRadius;
	}
}

