using LetterboxdComparer.ViewRelated;
using System.Diagnostics;
using System.Windows.Input;

namespace LetterboxdComparer.Presenter
{
    public class MainWindowPresenter : Notifier
    {
        #region Navigation

        private AppView _currentView = AppView.Statistics;
        public AppView CurrentView
        {
            get => _currentView;
            set
            {
                Debug.WriteLine(value);
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand ShowStatisticsCommand { get; }
        public ICommand ShowDetailsCommand { get; }
        public ICommand ShowMovieStoreCommand { get; }

        #endregion

        public MainWindowPresenter()
        {
            ShowStatisticsCommand = new RelayCommand(_ => CurrentView = AppView.Statistics);
            ShowDetailsCommand = new RelayCommand(_ => CurrentView = AppView.Details);
            ShowMovieStoreCommand = new RelayCommand(_ => CurrentView = AppView.MovieStore);
            // Default view
            CurrentView = AppView.Statistics;
        }
    }
}
