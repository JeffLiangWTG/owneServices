using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class LineBreaker
	{
		public LineBreaker(int maxFieldWidthInDB)
			 : this(0, maxFieldWidthInDB, 0, null)
		{
		}

		public LineBreaker(int safeLength, int maxFieldWidthInDB, int maxTextWidthInMillimeters, Font displayFont)
		{
			this.safeLength = safeLength;
			this.maxFieldWidthInDB = maxFieldWidthInDB;
			this.maxTextWidthInMillimeters = maxTextWidthInMillimeters;
			this.displayFont = displayFont;
		}

		readonly int safeLength, maxFieldWidthInDB, maxTextWidthInMillimeters;
		readonly Font displayFont;

		public ZString GetBrokenLines(ZString sourceLine, int? targetLineNumber = null)
		{
			var entries = GetBrokenLinesList(sourceLine, 0, targetLineNumber);
			return string.Join("\n", entries) + "\n";
		}

		public List<ZString> GetBrokenLinesList(ZString sourceLine, int maxNumberOfLines = 0, int? targetLineNumber = null)
		{
			var entries = new List<ZString>();

			ZString[] lines = sourceLine.Trim().Replace("\r", "").Split('\n');
			foreach (ZString nextLine in lines)
			{
				ZString fullLine = nextLine;

				while (fullLine.Length > 0)
				{
					ZString line1 = ZString.Empty;
					int nextPos = GetNextLineBreakOrSpacePosition(fullLine);
					int endOfNextWordPos = fullLine.IndexOf(" ", StringComparison.Ordinal);

					if ((nextPos < 0 && fullLine.Length > 0)
						|| (!IsThereSpaceForNextString(fullLine.Left(nextPos), targetLineNumber) && nextPos >= 0))
					{
						line1 = fullLine.Left(safeLength);
						fullLine = fullLine.SubstringSafe(safeLength);

						while (fullLine.Length > 0 && IsThereSpaceForNextString(line1 + fullLine.Left(1), targetLineNumber))
						{
							line1 += fullLine.Left(1);
							fullLine = fullLine.SubstringSafe(1);
						}
					}

					while (nextPos >= 0 && fullLine.Length > 0 && IsThereSpaceForNextString(line1 + fullLine.Left(nextPos), targetLineNumber))
					{
						line1 += fullLine.Left(nextPos + 1);
						fullLine = fullLine.SubstringSafe(nextPos + 1);

						if (nextPos < endOfNextWordPos && nextPos > 0)
						{
							break;
						}

						nextPos = GetNextLineBreakOrSpacePosition(fullLine);
						endOfNextWordPos = fullLine.IndexOf(" ", StringComparison.Ordinal);
						if (nextPos < 0 && IsThereSpaceForNextString(line1 + fullLine, targetLineNumber))
						{
							line1 += fullLine;
							fullLine = ZString.Empty;
						}
					}

					var line = line1.Trim();

					if (maxNumberOfLines > 0 && entries.Count == maxNumberOfLines)
					{
						entries[maxNumberOfLines - 1] += line;
					}
					else
					{
						entries.Add(line);

						if (targetLineNumber.HasValue)
						{
							targetLineNumber++;
						}
					}

					line1 = "";
				}
			}

			return entries;
		}

		int GetNextLineBreakOrSpacePosition(String sourceText)
		{
			int nextNewLinePos = sourceText.IndexOf("\n", StringComparison.Ordinal);
			int nextSpacePos = sourceText.IndexOf(" ", StringComparison.Ordinal);
			if (nextSpacePos < 0 || (nextNewLinePos <= nextSpacePos && nextNewLinePos > 0))
			{
				return nextNewLinePos;
			}
			return nextSpacePos;
		}

		public bool IsThereSpaceForNextString(string text, int? lineNumber = null)
		{
			if (String.IsNullOrEmpty(text))
			{
				return true;
			}

			if (text.Length > GetMaxFieldWidth(lineNumber))
			{
				return false;
			}

			if (displayFont != null && maxTextWidthInMillimeters > 0)
			{
				return (new TextSizeCalculator(displayFont).GetLengthInMillimeter(text) <= maxTextWidthInMillimeters);
			}

			return true;
		}

		int GetMaxFieldWidth(int? lineNumber)
		{
			if (lineNumber.HasValue && lineMaxLengthLookup != null && lineMaxLengthLookup.ContainsKey(lineNumber.Value))
			{
				return lineMaxLengthLookup[lineNumber.Value];
			}

			return maxFieldWidthInDB;
		}

		public void SetTargetLineMaxLengths(IReadOnlyDictionary<int, int> lineMaxLengthLookupParam)
		{
			lineMaxLengthLookup = lineMaxLengthLookupParam;
		}

		IReadOnlyDictionary<int, int> lineMaxLengthLookup;
	}
}
