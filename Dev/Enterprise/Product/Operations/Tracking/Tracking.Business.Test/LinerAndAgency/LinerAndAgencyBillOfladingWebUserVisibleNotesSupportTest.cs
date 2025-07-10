using Enterprise.Freight.Agency.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class LinerAndAgencyBillOfladingWebUserVisibleNotesSupportTest : LinerAndAgencyBaseWebUserVisibleNotesSupportTest
	{
		protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
		{
			return new LinerAndAgencyBillOfLadingWebInterfacesHelper(BillOfLading);
		}

		protected override AgencyShipment GetBusinessObjectForHelper()
		{
			return Factory.NewWithValidTestData<BillOfLading>();
		}

		BillOfLading BillOfLading
		{
			get { return Shipment as BillOfLading; }
		}
	}
}
