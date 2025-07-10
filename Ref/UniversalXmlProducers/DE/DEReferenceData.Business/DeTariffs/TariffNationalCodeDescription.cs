using System;
using System.Globalization;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs;

record TariffNationalCodeDescription
{
	public string Sid_Wn { get; set; }
	public string Sid { get; set; }
	public string Description { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string UpdateType { get; set; }

	public static TariffNationalCodeDescription Parse(XElement element)
	{
		var maxSupportedEndDate = new DateTime(2079, 06, 06);
		if (!DateTime.TryParse(element.Element("DAT_END")?.Value, CultureInfo.InvariantCulture,
				out var endDate))
		{
			endDate = maxSupportedEndDate;
		}

		if (endDate > maxSupportedEndDate)
		{
			endDate = maxSupportedEndDate;
		}

		return new TariffNationalCodeDescription
		{
			Sid_Wn = element.Element("WAREN_NOM_SID")?.Value,
			Sid = element.Element("WAREN_NOM_BESCHR_SID")?.Value,
			Description = element.Element("LANG_BESCHR")?.Value,
			UpdateType = element.Element("AEND_ART")?.Value ?? DeTariffsConstants.InsertFlag,
			StartDate = DateTime.Parse(element.Element("DAT_START")!.Value, CultureInfo.InvariantCulture),
			EndDate = endDate,
		};
	}
}

