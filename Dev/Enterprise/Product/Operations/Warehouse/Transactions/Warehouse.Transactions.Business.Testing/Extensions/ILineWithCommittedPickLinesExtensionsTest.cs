using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ILineWithCommittedPickLinesExtensionsTest : ILineWithInventoryExtensionsTest
	{
		#region TestGetQtyCommittedToThisLine

		public void TestGetQtyCommittedToThisLine()
		{
			var dummyLine = GetNewDummy();
			AssertEquals(0m, dummyLine.GetQtyCommittedToThisLine());
			dummyLine.PickLines.AddNew().WZ_Units = 5m;
			AssertEquals("Qty Committed should sum Picklines attached to the line.", 5m,
				dummyLine.GetQtyCommittedToThisLine());
			dummyLine.PickLines.AddNew().WZ_Units = 7m;
			AssertEquals("Qty Committed should sum Picklines attached to the line.", 12m,
				dummyLine.GetQtyCommittedToThisLine());
			TestGetQtyCommittedToThisLineCore();
		}

		protected virtual void TestGetQtyCommittedToThisLineCore()
		{
		}

		#endregion

		#region TestCheckEnoughInventoryExistsToCommit

		public void TestCheckEnoughInventoryExistsToCommit()
		{
			var dummyLine = GetNewDummy();
			dummyLine.TransactionQty = 10m;
			dummyLine.IsFinalising = false;
			using (dummyLine.SuspendValidationTesting())
			{
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
				dummyLine.IsFinalising = true;
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
				// need product & client to be able to validate Quantity
				dummyLine.Product = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
				dummyLine.ParentDocket = Factory.New<WhsTransfer>();
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
				dummyLine.ParentDocket.WD_OH_Client = Factory.New<OrgHeader>().PK;
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertHasError(dummyLine.Z0_DescriptionInfo,
					@"Attempted to dummify 10 Units, but no Units are available for dummy out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the dummy line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to dummify.
If you are trying to dummify stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to dummify stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
				var pickLine = dummyLine.PickLines.AddNew();
				pickLine.WZ_Units = 10m;
				dummyLine.Z0_DescriptionInfo.ClearAllNotifications();
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
				pickLine.WZ_Units = 6m;
				dummyLine.Z0_DescriptionInfo.ClearAllNotifications();
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertHasError(dummyLine.Z0_DescriptionInfo,
					@"Attempted to dummify 10 Units, but only 6 Units are available for dummy out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the dummy line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to dummify.
If you are trying to dummify stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to dummify stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
				// if ShouldValidateQty is false and we are not in Pre-save Validation, then we should not be able to Validate Qty
				dummyLine.IsFinalising = false;
				dummyLine.Z0_DescriptionInfo.ClearAllNotifications();
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
				dummyLine.Z0_DescriptionInfo.ClearAllNotifications();
				dummyLine.Z0_DescriptionInfo.AdditionalValidation += delegate
				{
					dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				};
				dummyLine.RunPreSaveValidation();
				AssertHasError("During Pre-save Validation, the Qty Validation should be done.",
					dummyLine.Z0_DescriptionInfo,
					@"Attempted to dummify 10 Units, but only 6 Units are available for dummy out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the dummy line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to dummify.
If you are trying to dummify stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to dummify stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
			}

			TestCheckEnoughInventoryExistsToCommitCore();
		}

		protected virtual void TestCheckEnoughInventoryExistsToCommitCore()
		{
		}

		#endregion

		#region TestDecreaseStockInLocation

		public void TestDecreaseStockInLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			var dummyLine = GetNewDummy();
			dummyLine.ParentDocket = Factory.New<WhsTransfer>();
			dummyLine.ParentDocket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			dummyLine.Product = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			dummyLine.TransactionQty = 20m;
			dummyLine.Z0_DescriptionInfo.AdditionalValidation += delegate
			{
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
			};
			var pickLine1 = dummyLine.PickLines.AddNew();
			var pickLine2 = dummyLine.PickLines.AddNew();
			pickLine1.WZ_WE_InventoryLine = inventory1.InDocketLine.PK;
			pickLine2.WZ_WE_InventoryLine = inventory2.InDocketLine.PK;
			pickLine1.WZ_Units = 10m;
			pickLine2.WZ_Units = 10m;
			AssertExceptionThrown<InvalidOperationException>(
				"Should not decrease Total Units on Inventory if not during Finalisation.",
				() => dummyLine.DecreaseStockInLocation(dummyLine.Z0_DescriptionInfo));
			dummyLine.IsFinalising = true;
			using (dummyLine.SuspendValidationTesting())
			{
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
			}

			dummyLine.DecreaseStockInLocation(dummyLine.Z0_DescriptionInfo);
			AssertEquals("Inventory with enough stock should have been reduced.", 2m,
				inventory1.InDocketLine.WE_StockOnHand);
			AssertEquals("Inventory with not enough stock should not have been reduced.", 5m,
				inventory2.InDocketLine.WE_StockOnHand);
			AssertEquals("Pick Line units should have been reduced for the Validation Error to show.", 5m,
				pickLine2.WZ_Units);
			AssertHasError(dummyLine.Z0_DescriptionInfo,
				@"Attempted to dummify 20 Units, but only 15 Units are available for dummy out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the dummy line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to dummify.
If you are trying to dummify stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to dummify stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
		}

		#endregion

		#region TestReloadChangedInventoryOnRollback

		public void TestReloadChangedInventoryOnRollback()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();
			var dummyLine = GetNewDummy();
			var pickLine = dummyLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.InDocketLine.PK;
			pickLine.WZ_Units = 5m;
			dummyLine.IsFinalising = true;
			using (dummyLine.SuspendValidationTesting())
			{
				dummyLine.DecreaseStockInLocation(dummyLine.Z0_DescriptionInfo);
			}

			AssertEquals("Precondition: Stock is reduced.", 5m, inventory.WI_TotalUnits);
			AssertEquals("Precondition: PickLine is picked.", true, pickLine.IsPicked);
			dummyLine.ReloadChangedInventoryOnRollback();
			AssertEquals("Should have reloaded Inventory.", 10m, inventory.WI_TotalUnits);
			AssertEquals("PickLine should no longer be picked.", false, pickLine.IsPicked);
		}

		#endregion

		#region TestReloadChangedInventoryOnRollback_DoesNotOccurForAlreadyPickedInventory

		public void TestReloadChangedInventoryOnRollback_DoesNotOccurForAlreadyPickedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();
			var dummyLine = GetNewDummy();
			var pickLine = dummyLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.InDocketLine.PK;
			pickLine.WZ_Units = 6m;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			// this is just to set a DocketLine FK to allow the PickLine to save.
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, inventory.Location, inventory.Location);
			pickLine.WZ_WE_TransactionLine = transferLine.PK;
			Factory.Save();
			AssertEquals("Precondition: Stock is reduced.", 4m, inventory.WI_TotalUnits);
			int hitCount = 0;
			inventory.InDocketLine.Reloaded += (sender, e) => hitCount++;
			dummyLine.ReloadChangedInventoryOnRollback();
			AssertEquals("Should not reload Inventory that is already Finalised.", 0, hitCount);
		}

		#endregion

		#region Implementation

		protected new DummyLineWithPickLines GetNewDummy()
		{
			return (DummyLineWithPickLines)base.GetNewDummy();
		}

		protected override Type GetDummyType()
		{
			return typeof(DummyLineWithPickLines);
		}

		protected class DummyLineWithPickLines : DummyLineWithInventory, ILineWithCommittedPickLines,
			ICommittedInventoryStrategy
		{
			public DummyLineWithPickLines(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ICommittedInventoryStrategy CommittedStrategy
			{
				get
				{
					return this;
				}
			}

			public ZGuid ParentDocketPK { get; set; }

			public WhsLocation LocationToCommit { get; set; }

			public WhsPickLineCollection PickLines
			{
				get
				{
					return pickLines ?? (pickLines = new WhsPickLineCollection(Factory));
				}
			}

			WhsPickLineCollection pickLines;
			public ZShort LineNo { get; set; }

			public ZString PackageGroupID { get; set; }

			public ZString PalletIDToCommit { get; set; }

			public bool IsInTransit { get; }

			public string Noun
			{
				get
				{
					return "dummy";
				}
			}

			public string Verb
			{
				get
				{
					return "dummify";
				}
			}

			public ZDecimal PerPackageQty { get; set; }

			public ZDecimal TransactionQty
			{
				get
				{
					return Z0_Decimal;
				}

				set
				{
					Z0_Decimal = value;
				}
			}

			protected override bool EnableLightValidationIfAvailable
			{
				get
				{
					return false;
				}
			}

			public ZDecimal TotalTransactionQty
			{
				get
				{
					return GetTotalTransactionQty();
				}
			}

			protected virtual ZDecimal GetTotalTransactionQty()
			{
				return TransactionQty;
			}

			public ZDecimal TotalQtyCommitted
			{
				get
				{
					return GetTotalQtyCommitted();
				}
			}

			protected virtual ZDecimal GetTotalQtyCommitted()
			{
				return this.GetQtyCommittedToThisLine();
			}
		}

		#endregion
	}
}
