using LetterboxdComparer.Data;
using System;
using System.IO;
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
            
            try
            {
                InitDatastore();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not initialize DB: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private static void InitDatastore()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string dataDir = Path.Combine(projectRoot, "Data");
            string dbPath = Path.Combine(dataDir, "letterboxd_comparer.db");
            string connectionString = $"Data Source={dbPath}";
            SQLiteHandler sqliteHandler = new(connectionString);
            Datastore.Initialize(sqliteHandler);
        }

    }
}
