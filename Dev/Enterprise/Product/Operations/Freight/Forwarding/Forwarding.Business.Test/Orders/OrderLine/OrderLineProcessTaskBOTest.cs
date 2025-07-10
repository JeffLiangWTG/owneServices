using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineProcessTask))]
	sealed class OrderLineProcessTaskBOTest : BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.MainAddress.OA_Address1 = "BuyerAddress";

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			return orderLine.WorkflowItems.AddNew();
		}
	}
}
