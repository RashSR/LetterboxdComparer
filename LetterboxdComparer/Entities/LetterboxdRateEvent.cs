using System;

namespace LetterboxdComparer.Entities
{
    public class LetterboxdRateEvent
    {
        public LetterboxdRateEvent(DateTime rateDate, LetterboxdMovie movie, int rating)
        {
            if(rating < 1 || rating > 10)
                throw new ArgumentOutOfRangeException("Rating must be between 0 and 10!");
            if(movie == null)
                throw new ArgumentNullException("Movie cannot be null!");

            _rateDate = rateDate;
            _movie = movie;
            _rating = rating;
        }

        #region Properties
        private DateTime _rateDate;
        public DateTime RateDate
        {
            get { return _rateDate; }
            set { _rateDate = value; }
        }

        private int _rating;
        public int Rating
        {
            get { return _rating; }
            set { _rating = value; }
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
            return $"{RateDate:yyyy-MM-dd}: {Rating} stars - {Movie}";
        }
        #endregion
    }
}
