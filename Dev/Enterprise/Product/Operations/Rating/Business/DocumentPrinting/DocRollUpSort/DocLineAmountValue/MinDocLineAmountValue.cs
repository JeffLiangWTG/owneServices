using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class MinDocLineAmountValue : BaseDocLineAmountValue
	{
		public MinDocLineAmountValue(string currency, decimal min, string applyTo)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (ApplyToPropertyKey, applyTo) },
				values: new[] { (AmountValueKey, min) }
			)
		{
		}

		public MinDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		static string MinText => Res.GetString("47497A35-A22E-42E5-8A58-20EA918F3008", "Minimum");

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new MinDocLineAmountValue(Property);

			result.Values[AmountValueKey] = Values[AmountValueKey] < value.Values[AmountValueKey]
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
					Property.TryGetValue(ApplyToPropertyKey, out var value) ? value : MinText,
					(NoResString)"",
					Property[CurrencyPropertyKey]
				)
			};
	}
}
