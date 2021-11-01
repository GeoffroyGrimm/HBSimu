// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
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

        protected override void OnCreate()
        {
            m_qFieldSettings = GetEntityQuery(typeof(Components.FieldSettings));
            m_qSelection = GetEntityQuery(typeof(Selection.Components.HitInfo), typeof(Selection.Components.Tags.Tile));

            RequireForUpdate(m_qFieldSettings);
        }
        protected override void OnStartRunning()
        {
            var settings = EntityManager.GetComponentData<Components.FieldSettings>(m_qFieldSettings.GetSingletonEntity());
            m_dimension = settings.dimensions;
            m_maxTiles = (int)math.pow(m_dimension, 2);

            m_tiles = new NativeArray<Entity>(m_maxTiles, Allocator.Persistent);
            m_field = new NativeArray<float>(m_maxTiles, Allocator.Persistent);
            var offset = 20f / (m_dimension / 10);

            for (int i = 0; i < m_maxTiles; i++)
            {
                var newTile = EntityManager.Instantiate(settings.tile);
                var origin = EntityManager.GetComponentData<Translation>(settings.tile).Value;
                EntityManager.SetComponentData(newTile, new Translation() { Value = origin + new float3(offset * (int)(i % settings.dimensions), 0, offset * (int)(i / settings.dimensions)) });
                var id = i;
                EntityManager.AddComponentData(newTile, new Components.Tile() { index = id });
                m_tiles[i] = newTile;
            }
            EntityManager.SetEnabled(settings.tile, false);
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
                        EntityManager.SetComponentData(m_tiles[i], new Scale() { Value = .2f });

                    var range = 4;
                    var maxRange = range * 2 + 1;
                    var max = math.pow(maxRange, 2);
                    var startIndex = math.clamp(index - (range + range * m_dimension), 0, m_maxTiles);
                    for (int i = 0; i < max; i++)
                    {
                        var j = startIndex + i + (int)(i / maxRange) * (m_dimension - maxRange);
                        if (j >= m_maxTiles)
                            continue;
                        EntityManager.SetComponentData(m_tiles[j], new Scale() { Value = .3f });
                    }
                    EntityManager.SetComponentData(m_tiles[index], new Scale() { Value = .5f });
                }
            }
            // Debug
        }
        protected override void OnStopRunning()
        {
            m_tiles.Dispose();
            m_field.Dispose();
        }
    }
}