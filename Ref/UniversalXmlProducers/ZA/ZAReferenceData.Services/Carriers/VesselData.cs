using System.Globalization;
using System.Linq;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public class VesselData
	{
		public VesselData(string radioCallSign, string vesselName, CarrierData carrier)
		{
			RadioCallSign = radioCallSign.Trim();
			VesselName = NormalizeName(vesselName);
			Carrier = carrier;
		}

		public string RadioCallSign { get; private set; }
		public string VesselName { get; private set; }
		public CarrierData Carrier { get; private set; }

		protected static string NormalizeName(string name)
		{
			var result = name.Trim().Normalize(System.Text.NormalizationForm.FormD);

			result = string.IsNullOrEmpty(result) ? null : new string(result.ToCharArray().Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());

			return result;
		}
	}
}
