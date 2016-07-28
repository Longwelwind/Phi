using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    class CheckboxLabeledWidget : Displayable
    {
        string label;
        Boolean checkedOn;
        Action<bool> onChange;

        public CheckboxLabeledWidget(string label, Action<bool> onChange)
        {
            this.label = label;
            this.onChange = onChange;
        }

        public override void Draw(Rect inRect)
        {
            bool oldValue = checkedOn;
            Widgets.CheckboxLabeled(inRect, label, ref checkedOn);

            if (oldValue != checkedOn)
            {
                onChange(checkedOn);
            }
        }

        public override bool IsFluidHeight()
        {
            return false;
        }

        public override float CalcHeight(float width)
        {
            return Text.LineHeight;
        }
    }
}
