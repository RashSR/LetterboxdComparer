using LetterboxdComparer.Presenter;
using System.Windows.Controls;

namespace LetterboxdComparer
{
    /// <summary>
    /// Interaction logic for DetailsView.xaml
    /// </summary>
    public partial class DetailsView : UserControl
    {
        public DetailsView()
        {
            DataContext = new DetailsPresenter();
            InitializeComponent();
        }
    }
}
