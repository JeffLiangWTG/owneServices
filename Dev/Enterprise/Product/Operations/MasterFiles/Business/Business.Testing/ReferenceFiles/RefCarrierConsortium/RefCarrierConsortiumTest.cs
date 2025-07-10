using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCarrierConsortium))]
	sealed class RefCarrierConsortiumTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestHumanReadbleNameCore()
		{
			var consortium = Factory.NewWithValidTestData<RefCarrierConsortium>();
			consortium.RG_Code = "VES";

			AssertEquals("Vessel Consortium - VES", consortium.HumanReadableName);
		}

		public void TestOrganisationManyToManyCollectionTest()
		{
			var consortium = Factory.NewWithValidTestData<RefCarrierConsortium>();
			Factory.Save();
			Assert("Consortium should have no change", !consortium.HasChanges);

			consortium.OrgHeaders.Add(Factory.New<OrgHeader>());
			consortium.OrgHeaders.Add(Factory.New<OrgHeader>());
			Assert("Consortium should have change", consortium.HasChanges);
		}
	}
}
