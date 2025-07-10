using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	public class TestTG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised : TestCase
	{
		#region TestTrigger_InsertingAnUnsynchronisedChild

		[ExpectNoExceptions]
		public void TestTrigger_InsertingAnUnsynchronisedChild()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var parentTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			parentTransferLine.RunPreSaveValidation();
			AssertEquals("Precondition", 10m, parentTransferLine.GetQtyCommittedIncludingMatchingLines());

			parentTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, parentTransferLine.WE_DocketLineStatus);

			var childTransferLine = parentTransferLine.ChildTransferLine;
			AssertNotNull("Precondition: Child Line created.", childTransferLine);
			AssertEquals("Precondition: Not yet saved to database.", false, childTransferLine.IsInDatabase);
			childTransferLine.WE_PalletID = "ABC";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to desynchronise InterWhs Child!", true), "Should have prevented saving unsynchronised child line!");

			childTransferLine.WE_PalletID = "";
			AssertNoExceptionThrown("Should save succesfully when put back in sync.", Factory.Save);
		}

		#endregion

		#region TestTrigger_UpdatingParentDocketLineFKToAnUnsynchronisedChild

		[ExpectNoExceptions]
		public void TestTrigger_UpdatingParentDocketLineFKToAnUnsynchronisedChild()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part1, 20m, whs1.DefaultLocation, "BAD");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var parentTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			var otherTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			otherTransferLine.WE_PalletID = "BAD";
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			otherTransferLine.WE_WE_ParentDocketLine = parentTransferLine.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to desynchronise InterWhs Child!", true), "Should have prevented linking unsynchronised child line!");

			otherTransferLine.WE_PalletID = "";
			transfer.RunPreSaveValidation(); // to commit inventory
			AssertNoExceptionThrown("Should save succesfully when put back in sync.", Factory.Save);
		}

		#endregion

		#region TestTrigger_UpdatingParentDocketLineFKToAnUnsynchronisedChild_DoNotCareIfNotTransfer

		public void TestTrigger_UpdatingParentDocketLineFKToAnUnsynchronisedChild_DoNotCareIfNotTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transferLine.WE_PalletID = "BAD";
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine.WE_WE_ParentDocketLine = receive.Lines[0].PK;
			AssertNoExceptionThrown("Should not fail if a non transfer line is linked.", Factory.Save);

			transferLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			receive.Lines[0].WE_WE_ParentDocketLine = transferLine.PK;
			receive.Lines[0].WE_WE_OriginalDocketLineForRating = transferLine.PK;
			receive.Lines[0].WE_IsOriginalInventory = false;
			AssertNoExceptionThrown("Should not fail if a non transfer line is linked.", Factory.Save);
		}

		#endregion

		#region TestTrigger_UpdatingTask

		public void TestTrigger_UpdatingTask()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part1, 20m, whs1.DefaultLocation, "BAD");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var parentTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			var otherTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			otherTransferLine.WE_WE_ParentDocketLine = parentTransferLine.PK;
			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_ParentID = transfer.PK;
			task.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task.P9_FormFlowType = "WTR";

			otherTransferLine.WE_P9_Task = task.PK;
			AssertNoExceptionThrown("Should save succesfully", Factory.Save);

			otherTransferLine.WE_P9_Task = ZGuid.Empty;
			Factory.Save();

			parentTransferLine.WE_P9_Task = task.PK;
			AssertNoExceptionThrown("Should save succesfully", Factory.Save);
		}

		#endregion

		#region TestTrigger_WhenSaveFailsForUnrelatedReason

		public void TestTrigger_WhenSaveFailsForUnrelatedReason()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", data.Whs1.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Today;
			var childTransferLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created Child line.", childTransferLine);
			Factory.Save();

			BusinessObjectFactory.SavingEventHandler handler = f => { f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f)); };
			Factory.Saving += handler;

			transferLine.WE_PalletID = "A";
			childTransferLine.WE_PalletID = "A";
			AssertEquals("Precondition.", true, transferLine.HasChanges);
			AssertEquals("Precondition.", true, childTransferLine.HasChanges);

			// Precondition
			Helper.AssertZCannotSaveExceptionThrown("Test", Factory.Save);
			Factory.Saving -= handler;

			// If the previous error has caused problems with trigger suspension, the updates below will fail as the trigger will fail when updating one of the rows
			transferLine.WE_PalletID = "B";
			childTransferLine.WE_PalletID = "B";
			AssertEquals("Precondition.", true, transferLine.HasChanges);
			AssertEquals("Precondition.", true, childTransferLine.HasChanges);

			AssertNoExceptionThrown(Factory.Save); // should not fail when saving this transaction
		}

		// throw exception before committing transaction
		class ServiceThatThrowsException : IAfterOnSavingBOProcessingService
		{
			public ServiceThatThrowsException(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<ServiceThatThrowsException>();
				throw new ZCannotSaveException("Test", "Test", ExceptionType.BusinessFailure);
			}
		}

		#endregion

		#region TestTrigger_HoldCodeChanged_InnerWhsTransfer

		public void TestTrigger_HoldCodeChanged_InnerWhsTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1-1", "A-1-2");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition", 10m, transferLine.GetQtyCommittedIncludingMatchingLines());

			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			transferLine.IsInventoryEditForm = true;
			transferLine.HeldCodeChangeQuantity = 5m;
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition: Split inventory.", 5m, transferLine.WE_StockOnHand);

			AssertNoExceptionThrown("Should not fail when saving the hold code changed line.", Factory.Save);

			// Change "Parent"
			var connection = ((IDbConnected)Factory).Connection;
			AssertNoExceptionThrown("Should not fail if Parent is put out of sync.",
				() => ExecuteSqlInTransaction(connection, WhsDocketLineDO.UpdateWhere(transferLine.PK.ToGuid()).Set(l => l.WE_PalletID, "BAD1").AsSQL()));

			// Change "Child"
			var childQuery = new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 5m);
			childQuery.AddToFilter(WhsDocketLineSchema.WE_WE_ParentDocketLine, transferLine.PK);

			var holdCodeChangeLine = Factory.LoadTop1<WhsTransferLine>(childQuery);
			AssertNotEquals("Precondition.", transferLine.PK, holdCodeChangeLine.PK);
			AssertNoExceptionThrown("Should not fail if Child is put out of sync.",
				() => ExecuteSqlInTransaction(connection, WhsDocketLineDO.UpdateWhere(holdCodeChangeLine.PK.ToGuid()).Set(l => l.WE_PalletID, "BAD2").AsSQL()));
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		#endregion

		#region TestTrigger_HoldCodeChanged_InterWhsTransfer

		#region TestTrigger_HoldCodeChanged_InterWhsTransfer_Source

		public void TestTrigger_HoldCodeChanged_InterWhsTransfer_Source()
		{
			TestTrigger_HoldCodeChanged_InterWhsTransfer_Core(parentType: TransferType.Codes.InterWhsDest); // Parent = Dest
		}

		#endregion

		#region TestTrigger_HoldCodeChanged_InterWhsTransfer_Destination

		public void TestTrigger_HoldCodeChanged_InterWhsTransfer_Destination()
		{
			TestTrigger_HoldCodeChanged_InterWhsTransfer_Core(parentType: TransferType.Codes.InterWhsSource); // Parent = Source
		}

		#endregion

		#region TestTrigger_HoldCodeChanged_InterWhsTransfer_Core

		void TestTrigger_HoldCodeChanged_InterWhsTransfer_Core(string parentType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, parentType == TransferType.Codes.InterWhsDest ? whs2 : data.Whs1);
			transfer.DocketSubType = parentType;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", parentType == TransferType.Codes.InterWhsDest ? data.Whs1.PK : whs2.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			var childTransferLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created Child line.", childTransferLine);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(childTransferLine);
			Factory.Save();

			var lineToHoldCodeChange = parentType == TransferType.Codes.InterWhsDest ? transferLine : childTransferLine;
			lineToHoldCodeChange.IsInventoryEditForm = true;
			lineToHoldCodeChange.HeldCodeChangeQuantity = 5m;
			lineToHoldCodeChange.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			lineToHoldCodeChange.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition: Split inventory.", 5m, lineToHoldCodeChange.WE_StockOnHand);

			AssertNoExceptionThrown("Should not fail when saving the hold code changed line.", Factory.Save);
		}

		#endregion

		#endregion

		#region TestTrigger_ConcurrencyCase_ChildTransferCreated

		[ExpectNoExceptions]
		public void TestTrigger_ConcurrencyCase_ChildTransferCreated()
		{
			// Connection1: Create + link a child line, save
			// Connection2: Change a field that needs to be synchonised
			// Connection2 change should be blocked and then rejected (tests the join to child)
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", data.Whs1.PK, "A");
			var transferLinePK = transferLine.PK;
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertNull("Precondition: No child created yet.", transferLine.ChildTransferLine);
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
				var transferLineInFactory2 = factory2.Load<WhsTransferLine>(transferLine.PK);

				transferLineInFactory2.PickedTime = ZDateTimeOffset.Now;
				AssertNotNull("Precondition: Child created.", transferLineInFactory2.ChildTransferLine);

				connection2.BeginTransaction();
				AssertNoExceptionThrown(factory2.Save);

				var taskUpdateParentTransferLine = new Task(() =>
				{
					using (var connection3 = Db.NewExtraConnectionToMainDb())
					{
						connection3.BeginTransaction();

						// Some triggers may be deferred in production, this test should defer them to ensure (something else) is always locking as expected
						SuspendOtherDeferrableTriggers(connection3);

						NUnit.Framework.Assert.That(delegate
						{
							WhsDocketLineDO.UpdateWhere(transferLinePK.ToGuid()).Set(l => l.WE_PalletID, "BAD").Post(connection3);
						}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "TriggerLikelyConcurrencyError: Attempt to desynchronise InterWhs Child!", true), "Should have prevented saving change that would lead to an unsynchronised child line.");
					}
				});

				taskUpdateParentTransferLine.Start();
				taskUpdateParentTransferLine.Wait(5000); // Should wait as the other transaction has a lock
				connection2.CommitTransaction();
				taskUpdateParentTransferLine.Wait();
			}
		}

		#endregion

		#region TestTrigger_ConcurrencyCase_UnsyncedChildTransferCreated

		[ExpectNoExceptions]
		public void TestTrigger_ConcurrencyCase_UnsyncedChildTransferCreated()
		{
			// Connection1: Link a child line, save
			// Connection2: Change a field on the child line that needs to be synchonised
			// Connection2 change should be blocked and then rejected (tests the join to parent)
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", data.Whs1.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertNull("Precondition: No child created yet.", transferLine.ChildTransferLine);

			transferLine.PickedTime = ZDateTimeOffset.Now;

			var childTransferLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Child created.", childTransferLine);

			// Dodgy - set up a "child line" that is not linked to its parent.
			childTransferLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			Helper.CreateWhsPickLine(childTransferLine, receive.Inventory[0], 10m); // add dodgy pick line to ensure imbalance triggers pass.
			AssertNull("Precondition: No child.", transferLine.ChildTransferLine);
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
				var childTransferLineInFactory2 = factory2.Load<WhsTransferLine>(childTransferLine.PK);
				childTransferLineInFactory2.WE_WE_ParentDocketLine = transferLine.PK;

				connection2.BeginTransaction();
				AssertNoExceptionThrown(factory2.Save);

				var taskUpdateChildTransferLine = new Task(() =>
				{
					using (var connection3 = Db.NewExtraConnectionToMainDb())
					{
						connection3.BeginTransaction();

						// Some triggers may be deferred in production, this test should defer them to ensure (something else) is always locking as expected
						SuspendOtherDeferrableTriggers(connection3);

						NUnit.Framework.Assert.That(() =>
							{
								connection3.ExecuteNonQuery(WhsDocketLineDO.UpdateWhere(childTransferLine.PK.ToGuid()).Set(l => l.WE_PalletID, "BAD").AsSQL());
							}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "TriggerLikelyConcurrencyError: Attempt to desynchronise InterWhs Child!", true), "Should have prevented saving change that would lead to an unsynchronised child line.");
					}
				});

				taskUpdateChildTransferLine.Start();
				taskUpdateChildTransferLine.Wait(5000); // Should wait as the other transaction has a lock
				connection2.CommitTransaction();
				taskUpdateChildTransferLine.Wait();
			}
		}

		#endregion

		#region TestTrigger_UnsychronisedHoldCodes

		public void TestTrigger_UnsychronisedHoldCodes()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				TestTrigger_UnsychronisedHoldCodes_Core(factory);
			}
		}

		void TestTrigger_UnsychronisedHoldCodes_Core(BusinessObjectFactory testFactory)
		{
			var lastWeek = ZDate.Today.AddDays(-7);

			var data = new TestDataSimpleEnvironment(testFactory);
			var helper = new WhsTestHelperFunctions(testFactory);

			var whs1 = data.Whs1;
			var whs2 = helper.CreateWarehouse("Wh2", "A", 1, 1);
			var receive = helper.CreateWhsReceive(data.Org1, whs1, "R1");
			var receiveLine = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			receiveLine.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			testFactory.Save();

			// Set up InterWhs Line
			var transfer = helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var parentTransferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			parentTransferLine.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			parentTransferLine.RunPreSaveValidation();
			AssertEquals("Precondition", 10m, parentTransferLine.GetQtyCommittedIncludingMatchingLines());

			parentTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, parentTransferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on source transfer line", InventoryStatus.Codes.InTransit, parentTransferLine.WE_OriginalInventoryStatus);

			var childTransfers = transfer.ChildTransfers;
			var childTransferLine = parentTransferLine.ChildTransferLine;
			AssertEquals("Should have created create 1 destination child transfer.", 1, childTransfers.Count());
			AssertNotNull("Should have a child transfer line.", childTransferLine);

			testFactory.Save();

			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var value = new ZString("HOLD");

				// Original inventory held code should be synchronised. 
				// Need to change current held code simultaneously due to constraint [Constraint_CurrentHoldCodeEqualOriginalIfNotFinalised] 
				// and cannot change original held code once finalised.
				var propertyAndValuePairs = new Dictionary<ZPropertyInfo, IZType>();
				propertyAndValuePairs.Add(parentTransferLine.ZPropertyInfoHash["WE_WHC_NKOriginalInventoryHeldCode"], value);
				propertyAndValuePairs.Add(parentTransferLine.ZPropertyInfoHash["WE_WHC_NKCurrentInventoryHeldCode"], value);

				TryChangeValuesSimultaneouslyInSQL(connection2, childTransferLine.PK, propertyAndValuePairs, shouldFail: true); // Change child via SQL
				TryChangeValuesSimultaneouslyInSQL(connection2, parentTransferLine.PK, propertyAndValuePairs, shouldFail: true); // Change parent via SQL
				TryChangeValuesSimultaneouslyInSQL(connection2, parentTransferLine.PK, propertyAndValuePairs, shouldFail: false, secondDocketLinePK: childTransferLine.PK); // Change both simultaneously via SQL

				TryChangeValuesSimultaneouslyInBizo(connection2, childTransferLine.PK, propertyAndValuePairs, shouldFail: true); // Change child via business logic
				TryChangeValuesSimultaneouslyInBizo(connection2, parentTransferLine.PK, propertyAndValuePairs, shouldFail: true); // Change parent via business logic
				TryChangeValuesSimultaneouslyInBizo(connection2, parentTransferLine.PK, propertyAndValuePairs, shouldFail: false, secondDocketLinePK: childTransferLine.PK); // Change both via business logic in same Factory.Save

				parentTransferLine.FinaliseDocketLine();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(parentTransferLine);
				parentTransferLine.Factory.Save();

				// You can change the current inventory held code after finalisation
				var currentInventoryHeldCode = parentTransferLine.ZPropertyInfoHash["WE_WHC_NKCurrentInventoryHeldCode"];
				TryChangeValueInSQL(connection2, childTransferLine.PK, currentInventoryHeldCode.Name, value, shouldFail: false); // Change child via SQL
				TryChangeValueInSQL(connection2, parentTransferLine.PK, currentInventoryHeldCode.Name, value, shouldFail: false); // Change parent via SQL

				TryChangeValueInBizo(connection2, childTransferLine.PK, currentInventoryHeldCode, value, shouldFail: false); // Change child via business logic
				TryChangeValueInBizo(connection2, parentTransferLine.PK, currentInventoryHeldCode, value, shouldFail: false); // Change parent via business logic

				// Try changing parent or child where the row is out of sync, but no relevant fields have changed
				TryChangeValueInSQL_AfterChangingUnrelatedField(connection2, childTransferLine.PK, currentInventoryHeldCode.Name, newValue: value);
				TryChangeValueInSQL_AfterChangingUnrelatedField(connection2, parentTransferLine.PK, currentInventoryHeldCode.Name, newValue: value);
				TryChangeValueInBizo_AfterChangingUnrelatedField(connection2, childTransferLine.PK, currentInventoryHeldCode, newValue: value);
				TryChangeValueInBizo_AfterChangingUnrelatedField(connection2, parentTransferLine.PK, currentInventoryHeldCode, newValue: value);
			}
		}

		#endregion

		#region TestTrigger_UnloadedTimeAndUnloadedBy

		[ExpectNoExceptions]
		public void TestTrigger_UnloadedTimeAndUnloadedBy()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var parentTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			parentTransferLine.RunPreSaveValidation();
			AssertEquals("Precondition", 10m, parentTransferLine.GetQtyCommittedIncludingMatchingLines());

			var now = DateTime.Now;
			parentTransferLine.PickedTime = now;
			parentTransferLine.WE_UnloadedTime = now;
			parentTransferLine.WE_GS_NKUnloadedBy = "A";
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, parentTransferLine.WE_DocketLineStatus);

			var childTransferLine = parentTransferLine.ChildTransferLine;
			AssertNotNull("Precondition: Child Line created.", childTransferLine);
			AssertEquals("Precondition: Not yet saved to database.", false, childTransferLine.IsInDatabase);

			childTransferLine.WE_UnloadedTime = now;
			childTransferLine.WE_GS_NKUnloadedBy = "B";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to desynchronise InterWhs Child!", true), "Should have prevented saving unsynchronised child line!");

			childTransferLine.WE_UnloadedTime = now.AddDays(-1);
			childTransferLine.WE_GS_NKUnloadedBy = "A";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to desynchronise InterWhs Child!", true), "Should have prevented saving unsynchronised child line!");

			childTransferLine.WE_UnloadedTime = now;
			childTransferLine.WE_GS_NKUnloadedBy = "A";
			AssertNoExceptionThrown("Should save succesfully when put back in sync.", Factory.Save);
		}

		#endregion

		#region TestTrigger_WorksForAllColumns

		[TestDate(2017, 1, 1)]
		public void TestTrigger_WorksForAllColumns()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				TestTrigger_WorksForAllColumns_Core(factory);
			}
		}

		void TestTrigger_WorksForAllColumns_Core(BusinessObjectFactory testFactory)
		{
			var lastWeek = ZDate.Today.AddDays(-7);

			var data = new TestDataSimpleEnvironment(testFactory);
			var helper = new WhsTestHelperFunctions(testFactory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var whs1 = data.Whs1;
			var whs2 = helper.CreateWarehouse("Wh2", "A", 1, 1);
			var receive = helper.CreateWhsReceive(data.Org1, whs1, "R1");
			var receiveLine = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, lastWeek, lastWeek, "", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			testFactory.Save();

			// Set up InterWhs Line
			var transfer = helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var parentTransferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			parentTransferLine.WE_PackingDate = lastWeek;
			parentTransferLine.WE_ExpiryDate = lastWeek;
			parentTransferLine.RunPreSaveValidation();
			AssertEquals("Precondition", 10m, parentTransferLine.GetQtyCommittedIncludingMatchingLines());

			// Ensure all nullable fields have values (for easily testing null handling below)
			parentTransferLine.WE_CustomDate1 = lastWeek;
			parentTransferLine.WE_CustomDate2 = lastWeek;
			parentTransferLine.WE_CustomDate3 = lastWeek;
			parentTransferLine.WE_CustomDate4 = lastWeek;
			parentTransferLine.WE_CustomDate5 = lastWeek;
			parentTransferLine.WE_RequiredByDate = lastWeek.ToZDateTime().ToOffset();

			parentTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, parentTransferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on source transfer line", InventoryStatus.Codes.InTransit, parentTransferLine.WE_OriginalInventoryStatus);

			var childTransfers = transfer.ChildTransfers;
			var childTransferLine = parentTransferLine.ChildTransferLine;
			AssertEquals("Should have created create 1 destination child transfer.", 1, childTransfers.Count());
			AssertNotNull("Should have a child transfer line.", childTransferLine);
			childTransferLine.WE_RequiredByDate = lastWeek.ToZDateTime().ToOffset(); // Synchronise with above
			parentTransferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			testFactory.Save();

			var valuesToReplace = GetValuesToReplace(parentTransferLine, helper);
			var propertiesThatDoNotHaveToBeSynchronised = GetPropertiesThatDoNotHaveToBeSynchronised();
			var propertiesBeyondTheScopeOfThisTest = GetPropertiesThatShouldBeTestedElsewhere();
			var propertiesThatHaveConflictingConstraints = GetPropertiesThatHaveConflictingConstraints();
			var notNullInPracticeProperties = GetPropertiesNotNullInPractice();
			var propertiesThatRequireFinalisation = GetPropertiesThatRequireFinalisation();

			foreach (var propertyInfo in parentTransferLine.ZPropertyInfoHash.Cast<ZPropertyInfo>()
				.Where(pi => pi.IsPersistent && !propertiesBeyondTheScopeOfThisTest.Contains(pi.Name) && !propertiesThatHaveConflictingConstraints.Contains(pi.Name))
				.OrderBy(pi => propertiesThatRequireFinalisation.Contains(pi.Name))) // Do all properties that require finalisation at the end
			{
				var testWithNullValue = ShouldRunNullableTestCases(parentTransferLine, propertiesThatDoNotHaveToBeSynchronised, notNullInPracticeProperties, propertyInfo);

				FinaliseTransferLineIfRequired(parentTransferLine, propertiesThatRequireFinalisation, propertyInfo);

				var valueToReplace = GetReplacementValueForTest(valuesToReplace, propertyInfo);
				foreach (var value in testWithNullValue ? new[] { valueToReplace, null } : new[] { valueToReplace })
				{
					var propertyInfoName = propertyInfo.Name;

					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						if (propertiesThatDoNotHaveToBeSynchronised.Contains(propertyInfoName))
						{
							TryChangeValueInSQL(connection2, childTransferLine.PK, propertyInfoName, value, shouldFail: false); // Change child via SQL
							TryChangeValueInSQL(connection2, parentTransferLine.PK, propertyInfoName, value, shouldFail: false); // Change parent via SQL

							TryChangeValueInBizo(connection2, childTransferLine.PK, propertyInfo, value, shouldFail: false); // Change child via business logic
							TryChangeValueInBizo(connection2, parentTransferLine.PK, propertyInfo, value, shouldFail: false); // Change parent via business logic

							// Try changing parent or child where the row is out of sync, but no relevant fields have changed
							TryChangeValueInSQL_AfterChangingUnrelatedField(connection2, childTransferLine.PK, propertyInfoName, newValue: value);
							TryChangeValueInSQL_AfterChangingUnrelatedField(connection2, parentTransferLine.PK, propertyInfoName, newValue: value);
							TryChangeValueInBizo_AfterChangingUnrelatedField(connection2, childTransferLine.PK, propertyInfo, newValue: value);
							TryChangeValueInBizo_AfterChangingUnrelatedField(connection2, parentTransferLine.PK, propertyInfo, newValue: value);
						}
						else
						{
							TryChangeValueInSQL(connection2, childTransferLine.PK, propertyInfoName, value, shouldFail: true); // Change child via SQL
							TryChangeValueInSQL(connection2, parentTransferLine.PK, propertyInfoName, value, shouldFail: true); // Change parent via SQL
							TryChangeValueInSQL(connection2, parentTransferLine.PK, propertyInfoName, value, shouldFail: false, secondDocketLinePK: childTransferLine.PK); // Change both simultaneously via SQL

							TryChangeValueInBizo(connection2, childTransferLine.PK, propertyInfo, value, shouldFail: true); // Change child via business logic
							TryChangeValueInBizo(connection2, parentTransferLine.PK, propertyInfo, value, shouldFail: true); // Change parent via business logic
							TryChangeValueInBizo(connection2, parentTransferLine.PK, propertyInfo, value, shouldFail: false, secondDocketLinePK: childTransferLine.PK); // Change both via business logic in same Factory.Save
						}
					}
				}
			}
		}

		static bool ShouldRunNullableTestCases(WhsTransferLine parentTransferLine, HashSet<ZString> propertiesThatDoNotHaveToBeSynchronised, HashSet<ZString> notNullInPracticeProperties, ZPropertyInfo propertyInfo)
		{
			var shouldRunNullableTestCases = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(propertyInfo.Name, WhsDocketLineSchema.Constants.TableName).IsNullable
				&& !notNullInPracticeProperties.Contains(propertyInfo.Name) // Other constraints/triggers make null values not really possible
				&& !propertiesThatDoNotHaveToBeSynchronised.Contains(propertyInfo.Name); // There is no value in testing null edge cases for properties which are not used in the trigger

			if (shouldRunNullableTestCases && ((IZType)parentTransferLine[propertyInfo.Name]).IsEmpty)
			{
				Fail($"Nullable columns should have a value by default to simplify testing null handling behaviour. Column Name: {propertyInfo.Name}");
			}

			return shouldRunNullableTestCases;
		}

		static IZType GetReplacementValueForTest(Dictionary<ZString, IZType> valuesToReplace, ZPropertyInfo propertyInfo)
		{
			IZType valueToReplace;
			if (!valuesToReplace.TryGetValue(propertyInfo.Name, out valueToReplace))
			{
				if (propertyInfo.PropertyType == typeof(ZGuid))
				{
					Fail($"Column: {propertyInfo.Name} is unhandled! Guid properties must be specially handled in this test as they require creating new data.");
				}

				valueToReplace = GetDefaultValueToReplace(propertyInfo.PropertyType);
			}

			return valueToReplace;
		}

		void FinaliseTransferLineIfRequired(WhsTransferLine parentTransferLine, HashSet<ZString> propertiesThatRequireFinalisation, ZPropertyInfo propertyInfo)
		{
			if (!parentTransferLine.IsFinalised && propertiesThatRequireFinalisation.Contains(propertyInfo.Name))
			{
				parentTransferLine.FinaliseDocketLine();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(parentTransferLine);
				parentTransferLine.Factory.Save();
			}
		}

		void TryChangeValueInBizo(DbConnection connection, ZGuid docketLinePK, ZPropertyInfo property, IZType newValue, bool shouldFail, ZGuid secondDocketLinePK = new ZGuid())
		{
			var factory2 = new BusinessObjectFactory(connection) { RefreshEnabled = false };
			var transferLineInFactory2 = factory2.Load<WhsTransferLine>(docketLinePK);

			var valueForDataLayer = newValue != null ? ((IZTypeInternals)newValue).GetValueForLogicalDataLayer(property.IsNullable) : DBNull.Value;

			// Avoid future syncing logic by using rows
			// I'm doing this rather than supressing synchronisation as it also bypasses various exceptions/checks on the setters
			((IBusinessObjectInternals)transferLineInFactory2).Row[property.Name] = valueForDataLayer;
			transferLineInFactory2.HasChanges = true;

			if (secondDocketLinePK.IsValid)
			{
				var secondTransferLineInFactory2 = factory2.Load<WhsTransferLine>(secondDocketLinePK);

				((IBusinessObjectInternals)secondTransferLineInFactory2).Row[property.Name] = valueForDataLayer;
				secondTransferLineInFactory2.HasChanges = true;
			}

			connection.BeginTransaction();

			// delete imbalance check for WE_TransactionQuantity to test sync trigger
			if (property.Name == WhsDocketLineSchema.Constants.WE_TransactionQuantity)
			{
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsTestHelperFunctions.WhsCheckTransactionAndPickQtyIsCorrect} (@TransactionLinePKs dbo.TVP_uniqueidentifier READONLY)", "0", connection);
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0", connection);
			}

			if (shouldFail)
			{
				CombineAssertions($"Column: {property.Name}", () => AssertTriggerExceptionThrown_OrCannotModifyPropertyThroughBusinessLayer(factory2));
			}
			else
			{
				CombineAssertions($"Column: {property.Name}",
					() =>
					{
						try
						{
							factory2.Save();
						}
						catch (ZCannotSaveException ex) when (ex.Message.Contains("Cannot save as a Finalized Transfer Line has been modified."))
						{
							// Cannot save a line with this property changed via the business layer, it is sufficient to just test the trigger works with SQL
							AssertEquals(ExceptionType.BusinessFailure, ex.Type);
						}
					});
			}

			connection.RollbackTransaction();
		}

		void TryChangeValuesSimultaneouslyInBizo(DbConnection connection, ZGuid docketLinePK, Dictionary<ZPropertyInfo, IZType> propertyAndValuePairs, bool shouldFail, ZGuid secondDocketLinePK = new ZGuid())
		{
			var factory2 = new BusinessObjectFactory(connection) { RefreshEnabled = false };
			var transferLineInFactory2 = factory2.Load<WhsTransferLine>(docketLinePK);

			foreach (var propertyAndValuePair in propertyAndValuePairs)
			{
				var valueForDataLayer = propertyAndValuePair.Value != null ? ((IZTypeInternals)propertyAndValuePair.Value).GetValueForLogicalDataLayer(propertyAndValuePair.Key.IsNullable) : DBNull.Value;

				// Avoid future syncing logic by using rows
				// I'm doing this rather than supressing synchronisation as it also bypasses various exceptions/checks on the setters
				((IBusinessObjectInternals)transferLineInFactory2).Row[propertyAndValuePair.Key.Name] = valueForDataLayer;
				transferLineInFactory2.HasChanges = true;

				if (secondDocketLinePK.IsValid)
				{
					var secondTransferLineInFactory2 = factory2.Load<WhsTransferLine>(secondDocketLinePK);

					((IBusinessObjectInternals)secondTransferLineInFactory2).Row[propertyAndValuePair.Key.Name] = valueForDataLayer;
					secondTransferLineInFactory2.HasChanges = true;
				}

				// delete imbalance check for WE_TransactionQuantity to test sync trigger
				if (propertyAndValuePair.Key == WhsDocketLineSchema.Constants.WE_TransactionQuantity)
				{
					WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsTestHelperFunctions.WhsCheckTransactionAndPickQtyIsCorrect} (@TransactionLinePKs dbo.TVP_uniqueidentifier READONLY)", "0", connection);
				}
			}
			connection.BeginTransaction();

			if (shouldFail)
			{
				CombineAssertions("Multiple fields", () => AssertTriggerExceptionThrown_OrCannotModifyPropertyThroughBusinessLayer(factory2));
			}
			else
			{
				CombineAssertions("Multiple fields",
					() =>
					{
						try
						{
							factory2.Save();
						}
						catch (ZCannotSaveException ex) when (ex.Message.Contains("Cannot save as a Finalized Transfer Line has been modified."))
						{
							// Cannot save a line with this property changed via the business layer, it is sufficient to just test the trigger works with SQL
							AssertEquals(ExceptionType.BusinessFailure, ex.Type);
						}
					});
			}

			connection.RollbackTransaction();
		}

		static void AssertTriggerExceptionThrown_OrCannotModifyPropertyThroughBusinessLayer(BusinessObjectFactory factory2)
		{
			var exceptionThrown = false;

			try
			{
				factory2.Save();
			}
			catch (Exception ex)
			{
				if (ex is ZSaveConcurrencyException || ex is ZConcurrencyCheckFailureException)
				{
					var exceptionToAssert = ex.GetInnermostException();
					AssertContains("Exception Message.", "Attempt to desynchronise InterWhs Child!", exceptionToAssert.Message);
					exceptionThrown = true;
				}
				else if (ex is ZCannotSaveException && ex.Message.Contains("Cannot save as a Finalized Transfer Line has been modified."))
				{
					// Cannot save a line with this property changed via the business layer, it is sufficient to just test the trigger works with SQL
					exceptionThrown = true;
					AssertEquals(ExceptionType.BusinessFailure, ((ZCannotSaveException)ex).Type);
				}
				else
				{
					throw;
				}
			}

			AssertEquals("Should have thrown an exception.", true, exceptionThrown);
		}

		void TryChangeValueInSQL(DbConnection connection, ZGuid docketLinePK, string propertyName, IZType newValue, bool shouldFail, ZGuid secondDocketLinePK = new ZGuid())
		{
			// Create Query
			var sqlValue = ZSqlParameter.New("", newValue, ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(propertyName, WhsDocketLineSchema.Constants.TableName)).ParameterValueTextSql;
			var secondDocketLineSqlText = secondDocketLinePK.IsValid ? $", '{secondDocketLinePK}'" : "";
			var sqlText = $"UPDATE dbo.WhsDocketLine SET {propertyName} = {sqlValue}, WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' WHERE WE_PK in ('{docketLinePK}'{secondDocketLineSqlText})";

			connection.BeginTransaction();

			// disabling imbalance triggers to test sync trigger
			if (propertyName == WhsDocketLineSchema.Constants.WE_TransactionQuantity)
			{
				connection.ExecuteNonQuery($"disable trigger TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect on {WhsDocketLineSchema.Constants.TableName}");
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0", connection);
			}

			if (shouldFail)
			{
				NUnit.Framework.Assert.That(delegate
				{
					connection.ExecuteNonQuery(sqlText);
				}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "TriggerLikelyConcurrencyError: Attempt to desynchronise InterWhs Child!", true), $"Column: {propertyName}");
			}
			else
			{
				AssertNoExceptionThrown($"Column: {propertyName}", () => connection.ExecuteNonQuery(sqlText));
			}

			connection.RollbackTransaction();
		}

		void TryChangeValuesSimultaneouslyInSQL(DbConnection connection, ZGuid docketLinePK, Dictionary<ZPropertyInfo, IZType> propertyAndValuePairs, bool shouldFail, ZGuid secondDocketLinePK = new ZGuid())
		{
			// Create Query
			var sqlText = "UPDATE dbo.WhsDocketLine SET ";
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			foreach (var propertyAndValuePair in propertyAndValuePairs)
			{
				var sqlValue = ZSqlParameter.New("", propertyAndValuePair.Value, schemaResolver.GetSchemaColumn(propertyAndValuePair.Key.Name, WhsDocketLineSchema.Constants.TableName)).ParameterValueTextSql;
				var secondDocketLineSqlText = secondDocketLinePK.IsValid ? $", '{secondDocketLinePK}'" : "";
				if (propertyAndValuePair.Key != propertyAndValuePairs.Last().Key)
				{
					sqlText += $"{propertyAndValuePair.Key.Name} = '{propertyAndValuePair.Value}', ";
				}
				else
				{
					sqlText += $"{propertyAndValuePair.Key.Name} = '{propertyAndValuePair.Value}', WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' WHERE WE_PK in ('{docketLinePK}'{secondDocketLineSqlText})";
				}
			}

			connection.BeginTransaction();
			// disabling imbalance trigger to test sync trigger
			connection.ExecuteNonQuery($"disable trigger TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect on {WhsDocketLineSchema.Constants.TableName}");

			if (shouldFail)
			{
				NUnit.Framework.Assert.That(delegate
				{
					connection.ExecuteNonQuery(sqlText);
				}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "TriggerLikelyConcurrencyError: Attempt to desynchronise InterWhs Child!", true), "Multiple columns");
			}
			else
			{
				AssertNoExceptionThrown("Multiple columns", () => connection.ExecuteNonQuery(sqlText));
			}

			connection.RollbackTransaction();
		}

		void TryChangeValueInSQL_AfterChangingUnrelatedField(DbConnection connection, ZGuid docketLinePK, string propertyName, IZType newValue)
		{
			AssertNotEquals("Precondition: This test relies on the fact that WE_PalletID is a synchronised field. Modify the test if it becomes one.", WhsDocketLineSchema.Constants.WE_PalletID, propertyName);

			connection.BeginTransaction();

			// Put the lines out of sync - i.e. existing bad data
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised ON WhsDocketLine;");
			WhsDocketLineDO.UpdateWhere(docketLinePK.ToGuid()).Set(l => l.WE_PalletID, "BAD").Post(connection);
			connection.ExecuteNonQuery("ENABLE TRIGGER TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised ON WhsDocketLine;");

			TryChangeValueInSQL(connection: connection, docketLinePK: docketLinePK, propertyName: propertyName, newValue: newValue, shouldFail: false);

			// Assert trigger was enabled for the previous assertion
			TryChangeValueInSQL(connection: connection, docketLinePK: docketLinePK, propertyName: WhsDocketLineSchema.Constants.WE_PalletID, newValue: (ZString)"BAD2", shouldFail: true);

			connection.RollbackTransaction();
		}

		void TryChangeValueInBizo_AfterChangingUnrelatedField(DbConnection connection, ZGuid docketLinePK, ZPropertyInfo property, IZType newValue)
		{
			AssertNotEquals("Precondition: This test relies on the fact that WE_PalletID is a synchronised field. Modify the test if it becomes one.", WhsDocketLineSchema.Constants.WE_PalletID, property.Name);

			connection.BeginTransaction();

			// Put the lines out of sync - i.e. create existing bad data
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised ON WhsDocketLine;");
			WhsDocketLineDO.UpdateWhere(docketLinePK.ToGuid()).Set(l => l.WE_PalletID, "BAD").Post(connection);
			connection.ExecuteNonQuery("ENABLE TRIGGER TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised ON WhsDocketLine;");

			TryChangeValueInBizo(connection: connection, docketLinePK: docketLinePK, property: property, newValue: newValue, shouldFail: false);

			// Assert trigger was enabled for the previous assertion
			TryChangeValueInBizo(connection: connection, docketLinePK: docketLinePK, property: ((WhsTransferLine)property.BizObj).WE_PalletIDInfo, newValue: (ZString)"BAD2", shouldFail: true);

			connection.RollbackTransaction();
		}

		HashSet<ZString> GetPropertiesThatRequireFinalisation()
		{
			// These fields cannot be changed until after finalisation
			return new HashSet<ZString>
			{
				WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus,
				WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus,
				WhsDocketLineSchema.Constants.WE_FinalisedDate,
				WhsDocketLineSchema.Constants.WE_PutawayTime,
				WhsDocketLineSchema.Constants.WE_WPL_PutawayLine
			};
		}

		HashSet<ZString> GetPropertiesThatShouldBeTestedElsewhere()
		{
			// These fields affect how the trigger finds child lines and as such are beyond the scope of the test
			// These should be tested individually in separate tests where possible (i.e. PK can't really be)
			return new HashSet<ZString>
			{
				WhsDocketLineSchema.Constants.PK,
				WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine,
				WhsDocketLineSchema.Constants.WE_IsOriginalInventory,
				WhsDocketLineSchema.Constants.WE_DocketLineType, // cannot be changed after saving anyway
				WhsDocketLineSchema.Constants.WE_StockOnHand, // will only be present on Destination transfer + will be handled by imbalance triggers
				WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc, // Irrelevant, system column value
				WhsDocketLineSchema.Constants.WE_SystemCreateUser, // Irrelevant, system column value
				WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc, // Irrelevant, system column value
				WhsDocketLineSchema.Constants.WE_SystemLastEditUser, // Irrelevant, system column value
				WhsDocketLineSchema.Constants.WE_WB_CustomsData,
				WhsDocketLineSchema.Constants.WE_P9_Task,
			};
		}

		HashSet<ZString> GetPropertiesThatHaveConflictingConstraints()
		{
			// These fields have conflicting constraints
			return new HashSet<ZString>
			{
				// Due to [Constraint_OrderSpecificFields] these fields can never be set if the line is not 'ORD' or 'WOR'
				WhsDocketLineSchema.Constants.WE_RecommendedUnitPrice,
				WhsDocketLineSchema.Constants.WE_RX_NKUnitPriceCurrency,
				WhsDocketLineSchema.Constants.WE_UnitDiscountPercent,
				WhsDocketLineSchema.Constants.WE_UnitDiscountAmount,
				WhsDocketLineSchema.Constants.WE_UnitPriceAfterDiscount,
				WhsDocketLineSchema.Constants.WE_ExtendedLinePrice,

				// Due to [Constraint_WE_PickGroupIsOnlyUsedByOrders] this field can never be set if the line is not 'ORD'
				WhsDocketLineSchema.Constants.WE_PickGroup,

				// Due to [Constraint_WE_AllocationKey] this field can never be set if the line is not 'ORD'/'INW'
				WhsDocketLineSchema.Constants.WE_AllocationKey,

				// Due to [Constraint_OrderedHoldCodeOnlySetForOrders] this field can never be set if the line is not 'ORD'
				WhsDocketLineSchema.Constants.WE_WHC_NKOrderedHeldCode,

				// Due to [Constraint_CurrentHoldCodeEqualOriginalIfNotFinalised] these are tested seperately in TestTrigger_UnsychronisedHoldCodes()
				WhsDocketLineSchema.Constants.WE_WHC_NKCurrentInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode,

				// Due to [Constraint_WE_CurrentHoldReason] this field cannot be set without a current hold code set
				WhsDocketLineSchema.Constants.WE_CurrentHoldReason,

				// Due to [Constraint_WE_GS_NKUnloadedBy] The WE_UnloadedTime cannot be set without setting the WE_GS_NKUnloadedBy, and vice versa (Tested separately in this file)
				WhsDocketLineSchema.Constants.WE_UnloadedTime,
				WhsDocketLineSchema.Constants.WE_GS_NKUnloadedBy,
			};
		}

		HashSet<ZString> GetPropertiesNotNullInPractice()
		{
			// These fields are effectively not nullable for relevant data, so should be excluded from the null tests
			return new HashSet<ZString>
			{
				WhsDocketLineSchema.Constants.WE_WL_TransferFrom, // Due to not null check in [Constraint_WE_CurrentInventoryStatus]
				WhsDocketLineSchema.Constants.WE_FinalisedDate, // Due to [Constraint_WE_FinalisedDate] and fact we are checking DocketLineStatus is synchronised
				WhsDocketLineSchema.Constants.WE_PutawayTime, // Due to other trigger "Attempt to save Unputaway Finalised Transfer Line"
				WhsDocketLineSchema.Constants.WE_UnloadedTime
			};
		}

		HashSet<ZString> GetPropertiesThatDoNotHaveToBeSynchronised()
		{
			// NOTE: Do not add to this list without carefully considering whether the property should be synchronised
			return new HashSet<ZString>
			{
				WhsDocketLineSchema.Constants.WE_WD, // PK, will be for a different docket
				WhsDocketLineSchema.Constants.WE_WE_MatchingLine, // PK will be for a different line
				WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, // This is how we find parents/dockets!
				WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating, // PK, could be for a different docket
				WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus, // Can be modified after finalisation by Hold Code change
				WhsDocketLineSchema.Constants.WE_LineNo, // Irrelevant, child could have different amount of lines
				WhsDocketLineSchema.Constants.WE_SubLineNo, // Irrelevant, child could have different amount of lines
				WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate, // Does not get synced, and can be set *only* on dest when creating stock
				WhsDocketLineSchema.Constants.WE_WPL_PutawayLine, // Does not get synced, and can be set *only* on dest when creating stock
				WhsDocketLineSchema.Constants.WE_P9_Task, // Does not get synced, Inter-Whs is also out of scope for TaskManagement currently
			};
		}

		Dictionary<ZString, IZType> GetValuesToReplace(WhsTransferLine line, WhsTestHelperFunctions helper)
		{
			var newProduct = helper.CreateProduct(line.Docket.Client, "2");
			var newTransfer = helper.CreateWhsTransfer(line.Docket.Client, line.Warehouse, "T2");
			var putawayJob = Helper.CreateWhsPutawayJob(line.Warehouse, GlbStaff.CurrentUser);
			var putawayLine = Helper.CreateWhsPutawayLine(putawayJob, "PLT");

			var dictionary = new Dictionary<ZString, IZType>();
			dictionary.Add(WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus, (ZString)InventoryStatus.Codes.Putaway);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus, (ZString)InventoryStatus.Codes.Putaway);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_DocketLineStatus, (ZString)DocketLineStatus.Codes.Entered);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_TransactionQuantity, new ZDecimal(15m));
			dictionary.Add(WhsDocketLineSchema.Constants.WE_WL, line.Warehouse.WW_DefaultOutboundDockDoor);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_WL_TransferFrom, line.TransferFromWarehouse.WW_DefaultOutboundDockDoor);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_OP, newProduct.PK);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_WD, newTransfer.PK);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_WPL_PutawayLine, putawayLine.PK);

			var newTransferLine = (WhsTransferLine)line.Clone();
			newTransferLine.WE_WD = line.WE_WD;
			newTransferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			dictionary.Add(WhsDocketLineSchema.Constants.WE_WE_MatchingLine, newTransferLine.PK);
			dictionary.Add(WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating, newTransferLine.PK);
			newTransferLine.RunPreSaveValidation(); // to commit inventory
			line.Factory.Save();

			return dictionary;
		}

		static IZType GetDefaultValueToReplace(Type propertyType)
		{
			if (propertyType == typeof(ZGuid))
			{
				throw new ArgumentException("GUID properties must be handled specially!");
			}

			IZType valueToReplace;
			if (!DefaultValuesToReplace.TryGetValue(propertyType, out valueToReplace))
			{
				throw new ArgumentException($"Property type not supported: {propertyType.Name}.");
			}

			return valueToReplace;
		}

		static Dictionary<Type, IZType> DefaultValuesToReplace
		{
			get
			{
				if (defaultValuesToReplace == null)
				{
					defaultValuesToReplace = new Dictionary<Type, IZType>();
					defaultValuesToReplace[typeof(ZString)] = new ZString("ABC");
					defaultValuesToReplace[typeof(ZInt)] = new ZInt(2);
					defaultValuesToReplace[typeof(ZShort)] = new ZShort(3);
					defaultValuesToReplace[typeof(ZDecimal)] = new ZDecimal(1.1m);
					defaultValuesToReplace[typeof(ZBool)] = ZBool.True;
					defaultValuesToReplace[typeof(ZDateTime)] = ZDateTime.Today.AddDays(1);
					defaultValuesToReplace[typeof(ZDate)] = ZDate.Today.AddDays(1);
					defaultValuesToReplace[typeof(ZDateTimeOffset)] = ZDateTimeOffset.Today.AddDays(1);
				}

				return defaultValuesToReplace;
			}
		}

		[ThreadStatic]
		static Dictionary<Type, IZType> defaultValuesToReplace;

		#endregion

		#region TestTrigger_UnsynchronisedChildForCustomsData

		public void TestTrigger_UnsynchronisedChildForCustomsData()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			using (WarehouseDataRegistry.Instance.EnableImprovedStorageOfCustomsData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1");
				transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
				var parentTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
				parentTransferLine.RunPreSaveValidation();
				AssertEquals("Precondition", 10m, parentTransferLine.GetQtyCommittedIncludingMatchingLines());

				parentTransferLine.PickedTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, parentTransferLine.WE_DocketLineStatus);

				var childTransferLine = parentTransferLine.ChildTransferLine;
				AssertNotNull("Precondition: Child Line created.", childTransferLine);
				AssertEquals("Precondition: Not yet saved to database.", false, childTransferLine.IsInDatabase);

				var attribute = Factory.NewWithValidTestData<WhsBondedWarehouseAttribute>();
				attribute.SetParent(childTransferLine);

				childTransferLine.WE_WB_CustomsData = attribute.PK;

				AssertNoExceptionThrown(Factory.Save);
			}
		}

		#endregion

		#region Implementation

		void SuspendOtherDeferrableTriggers(DbConnection connection)
		{
			connection.ExecuteNonQuery(@"
				DECLARE @sql VARCHAR(MAX)
				SET @SQL = ''
				
				SELECT @sql = @sql + 'EXEC dbo.SuspendTrigger ''' + TriggerName + '''
				'
				FROM
					dbo.SuspendedTriggers
				WHERE
					TriggerName != 'TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised'

				EXEC(@sql)");
		}

		#region Factory

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
