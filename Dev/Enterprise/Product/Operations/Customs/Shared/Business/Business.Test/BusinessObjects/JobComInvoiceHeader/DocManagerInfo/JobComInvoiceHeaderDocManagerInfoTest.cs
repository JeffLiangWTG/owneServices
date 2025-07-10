using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderDocManagerInfo))]
	sealed class JobComInvoiceHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		Order order1;
		Order order2;

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<BaseJobComInvoiceHeader>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			order1 = invoice.AttachedOrders.AddNew();
			order2 = invoice.AttachedOrders.AddNew();

			return invoice;
		}

		#region TestRelatedObjectsRetrieved

		public void TestRelatedObjectsRetrieved()
		{
			var invoice = (BaseJobComInvoiceHeader)GetPopulatedParentBusinessObject();
			IList<BusinessObject> relativedObjects = invoice.DocManagerInfo.RelatedObjects;

			AssertEquals("Should have Order 1 in the related business objects", true, relativedObjects.Contains(order1));
			AssertEquals("Should have Order 2 in the related business objects", true, relativedObjects.Contains(order2));
		}

		#endregion
	}
}
