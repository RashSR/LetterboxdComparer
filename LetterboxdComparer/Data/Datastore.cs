using LetterboxdComparer.Entities;
using System;
using System.Collections.Generic;

namespace LetterboxdComparer.Data
{
    public class Datastore
    {
        #region Constructor
        private static Datastore? _instance;
        private static readonly object _lock = new();

        private Datastore(ICRUDHandler crudHandler)
        {
            _crudHandler = crudHandler;
        }

        public static Datastore Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("Datastore not initialized. Call Initialize() first.");
                return _instance;
            }
        }

        public static void Initialize(ICRUDHandler crudHandler)
        {
            lock (_lock)
            {
                if (_instance != null)
                    throw new InvalidOperationException("Datastore already initialized!");

                _instance = new Datastore(crudHandler);
            }
        }

        #endregion

        #region Fields
        private readonly Dictionary<Type, List<BaseEntity>> _cachedEntities = [];
        private readonly ICRUDHandler _crudHandler;

        #endregion

        #region Methods

        #region Add
        public bool StoreEntity<T>(T entity) where T : BaseEntity
        {
            return StoreEntities([entity]);
        }

        public bool StoreEntities<T>(List<T> entities) where T : BaseEntity
        {
            Type type = typeof(T);

            //Ensure the cache has a list for this type
            if (!_cachedEntities.ContainsKey(type))
                _cachedEntities[type] = [];

            //TODO: look up which entities are needed to be created
            _cachedEntities.TryGetValue(type, out List<BaseEntity>? value);

            if (_crudHandler.Create(entities))
            {
                if(value != null)
                {
                    value.AddRange(entities);
                    return true;
                }
            }
            
            return false;
        }

        #endregion

        #region Get
        public T? GetEntity<T>(int id) where T : BaseEntity
        {
            Type type = typeof(T);
            if (_cachedEntities.TryGetValue(type, out List<BaseEntity>? value))
            {
                foreach(BaseEntity entity in value)
                {
                    if (entity.Id == id)
                        return (T)entity;
                }
            }

            return null;
        }
        public List<T> GetEntities<T>() where T : BaseEntity
        {
            Type type = typeof(T);
            if (_cachedEntities.TryGetValue(type, out List<BaseEntity>? value))
            {
                List<T> typedList = value.ConvertAll(item => (T)item);
                return typedList;
            }

            return [];
        }

        #endregion

        #region Remove
        public void RemoveEntity<T>(T entity) where T : BaseEntity
        {
            Type type = typeof(T);
            if (_cachedEntities.TryGetValue(type, out List<BaseEntity>? value))
                value.Remove(entity);
        }

        public void RemoveEntities<T>(List<T> entities) where T : BaseEntity
        {
            Type type = typeof(T);
            if (_cachedEntities.TryGetValue(type, out List<BaseEntity>? value))
            {
                if(_crudHandler.Delete(entities))
                    foreach(T entity in entities)
                        value.Remove(entity);
                else
                    throw new Exception("Failed to delete entities from persistent storage.");
            }
        }

        #endregion

        #endregion
    }
}
