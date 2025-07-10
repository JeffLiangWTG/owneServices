using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class BulkUpdateActionAddOrReplaceCharge : BulkUpdateActionReplaceCharge
	{
		public BulkUpdateActionAddOrReplaceCharge(BulkRateUpdater updater)
			: base(updater)
		{
		}

		protected override ZDBOnlySubQuery IncludeInActionSubQuery
		{
			get { return null; }
		}

		protected override void ApplyBulkUpdateActionCore(RateEntry entry)
		{
			var lines = GetRateLinesWithActionLineChargeCode(entry);
			if (!lines.Any())
			{
				AddRateLine(entry);
			}
			else
			{
				base.ApplyBulkUpdateActionCore(entry);
			}
		}

		void AddRateLine(RateEntry entry)
		{
			if (Updater.ActionsLine.ChargeCode != null)
			{
				var clone = Updater.ActionsLine.Clone(entry.RateLines);

				clone.RateCalculatorChanged = false;
				clone.RateLineItems.Clone(Updater.ActionsLine);
			}
		}
	}
}
