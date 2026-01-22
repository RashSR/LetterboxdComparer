using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using System.Collections.ObjectModel;

namespace LetterboxdComparer.Presenter
{
    public class LetterboxdMovieStorePresenter : Notifier, IActivatable
    {
        public ObservableCollection<LetterboxdMovie> Movies { get; }
        public LetterboxdMovieStorePresenter()
        {
            PresenterCollection.Instance.Add(AppView.MovieStore, this);
            Movies = new ObservableCollection<LetterboxdMovie>(LetterboxdMovieStore.Instance.StoredMovies);
        }

        public void OnActivated()
        {
            LetterboxdMovie lm = new LetterboxdMovie("Example Movie", 2024, "example-uuid");
            Movies.Add(lm);
            OnPropertyChanged(nameof(Movies));
        }
    }
}
