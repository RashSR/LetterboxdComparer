using System;
using System.Collections.Generic;

namespace LetterboxdComparer.Entities
{
    public class LetterboxdUser : BaseEntity
    {
        #region Constructor 
        public LetterboxdUser(string userName, DateTime exportDate)
        {
            _userName = userName;
            _exportDate = exportDate;
            _rssExports = [];
            _watchEvents = [];
            _watchlist = [];
            _movieRatings = [];
        }

        #endregion

        #region Properties
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private DateTime _exportDate;
        public DateTime ExportDate
        {
            get { return _exportDate; }
            set { _exportDate = value; }
        }

        private List<LetterboxdWatchEvent> _watchEvents;
        public List<LetterboxdWatchEvent> WatchEvents
        {
            get { return _watchEvents; }
            set { _watchEvents = value; }
        }

        public List<LetterboxdWatchlistEvent> _watchlist;
        public List<LetterboxdWatchlistEvent> Watchlist
        {
            get { return _watchlist; }
            set { _watchlist = value; }
        }

        private List<LetterboxdRateEvent> _movieRatings;
        public List<LetterboxdRateEvent> MovieRatings
        {
            get { return _movieRatings; }
            set { _movieRatings = value; }
        }

        private List<DateTime> _rssExports;
        public List<DateTime> RssExports
        {
            get { return _rssExports; }
            set { _rssExports = value; }
        }
        #endregion

        #region Methods

        public SortedDictionary<int, int> GetMovieCountPerReleaseYear()
        {
            //TODO: add a toggle to add the german cinema release year
            SortedDictionary<int, int> moviesPerYear = new SortedDictionary<int, int>();
            foreach (LetterboxdWatchEvent watchEvent in WatchEvents)
            {
                if(moviesPerYear.ContainsKey(watchEvent.Movie.ReleaseYear))
                    moviesPerYear[watchEvent.Movie.ReleaseYear]++;
                else
                    moviesPerYear[watchEvent.Movie.ReleaseYear] = 1;
            }

            return moviesPerYear;
        }

        public float GetAverageRating()
        {
            if(MovieRatings.Count == 0)
                return 0.0f;

            int totalRating = 0;
            foreach(LetterboxdRateEvent rateEvent in MovieRatings)
                totalRating += rateEvent.Rating;

            return (float)totalRating / MovieRatings.Count;
        }

        public float GetRateToWatchRatio()
        {
            if(WatchEvents.Count == 0)
                return 0.0f;

            return (float)MovieRatings.Count / WatchEvents.Count;
        }

        public override string ToString()
        {
            return $"{UserName} (Exported on {ExportDate:yyyy-MM-dd HH:mm}), WatchEvents: {WatchEvents.Count}, Ratings: {MovieRatings.Count}, WatchList: {Watchlist.Count}";
        }
        #endregion

    }
}
