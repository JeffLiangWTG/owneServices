using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLinkPartCollection))]
	sealed class OrgSupplierBuyerLinkPartCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierBuyerLinkPartCollection(Factory);
		}

		public void TestFilterBusinessObjectDefaults()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var supplierBuyerLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				var collection = new OrgSupplierBuyerLinkPartCollection(Factory, supplierBuyerLink);

				AssertEquals("exact", collection.FilterBusinessObjectDefaults["Importer/Supplier:FilterCondition"].Value);
				AssertEquals(supplierBuyerLink.Buyer.PK, collection.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
				AssertEquals(supplierBuyerLink.Supplier.PK, collection.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);
			}
		}
	}
}
