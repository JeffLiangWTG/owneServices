using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public  class ETradeExemptionCodes
	{
		public string ExemptionCode { get; set; }
		
		public string ExemptionDescEnglish { get; set; }

		public string DutyPercent { get; set; }

		public string DutyFormula { get; set; }

		public string ExemptionDescTurkish { get; set; }

		public string RateCode { get; set; }

		public string RateType { get; set; }

		public string TradeGroup { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }
	}
}
