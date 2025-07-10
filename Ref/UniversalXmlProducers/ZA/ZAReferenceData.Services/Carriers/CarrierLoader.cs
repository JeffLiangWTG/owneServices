using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public class CarrierLoader : BaseLoader<CarrierData>
	{
		readonly Dictionary<string, CarrierMapping> fileMappings;

		public CarrierLoader(string urls) : base(urls)
		{
			fileMappings = GetFileMappings();
		}

		protected static Dictionary<string, CarrierMapping> GetFileMappings()
		{
			return new Dictionary<string, CarrierMapping>
			{
				{ "CargoCarrierAir.csv", new CarrierMapping { IsAir = true, Attributes = new List<string>() { Constants.Attributes.CargoCarrier } } },
				{ "CargoCarrierSea.csv", new CarrierMapping { IsSea = true, Attributes = new List<string>() { Constants.Attributes.CargoCarrier } } },
				{ "MasterCargoCarrierSea.csv", new CarrierMapping { IsSea = true, Attributes = new List<string>() { Constants.Attributes.Master } } }
			};
		}

		protected override List<CarrierData> ExtractCsv(string fileName, List<string[]> csvData)
		{
			var carriers = new List<CarrierData>();

			fileMappings.TryGetValue(fileName, out var mapping);

			foreach (var line in csvData)
			{
				if (line?.Length >= 2)
				{
					carriers.Add(new CarrierData(line[0], line[1], mapping.IsAir, mapping.IsSea , mapping.Attributes));
				}
			}

			return carriers;
		}

		protected override List<string> GetFileNamesToDownload() => fileMappings.Keys.ToList();

		protected override void MergeData(List<CarrierData> currentData, List<CarrierData> newData)
		{
			foreach (var item in newData)
			{
				var existing = currentData.FirstOrDefault(x => x.CarrierCode == item.CarrierCode);

				if (existing != null)
				{
					existing.IsAir |= item.IsAir;
					existing.IsSea |= item.IsSea;
					existing.MergeAttributes(item.Attributes);
				}
				else
				{
					currentData.Add(item);
				}
			}
		}
	}

	public class CarrierMapping
	{
		public bool IsAir { get; set; }
		public bool IsSea { get; set; }
		public List<string> Attributes { get; set; }
	}
}
