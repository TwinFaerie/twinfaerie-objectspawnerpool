using System;
using System.Collections.Generic;
using System.Linq;

namespace TF.ObjectSpawnerPool
{
    public class ObjectPool<T> : IDisposable where T : class
    {
        private readonly List<PooledItem<T>> list = new();

        private readonly Func<T> create;
        private readonly Action<T> get;
        private readonly Action<T> release;
        private readonly Action<T> destroy;

        public ObjectPool(Func<T> create, Action<T> get = null, Action<T> release = null, Action<T> destroy = null)
        {
            this.create = create;
            this.get = get;
            this.release = release;
            this.destroy = destroy;
        }

        public List<T> Allocate(int count)
        {
            if (count <= 0)
            { return null; }

            List<T> allocatedItemList = new();

            for (int i = 0; i < count; i++)
            {
                var newItem = CreateNewPooledItem();
                newItem.IsUsed = false;
                
                allocatedItemList.Add(newItem.Item);
            }

            return allocatedItemList;
        }

        private PooledItem<T> CreateNewPooledItem()
        {
            var newItem = new PooledItem<T>
            {
                Item = create(),
                OnDispose = destroy
            };
            
            list.Add(newItem);

            return newItem;
        }
        
        public T Get()
        {
            var foundItem = list.FirstOrDefault(poolObject => !poolObject.IsUsed);
            
            if (foundItem is null)
            {
                foundItem = CreateNewPooledItem();
            }
            
            foundItem.IsUsed = true;
            get(foundItem.Item);

            return foundItem.Item;
        }

        public void Release(T item)
        {
            var foundItem = list.FirstOrDefault(poolItem => poolItem.Item == item);
            
            if (foundItem is null)
            { return; }
            
            foundItem.IsUsed = false;
            release(foundItem.Item);
        }

        public void Dispose()
        {
            list.ForEach(poolItem => poolItem.Dispose());
            list.Clear();
        }
    }
}