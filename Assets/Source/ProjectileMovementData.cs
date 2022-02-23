using Unity.Entities;
using UnityEngine;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct ProjectileMovementData : IComponentData
	{
		public float speed;
		[HideInInspector]
		public Vector2 direction;
	}
}