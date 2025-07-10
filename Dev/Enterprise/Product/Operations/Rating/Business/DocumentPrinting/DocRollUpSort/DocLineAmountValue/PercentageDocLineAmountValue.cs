using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class PercentageDocLineAmountValue : BaseDocLineAmountValue
	{
		public PercentageDocLineAmountValue(string currency, string applyTo, string unit, decimal percent)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (ApplyToPropertyKey, applyTo), (UnitPropertyKey, unit) },
				values: new[] { (PercentValueKey, percent) }
			)
		{
		}

		public PercentageDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string PercentValueKey = "Percent";

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new PercentageDocLineAmountValue(Property);

			result.Values[PercentValueKey] = Values[PercentValueKey] + value.Values[PercentValueKey];

			return result;
		}

		public override QuotationLineList GetQuotationLineList(RateLine rateLine)
			=> new()
			{
				QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Property.TryGetValue(ApplyToPropertyKey, out var value) ? value : string.Empty,
					Values[PercentValueKey],
					(NoResString)Property[UnitPropertyKey]
				)
			};
	}
}
