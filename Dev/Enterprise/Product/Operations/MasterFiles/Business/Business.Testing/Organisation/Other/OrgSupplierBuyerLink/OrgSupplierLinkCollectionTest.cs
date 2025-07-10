using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierLinkCollection))]
	sealed class OrgSupplierLinkCollectionTest : OrgSupplierBuyerLinkDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			return new OrgSupplierLinkCollection(buyer, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(OrgSupplierBuyerLink));
		}

		public void TestUniquenessValidationUpdatedOnDelete()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_IsConsignor = true;
			OrgHeader supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_IsConsignee = true;

			OrgSupplierLinkCollection collection = buyer.SupplierLinks;
			OrgSupplierBuyerLink link1 = collection.AddNew(supplier1);
			link1.OL_RN_NKImporterCountry = "NZ";
			Assert("Only 1 relationship, so no errors", !link1.HasRowErrors);

			OrgSupplierBuyerLink link2 = collection.AddNew(supplier1);
			link2.OL_RN_NKImporterCountry = "NZ";
			Assert("Error Shown On Link2", !link1.HasRowErrors);
			Assert("2 relationships entered with same transport mode, container mode & importer country", link2.HasRowErrors);

			collection.RemoveAndDelete(link2);
			Assert("Link is now unique, should not have errors", !link1.HasRowErrors);
		}

		public void TestCheckTransportImportCountryContainerModeUnique()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_IsConsignor = true;
			OrgHeader supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_IsConsignee = true;
			OrgHeader supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_IsConsignee = true;

			OrgSupplierLinkCollection collection = buyer.SupplierLinks;
			OrgSupplierBuyerLink link1 = collection.AddNew(supplier1);
			link1.OL_RN_NKImporterCountry = "NZ";
			Assert("Only 1 relationship, so no errors", !link1.HasRowErrors);

			OrgSupplierBuyerLink link2 = collection.AddNew(supplier1);
			link2.OL_RN_NKImporterCountry = "NZ";
			Assert("No Error Shown on Link2", !link1.HasRowErrors);
			Assert("2 relationships entered with same transport mode, container mode & importer country", link2.HasRowErrors);

			link2.OL_RN_NKImporterCountry = "SG";
			collection.RunPreSaveValidation();
			Assert("2 different Importer countries, should not have errors", !link1.HasRowErrors);
			Assert("2 different Importer countries, should not have errors", !link2.HasRowErrors);

			link2.OL_RN_NKImporterCountry = "NZ";
			collection.RunPreSaveValidation();
			Assert("Error shown on Link1", link1.HasRowErrors);
			Assert("Importer countries are the same, should have errors", link2.HasRowErrors);
		}

		public void TestAddNewItem()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader supplier = Factory.New<OrgHeader>();

			OrgSupplierBuyerLink noSupplier = org.SupplierLinks.AddNew();
			AssertEquals("OL_OH_Supplier", Guid.Empty, noSupplier.OL_OH_Supplier);

			OrgSupplierBuyerLink withSupplier = org.SupplierLinks.AddNew(supplier);
			AssertEquals("OL_OH_Supplier", supplier.PK, withSupplier.OL_OH_Supplier);
		}
	}
}
