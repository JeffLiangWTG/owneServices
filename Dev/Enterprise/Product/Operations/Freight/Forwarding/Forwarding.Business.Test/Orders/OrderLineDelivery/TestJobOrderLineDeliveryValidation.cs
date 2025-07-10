using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class TestJobOrderLineDeliveryValidation : BusinessObjectValidationTestCase
	{
		public void TestValidateJ4_Allocated()
		{
			BO.J4_Allocated = 3;
			OrderLineDeliverContainer container1 = BO.Containers.AddNew();
			OrderLineDeliverContainer container2 = BO.Containers.AddNew();
			container1.J5_QuantityInStore = 1;
			container2.J5_QuantityInStore = 2;

			AssertEquals("Total invoiced correct, should be no warning", false, BO.J4_AllocatedInfo.HasWarnings());
			container2.J5_QuantityInStore = 1;
			AssertEquals("Total invoiced wrong, should be a warning", true, BO.J4_AllocatedInfo.HasWarnings());

			BO.Containers.DeleteAll();
			AssertEquals("No containers exist any more, there should be no more warning", false, BO.J4_AllocatedInfo.HasWarnings());

			var container3 = BO.Containers.AddNew();
			AssertEquals("Containers now exist, there should be a warning", true, BO.J4_AllocatedInfo.HasWarnings());

			container3.J5_QuantityInStore = 3;
			AssertEquals("Total is now correct. There should be no more warning.", false, BO.J4_AllocatedInfo.HasWarnings());

			var container4 = BO.Containers.AddNew();
			container4.J5_QuantityInStore = 1;
			AssertEquals("Total is now wrong. There should be a warning", true, BO.J4_AllocatedInfo.HasWarnings());

			container4.Delete();
			AssertEquals("Container 4 deleted. Total is now correct.", false, BO.J4_AllocatedInfo.HasWarnings());
		}

		#region Implementation

		OrderLineDelivery BO;

		protected override void SetUp()
		{
			base.SetUp();
			BO = (OrderLineDelivery)GetNewBusinessObject("order");
		}

		BusinessObject GetNewBusinessObject(string orderNumber)
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order = Factory.New<Order>();

			order.JD_OrderNumber = orderNumber;
			order.BuyerPK = org.PK;
			order.SupplierPK = org.PK;
			Factory.Save();
			OrderLine line = Factory.New<OrderLine>();
			line.JO_JD = order.PK;
			line.JO_LineNo = 1000;

			return line.Deliveries.AddNew();
		}

		#endregion
	}
}
