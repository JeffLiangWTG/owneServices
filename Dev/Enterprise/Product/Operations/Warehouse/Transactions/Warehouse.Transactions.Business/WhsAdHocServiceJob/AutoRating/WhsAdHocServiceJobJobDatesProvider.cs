using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobJobDatesProvider : JobDatesProvider<WhsAdHocServiceJob>
	{
		public WhsAdHocServiceJobJobDatesProvider(WhsAdHocServiceJob adHocServiceJob)
			: base(adHocServiceJob)
		{
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return GetAdHocServiceJobJobDate();
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return GetAdHocServiceJobJobDate();
		}

		ZDateTime GetAdHocServiceJobJobDate()
		{
			return Parent.BillingDate.IsValid ? Parent.BillingDate : ZDateTime.Today;
		}
	}
}
