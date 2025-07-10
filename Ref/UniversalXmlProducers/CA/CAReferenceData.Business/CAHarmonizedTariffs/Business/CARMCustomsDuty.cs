using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	[XmlRoot(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
	public class CARMCustomsDuty : CARMContentProperties
	{
		[XmlElement(ElementName = "TariffItemNumber", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string TariffItemNumber
		{
			get => tariffItemNumber;
			set
			{
				var newValue = value?.Replace(".", "") ?? string.Empty;
				tariffItemNumber = string.IsNullOrEmpty(newValue) ? string.Empty : newValue.PadRight(CARMConstants.TariffItemNumberLength, '0');
			}
		}
		string tariffItemNumber;

		[XmlElement(ElementName = "TariffTreatmentCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string TariffTreatmentCode
		{
			get => tariffTreatmentCode;
			set
			{
				var newValue = value;
				if (!string.IsNullOrEmpty(newValue) && newValue.Length > 2)
				{
					newValue = newValue.Substring(1, 2);
				}
				tariffTreatmentCode = newValue;
			}
		}
		string tariffTreatmentCode { get; set; }
		[XmlElement(ElementName = "CustomsDutyValidStartDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime CustomsDutyValidStartDate
		{
			get => customsDutyValidStartDate;
			set
			{
				customsDutyValidStartDate = ParserHelper.ParseDateSafe(value);
			}
		}
		DateTime customsDutyValidStartDate;

		[XmlElement(ElementName = "CustomsDutyValidEndDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime CustomsDutyValidEndDate
		{
			get => customsDutyValidEndDate;
			set
			{
				customsDutyValidEndDate = ParserHelper.ParseDateSafe(value, true);
			}
		}
		DateTime customsDutyValidEndDate { get; set; }

		[XmlElement(ElementName = "SpecificRateRegValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double SpecificRateRegValue { get; set; }
		[XmlElement(ElementName = "SpecificRateRegQualifierCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string SpecificRateRegQualifierCode { get; set; }
		[XmlElement(ElementName = "AdValoremRateMinValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double AdValoremRateMinValue { get; set; }
		[XmlElement(ElementName = "AdValoremRateMinQualifierCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string AdValoremRateMinQualifierCode { get; set; }
		[XmlElement(ElementName = "AdValoremRateMaxValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double AdValoremRateMaxValue { get; set; }
		[XmlElement(ElementName = "AdValoremRateMaxQualifierCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string AdValoremRateMaxQualifierCode { get; set; }
		[XmlElement(ElementName = "AdValoremRateRegValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double AdValoremRateRegValue { get; set; }
		[XmlElement(ElementName = "AdValoremRateRegQualifierCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string AdValoremRateRegQualifierCode { get; set; }
		[XmlElement(ElementName = "SpecificRateMinValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double SpecificRateMinValue { get; set; }
		[XmlElement(ElementName = "SpecificRateMinQualifierCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string SpecificRateMinQualifierCode { get; set; }
		[XmlElement(ElementName = "SpecificRateMaxValue", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public double SpecificRateMaxValue { get; set; }
		[XmlElement(ElementName = "SpecificRateMaxQualifierCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string SpecificRateMaxQualifierCode { get; set; }
		[XmlElement(ElementName = "UnitOfMeasureCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string UnitOfMeasureCode { get; set; }

		public override bool IsValid => !string.IsNullOrWhiteSpace(TariffItemNumber);

		public RefCusRate ParseToRefCusRate()
		{
			RefCusRate rate = null;
			if (IsValid)
			{
				rate = new RefCusRate
				{
					ZZ2_RateFormula = GenerateFormula(),
					ZZ2_StartDate = CustomsDutyValidStartDate,
					ZZ2_EndDate = CustomsDutyValidEndDate,
					ZZ2_ZY1_NKRateCode = Constants.RateTypeDty,
					ZZ2_ZY1_ZZR_NKRateType = Constants.RateTypeDty,
					ZZ2_ZZS_NKPreference = TariffTreatmentCode,
					ZZ2_ZZZ_NKDataGrouping = Constants.DefaultValues.CountryCodeCanada
				};
				rate.RefCusApplicabilities = new RefCusApplicability[] {new RefCusApplicability
				{
					ZZT_StartDate = CustomsDutyValidStartDate,
					ZZT_EndDate = CustomsDutyValidEndDate,
					ZZT_ZZA_NKTradeGroup = TariffTreatmentCode
				}};
			}
			return rate;
		}

		//MIN(AdMAX, MAX(SpReg,AdMIN)) + MIN(SpMAX, MAX(AdReg,SpMIN))
		string GenerateFormula()
		{
			var result = string.Empty;
			var specificRegFormula = ParserHelper.GenerateRateFormula(SpecificRateRegValue, SpecificRateRegQualifierCode, UnitOfMeasureCode);
			var specificMinFormula = ParserHelper.GenerateRateFormula(SpecificRateMinValue, SpecificRateMinQualifierCode, UnitOfMeasureCode);
			var specificMaxFormula = ParserHelper.GenerateRateFormula(SpecificRateMaxValue, SpecificRateMaxQualifierCode, UnitOfMeasureCode);

			var adValoremRegFormula = ParserHelper.GenerateRateFormula(AdValoremRateRegValue, AdValoremRateRegQualifierCode, UnitOfMeasureCode);
			var adValoremMinFormula = ParserHelper.GenerateRateFormula(AdValoremRateMinValue, AdValoremRateMinQualifierCode, UnitOfMeasureCode);
			var adValoremMaxFormula = ParserHelper.GenerateRateFormula(AdValoremRateMaxValue, AdValoremRateMaxQualifierCode, UnitOfMeasureCode);

			var left = GenerateCombinedRateFormula(adValoremMaxFormula, specificRegFormula, adValoremMinFormula);
			var right = GenerateCombinedRateFormula(specificMaxFormula, adValoremRegFormula, specificMinFormula);

			result = IsEmptyFormula(left) ? right : (IsEmptyFormula(right) ? left : $"{left} + {right}");
			return IsEmptyFormula(result) ? "0" : result;
		}

		string GenerateCombinedRateFormula(string max, string reg, string min)
		{
			var formula = IsEmptyFormula(reg) ? min : IsEmptyFormula(min) ? reg : $"MAX({reg},{min})";
			return IsEmptyFormula(formula) ? max : IsEmptyFormula(max) ? formula : $"MIN({max}, {formula})";
		}

		bool IsEmptyFormula(string formula)
		{
			return string.IsNullOrEmpty(formula) || formula == "0";
		}
	}
}
