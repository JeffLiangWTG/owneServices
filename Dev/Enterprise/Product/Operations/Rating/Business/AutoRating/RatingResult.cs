using System.Runtime.Serialization;

namespace Enterprise.Rating.Business
{
	[DataContract]
	public class RatingResults
	{
		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public RatingResult[] Results { get; set; }

		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] Logs { get; set; }
	}

	[DataContract]
	public class RatingResult
	{
		[DataMember]
		public string Target { get; set; }

		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public RatesAdditionInfo[] Results { get; set; }

		[DataMember]
		public string AutoRatingExplorer { get; set; }
	}

	[DataContract]
	public class RatesAdditionInfo
	{
		[DataMember]
		public string Target { get; set; }

		[DataMember]
		public string CostSell { get; set; }

		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ChargeInfo[] CreatedCharges { get; set; }

		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ChargeInfo[] ModifiedCharges { get; set; }

		[DataMember]
		public int DeletedChargesCount { get; set; }

		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ChargeInfo[] RatesFound { get; set; }
	}

	[DataContract]
	public class ChargeInfo
	{
		public ChargeInfo()
		{
		}

		public ChargeInfo(string chargeCode, AutoRateInfo info)
		{
			Info = info;
			RateId = info.RateId;
			ChargeCode = chargeCode;
			RateProviderCode = info.RateProviderCode;

			var type = info?.Line?.ParentRateEntry?.ParentRatingHeader?.TH_RateType;
			if (type.HasValue)
			{
				RateType = type.Value;
			}

			var mode = info?.Line?.ParentRateEntry?.TI_Mode;
			if (mode.HasValue)
			{
				RateMode = mode.Value;
			}

			var category = info?.Line?.ParentRateEntry?.TI_RateCategory;
			if (category.HasValue)
			{
				RateCategory = category.Value;
			}

			var calculator = info?.Line?.TL_RateCalculator;
			if (calculator.HasValue)
			{
				Calculator = calculator.Value;
			}
		}

		[DataMember]
		public string ChargeCode { get; set; }
		public string RateProviderCode { get; set; }
		public string RateId { get; set; }
		public string RateMode { get; set; }
		public string RateType { get; set; }
		public string RateCategory { get; set; }
		public string Calculator { get; set; }
		public AutoRateInfo Info { get; }
	}
}