using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	[XmlRoot(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
	public class CARMExciseDuty : CARMContentProperties
	{
		[XmlElement(ElementName = "TariffNumber", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string TariffNumber
		{
			get => tariffNumber;
			set
			{
				var newValue = value?.Replace(".", "") ?? string.Empty;
				tariffNumber = string.IsNullOrEmpty(newValue) ? string.Empty : newValue.PadRight(CARMConstants.TariffNumberLength, '0');
			}
		}
		string tariffNumber;

		[XmlElement(ElementName = "ExciseDutyValidStartDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime ExciseDutyValidStartDate
		{
			get => exciseDutyValidStartDate;
			set
			{
				exciseDutyValidStartDate = ParserHelper.ParseDateSafe(value);
			}
		}
		DateTime exciseDutyValidStartDate;

		[XmlElement(ElementName = "ExciseDutyValidEndDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime ExciseDutyValidEndDate
		{
			get => exciseDutyValidEndDate;
			set
			{
				exciseDutyValidEndDate = ParserHelper.ParseDateSafe(value, true);
			}
		}
		DateTime exciseDutyValidEndDate;

		[XmlElement(ElementName = "UnitOfMeasureCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string UnitOfMeasureCode { get; set; }
		[XmlElement(ElementName = "ExciseDutyValueType", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string ExciseDutyValueType { get; set; }
		[XmlElement(ElementName = "ExciseDutyRateValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double ExciseDutyRateValue { get; set; }

		public override bool IsValid => !string.IsNullOrWhiteSpace(TariffNumber);

		public RefCusRate ParseToRefCusRate()
		{
			return IsValid ? new RefCusRate
			{
				ZZ2_StartDate = ExciseDutyValidStartDate,
				ZZ2_EndDate = ExciseDutyValidEndDate,
				ZZ2_RateFormula = ParserHelper.GenerateRateFormula(ExciseDutyRateValue, ExciseDutyValueType, UnitOfMeasureCode),
				ZZ2_ZY1_ZZR_NKRateType = Constants.RateTypeExc,
				ZZ2_ZY1_NKRateCode = Constants.RateTypeExs,
				ZZ2_ZZZ_NKDataGrouping = Constants.DefaultValues.CountryCodeCanada
			} : null;
		}
	}

}
