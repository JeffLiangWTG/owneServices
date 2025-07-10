using CargoWise.Common.Cache;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount
{
	internal sealed class AmountWithDescriptionDocAmount : DocAmount
	{
		internal AmountWithDescriptionDocAmount(ZDecimal amountAsDecimal, int decimalDigit, string description)
		{
			AmountAsDecimal = amountAsDecimal;
			DecimalDigit = decimalDigit;
			Description = description;

			CachedAmount = new PropertyCache<MultilingualString>(getProperty: () => (NoResString)string.Format(Description, AmountAsDecimal.ToString("N" + DecimalDigit, Culture.CurrentCompanyCountryCulture)));
		}

		public override MultilingualString AmountAsString => CachedAmount.Get(Culture.CurrentCompanyCountryCulture.ToString());

		ZDecimal AmountAsDecimal { get; }

		int DecimalDigit { get; }

		string Description { get; }

		PropertyCache<MultilingualString> CachedAmount { get; }
	}
}
