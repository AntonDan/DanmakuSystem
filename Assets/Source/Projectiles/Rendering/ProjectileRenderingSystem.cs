using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Projectiles
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class ProjectileRenderingSystem : SystemBase
	{
		public struct ProjectileRenderingJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<Translation> translations;

			public NativeArray<float3> positions;

			[BurstCompile]
			public void Execute(int index)
			{
				positions[index] = translations[index].Value;
			}
		}

		private Camera _mainCamera;
		private Mesh _quad;
		private Material _material;

		private ComputeBuffer _argsBuffer;
		ComputeBuffer _meshPropertiesBuffer;
		private Transform _mainCameraTransform;
		private Vector3 _cameraHalfSize;
		private Vector3 _cameraClipPlane;
		private Bounds _cameraBounds;
		private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

		protected override void OnCreate()
		{
			base.OnCreate();
			_mainCamera = Camera.main;
			_quad = GenerateQuad(0.25f);
			_argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (_argsBuffer != null)
			{
				_argsBuffer.Dispose();
			}
			if (_meshPropertiesBuffer != null)
			{
				_meshPropertiesBuffer.Dispose();
			}

		}


		protected override void OnUpdate()
		{
			RefreshCameraBounds();

			EntityQuery projectileQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<ProjectileRenderingData>());
			NativeArray<Translation> translations = projectileQuery.ToComponentDataArray<Translation>(Allocator.Temp);

			int projectileCount = translations.Length;
			if (projectileCount == 0)
			{
				translations.Dispose();
				return;
			}

			// 0 == number of triangle indices, 1 == population
			args[0] = (uint)_quad.GetIndexCount(0);
			args[1] = (uint)projectileCount;
			_argsBuffer.SetData(args);

			if (_meshPropertiesBuffer != null)
			{
				_meshPropertiesBuffer.Release();
			}
			_meshPropertiesBuffer = new ComputeBuffer(projectileCount, sizeof(float) * 3);
			_meshPropertiesBuffer.SetData(translations);
			_material = SharedParticleRenderingConfig.Instance.projectileMaterial;
			_material.SetBuffer("positionBuffer", _meshPropertiesBuffer);

			Graphics.DrawMeshInstancedIndirect(_quad, 0, _material, _cameraBounds, _argsBuffer);
			translations.Dispose();
		}

		private void RefreshCameraBounds()
		{
			if (_mainCamera == null || _mainCameraTransform == null)
			{
				_mainCamera = Camera.main;
				if (_mainCamera != null)
				{
					_mainCameraTransform = _mainCamera.transform;
					_cameraHalfSize = new Vector3(_mainCamera.orthographicSize * 2.0f, _mainCamera.orthographicSize / _mainCamera.aspect * 2.0f, 0.0f);
					_cameraClipPlane = new Vector3(0, 0, _mainCamera.farClipPlane);
				}
				else
				{
					return;
				}
			}
			_cameraBounds.min = _mainCameraTransform.position - _cameraHalfSize;
			_cameraBounds.max = _mainCameraTransform.position + _cameraHalfSize + _cameraClipPlane;
		}

		private Mesh GenerateQuad(float size)
		{
			Mesh quad = new Mesh();
			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(0, 0, 0),
				new Vector3(size, 0, 0),
				new Vector3(0, size, 0),
				new Vector3(size, size, 0)
			};
			quad.SetVertices(vertices);
			int[] tris = new int[6]
			{
				0, 2, 1,
				2, 3, 1
			};
			quad.triangles = tris;
			Vector3[] normals = new Vector3[4]
			{
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward
			};
			quad.normals = normals;
			Vector2[] uv = new Vector2[4]
			{
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(0, 1),
				new Vector2(1, 1)
			};
			quad.uv = uv;
			return quad;
		}
	}
}