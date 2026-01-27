using LetterboxdComparer.Data;
using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace LetterboxdComparer.Presenter
{
    public class StatisticsPresenter : Notifier
    {
        #region Data

        private LetterboxdUser? _loadedUser;
        public LetterboxdUser? LoadedUser
        {
            get => _loadedUser;
            private set
            {
                _loadedUser = value;
                OnPropertyChanged(nameof(LoadedUser));
                OnPropertyChanged(nameof(MovieCountsPerYear));
            }
        }

        public IEnumerable<KeyValuePair<int, int>>? MovieCountsPerYear => LoadedUser?.GetMovieCountPerReleaseYear();

        #endregion

        public ICommand PickZipCommand { get; }

        public StatisticsPresenter()
        {
            PickZipCommand = new RelayCommand(_ => PickAndLoadZip());
            _loadedUser = null;
        }

        #region ZIP Loading

        private async Task PickAndLoadZip()
        {
            OpenFileDialog dlg = new()
            {
                Title = "Select ZIP File",
                Filter = "ZIP Files (*.zip)|*.zip"
            };

            if(dlg.ShowDialog() != true)
                return;

            string zipPath = dlg.FileName;
            LoadedUser = CreateLetterboxdUserFromZipName(Path.GetFileName(zipPath));
            Debug.WriteLine(LoadedUser);
            if(LoadedUser == null)
                throw new Exception("Could not Load user from ZIP");

            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            ZipFile.ExtractToDirectory(zipPath, tempFolder);
            string[] csvFiles = Directory.GetFiles(tempFolder, "*.csv", System.IO.SearchOption.AllDirectories);

            if(csvFiles.Length == 0)
                return;

            //TODO: Handle all elements in the ZIP
            foreach(string csvFile in csvFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(csvFile);

                switch(fileName)
                {
                    case "watched":
                        LoadedUser.WatchEvents = ExtractEventsFromFile<LetterboxdWatchEvent>(csvFile);
                        break;
                    case "watchlist":
                        LoadedUser.Watchlist = ExtractEventsFromFile<LetterboxdWatchlistEvent>(csvFile);
                        break;
                    case "ratings":
                        LoadedUser.MovieRatings = ExtractEventsFromFile<LetterboxdRateEvent>(csvFile, hasRating: true);
                        break;
                }
            }
            OnPropertyChanged(nameof(MovieCountsPerYear));
            Debug.WriteLine(LoadedUser);
            Debug.WriteLine("Average Rating: " + LoadedUser.GetAverageRating()/2);
            Debug.WriteLine("RateToWatchRatio: " + LoadedUser.GetRateToWatchRatio()*100 + "%");
            Debug.WriteLine(LetterboxdMovieStore.Instance);

            await CheckForRssUpdates(LoadedUser);
        }

        #endregion

        #region Helpers

        private static LetterboxdUser? CreateLetterboxdUserFromZipName(string fileName)
        {
            //letterboxd forbids user names with dashes -> safe to split like this
            string[] parts = fileName.Split('-');
            if(parts.Length != 8)
                throw new ArgumentException("Array must have length 8!");

            string userName = parts[1];
            DateTime exportTime = new(int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]), 0);
            
            LetterboxdUser userToCreate = new(userName, exportTime);
            LetterboxdUser? createdUser = Datastore.Instance.StoreEntity(userToCreate);
            return createdUser;
        }

        private static List<T> ExtractEventsFromFile<T>(string filePath, bool hasRating = false)
        {
            List<T> eventEntries = [];
            using(TextFieldParser parser = new(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                string[] columnNames = parser.ReadFields();

                if(!hasRating && columnNames.Length != 4)
                    throw new InvalidDataException("CSV file has invalid header for watchlist movies!");
                if(hasRating && columnNames.Length != 5 && columnNames[4] != "Rating")
                    throw new InvalidDataException("CSV file has invalid header for rated movies!");
                if(columnNames[0] != "Date" || columnNames[1] != "Name" || columnNames[2] != "Year" || columnNames[3] != "Letterboxd URI")
                     throw new InvalidDataException("CSV file has invalid header");

                while(!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    DateTime addedDate = DateTime.ParseExact(fields[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string movieName = fields[1];
                    int releaseYear = int.Parse(fields[2]);
                    string uuid = new Uri(fields[3]).Segments.Last(); //csv provides the format https://boxd.it/<uuid> -> extract id

                    LetterboxdMovie movie = LetterboxdMovieStore.Instance.CreateOrGetMovie(movieName, releaseYear, uuid);
                    T eventElement;
                    if(hasRating)
                    {
                        int rating = (int)(float.Parse(fields[4]) * 2); //convert star rating from 0.5 steps to full integer steps
                        eventElement = (T)Activator.CreateInstance(typeof(T), addedDate, movie, rating);
                    }
                    else
                        eventElement = (T)Activator.CreateInstance(typeof(T), addedDate, movie);
                    
                    eventEntries.Add(eventElement!);
                }
            }
            return eventEntries;
        }

        private static async Task CheckForRssUpdates(LetterboxdUser user)
        {
            string url = $"https://letterboxd.com/{user.UserName}/rss/";
            HttpClient httpClient = new();
            string xmlString = await httpClient.GetStringAsync(url);
            XDocument rss = XDocument.Parse(xmlString);
            XNamespace letterboxd = "https://letterboxd.com";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace tmdb = "https://www.themoviedb.org/";
            IEnumerable<XElement> items = rss.Descendants("item").Where(i => i.Element("title") != null && i.Element(letterboxd + "filmTitle") != null);
            
            var films = items.Select(item => new
            {
                // RSS
                Title = (string)item.Element("title"),
                Link = (string)item.Element("link"),
                Guid = (string)item.Element("guid"),

                Published = DateTime.TryParse((string)item.Element("pubDate"), out var pub) ? (DateTime?)pub : null,
                DescriptionHtml = (string)item.Element("description"),

                // Letterboxd

                WatchedDate = DateTime.TryParse((string)item.Element(letterboxd + "watchedDate"), out var watched) ? (DateTime?)watched : null,

                Rewatch = ((string)item.Element(letterboxd + "rewatch")) == "Yes",
                FilmTitle = (string)item.Element(letterboxd + "filmTitle"),
                FilmYear = (int?)item.Element(letterboxd + "filmYear"),
                Rating = (double?)item.Element(letterboxd + "memberRating"),
                Liked = ((string)item.Element(letterboxd + "memberLike")) == "Yes",

                // External
                TmdbMovieId = (int?)item.Element(tmdb + "movieId"),

                // Author
                Creator = (string)item.Element(dc + "creator")
            }).ToList();

            foreach (var item in films)
            {
                Debug.WriteLine("=================================");
                Debug.WriteLine($"Title:            {item.Title}");
                Debug.WriteLine($"Film:             {item.FilmTitle} ({item.FilmYear})");
                Debug.WriteLine($"Rating:           {(item.Rating.HasValue ? item.Rating + " ★" : "N/A")}");
                Debug.WriteLine($"Liked:            {(item.Liked ? "Yes" : "No")}");
                Debug.WriteLine($"Rewatch:          {(item.Rewatch ? "Yes" : "No")}");
                Debug.WriteLine($"Watched Date:     {(item.WatchedDate?.ToString("yyyy-MM-dd") ?? "N/A")}");
                Debug.WriteLine($"Published:        {(item.Published?.ToString("yyyy-MM-dd HH:mm") ?? "N/A")}");

                Debug.WriteLine($"Link:             {item.Link}");
                Debug.WriteLine($"GUID:             {item.Guid}");
                Debug.WriteLine($"TMDB Movie ID:    {(item.TmdbMovieId?.ToString() ?? "N/A")}");
                Debug.WriteLine($"Creator:          {item.Creator}");

                Debug.WriteLine("Description (HTML):");
                Debug.WriteLine(item.DescriptionHtml ?? "N/A");

                Debug.WriteLine("=================================\n");
            }

        }
        #endregion
    }
}
