using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	public class AgencyDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObject_Null()
		{
			AssertNull(new AgencyDocDataObjectProvider().GetDocDataObject(Factory.New<AgencyShipment>(), "Test Data Context", null));
		}

		public void TestGetDocDataObject_BillOfLading_HouseBill()
		{
			AssertType<AgencyHouseBill>(new AgencyDocDataObjectProvider().GetDocDataObject(Factory.New<BillOfLading>(), DataContext.HouseBill, null));
		}
	}
}
