using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    class ThingIconWidget : Displayable
    {
        public Thing thing;

        public ThingIconWidget(Thing thing)
        {
            this.thing = thing;
        }

        public override void Draw(Rect inRect)
        {
            Widgets.ThingIcon(inRect, thing);
        }
    }
}
