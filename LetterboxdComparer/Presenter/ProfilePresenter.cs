using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using System;
using System.Collections.ObjectModel;

namespace LetterboxdComparer.Presenter
{
    public class ProfilePresenter : Notifier, IActivatable
    {
        public ProfilePresenter()
        {
            PresenterCollection.Instance.Add(AppView.Profile, this);
            StoredUsers = [];
        }

        public ObservableCollection<LetterboxdUser> StoredUsers { get; set; }

        public void OnActivated()
        {
            LetterboxdUser user = new("ExampleUser", DateTime.Now);
            StoredUsers.Add(user);
        }
    }

}
