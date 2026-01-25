using LetterboxdComparer.ViewRelated;
using System;

namespace LetterboxdComparer.Data
{
    public class Datastore
    {
        #region Constructor
        private static readonly Lazy<Datastore> _instance = new(() => new Datastore());
        public static Datastore Instance => _instance.Value;
        private Datastore() { }

        #endregion
    }
}
