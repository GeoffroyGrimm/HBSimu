// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using Unity.Entities;

namespace Selection.Components
{
    public struct HitInfo : IComponentData
    {
        public Entity collider;
        public float3 hitPosition;
    }

    public struct SelectionSettings : IComponentData { }

    namespace Tags
    {
        public struct Selected : IComponentData { }
        public struct Hovered : IComponentData { }
        public struct Raycast : IComponentData { }
        public struct Tile : IComponentData { }
        public struct Terrain : IComponentData { }
    }
}