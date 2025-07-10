namespace Enterprise.Rating.Business
{
	public class BulkUpdateActionDeleteCharge
		: BulkUpdateAction
	{
		public BulkUpdateActionDeleteCharge(BulkRateUpdater updater) : base(updater)
		{
		}

		protected override void ApplyBulkUpdateActionCore(RateEntry entry)
		{
			var rateLines = GetRateLinesWithActionLineChargeCode(entry);
			foreach (var line in rateLines)
			{
				entry.RateLines.RemoveAndDelete(line);
			}
		}
	}
}
