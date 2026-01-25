using System;
using System.Collections.Generic;

namespace LetterboxdComparer.ViewRelated
{
    public class PresenterCollection
    {
        #region Constructor
        private static readonly Lazy<PresenterCollection> _instance = new(() => new PresenterCollection());

        public static PresenterCollection Instance => _instance.Value;

        private PresenterCollection() { }

        #endregion

        #region Fields
        private readonly Dictionary<AppView, IActivatable> _presenters = [];
        
        #endregion

        #region Methods

        public void Add(AppView view, IActivatable presenter)
        {
            _presenters.TryAdd(view, presenter);
        }

        public IActivatable? Get(AppView view)
        {
            return _presenters.TryGetValue(view, out var presenter) ? presenter : null;
        }

        public void Activate(AppView view)
        {
            var presenter = Get(view);
            presenter?.OnActivated();
        }

        #endregion
    }
}
