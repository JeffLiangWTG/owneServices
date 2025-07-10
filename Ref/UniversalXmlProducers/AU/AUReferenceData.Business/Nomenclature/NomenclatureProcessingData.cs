using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	[Serializable]
	public class NomenclatureProcessingData
	{	
		public string AUNomenclatureParserLastRun { get; set; }
		public int AUNomenclatureRetryCount { get; set; }
	}
}
