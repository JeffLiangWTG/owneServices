using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgWhsClientAccountAssociationCollection))]
	sealed class OrgWhsClientAccountAssociationCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgWhsClientAccountAssociationCollection>
	{
		protected override OrgWhsClientAccountAssociationCollection GetCollectionToTest()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			return new OrgWhsClientAccountAssociationCollection(org1);
		}

		public void TestOrgWhsClientAccountAssociationCollection()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierAccount = carrier.CarrierAccounts.AddNew();

			var clientAccountAssociation = client.OrgWhsClientAccountAssociations.AddNew();
			clientAccountAssociation.OWC_OAN_CarrierAccount = carrierAccount.PK;

			var collection = new OrgWhsClientAccountAssociationCollection(client);
			AssertEquals(1, collection.Count);
		}
	}
}
