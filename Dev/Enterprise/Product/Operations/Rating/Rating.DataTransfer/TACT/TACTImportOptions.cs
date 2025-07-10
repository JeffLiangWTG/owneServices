using System;

namespace Enterprise.Rating.DataTransfer.TACT
{
	public class TACTImportOptions
	{
		public Guid RatingHeaderPK { get; set; }
		public Guid CompanyPK { get; set; }
		public bool IsJobLevelCharge { get; set; }
		public string Rounding { get; set; }
		public bool ShouldExcludeFromAutoRating { get; set; }
	}
}
