using LetterboxdComparer.Presenter;
using System.Windows.Controls;

namespace LetterboxdComparer.Views
{
    /// <summary>
    /// Interaction logic for LetterboxdMovieStoreView.xaml
    /// </summary>
    public partial class LetterboxdMovieStoreView : UserControl
    {
        public LetterboxdMovieStoreView()
        {
            InitializeComponent();
            DataContext = new LetterboxdMovieStorePresenter();
        }
    }
}
