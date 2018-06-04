using System;

namespace PhiClient
{
    [Serializable]
    public class UserPreferences
    {
        public bool receiveItems = true;
        public bool receiveColonists = false;
        public bool receiveAnimals = false;
    }
}
