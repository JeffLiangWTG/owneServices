using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Builders.Testing
{
	sealed class CarrierMessageContactBuilderTest : TestCaseWithFactory
	{
		public void TestPopulatePhoneWithFallback()
		{
			var contactBO = Factory.New<OrgContact>();

			var contactBuilder = new CarrierMessageContactBuilder();
			var contact = contactBuilder.Build(contactBO);
			Assert("precondition", contact.Phone == ZString.Empty);

			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			parentOrg.MainAddress.OA_Mobile = "OA111111";
			parentOrg.Contacts.Add(contactBO);
			contact = contactBuilder.Build(contactBO);
			AssertEquals("contact phone fall back to parent orgnization main address mobile", "OA111111", contact.Phone);

			parentOrg.MainAddress.OA_Phone = "OA222222";
			contact = contactBuilder.Build(contactBO);
			AssertEquals("contact phone fall back to parent orgnization main address phone", "OA222222", contact.Phone);

			contactBO.OC_Mobile = "OA333333";
			contact = contactBuilder.Build(contactBO);
			AssertEquals("contact phone fall back to contact mobile", "OA333333", contact.Phone);

			contactBO.OC_Phone = "OA444444";
			contact = contactBuilder.Build(contactBO);
			AssertEquals("contact show phone when it is not empty", "OA444444", contact.Phone);
		}
	}
}
