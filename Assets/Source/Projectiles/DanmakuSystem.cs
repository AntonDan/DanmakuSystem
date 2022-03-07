using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System;
using Unity.Mathematics;

namespace Projectiles
{
	public class DanmakuSystem : MonoBehaviour
	{
	}


	public abstract class ProjectileEmitterBase
	{
		public abstract ProjectileEmitterBase WithMovement(float movementSpeed, float rotationSpeed);
		public abstract ProjectileEmitterBase WithFiringParameters(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle);

		public abstract ProjectileEmitterBase Of(ProjectileEmitterBase subemitter);
		public abstract ProjectileEmitterBase Of(Entity projectile);
		public abstract void Fire();

		internal abstract Entity GetOrCreateEntity();
		internal abstract void CreateEntity();
		internal abstract void UpdateEntity();
	}

	public class ProjectileEmitter<SpawnerConfig> : ProjectileEmitterBase where SpawnerConfig : struct, IProjectileSpawnerConfig
	{
		protected EntityArchetype archetype;
		protected EntityManager entityManager;
		protected ProjectileEmitterBase child;

		protected Entity entity;

		protected Translation translation;
		protected Rotation2D rotation;
		protected MovementComponent movement;
		protected SpawnerConfig config;

		protected bool isEntityDirty;

		public ProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, SpawnerConfig config)
		{
			SetTransform(position, rotation);
			SetMovementData(movementSpeed, rotationSpeed);
			SetConfig(config);
		}

		#region Getters
		public float3 GetPosition()
		{
			return translation.Value;
		}

		public float GetRotation()
		{
			return rotation.angle;
		}

		public Vector2 GetDirection()
		{
			return rotation.direction;
		}

		public SpawnerConfig GetConfig()
		{
			return config;
		}
		#endregion

		#region  Setters
		public void SetSubemitter(ProjectileEmitterBase subemitter)
		{
			child = subemitter;
			MarkEntityDirty();
		}

		public void SetProjectile(Entity projectile)
		{
			config.SetEntityToSpawn(projectile);
			MarkEntityDirty();
		}

		public void SetPosition(Vector2 position)
		{
			translation.Value = new float3(position.x, position.y, 0);
			MarkEntityDirty();
		}

		public void SetPosition(float3 position)
		{
			translation.Value = position;
			MarkEntityDirty();
		}

		public void SetRotation(float angle)
		{
			rotation.SetAngle(angle);
			MarkEntityDirty();
		}

		public void SetTransform(Vector2 position, float rotation)
		{
			this.translation.Value = new float3(position.x, position.y, 0);
			this.rotation.SetAngle(rotation);
			MarkEntityDirty();
		}

		public void SetMovementData(float movementSpeed, float rotationSpeed)
		{
			movement.movementSpeed = movementSpeed;
			movement.rotationSpeed = rotationSpeed;
			MarkEntityDirty();
		}

		public void SetConfig(SpawnerConfig config)
		{
			this.config = config;
			MarkEntityDirty();
		}

		public void SetConfig(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle)
		{
			config.SetBurstAmount(burstAmount);
			config.SetFiringCyclePeriod(firingCyclePeriod);
			config.SetProjectileCountPerCycle(projectileCountPerCycle);
			MarkEntityDirty();
		}
		#endregion

		#region Internal Methods
		protected void MarkEntityDirty()
		{
			isEntityDirty = true;
		}

		protected void MarkEntityClean()
		{
			isEntityDirty = false;
		}

		protected void CreateArchetype()
		{
			entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			archetype = entityManager.CreateArchetype(
							typeof(Translation),
							typeof(Rotation2D),
							typeof(LocalToWorld),
							typeof(MovementComponent),
							typeof(ProjectileSpawnerState),
							typeof(SpawnerConfig),
							typeof(Prefab)
			);
		}

		protected EntityArchetype GetOrCreateArchetype()
		{
			if (!archetype.Valid)
			{
				CreateArchetype();
			}
			return archetype;
		}

		internal override Entity GetOrCreateEntity()
		{
			if (entity == Entity.Null)
			{
				CreateEntity();
			}
			if (isEntityDirty)
			{
				UpdateEntity();
				MarkEntityClean();
			}
			return entity;
		}

		internal override void CreateEntity()
		{
			if (entity != Entity.Null)
			{
				entityManager.DestroyEntity(entity);
			}

			if (child != null)
			{
				config.SetEntityToSpawn(child.GetOrCreateEntity());
			}
			entity = entityManager.CreateEntity(GetOrCreateArchetype());
			entityManager.SetComponentData<Translation>(entity, translation);
			entityManager.SetComponentData<Rotation2D>(entity, rotation);
			entityManager.SetComponentData<MovementComponent>(entity, movement);
			entityManager.SetComponentData<SpawnerConfig>(entity, config);
		}

		internal override void UpdateEntity()
		{
			if (entity == Entity.Null)
			{
				CreateEntity();
			}

			if (child != null)
			{
				config.SetEntityToSpawn(child.GetOrCreateEntity());
			}
			entityManager.SetComponentData<Translation>(entity, translation);
			entityManager.SetComponentData<Rotation2D>(entity, rotation);
			entityManager.SetComponentData<MovementComponent>(entity, movement);
			entityManager.SetComponentData<SpawnerConfig>(entity, config);
		}
		#endregion

		#region API methods
		public override ProjectileEmitterBase WithMovement(float movementSpeed, float rotationSpeed)
		{
			SetMovementData(movementSpeed, rotationSpeed);
			return this;
		}

		public override ProjectileEmitterBase WithFiringParameters(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle)
		{
			SetConfig(burstAmount, firingCyclePeriod, projectileCountPerCycle);
			return this;
		}

		public override ProjectileEmitterBase Of(ProjectileEmitterBase subemitter)
		{
			if (subemitter == null)
			{
				throw new ArgumentNullException("The provided subemitter was null. Please pass a valid emitter");
			}
			SetSubemitter(subemitter);
			return this;
		}

		public override ProjectileEmitterBase Of(Entity projectile)
		{
			if (projectile == Entity.Null)
			{
				throw new ArgumentNullException("The provided projectile was null. Please pass a valid entity");
			}
			SetProjectile(projectile);
			return this;
		}

		public override void Fire()
		{
			entityManager.Instantiate(GetOrCreateEntity());
		}

		#endregion
	}

	public class CircularProjectileEmitter : ProjectileEmitter<ProjectileCircleSpawnerConfig>
	{
		public CircularProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, ProjectileCircleSpawnerConfig config) : base(position, rotation, movementSpeed, rotationSpeed, config)
		{ }
	}
}
