using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrdersOrgSupplierPartCollection))]
	sealed class OrdersOrgSupplierPartCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrdersOrgSupplierPartCollection(Factory);
		}

		public void TestFilterBusinessObjectDefaults()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				var buyer = Factory.NewWithValidTestData<OrgHeader>();

				var collection = new OrdersOrgSupplierPartCollection(Factory, supplier, buyer);

				AssertEquals("exact", collection.FilterBusinessObjectDefaults["Importer/Supplier:FilterCondition"].Value);
				AssertEquals(buyer.PK, collection.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
				AssertEquals(supplier.PK, collection.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);
			}
		}
	}
}
