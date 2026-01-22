using LetterboxdComparer.ViewRelated;
using System.Windows.Input;

namespace LetterboxdComparer
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
            }
        }

        public ICommand ShowStatisticsCommand { get; }
        public ICommand ShowDetailsCommand { get; }

        #endregion

        public MainWindowPresenter()
        {
            ShowStatisticsCommand = new RelayCommand(_ => CurrentView = AppView.Statistics);
            ShowDetailsCommand = new RelayCommand(_ => CurrentView = AppView.Details);
            // Default view
            CurrentView = AppView.Statistics;
        }
    }
}
