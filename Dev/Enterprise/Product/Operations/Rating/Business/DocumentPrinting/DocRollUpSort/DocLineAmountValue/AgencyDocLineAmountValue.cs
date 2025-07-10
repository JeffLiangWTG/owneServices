using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class AgencyDocLineAmountValue : BaseDocLineAmountValue
	{
		public AgencyDocLineAmountValue(string currency, string description, decimal agencyRate, string agencyRateDescription, decimal additionalRate, string additionalRateDescription, decimal costPerAdditionalRate, string costPerAdditionalRateDescription, string unit)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (DescriptionProperty, description), (AgencyRateDescriptionProperty, agencyRateDescription), (AdditionalRateDescriptionProperty, additionalRateDescription), (CostPerAdditionalRateDescriptionProperty, costPerAdditionalRateDescription), (UnitProperty, unit) },
				values: new[] { (AgencyRateValue, agencyRate), (AdditionalRateValue, additionalRate), (CostPerAdditionalRateValue, costPerAdditionalRate) }
			)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string DescriptionProperty = "Description";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string AgencyRateDescriptionProperty = "AgencyRateDescription";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string AdditionalRateDescriptionProperty = "AdditionalRateDescription";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string CostPerAdditionalRateDescriptionProperty = "CostPerAdditionalRateDescription";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string CostPerAdditionalRateValue = "CostPerAdditionalRate";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string AdditionalRateValue = "AdditionalRate";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string AgencyRateValue = "AgencyRate";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string UnitProperty = "Unit";

		public AgencyDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new AgencyDocLineAmountValue(Property);

			result.Values[AgencyRateValue] = Values[AgencyRateValue] + value.Values[AgencyRateValue];
			result.Values[AdditionalRateValue] = Values[AdditionalRateValue] + value.Values[AdditionalRateValue];
			result.Values[CostPerAdditionalRateValue] = Values[CostPerAdditionalRateValue] + value.Values[CostPerAdditionalRateValue];

			return result;
		}

		public override QuotationLineList GetQuotationLineList(RateLine rateLine)
		{
			var result = new QuotationLineList
			{
				QuotationLine.Header(rateLine, QuotationLineType.Mandatory, Property[DescriptionProperty])
			};

			if (Values[AgencyRateValue] != 0)
			{
				var agencyRateValue = QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[AgencyRateValue],
					Property[AgencyRateDescriptionProperty],
					(NoResString)"",
					Property[CurrencyPropertyKey]
				);
				agencyRateValue.Shift();
				result.Add(agencyRateValue);
			}

			if (Values[AdditionalRateValue] != 0)
			{
				var additionalRateValue = QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[AdditionalRateValue],
					Property[AdditionalRateDescriptionProperty],
					(NoResString)"",
					Property[CurrencyPropertyKey]
				);
				additionalRateValue.Shift();
				result.Add(additionalRateValue);
			}

			if (Values[CostPerAdditionalRateValue] != 0)
			{
				var costPerAdditionalRateValue = QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[CostPerAdditionalRateValue],
					Property[CostPerAdditionalRateDescriptionProperty],
					(NoResString)Property[UnitPropertyKey],
					Property[CurrencyPropertyKey]
				);
				costPerAdditionalRateValue.Shift();
				result.Add(costPerAdditionalRateValue);
			}

			return result;
		}
	}
}
