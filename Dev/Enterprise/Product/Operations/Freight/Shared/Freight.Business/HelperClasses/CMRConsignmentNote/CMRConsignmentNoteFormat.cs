using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class CMRConsignmentNoteFormat
	{
		public static string TrimTrailingCarriageReturns(string sourceString)
		{
			Char[] trimChars = { '\r', '\n' };

			return sourceString.TrimEnd(trimChars);
		}
		public static ZString GetFormattedColumn(ZStringBuilder builder)
		{
			var formattedValue = ZString.Empty;

			if (!builder.IsEmpty)
			{
				formattedValue = builder.ToStringWithNewLineBetweenAppends();
			}

			return TrimTrailingCarriageReturns(formattedValue);
		}

		public static  ZString RemoveCarriageReturnsFromDescription(ZString description) => description.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

		public static ZString GetFormattedDecimal(ZDecimal value)
		{
			var formattedValue = ZString.Empty;
			if (value != ZDecimal.Zero)
			{
				var format = string.Concat("F", CMRConsignmentNoteConstants.Formats.NumberOfSignificativeDecimalsToUse.ToString());
				formattedValue = TrimTrailingZerosAndAlignToDecimalSeparator(value.ToString(format));
			}

			return formattedValue;
		}

		static ZString TrimTrailingZerosAndAlignToDecimalSeparator(ZString numberWithTrailingZeros)
		{
			var originalNumberOfCharacters = numberWithTrailingZeros.Length;
			var trimmedValue = numberWithTrailingZeros.TrimEnd('0');

			var trailingSpacesToAdd = originalNumberOfCharacters - trimmedValue.Length;

			if (trailingSpacesToAdd == CMRConsignmentNoteConstants.Formats.NumberOfSignificativeDecimalsToUse)
			{
				trimmedValue = trimmedValue + new string('0', CMRConsignmentNoteConstants.Formats.TrailingZerosWhenNumberHasNoSignificativeDecimals);
				trailingSpacesToAdd -= CMRConsignmentNoteConstants.Formats.TrailingZerosWhenNumberHasNoSignificativeDecimals;
			}

			return trimmedValue + new string(' ', trailingSpacesToAdd);
		}

		public static string FormatEndOfLine(string sourceString) => sourceString?.Replace("\n", "\r\n");
	}
}
