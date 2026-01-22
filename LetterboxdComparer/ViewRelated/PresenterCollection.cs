using System;
using System.Collections.Generic;

namespace LetterboxdComparer.ViewRelated
{
    public class PresenterCollection
    {
        private readonly Dictionary<AppView, IActivatable> _presenters = new Dictionary<AppView, IActivatable>();
        private static readonly Lazy<PresenterCollection> _instance = new Lazy<PresenterCollection>(() => new PresenterCollection());

        public static PresenterCollection Instance => _instance.Value;

        private PresenterCollection() { }

        public void Add(AppView view, IActivatable presenter)
        {
            if (!_presenters.ContainsKey(view))
                _presenters.Add(view, presenter);
        }

        public IActivatable Get(AppView view)
        {
            return _presenters.TryGetValue(view, out var presenter) ? presenter : null;
        }

        public void Activate(AppView view)
        {
            var presenter = Get(view);
            presenter?.OnActivated();
        }
    }
}
