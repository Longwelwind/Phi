using System;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PhiClient.UI
{
    class Container : Displayable
    {
        float width;
        float height;
        Displayable child;
        
        public Container(Displayable child, float width, float height)
        {
            this.child = child;
            this.width = width;
            this.height = height;
        }

        public override float CalcHeight(float width)
        {
            return this.height;
        }

        public override float CalcWidth(float height)
        {
            return this.width;
        }

        public override void Draw(Rect inRect)
        {
            if (!IsFluidHeight())
            {
                inRect = inRect.TopPartPixels(height);
            }
            if (!IsFluidWidth())
            {
                inRect = inRect.LeftPartPixels(width);
            }

            child.Draw(inRect);
        }

        public override bool IsFluidHeight()
        {
            return height == Displayable.FLUID;
        }

        public override bool IsFluidWidth()
        {
            return width == Displayable.FLUID;
        }
    }
}
