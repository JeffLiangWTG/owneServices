using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ILineWithInventoryExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestAddErrorsFromCreatedInventory

		public void TestAddErrorsFromCreatedInventory()
		{
			var dummyLine = GetNewDummy();
			dummyLine.Inventory.AddNew();
			var inventory = dummyLine.Inventory.AddNew();
			inventory.AddRowError("Test Error 1");
			inventory.AddRowError("Test Error 1");
			inventory.AddRowError("Test Error 2");
			dummyLine.AddErrorsFromCreatedInventory();
			AssertHasRowError(dummyLine, @"Could not update Inventory:
Error - WhsInventoryView: Test Error 1" + "\n" + @"Error - WhsInventoryView: Test Error 2
Try deleting this line and re-adding it.");
		}

		#endregion

		#region TestIncreaseStockInLocation

		public void TestIncreaseStockInLocation()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();
			var dummyLine = GetNewDummy();
			dummyLine.PartAttrib1 = "PA1";
			dummyLine.PackType = "CTN";
			dummyLine.ParentDocket = receive;
			dummyLine.Product = WhsProduct.GetWhsProduct(data.Part1);
			dummyLine.ProductPK = data.Part1.PK;
			dummyLine.TransactionInventoryStatus = "HEL";
			dummyLine.TransactionInventoryHeldCode = "DAM";
			dummyLine.TransactionLocation = inventory.WI_WL;
			dummyLine.TransactionPalletID = "PLT-123";
			AssertExceptionThrown<InvalidOperationException>(
				"Should not try to create Inventory with a negative amount.",
				() => dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, -1m));
			AssertExceptionThrown<InvalidOperationException>(
				"Should not try to create Inventory if there is no Arrival Date set.",
				() => dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m));
			dummyLine.ArrivalDate = new ZDateTimeOffset(year, 1, 1);
			AssertExceptionThrown<InvalidOperationException>("Should not create Inventory if not during Finalisation.",
				() => dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m));
			dummyLine.IsFinalising = true;
			dummyLine.Inventory.AddNew();
			AssertExceptionThrown<InvalidOperationException>(
				"Should not try to create Inventory if it already exists for this line.",
				() => dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m));
			dummyLine.Inventory.RemoveAndDeleteAll();
			dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m);
			var createdInventory = (WhsInventoryView)dummyLine.Inventory.Single();
			AssertEquals("createdInventory.WI_InDocketLineType", "INW", createdInventory.WI_InDocketLineType);
			AssertEquals("createdInventory.WI_WE_InDocketLine", dummyLine.PK, createdInventory.WI_WE_InDocketLine);
			AssertEquals("createdInventory.WI_WD", receive.PK, createdInventory.WI_WD);
			AssertEquals("createdInventory.WI_OH_Client", data.Org1.PK, createdInventory.WI_OH_Client);
			AssertEquals("createdInventory.WI_WW_Whs", data.Whs1.PK, createdInventory.WI_WW_Whs);
			AssertEquals("createdInventory.WI_OP", data.Part1.PK, createdInventory.WI_OP);
			AssertEquals("createdInventory.WI_ArrivalDate", new ZDateTimeOffset(year, 1, 1), createdInventory.WI_ArrivalDate);
			AssertEquals("createdInventory.WI_F3_NKPackType", "CTN", createdInventory.WI_F3_NKPackType);
			AssertEquals("createdInventory.WI_TotalUnits", 5m, createdInventory.WI_TotalUnits);
			AssertEquals("createdInventory.WI_InDocketLineUnits", 0m, createdInventory.WI_InDocketLineUnits);
			AssertEquals("createdInventory.WI_WL", data.Whs1.DefaultLocation.PK, createdInventory.WI_WL);
			AssertEquals("createdInventory.WI_PalletID", "PLT-123", createdInventory.WI_PalletID);
			AssertEquals("createdInventory.WI_WE_OriginalInDocketLineForRating", inventory.InDocketLine.PK,
				createdInventory.WI_WE_OriginalInDocketLineForRating);
			AssertEquals("createdInventory.WI_InventoryStatus", "HEL", createdInventory.WI_InventoryStatus);
			AssertEquals("createdInventory.WI_PartAttrib1", "PA1", createdInventory.WI_PartAttrib1);
			AssertEquals("createdInventory.WI_HeldCode", "DAM", createdInventory.WI_HeldCode);
		}

		#endregion

		#region TestIncreaseStockInLocation_WhenPackTypeIsEmpty

		public void TestIncreaseStockInLocation_WhenPackTypeIsEmpty()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "PLT";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();
			var dummyLine = GetNewDummy();
			dummyLine.ArrivalDate = new ZDateTimeOffset(year, 1, 1);
			dummyLine.ParentDocket = receive;
			dummyLine.Product = WhsProduct.GetWhsProduct(data.Part1);
			dummyLine.ProductPK = data.Part1.PK;
			dummyLine.TransactionInventoryStatus = "HEL";
			dummyLine.TransactionLocation = inventory.WI_WL;
			dummyLine.TransactionPalletID = "PLT-123";
			dummyLine.IsFinalising = true;
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Should not create Inventory if PackType is not set.",
				() => dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m));
		}

		#endregion

		#region TestIncreaseStockInLocation_WithDifferentInventoryStatus

		public void TestIncreaseStockInLocation_WithDifferentInventoryStatus()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();
			var dummyLine = GetNewDummy();
			dummyLine.PackType = "CTN";
			dummyLine.ParentDocket = receive;
			dummyLine.Product = WhsProduct.GetWhsProduct(data.Part1);
			dummyLine.ProductPK = data.Part1.PK;
			dummyLine.TransactionInventoryStatus = "AVL";
			dummyLine.TransactionLocation = inventory.WI_WL;
			dummyLine.ArrivalDate = new ZDateTimeOffset(year, 1, 1);
			dummyLine.IsFinalising = true;
			// Inventory Status passed in should have been set on the created inventory, and the Transaction Inventory Status should remain the same
			dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m, "HEL");
			var createdInventory = (WhsInventoryView)dummyLine.Inventory.Single();
			AssertEquals("createdInventory.WI_InventoryStatus", "HEL", createdInventory.WI_InventoryStatus);
			AssertEquals("dummyLine.TransactionInventoryStatus", "AVL", dummyLine.TransactionInventoryStatus);
		}

		#endregion

		#region TestIncreaseStockInLocation_WithDifferentInventoryStatus

		public void TestIncreaseStockInLocation_WithDifferentReceiveUQ()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Part1.PartUnits.GetUnitConversion(Constants.PkgUnit.Carton, Constants.PkgUnit.Unit).OF_QuantityInParent = 10m;
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1).W3_F3_NKReceivedPackType =
				Constants.PkgUnit.Carton;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			AssertEquals("Precondition", Constants.PkgUnit.Carton, receive.Lines.Single().WE_F3_NKPackType);
			Factory.Save();
			var dummyLine = GetNewDummy();
			dummyLine.PackType = Constants.PkgUnit.Unit;
			dummyLine.ParentDocket = receive;
			dummyLine.Product = WhsProduct.GetWhsProduct(data.Part1);
			dummyLine.ProductPK = data.Part1.PK;
			dummyLine.TransactionInventoryStatus = "AVL";
			dummyLine.TransactionLocation = inventory.WI_WL;
			dummyLine.ArrivalDate = new ZDateTimeOffset(year, 1, 1);
			dummyLine.IsFinalising = true;
			// It should keep the same pack type as the pack type on the line instead of getting the pack type from warehouse product params
			dummyLine.IncreaseStockInLocation(inventory.WI_WE_InDocketLine, 5m, InventoryStatus.Codes.Available);
			var createdInventory = (WhsInventoryView)dummyLine.Inventory.Single();
			AssertEquals("createdInventory.WI_F3_NKPackType", Constants.PkgUnit.Unit,
				createdInventory.WI_F3_NKPackType);
		}

		#endregion

		#region TestIncreaseStockInLocation_DoesNotModifyTransactionQty

		public void TestIncreaseStockInLocation_DoesNotModifyTransactionQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 1560m);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 0m, "A-1", ZDateTimeOffset.Today);
			adjustmentLine.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			adjustmentLine.WE_TransactionQuantity = 864m;
			AssertEquals("Precondition", Constants.PkgUnit.Pallet, adjustmentLine.WE_F3_NKPackType);
			AssertEquals("Precondition", 864m, adjustmentLine.WE_TransactionQuantity);
			using (new SemaphoreManager(adjustment.FinaliseDocketSemaphore))
			{
				adjustmentLine.IncreaseStockInLocation(adjustmentLine.PK, 864m);
				var createdInventory = (WhsInventoryView)adjustmentLine.Inventory.Single();
				AssertEquals("Transaction line's Qty should not have been modified.", 864m,
					adjustmentLine.WE_TransactionQuantity);
				AssertEquals("Inventory should be created for the full amount requested.", 864m,
					createdInventory.WI_TotalUnits);
			}
		}

		#endregion

		#region Implementation

		protected DummyLineWithInventory GetNewDummy()
		{
			return (DummyLineWithInventory)Factory.New(GetDummyType());
		}

		protected virtual Type GetDummyType()
		{
			return typeof(DummyLineWithInventory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = GetDummyType();
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		protected class DummyLineWithInventory : DummyBusinessObject, ILineWithInventory
		{
			public DummyLineWithInventory(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public WhsDocket ParentDocket
			{
				get
				{
					return parentDocket;
				}

				set
				{
					if (ParentDocket != null)
					{
						ParentDocket.UnRegisterEditableChildObject(this);
					}

					parentDocket = value;
					if (ParentDocket != null)
					{
						ParentDocket.RegisterEditableChildObject(this);
					}
				}
			}

			WhsDocket parentDocket;
			public WhsProduct Product { get; set; }

			public ZGuid ProductPK { get; set; }

			public WhsInventoryViewCollection Inventory
			{
				get
				{
					return inventory ?? (inventory = new WhsInventoryViewCollection(Factory));
				}
			}

			WhsInventoryViewCollection inventory;
			public ZDateTimeOffset ArrivalDate { get; set; }

			public ZString PackType { get; set; }

			public ZString TransactionInventoryStatus { get; set; }

			public ZString TransactionInventoryHeldCode { get; set; }

			public ZGuid TransactionLocation { get; set; }

			public ZString TransactionPalletID { get; set; }

			public bool IsFinalising { get; set; }

			public ZString BondedEntryKey { get; set; }

			public ZDate ExpiryDate { get; set; }

			public ZDate PackingDate { get; set; }

			public ZString PartAttrib1 { get; set; }

			public ZString PartAttrib2 { get; set; }

			public ZString PartAttrib3 { get; set; }

			public ZString SerialNumber { get; set; }

			public ZString AllocationKey { get; set; }

			public bool CanCreateInventory
			{
				get
				{
					return IsFinalising;
				}
			}

			public void SetAttributes(ILineAttributes src)
			{
				throw new NotImplementedException();
			}

			public ReservedPickLineCollection ReservedPickLines =>
				pickLines ?? (pickLines = new ReservedPickLineCollection(Inventory[0]));

			ReservedPickLineCollection pickLines;
			public ZDecimal ReservedQuantity => ReservedPickLines.GetQtyCommitted();
		}

		#endregion
	}
}
