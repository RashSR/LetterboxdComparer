using LetterboxdComparer.Entities;
using System;
using System.Collections.Generic;

namespace LetterboxdComparer.Entities
{
    public class LetterboxdUser
    {
        #region Constructor 
        public LetterboxdUser(string userName, DateTime exportTime)
        {
            _userName = userName;
            _exportTime = exportTime;
            _watchEvents = new List<LetterboxdWatchEvent>();
            _watchlist = new List<LetterboxdWatchlistEvent>();
        }

        #endregion

        #region Properties
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private DateTime _exportTime;
        public DateTime ExportTime
        {
            get { return _exportTime; }
            set { _exportTime = value; }
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

        public override string ToString()
        {
            return $"{UserName} (Exported on {ExportTime:yyyy-MM-dd HH:mm}), WatchEvents: {WatchEvents.Count}, WatchList: {Watchlist.Count}";
        }
        #endregion

    }
}
