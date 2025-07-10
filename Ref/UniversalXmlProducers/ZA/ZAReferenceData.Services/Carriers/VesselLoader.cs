using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public class VesselLoader : BaseLoader<VesselData>
	{
		public VesselLoader(string baseUrl) : base(baseUrl)
		{
		}

		protected override List<string> GetFileNamesToDownload() => new List<string> { "transportsea.csv" };

		protected override List<VesselData> ExtractCsv(string fileName, List<string[]> csvData)
		{
			var results = new List<VesselData>();

			foreach (var line in csvData)
			{
				if (line?.Length >= 4)
				{
					var carrierName = line[3];

					for (int i = 4; i < line.Length; i++)
					{
						carrierName = $"{carrierName}{line[i]}";
					}

					results.Add(new VesselData(line[0], line[1], new CarrierData(line[2], RemoveSpecialCharacters(carrierName), false, true, vesselCarrierAttributes)));
				}
			}

			return results;
		}

		protected static List<string> vesselCarrierAttributes => new List<string> { Constants.Attributes.Master };

		protected static string RemoveSpecialCharacters(string str)
		{
			return Regex.Replace(str, "[^0-9a-zA-Z._()&, /-]", " ");
		}
	}
}
