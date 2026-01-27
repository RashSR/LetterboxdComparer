using LetterboxdComparer.Data;
using LetterboxdComparer.Entities;
using LetterboxdComparer.ViewRelated;
using Microsoft.Playwright;
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
            Debug.WriteLine("After Loading basic info from ZIP: " + LoadedUser);

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
            Debug.WriteLine("After Loading Events from ZIP: " + LoadedUser);
            Debug.WriteLine("Average Rating: " + LoadedUser.GetAverageRating()/2);
            Debug.WriteLine("RateToWatchRatio: " + LoadedUser.GetRateToWatchRatio()*100 + "%");
            Debug.WriteLine(LetterboxdMovieStore.Instance);

            
            await CheckForRssUpdates(LoadedUser);
            OnPropertyChanged(nameof(MovieCountsPerYear));
            Debug.WriteLine("After RSS Update:" + LoadedUser);
            Debug.WriteLine("Average Rating: " + LoadedUser.GetAverageRating() / 2);
            Debug.WriteLine("RateToWatchRatio: " + LoadedUser.GetRateToWatchRatio() * 100 + "%");
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
            XDocument rss = await LoadRssInformation(user);

            XNamespace letterboxd = "https://letterboxd.com";
            IEnumerable<XElement> loadedUnloggedMovies = rss.Descendants("item")
                .Where(i => i.Element("title") != null &&
                            i.Element(letterboxd + "filmTitle") != null)
                .Where(i =>
                {
                    if (DateTime.TryParse((string)i.Element("pubDate")!, out var pub))
                        return pub > user.ExportDate;

                    return false;
                });

            List<RssMovieItem> newMoviesInDiary = [.. loadedUnloggedMovies.Select(item => new RssMovieItem
            {
                Link = (string)item.Element("link")!,
                Published = DateTime.Parse((string)item.Element("pubDate")!),
                WatchedDate = DateTime.Parse((string)item.Element(letterboxd + "watchedDate")!),
                Rewatch = ((string)item.Element(letterboxd + "rewatch"))! == "Yes",
                FilmTitle = (string)item.Element(letterboxd + "filmTitle")!,
                FilmYear = (int)item.Element(letterboxd + "filmYear")!,
                Rating = (double?)item.Element(letterboxd + "memberRating"),
                Liked = ((string)item.Element(letterboxd + "memberLike"))! == "Yes",
            })];

            await LoadLetterboxdMovieIdFromPage(newMoviesInDiary, user);
        }

        private static async Task<XDocument> LoadRssInformation(LetterboxdUser user)
        {
            HttpClient httpClient = new();
            string url = $"https://letterboxd.com/{user.UserName}/rss/";
            string xmlString = await httpClient.GetStringAsync(url);
            return XDocument.Parse(xmlString);
        }

        private static async Task LoadLetterboxdMovieIdFromPage(List<RssMovieItem> newMoviesInDiary, LetterboxdUser user)
        {
            IPage page = await InitializeWebPage();

            foreach(RssMovieItem movie in newMoviesInDiary)
            {
                try
                {
                    await page.GotoAsync(movie.Link, new()
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 30_000
                    });

                    string? filmId = await page.EvaluateAsync<string?>(@"
                        () => {
                            if (window.analytic_params && window.analytic_params.film_id) {
                                return window.analytic_params.film_id;
                            }
                            return null;
                        }
                    ");

                    LetterboxdMovie newMovie = LetterboxdMovieStore.Instance.CreateOrGetMovie(movie.FilmTitle, movie.FilmYear, filmId!);
                    LetterboxdWatchEvent watchEvent = new(movie.WatchedDate, newMovie);
                    user.WatchEvents.Add(watchEvent);
                    if(movie.Rating != null)
                    {
                        LetterboxdRateEvent rateEvent = new(movie.WatchedDate, newMovie, (int)(movie.Rating * 2));
                        user.MovieRatings.Add(rateEvent);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to fetch film ID for {movie.FilmTitle}: {ex.Message}");
                }
                //anti-bot friendliness -> await Task.Delay(500);
            }

            await CloseBrowserPage(page);
        }

        private static async Task<IPage> InitializeWebPage()
        {
            IPlaywright playwright = await Playwright.CreateAsync();
            IBrowser browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });
            IBrowserContext context = await browser.NewContextAsync();
            return await context.NewPageAsync();
        }

        private static async Task CloseBrowserPage(IPage page)
        {
            await page.CloseAsync();
            await page.Context.CloseAsync();
            await page.Context.Browser!.CloseAsync();
        }
        #endregion
    }
}
