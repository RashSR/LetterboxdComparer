using LetterboxdComparer.Entities;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;

namespace LetterboxdComparer
{
    public class MovieStatsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #region Properties

        private LetterboxdUser _loadedUser;
        public LetterboxdUser LoadedUser
        {
            get => _loadedUser;
            private set
            {
                _loadedUser = value;
                OnPropertyChanged(nameof(LoadedUser));
                OnPropertyChanged(nameof(MovieCountsPerYear));
            }
        }

        #endregion

        public IEnumerable<KeyValuePair<int, int>> MovieCountsPerYear => LoadedUser?.GetMovieCountPerReleaseYear();

        #region Commands
        public ICommand PickZipCommand { get; }

        public MovieStatsViewModel()
        {
            PickZipCommand = new RelayCommand(_ => PickAndLoadZip());
        }

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
            LoadedUser = CreateLetterboxdUserFromZipName(Path.GetFileName(zipPath));
            Debug.WriteLine(LoadedUser);

            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            ZipFile.ExtractToDirectory(zipPath, tempFolder);
            string[] csvFiles = Directory.GetFiles(tempFolder, "*.csv", System.IO.SearchOption.AllDirectories);

            if(csvFiles.Length == 0)
                return;

            foreach(string csvFile in csvFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(csvFile);

                switch (fileName)
                {
                    case "watched":
                        LoadedUser.WatchEvents = ExtractEventsFromFile<LetterboxdWatchEvent>(csvFile);
                        break;
                    case "watchlist":
                        LoadedUser.Watchlist = ExtractEventsFromFile<LetterboxdWatchlistEvent>(csvFile);
                        break;
                }
            }
            OnPropertyChanged(nameof(MovieCountsPerYear));
            Debug.WriteLine(LoadedUser);
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

        private List<T> ExtractEventsFromFile<T>(string filePath)
        {
            List<T> eventEntries = new List<T>();
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                string[] columnNames = parser.ReadFields();
                if (columnNames.Length != 4 || columnNames[0] != "Date" || columnNames[1] != "Name" || columnNames[2] != "Year" || columnNames[3] != "Letterboxd URI")
                    throw new InvalidDataException("CSV file has invalid header for watchlist movies!");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    DateTime addedDate = DateTime.ParseExact(fields[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string movieName = fields[1];
                    int releaseYear = int.Parse(fields[2]);
                    string uuid = new Uri(fields[3]).Segments.Last(); //csv provides the format https://boxd.it/<uuid> -> extract id

                    LetterboxdMovie movie = LetterboxdMovieStore.Instance.CreateMovie(movieName, releaseYear, uuid);
                    T eventElement = (T)Activator.CreateInstance(typeof(T), addedDate, movie);
                    eventEntries.Add(eventElement);
                }
            }
            return eventEntries;
        }

        #endregion
    }
}
