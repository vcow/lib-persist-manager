using System;
using Base.PersistManager;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sample
{
    [Serializable]
    public class PersistentObject: IPersistent<PersistentObject>

    {
        [SerializeField] public string _data;

        public string PersistentId => "id";

        public void Restore<T1>(T1 data) where T1 : IPersistent<PersistentObject>
        {
            var p  = data as PersistentObject;
            Assert.IsNotNull(p);
            _data = p._data;
        }
    }
}