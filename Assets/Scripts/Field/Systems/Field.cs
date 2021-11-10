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
        private NativeArray<Entity> m_tiles;
        private int m_height;
        private int m_width;
        private int m_maxTiles;
        private int m_index;
        private int m_range;
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
            var granul = m_settings.granulosity / 10f;
            m_height = (int)m_settings.boundB.z - (int)m_settings.boundA.z;
            m_width = (int)m_settings.boundB.x - (int)m_settings.boundA.x;
            m_tiles = new NativeArray<Entity>((int)((m_height / granul + 1) * (m_width / granul + 1)), Allocator.Persistent);

            var index = 0;
            for (int j = 0; j < m_height / granul + 1; j++)
                for (int i = 0; i < m_width / granul + 1; i++)
                {
                    var newTile = EntityManager.Instantiate(m_settings.tile);
                    m_tiles[index] = newTile;
                    EntityManager.SetComponentData(newTile, new Translation() { Value = new Vector3(m_settings.boundA.x + i * granul, 0, m_settings.boundA.z + j * granul) });
                    EntityManager.AddComponentData(newTile, new Components.Tile() { index = index });
                    EntityManager.RemoveComponent<Rotation>(newTile);
                    index++;
                }
            EntityManager.SetEnabled(m_settings.tile, false);

            var qPlayers = GetEntityQuery(typeof(Players.Components.PlayersSettings));
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
                    Debug.Log(m_index);
                    // Cadre
                    int ligne = (int)math.floor(index / m_height);
                    var colonne = MathMod(index, m_width);
                    var startIndex = (int)math.floor(math.clamp(colonne - m_range, 0, int.MaxValue) + math.clamp(ligne - m_range, 0, int.MaxValue) * this.m_width);

                    int offsetX = colonne - m_range < 0 ? colonne - m_range : 0;
                    int offsetY = ligne - m_range < 0 ? ligne - m_range : 0;

                    var ligneS = math.floor(startIndex / m_height);
                    var colonneS = MathMod(startIndex, m_width);

                    var width = (int)math.clamp(m_range * 2, 0, m_width - colonne + m_range - 1) + offsetX;
                    var height = (int)math.clamp(m_range * 2, 0, m_height - ligne + m_range - 1) + offsetY;

                    var LeftBot = startIndex;
                    var RightBot = startIndex + width;
                    var LeftTop = startIndex + height * m_height;
                    var RightTop = startIndex + width + height * m_width;

                    var max = height * width;

                    //Debug
                    for (int i = 0; i < m_tiles.Length; i++)
                        EntityManager.AddComponentData(m_tiles[i], new Scale() { Value = .1f });
                    //Debug

                    // Force
                    // var hWidth = width * .5f;
                    // var hHeight = height * .5f;
                    // var c_maxsqrt = math.sqrt(m_range * m_range);
                    // for (int j = 0; j < height; j++)
                    // {
                    //     var li = j * m_dimension;
                    //     for (int i = 0; i < width; i++)
                    //     {
                    //         var dex = (int)((startIndex + i) + li);

                    //         float ii = math.abs(i - hWidth);
                    //         float jj = math.abs(j - hHeight);

                    //         float dist = math.sqrt(ii * ii + jj * jj);
                    //         dist /= c_maxsqrt;
                    //         dist = 1 - dist;
                    //         dist = math.clamp(dist, 0, 1);

                    // EntityManager.AddComponentData(m_tiles[dex], new Components.NewScale() { value = jj });
                    //         EntityManager.AddComponentData(m_tiles[dex], new Components.NewScale() { value = 0.1f + .3f * dist });
                    //     }
                    // }

                    //Debug
                    EntityManager.AddComponentData(m_tiles[index], new Scale() { Value = .7f });
                    EntityManager.AddComponentData(m_tiles[LeftBot], new Scale() { Value = .7f });
                    EntityManager.AddComponentData(m_tiles[RightBot], new Scale() { Value = .7f });
                    EntityManager.AddComponentData(m_tiles[LeftTop], new Scale() { Value = .7f });
                    EntityManager.AddComponentData(m_tiles[RightTop], new Scale() { Value = .7f });
                    //Debug

                    // var commandBuffer = m_barrier.CreateCommandBuffer().AsParallelWriter();
                    // Entities
                    // .WithAll<Components.Tile>()
                    // .WithNone<Components.NewScale>()
                    // .ForEach((Entity _entity,
                    // int entityInQueryIndex)
                    // =>
                    // {
                    //     commandBuffer.SetComponent(entityInQueryIndex, _entity, new Scale() { Value = .1f });
                    // }).ScheduleParallel();

                    // Entities
                    // .ForEach((Entity _entity,
                    // int entityInQueryIndex,
                    // Components.NewScale _newScale)
                    // =>
                    // {
                    //     commandBuffer.SetComponent(entityInQueryIndex, _entity, new Scale() { Value = _newScale.value });
                    //     commandBuffer.RemoveComponent<Components.NewScale>(entityInQueryIndex, _entity);
                    // }).ScheduleParallel();

                    // m_barrier.AddJobHandleForProducer(Dependency);
                }
            }
            // Debug
        }
        protected override void OnStopRunning()
        {
            m_tiles.Dispose();
        }

        static int MathMod(int a, int b) => (math.abs(a * b) + a) % b;
        static int MathMod(float a, float b) => (math.abs((int)a * (int)b) + (int)a) % (int)b;

    }
}