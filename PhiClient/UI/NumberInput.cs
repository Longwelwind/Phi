using System;
using Verse;
using UnityEngine;

namespace PhiClient.UI
{
	public class NumberInput : Displayable
	{
		int quantity;
		Action<int> onChange;

		public NumberInput (int quantity, Action<int> onChange)
		{
			this.quantity = quantity;
			this.onChange = onChange;
		}

		public override void Draw (UnityEngine.Rect inRect)
		{
			ListContainer controlsCont = new ListContainer (ListFlow.ROW);

			controlsCont.Add(new ButtonWidget("-100", () => onChange(quantity - 100)));
			controlsCont.Add(new ButtonWidget("-10", () => onChange(quantity - 10) ));
			controlsCont.Add(new ButtonWidget("-1", () => onChange(quantity - 1) ));
			controlsCont.Add(new TextWidget(quantity.ToString(), GameFont.Small, TextAnchor.MiddleCenter));
			controlsCont.Add(new ButtonWidget("+1", () => onChange(quantity - 1) ));
			controlsCont.Add(new ButtonWidget("+10", () => onChange(quantity - 10) ));
			controlsCont.Add(new ButtonWidget("+100", () => onChange(quantity - 100)));

			controlsCont.Draw (inRect);
		}
	}
}

