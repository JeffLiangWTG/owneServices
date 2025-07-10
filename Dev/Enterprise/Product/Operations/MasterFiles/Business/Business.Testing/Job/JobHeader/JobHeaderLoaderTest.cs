using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobHeader.Loader))]
	public class JobHeaderLoaderTest : LoaderTestCase
	{
		public void TestTryCreateWhenAllowedToCreateInvoicingJob()
		{
			((DummyJobHeaderParentJobInvoicingSupporter)((DummyJobHeaderParent)Parent).InvoicingSupporter).CanCreateInvoicingJob = false;
			AssertNull("TryCreate", Loader.TryCreate());
			AssertEquals("GetJobCreationErrorMessage", "Operational job 'JobNumber' does not support creation of invoicing job.", Loader.GetJobCreationError().Message);
			AssertEquals("GetJobCreationErrorMessage", "Operational job 'JobNumber' does not support creation of invoicing job.", Loader.GetJobCreationErrorForService().Message);

			((DummyJobHeaderParentJobInvoicingSupporter)((DummyJobHeaderParent)Parent).InvoicingSupporter).CanCreateInvoicingJob = true;
			AssertNotNull("TryCreate", Loader.TryCreate());
		}

		public void TestActivateJob()
		{
			var job = Loader.TryCreate();
			Factory.Save();

			job.JH_IsActive = true;
			AssertContains("Error message", "We should call MarkAsInactive or ActivateJob to modify JH_IsActive", ExceptionReporterTestListener.Instance[0].InnerException.Message);

			ErrorReporter.Clear();
			job.JH_AgentChargesCFX = 200M;
			job.JH_ARInvoiceReference = "testReference";
			job.JH_A_JCL = ZDateTime.Now;
			job.JH_Description = "TestJH_Description";
			job.JH_ExcludeFromPeriodicRating = true;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GS_NKRepOps = "A";
			job.JH_GS_NKRepSales = "E";
			job.JH_HeaderType = "TES";
			job.JH_HoldReason = "TestTest";
			job.JH_IsProfitSharePosted = true;
			job.JH_JobNum = "TestJH_JobNum";
			job.JH_JobPlannedStartDate = ZDateTime.Now;
			job.JH_LocalChargesCFX = 500M;
			job.JH_LocalClientInvoicingStyle = "Tes";
			job.JH_Name = "TestJH_Name";

			job.MarkAsInactive();
			Factory.Save();
			job.ActivateJob();

			AssertEquals((decimal)0, job.JH_AgentChargesCFX);
			AssertEquals("", job.JH_ARInvoiceReference);
			AssertEquals(ZDateTime.Empty, job.JH_A_JCL);
			AssertEquals("", job.JH_Description);
			AssertEquals(false, job.JH_ExcludeFromPeriodicRating);
			AssertEquals(Guid.Empty, job.JH_GB);
			AssertEquals(Guid.Empty, job.JH_GE);
			AssertEquals("E", job.JH_GS_NKRepOps);
			AssertEquals("", job.JH_GS_NKRepSales);
			AssertEquals("JOB", job.JH_HeaderType);
			AssertEquals("", job.JH_HoldReason);
			AssertEquals(false, job.JH_IsProfitSharePosted);
			AssertEquals("", job.JH_JobNum);
			AssertEquals(ZDateTime.Empty, job.JH_JobPlannedStartDate);
			AssertEquals((decimal)0, job.JH_LocalChargesCFX);
			AssertEquals("", job.JH_LocalClientInvoicingStyle);
			AssertEquals("", job.JH_Name);
			Assert(job.JH_IsActive);
			AssertNotEquals("JH_JobLocalReference should not be set to default.", "", job.JH_JobLocalReference);
			AssertNotEquals("JH_SystemCreateTimeUtc should not be set to default.", "", job.JH_SystemCreateTimeUtc);
			AssertNotEquals("JH_SystemCreateUser should not be set to default.", "", job.JH_SystemCreateUser);
			AssertEquals("Has No error", "", ErrorReporter.LastMessageReported);
		}

		public void TestTryCreateToActivateJobHeader()
		{
			var mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			var newLoader = new JobHeader.Loader((IJobHeaderParent)declaration);
			var jobHeader = newLoader.TryCreate();
			Factory.Save();

			jobHeader.MarkAsInactive();
			Factory.Save();

			jobHeader = newLoader.TryCreate();

			Assert(jobHeader.JH_IsActive);

			ErrorReporter.Clear();
		}

		public void TestTryCreateToActivateJobHeader_SuspendSettingHasChanges()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			var newLoader = new JobHeader.Loader((IJobHeaderParent)declaration);
			var jobHeader = newLoader.TryCreate();
			Factory.Save();

			jobHeader.MarkAsInactive();
			Factory.Save();

			Assert(!jobHeader.JH_IsActive);
			Assert(!jobHeader.HasChanges);

			using (newLoader.SetJobHasChangesSuspender.GetSuspender())
			{
				jobHeader = newLoader.TryCreate();
			}
			Assert(jobHeader.JH_IsActive);
			Assert(!jobHeader.HasChanges);

			ErrorReporter.Clear();
		}

		public void TestIsJobActivating()
		{
			var newJob = Loader.TryCreate();
			Factory.Save();

			Assert(!newJob.IsJobActivating);

			newJob.MarkAsInactive();
			Factory.Save();

			Assert(!newJob.IsJobActivating);

			newJob.ActivateJob();

			Assert(newJob.IsJobActivating);
		}

		public void TestLoad()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			JobHeader job = Loader.TryCreate();
			JobHeader decoyJob = Loader.TryCreate(branch);

			JobHeader loadedJob = Loader.Load();
			AssertEquals("Correct job loaded", job, loadedJob);

			loadedJob = Loader.Load(false, company);
			AssertEquals("Correct job loaded", decoyJob, loadedJob);
		}

		public void TestCreate()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			JobHeader newJob = Loader.TryCreate();
			AssertJobDefaults(newJob);
			AssertEquals("Mutex not acquired", false, Mutex.IsLocked);

			parent = Factory.New<DummyJobHeaderOnlyParent>();
			loader = null;
			newJob = Loader.TryCreate(branch);
			AssertJobDefaults(newJob, company, branch);
		}

		public void TestCreateWithMutex()
		{
			using (JobHeader newJob = Loader.TryCreateWithMutex())
			{
				AssertJobDefaults(newJob);

				ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);
				AssertEquals("Mutex acquired", true, mutex.IsLocked);
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestCreateWithMutex_WhenMutexAlreadyAquired()
		{
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);

			try
			{
				AssertEquals("Mutex acquired for the test", true, mutex.Lock());

				using (JobHeader newJob = Loader.TryCreateWithMutex())
				{
					AssertEquals("Job not created due as the mutex could not be acquired", null, newJob);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestActivateWithMutex_WhenMutexAlreadyAquired()
		{
			var newJob = Loader.TryCreate();
			Factory.Save();
			newJob.MarkAsInactive();
			Factory.Save();

			var mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);

			try
			{
				AssertEquals("Mutex acquired for the test", true, mutex.Lock());

				using (var job = Loader.TryLoadOrCreateWithMutex())
				{
					AssertNull(job);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
				ErrorReporter.Clear();
			}
		}

		public void TestLoadOrCreate()
		{
			var createdJob = Loader.TryLoadOrCreate();
			var loadedJob = Loader.TryLoadOrCreate();
			AssertEquals("Job loaded same as the job created", loadedJob, createdJob);
		}

		public void TestLoadOrCreateWithMutex()
		{
			using (JobHeader createdJob = Loader.TryLoadOrCreateWithMutex())
			using (JobHeader loadedJob = Loader.TryLoadOrCreateWithMutex())
			{
				AssertEquals("Job loaded same as the job created", loadedJob, createdJob);

				ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);
				AssertEquals("Mutex acquired", true, mutex.IsLocked);
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestLoadOrCreateWithMutex_WhenMutexAlreadyAquired()
		{
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);

			try
			{
				AssertEquals("Mutex acquired for the test", true, mutex.Lock());

				using (JobHeader newJob = Loader.TryLoadOrCreateWithMutex())
				{
					AssertEquals("Job not created due as the mutex could not be acquired", null, newJob);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestLoadSetsParent()
		{
			Loader.TryCreate();
			Factory.Save();
			var loadedJob = new JobHeader.Loader(new BusinessObjectFactory(), Parent).Load(true);
			AssertNotNull(loadedJob);
			AssertNotNull(loadedJob.Parent);

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			var newLoader = new JobHeader.Loader((IJobHeaderParent)declaration);
			newLoader.TryCreate();
			Factory.Save();

			loadedJob = new JobHeader.Loader(new BusinessObjectFactory(), (IJobHeaderParent)declaration).Load(true);
			AssertNotNull(loadedJob);
			AssertEquals(shipment, loadedJob.Parent);
		}

		public void TestGetJobDeleteErrorMessage()
		{
			Loader.TryCreate();
			Factory.Save();
			var loadedJob = new JobHeader.Loader(new BusinessObjectFactory(), Parent).Load(true);
			AssertNotNull(loadedJob);
			AssertNotNull(loadedJob.Parent);

			var errorMessage = Loader.GetJobDeleteErrorMessage();
			AssertEquals("Should have error message before marked job as inactive.", @"This record cannot be deleted.
An Invoicing Job Header (JobNumber) has been created in the company EDI.", errorMessage);

			loadedJob.MarkAsInactive();
			Factory.Save();

			errorMessage = Loader.GetJobDeleteErrorMessage();
			AssertEquals("Should have error message after marked job as inactive.", @"This record cannot be deleted.
An Invoicing Job Header (JobNumber) has been created in the company EDI.", errorMessage);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.China))
			{
				errorMessage = Loader.GetJobDeleteErrorMessage();
				AssertEquals("Should have error message after marked job as inactive.", @"This record cannot be deleted.
An Invoicing Job Header (JobNumber) has been created in the company EDI.", errorMessage);
			}
		}

		#region TestLoadWithParentExistingJob

		/*Please create JobHeader via switching user context*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoadJobHeaderFromTopParentWhenImmediateParentNotInDB()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var shipmentJob = Loader.TryCreate(branch);
			shipmentJob.Parent = shipment as IJobHeaderParent;
			shipmentJob.JH_ParentID = shipment.PK;
			shipmentJob.JH_ParentTableCode = "JS";
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_GC = company.PK;

			var consolidation = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			((IDtbBookingConsolidation)consolidation).KB_ParentID = shipment.PK;
			((IDtbBookingConsolidation)consolidation).KB_ParentTableCode = "JS";

			var booking = (BusinessObject)Factory.New<IDtbBooking>();
			((IDtbBooking)booking).KM_KB_Booking = ((IDtbBookingConsolidation)consolidation).PK;
			((IDtbBookingConsolidation)consolidation).Bookings.Append((IDtbBooking)booking);

			Factory.Save();

			var notInDatabaseBooking = (BusinessObject)Factory.New<IDtbBooking>();
			((IDtbBooking)notInDatabaseBooking).KM_KB_Booking = ((IDtbBookingConsolidation)consolidation).PK;
			((IDtbBookingConsolidation)consolidation).Bookings.Append((IDtbBooking)notInDatabaseBooking);

			var newFactory = Factory.CreateNewFactory();    // To make sure we don't have any cached jobHeaders.
			var bookingNotInDBLoader = new JobHeader.Loader(newFactory, (IJobHeaderParent)notInDatabaseBooking);
			var shipmentLoader = new JobHeader.Loader(newFactory, (IJobHeaderParent)shipment);
			var bookingLoader = new JobHeader.Loader(newFactory, (IJobHeaderParent)booking);

			var loadedJobFromParentNotInDB = bookingNotInDBLoader.Load(true, company);
			AssertNotNull("When loading on a clean factory for a booking not in DB it should return a valid job header.", loadedJobFromParentNotInDB);
			AssertEquals("When loading on a clean factory for a booking not in DB it should return the shipment job header.", shipmentJob.PK, loadedJobFromParentNotInDB.PK);
			AssertEquals("It should load job from the shipment.", shipmentJob.PK, shipmentLoader.Load(true, company).PK);
			AssertEquals("It should load job from the booking.", shipmentJob.PK, bookingLoader.Load(true, company).PK);
		}

		#endregion

		#region Implementation

		void AssertJobDefaults(JobHeader newJob)
		{
			AssertJobDefaults(newJob, GlbCompany.CurrentCompany, GlbBranch.CurrentBranch);
		}

		void AssertJobDefaults(JobHeader newJob, GlbCompany company, GlbBranch branch)
		{
			AssertEquals("JH_ParentID", Parent.PK, newJob.JH_ParentID);
			AssertEquals("JH_ParentTableCode", ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Parent.TableName), newJob.JH_ParentTableCode);
			AssertEquals("JH_GC", company.PK, newJob.JH_GC);
			AssertEquals("JH_GB", branch.PK, newJob.JH_GB);
		}

		JobHeader.Loader Loader
		{
			get { return loader ?? (loader = (JobHeader.Loader)GetNewLoaderToTest()); }
		}
		JobHeader.Loader loader;

		protected DummyJobHeaderOnlyParent Parent
		{
			get { return parent ?? (parent = Factory.New<DummyJobHeaderParent>()); }
		}
		DummyJobHeaderOnlyParent parent;

		ZGlobalMutex Mutex
		{
			get { return JobHeader.GetMutex_ForTestOnly(Parent.PK); }
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new JobHeader.Loader(Parent);
		}

		#endregion
	}
}
