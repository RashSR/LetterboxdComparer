using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

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

        private ICommand _openUrlCommand;
        public ICommand OpenUrlCommand
        {
            get
            {
                if(_openUrlCommand == null)
                {
                    _openUrlCommand = new RelayCommand(param =>
                    {
                        if(param is string url)
                            HyperlinkJump(url);
                    });
                }
                return _openUrlCommand;
            }
        }

        private void HyperlinkJump(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Failed to open URL: {url}", ex);
            }
        }
    }
}
