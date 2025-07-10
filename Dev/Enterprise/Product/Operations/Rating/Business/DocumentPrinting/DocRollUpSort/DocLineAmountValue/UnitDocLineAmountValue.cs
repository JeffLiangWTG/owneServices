using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class UnitDocLineAmountValue : BaseDocLineAmountValue
	{
		public UnitDocLineAmountValue(string currency, string unit, decimal rate, string applyTo)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (UnitPropertyKey, unit), (ApplyToPropertyKey, applyTo) },
				values: new[] { (AmountValueKey, rate) }
			)
		{
		}

		public UnitDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new UnitDocLineAmountValue(Property);
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
					Property.TryGetValue(ApplyToPropertyKey, out var value) ? value : ZString.Empty,
					(NoResString)$"{Property[UnitPropertyKey]}",
					Property[CurrencyPropertyKey]
				)
			};
	}
}
