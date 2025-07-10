using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ILineWithMatchingLinesExtensionsTest : ILineWithCommittedPickLinesExtensionsTest
	{
		#region TestCheckEnoughInventoryExistsToCommit

		protected override void TestCheckEnoughInventoryExistsToCommitCore()
		{
			base.TestCheckEnoughInventoryExistsToCommitCore();
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.TransactionQty = 10m;
			dummyLine.IsFinalising = true;
			// need product & client to be able to validate Quantity
			dummyLine.Product = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			dummyLine.ParentDocket = Factory.New<WhsTransfer>();
			dummyLine.ParentDocket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			var child = Factory.New<DummyLineWithMatchingLines>();
			child.TransactionQty = 4m;
			var pickLine = child.PickLines.AddNew();
			pickLine.WZ_Units = 3m;
			dummyLine.PickLines.AddNew().WZ_Units = 10m;
			dummyLine.MatchingLines = new[] { child };
			using (dummyLine.SuspendValidationTesting())
			{
				dummyLine.Z0_DescriptionInfo.ClearAllNotifications();
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertHasError(dummyLine.Z0_DescriptionInfo,
					@"Attempted to dummify 14 Units, but only 13 Units are available for dummy out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the dummy line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to dummify.
If you are trying to dummify stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to dummify stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
				pickLine.WZ_Units = 4m;
				dummyLine.Z0_DescriptionInfo.ClearAllNotifications();
				dummyLine.CheckEnoughInventoryExistsToCommit(dummyLine.Z0_DescriptionInfo);
				AssertNoErrors(dummyLine.Z0_DescriptionInfo);
			}
		}

		#endregion

		#region TestSplitIntoMatchingLine

		#region TestSplitIntoMatchingLine_CanOnlyBeDoneOnMainTransactionLine

		public void TestSplitIntoMatchingLine_CanOnlyBeDoneOnMainTransactionLine()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.TransactionQty = 10m;
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			var child = Factory.New<DummyLineWithMatchingLines>();
			child.TransactionQty = 4m;
			dummyLine.MatchingLines = new[] { child };
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Should only call CreateMatchingLine() on Main Transaction Line.",
				() => child.SplitIntoMatchingLine(1m));
			AssertNoExceptionThrown(() => dummyLine.SplitIntoMatchingLine(1m));
		}

		#endregion

		#region TestSplitIntoMatchingLine_DocketPKShouldBeValid

		public void TestSplitIntoMatchingLine_DocketPKShouldBeValid()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"ParentDocketPK should be a valid Guid.\r\nParameter name: ParentDocketPK",
				() => dummyLine.SplitIntoMatchingLine(1m));
		}

		#endregion

		#region TestSplitIntoMatchingLine_QuantityShouldNotBeNegative

		public void TestSplitIntoMatchingLine_QuantityShouldNotBeNegative()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"qtyForMatchingLine cannot be Negative.\r\nParameter name: qtyForMatchingLine",
				() => dummyLine.SplitIntoMatchingLine(-1m));
		}

		#endregion

		#region TestSplitIntoMatchingLine_QuantityShouldNotBeGreaterThanOrEqualToTransactionQty

		public void TestSplitIntoMatchingLine_QuantityShouldNotBeGreaterThanOrEqualToTransactionQty()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"qtyForMatchingLine must be less than the Transaction Qty.\r\nParameter name: qtyForMatchingLine",
				() => dummyLine.SplitIntoMatchingLine(10m));
			AssertExceptionThrown(typeof(ArgumentException),
				"qtyForMatchingLine must be less than the Transaction Qty.\r\nParameter name: qtyForMatchingLine",
				() => dummyLine.SplitIntoMatchingLine(11m));
		}

		#endregion

		#region TestSplitIntoMatchingLine_QuantityShouldNotBeZero

		public void TestSplitIntoMatchingLine_QuantityShouldNotBeZero()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"qtyForMatchingLine cannot be Zero.\r\nParameter name: qtyForMatchingLine",
				() => dummyLine.SplitIntoMatchingLine(0m));
		}

		#endregion

		#region TestSplitIntoMatchingLine_SetsCorrectData

		public void TestSplitIntoMatchingLine_SetsCorrectData()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			var docketPK = Factory.New<WhsTransfer>().PK;
			dummyLine.ParentDocketPK = docketPK;
			dummyLine.Z0_Code = "ABC";
			dummyLine.TransactionQty = 10m;
			var createMatchingLine = dummyLine.SplitIntoMatchingLine(2m);
			AssertEquals("New Matching Line should have been cloned.", "ABC", createMatchingLine.Z0_Code);
			AssertEquals("New Matching Line should have correct Transaction Qty.", 2m,
				createMatchingLine.TransactionQty);
			AssertEquals("Original Line should have correct Transaction Qty.", 8m, dummyLine.TransactionQty);
			AssertEquals("New Matching Line should have correct Docket PK.", docketPK,
				createMatchingLine.ParentDocketPK);
			AssertEquals("New Matching Line should point to Transaction Line that created it.", dummyLine.PK,
				createMatchingLine.MatchingLinePK);
		}

		#endregion

		#region TestSplitIntoMatchingLine_SetsPutawayData

		public void TestSplitIntoMatchingLine_SetsPutawayData()
		{
			try
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride =
					typeof(DummyLineWithMatchingLinesAndPutawayDetails);
				var dummyLine = Factory.New<DummyLineWithMatchingLinesAndPutawayDetails>();
				dummyLine.ParentDocketPK = ZGuid.NewZGuid();
				dummyLine.TransactionQty = 10m;
				dummyLine.PutawayBy = "BBB";
				var docketPK = Factory.New<WhsTransfer>().PK;
				var createMatchingLine = dummyLine.SplitIntoMatchingLine(2m);
				AssertEquals("New Matching Line should have correct Putaway By.", "BBB", createMatchingLine.PutawayBy);
			}
			finally
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
			}
		}

		#endregion

		#endregion

		#region TestCreateMatchingLineWithQuantity

		#region TestCreateMatchingLineWithQuantity_CanOnlyBeDoneOnMainTransactionLine

		public void TestCreateMatchingLineWithQuantity_CanOnlyBeDoneOnMainTransactionLine()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.TransactionQty = 10m;
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			var child = Factory.New<DummyLineWithMatchingLines>();
			child.TransactionQty = 4m;
			dummyLine.MatchingLines = new[] { child };
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Should only call CreateMatchingLine() on Main Transaction Line.",
				() => child.SplitIntoMatchingLine(1m));
			AssertNoExceptionThrown(() => dummyLine.CreateMatchingLineWithQuantity(1m));
		}

		#endregion

		#region TestCreateMatchingLineWithQuantity_DocketPKShouldBeValid

		public void TestCreateMatchingLineWithQuantity_DocketPKShouldBeValid()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"ParentDocketPK should be a valid Guid.\r\nParameter name: ParentDocketPK",
				() => dummyLine.SplitIntoMatchingLine(1m));
		}

		#endregion

		#region TestCreateMatchingLineWithQuantity_QuantityShouldNotBeNegative

		public void TestCreateMatchingLineWithQuantity_QuantityShouldNotBeNegative()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"qtyForMatchingLine cannot be Negative.\r\nParameter name: qtyForMatchingLine",
				() => dummyLine.CreateMatchingLineWithQuantity(-1m));
		}

		#endregion

		#region TestCreateMatchingLineWithQuantity_QuantityShouldNotBeZero

		public void TestCreateMatchingLineWithQuantity_QuantityShouldNotBeZero()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.ParentDocketPK = ZGuid.NewZGuid();
			dummyLine.TransactionQty = 10m;
			AssertExceptionThrown(typeof(ArgumentException),
				"qtyForMatchingLine cannot be Zero.\r\nParameter name: qtyForMatchingLine",
				() => dummyLine.CreateMatchingLineWithQuantity(0m));
		}

		#endregion

		#region TestCreateMatchingLineWithQuantity_SetsCorrectData

		public void TestCreateMatchingLineWithQuantity_SetsCorrectData()
		{
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			var docketPK = Factory.New<WhsTransfer>().PK;
			dummyLine.ParentDocketPK = docketPK;
			dummyLine.Z0_Code = "ABC";
			dummyLine.TransactionQty = 10m;
			var createMatchingLine = dummyLine.CreateMatchingLineWithQuantity(2m);
			AssertEquals("New Matching Line should have been cloned.", "ABC", createMatchingLine.Z0_Code);
			AssertEquals("New Matching Line should have correct Transaction Qty.", 2m,
				createMatchingLine.TransactionQty);
			AssertEquals("Original Line should not modify Transaction Qty.", 10m, dummyLine.TransactionQty);
			AssertEquals("New Matching Line should have correct Docket PK.", docketPK,
				createMatchingLine.ParentDocketPK);
			AssertEquals("New Matching Line should point to Transaction Line that created it.", dummyLine.PK,
				createMatchingLine.MatchingLinePK);
		}

		#endregion

		#region TestCreateMatchingLineWithQuantity_SetsPutawayData

		public void TestCreateMatchingLineWithQuantity_SetsPutawayData()
		{
			try
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride =
					typeof(DummyLineWithMatchingLinesAndPutawayDetails);
				var dummyLine = Factory.New<DummyLineWithMatchingLinesAndPutawayDetails>();
				dummyLine.ParentDocketPK = ZGuid.NewZGuid();
				dummyLine.TransactionQty = 10m;
				dummyLine.PutawayBy = "BBB";
				var docketPK = Factory.New<WhsTransfer>().PK;
				var createMatchingLine = dummyLine.CreateMatchingLineWithQuantity(2m);
				AssertEquals("New Matching Line should have correct Putaway By.", "BBB", createMatchingLine.PutawayBy);
			}
			finally
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
			}
		}

		#endregion

		#endregion

		#region TestGetQtyCommittedToThisLine

		protected override void TestGetQtyCommittedToThisLineCore()
		{
			base.TestGetQtyCommittedToThisLineCore();
			var dummyLine = (DummyLineWithMatchingLines)GetNewDummy();
			dummyLine.PickLines.AddNew().WZ_Units = 10m;
			var child = Factory.New<DummyLineWithMatchingLines>();
			child.PickLines.AddNew().WZ_Units = 2m;
			dummyLine.MatchingLines = new[] { child };
			AssertEquals("Qty Committed should only include Picklines attached to line.", 10m,
				dummyLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestGetQtyCommittedIncludingMatchingLines

		public void TestGetQtyCommittedIncludingMatchingLines()
		{
			var dummyLine = Factory.New<DummyLineWithMatchingLines>();
			AssertEquals(0m, dummyLine.GetQtyCommittedIncludingMatchingLines());
			dummyLine.PickLines.AddNew().WZ_Units = 5m;
			AssertEquals("Qty Committed including matching, should sum Picklines attached to the line.", 5m,
				dummyLine.GetQtyCommittedIncludingMatchingLines());
			var child1 = Factory.New<DummyLineWithMatchingLines>();
			child1.PickLines.AddNew().WZ_Units = 2m;
			dummyLine.MatchingLines = new[] { child1 };
			AssertEquals(
				"Qty Committed including matching, should sum Picklines attached to the line and matching lines.", 7m,
				dummyLine.GetQtyCommittedIncludingMatchingLines());
			var child2 = Factory.New<DummyLineWithMatchingLines>();
			child2.PickLines.AddNew().WZ_Units = 4m;
			dummyLine.MatchingLines = new[] { child1, child2 };
			AssertEquals(
				"Qty Committed including matching, should sum Picklines attached to the line and matching lines.", 11m,
				dummyLine.GetQtyCommittedIncludingMatchingLines());
			AssertExceptionThrown<InvalidOperationException>(
				"Should only call GetQtyIncludingMatchingLines() on Main Transaction Line.",
				() => child1.GetQtyCommittedIncludingMatchingLines());
		}

		#endregion

		#region TestGetQtyIncludingMatchingLines

		public void TestGetQtyIncludingMatchingLines()
		{
			var dummyLine = Factory.New<DummyLineWithMatchingLines>();
			dummyLine.PackQty = 2m;
			var child = Factory.New<DummyLineWithMatchingLines>();
			child.PackQty = 3m;
			dummyLine.MatchingLines = new[] { child };
			AssertEquals("Should sum Qty on Main Line and children.", 5m,
				dummyLine.GetQtyIncludingMatchingLines(l => l.PackQty));
			AssertExceptionThrown<InvalidOperationException>(
				"Should only call GetQtyIncludingMatchingLines() on Main Transaction Line.",
				() => child.GetQtyIncludingMatchingLines(l => l.PackQty));
		}

		#endregion

		#region TestGetTransactionQtyIncludingMatchingLines

		public void TestGetTransactionQtyIncludingMatchingLines()
		{
			var dummyLine = Factory.New<DummyLineWithMatchingLines>();
			AssertEquals(0m, dummyLine.GetTransactionQtyIncludingMatchingLines());
			dummyLine.TransactionQty = 5m;
			AssertEquals("Transaction Qty including matching, should include the qty on the line.", 5m,
				dummyLine.GetTransactionQtyIncludingMatchingLines());
			var child1 = Factory.New<DummyLineWithMatchingLines>();
			child1.TransactionQty = 7m;
			dummyLine.MatchingLines = new[] { child1 };
			AssertEquals("Transaction Qty including matching, should include the qty on the line and matching lines.",
				12m, dummyLine.GetTransactionQtyIncludingMatchingLines());
			var child2 = Factory.New<DummyLineWithMatchingLines>();
			child2.TransactionQty = 4m;
			dummyLine.MatchingLines = new[] { child1, child2 };
			AssertEquals("Transaction Qty including matching, should include the qty on the line and matching lines.",
				16m, dummyLine.GetTransactionQtyIncludingMatchingLines());
			AssertExceptionThrown<InvalidOperationException>(
				"Should only call GetQtyIncludingMatchingLines() on Main Transaction Line.",
				() => child1.GetTransactionQtyIncludingMatchingLines());
		}

		#endregion

		#region TestIsMainTransactionLine

		public void TestIsMainTransactionLine()
		{
			var dummyLine = Factory.New<DummyLineWithMatchingLines>();
			dummyLine.MatchingLinePK = ZGuid.NewZGuid();
			AssertEquals(false, dummyLine.IsMainTransactionLine());
			dummyLine.MatchingLinePK = ZGuid.Empty;
			AssertEquals(true, dummyLine.IsMainTransactionLine());
		}

		#endregion

		#region Implementation

		protected override Type GetDummyType()
		{
			return typeof(DummyLineWithMatchingLines);
		}

		class DummyLineWithMatchingLinesAndPutawayDetails :
			DummyLineWithMatchingLinesBase<DummyLineWithMatchingLinesAndPutawayDetails>, ILineWithPickAndPutawayDetails
		{
			public DummyLineWithMatchingLinesAndPutawayDetails(BusinessObjectFactory factory, DataRow row) : base(
				factory, row)
			{
			}

			public ZString PickedBy { get; set; }

			public ZString PutawayBy { get; set; }
		}

		class DummyLineWithMatchingLines : DummyLineWithMatchingLinesBase<DummyLineWithMatchingLines>
		{
			public DummyLineWithMatchingLines(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		abstract class DummyLineWithMatchingLinesBase<T> : DummyLineWithPickLines, ILineWithMatchingLines<T>
			where T : DummyLineWithMatchingLinesBase<T>
		{
			public DummyLineWithMatchingLinesBase(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZDecimal PackQty
			{
				get
				{
					return Z0_AnotherDecimal;
				}

				set
				{
					Z0_AnotherDecimal = value;
				}
			}

			public override void Delete()
			{
				if (!MatchingLinePK.IsEmpty)
				{
					var mainLine = Factory.Load<T>(MatchingLinePK);
					mainLine.matchingLines = mainLine.matchingLines.Where(l => l != this).ToArray();
				}

				base.Delete();
			}

			public IEnumerable<T> MatchingLines
			{
				get
				{
					return matchingLines ?? (matchingLines = Enumerable.Empty<T>());
				}

				set
				{
					matchingLines = value;
					foreach (var line in MatchingLines)
					{
						line.MatchingLinePK = PK;
					}
				}
			}

			IEnumerable<T> matchingLines;
			public ZGuid MatchingLinePK { get; set; }

			protected override ZDecimal GetTotalTransactionQty()
			{
				return ((T)this).GetTransactionQtyIncludingMatchingLines();
			}

			protected override ZDecimal GetTotalQtyCommitted()
			{
				return ((T)this).GetQtyCommittedIncludingMatchingLines();
			}
		}

		#endregion
	}
}
