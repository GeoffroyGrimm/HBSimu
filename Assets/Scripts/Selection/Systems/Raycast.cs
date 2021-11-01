// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.Profiling;
using UnityEngine.InputSystem;

namespace Selection.Systems
{
    // [UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class Raycast : SystemBase
    {
        private Entity m_raycastTerrain;
        private Entity m_raycastTile;
        private EntityQuery m_qCamera;
        private static UnityEngine.Camera m_camera;

        private int m_TerrainLayer = LayerMask.NameToLayer("Terrain");
        private int m_TileLayer = LayerMask.NameToLayer("Tile");
        private BuildPhysicsWorld m_buildPhysicWorld;

        private static readonly Components.HitInfo s_nullRaycast = new Components.HitInfo
        {
            collider = Entity.Null,
            hitPosition = default
        };
        protected override void OnCreate()
        {
            m_qCamera = GetEntityQuery(typeof(Camera.Components.CameraSettings));
            m_buildPhysicWorld = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            m_raycastTerrain = EntityManager.CreateEntity();
            m_raycastTile = EntityManager.CreateEntity();
            EntityManager.AddComponent<Components.Tags.Raycast>(m_raycastTerrain);
            EntityManager.AddComponent<Components.Tags.Terrain>(m_raycastTerrain);
            EntityManager.AddComponent<Components.Tags.Raycast>(m_raycastTile);
            EntityManager.AddComponent<Components.Tags.Tile>(m_raycastTile);
            EntityManager.SetName(m_raycastTerrain, "raycastTerrain");
            EntityManager.SetName(m_raycastTile, "raycastTile");

            RequireForUpdate(m_qCamera);
        }

        protected override void OnStartRunning()
        {
            m_camera = UnityEngine.Camera.main;
        }
        protected override void OnUpdate()
        {
            // terrain
            var hitTerrain = Raycast.GetRaycastHitInfos(
                500,
                m_buildPhysicWorld,
                new CollisionFilter
                {
                    GroupIndex = 0,
                    BelongsTo = (uint)(1 << m_TerrainLayer), // is a mask
                    CollidesWith = (uint)(1 << m_TerrainLayer) // is a mask
                });

            if (hitTerrain.Entity == Entity.Null)
            {
                if (EntityManager.HasComponent<Components.HitInfo>(m_raycastTerrain))
                    EntityManager.RemoveComponent<Components.HitInfo>(m_raycastTerrain);
            }
            else if (EntityManager.HasComponent<Components.HitInfo>(m_raycastTerrain))
                EntityManager.SetComponentData(m_raycastTerrain, new Components.HitInfo
                {
                    collider = hitTerrain.Entity,
                    hitPosition = hitTerrain.Position
                });
            else
                EntityManager.AddComponentData(m_raycastTerrain, new Components.HitInfo
                {
                    collider = hitTerrain.Entity,
                    hitPosition = hitTerrain.Position
                });


            // Tile
            var hitTile = Raycast.GetRaycastHitInfos(
                500,
                m_buildPhysicWorld,
                new CollisionFilter
                {
                    GroupIndex = 0,
                    BelongsTo = (uint)(1 << m_TileLayer), // is a mask
                    CollidesWith = (uint)(1 << m_TileLayer) // is a mask
                });

            if (hitTile.Entity == Entity.Null)
            {
                if (EntityManager.HasComponent<Components.HitInfo>(m_raycastTile))
                    EntityManager.RemoveComponent<Components.HitInfo>(m_raycastTile);
            }
            else if (EntityManager.HasComponent<Components.HitInfo>(m_raycastTile))
                EntityManager.SetComponentData(m_raycastTile, new Components.HitInfo
                {
                    collider = hitTile.Entity,
                    hitPosition = hitTile.Position
                });
            else
                EntityManager.AddComponentData(m_raycastTile, new Components.HitInfo
                {
                    collider = hitTile.Entity,
                    hitPosition = hitTile.Position
                });
        }

        public static Unity.Physics.RaycastHit GetRaycastHitInfos(
            float _maxDistance,
            BuildPhysicsWorld _physicsWorld,
            CollisionFilter _collisionFilter)
        {
            var mousePos = Mouse.current.position.ReadValue();
            var hit = new Unity.Physics.RaycastHit();
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            var isInScreen = screenRect.Contains(mousePos);

            if (m_camera == null || !isInScreen)
                return hit;

            var unityRay = m_camera.ScreenPointToRay(mousePos);
            var dir = unityRay.direction;
            var rayInput = new RaycastInput
            {
                Start = unityRay.origin,
                End = unityRay.origin + dir * _maxDistance,
                Filter = _collisionFilter
            };
            _physicsWorld.PhysicsWorld.CastRay(rayInput, out hit);

            return hit;
        }
    }
}