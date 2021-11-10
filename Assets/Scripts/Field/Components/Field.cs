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
        public Entity tile;
        public Vector3 boundA;
        public Vector3 boundB;
        public int granulosity;
    }

    public struct Tile : IComponentData
    {
        public int index;
    }

    public struct NewScale : IComponentData
    {
        public float value;
    }

    namespace Tags
    {

    }
}