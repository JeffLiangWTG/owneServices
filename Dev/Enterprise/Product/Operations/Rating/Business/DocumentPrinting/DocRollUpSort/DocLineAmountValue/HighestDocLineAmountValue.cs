using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	sealed class HighestDocLineAmountValue : BaseDocLineAmountValue
	{
		public HighestDocLineAmountValue(string currency, string applyTo, string weightUnit, decimal weightRate, decimal weightFlat, string volumeUnit, decimal volumeRate, decimal volumeFlat)
			: base
			(
				properties: new[] { (CurrencyPropertyKey, currency), (ApplyToPropertyKey, applyTo), (WeightUnitPropertyKey, weightUnit), (VolumeUnitPropertyKey, volumeUnit), (AmountValueKey, $"{weightRate}-{weightFlat}-{volumeRate}-{volumeFlat}") },
				values: new[] { (WeightRateValueKey, weightRate), (WeightFlatValueKey, weightFlat), (VolumeRateValueKey, volumeRate), (VolumeFlatValueKey, volumeFlat) }
			)
		{
		}

		#region Property

		const string WeightUnitPropertyKey = "WeightUnit";
		const string VolumeUnitPropertyKey = "VolumeUnit";

		#endregion

		#region Value

		const string WeightRateValueKey = "WeightRate";
		const string WeightFlatValueKey = "WeightFlat";
		const string VolumeRateValueKey = "VolumeRate";
		const string VolumeFlatValueKey = "VolumeFlat";

		#endregion

		public HighestDocLineAmountValue(DocLineAmountProperty key)
			: base(key)
		{
		}

		public override BaseDocLineAmountValue Add(BaseDocLineAmountValue value)
		{
			var result = new HighestDocLineAmountValue(Property);

			result.Values[WeightRateValueKey] = Values[WeightRateValueKey] + value.Values[WeightRateValueKey];
			result.Values[WeightFlatValueKey] = Values[WeightFlatValueKey] + value.Values[WeightFlatValueKey];

			result.Values[VolumeRateValueKey] = Values[VolumeRateValueKey] + value.Values[VolumeRateValueKey];
			result.Values[VolumeFlatValueKey] = Values[VolumeFlatValueKey] + value.Values[VolumeFlatValueKey];

			return result;
		}

		public override QuotationLineList GetQuotationLineList(RateLine rateLine)
		{
			var result = new QuotationLineList { QuotationLine.New(rateLine, Property[ApplyToPropertyKey]) };

			if (Values[WeightRateValueKey] != 0)
			{
				result.Add(QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[WeightRateValueKey],
					"",
					(NoResString)Property[WeightUnitPropertyKey],
					Property[CurrencyPropertyKey]
				));
			}

			if (Values[WeightFlatValueKey] != 0)
			{
				result.Add(QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[WeightFlatValueKey],
					"",
					(NoResString)ZString.Empty,
					Property[CurrencyPropertyKey]
				));
			}

			if (Values[VolumeRateValueKey] != 0)
			{
				result.Add(QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[VolumeRateValueKey],
					"",
					(NoResString)Property[VolumeUnitPropertyKey],
					Property[CurrencyPropertyKey]
				));
			}

			if (Values[VolumeFlatValueKey] != 0)
			{
				result.Add(QuotationLine.NewWithValue
				(
					rateLine,
					QuotationLineType.Mandatory,
					Values[VolumeFlatValueKey],
					"",
					(NoResString)ZString.Empty,
					Property[CurrencyPropertyKey]
				));
			}

			return result;
		}
	}
}
