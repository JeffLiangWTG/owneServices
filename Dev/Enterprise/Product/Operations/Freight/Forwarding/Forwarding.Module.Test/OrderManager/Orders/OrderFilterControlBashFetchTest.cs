using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class OrderFilterControlBashFetchTest : FilterControlBashFetchHintTest<Order>
	{
		#region BashFetchTest

		public void TestBashFetchForView_GoodsAvailableAtAddress_OrganisationNameOrPK()
		{
			BashFetchForView("GoodsAvailableAtAddress+OrganisationNameOrPK", 12);
		}

		public void TestBashFetchForView_GoodsAvailableAtAddress_E2_Address1()
		{
			BashFetchForView("GoodsAvailableAtAddress.E2_Address1", 12);
		}

		public void TestBashFetchForView_GoodsDeliveredToAddress_OrganisationNameOrPK()
		{
			BashFetchForView("GoodsDeliveredToAddress+OrganisationNameOrPK", 12);
		}

		public void TestBashFetchForView_GoodsDeliveredToAddress_E2_Address1()
		{
			BashFetchForView("GoodsDeliveredToAddress.E2_Address1", 12);
		}

		public void TestBashFetchForView_JD_SystemCreateUser()
		{
			BashFetchForView("JD_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JD_SystemCreateBranch()
		{
			BashFetchForView("JD_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JD_SystemCreateDepartment()
		{
			BashFetchForView("JD_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JD_SystemCreateTimeUtc()
		{
			BashFetchForView("JD_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JD_SystemLastEditUser()
		{
			BashFetchForView("JD_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JD_SystemLastEditTimeUtc()
		{
			BashFetchForView("JD_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_JD_OrderNumberAndSplit()
		{
			BashFetchForView("JD_OrderNumberAndSplit", 0);
		}

		public void TestBashFetchForView_JD_CalcShipmentBrokerageNumber()
		{
			BashFetchForView("JD_CalcShipmentBrokerageNumber", 0);
		}

		public void TestBashFetchForView_JD_OrderDate()
		{
			BashFetchForView("JD_OrderDate", 0);
		}

		public void TestBashFetchForView_JD_EF_ShipmentPrePlanning()
		{
			BashFetchForView("JD_EF_ShipmentPrePlanning", 0);
		}

		public void TestBashFetchForView_JD_Calc_BuyerCode()
		{
			// OrgHeader: 1
			// OrgAddress: 1
			BashFetchForView("JD_Calc_BuyerCode", 2);
		}

		public void TestBashFetchForView_JD_Calc_SupplierCode()
		{
			BashFetchForView("JD_Calc_SupplierCode", 0);
		}

		public void TestBashFetchForView_ControllingCustomerDocAddress_E2_CompanyName()
		{
			// JobDocAddress: 12

			BashFetchForView("ControllingCustomerDocAddress+E2_CompanyName", 12);
		}

		public void TestBashFetchForView_JD_TransportMode()
		{
			BashFetchForView("JD_TransportMode", 0);
		}

		public void TestBashFetchForView_JD_ContainerMode()
		{
			BashFetchForView("JD_ContainerMode", 0);
		}

		public void TestBashFetchForView_JD_Milestone_E_DEP()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_DEP", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_DEP()
		{
			// ProcessTasks: 12

			BashFetchForView("JD_Milestone_A_DEP", 12);
		}

		public void TestBashFetchForView_JD_E_ARV_1stIntermediate()
		{
			BashFetchForView("JD_E_ARV_1stIntermediate", 0);
		}

		public void TestBashFetchForView_JD_E_DEP_2()
		{
			BashFetchForView("JD_E_DEP_2", 0);
		}

		public void TestBashFetchForView_JD_E_ARV_2ndIntermediate()
		{
			BashFetchForView("JD_E_ARV_2ndIntermediate", 0);
		}

		public void TestBashFetchForView_JD_E_DEP_3()
		{
			BashFetchForView("JD_E_DEP_3", 0);
		}

		public void TestBashFetchForView_JD_Milestone_E_ARV()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_ARV", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_ARV()
		{
			// ProcessTasks: 12

			BashFetchForView("JD_Milestone_A_ARV", 12);
		}

		public void TestBashFetchForView_JD_RV_NKDepartureVessel()
		{
			BashFetchForView("JD_RV_NKDepartureVessel", 0);
		}

		public void TestBashFetchForView_JD_DepartureVoyage()
		{
			BashFetchForView("JD_DepartureVoyage", 0);
		}

		public void TestBashFetchForView_JD_RV_NKIntermediateVessel()
		{
			BashFetchForView("JD_RV_NKIntermediateVessel", 0);
		}

		public void TestBashFetchForView_JD_IntermediateVoyage()
		{
			BashFetchForView("JD_IntermediateVoyage", 0);
		}

		public void TestBashFetchForView_JD_RV_NKArrivalVessel()
		{
			BashFetchForView("JD_RV_NKArrivalVessel", 0);
		}

		public void TestBashFetchForView_JD_ArrivalVoyage()
		{
			BashFetchForView("JD_ArrivalVoyage", 0);
		}

		public void TestBashFetchForView_JD_DepartureVesselCutoffDate()
		{
			BashFetchForView("JD_DepartureVesselCutoffDate", 0);
		}

		public void TestBashFetchForView_JD_IncoTerm()
		{
			BashFetchForView("JD_IncoTerm", 0);
		}

		public void TestBashFetchForView_JD_AdditionalTerms()
		{
			BashFetchForView("JD_AdditionalTerms", 0);
		}

		public void TestBashFetchForView_JD_IsCancelled()
		{
			BashFetchForView("JD_IsCancelled", 0);
		}

		public void TestBashFetchForView_JD_BookingConfDate()
		{
			BashFetchForView("JD_BookingConfDate", 0);
		}

		public void TestBashFetchForView_JD_BookingConfRef()
		{
			BashFetchForView("JD_BookingConfRef", 0);
		}

		public void TestBashFetchForView_JD_EstimatedExchangeRate()
		{
			BashFetchForView("JD_EstimatedExchangeRate", 0);
		}

		public void TestBashFetchForView_JD_FollowUpDate()
		{
			BashFetchForView("JD_FollowUpDate", 0);
		}

		public void TestBashFetchForView_JD_InvoiceDate()
		{
			BashFetchForView("JD_InvoiceDate", 0);
		}

		public void TestBashFetchForView_JD_InvoiceNumber()
		{
			BashFetchForView("JD_InvoiceNumber", 0);
		}

		public void TestBashFetchForView_JD_OH_ReceivingAgent()
		{
			BashFetchForView("JD_OH_ReceivingAgent", 0);
		}

		public void TestBashFetchForView_JD_OH_SendingAgent()
		{
			BashFetchForView("JD_OH_SendingAgent", 0);
		}

		public void TestBashFetchForView_JD_OrderGoodsDescription()
		{
			BashFetchForView("JD_OrderGoodsDescription", 0);
		}

		public void TestBashFetchForView_JD_OrderStatus()
		{
			BashFetchForView("JD_OrderStatus", 0);
		}

		public void TestBashFetchForView_JD_RL_NKGoodsAvailableAt()
		{
			BashFetchForView("JD_RL_NKGoodsAvailableAt", 0);
		}

		public void TestBashFetchForView_JD_RL_NKGoodsDeliveredTo()
		{
			BashFetchForView("JD_RL_NKGoodsDeliveredTo", 0);
		}

		public void TestBashFetchForView_JD_RL_NKPortOfLoading()
		{
			BashFetchForView("JD_RL_NKPortOfLoading", 0);
		}

		public void TestBashFetchForView_JD_RL_NKPortOfDischarge()
		{
			BashFetchForView("JD_RL_NKPortOfDischarge", 0);
		}

		public void TestBashFetchForView_JD_RS_NKServiceLevel_NI()
		{
			BashFetchForView("JD_RS_NKServiceLevel_NI", 0);
		}

		public void TestBashFetchForView_JD_RX_NKOrderCurrency()
		{
			BashFetchForView("JD_RX_NKOrderCurrency", 0);
		}

		public void TestBashFetchForView_JD_Milestone_E_CCC()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_CCC", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_CCC()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_CCC", 1);
		}

		public void TestBashFetchForView_JD_Milestone_E_CLR()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_CLR", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_CLR()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_CLR", 1);
		}

		public void TestBashFetchForView_JD_Milestone_E_EXW()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_EXW", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_EXW()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_EXW", 1);
		}

		public void TestBashFetchForView_JD_Milestone_E_DCF()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_DCF", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_DCF()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_DCF", 1);
		}

		public void TestBashFetchForView_JD_Milestone_E_DCA()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_DCA", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_DCA()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_DCA", 1);
		}

		public void TestBashFetchForView_JD_Milestone_E_GIW()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_GIW", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_GIW()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_GIW", 1);
		}

		public void TestBashFetchForView_JD_Milestone_E_CAV()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_E_CAV", 1);
		}

		public void TestBashFetchForView_JD_Milestone_A_CAV()
		{
			// ProcessTasks: 1

			BashFetchForView("JD_Milestone_A_CAV", 1);
		}

		public void TestBashFetchForView_Packs()
		{
			BashFetchForView("Packs", 0);
		}

		public void TestBashFetchForView_JD_Calc_PackType()
		{
			BashFetchForView("JD_Calc_PackType", 0);
		}

		public void TestBashFetchForView_Volume()
		{
			BashFetchForView("Volume", 0);
		}

		public void TestBashFetchForView_VolumeUnit()
		{
			BashFetchForView("VolumeUnit", 0);
		}

		public void TestBashFetchForView_Weight()
		{
			BashFetchForView("Weight", 0);
		}

		public void TestBashFetchForView_WeightUnit()
		{
			BashFetchForView("WeightUnit", 0);
		}

		public void TestBashFetchForView_JD_DeliveryRequiredBy()
		{
			BashFetchForView("JD_DeliveryRequiredBy", 0);
		}

		public void TestBashFetchForView_JD_ExWorksRequiredBy()
		{
			BashFetchForView("JD_ExWorksRequiredBy", 0);
		}

		public void TestBashFetchForView_JD_Calc_HouseBill()
		{
			BashFetchForView("JD_Calc_HouseBill", 0);
		}

		public void TestBashFetchForView_JD_Calc_MasterBill()
		{
			BashFetchForView("JD_Calc_MasterBill", 0);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_ActualDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_ScheduledDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding", 12);
		}

		public void TestBashFetchForView_JD_Calc_LineCount()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_LineCount", 1);
		}

		public void TestBashFetchForView_JD_Calc_OuterPacks()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_OuterPacks", 1);
		}

		public void TestBashFetchForView_JD_Calc_InnerPacks()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_InnerPacks", 1);
		}

		public void TestBashFetchForView_JD_Calc_TotalQuantity()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_TotalQuantity", 1);
		}

		public void TestBashFetchForView_JD_Calc_TotalQuantityInvoiced()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_TotalQuantityInvoiced", 1);
		}

		public void TestBashFetchForView_JD_Calc_TotalQuantityReceived()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_TotalQuantityReceived", 1);
		}

		public void TestBashFetchForView_JD_Calc_TotalQuantityRemaining()
		{
			// JobOrderLine: 1

			BashFetchForView("JD_Calc_TotalQuantityRemaining", 1);
		}

		public void TestBashFetchForView_Carrier_OH_Code()
		{
			// OrgHeader: 12

			BashFetchForView("Carrier+OH_Code", 12);
		}

		public void TestBashFetchForView_JD_OrderNumberSplit()
		{
			BashFetchForView("JD_OrderNumberSplit", 0);
		}

		public void TestBashFetchForView_JD_ShipmentWindowStart()
		{
			BashFetchForView("JD_ShipmentWindowStart", 0);
		}

		public void TestBashFetchForView_JD_ShipmentWindowEnd()
		{
			BashFetchForView("JD_ShipmentWindowEnd", 0);
		}

		public void TestBashFetchForView_JD_IsReleased()
		{
			BashFetchForView("JD_IsReleased", 0);
		}

		#endregion

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			for (var i = 0; i < 12; i++)
			{
				var controllingAgent = factory.NewWithValidTestData<OrgHeader>();
				var carrier = factory.NewWithValidTestData<OrgHeader>();

				var order = factory.NewWithValidTestData<Order>();
				order.JD_OH_Carrier = carrier.PK;

				var docAddress = order.ControllingAgentDocAddress;
				docAddress.E2_OA_Address = controllingAgent.MainAddress.PK;

				var milestone = order.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = order.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = order.WorkflowItems.MilestonesIncludingRelated.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = order.WorkflowItems.MilestonesIncludingRelated.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				result.Add(order.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override SchemaPKColumn PkColumn => JobOrderHeaderSchema.PK;

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new OrderCollection(Factory);
			var filterBusinessObject = new OrdersFilterBusinessObject();
			return new OrdersFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new OrderCollection(Factory);
		}
	}
}
