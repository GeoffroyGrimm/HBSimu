// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

namespace Camera.Components
{
    [DisallowMultipleComponent]

    public class CameraProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float2 zoomBounds;
        public void Convert(
                   Entity _entity,
                   EntityManager _dstManager,
                   GameObjectConversionSystem _conversionSystem)
        {
            var settings = new CameraSettings()
            {
                zoomBounds = zoomBounds
            };
            _dstManager.AddComponentObject(_entity, settings);
        }
    }
}