using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderJobDatesProvider : JobDatesProvider<WhsVASOrder>
	{
		public WhsVASOrderJobDatesProvider(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
		}

		protected override ZDateTime GetArrivalDateCore() => GetJobDate();

		protected override ZDateTime GetDepartureDateCore() => GetJobDate();

		ZDateTime GetJobDate()
		{
			var finalisedTimeUtc = Parent.WVO_FinalizedTimeUtc;
			return finalisedTimeUtc.IsValid ? finalisedTimeUtc.ToLocationTime(Parent.Warehouse.RelatedCompanyBranch?.HomePort).ToLocalZDateTime() : ZDateTime.Today;
		}
	}
}
