using System;
using System.Collections.Generic;

namespace LetterboxdComparer
{
    public class LetterboxdUser
    {
        #region Constructor 
        public LetterboxdUser(string userName, DateTime exportTime)
        {
            _userName = userName;
            _exportTime = exportTime;
            _watchEvents = new List<LetterboxdWatchEvent>();
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

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{UserName} (Exported on {ExportTime:yyyy-MM-dd HH:mm}), WatchEvents: {WatchEvents.Count}";
        }
        #endregion

    }
}
