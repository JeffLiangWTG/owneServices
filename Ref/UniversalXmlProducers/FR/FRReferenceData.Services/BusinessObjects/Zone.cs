using System.Globalization;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class Zone
	{
		public Zone(string code, string description, string percentOutEu, string percentInEU)
		{
			Code = code;
			Description = description;
			PercentOutEu = percentOutEu;
			PercentInEu = percentInEU;
		}

		public string Code { get; set; }

		public string Description { get; set; }

		public string PercentOutEu { get; set; }

		public string PercentInEu { get; set; }

		public string PercentDomestic => (100 - int.Parse(PercentOutEu, CultureInfo.InvariantCulture) - int.Parse(PercentInEu, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);
	}
}
