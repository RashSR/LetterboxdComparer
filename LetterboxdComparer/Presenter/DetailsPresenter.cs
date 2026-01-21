using LetterboxdComparer.Entities;
using System.ComponentModel;

namespace LetterboxdComparer
{
    public class DetailsPresenter : INotifyPropertyChanged
    {
        // Handle watchlist or other details
        public LetterboxdUser LoadedUser { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
