using System;
using System.Globalization;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs;

record TariffVatCode
{
	public string Sid { get; set; }
	public string Sid_Wn { get; set; }
	public string VatCode { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string UpdateType { get; set; }

	public static bool TryParse(XElement element, out TariffVatCode code)
	{
		code = null;
		if (element.Element("HINW_ART_ID")?.Value != "400")
		{
			return false;
		}

		var vatCode = element.Element("HINW_ID")!.Value switch
		{
			"011" => "REG",
			"012" => "KEI",
			"013" => "ERM",
			_ => null,
		};

		if (vatCode == null)
		{
			return false;
		}

		var maxSupportedEndDate = new DateTime(2079, 06, 06);
		if (!DateTime.TryParse(element.Element("ENDEDATUM")?.Value, CultureInfo.InvariantCulture,
				out var endDate))
		{
			endDate = maxSupportedEndDate;
		}

		if (endDate > maxSupportedEndDate)
		{
			endDate = maxSupportedEndDate;
		}


		code = new TariffVatCode
		{
			Sid = element.Element("SID")?.Value,
			Sid_Wn = element.Element("SID_WN_N")?.Value,
			VatCode = vatCode,
			UpdateType = element.Element("AEND_ART")?.Value ?? DeTariffsConstants.InsertFlag,
			StartDate = DateTime.Parse(element.Element("BEGINNDATUM")!.Value, CultureInfo.InvariantCulture),
			EndDate = endDate,
		};

		return true;
	}
}

