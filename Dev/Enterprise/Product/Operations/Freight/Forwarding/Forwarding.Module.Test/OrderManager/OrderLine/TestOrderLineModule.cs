using System;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrderLineModule))]
	public class TestOrderLineModule : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (OrderLineModule module = new OrderLineModule())
			{
				AssertEquals(ModuleIDs.OrderLine, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrderLine;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var order = SetupOrder(factory);
			var orderLine = order.OrderLines.AddNew();
			return orderLine;
		}

		Order SetupOrder(BusinessObjectFactory factory)
		{
			var buyer = factory.New<OrgHeader>();
			buyer.OH_Code = "Buyer" + buyerFoutainNumber.ToString(CultureInfo.InvariantCulture);
			buyerFoutainNumber++;
			buyer.MainAddress.OA_Address1 = "BUYER ADDRESS 1";

			var order = factory.New<Order>();
			order.BuyerPK = buyer.PK;

			return order;
		}

		int buyerFoutainNumber;

		#endregion
	}
}
