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

            for (int i = 0; i < m_maxTiles; i++)
            {
                var newTile = EntityManager.Instantiate(settings.tile);
                var origin = EntityManager.GetComponentData<Translation>(settings.tile).Value;
                EntityManager.SetComponentData(newTile, new Translation() { Value = origin + new float3(10 * (int)(i % settings.dimensions), 0, 10 * (int)(i / settings.dimensions)) });
                var id = i;
                EntityManager.AddComponentData(newTile, new Components.Tile() { index = id });
                m_tiles[i] = newTile;
            }
        }

        protected override void OnUpdate()
        {
            // Debug
            if (!m_qSelection.IsEmptyIgnoreFilter)
            {
                var tile = EntityManager.GetComponentData<Selection.Components.HitInfo>(m_qSelection.GetSingletonEntity()).collider;
                var index = EntityManager.GetComponentData<Components.Tile>(tile).index;
                EntityManager.SetComponentData(tile, new Scale() { Value = 1 });
                for (int i = 0; i < m_maxTiles; i++)
                    EntityManager.SetComponentData(m_tiles[i], new Scale() { Value = .2f });

                var distance = 3;
                var max = math.pow(distance * 2, 2);
                var startIndex = math.clamp(index - (distance + distance * m_dimension), 0, m_maxTiles);
                for (int i = 0; i < max; i++)
                {
                    if (i >= m_maxTiles)
                        continue;
                    Debug.Log(i + "  " + (distance * 2) + "  " + (int)(i % (distance * 2)));
                    var j = startIndex + (i + i * (int)(i / (distance * 2)));
                    EntityManager.SetComponentData(m_tiles[j], new Scale() { Value = .5f });
                }

                EntityManager.SetComponentData(m_tiles[index], new Scale() { Value = 1 });
                EntityManager.SetComponentData(m_tiles[startIndex], new Scale() { Value = 1 });
            }
        }
        protected override void OnStopRunning()
        {
            m_tiles.Dispose();
            m_field.Dispose();
        }
    }
}