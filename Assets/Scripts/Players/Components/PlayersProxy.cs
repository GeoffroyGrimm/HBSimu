// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

namespace Players.Components
{
    [DisallowMultipleComponent]

    public class PlayersProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        private EntityManager m_em;
        public E_role role;
        public bool IsTeamA;

        public void Convert(
                   Entity _entity,
                   EntityManager _dstManager,
                   GameObjectConversionSystem _conversionSystem)
        {
            var settings = new PlayersSettings()
            {
                role = role
            };
            _dstManager.AddComponentData(_entity, settings);
            _dstManager.AddComponentData(_entity, new Components.State() { value = E_states.None });

            if (IsTeamA)
                _dstManager.AddComponent<Components.Tags.IsTeamA>(_entity);
            else
                _dstManager.AddComponent<Components.Tags.IsTeamB>(_entity);
        }
    }
}