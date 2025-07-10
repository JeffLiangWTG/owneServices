using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	#region Class: ReceiveFilterBusinessObjectTest

	[TestedType(typeof(ReceiveFilterBusinessObject))]
	public class ReceiveFilterBusinessObjectTest : ExtendedDocketFilterBusinessObjectTest<ReceiveFilterBusinessObject, WhsReceive>
	{
		#region TestDocketStatuses

		public void TestDocketStatuses()
		{
			AssertEquals(6, DocketFilter.DocketStatuses.IndexOfCode("UNF"));
			AssertEquals(-1, DocketFilter.DocketStatuses.IndexOfCode("NEW"));
			AssertEquals(-1, DocketFilter.DocketStatuses.IndexOfCode("PIC"));
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			var etaFilter = Array.Find(DocketFilter.ModuleFilters.ToSortedArrayWithIsExclusiveLast(),
				filter => filter.Code == "ETA" && filter.Category.Description == "Dates" && filter.FilterColumn == WhsDocketSchema.WD_ETA);
			AssertNotNull("ETA Filter Exists", etaFilter);
		}

		#endregion

		#region ExternalReferenceFieldName

		protected override ZString ExternalReferenceFieldName => "Receive Reference";

		#endregion

		#region TestWorkflowFiltersPresent

		public void TestWorkflowFiltersPresent()
		{
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", (WorkflowModuleFilter)DocketFilter["Milestone Date"]);
		}

		#endregion

		#region TestServiceTypeDateBooked_WithNoFields

		public void TestServiceTypeDateBooked_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All Receives should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive1, receive2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithDateField

		public void TestServiceTypeDateBooked_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Receives with matching date booked should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithServiceTypeField

		public void TestServiceTypeDateBooked_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Receives with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Receives with matching service type and date booked should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithNoFields

		public void TestServiceTypeDateCompleted_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All Receives should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive1, receive2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithDateField

		public void TestServiceTypeDateCompleted_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Receives with matching date completed should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithServiceTypeField

		public void TestServiceTypeDateCompleted_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Receives with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Receives with matching service type and date completed should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(receive1, receive2) => new[] { receive2 });
		}

		#endregion

		#region TestFilterBySupplierCompanyName

		public void TestFilterBySupplierCompanyName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive1");
			receive1.SupplierDocAddress.OrganisationPK = data.Org1.PK;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive2");
			receive2.SupplierDocAddress.E2_AddressOverride = true;
			receive2.SupplierDocAddress.E2_CompanyName = "CargoWise 123";

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).Property = "CargoWise";
			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(2, receives.Length);
			AssertCollectionContains(receive1, receives);
			AssertCollectionContains(receive2, receives);

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(2, receives.Length);
			AssertCollectionContains(receive1, receives);
			AssertCollectionContains(receive2, receives);

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(1, receives.Length);
			AssertCollectionContains(receive1, receives);

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(0, receives.Length);

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(0, receives.Length);

			((ModuleTextFilter)FilterStripBizO["Supplier Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(1, receives.Length);
			AssertCollectionContains(receive2, receives);
		}

		#endregion

		#region TestHoldPalletIDPutaway

		public void TestHoldPalletIDPutawayFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Transport";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive1");
			receive1.WD_HoldPalletIDPutaway = true;
			
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive2");
			receive2.WD_HoldPalletIDPutaway = false;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive3");
			Factory.Save();

			var receiveFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)receiveFilter[ReceiveFilterBusinessObject.Schema.IsHeldForPutaway];
			filter.IsActive = true;
			Asserter.AddToScope(receive1, receive2, receive3);

			filter.Property = ReceiveFilterBusinessObject.Schema.IsHeldForPutawayTypeCodes.All;
			Asserter.AssertMatches("Should have 3 receives", filter, receive1, receive2, receive3);

			filter.Property = ReceiveFilterBusinessObject.Schema.IsHeldForPutawayTypeCodes.HeldForPutaway;
			Asserter.AssertMatches("Should have 1 receive", filter, receive1);

			filter.Property = ReceiveFilterBusinessObject.Schema.IsHeldForPutawayTypeCodes.NotHeldForPutaway;
			Asserter.AssertMatches("Should have 2 receives", filter, receive2, receive3);
		}

		#endregion

		#region TestFilterByTransportCompanyName

		public void TestFilterByTransportCompanyName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Transport";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive1");
			receive1.TransportCoPK = data.Org1.PK;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive2");
			receive2.TransportCoDocAddress.E2_AddressOverride = true;
			receive2.TransportCoDocAddress.E2_CompanyName = "CargoWise Transport 123";

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).Property = "CargoWise Transport";
			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(2, receives.Length);
			AssertCollectionContains(receive1, receives);
			AssertCollectionContains(receive2, receives);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(2, receives.Length);
			AssertCollectionContains(receive1, receives);
			AssertCollectionContains(receive2, receives);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(1, receives.Length);
			AssertCollectionContains(receive1, receives);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(0, receives.Length);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(0, receives.Length);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(1, receives.Length);
			AssertCollectionContains(receive2, receives);
		}

		#endregion

		#region TestFilterTransportCo

		protected override WhsReceive GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, docketReference, data.Part1, 10m);
			if (transportCo != null)
			{
				receive.TransportCoPK = transportCo.PK;
			}

			return receive;
		}

		#endregion

		#region WorkflowDescriptorCode

		protected override string WorkflowDescriptorCode
		{
			get { return WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode; }
		}

		#endregion

		#region TestFilterNotInvoiced

		protected override bool IsNotInvoicedFilterUsed
		{
			get { return true; }
		}

		protected override AccChargeCode GetJobSpecificChargeCode()
		{
			return Helper.CreateChargeCode("WREC", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
		}

		protected override WhsReceive CreateFinalisedDocket(TestDataSimpleEnvironment data, ZString reference, ZDateTime finalisedDate)
		{
			return Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, reference, finalisedDate.ToOffset(), data.Part1, 10m, data.Whs1.DefaultLocation, "");
		}

		#endregion

		#region TestFilterProductCount

		public void TestFilterProductCount_FilterExists()
		{
			AssertNotNull("Filter Product Count exists.", (ModuleNumberRangeFilter)DocketFilter["Product Count"]);
		}

		public void TestFilterProductCount_EqualsCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m, locA1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, locA1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 5m, locA2);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 1;

			var result1 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned", 1, result1.Length);
			AssertCollectionNotContains(receive1, result1);
			AssertCollectionContains(receive2, result1);

			filter.Property1 = 2;
			var result2 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned", 1, result2.Length);
			AssertCollectionContains(receive1, result2);
			AssertCollectionNotContains(receive2, result2);
		}

		public void TestFilterProductCount_LessThanCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m, locA1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 6m, locA1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, locA2);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 1;

			var result1 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned", 1, result1.Length);
			AssertCollectionNotContains(receive1, result1);
			AssertCollectionContains(receive2, result1);

			filter.Property2 = 2;
			var result2 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned", 2, result2.Length);
			AssertCollectionContains(receive1, result2);
			AssertCollectionContains(receive2, result2);
		}

		public void TestFilterProductCount_GreaterThanCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m, locA1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 6m, locA1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, locA2);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = 1;

			var result1 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned", 2, result1.Length);
			AssertCollectionContains(receive1, result1);
			AssertCollectionContains(receive2, result1);

			filter.Property1 = 2;
			var result2 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned", 1, result2.Length);
			AssertCollectionContains(receive1, result2);
			AssertCollectionNotContains(receive2, result2);
		}

		public void TestFilterProductCount_BetweenCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m, locA1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 6m, locA1);
			Helper.CreateWhsReceiveLine(receive1, part3, 3m, locA1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, locA2);
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 9m, locA2);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveLine(receive3, part3, 15m, data.Whs1.FindLocation("A-3"));
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 0;
			filter.Property2 = 1;

			var result1 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned in result1", 1, result1.Length);
			AssertCollectionNotContains(receive1, result1);
			AssertCollectionNotContains(receive2, result1);
			AssertCollectionContains(receive3, result1);

			filter.Property1 = 2;
			filter.Property2 = 3;
			var result2 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned in result2", 2, result2.Length);
			AssertCollectionContains(receive1, result2);
			AssertCollectionContains(receive2, result2);
			AssertCollectionNotContains(receive3, result2);

			filter.Property1 = 3;
			filter.Property2 = 3;
			var result3 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Correct number of receives returned in result3", 1, result3.Length);
			AssertCollectionContains(receive1, result3);
			AssertCollectionNotContains(receive2, result3);
			AssertCollectionNotContains(receive3, result3);
		}

		#endregion

		#region IsServiceLevelUsed

		protected override bool IsServiceLevelUsed
		{
			get { return true; }
		}

		#endregion

		#region TestPalletIDFilter

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units, ZString palletID)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine((WhsReceive)docket, part, units, null, palletID);
			Factory.Save();

			return inventory.InDocketLine;
		}

		#endregion

		#region TestTotalUnitsMismatchedFilter

		#region TestTotalUnitsMismatchedFilterType

		public void TestTotalUnitsMismatchedFilterType()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Total Units Mismatch"];
			AssertEquals(FilterCategories.Other, filter.Category);
		}

		#endregion

		#region TestTotalUnitsMismatchedFilter_FindMismatchedReceives

		public void TestTotalUnitsMismatchedFilter_FindMismatchedReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			// mismatched
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 20m, data.Whs1.DefaultLocation);
			receive1.WD_TotalUnits = 20m;

			// not mismatched
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var line21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation);
			var line22 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, data.Whs1.DefaultLocation);
			receive2.WD_TotalUnits = 30m;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Total Units Mismatch"];

			filter.Property0 = ZBool.True;
			var result1 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals(1, result1.Length);
			AssertCollectionContains(receive1, result1);
			AssertCollectionNotContains(receive2, result1);

			filter.Property0 = ZBool.False;
			var result2 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals(1, result2.Length);
			AssertCollectionNotContains(receive1, result2);
			AssertCollectionContains(receive2, result2);
		}

		#endregion

		#endregion

		#region TestActualVsExpectedMismatchedFilter

		#region TestActualVsExpectedMismatchedFilterType

		public void TestActualVsExpectedMismatchedFilterType()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["ASN Actual vs. Expected Mismatch"];
			AssertEquals(FilterCategories.Other, filter.Category);
		}

		#endregion

		#region TestActualVsExpectedMismatchedFilter_FindMismatchedReceives

		public void TestActualVsExpectedMismatchedFilter_FindMismatchedReceives()
		{
			var client = Helper.CreateClient("client");
			var whs = Helper.CreateWarehouse("whs");
			Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);
			var product = Helper.CreateProduct("product", client);

			// mismatched
			var receive1 = Helper.CreateWhsReceiveWithInventory(client, whs, "receive1", product, 5m, finalise: false);
			receive1.Lines[0].WE_TransactionQuantity = 10m;

			// not mismatched
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs, "receive2", product, 10m, finalise: false);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var emptyFilter = new ZQuery();
			var result1 = Factory.Load<WhsReceive>(emptyFilter);
			AssertEquals("Precondition: Receives should be not be filtered.", 2, result1.Length);
			AssertContainsExactElementsInAnyOrder("Precondition: Receives should be not be filtered.", new[] { receive1, receive2 }, result1);

			var filter = (ModuleFlagsFilter)filterBizO["ASN Actual vs. Expected Mismatch"];
			filter.Property0 = ZBool.True;
			var result2 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Receives should be filtered by mismatching received units and expected units.", 1, result2.Length);
			AssertCollectionContains("Receives should be filtered by mismatching received units and expected units.", receive1, result2);
			AssertCollectionNotContains("Receives should be filtered by mismatching received units and expected units.", receive2, result2);

			filter.Property0 = ZBool.False;
			var result3 = Factory.Load<WhsReceive>(filter.Query);
			AssertEquals("Receives should be filtered by matching received units and expected units.", 1, result3.Length);
			AssertCollectionNotContains("Receives should be filtered by matching received units and expected units.", receive1, result3);
			AssertCollectionContains("Receives should be filtered by matching received units and expected units.", receive2, result3);
		}

		#endregion

		#endregion

		#region TestTotalPalletsMismatchedFilter

		#region TestTotalPalletsMismatchedFilterType

		public void TestTotalPalletsMismatchedFilterType()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Total Pallets Mismatch"];
			AssertEquals(FilterCategories.Other, filter.Category);
		}

		#endregion

		#region TestTotalPalletsMismatchedFilter_FindMismatchedReceives

		public void TestTotalPalletsMismatchedFilter_FindMismatchedReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			// mismatched
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_TotalPallets = 1;
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");

			// not mismatched
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_TotalPallets = 2;
			var line21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var line22 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			receive3.WD_TotalPallets = 1;
			var line31 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var line32 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 20m, data.Whs1.DefaultLocation);

			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			receive4.WD_TotalPallets = 0;
			var line41 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, data.Whs1.DefaultLocation);
			var line42 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 20m, data.Whs1.DefaultLocation);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Total Pallets Mismatch"];

			Asserter.AddToScope(receive1, receive2, receive3, receive4);

			filter.Property0 = ZBool.True;
			Asserter.AssertMatches("Should have 1 mismatch receive", filter, receive1);

			filter.Property0 = ZBool.False;
			Asserter.AssertMatches("Should have 3 not mismatch receives", filter, receive2, receive3, receive4);
		}

		#endregion

		#endregion

		#region TestUnloadCompletedTimeFilter

		[TestDate(2025, 05, 19, 10, 0, 0)]
		public void TestUnloadCompletedTimeFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_UnloadCompletedTime = ZDateTimeOffset.Now;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_UnloadCompletedTime = ZDateTimeOffset.Now.AddDays(-5);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			receive3.WD_UnloadCompletedTime = ZDateTimeOffset.Now.AddDays(-12);

			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			receive4.WD_UnloadCompletedTime = ZDateTimeOffset.Now.AddMonths(-2);

			var receive5 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R5");
			receive5.WD_UnloadCompletedTime = ZDateTimeOffset.Now.AddMonths(-8);

			var receive6 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R6");
			receive6.WD_UnloadCompletedTime = ZDateTimeOffset.Now.AddYears(-2);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateTimeOffsetFilter)filterBizO["Unload Completed Time"];

			Asserter.AddToScope(receive1, receive2, receive3, receive4, receive5, receive6);

			filter.PropertySearch = "Today";
			Asserter.AssertMatches("Should have 1 mismatch receive", filter, receive1);

			filter.PropertySearch = "Last 7 Days";
			Asserter.AssertMatches("Should have 2 mismatch receive", filter, receive1, receive2);

			filter.PropertySearch = "Last Month";
			Asserter.AssertMatches("Should have 3 mismatch receive", filter, receive1, receive2, receive3);

			filter.PropertySearch = "Last 6 Mths.";
			Asserter.AssertMatches("Should have 4 mismatch receive", filter, receive1, receive2, receive3, receive4);

			filter.PropertySearch = "Last 12 Mths.";
			Asserter.AssertMatches("Should have 5 mismatch receive", filter, receive1, receive2, receive3, receive4, receive5);

			filter.PropertySearch = "In the Past";
			Asserter.AssertMatches("Should have 6 mismatch receive", filter, receive1, receive2, receive3, receive4, receive5, receive6);
		}

		#endregion

		#region TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var bookingDate = "Booking Date";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = bookingDate;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Datetime;

			Factory.Save();

			var collection = new ReceiveFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[bookingDate]);
			AssertNotNull(collection[bookingDate + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		#endregion

		#region Test Filter MaxLength

		public override void TestFilterMaxLength()
		{
			base.TestFilterMaxLength();

			var supplierCompanyNameLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			AssertEquals("MaxLength of Supplier Company Name should be set correctly.", supplierCompanyNameLength, FilterStripBizO["Supplier Company Name"].MaxLength);
		}

		#endregion

		#region TestFilterDocketTaskPlanningStatus

		protected override bool SupportsDocketPlanningStatus => true;

		#endregion

		#region TestFilterTransportCo

		protected override bool SupportsTransportCoFilters => true;

		#endregion

		#region TestAddReceiveCategoryFilter

		public void TestAddReceiveCategoryFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Transport";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive1");
			receive1.WD_ReceiveCategory = "CA1";

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive2");
			receive2.TransportCoDocAddress.E2_AddressOverride = true;
			receive2.TransportCoDocAddress.E2_CompanyName = "CargoWise Transport 123";
			receive2.WD_ReceiveCategory = "CA2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["ReceiveCategories"];
			filter.Property = "CA1";
			filter.IsActive = true;
			AssertEquals("ReceiveCategory filter category should be StatusAndFlags.", filter.Category, FilterCategories.StatusAndFlags);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			var receives = Factory.Load<WhsReceive>(FilterStripBizO.Filter);
			AssertEquals(1, receives.Length);
			AssertCollectionContains(receive1, receives);
			AssertCollectionNotContains(receive2, receives);
		}

		#endregion

		#region TestFilterIsCreatedFromPickOnSalesOrder

		public void TestFilterIsCreatedFromPickOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 100m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 50m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertNotNull(createdReceive);

			var filter = (ModuleFlagsFilter)FilterStripBizO["Is Created From Pick On Sales Order"];

			AssertEquals("Is Created From Pick On Sales Order", filter.Description);
			AssertEquals("Is Created From Pick On Sales Order", filter.MultilingualDescription);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertEquals(FilterVisibility.AlwaysApplied, filter.Visibility);
			AssertEquals(ZBool.False, filter.Property0);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query.AddToFilter(FilterStripBizO.Filter);
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals(1, receives.Length);
			AssertCollectionContains(receive1, receives);
			AssertCollectionNotContains(createdReceive, receives);

			filter.Property0 = ZBool.True;
			FilterStripBizO.ModuleFilters.InvalidateCachedQuery();
			var query2 = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query2.AddToFilter(FilterStripBizO.Filter);
			receives = Factory.Load<WhsReceive>(query2);
			AssertEquals(1, receives.Length);
			AssertCollectionNotContains(receive1, receives);
			AssertCollectionContains(createdReceive, receives);
		}

		#endregion

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<WhsReceive>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.WhsReceiveCRMSecurity);
		}

		#region Implementation

		void TestServiceTypeFilters(string filterName, ZDate filterDate, string assertionMessage, Action<ServiceTypeDateFilter> setupFilter, Func<WhsReceive, WhsReceive, IEnumerable<WhsReceive>> getExpectedJobs)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var testDate = new ZDate(2023, 12, 08);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive2");

			var fumigationService = receive2.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			if (filterName.Equals(ServiceTypeDateFilter.ServiceTypeDateCompleted))
			{
				fumigationService.ES_Completed = filterDate;
			}
			else
			{
				fumigationService.ES_Booked = filterDate;
			}

			Factory.Save();
			Asserter.AddToScope(receive1, receive2);

			var filter = (ServiceTypeDateFilter)FilterStripBizO[filterName];
			setupFilter(filter);

			Asserter.AssertMatches(assertionMessage, filter, getExpectedJobs(receive1, receive2).ToArray());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ReceiveFilterBusinessObject();
		}

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref)
		{
			return Helper.CreateWhsReceive(org, whs, @ref);
		}

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine((WhsReceive)docket, part, units);
			Factory.Save();

			return inventory.InDocketLine;
		}

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection()
		{
			return new WhsReceiveCollection(Factory);
		}

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = base.GetExpectedDocketStatus();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.AttachedToPick));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Picking));
			return status;
		}

		protected override void SetupTestLineData()
		{
			Part11 = Helper.CreateProduct(Org1, "P11");
			Part12 = Helper.CreateProduct(Org1, "P12");
			Part21 = Helper.CreateProduct(Org2, "P21");
			Part22 = Helper.CreateProduct(Org2, "P22");
			Part = Helper.CreateProduct(Org1, "P");

			var relation = Part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = Org2.PK;

			Inv1 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket11, Part11, 10);
			Inv2 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket11, Part12, 10);
			Inv3 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket12, Part11, 10);
			Inv4 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket12, Part12, 10);
			Inv5 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket12, Part, 10);
			Inv6 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket21, Part21, 10);
			Inv7 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket21, Part22, 10);
			Inv8 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket21, Part, 10);
			Inv9 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket22, Part21, 10);
			Inv10 = Helper.CreateWhsReceiveInventoryLine((WhsReceive)Docket22, Part22, 10);
		}

		protected override void SetAttributesForTestFilterLine()
		{
			Inv1.WI_PartAttrib1 = "PA11";
			Inv2.WI_PartAttrib1 = "PA12";
			Inv3.WI_PartAttrib1 = "PA13";
			Inv4.WI_PartAttrib1 = "PA14";
			Inv6.WI_PartAttrib1 = "PA15";
			Inv7.WI_PartAttrib1 = "PA16";

			// attribute 2
			Inv1.WI_PartAttrib2 = "PA21";
			Inv2.WI_PartAttrib2 = "PA22";
			Inv3.WI_PartAttrib2 = "PA23";
			Inv4.WI_PartAttrib2 = "PA24";
			Inv6.WI_PartAttrib2 = "PA25";
			Inv7.WI_PartAttrib2 = "PA26";

			// attribute 3
			Inv1.WI_PartAttrib3 = "PA31";
			Inv2.WI_PartAttrib3 = "PA32";
			Inv3.WI_PartAttrib3 = "PA33";
			Inv4.WI_PartAttrib3 = "PA34";
			Inv6.WI_PartAttrib3 = "PA35";
			Inv7.WI_PartAttrib3 = "PA36";
		}

		protected override void SetCustomsDataForTestFilterEntryKey()
		{
			// attribute 1
			Inv1.WI_BondedEntryKey = "BE11";
			Inv2.WI_BondedEntryKey = "BE12";
			Inv3.WI_BondedEntryKey = "BE13";
			Inv4.WI_BondedEntryKey = "BE14";
			Inv6.WI_BondedEntryKey = "BE15";
			Inv7.WI_BondedEntryKey = "BE16";
		}

		WhsInventoryView Inv1;
		WhsInventoryView Inv2;
		WhsInventoryView Inv3;
		WhsInventoryView Inv4;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupTestLineData")]
		WhsInventoryView Inv5;
		WhsInventoryView Inv6;
		WhsInventoryView Inv7;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupTestLineData")]
		WhsInventoryView Inv8;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupTestLineData")]
		WhsInventoryView Inv9;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupTestLineData")]
		WhsInventoryView Inv10;

		#endregion
	}

	#endregion

	#region Class: ReceiveFilterBusinessObject_AccountingFilterStripTest

	public class ReceiveFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsReceive>
	{
		#region GetNewBusinessObjectForFilterCollection

		protected override WhsReceive GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<WhsReceive>();
		}

		#endregion

		#region FilterStripModuleID

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.WhsReceive; }
		}

		#endregion

		#region ShouldUseBillingFilters

		protected override bool ShouldUseBillingFilters
		{
			get { return false; }
		}

		#endregion
	}

	#endregion
}
