using Unity.Entities;
using UnityEngine;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct MovementComponent : IComponentData
	{
		public float rotationSpeed;
		public float movementSpeed;
	}
}