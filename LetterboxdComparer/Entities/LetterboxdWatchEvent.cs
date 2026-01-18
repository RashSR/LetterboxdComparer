
using System;

namespace LetterboxdComparer
{
    public class LetterboxdWatchEvent
    {
        #region Constructor
        public LetterboxdWatchEvent(DateTime watchDate, LetterboxdMovie movie)
        {
            _watchDate = watchDate;
            _movie = movie;
        }

        #endregion

        #region Properties
        private DateTime _watchDate;
        public DateTime WatchDate
        {
            get { return _watchDate; }
            set { _watchDate = value; }
        }
        private LetterboxdMovie _movie;
        public LetterboxdMovie Movie
        {
            get { return _movie; }
            set { _movie = value; }
        }

        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{WatchDate:yyyy-MM-dd}: {Movie}";
        }

        #endregion
    }
}
