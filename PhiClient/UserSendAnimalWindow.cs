using PhiClient.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PhiClient
{
    class UserSendAnimalWindow : Window
    {
        User user;
        Vector2 scrollPosition = Vector2.zero;

        public UserSendAnimalWindow(User user)
        {
            this.user = user;
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.closeOnEscapeKey = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            ListContainer mainCont = new ListContainer();
            mainCont.spaceBetween = ListContainer.SPACE;

            mainCont.Add(new TextWidget("ATTENTION: This feature is highly experimental. Only use it if you're playing with a save you could potentially corrupt."));

            //Adds a scrollable container for trading animals.
            ListContainer columnCont = new ListContainer();
            columnCont.drawAlternateBackground = true;
            mainCont.Add(new ScrollContainer(columnCont, scrollPosition, (s) => { scrollPosition = s; }));

            float beginY = 0f; //Unused
            foreach (Pawn animal in Find.VisibleMap.mapPawns.AllPawns.Where(p => p.RaceProps.Animal && p.Faction == Faction.OfPlayer))
            {
                ListContainer rowCont = new ListContainer(ListFlow.ROW);
                rowCont.spaceBetween = ListContainer.SPACE;
                columnCont.Add(new HeightContainer(rowCont, 40f));
                rowCont.Add(new ButtonWidget(animal.Label, () => OnAnimalClick(animal), false));
            }

            mainCont.Draw(inRect);
        }

        public void OnAnimalClick(Pawn pawn)
        {
            PhiClient client = PhiClient.instance;
            client.SendAnimal(user, pawn);
        }
    }
}
