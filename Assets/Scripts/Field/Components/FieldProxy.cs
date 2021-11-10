// System
using System.Collections;
using System.Collections.Generic;

// Unity
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Field.Components
{
    [DisallowMultipleComponent]

    public class FieldProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject goTile;
        public Transform boundA;
        public Transform boundB;
        public int granulosity;
        private BlobAssetStore m_blobAssetOwner;
        private void OnEnable() => m_blobAssetOwner = new BlobAssetStore();
        private void OnDestroy() => m_blobAssetOwner.Dispose();

        public void Convert(
                   Entity _entity,
                   EntityManager _dstManager,
                   GameObjectConversionSystem _conversionSystem)
        {
            var tile = CreateEntityPrefab(goTile);
            _dstManager.AddComponent<Components.Tile>(tile);
            _dstManager.AddComponent<Static>(tile);
            _dstManager.AddComponentData(tile, new Scale() { Value = .5f });
            _dstManager.SetComponentData(tile, new Translation() { Value = goTile.transform.position });
            var settings = new FieldSettings()
            {
                tile = tile,
                boundA = boundA.position,
                boundB = boundB.position,
                granulosity = granulosity,
            };
            _dstManager.AddComponentData(_entity, settings);
            goTile.SetActive(false);
        }

        private Entity CreateEntityPrefab(GameObject _object) => ConvertPrefabToEntityMesh(_object);

        private Entity ConvertPrefabToEntityMesh(GameObject _object)
        {
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, m_blobAssetOwner);
            settings.ConversionFlags |= GameObjectConversionUtility.ConversionFlags.AssignName;
            return GameObjectConversionUtility.ConvertGameObjectHierarchy(_object, settings);
        }

    }
}