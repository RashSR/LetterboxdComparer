using LetterboxdComparer.Presenter;
using System.Windows.Controls;

namespace LetterboxdComparer.Views
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            DataContext = new ProfilePresenter();
            InitializeComponent();
        }
    }
}
