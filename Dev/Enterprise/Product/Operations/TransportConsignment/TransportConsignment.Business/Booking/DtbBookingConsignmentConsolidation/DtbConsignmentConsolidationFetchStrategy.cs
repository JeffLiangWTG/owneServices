using CargoWise.Application;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentConsolidationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbConsignmentConsolidationFetchStrategy(DtbConsignmentConsolidation consolidation)
			: base(consolidation)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			// the route planner starts from confirmations --> instructions --> consignment --> booking (for the booking ID)
			Factory.AddFetchHint(ObjectFactory.GetType<IDtbBooking>(), Consolidation.KB_ParentID);
		}

		DtbConsignmentConsolidation Consolidation
		{
			get { return (DtbConsignmentConsolidation)BusinessObject; }
		}
	}
}
