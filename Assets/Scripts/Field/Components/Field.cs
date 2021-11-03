// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

namespace Field.Components
{
    public struct FieldSettings : IComponentData
    {
        public int dimensions;
        public Entity tile;
        public float range;
    }

    public struct Tile : IComponentData
    {
        public int index;

    }

    namespace Tags
    {
    }
}