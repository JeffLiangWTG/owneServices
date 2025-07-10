using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageProcessTask))]
	class CartageProcessTaskTest : ProcessTaskTest
	{
		public void TestJobNumberFromCartage()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_ConsignmentID = "S1/E";
			var cartageProcessTask = ((IWorkflowProvider)cartage).WorkflowItems.AddNew();
			AssertEquals("S1/E", cartageProcessTask.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			return ((IWorkflowProvider)cartage).WorkflowItems.AddNew();
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
