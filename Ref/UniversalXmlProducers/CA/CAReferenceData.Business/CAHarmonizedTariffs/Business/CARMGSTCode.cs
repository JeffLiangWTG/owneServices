using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;


namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	[XmlRoot(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
	public class CARMGSTCode : CARMContentProperties
	{

		[XmlElement(ElementName = "GSTCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string GSTCode
		{
			get => gstCode;
			set
			{
				gstCode = string.IsNullOrEmpty(value) ? string.Empty : value.PadLeft(CARMConstants.GSTCodeLength, '0');
			}
		}
		string gstCode;

		[XmlElement(ElementName = "GSTCodeValidStartDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime GSTCodeValidStartDate
		{
			get => gstCodeValidStartDate;
			set
			{
				gstCodeValidStartDate = ParserHelper.ParseDateSafe(value);
			}
		}
		DateTime gstCodeValidStartDate;

		[XmlElement(ElementName = "GSTCodeValidEndDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime GSTCodeValidEndDate
		{
			get => gstCodeValidEndDate;
			set
			{
				gstCodeValidEndDate = ParserHelper.ParseDateSafe(value, true);
			}
		}
		DateTime gstCodeValidEndDate;

		[XmlElement(ElementName = "ExciseTaxRateCheckIndicator", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string ExciseTaxRateCheckIndicator
		{
			get => exciseTaxRateCheckIndicator;
			set
			{
				exciseTaxRateCheckIndicator = string.IsNullOrEmpty(value) ? string.Empty : (value.Equals("true", StringComparison.Ordinal) || value.Equals("Y", StringComparison.Ordinal) ? "Y" : "N");
			}
		}
		string exciseTaxRateCheckIndicator;

		[XmlElement(ElementName = "GSTCheckGroup", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string GSTCheckGroup { get; set; }

		[XmlElement(ElementName = "GSTRateType", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string GSTRateType { get; set; }

		[XmlElement(ElementName = "Description", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string Description { get; set; }

		public override bool IsValid => !string.IsNullOrWhiteSpace(GSTCode);

		public string Rate { get; set; }

		public RefCusCodeList ParseToRefCusCodeList()
		{
			RefCusCodeList refCusCode = null;
			if (!string.IsNullOrWhiteSpace(GSTCode))
			{
				refCusCode = new RefCusCodeList
				{
					ZZD_Code = GSTCode,
					ZZD_Description = ParserHelper.RemoveNewLines(Description.Substring(0, Math.Min(3999, Description.Length))),
					ZZD_EndDate = GSTCodeValidEndDate,
					ZZD_StartDate = GSTCodeValidStartDate
				};
				var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
				AddAttribute(Constants.AttributeName.CheckIndicator, ExciseTaxRateCheckIndicator);
				AddAttribute(Constants.AttributeName.CheckGroup, GSTCheckGroup);
				AddAttribute(Constants.AttributeName.RateType, GSTRateType);
				AddAttribute(Constants.AttributeName.Rate, Rate);

				void AddAttribute(string name, string value)
				{
					if (!string.IsNullOrWhiteSpace(value))
					{
						if (refCusCodeListAttributes == null)
						{
							refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
						}
						refCusCodeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = name, ZZE_Value = value });
					}
				}
				refCusCode.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
			}
			return refCusCode;
		}

		public void PopulateByCACTaxRate(CACTaxRate taxRate)
		{
			GSTCode = taxRate.TaxRefNumber;
			GSTCodeValidStartDate = taxRate.EffectiveDate;
			GSTCodeValidEndDate = taxRate.ExpiryDate;
			ExciseTaxRateCheckIndicator = taxRate.CheckInd;
			GSTCheckGroup = taxRate.CheckGroup;
			GSTRateType = taxRate.RateType;
			Description = taxRate.Title;
			Rate = taxRate.Rate;
		}
	}

}
