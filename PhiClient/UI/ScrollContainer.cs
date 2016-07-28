using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    class ScrollContainer : Displayable
    {
        const float SCROLL_BAR_SIZE = 16f;

        Displayable child;
        Action<Vector2> onScroll;
        Vector2 scrollPosition = Vector2.zero;

        public ScrollContainer(Displayable child, Vector2 scrollPosition, Action<Vector2> onScroll)
        {
            this.scrollPosition = scrollPosition;
            this.child = child;
            this.onScroll = onScroll;
        }

        public override void Draw(Rect inRect)
        {
            Rect viewRect = inRect.LeftPartPixels(inRect.width - SCROLL_BAR_SIZE);

            // We calculate the overflowed size the children will take
            // Only supports y-overflow at the moment
            float widthChild = viewRect.width;
            float heightChild = child.CalcHeight(viewRect.width);
            if (heightChild == -1)
            {
                // If the child is height-fluid, we attribute the available space
                heightChild = viewRect.height;
            }
            Rect childRect = new Rect(0, 0, widthChild, heightChild);
            Widgets.BeginScrollView(inRect, ref scrollPosition, childRect);
            onScroll(scrollPosition);

            child.Draw(childRect);

            Widgets.EndScrollView();
        }
    }
}
