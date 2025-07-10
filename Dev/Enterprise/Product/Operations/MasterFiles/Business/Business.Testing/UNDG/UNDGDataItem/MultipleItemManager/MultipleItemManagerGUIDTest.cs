using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MultipleItemManagerGUID<OrgContact>))]
	public class MultipleItemManagerGUIDTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			testcontact = Factory.NewWithValidTestData<OrgContact>();
			testcontact.OC_ContactName = "OwlBear";

			var dummy = Factory.NewWithValidTestData<OrgSupplierPart>();
			var collection = new UNDGDataItemCollection(dummy);

			var manager = new MultipleItemManagerGUID<OrgContact>(collection, UNDGDataItemSchema.DI_OC_DGContact, Factory);

			return manager;
		}

		public void TestRelatedBusinessObject()
		{
			var manager = GetNewBusinessObject() as MultipleItemManagerGUID<OrgContact>;
			manager.Value = testcontact.PK;

			Factory.Save();
			AssertEquals("Related BusinessObject should resolve", manager.RelatedBusinessObject.OC_ContactName, "OwlBear");
		}

		OrgContact testcontact;
	}
}
