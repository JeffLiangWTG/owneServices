using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LocalClientJobHandlerTest : TestCaseWithFactory
	{
		#region TestInitializationMessage_WithAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobOff

		public void TestInitializationMessage_WithAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobOff()
		{
			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var localClientJob = Factory.New<DummyLocalClientJob>();
			var jobHandler = new LocalClientJobHandler(localClientJob);
			jobHandler.Initialize();
			AssertNull(localClientJob.JobHeader);
			AssertEquals(string.Format("The registry item [{0}] has been set so that billing jobs will only be created upon entry to the Billing tab.",
				accounting.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation), jobHandler.InitializationMessage);
			Assert(!jobHandler.IsClientJobHandlerEligibleToCreateJob);
			Assert(!jobHandler.HasJobCreationTriggered);
		}

		#endregion

		#region TestInitializationMessage_WithAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobOff_JobInactive

		public void TestInitializationMessage_WithAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobOff_JobInactive()
		{
			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var localClientJob = Factory.New<DummyLocalClientJob>();
			var job = new JobHeader.Loader(localClientJob).TryCreate();
			Factory.Save();

			job.MarkAsInactive();

			var jobHandler = new LocalClientJobHandler(localClientJob);
			jobHandler.Initialize();
			Assert("Job is cancelled", job.IsCancelled);
			AssertEquals(string.Format(@"The Job Invoicing Record has been created but is currently not active. 
Please click on 'Billing' tab or 'Job Invoicing' menu to activate the job.",
				accounting.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation), jobHandler.InitializationMessage);
			Assert(!jobHandler.IsClientJobHandlerEligibleToCreateJob);
			Assert(!jobHandler.HasJobCreationTriggered);
		}

		#endregion

		#region TestInitializationMessage_WithAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobOn

		public void TestInitializationMessage_WithAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobOn()
		{
			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var localClientJob = Factory.New<DummyLocalClientJob>();
			var support = localClientJob.InvoicingSupporter as DummyJobHeaderParentJobInvoicingSupporter;

			support.CreateAccountingJobOnSavingOfOperationsJob = false;
			var jobHandler = new LocalClientJobHandler(localClientJob);
			jobHandler.Initialize();
			AssertNull(localClientJob.JobHeader);
			AssertEquals("Automated creation of Job Header.", jobHandler.InitializationMessage);

			support.CreateAccountingJobOnSavingOfOperationsJob = true;
			jobHandler = new LocalClientJobHandler(localClientJob);
			jobHandler.Initialize();
			AssertNotNull(localClientJob.JobHeader);
			Assert(jobHandler.IsClientJobHandlerEligibleToCreateJob);
			Assert(jobHandler.HasJobCreationTriggered);
			AssertNull(jobHandler.InitializationMessage);
			localClientJob.JobHeader.Dispose(); // To Dispose the semaphor
		}

		#endregion

		#region TestInitializeMessage_WithTemplateRecord

		public void TestInitializeMessage_WithTemplateRecord()
		{
			var localClientJob = Factory.New<DummyLocalClientJob>();
			localClientJob.IsTemplateRecord = true;
			var jobHandler = new LocalClientJobHandler(localClientJob);
			jobHandler.Initialize();
			AssertNull(localClientJob.JobHeader);
			AssertEquals("The Billing tab will be available when a Record is created from this Template.", jobHandler.InitializationMessage);
			Assert(!jobHandler.IsClientJobHandlerEligibleToCreateJob);
			Assert(!jobHandler.HasJobCreationTriggered);
		}

		#endregion

		#region TestInitialize_CancelledLocalClientJob

		public void TestInitialize_CancelledLocalClientJob()
		{
			var localClientJob = Factory.New<DummyLocalClientJob>();
			localClientJob.IsCancelled = true;

			var jobHandler = new LocalClientJobHandler(localClientJob);
			jobHandler.Initialize();

			AssertNull(localClientJob.JobHeader);
			Assert(!jobHandler.IsClientJobHandlerEligibleToCreateJob);
			Assert(!jobHandler.HasJobCreationTriggered);
			AssertEquals("InitializationMessage is null for cancelled localClientJob", null, jobHandler.InitializationMessage);
		}

		#endregion
	}
}
