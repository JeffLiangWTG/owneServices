using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
	public class StringWrapper
	{
		int maxLength;
		int maxLoop;
		List<string> values;
		int currentIndex;
		bool splitByCRLF;

		public void SetupStringWrapper(int maximumLength, int maximumLoop)
		{
			SetupStringWrapper(maximumLength, maximumLoop, false);
		}

		public void SetupStringWrapper(int maximumLength, int maximumLoop, bool splitByCRLF)
		{
			currentIndex = 0;
			maxLength = maximumLength;
			maxLoop = maximumLoop;
			values = new List<string>();
			this.splitByCRLF = splitByCRLF;
		}

		public void ResetStringWrapper()
		{
			currentIndex = 0;
			maxLength = 0;
			maxLoop = 0;
			values = new List<string>();
		}

		public void SplitStringInWrapper(string inputString)
		{
			var remainingText = NormalizeString(inputString);
			var count = 0;
			while (count < maxLoop)
			{
				if (string.IsNullOrEmpty(remainingText))
				{
					break;
				}
				var lineIndex = GetLineIndex(remainingText);

				var result = Helper.SubstringSafe(remainingText, 0, lineIndex);
				if (splitByCRLF)
				{
					result = result.Replace("\n", "");
				}
				if (!string.IsNullOrEmpty(result.Trim()))
				{
					values.Add(result);
					count++;
				}
				remainingText = Helper.SubstringSafe(remainingText, lineIndex);
			}
		}

		string NormalizeString(string inputString)
		{
			var remainingText = inputString.Trim();
			remainingText = Regex.Replace(remainingText, @"[ \f\t\v]\s*", " ");
			remainingText = Regex.Replace(remainingText, @"[\r\n]\s*", "\n");

			return remainingText;
		}

		public int Count()
		{
			return values.Count;
		}

		public void AddToList(string inputString)
		{
			if (!string.IsNullOrEmpty(inputString.Trim()))
			{
				values.Add(inputString.Trim());
			}
		}

		public string GetTextFromCurrentIndex()
		{
			var result = GetTextAtIndex(currentIndex);

			if (!string.IsNullOrEmpty(result))
			{
				currentIndex++;
			}

			return result;
		}

		//this is used for validation, it will not increase the index counter (currentIndex)
		public string GetTextFromNextIndex()
		{
			return GetTextAtIndex(currentIndex);
		}

		string GetTextAtIndex(int index)
		{
			return index < values.Count
			  ? values[index]
			  : string.Empty;
		}

		int GetLineIndex(string inputText)
		{
			var lineIndex = GetStringEndIndex(inputText);

			return lineIndex == maxLength
			  ? lineIndex
			  : lineIndex + 1;
		}

		int GetStringEndIndex(string inputText)
		{
			if (string.IsNullOrEmpty(inputText))
			{
				return 0;
			}

			var firstCharIndex = 0;
			while (firstCharIndex < inputText.Length && (inputText[firstCharIndex] == ' ' || splitByCRLF && inputText[firstCharIndex] == '\n'))
			{
				firstCharIndex++;
			}
			var textToCheck = Helper.SubstringSafe(inputText, 0, maxLength + firstCharIndex);

			var lastSpaceIndex = 0;
			if (textToCheck.Length < inputText.Length && inputText[maxLength + firstCharIndex] != ' ' && inputText[maxLength + firstCharIndex] != '\n')
			{
				for (int index = textToCheck.Length - 1; index >= 0; index--)
				{
					if (textToCheck[index] == ' ' || textToCheck[index] == '\n')
					{
						lastSpaceIndex = index;
						break;
					}
				}
			}

			var firstCRLFIndex = int.MaxValue;
			if (splitByCRLF)
			{
				firstCRLFIndex = textToCheck.IndexOf("\n");
				firstCRLFIndex = firstCRLFIndex == -1 ? int.MaxValue : firstCRLFIndex;
			}

			var result = Math.Min(lastSpaceIndex > 0 ? lastSpaceIndex : textToCheck.Length - 1, firstCRLFIndex);

			return result;
		}

		StringHelper Helper
		{
			get { return helper ?? (helper = new StringHelper()); }
		}
		StringHelper helper;
	}
}