using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class RefCusMapUOMSchema : JsonlSchema
	{
		public IEnumerable<MapCodes> value { get; set; }

		public class MapCodes
		{
			public string ZZM_CW1orCommercialValue { get; set; } = string.Empty;
			public string ZZM_CustomsValue { get; set; } = string.Empty;
		}
	}
}
