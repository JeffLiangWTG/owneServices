using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class TWSequenceformatterOnlyFirstCharCanHaveEnglish : TWSequenceformatterWithFirstCharLimitation
	{
		public TWSequenceformatterOnlyFirstCharCanHaveEnglish(int maximumValue, int totalSize, List<char> alphabets, List<char> firstAlphabets) : base(maximumValue, totalSize, alphabets, firstAlphabets)
		{
		}

		public override string FormatIntToString(int nextNumber)
		{
			return FormatNumberToKey(nextNumber, TotalSize, FirstAlphabets.ToArray(), Alphabets.ToArray(), MaximumValue, 0, 1);
		}

		public override int FormatStringToInt(string nextNumber)
		{
			return FormatStringToNumber(nextNumber, TotalSize, FirstAlphabets.ToArray(), Alphabets.ToArray(), MaximumValue, 1);
		}

		public override ZString AllowedFormatDescription => Res.GetString("EF421B93-6AA5-44A9-9625-A639FEA0859A", "The first character only accept English Characters {0}, the other character only accept digits and English Characters {1}, and last character must is digits, and the length is {2}.", string.Join("/", FirstAlphabets), string.Join("/", Alphabets), TotalSize);

		public override bool DoesEntryNumberFallIntoThisCategory(ZString number)
		{
			bool result = false;
			if (EnglishCharStartIndex >= 0)
			{
				var englishChar = number.SubstringSafe(EnglishCharStartIndex, 1);
				var firstAlphabets = FirstAlphabets;
				if (!englishChar.IsEmpty && firstAlphabets != null && firstAlphabets.Any())
				{
					result = !firstAlphabets.Contains(englishChar[0]);
				}
			}
			return result;
		}
	}
}
