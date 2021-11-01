// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace Camera.Systems
{

    public partial class Camera : SystemBase
    {
        private float2 c_zoomBounds;
        private float m_zoomRatio;
        private EntityQuery m_qCameraSettings;
        private Entity m_Camera;
        private Transform m_TCamera;
        protected override void OnCreate()
        {
            m_qCameraSettings = GetEntityQuery(typeof(Components.CameraSettings));
            RequireForUpdate(m_qCameraSettings);
        }
        protected override void OnStartRunning()
        {
            m_Camera = m_qCameraSettings.GetSingletonEntity();
            m_TCamera = EntityManager.GetComponentObject<Transform>(m_Camera);
            c_zoomBounds = EntityManager.GetComponentObject<Components.CameraSettings>(m_Camera).zoomBounds;
        }

        protected override void OnStopRunning()
        {

        }
        protected override void OnUpdate()
        {
            var pos = m_TCamera.position;
            m_zoomRatio = 0.01f + pos.y / c_zoomBounds.y;
            if (Keyboard.current.wKey.isPressed)
                pos += Vector3.forward * m_zoomRatio;
            if (Keyboard.current.sKey.isPressed)
                pos += Vector3.back * m_zoomRatio;

            if (Keyboard.current.aKey.isPressed)
                pos += Vector3.left * m_zoomRatio;
            if (Keyboard.current.dKey.isPressed)
                pos += Vector3.right * m_zoomRatio;

            if (Keyboard.current.fKey.isPressed)
                pos += Vector3.up * m_zoomRatio;
            if (Keyboard.current.rKey.isPressed)
                pos += Vector3.down * m_zoomRatio;

            pos.y = math.clamp(pos.y, c_zoomBounds.x, c_zoomBounds.y);
            m_TCamera.position = pos;
        }
    }
}