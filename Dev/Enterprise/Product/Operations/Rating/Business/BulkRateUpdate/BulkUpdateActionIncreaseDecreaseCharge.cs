namespace Enterprise.Rating.Business
{
	public class BulkUpdateActionIncreaseDecreaseCharge : BulkUpdateAction
	{
		public BulkUpdateActionIncreaseDecreaseCharge(BulkRateUpdater updater)
			: base(updater)
		{
		}

		protected override void ApplyBulkUpdateActionCore(RateEntry entry)
		{
			var rateLines = GetRateLinesWithActionLineChargeCode(entry);
			foreach (var line in rateLines)
			{
				if (line != null && Updater.ActionsLine.UsesCostBasedCalculator())
				{
					var cloneHelper = new CompanyTariffOrCostLineCloneHelper(Updater.ActionsLine);
					var clonedLine = cloneHelper.CreateClone(line, Updater.Factory, Updater.SelectedRatesTypeToUpdate);
					if (clonedLine != null)
					{
						line.TL_RateCalculator = clonedLine.TL_RateCalculator;
						line.RateCalculatorChanged = false;
						line.RateLineItems.Clone(clonedLine);
					}
				}
			}
		}
	}
}
