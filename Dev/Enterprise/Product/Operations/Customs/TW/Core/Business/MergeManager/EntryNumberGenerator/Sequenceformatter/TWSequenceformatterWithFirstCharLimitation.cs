using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class TWSequenceformatterWithFirstCharLimitation : BaseTWSequenceformatter
	{
		public TWSequenceformatterWithFirstCharLimitation(int maximumValue, int totalSize, List<char> alphabets, List<char> firstAlphabets) : base(maximumValue, totalSize, alphabets)
		{
			FirstAlphabets = firstAlphabets;
		}

		public readonly List<char> FirstAlphabets;

		public override ZString AllowedFormatDescription => Res.GetString("40ABF1D9-61D6-4AC2-91C0-01A1BFFE4131", "The first character only accept digits and English Characters {0}, the other character only accept digits and English Characters {1}, and last character must is digits, and the length is {2}.", string.Join("/", FirstAlphabets), string.Join("/", Alphabets), TotalSize);

		public override string FormatIntToString(int nextNumber)
		{
			int startRangeForSeqFormatter = (int)Math.Pow(10, TotalSize) - 1;
			if (nextNumber <= startRangeForSeqFormatter)
			{
				return nextNumber.ToString(CultureInfo.InvariantCulture).PadLeft(TotalSize, '0');
			}
			return FormatNumberToKey(nextNumber, TotalSize, FirstAlphabets.ToArray(), Alphabets.ToArray(), MaximumValue, 0);
		}

		public override int FormatStringToInt(string nextNumber)
		{
			return FormatStringToNumber(nextNumber, TotalSize, FirstAlphabets.ToArray(), Alphabets.ToArray(), MaximumValue);
		}

		protected virtual int EnglishCharStartIndex => -1;

		public override bool DoesEntryNumberFallIntoThisCategory(ZString number)
		{
			bool result = false;
			if (EnglishCharStartIndex >= 0)
			{
				var englishChar = number.SubstringSafe(EnglishCharStartIndex, 1);
				var firstAlphabets = FirstAlphabets;
				if (!englishChar.IsEmpty && firstAlphabets != null && firstAlphabets.Any())
				{
					result = !Regex.IsMatch(englishChar, "[0-9]") && !firstAlphabets.Contains(englishChar[0]);
				}
			}
			return result;
		}
	}
}
