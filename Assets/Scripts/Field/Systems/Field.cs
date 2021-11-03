// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;
using UnityEngine.InputSystem;


namespace Field.Systems
{
    public class Field : SystemBase
    {
        private EntityQuery m_qFieldSettings;
        private EntityQuery m_qSelection;

        private Texture m_texture;
        private NativeArray<float> m_field;
        private NativeArray<Entity> m_tiles;
        private int m_dimension;
        private int m_maxTiles;
        private int m_index;
        private Components.FieldSettings m_settings;

        protected override void OnCreate()
        {
            m_qFieldSettings = GetEntityQuery(typeof(Components.FieldSettings));
            m_qSelection = GetEntityQuery(typeof(Selection.Components.HitInfo), typeof(Selection.Components.Tags.Tile));

            RequireForUpdate(m_qFieldSettings);
        }
        protected override void OnStartRunning()
        {
            m_settings = EntityManager.GetComponentData<Components.FieldSettings>(m_qFieldSettings.GetSingletonEntity());
            m_dimension = m_settings.dimensions;
            m_maxTiles = (int)math.pow(m_dimension, 2);

            m_tiles = new NativeArray<Entity>(m_maxTiles, Allocator.Persistent);
            m_field = new NativeArray<float>(m_maxTiles, Allocator.Persistent);
            var offset = 20f / (m_dimension / 10);

            for (int i = 0; i < m_maxTiles; i++)
            {
                var newTile = EntityManager.Instantiate(m_settings.tile);
                var origin = EntityManager.GetComponentData<Translation>(m_settings.tile).Value;
                EntityManager.SetComponentData(newTile, new Translation() { Value = origin + new float3(offset * (int)(i % m_settings.dimensions), 0, offset * (int)(i / m_settings.dimensions)) });
                var id = i;
                EntityManager.AddComponentData(newTile, new Components.Tile() { index = id });
                m_tiles[i] = newTile;
            }
            EntityManager.SetEnabled(m_settings.tile, false);
        }

        protected override void OnUpdate()
        {
            // Debug
            if (!m_qSelection.IsEmptyIgnoreFilter)
            {
                var tile = EntityManager.GetComponentData<Selection.Components.HitInfo>(m_qSelection.GetSingletonEntity()).collider;
                var index = EntityManager.GetComponentData<Components.Tile>(tile).index;
                if (m_index != index)
                {
                    m_index = index;
                    EntityManager.SetComponentData(tile, new Scale() { Value = 1 });
                    for (int i = 0; i < m_maxTiles; i++)
                        EntityManager.SetComponentData(m_tiles[i], new Scale() { Value = .5f });

                    float range = m_settings.range;
                    var maxRange = range * 2 + 1;
                    var max = math.pow(maxRange, 2);

                    var startIndex = math.clamp(index - (range + range * m_dimension), 0, m_maxTiles);
                    for (int i = 0; i < max; i++)
                    {
                        var lin = (int)(i / maxRange) - range;
                        var col = MathMod(i, maxRange) - range;
                        var j = (int)(startIndex + i + (int)(i / maxRange) * (m_dimension - maxRange));
                        if (j >= m_maxTiles)
                            continue;

                        float dist = (math.abs(lin) + math.abs(col)) / maxRange;
                        EntityManager.SetComponentData(m_tiles[j], new Scale() { Value = .5f * dist });
                    }
                }
            }
            // Debug
        }
        protected override void OnStopRunning()
        {
            m_tiles.Dispose();
            m_field.Dispose();
        }

        static int MathMod(int a, int b) => (math.abs(a * b) + a) % b;
        static int MathMod(float a, float b) => (math.abs((int)a * (int)b) + (int)a) % (int)b;

    }

    // JobSection
    public class FieldJob : JobComponentSystem
    {
        EntityQuery m_qField;
        EntityCommandBufferSystem m_bufferSystem;


        protected override void OnCreate()
        {
            m_bufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }
        protected override void OnStartRunning()
        {
        }

        protected override void OnStopRunning()
        {
        }

        protected override JobHandle OnUpdate(JobHandle _inputDependencies)
        {
            var jobHandle = SchedulexxxJob(
                    disposeArrayJobHandle,
                    cursorEntity
                    );
            return jobHandle;
        }


// JOBS
        public JobHandle SchedulexxxJob(
            JobHandle _dependencies,
            Entity _cursorEntity)
        {
            var jobHandle = new xxxJob
            {
                CommandBuffer = m_bufferSystem.CreateCommandBuffer().AsParallelWriter(),
                HiddenFromEntity = GetComponentDataFromEntity<Unity.Rendering.DisableRendering>(true),
                CursorTrackersFromEntity = GetComponentDataFromEntity<Components.CursorTracker>(true),
                Cursor = _cursorEntity
            }.Schedule(this, _dependencies);


            m_bufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
// JBOS DEF
        public struct xxxJob : IJobForEachWithEntity_ECCCC<Components.FollowCursor, Translation, Rotation, LocalToWorld>
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;
            [ReadOnly] public ComponentDataFromEntity<Unity.Rendering.DisableRendering> HiddenFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Components.CursorTracker> CursorTrackersFromEntity;
            [ReadOnly] public Entity Cursor;

            public void Execute(
                Entity entity,
                int index,
                [ReadOnly] ref Components.FollowCursor _fc,
                ref Translation _translation,
                ref Rotation _rotation,
                [ReadOnly] ref LocalToWorld _localtoworld)
            {
                if (CursorTrackersFromEntity[Cursor].IsValid)
                {
                    if (HiddenFromEntity.HasComponent(entity))
                        CommandBuffer.RemoveComponent<Unity.Rendering.DisableRendering>(index, entity);
                    _translation = new Translation { Value = CursorTrackersFromEntity[Cursor].position };
                    _rotation.Value = math.mul(
                        math.fromToRotation(
                            _localtoworld.Up,
                            CursorTrackersFromEntity[Cursor].normalHitPoint),
                            _rotation.Value
                        );
                    _translation.Value = CursorTrackersFromEntity[Cursor].position + (_localtoworld.Up * (_fc.heightAdjustment/2 + _fc.heightOffset));
                }
                else
                {
                    if (!HiddenFromEntity.HasComponent(entity))
                       CommandBuffer.AddComponent<Unity.Rendering.DisableRendering>(index, entity, default(Unity.Rendering.DisableRendering));
                    _rotation.Value = quaternion.identity;
                }
            }
        }


    }

}