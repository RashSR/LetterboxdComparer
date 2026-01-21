using System.Windows.Controls;

namespace LetterboxdComparer
{
    /// <summary>
    /// Interaction logic for StatisticsView.xaml
    /// </summary>
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            InitializeComponent();
            this.DataContext = new StatisticsPresenter();
        }
    }
}
