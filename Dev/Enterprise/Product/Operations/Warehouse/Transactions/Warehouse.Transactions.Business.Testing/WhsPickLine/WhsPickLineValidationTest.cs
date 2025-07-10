using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickLineValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckReservedQuantity

		public void TestCheckReservedQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 120m);
			var pickLine = Helper.CreateReservePickLine(orderLine, data.Line111, 100m);
			AssertNoErrors(pickLine.ReservedQuantityInfo);

			pickLine.ReservedQuantity = -1m;
			AssertHasError(pickLine.ReservedQuantityInfo, "Please enter a quantity greater than zero");

			pickLine.ReservedQuantity = 0m;
			AssertHasError(pickLine.ReservedQuantityInfo, "An unsaved Cross Dock allocation cannot have zero units.");

			pickLine.ReservedQuantity = 150m;
			AssertHasError(pickLine.ReservedQuantityInfo, "You have allocated more than the ordered quantity");

			pickLine.ReservedQuantity = 110m;
			AssertHasError(pickLine.ReservedQuantityInfo, "This quantity is not available");

			pickLine.ReservedQuantity = 100m;
			AssertNoErrors(pickLine.ReservedQuantityInfo);

			Factory.Save();
			pickLine.ReservedQuantity = 0m;
			AssertHasWarning(pickLine.ReservedQuantityInfo, "Attempting to Cross Dock zero units.");
		}

		#endregion

		#region TestCheckReservedQuantity_OnALargeCrossDockedOrderLine

		[SnailTest]
		public void TestCheckReservedQuantity_OnALargeCrossDockedOrderLine()
		{
			const int linesToCreate = 500; // 600 caused issue for client + JMB, on a debug 64-bit stack the problem starts occuring between 250-300 lines

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < linesToCreate; i++)
			{
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory.WI_SerialNumber = "SN" + i;
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition.", linesToCreate, receive.Lines.Count);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, new ZDecimal(linesToCreate));
			var orderLine = order.Lines[0];
			foreach (WhsInventoryView inventory in receive.Inventory)
			{
				orderLine.ReserveStockIfAbleTo(inventory, 1m);
			}

			AssertEquals("Precondition.", new ZDecimal(linesToCreate), orderLine.ReservedQuantity);
			AssertNoExceptionThrown(orderLine.RunPreSaveValidation);
		}

		#endregion

		#region TestCheckWZ_OriginalReservedQty

		public void TestCheckWZ_OriginalReservedQty()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNoErrors("Precondition:", pickLine.WZ_OriginalReservedQtyInfo);

			IBusinessObjectInternals iBizO = pickLine;
			iBizO.Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = -1m;
			pickLine.Validation.ValidateWZ_OriginalReservedQty();
			AssertHasError(pickLine.WZ_OriginalReservedQtyInfo, "Original Quantity cannot be negative.");

			iBizO.Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 0m;
			pickLine.Validation.ValidateWZ_OriginalReservedQty();
			AssertNoErrors(pickLine.WZ_OriginalReservedQtyInfo);

			pickLine.IsReserveLine = true;
			iBizO.Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = -1m;
			pickLine.Validation.ValidateWZ_OriginalReservedQty();
			AssertHasError(pickLine.WZ_OriginalReservedQtyInfo, "Original Quantity cannot be negative.");

			iBizO.Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 0m;
			pickLine.Validation.ValidateWZ_OriginalReservedQty();
			AssertHasError(pickLine.WZ_OriginalReservedQtyInfo, "Original Quantity cannot be zero.");

			iBizO.Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m;
			pickLine.Validation.ValidateWZ_OriginalReservedQty();
			AssertNoErrors(pickLine.WZ_OriginalReservedQtyInfo);
		}

		public void TestCheckWZ_OriginalReservedQty_ForPickedPickLine()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.IsReserveLine = true;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition - pickLine should be picked.", true, pickLine.IsPickedFromPutawayLocation);
			AssertEquals("Precondition - WZ_OriginalReservedQty should be zero.", 0m, pickLine.WZ_OriginalReservedQty);

			pickLine.Validation.ValidateWZ_OriginalReservedQty();
			AssertEquals("ValidateWZ_OriginalReservedQty should succeed for picked pickLine.", false, pickLine.HasErrors);
		}

		#endregion

		#region TestCheckWZ_Units

		public void TestCheckWZ_Units()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			pickLine.DocketLine.WE_TransactionQuantity = 10.2m;

			pickLine.WZ_Units = -1;
			AssertHasError(pickLine.WZ_UnitsInfo, "Pick line Units must be greater than or equal to zero.");

			pickLine.WZ_Units = 0;
			AssertNoErrors(pickLine.WZ_UnitsInfo);

			pickLine.WZ_Units = 10.3m;
			AssertHasError(pickLine.WZ_UnitsInfo, "Pick line Units must not exceed Quantity Ordered on the related Order Line record");

			pickLine.WZ_Units = 10.1m;
			AssertNoErrors(pickLine.WZ_UnitsInfo);
		}

		#endregion

		#region TestCheckWZ_WE_InventoryLine

		public void TestCheckWZ_WE_InventoryLine()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = Factory.New<WhsReceiveLine>().PK;
			AssertMandatoryValidationError(pickLine.WZ_WE_InventoryLineInfo, false);

			pickLine.WZ_WE_InventoryLine = ZGuid.Empty;
			AssertMandatoryValidationError(pickLine.WZ_WE_InventoryLineInfo, true);
		}

		#endregion

		#region TestCheckWZ_WE_TransactionLine

		public void TestCheckWZ_WE_TransactionLine()
		{
			var order = Factory.New<WhsOrder>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_TransactionLine = order.Lines.AddNew().PK;
			AssertMandatoryValidationError(pickLine.WZ_WE_TransactionLineInfo, false);

			pickLine.WZ_WE_TransactionLine = ZGuid.Empty;
			AssertMandatoryValidationError(pickLine.WZ_WE_TransactionLineInfo, true);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			reservedPickLine.Validation.ValidateAll();
			AssertNoRowErrors(reservedPickLine);

			using (reservedPickLine.GetValidationSuspender())
			{
				reservedPickLine.ReservedQuantity = 11m;
			}

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertNoErrors(reservedPickLine.ReservedQuantityInfo);
			AssertNoRowErrors("Precondition - Validation has not yet run.", reservedPickLine);

			var expectedErrorMessage = "Cross Docked Inventory is either damaged or has a mismatch on Warehouse, Client, Product, Part Attribute(s), Expiry Date, Packing Date or Ordered Pallet ID.";
			reservedPickLine.Validation.ValidateAll();
			reservedPickLine.Validation.ValidateAll();
			AssertHasError(reservedPickLine.ReservedQuantityInfo, "You have allocated more than the ordered quantity");
			AssertEquals("Should only have 1 row error", 1, reservedPickLine.RowErrors.Count(e => e.Message.Equals(expectedErrorMessage)));
			AssertHasRowError(reservedPickLine, expectedErrorMessage);

			reservedPickLine.AddRowError("TestError");

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "";
			reservedPickLine.Validation.ValidateAll();
			AssertNoRowError(reservedPickLine, expectedErrorMessage);
			AssertHasRowError("Should not clear Hack Error", reservedPickLine, "TestError");
		}

		#endregion

		#region TestValidateAll_UpdatePalletIDAfterReservationOnOrderLine

		public void TestValidateAll_UpdatePalletIDAfterReservationOnOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT1");
			var inventory = receive1.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PalletID = "PLT1";

			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);
			reservedPickLine.Validation.ValidateAll();
			AssertNoRowErrors(reservedPickLine);

			orderLine.WE_PalletID = "PLT3";
			var expectedErrorMessage = "Cross Docked Inventory is either damaged or has a mismatch on Warehouse, Client, Product, Part Attribute(s), Expiry Date, Packing Date or Ordered Pallet ID.";
			reservedPickLine.Validation.ValidateAll();
			AssertEquals("Should only have 1 row error", 1, reservedPickLine.RowErrors.Count(e => e.Message.Equals(expectedErrorMessage)));
			AssertHasRowError(reservedPickLine, expectedErrorMessage);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var pickLine = Factory.New<WhsPickLine>();
			var validation = new TestWhsPickLineValidation(pickLine);

			var list = new string[]
			{
				WhsPickLineSchema.Constants.WZ_WE_InventoryLine,
				WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine,
				WhsPickLineSchema.Constants.WZ_WE_OriginalPickedInventoryLine,
				WhsPickLineSchema.Constants.WZ_WE_TransactionLine,
				WhsPickLineSchema.Constants.WZ_F3_NKAllocatedPackType,
				WhsPickLineSchema.Constants.WZ_P9_Task,
			};

			foreach (var propertyInfo in pickLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
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

		#region TestWhsPickLineValidation

		class TestWhsPickLineValidation : WhsPickLineValidation
		{
			public TestWhsPickLineValidation(WhsPickLine parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
