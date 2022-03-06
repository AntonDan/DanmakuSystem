using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Projectiles
{
	[GenerateAuthoringComponent]
	public struct Rotation2D : IComponentData
	{

		public float angle;

		public float2 direction;

		public Rotation2D(float angle)
		{
			this.angle = 0;
			this.direction = float2.zero;
			SetAngle(angle);
		}

		[BurstCompile]
		public void SetAngle(float newAngle)
		{
			angle = newAngle % 360.0f;
			if (angle < 0)
			{
				angle += 360.0f;
			}

			float rotationRadians = Mathf.Deg2Rad * angle;
			direction.x = Mathf.Cos(rotationRadians);
			direction.y = Mathf.Sin(rotationRadians);
		}

		[BurstCompile]
		public void SetDirection(float2 newDirection)
		{
			direction = math.normalize(newDirection);

			float rotationRadians = Mathf.Acos(direction.x);
			angle = Mathf.Rad2Deg * rotationRadians;
		}

		public void RotateBy(float delta)
		{
			SetAngle(angle + delta);
		}
	}
}
