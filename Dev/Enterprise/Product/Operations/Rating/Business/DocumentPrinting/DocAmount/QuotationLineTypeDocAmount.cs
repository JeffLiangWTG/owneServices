using System.Globalization;
using CargoWise.Common.Cache;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount
{
	internal sealed class QuotationLineTypeDocAmount : DocAmount
	{
		internal QuotationLineTypeDocAmount(ZDecimal amountAsDecimal, QuotationLineType type)
		{
			Type = type;
			AmountAsDecimal = amountAsDecimal;

			CachedAmount = new PropertyCache<MultilingualString>(getProperty: () => (NoResString)GetAmountAsString(Type, AmountAsDecimal, CultureInfo.CurrentCulture));
		}

		public override MultilingualString AmountAsString => CachedAmount.Get(CultureInfo.CurrentCulture.ToString());

		static MultilingualString GetAmountAsString(QuotationLineType type, ZDecimal amountAsDecimal, CultureInfo cultureInfo)
		{
			string amountAsString;
			if ((type & QuotationLineType.f0) != 0)
			{
				amountAsString = amountAsDecimal.ToString("f0", cultureInfo);
			}
			else if ((type & QuotationLineType.f4) != 0)
			{
				amountAsString = amountAsDecimal.ToString("f4", cultureInfo);
			}
			else
			{
				if (amountAsDecimal.DecimalPlaces <= 2)
				{
					amountAsString = amountAsDecimal.ToString("f2", cultureInfo);
				}
				else
				{
					amountAsString = amountAsDecimal.ToStringTrimZeros();
				}
			}

			return (NoResString)amountAsString;
		}

		QuotationLineType Type { get; }

		ZDecimal AmountAsDecimal { get; }

		PropertyCache<MultilingualString> CachedAmount { get; }
	}
}
