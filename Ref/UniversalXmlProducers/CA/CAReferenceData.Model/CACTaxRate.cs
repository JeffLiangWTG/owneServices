using System;
using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class CACTaxRate
	{
		[Name("ZH_TaxRefNumber")]
		public string TaxRefNumber { get; set; }

		[Name("ZH_EffectiveDate")]
		public DateTime EffectiveDate { get; set; }

		[Name("ZH_ExpiryDate")]
		public DateTime ExpiryDate { get; set; }

		[Name("ZH_CheckInd")]
		public string CheckInd { get; set; }

		[Name("ZH_CheckGroup")]
		public string CheckGroup { get; set; }

		[Name("ZH_RateType")]
		public string RateType { get; set; }

		[Name("ZH_Rate")]
		public string Rate { get; set; }

		[Name("ZH_Title")]
		public string Title { get; set; }
	}
}
