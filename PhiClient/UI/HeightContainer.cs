using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient.UI
{
    class HeightContainer : Container
    {
        public HeightContainer(Displayable child, float height): base(child, Displayable.FLUID, height)
        {

        }

        public override bool IsFluidHeight()
        {
            return false;
        }
    }
}
