using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class OrganizationLocalAddressDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestLocalAddresses()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var writer = new OrganizationLocalAddressDataObjectWriter(writeManager);

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.CompanyName = "Test Company Name";
			translatedAddress.Address1 = "Test address 1";
			translatedAddress.Address2 = "Test address 2";
			translatedAddress.City = "Test city";
			translatedAddress.StateCode = "State Code 1";
			translatedAddress.Postcode = "Post code";
			translatedAddress.Language = "EN";

			var dataobject = writer.GetDataObject(translatedAddress);

			AssertEquals("Test Company Name", dataobject.CompanyName);
			AssertEquals("Test address 1", dataobject.Address1);
			AssertEquals("Test address 2", dataobject.Address2);
			AssertEquals("Test city", dataobject.City);
			AssertEquals("State Code 1", dataobject.State);
			AssertEquals("Post code", dataobject.Postcode);
			AssertEquals("EN", dataobject.Language.Code);
			AssertEquals("English", dataobject.Language.Description);
		}
	}
}
