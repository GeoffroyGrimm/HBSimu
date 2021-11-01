// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

namespace Selection.Components
{
    [DisallowMultipleComponent]

    public class SelectionProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject camera;
        private EntityManager m_em;
        public void Convert(
                   Entity _entity,
                   EntityManager _dstManager,
                   GameObjectConversionSystem _conversionSystem)
        {
            var settings = new SelectionSettings()
            {
            };
            _dstManager.AddComponentObject(_entity, settings);
        }
    }
}