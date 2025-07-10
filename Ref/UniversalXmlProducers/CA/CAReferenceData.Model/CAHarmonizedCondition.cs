using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class CAHarmonizedCondition
	{
		public string HSCodeFrom { get; set; }
		public string HSCodeTo { get; set; }
		public List<PGA> PGAs { get; set; }
	}

	public class PGA
	{
		public string PGACode { get; set; }
		public string Program { get; set; }
	}
}



