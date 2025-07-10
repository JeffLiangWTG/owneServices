using System.Globalization;
using CargoWise.Common.Cache;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount
{
	internal sealed class DecimalDigitDocAmount : DocAmount
	{
		internal DecimalDigitDocAmount(ZDecimal amountAsDecimal, int decimalDigit)
		{
			AmountAsDecimal = amountAsDecimal;
			DecimalDigit = decimalDigit;

			CachedAmount = new PropertyCache<MultilingualString>(getProperty: () => (NoResString)Utilities.FormatNumber(AmountAsDecimal, DecimalDigit, CultureInfo.CurrentCulture));
		}

		public override MultilingualString AmountAsString => CachedAmount.Get(CultureInfo.CurrentCulture.ToString());

		ZDecimal AmountAsDecimal { get; }

		int DecimalDigit { get; }

		PropertyCache<MultilingualString> CachedAmount { get; }
	}
}
