using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using System.Collections.ObjectModel;

namespace LetterboxdComparer.Presenter
{
    public class LetterboxdMovieStorePresenter : Notifier
    {
        public ObservableCollection<LetterboxdMovie> Movies { get; }
        public LetterboxdMovieStorePresenter()
        {
            Movies = new ObservableCollection<LetterboxdMovie>(LetterboxdMovieStore.Instance.StoredMovies);
        }
    }
}
