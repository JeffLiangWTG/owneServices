using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs;

record TariffNationalCode
{
	public string Sid { get; set; }
	public string Sid_Wn { get; set; }
	public string TariffCode { get; set; }
	public List<TariffNationalCodeDescription> Description { get; set; } = [];
	public string NationalCode { get; set; }
	public string FullTariffCode { get; set; }
	public string Text { get; set; }
	public List<TariffVatCode> VatCode { get; set; } = [];
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string UpdateType { get; set; }

	public string Wl { get; set; }

	public bool IsWl80 => Wl == "80";

	public int Level => (12 - FullTariffCode.TrimEnd('0').Length) / 2;

	public IEnumerable<string> GetUpperLevelCodes()
	{
		for (int level = Level; level < 5; level++)
		{
			yield return FullTariffCode[..^(level * 2 + 1)].PadRight(11, '0');
		}
	}

	public static TariffNationalCode Parse(XElement element)
	{
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

		var fullCode = element.Element("WN_N_ID")!.Value;
		return new TariffNationalCode
		{
			TariffCode = fullCode[..10],
			NationalCode = fullCode[10..11],
			FullTariffCode = fullCode[..11],
			StartDate = DateTime.Parse(element.Element("BEGINNDATUM")!.Value, CultureInfo.InvariantCulture),
			Sid = element.Element("SID")?.Value,
			Sid_Wn = element.Element("SID_WN")?.Value,
			Text = element.Element("TEXT")?.Value,
			EndDate = endDate,
			UpdateType = element.Element("AEND_ART")?.Value ?? DeTariffsConstants.InsertFlag,
			Wl = element.Element("WL")?.Value
		};
	}
}

