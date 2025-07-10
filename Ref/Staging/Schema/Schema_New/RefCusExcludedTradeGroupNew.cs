using System;
using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public partial class RefCusExcludedTradeGroupNew
	{
		public RefCusExcludedTradeGroupNew() { }

		public RefCusExcludedTradeGroupNew(RefCusExcludedTradeGroup ex)
		{
			S03_ZZA_NKTradeGroup = ex.ZZC_ZZA_NKTradeGroup;
			S03_ZZA_ZZZ_NKDataGrouping = ex.ZZC_ZZA_ZZZ_NKDataGrouping;
		}

		public Guid S03_PK { get; set; }
		public Guid? S03_S01_RateApplicability { get; set; }
		public Guid? S03_S07_ConditionApplicability { get; set; }
		public string S03_ZZA_NKTradeGroup { get; set; }
		public string S03_ZZA_ZZZ_NKDataGrouping { get; set; }
	}
}
