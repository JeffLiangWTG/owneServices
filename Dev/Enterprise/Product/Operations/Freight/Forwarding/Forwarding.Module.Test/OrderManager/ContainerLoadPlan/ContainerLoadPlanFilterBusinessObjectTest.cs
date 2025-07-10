using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ContainerLoadPlanFilterBusinessObject))]
	class ContainerLoadPlanFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ContainerLoadPlanFilterBusinessObject();
		}

		public void TestCLH_LoadListId()
		{
			setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load List #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CLH002";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
		}

		public void TestCLH_LoadStatus()
		{
			setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load List Status"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.ContainerLoadListHeaderStatus.Placed;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
		}

		public void TestCLH_PlannedTransportMode()
		{
			setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Planned Transport Mode"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.TransportModes.Air;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
		}

		public void TestCFS()
		{
			(var org1, _, _, _, _, _) = setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["CFS"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = org1.OH_FullName;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
		}

		public void TestCLH_OA_CFSAddres()
		{
			(var org1, _, _, _, _, _) = setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["CFS Address"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = org1.PK;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
		}

		public void TestPlannedDischargePort_PlannedLoadPort()
		{
			setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Planned Load / Planned Discharge Port"] as ModuleLocationFilter;
			filter.IsActive = true;
			filter.Property1 = "CNSZX";
			filter.Property2 = "AUSYD";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
		}

		public void TestControllingCustomer()
		{
			(var org1, _, _, _,_ ,_) = setUpTestData();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Controlling Customer"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = org1.PK;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
		}

		(OrgHeader org1, OrgHeader org2, OrgHeader org3, CommonContainerLoadList containerLoadPlan1, CommonContainerLoadList containerLoadPlan2, CommonContainerLoadList containerLoadPlan3) setUpTestData()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "org1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "org2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "org3";

			var controllingCustomerAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var controllingCustomerAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var controllingCustomerAddress3 = Factory.NewWithValidTestData<OrgAddress>();

			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress3 = Factory.NewWithValidTestData<JobDocAddress>();

			Factory.Save();

			docAddress1.E2_OA_Address = controllingCustomerAddress1.PK;
			docAddress2.E2_OA_Address = controllingCustomerAddress2.PK;
			docAddress3.E2_OA_Address = controllingCustomerAddress3.PK;

			controllingCustomerAddress1.OA_OH = org1.PK;
			controllingCustomerAddress1.OA_Code = org1.OH_Code;
			controllingCustomerAddress2.OA_OH = org2.PK;
			controllingCustomerAddress2.OA_Code = org2.OH_Code;
			controllingCustomerAddress3.OA_OH = org3.PK;
			controllingCustomerAddress3.OA_Code = org3.OH_Code;

			var containerLoadPlan1 = CreateContainerLoadPlan("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete);
			containerLoadPlan1.CLH_PlannedTransportMode = Constants.TransportModes.Sea;
			containerLoadPlan1.CLH_OA_CFSAddress = controllingCustomerAddress1.PK;
			containerLoadPlan1.CLH_RL_NKPlannedLoadPort = "THBKK";
			containerLoadPlan1.CLH_RL_NKPlannedDischargePort = "AUSYD";

			var containerLoadPlan2 = CreateContainerLoadPlan("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Placed);
			containerLoadPlan2.CLH_PlannedTransportMode = Constants.TransportModes.Air;
			containerLoadPlan2.CLH_OA_CFSAddress = controllingCustomerAddress2.PK;
			containerLoadPlan2.CLH_RL_NKPlannedLoadPort = "CNSZX";
			containerLoadPlan2.CLH_RL_NKPlannedDischargePort = "AUSYD";

			var containerLoadPlan3 = CreateContainerLoadPlan("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Converted);
			containerLoadPlan3.CLH_PlannedTransportMode = Constants.TransportModes.Rail;
			containerLoadPlan3.CLH_OA_CFSAddress = controllingCustomerAddress3.PK;
			containerLoadPlan3.CLH_RL_NKPlannedLoadPort = "CNSZX";
			containerLoadPlan3.CLH_RL_NKPlannedDischargePort = "SGSIN";

			Factory.Save();

			return (org1, org2, org3, containerLoadPlan1, containerLoadPlan2, containerLoadPlan3);
		}

		public CFSContainerLoadList CreateContainerLoadPlan(string orderNumber, string partNo, string bookingId, string loadListId, string containerLoadPlanStatus)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			order.JD_OrderNumber = orderNumber;
			orderLine.JO_Quantity = 20;
			orderLine.JO_Partno = partNo;
			orderLine.JO_JD = order.PK;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;
			booking.JSB_Status = Constants.SupplierBookingStatus.Planned;
			booking.JSB_BookingId = bookingId;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_Status = containerLoadPlanStatus;
			containerLoadPlan.CLH_LoadListId = loadListId;

			var containerLoadPlanLine = containerLoadPlan.LoadListLines.AddNew();
			containerLoadPlanLine.CLL_JSL_BookingLine = bookingLine.PK;

			return containerLoadPlan;
		}

		#region TestWorkflowFiltersAreNotAddedTwice

		public void TestWorkflowFiltersAreNotAddedTwice()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode;
			template.P0_IsActive = true;
			template.P0_Name = "WFTesting";

			var customColumn = Factory.New<GenCustomColumnDefinition>();
			customColumn.XC_ParentID = template.PK;
			customColumn.XC_ParentTableCode = "P0";
			customColumn.XC_Type = "INT";
			customColumn.XC_Name = "WFCustomField";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			filterBizo.LoadModuleFilters();

			AssertNull("We shouldn't be duplicating this custom field", filterBizo["WFCustomField (WF)"]);
		}

		#endregion
	}
}
