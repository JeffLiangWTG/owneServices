using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class FirstPlusAdditionalDocLineAmountValue : BaseDocLineAmountValue
	{
		public FirstPlusAdditionalDocLineAmountValue(string currency, string unit, decimal first, decimal additional)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (UnitPropertyKey, unit) },
				values: new[] { (FirstValueKey, first), (AdditionalValueKey, additional) }
			)
		{
		}

		public FirstPlusAdditionalDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new FirstPlusAdditionalDocLineAmountValue(Property);
			result.Values[FirstValueKey] = Values[FirstValueKey] + value.Values[FirstValueKey];
			result.Values[AdditionalValueKey] = Values[AdditionalValueKey] + value.Values[AdditionalValueKey];
			return result;
		}

		public override QuotationLineList GetQuotationLineList(RateLine rateLine)
			=> new QuotationLineList
			{
				QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[FirstValueKey],
					$"{FirstText} {Property[UnitPropertyKey]}",
					(NoResString)"",
					Property[CurrencyPropertyKey]
				),
				QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[AdditionalText],
					AdditionalText,
					(NoResString)Property[UnitPropertyKey],
					Property[CurrencyPropertyKey]
				)
			};

		#region Value

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string AdditionalValueKey = "Additional";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string FirstValueKey = "First";

		#endregion

		static ResourceString FirstText => ResString.GetMultilingualString("321175F9-4B34-4880-B082-ACABC749BAD1", "First");

		static ResourceString AdditionalText => ResString.GetMultilingualString("3576E020-7602-4A44-8B0F-D53AC0CAD127", "Additional");
	}
}
