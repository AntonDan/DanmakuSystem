using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System;
using Unity.Mathematics;

[assembly: RegisterGenericComponentType(typeof(Projectiles.BaseSpawnerConfig<Projectiles.ProjectileArcSpawnerConfig>))]
[assembly: RegisterGenericComponentType(typeof(Projectiles.BaseSpawnerConfig<Projectiles.ProjectileCircleSpawnerConfig>))]

namespace Projectiles
{
	public class DanmakuSystem : MonoBehaviour
	{
	}


	public abstract class ProjectileEmitterBase
	{
		public abstract ProjectileEmitterBase WithTransform(Vector2 position, float rotation);
		public abstract ProjectileEmitterBase WithMovement(float movementSpeed, float rotationSpeed, Vector2? movementDirection = null);
		public abstract ProjectileEmitterBase WithFiringParameters(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle);

		public abstract ProjectileEmitterBase Of(ProjectileEmitterBase subemitter);
		public abstract ProjectileEmitterBase Of(Entity projectile);
		public abstract Entity Fire();

		internal abstract Entity GetOrCreateEntity();
		internal abstract void CreateEntity();
		internal abstract void UpdateEntity();
	}

	public class ProjectileEmitter<SpawnerConfig> : ProjectileEmitterBase where SpawnerConfig : struct
	{
		protected Translation translation;
		protected Rotation2D rotation;
		protected MovementComponent movement;

		protected EntityArchetype archetype;
		protected EntityManager entityManager;
		protected ProjectileEmitterBase child;
		protected Entity entity;

		protected bool isEntityDirty;

		private BaseSpawnerConfig<SpawnerConfig> _config;

		public ProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, Vector2? movementDirection, SpawnerConfig config)
		{
			SetTransform(position, rotation);
			SetMovementData(movementSpeed, rotationSpeed, movementDirection);
			_config.specializedConfig = config;
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
		#endregion

		#region  Setters
		public void SetSubemitter(ProjectileEmitterBase subemitter)
		{
			child = subemitter;
			MarkEntityDirty();
		}

		public void SetProjectile(Entity projectile)
		{
			_config.SetEntityToSpawn(projectile);
			MarkEntityDirty();
		}

		public void SetTransform(Vector2 position, float rotation)
		{
			this.translation.Value = new float3(position.x, position.y, 0);
			this.rotation.SetAngle(rotation);
			MarkEntityDirty();
		}

		public void SetMovementData(float movementSpeed, float rotationSpeed, Vector2? movementDirection = null)
		{
			movement.movementSpeed = movementSpeed;
			movement.rotationSpeed = rotationSpeed;
			movement.SetMovementDirection(movementDirection);
			MarkEntityDirty();
		}

		public void SetFiringParameters(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle)
		{
			_config.SetBurstAmount(burstAmount);
			_config.SetFiringCyclePeriod(firingCyclePeriod);
			_config.SetProjectileCountPerCycle(projectileCountPerCycle);
			MarkEntityDirty();
		}

		public void SetConfig(SpawnerConfig config)
		{
			_config.specializedConfig = config;
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
							typeof(BaseSpawnerConfig<SpawnerConfig>),
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

			entity = entityManager.CreateEntity(GetOrCreateArchetype());
			UpdateEntity();
		}

		internal override void UpdateEntity()
		{
			if (entity == Entity.Null)
			{
				CreateEntity();
			}

			if (child != null)
			{
				_config.SetEntityToSpawn(child.GetOrCreateEntity());
			}
			entityManager.SetComponentData<Translation>(entity, translation);
			entityManager.SetComponentData<Rotation2D>(entity, rotation);
			entityManager.SetComponentData<MovementComponent>(entity, movement);
			entityManager.SetComponentData<BaseSpawnerConfig<SpawnerConfig>>(entity, _config);
		}
		#endregion

		#region API methods
		public override ProjectileEmitterBase WithTransform(Vector2 position, float rotation)
		{
			SetTransform(position, rotation);
			return this;
		}

		public override ProjectileEmitterBase WithMovement(float movementSpeed, float rotationSpeed, Vector2? movementDirection = null)
		{
			SetMovementData(movementSpeed, rotationSpeed, movementDirection);
			return this;
		}

		public override ProjectileEmitterBase WithFiringParameters(int burstAmount, float firingCyclePeriod, int projectileCountPerCycle)
		{
			SetFiringParameters(burstAmount, firingCyclePeriod, projectileCountPerCycle);
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

		public override Entity Fire()
		{
			return entityManager.Instantiate(GetOrCreateEntity());
		}

		public ProjectileEmitterBase WithConfig(SpawnerConfig config)
		{
			SetConfig(config);
			return this;
		}
		#endregion
	}

	public class CircularProjectileEmitter : ProjectileEmitter<ProjectileCircleSpawnerConfig>
	{
		public CircularProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, Vector2? movementDirection, ProjectileCircleSpawnerConfig config) : base(position, rotation, movementSpeed, rotationSpeed, movementDirection, config)
		{ }
	}

	public class ArcProjectileEmitter : ProjectileEmitter<ProjectileArcSpawnerConfig>
	{
		public ArcProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, Vector2? movementDirection, ProjectileArcSpawnerConfig config) : base(position, rotation, movementSpeed, rotationSpeed, movementDirection, config)
		{ }
	}

	public class ConeProjectileEmitter : ProjectileEmitter<ProjectileConeSpawnerConfig>
	{
		public ConeProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, Vector2? movementDirection, ProjectileConeSpawnerConfig config) : base(position, rotation, movementSpeed, rotationSpeed, movementDirection, config)
		{ }
	}
}
