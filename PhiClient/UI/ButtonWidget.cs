using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    class ButtonWidget : Displayable
    {
        public string label;
        public bool drawBackground;
        public Action clickAction;

        public ButtonWidget(string label, Action clickAction, bool drawBackground = true)
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

        public override bool IsFluidHeight()
        {
            return !drawBackground;
        }

        public override float CalcHeight(float width)
        {
            return Text.CalcHeight(label, width);
        }
    }
}
