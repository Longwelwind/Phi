using PhiClient.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient
{
    class UserSendColonistWindow : Window
    {
        User user;

        public UserSendColonistWindow(User user)
        {
            this.user = user;
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.closeOnEscapeKey = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            ListContainer cont = new ListContainer();

            cont.Add(new TextWidget("ATTENTION: This feature is highly experimental. Only use it if you're playing with a save you could potentially corrupt."));

            float beginY = 0f;
            foreach (Pawn colonist in Find.MapPawns.FreeColonists)
            {
                cont.Add(new ButtonWidget(colonist.Label, () => OnColonistClick(colonist), false));
            }

            cont.Draw(inRect);
        }

        public void OnColonistClick(Pawn pawn)
        {
            PhiClient client = PhiClient.instance;
            client.SendPawn(user, pawn);
        }
    }
}
