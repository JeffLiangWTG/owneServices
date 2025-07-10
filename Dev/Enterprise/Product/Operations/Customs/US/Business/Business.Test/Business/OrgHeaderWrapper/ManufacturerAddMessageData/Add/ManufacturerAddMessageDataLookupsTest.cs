using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class ManufacturerAddMessageDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryList()
		{
			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(Factory.New<OrgHeader>()));
			AssertEquals(typeof(USCCountryCollection), new ManufacturerAddMessageDataLookups(messageData).CountryList.GetType());
		}
	}
}
