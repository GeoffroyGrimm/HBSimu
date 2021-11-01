// System
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;

namespace Selection.Systems
{
    public partial class Selection : SystemBase
    {
        private float m_zoomRatio;
        private EntityQuery m_qSelectionSettings;
        private Entity m_currentSelection;
        private Entity m_currentHover;
        private float4[] m_colors;

        protected override void OnCreate()
        {
            m_qSelectionSettings = GetEntityQuery(typeof(Camera.Components.CameraSettings));
            RequireForUpdate(m_qSelectionSettings);
        }

        protected override void OnStartRunning()
        {
        }

        protected override void OnUpdate()
        {
            var q_hit = GetEntityQuery(typeof(Components.HitInfo), typeof(Components.Tags.Terrain));
            // Hover
            if (!q_hit.IsEmptyIgnoreFilter)
            {
                var datas = EntityManager.GetComponentData<Components.HitInfo>(q_hit.GetSingletonEntity());
                var hovered = GetEntityQuery(typeof(Components.Tags.Hovered));
                if (!hovered.IsEmptyIgnoreFilter)
                {
                    var hover = hovered.GetSingletonEntity();
                    if (datas.collider != hover)
                    {
                        EntityManager.RemoveComponent<Components.Tags.Hovered>(hover);
                        if (datas.collider != Entity.Null)
                            EntityManager.AddComponent<Components.Tags.Hovered>(datas.collider);
                        UpdatePlayerColors();
                    }
                }
                else if (datas.collider != Entity.Null)
                {
                    EntityManager.AddComponent<Components.Tags.Hovered>(datas.collider);
                    UpdatePlayerColors();
                }
            }
            else
            {
                var hovered = GetEntityQuery(typeof(Components.Tags.Hovered));
                if (hovered.IsEmptyIgnoreFilter)
                {
                    EntityManager.RemoveComponent<Components.Tags.Hovered>(hovered);
                    UpdatePlayerColors();
                }
            }
            // Selection
            if (Mouse.current.leftButton.wasPressedThisFrame) // Show Infos
            {
                if (!q_hit.IsEmptyIgnoreFilter)
                {
                    var datas = EntityManager.GetComponentData<Components.HitInfo>(q_hit.GetSingletonEntity());
                    var selected = GetEntityQuery(typeof(Components.Tags.Selected));
                    if (!selected.IsEmptyIgnoreFilter)
                    {
                        var select = selected.GetSingletonEntity();
                        if (datas.collider != select)
                        {
                            EntityManager.RemoveComponent<Components.Tags.Selected>(select);
                            if (datas.collider != Entity.Null)
                            {
                                EntityManager.AddComponent<Components.Tags.Selected>(datas.collider);
                                var state = EntityManager.GetComponentData<Players.Components.State>(select).value;
                            }
                        }
                    }
                    else if (datas.collider != Entity.Null)
                        EntityManager.AddComponent<Components.Tags.Selected>(datas.collider);
                }
                else
                {
                    var selected = GetEntityQuery(typeof(Components.Tags.Selected));
                    if (!selected.IsEmptyIgnoreFilter)
                        EntityManager.RemoveComponent<Components.Tags.Selected>(selected);
                }
                UpdatePlayerColors();
            }

            // Right button Reset
            if (Mouse.current.rightButton.wasPressedThisFrame) // Stop Draw Tile
            {
                var selected = GetEntityQuery(typeof(Components.Tags.Selected));
                if (selected.IsEmptyIgnoreFilter)
                    EntityManager.RemoveComponent<Components.Tags.Selected>(selected);
                UpdatePlayerColors();
            }
        }

        private void UpdatePlayerColors()
        {

            // var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(_entity);
            // var col = new Color(m_colors[(int)_state].x, m_colors[(int)_state].y, m_colors[(int)_state].z);
            // renderMesh.material.SetColor("_Color", col);
            // EntityManager.SetSharedComponentData(_entity, renderMesh);
        }
    }
}