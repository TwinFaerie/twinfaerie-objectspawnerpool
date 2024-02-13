using System;
using System.Collections.Generic;
using UnityEngine;

namespace TF.ObjectSpawnerPool
{
    using Object = UnityEngine.Object;
    
    public class Spawner
    {
        private readonly Transform parent;
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> pool = new();
        private readonly Dictionary<GameObject, GameObject> activeItemReferenceList = new();

        private readonly Action<GameObject, GameObject> onCreateObject;
        private readonly Action<GameObject> onGetObject;
        private readonly Action<GameObject> onReleaseObject;
        private readonly Action<GameObject> onDestroyObject;

        public Spawner(Transform parent, Action<GameObject, GameObject> onCreateObject = null, Action<GameObject> onGetObject = null, Action<GameObject> onReleaseObject = null, Action<GameObject> onDestroyObject = null)
        {
            this.parent = parent;

            this.onCreateObject = onCreateObject;
            this.onGetObject = onGetObject;
            this.onReleaseObject = onReleaseObject;
            this.onDestroyObject = onDestroyObject;
        }

        public ObjectPool<GameObject> Allocate(GameObject key, int initial = 0)
        {
            if (!pool.TryGetValue(key, out var itemPool))
            { 
                itemPool = new ObjectPool<GameObject>(() => CreateObjectInternal(key), OnGetObjectInternal, OnReleaseObjectInternal, OnDestroyObjectInternal);
                pool.Add(key, itemPool);
            }

            itemPool.Allocate(initial)?.ForEach(item => item.SetActive(false));
            
            return itemPool;
        }
        
        public GameObject Get(GameObject key, Transform itemParent = null)
        {
            GameObject item;
            
            if (pool.TryGetValue(key, out var value))
            {
                item = value.Get();
            }
            else
            {
                var newPool = Allocate(key);
                item = newPool.Get();
            }

            if (itemParent is not null)
            {
                item.transform.parent = itemParent;
            }
            
            activeItemReferenceList.Add(item, key);
            return item;
        }

        public void Return(GameObject item)
        {
            if (activeItemReferenceList.TryGetValue(item, out var key))
            {
                Return(key, item);
            }
        }
        
        public void Return(GameObject key, GameObject item)
        {
            if (pool.TryGetValue(key, out var value))
            {
                value.Release(item);
                activeItemReferenceList.Remove(item);
            }
        }
        
        // Object Pool Callbacks

        private GameObject CreateObjectInternal(GameObject key)
        {
            var item = Object.Instantiate(key, parent);
            onCreateObject?.Invoke(key, item);

            return item;
        }

        private void OnGetObjectInternal(GameObject item)
        {
            item.SetActive(true);
            onGetObject?.Invoke(item);
        }
        
        private void OnReleaseObjectInternal(GameObject item)
        {
            item.SetActive(false);
            item.transform.SetParent(parent);
            
            onReleaseObject?.Invoke(item);
        }
        
        private void OnDestroyObjectInternal(GameObject item)
        {
            Object.Destroy(item);
            onDestroyObject?.Invoke(item);
        }
    }
}