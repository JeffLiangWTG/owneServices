using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineProcessTask))]
	sealed class OrderLineProcessTaskEnterpirseBOTest : EnterpriseBusinessObjectTestCase
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
