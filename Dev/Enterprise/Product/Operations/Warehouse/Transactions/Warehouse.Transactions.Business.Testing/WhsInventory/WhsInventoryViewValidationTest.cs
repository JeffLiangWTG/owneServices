using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsInventoryViewValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckVeryOldDates

		#region TestCheckWI_ArrivalDateIsValidZDateTimeRange

		[TestDate(2015, 1, 1)]
		public void TestCheckWI_ArrivalDateIsValidZDateTimeRange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var arrivalDate = ZDateTimeOffset.Now.AddYears(-11);
			var docket = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", arrivalDate, data.Part1, 10m, finalise: false);
			var inventory = docket.Inventory[0];

			AssertNoErrors(inventory.WI_ArrivalDateInfo);
		}

		#endregion

		#region TestCheckReceiveDateErrorsOnVeryOldDate

		public void TestCheckReceiveDateErrorsOnVeryOldDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var docket = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
				var inventory = docket.Inventory[0];

				CheckDateErrorsForOldDate(inventory.WI_ExpiryDateInfo, shouldHaveError: true);
				CheckDateErrorsForOldDate(inventory.WI_PackingDateInfo, shouldHaveError: true);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckReceiveDateErrorsOnVeryOldDate_FinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = docket.Inventory[0];

			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, docket.IsFinalised);
			CheckDateErrorsForOldDate(inventory.WI_ExpiryDateInfo, shouldHaveError: false);
			CheckDateErrorsForOldDate(inventory.WI_PackingDateInfo, shouldHaveError: false);
		}

		public void TestCheckReceiveDateErrorsOnVeryOldDate_ReceivedInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "A");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			AssertEquals(InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			CheckDateErrorsForOldDate(inventory.WI_ExpiryDateInfo, shouldHaveError: false);
			CheckDateErrorsForOldDate(inventory.WI_PackingDateInfo, shouldHaveError: false);
		}

		public void TestCheckReceiveDateErrorsOnVeryOldDate_HasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "A");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR4");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation, "A", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertEquals(true, receiveLine.HasPutawayTransfer);
			CheckDateErrorsForOldDate(inventory.WI_ExpiryDateInfo, shouldHaveError: false);
			CheckDateErrorsForOldDate(inventory.WI_PackingDateInfo, shouldHaveError: false);
		}

		#endregion

		#region TestCheckAdjustmentInDateErrorsOnVeryOldDate

		public void TestCheckAdjustmentInDateErrorsOnVeryOldDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, "A");
				adjustment.FinaliseDocketWithoutUserConfirmation();
				var inventory = adjustmentLine.Inventory[0];

				CheckDateErrorsForOldDate(inventory.WI_ExpiryDateInfo, shouldHaveError: false);
				CheckDateErrorsForOldDate(inventory.WI_PackingDateInfo, shouldHaveError: false);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckTransferDateErrorsOnVeryOldDate

		public void TestCheckTransferDateErrorsOnVeryOldDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5, "A-1", "A-2");
				transfer.FinaliseDocketWithoutUserConfirmation();
				var inventory = transferLine.Inventory[0];

				CheckDateErrorsForOldDate(inventory.WI_ExpiryDateInfo, shouldHaveError: false);
				CheckDateErrorsForOldDate(inventory.WI_PackingDateInfo, shouldHaveError: false);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region CheckDateErrorsForOldDate

		void CheckDateErrorsForOldDate(ZPropertyInfo propertyInfo, bool shouldHaveError)
		{
			AssertNoErrors(propertyInfo);
			propertyInfo.Value = new ZDate(1917, 11, 7);
			if (shouldHaveError)
			{
				AssertHasError(propertyInfo, $"The date '07-Nov-1917' is more than {ValidationLimits.PastYearsBeforeError} years old and thus is not valid.");
			}
			else
			{
				AssertNoErrors(propertyInfo);
				AssertHasWarning(propertyInfo, "The date '07-Nov-1917' is more than 1 year old.");
			}
		}

		#endregion

		#endregion

		#region TestCheckWI_SplitQuantity

		public void TestCheckWI_SplitQuantity()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 0, false, false);
				var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
				inventory.WI_InDocketLineUnits = 20m;
				inventory.WI_SplitQuantity = 10m;
				AssertNoErrors(inventory.WI_SplitQuantityInfo);

				inventory.WI_SplitQuantity = -1m;
				AssertHasError(inventory.WI_SplitQuantityInfo, "Split quantity cannot be negative.");

				inventory.WI_SplitQuantity = 0m;
				AssertNoErrors(inventory.WI_SplitQuantityInfo);

				inventory.WI_SplitQuantity = 21m;
				AssertHasError(inventory.WI_SplitQuantityInfo, "Split quantity cannot be greater than the Line Quantity.");
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP

		#region TestCheckWI_OP_CheckForInvalidProduct_IsNotActive

		public void TestCheckWI_OP_CheckForInvalidProduct_IsNotActive()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				OrgSupplierPart part = Factory.New<OrgSupplierPart>();
				part.OP_IsActive = false;
				Inventory.WI_OP = part.PK;
				AssertHasError(Inventory.WI_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);

				Inventory.WI_OP = ZGuid.Empty;
				part.OP_IsActive = true;
				Inventory.WI_OP = part.PK;
				AssertNoError(Inventory.WI_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_CheckForInvalidProduct_IsInvalid

		public void TestCheckWI_OP_CheckForInvalidProduct_IsInvalid()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				OrgSupplierPart part = Factory.New<OrgSupplierPart>();
				Inventory.WI_OP = part.PK;
				Inventory.SupplierPart.OP_PartNum = "TEST";
				AssertNoError(Inventory.WI_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);

				part.OP_PartNum = ProductType.Codes.Invalid;
				Inventory.WI_OP = ZGuid.Empty;
				Inventory.WI_OP = part.PK;
				AssertHasError(Inventory.WI_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_ValidationProductWarningMessage

		public void TestCheckWI_OP_ValidationProductWarningMessage()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				Inventory.ValidationProductWarningMessage = "XXX";
				AssertHasWarning(Inventory.WI_OPInfo, "XXX");

				Inventory.ValidationProductWarningMessage = "";
				AssertNoWarnings(Inventory.WI_OPInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_CheckProductHasWeightDefinition

		public void TestCheckWI_OP_CheckProductHasWeightDefinition()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var part = Factory.New<OrgSupplierPart>();
				Inventory.WI_OP = part.PK;
				AssertEquals(true, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.ProductHasNoWeightDefinitionError));

				part.OP_Weight = 10m;
				Inventory.WI_OP = ZGuid.Empty;
				Inventory.WI_OP = part.PK;
				AssertEquals(false, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.ProductHasNoWeightDefinitionError));
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_CheckProductHasCubicDefinition

		public void TestCheckWI_OP_CheckProductHasCubicDefinition()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var part = Factory.New<OrgSupplierPart>();
				Inventory.WI_OP = part.PK;
				AssertEquals(true, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.ProductHasNoVolumeDefinitionError));

				part.OP_Cubic = 10m;
				Inventory.WI_OP = ZGuid.Empty;
				Inventory.WI_OP = part.PK;
				AssertEquals(false, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.ProductHasNoVolumeDefinitionError));
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_ProductHasNoPalletDefinitionError

		public void TestCheckWI_OP_ProductHasNoPalletDefinitionError()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var part = Factory.New<OrgSupplierPart>();
				Inventory.WI_OP = part.PK;
				AssertEquals(true, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.ProductHasNoPalletDefinitionError));

				Helper.CreateProductUnit(part, "UNT", "PLT", 10m);
				Inventory.WI_OP = ZGuid.Empty;
				Inventory.WI_OP = part.PK;
				AssertEquals(false, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.ProductHasNoPalletDefinitionError));
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_ShouldNotBeChangedIfThisInventoryIsReserved

		public void TestCheckWI_OP_ShouldNotBeChangedIfThisInventoryIsReserved()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
				var inventory = receive.Inventory[0];
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_OPInfo);

				inventory.WI_OP = data.Part2.PK;
				AssertHasError(inventory.WI_OPInfo, WhsValidationHelper.CannotChangeCrossDocketInventoryProduct);

				inventory.WI_OP = data.Part1.PK;
				AssertNoErrors(inventory.WI_OPInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_OP_TempProductWarning

		public void TestCheckWI_OP_TempProductWarning()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "P1";
				Inventory.WI_OP = part.PK;
				AssertEquals(false, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.NoPartFoundWithThisCode));

				Inventory.WI_OP_PartNum = "NewProduct";
				Inventory.WI_OP = ZGuid.Invalid;
				AssertEquals(true, Inventory.WI_OPInfo.HasWarning(WhsValidationHelper.NoPartFoundWithThisCode));
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestChekcWI_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed

		public void TestChekcWI_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var client = Helper.CreateClient("CLIENT");
				var whs = Helper.CreateWarehouse("WHS");
				Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(client, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // should set use Expiry Date.

				var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", null, 0m, false, false);
				var inventory = receive.Inventory[0];

				AssertNoError("Precondition", inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Normal Attribute - NO ERROR
				var partWithNormalAttribute = Helper.CreateProduct(client, "P1");
				Helper.SetProductAttributeUse(client, partWithNormalAttribute, AttributeNumber.One, true);
				inventory.WI_OP = partWithNormalAttribute.PK;
				AssertNoError(inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute but without Product-Client-Warehouse parameter - ERROR
				var partWithJulianAttributeButWithoutClientWhsParam = Helper.CreateProduct(client, "P2");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeButWithoutClientWhsParam, AttributeNumber.Two, true); // should set use Expiry Date.
				inventory.WI_OP = partWithJulianAttributeButWithoutClientWhsParam.PK;
				AssertHasError(inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute but with wrong client Product-Client-Warehouse parameter - ERROR
				var partWithJulianAttributeAndClientWhsParam_InvalidClient = Helper.CreateProduct(client, "P3");
				var incorrectClient = Helper.CreateClient("CLIENT2");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidClient, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidClient, incorrectClient, whs);
				inventory.WI_OP = partWithJulianAttributeAndClientWhsParam_InvalidClient.PK;
				AssertHasError(inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute but with wrong warehouse Product-Client-Warehouse parameter - ERROR
				var partWithJulianAttributeAndClientWhsParam_InvalidWhs = Helper.CreateProduct(client, "P4");
				var incorrectWhs = Helper.CreateWarehouse("WH2");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidWhs, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidWhs, client, incorrectWhs);
				inventory.WI_OP = partWithJulianAttributeAndClientWhsParam_InvalidWhs.PK;
				AssertHasError(inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life = 0 - ERROR
				var partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife = Helper.CreateProduct(client, "P5");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, client, whs);
				inventory.WI_OP = partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife.PK;
				AssertHasError(inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life > 0 - NO ERROR
				var partWithJulianAttributeAndClientWhsParam_Correct = Helper.CreateProduct(client, "P6");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_Correct, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_Correct, client, whs).W3_MaximumShelfLife = 5;
				inventory.WI_OP = partWithJulianAttributeAndClientWhsParam_Correct.PK;
				AssertNoError(inventory.WI_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#endregion

		#region TestCheckWI_InDocketLineUnits

		#region TestCheckWI_InDocketLineUnits

		public void TestCheckWI_InDocketLineUnits()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				Inventory.WI_InDocketLineUnits = -1m;
				AssertHasError(Inventory.WI_InDocketLineUnitsInfo, "Quantity must be greater than or equal to zero. Otherwise delete this row");

				Inventory.WI_InDocketLineUnits = 0m;
				Inventory.WI_ExpectedReceiptQuantity = 1m;
				Inventory.Validation.ValidateWI_InDocketLineUnits();
				AssertNoErrors(Inventory.WI_InDocketLineUnitsInfo);

				SetupRelatedObjects();
				AssertNoWarnings(Inventory.WI_InDocketLineUnitsInfo);

				Inventory.WI_ExpectedReceiptQuantity = 0m;
				AssertNoErrors(Inventory.WI_InDocketLineUnitsInfo);

				Assert("incomplete test", true);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_InDocketLineUnits_ShouldNotBeSetLowerThanReservedAmount

		public void TestCheckWI_InDocketLineUnitsShouldNotBeSetLowerThanReservedAmount()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, finalise: false);
				var inventory = receive.Inventory[0];
				inventory.WI_InDocketLineUnits = 10m;
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_InDocketLineUnitsInfo);

				inventory.WI_InDocketLineUnits = 9m;
				AssertHasError(inventory.WI_InDocketLineUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

				inventory.WI_InDocketLineUnits = 10m;
				AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_InDocketLineUnitsLessThanReservedAmount_ExpectedReceiptQuantityAsReservedAmount

		public void TestCheckWI_InDocketLineUnitsLessThanReservedAmount_ExpectedReceiptQuantityAsReservedAmount()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
				var inventory = receive.Inventory[0];
				inventory.WI_ExpectedReceiptQuantity = 10m;
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_InDocketLineUnitsInfo);
				AssertEquals("WI_ExpectedReceiptQuantity is equal to reserved quantity.", inventory.WI_ExpectedReceiptQuantity, orderLine.WE_CrossDockQuantity);

				inventory.WI_InDocketLineUnits = 9m;
				AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_InDocketLineUnitsLessThanReservedAmount_ErrorClearedWhenExpectedReceiptQuantityBecomeReservedQuantity

		public void TestCheckWI_InDocketLineUnitsLessThanReservedAmount_ErrorClearedWhenExpectedReceiptQuantityBecomeReservedQuantity()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, finalise: false);
				var inventory = receive.Inventory[0];
				inventory.WI_InDocketLineUnits = 10m;
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_InDocketLineUnitsInfo);
				AssertEquals("WI_InDocketLineUnits is equal to reserved quantity.", inventory.WI_InDocketLineUnits, orderLine.WE_CrossDockQuantity);

				inventory.WI_InDocketLineUnits = 9m;
				AssertHasError(inventory.WI_InDocketLineUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

				inventory.WI_ExpectedReceiptQuantity = 10m;
				inventory.WI_InDocketLineUnits = 9m;
				AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_InDocketLineUnits_ShouldNotBeSetLowerThanReservedAmount_OnAdjustment

		public void TestCheckWI_InDocketLineUnits_ShouldNotBeSetLowerThanReservedAmount_OnAdjustment()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
				adjustment.FinaliseDocketWithoutUserConfirmation();
				var inventory = adjustmentLine.Inventory[0];
				AssertIsFinalisedPrecondition(adjustment);

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition - Divot was created.", orderLine.ReserveStockIfAbleTo(inventory));
				AssertEquals("Precondition - Inventory from Adjustment should have 0 InDocketLineUnits.", 0m, inventory.WI_InDocketLineUnits);
				AssertNoErrors("Precondition", inventory.WI_InDocketLineUnitsInfo);

				Factory.Save();
				inventory.Validation.ValidateWI_InDocketLineUnits();
				AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_InDocketLineUnits_WithSerialNumberProduct

		public void TestCheckWI_InDocketLineUnits_WithSerialNumberProduct()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
				AssertNoError("Precondition:", inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				inventory.WI_InDocketLineUnits = 2m;
				AssertNoError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				inventory.WI_InDocketLineUnits = 1m;
				AssertNoError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inventory.WI_InDocketLineUnits = 2m;
					AssertNoError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					inventory.WI_SerialNumber = "SN01";
					inventory.WI_InDocketLineUnits = 2m;
					AssertHasError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					inventory.WI_InDocketLineUnits = 1m;
					AssertNoError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						inventory.WI_InDocketLineUnits = 2m;
						AssertNoError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
					}

					Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
					inventory.WI_InDocketLineUnits = 2m;
					AssertNoError(inventory.WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#endregion

		#region TestCheckWI_F3_NKPackType

		public void TestCheckWI_F3_NKPackType()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var expectedWarningMessage = "The Pack Type {0} has not been defined for this Product.\r\nPack Types are defined on the Maintain -> Warehouse -> Products -> Details -> Unit Conversions Tab.\r\nBecause this Pack Type is not defined, a conversion to Units is not possible.";
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				inventory.WI_F3_NKPackType = "";
				AssertMandatoryValidationError(inventory.WI_F3_NKPackTypeInfo, true);

				inventory.WI_F3_NKPackType = "UNT";
				AssertMandatoryValidationError(inventory.WI_F3_NKPackTypeInfo, false);

				inventory.WI_F3_NKPackType = "BAG";
				AssertEquals(false, inventory.Product.IsPackTypeUsedByProduct("BAG"));
				AssertHasWarning("Not defined PackType warning should exist", inventory.WI_F3_NKPackTypeInfo, string.Format(expectedWarningMessage, inventory.WI_F3_NKPackType));

				Helper.CreateProductUnit(data.Part1, "BAG", "UNT", 0.1m);
				inventory.Validation.ValidateWI_F3_NKPackType();
				AssertEquals(true, inventory.Product.IsPackTypeUsedByProduct("BAG"));
				AssertNoWarning("No warning related to define should exist", inventory.WI_F3_NKPackTypeInfo, string.Format(expectedWarningMessage, inventory.WI_F3_NKPackType));
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckUniqueSerialNumber_LineWithNoQuantity

		public void TestCheckUniqueSerialNumber_LineWithNoQuantity_BeforeReceiveStartedReceiving()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Factory.Save();

				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
				var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventoryLine1.WI_SerialNumber = "SERIAL123";
				inventoryLine2.WI_SerialNumber = "SERIAL123";
				Factory.Save();

				AssertEquals("Precondition", false, receive.StartedReceiving);
				AssertEquals("Precondition", 1m, inventoryLine1.WI_InDocketLineUnits);
				AssertEquals("Precondition", 1m, inventoryLine1.WI_ExpectedReceiptQuantity);
				AssertEquals("Precondition", 1m, inventoryLine2.WI_InDocketLineUnits);
				AssertEquals("Precondition", 1m, inventoryLine2.WI_ExpectedReceiptQuantity);

				AssertNoError(inventoryLine1.WI_SerialNumberInfo, "Serial # already used.");
				AssertHasError("SERIAL123 is already used in inventoryLine1.", inventoryLine2.WI_SerialNumberInfo, "Serial # already used.");

				inventoryLine2.WI_InDocketLineUnits = 0m;
				Factory.Save();
				AssertHasError("Serial number duplicate error is not cleared as inventoryLine2 still has expected quantity.", inventoryLine2.WI_SerialNumberInfo, "Serial # already used.");

				inventoryLine2.WI_ExpectedReceiptQuantity = 0m;
				Factory.Save();
				AssertNoError("Serial number duplicate error is cleared as inventoryLine2 has no more transaction and expected quantities.", inventoryLine2.WI_SerialNumberInfo, "Serial # already used.");
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckUniqueSerialNumber_LineWithNoQuantity_AfterReceiveStartedReceiving()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Factory.Save();

				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
				var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
				receiveLine1.WI_SerialNumber = "SERIAL123";
				receiveLine2.WI_SerialNumber = "SERIAL123";
				Factory.Save();
				receive.PopulateASNLines();

				AssertEquals("Precondition", true, receive.StartedReceiving);
				AssertEquals("Precondition", 1m, receiveLine1.WI_InDocketLineUnits);
				AssertEquals("Precondition", 1m, receiveLine1.WI_ExpectedReceiptQuantity);
				AssertEquals("Precondition", 1m, receiveLine2.WI_InDocketLineUnits);
				AssertEquals("Precondition", 1m, receiveLine2.WI_ExpectedReceiptQuantity);

				AssertNoError(receiveLine1.WI_SerialNumberInfo, "Serial # already used.");
				AssertHasError("SERIAL123 is already used in receiveLine1.", receiveLine2.WI_SerialNumberInfo, "Serial # already used.");

				receiveLine2.WI_InDocketLineUnits = 0m;
				Factory.Save();
				AssertNoError("Serial number duplicate error is cleared as receiveLine2 has no more transaction quantity.", receiveLine2.WI_SerialNumberInfo, "Serial # already used.");
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestDoesNotValidateUnneccessarily

		public void TestDoesNotValidateUnneccessarily()
		{
			// When WhsInventory was removed, the Generator lost default values for this view (was getting them by side effect due to shared column names)
			// This triggered validation we don't want/need to be added automatically (neccessary defaulting/validation is handled by WhsDocketLine)
			var newInventory = GetNewInventory();
			newInventory.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertNoErrors("WI_BondedEntryKeyInfo", newInventory.WI_BondedEntryKeyInfo);
				AssertNoErrors("WI_InDocketLineTypeInfo", newInventory.WI_InDocketLineTypeInfo);
				AssertNoErrors("WI_InDocketLineUnitsInfo", newInventory.WI_InDocketLineUnitsInfo);
				AssertNoErrors("WI_InventoryStatusInfo", newInventory.WI_InventoryStatusInfo);
				AssertNoErrors("WI_IsOriginalReceiptLineInfo", newInventory.WI_IsOriginalReceiptLineInfo);
				AssertNoErrors("WI_PalletIDInfo", newInventory.WI_PalletIDInfo);
				AssertNoErrors("WI_PartAttrib1Info", newInventory.WI_PartAttrib1Info);
				AssertNoErrors("WI_PartAttrib2Info", newInventory.WI_PartAttrib2Info);
				AssertNoErrors("WI_PartAttrib3Info", newInventory.WI_PartAttrib3Info);
				AssertNoErrors(nameof(WhsInventoryView.WI_SerialNumberInfo), newInventory.WI_SerialNumberInfo);
				AssertNoErrors("WI_TotalUnitsInfo", newInventory.WI_TotalUnitsInfo);

				if (!WhsEnvironment.IsWebTracker)
				{
					// Pack type has mandatory validation for tracker? See functional code in CheckWI_F3_NKPackType
					AssertNoErrors("WI_F3_NKPackTypeInfo", newInventory.WI_F3_NKPackTypeInfo);
				}
			});
		}

		#endregion

		#region CheckPartAttributes

		#region TestCheckWI_ExpiryDate

		public void TestCheckWI_ExpiryDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				SetupRelatedObjects();

				// AttributeCallChecker -- ensure no validation is called when setting the field
				using (new PartAttributeValidationChecker.AttributeCallChecker())
				{
					Inventory.WI_ExpiryDate = ZDate.Today;
				}

				// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
				Inventory.WI_TotalUnits = 0m;
				using (new PartAttributeValidationChecker.AttributeCallChecker(false, Inventory.Client, Inventory.SupplierPart, Inventory.WI_ExpiryDateInfo, -1))
				using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
				{
					Inventory.WI_ExpiryDate = ZDate.Today.AddDays(1);
				}

				Inventory.WI_TotalUnits = Inventory.WI_InDocketLineUnits; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

				// AttributeCallChecker - validation called and passed
				using (new PartAttributeValidationChecker.AttributeCallChecker(Inventory.Client, Inventory.SupplierPart, Inventory.WI_ExpiryDateInfo, -1))
				using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
				{
					Inventory.WI_ExpiryDate = ZDate.Today.AddDays(2);
				}

				// AttributeCallChecker - validation called and passed
				using (new PartAttributeValidationChecker.AttributeCallChecker(Inventory.Client, Inventory.SupplierPart, Inventory.WI_ExpiryDateInfo, -1))
				using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
				{
					Inventory.WI_ExpiryDate = ZDate.Today.AddYears(22);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_ExpiryDate_ExpiryNotificationPeriod()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				// set up data
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				inventory.LocationWhsGuid = data.Whs1.PK;

				var whsProductParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
				whsProductParam.W3_ExpiryNotificationPeriod = 60;

				inventory.WI_ExpiryDate = ZDate.Empty; // empty date
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertNoWarnings(inventory.WI_ExpiryDateInfo);

				inventory.WI_ExpiryDate = ZDate.Today.AddDays(61); //	expiry date >= expiry notification period
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertNoWarnings(inventory.WI_ExpiryDateInfo);

				inventory.WI_ExpiryDate = ZDate.Today.AddDays(59); //	expiry date < expiry notification period, product is not using expiry date
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertNoWarnings(inventory.WI_ExpiryDateInfo);

				var productRelationshipList = data.Part1.RelatedOrganisations; //	change product to use expiry date
				data.Org1.MiscServ.OM_IMUseExpiryDate = true;
				var partRelation = productRelationshipList.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
				partRelation.OU_UseExpiryDate = true;

				var message = @"The Expiry Date allows for less days before expiry, than the nominated minimum shelf life requirement i.e. Expiry Notification Period (Days).";
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertHasWarning(inventory.WI_ExpiryDateInfo, message);

				inventory.WI_ExpiryDate = ZDate.Today.AddDays(61); //	expiry date >= expiry notification period, no warning
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertNoWarnings(inventory.WI_ExpiryDateInfo);

				inventory.WI_ExpiryDate = ZDate.Today.AddDays(60); //	expiry date = expiry notification period, has warning
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertHasWarnings(inventory.WI_ExpiryDateInfo);

				inventory.WI_ExpiryDate = ZDate.Today.AddDays(59); // expiry date < expiry notification period, has warning
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertHasWarning(inventory.WI_ExpiryDateInfo, message);

				receive.AllocateLocationsWithMock();
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore)) // finalising, then should have no warning
				{
					AssertEquals("Pre-condition: receive is finalising.", true, receive.IsFinalising);
					inventory.Validation.ValidateWI_ExpiryDate();
					AssertNoErrors(inventory.WI_ExpiryDateInfo);
					AssertNoWarnings(inventory.WI_ExpiryDateInfo);
				}

				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertHasWarning(inventory.WI_ExpiryDateInfo, message);

				receive.FinaliseDocketWithoutUserConfirmation(); // finalised, no warning
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);
				inventory.Validation.ValidateWI_ExpiryDate();
				AssertNoErrors(inventory.WI_ExpiryDateInfo);
				AssertNoWarnings(inventory.WI_ExpiryDateInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_PackingDate

		public void TestCheckWI_PackingDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				SetupRelatedObjects();
				// AttributeCallChecker -- ensure no validation is called when setting the field
				using (new PartAttributeValidationChecker.AttributeCallChecker())
				{
					Inventory.WI_PackingDate = ZDate.Today;
				}

				// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
				Inventory.WI_TotalUnits = 0m;
				using (new PartAttributeValidationChecker.AttributeCallChecker(false, Inventory.Client, Inventory.SupplierPart, Inventory.WI_PackingDateInfo, -2))
				using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
				{
					Inventory.WI_PackingDate = ZDate.Today.AddDays(1);
				}

				Inventory.WI_TotalUnits = Inventory.WI_InDocketLineUnits; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

				// AttributeCallChecker - validation called and passed
				using (new PartAttributeValidationChecker.AttributeCallChecker(Inventory.Client, Inventory.SupplierPart, Inventory.WI_PackingDateInfo, -2))
				using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
				{
					Inventory.WI_PackingDate = ZDate.Today.AddDays(2);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region Release Captured Validation

		#region TestCheckWI_PartAttrib1_ChecksIfAttribIsReleaseCaptured

		public void TestCheckWI_PartAttrib1_ChecksIfAttribIsReleaseCaptured()
		{
			AssertCheckIfAttribIsReleaseCaptured(AttributeNumber.One, WhsInventoryViewSchema.WI_PartAttrib1, inventory => inventory.WI_PartAttrib1Info);
		}

		#endregion

		#region TestCheckWI_PartAttrib2_ChecksIfAttribIsReleaseCaptured

		public void TestCheckWI_PartAttrib2_ChecksIfAttribIsReleaseCaptured()
		{
			AssertCheckIfAttribIsReleaseCaptured(AttributeNumber.Two, WhsInventoryViewSchema.WI_PartAttrib2, inventory => inventory.WI_PartAttrib2Info);
		}

		#endregion

		#region TestCheckWI_PartAttrib3_ChecksIfAttribIsReleaseCaptured

		public void TestCheckWI_PartAttrib3_ChecksIfAttribIsReleaseCaptured()
		{
			AssertCheckIfAttribIsReleaseCaptured(AttributeNumber.Three, WhsInventoryViewSchema.WI_PartAttrib3, inventory => inventory.WI_PartAttrib3Info);
		}

		#endregion

		void AssertCheckIfAttribIsReleaseCaptured(AttributeNumber attributeNumber, SchemaColumn attributeColumn, Func<WhsInventoryView, ZPropertyInfo> getPropertyInfo)
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.VIN);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
				receive.AllocateLocationsWithMock();

				var info = getPropertyInfo(inventory);
				inventory[attributeColumn] = "123";
				AssertNoErrors(info);

				Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);
				inventory.Validation.ValidateAll();
				AssertHasError(info, "This attribute is specified as Release Captured for this Product, no value should be entered.");

				inventory[attributeColumn] = "";
				AssertNoErrors(info);

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals(true, receive.IsFinalised);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestWI_PartAttrib1_MaxLength

		public void TestWI_PartAttrib1_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertNoExceptionThrown(() => inventory.WI_PartAttrib1 = "".PadLeft(WhsInventoryViewSchema.WI_PartAttrib1.MaxLength, 'A'));
		}

		#endregion

		#region TestWI_PartAttrib1_Exceed_MaxLength

		public void TestWI_PartAttrib1_Exceed_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					inventory.WI_PartAttrib1 = ZString.Replicate('A', WhsInventoryViewSchema.WI_PartAttrib1.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWI_PartAttrib2_MaxLength

		public void TestWI_PartAttrib2_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertNoExceptionThrown(() => inventory.WI_PartAttrib2 = "".PadLeft(WhsInventoryViewSchema.WI_PartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWI_PartAttrib2_Exceed_MaxLength

		public void TestWI_PartAttrib2_Exceed_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					inventory.WI_PartAttrib2 = ZString.Replicate('A', WhsInventoryViewSchema.WI_PartAttrib2.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWI_PartAttrib3_MaxLength

		public void TestWI_PartAttrib3_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertNoExceptionThrown(() => inventory.WI_PartAttrib1 = "".PadLeft(WhsInventoryViewSchema.WI_PartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestWI_PartAttrib3_Exceed_MaxLength

		public void TestWI_PartAttrib3_Exceed_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					inventory.WI_PartAttrib3 = ZString.Replicate('A', WhsInventoryViewSchema.WI_PartAttrib3.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestCheckWI_PartAttrib1

		public void TestCheckWI_PartAttrib1()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				Inventory.SetValidationPartAttribWarningMessage(1, "Hello");
				Inventory.WI_PartAttrib1 = "PA1";
				AssertHasWarning(Inventory.WI_PartAttrib1Info, "Hello");

				SetupRelatedObjects();
				// AttributeCallChecker -- ensure no validation is called when setting the field
				using (new PartAttributeValidationChecker.AttributeCallChecker())
				{
					Inventory.WI_PartAttrib1 = "1";
				}

				// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
				Inventory.WI_TotalUnits = 0m;
				using (new PartAttributeValidationChecker.AttributeCallChecker(false, Inventory.Client, Inventory.SupplierPart, Inventory.WI_PartAttrib1Info, 1))
				{
					using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
					{
						Inventory.WI_PartAttrib1 = "2";
					}
				}

				Inventory.WI_TotalUnits = Inventory.WI_InDocketLineUnits; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

				// AttributeCallChecker - validation called and passed
				using (new PartAttributeValidationChecker.AttributeCallChecker(Inventory.Client, Inventory.SupplierPart, Inventory.WI_PartAttrib1Info, 1))
				{
					using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
					{
						Inventory.WI_PartAttrib1 = "3";
					}
				}

				Inventory.WI_PartAttrib1 = "  PA1";
				AssertHasErrors(WhsValidationHelper.ValueHasToBeTrimmed, Inventory.WI_PartAttrib1Info);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_PartAttrib1_JulianBatchNumber()
		{
			TestCheckWI_PartAttrib_JulianBatchNumberCore(WhsInventoryViewSchema.WI_PartAttrib1, AttributeNumber.One, line => line.WI_PartAttrib1Info);
		}

		#endregion

		#region TestCheckWI_PartAttrib2

		public void TestCheckWI_PartAttrib2()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				Inventory.SetValidationPartAttribWarningMessage(2, "Hello");
				Inventory.WI_PartAttrib2 = "PA2";
				AssertHasWarning(Inventory.WI_PartAttrib2Info, "Hello");

				SetupRelatedObjects();
				// AttributeCallChecker -- ensure no validation is called when setting the field
				using (new PartAttributeValidationChecker.AttributeCallChecker())
				{
					Inventory.WI_PartAttrib2 = "1";
				}

				// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
				Inventory.WI_TotalUnits = 0m;
				using (new PartAttributeValidationChecker.AttributeCallChecker(false, Inventory.Client, Inventory.SupplierPart, Inventory.WI_PartAttrib2Info, 2))
				{
					using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
					{
						Inventory.WI_PartAttrib2 = "2";
					}
				}

				Inventory.WI_TotalUnits = Inventory.WI_InDocketLineUnits; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

				// AttributeCallChecker - validation called and passed
				using (new PartAttributeValidationChecker.AttributeCallChecker(Inventory.Client, Inventory.SupplierPart, Inventory.WI_PartAttrib2Info, 2))
				{
					using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
					{
						Inventory.WI_PartAttrib2 = "3";
					}
				}

				Inventory.WI_PartAttrib2 = "  PA2";
				AssertHasErrors(WhsValidationHelper.ValueHasToBeTrimmed, Inventory.WI_PartAttrib2Info);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_PartAttrib2_JulianBatchNumber()
		{
			TestCheckWI_PartAttrib_JulianBatchNumberCore(WhsInventoryViewSchema.WI_PartAttrib2, AttributeNumber.Two, line => line.WI_PartAttrib2Info);
		}

		#endregion

		#region TestCheckWI_PartAttrib3

		public void TestCheckWI_PartAttrib3()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				Inventory.SetValidationPartAttribWarningMessage(3, "Hello");
				Inventory.WI_PartAttrib3 = "PA3";
				AssertHasWarning(Inventory.WI_PartAttrib3Info, "Hello");

				SetupRelatedObjects();
				// AttributeCallChecker -- ensure no validation is called when setting the field
				using (new PartAttributeValidationChecker.AttributeCallChecker())
				{
					Inventory.WI_PartAttrib3 = "1";
				}

				// AttributeCallChecker - ensure no validation called when WI_TotalUnits is 0
				Inventory.WI_TotalUnits = 0m;
				using (new PartAttributeValidationChecker.AttributeCallChecker(false, Inventory.Client, Inventory.SupplierPart, Inventory.WI_PartAttrib3Info, 3))
				{
					using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
					{
						Inventory.WI_PartAttrib3 = "2";
					}
				}

				Inventory.WI_TotalUnits = Inventory.WI_InDocketLineUnits; // need to set Total Units manualy, because otherwise it will be set only during RunPreSaveValidation.

				// AttributeCallChecker - validation called and passed
				using (new PartAttributeValidationChecker.AttributeCallChecker(Inventory.Client, Inventory.SupplierPart, Inventory.WI_PartAttrib3Info, 3))
				{
					using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore))
					{
						Inventory.WI_PartAttrib3 = "3";
					}
				}

				Inventory.WI_PartAttrib3 = "  PA3";
				AssertHasErrors(WhsValidationHelper.ValueHasToBeTrimmed, Inventory.WI_PartAttrib3Info);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_PartAttrib3_JulianBatchNumber()
		{
			TestCheckWI_PartAttrib_JulianBatchNumberCore(WhsInventoryViewSchema.WI_PartAttrib3, AttributeNumber.Three, line => line.WI_PartAttrib3Info);
		}

		#endregion

		#region TestPartAttributesCheckForSerialNumberAgainstThisReceipt

		void AssertSerial(ZPropertyInfo inv1attribInfo, ZPropertyInfo inv2attribInfo, bool inventoriesHaveSameProduct, string attrib2Name)
		{
			ZString value1 = "SER1";
			ZString value2 = "SER2";

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO");
			inv1attribInfo.Value = value1;
			inv2attribInfo.Value = value2;
			AssertNoErrors("By Product - But differnt Serial, so should always have NO errors.", inv2attribInfo);

			inv2attribInfo.Value = value1;
			if (inventoriesHaveSameProduct)
			{
				AssertHasError("By Product - Same Serial - Same Products, so should have errors.", inv2attribInfo, string.Format("{0} already used.", attrib2Name));
			}
			else
			{
				AssertNoErrors("By Product - Same Serial - Different Products, so should have NO errors.", inv2attribInfo);
			}

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI");
			inv1attribInfo.Value = value1;
			inv2attribInfo.Value = value2;
			AssertNoErrors("By Client - But differnt Serial, so should always have NO errors.", inv2attribInfo);

			inv2attribInfo.Value = value1;
			AssertHasError("By Cient - Same Serial - Product doesn't matter, so should have errors.", inv2attribInfo, string.Format("{0} already used.", attrib2Name));
		}

		public void TestPartAttributesCheckForSerialNumberAgainstThisReceipt()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var inventoryA1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
				inventoryA1.LocationWhsGuid = data.Whs1.PK;
				var inventoryA2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
				var inventoryB = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);

				AssertSerial(inventoryA1.WI_SerialNumberInfo, inventoryA2.WI_SerialNumberInfo, true, "Serial #");
				AssertSerial(inventoryA1.WI_SerialNumberInfo, inventoryB.WI_SerialNumberInfo, false, "Serial #");

				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);

				WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI");
				inventoryA1.WI_SerialNumber = "SER1";
				inventoryB.WI_SerialNumber = "SER1";
				AssertNoError(inventoryB.WI_SerialNumberInfo, "Serial # already used.");
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_PartAttrib_JulianBatchNumberCore

		void TestCheckWI_PartAttrib_JulianBatchNumberCore(SchemaStringColumn partAttributeSchemaColumn, AttributeNumber attributeNumber, Func<WhsInventoryView, ZPropertyInfo> getPropertyInfo)
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
				data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
				inventory = receive.Inventory[0];
				var info = getPropertyInfo(inventory);

				using (new SemaphoreManager(Inventory.Docket.FinaliseDocketSemaphore)) // attribute validation is only during finalise.
				{
					AssertNoError("Precondition", info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

					inventory[partAttributeSchemaColumn] = "ABCD";
					AssertNoError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

					Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
					inventory[partAttributeSchemaColumn] = "ABCD";
					AssertHasError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

					inventory[partAttributeSchemaColumn] = "12345ABCD";
					AssertHasError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

					inventory[partAttributeSchemaColumn] = "ABCD12345";
					AssertNoError(info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheck_SerialNumber

		public void TestCheckWI_SerialNumber_Required()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var inv = receiveLine.Inventory[0];
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inv.WI_SerialNumber = "Ser";
					AssertNoErrors("Precondition: No errors for serial number existing", receiveLine.Inventory[0].WI_SerialNumberInfo);

					inv.WI_SerialNumber = "";
					AssertHasError("Errors for missing manadatory serial number", receiveLine.Inventory[0].WI_SerialNumberInfo, "Please enter a Serial Number.");

					inv.WI_SerialNumber = "FRD";
					AssertNoErrors("No errors for another serial number", receiveLine.Inventory[0].WI_SerialNumberInfo);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_SerialNumber_ReleaseCaptured()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var inv = receiveLine.Inventory[0];
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inv.WI_SerialNumber = "";
					AssertNoErrors("Precondition: No errors for no serial number", receiveLine.Inventory[0].WI_SerialNumberInfo);

					inv.WI_SerialNumber = "SER";
					AssertHasError("Errors for added value on release captured serial number", receiveLine.Inventory[0].WI_SerialNumberInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");

					inv.WI_SerialNumber = "";
					AssertNoErrors("No errors for cleared serial number", receiveLine.Inventory[0].WI_SerialNumberInfo);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_SerialNumber_TrimmedCorrectly()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var inv = receiveLine.Inventory[0];
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inv.WI_SerialNumber = "Ser";
					AssertNoErrors("Precondition: No errors for serial number", receiveLine.Inventory[0].WI_SerialNumberInfo);

					inv.WI_SerialNumber = "   Ser";
					AssertHasError("Errors for serial number untrimmed", receiveLine.Inventory[0].WI_SerialNumberInfo, WhsValidationHelper.ValueHasToBeTrimmed);

					inv.WI_SerialNumber = "RFT";
					AssertNoErrors("No errors for No serial number", receiveLine.Inventory[0].WI_SerialNumberInfo);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_InDocketLineUnits_SerialNumber_QtyGreaterThanOne()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var inv = receiveLine.Inventory[0];
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inv.WI_SerialNumber = "Ser";
					AssertNoErrors("Precondition: No errors for serial number and 1 Qty", receiveLine.Inventory[0].WI_InDocketLineUnitsInfo);

					inv.WI_InDocketLineUnits = 10m;
					AssertHasError("Errors for serial number and 10 Qty", receiveLine.Inventory[0].WI_InDocketLineUnitsInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					inv.WI_SerialNumber = "";
					AssertNoErrors("No errors for No serial number and 10 Qty", receiveLine.Inventory[0].WI_InDocketLineUnitsInfo);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_ExpectedReceiptQuantity_SerialNumber_QtyGreaterThanOne()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var inv = receiveLine.Inventory[0];
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inv.WI_SerialNumber = "Ser";
					AssertNoErrors("Precondition: No errors for serial number and 1 Qty", receiveLine.Inventory[0].WI_ExpectedReceiptQuantityInfo);

					inv.WI_ExpectedReceiptQuantity = 10m;
					AssertHasError("Errors for serial number and 10 Qty", receiveLine.Inventory[0].WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					inv.WI_SerialNumber = "";
					AssertNoErrors("No errors for No serial number and 10 Qty", receiveLine.Inventory[0].WI_ExpectedReceiptQuantityInfo);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		public void TestCheckWI_SerialNumber_ValidationSerialNumberWarningMessage()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var inv = receiveLine.Inventory[0];
				inv.ValidationSerialNumberWarningMessage = "Hello";
				inv.WI_SerialNumber = "SN1";
				AssertHasWarning(inv.WI_SerialNumberInfo, "Hello");
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#endregion

		#region CheckWI_ExpectedReceiptQuantity

		#region TestCheckWI_ExpectedReceiptQuantity_ShouldNotBeSetLowerThanReservedAmount

		public void TestCheckWI_ExpectedReceiptQuantity_ShouldNotBeSetLowerThanReservedAmount()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, finalise: false);
				var inventory = receive.Inventory[0];
				inventory.WI_ExpectedReceiptQuantity = 10m;
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_ExpectedReceiptQuantityInfo);

				inventory.WI_ExpectedReceiptQuantity = 9m;
				AssertHasError(inventory.WI_ExpectedReceiptQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

				inventory.WI_ExpectedReceiptQuantity = 10m;
				AssertNoErrors(inventory.WI_ExpectedReceiptQuantityInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_ExpectedReceiptQuantityLessThanReservedAmount_InDocketLineUnitsAsReservedAmount

		public void TestCheckWI_ExpectedReceiptQuantityLessThanReservedAmount_InDocketLineUnitsAsReservedAmount()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
				var inventory = receive.Inventory[0];
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_ExpectedReceiptQuantityInfo);
				AssertEquals("WI_InDocketLineUnits is equal to reserved quantity.", inventory.WI_InDocketLineUnits, orderLine.WE_CrossDockQuantity);

				inventory.WI_ExpectedReceiptQuantity = 9m;
				inventory.WI_InDocketLineUnits = 10m;
				AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_ExpectedReceiptQuantityLessThanReservedAmount_ErrorClearedWhenInDocketLineUnitsBecomeReservedQuantity

		public void TestCheckWI_ExpectedReceiptQuantityLessThanReservedAmount_ErrorClearedWhenInDocketLineUnitsBecomeReservedQuantity()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
				var inventory = receive.Inventory[0];
				inventory.WI_InDocketLineUnits = 5m;
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
				AssertNoErrors("Precondition", inventory.WI_ExpectedReceiptQuantityInfo);
				AssertEquals("WI_ExpectedReceiptQuantity is equal to reserved quantity.", inventory.WI_ExpectedReceiptQuantity, orderLine.WE_CrossDockQuantity);

				inventory.WI_ExpectedReceiptQuantity = 9m;
				inventory.WI_InDocketLineUnits = 5m;
				AssertHasError(inventory.WI_ExpectedReceiptQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

				inventory.WI_InDocketLineUnits = 10m;
				AssertNoErrors(inventory.WI_ExpectedReceiptQuantityInfo);
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#region TestCheckWI_ExpectedReceiptQuantity_WithSerialNumberProduct

		public void TestCheckWI_ExpectedReceiptQuantity_WithSerialNumberProduct()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
				AssertNoError("Precondition:", inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				inventory.WI_ExpectedReceiptQuantity = 2m;
				AssertNoError(inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				inventory.WI_ExpectedReceiptQuantity = 1m;
				AssertNoError(inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					inventory.WI_ExpectedReceiptQuantity = 2m;
					AssertNoError(inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					inventory.WI_SerialNumber = "SN01";
					inventory.WI_ExpectedReceiptQuantity = 2m;
					AssertHasError(inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					inventory.WI_ExpectedReceiptQuantity = 1m;
					AssertNoError(inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

					Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
					inventory.WI_ExpectedReceiptQuantity = 2m;
					AssertNoError(inventory.WI_ExpectedReceiptQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
				}
			}
			else
			{
				Assert("This validation should only be run for Web Tracker.", true);
			}
		}

		#endregion

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 0, false, false);
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			inventory.WI_InDocketLineUnits = 20m;
			inventory.WI_SplitQuantity = 10m;
			var validation = new TestWhsInventoryViewValidation(inventory);

			var list = new string[]
			{
				WhsInventoryViewSchema.Constants.WI_WL,
				WhsInventoryViewSchema.Constants.WI_WE_InDocketLine,
				WhsInventoryViewSchema.Constants.WI_WE_OriginalInDocketLineForRating,
				WhsInventoryViewSchema.Constants.WI_F3_NKPackType
			};

			foreach (var propertyInfo in inventory.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsInventoryViewValidation

		class TestWhsInventoryViewValidation : WhsInventoryViewValidation
		{
			public TestWhsInventoryViewValidation(WhsInventoryView parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		protected WhsInventoryView Inventory
		{
			get { return inventory ?? (inventory = GetNewInventory()); }
			set { inventory = value; }
		}

		protected virtual WhsInventoryView GetNewInventory()
		{
			return Factory.New<WhsInventoryView>();
		}

		protected virtual void SetupRelatedObjects()
		{
			Whs = Helper.CreateWarehouse("1");
			Row = Helper.CreateRowAndGenerateLocations(Whs, "A", 2, 2);

			Client = Helper.CreateClient("A");
			Part = Helper.CreateProduct(Client, "A");
			Docket = Helper.CreateWhsReceive(Client, Whs);

			Inventory = Helper.CreateWhsReceiveInventoryLine(Docket, Part, 10m);
			inventory.LocationWhsGuid = Whs.PK;
		}

		WhsInventoryView inventory;
		protected WhsWarehouse Whs;
		protected WhsRow Row;
		protected OrgHeader Client;
		protected OrgSupplierPart Part;
		protected WhsReceive Docket;

		#endregion
	}
}
