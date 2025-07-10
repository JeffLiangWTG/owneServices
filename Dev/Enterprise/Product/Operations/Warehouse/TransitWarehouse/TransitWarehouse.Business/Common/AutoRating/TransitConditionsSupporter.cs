using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitConditionsSupporter<T> : RateLineConditionsSupporter where T : BusinessObject, ITransitJobForRating
	{
		public TransitConditionsSupporter(BusinessObject parent) : base(parent)
		{
		}

		protected override OrgHeader GetArrivalCFS() => null;

		protected override OrgHeader GetControllingAgent() => null;

		protected override OrgHeader GetDepartureCFS() => null;

		protected override OrgHeader GetExportBroker() => null;

		protected override bool GetHasDangerousGoods() => ((ITransitJobForRating)ObjectToWrap).TransitPackagesForRating.Any(p => p.HasDangerousGoods);

		protected override OrgHeader GetImportBroker() => null;

		protected override OrgHeader GetReceivingAgent() => null;

		protected override OrgHeader GetSendingAgent() => null;
	}
}
