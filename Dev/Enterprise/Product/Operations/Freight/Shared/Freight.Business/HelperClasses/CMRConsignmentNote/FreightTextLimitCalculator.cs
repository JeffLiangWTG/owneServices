using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Freight.Business
{
	public class FreightTextLimitCalculator : ITextLimitCalculator
	{
		public ZString CalculateLimits(ZString text, int maxLines, int lineMaxLength)
		{
			if (string.IsNullOrEmpty(text))
			{
				return ZString.Empty;
			}

			if (maxLines <= 0 || lineMaxLength <= 0)
			{
				return ZString.Empty;
			}

			var textString = text.ToString();

			var outputLines = new List<string>();
			var originalLines = textString.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None)
				.Where(line => !string.IsNullOrEmpty(line)).ToArray();

			var resultLines = new List<string>();
			var subStringLength = lineMaxLength - System.Environment.NewLine.Length;

			for (int i = 0; i < originalLines.Length; i++)
			{
				if (resultLines.Count >= maxLines)
				{
					break;
				}

				var line = originalLines[i];

				if (resultLines.Count == maxLines - 1 || i == originalLines.Length - 1)
				{
					// this is the last line, so we don't add a new line character at the end
					subStringLength = lineMaxLength;
				}

				var processedLine = line.Length > subStringLength
					? line.Substring(0, subStringLength)
					: line;

				resultLines.Add(processedLine);
			}

			return string.Join(System.Environment.NewLine, resultLines);
		}
	}
}
