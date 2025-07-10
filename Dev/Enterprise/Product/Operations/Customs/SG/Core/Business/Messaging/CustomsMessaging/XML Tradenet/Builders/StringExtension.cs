using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public static class StringExtension
	{
		public static string[] SplitIntoArray(this ZString value, int maxLength, int maxElements, bool useUpper = true)
		{
			return SplitIntoArrayCore(value, maxLength, maxElements, useUpper).ToArray();
		}

		static IEnumerable<string> SplitIntoArrayCore(this ZString value, int maxLength, int maxElements, bool useUpper = true)
		{
			if (maxElements > 0)
			{
				var splitter = new TextSplitElegantly(maxLength, maxElements) { Text = value };
				var count = Math.Min(maxElements, splitter.Count);

				for (int i = 0; i < count; i++)
				{
					if (useUpper)
					{
						yield return splitter[i].ToUpperInvariant();
					}
					else
					{
						yield return splitter[i];
					}
				}
			}
		}

		public static string GetReferenceNumber(this UniqueReferenceNumber uniqueReferenceNumber)
		{
			return uniqueReferenceNumber != null
				? string.Concat(uniqueReferenceNumber.ID?.Trim() ?? string.Empty, uniqueReferenceNumber.Date?.Trim() ?? string.Empty, uniqueReferenceNumber.SequenceNumeric?.Trim()?.PadLeft(4, '0') ?? string.Empty)
				: string.Empty;
		}
	}
}
