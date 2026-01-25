
using System.Collections.Generic;

namespace LetterboxdComparer.Entities
{
    public sealed class LetterboxdMovieStore
    {
        #region Constructor 

        private LetterboxdMovieStore()
        {
        }
        public static LetterboxdMovieStore Instance
        {
            get
            {
                if(instance == null)
                    instance = new LetterboxdMovieStore();

                return instance;
            }
        }

        #endregion

        #region Fields
        private static LetterboxdMovieStore? instance = null;
        private readonly Dictionary<string, LetterboxdMovie> _movieDictionary = new Dictionary<string, LetterboxdMovie>();
        
        public List<LetterboxdMovie> StoredMovies
        {
            get
            {
                return new List<LetterboxdMovie>(_movieDictionary.Values);
            }
        }

        #endregion

        #region Methods
        public LetterboxdMovie CreateOrGetMovie(string movieName, int releaseYear, string uuid)
        {
            _movieDictionary.TryGetValue(uuid, out LetterboxdMovie? existingMovie);
            if(existingMovie != null)
                return existingMovie;
            
            LetterboxdMovie newMovie = new(movieName, releaseYear, uuid);
            _movieDictionary.Add(uuid, newMovie);
            return newMovie;
        }

        public LetterboxdMovie? GetMovieByUUID(string uuid)
        {
            _movieDictionary.TryGetValue(uuid, out LetterboxdMovie? existingMovie);
            return existingMovie;
        }

        public override string ToString()
        {
            return $"LetterboxdMovieStore: {_movieDictionary.Count} movies stored.";
        }

        #endregion
    }
}