
using System.Collections.Generic;

namespace LetterboxdComparer.Entities
{
    public sealed class LetterboxdMovieStore
    {
        private static LetterboxdMovieStore instance = null;
        private readonly Dictionary<string, LetterboxdMovie> _movieDictionary = new Dictionary<string, LetterboxdMovie>();
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

        public LetterboxdMovie CreateMovie(string movieName, int releaseYear, string uuid)
        {
            _movieDictionary.TryGetValue(uuid, out LetterboxdMovie existingMovie);
            if(existingMovie != null)
                return existingMovie;
            
            LetterboxdMovie newMovie = new LetterboxdMovie(movieName, releaseYear, uuid);
            _movieDictionary.Add(uuid, newMovie);
            return newMovie;
        }

        public LetterboxdMovie GetMovieByUUID(string uuid)
        {
            _movieDictionary.TryGetValue(uuid, out LetterboxdMovie existingMovie);
            return existingMovie;
        }

    }



}
