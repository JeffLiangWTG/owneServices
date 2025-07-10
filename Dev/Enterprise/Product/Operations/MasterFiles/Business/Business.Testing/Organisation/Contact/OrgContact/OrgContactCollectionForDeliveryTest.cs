using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactCollectionForDelivery))]
	sealed class OrgContactCollectionForDeliveryTest : BusinessObjectCollectionViewTestCase<OrgContactCollectionForDelivery>
	{
		protected override OrgContactCollectionForDelivery GetCollectionToTest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.Contacts.AddNew();
			orgHeader.Contacts.AddNew();
			var contactCollection = new OrgContactDependentCollection(orgHeader, Factory);
			contactCollection.Load();
			return new OrgContactCollectionForDelivery(contactCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgContact>();
		}
	}
}
