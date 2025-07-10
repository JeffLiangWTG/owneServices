using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class RefCusTariffCodeSchema : JsonlSchema
	{
		public IEnumerable<TariffCode> value { get; set; }

		public class TariffCode
		{
			public string ZZ1_TariffCode { get; set; } = string.Empty;
		}
	}
}
