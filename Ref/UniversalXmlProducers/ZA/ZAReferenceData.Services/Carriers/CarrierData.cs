using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public class CarrierData
	{
		public CarrierData(string carrierCode, string carrierName, bool isAir, bool isSea, IReadOnlyCollection<string> attributes)
		{
			CarrierCode = carrierCode.Trim();
			CarrierName = carrierName.Trim();
			IsAir = isAir;
			IsSea = isSea;
			attributesList.AddRange(attributes);
		}

		public string CarrierCode { get; private set; }
		public string CarrierName { get; private set; }
		public bool IsAir { get; set; }
		public bool IsSea { get; set; }
		public IReadOnlyCollection<string> Attributes => attributesList;
		List<string> attributesList = new List<string>();

		public void MergeAttributes(IReadOnlyCollection<string> attributes)
		{
			foreach (var attribute in attributes)
			{
				if (!attributesList.Contains(attribute))
				{
					attributesList.Add(attribute);
				}
			}
		}
	}
}
