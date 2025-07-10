using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CustomsBrokerDetailHelperTest : TestCaseWithFactory
	{
		public void TestCustomsBrokerDetails()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.Address1 = "BOB";
			var customsBrokerDetail = new CustomsBrokerDetailWrapper(orgHeader.MainAddress, "NAME", "PHONE", "EMAIL");
			var icustomsBrokerDetail = customsBrokerDetail as ICustomsBrokerDetails;

			AssertNotNull(icustomsBrokerDetail);
			AssertNotNull(icustomsBrokerDetail.Address);
			AssertEquals("BOB", icustomsBrokerDetail.Address.AddressLine1);
			AssertEquals("NAME", icustomsBrokerDetail.ContactName);
			AssertEquals("EMAIL", icustomsBrokerDetail.ContactEmail);
			AssertEquals("PHONE", icustomsBrokerDetail.ContactPhone);
		}
	}
}
