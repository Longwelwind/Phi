using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PhiClient.UI
{
    public abstract class Displayable
    {
        public const float FLUID = -1;

        public abstract void Draw(Rect inRect);

        public virtual bool IsFluidWidth()
        {
            return true;
        }

        public virtual bool IsFluidHeight()
        {
            return true;
        }

        public virtual float CalcWidth(float height)
        {
            return Displayable.FLUID;
        }

        public virtual float CalcHeight(float width)
        {
            return Displayable.FLUID;
        }
    }
}
