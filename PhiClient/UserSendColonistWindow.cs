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
        }

        public override void DoWindowContents(Rect inRect)
        {
            float beginY = 0f;
            foreach (Pawn colonist in Find.MapPawns.FreeColonists)
            {
                string label = colonist.Label;
                float height = Text.CalcHeight(label, inRect.width);

                Rect rowArea = new Rect(inRect.x, inRect.y + beginY, inRect.width, height);

                if (Widgets.ButtonText(rowArea, label, false))
                {
                    OnColonistClick(colonist);
                }

                beginY += height;
            }
        }

        public void OnColonistClick(Pawn pawn)
        {
            PhiClient client = PhiClient.instance;
            client.SendPacket(new SendColonistPacket { userTo=user, realmPawn= client.realmData.ToRealmPawn(pawn) });
        }
    }
}
