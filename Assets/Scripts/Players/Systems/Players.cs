// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;


namespace Players.Systems
{

    public partial class Players : SystemBase
    {
        private EntityQuery m_qCameraSettings;
        private Entity m_Camera;
        protected override void OnCreate()
        {
            // RequireForUpdate(m_qCameraSettings);
        }
        protected override void OnStartRunning()
        {
        }

        protected override void OnStopRunning()
        {

        }
        protected override void OnUpdate()
        {
         
        }
    }
}