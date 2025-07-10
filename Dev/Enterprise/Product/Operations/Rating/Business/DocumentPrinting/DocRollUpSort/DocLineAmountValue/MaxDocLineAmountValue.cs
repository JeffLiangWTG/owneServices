using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class MaxDocLineAmountValue : BaseDocLineAmountValue
	{
		public MaxDocLineAmountValue(string currency, string applyTo, decimal max)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (ApplyToPropertyKey, applyTo) },
				values: new[] { (AmountValueKey, max) }
			)
		{
		}

		public MaxDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		static string MaxText => Res.GetString("A1CEFDEE-4A55-45B6-A941-D5DDE8CE273D", "Maximum");

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new MaxDocLineAmountValue(Property);

			result.Values[AmountValueKey] = Values[AmountValueKey] > value.Values[AmountValueKey]
				? Values[AmountValueKey]
				: value.Values[AmountValueKey];

			return result;
		}

		public override QuotationLineList GetQuotationLineList(RateLine rateLine)
			=> new()
			{
				QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[AmountValueKey],
					MaxText,
					(NoResString)(Property.TryGetValue(ApplyToPropertyKey, out var value) ? value : string.Empty),
					Property[CurrencyPropertyKey]
				)
			};
	}
}
