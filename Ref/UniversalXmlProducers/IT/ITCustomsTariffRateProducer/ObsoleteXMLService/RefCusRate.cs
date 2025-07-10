using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusRate
	{
		[XmlElement(ElementName = "ZZ2_StartDate")]
		public string StartDate { get; set; }

		[XmlElement(ElementName = "ZZ2_ZY1_NKRateCode")]
		public string RateCode { get; set; }

		[XmlElement(ElementName = "ZZ2_ZY1_ZZR_NKRateType")]
		public string RateType { get; set; }

		[XmlElement(ElementName = "ZZ2_RateFormula")]
		public string RateFormula { get; set; }

		[XmlElement(ElementName = "ZZ2_ZZS_NKPreference")]
		public string Preference { get; set; }

		[XmlElement(ElementName = "ZZ2_ZZS_ZZZ_NKDataGrouping")]
		public string PreferenceDataGrouping { get; set; }

		[XmlElement(ElementName = "RefCusApplicability")]
		public RefCusApplicability Applicability { get; set; }

		[XmlElement(ElementName = "RefCusRateUOM")]
		public List<RefCusRateUOM> RateUOMs { get; set; }

		RefCusRate()
		{
		}

		public static RefCusRate Create(IRate rate)
		{
			Argument.NotNull(rate, nameof(rate));

			var applicability = rate.Applicability;
			var refCusApplicability = applicability is null
				? null
				: new RefCusApplicability(applicability.TradeGroup, applicability.AdditionalCode, applicability.StartDate, null);

			var refCusMeasurementUnits = rate.MeasurementUnits
				?.Select(unitOfMeasure => new RefCusRateUOM { UOM = unitOfMeasure })
				.ToList() ?? new List<RefCusRateUOM>();

			return new RefCusRate
			{
				Applicability = refCusApplicability,
				RateCode = rate.RateCode,
				RateType = rate.RateType,
				RateFormula = rate.RateFormula,
				StartDate = rate.StartDate.ToString("s"),
				RateUOMs = refCusMeasurementUnits
			};
		}

		public static RefCusRate CreateWithPreference(IRate rate, string preference, string preferenceDataGrouping)
		{
			Argument.NotNullOrEmpty(preference, nameof(preference));
			Argument.NotNullOrEmpty(preferenceDataGrouping, nameof(preferenceDataGrouping));

			var refCusRate = Create(rate);
			refCusRate.Preference = preference;
			refCusRate.PreferenceDataGrouping = preferenceDataGrouping;
			return refCusRate;
		}
	}
}
