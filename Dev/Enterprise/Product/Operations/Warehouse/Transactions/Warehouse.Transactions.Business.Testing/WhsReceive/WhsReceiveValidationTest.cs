using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveValidationTest : WhsDocketValidationTestCase<WhsReceive>
	{
		#region TestCheckWD_DocketType

		public void TestCheckWD_DocketType()
		{
			Docket.WD_DocketType = "";
			AssertEquals("Docket type validation accepting blank", true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Receive;
			AssertEquals("Docket type validation not accepting valid docket type: " + Docket.WD_DocketType, false, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Adjustment;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Transfer;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Order;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			// Also test a dodgy random type
			Docket.WD_DocketType = ";;;";
			AssertEquals("Docket type validation accepting junk", true, Docket.WD_DocketTypeInfo.HasErrors());
		}

		#endregion

		#region TestCheckWD_ReceiveCategory

		public void TestCheckWD_ReceiveCategory()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			receiveCategories.Add("RC3", (NoResString)"Receive Category 3");

			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ReceiveCategory = "ABC";
			AssertEquals("Receive category should have an error when an invalid category is entered.", true, receive.WD_ReceiveCategoryInfo.HasErrors());

			receive.WD_ReceiveCategory = "";
			AssertEquals("Receive category can be empty.", false, Docket.WD_ReceiveCategoryInfo.HasErrors());

			receive.WD_ReceiveCategory = "RC1";
			AssertEquals("Receive category should not have any error when a valid category is entered.", false, receive.WD_ReceiveCategoryInfo.HasErrors());

			var today = ZDate.Today;
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1");
			inventory.WI_SerialNumber = "SNN";
			receive2.AllocateLocationsWithMock();

			receive2.FinaliseDocket();
			receive2.WD_ReceiveCategory = "ABC";
			AssertEquals("Receive category should not have any error if the receive is finalized.", false, receive2.WD_ReceiveCategoryInfo.HasErrors());
		}

		#endregion

		#region TestValidateWD_WW_Whs

		protected override void TestValidateWD_WW_WhsCore()
		{
			base.TestValidateWD_WW_WhsCore();

			var docket = GetNewBusinessObject();
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;
			docket.WD_WW_Whs = Helper.CreateWarehouse("1").PK;
			AssertHasError(docket.WD_WW_WhsInfo, WhsDocketValidation.BondNotEnabledErrorMsg);

			Helper.EnableWarehouseForBond(docket.Warehouse, true);
			docket.Validation.ValidateWD_WW_Whs();
			AssertNoErrors(docket.WD_WW_WhsInfo);

			Helper.EnableWarehouseForFreeStore(docket.Warehouse, false);
			docket.WD_DocketSubType = ReceiveType.Codes.Receipt;
			docket.Validation.ValidateWD_WW_Whs();
			AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.FreeStoreNotEnabledErrorMsg);
		}

		#endregion

		#region TestCheckWD_DocketStatus

		public void TestCheckWD_DocketStatus()
		{
			Docket.WD_DocketStatus = "";
			AssertEquals("Docket status validation accepting blank", true, Docket.WD_DocketStatusInfo.HasErrors());

			// here we test each type, but only Receive should be accepted
			for (int i = 0; i < Docket.Statuses.Count; i++)
			{
				Docket.WD_DocketStatus = (ZString)Docket.Statuses[i].Code;
				if ((Docket.WD_DocketStatus == DocketStatus.Codes.New) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Entered) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Finalised) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Cancelled) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Putaway) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Error))
				{
					AssertEquals("Docket status validation NOT accepting valid docket status: " + Docket.WD_DocketStatus, false, Docket.WD_DocketStatusInfo.HasErrors());
				}
				else
				{
					AssertEquals("Docket status validation accepting invalid docket status: " + Docket.WD_DocketStatus, true, Docket.WD_DocketStatusInfo.HasErrors());
				}
			}

			// Also test a dodgy random type
			Docket.WD_DocketStatus = ";;;";
			AssertEquals("Docket status validation accepting junk", true, Docket.WD_DocketStatusInfo.HasErrors());
		}

		#endregion

		#region TestCheckWD_BookingDate

		public void TestCheckWD_BookingDate()
		{
			TestMinMaxDateTimeOffset(Docket.WD_BookingDateInfo, ErrorCheckType.HasWarnings, ZDateTimeOffset.Today.AddMonths(-1), ZDateTimeOffset.Today);
		}

		#endregion

		#region TestCheckWE_OP

		public void TestCheckWE_OP_InactiveProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			var line = receive.Lines[0];
			Helper.CreateAsnLine(receive, data.Part1, 10m);
			Factory.Save();

			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receive.FinaliseDocket();
				AssertNoError(line.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
				AssertNoWarning(line.WE_OPInfo, "This Product is Inactive");
			}

			data.Part1.OP_IsActive = false;
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receive.FinaliseDocket();
				AssertHasError(line.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
				AssertNoWarning(line.WE_OPInfo, "This Product is Inactive");
			}

			line.WE_TransactionQuantity = 0m;
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receive.FinaliseDocket();
				AssertNoError(line.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
				AssertHasWarning(line.WE_OPInfo, "This Product is Inactive");
			}
		}

		#endregion

		#region CheckWD_ArrivalDate

		public void TestCheckWD_ArrivalDate()
		{
			Docket.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Docket.FinaliseDocket();
			AssertHasErrors(Docket.WD_ArrivalDateInfo);

			Docket.WD_ArrivalDate = ZDateTimeOffset.Now;
			Docket.FinaliseDocket();
			AssertNoErrors(Docket.WD_ArrivalDateInfo);

			TestMinMaxDateTimeOffset(Docket.WD_ArrivalDateInfo, ErrorCheckType.HasWarnings, ZDateTimeOffset.Today.AddMonths(-1), ZDateTimeOffset.Today);
		}

		public void TestCheckWD_ArrivalDateRestrictionOnPreDateWithoutFinalisedDateOverride()
		{
			Env.Security.WhsReceivePreDate.IsAllowed = false;
			Docket.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-7);
			AssertNoErrors(Docket.WD_ArrivalDateInfo);

			Docket.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertNoErrors(Docket.WD_ArrivalDateInfo);
		}

		public void TestCheckWD_ArrivalDateRestrictionOnPreDateWithFinalisedDateOverride()
		{
			var warehouse = Helper.CreateWarehouse("1");
			Docket.WD_WW_Whs = warehouse.PK;
			warehouse.WW_UseArrivalDateForInwardsFinalisedDate = true;

			Env.Security.WhsReceivePreDate.IsAllowed = true;
			Docket.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertNoErrors(Docket.WD_ArrivalDateInfo);

			Env.Security.WhsReceivePreDate.IsAllowed = false;
			Docket.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-7);
			AssertNoErrors(Docket.WD_ArrivalDateInfo);

			// ensure this error has preference over standard < 1 month warning
			Docket.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			AssertHasErrors(Docket.WD_ArrivalDateInfo);

			Docket.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertHasErrors(Docket.WD_ArrivalDateInfo);
		}

		[TestDate(2016, 1, 1)]
		public void TestCheckWD_ArrivalDateErrorFutureDateFinalization()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, finalise: false);
			AssertNoError("Precondition - Make sure that arrival date is not a future date. ", receive.WD_ArrivalDateInfo, "Arrival date is a future date.");

			receive.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			AssertNoError("Should not show error until finalization", receive.WD_ArrivalDateInfo, "Arrival date is a future date.");
			AssertHasWarning("Should not show error until finalization", receive.WD_ArrivalDateInfo, "Arrival date is a future date.");

			receive.FinaliseDocket();
			AssertHasError("Should show error when finalizig.", receive.WD_ArrivalDateInfo, "Arrival date is a future date.");
		}

		public void TestCheckWD_ArrivalDateEmptyWithReceivedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 0m);

			AssertNoErrors("Precondition - Arrival Date is empty and docket can be saved.", receive.WD_ArrivalDateInfo);

			receiveLine2.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			receiveLine2.WE_PalletID = "PLT001";
			receive.Validation.ValidateWD_ArrivalDate();
			AssertHasError("Docket must have validation errors for Arrival Date.", receive.WD_ArrivalDateInfo, "Please enter an Arrival Date.");

			receive.FinaliseDocket();
			AssertHasError("Should show error when finalizing.", receive.WD_ArrivalDateInfo, "Please enter an Arrival Date.");
		}

		public void TestCheckWD_ArrivalDateEmptyWithPutawayLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 0m);

			AssertNoErrors("Precondition - Arrival Date is empty and docket can be saved.", receive.WD_ArrivalDateInfo);

			receiveLine2.WE_WL = location.PK;
			receive.Validation.ValidateWD_ArrivalDate();
			AssertHasError("Docket must have validation errors for Arrival Date.", receive.WD_ArrivalDateInfo, "Please enter an Arrival Date.");

			receive.FinaliseDocket();
			AssertHasError("Should show error when finalizing.", receive.WD_ArrivalDateInfo, "Please enter an Arrival Date.");
		}

		public void TestCheckWD_ArrivalDateEmptyWithStartedUnloading()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 0m);

			AssertNoErrors("Precondition - Arrival Date is empty and docket can be saved.", receive.WD_ArrivalDateInfo);

			receive.WD_StartedReceivingTimeUtc = DateTime.UtcNow;
			receive.Validation.ValidateWD_ArrivalDate();
			AssertHasError("Docket must have validation errors for Arrival Date.", receive.WD_ArrivalDateInfo, "Please enter an Arrival Date.");
		}

		#endregion

		#region CheckWD_TotalUnits

		public void TestCheckWD_TotalUnits()
		{
			Docket.WD_TotalUnits = 5m;
			AssertNotEquals("Precondition - make sure that WD_TotalUnits is different to Docket.WD_TotalUnitsFromLines", Docket.WD_TotalUnits, Docket.WD_TotalUnitsFromLines);
			AssertHasWarning(Docket.WD_TotalUnitsInfo, "Total Units 5 does not equal the total of all line units 0.");

			var inventory = Docket.Lines.AddNew().Inventory[0];
			inventory.WI_InDocketLineUnits = 5m;
			Docket.Validation.ValidateWD_TotalUnits();
			AssertNoWarnings(Docket.WD_TotalUnitsInfo);
		}

		#endregion

		#region TestCheckWD_TotalUnits_ErrorWhenFinalising

		public void TestCheckWD_TotalUnits_ErrorWhenFinalising()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 20m, data.Whs1.DefaultLocation);
			receive1.WD_TotalUnits = 20m;

			var expectedErrorMessage = "Total Units 20 does not equal the total of all line units 30.";
			WarehouseDataRegistry.Instance.TotalUnitsValidation.SetValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, false);
			using (new SemaphoreManager(receive1.FinaliseDocketSemaphore)) // is finalising
			{
				receive1.Validation.ValidateWD_TotalUnits();
				AssertHasWarning(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
				AssertNoError(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
			}

			receive1.Validation.ValidateWD_TotalUnits();
			AssertHasWarning(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
			AssertNoError(receive1.WD_TotalUnitsInfo, expectedErrorMessage);

			WarehouseDataRegistry.Instance.TotalUnitsValidation.SetValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true);
			using (new SemaphoreManager(receive1.FinaliseDocketSemaphore)) // is finalising
			{
				receive1.Validation.ValidateWD_TotalUnits();
				AssertHasError(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
				AssertNoWarning(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
			}

			receive1.Validation.ValidateWD_TotalUnits();
			AssertHasWarning(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
			AssertNoError(receive1.WD_TotalUnitsInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWD_TotalUnits_ErrorWhenFinalising_IgnorePickByBOMReceives

		public void TestCheckWD_TotalUnits_ErrorWhenFinalising_IgnorePickByBOMReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive1.WD_TotalUnits = 20m;
			receive1.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();

			WarehouseDataRegistry.Instance.TotalUnitsValidation.SetValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, false);
			using (new SemaphoreManager(receive1.FinaliseDocketSemaphore)) // is finalising
			{
				receive1.Validation.ValidateWD_TotalUnits();
				AssertNoWarnings(receive1.WD_TotalUnitsInfo);
				AssertNoErrors(receive1.WD_TotalUnitsInfo);
			}

			receive1.Validation.ValidateWD_TotalUnits();
			AssertNoWarnings(receive1.WD_TotalUnitsInfo);
			AssertNoErrors(receive1.WD_TotalUnitsInfo);

			WarehouseDataRegistry.Instance.TotalUnitsValidation.SetValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true);
			using (new SemaphoreManager(receive1.FinaliseDocketSemaphore)) // is finalising
			{
				receive1.Validation.ValidateWD_TotalUnits();
				AssertNoWarnings(receive1.WD_TotalUnitsInfo);
				AssertNoErrors(receive1.WD_TotalUnitsInfo);
			}

			receive1.Validation.ValidateWD_TotalUnits();
			AssertNoWarnings(receive1.WD_TotalUnitsInfo);
			AssertNoErrors(receive1.WD_TotalUnitsInfo);
		}

		#endregion

		#region TestCheckWD_TotalPallets_WhenFinalizing

		public void TestCheckWD_TotalPallets_WarningWhenFinalizing_RegistryValidationDisabled()
		{
			TestCheckWD_TotalPallets_WhenFinalizingCore(false);
		}

		public void TestCheckWD_TotalPallets_ErrorWhenFinalizing_RegistryValidationEnabled()
		{
			TestCheckWD_TotalPallets_WhenFinalizingCore(true);
		}

		void TestCheckWD_TotalPallets_WhenFinalizingCore(bool enableValidation)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 1;

			var expectedErrorMessage = "Total Pallets 1 does not equal the total of received Pallets 2.";

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, enableValidation))
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore)) // is finalising
			{
				receive.Validation.ValidateWD_TotalPallets();
				if (enableValidation)
				{
					AssertHasError(receive.WD_TotalPalletsInfo, expectedErrorMessage);
					AssertNoWarning(receive.WD_TotalPalletsInfo, expectedErrorMessage);
				}
				else
				{
					AssertHasWarning(receive.WD_TotalPalletsInfo, expectedErrorMessage);
					AssertNoError(receive.WD_TotalPalletsInfo, expectedErrorMessage);
				}
			}

			receive.Validation.ValidateWD_TotalPallets();
			AssertHasWarning(receive.WD_TotalPalletsInfo, expectedErrorMessage);
			AssertNoError(receive.WD_TotalPalletsInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWD_TotalWeightVolume

		#region TestCheckWD_TotalWeight_MatchesLinesLevelWeight_WithInventory

		public void TestCheckWD_TotalWeight_MatchesLinesLevelWeight_WithInventory()
		{
			TestCheckWD_TotalWeightVolume_MatchesLinesLevelWeightVolume_WithInventoryCore("Weight", WhsDocketSchema.WD_TotalWeight, WhsDocketSchema.WD_TotalWeightUnit, OrgSupplierPartSchema.OP_Weight, OrgSupplierPartSchema.OP_WeightUQ, Constants.Weight.Kilograms);
		}

		#endregion

		#region TestCheckWD_TotalCubic_MatchesLinesLevelVolume_WithInventory

		public void TestCheckWD_TotalCubic_MatchesLinesLevelVolume_WithInventory()
		{
			TestCheckWD_TotalWeightVolume_MatchesLinesLevelWeightVolume_WithInventoryCore("Volume", WhsDocketSchema.WD_TotalCubic, WhsDocketSchema.WD_TotalCubicUnit, OrgSupplierPartSchema.OP_Cubic, OrgSupplierPartSchema.OP_CubicUQ, Constants.Volume.CubicMetres);
		}

		#endregion

		#region TestCheckWD_TotalWeight_MatchesLinesLevelWeight_WithInventoryCore

		void TestCheckWD_TotalWeightVolume_MatchesLinesLevelWeightVolume_WithInventoryCore(ZString propertyDescription, SchemaDecimalColumn docketWeightVolumeSchemaColumn, SchemaStringColumn docketWeightVolumeUQShemaColumn, SchemaDecimalColumn partWeightVolumeSchemaColumn, SchemaStringColumn partWeightVolumeUQSchemaColumn, ZString weightVolumeUQ)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1[partWeightVolumeSchemaColumn] = 5m;
			data.Part1[partWeightVolumeUQSchemaColumn] = weightVolumeUQ;

			var expectedWarningMessagePart = ZString.Format("Original {0} of product calculated from the product master file was", propertyDescription);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive[docketWeightVolumeUQShemaColumn] = weightVolumeUQ;
			var info = receive.ZPropertyInfoHash[docketWeightVolumeSchemaColumn.Name];

			var inventory_Unchanged = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			var inventory_Modified = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory_Modified.WI_InDocketLineUnits = 15m;
			AssertNotNull("Precondition - inventory should have a docket line", inventory_Unchanged.InDocketLine);
			AssertNotNull("Precondition - inventory should have a docket line", inventory_Modified.InDocketLine);
			AssertEquals("Weight/Volume of a Receive should be populated from lines.", 100m, receive[docketWeightVolumeSchemaColumn]);
			AssertEquals(false, info.Notifications.Any(n => n.Message.Contains(expectedWarningMessagePart)));

			receive[docketWeightVolumeSchemaColumn] = 70m;
			info.Notifications.Single(n => n.Message.Contains(expectedWarningMessagePart));

			receive[docketWeightVolumeSchemaColumn] = -10m;
			AssertHasErrors(info);
			AssertEquals(false, info.Notifications.Any(n => n.Message.Contains(expectedWarningMessagePart)));

			receive[docketWeightVolumeSchemaColumn] = 100m;
			AssertEquals(false, info.Notifications.Any(n => n.Message.Contains(expectedWarningMessagePart)));
		}

		#endregion

		public void TestCheckWD_TotalVolume_ProductWithValidPkButNotInDB()
		{
			TestCheckWD_TotalWeightVolume_ProductWithValidPkButNotInDBCore(WhsDocketSchema.WD_TotalCubic);
		}

		public void TestCheckWD_TotalWeight_ProductWithValidPkButNotInDB()
		{
			TestCheckWD_TotalWeightVolume_ProductWithValidPkButNotInDBCore(WhsDocketSchema.WD_TotalWeight);
		}

		void TestCheckWD_TotalWeightVolume_ProductWithValidPkButNotInDBCore(SchemaDecimalColumn docketWeightVolumeSchemaColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var newFactory = new BusinessObjectFactory();
			var prodInNewFactory = newFactory.Load<OrgSupplierPart>(data.Part1.PK);
			prodInNewFactory.Delete();
			newFactory.Save();

			AssertEquals("Precondition", true, receiveLine.WE_OP.IsValid);
			AssertNull("Precondition", Factory.Load<OrgSupplierPart>(receiveLine.WE_OP));
			AssertNoExceptionThrown(() => receive[docketWeightVolumeSchemaColumn] = 10);
		}

		#endregion

		#region TestTransportCoNameOrPKValidation

		protected override void ValidateTransportCoNameOrPK(WhsDocket docket)
		{
			((WhsReceiveValidation)docket.Validation).ValidateTransportCoNameOrPK();
		}

		#endregion

		#region TestCheckWD_IsInwardsProcessingJob

		public void TestCheckWD_IsInwardsProcessingJob()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_IsVirtualWarehouse = false;

			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = warehouse.PK;

			docket.WD_IsInwardsProcessingJob = true;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Inward Processing Jobs can only be created in Virtual Warehouses.");

			docket.WD_IsInwardsProcessingJob = false;
			AssertNoErrors(docket.WD_IsInwardsProcessingJobInfo);

			warehouse.WW_IsVirtualWarehouse = true;
			docket.WD_IsInwardsProcessingJob = true;
			AssertNoErrors(docket.WD_IsInwardsProcessingJobInfo);

			docket.WD_WW_Whs = ZGuid.Empty;
			docket.WD_IsInwardsProcessingJob = true;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Inward Processing Jobs can only be created in Virtual Warehouses.");
		}

		#endregion

		#region TestCheckWD_HoldPalletIDPutaway

		public void TestCheckWD_HoldPalletIDPutaway_EnableFlagBeforeStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			receive.WD_HoldPalletIDPutaway = false;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PID1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			//Enable flag allowed
			receive.WD_HoldPalletIDPutaway = true;
			receive.Validation.ValidateWD_HoldPalletIDPutaway();
			AssertEquals("Precondition - flag enabled", true, receive.WD_HoldPalletIDPutaway);
			AssertEquals("Precondition - orginal value of flag", false, receive.WD_HoldPalletIDPutawayInfo.OriginalValue);
			AssertNoNotifications(receive.WD_HoldPalletIDPutawayInfo);
		}

		public void TestCheckWD_HoldPalletIDPutaway_DisableFlagBeforeStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			receive.WD_HoldPalletIDPutaway = true;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PID1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			//Disable flag allowed
			receive.WD_HoldPalletIDPutaway = false;
			receive.Validation.ValidateWD_HoldPalletIDPutaway();
			AssertEquals(false, receive.WD_HoldPalletIDPutaway);
			AssertEquals(true, receive.WD_HoldPalletIDPutawayInfo.OriginalValue);
			AssertNoNotifications(receive.WD_HoldPalletIDPutawayInfo);
		}

		public void TestCheckWD_HoldPalletIDPutaway_EnableFlagAfterStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			receive.WD_HoldPalletIDPutaway = false;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PID1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.Now;

			Factory.Save();

			//Enable flag not allowed while receiving
			receive.WD_HoldPalletIDPutaway = true;
			receive.Validation.ValidateWD_HoldPalletIDPutaway();
			AssertHasError(receive.WD_HoldPalletIDPutawayInfo, "Cannot set Hold Pallet ID For RF Putaway once Receiving has started.");
		}

		public void TestCheckWD_HoldPalletIDPutaway_DisableFlagAfterStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			receive.WD_HoldPalletIDPutaway = true;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PID1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			//Disable flag allowed after receiving
			receive.WD_HoldPalletIDPutaway = false;
			receive.Validation.ValidateWD_HoldPalletIDPutaway();
			AssertNoNotifications(receive.WD_HoldPalletIDPutawayInfo);
		}

		public void TestCheckWD_HoldPalletIDPutaway_PreventAfterTransfersCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PID1");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation, "PID1", 10m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			receive.WD_HoldPalletIDPutaway = true;
			receive.Validation.ValidateWD_HoldPalletIDPutaway();
			AssertHasError(receive.WD_HoldPalletIDPutawayInfo, "Cannot set Hold Pallet ID For RF Putaway once a putaway transfer has been created.");
		}

		public void TestCheckWD_HoldPalletIDPutaway_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			for (var i = 1; i < 10; i++)
			{
				var palletID = $"PID1{i}";
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, palletID);

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				transfer.WD_IsPutawayTransfer = true;
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.DefaultLocation, palletID, 10m);
				transfer.RunPreSaveValidation();
			}
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				receiveInNewFactory.WD_HoldPalletIDPutaway = true;
				receiveInNewFactory.Validation.ValidateWD_HoldPalletIDPutaway();
			}
		}

		#endregion

		#region TestCheckWD_ExternalReference

		public void TestCheckWD_ExternalReference()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			AssertNotEquals("Precondition", ReceiveType.Codes.Returns, receive.WD_DocketSubType);
			AssertEquals("Precondition", string.Empty, receive.WD_ExternalReference);
			AssertNoWarnings(receive.WD_ExternalReferenceInfo);

			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.Validation.ValidateWD_ExternalReference();
			AssertHasWarning(receive.WD_ExternalReferenceInfo, "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_OrderReference()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "SomeRandomOrderNo";
			receive.Validation.ValidateWD_ExternalReference();
			AssertHasWarning(receive.WD_ExternalReferenceInfo, "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");

			receive.WD_ExternalReference = "Order123";
			receive.Validation.ValidateWD_ExternalReference();
			AssertNoWarnings(receive.WD_ExternalReferenceInfo);

			receive.WD_ExternalReference = order.WD_DocketID;
			receive.Validation.ValidateWD_ExternalReference();
			AssertHasWarning(receive.WD_ExternalReferenceInfo, "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_MultipleReturnReceivesOnSameOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive1.WD_ExternalReference = "Order123";
			returnReceive1.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(returnReceive1.WD_ExternalReferenceInfo);
			Factory.Save();

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive2.WD_ExternalReference = "Order123";
			returnReceive2.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(returnReceive2.WD_ExternalReferenceInfo);
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_OrderReference_UnfinalizedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "Order123";
			receive.Validation.ValidateWD_ExternalReference();
			AssertHasWarning(receive.WD_ExternalReferenceInfo, "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_AutoCreatedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.IsAutoCreatingReceive = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "SomeRandomOrderNo";
			receive.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(receive.WD_ExternalReferenceInfo);
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_ReceiveInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "Order123";
			receive.Validation.ValidateWD_ExternalReference();
			AssertNoWarnings(receive.WD_ExternalReferenceInfo);
			Factory.Save();

			Assert("Precondition", receive.IsInDatabase);
			receive.WD_ExternalReference = "SomeRandomOrderNo";
			receive.Validation.ValidateWD_ExternalReference();
			AssertHasWarning(receive.WD_ExternalReferenceInfo, "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_FullyReturnedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part1, 10m);
			returnReceive1.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(returnReceive1.WD_ExternalReferenceInfo);

			returnReceive1.WD_WD_ParentDocket = order.PK;
			Factory.Save();

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive2.Validation.ValidateWD_ExternalReference();
			AssertHasError(returnReceive2.WD_ExternalReferenceInfo, "This reference points to a departed order that has already been fully returned or is in the process of being fully returned.");
		}

		public void TestCheckWD_ExternalReference_ReturnReceive_FullyReturnedOrder_ReceiveLinkedToOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_ExternalReference = "Order123";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part1, 10m);
			returnReceive1.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(returnReceive1.WD_ExternalReferenceInfo);

			returnReceive1.WD_WD_ParentDocket = order.PK;
			Factory.Save();

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive2.WD_WD_ParentDocket = order.PK;
			returnReceive2.Validation.ValidateWD_ExternalReference();
			AssertNoErrors(returnReceive2.WD_ExternalReferenceInfo);
		}

		#endregion

		#region TestFinaliseDocket_UNDG

		public void TestFinaliseDocket_UNDG_UnderLimit()
		{
			TestFinaliseDocket_UNDG_Core(10m, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_OverLimit()
		{
			TestFinaliseDocket_UNDG_Core(11m, shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_Core(decimal quantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit3);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, quantity, location, "Pallet-1");
			receive.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, receive.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", receive.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, receive.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_ChangeProduct

		public void TestFinaliseDocket_UNDG_ChangeProduct()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			warehouse.UNDGLimits.Add(undgLimit);

			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 11, location, "Pallet-1");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.WD_DocketStatusInfo.HasErrors());
			AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", receive.WD_DocketStatusInfo.GetErrors().Single().Message);

			line.WE_OP = data.Part2.PK;
			receive.FinaliseDocketWithoutUserConfirmation();

			// Assert
			AssertEquals(false, receive.WD_DocketStatusInfo.HasErrors());
		}

		#endregion

		#region TestFinaliseDocket_UNDG_UQ

		public void TestFinaliseDocket_UNDG_WeightUQ_UnderLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitWeightUQ: "T", shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_WeightUQ_OverLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitWeightUQ: "G", shouldHaveError: true);
		}

		public void TestFinaliseDocket_UNDG_VolumetUQ_UnderLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitVolumeUQ: "ML", shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_VolumeUQ_OverLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitVolumeUQ: "L", shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_UQCore(string undgLimitWeightUQ = "KG", string undgLimitVolumeUQ = "M3", bool shouldHaveError = false)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 1, totalWeightLimitUQ: undgLimitWeightUQ, totalVolumeLimit: 2, totalVolumeLimitUQ: undgLimitVolumeUQ);
			warehouse.UNDGLimits.Add(undgLimit);

			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1, location, "Pallet-1");
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_F3_NKPackType = "XXX";
			receive.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, receive.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", receive.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, receive.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_DBHits

		public void TestFinaliseDocket_UNDG_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 50;

			var location = data.Whs1.Rows[0].Locations[0];
			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			location.WLV_WLT_LocationType = normalLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			for (var i = 1; i < 10; i++)
			{
				CreateDG(i);
			}

			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
				{ WhsClientParameterByWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsUNDGLimitSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertNoErrors(receiveInNewFactory.WD_DocketStatusInfo);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				receiveInNewFactory.FinaliseDocketWithoutUserConfirmation();

				AssertHasError(receiveInNewFactory.WD_DocketStatusInfo, @$"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'S006a'
Substance Code 'S007a'
Substance Code 'S008a'
Substance Code 'S009a'
Country Reference 'AU6'
Country Reference 'AU7'
Country Reference 'AU8'
Country Reference 'AU9'
Class Code '6'
Class Code '7'
Class Code '8'
Class Code '9'
");
			}

			void CreateDG(int i)
			{
				var undgUNNOCode = $"S00{i}";
				var undgVariant = "a";
				var undgCode = $"{undgUNNOCode}{undgVariant}";
				var undgStandard = "IMO";
				var undgClass = $"{i}.1D";
				var client = data.Org1;

				var substance = Helper.CreateUNDGSubstance(undgUNNOCode, undgClass, undgCode);
				var product = Helper.CreateProduct(client, $"PRD{i}");
				var dgItem = product.UNDGs.AddNew();
				dgItem.DI_DG = substance.PK;
				dgItem.DI_DGWeight = 1m;
				dgItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
				dgItem.DI_DGVolume = 1m;
				dgItem.DI_UnitOfVolume = Constants.Volume.CubicMetres;

				Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 100m, totalVolumeLimit: 100m);

				var reference = Helper.CreateCountryReference(referenceCode: $"AU{i}");
				Helper.CreateUNDGCountryReferencePivot(reference.PK, undgUNNOCode, undgVariant, undgStandard);
				var undgLimitCountryReference = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 100m, totalVolumeLimit: 100m);
				warehouse.UNDGLimits.Add(undgLimitCountryReference);

				var undgLimitClass = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, $"{i}", null, totalWeightLimit: 100m, totalVolumeLimit: 100m);
				warehouse.UNDGLimits.Add(undgLimitClass);

				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
				Helper.CreateWhsReceiveLine(receive, product, 4 * i, location: location);
			}
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> ValidSubTypeForBondedWarehouse => new[] { ReceiveType.Codes.Customs };
		protected override IEnumerable<string> ValidSubTypeForInwardProcessing => new[] { ReceiveType.Codes.Customs };

		#endregion
	}
}
