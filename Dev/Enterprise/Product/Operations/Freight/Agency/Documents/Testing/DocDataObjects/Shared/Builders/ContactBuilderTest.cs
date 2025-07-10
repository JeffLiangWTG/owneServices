using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class ContactBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var contactBO = Factory.New<OrgContact>();
			contactBO.OC_ContactName = "Mr X";
			contactBO.OC_Email = "x@x.com";
			contactBO.OC_Phone = "12345";
			contactBO.OC_Fax = "67890";

			var contact = new ContactBuilder().Build(contactBO);

			AssertEquals(contactBO.OC_ContactName, contact.FullName);
			AssertEquals(contactBO.OC_Phone, contact.Phone);
			AssertEquals(contactBO.OC_Email, contact.Email);
		}
	}
}
