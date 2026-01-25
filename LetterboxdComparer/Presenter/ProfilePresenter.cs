using LetterboxdComparer.Data;
using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LetterboxdComparer.Presenter
{
    public class ProfilePresenter : Notifier, IActivatable
    {
        public ProfilePresenter()
        {
            PresenterCollection.Instance.Add(AppView.Profile, this);
            StoredUsers = [];
            LetterboxdUser user = new("Rash", DateTime.Now);
            user.Id = 1;
            LetterboxdUser user2 = new("Rash2", DateTime.Now);
            user2.Id = 2;
            List<LetterboxdUser> users = [ user, user2 ];
            Datastore.Instance.StoreEntities(users);
            Datastore.Instance.RemoveEntity(user);
        }

        public ObservableCollection<LetterboxdUser> StoredUsers { get; set; }

        public void OnActivated()
        {
            List<LetterboxdUser> users = Datastore.Instance.GetEntities<LetterboxdUser>();
            foreach(LetterboxdUser user in users)
                StoredUsers.Add(user);
        }
    }
}