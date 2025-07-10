using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveTestFinaliseReceiveWithConcurrencyProblems : TestCase
	{
		#region TestFinaliseReceive_LinesAddedBeforeSave

		// WI00049628, defect could not be reproduced in the test using business objects
		[UseSnapshotProtection]
		public void TestFinaliseReceive_LinesAddedBeforeSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var localFactory = new BusinessObjectFactory();
			localFactory.RefreshEnabled = false;
			var localHelper = new WhsTestHelperFunctions(localFactory);
			var loadedReceive = localFactory.Load<WhsReceive>(receive.PK);

			OrgHeader org = localHelper.CreateClient();
			OrgSupplierPart part = localHelper.CreateProduct(org, "AAA");

			WhsInventoryView receiveLine1 = localHelper.CreateWhsReceiveInventoryLine(loadedReceive, part, 10m);
			receiveLine1.WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Held;

			loadedReceive.AllocateLocationsWithMock();
			localFactory.Save();

			AssertExceptionThrown(
				"Should have thrown a save exception as receipt has been modified while finalizing.",
				typeof(ZSaveConcurrencyException), () => Factory.Save());
		}

		#endregion

		#region TestFinaliseReceive_LinesModifiedAfterSave

		[UseSnapshotProtection]
		public void TestFinaliseReceive_LinesModifiedAfterSave()
		{
			var receive = GetUnfinalisedReceiveThatWasFinalisedAndSavedOnAnotherConnection();
			AssertEquals("Precondition", false, receive.IsFinalised);

			receive.Inventory[0].WI_TotalUnits = 99;
			AssertExceptionThrown("Should have thrown a save exception as receipt has been finalised.",
				typeof(ZSaveConcurrencyException), () => Factory.Save());
		}

		#endregion

		#region TestFinaliseReceive_WithNewlyAddedReceiveLines

		[UseSnapshotProtection]
		public void TestFinaliseReceive_WithNewlyAddedReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.RefreshEnabled = false;
			Factory.Save();
			AssertEquals("Precondition", false, receive.IsFinalised);

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory2 = new BusinessObjectFactory(secondConnection);
				factory2.RefreshEnabled = false;
				var receiveInFactory2 = factory2.Load<WhsReceive>(receive.PK);
				receiveInFactory2.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receiveInFactory2);
				factory2.Save();

				AssertEquals("Precondition", false, receive.IsFinalised);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				AssertExceptionThrown("Should have thrown a save exception as receipt has been finalised.",
					typeof(ZSaveConcurrencyException), () => Factory.Save());
			}
		}

		#endregion

		#region TestFinaliseReceive_DocketChanged

		[TestDate(2013, 11, 15)]
		[UseSnapshotProtection]
		public void TestFinaliseReceive_DocketChanged()
		{
			TestFinaliseReceive_SaveError(WhsDocketDO.UpdateWhere(d => true)
				.Set(d => d.WD_ArrivalDate, new DateTime(2013, 11, 16, 11, 11, 0)).AsSQL());
		}

		void TestFinaliseReceive_SaveError(string sqlQuery)
		{
			TestFinaliseReceive_Error(sqlQuery, typeof(ZSaveException));
		}

		#endregion

		#region TestFinaliseReceive_ArrivalDateModifiedBeforeSave

		[UseSnapshotProtection]
		public void TestFinaliseReceive_ArrivalDateModifiedBeforeSave()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocket_SystemLastEditAuditInfoMustBeUpdated_Update ON WhsDocket;");
				TestFinaliseReceive_ConcurrencyError(@"UPDATE dbo.WhsDocket SET WD_ArrivalDate = '2011-11-11 11:11'");
			}
		}

		#endregion

		#region TestFinaliseReceive_ArrivalDateModifiedAfterSave

		[UseSnapshotProtection]
		public void TestFinaliseReceive_ArrivalDateModifiedAfterSave_DateInPast()
		{
			TestFinaliseReceive_ArrivalDateModifiedAfterSave(ZDateTime.BrettsBirthday.ToOffset());
		}

		[UseSnapshotProtection]
		public void TestFinaliseReceive_ArrivalDateModifiedAfterSave_DateInFuture()
		{
			TestFinaliseReceive_ArrivalDateModifiedAfterSave(ZDateTimeOffset.Today.AddDays(1));
		}

		void TestFinaliseReceive_ArrivalDateModifiedAfterSave(ZDateTimeOffset date)
		{
			var receive = GetUnfinalisedReceiveThatWasFinalisedAndSavedOnAnotherConnection();
			receive.WD_ArrivalDate = date;
			AssertExceptionThrown("Should have thrown a save exception as receipt has been finalised.",
				typeof(ZSaveConcurrencyException), () => Factory.Save());
		}

		#endregion

		#region TestFinaliseReceive_BookingDateModifiedBeforeSave

		[UseSnapshotProtection]
		public void TestFinaliseReceive_BookingDateModifiedBeforeSave()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocket_SystemLastEditAuditInfoMustBeUpdated_Update ON WhsDocket;");
				TestFinaliseReceive_SaveError(@"UPDATE dbo.WhsDocket SET WD_BookingDate = '2011-11-11 11:11'");
			}
		}

		#endregion

		#region TestFinaliseReceive_BookingDateModifiedAfterSave

		[UseSnapshotProtection]
		public void TestFinaliseReceive_BookingDateModifiedAfterSave_DateInPast()
		{
			TestFinaliseReceive_BookingDateModifiedAfterSave(ZDateTime.BrettsBirthday.ToOffset());
		}

		[UseSnapshotProtection]
		public void TestFinaliseReceive_BookingDateModifiedAfterSave_DateInFuture()
		{
			TestFinaliseReceive_BookingDateModifiedAfterSave(ZDateTimeOffset.Today.AddDays(1));
		}

		void TestFinaliseReceive_BookingDateModifiedAfterSave(ZDateTimeOffset date)
		{
			var receive = GetUnfinalisedReceiveThatWasFinalisedAndSavedOnAnotherConnection();
			receive.WD_BookingDate = date;
			AssertExceptionThrown("Should have thrown a save exception as receipt has been finalised.",
				typeof(ZSaveConcurrencyException), () => Factory.Save());
		}

		#endregion

		#region TestOtherPropertiesChangedTriggerConcurrencyErrors

		[UseSnapshotProtection]
		public void TestFinaliseReceive_DocketStatus()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocket_SystemLastEditAuditInfoMustBeUpdated_Update ON WhsDocket;");
				TestFinaliseReceive_ConcurrencyError(@"UPDATE dbo.WhsDocket SET WD_DocketStatus = 'ENT'");
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestFinaliseReceive_Warehouse()
		{
			var wh2 = Helper.CreateWarehouse("Wh2");
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.Save();

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				using (secondConnection.BeginTransactionWithManager())
				{
					NUnit.Framework.Assert.That(
						delegate
						{
							WhsDocketDO.UpdateWhere(receive.PK.ToGuid()).Set(w => w.WD_WW_Whs, wh2.PK.ToGuid())
								.Post(secondConnection);
						}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change docket subtype or warehouse for a job with captured lines.", true), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestFinaliseReceive_ExternalReference()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocket_SystemLastEditAuditInfoMustBeUpdated_Update ON WhsDocket;");
				TestFinaliseReceive_ConcurrencyError(@"UPDATE dbo.WhsDocket SET WD_ExternalReference = 'BLAAAAAAH'");
			}
		}

		#endregion

		#region GetUnfinalisedReceiveThatWasFinalisedAndSavedOnAnotherConnection

		WhsReceive GetUnfinalisedReceiveThatWasFinalisedAndSavedOnAnotherConnection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.RefreshEnabled = false;
			Factory.Save();

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory2 = new BusinessObjectFactory(secondConnection);
				factory2.RefreshEnabled = false;
				var receiveInFactory2 = factory2.Load<WhsReceive>(receive.PK);
				receiveInFactory2.FinaliseDocketWithoutUserConfirmation();
				factory2.Save();
			}

			return receive;
		}

		#endregion

		#region TestFinaliseReceive_ConcurrencyError

		void TestFinaliseReceive_ConcurrencyError(string sqlQuery)
		{
			TestFinaliseReceive_Error(sqlQuery, typeof(ZSaveConcurrencyException));
		}

		#endregion

		#region TestFinaliseReceive_Error

		void TestFinaliseReceive_Error(string sqlQuery, Type type)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.Save();

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

				using (secondConnection.BeginTransactionWithManager())
				{
					secondConnection.ExecuteScalar(sqlQuery);
					secondConnection.CommitTransaction();
					AssertExceptionThrown(
						"Should have thrown an exception as receipt has been modified while finalizing.", type,
						() => Factory.Save());
				}
			}
		}

		#endregion

		#region Implementation

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		public DbConnection TestConnection { get; private set; }

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
