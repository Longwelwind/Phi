using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient
{
    [Serializable]
    public class User : IDable
    {
        public const int MIN_NAME_LENGTH = 4;
        public const int MAX_NAME_LENGTH = 32;

        public int id;
        public string name;
        public bool connected;
        public bool inGame;
        public UserPreferences preferences = new UserPreferences();

        public int lastTransactionId = 0;
        public DateTime lastTransactionTime = DateTime.MinValue;

        public int getID()
        {
            return this.id;
        }
    }
}