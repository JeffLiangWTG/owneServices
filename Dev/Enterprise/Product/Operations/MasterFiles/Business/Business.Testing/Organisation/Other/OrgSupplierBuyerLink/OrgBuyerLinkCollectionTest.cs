using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgBuyerLinkCollection))]
	sealed class OrgBuyerLinkCollectionTest : OrgSupplierBuyerLinkDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			return new OrgBuyerLinkCollection(supplier, Factory);
		}

		public void TestUniquenessValidationUpdatedOnDelete()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			OrgHeader buyer1 = Factory.New<OrgHeader>();
			buyer1.OH_IsConsignee = true;

			OrgBuyerLinkCollection collection = supplier.BuyerLinks;
			OrgSupplierBuyerLink link1 = collection.AddNew(buyer1);
			link1.OL_RN_NKImporterCountry = "AU";
			Assert("Only 1 relationship, so no errors", !link1.HasRowErrors);

			OrgSupplierBuyerLink link2 = collection.AddNew(buyer1);
			link2.OL_RN_NKImporterCountry = "AU";
			Assert("Error shown on Link2", !link1.HasRowErrors);
			Assert("2 relationships entered with same transport mode, container mode & importer country", link2.HasRowErrors);

			collection.RemoveAndDelete(link2);
			Assert("Link is now unique, should not have errors", !link1.HasRowErrors);
		}

		public void TestCheckTransportImportCountryContainerModeUnique()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader supplier = factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			OrgHeader buyer1 = factory.New<OrgHeader>();
			buyer1.OH_IsConsignee = true;
			OrgHeader buyer2 = factory.New<OrgHeader>();
			buyer2.OH_IsConsignee = true;

			OrgBuyerLinkCollection collection = supplier.BuyerLinks;
			OrgSupplierBuyerLink link1 = collection.AddNew(buyer1);
			link1.OL_RN_NKImporterCountry = "AU";
			Assert("Only 1 relationship, so no errors", !link1.HasRowErrors);

			OrgSupplierBuyerLink link2 = collection.AddNew(buyer1);
			link2.OL_RN_NKImporterCountry = "AU";
			AssertNoErrors("Link1 has no errors", link1);
			Assert("2 relationships entered with same transport mode, container mode & importer country", link2.HasErrors);

			collection.RunPreSaveValidation();
			Assert("Link1 has errors", link1.HasErrors);
			Assert("Link2 has errors", link2.HasErrors);

			link2.OL_RN_NKImporterCountry = "SG";
			collection.RunPreSaveValidation();
			AssertNoErrors("2 different Importer countries, should not have errors", link1);
			AssertNoErrors("2 different Importer countries, should not have errors", link2);

			link2.OL_RN_NKImporterCountry = "AU";
			collection.RunPreSaveValidation();
			Assert("Importer countries are the same, should have errors", link1.HasErrors);
			Assert("Importer countries are the same, should have errors", link2.HasErrors);
		}

		public void TestAddNewItem()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader buyer = Factory.New<OrgHeader>();

			OrgSupplierBuyerLink noBuyer = org.BuyerLinks.AddNew();
			AssertEquals("OL_OH_Buyer", Guid.Empty, noBuyer.OL_OH_Buyer);

			OrgSupplierBuyerLink withBuyer = org.BuyerLinks.AddNew(buyer);
			AssertEquals("OL_OH_Buyer", buyer.PK, withBuyer.OL_OH_Buyer);
		}
	}
}
