using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(AgencyBookingFilterStrip))]
	internal class AgencyBookingFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestShipmentNumberFilterShouldFindBills()
		{
			Shipment1.JS_UniqueConsignRef = "V00000001";
			Shipment1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Shipment2.JS_UniqueConsignRef = "V00000002";
			Shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			Shipment3.JS_UniqueConsignRef = "V00000003";
			Shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_UniqueConsignRef);
			Asserter.AddFieldOfInterest(JobShipmentSchema.Constants.JS_ShipmentStatus);
			ModuleNumberFilter numFilter = (ModuleNumberFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.ShipmentNumber];
			numFilter.IsActive = true;
			numFilter.Property = "";
			Asserter.AssertMatches("empty", FilterStrip.Filter, Shipment1, Shipment2);
			numFilter.Property = "V00000001";
			Asserter.AssertMatches("V00000001", FilterStrip.Filter, Shipment1);
			numFilter.Property = "V00000002";
			Asserter.AssertMatches("V00000002", FilterStrip.Filter, Shipment2);
			numFilter.Property = "V00000003";
			Asserter.AssertMatches("V00000003", FilterStrip.Filter, Shipment3);
		}

		public void TestBranchRelatedPortsFilterVisibility()
		{
			Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
			ModuleGuidFilter branchRelatedPortsFilter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(FilterVisibility.AlwaysVisible, branchRelatedPortsFilter.Visibility);
			Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
			filterStrip = null;
			branchRelatedPortsFilter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(FilterVisibility.Visible, branchRelatedPortsFilter.Visibility);
		}

		public void TestBranchRelatedPortsDefaultValue()
		{
			Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(ZGuid.Empty, filter.DefaultProperty);
			filterStrip = null;
			Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
			filter = (ModuleGuidFilter)FilterStrip[AgencyShipmentFilterStrip.Descriptions.BranchRelatedPorts];
			AssertEquals(GlbBranch.CurrentBranch.PK, filter.DefaultProperty);
		}

		public void TestBranchFilterValidation()
		{
			string expectedError = "Your current security rights only allow you to view shipments relating to your current login branch.\r\n" + "If you think this is incorrect, please contact your system administrator.";
			ModuleGuidFilter filter;
			Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
			filter = (ModuleGuidFilter)FilterStrip[AgencyBookingFilterStrip.Descriptions.BranchRelatedPorts];
			filter.Property = ZGuid.Empty;
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, expectedError);
			filterStrip = null;
			Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
			filter = (ModuleGuidFilter)FilterStrip[AgencyBookingFilterStrip.Descriptions.BranchRelatedPorts];
			filter.Property = ZGuid.Empty;
			filter.Validation.ValidateProperty();
			AssertHasError(filter.PropertyInfo, expectedError);
		}

		public void TestShipmentStatusFilterDetails_ElectronicBookingAndShippingInstructions_true()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyBookingFilterStrip.Descriptions.ShipmentStatus];
				AssertEquals(FilterVisibility.AlwaysApplied, filter.Visibility);
				AssertEquals("UCF", filter.DefaultProperty);
				AssertEquals("UCF, EBK, EBC, BKD, BKX, BKJ, WEB, WTL", ((CodeDescriptionPairList)filter.List).CodesAsString);
			}
		}

		public void TestShipmentStatusFilterDetails_ElectronicBookingAndShippingInstructions_false()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[AgencyBookingFilterStrip.Descriptions.ShipmentStatus];
				AssertEquals(FilterVisibility.AlwaysApplied, filter.Visibility);
				AssertEquals("UCF", filter.DefaultProperty);
				AssertEquals("UCF, EBK, BKD, WEB, WTL", ((CodeDescriptionPairList)filter.List).CodesAsString);
			}
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

		public void TestProfitLossReasonFilterWithOperators()
		{
			var shipment1 = Factory.NewWithValidTestData<AgencyShipment>();
			var job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_ProfitLossReasonCode = "ND1";
			Asserter.AddToScope(shipment1);

			var shipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			var job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_ProfitLossReasonCode = "CD1";
			Asserter.AddToScope(shipment2);

			var shipment3 = Factory.NewWithValidTestData<AgencyShipment>();
			var job3 = new JobHeader.Loader(shipment3).TryCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;
			Asserter.AddToScope(shipment3);

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStrip["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for Shipment1.", profitLossReasonFilter, shipment1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for Shipment1.", profitLossReasonFilter, shipment1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for Shipment1 and Shipment2", profitLossReasonFilter, shipment1, shipment2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for Shipment2 and Shipment3", profitLossReasonFilter, shipment2, shipment3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for Shipment2 and Shipment3", profitLossReasonFilter, shipment2, shipment3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for Shipment2 and Shipment3", profitLossReasonFilter, shipment2, shipment3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for Shipment3", profitLossReasonFilter, shipment3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for Shipment1 and Shipment2", profitLossReasonFilter, shipment1, shipment2);
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<AgencyBooking>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.AgencyBookingCRMSecurity);
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new AgencyBookingFilterStrip().ModuleFilters;

			AssertNull(collection["Custom1 String"]);
			AssertNull(collection["Custom1 Integer"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["Custom2 Boolean"]);
			AssertNull(collection["Custom2 Datetime"]);
			AssertNull(collection["Unrelated Custom String"]);

			PrepareTemplatesWithCustomFields();

			collection = new AgencyBookingFilterStrip().ModuleFilters;

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
			template1.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;

			var template1Definition1 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition1.XC_Name = "Custom1 String";
			template1Definition1.XC_Type = AddOnColumnDataType.Codes.String;

			var template1Definition2 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition2.XC_Name = "Custom1 Integer";
			template1Definition2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;

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
		#region FilterStrip
		AgencyBookingFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new AgencyBookingFilterStrip();
				}

				return filterStrip;
			}
		}

		AgencyBookingFilterStrip filterStrip;
		#endregion
		FilterStripAsserter<AgencyShipment> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<AgencyShipment>(Factory, (s) => s.JS_HouseBill));
			}
		}

		FilterStripAsserter<AgencyShipment> asserter;
		AgencyShipment Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = Factory.New<AgencyShipment>();
					shipment1.JS_HouseBill = "Shipment1";
					Asserter.AddToScope(shipment1);
				}

				return shipment1;
			}
		}

		AgencyShipment shipment1;
		AgencyShipment Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = Factory.New<AgencyShipment>();
					shipment2.JS_HouseBill = "Shipment2";
					Asserter.AddToScope(shipment2);
				}

				return shipment2;
			}
		}

		AgencyShipment shipment2;
		AgencyShipment Shipment3
		{
			get
			{
				if (shipment3 == null)
				{
					shipment3 = Factory.New<AgencyShipment>();
					shipment3.JS_HouseBill = "Shipment3";
					Asserter.AddToScope(shipment3);
				}

				return shipment3;
			}
		}

		AgencyShipment shipment3;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AgencyBookingFilterStrip();
		}
		#endregion
	}
}
