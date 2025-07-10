using System.Collections.Generic;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class CessGroupedChunk
	{
		public decimal StartX { get; set; }
		public decimal EndX { get; set; }
		public decimal Y { get; set; }
		public List<string> Words { get; set; }
	}
}
