
namespace LetterboxdComparer.Entities
{
    public class LetterboxdMovie
    {
        #region Constructor

        //Should only be called by LetterboxdMovieStore
        internal LetterboxdMovie(string name, int releaseYear, string uuid)
        {
            _name = name;
            _releaseYear = releaseYear;
            _uuid = uuid;
        }

        #endregion
        
        #region Properties
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private int _releaseYear;
        public int ReleaseYear
        {
            get { return _releaseYear; }
            set { _releaseYear = value; }
        }
        private string _uuid;
        public string Uuid
        {
            get { return _uuid; }
            set { _uuid = value; }
        }

        public string LetterboxdUrl
        {
            get { return $"https://boxd.it/{Uuid}"; }
        }
        
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{Uuid}: {Name} ({ReleaseYear})";
        }

        #endregion
    }
}
