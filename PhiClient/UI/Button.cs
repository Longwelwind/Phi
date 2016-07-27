using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    class Button : Displayable
    {
        public string label;
        public bool drawBackground;
        public Action clickAction;

        public Button(string label, Action clickAction, bool drawBackground = true)
        {
            this.label = label;
            this.drawBackground = drawBackground;
            this.clickAction = clickAction;
        }

        public override void Draw(Rect inRect)
        {
            if (Widgets.ButtonText(inRect, label, drawBackground))
            {
                clickAction();
            }
        }
    }
}
