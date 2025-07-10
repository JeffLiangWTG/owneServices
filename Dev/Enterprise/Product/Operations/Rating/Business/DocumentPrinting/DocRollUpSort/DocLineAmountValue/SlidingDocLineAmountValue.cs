using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class SlidingDocLineAmountValue : BaseDocLineAmountValue
	{
		public SlidingDocLineAmountValue(string currency, string operatorBreak, string unit, decimal rate, decimal flat)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (OperatorBreakPropertyKey, operatorBreak), (UnitPropertyKey, unit) },
				values: new[] { (RateValueKey, rate), (AmountValueKey, flat) }
			)
		{
		}

		public SlidingDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		public override QuotationLineList GetQuotationLineList(RateLine rateLine)
		{
			var result = new QuotationLineList();

			if (Property.ContainsKey(UnitPropertyKey))
			{
				result.Add(QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[RateValueKey],
					Property[OperatorBreakPropertyKey],
					(NoResString)Property[UnitPropertyKey],
					Property[CurrencyPropertyKey]
				));
			}

			if (Values[AmountValueKey] != 0)
			{
				result.Add(QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[AmountValueKey],
					Property[OperatorBreakPropertyKey],
					(NoResString)"",
					Property[CurrencyPropertyKey]
				));
			}

			return result;
		}

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new SlidingDocLineAmountValue(Property);

			result.Values[RateValueKey] = Values[RateValueKey] + value.Values[RateValueKey];
			result.Values[AmountValueKey] = Values[AmountValueKey] + value.Values[AmountValueKey];

			return result;
		}
	}
}
