using CommunityCoreLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace PhiClient
{
    class ServerMainMenuButton : MainMenu
    {
        public override void ClickAction()
        {
            Find.WindowStack.Add(new ServerMainMenuWindow());
        }
    }
}
