using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class BaseTWSequenceformatter : ITWSequenceformatter
	{
		public BaseTWSequenceformatter(int maximumValue, int totalSize, List<char> alphabets)
		{
			MaximumValue = maximumValue;
			TotalSize = totalSize;
			Alphabets = alphabets;
		}

		public int MaximumValue { get; }

		protected int TotalSize { get; }

		protected List<char> Alphabets { get; }

		public static ITWSequenceformatter New(string rangeType)
		{
			switch (rangeType)
			{
				case Constants.RangeTypes.A1:
					return new TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1();
				case RangeTypeList.Codes.B:
					return new TWSequenceformatterWithFirstCharLimitationForB();
				case RangeTypeList.Codes.A:
				case RangeTypeList.Codes.T:
				case RangeTypeList.Codes.C:
				case RangeTypeList.Codes.D:
					return new BaseTWSequenceformatter(6888105, 5, new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' });
				default:
					return null;
			}
		}

		public virtual bool DoesEntryNumberFallIntoThisCategory(ZString number)
		{
			return false;
		}

		public bool IsSequenceNumberMatchEntryNumber(ZString entryNumber, ZString providerSequenceNumber)
		{
			return providerSequenceNumber.IsEmpty || entryNumber.EndsWith(FormatSequenceNumber(providerSequenceNumber));
		}

		public virtual string FormatIntToString(int nextNumber)
		{
			int startRangeForSeqFormatter = (int)Math.Pow(10, TotalSize) - 1;
			if (nextNumber <= startRangeForSeqFormatter)
			{
				return nextNumber.ToString(CultureInfo.InvariantCulture).PadLeft(TotalSize, '0');
			}
			return FormatNumberToKey(nextNumber, TotalSize, null, Alphabets.ToArray(), MaximumValue, 0);
		}

		public virtual int FormatStringToInt(string nextNumber)
		{
			return FormatStringToNumber(nextNumber, TotalSize, null, Alphabets.ToArray(), MaximumValue);
		}

		public ZString FormatSequenceNumber(ZString sequenceNumber)
		{
			return sequenceNumber.Right(TotalSize).PadLeft(TotalSize, '0');
		}

		public static string FormatNumberToKey(int inputInt, int totalSize, char[] alphabet, int maximumValue, int startRangeRef = 0, int prefixLength = 0)
		{
			return FormatNumberToKey(inputInt, totalSize, null, alphabet, maximumValue, startRangeRef, prefixLength);
		}

		public static string FormatNumberToKey(int inputInt, int totalSize, char[] firstAlphabet, char[] alphabet, int maximumValue, int startRangeRef = 0, int prefixLength = 0)
		{
			if (inputInt > maximumValue)
			{
				var limitReachingTime = inputInt / maximumValue;
				inputInt = inputInt % maximumValue;
				if (limitReachingTime == 1 && inputInt == 1)
				{
					ErrorReporter.ReportOnce("Reach the limit of sequence");
				}
				return FormatNumberToKey(inputInt, totalSize, firstAlphabet, alphabet, maximumValue, startRangeRef, prefixLength);
			}

			string key = string.Empty;

			int modBase = (int)(Math.Pow(10, totalSize - prefixLength) - 1);
			var multiple = GetMultiple(firstAlphabet, alphabet, prefixLength);
			int endRange = startRangeRef + multiple * modBase;
			if (inputInt <= endRange)
			{
				int effectiveInput = inputInt - startRangeRef;
				int p1 = effectiveInput / modBase;
				int p2 = effectiveInput % modBase;
				bool atTheEdge = p2 == 0;
				if (atTheEdge)
				{
					p1 = p1 - 1;
					p2 = modBase;
				}
				var remaining = p2.ToString(CultureInfo.InvariantCulture).PadLeft(totalSize - prefixLength, '0');
				key = ConvertToPrefixString(p1, prefixLength, firstAlphabet, alphabet) + remaining;
			}
			else if (prefixLength + 1 < totalSize)
			{
				key = FormatNumberToKey(inputInt, totalSize, firstAlphabet, alphabet, maximumValue, endRange, prefixLength + 1);
			}

			return key;
		}

		#region FormatStringToNumber
		static int FormatBaseStringToNumber(string inputStr, string firstAlphabetString, string baseAlphabetString)
		{
			var result = 0;
			var length = inputStr.Length;
			string prefixString = "";
			int lastValue = 0;
			for (var i = 0; i < length; i++)
			{
				var inputChar = inputStr[i];
				var alphabetString = i == 0 && firstAlphabetString.Length > 0 ? firstAlphabetString : baseAlphabetString;
				if (alphabetString.IndexOf(inputChar) < -0)
				{
					var lastString = inputStr.Substring(i);
					if (lastString.Length < 0 || !int.TryParse(lastString, out lastValue))
					{
						result = -1;
					}
					break;
				}
				prefixString += inputChar;
			}

			for (var i = 0; i < prefixString.Length; i++)
			{
				var inputChar = prefixString[i];
				var alphabetString = i == 0 && firstAlphabetString.Length > 0 ? firstAlphabetString : baseAlphabetString;
				var baseValue = alphabetString.IndexOf(inputChar) + 1;
				result = result == 0 ? baseValue : (result - 1) * alphabetString.Length + baseValue;
			}
			if (prefixString.Length > 0)
			{
				var baseMode = (int)(Math.Pow(10, length - prefixString.Length)) - 1;
				result = (result - 1) * baseMode;
			}
			result += lastValue;
			if (lastValue == 0)
			{
				result = -1;
			}
			return result;
		}

		static string GetFormatBaseString(char[] firstAlphabet, char[] alphabet, int length, int totalLength)
		{
			string baseString = "";
			for (var x = 0; x < length; x++)
			{
				if (x == 0 && firstAlphabet != null && firstAlphabet.Length > 0)
				{
					baseString += firstAlphabet[firstAlphabet.Length - 1];
				}
				else if (alphabet != null && alphabet.Length > 0)
				{
					baseString += alphabet[alphabet.Length - 1];
				}
			}
			return baseString.PadRight(totalLength, '9');
		}

		public static int FormatStringToNumber(string inputStr, int totalSize, char[] firstAlphabet, char[] alphabet, int maximumValue, int prefixLength = 0)
		{
			var length = inputStr.Length;
			int result = 0;
			if (totalSize != length || (length > 0 && !int.TryParse(inputStr.Substring(length - 1, 1), out _)))
			{
				result = -1;
			}
			else
			{
				var firstAlphabetString = firstAlphabet != null && firstAlphabet.Length > 0 ? string.Join("", firstAlphabet) : string.Empty;
				var baseAlphabetString = alphabet != null && alphabet.Length > 0 ? string.Join("", alphabet) : string.Empty;
				for (var i = 0; i < length; i++)
				{
					var inputChar = inputStr[i];
					var baseString = string.Empty;
					var alphabetString = i == 0 && firstAlphabetString.Length > 0 ? firstAlphabetString : baseAlphabetString;
					bool isEnd = false;
					bool isNotInAlphabet = alphabetString.IndexOf(inputChar) < 0;
					if (i < prefixLength && isNotInAlphabet)
					{
						isEnd = true;
						result = -1;
					}
					else if (isNotInAlphabet)
					{
						isEnd = true;
						baseString = inputStr;
					}
					else if (i >= prefixLength)
					{
						baseString = GetFormatBaseString(firstAlphabet, alphabet, i, length);
					}
					if (!string.IsNullOrEmpty(baseString))
					{
						int baseStringNumber = FormatBaseStringToNumber(baseString, firstAlphabetString, baseAlphabetString);
						if (baseStringNumber < 0)
						{
							result = -1;
							isEnd = true;
						}
						else
						{
							result += baseStringNumber;
						}
					}
					if (isEnd)
					{
						break;
					}
				}
				if (result > maximumValue)
				{
					result = -1;
				}
			}
			return result;
		}
		#endregion

		static string ConvertToPrefixString(int inputInput, int lengthIneed, char[] firstAlphabet, char[] alphabet)
		{
			int calcBase = inputInput;
			var result = new List<char>();
			for (int i = 0; i < lengthIneed; i++)
			{
				var chars = (i == lengthIneed - 1) && firstAlphabet != null && firstAlphabet.Length > 0 ? firstAlphabet : alphabet;
				int prefLen = chars.Length;
				int p1 = calcBase % prefLen;
				calcBase = calcBase >= prefLen ? (calcBase - p1) / prefLen : 0;
				result.Add(chars[p1]);
			}
			result.Reverse();
			var key = string.Join("", result.ToArray());
			return key;
		}

		static int GetMultiple(char[] firstAlphabet, char[] alphabet, int prefixLength)
		{
			int multiple = 1;
			if (firstAlphabet != null && firstAlphabet.Length > 0)
			{
				if (prefixLength >= 1)
				{
					multiple = firstAlphabet.Length * (int)Math.Pow(alphabet.Length, prefixLength - 1);
				}
			}
			else
			{
				multiple = (int)Math.Pow(alphabet.Length, prefixLength);
			}

			return multiple;
		}

		public virtual ZString AllowedFormatDescription => Res.GetString("6D4682A5-4A0A-4AE1-878A-1A7170DE3168", "The character only accept digits and English Characters {0}, the length is {1}, and the last character must is digits.", string.Join("/", Alphabets), TotalSize);
	}
}
