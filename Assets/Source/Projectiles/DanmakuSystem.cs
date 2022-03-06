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

	public static class SpawnerExtensions
	{
		public static ProjectileEmitterBase Of(this ProjectileEmitterBase emitter, ProjectileEmitterBase subemitter)
		{
			if (emitter == null)
			{
				throw new ArgumentNullException("The provided emitter was null. Please pass a valid emitter");
			}
			if (emitter == null)
			{
				throw new ArgumentNullException("The provided subemitter was null. Please pass a valid emitter");
			}
			ProjectileEmitterBase lowestEmitter = emitter.GetLowestSubemitter();
			lowestEmitter.SetSubemitter(subemitter);
			return emitter;
		}

		public static ProjectileEmitterBase Of(this ProjectileEmitterBase emitter, Entity projectile)
		{
			if (emitter == null)
			{
				throw new ArgumentNullException("The provided emitter was null. Please pass a valid emitter");
			}
			if (projectile == Entity.Null)
			{
				throw new ArgumentNullException("The provided projectile was null. Please pass a valid entity");
			}
			ProjectileEmitterBase lowestEmitter = emitter.GetLowestSubemitter();
			lowestEmitter.SetProjectile(projectile);
			return emitter;
		}
	}

	public abstract class ProjectileEmitterBase
	{
		public abstract void Fire();

		public abstract void SetSubemitter(ProjectileEmitterBase subemitter);
		public abstract void SetProjectile(Entity projectile);

		internal abstract EntityArchetype GetOrCreateArchetype();
		internal abstract void CreateArchetype();
		internal abstract Entity GetOrCreateEntity();
		internal abstract void CreateEntity();
		internal abstract void UpdateEntity();
		internal abstract ProjectileEmitterBase GetLowestSubemitter();
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

		public void MarkEntityDirty()
		{
			isEntityDirty = true;
		}

		public void MarkEntityClean()
		{
			isEntityDirty = false;
		}

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

		public override void SetSubemitter(ProjectileEmitterBase subemitter)
		{
			child = subemitter;
			MarkEntityDirty();
		}

		public override void SetProjectile(Entity projectile)
		{
			config.SetEntityToSpawn(projectile);
			MarkEntityDirty();
		}

		internal override void CreateArchetype()
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

		internal override EntityArchetype GetOrCreateArchetype()
		{
			if (!archetype.Valid)
			{
				CreateArchetype();
			}
			return archetype;
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

		public override void Fire()
		{
			entityManager.Instantiate(GetOrCreateEntity());
		}

		internal override ProjectileEmitterBase GetLowestSubemitter()
		{
			if (child == null)
			{
				return this;
			}
			return child.GetLowestSubemitter();
		}
	}

	public class CircularProjectileEmitter : ProjectileEmitter<ProjectileCircleSpawnerConfig>
	{
		public CircularProjectileEmitter(Vector2 position, float rotation, float movementSpeed, float rotationSpeed, ProjectileCircleSpawnerConfig config) : base(position, rotation, movementSpeed, rotationSpeed, config)
		{ }
	}
}
