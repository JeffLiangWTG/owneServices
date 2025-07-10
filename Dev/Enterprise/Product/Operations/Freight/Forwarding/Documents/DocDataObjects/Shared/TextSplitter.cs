using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class TextSplitter
	{
		public static string[] Split(string text, int maxLineLength)
		{
			if (string.IsNullOrWhiteSpace(text)
				|| maxLineLength <= 0)
			{
				return Array.Empty<string>();
			}

			var res = new List<string>();
			var accumulator = new List<char>();
			var indexOfSpace = 0;

			void AddLine(string line)
			{
				if (!string.IsNullOrEmpty(line))
				{
					res.Add(line);
				}
			}

			for (int i = 0; i < text.Length; i++)
			{
				var ch = text[i];

				switch (ch)
				{
					case '\n':
						var line = new string(accumulator.ToArray()).TrimEnd();
						AddLine(line);
						accumulator.Clear();
						indexOfSpace = 0;
						continue;

					case ' ':
						indexOfSpace = accumulator.Count;
						break;
				}

				accumulator.Add(ch);

				if (accumulator.Count == maxLineLength)
				{
					var line = indexOfSpace > 0
						? new string(accumulator.ToArray()).Substring(0, indexOfSpace)
						: new string(accumulator.ToArray());

					AddLine(line);

					if (indexOfSpace > 0)
					{
						var reminder = accumulator
							.Skip(indexOfSpace + 1)
							.TakeWhile(remainingChar => char.IsLetterOrDigit(remainingChar))
							.ToArray();

						accumulator.Clear();
						accumulator.AddRange(reminder);
					}
					else
					{
						accumulator.Clear();
					}

					indexOfSpace = 0;
				}
			}

			if (accumulator.Count > 0)
			{
				AddLine(new string(accumulator.ToArray()));
			}

			return res.ToArray();
		}
	}
}
