using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class TRCustomsRefCusCodes
	{
		public string CodeType { get; set; }
		public string Code { get; set; }

		public string Description { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }
	}
}
