using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferFilterBusinessObject))]
	public class TransferFilterBusinessObjectTest : DocketFilterBusinessObjectTest<TransferFilterBusinessObject, WhsTransfer>
	{
		#region Filters

		protected override bool HasAdditionalReferences => false;

		protected override bool SupportsContainerNoFilter => false;

		protected override bool SupportsCustomerReferenceFilter => false;

		protected override bool SupportsTransportReferenceFilter => false;

		#region TestTransferTypeFilter

		public void TestTransferTypeFilter()
		{
			const string filterName = "Transfer Type";

			SetupTestData();
			Docket11.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsSource;
			Docket21.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsDest;
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)TransferFilterBusinessObject.TransferTypeOptions.InterWarehouseTransfers);
			DocketAssert(true, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)TransferType.Codes.InterWhsSource);
			DocketAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)TransferType.Codes.InterWhsDest);
			DocketAssert(false, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)TransferType.Codes.Internal);
			DocketAssert(false, true, false, true);
		}

		public void TestTransferTypeFilter_NonPersistentTypes()
		{
			const string filterName = "Transfer Type";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var dockDoorTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			var dockDoorTransfer = dockDoorTransferLine.Docket;

			var internalTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;

			var autoCreatedReplenishmentTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			autoCreatedReplenishmentTransfer.WD_IsPickFaceReplenishment = true;
			Factory.Save();

			Asserter.AddToScope(dockDoorTransfer);
			Asserter.AddToScope(internalTransfer);
			Asserter.AddToScope(putawayTransfer);
			Asserter.AddToScope(autoCreatedReplenishmentTransfer);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, ZString.Empty);
			Asserter.AssertMatches("All transfers should be found if filter is empty.", DocketFilter.Filter, dockDoorTransfer, internalTransfer, putawayTransfer, autoCreatedReplenishmentTransfer);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)TransferType.Codes.Internal);
			Asserter.AssertMatches("Should only show the internal transfer.", DocketFilter.Filter, internalTransfer);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)NonPersistentTransferType.Codes.Putaway);
			Asserter.AssertMatches("Should only show the putaway transfer.", DocketFilter.Filter, putawayTransfer);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)NonPersistentTransferType.Codes.OutboundDockDoor);
			Asserter.AssertMatches("Should only show the outbound dock door transfer.", DocketFilter.Filter, dockDoorTransfer);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)NonPersistentTransferType.Codes.AutoCreatedReplenishment);
			Asserter.AssertMatches("Should only show the auto created replenishment transfer.", DocketFilter.Filter, autoCreatedReplenishmentTransfer);
		}

		#endregion

		#region TestStaffFilters

		#region TestPickedByFilter

		public void TestPickedByFilter()
		{
			var filterName = "Picked By";
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var staff1 = Helper.CreateGlbStaff("AAA", "AAA");
			var staff2 = Helper.CreateGlbStaff("BBB", "BBB");
			var staff3 = Helper.CreateGlbStaff("CCC", "CCC");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation).GS_NKPickedBy = "AAA";
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation).GS_NKPickedBy = "BBB";
			transfer1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation).GS_NKPickedBy = "AAA";
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer2.RunPreSaveValidation();

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer3.RunPreSaveValidation();

			AssertStaffFilter(filterName, transfer1, transfer2, transfer3);
		}

		#endregion

		#region TestPutawayByFilter

		public void TestPutawayByFilter()
		{
			var filterName = "Putaway By";
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation, "");

			var staff1 = Helper.CreateGlbStaff("AAA", "AAA");
			var staff2 = Helper.CreateGlbStaff("BBB", "BBB");
			var staff3 = Helper.CreateGlbStaff("CCC", "CCC");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation)[WhsDocketLineSchema.WE_GS_NKPutawayBy] = "AAA";
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation)[WhsDocketLineSchema.WE_GS_NKPutawayBy] = "BBB";
			transfer1.RunPreSaveValidation(); // to commit inventory

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation)[WhsDocketLineSchema.WE_GS_NKPutawayBy] = "AAA";
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer2.RunPreSaveValidation(); // to commit inventory

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer3.RunPreSaveValidation(); // to commit inventory

			AssertStaffFilter(filterName, transfer1, transfer2, transfer3);
		}

		#endregion

		void AssertStaffFilter(string filterName, WhsTransfer transfer1, WhsTransfer transfer2, WhsTransfer transfer3)
		{
			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(transfer1);
			Asserter.AddToScope(transfer2);
			Asserter.AddToScope(transfer3);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, ZString.Empty);
			Asserter.AssertMatches("All transfers should be found if filter is empty.", DocketFilter.Filter, transfer1, transfer2, transfer3);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)"AAA");
			Asserter.AssertMatches(string.Format("Only transfers that have AAA as a {0} of at least 1 line should be found.", filterName), DocketFilter.Filter, transfer1, transfer2);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)"BBB");
			Asserter.AssertMatches(string.Format("Only one transfer have BBB as a {0}, so only it should be found.", filterName), DocketFilter.Filter, transfer1);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, (ZString)"CCC");
			Asserter.AssertMatches(string.Format("No transfers have CCC as {0}, so none should be found.", filterName), DocketFilter.Filter);
		}

		#endregion

		#region TestDateFilters

		[TestDate(2012, 1, 19)]
		public void TestRFSourceDatePicked()
		{
			string filterName = "RF Source Date Picked";
			string staffColumnDescription = "Date Picked";

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			var today = ZDateTimeOffset.Today;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation).PickedTime = today;
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation).PickedTime = today.AddDays(2);
			transfer1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation).PickedTime = today;
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer2.RunPreSaveValidation();

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer3.RunPreSaveValidation();

			AssertRFDateFilters(filterName, staffColumnDescription, today.Date, transfer1, transfer2, transfer3);
		}

		[TestDate(2012, 1, 19)]
		public void TestRFDestDateTransferred()
		{
			string filterName = "RF Dest. Date Transferred";
			string staffColumnDescription = "Date Transferred";
			var today = ZDateTime.Today;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			var transferLine11 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation);
			transferLine11.FinaliseDocketLine();

			var transferLine12 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation);
			var twoDaysLater = today.AddDays(2);
			TestDateAttribute.Date = twoDaysLater.ToDateTime();
			transferLine12.FinaliseDocketLine();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			var transferLine21 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation);
			TestDateAttribute.Date = today.ToDateTime();
			transferLine21.FinaliseDocketLine();

			var transferLine22 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation);
			transferLine22.RunPreSaveValidation(); // to commit inventory

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer3.RunPreSaveValidation(); // to commit inventory

			AssertRFDateFilters(filterName, staffColumnDescription, today, transfer1, transfer2, transfer3);
		}

		void AssertRFDateFilters(string filterName, string staffColumnDescription, ZDateTime today, WhsTransfer transfer1, WhsTransfer transfer2, WhsTransfer transfer3)
		{
			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(transfer1);
			Asserter.AddToScope(transfer2);
			Asserter.AddToScope(transfer3);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, ZDateTime.Empty);
			Asserter.AssertMatches("All transfers should be found if filter is empty.", DocketFilter.Filter, transfer1, transfer2, transfer3);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, today);
			Asserter.AssertMatches(string.Format("Only transfers that have {0} of today on at least 1 line should be found.", staffColumnDescription), DocketFilter.Filter, transfer1, transfer2);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, today.AddDays(1), today.AddDays(5));
			Asserter.AssertMatches(
				string.Format("Only one transfer have {0} on at least 1 line between {1} and {2}, so only it should be found.",
				staffColumnDescription, today.AddDays(1).ToShortDateString(), today.AddDays(5).ToShortDateString()),
				DocketFilter.Filter, transfer1);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, filterName, today.AddDays(-1));
			Asserter.AssertMatches(string.Format("No transfers have {0} of {1} on any line, so no transfers should be found.", staffColumnDescription, today.AddDays(-1).ToShortDateString()), DocketFilter.Filter);
		}

		#endregion

		#region TestTransferPalletIdFilters

		#region TestTransferPalletIdFilters_DestinationPalletIdFilter

		public void TestTransferPalletIdFilters_DestinationPalletIdFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocName = "A-1";
			var destinationLocName = "A-2";
			var sourcePalletId = "ID12";
			var destinaitonPalletId = "";
			var sourcePalletId2 = "XD56";
			var destinaitonPalletId2 = "XD78";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.FindLocation(sourceLocName), sourcePalletId);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, data.Whs1.FindLocation(sourceLocName), sourcePalletId2);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocName, sourcePalletId, destinationLocName, destinaitonPalletId);
			transferLine.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocName, sourcePalletId2, destinationLocName, destinaitonPalletId2);
			transferLine2.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine2);

			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(transfer1);
			Asserter.AddToScope(transfer2);

			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID (Destination)"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "XD7";
			Asserter.AssertMatches("Starts With 'XD7' should return only transfer2", filter, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "ID3";
			Asserter.AssertMatches("DoesNotStartWith 'ID4' should return both transfers", filter, transfer1, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "XD78";
			Asserter.AssertMatches("Equals 'XD78' should return only transfer2", filter, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "XD78";
			Asserter.AssertMatches("NotEqual 'XD78' should return only transfer1", filter, transfer1);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank should return transfer1.", filter, transfer1);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank should return transfer2.", filter, transfer2);
		}

		#endregion //TestTransferPalletIdFilters_DestinationPalletIdFilter

		#region TestTransferPalletFilters_SourcePalletIdFilter

		public void TestTransferPalletFilters_SourcePalletIdFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocName = "A-1";
			var destinationLocName = "A-2";
			var sourcePalletId = "";
			var destinaitonPalletId = "ID34";
			var sourcePalletId2 = "XD56";
			var destinaitonPalletId2 = "XD78";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.FindLocation(sourceLocName), sourcePalletId);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, data.Whs1.FindLocation(sourceLocName), sourcePalletId2);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocName, sourcePalletId, destinationLocName, destinaitonPalletId);
			transferLine.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocName, sourcePalletId2, destinationLocName, destinaitonPalletId2);
			transferLine2.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine2);

			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(transfer1);
			Asserter.AddToScope(transfer2);

			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID (Source)"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "XD5";
			Asserter.AssertMatches("Starts With 'XD5' should return only transfer2", filter, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "Source";
			Asserter.AssertMatches("DoesNotStartWith 'Source' should return both transfers", filter, transfer1, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "XD56";
			Asserter.AssertMatches("Equals 'XD56' should return only transfer2", filter, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "Source";
			Asserter.AssertMatches("NotEqual 'XD56' should return both transfers", filter, transfer1, transfer2);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank should return transfer1.", filter, transfer1);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank should return transfer2.", filter, transfer2);
		}

		#endregion //TestTransferFilters_SourcePalletIdFilter

		protected override void TestPalletIDCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, "A-1", "", "A-2", "");
			transferLine.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, "A-1", "", "A-2", "DestinationId");
			transferLine2.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine2);

			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(transfer1);
			Asserter.AddToScope(transfer2);

			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID (Destination)"];

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("Destination IsNotBlank should return transfer2.", filter, transfer2);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "DestinationId";
			Asserter.AssertMatches("Destination NotEqual 'DestinationId' should return transfer1.", filter, transfer1);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("Destination IsBlank should return transfer1.", filter, transfer1);
		}

		#region Test Filter MaxLength

		public override void TestFilterMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Pallet ID (Destination) should be set correctly.", WhsDocketLineSchema.WE_PalletID.MaxLength, FilterStripBizO["Pallet ID (Destination)"].MaxLength);
				AssertEquals("MaxLength of Pallet ID (Source) should be set correctly.", WhsDocketLineSchema.WE_TransferFromPalletId.MaxLength, FilterStripBizO["Pallet ID (Source)"].MaxLength);
				AssertEquals("MaxLength of Customs Entry Key should be set correctly.", WhsDocketLineSchema.WE_BondedEntryKey.MaxLength, FilterStripBizO["Customs Entry Key"].MaxLength);
			});
		}

		#endregion

		#endregion // TestFilterMaxLength

		#region TestSourceLocationFilter

		public void TestSourceLocationFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var sourceLocation1 = data.Whs1.FindLocation("A-1");
			var sourceLocation2 = data.Whs1.FindLocation("A-2");
			var sourceLocation3 = data.Whs1.FindLocation("A-3");
			var destinationLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, sourceLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, sourceLocation2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m, sourceLocation3, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation1, destinationLocation).FinaliseDocketLine();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2");
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation2, destinationLocation).FinaliseDocketLine();

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3");
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 1m, sourceLocation2, destinationLocation).FinaliseDocketLine();

			Factory.Save();
			Asserter.AddToScope(transfer1, transfer2, transfer3);

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];
			AssertEquals("Category should be Locations.", FilterCategories.Locations, sourceLocationFilter.Category);
			AssertEquals("Should not be published on web.", false, sourceLocationFilter.IsPublishedOnWeb);

			warehouseFilter.IsActive = true;
			sourceLocationFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			sourceLocationFilter.Property = sourceLocation1.PK;
			Asserter.AssertMatches("Should match 1 Transfer.", transferFilter.Filter, transfer1);
			sourceLocationFilter.Property = sourceLocation2.PK;
			Asserter.AssertMatches("Should match 2 Transfers.", transferFilter.Filter, transfer2, transfer3);
			sourceLocationFilter.Property = sourceLocation3.PK;
			Asserter.AssertMatches("Should match no Transfers.", transferFilter.Filter);
		}

		public void TestSourceLocationFilter_MultipleTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var sourceLocation1 = data.Whs1.FindLocation("A-1");
			var sourceLocation2 = data.Whs1.FindLocation("A-2");
			var sourceLocation3 = data.Whs1.FindLocation("A-3");
			var destinationLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, sourceLocation2, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation1, destinationLocation).FinaliseDocketLine();
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation1, destinationLocation).FinaliseDocketLine();
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation1, destinationLocation).FinaliseDocketLine();
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation2, destinationLocation).FinaliseDocketLine();
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation2, destinationLocation).FinaliseDocketLine();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2");
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation1, destinationLocation).FinaliseDocketLine();
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation1, destinationLocation).FinaliseDocketLine();

			Factory.Save();
			Asserter.AddToScope(transfer1, transfer2);

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];
			AssertEquals("Category should be Locations.", FilterCategories.Locations, sourceLocationFilter.Category);
			AssertEquals("Should not be published on web.", false, sourceLocationFilter.IsPublishedOnWeb);

			warehouseFilter.IsActive = true;
			sourceLocationFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			sourceLocationFilter.Property = sourceLocation3.PK;
			Asserter.AssertMatches("Should match no Transfer.", transferFilter.Filter);
			sourceLocationFilter.Property = sourceLocation1.PK;
			Asserter.AssertMatches("Should match both Transfers.", transferFilter.Filter, transfer1, transfer2);
			sourceLocationFilter.Property = sourceLocation2.PK;
			Asserter.AssertMatches("Should match 1 Transfer.", transferFilter.Filter, transfer1);
		}

		public void TestSourceLocationFilter_WarehouseCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS");
			Helper.CreateRowAndGenerateLocations(whs2, "B", 5, 2, 2);
			Factory.Save();

			var sourceLocation1 = data.Whs1.FindLocation("A-1");

			var transferFilter = GetNewFilterStripBusinessObject();
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];

			sourceLocationFilter.IsActive = true;
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			sourceLocationFilter.Property = sourceLocation1.PK;
			AssertNoError(sourceLocationFilter.PropertyInfo, "Enter a valid selection.");

			warehouseFilter.Property = whs2.PK;
			sourceLocationFilter.Property = sourceLocation1.PK;
			AssertHasError(sourceLocationFilter.PropertyInfo, "Enter a valid selection.");

			sourceLocationFilter.IsActive = false;
			warehouseFilter.Property = data.Whs1.PK;
			sourceLocationFilter.IsActive = true;
			sourceLocationFilter.Property = sourceLocation1.PK;
			AssertNoError(sourceLocationFilter.PropertyInfo, "Enter a valid selection.");
		}

		public void TestSourceLocationFilter_Validator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];
			sourceLocationFilter.IsActive = true;

			sourceLocationFilter.Validation.ValidateProperty();
			AssertNoError(sourceLocationFilter.PropertyInfo, "Source Location can not be entered without a warehouse.");

			sourceLocationFilter.Property = sourceLocation.PK;
			AssertHasError(sourceLocationFilter.PropertyInfo, "Source Location can not be entered without a warehouse.");

			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			warehouseFilter.Property = ZGuid.Empty;
			sourceLocationFilter.Property = sourceLocation.PK;
			AssertHasError(sourceLocationFilter.PropertyInfo, "Source Location can not be entered without a warehouse.");

			warehouseFilter.Property = ZGuid.Invalid;
			sourceLocationFilter.Property = sourceLocation.PK;
			AssertHasError(sourceLocationFilter.PropertyInfo, "Source Location can not be entered without a warehouse.");

			warehouseFilter.Property = data.Whs1.PK;
			warehouseFilter.IsActive = true;
			sourceLocationFilter.Property = sourceLocation.PK;
			AssertNoError(sourceLocationFilter.PropertyInfo, "Source Location can not be entered without a warehouse.");
		}

		public void TestSourceLocationFilter_FiltersMatchOptionPresent()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];
			var comparisonOperators = sourceLocationFilter.ComparisonOperator_List;
			AssertContains("Source Location Filter should have Filters Match behaviour.", "filters match", comparisonOperators.CodesAsString);
		}

		public void TestSourceLocationFilter_ClearedOnWarehouseChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");

			var whs2 = Helper.CreateWarehouse("W2");
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];

			warehouseFilter.IsActive = true;
			sourceLocationFilter.IsActive = true;
			var whs1Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, data.Whs1.PK));
			var whs2Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs2.PK));
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = data.Whs1.PK;
			sourceLocationFilter.Property = sourceLocation.PK;
			AssertContainsExactElementsInAnyOrder(whs1Locations.Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = ZGuid.Empty;
			AssertEquals("Source Location Filter should be cleared when warehouse is changed.", ZGuid.Empty, sourceLocationFilter.Property);
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));
		}

		public void TestSourceLocationFilter_ClearedOnWarehouseChange_WhenInactive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");

			var whs2 = Helper.CreateWarehouse("W2");
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];

			warehouseFilter.IsActive = true;
			sourceLocationFilter.IsActive = true;
			var whs1Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, data.Whs1.PK));
			var whs2Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs2.PK));
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = data.Whs1.PK;
			sourceLocationFilter.Property = sourceLocation.PK;
			AssertContainsExactElementsInAnyOrder(whs1Locations.Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			sourceLocationFilter.IsActive = false;
			warehouseFilter.Property = ZGuid.Empty;
			sourceLocationFilter.IsActive = true;
			AssertEquals("Source Location Filter should be cleared when warehouse is changed.", ZGuid.Empty, sourceLocationFilter.Property);
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));
		}

		public void TestSourceLocationFilter_NotClearedOnWarehousePopulationToCorrectValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];

			warehouseFilter.IsActive = true;
			sourceLocationFilter.IsActive = true;
			sourceLocationFilter.Property = sourceLocation.PK;
			warehouseFilter.Property = data.Whs1.PK;

			AssertEquals("Source Location Filter should not be cleared when warehouse is populated.", sourceLocation.PK, sourceLocationFilter.Property);
		}

		public void TestSourceLocationFilter_InvalidWarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");

			var whs2 = Helper.CreateWarehouse("W2");
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var sourceLocationFilter = (ModuleGuidFilter)transferFilter[TransferFilterBusinessObject.FilterConstants.SourceLocation];

			warehouseFilter.IsActive = true;
			sourceLocationFilter.IsActive = true;
			var whs1Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, data.Whs1.PK));
			var whs2Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs2.PK));
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = data.Whs1.PK;
			AssertContainsExactElementsInAnyOrder(whs1Locations.Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), sourceLocationFilter.List.Cast<WhsLocation>().Select(l => l.PK));
		}

		#endregion

		#region TestFilterDocketTaskPlanningStatus

		protected override bool SupportsDocketPlanningStatus => true;

		#endregion

		#endregion // Filters

		#region Lookups

		#region TestTransferTypes

		public void TestTransferTypes()
		{
			var filter = new TransferFilterBusinessObject();
			AssertEquals(7, filter.TransferTypes.Count);
			AssertEquals(true, filter.TransferTypes.ContainsCode(TransferType.Codes.Internal));
			AssertEquals(true, filter.TransferTypes.ContainsCode(TransferFilterBusinessObject.TransferTypeOptions.InterWarehouseTransfers));
			AssertEquals(true, filter.TransferTypes.ContainsCode(NonPersistentTransferType.Codes.Putaway));
			AssertEquals(true, filter.TransferTypes.ContainsCode(NonPersistentTransferType.Codes.OutboundDockDoor));
			AssertEquals(true, filter.TransferTypes.ContainsCode(NonPersistentTransferType.Codes.AutoCreatedReplenishment));
			AssertEquals(true, filter.TransferTypes.ContainsCode(TransferType.Codes.InterWhsSource));
			AssertEquals(true, filter.TransferTypes.ContainsCode(TransferType.Codes.InterWhsDest));
		}

		#endregion

		#region TestGlobalStaffs

		public void TestGlobalStaffs()
		{
			var filter = new TransferFilterBusinessObject();
			var glbStaff1 = Factory.New<GlbStaff>();
			var glbStaff2 = Factory.New<GlbStaff>();

			AssertEquals(true, filter.GlobalStaffs.Contains(glbStaff1));
			AssertEquals(true, filter.GlobalStaffs.Contains(glbStaff2));
		}

		#endregion

		#endregion

		#region Implementation

		protected override WhsTransfer GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null)
			=> throw new System.NotSupportedException("Transfer does not support Filter by Transport Co.");

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref)
		{
			return Helper.CreateWhsTransfer(org, whs, @ref);
		}

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			var whs = docket.Warehouse;
			var location = whs.DefaultLocation;
			var receiveReference = "R" + docket.WD_ExternalReference + docket.Lines.Count;
			Helper.CreateWhsReceiveWithInventory(docket.Client, whs, receiveReference, part, units, location, "");

			var transferLine = Helper.CreateWhsTransferLine((WhsTransfer)docket, part, units, location, location);
			transferLine.RunPreSaveValidation(); // to commit inventory

			return transferLine;
		}

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection()
		{
			return new WhsTransferCollection(Factory);
		}

		protected override bool SupportsFilterForErrorStatus => false;

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = new DocketStatus();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.New));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.AttachedToPick));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Picking));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			return status;
		}

		#endregion
	}
}
