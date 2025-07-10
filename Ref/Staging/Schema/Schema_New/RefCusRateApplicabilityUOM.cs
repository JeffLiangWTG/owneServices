using System;
using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public partial class RefCusRateApplicabilityUOM
	{
		public RefCusRateApplicabilityUOM() { }

		public RefCusRateApplicabilityUOM(RefCusRateUOM uom) : this()
		{
			S02_UOM = uom.ZXG_UOM;
		}

		public Guid S02_PK { get; set; }
		public Guid S02_S01_RateApplicability { get; set; }
		public string S02_UOM { get; set; }

		public RefCusRateApplicability RefCusRateApplicability { get; set; }

	}
}
