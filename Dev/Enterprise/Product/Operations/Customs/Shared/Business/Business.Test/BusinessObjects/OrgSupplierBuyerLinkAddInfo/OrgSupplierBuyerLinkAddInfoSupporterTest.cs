using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLinkAddInfo))]
	sealed class OrgSupplierBuyerLinkAddInfoSupporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeserialiseOnConstruction()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var link = importer.SupplierLinks.AddNew(supplier);
			link.GetAddInfo().ZO_FirstSale = "Y";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var linkLoaded = factory2.Load<OrgSupplierBuyerLink>(link.PK);

			AssertNotNull(linkLoaded);
			var addInfoLoaded = linkLoaded.GetAddInfo();

			AssertEquals("Y", addInfoLoaded.ZO_FirstSale);
			AssertEquals(false, addInfoLoaded.HasChanges);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			return link.GetAddInfo();
		}
	}
}
