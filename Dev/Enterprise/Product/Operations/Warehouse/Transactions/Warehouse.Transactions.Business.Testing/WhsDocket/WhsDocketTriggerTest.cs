using System.Linq;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketTriggerTest<T> : WhsTestCaseWithFactory
	where T : WhsDocket
	{
		#region TestTG_FinalisedDocketWithFinalisedLine

		public void TestTG_FinalisedDocketWithFinalisedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			docket.RunPreSaveValidation();
			Factory.Save();

			CreateNewPick(docket);
			docket.FinaliseDocket();
			docket.RunPreSaveValidation();

			if (docket.WD_DocketType != DocketType.Codes.Transfer)
			{
				//Transfers docket and docketLine finalised dates can be different
				AssertEquals("Precondition: Finalised Date should be same", docket.WD_FinalisedDate, docketLine.WE_FinalisedDate);
			}

			AssertIsFinalisedPrecondition(docket);
			AssertIsFinalisedPrecondition(docketLine);
			AssertEquals("Precondition: Finalised Date of Docket should not be empty", false, docket.WD_FinalisedDate.IsEmpty);
			AssertEquals("Precondition: Finalised Date of Docket Line should not be empty", false, docketLine.WE_FinalisedDate.IsEmpty);

			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
		}

		protected virtual WhsPick CreateNewPick(WhsDocket docket)
		{
			return null;
		}

		#endregion

		#region TestTG_FinalisedDocketWithFinalisedLine_FinalisedDateAreDifferent

		public void TestTG_FinalisedDocketWithFinalisedLine_FinalisedDateAreDifferent()
		{
			// This unit test is used to test the trigger can prevent save, suspend the check trigger on WhsDocketLine to avoid Exception thrown.
			var connection = ((IDbConnected)Factory).Connection;
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_CheckDocketLineStatusAndDateForDocketLine ON WhsDocketLine");

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			docket.RunPreSaveValidation();
			Factory.Save();

			CreateNewPick(docket);
			docket.FinaliseDocket();
			docket.RunPreSaveValidation();

			AssertIsFinalisedPrecondition(docket);
			AssertIsFinalisedPrecondition(docketLine);
			if (docket.WD_DocketType != DocketType.Codes.Transfer)
			{
				//Transfers docket and docketLine finalised dates can be different
				AssertEquals("Precondition: Finalised Date should be same", docket.WD_FinalisedDate, docketLine.WE_FinalisedDate);
			}
			var commandSQL = $"SELECT Result FROM dbo.IsTriggerSuspended('TG_PreventMismatchOnDocketStatusAndDateWithLines')";
			AssertEquals("Trigger is not suspended, function should return 0.", 0, TestConnection.ExecuteScalar(commandSQL));
			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());

			if (docket.PropagatesFinalisedDateAndStatusToLines)
			{
				AssertEquals("Trigger is not suspended, function should return 0.", 0, TestConnection.ExecuteScalar(commandSQL));
				AssertExceptionThrown("Should have thrown an exception", typeof(SqlException), $"{WhsDocket.PreventFinalisedDocketWithUnFinalisedLines}\r\nThe transaction ended in the trigger. The batch has been aborted.",
					() => CargoWise.Database.TestFramework.ObjectModel.WhsDocket
						.UpdateWhere(docket.PK.ToGuid())
						.Set(l => l.WD_FinalisedDate, ZDateTime.Today.AddDays(-1).ToDateTime())
						.Post(TestConnection));
			}
			else
			{
				AssertNoExceptionThrown("Should have thrown an exception",
					() => CargoWise.Database.TestFramework.ObjectModel.WhsDocket
						.UpdateWhere(docket.PK.ToGuid())
						.Set(l => l.WD_FinalisedDate, ZDateTime.Today.AddDays(-1).ToDateTime())
						.Post(TestConnection));
			}
		}

		#endregion

		#region TestTG_FinalisedDocketWithUnFinalisedLine

		[ExpectNoExceptions]
		public void TestTG_FinalisedDocketWithUnFinalisedLine()
		{
			// This unit test is used to test the trigger can prevent save, suspend the check trigger on WhsDocketLine to avoid Exception thrown.
			var connection = ((IDbConnected)Factory).Connection;
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_CheckDocketLineStatusAndDateForDocketLine ON WhsDocketLine");

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			docket.RunPreSaveValidation();
			Factory.Save();

			CreateNewPick(docket);
			docket.WD_DocketStatus = StatusForFinalisedDocket;
			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			foreach (var line in GetAllLines(docket))
			{
				line.WE_DocketLineStatus = DefaultDocketLineStatus;
				line.WE_FinalisedDate = ZDateTimeOffset.Empty;
			}

			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocket.PreventFinalisedDocketWithUnFinalisedLines), "Should have thrown an exception");
		}

		#endregion

		#region TestTG_CancelledDocketWithUnCancelledLine

		[ExpectNoExceptions]
		public void TestTG_CancelledDocketWithUnCancelledLine()
		{
			if (!CanBeCancelled)
			{
				Assert("The docket can't be cancelled.", true);
			}
			else
			{
				// This unit test is used to test the trigger can prevent save, suspend the check trigger on WhsDocketLine to avoid Exception thrown.
				var connection = ((IDbConnected)Factory).Connection;
				connection.ExecuteNonQuery("DISABLE TRIGGER TG_CheckDocketLineStatusAndDateForDocketLine ON WhsDocketLine");
				connection.ExecuteNonQuery(@"
IF OBJECT_ID('Constraint_ReceiveStockOnHandEqualsTransactionQtyWhenNotFinalised', 'C') IS NOT NULL
	ALTER TABLE dbo.WhsDocketLine DROP CONSTRAINT Constraint_ReceiveStockOnHandEqualsTransactionQtyWhenNotFinalised
");
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var docket = GetNewDocket(data.Org1, data.Whs1);
				var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
				SetupDocketLine_CancelledDocketWithUnCancelledLine(docketLine);
				docket.RunPreSaveValidation();
				Factory.Save();

				docket.CancelReactivateDocket();
				docketLine.WE_DocketLineStatus = ZString.Empty;

				AssertEquals("Docket is cancelled", "CAN", docket.WD_DocketStatus);
				AssertNotEquals("DocketLine is not cancelled", "CAN", docketLine.WE_DocketLineStatus);
				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocket.PreventSaveOnMismatchCanacelledStatusOnDocketWithLines), "Should have thrown an exception");
			}
		}

		protected virtual void SetupDocketLine_CancelledDocketWithUnCancelledLine(WhsDocketLine line) { }

		#endregion

		#region TestTG_CancelledDocketWithUnCancelledLine

		[ExpectNoExceptions]
		public void TestTG_UnCancelledDocketWithCancelledLine()
		{
			if (!CanBeCancelled)
			{
				Assert("The docket can't be cancelled.", true);
			}
			else
			{
				// This unit test is used to test the trigger can prevent save, suspend the check trigger on WhsDocketLine to avoid Exception thrown.
				var connection = ((IDbConnected)Factory).Connection;
				connection.ExecuteNonQuery("DISABLE TRIGGER TG_CheckDocketLineStatusAndDateForDocketLine ON WhsDocketLine");

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var docket = GetNewDocket(data.Org1, data.Whs1);
				var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
				SetupDocketLine_CancelledDocketWithUnCancelledLine(docketLine);
				docket.RunPreSaveValidation();
				Factory.Save();

				docket.CancelReactivateDocket();
				AssertEquals("Docket is cancelled", "CAN", docket.WD_DocketStatus);

				AssertNoExceptionThrown("Should not have thrown an exception", () => Factory.Save());

				docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				docketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
				docketLine.WE_StockOnHand = 0m;

				AssertNotEquals("Docket is not cancelled", "CAN", docket.WD_DocketStatus);
				AssertEquals("DocketLine is cancelled", "CAN", docketLine.WE_DocketLineStatus);

				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocket.PreventSaveOnMismatchCanacelledStatusOnDocketWithLines), "Should have thrown an exception");
			}
		}

		#endregion

		#region TestUpdatingWD_DocketTypeFails

		public void TestUpdatingWD_DocketTypeFails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			Factory.Save();
			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			Factory.Save();

			var differentDocketType = new DocketType().Cast<ICodeDescription>().First(c => c.Code != docket.WD_DocketType).Code;

			AssertExceptionThrown("Trigger must be fired to prevent docket type being changed.", typeof(SqlException),
				() => TestConnection.ExecuteNonQuery(CargoWise.Database.TestFramework.ObjectModel.WhsDocket.UpdateWhere(docket.PK.ToGuid()).Set(l => l.WD_DocketType, differentDocketType).AsSQL()));
		}

		#endregion

		#region TestTG_WhsDocket_PreventDocketSubTypeChange

		[ExpectNoExceptions]
		public void TestTG_WhsDocket_PreventDocketSubTypeChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			Factory.Save();

			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			Factory.Save();

			docket.WD_DocketSubType = GetDocketSubTypeNotDefault();    // Set to a different subtype just need to be different from the default subtype.

			var expectedExceptionMessage = "Attempt to change docket subtype or warehouse for a job with captured lines.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMessage, true), "ZSaveException should have been thrown.");
			AssertEquals("Changes to Docket must not have been saved.", true, docket.HasChanges);
		}

		protected abstract string GetDocketSubTypeNotDefault();

		#endregion

		#region TestTG_WhsDocket_PreventClientChange

		[ExpectNoExceptions]
		public void TestTG_WhsDocket_PreventClientChange()
		{
			var otherClient = Helper.CreateClient("C2");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductClientRelationShip(otherClient, data.Part1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			Factory.Save();

			var originalClientPK = docket.WD_OH_Client;
			docket.WD_OH_Client = otherClient.PK;
			AssertNoExceptionThrown("Should allow changing client when there are no lines.", Factory.Save);

			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			Factory.Save();

			docket.WD_OH_Client = originalClientPK;
			var expectedExceptionMessage = "Attempt to change client for a job with captured lines.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMessage, true), "Should prevent changing Client on dockets that have line.");
		}

		#endregion

		#region TestTG_WhsDocket_PreventWarehouseChange

		[ExpectNoExceptions]
		public void TestTG_WhsDocket_PreventWarehouseChange()
		{
			TestTG_WhsDocket_PreventWarehouseChangeCore();
		}

		protected virtual void TestTG_WhsDocket_PreventWarehouseChangeCore()
		{
			var otherWhs = Helper.CreateWarehouse("WH2", "A", 1, 1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			Factory.Save();

			var originalWarehousePK = docket.WD_WW_Whs;
			docket.WD_WW_Whs = otherWhs.PK;
			AssertNoExceptionThrown("Should allow changing warehouse when there are no lines.", Factory.Save);

			var docketLine = GetNewDocketLine(docket, data.Part1, otherWhs.DefaultLocation);
			Factory.Save();

			docket.WD_WW_Whs = originalWarehousePK;
			var expectedExceptionMessage = "Attempt to change docket subtype or warehouse for a job with captured lines.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMessage, true), "Should prevent changing Warehouse on dockets that have line.");
		}

		#endregion

		#region GetNewDocket & GetNewDocketLine & GetAllLines

		protected abstract T GetNewDocket(OrgHeader client, WhsWarehouse warehouse);
		protected abstract WhsDocketLine GetNewDocketLine(T docket, OrgSupplierPart part, WhsLocation location);
		protected virtual WhsDocketLineCollection GetAllLines(T docket) => docket.Lines;
		protected virtual string StatusForFinalisedDocket => DocketStatus.Codes.Finalised;

		#endregion

		protected virtual bool CanBeCancelled => true;

		protected virtual ZString DefaultDocketLineStatus => "";
	}
}
