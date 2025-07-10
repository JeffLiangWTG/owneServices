using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class TestConcurrencyProblemsForAdjustment : TestCase
	{
		#region TestExceptionThrown_IfFinalisedInAnotherFactory_AndCriticalFieldsChangedInFactory

		[UseSnapshotProtection]
		public void TestExceptionThrown_IfFinalisedInAnotherFactory_AndCriticalFieldsChangedInFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, inventory.Location);
			adjustment.RunPreSaveValidation(); // to commit inventory and create pickline
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
				var adjustmentInSecondFactory = secondFactory.Load<WhsAdjustment>(adjustment.PK);
				adjustmentInSecondFactory.FinaliseDocketWithoutUserConfirmation();
				secondFactory.Save();

				adjustmentLine.WE_TransactionQuantity = 10;
				AssertExceptionThrown("Since the adjustmentLine value has changed, exception should be thrown.",
							typeof(ZSaveConcurrencyException), () => Factory.Save());
			}
		}

		#endregion

		#region TestExceptionThrown_IfFinalisedInAnotherFactory_AndCriticalFieldsChangedOnAdjustmentInFactory

		[UseSnapshotProtection]
		public void TestExceptionThrown_IfFinalisedInAnotherFactory_AndCriticalFieldsChangedOnAdjustmentInFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, inventory.Location);
			adjustment.RunPreSaveValidation(); // to commit inventory and create pickline
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
				var adjustmentInSecondFactory = secondFactory.Load<WhsAdjustment>(adjustment.PK);
				adjustmentInSecondFactory.FinaliseDocketWithoutUserConfirmation();
				secondFactory.Save();

				adjustment.WD_WW_Whs = Helper.CreateWarehouse("W2").PK;
				AssertExceptionThrown("Since the adjustment value has changed, exception should be thrown.",
							typeof(ZSaveConcurrencyException), () => Factory.Save());
			}
		}

		#endregion

		#region TestExceptionThrown_IfCriticalFieldsChangedInAnotherFactory_AndFinalisedInFactory

		[UseSnapshotProtection]
		public void TestExceptionThrown_IfCriticalFieldsChangedInAnotherFactory_AndFinalisedInFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, inventory.Location);
			adjustment.RunPreSaveValidation();
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
				var adjustmentInSecondFactory = secondFactory.Load<WhsAdjustment>(adjustment.PK);
				adjustmentInSecondFactory.Lines[0].WE_TransactionQuantity = 5;
				adjustmentInSecondFactory.RunPreSaveValidation();
				secondFactory.Save();

				adjustment.FinaliseDocketWithoutUserConfirmation();
				AssertExceptionThrown("Since the adjustmentLine value has changed, exception should be thrown.",
							typeof(ZSaveConcurrencyException), () => Factory.Save());
			}
		}

		#endregion

		#region TestExceptionThrown_IfCriticalFieldsChangedAndFinalisedInAnotherFactory_AndFinalisedInFactory

		[UseSnapshotProtection]
		public void TestExceptionThrown_IfCriticalFieldsChangedAndFinalisedInAnotherFactory_AndFinalisedInFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, inventory.Location);
			adjustmentLine.RunPreSaveValidation(); // to save pick lines
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
				var adjustmentInSecondFactory = secondFactory.Load<WhsAdjustment>(adjustment.PK);
				adjustmentInSecondFactory.Lines[0].WE_TransactionQuantity = 5;
				adjustmentInSecondFactory.FinaliseDocketWithoutUserConfirmation();
				secondFactory.Save();

				adjustment.FinaliseDocketWithoutUserConfirmation();
				AssertExceptionThrown("Since the adjustmentLine value has changed, exception should be thrown.", typeof(ZSaveConcurrencyException), () => Factory.Save());
			}
		}

		#endregion

		#region TestLockCriticalFieldsOnAdjustmentLine_OnSave

		[UseSnapshotProtection]
		public void TestLockCriticalFieldsOnAdjustmentLine_OnSave()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, inventory.Location);
			adjustment.RunPreSaveValidation(); // to commit inventory and create pickline
			Factory.Save();

			((IDbConnected)Factory).Connection.BeginTransaction();
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var isSecondUserLocked = false;
			var task = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var adjustmentInSecondFactory = secondFactory.Load<WhsAdjustment>(adjustment.PK);

					connectionForSecondUser.BeginTransaction();
					try
					{
						adjustmentInSecondFactory.Lines[0].WE_TransactionQuantity = 10;
						isSecondUserLocked = true;
						AssertExceptionThrown("Since the adjustment already finalised, quantity cannot be changed on line, exception should be thrown.",
							typeof(ZSaveConcurrencyException),
							() => secondFactory.Save()); // should be locked on SQL level by first instance and once the lock released by Commit should fail with critical change
						isSecondUserLocked = false;
					}
					finally
					{
						connectionForSecondUser.RollbackTransaction();
					}
				}
			});

			Thread.Sleep(5000);
			AssertEquals("Second user should be currently locked.", true, isSecondUserLocked);
			AssertEquals(true, (Factory as IDbConnected).Connection.IsInTransaction);

			((IDbConnected)Factory).Connection.CommitTransaction();
			task.Wait();
			AssertEquals("Second user should not be locked anymore.", false, isSecondUserLocked);
			AssertEquals(false, (Factory as IDbConnected).Connection.IsInTransaction);
		}

		#endregion

		#region TestFinaliseDocket_AdjustOut_TriggerPreventsSavingInInconsistentState

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestFinaliseDocket_AdjustOut_TriggerPreventsSavingInInconsistentState()
		{
			var staff = Helper.CreateGlbStaff("AA", "AA");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, receive1.Lines[0].Location);

			adjustment.RunPreSaveValidation();
			AssertEquals("Precondition: Adjustment is committed.", 5m, adjustment.Lines.Cast<WhsAdjustmentLine>().Sum(al => al.CommittedQuantity));
			AssertEquals("Precondition: All Picked Times and Pickers should be empty.", true,
				adjustment.Lines.Cast<WhsAdjustmentLine>().All(al => al.PickLines.All(pl => pl.WZ_PickedDateTime.IsEmpty && pl.WZ_GS_NKAssignedTo.IsEmpty)));

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				adjustment.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Adjustment should be finalised.", true, adjustment.IsFinalised);
				AssertEquals("Precondition: Finalised Date is set.", true, !adjustment.WD_FinalisedDate.IsEmpty);
				AssertEquals("All Picked Times should be set to finalised time, and Picker should be set to AA.", true,
					adjustment.Lines.Cast<WhsAdjustmentLine>().All(al => al.PickLines.All(pl => pl.WZ_PickedDateTime == adjustment.WD_FinalisedDate && pl.WZ_GS_NKAssignedTo == "AA")));
			}

			Factory.Save();

			var connection = ((IDbConnected)Factory).Connection;
			connection.ExecuteNonQuery(@"
DISABLE TRIGGER TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalized ON WhsPickLine;
DISABLE TRIGGER TG_WhsPickLine_StockOnHandIsBalanced ON WhsPickLine;
DISABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced ON WhsDocketLine;
DISABLE TRIGGER TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect ON WhsDocketLine;
"); // This test relies on a bad datashape

			var skipRunningSPAferFactorySave = new Mock<IDeferredTriggerRunner>();
			skipRunningSPAferFactorySave.Setup(x => x.RunDeferredTriggers(It.IsAny<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>>(), It.IsAny<IDbConnected>()))
				.Callback<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>, IDbConnected>((triggers, factory) => { });

			using (ObjectFactory.Substitute(skipRunningSPAferFactorySave.Object))
			{
				// Try clearing the existing pick lines picked time
				var pickLine = adjustmentLine.PickLines[0];
				var pickLineRow = ((IBusinessObjectInternals)pickLine).Row;
				pickLineRow[WhsPickLineSchema.Constants.WZ_PickedDateTime] = DBNull.Value; // it is not valid to unpick pickLines, but we will go through the Row Setter for the sake of testing the trigger
				pickLine.HasChanges = true; // need HasChanges true for Factory Save to attempt saving the PickLine
				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to save unpicked PickLines that are attached to a Finalised Job.", true), "Should have thrown an exception.");

				pickLineRow[WhsPickLineSchema.Constants.WZ_PickedDateTime] = adjustment.WD_FinalisedDate.ToDateTimeOffset(); // need to revert to valid Picked DateTime
				AssertNoExceptionThrown(Factory.Save);

				// Try adding another Pick line with no picked time set
				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
				Factory.Save();

				var args = new BusinessObjectCloneArgs(new[] { WhsPickLineSchema.Constants.WZ_PickedDateTime }, performRowCopyWithoutTriggeringValidationAndSetter: true);
				var newPickLine = (WhsPickLine)pickLine.Clone(args);
				newPickLine.WZ_WE_InventoryLine = receive2.Lines[0].PK;
				adjustmentLine.WE_TransactionQuantity = -10; // Allow Transaction Quantity trigger to pass
				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to save unpicked PickLines that are attached to a Finalised Job.", true), "Should have thrown an exception.");

				newPickLine.Delete();
				adjustmentLine.WE_TransactionQuantity = -5; // revert changes
				AssertNoExceptionThrown(Factory.Save);
			}
		}

		#endregion

		#region Implementation

		#region Helpers

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory(testCaseDbConnection)); }
		}
		BusinessObjectFactory factory;

		DbConnection testCaseDbConnection;

		protected override void SetUp()
		{
			testCaseDbConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion

		#endregion
	}
}
