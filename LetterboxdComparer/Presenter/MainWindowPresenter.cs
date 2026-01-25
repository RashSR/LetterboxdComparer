using LetterboxdComparer.ViewRelated;
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
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
                InitiateActivation(); 
            }
        }

        private void InitiateActivation()
        {
            PresenterCollection.Instance.Activate(CurrentView);
        }

        public ICommand ShowStatisticsCommand { get; }
        public ICommand ShowProfileCommand { get; }
        public ICommand ShowMovieStoreCommand { get; }

        #endregion

        public MainWindowPresenter()
        {
            ShowStatisticsCommand = new RelayCommand(_ => CurrentView = AppView.Statistics);
            ShowProfileCommand = new RelayCommand(_ => CurrentView = AppView.Profile);
            ShowMovieStoreCommand = new RelayCommand(_ => CurrentView = AppView.MovieStore);
            // Default view
            CurrentView = AppView.Statistics;
        }
    }
}
