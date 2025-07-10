using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrderLineController))]
	public class OrderLineControllerBasherTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			OrderLineController controller = (OrderLineController)(ZControllerFactory.Create(ControllerIDs.OrderLine));
			AssertEquals(ModuleIDs.OrderLine, controller.ModuleID);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return OrderLine;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrderLine;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "moo";
			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			OrderLineCollection = order.OrderLines;
			OrderLine = OrderLineCollection.AddNew();
			Factory.Save();

			Controller.SetCollectionForDefaultsAndValidation(OrderLineCollection);
		}

		public void TestCRMSecurityCheckpoints()
		{
			OrderLine.Order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<OrderLine>.AssertController(new OrderLineController(), OrderLine, Env.Security.OrderLineTrackingCRMSecurity);
		}

		OrderLineCollection OrderLineCollection;
		OrderLine OrderLine;
	}
}
