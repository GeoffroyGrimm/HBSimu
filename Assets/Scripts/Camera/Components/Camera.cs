// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using Unity.Entities;

namespace Camera.Components
{
    public class CameraSettings : IComponentData
    {
        public float2 zoomBounds;
    }
}