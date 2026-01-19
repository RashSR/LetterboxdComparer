
using System;

namespace LetterboxdComparer.Entities
{
    public class LetterboxdWatchlistEvent
    {
        public LetterboxdWatchlistEvent(DateTime addedDate, LetterboxdMovie movie)
        {
            _addedDate = addedDate;
            _movie = movie;
        }
        private DateTime _addedDate;
        public DateTime AddedDate
        {
            get { return _addedDate; }
            set { _addedDate = value; }
        }
        private LetterboxdMovie _movie;
        public LetterboxdMovie Movie
        {
            get { return _movie; }
            set { _movie = value; }
        }

        public override string ToString()
        {
            return $"{AddedDate:yyyy-MM-dd}: {Movie}";
        }
    }
}
