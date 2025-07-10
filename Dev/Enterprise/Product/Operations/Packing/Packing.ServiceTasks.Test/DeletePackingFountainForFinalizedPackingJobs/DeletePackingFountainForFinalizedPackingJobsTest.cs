using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Packing.ServiceTasks.Testing
{
	[TestedType(typeof(DeletePackingFountainForFinalizedPackingJobs))]
	class DeletePackingFountainForFinalizedPackingJobsTest : ServiceTaskTestCase<DeletePackingFountainForFinalizedPackingJobs>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().SingleOrDefault();
			AssertEquals("Delete Packing Fountains Of Finalized Package Jobs", hostedServiceAttribute.Description);
			AssertEquals("PJF", hostedServiceAttribute.Code);
			AssertEquals("PKG", hostedServiceAttribute.Category);
			AssertEquals(true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals(typeof(DeletePackingFountainForFinalizedPackingJobs), hostedServiceAttribute.Type);
			AssertEquals("1hour", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("1day", hostedServiceAttribute.DefaultScheduleRunEvery);
			AssertEquals(true, hostedServiceAttribute.ActiveByDefault);
			AssertEquals(true, hostedServiceAttribute.IsMandatory);
		}

		public void TestServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new DeletePackingFountainForFinalizedPackingJobs { ServiceLogger = logger };
			serviceTask.RunTask();
			AssertEquals("logger.ToString()", "Information|No Packing Fountains were deleted.", logger.ToString().Trim());
		}

		public void TestServiceTask_BatchSize()
		{
			var deletePackingFountainForFinalizedPackingJobsProcessingManagerMock = new Mock<IDeletePackingFountainForFinalizedPackingJobsProcessingManager>();
			using (ObjectFactory.Substitute(deletePackingFountainForFinalizedPackingJobsProcessingManagerMock.Object))
			{
				var logger = new TestServiceLogger();
				var serviceTask = new DeletePackingFountainForFinalizedPackingJobs { ServiceLogger = logger };
				serviceTask.RunTask();

				deletePackingFountainForFinalizedPackingJobsProcessingManagerMock.Verify(mock => mock.DeletePackingFountainsForFinalizedPackingJobs(logger, 500, It.IsAny<CancellationToken>()));
				deletePackingFountainForFinalizedPackingJobsProcessingManagerMock.VerifyAll();

				Assert(true);
			}
		}

		public void TestServiceTask_WarehouseOrder_EndToEnd()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order1 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000001", "O00000001", isFinalised: true, index: 1);
			var packageJob1 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order1.PK, "WD", "JOB1");

			var order2 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupWhsOrderPackingParent(sql, client, whs, product.PK, location.PK, "O00000002", "O00000002", isFinalised: true, index: 2);
			var packageJob2 = DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupPackageWithPackingParent(sql, order2.PK, "WD", "JOB2");

			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_StockOnHandIsBalanced_Insert", WhsDocketLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsPickLine_StockOnHandIsBalanced", WhsPickLineSchema.Constants.TableName, TestConnection))
			using (DeletePackingFountainForFinalizedPackingJobsTestHelper.SuspendTrigger("TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect", WhsDocketLineSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName1", packageJob1.PK, TestConnection);
			DeletePackingFountainForFinalizedPackingJobsTestHelper.SetupNumberFountain("GeneratorFountain-PKGID-SomeName2", packageJob2.PK, TestConnection);

			using (EnvProxy.Instance.TemporaryServiceTaskContext("PJF", canRunInAnyBranch: true))
			{
				var logger = new TestServiceLogger();
				var serviceTask = new DeletePackingFountainForFinalizedPackingJobs { ServiceLogger = logger };
				serviceTask.RunTask();
				AssertEquals("Information|Successfully deleted 2 Packing Fountain(s).", logger.ToString().Trim());

				AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob1.PK, "GeneratorFountain-PKGID-SomeName1", TestConnection));
				AssertEquals("Number fountain deleted.", 0, DeletePackingFountainForFinalizedPackingJobsTestHelper.GetNumberFountain(packageJob2.PK, "GeneratorFountain-PKGID-SomeName2", TestConnection));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
