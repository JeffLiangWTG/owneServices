using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsJobServiceValidationTest : WhsTestCaseWithFactory
	{
		public void TestCheckES_ServiceCode()
		{
			var jobService = Factory.New<WhsJobService>();
			jobService.ES_ServiceCode = "STG";
			AssertHasError(jobService.ES_ServiceCodeInfo, "STG code is reserved to calculate storage charges in periodic billing. Please choose another code.");

			jobService.ES_ServiceCode = "STS";
			AssertNoErrors(jobService.ES_ServiceCodeInfo);
		}

		#region TestCheckES_Completed

		public void TestCheckES_Completed_VASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var notify = new Mock<INotifications>();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(notify.Object);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(notify.Object);
			Factory.Save();

			TestCheckES_Completed_Core(vasOrder, (notifications, confirmComfirmation) => vasOrder.FinaliseVASOrder(notifications, confirmComfirmation));
		}

		public void TestCheckES_Completed_AdHocService()
		{
			var parentJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			TestCheckES_Completed_Core(parentJob, (notifications, confirmComfirmation) => parentJob.FinaliseAdHocServiceJob(notifications, confirmComfirmation));
		}

		void TestCheckES_Completed_Core(IHaveServices parentJob, Func<INotifications, bool, bool> finaliseFunc)
		{
			var jobService = Factory.New<WhsJobService>();
			jobService.ES_ParentID = parentJob.PK;
			jobService.ES_ParentTableCode = parentJob is WhsVASOrder ? WhsVASOrderSchema.Constants.Prefix : WhsAdHocServiceJobSchema.Constants.Prefix;
			jobService.ES_Completed = ZDateTime.Empty;
			jobService.ES_ServiceCode = "CHO";
			Factory.Save();

			var notify = new Mock<INotifications>();
			finaliseFunc(notify.Object, false);
			AssertHasError(jobService.ES_CompletedDateTimeOffsetInfo, "Service Job should be Completed before Finalizing.");

			jobService.ES_Completed = ZDateTime.Now;
			Factory.Save();
			finaliseFunc(notify.Object, false);
			AssertNoErrors(jobService.ES_CompletedDateTimeOffsetInfo);
		}

		public void TestCheckES_Completed_Receive() => TestCheckES_Completed_OtherDocketsCore(Factory.New<WhsReceive>());
		public void TestCheckES_Completed_Order() => TestCheckES_Completed_OtherDocketsCore(Factory.New<WhsOrder>());
		public void TestCheckES_Completed_WorkOrder() => TestCheckES_Completed_OtherDocketsCore(Factory.New<WhsWorkOrder>());
		public void TestCheckES_Completed_DynamicWorkOrder() => TestCheckES_Completed_OtherDocketsCore(Factory.New<WhsDynamicWorkOrder>());

		void TestCheckES_Completed_OtherDocketsCore(WhsDocket parentJob)
		{
			var jobService = Factory.New<WhsJobService>();
			jobService.Parent = parentJob;
			jobService.ES_Completed = ZDateTime.Empty;

			var notify = new Mock<INotifications>();
			parentJob.FinaliseDocket();
			AssertNoErrors(jobService.ES_CompletedDateTimeOffsetInfo);

			jobService.ES_Completed = ZDateTime.Now;
			parentJob.FinaliseDocket();
			AssertNoErrors(jobService.ES_CompletedDateTimeOffsetInfo);
		}

		#endregion
	}
}
