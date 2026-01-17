using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace LetterboxdComparer
{
    public class MovieStatsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MoviePerYear> YearCounts { get; } = new ObservableCollection<MoviePerYear>();
        public DataView DataTableView { get; private set; }

        public ICommand PickCsvCommand { get; }

        public MovieStatsViewModel()
        {
            PickCsvCommand = new RelayCommand(_ => PickAndLoadCsv());
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
    }
}
