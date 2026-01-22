using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using System.Collections.ObjectModel;

namespace LetterboxdComparer.Presenter
{
    public class LetterboxdMovieStorePresenter : Notifier, IActivatable
    {
        public ObservableCollection<LetterboxdMovie> Movies { get; set; }
        public LetterboxdMovieStorePresenter()
        {
            PresenterCollection.Instance.Add(AppView.MovieStore, this);
        }

        public void OnActivated()
        {
            Movies = new ObservableCollection<LetterboxdMovie>(LetterboxdMovieStore.Instance.StoredMovies);
            OnPropertyChanged(nameof(Movies));
        }
    }
}
