using System;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Packing.ServiceTasks.Testing
{
	public class DeletePackingFountainForFinalizedPackingJobsProcessingManagerTest : TestCaseWithFactory
	{
		public void TestGetFinalizedPackingParentJobs_WarehouseOrder()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order1 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1, finalizedDate: DateTime.Today.AddDays(-2));
			var packageJob1 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order1.PK, "WD", "JOB1");

			var maxFinalizedDate = DateTime.Today.AddDays(-1);
			var order2 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", isFinalised: true, index: 2, finalizedDate: maxFinalizedDate);
			var packageJob2 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order2.PK, "WD", "JOB2");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName1", packageJob1.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName2", packageJob2.PK, TestConnection);

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, 5, new CancellationToken());
				AssertEquals("Information|Successfully deleted 2 Packing Fountain(s).", logger.ToString().Trim());

				AssertEquals("LastPackingFountainsDeleteTimeUtc updated to last max packing job completed date.", maxFinalizedDate, PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.Value);
			}

			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob1.PK, "GeneratorFountain-PKGID-SomeName1", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob2.PK, "GeneratorFountain-PKGID-SomeName2", TestConnection));
		}

		public void TestGetFinalizedPackingParentJobs_WarehouseOrder_MultipleNumberFountainsOnSameOwner()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var maxFinalizedDate = DateTime.Today.AddDays(-1);
			var order = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", isFinalised: true, index: 2, finalizedDate: maxFinalizedDate);
			var packageJob = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB2");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName1", packageJob.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName2", packageJob.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName3", packageJob.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName4", packageJob.PK, TestConnection);

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, 5, new CancellationToken());
				AssertEquals("Information|Successfully deleted 4 Packing Fountain(s).", logger.ToString().Trim());

				AssertEquals("LastPackingFountainsDeleteTimeUtc updated to last max packing job completed date.", maxFinalizedDate, PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.Value);
			}

			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob.PK, "GeneratorFountain-PKGID-SomeName1", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob.PK, "GeneratorFountain-PKGID-SomeName2", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob.PK, "GeneratorFountain-PKGID-SomeName3", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob.PK, "GeneratorFountain-PKGID-SomeName4", TestConnection));
		}

		public void TestGetFinalizedPackingParentJobs_WarehouseOrder_NotFinalized()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", index: 1);
			var packageJob = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName1", packageJob.PK, TestConnection);

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, 5, new CancellationToken());
				AssertEquals("Information|No Packing Fountains were deleted.", logger.ToString().Trim());
			}

			AssertEquals("Number fountain not deleted.", 1, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob.PK, "GeneratorFountain-PKGID-SomeName1", TestConnection));
		}

		public void TestGetFinalizedPackingParentJobs_ExcludesFinalizedPackageJobsBeforeLastRun()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var oldJob = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, finalizedDate: DateTime.UtcNow.AddDays(-10), index: 1);
			var packageJobOld = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, oldJob.PK, "WD", "JOB1");

			var newJob = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", isFinalised: true, index: 2);
			var packageJobNew = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, newJob.PK, "WD", "JOB2");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName1", packageJobOld.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName2", packageJobNew.PK, TestConnection);

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, 5, new CancellationToken());
				AssertEquals("Information|Successfully deleted 1 Packing Fountain(s).", logger.ToString().Trim());
			}

			AssertEquals("Number fountain not deleted.", 1, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJobOld.PK, "GeneratorFountain-PKGID-SomeName1", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJobNew.PK, "GeneratorFountain-PKGID-SomeName2", TestConnection));
		}

		public void TestDeletePackingFountainForFinalizedPackingJobs_Batching()
		{
			var batchSize = 3;

			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order1 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1, finalizedDate: DateTime.UtcNow.AddDays(-7));
			var packageJob1 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order1.PK, "WD", "JOB1");

			var order2 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", isFinalised: true, index: 2, finalizedDate: DateTime.UtcNow.AddDays(-6));
			var packageJob2 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order2.PK, "WD", "JOB2");

			var order3 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000003", "O00000003", isFinalised: true, index: 3, finalizedDate: DateTime.UtcNow.AddDays(-5));
			var packageJob3 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order3.PK, "WD", "JOB3");

			var order4 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000004", "O00000004", isFinalised: true, index: 4, finalizedDate: DateTime.UtcNow.AddDays(-4));
			var packageJob4 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order4.PK, "WD", "JOB4");

			var order5 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000005", "O00000005", isFinalised: true, index: 5, finalizedDate: DateTime.UtcNow.AddDays(-3));
			var packageJob5 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order5.PK, "WD", "JOB5");

			var order6 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000006", "O00000006", isFinalised: true, index: 6, finalizedDate: DateTime.UtcNow.AddDays(-2));
			var packageJob6 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order6.PK, "WD", "JOB6");

			var order7 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000007", "O00000007", isFinalised: true, index: 7, finalizedDate: DateTime.UtcNow.AddDays(-1));
			var packageJob7 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order7.PK, "WD", "JOB7");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName1", packageJob1.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName2", packageJob2.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName3", packageJob3.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName4", packageJob4.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName5", packageJob5.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName6", packageJob6.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName7", packageJob7.PK, TestConnection);

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-10)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			using (TestConnection.TrackExecutedCommands())
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, batchSize, new CancellationToken());
				AssertEquals("Information|Successfully deleted 7 Packing Fountain(s).", logger.ToString().Trim());

				var commands = TestConnection.ExecutedCommands;
				AssertEquals("There were 7 commands (4 DELETE + 3 StmData update for last run registry).", 7, commands.Count());
				AssertEquals("There were 4 DELETE commands.", 4, commands.Count(command => command.Contains("DELETE")));
			}

			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob1.PK, "GeneratorFountain-PKGID-SomeName1", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob2.PK, "GeneratorFountain-PKGID-SomeName2", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob3.PK, "GeneratorFountain-PKGID-SomeName3", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob4.PK, "GeneratorFountain-PKGID-SomeName4", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob5.PK, "GeneratorFountain-PKGID-SomeName5", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob6.PK, "GeneratorFountain-PKGID-SomeName6", TestConnection));
			AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob7.PK, "GeneratorFountain-PKGID-SomeName7", TestConnection));
		}

		public void TestDeletePackingFountainForFinalizedPackingJobs_NotPackingFountain()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1);
			var packageJob = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("SomeFountain-SomeName1", packageJob.PK, TestConnection);

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-3)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, 3, new CancellationToken());
				AssertEquals("Information|No Packing Fountains were deleted.", logger.ToString().Trim());

				AssertEquals("Number fountain not deleted.", 1, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob.PK, "SomeFountain-SomeName1", TestConnection));
			}
		}

		public void TestGetFinalizedPackingParentJobs_WithFinalizedPackingJobButNothingDeleted()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var finalizedDate = DateTime.Today.AddDays(-2);
			var order = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1, finalizedDate: finalizedDate);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-5)))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var processingManager = new DeletePackingFountainForFinalizedPackingJobsProcessingManager();
				processingManager.DeletePackingFountainsForFinalizedPackingJobs(logger, 5, new CancellationToken());
				AssertEquals("Information|No Packing Fountains were deleted.", logger.ToString().Trim());
				AssertEquals("LastPackingFountainsDeleteTimeUtc updated to last max packing job completed date.", finalizedDate, PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.Value);
			}
		}

		public void TestDeletePackingFountainForFinalizedPackingJobs_SqlExceptions()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order.PK, "WD", "JOB1");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			TestConnection.ExecuteNonQuery("DROP VIEW ViewStmNums");
			TestConnection.ExecuteNonQuery("DROP TABLE StmNumberCache");
			TestConnection.ExecuteNonQuery("DROP TABLE StmNums");

			var lastRun = DateTime.Today.AddDays(-10);
			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lastRun))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				AssertExceptionThrown<SqlException>(() => new DeletePackingFountainForFinalizedPackingJobsProcessingManager().DeletePackingFountainsForFinalizedPackingJobs(logger, 1, new CancellationToken()));
				AssertEquals("Registry value did not get updated.", lastRun, PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.Value);
			}
		}

		public void TestDeletePackingFountainForFinalizedPackingJobs_DoesNotUpdateLastPackingFountainsDeleteTimeUtcRegistryWhenNoNewFinalizedPackingJobs()
		{
			var lastRun = DateTime.Today.AddDays(-10);
			using (PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lastRun))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				new DeletePackingFountainForFinalizedPackingJobsProcessingManager().DeletePackingFountainsForFinalizedPackingJobs(logger, 1, new CancellationToken());
				AssertEquals("Information|No Packing Fountains were deleted.", logger.ToString().Trim());

				AssertEquals("Registry value did not get updated.", lastRun, PackingRegistry.Instance.LastPackingFountainsDeleteTimeUtc.Value);
			}
		}
	}
}
