using System;

namespace LetterboxdComparer.Entities
{
    public sealed class RssMovieItem
    {
        public string Link { get; init; } = null!;
        public DateTime Published { get; init; }
        public DateTime WatchedDate { get; init; }
        public bool Rewatch { get; init; }
        public string FilmTitle { get; init; } = null!;
        public int FilmYear { get; init; }
        public double? Rating { get; init; }
        public bool Liked { get; init; }
    }

}
