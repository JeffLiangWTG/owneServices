using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillOfLadingFilterStrip))]
	internal class BillOfLadingFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestBranchRelatedPortsFilterVisibility()
		{
			Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
			ModuleGuidFilter branchRelatedPortsFilter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(FilterVisibility.AlwaysVisible, branchRelatedPortsFilter.Visibility);
			Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
			filterStrip = null;
			branchRelatedPortsFilter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(FilterVisibility.Visible, branchRelatedPortsFilter.Visibility);
		}

		public void TestEstimatedTimeDeparture()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NLAMS");
			sailing1.Origin.JA_E_DEP = new ZDateTime(2010, 1, 1);
			var sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "NLAMS");
			sailing2.Origin.JA_E_DEP = new ZDateTime(2010, 2, 2);
			var sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUMEL", "NLAMS");
			sailing3.Origin.JA_A_DEP = ZDateTime.Empty;
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Factory.Save();
			// Estimated Time Departure
			var estimatedFilter = (ModuleDateFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.ETD];
			estimatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			estimatedFilter.Property1 = ZDateTime.Empty;
			estimatedFilter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty ETD Filter Matches All Shipments.", estimatedFilter, Shipment1, Shipment2, Shipment3);
			estimatedFilter.Property1 = sailing1.Origin.JA_E_DEP;
			estimatedFilter.Property2 = sailing1.Origin.JA_E_DEP.AddHours(1);
			Asserter.AssertMatches("Sailing1 ETD matches Shipment1.", estimatedFilter, Shipment1);
			AssertEquals(sailing1.Origin.JA_E_DEP, Shipment1.Sailing.JX_JA_E_DEP);
			estimatedFilter.Property1 = sailing2.Origin.JA_E_DEP;
			estimatedFilter.Property2 = sailing2.Origin.JA_E_DEP.AddHours(1);
			Asserter.AssertMatches("Sailing2 ETD matches Shipment2.", estimatedFilter, Shipment2);
			AssertEquals(sailing2.Origin.JA_E_DEP, Shipment2.Sailing.JX_JA_E_DEP);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has ETD Date Entered matches Shipment1 and Shipment2.", estimatedFilter, Shipment1, Shipment2);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No ETD Date Entered matches Shipment3.", estimatedFilter, Shipment3);
			AssertEquals(ZDateTime.Empty, Shipment3.Sailing.JX_JA_E_DEP);
		}

		public void TestEstimatedTimeArrival()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUSYD");
			sailing1.Destination.JB_E_ARV = new ZDateTime(2010, 1, 1);
			var sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE");
			sailing2.Destination.JB_E_ARV = new ZDateTime(2010, 2, 2);
			var sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUMEL");
			sailing3.Destination.JB_E_ARV = ZDateTime.Empty;
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Factory.Save();
			// Estimated Time of Arrival
			var estimatedFilter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EstimatedTimeArrival];
			estimatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			estimatedFilter.Property1 = ZDateTime.Empty;
			estimatedFilter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty ETA Filter Matches All Shipments.", estimatedFilter, Shipment1, Shipment2, Shipment3);
			estimatedFilter.Property1 = sailing1.Destination.JB_E_ARV;
			estimatedFilter.Property2 = sailing1.Destination.JB_E_ARV.AddHours(1);
			Asserter.AssertMatches("Sailing1 ETA matches Shipment1.", estimatedFilter, Shipment1);
			AssertEquals(sailing1.Destination.JB_E_ARV, Shipment1.Sailing.JX_JB_E_ARV);
			estimatedFilter.Property1 = sailing2.Destination.JB_E_ARV;
			estimatedFilter.Property2 = sailing2.Destination.JB_E_ARV.AddHours(1);
			Asserter.AssertMatches("Sailing2 ETA matches Shipment2.", estimatedFilter, Shipment2);
			AssertEquals(sailing2.Destination.JB_E_ARV, Shipment2.Sailing.JX_JB_E_ARV);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date ETA Entered matches Shipment1 and Shipment2.", estimatedFilter, Shipment1, Shipment2);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No ETA Date Enetered matches Shipment3.", estimatedFilter, Shipment3);
			AssertEquals(ZDateTime.Empty, Shipment3.Sailing.JX_JB_E_ARV);
		}

		public void TestBranchRelatedPortsDefaultValue()
		{
			Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(ZGuid.Empty, filter.DefaultProperty);
			filterStrip = null;
			Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
			filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(GlbBranch.CurrentBranch.PK, filter.DefaultProperty);
		}

		public void TestBranchFilterValidation()
		{
			string expectedError = "Your current security rights only allow you to view shipments relating to your current login branch.\r\n" + "If you think this is incorrect, please contact your system administrator.";
			ModuleGuidFilter filter;
			Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
			filter = (ModuleGuidFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.BranchRelatedPorts];
			filter.Property = ZGuid.Empty;
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, expectedError);
			filterStrip = null;
			Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
			filter = (ModuleGuidFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.BranchRelatedPorts];
			filter.Property = ZGuid.Empty;
			filter.Validation.ValidateProperty();
			AssertHasError(filter.PropertyInfo, expectedError);
		}

		[EIDOMessagingConfiguration(Enabled = false)]
		public void TestEIDOStatusFilterHiddenIfDisabled()
		{
			AssertNull("Should not find the E-IDO Status filter", FilterStrip[BillOfLadingFilterStrip.Descriptions.EIDOStatus]);
		}

		[EIDOMessagingConfiguration]
		public void TestEIDOStatusFilter()
		{
			ZDateTime now = ZDateTime.Now;
			BillOfLading noContainers = Factory.New<BillOfLading>();
			noContainers.JS_UniqueConsignRef = "No Containers";
			BillOfLadingContainer notSent = Factory.New<BillOfLading>().RealContainers.AddNew();
			notSent.Booking.JS_UniqueConsignRef = "Not Sent";
			BillOfLadingContainer originalSent = Factory.New<BillOfLading>().RealContainers.AddNew();
			originalSent.Booking.JS_UniqueConsignRef = "Original Sent";
			SendEIDOMessage(originalSent, EIDOMessageFunction.Original, now, 1);
			BillOfLadingContainer withdrawSent = Factory.New<BillOfLading>().RealContainers.AddNew();
			withdrawSent.Booking.JS_UniqueConsignRef = "Withdraw Sent";
			ResponseMessage(SendEIDOMessage(withdrawSent, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			SendEIDOMessage(withdrawSent, EIDOMessageFunction.Cancelation, now, 2);
			BillOfLadingContainer acknowledged = Factory.New<BillOfLading>().RealContainers.AddNew();
			acknowledged.Booking.JS_UniqueConsignRef = "Acknowledged";
			ResponseMessage(SendEIDOMessage(acknowledged, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Received);
			BillOfLadingContainer accepted = Factory.New<BillOfLading>().RealContainers.AddNew();
			accepted.Booking.JS_UniqueConsignRef = "Original Acc";
			ResponseMessage(SendEIDOMessage(accepted, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			BillOfLadingContainer withdrawn = Factory.New<BillOfLading>().RealContainers.AddNew();
			withdrawn.Booking.JS_UniqueConsignRef = "Withdraw Acc";
			ResponseMessage(SendEIDOMessage(withdrawSent, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(withdrawn, EIDOMessageFunction.Cancelation, now, 2), EIDOResponseType.Accepted);
			BillOfLadingContainer originalRejected = Factory.New<BillOfLading>().RealContainers.AddNew();
			originalRejected.Booking.JS_UniqueConsignRef = "Original Rej";
			ResponseMessage(SendEIDOMessage(originalRejected, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Rejected);
			BillOfLadingContainer withdrawRejected = Factory.New<BillOfLading>().RealContainers.AddNew();
			withdrawRejected.Booking.JS_UniqueConsignRef = "Withdraw Rej";
			ResponseMessage(SendEIDOMessage(withdrawSent, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(withdrawRejected, EIDOMessageFunction.Cancelation, now, 2), EIDOResponseType.Rejected);
			Asserter.AddToScope(noContainers);
			Asserter.AddToScope(notSent.Booking);
			Asserter.AddToScope(originalSent.Booking);
			Asserter.AddToScope(withdrawSent.Booking);
			Asserter.AddToScope(acknowledged.Booking);
			Asserter.AddToScope(accepted.Booking);
			Asserter.AddToScope(withdrawn.Booking);
			Asserter.AddToScope(originalRejected.Booking);
			Asserter.AddToScope(withdrawRejected.Booking);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.EIDOStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, noContainers, notSent.Booking, originalSent.Booking, withdrawSent.Booking, acknowledged.Booking, accepted.Booking, withdrawn.Booking, originalRejected.Booking, withdrawRejected.Booking);
			filter.Property = EIDOFilterList.Codes.Acknowledged;
			Asserter.AssertMatches("Acknowledged", filter, acknowledged.Booking);
			filter.Property = EIDOFilterList.Codes.Accepted;
			Asserter.AssertMatches("Accepted", filter, accepted.Booking);
			filter.Property = EIDOFilterList.Codes.NotSent;
			Asserter.AssertMatches("Not Sent", filter, notSent.Booking, withdrawn.Booking);
			filter.Property = EIDOFilterList.Codes.PendingResponse;
			Asserter.AssertMatches("Pending Response", filter, originalSent.Booking, withdrawSent.Booking);
			filter.Property = EIDOFilterList.Codes.Rejected;
			Asserter.AssertMatches("Rejected", filter, originalRejected.Booking, withdrawRejected.Booking);
		}

		public void TestShipmentStatusFilterDetails_ElectronicBookingAndShippingInstructions_true()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.ShipmentStatus];
				AssertEquals(FilterVisibility.AlwaysApplied, filter.Visibility);
				AssertEquals("BOL-All", filter.DefaultProperty);
				AssertEquals("BOL-All, ESI, CNF, SIJ, WFI", ((CodeDescriptionPairList)filter.List).CodesAsString);
			}
		}

		public void TestShipmentStatusFilterDetails_ElectronicBookingAndShippingInstructions_false()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.ShipmentStatus];
				AssertEquals(FilterVisibility.AlwaysApplied, filter.Visibility);
				AssertEquals("BOL-All", filter.DefaultProperty);
				AssertEquals("BOL-All, ESI, CNF, WFI", ((CodeDescriptionPairList)filter.List).CodesAsString);
			}
		}

		public void TestShipmentNumberFilterShouldNotFindBookings()
		{
			var shipment1 = Factory.NewWithValidTestData<BillOfLading>();
			shipment1.JS_UniqueConsignRef = "V00000001";
			shipment1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			var shipment2 = Factory.NewWithValidTestData<BillOfLading>();
			shipment2.JS_UniqueConsignRef = "V00000002";
			shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;
			var shipment3 = Factory.NewWithValidTestData<BillOfLading>();
			shipment3.JS_UniqueConsignRef = "V00000003";
			shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Asserter.AddToScope(shipment1);
			Asserter.AddToScope(shipment2);
			Asserter.AddToScope(shipment3);
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_UniqueConsignRef);
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_ShipmentStatus);
			var filter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ShipmentNumber];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("empty", FilterStrip.Filter, shipment1, shipment2);
			filter.Property = "V00000001";
			Asserter.AssertMatches("V00000001", FilterStrip.Filter, shipment1);
			filter.Property = "V00000002";
			Asserter.AssertMatches("V00000002", FilterStrip.Filter, shipment2);
			filter.Property = "V00000003";
			Asserter.AssertMatches("V00000003", FilterStrip.Filter);
		}

		public void TestContainerDockReceiptNumberFilter()
		{
			var shipment1 = Factory.New<BillOfLading>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			Asserter.AddToScope(shipment1);
			var container1a = shipment1.RealContainers.AddNew();
			container1a.JC_DepartureDockReceipt = "DR1";
			AgencyShipmentContainer container1b = shipment1.RealContainers.AddNew();
			container1b.JC_DepartureDockReceipt = "DR2";
			var shipment2 = Factory.New<BillOfLading>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			Asserter.AddToScope(shipment2);
			AgencyShipmentContainer container2a = shipment2.RealContainers.AddNew();
			container2a.JC_DepartureDockReceipt = "DR1";
			var shipment3 = Factory.New<BillOfLading>();
			shipment3.JS_UniqueConsignRef = "Shipment3";
			Asserter.AddToScope(shipment3);
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.DockReceiptNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, shipment1, shipment2, shipment3);
			filter.Property = "DR1";
			Asserter.AssertMatches("DR1", filter, shipment1, shipment2);
			filter.Property = "DR2";
			Asserter.AssertMatches("DR2", filter, shipment1);
		}

		public void TestWorkflowMilestonesFilters()
		{
			var milestone1 = Shipment1.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Some test description";
			milestone1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			var milestone3 = Shipment3.WorkflowItems.Milestones.AddNew();
			milestone3.P9_Description = "Some test description";
			milestone3.TriggerConditions.TriggerEventCode = Events.ArrivalDetailsChanged.Code;
			Factory.Save();
			var filter = (WorkflowModuleFilter)FilterStrip["Milestone Date"];
			Asserter.AssertMatches("", filter, Shipment1, Shipment2, Shipment3);
			filter.MilestoneEvent = Events.Arrival.Code;
			Asserter.AssertMatches("", filter, Shipment1);
			filter.MilestoneEvent = Events.ArrivalDetailsChanged.Code;
			Asserter.AssertMatches("", filter, Shipment3);
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<BillOfLading>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.AgencyBillOfLadingCRMSecurity);
		}

		public void TestPackLineRefNumber()
		{
			AssertPackLineRefNumber(JobPackLinesSchema.JL_ImportRefNumber, BillOfLadingFilterStrip.Descriptions.PackLineImportReference);
			AssertPackLineRefNumber(JobPackLinesSchema.JL_ExportRefNumber, BillOfLadingFilterStrip.Descriptions.PackLineExportReference);
		}

		void AssertPackLineRefNumber(SchemaStringColumn refNumColumn, string filterName)
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			Asserter.AddToScope(billOfLading);
			var packline1 = billOfLading.OuterPackLines.AddNew();
			packline1[refNumColumn] = "REF-00001";
			var packline2 = billOfLading.OuterPackLines.AddNew();
			packline2[refNumColumn] = "REF-00002";
			var packline3 = billOfLading.OuterPackLines.AddNew();
			packline3[refNumColumn] = "REF-00002";
			var packline4 = billOfLading.OuterPackLines.AddNew();
			packline4[refNumColumn] = "REF-00003";
			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			Asserter.AddToScope(billOfLading2);
			var packline5 = billOfLading2.OuterPackLines.AddNew();
			packline5[refNumColumn] = "REF-00002";
			var billOfLading3 = Factory.NewWithValidTestData<BillOfLading>();
			Asserter.AddToScope(billOfLading3);
			var packline6 = billOfLading3.OuterPackLines.AddNew();
			packline6[refNumColumn] = "REF-00003";
			Factory.Save();
			var filter = (ModuleTextFilter)FilterStrip[filterName];
			filter.Property = "REF-00001";
			Asserter.AssertMatches(filterName, filter, new[] { billOfLading });
			filter.Property = "REF-00002";
			Asserter.AssertMatches(filterName, filter, new[] { billOfLading, billOfLading2 });
			filter.Property = "REF-00003";
			Asserter.AssertMatches(filterName, filter, new[] { billOfLading, billOfLading3 });
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
			var job1 = new JobHeader.Loader(billOfLading1).TryCreate();
			job1.JH_ProfitLossReasonCode = "ND1";
			Asserter.AddToScope(billOfLading1);

			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			var job2 = new JobHeader.Loader(billOfLading2).TryCreate();
			job2.JH_ProfitLossReasonCode = "CD1";
			Asserter.AddToScope(billOfLading2);

			var billOfLading3 = Factory.NewWithValidTestData<BillOfLading>();
			var job3 = new JobHeader.Loader(billOfLading3).TryCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;
			Asserter.AddToScope(billOfLading3);

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStrip["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for billOfLading1.", profitLossReasonFilter, billOfLading1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for billOfLading1.", profitLossReasonFilter, billOfLading1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for billOfLading1 and billOfLading2", profitLossReasonFilter, billOfLading1, billOfLading2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for billOfLading2 and billOfLading3", profitLossReasonFilter, billOfLading2, billOfLading3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for billOfLading2 and billOfLading3", profitLossReasonFilter, billOfLading2, billOfLading3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for billOfLading2 and billOfLading3", profitLossReasonFilter, billOfLading2, billOfLading3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for billOfLading3", profitLossReasonFilter, billOfLading3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for billOfLading1 and billOfLading2", profitLossReasonFilter, billOfLading1, billOfLading2);
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new BillOfLadingFilterStrip().ModuleFilters;

			AssertNull(collection["Custom1 String"]);
			AssertNull(collection["Custom1 Integer"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["Custom2 Boolean"]);
			AssertNull(collection["Custom2 Datetime"]);
			AssertNull(collection["Unrelated Custom String"]);

			PrepareTemplatesWithCustomFields();

			collection = new BillOfLadingFilterStrip().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), collection["Custom1 String"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), collection["Custom1 Integer"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), collection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), collection["Custom2 Boolean"].GetType());
			AssertEquals(typeof(ModuleDateFilter), collection["Custom2 Datetime"].GetType());
			AssertNull(collection["Unrelated Custom String"]);
		}

		void PrepareTemplatesWithCustomFields()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;

			var template1Definition1 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition1.XC_Name = "Custom1 String";
			template1Definition1.XC_Type = AddOnColumnDataType.Codes.String;

			var template1Definition2 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition2.XC_Name = "Custom1 Integer";
			template1Definition2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;

			var template2Definition1 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition1.XC_Name = "Custom2 Boolean";
			template2Definition1.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var template2Definition2 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition2.XC_Name = "Custom2 Datetime";
			template2Definition2.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var unrelatedTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			unrelatedTemplate.P0_ProcessType = "ZZZ";

			var unrelatedTemplateDefinition = unrelatedTemplate.GenCustomColumnDefinitions.AddNew();
			unrelatedTemplateDefinition.XC_Name = "Unrelated Custom String";
			unrelatedTemplateDefinition.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#region Implementation
		EIDOMessage SendEIDOMessage(AgencyShipmentContainer container, EIDOMessageFunction function, ZDateTime now, int offset)
		{
			EIDOMessage message = EIDOMessage.New(container, function, EDIMessage.MessageNumberPlaceHolder);
			message.EM_SystemCreateTimeUtc = now.AddMinutes(offset);
			return message;
		}

		void ResponseMessage(EIDOMessage sentMessage, EIDOResponseType responseType)
		{
			EIDOMessage message = Factory.New<EIDOMessage>();
			message.EM_ApplicationCode = EIDOMessage.ApplicationCodes.EIDO;
			message.EM_ReceiveTransmit = EIDOMessage.Direction.Receive;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_LinkTable = sentMessage.EM_LinkTable;
			message.EM_LinkUniqueID = sentMessage.EM_LinkUniqueID;
			message.EM_Status = EIDOMessage.Status.Recognised;
			switch (responseType)
			{
				case EIDOResponseType.Accepted:
					sentMessage.EM_Status = EIDOMessage.Status.Received;
					break;
				case EIDOResponseType.Received:
					sentMessage.EM_Status = EIDOMessage.Status.Acknowledged;
					break;
				case EIDOResponseType.Rejected:
					sentMessage.EM_Status = EIDOMessage.Status.Rejected;
					break;
			}
		}

		FilterStripAsserter<BillOfLading> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<BillOfLading>(Factory, (s) => s.JS_UniqueConsignRef));
			}
		}

		FilterStripAsserter<BillOfLading> asserter;
		BillOfLadingFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new BillOfLadingFilterStrip();
				}

				return filterStrip;
			}
		}

		BillOfLadingFilterStrip filterStrip;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BillOfLadingFilterStrip();
		}

		BillOfLading Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = Factory.New<BillOfLading>();
					shipment1.JS_HouseBill = "Shipment1";
					Asserter.AddToScope(shipment1);
				}

				return shipment1;
			}
		}

		BillOfLading shipment1;
		BillOfLading Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = Factory.New<BillOfLading>();
					shipment2.JS_HouseBill = "Shipment2";
					Asserter.AddToScope(shipment2);
				}

				return shipment2;
			}
		}

		BillOfLading shipment2;
		BillOfLading Shipment3
		{
			get
			{
				if (shipment3 == null)
				{
					shipment3 = Factory.New<BillOfLading>();
					shipment3.JS_HouseBill = "Shipment3";
					Asserter.AddToScope(shipment3);
				}

				return shipment3;
			}
		}

		BillOfLading shipment3;
		#endregion
	}
}
