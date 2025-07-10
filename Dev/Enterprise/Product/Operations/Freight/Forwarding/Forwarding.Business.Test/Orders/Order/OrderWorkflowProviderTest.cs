using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(Order))]
	sealed class OrderWorkflowProviderTest : WorkflowProviderTest<Order, OrderProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.OrderWorkflowDescriptorCode; }
		}

		#region GetTemplateFilterCriteria

		public void TestGetTemplateFilterCriteria_ForBuyer()
		{
			Order.JD_RL_NKGoodsAvailableAt = "MYPKG";
			Order.JD_RL_NKGoodsDeliveredTo = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Order.Factory.Save();
			AssertGetTemplateFilterCriteria(Order.BuyerPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForSupplier()
		{
			Order.JD_RL_NKGoodsAvailableAt = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Order.JD_RL_NKGoodsDeliveredTo = "MYPKG";
			Order.Factory.Save();
			AssertGetTemplateFilterCriteria(Order.SupplierPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForTransportMode()
		{
			Order.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Order.JD_TransportModeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, ZString.Empty);
		}

		#endregion

		public void TestTemplateSelectionForOrderCreatedFromShipment()
		{
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();

			var buyerTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			buyerTemplate.P0_ProcessType = ExpectedWorkflowType;
			buyerTemplate.P0_OH_Client = client2.PK;

			var buyerTemplateMilestone = buyerTemplate.WorkflowItems.Milestones.AddNew();
			buyerTemplateMilestone.TriggerConditions.TriggerEventCode = "BBB";

			Factory.Save();

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";
			shipment1.ConsigneePK = client1.PK;

			var order1 = shipment1.AttachedOrders.AddNew();

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = "SEA";
			shipment2.ConsigneePK = client2.PK;

			var order2 = shipment2.AttachedOrders.AddNew();

			AssertEquals("Template doesn't match client", 0, ((IWorkflowProvider)order1).WorkflowItems.Milestones.Count);

			AssertEquals("Template task added", 1, ((IWorkflowProvider)order2).WorkflowItems.Milestones.Count);
			AssertEquals("Tasks created from client template", "BBB", ((IWorkflowProvider)order2).WorkflowItems.Milestones[0].P9_SE_NKMilestoneEvent);
		}

		#region Implementation

		Order Order
		{
			get { return BusinessObject; }
		}

		#endregion
	}
}
