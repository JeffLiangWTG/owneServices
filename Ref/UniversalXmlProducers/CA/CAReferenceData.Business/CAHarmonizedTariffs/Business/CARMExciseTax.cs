using System;
using System.Xml;
using System.Xml.Serialization;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	[XmlRoot(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
	public class CARMExciseTax : CARMContentProperties
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

		[XmlElement(ElementName = "ExciseTaxCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string ExciseTaxCode { get; set; }

		[XmlElement(ElementName = "ExciseTaxCodeValidStartDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime ExciseTaxCodeValidStartDate
		{
			get => exciseTaxCodeValidStartDate;
			set
			{
				exciseTaxCodeValidStartDate = ParserHelper.ParseDateSafe(value);
			}
		}
		DateTime exciseTaxCodeValidStartDate;

		[XmlElement(ElementName = "ExciseTaxCodeValidEndDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime ExciseTaxCodeValidEndDate
		{
			get => exciseTaxCodeValidEndDate;
			set
			{
				exciseTaxCodeValidEndDate = ParserHelper.ParseDateSafe(value, true);
			}
		}
		DateTime exciseTaxCodeValidEndDate;

		public override bool IsValid => !string.IsNullOrWhiteSpace(TariffNumber) && !string.IsNullOrWhiteSpace(ExciseTaxCode);
	}

}
