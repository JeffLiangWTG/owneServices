using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyBillOfLadingWebInterfacesHelper))]
	sealed class LinerAndAgencyBillOfLadingWebDocumentsTest : LinerAndAgencyBaseWebDocumentsTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
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
