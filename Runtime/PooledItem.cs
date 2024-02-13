using System;

namespace TF.ObjectSpawnerPool
{
    public class PooledItem<T> : IDisposable where T : class
    {
        public bool IsUsed = false;
        public T Item;

        public Action<T> OnDispose;
        
        public void Dispose()
        {
            OnDispose(Item);
        }
    }
}

