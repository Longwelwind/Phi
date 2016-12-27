using System;

namespace Verse
{
    /**
     * This class is a copy-pasted version of Verse.Dialog_Confirm present in A15,
     * but removed in A16. Since it does everything it needs to, it is simply
     * copy/pasted here to keep the mod compatible.
     */
    public class DialogChoiceConfig
    {
        public string text;

        public string buttonAText = string.Empty;

        public Action buttonAAction;

        public string buttonBText = string.Empty;

        public Action buttonBAction;
    }
}