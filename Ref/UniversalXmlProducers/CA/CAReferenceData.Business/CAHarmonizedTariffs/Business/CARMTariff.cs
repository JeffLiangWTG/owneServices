using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	[XmlRoot(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
	public class CARMTariff : CARMContentProperties
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

		[XmlElement(ElementName = "TariffNumberValidStartDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime TariffNumberValidStartDate
		{
			get => tariffNumberValidStartDate;
			set
			{
				tariffNumberValidStartDate = ParserHelper.ParseDateSafe(value);
			}
		}
		DateTime tariffNumberValidStartDate;

		[XmlElement(ElementName = "TariffNumberValidEndDate", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime TariffNumberValidEndDate
		{
			get => tariffNumberValidEndDate;
			set
			{
				tariffNumberValidEndDate = ParserHelper.ParseDateSafe(value, true);
			}
		}
		DateTime tariffNumberValidEndDate;

		[XmlElement(ElementName = "UnitOfMeasureCode", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string UnitOfMeasureCode { get; set; }
		[XmlElement(ElementName = "Description", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public string Description { get; set; }

		public override bool IsValid => !string.IsNullOrWhiteSpace(TariffNumber);

		public RefCusTariff ParseToRefCusTariff()
		{
			RefCusTariff tariff = null;
			if (IsValid)
			{
				tariff = new RefCusTariff
				{
					ZZ1_TariffCode = TariffNumber,
					ZZ1_StartDate = TariffNumberValidStartDate,
					ZZ1_EndDate = TariffNumberValidEndDate,
					ZZ1_Description = ParserHelper.RemoveNewLines(Description.Substring(0, Math.Min(3999, Description.Length)))
				};

				ParserHelper.AddUOM(UnitOfMeasureCode, tariff);
				ParserHelper.MarkConveyanceTariffs(tariff);
			}
			return tariff;
		}
	}

}
