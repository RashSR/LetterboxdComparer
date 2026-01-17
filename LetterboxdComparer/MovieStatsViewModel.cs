using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;

namespace LetterboxdComparer
{
    public class MovieStatsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MoviePerYear> YearCounts { get; } = new ObservableCollection<MoviePerYear>();
        public DataView DataTableView { get; private set; }

        public ICommand PickCsvCommand { get; }
        public ICommand PickZipCommand { get; }

        public MovieStatsViewModel()
        {
            PickCsvCommand = new RelayCommand(_ => PickAndLoadCsv());
            PickZipCommand = new RelayCommand(_ => PickAndLoadZip());
        }

        private void PickAndLoadCsv()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select CSV File",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true) return;

            DataTable dt = LoadCsv(dlg.FileName);
            DataTableView = dt.DefaultView;
            OnPropertyChanged(nameof(DataTableView));

            UpdateYearCounts(dt);
        }

        private DataTable LoadCsv(string filePath)
        {
            var dt = new DataTable();

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                // Headers
                if (!parser.EndOfData)
                {
                    var headers = parser.ReadFields();
                    foreach (var h in headers) dt.Columns.Add(h);
                }

                // Rows
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    dt.Rows.Add(fields);
                }
            }

            if (!dt.Columns.Contains("Year"))
                throw new InvalidDataException("CSV requires column: 'Year'.");

            return dt;
        }

        private void UpdateYearCounts(DataTable dt)
        {
            YearCounts.Clear();

            var counts = dt.AsEnumerable()
                .Where(r => !string.IsNullOrWhiteSpace(r["Year"].ToString()))
                .GroupBy(r => r["Year"].ToString())
                .OrderBy(g => g.Key)
                .Select(g => new MoviePerYear { Year = g.Key, Count = g.Count() });

            foreach (var item in counts)
                YearCounts.Add(item);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void PickAndLoadZip()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select ZIP File",
                Filter = "ZIP Files (*.zip)|*.zip"
            };

            if(dlg.ShowDialog() != true) 
                return;

            string zipPath = dlg.FileName;
            ExtractInforamtionFromZipName(Path.GetFileName(zipPath));

            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            // Extract the zip file
            ZipFile.ExtractToDirectory(zipPath, tempFolder);

            // Get all CSV files inside
            var csvFiles = Directory.GetFiles(tempFolder, "*.csv", System.IO.SearchOption.AllDirectories);

            if(csvFiles.Length == 0)
                return;

        }

        private void ExtractInforamtionFromZipName(string fileName)
        {
            //letterboxd forbids user names with dashes -> safe to split like this
            string[] parts = fileName.Split('-');
            if (parts.Length != 8)
                throw new ArgumentException("Array must have length 8!");

            string userName = parts[1];
            Debug.WriteLine("User Name: " + userName);

            DateTime exportTime = new DateTime(int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]), 0);
            Debug.WriteLine("Export Time: " + exportTime.ToString("yyyy-MM-dd HH:mm"));
        }

    }
}
