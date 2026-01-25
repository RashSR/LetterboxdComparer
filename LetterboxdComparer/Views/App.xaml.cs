using LetterboxdComparer.Data;
using System.Windows;

namespace LetterboxdComparer.Views
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Datastore.Initialize(null);
        }
    }
}
