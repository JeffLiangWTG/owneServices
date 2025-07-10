using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class FlatDocLineAmountValue : BaseDocLineAmountValue
	{
		public FlatDocLineAmountValue(string currency, decimal flat, string applyTo)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (ApplyToPropertyKey, applyTo) },
				values: new[] { (AmountValueKey, flat) }
			)
		{
		}

		public FlatDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new FlatDocLineAmountValue(Property);
			result.Values[AmountValueKey] = Values[AmountValueKey] + value.Values[AmountValueKey];
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
					Property.TryGetValue(ApplyToPropertyKey, out var value) ? value : string.Empty,
					(NoResString)ZString.Empty,
					Property[CurrencyPropertyKey]
				)
			};
	}
}
