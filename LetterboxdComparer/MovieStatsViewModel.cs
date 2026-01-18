using LetterboxdComparer.Entities;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
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
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select CSV File",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if(dlg.ShowDialog() != true)
                return;

            DataTable dt = LoadCsv(dlg.FileName);
            DataTableView = dt.DefaultView;
            OnPropertyChanged(nameof(DataTableView));

            UpdateYearCounts(dt);
        }

        private DataTable LoadCsv(string filePath)
        {
            DataTable dt = new DataTable();

            using(TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                //Columns
                if(!parser.EndOfData)
                {
                    string[] columnNames = parser.ReadFields();
                    foreach (var h in columnNames) 
                        dt.Columns.Add(h);
                }

                //Rows
                while(!parser.EndOfData)
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
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select ZIP File",
                Filter = "ZIP Files (*.zip)|*.zip"
            };

            if(dlg.ShowDialog() != true) 
                return;

            string zipPath = dlg.FileName;
            LetterboxdUser user = CreateLetterboxdUserFromZipName(Path.GetFileName(zipPath));
            Debug.WriteLine(user);

            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            // Extract the zip file
            ZipFile.ExtractToDirectory(zipPath, tempFolder);

            // Get all CSV files inside
            string[] csvFiles = Directory.GetFiles(tempFolder, "*.csv", System.IO.SearchOption.AllDirectories);

            if(csvFiles.Length == 0)
                return;

            foreach(string csvFile in csvFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(csvFile);
                if(fileName == "watched")
                    user.WatchEvents = ExtractWatchEventsFromFile(csvFile);
            }
            Debug.WriteLine(user);
        }

        private LetterboxdUser CreateLetterboxdUserFromZipName(string fileName)
        {
            //letterboxd forbids user names with dashes -> safe to split like this
            string[] parts = fileName.Split('-');
            if (parts.Length != 8)
                throw new ArgumentException("Array must have length 8!");

            string userName = parts[1];
            DateTime exportTime = new DateTime(int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]), 0);
            return new LetterboxdUser(userName, exportTime);
        }

        private List<LetterboxdWatchEvent> ExtractWatchEventsFromFile(string filePath)
        {
            List<LetterboxdWatchEvent> watchEvents = new List<LetterboxdWatchEvent>();

            using(TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                string[] columnNames = parser.ReadFields();
                if(columnNames.Length != 4 || columnNames[0] != "Date" || columnNames[1] != "Name" || columnNames[2] != "Year" || columnNames[3] != "Letterboxd URI")
                    throw new InvalidDataException("CSV file has invalid header for watched movies!");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    DateTime watchDate = DateTime.ParseExact(fields[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string movieName = fields[1];
                    int releaseYear = int.Parse(fields[2]);
                    string uuid = new Uri(fields[3]).Segments.Last(); //csv provides the format https://boxd.it/<uuid> -> extract id

                    LetterboxdMovie movie = LetterboxdMovieStore.Instance.CreateMovie(movieName, releaseYear, uuid);
                    LetterboxdWatchEvent watchEvent = new LetterboxdWatchEvent(watchDate, movie);
                    watchEvents.Add(watchEvent);
                }
            }

            return watchEvents;
        }
    }
}
