using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct MovementComponent : IComponentData
	{
		public float rotationSpeed;
		public float movementSpeed;
		public float2 movementDirection;
		public bool shouldUseEntityRotationInstead;

		public void SetMovementDirection(Vector2? direction)
		{
			if (direction == null)
			{
				shouldUseEntityRotationInstead = true;
			}
			else
			{
				movementDirection = direction.Value.normalized;
				shouldUseEntityRotationInstead = false;
			}
		}
	}
}