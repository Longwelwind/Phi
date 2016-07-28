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
        const float CHECKBOX_SIZE = 40f;

        string label;
        bool checkedOn;
        Action<bool> onChange;

        public CheckboxLabeledWidget(string label, bool checkedOn, Action<bool> onChange)
        {
            this.label = label;
            this.checkedOn = checkedOn;
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
            return CHECKBOX_SIZE;
        }
    }
}
