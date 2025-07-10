using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	[XmlRoot(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
	public class CARMExciseTaxCode : CARMContentProperties
	{
		[XmlElement(ElementName = "ExciseTaxCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string ExciseTaxCode { get; set; }
		[XmlElement(ElementName = "ExciseTaxRateTypeCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string ExciseTaxRateTypeCode { get; set; }
		[XmlElement(ElementName = "ExciseTaxRateValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double ExciseTaxRateValue { get; set; }
		[XmlElement(ElementName = "UnitOfMeasureCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string UnitOfMeasureCode { get; set; }
		[XmlElement(ElementName = "Description", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string Description
		{
			get => description;
			set
			{
				description = string.IsNullOrEmpty(value) ? value : value.Trim();
			}
		}
		string description;

		[XmlElement(ElementName = "ExciseTaxRateValidStartDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime ExciseTaxRateValidStartDate { get; set; }
		[XmlElement(ElementName = "ExciseTaxRateValidEndDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime ExciseTaxRateValidEndDate { get; set; }

		public override bool IsValid => !string.IsNullOrWhiteSpace(ExciseTaxCode);

		public RefCusRate ParseToRefCusRate()
		{
			return IsValid ? new RefCusRate
			{
				ZZ2_RateFormula = ParserHelper.GenerateRateFormula(ExciseTaxRateValue, ExciseTaxRateTypeCode, UnitOfMeasureCode),
				ZZ2_ZY1_ZZR_NKRateType = Constants.RateTypeExs,
				ZZ2_ZY1_NKRateCode = ExciseTaxCode,
				ZZ2_ZZZ_NKDataGrouping = Constants.DefaultValues.CountryCodeCanada,
				ZZ2_StartDate = ExciseTaxRateValidStartDate,
				ZZ2_EndDate = ExciseTaxRateValidEndDate,
			} : null;
		}

		public CARMExciseTaxCode ShallowCopy()
		{
			return (CARMExciseTaxCode)MemberwiseClone();
		}

		public void PopulateDateRangeByExciseTax(CARMExciseTax exciseTax)
		{
			ExciseTaxRateValidStartDate = exciseTax.ExciseTaxCodeValidStartDate;
			ExciseTaxRateValidEndDate = exciseTax.ExciseTaxCodeValidEndDate;
		}

		public RefCusRateCode ParseToRefCusRateCode()
		{
			return string.IsNullOrWhiteSpace(ExciseTaxCode) ? null : new RefCusRateCode
			{
				ZY1_RateCode = ExciseTaxCode,
				ZY1_Description = Description
			};
		}
	}
}
