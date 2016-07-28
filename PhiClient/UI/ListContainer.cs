using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PhiClient.UI
{
    class ListContainer : Displayable
    {
        public const float SPACE = 10f;

        List<Displayable> children;
        ListFlow flow;
        ListDirection direction;
        public float spaceBetween = 0f;

        public ListContainer(List<Displayable> children, ListFlow flow = ListFlow.COLUMN, ListDirection direction = ListDirection.NORMAL)
        {
            this.children = children;
            this.flow = flow;
            this.direction = direction;
        }

        public ListContainer() : this(new List<Displayable>())
        {

        }

        public ListContainer(ListFlow flow = ListFlow.COLUMN, ListDirection direction = ListDirection.NORMAL) : this(new List<Displayable>(), flow, direction)
        {

        }

        public void Add(Displayable display)
        {
            this.children.Add(display);
        }

        public override float CalcHeight(float width)
        {
            if (IsFluidHeight())
            {
                return -1;
            }
            else
            {
                return children.Sum((c) => c.CalcHeight(width));
            }
        }

        public override float CalcWidth(float height)
        {
            if (IsFluidWidth())
            {
                return -1;
            }
            else
            {
                return children.Sum((c) => c.CalcWidth(height));
            }
        }

        public override void Draw(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            if (flow == ListFlow.COLUMN)
            {
                DrawColumn(inRect);
            }
            else
            {
                DrawRow(inRect);
            }

            GUI.EndGroup();
        }

        private void DrawRow(Rect inRect)
        {
            float height = inRect.height;

            // We first calculate the remaining space that will be given to the fluid element
            float takenWidth;
            int countFluidElements = 0;
            if (IsFluidWidth())
            {
                takenWidth = children.Sum((c) =>
                {
                    float childWidth = c.CalcWidth(height);
                    if (childWidth != -1)
                    {
                        return childWidth;
                    }
                    else
                    {
                        countFluidElements++;
                        return 0;
                    }
                });
            }
            else
            {
                // Will never be used anyway
                takenWidth = inRect.width;
            }
            
            float widthForFluid = inRect.width - takenWidth;
            // We remove the width taken by the spaces between elements
            widthForFluid -= (children.Count - 1) * spaceBetween;
            // Each fluid element will take equal space
            widthForFluid /= countFluidElements;

            float beginX = 0;
            // If going right to left, we begin at the end of the Rect
            if (direction == ListDirection.OPPOSITE)
            {
                beginX += inRect.width;
            }

            foreach (Displayable child in children)
            {
                float width = child.CalcWidth(height);
                if (width == -1)
                {
                    width = widthForFluid;
                }

                // If going from right to left, we first remove the width
                // of the child
                if (direction == ListDirection.OPPOSITE)
                {
                    beginX -= width + spaceBetween;
                }

                Rect childArea = new Rect(beginX, 0, width, height);
                GUI.BeginGroup(childArea);
                childArea.x = 0;
                
                child.Draw(childArea);

                GUI.EndGroup();

                // If going from left to right, we then add the width
                // of the child
                if (direction == ListDirection.NORMAL)
                {
                    beginX += width + spaceBetween;
                }
            }
        }

        private void DrawColumn(Rect inRect)
        {
            float width = inRect.width;

            // We first calculate the remaining space that will be given to the fluid element
            float takenHeight;
            int countFluidElements = 0;
            if (IsFluidWidth())
            {
                takenHeight = children.Sum((c) =>
                {
                    float childHeight = c.CalcHeight(width);
                    if (childHeight != -1)
                    {
                        return childHeight;
                    }
                    else
                    {
                        countFluidElements++;
                        return 0;
                    }
                });
            }
            else
            {
                // Will never be used anyway
                takenHeight = inRect.height;
            }

            float heightForFluid = inRect.height - takenHeight;
            // We remove the height taken by the spaces between elements
            heightForFluid -= (children.Count - 1) * spaceBetween;
            // Each fluid element will take equal space
            heightForFluid /= countFluidElements;

            float beginY = 0;
            // If going bottom to top, we begin at the end of the Rect
            if (direction == ListDirection.OPPOSITE)
            {
                beginY += inRect.height;
            }

            foreach (Displayable child in children)
            {
                float height = child.CalcHeight(width);
                if (height == -1)
                {
                    height = heightForFluid;
                }

                // If going from bottom to top, we first remove the height
                // of the child
                if (direction == ListDirection.OPPOSITE)
                {
                    beginY -= height + spaceBetween;
                }

                Rect childArea = new Rect(0, beginY, width, height);
                GUI.BeginGroup(childArea);
                childArea.y = 0;

                child.Draw(childArea);

                GUI.EndGroup();

                // If going from top to bottom, we then add the height
                // of the child
                if (direction == ListDirection.NORMAL)
                {
                    beginY += height + spaceBetween;
                }
            }
        }

        private int CountFluidHeight()
        {
            return children.Count((c) => c.IsFluidHeight());
        }

        private int CountFluidWidth()
        {
            return children.Count((c) => c.IsFluidWidth());
        }

        public override bool IsFluidHeight()
        {
            if (flow == ListFlow.COLUMN)
            {
                return children.Any((c) => c.IsFluidHeight());
            }
            else
            {
                // If the list is a row, it takes all height
                return true;
            }
        }

        public override bool IsFluidWidth()
        {
            if (flow == ListFlow.ROW)
            {
                return children.Any((c) => c.IsFluidWidth());
            }
            else
            {
                // If the list is a column, it takes all width
                return true;
            }
        }
    }

    public enum ListFlow
    {
        ROW,
        COLUMN
    }

    public enum ListDirection
    {
        NORMAL,
        OPPOSITE
    }
}
