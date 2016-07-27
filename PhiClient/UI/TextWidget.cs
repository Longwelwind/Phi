using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    public class TextWidget : Displayable
    {
        string text;
        GameFont font;
        TextAnchor anchor;

        public TextWidget(string text, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            this.text = text;
            this.font = font;
            this.anchor = anchor;
        }

        public override float CalcHeight(float width)
        {
            SetStyle();
            float height = Text.CalcHeight(text, width);
            ClearStyle();
            return height;
        }

        public override void Draw(Rect inRect)
        {
            SetStyle();
            Widgets.Label(inRect, text);
            ClearStyle();
        }

        private void SetStyle()
        {
            Text.Anchor = this.anchor;
            Text.Font = this.font;
        }

        private void ClearStyle()
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public override bool IsFluidHeight()
        {
            return false;
        }
    }
}
