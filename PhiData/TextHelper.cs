using System;
using System.Text.RegularExpressions;

namespace PhiClient
{
	public static class TextHelper
	{
		public const string SIZE = "size";
		public const string B = "b";
		public const string I = "i";
		public const string COLOR = "color";

		public static string StripRichText(string input, params string[] strippedTags)
		{
			foreach (string tag in strippedTags) {
				// Maybe a better way than a Regex to parse RichText ?
				string pattern = @"<\/?" + tag + @"(=[\w#]+)?>";

				Regex regex = new Regex(pattern);
				input = regex.Replace (input, "");
			}

			return input;
		}

		public static string StripRichText()
		{
			return StripRichText(SIZE, B, I, COLOR);
		}
	}
}

