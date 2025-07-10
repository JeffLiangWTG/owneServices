using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	[Serializable]
	public class NZTariffProcessingData
	{
		public DateTime LastRunDateTariff { get; set; }
	}

	[Serializable]
	public class NZConcessionProcessingData
	{
		public DateTime LastRunDateConcession { get; set; }
	}
}
