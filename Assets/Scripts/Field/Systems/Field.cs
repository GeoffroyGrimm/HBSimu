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
using UnityEngine.Profiling;


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
        private float m_range;
        private Components.FieldSettings m_settings;
        private EndSimulationEntityCommandBufferSystem m_barrier;

        protected override void OnCreate()
        {
            m_qFieldSettings = GetEntityQuery(typeof(Components.FieldSettings));
            m_qSelection = GetEntityQuery(typeof(Selection.Components.HitInfo), typeof(Selection.Components.Tags.Tile));
            m_barrier = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

            RequireForUpdate(m_qFieldSettings);
        }
        protected override void OnStartRunning()
        {
            m_settings = EntityManager.GetComponentData<Components.FieldSettings>(m_qFieldSettings.GetSingletonEntity());
            m_dimension = m_settings.dimensions;
            m_maxTiles = (int)math.pow(m_dimension, 2);

            m_tiles = new NativeArray<Entity>(m_maxTiles, Allocator.Persistent);
            m_field = new NativeArray<float>(m_maxTiles, Allocator.Persistent);
            m_range = m_settings.range;
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

                    var maxRange = m_range * 2 + 1;
                    var max = math.pow(maxRange, 2);
                    var startIndex = math.clamp(index - (m_range + m_range * m_dimension), 0, m_maxTiles);

                    var commandBuffer = m_barrier.CreateCommandBuffer().AsParallelWriter();

                    List<Entity> tilesToChange = new List<Entity>();
                    List<float> tilesToValue = new List<float>();
                    for (int i = 0; i < max; i++)
                    {
                        var lin = (int)(i / maxRange) - m_range;
                        var col = MathMod(i, maxRange) - m_range;
                        var j = (int)(startIndex + i + (int)(i / maxRange) * (m_dimension - maxRange));
                        if (j >= m_maxTiles)
                            continue;

                        float dist = 1 - (math.abs(lin) + math.abs(col)) / maxRange;
                        EntityManager.AddComponentData(m_tiles[j], new Components.NewScale() { value = .3f * dist });
                    }

                    var jobHandle1 = Entities
                    .WithAll<Components.Tile>()
                    .WithNone<Components.NewScale>()
                    .ForEach((Entity _entity,
                    int entityInQueryIndex)
                    =>
                    {
                        commandBuffer.SetComponent(entityInQueryIndex, _entity, new Scale() { Value = .1f });
                    }).ScheduleParallel(Dependency);

                    var jobHandle2 = Entities
                     .ForEach((Entity _entity,
                     int entityInQueryIndex,
                     Components.NewScale _newScale)
                     =>
                     {
                         commandBuffer.SetComponent(entityInQueryIndex, _entity, new Scale() { Value = _newScale.value });
                         commandBuffer.RemoveComponent<Components.NewScale>(entityInQueryIndex, _entity);
                     }).ScheduleParallel(jobHandle1);

                    m_barrier.AddJobHandleForProducer(jobHandle2);
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
}