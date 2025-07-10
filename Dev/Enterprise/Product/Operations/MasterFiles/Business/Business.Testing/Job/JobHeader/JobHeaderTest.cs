using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using static Enterprise.MasterFiles.Business.JobHeader.Loader;
using static Enterprise.MasterFiles.Business.Testing.AccountingAssertionHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoginToJobCompanyDuringRefreshParent()
		{
			JobHeader jobHeader;
			var currentCompany = GlbCompany.CurrentCompany;
			var testCompany = Factory.Load<GlbCompany>(new ZGuid("03052ED3-2C64-49AC-97D8-C6079D5015B5"));
			var branch = testCompany.Branches[0];
			AssertNotEquals("Companies are different", currentCompany, testCompany);

			AssertLoginToJobCompanyDuringRefreshParent();

			void AssertLoginToJobCompanyDuringRefreshParent()
			{
				jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_GC = testCompany.PK;
				jobHeader.JH_GB = branch.PK;
				jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

				AssertCurrentCompanyBeforeAndAfterJobRefreshParent(() => jobHeader.Parent = Parent);

				jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_GC = testCompany.PK;
				jobHeader.JH_GB = branch.PK;
				jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
				jobHeader.JH_ParentID = ZGuid.NewZGuid();
				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				using (jobHeader.RefreshJobParentSuspender.GetSuspender())
				{
					jobHeader.Parent = Parent;
				}

				AssertCurrentCompanyBeforeAndAfterJobRefreshParent(() => jobHeader.InitializeParentFromGenericJobWithSettingDefaults());
			}

			void AssertCurrentCompanyBeforeAndAfterJobRefreshParent(Action actionThatCallsRefreshParent)
			{
				AssertEquals("Current company", currentCompany, GlbCompany.CurrentCompany);
				Assert("Default values not assigned", !jobHeader.DefaultValuesHasBeenAssigned);
				actionThatCallsRefreshParent();
				AssertEquals("Current company", currentCompany, GlbCompany.CurrentCompany);
				Assert("Default values assigned", jobHeader.DefaultValuesHasBeenAssigned);
			}
		}

		public void TestIsDataVersionsAutoLogged()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>() as IDataVersionLoggingSupported;
			AssertEquals("IsDataVersionsAutoLogged", true, jobHeader.IsDataVersionsAutoLogged);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesJobHeader()
		{
			var job = Factory.New<DummyJobHeader>();

			var percentList = new List<string>
			{
				nameof(job.JH_AgentChargesCFX),
				nameof(job.JH_LocalChargesCFX)
			};

			var tester = new DecimalPlacesAttributeTester(job);
			tester.CheckConstant(percentList, nameof(job.PercentageDecimals), Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		public void TestInitializeParentFromGenericJobWithSettingDefaultsAndWithoutSettingDefaults()
		{
			var jobHeaderParent = Factory.New<IForwardingShipment>();
			var job = Factory.New<DummyJobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = jobHeaderParent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_JobNum = "One";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJob = newFactory.Load<DummyJobHeader>(job.PK);
			var logs = new List<string>();
			loadedJob.ProcessLogs = logs;

			AssertEquals(0, logs.Count);
			loadedJob.InitializeParentFromGenericJobWithoutSettingDefaults();
			AssertEquals("Job defaults are not set", "", string.Join("=>", logs));
			AssertEquals(false, loadedJob.DefaultValuesHasBeenAssigned);

			logs.Clear();
			AssertEquals(0, logs.Count);
			loadedJob.InitializeParentFromGenericJobWithSettingDefaults();
			AssertEquals("Job defaults are set", "SetParentCore", string.Join("=>", logs));
			AssertEquals(true, loadedJob.DefaultValuesHasBeenAssigned);
		}

		public void TestInitializeParentFromGenericJobWithSettingDefaultsAndHasBusinessContextWipAccrualReversing()
		{
			var jobHeaderParent = Factory.New<IForwardingShipment>();
			var job = Factory.New<DummyJobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = jobHeaderParent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_JobNum = "One";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJob = newFactory.Load<DummyJobHeader>(job.PK);
			using (loadedJob.RefreshJobParentSuspender.GetSuspender())
			{
				loadedJob.Parent = job.LoadGenericJob().Consumer;
			}
			var logs = new List<string>();
			loadedJob.ProcessLogs = logs;

			AssertEquals(0, logs.Count);
			newFactory.SetContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing);
			loadedJob.InitializeParentFromGenericJobWithSettingDefaults();
			AssertEquals("Job defaults are not set", "", string.Join("=>", logs));
			AssertEquals(false, loadedJob.DefaultValuesHasBeenAssigned);

			logs.Clear();
			AssertEquals(0, logs.Count);
			newFactory.RemoveContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing);
			loadedJob.InitializeParentFromGenericJobWithSettingDefaults();
			AssertEquals("Job defaults are set", "SetParentCore", string.Join("=>", logs));
			AssertEquals(true, loadedJob.DefaultValuesHasBeenAssigned);
		}

		public void TestUpdateLocalChargesOnDataRefresh()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			bool propertyRefreshed = false;
			jobHeader.JH_OA_LocalChargesAddrInfo.ValueChanged += (s, e) => { propertyRefreshed = true; };
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobHeaderClone = newFactory.Load<JobHeader>(jobHeader.PK);
			jobHeaderClone.JH_OA_LocalChargesAddr = newFactory.NewWithValidTestData<OrgAddress>().PK;
			newFactory.Save();

			Assert(propertyRefreshed);
		}

		[TestDate(2013, 9, 23)]
		public void TestClosingJobHeaderWillSetJobCloseDate()
		{
			var job = Factory.NewJobForTesting<JobHeader>();

			job.JH_A_JOP = ZDateTime.Empty;
			job.JH_Status = "";

			job.JH_Status = JobHeaderStatus.Working.Code;
			Assert(!job.JH_A_JOP.IsEmpty);
			AssertEquals(ZDateTime.Today, job.JH_A_JOP);

			Assert(job.JH_A_JCL.IsEmpty);

			job.JH_Status = JobHeaderStatus.Closed.Code;
			Assert(!job.JH_A_JCL.IsEmpty);
			AssertEquals(ZDateTime.Today, job.JH_A_JCL);

			job.JH_Status = JobHeaderStatus.Working.Code;
			Assert(job.JH_A_JCL.IsEmpty);
		}

		[TestDate(2014, 12, 3)]
		public void TestJH_Status()
		{
			var job = Factory.NewJobForTesting<JobHeader>();

			job.JH_A_JOP = ZDateTime.Empty;
			job.JH_A_JCL = ZDateTime.Empty;

			job.JH_Status = JobHeaderStatus.Closed.Code;

			Assert(job.JH_A_JOP.IsEmpty);
			Assert(!job.JH_A_JCL.IsEmpty);
			AssertEquals(ZDateTime.Today, job.JH_A_JCL);

			job.JH_Status = JobHeaderStatus.Working.Code;

			Assert(!job.JH_A_JOP.IsEmpty);
			Assert(job.JH_A_JCL.IsEmpty);
			AssertEquals(ZDateTime.Today, job.JH_A_JOP);

			job.Delete();
			AssertEquals(JobHeaderStatus.Closed.Code, job.JH_Status);
		}

		#region TestJH_JobNum_CreatesALogWhenChanges

		public void TestJH_JobNum_CreatesALogWhenChanges()
		{
			var job = Factory.NewJobForTesting<JobHeader>();

			job.JH_JobNum = "H00001001/I";
			var jobNumLogs = job.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Should find no log when change value from empty.", 0, jobNumLogs.Length);

			job.JH_JobNum = "S00001001/I";
			jobNumLogs = job.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Should find log when changes value.", 1, jobNumLogs.Length);
			AssertEquals("Changed job number from H00001001/I to S00001001/I", jobNumLogs[0].SL_Reference);
		}

		#endregion

		public void TestAlwaysLoadJobHeaderAsJob()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var expectedType = GetExpectedJobTypeToLoad() ?? ObjectFactory.GetType<IJobHeader>();
			AssertType("Job creating", expectedType, jobHeader);

			jobHeader = Factory.Load<JobHeader>(jobHeader.PK);
			AssertType("Job loading", expectedType, jobHeader);
		}

		// This test method will be removed once the delete checkers, including JobInvoicingParentLinkDeleteChecker, are published in release mode, and The changes implemented in WI00044228 will no longer be necessary and will be reverted as part of WI00858706.
		public void TestCase_NoExceptionShouldHappenWhileSavingJobWithDeactiveShipment()
		{
			var newFactory = new BusinessObjectFactory();
			BusinessObject parent = (BusinessObject)newFactory.New<IForwardingShipment>();

			IJobInvoicingPlugIn bo = parent as IJobInvoicingPlugIn;
			ICancellable boAsICancellable = bo as ICancellable;
			if (boAsICancellable != null)
			{
				JobHeader jobHeader = newFactory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.Parent = bo;
				jobHeader.Delete();
				parent.Delete();

				newFactory.Save();

				AssertEquals("No developer exception should be raised", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestRefreshParentShouldNotCauseExceptionWhenJobHeaderDeleted()
		{
			var parent = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var loader = new JobHeader.Loader(Factory, parent);
			var job = loader.TryCreate();

			job.Delete();
			job.Parent = parent;
		}

		public void TestAnyJobChangeAddsJobEditEvent()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();
			var filter = new ZQuery(StmALogSchema.SL_Parent, job.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.BillingJobEditCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, string.Empty);
			var filter2 = new ZQuery(StmALogSchema.SL_Parent, job2.PK);
			filter2.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.BillingJobEditCode);
			filter2.AddToFilter(StmALogSchema.SL_Reference, string.Empty);
			var logs = Factory.Load<StmALog>(filter);
			AssertEquals("Should not have a Log for new job ", 1, logs.Length);
			var logs2 = Factory.Load<StmALog>(filter2);
			AssertEquals("Should not have a Log for new job ", 1, logs2.Length);

			job.JH_Name = "ss";
			job2.JH_UniqueJobInvoiceNumber = 2;
			Factory.Save();
			logs = Factory.Load<StmALog>(filter);
			AssertEquals("Should have a Log for any job change", 2, logs.Length);
			logs2 = Factory.Load<StmALog>(filter);
			AssertEquals("Should have a Log for any job change", 2, logs2.Length);
		}

		public void TestFactorySave_JobStatusHasChanges_GenerateStatusUpdatedEvent()
		{
			var header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			header.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			AssertEventHasBeenGenerated(
				header,
				Events.StatusUpdated,
				new Dictionary<string, string>
				{
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceMessageTypes.JobStatus },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, JobHeaderStatus.Working.Code },
				});

			header.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			header.JH_HoldReason = "Because";
			Factory.Save();
			AssertEventHasBeenGenerated(
				header,
				Events.StatusUpdated,
				new Dictionary<string, string>
				{
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceMessageTypes.JobStatus },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, JobHeaderStatus.Working.Code },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, JobHeaderStatus.WorkOnHold.Code },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Because" },
				});

			header.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			header.JH_HoldReason = ZString.Empty;
			Factory.Save();
			AssertEventHasBeenGenerated(
				header,
				Events.StatusUpdated,
				new Dictionary<string, string>
				{
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceMessageTypes.JobStatus },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, JobHeaderStatus.WorkOnHold.Code },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, JobHeaderStatus.JobReadyForCostPosting.Code },
				});
		}

		void AssertEventHasBeenGenerated(JobHeader header, Event expectedEvent, Dictionary<string, string> expectedParameters)
		{
			var evnt = header.Logs.MostRecentLogByEventTime(expectedEvent);

			AssertNotNull(string.Format("A {0} has been generated", expectedEvent), evnt);

			foreach (var parameter in expectedParameters)
			{
				AssertEquals(string.Format("{0} parameter value", parameter.Key), parameter.Value, evnt.Parameters[parameter.Key]);
			}
		}

		public void TestDefaultLocalChargesAddressToARAddressWithMultipleLanguages()
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = Constants.Languages.ChineseTraditional;

			using (JobHeader newJob = Loader.TryCreateWithMutex())
			{
				OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
				newOrg.OH_Language = Constants.Languages.ChineseTraditional;

				OrgAddress officeAddress = newOrg.Addresses.AddNew();
				officeAddress.OA_Language = Constants.Languages.English;
				officeAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
				officeAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office);
				newJob.LocalChargesPK = newOrg.PK;
				AssertEquals(officeAddress.PK, newJob.JH_OA_LocalChargesAddr);

				OrgAddress receivablesAddress = newOrg.Addresses.AddNew();
				receivablesAddress.OA_Language = Constants.Languages.ChineseTraditional;
				receivablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
				receivablesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
				newJob.LocalChargesPK = ZGuid.Empty;
				newJob.LocalChargesPK = newOrg.PK;
				AssertEquals(receivablesAddress.PK, newJob.JH_OA_LocalChargesAddr);

				OrgAddress receivablesAddress2 = newOrg.Addresses.AddNew();
				receivablesAddress2.OA_Language = Constants.Languages.English;
				receivablesAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
				receivablesAddress2.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
				newJob.LocalChargesPK = ZGuid.Empty;
				newJob.LocalChargesPK = newOrg.PK;
				AssertEquals(receivablesAddress.PK, newJob.JH_OA_LocalChargesAddr);
			}
		}

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete_JobIsNotInDatabase()
		{
			JobHeader job = Loader.TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var newFactory = Factory.CreateNewFactory();
			var invoice = newFactory.NewWithValidTestData<AccTransactionHeader>();
			newFactory.Save();

			invoice.AH_JH = job.PK;
			AssertEquals(string.Empty, job.ReasonForNotAbleToDelete);
		}

		public void TestReasonForNotAbleToDelete_JobInDatabase()
		{
			var newCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch1 = newCompany1.Branches.AddNew();
			newBranch1.GB_Code = "BC1";
			var newCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch2 = newCompany2.Branches.AddNew();
			newBranch2.GB_Code = "BC2";
			Factory.Save();

			var job = Loader.TryCreateWithMutex();
			Factory.Save();
			
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), newBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Loader.TryCreateWithMutex();
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), newBranch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var loader2 = new JobHeader.Loader(Factory.New<DummyJobHeaderParent>());
				var job2 = loader2.TryCreateWithMutex(newBranch2);
				job2.JH_JobNum = job.JH_JobNum;
				Factory.Save();
			}

			AssertEquals($@"This record cannot be deleted.
An Invoicing Job Header (JobNumber) has been created in the company {GlbCompany.CurrentCompany.GC_Code}, {newCompany1.GC_Code}.", job.ReasonForNotAbleToDelete);
			job.Dispose();
		}

		#endregion

		#region TestReasonForNotAbleToDeactivate

		public void TestReasonForNotAbleToDeactivate()
		{
			var job = Loader.TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_JH = job.PK;
			Factory.Save();
			AssertEquals("Job cannot be deactivated.", false, job.CanDeactivate);
			AssertEquals(@"This record cannot be deactivated.
Accounting Transaction(s) have been saved against this Invoicing Job Header (JobNumber) in the company EDI.", job.ReasonForNotAbleToDeactivate);

			var invoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			AssertEquals("Job cannot be deactivated.", false, job.CanDeactivate);
			AssertEquals(@"This record cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header (JobNumber) in the company EDI.", job.ReasonForNotAbleToDeactivate);

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = job.PK;
			Factory.Save();
			var allAmountsZero = charge1.JR_OSCostAmt == 0m && charge1.JR_LocalCostAmt == 0m && charge1.JR_EstimatedCost == 0m && charge1.JR_AgentDeclaredCostAmt == 0m &&
				charge1.JR_OSSellAmt == 0m && charge1.JR_LocalSellAmt == 0m && charge1.JR_EstimatedRevenue == 0m && charge1.JR_AgentDeclaredSellAmt == 0m;
			AssertEquals("Job cannot be deactivated.", false, job.CanDeactivate);
			Assert("Should be Empty JR_OSCostAmt, JR_LocalCostAmt, JR_EstimatedCost, JR_AgentDeclaredCostAmt, JR_OSSellAmt, JR_LocalSellAmt, JR_EstimatedRevenue, JR_AgentDeclaredSellAmt amounts on the JobCharge", allAmountsZero);
			AssertEquals("Should not complain about JobCharges because all amounts are zero", @"This record cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header (JobNumber) in the company EDI.", job.ReasonForNotAbleToDeactivate);

			charge1.JR_OSCostAmt = 5m;
			charge1.JR_LocalCostAmt = 5m;
			Factory.Save();
			AssertEquals("Job cannot be deactivated.", false, job.CanDeactivate);
			AssertEquals(@"This record cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header (JobNumber) in the company EDI.", job.ReasonForNotAbleToDeactivate);
		}

		#endregion

		#region DeleteAllJobs

		/*Please create JobHeader via switching user context instead of calling TryCreate(GlbBranch branch)*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDeleteAllJobs()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			var job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			JobHeader.DeleteAllJobs(Parent);
			AssertEquals(true, job1.IsDeleted);
			AssertEquals(true, job2.IsDeleted);

			job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			JobHeader.DeleteAllJobs(Parent, false);
			AssertEquals(true, job1.IsDeleted);
			AssertEquals(true, job2.IsDeleted);

			job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			JobHeader.DeleteAllJobs(Parent, true);
			AssertEquals(true, job1.IsDeleted);
			AssertEquals(true, job2.IsDeleted);

			job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			job2 = new JobHeader.Loader(Parent).TryCreate(branch);

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_JH = job2.PK;
			line.AL_GC = job2.JH_GC;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			JobHeader.DeleteAllJobs(Parent, true);
			AssertEquals("You cannot delete a job if the job parent has transaction line in another or same company.", false, job1.IsDeleted);
			AssertEquals(false, job2.IsDeleted);
		}

		#endregion

		#region DeactivateAllJobs

		/*Please create JobHeader via switching user context instead of calling TryCreate(GlbBranch branch)*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDeactivateAllJobs()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			var job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			Factory.Save();
			JobHeader.DeactivateAllJobs(Parent);
			AssertEquals(true, job1.IsCancelled);
			AssertEquals(true, job2.IsCancelled);

			job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			Factory.Save();
			JobHeader.DeactivateAllJobs(Parent, false);
			AssertEquals(true, job1.IsCancelled);
			AssertEquals(true, job2.IsCancelled);

			job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			Factory.Save();
			JobHeader.DeactivateAllJobs(Parent, true);
			AssertEquals(true, job1.IsCancelled);
			AssertEquals(true, job2.IsCancelled);

			job1 = new JobHeader.Loader(Parent).TryCreate(GlbBranch.CurrentBranch);
			job2 = new JobHeader.Loader(Parent).TryCreate(branch);
			Factory.Save();

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_JH = job2.PK;
			line.AL_GC = job2.JH_GC;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			JobHeader.DeactivateAllJobs(Parent, true);
			AssertEquals("You cannot deactivate a job if the job parent has transaction line in another or same company.", false, job1.IsCancelled);
			AssertEquals(false, job2.IsCancelled);
		}

		public void TestDeactivateAllJobs_HaveContextWhenRelatedJobNotGetChanged()
		{
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var branchForAnotherCompany = Factory.NewWithValidTestData<GlbBranch>();
			branchForAnotherCompany.GB_GC = anotherCompany.PK;
			Factory.Save();

			var jobNoOtherChange = CreateJobHeaderByCompany(GlbCompany.CurrentCompany, Parent);
			var jobGetChanged = CreateJobHeaderByCompany(anotherCompany, Parent);

			jobGetChanged.JH_JobNum += "A";
			CombineAssertions("PreConditions", () => {
				AssertEquals("jobNoOtherChange HasChanges", false, jobNoOtherChange.HasChanges);
				AssertEquals("jobGetChanged HasChanges", true, jobGetChanged.HasChanges);
			});
			JobHeader.DeactivateAllJobs(Parent);

			AssertEquals("When JobHeader is being deactivated and no other changes, JobHeader should have JobDeactivationForAllCompanies context."
				, true, jobNoOtherChange.HasContext(BusinessContext.JobDeactivationForAllCompanies));
			AssertEquals("When JobHeader is being deactivated but having other changes, JobHeader should not have JobDeactivationForAllCompanies context."
				, false, jobGetChanged.HasContext(BusinessContext.JobDeactivationForAllCompanies));
		}

		JobHeader CreateJobHeaderByCompany(GlbCompany company, IJobHeaderParent parent)
		{
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.ActiveBranches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = new JobHeader.Loader(parent).TryCreateWithMutex();
				Factory.Save();
				return job;
			}
		}

		#endregion

		#region BusinessObject Overrides

		public virtual void TestSetDefaultValues()
		{
			JobHeader job = (JobHeader)GetNewBusinessObject();
			AssertEquals(Env.CurrentCompany.PK, job.JH_GC);
		}

		#endregion

		public void TestJobHeaderFetchHints()
		{
			OrgHeader newOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			newOrg1.Addresses.AddNew(OrgAddressType.Office, true);

			OrgHeader newOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrg2.Addresses.AddNew(OrgAddressType.Office, true);

			OrgHeader newOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrg3.Addresses.AddNew(OrgAddressType.Office, true);

			JobHeader header1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header1.JH_OA_LocalChargesAddr = newOrg1.Addresses[0].PK;
			header1.JH_OA_AgentCollectAddr = newOrg2.Addresses[0].PK;
			JobHeader header2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header2.JH_OA_LocalChargesAddr = newOrg2.Addresses[0].PK;
			header2.JH_OA_AgentCollectAddr = newOrg3.Addresses[0].PK;
			JobHeader header3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header3.JH_OA_LocalChargesAddr = newOrg3.Addresses[0].PK;
			header3.JH_OA_AgentCollectAddr = newOrg1.Addresses[0].PK;

			Factory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			JobHeader[] jobs = loadFactory.Load<JobHeader>(new ZQuery());
			int loads = loadFactory.DatabaseLoadCount;
			ZGuid result = jobs[0].LocalChargesPK;
			AssertEquals("Should be one additional DB hit", loadFactory.DatabaseLoadCount, loads + 1);
			result = jobs[1].LocalChargesPK;
			AssertEquals("Should be one additional DB hit", loadFactory.DatabaseLoadCount, loads + 1);
		}

		public void TestDefaultAddressType()
		{
			using (JobHeader newJob = Loader.TryCreateWithMutex())
			{
				AssertEquals(AddressType.ARM, newJob.JH_OA_AgentCollectAddr_ZAddress.DefaultAddressType);
				AssertEquals(AddressType.ARM, newJob.JH_OA_LocalChargesAddr_ZAddress.DefaultAddressType);
			}
		}

		public void TestGetDefaultAddressByHeader()
		{
			TestGetDefaultAddressByHeaderLocalCharges();
			TestGetDefaultAddressByHeaderAgentCollect();
		}

		void TestGetDefaultAddressByHeaderLocalCharges()
		{
			JobHeader newJob = Loader.TryCreate();
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
			newJob.LocalChargesPK = ZGuid.Empty;
			newJob.LocalChargesPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_LocalChargesAddr);
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_LocalChargesAddr);

			OrgAddress postalAddress = newOrg.Addresses.AddNew(OrgAddressType.Postal, true);
			newJob.LocalChargesPK = ZGuid.Empty;
			newJob.LocalChargesPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_LocalChargesAddr);
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_LocalChargesAddr);

			OrgAddress receivablesAddress = newOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
			newJob.LocalChargesPK = ZGuid.Empty;
			newJob.LocalChargesPK = newOrg.PK;
			AssertEquals(receivablesAddress.PK, newJob.JH_OA_LocalChargesAddr);
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(receivablesAddress.PK, newJob.JH_OA_LocalChargesAddr);

			foreach (OrgAddressCapabilityWrapper oAC in receivablesAddress.AddressCapability)
			{
				oAC.SetMain(false);
			}
			newJob.LocalChargesPK = ZGuid.Empty;
			newJob.LocalChargesPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_LocalChargesAddr);
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_LocalChargesAddr);

			foreach (OrgAddressCapabilityWrapper oAC in postalAddress.AddressCapability)
			{
				oAC.SetMain(false);
			}
			newJob.LocalChargesPK = ZGuid.Empty;
			newJob.LocalChargesPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_LocalChargesAddr);
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_LocalChargesAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_LocalChargesAddr);
		}

		void TestGetDefaultAddressByHeaderAgentCollect()
		{
			JobHeader newJob = Loader.TryCreate();
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
			newJob.AgentCollectPK = ZGuid.Empty;
			newJob.AgentCollectPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_AgentCollectAddr);
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_AgentCollectAddr);

			OrgAddress postalAddress = newOrg.Addresses.AddNew(OrgAddressType.Postal, true);
			newJob.AgentCollectPK = ZGuid.Empty;
			newJob.AgentCollectPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_AgentCollectAddr);
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_AgentCollectAddr);

			OrgAddress receivablesAddress = newOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
			newJob.AgentCollectPK = ZGuid.Empty;
			newJob.AgentCollectPK = newOrg.PK;
			AssertEquals(receivablesAddress.PK, newJob.JH_OA_AgentCollectAddr);
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(receivablesAddress.PK, newJob.JH_OA_AgentCollectAddr);

			foreach (OrgAddressCapabilityWrapper oAC in receivablesAddress.AddressCapability)
			{
				oAC.SetMain(false);
			}
			newJob.AgentCollectPK = ZGuid.Empty;
			newJob.AgentCollectPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_AgentCollectAddr);
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(postalAddress.PK, newJob.JH_OA_AgentCollectAddr);

			foreach (OrgAddressCapabilityWrapper oAC in postalAddress.AddressCapability)
			{
				oAC.SetMain(false);
			}
			newJob.AgentCollectPK = ZGuid.Empty;
			newJob.AgentCollectPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_AgentCollectAddr);
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = ZGuid.Empty;
			newJob.JH_OA_AgentCollectAddr_ZAddress.OrgPK = newOrg.PK;
			AssertEquals(officeAddress.PK, newJob.JH_OA_AgentCollectAddr);
		}

		public void TestDefaultLocalChargesAddressToARAddress()
		{
			using (JobHeader newJob = Loader.TryCreateWithMutex())
			{
				OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
				newJob.LocalChargesPK = newOrg.PK;
				AssertEquals(officeAddress.PK, newJob.JH_OA_LocalChargesAddr);

				OrgAddress receivablesAddress = newOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
				newJob.LocalChargesPK = ZGuid.Empty;
				newJob.LocalChargesPK = newOrg.PK;
				AssertEquals(receivablesAddress.PK, newJob.JH_OA_LocalChargesAddr);
			}
		}

		public void TestDefaultAgentCollectAddressToARAddress()
		{
			using (JobHeader newJob = Loader.TryCreateWithMutex())
			{
				OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
				newJob.AgentCollectPK = newOrg.PK;
				AssertEquals(officeAddress.PK, newJob.JH_OA_AgentCollectAddr);

				OrgAddress receivablesAddress = newOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
				newJob.AgentCollectPK = ZGuid.Empty;
				newJob.AgentCollectPK = newOrg.PK;
				AssertEquals(receivablesAddress.PK, newJob.JH_OA_AgentCollectAddr);
			}
		}

		public void TestGetAgentCollect_DeletedJob()
		{
			JobHeader newJob = Loader.TryCreate();
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
			newJob.AgentCollectPK = newOrg.PK;
			newJob.Delete();
			AssertEquals("Should return null when this job is deleted", null, newJob.AgentCollect);
			AssertEquals("Should return Empty ZGuid when this job is deleted", ZGuid.Empty, newJob.AgentCollectPK);
		}

		public void TestGetLocalCharges_DeletedJob()
		{
			JobHeader newJob = Loader.TryCreate();
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
			newJob.LocalChargesPK = newOrg.PK;
			newJob.Delete();
			AssertEquals("Should return null when this job is deleted", null, newJob.LocalCharges);
			AssertEquals("Should return Empty ZGuid when this job is deleted", ZGuid.Empty, newJob.LocalChargesPK);
		}

		public void TestDeleteDoesNotAddRecordToParentsLogIfNotInDB()
		{
			var parent = (BusinessObject)Factory.New<IForwardingShipment>();
			Factory.Save();
			var job = Loader.TryCreate();
			job.Parent = parent as IJobHeaderParent;
			job.JH_ParentID = parent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jobLocalReference = job.JH_JobLocalReference;
			job.Delete();
			var filter = new ZQuery(StmALogSchema.SL_Parent, parent.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, string.Format("Deleted Job Record - {0}", jobLocalReference));
			var logs = (StmALog[])job.Factory.Load(typeof(StmALog), filter);
			AssertEquals(0, logs.Length);
		}

		public void TestDeactivateAddRecordToParentsLog()
		{
			DeactivateAddsRecordToParentsLog(true);
		}

		public void TestDeactivateDoesNotAddRecordToParentsLogIfNotInDB()
		{
			DeactivateAddsRecordToParentsLog(false);
		}

		public void TestSuspendSettingHasChangesActiveJob()
		{
			var parent = (BusinessObject)Factory.New<IForwardingShipment>();
			var job = Loader.TryCreate();
			job.Parent = parent as IJobHeaderParent;
			job.JH_ParentID = parent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_Description = "";
			job.Factory.Save();

			var jobLocalReference = job.JH_JobLocalReference;

			job.MarkAsInactive();
			System.Threading.Thread.Sleep(10);
			AssertJobMarkedAsInactive(1, job.Factory, parent.PK, jobLocalReference);
			AssertJobMarkedAsActive(0, job.Factory, parent.PK, jobLocalReference);
			job.Factory.Save();

			job.ActivateJob();
			System.Threading.Thread.Sleep(10);
			job.Parent = parent as IJobHeaderParent;
			AssertJobMarkedAsActive(0, job.Factory, parent.PK, jobLocalReference);
			job.MarkAsInactive();
			System.Threading.Thread.Sleep(10);
			AssertJobMarkedAsInactive(2, job.Factory, parent.PK, jobLocalReference);
			job.Factory.Save();
			AssertJobMarkedAsActive(1, job.Factory, parent.PK, jobLocalReference);
			AssertJobMarkedAsInactive(2, job.Factory, parent.PK, jobLocalReference);

			var newFactoryForReload = new BusinessObjectFactory();

			using (job.SuspendSettingHasChanges())
			{
				job.JH_Description = "AAA";
				AssertEquals(false, job.HasChanges);

				job.ActivateJob();
				job.Parent = parent as IJobHeaderParent;
				AssertEquals(false, job.HasChanges);
				job.Factory.Save();
				AssertEquals(false, newFactoryForReload.Load<JobHeader>(job.PK).JH_IsActive);
				AssertJobMarkedAsActive(1, newFactoryForReload, parent.PK, jobLocalReference);
			}
			AssertEquals(false, job.HasChanges);
			job.Factory.Save();
			AssertEquals(false, newFactoryForReload.Load<JobHeader>(job.PK).JH_IsActive);
			AssertJobMarkedAsActive(1, newFactoryForReload, parent.PK, jobLocalReference);

			job.ActivateJob();
			job.Parent = parent as IJobHeaderParent;
			AssertEquals(true, job.JH_IsActive);
			AssertEquals(true, job.HasChanges);
			AssertJobMarkedAsActive(1, newFactoryForReload, parent.PK, jobLocalReference);
			job.Factory.Save();
			AssertJobMarkedAsActive(2, newFactoryForReload, parent.PK, jobLocalReference);
			AssertEquals(false, job.HasChanges);

			var filter = new ZQuery(StmALogSchema.SL_Parent, parent.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);

			var subQuery = new ZQuery(StmALogSchema.SL_Reference, $"Job marked as Active - {jobLocalReference}")
				.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, $"Job marked as Inactive - {jobLocalReference}");
			filter.AddToFilter(subQuery);

			var logs = newFactoryForReload.Load<StmALog>(filter).OrderBy(log => log.SL_EventTime).ToArray();
			AssertEquals(4, logs.Length);
			AssertEquals($"Job marked as Inactive - {jobLocalReference}", logs[0].SL_Reference);
			AssertEquals($"Job marked as Active - {jobLocalReference}", logs[1].SL_Reference);
			AssertEquals($"Job marked as Inactive - {jobLocalReference}", logs[2].SL_Reference);
			AssertEquals($"Job marked as Active - {jobLocalReference}", logs[3].SL_Reference);
		}

		void DeactivateAddsRecordToParentsLog(bool isInDatabase)
		{
			var parent = (BusinessObject)Factory.New<IForwardingShipment>();
			var expectedLogs = isInDatabase ? 1 : 0;
			if (!isInDatabase)
			{
				Factory.Save();
			}
			var job = Loader.TryCreate();
			job.Parent = parent as IJobHeaderParent;
			job.JH_ParentID = parent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			if (isInDatabase)
			{
				job.Factory.Save();
			}

			var jobLocalReference = job.JH_JobLocalReference;
			job.MarkAsInactive();
			AssertJobMarkedAsInactive(expectedLogs, job.Factory, parent.PK, jobLocalReference);

			if (isInDatabase)
			{
				job.ActivateJob();
				job.Parent = parent as IJobHeaderParent;
				AssertJobMarkedAsActive(0, job.Factory, parent.PK, jobLocalReference);

				job.Factory.Save();
				AssertJobMarkedAsActive(1, job.Factory, parent.PK, jobLocalReference);
			}
		}

		void AssertJobMarkedAsInactive(int expectedLogs, BusinessObjectFactory factory, ZGuid parentPK, ZString jobLocalReference)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, parentPK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, string.Format("Job marked as Inactive - {0}", jobLocalReference));

			var logs = (StmALog[])factory.Load(typeof(StmALog), filter);
			AssertEquals(expectedLogs, logs.Length);
		}

		void AssertJobMarkedAsActive(int expectedLogs, BusinessObjectFactory factory, ZGuid parentPK, ZString jobLocalReference)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, parentPK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, string.Format("Job marked as Active - {0}", jobLocalReference));

			var logs = (StmALog[])factory.Load(typeof(StmALog), filter);
			AssertEquals(expectedLogs, logs.Length);
		}

		[UseSnapshotProtection]
		public void TestJobLocalReferenceNumberIsNotSkippedAndCanBeReusedIfSaveFailed()
		{
			using (RunNonTransactioned())
			{
				JobHeader job1 = null;
				BusinessObject parent1;
				JobHeader job2 = null;
				BusinessObject parent2;

				parent1 = (BusinessObject)Factory.New<IForwardingShipment>();
				parent2 = (BusinessObject)Factory.New<IForwardingShipment>();
				Factory.Save();

				job1 = Factory.NewJobForTesting<JobHeader>();
				job1.Parent = parent1 as IJobHeaderParent;
				job1.JH_ParentTableCode = "JS";
				job1.JH_ParentID = parent1.PK;
				job1.JH_JobNum = "S00005001";
				job1.JH_GB = GlbBranch.CurrentBranch.PK;
				job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job1.Factory.Save();
				AssertEquals("First job's local reference should be 00000001", "00000001", job1.JH_JobLocalReference);

				try
				{
					job2 = Factory.NewJobForTesting<JobHeader>();
					job2.Parent = parent2 as IJobHeaderParent;
					job2.JH_ParentTableCode = "JS";
					job2.JH_ParentID = parent2.PK;
					job2.JH_JobNum = "S00005002";
					job2.JH_GB = GlbBranch.CurrentBranch.PK;
					job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
					job2.JH_OC_LocalBillingContact = ZGuid.NewZGuid();  // cause saving error
					Factory.Save();
					Fail("shouldn't reach this point");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}
				AssertEquals("Second job's local reference should be reset to empty when save failed due to error", ZString.Empty, job2.JH_JobLocalReference);

				job2.JH_OC_LocalBillingContact = ZGuid.Empty;
				Factory.Save();
				AssertEquals("Second job's local reference should be 00000002 after error is fixed and save succeeded", "00000002", job2.JH_JobLocalReference);
			}
		}

		public void TestJobLocalReferenceNumberIsNotRegeneratedIfNotEmpty()
		{
			JobHeader job = null;
			BusinessObject parent;
			parent = (BusinessObject)Factory.New<IForwardingShipment>();
			Factory.Save();

			job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = parent as IJobHeaderParent;
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = parent.PK;
			job.JH_JobNum = "S00005003";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobLocalReference = "00000008";
			job.Factory.Save();
			AssertEquals("job's local reference should not be regenerated if it's not empty", "00000008", job.JH_JobLocalReference);
		}

		public void TestJH_JH_ParentJob()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.LoadJobHeader);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JH_GCChangedFromNonEmptyToNonEmpty);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			JobHeader job = Loader.TryCreate();
			JobHeader decoyJob = Loader.TryCreate(branch);
			Factory.Save();

			JobHeader loadedJob = Loader.Load(false);
			AssertEquals("Correct job loaded", job, loadedJob);

			loadedJob = Loader.Load(false, company);
			AssertEquals("Correct job loaded", decoyJob, loadedJob);

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			job.JH_GC = company1.PK;
			decoyJob.JH_GC = company2.PK;

			ErrorReporter.Clear();
			job.JH_JH_ParentJob = decoyJob.PK;
			Assert(job.JH_JH_ParentJob.IsEmpty);

			AssertContains("Error message", FormattableString.Invariant($@"Tried to set JH_JH_ParentJob on JobHeader(PK:{job.PK}, GC:{company1.PK}) to JobHeader(PK:{decoyJob.PK}, GC:{company2.PK}), CurrentCompany: {currentCompanyPK}"), ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Error message", FormattableString.Invariant($@"Current Job:

LoadJobHeader:
JH_GC: {currentCompanyPK}, JH_Parent_ID: {Parent.PK}, JH_ParentTableCode: {Parent.PKSchemaColumn.ColumnPrefix}, CurrentCompany: {currentCompanyPK}, job.IsInDatabase: True, StackTrace ->"), ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Error message", FormattableString.Invariant($@"JH_GCChangedFromNonEmptyToNonEmpty:
Previous JH_GC: {currentCompanyPK}, New JH_GC: {company1.PK}, CurrentCompany: {currentCompanyPK}, StackTrace ->"), ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Should contain info about all property values",
$@"	PK = {job.PK}
	Type = Job
	Types around row = Job
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Error message", FormattableString.Invariant($@"Parent Job:

LoadJobHeader:
JH_GC: {company.PK}, JH_Parent_ID: {Parent.PK}, JH_ParentTableCode: {Parent.PKSchemaColumn.ColumnPrefix}, CurrentCompany: {currentCompanyPK}, job.IsInDatabase: True, StackTrace ->"), ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Error message", FormattableString.Invariant($@"JH_GCChangedFromNonEmptyToNonEmpty:
Previous JH_GC: {company.PK}, New JH_GC: {company2.PK}, CurrentCompany: {currentCompanyPK}, StackTrace ->"), ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Should contain info about all property values",
$@"	PK = {decoyJob.PK}
	Type = Job
	Types around row = Job
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			AssertContains("Error message", "Please assign this issue to Ajit Prabhu/Andrii Sarnavskyi for further investigation", ExceptionReporterTestListener.Instance[0].InnerException.Message);

			ErrorReporter.Clear();
			job.JH_GC = currentCompanyPK;
			JobHeader job2 = Loader.TryCreate();
			job.JH_JH_ParentJob = job2.PK;
			AssertEquals("Should have parent jobheader", job2.PK, job.JH_JH_ParentJob);
			AssertEquals("Has NO error", "", ErrorReporter.LastMessageReported);
		}

		public void TestInfoCollectedDuringOnLoaded()
		{
			JobHeader job = Loader.TryCreate();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			CriticalValidationInfoCollectorService.GetOrCreateService(factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.LoadJobHeader);
			CriticalValidationInfoCollectorService.GetOrCreateService(factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JH_GCChangedFromNonEmptyToNonEmpty);

			var loadedJob = factory.Load<JobHeader>(job.PK);
			AssertContains("Info is collected", $@"LoadJobHeader:
JH_GC: {GlbCompany.CurrentCompany.PK}, JH_Parent_ID: {Parent.PK}, JH_ParentTableCode: {Parent.PKSchemaColumn.ColumnPrefix}, CurrentCompany: {GlbCompany.CurrentCompany.PK}, job.IsInDatabase: True, StackTrace ->", CriticalValidationInfoCollectorService.GetOrCreateService(factory).GetInfo(loadedJob.PK, CriticalValidationInfoCollectorServiceKeyType.LoadJobHeader));
		}

		public void TestParentWorkflowProviders()
		{
			BusinessObject parent = (BusinessObject)Factory.New<IForwardingShipment>();
			JobHeader job = Loader.TryCreate();
			job.Parent = parent as IJobHeaderParent;
			job.JH_ParentID = parent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			AssertNotNull("PreCondition", job.Parent);

			job = new BusinessObjectFactory().Load<JobHeader>(job.PK);
			AssertNull("Parent not applied", job.Parent);

			AssertEquals("ParentWorkflowProviders collection is not empty", 1, job.ParentWorkflowProviders.Count);
			AssertEquals("ParentWorkflowProvider is correct", parent.PK, job.ParentWorkflowProviders[0].PK);
		}

		public void TestDefaultAddressByHeaderInCommonLanguageIsNotInactive()
		{
			JobHeader newJob = Loader.TryCreate();
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress officeAddress = newOrg.Addresses.AddNew(OrgAddressType.Office, true);
			OrgAddress postalAddress = newOrg.Addresses.AddNew(OrgAddressType.Postal, true);
			OrgAddress receivablesAddress = newOrg.Addresses.AddNew(OrgAddressType.Receivables, true);

			AssertEquals(receivablesAddress.PK, JobHeader.GetDefaultAddressByHeaderInCommonLanguage(newOrg));
			Assert(Factory.Load<OrgAddress>(JobHeader.GetDefaultAddressByHeaderInCommonLanguage(newOrg)).OA_IsActive);

			receivablesAddress.OA_IsActive = false;
			AssertEquals(postalAddress.PK, JobHeader.GetDefaultAddressByHeaderInCommonLanguage(newOrg));
			Assert(Factory.Load<OrgAddress>(JobHeader.GetDefaultAddressByHeaderInCommonLanguage(newOrg)).OA_IsActive);

			postalAddress.OA_IsActive = false;
			AssertEquals(officeAddress.PK, JobHeader.GetDefaultAddressByHeaderInCommonLanguage(newOrg));
			Assert(Factory.Load<OrgAddress>(JobHeader.GetDefaultAddressByHeaderInCommonLanguage(newOrg)).OA_IsActive);
		}

		public void TestOnSaving_ShouldIncludePKOfBranchAndDepartmentIfNull()
		{
			//Set to report at the first occurrence so the exception throw and can be assert
			AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			AssertNull(jobHeader.Branch);
			AssertNull(jobHeader.Department);

			var ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
			AssertContains("Branch (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", ex.Message);

			jobHeader.JH_GB = Env.CurrentBranch.PK;
			AssertNotNull(jobHeader.Branch);

			ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
			AssertContains("Department (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", ex.Message);
		}

		public void TestOnSaving_AutoAssignBranchAndDepartmentIfNull()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				//Branch
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";
				AssertNull(jobHeader.Branch);
				AssertNull(jobHeader.Department);
				AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				jobHeader.OnSaving();
				AssertNotNull(jobHeader.Branch);
				AssertNotNull(jobHeader.Department);
				AssertEquals("JHBranchIsNullOccurrenceCounter should be 1 by now", 1, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);

				//Department
				jobHeader.JH_GB = Env.CurrentBranch.PK;
				jobHeader.JH_GE = Guid.Empty;
				AssertNotNull(jobHeader.Branch);
				AssertNull(jobHeader.Department);
				AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				jobHeader.OnSaving();
				AssertEquals("JHBranchIsNullOccurrenceCounter should be 1 by now", 1, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);
				AssertNotNull(jobHeader.Branch);
				AssertNotNull(jobHeader.Department);
			}
		}

		public void TestOnSaving_OccurrenceCounterCanIncrementAndReset()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";

				int maxOccurrence = 5;
				AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maxOccurrence);
				AssertEquals(maxOccurrence, AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.Value);

				AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				for (int i = 1; i <= maxOccurrence; i++)
				{
					jobHeader.JH_GB = Guid.Empty;
					jobHeader.JH_GE = Guid.Empty;
					AssertNull(jobHeader.Branch);
					AssertNull(jobHeader.Department);

					jobHeader.OnSaving();

					AssertNotNull(jobHeader.Branch);
					AssertNotNull(jobHeader.Department);
					AssertEquals("JHBranchIsNullOccurrenceCounter", i, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);
				}

				jobHeader.JH_GB = Guid.Empty;
				jobHeader.JH_GE = Guid.Empty;
				AssertNull(jobHeader.Branch);
				AssertNull(jobHeader.Department);

				var ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
				AssertEquals("JHBranchIsNullOccurrenceCounter should be reset to 0 by now", 0, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);
			}
		}

		public void TestOnSaving_NullBranchDepartmentErrorIsShownAfterExceedingAllowedMaximumNoOfOccurances()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";

				int maxOccurrence = 2;
				AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maxOccurrence);
				AssertEquals(maxOccurrence, AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.Value);

				AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				AssertEquals(0, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);
				//Before maximum allowable occurrence hit
				for (int i = 1; i <= maxOccurrence; i++)
				{
					jobHeader = Factory.NewJobForTesting<JobHeader>();
					jobHeader.JH_JobNum = "dummy";
					AssertNull(jobHeader.Branch);
					AssertNull(jobHeader.Department);
					jobHeader.OnSaving();
					AssertNotNull(jobHeader.Branch);
					AssertNotNull(jobHeader.Department);
					AssertEquals("JHBranchIsNullOccurrenceCounter", i, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);
				}

				//After maximum allowable occurrence hit
				jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";
				AssertNull(jobHeader.Branch);
				AssertNull(jobHeader.Department);
				var ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
				AssertContains("Branch (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", ex.Message);
				AssertEquals("JHBranchIsNullOccurrenceCounter should be reset to 0 by now", 0, AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);
			}
		}

		#region NullBranchMessageHandler
		[TestDate(2018, 11, 29, 12, 02, 32, 127)]
		public void TestOnSaving_NullBranchMessageWithJobHeaderInDatabaseAndSetBranchFromNullToNull()
		{
			AssertNullBranchMessageWithSetBranchFromNullToNull(true);
		}

		[TestDate(2018, 11, 29, 12, 02, 32, 127)]
		public void TestOnSaving_NullBranchMessageWithJobHeaderInDatabaseAndSetBranchFromValidToNull()
		{
			AssertNullBranchMessageWithSetBranchFromValidToNull(true);
		}

		[TestDate(2018, 11, 29, 12, 02, 32, 127)]
		public void TestOnSaving_NullBranchMessageWithJobHeaderNotInDatabaseAndSetBranchFromNullToNull()
		{
			AssertNullBranchMessageWithSetBranchFromNullToNull(false);
		}

		[TestDate(2018, 11, 29, 12, 02, 32, 127)]
		public void TestOnSaving_NullBranchMessageWithJobHeaderNotInDatabaseAndSetBranchFromValidToNull()
		{
			AssertNullBranchMessageWithSetBranchFromValidToNull(false);
		}

		void AssertNullBranchMessageWithSetBranchFromNullToNull(bool shouldTestSavedJob)
		{
			using (AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";

				if (shouldTestSavedJob)
				{
					jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
					Factory.Save();
				}

				jobHeader.JH_GB = ZGuid.Empty;
				AssertNull(jobHeader.Branch);
				TestDateAttribute.AddMilliseconds(205);
				var testDateTime = ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				jobHeader.JH_GB = ZGuid.Empty;
				AssertNull(jobHeader.Branch);

				var ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
				AssertResult(testDateTime, ex);

				jobHeader.JH_GB = ZGuid.Empty;
				AssertNull(jobHeader.Branch);
				TestDateAttribute.AddMilliseconds(100);
				testDateTime = ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				jobHeader.JH_GB = ZGuid.Empty;
				AssertNull(jobHeader.Branch);

				ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
				AssertContains("Argument 'Branch (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", ex.Message);
				AssertContains(jobHeader.GetJobInfo(), ex.Message);
				AssertResult(testDateTime, ex);
			}
			void AssertResult(string dateTime, Exception ex)
			{
				if (shouldTestSavedJob)
				{
					AssertContains("JH_GBChanged: There is no data collected for this PK.", ex.Message);
					AssertContains("JH_GBChangedFromValidToEmpty: There is no data collected for this PK.", ex.Message);
					AssertContains("JH_GBChangedFromValidToEmptyForSavedJob: There is no data collected for this PK.", ex.Message);
				}
				else
				{
					AssertContains($@"JH_GBChanged:
Branch: <NULL>
Branch Changed Time: {dateTime}", ex.Message);
					AssertContainsInOrderNewLineSensitive(string.Empty, ex.Message, "JH_GB change StackTrace ->\r\n", "at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions", "<RecordLastJH_GBChange>");
					AssertContains("JH_GBChangedFromValidToEmpty: There is no data collected for this key.", ex.Message);
					AssertContains("JH_GBChangedFromValidToEmptyForSavedJob: There is no data collected for this key.", ex.Message);
				}
				AssertContains($@"JobConstructorStackTrace:
Job Created Time: 2018-11-29 12:02:32.127
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", ex.Message);
			}
		}

		void AssertNullBranchMessageWithSetBranchFromValidToNull(bool shouldTestSavedJob)
		{
			using (AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";
				jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;

				if (shouldTestSavedJob)
				{
					Factory.Save();
				}

				AssertNotNull(jobHeader.Branch);
				TestDateAttribute.AddMilliseconds(205);
				jobHeader.JH_GB = ZGuid.Empty;
				AssertNull(jobHeader.Branch);
				var ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
				if (shouldTestSavedJob)
				{
					AssertContains("JH_GBChanged: There is no data collected for this PK.", ex.Message);
					AssertContains("JH_GBChangedFromValidToEmpty: There is no data collected for this PK.", ex.Message);
					AssertContains("JH_GBChangedFromValidToEmptyForSavedJob: There is no data collected for this PK.", ex.Message);
				}
				else
				{
					AssertContains($@"JH_GBChanged:
Branch: <NULL>
Branch Changed Time: 2018-11-29 12:02:32.332", ex.Message);
					AssertContainsInOrderNewLineSensitive(string.Empty, ex.Message, "JH_GB change StackTrace ->\r\n", "at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions", "<RecordLastJH_GBChange>");
					AssertContains($@"JH_GBChangedFromValidToEmpty:
Branch: <NULL>
Previous Branch: (Code: {GlbBranch.CurrentBranch.GB_Code})
Previous Branch Changed Time: 2018-11-29 12:02:32.332", ex.Message);
					AssertContainsInOrderNewLineSensitive(string.Empty, ex.Message, "JH_GB change from valid to empty StackTrace ->\r\n", "at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions", "<RecordLastJH_GBChangeFromValidToEmpty>");
					AssertContains($@"JH_GBChangedFromValidToEmptyForSavedJob: There is no data collected for this key.", ex.Message);
				}
				AssertContains($@"JobConstructorStackTrace:
Job Created Time: 2018-11-29 12:02:32.127
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", ex.Message);

				TestDateAttribute.AddMilliseconds(100);
				jobHeader.JH_GB = Env.CurrentBranchPK;
				jobHeader.JH_GB = ZGuid.Empty;
				AssertNull(jobHeader.Branch);
				ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);
				AssertContains("Argument 'Branch (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", ex.Message);
				AssertContains(jobHeader.GetJobInfo(), ex.Message);

				if (shouldTestSavedJob)
				{
					AssertContains("JH_GBChanged: There is no data collected for this key.", ex.Message);
					AssertContains("JH_GBChangedFromValidToEmpty: There is no data collected for this key.", ex.Message);
					AssertContains($@"JH_GBChangedFromValidToEmptyForSavedJob:
Branch: <NULL>
Previous Branch: (Code: {GlbBranch.CurrentBranch.GB_Code})
Previous Branch Changed Time: 2018-11-29 12:02:32.432
JH_GB change from valid to empty StackTrace ->
   at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions.<>c__DisplayClass7_0.<RecordLastJH_GBChangeFromValidToEmpty>g__getMessage|0()", ex.Message);
				}
				else
				{
					AssertContains($@"JH_GBChanged:
Branch: <NULL>
Branch Changed Time: 2018-11-29 12:02:32.432", ex.Message);
					AssertContainsInOrderNewLineSensitive(string.Empty, ex.Message, "JH_GB change StackTrace ->\r\n", "at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions", "<RecordLastJH_GBChange>");
					AssertContains($@"JH_GBChangedFromValidToEmpty:
Branch: <NULL>
Previous Branch: (Code: {GlbBranch.CurrentBranch.GB_Code})
Previous Branch Changed Time: 2018-11-29 12:02:32.432", ex.Message);
					AssertContainsInOrderNewLineSensitive(string.Empty, ex.Message, "JH_GB change from valid to empty StackTrace ->\r\n", "at Enterprise.MasterFiles.Business.JobHeaderDataCollectionExtensions", "<RecordLastJH_GBChangeFromValidToEmpty>");
					AssertContains($@"JH_GBChangedFromValidToEmptyForSavedJob: There is no data collected for this key.", ex.Message);
				}
				AssertContains($@"JobConstructorStackTrace:
Job Created Time: 2018-11-29 12:02:32.127
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", ex.Message);
			}
		}
		#endregion

		#region NullDepartmentMessageHandler
		[TestDate(2018, 11, 29, 12, 02, 49, 55)]
		public void TestOnSaving_NullDepartmentMessageWithJobHeaderInDatabase()
		{
			AssertNullDepartmentMessage(true);
		}

		[TestDate(2018, 11, 29, 12, 02, 49, 55)]
		public void TestOnSaving_NullDepartmentMessageWithJobHeaderNotInDatabase()
		{
			AssertNullDepartmentMessage(false);
		}

		void AssertNullDepartmentMessage(bool shouldTestSavedJob)
		{
			using (AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "dummy";
				jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
				jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
				if (shouldTestSavedJob)
				{
					Factory.Save();
				}

				jobHeader.JH_GE = ZGuid.Empty;
				AssertNotNull(jobHeader.Branch);
				AssertNull(jobHeader.Department);
				var ex = AssertExceptionThrown<ArgumentNullException>(jobHeader.OnSaving);

				if (shouldTestSavedJob)
				{
					var oldMsgForJH_GEChanged = Factory.ServiceContainer.GetService<CriticalValidationInfoCollectorService>()?.GetInfo(jobHeader.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GEChanged);
					AssertContains("JH_GEChanged: There is no data collected for this PK.", oldMsgForJH_GEChanged);
				}
				else
				{
					AssertContains("Argument 'Department (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", ex.Message);
					AssertContains(jobHeader.GetJobInfo(), ex.Message);

					AssertContains("JH_GEChanged:", ex.Message);
					AssertContains("Department Changed Time: 2018-11-29 12:02:49.055", ex.Message);
					AssertContains("at Enterprise.MasterFiles.Business.JobHeader.set_JH_GE(ZGuid value)", ex.Message);
				}
				AssertContains("JobConstructorStackTrace:", ex.Message);
				AssertContains("Job Created Time: 2018-11-29 12:02:49.055", ex.Message);
				AssertContains("at Enterprise.MasterFiles.Business.JobHeader..ctor(BusinessObjectFactory factory, DataRow row)", ex.Message);
			}
		}

		[TestDate(2019, 12, 20, 11, 12, 13)]
		public void TestGetJobCreationWithEmptyBranchMessage()
		{
			AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var job = Factory.NewWithValidTestData<JobHeader>();
			job.JH_GB = Env.CurrentBranchPK;
			job.JH_GB = ZGuid.Empty;
			var msg = job.GetJobCreationWithEmptyBranchMessage();

			var exceptMsg1 = FormattableString.Invariant($@"
JH_GBChanged:
Branch: <NULL>
Branch Changed Time: 2019-12-20 11:12:13.000
JH_GB change StackTrace ->");
			var exceptMsg2 = FormattableString.Invariant($@"
JH_GBChangedFromValidToEmpty:
Branch: <NULL>
Previous Branch: (Code: BNE)
Previous Branch Changed Time: 2019-12-20 11:12:13.000
JH_GB change from valid to empty StackTrace ->");
			var exceptMsg3 = FormattableString.Invariant($@"
JobConstructorStackTrace:
Job Created Time: 2019-12-20 11:12:13.000");

			AssertContains("Argument 'Branch (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", msg);
			AssertContains(job.GetJobInfo(), msg);
			AssertContains(exceptMsg1, msg);
			AssertContains(exceptMsg2, msg);
			AssertContains(exceptMsg3, msg);
			ErrorReporter.Clear();
		}

		[TestDate(2019, 12, 21, 11, 12, 13)]
		public void TestGetJobCreationWithEmptyDepartmentMessage()
		{
			AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var job = Factory.NewWithValidTestData<JobHeader>();
			job.JH_GE = Env.CurrentDepartmentPK;
			job.JH_GE = ZGuid.Empty;
			var msg = job.GetJobCreationWithEmptyDepartmentMessage();

			var exceptMsg1 = FormattableString.Invariant($@"
JH_GEChanged:
Department: <NULL>
Department Changed Time: 2019-12-21 11:12:13.000
JH_GE change StackTrace ->");
			var exceptMsg2 = FormattableString.Invariant($@"
JobConstructorStackTrace:
Job Created Time: 2019-12-21 11:12:13.000");

			AssertContains("Argument 'Department (PK: 00000000-0000-0000-0000-000000000000)' cannot be null.", msg);
			AssertContains(job.GetJobInfo(), msg);
			AssertContains(exceptMsg1, msg);
			AssertContains(exceptMsg2, msg);
			ErrorReporter.Clear();
		}
		#endregion

		public void TestOnSaving_RecordInvalidJobCreationByUser_WhenRegistryHasMultipleValues()
		{
			var staff = CreateStaff("zz1", "xxx", "zz1", true, true);
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate(GlbBranch.CurrentBranch);
			job.JH_SystemCreateUser = staff.GS_Code;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("Pre-Condition", "zz1", job.JH_SystemCreateUser);
			AssertEquals("Pre-Condition", "EDI", job.Company.GC_Code);
			AssertEquals("Pre-Condition", "BNE", job.Branch.GB_Code);
			AssertEquals("Pre-Condition", "BRN", job.Department.GE_Code);
			AssertNullOrEmpty(AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.Value);

			job.ReportInvalidJobCreationAsDeveloperException();
			AssertNullOrEmpty(ErrorReporter.LastKeyReported);

			CombineAssertions("Should NOT Report when Registry 'RecordInvalidJobCreationByUser' does not include userinfo in job", () =>
			{
				AssertShouldReport(false, $@"");
				AssertShouldReport(false, $@"[xxx]
[xxx]-[DKR]
[xxx]-[DKR]-[SEL]
[xxx]-[DKR]-[SEL]-[FI1]
[xxx]-[DKR]-[SEL]-[FI3]
");
			});

			CombineAssertions("Should Report when Registry 'RecordInvalidJobCreationByUser' includes userinfo in job", () =>
			{
				AssertShouldReport(true, $@"[zz1]
[xxx]-[DKR]
[xxx]-[DKR]-[SEL]
[xxx]-[DKR]-[SEL]-[FI1]
[xxx]-[DKR]-[SEL]-[FI3]
");
				AssertShouldReport(true, $@"[xxx]
[zz1]-[EDI]
[xxx]-[DKR]-[SEL]
[xxx]-[DKR]-[SEL]-[FI1]
[xxx]-[DKR]-[SEL]-[FI3]
");
				AssertShouldReport(true, $@"[xxx]
[xxx]-[DKR]
[zz1]-[EDI]-[BNE]
[xxx]-[DKR]-[SEL]-[FI1]
[xxx]-[DKR]-[SEL]-[FI3]
");
				AssertShouldReport(true, $@"[xxx]
[xxx]-[DKR]
[xxx]-[DKR]-[SEL]
[zz1]-[EDI]-[BNE]-[BRN]
[xxx]-[DKR]-[SEL]-[FI3]
");
				AssertShouldReport(true, $@"[zz1]
[zz1]-[EDI]
[zz1]-[EDI]-[BNE]
[zz1]-[EDI]-[BNE]-[BRN]
[xxx]-[DKR]-[SEL]-[FI3]
");
			});

			CombineAssertions("Check if Registry 'RecordInvalidJobCreationByUser' can work when the user enters some strange characters.", () =>
			{
				AssertShouldReport(true, $@"   [xxx]
  [xxx]-[DKR] 
    [xxx]-[DKR]-[SEL]  
  [zz1]-[EDI]-[BNE]-[BRN]  
 [xxx]-[DKR]-[SEL]-[FI3]   

");
				AssertShouldReport(false, $@"   [xxx]  
  [xxx]-[DKR]  
    [xxx]-[DKR]-[SEL]  
  [xxx]-[DKR]-[SEL]-[FI1]  
 [xxx]-[DKR]-[SEL]-[FI3]  

");
				AssertShouldReport(false, $@"   [xxx]  
  [x x x]-[D  K R]  
    [xxx]-[D  KR]-[SE  L]  
  [xxx]-[DK R]-[S  EL]-[FI1]  
 [xxx]-[D KR]-[SEL]-[FI  3]  

");
				AssertShouldReport(false, $@"

");
			});

			void AssertShouldReport(bool shouldReport, FormattableString value)
			{
				AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FormattableString.Invariant(value));
				job.ReportInvalidJobCreationAsDeveloperException();

				if (shouldReport)
				{
					AssertContains("InvalidJobCreation_1", ErrorReporter.LastKeyReported);
					ErrorReporter.Clear();
				}
				else
				{
					AssertNullOrEmpty(ErrorReporter.LastKeyReported);
				}
			}
		}

		public void TestOnSaving_UnwantedJobCreationIsReported_WhenUserDoesNotHaveLoginPermission()
		{
			var staff = CreateStaff("zzz", "xxx", "zzz", true, true);
			SetPermission(staff, AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.PK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK, new ZGuid(Env.CurrentDepartmentPK), Env.CurrentUserContext.LoginSecurityCheckpoint.DisplayText, false);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FormattableString.Invariant($"[{staff.GS_Code}]"));
			AssertOnSaving_UnwantedJobCreationIsReported(AccountingTestObjectCreator.NonCurrentCompanyBranch, staff
				, true
				, $"Current user does not have login permission to {AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.GC_Code}-{AccountingTestObjectCreator.NonCurrentCompanyBranch.GB_Code}-{GlbDepartment.CurrentDepartment.GE_Code}");
		}

		public void TestOnSaving_UnwantedJobCreationIsReported_WhenUserDoesNotHaveBillingPermission()
		{
			var staff = CreateStaff("zzz", "xxx", "zzz", true, true);
			SetPermission(staff, AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.PK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK, new ZGuid(Env.CurrentDepartmentPK), "Billing", false);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FormattableString.Invariant($"[{staff.GS_Code}]"));
			AssertOnSaving_UnwantedJobCreationIsReported(AccountingTestObjectCreator.NonCurrentCompanyBranch, staff
				, true
				, $"Current user has login permission to {AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.GC_Code}-{AccountingTestObjectCreator.NonCurrentCompanyBranch.GB_Code}-{GlbDepartment.CurrentDepartment.GE_Code}");
		}

		public void TestOnSaving_UnwantedJobCreationIsNotReported_WhenRegistryIsNotSet()
		{
			var staff = CreateStaff("zzz", "xxx", "zzz", true, true);
			SetPermission(staff, AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.PK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK, new ZGuid(Env.CurrentDepartmentPK), "Billing", false);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertOnSaving_UnwantedJobCreationIsReported(AccountingTestObjectCreator.NonCurrentCompanyBranch
				, staff
				, false);
		}

		public void TestOnSaving_UnwantedJobCreationIsNotReported_WhenRegistryDoesNotMatch()
		{
			var staff = CreateStaff("zzz", "xxx", "zzz", true, true);
			SetPermission(staff, AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.PK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK, new ZGuid(Env.CurrentDepartmentPK), "Billing", false);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, $"[zzz]-[{AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.GC_Code}]-[{GlbBranch.CurrentBranch.GB_Code}]-[{GlbDepartment.CurrentDepartment.GE_Code}]");
			AssertOnSaving_UnwantedJobCreationIsReported(AccountingTestObjectCreator.NonCurrentCompanyBranch
				, staff
				, false);
		}

		void AssertOnSaving_UnwantedJobCreationIsReported(GlbBranch branch, GlbStaff staff, bool expectedHasReported, params string[] extraExpectedReportMessages)
		{
			var shipment1 = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var shipment2 = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			Factory.Save();

			var job1 = new JobHeader.Loader(shipment1).TryCreateWithoutMutexForTestOnly(GlbBranch.CurrentBranch);
			Factory.Save();
			AssertNull(ErrorReporter.LastExceptionReported);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job2 = new JobHeader.Loader(shipment2).TryCreateWithMutex();
				Factory.Save();
			}

			if (expectedHasReported)
			{
				AssertContains("InvalidJobCreation_1", ErrorReporter.LastKeyReported);
				AssertContains($"User: zzz - Company: {branch.Company.GC_Code} - Branch: {branch.GB_Code} - Dept: {GlbDepartment.CurrentDepartment.GE_Code} - UTC Time: ", ErrorReporter.LastMessageReported);
				AssertContains($@"Is initiated by ProcessController: No - Service Task Code: 

Logged in user is creating the Job."
					, ErrorReporter.LastMessageReported);

				AssertContains($@"This billing job has {branch.GB_Code} as branch and {GlbDepartment.CurrentDepartment.GE_Code} as department.
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing

NOTE for the Developer: Please consult with product team to make sure that 'Record invalid job creation by user(CargoWise Support Only)' has been set correctly. If not, this may indicate a false positive. 
Current value of this registry is [zzz]

Job Information ->
", ErrorReporter.LastMessageReported);

				AssertContains("at Enterprise.MasterFiles.Business.JobHeader..ctor(BusinessObjectFactory factory, DataRow row)", ErrorReporter.LastMessageReported);
				AssertContains("at Enterprise.MasterFiles.Business.JobHeader.set_JH_GC(ZGuid value)", ErrorReporter.LastMessageReported);
				AssertContains("at Enterprise.MasterFiles.Business.JobHeader.set_JH_GB(ZGuid value)", ErrorReporter.LastMessageReported);
				AssertContains("at Enterprise.MasterFiles.Business.JobHeader.set_JH_GE(ZGuid value)", ErrorReporter.LastMessageReported);
				foreach (var extraExpectedReportMessage in extraExpectedReportMessages)
				{
					AssertContains(extraExpectedReportMessage, ErrorReporter.LastMessageReported);
				}
				ErrorReporter.Clear();
			}
			else
			{
				AssertNullOrEmpty(ErrorReporter.LastKeyReported);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestLocalZAddressWithContact()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			Assert(jobHeader.JH_OC_LocalBillingContact.IsEmpty);
			Assert(jobHeader.JH_OA_LocalChargesAddr.IsEmpty);
			AssertEquals(ZGuid.Empty, jobHeader.LocalZAddressWithContact.ContactFK);
			AssertEquals(ZGuid.Empty, jobHeader.LocalZAddressWithContact.AddressFK);
			var newContact = Factory.New<OrgContact>().PK;
			var newAddress = Factory.New<OrgAddress>().PK;
			jobHeader.JH_OC_LocalBillingContact = newContact;
			jobHeader.JH_OA_LocalChargesAddr = newAddress;
			AssertEquals(newContact, jobHeader.LocalZAddressWithContact.ContactFK);
			AssertEquals(newAddress, jobHeader.LocalZAddressWithContact.AddressFK);
		}

		public void TestDeniedRateSecurityCheckPoint()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				charge1.JR_JH = jobHeader.PK;
				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				charge2.JR_JH = jobHeader.PK;

				Factory.Save();

				jobHeader.LocalChargesPK = setupResult.AllowedOrg.PK;
				charge1.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
				charge1.JR_OH_CostAccount = setupResult.AllowedOrg.PK;
				charge2.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
				charge2.JR_OH_CostAccount = setupResult.AllowedOrg.PK;

				Factory.Save();

				AssertNull(jobHeader.DeniedRateSecurityCheckPoint);

				jobHeader.LocalChargesPK = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeader.DeniedRateSecurityCheckPoint.Code);

				jobHeader.LocalChargesPK = setupResult.AllowedOrg.PK;
				charge1.JR_OH_SellAccount = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeader.DeniedRateSecurityCheckPoint.Code);

				charge1.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
				charge1.JR_OH_CostAccount = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeader.DeniedRateSecurityCheckPoint.Code);
			}
		}

		public void TestDeniedRateSecurityCheckPoint_PartlyLoadedAndCahngedCharges()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.LocalChargesPK = setupResult.AllowedOrg.PK;

				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				charge1.JR_JH = jobHeader.PK;
				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				charge2.JR_JH = jobHeader.PK;
				var charge3 = Factory.NewWithValidTestData<JobCharge>();
				charge3.JR_JH = jobHeader.PK;

				charge1.JR_OH_SellAccount = ZGuid.Empty;
				charge1.JR_OH_CostAccount = setupResult.AllowedOrg.PK;

				charge2.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
				charge2.JR_OH_CostAccount = ZGuid.Empty;

				charge3.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
				charge3.JR_OH_CostAccount = setupResult.AllowedOrg.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting); //to prevent loading all job charges in new factory

				var jobHeaderInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);
				var charge1InNewFactory = newFactory.Load<JobCharge>(charge1.PK);

				AssertEquals("Precondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
				AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
				AssertEquals("Postcondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

				charge1InNewFactory.JR_OH_CostAccount = setupResult.DeniedOrg.PK;

				AssertEquals("Precondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
				AssertEquals("Postcondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

				charge1InNewFactory.JR_OH_CostAccount = setupResult.AllowedOrg.PK;

				AssertEquals("Precondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
				AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
				AssertEquals("Postcondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

				charge3.JR_OH_SellAccount = setupResult.DeniedOrg.PK;
				Factory.Save();

				AssertEquals("Precondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
				AssertNull("DeniedRateSecurityCheckPoint returns cached value as nothing was changed in newFactory.", jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
				AssertEquals("Postcondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

				jobHeaderInNewFactory.JH_Description = "Just to clear factory cache for DeniedRateSecurityCheckPoint";

				AssertEquals("Precondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
				AssertEquals("Postcondition: Charges loaded in factory", 1, newFactory.Load<JobCharge>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			}
		}

		public void TestGetOrgPKsFromChargesInDbGroupedByJob_LoadCount_Allow()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.LocalChargesPK = setupResult.AllowedOrg.PK;

				var chargeAllow = Factory.NewWithValidTestData<JobCharge>();
				chargeAllow.JR_JH = jobHeader.PK;
				chargeAllow.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
				chargeAllow.JR_OH_CostAccount = setupResult.AllowedOrg.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var jobHeaderInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);

				JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly = 0;
				AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
				AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);
				AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
				AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

				jobHeaderInNewFactory.JH_Description = "Update newFactory.CacheVersion for forcing DeniedRateSecurityCheckPoint update";
				AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
				AssertEquals(2, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

				newFactory = new BusinessObjectFactory();
				using (newFactory.SetTempContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb))
				{
					jobHeaderInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);
					JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly = 0;
					AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
					AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

					AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
					AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

					jobHeaderInNewFactory.JH_Description = "Update newFactory.CacheVersion for forcing DeniedRateSecurityCheckPoint update";
					AssertNull(jobHeaderInNewFactory.DeniedRateSecurityCheckPoint);
					AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);
				}
			}
		}

		public void TestGetOrgPKsFromChargesInDbGroupedByJob_LoadCount_Denied()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				for (var i = 0; i < 5; i++)
				{
					var charge = Factory.NewWithValidTestData<JobCharge>();
					charge.JR_JH = jobHeader.PK;
					charge.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
					charge.JR_OH_CostAccount = ZGuid.Empty;
				}
				var chargeDenied = Factory.NewWithValidTestData<JobCharge>();
				chargeDenied.JR_JH = jobHeader.PK;
				chargeDenied.JR_OH_SellAccount = ZGuid.Empty;
				chargeDenied.JR_OH_CostAccount = setupResult.DeniedOrg.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var jobHeaderInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);

				JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly = 0;
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
				AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
				AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

				jobHeaderInNewFactory.JH_Description = "Update newFactory.CacheVersion for forcing DeniedRateSecurityCheckPoint update";
				AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
				AssertEquals(2, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

				newFactory = new BusinessObjectFactory();
				using (newFactory.SetTempContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb))
				{
					jobHeaderInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);
					JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly = 0;

					AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
					AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

					AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
					AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);

					jobHeaderInNewFactory.JH_Description = "Update newFactory.CacheVersion for forcing DeniedRateSecurityCheckPoint update";
					AssertEquals(setupResult.DeniedSecurity.Code, jobHeaderInNewFactory.DeniedRateSecurityCheckPoint.Code);
					AssertEquals(1, JobHeader.GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly);
				}
			}
		}

		public void TestDeniedRateSecurityCheckPoint_ChargeUpdateByAnotherFactory()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				for (var i = 0; i < 2; i++)
				{
					var charge = Factory.NewWithValidTestData<JobCharge>();
					charge.JR_JH = jobHeader.PK;
					charge.JR_OH_SellAccount = setupResult.AllowedOrg.PK;
					charge.JR_OH_CostAccount = ZGuid.Empty;
				}
				var chargeDenied = Factory.NewWithValidTestData<JobCharge>();
				chargeDenied.JR_JH = jobHeader.PK;
				chargeDenied.JR_OH_SellAccount = ZGuid.Empty;
				chargeDenied.JR_OH_CostAccount = setupResult.DeniedOrg.PK;
				Factory.Save();

				var newFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};

				var jobInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);

				// Call one more time because newFactory latest cache version is increased when calculating DeniedRateSecurityCheckPoint CachedProperty.
				// That is why DeniedRateSecurityCheckPoint expired right after first calculation.
				AssertEquals(setupResult.DeniedSecurity.Code, jobInNewFactory.DeniedRateSecurityCheckPoint.Code);

				AssertEquals(setupResult.DeniedSecurity.Code, jobInNewFactory.DeniedRateSecurityCheckPoint.Code);
				chargeDenied.JR_OH_CostAccount = setupResult.AllowedOrg.PK;
				Factory.Save();
				AssertEquals("DeniedRateSecurityCheckPoint is cached property", setupResult.DeniedSecurity.Code, jobInNewFactory.DeniedRateSecurityCheckPoint.Code);
				jobInNewFactory.JH_Description = "Update newFactory.CacheVersion for forcing DeniedRateSecurityCheckPoint update, and get new result: 'not denied'";
				AssertNull(jobInNewFactory.DeniedRateSecurityCheckPoint);

				chargeDenied.JR_OH_CostAccount = setupResult.DeniedOrg.PK;
				Factory.Save();

				newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				using (newFactory.SetTempContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb))
				{
					jobInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);
					AssertEquals(setupResult.DeniedSecurity.Code, jobInNewFactory.DeniedRateSecurityCheckPoint.Code);

					chargeDenied.JR_OH_CostAccount = setupResult.AllowedOrg.PK;
					Factory.Save();

					AssertEquals(setupResult.DeniedSecurity.Code, jobInNewFactory.DeniedRateSecurityCheckPoint.Code);
					jobInNewFactory.JH_Description = "DeniedRateSecurityCheckPoint is still denied because of the cached result";
					AssertEquals(setupResult.DeniedSecurity.Code, jobInNewFactory.DeniedRateSecurityCheckPoint.Code);
				}
			}
		}

		public void TestIsReadyForCostPosting()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			foreach (JobHeaderStatus status in new JobHeaderStatusList())
			{
				jobHeader.JH_Status = status.Code;
				bool expected = status.Code == JobHeaderStatus.JobReadyForCostPosting.Code ||
					status.Code == JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;

				AssertEquals("IsReadyForCostPosting for " + status.Code, expected, jobHeader.IsReadyForCostPosting);
			}
		}

		public void TestIsReadyForRevenuePosting()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			foreach (JobHeaderStatus status in new JobHeaderStatusList())
			{
				jobHeader.JH_Status = status.Code;
				bool expected = status.Code == JobHeaderStatus.JobReadyForRevenuePosting.Code ||
					status.Code == JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;

				AssertEquals("IsReadyForRevenuePosting for " + status.Code, expected, jobHeader.IsReadyForRevenuePosting);
			}
		}

		public void TestLoadJobHeaderWithoutErrorReport()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(JobHeaderSchema.JH_JobNum, jobHeader.JH_JobNum);
			var job = newFactory.LoadTop1<JobHeader>(query);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestGetGSTID_OrgHeaderSpecific()
		{
			var jobHeader = Factory.New<TestJobHeader>();
			var gstID = jobHeader.GetGSTID(null, null, null, CostSell.Cost, out var overrideInvTaxMsg);
			Assert(gstID.IsEmpty);
			Assert(overrideInvTaxMsg.IsEmpty);

			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(true);
			autoJRJHelperMock.Setup(x => x.IsExcludedFromAutoJRJ(It.IsAny<GlbBranch>(), It.IsAny<OrgHeader>(), It.IsAny<string>())).Returns(false);

			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				jobHeader.JH_GC = ZGuid.Empty;
				gstID = jobHeader.GetGSTID(testCreditor, null, null, CostSell.Cost, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				var company = Factory.New<GlbCompany>();
				company.GC_OH_OrgProxy = jobHeader.PK;
				jobHeader.JH_GC = company.PK;
				gstID = jobHeader.GetGSTID(testCreditor, null, null, CostSell.Cost, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				testCreditor.CompanyData.SetAPTaxApplicable(true);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				gstID = jobHeader.GetGSTID(testCreditor, Factory.New<AccChargeCode>(), null, CostSell.Cost, out overrideInvTaxMsg);
				AssertEquals(new ZGuid("56077822-3783-498B-9D8D-C99844B3C7CC"), gstID);
				AssertEquals(new ZGuid("1E8BE15B-D917-430E-9D0C-EEB5709F6C7B"), overrideInvTaxMsg);

				gstID = jobHeader.GetGSTID(testCreditor, null, null, CostSell.Cost, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				gstID = jobHeader.GetGSTID(testCreditor, Factory.New<AccChargeCode>(), null, CostSell.Cost, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				gstID = jobHeader.GetGSTID(testCreditor, null, null, CostSell.Cost, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				testCreditor.CompanyData.SetARTaxApplicable(false);
				gstID = jobHeader.GetGSTID(testCreditor, Factory.New<AccChargeCode>(), null, CostSell.Revenue, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				testCreditor.CompanyData.SetARTaxApplicable(true);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				gstID = jobHeader.GetGSTID(testCreditor, Factory.New<AccChargeCode>(), null, CostSell.Revenue, out overrideInvTaxMsg);
				AssertEquals(new ZGuid("56077822-3783-498B-9D8D-C99844B3C7CC"), gstID);
				AssertEquals(new ZGuid("1E8BE15B-D917-430E-9D0C-EEB5709F6C7B"), overrideInvTaxMsg);

				gstID = jobHeader.GetGSTID(testCreditor, null, null, CostSell.Revenue, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);

				testCreditor.CompanyData.SetARTaxApplicable(true);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				gstID = jobHeader.GetGSTID(testCreditor, Factory.New<AccChargeCode>(), null, CostSell.Revenue, out overrideInvTaxMsg);
				Assert(gstID.IsEmpty);
				Assert(overrideInvTaxMsg.IsEmpty);
			}
		}

		public void TestGetGSTID_AutoJRJWithTaxRegistrationNumber()
		{
			var jobHeader = Factory.NewWithValidTestData<TestJobHeader>();
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var branch = GlbBranch.CurrentBranch;
			var company = GlbCompany.CurrentCompany;

			var taxID = jobHeader.GetGSTID(null, null, null, CostSell.Cost, out var overrideInvTaxMsg);
			AssertEquals("Precondition", true, taxID.IsEmpty);
			AssertEquals(true, overrideInvTaxMsg.IsEmpty);

			creditor.CompanyData.SetARTaxApplicable(true);
			company.GC_IsGSTRegistered = true;
			company.GC_OH_OrgProxy = creditor.PK;

			Factory.Save();
			company.Factory.Save();

			var autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(true);
			autoJRJHelperMock.Setup(x => x.IsExcludedFromAutoJRJ(It.IsAny<GlbBranch>(), It.IsAny<OrgHeader>(), It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				taxID = jobHeader.GetGSTID(creditor, chargeCode, branch, CostSell.Revenue, out overrideInvTaxMsg);
				AssertEquals("Should get proper tax info when is not eligible for auto JRJ", new ZGuid("56077822-3783-498B-9D8D-C99844B3C7CC"), taxID);
				AssertEquals(new ZGuid("1E8BE15B-D917-430E-9D0C-EEB5709F6C7B"), overrideInvTaxMsg);
			}

			autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(true);
			autoJRJHelperMock.Setup(x => x.IsExcludedFromAutoJRJ(It.IsAny<GlbBranch>(), It.IsAny<OrgHeader>(), It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				taxID = jobHeader.GetGSTID(creditor, chargeCode, branch, CostSell.Revenue, out overrideInvTaxMsg);
				AssertEquals("Should be not tax applicable when eligible for auto JRJ", true, taxID.IsEmpty);
				AssertEquals(true, overrideInvTaxMsg.IsEmpty);
			}

			autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(false);
			autoJRJHelperMock.Setup(x => x.IsExcludedFromAutoJRJ(It.IsAny<GlbBranch>(), It.IsAny<OrgHeader>(), It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				taxID = jobHeader.GetGSTID(creditor, chargeCode, branch, CostSell.Revenue, out overrideInvTaxMsg);
				AssertEquals("Should get proper tax info when Auto JRJ is disabled", new ZGuid("56077822-3783-498B-9D8D-C99844B3C7CC"), taxID);
				AssertEquals(new ZGuid("1E8BE15B-D917-430E-9D0C-EEB5709F6C7B"), overrideInvTaxMsg);
			}

			autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(false);
			autoJRJHelperMock.Setup(x => x.IsExcludedFromAutoJRJ(It.IsAny<GlbBranch>(), It.IsAny<OrgHeader>(), It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				taxID = jobHeader.GetGSTID(creditor, chargeCode, branch, CostSell.Revenue, out overrideInvTaxMsg);
				AssertEquals("Should get proper tax info when Auto JRJ is disabled", new ZGuid("56077822-3783-498B-9D8D-C99844B3C7CC"), taxID);
				AssertEquals(new ZGuid("1E8BE15B-D917-430E-9D0C-EEB5709F6C7B"), overrideInvTaxMsg);
			}
		}

		#region UniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandler_SameJobParentAndCompany()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";
			Factory.Save();

			var anotherInstanceFactory = new BusinessObjectFactory();
			var anotherLoader = new JobHeader.Loader((IJobHeaderParent)anotherInstanceFactory.Load<IForwardingShipment>(shipment.PK));
			var anotherJob = anotherLoader.TryCreateWithMutex();
			var anotherTestJob = anotherInstanceFactory.Load<TestJobHeader>(anotherJob.PK);
			anotherJob.Dispose();

			JobHeader job;
			using (SimulateFactoryIsCreatedByDifferentCargowise())
			{
				var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
				job = loader.TryCreateWithMutex();
				job.Factory.Save();
			}

			var message = "A Job Header record with same number and company (S1000000, EDI) already exist in the database.\r\nSystem will update the number, try to save again. If it doesn't solve your problem, you must close this job and re-apply your changes.";
			AssertUniqueIndexFailureHandlerCore(anotherTestJob, message);
		}

		public void TestUniqueIndexFailureHandler_SameParentID()
		{
			var job1 = Factory.NewWithValidTestData<TestJobHeader>();
			var job2 = Factory.NewWithValidTestData<TestJobHeader>();
			job1.JH_ParentID = job2.JH_ParentID;
			job1.JH_JobNum = "JOB1";
			job2.JH_JobNum = "JOB2";
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;

			var message = "A Job Header record with same number and company (JOB1, EDI) already exist in the database.\r\nSystem will update the number, try to save again. If it doesn't solve your problem, you must close this job and re-apply your changes.";
			AssertUniqueIndexFailureHandlerCore(job1, message);
		}

		public void TestUniqueIndexFailureHandler_SameJobNum()
		{
			var job1 = Factory.NewWithValidTestData<TestJobHeader>();
			var job2 = Factory.NewWithValidTestData<TestJobHeader>();
			job1.JH_JobNum = job2.JH_JobNum;
			job1.JH_ParentID = ZGuid.NewZGuid();
			job2.JH_ParentID = ZGuid.NewZGuid();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;

			var message = "A Job Header record with same number and company (DY30DWLEYWJEP733N3ENUVM8ENSQWGH18R0, EDI) already exist in the database.\r\nSystem will update the number, try to save again. If it doesn't solve your problem, you must close this job and re-apply your changes.";
			AssertUniqueIndexFailureHandlerCore(job1, message);
		}

		public void TestTwoJobHeadersLinkToTheSameParent_DisableRefresh()
			=> AssertTwoJobHeadersLinkToTheSameParent(false);

		public void TestTwoJobHeadersLinkToTheSameParent_EnableRefresh()
			=> AssertTwoJobHeadersLinkToTheSameParent(true);

		void AssertTwoJobHeadersLinkToTheSameParent(bool refreshEnabled)
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";
			Factory.Save();
			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);

			var anotherInstanceFactory = new BusinessObjectFactory() { RefreshEnabled = refreshEnabled };
			var anotherLoader = new JobHeader.Loader((IJobHeaderParent)anotherInstanceFactory.Load<IForwardingShipment>(shipment.PK));
			var anotherJob = anotherLoader.TryCreateWithMutex();

			AssertEquals("PreCondition, set null as cached result due to anotherJob is locking mutex.", null, loader.TryCreateWithMutex());
			using (SimulateFactoryIsCreatedByDifferentCargowise())
			{
				anotherJob.Factory.Save();
			}

			using (var jobCreateSecondTime = loader.TryCreateWithMutex())
			{
				AssertNotNull(jobCreateSecondTime);
				AssertEquals(jobCreateSecondTime.PK, anotherJob.PK);

				var testJob = Factory.Load<TestJobHeader>(jobCreateSecondTime.PK);
				AssertNoExceptionThrown(() => testJob.Factory.Save());
			}

			anotherJob.Dispose();
		}

		void AssertUniqueIndexFailureHandlerCore(TestJobHeader job, string expectedErrorMsg)
		{
			try
			{
				job.Factory.Save();
				Fail();
			}
			catch (ZSaveException ex)
			{
				var handler = job.UniqueIndexFailureHandlers.FirstOrDefault(h => h.HandledUniqueIndexNames.Contains(ex.IndexNameIfUniqueIndexViolation.ToString()));
				if (handler != null)
				{
					handler.NotifyUserAndAttemptToResolve(NotificationHandler, ex.IndexNameIfUniqueIndexViolation);
					AssertEquals("Caption", "Job changes", NotificationHandler.LastErrorCaption);
					AssertContains("Notify Message", expectedErrorMsg, NotificationHandler.LastErrorMessage);
				}
				else
				{
					throw;
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestJobHeaderLoaderWillQueryDatabaseToGetCorrectResultWhenTryCreateWithMutex()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";
			Factory.Save();

			var anotherInstanceFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var anotherInstanceLoader = new JobHeader.Loader((IJobHeaderParent)anotherInstanceFactory.Load<IForwardingShipment>(shipment.PK));
			var anotherInstanceJob = anotherInstanceLoader.TryCreateWithMutex();
			AssertNotNull("PreCondition, create job header and lock mutex via another CW1 factory", anotherInstanceJob);

			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
			AssertNull("PreCondition, do Load and set null cached result to RowFactory.", loader.Load());
			AssertNull("PreCondition, should not create Job Header because anotherInstanceLoader is locking mutex and Factory cached null value.", loader.TryCreateWithMutex());

			using (SimulateFactoryIsCreatedByDifferentCargowise())
			{
				anotherInstanceFactory.Save();
			}

			CombineAssertions("anotherInstanceJob is saved to database and it should be active to skip certain reload job logic.", () =>
			{
				AssertEquals("PK", shipment.PK, anotherInstanceJob.JH_ParentID);
				AssertEquals("IsInDatabase", true, anotherInstanceJob.IsInDatabase);
				AssertEquals("JH_IsActive", true, anotherInstanceJob.JH_IsActive);
			});

			var dbHitBeforeTryCreate = Factory.GetTableHitCount(JobHeaderSchema.Constants.TableName);
			CombineAssertions("Load but get cached null result.", () =>
			{
				AssertNull("loader.Load", loader.Load());
				AssertEquals("DB hits Increasement.",
					0,
					Factory.GetTableHitCount(JobHeaderSchema.Constants.TableName) - dbHitBeforeTryCreate
				);
			});
			AssertTryCreateWithMutexGetCorrectResultByIgnoringCachedResult();

			anotherInstanceJob.Dispose();

			void AssertTryCreateWithMutexGetCorrectResultByIgnoringCachedResult()
			{
				using (var currentInstanceJob = loader.TryCreateWithMutex())
				{
					CombineAssertions("Ignore cached result and load actual result from DB.", () =>
					{
						AssertEquals("PK", anotherInstanceJob.PK, currentInstanceJob.PK);
						AssertEquals("IsInDatabase", true, currentInstanceJob.IsInDatabase);
					});

					AssertEquals("Should query database for Job Header.",
						1,
						Factory.GetTableHitCount(JobHeaderSchema.Constants.TableName) - dbHitBeforeTryCreate
					);
				}
			}
		}

		public void TestJobHeaderLoaderTryCreateWithMutexDbHit_ShipmentInDatabase()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";

			Factory.Save();
			AssertEquals("Precondition,it is a shipment in Database.", true, ((BusinessObject)shipment).IsInDatabase);

			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
			AssertJobHeaderLoaderTryCreateDbHits("TryCreateWithMutex should only have 1 DB hit even when ignoring cached result.",
				1,
				Factory,
				loader
			);

			AssertJobHeaderLoaderTryCreateDbHits("More calls to TryCreateWithMutex should not increase DB hit because previous job header query result was cached.",
				0,
				Factory,
				loader
			);
		}

		public void TestJobHeaderLoaderTryCreateWithMutexDbHit_ShipmentNotInDatabase()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";

			AssertEquals("Precondition, it is a new shipment.", false, ((BusinessObject)shipment).IsInDatabase);

			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
			AssertJobHeaderLoaderTryCreateDbHits("TryCreateWithMutex do not query DB since job parent is not in databse, there is no way to have duplicated job header.",
				0,
				Factory,
				loader
			);

			AssertJobHeaderLoaderTryCreateDbHits("More calls to TryCreateWithMutex should not increase DB hit because previous job header query result was cached.",
				0,
				Factory,
				loader
			);
		}

		public void TestJobHeaderLoaderTryCreateWithMutexDbHit_DuplicatedCreateInAnotherFactory()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";

			Factory.Save();
			AssertEquals("Precondition,it is a shipment in Database.", true, ((BusinessObject)shipment).IsInDatabase);

			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
			AssertJobHeaderLoaderTryCreateDbHits("TryCreateWithMutex should only have 1 DB hit even when ignoring cached result.",
				1,
				Factory,
				loader
			);

			var newFactory = new BusinessObjectFactory();
			var newLoader = new JobHeader.Loader((IJobHeaderParent)newFactory.Load<IForwardingShipment>(shipment.PK));
			AssertJobHeaderLoaderTryCreateDbHits("Will query DB since no cached result in newFactory.",
				1,
				newFactory,
				newLoader
			);
		}

		public void TestJobHeaderLoaderTryLoadOrCreateDbHit()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";

			Factory.Save();
			AssertEquals("Precondition,it is a shipment in Database.", true, ((BusinessObject)shipment).IsInDatabase);

			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
			AssertJobHeaderLoaderTryLoadOrCreateDbHits("TryLoadOrCreate have 2 DB hits, one for loading active job, one for loading also deactive job.",
				2,
				Factory,
				loader
			);
			AssertJobHeaderLoaderLoadDbHits("Load should reuse cached result.",
				0,
				Factory,
				loader
			);
			AssertJobHeaderLoaderTryCreateDbHits("TryCreateDb should reuse cached result.",
				0,
				Factory,
				loader
			);

			var newFactory = new BusinessObjectFactory();
			var newLoader = new JobHeader.Loader((IJobHeaderParent)newFactory.Load<IForwardingShipment>(shipment.PK));
			AssertJobHeaderLoaderLoadDbHits("Should query DB since no cached result in newFactory.",
				1,
				newFactory,
				newLoader
			);
			AssertNull("Factory is not saved, newLoader should cache null result", newLoader.Load());
			AssertJobHeaderLoaderTryLoadOrCreateDbHits("Should query DB only 1 time that loading active & deactive job for Loader.TryCreate.",
				1,
				newFactory,
				newLoader
			);
		}

		void AssertJobHeaderLoaderTryCreateDbHits(string comment, int expectedHits, BusinessObjectFactory factory, JobHeader.Loader loader)
		{
			var dbHitBeforeTryCreate = factory.GetTableHitCount(JobHeaderSchema.Constants.TableName);
			using (loader.TryCreateWithMutex())
			{
				AssertEquals(comment,
					expectedHits,
					factory.GetTableHitCount(JobHeaderSchema.Constants.TableName) - dbHitBeforeTryCreate
				);
			}
		}

		void AssertJobHeaderLoaderTryLoadOrCreateDbHits(string comment, int expectedHits, BusinessObjectFactory factory, JobHeader.Loader loader)
		{
			var dbHitBeforeTryCreate = factory.GetTableHitCount(JobHeaderSchema.Constants.TableName);
			using (loader.TryLoadOrCreate())
			{
				AssertEquals(comment,
					expectedHits,
					factory.GetTableHitCount(JobHeaderSchema.Constants.TableName) - dbHitBeforeTryCreate
				);
			}
		}

		void AssertJobHeaderLoaderLoadDbHits(string comment, int expectedHits, BusinessObjectFactory factory, JobHeader.Loader loader)
		{
			var dbHitBeforeTryCreate = factory.GetTableHitCount(JobHeaderSchema.Constants.TableName);
			using (loader.Load())
			{
				AssertEquals(comment,
					expectedHits,
					factory.GetTableHitCount(JobHeaderSchema.Constants.TableName) - dbHitBeforeTryCreate
				);
			}
		}

		public void TestTryCreateWithMutexReturnActiveJobHeaderIfHaveOne()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000000";
			Factory.Save();
			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);

			AssertNull("PreCondition", loader.Load());

			var anotherInstanceFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var anotherLoader = new JobHeader.Loader((IJobHeaderParent)anotherInstanceFactory.Load<IForwardingShipment>(shipment.PK));
			var anotherJob = anotherLoader.TryCreateWithMutex();
			AssertNotNull("PreCondition", anotherJob);

			using (SimulateFactoryIsCreatedByDifferentCargowise())
			{
				anotherInstanceFactory.Save();
			}
			AssertNull("Load cached result.", loader.Load());

			var creatingJobHeader = loader.TryCreateWithMutex();
			AssertEquals("There is a JobHeader in database", anotherJob.PK, creatingJobHeader.PK);

			anotherJob.Dispose();
			creatingJobHeader.Dispose();
		}

		DisposableAction SimulateFactoryIsCreatedByDifferentCargowise()
		{
			var oldValue = ((IBusinessObjectFactoryInternals)Factory).DisableQueryCacheReset;
			((IBusinessObjectFactoryInternals)Factory).DisableQueryCacheReset = true;

			return new DisposableAction(() => ((IBusinessObjectFactoryInternals)Factory).DisableQueryCacheReset = oldValue);
		}
		#endregion

		#region Mutex

		public void TestGetJobCreationError_MutexAcquiredBySameUser()
		{
			using (ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK))
			{
				Assert(mutex.Lock());

				AssertEquals(
					"You have created the job JobNumber on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job JobNumber to continue.",
					Loader.GetJobCreationError().Message);
			}
		}

		public void TestGetJobCreationError_JobLockedByAnotherUserSetContext()
		{
			Globals.IsUserInteractive = false;

			var differentUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var mutexHoldingUser = GlbStaff.CurrentUser.GS_LoginName;

			var job = Loader.TryLoadOrCreateWithMutex();
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			Loader.TryLoadOrCreateWithMutex();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(differentUser.GS_LoginName))
			{
				AssertEquals(
					$"User {mutexHoldingUser} is in the process of re-activating the Job JobNumber. You cannot work on the job until he/she saves it or cancels the changes.",
					Loader.GetJobCreationError().Message);

				Assert(Factory.HasContext(BusinessContext.JobLockedByAnotherUser));
			}

			job.Dispose();
		}

		public void TestJobAutoDisposeWhenFactoryHasDisposableContext()
		{
			var factory = new BusinessObjectFactory();
			JobHeader job;
			using (factory.AddDisposableService())
			{
				var parent = factory.New<DummyJobHeaderParent>();
				job = new JobHeader.Loader(parent).TryLoadOrCreateWithMutex();
				AssertEquals(false, job.IsDisposed);
			}
			AssertEquals(true, job.IsDisposed);
		}

		public void TestGetJobCreationError_ActivateJob_MutexAcquiredBySameUser()
		{
			var job = Loader.TryLoadOrCreateWithMutex();
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			Loader.TryLoadOrCreateWithMutex();

			AssertEquals(
				"You have re-activated the job JobNumber on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job JobNumber to continue.",
				Loader.GetJobCreationError().Message);

			job.Dispose();
		}

		public void TestGetJobCreationError_MutexAcquiredByDifferentUser()
		{
			var differentUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			string mutexHoldingUser = GlbStaff.CurrentUser.GS_LoginName;

			using (ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK))
			{
				Assert(mutex.Lock());

				using (CurrentUserChanger.SwitchToNewUserTemporarily(differentUser.GS_LoginName))
				{
					AssertEquals("Current user was not switched.", differentUser.GS_LoginName, GlbStaff.CurrentUser.GS_LoginName);

					AssertEquals(
						$"User {mutexHoldingUser} is in the process of creating the Job JobNumber. You cannot work on the job until he/she saves it or cancels the changes.",
						Loader.GetJobCreationError().Message);
				}
			}
		}

		public void TestGetJobCreationError_ActivateJob_MutexAcquiredByDifferentUser()
		{
			var differentUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var mutexHoldingUser = GlbStaff.CurrentUser.GS_LoginName;

			var job = Loader.TryLoadOrCreateWithMutex();
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			Loader.TryLoadOrCreateWithMutex();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(differentUser.GS_LoginName))
			{
				AssertEquals("Current user was not switched.", differentUser.GS_LoginName, GlbStaff.CurrentUser.GS_LoginName);

				AssertEquals(
					$"User {mutexHoldingUser} is in the process of re-activating the Job JobNumber. You cannot work on the job until he/she saves it or cancels the changes.",
					Loader.GetJobCreationError().Message);
			}

			job.Dispose();
		}

		public void TestGetJobCreationError_NoMutex()
		{
			// This scenario can happen if mutex was released prior to message generation.
			AssertEquals(
				"User (undefined) is in the process of creating the Job JobNumber. You cannot work on the job until he/she saves it or cancels the changes.",
				Loader.GetJobCreationError().Message);
		}

		public void TestGetJobCreationErrorForService_MutexAcquired()
		{
			string mutexHoldingUser = GlbStaff.CurrentUser.GS_LoginName;

			using (ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK))
			{
				Assert(mutex.Lock());

				AssertEquals(
					$"User {mutexHoldingUser} has created a Billing Job for JobNumber within {BrandingFactory.Instance.ProductName}, but hasn't saved it yet.\r\nPlease resubmit this message after the user has saved the Billing Job.",
					Loader.GetJobCreationErrorForService().Message);
			}
		}

		public void TestGetJobCreationErrorForService_NoMutex()
		{
			// This scenario can happen if mutex was released prior to message generation, or if caller uses WebDummySemaphoreProvider.
			AssertEquals(
				$"User (undefined) has created a Billing Job for JobNumber within {BrandingFactory.Instance.ProductName}, but hasn't saved it yet.\r\nPlease resubmit this message after the user has saved the Billing Job.",
				Loader.GetJobCreationErrorForService().Message);
		}

		public void TestJobCreationError_IsValidMutexError()
		{
			var msg = "Test Message with user EDI";
			var msgWithUndefinedUser = "Test Message with user (undefined)";

			AssertEquals(false, new JobHeader.Loader.JobCreationError(JobCreationErrorType.JobIsNotAllowed, msg).IsValidMutexError);
			AssertEquals(false, new JobHeader.Loader.JobCreationError(JobCreationErrorType.JobIsNotAllowed, msgWithUndefinedUser).IsValidMutexError);

			AssertEquals(true, new JobHeader.Loader.JobCreationError(JobCreationErrorType.MutexError, msg).IsValidMutexError);
			AssertEquals(false, new JobHeader.Loader.JobCreationError(JobCreationErrorType.MutexError, msgWithUndefinedUser).IsValidMutexError);
		}

		public void TestDisposeUnlocksMutex()
		{
			AssertEquals("Mutex not locked initially", false, Mutex.IsLocked);
			JobHeader job = Loader.TryCreateWithMutex();
			AssertEquals("Mutex locked", true, Mutex.IsLocked);
			job.Dispose();
			AssertEquals("Mutex unlocked after Dispose()", false, Mutex.IsLocked);
		}

		public void TestDisposeUnlocksMutexForActivateJob()
		{
			AssertEquals("Mutex not locked initially", false, Mutex.IsLocked);
			var job = Loader.TryCreateWithMutex();
			AssertEquals("Mutex locked", true, Mutex.IsLocked);
			Factory.Save();
			job.MarkAsInactive();
			job.Dispose();
			AssertEquals("Mutex unlocked after Dispose()", false, Mutex.IsLocked);

			job = Loader.TryCreateWithMutex();
			AssertEquals("Mutex locked", true, Mutex.IsLocked);
			job.Dispose();
			AssertEquals("Mutex unlocked after Dispose()", false, Mutex.IsLocked);
		}

		public void TestFactorySaveUnlocksMutex()
		{
			AssertEquals("Mutex not locked initially", false, Mutex.IsLocked);
			JobHeader job = Loader.TryCreateWithMutex();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "JobNumber";
			AssertEquals("Mutex locked", true, Mutex.IsLocked);

			Factory.Save();
			AssertEquals("Mutex unlocked after a Factory.Save();", false, Mutex.IsLocked);
		}

		public void TestIsGatewayBillingJob_IsGatewayLegacyJob()
		{
			// Gateway Legacy Job
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			AssertEquals(false, jobHeader.IsGatewayBillingJob());
			jobHeader.JH_JobNum = "JH_JobNum" + Core.Constants.GatewaySuffixForJobHeaderDeprecated;
			AssertEquals("IsGatewayLegacyJob", false, jobHeader.IsGatewayLegacyJob);
			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("IsGatewayLegacyJob", false, jobHeader.IsGatewayLegacyJob);
			Factory.Save();
			AssertEquals("IsGatewayLegacyJob should be true as now it satisfies all requirements", true, jobHeader.IsGatewayLegacyJob);
			AssertEquals("IsGatewayBillingJob should be true as it is Gateway Legacy Job", true, jobHeader.IsGatewayBillingJob());

			jobHeader.JH_JobNum = "JH_JobNum";
			AssertEquals("IsGatewayLegacyJob", false, jobHeader.IsGatewayLegacyJob);
			AssertEquals("IsGatewayBillingJob", false, jobHeader.IsGatewayBillingJob());

			// Parent is IGateway
			var jobHeaderParentGateway = Factory.New<DummyJobHeaderParentGateway>();
			AssertEquals("IsGateway", true, jobHeaderParentGateway.GatewayBillingSupporter.IsGatewayBillingEnabled());
			jobHeader.Parent = jobHeaderParentGateway;
			AssertEquals("IsGatewayLegacyJob", false, jobHeader.IsGatewayLegacyJob);
			AssertEquals("IsGatewayBillingJob", true, jobHeader.IsGatewayBillingJob());

			// Parent is a ForwardingConsol with Gateway Agent Type saved in database
			jobHeader.Parent = null;
			jobHeader.JH_JobNum = "JH_JobNum002";
			AssertEquals("IsGatewayLegacyJob", false, jobHeader.IsGatewayLegacyJob);
			AssertEquals("IsGatewayBillingJob", false, jobHeader.IsGatewayBillingJob());

			var consolInNewFactory = (BusinessObject)new BusinessObjectFactory().New<IForwardingConsol>();
			consolInNewFactory.FillWithValidTestData();
			consolInNewFactory[JobConsolSchema.JK_AgentType] = "AGT";
			consolInNewFactory[JobConsolSchema.JK_SendingForwarderHandlingType] = "GTA";
			consolInNewFactory.Factory.Save();

			var consolAsJobParent = Factory.Load(JobConsolSchema.Constants.Prefix, consolInNewFactory.PK) as IJobHeaderParent;
			AssertNotNull("Consol as IJobHeaderParent", consolAsJobParent);
			AssertEquals("IsGateway should be false as we did not set up GatewayAgent", false, consolAsJobParent.IsGatewayBillingEnabled());

			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeader.JH_ParentID = consolAsJobParent.PK;
			jobHeader.Parent = consolAsJobParent;
			AssertEquals("IsGateway should be true as we added a Job to the Consol", true, consolAsJobParent.IsGatewayBillingEnabled());
			AssertEquals("IsGatewayLegacyJob", false, jobHeader.IsGatewayLegacyJob);
			AssertEquals("IsGatewayBillingJob should be true as Job is associated with a Consol", true, jobHeader.IsGatewayBillingJob());
		}

		public void TestFactorySaveAddLicenceUsageForGatewayJob()
		{
			Env.Licence.GatewayBilling.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);
			var job = Loader.TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "JobNumber";

			AssertEquals("No GatewayBilling licence usage", 0, GetGatewayBillingLicenceUsageCount());
			Factory.Save();
			AssertEquals("No GatewayBilling licence usage", 0, GetGatewayBillingLicenceUsageCount());

			job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job.JH_JobNum = "JobNumber" + Core.Constants.GatewaySuffixForJobHeaderDeprecated;

			AssertEquals("No GatewayBilling licence usage", 0, GetGatewayBillingLicenceUsageCount());
			Factory.Save();
			AssertEquals("One GatewayBilling licence usage", 1, GetGatewayBillingLicenceUsageCount());

			Factory.Save();
			AssertEquals("No changes to the JobHeader", 1, GetGatewayBillingLicenceUsageCount());

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.FillWithValidTestData();

			Factory.Save();
			AssertEquals("No changes to the JobHeader", 1, GetGatewayBillingLicenceUsageCount());
		}

		int GetGatewayBillingLicenceUsageCount()
		{
			var filter = new ZQuery(StmActivityLogSchema.S7_ControllerID, SQLComparisonOperator.StartsWith, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString());
			filter.AddToFilter(StmActivityLogSchema.S7_FormCaption, SQLComparisonOperator.Equal, Env.Licence.GatewayBilling.Name);
			return Factory.GetDatabaseCount(typeof(StmActivityLog), filter);
		}

		#endregion

		#region IJobHeaderParent

		public void TestSetParentEvents()
		{
			var logs = new List<string>();

			var job = Factory.New<DummyJobHeader>();
			job.ProcessLogs = logs;

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			jobHeaderParent.ProcessLogs = logs;

			AssertEquals(0, logs.Count);
			job.Parent = jobHeaderParent;
			AssertEquals("JobCreating=>SetParentCore=>JobCreated", string.Join("=>", logs));
		}

		public void TestDeleteFiresDeletingEvets()
		{
			var logs = new List<string>();

			var job = Factory.NewJobForTesting<JobHeader>();

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			job.Parent = jobHeaderParent;
			jobHeaderParent.ProcessLogs = logs;

			AssertEquals(0, logs.Count);
			job.Delete();
			Assert(job.IsDeleted);
			AssertEquals("JobDeleting=>JobDeleted", string.Join("=>", logs));
			Assert(!jobHeaderParent.IsJobDeletedInJobDeleting);
			Assert(jobHeaderParent.IsJobDeletedInJobDeleted);

			job = Factory.NewJobForTesting<JobHeader>();
			AssertNull("Precondition for Parent", job.Parent);
			AssertNoExceptionThrown("Deleting throws no exception when Parent is null", () => job.Delete());
			Assert(job.IsDeleted);
		}

		#endregion

		#region IsManuallyCreated

		public void TestIsManuallyCreated()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Assert("Not set by default", !jobHeader.IsManuallyCreated);

			jobHeader.IsManuallyCreated = true;
			Assert(jobHeader.IsManuallyCreated);

			jobHeader.IsManuallyCreated = false;
			Assert("Cannot be reset back", jobHeader.IsManuallyCreated);

			Factory.Save();
			Assert("Reset on saving to db", !jobHeader.IsManuallyCreated);

			jobHeader.IsManuallyCreated = true;
			Assert("Cannot be set after saving", !jobHeader.IsManuallyCreated);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewBusinessObjectForDeleteTest(factory);
			factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewBusinessObject();
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		#endregion

		public void TestMarkJobAsInactive()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_IsActive = false;
			Assert(job.JH_IsActive);
			AssertEquals($"We should call MarkAsInactive or ActivateJob to modify JH_IsActive", ExceptionReporterTestListener.Instance[0].InnerException.Message);

			job.MarkAsInactive();
			Factory.Save();

			AssertNull("JobHeader is not be save to database.", Factory.Load<JobHeader>(job.PK));

			job = Factory.NewJobForTesting<JobHeader>();
			Factory.Save();
			job.MarkAsInactive();
			Factory.Save();
			Assert(!job.JH_IsActive);
			AssertNotNull("JobHeader is save to database.", Factory.Load<JobHeader>(job.PK));

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestHasInactiveJobHeader()
		{
			AssertEquals("Don't have inactive jobHeader in database", false, Loader.HasInactiveJobHeader(GlbCompany.CurrentCompany));

			var job = Loader.TryCreate();
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			Assert(Loader.HasInactiveJobHeader(GlbCompany.CurrentCompany));
		}

		public void TestDeleteJobIsNotAllowedWhenInDatabase()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "job1";
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>("JobHeader should not be deleteable when in database", () => { job.Delete(); });
		}

		public void TestDeleteJobIsNotAllowedWhenInDatabaseEvenForDtbBookingParent()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "job1";
			job.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>("JobHeader should not be deleteable when in database even for DtbBooking parent", () => { job.Delete(); });
		}

		public void TestMarkJobAsInactive_ReleaseMutex()
		{
			var job = Loader.TryCreate();
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			job = Loader.TryLoadOrCreateWithMutex();
			var mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);

			AssertEquals("Has mutex after activate job.", true, mutex.IsLocked);

			job.MarkAsInactive();
			mutex = JobHeader.GetMutex_ForTestOnly(Parent.PK);
			AssertEquals("Don't have mutex after deactivate job.", false, mutex.IsLocked);
		}

		public void TestJH_OA_LocalChargesAddr_CollectsInfo_WhenValueIsSet()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_LocalChargesAddrSetterCallStack);
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var newAddress = Factory.New<OrgAddress>().PK;
			jobHeader.JH_OA_LocalChargesAddr = newAddress;
			var expectedInfo = $"\r\nJobHeaderJH_OA_LocalChargesAddrSetterCallStack:\r\nJH_OA_LocalChargesAddr: Old Value: 00000000-0000-0000-0000-000000000000, New Value: {newAddress} , StackTrace ->";
			AssertContains("Should report error", expectedInfo, service.GetInfo(jobHeader.PK, CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_LocalChargesAddrSetterCallStack));
		}

		public void TestJH_OA_AgentCollectAddr_CollectsInfo_WhenValueIsSet()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_AgentCollectAddrSetterCallStack);
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var newAddress = Factory.New<OrgAddress>().PK;
			jobHeader.JH_OA_AgentCollectAddr = newAddress;
			var expectedInfo = $"\r\nJobHeaderJH_OA_AgentCollectAddrSetterCallStack:\r\nJH_OA_AgentCollectAddr: Old Value: 00000000-0000-0000-0000-000000000000, New Value: {newAddress} , StackTrace ->";
			AssertContains("Should report error", expectedInfo, service.GetInfo(jobHeader.PK, CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_AgentCollectAddrSetterCallStack));
		}

		[TestDate(2019, 12, 20, 11, 12, 13)]
		public void TestReportJobIsChangedByDifferentCompany_GetChangedBeforeFactorySave()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			Factory.Save();

			job.JH_Description += "A";
			Factory.Save();
			AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);

			job.JH_Description += "A";
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}
			AssertReportJobIsChangedByDifferentCompany(
				$"CurrentCompany: {AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.PK}({AccountingTestObjectCreator.NonCurrentCompanyBranch.Company.GC_Code})"
				, $"PK = {job.PK}"
				, "Fields with changes: JH_Description (A, AA)."
				, $"JobConstructorStackTrace:\r\nJob Created Time: 2019-12-20 11:12:13.000\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)");
		}

		public void TestReportJobIsChangedByDifferentCompany_HasUserContextSwitchLog()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			Factory.Save();

			job.JH_Description += "A";
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				job.RunPreSaveValidation();
				Factory.Save();
			}
			AssertReportJobIsChangedByDifferentCompany("No UserContextSwitchLog");

			job.JH_Description += "A";
			var userSwitchLog = new UserContextSwitchLogger();
			using (Env.StartContextSwitchTrace(userSwitchLog))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				job.RunPreSaveValidation();
				Factory.Save();
			}
			AssertReportJobIsChangedByDifferentCompany("OldUserContext Company: EDI, Branch: BNE, User: CWSupportNewUserContext Company: DEM, Branch: DEM, User: CWSupport   at Enterprise.Environment.Env.<>c__DisplayClass74_0.<StartContextSwitchTrace>g__HookUserContextChanges|0(Object sender, IUserContextChangingEventArgs args)");
		}

		public void TestReportJobIsChangedByDifferentCompany_HasSkippedValidation()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				job.RunPreSaveValidation();
				Factory.Save();
			}
			AssertReportJobIsChangedByDifferentCompany("BizObj Level : (HasBeenValidatedByDifferentCompany)");
		}

		public void TestReportJobIsChangedByDifferentCompany_HaveCvInfos()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}

			AssertReportJobIsChangedByDifferentCompany(
				"JobHeaderJH_OA_LocalChargesAddrSetterCallStack:"
				, "JobHeaderJH_OA_AgentCollectAddrSetterCallStack:"
				, "JobJH_GS_NKRepSalesSetterCallStack:"
				, "JobRegisteredAsEditableChild:"
				, "JobChargeLoadFirstInDatabase:"
				, "MarkJobHeaderAsInactive:");
		}

		public void TestReportJobIsChangedByDifferentCompany_SkipReportingForContext_JobDeactivationForAllCompanies()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			Factory.Save();

			job.JH_Description += "A";
			AssertEquals("PreCondition", true, job.HasChanges);
			job.SetContext(BusinessContext.JobDeactivationForAllCompanies);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
				AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);
			}

			job.JH_Description += "A";
			AssertEquals("PreCondition", true, job.HasChanges);
			job.RemoveContext(BusinessContext.JobDeactivationForAllCompanies);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
				AssertReportJobIsChangedByDifferentCompany();
			}
		}

		void AssertReportJobIsChangedByDifferentCompany(params string[] expectedMessages)
		{
			AssertEquals("We should only one exception for silent report.", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Silent Report Key", "JobIsChangedByDifferentCompany_3", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			
			AssertEquals("Silent Report Exception Header Message"
				, "Job is not designed to be changed in different company."
				, ExceptionReporterTestListener.Instance[0].Message);

			AssertContainsInOrder("Silent Report Message"
				, ExceptionReporterTestListener.Instance.GetExceptionMessage(0)
				, expectedMessages);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestReportJobIsChangedByDifferentCompany_When_JobIsChangedBeforeCallingOnFactorySavingBeforeTransactionCore()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, AccountingTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Action updateJobDescription = () => job.JH_Description += "A";
				const string expectedIssueKey = "JobIsChangedByDifferentCompany_3";

				job.Factory.Saving += (_) => updateJobDescription();
				Factory.Save();
				AssertEquals("Reported Exceptions Count", 0, ExceptionReporterTestListener.Instance.Count);
				job.Factory.Saving -= (_) => updateJobDescription();
				updateJobDescription();
				Factory.Save();
				AssertEquals("LastKeyReported", expectedIssueKey, ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestMarkAsInactive_AddCvInfo_MarkJobHeaderAsInactive()
		{
			var cvService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			cvService.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.MarkJobHeaderAsInactive);

			var job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			job.MarkAsInactive();

			AssertStartsWith("We should get stack trace for the MarkAsInactive event."
				, "\r\n" + "MarkJobHeaderAsInactive:\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)"
				, cvService.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.MarkJobHeaderAsInactive));
		}

		public void TestJH_IsDisbursement()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var loader = new JobHeader.Loader(Factory, shipment);
			var shipmentJob = loader.TryCreate();
			AssertEquals(false, ObjectFactory.Get<IAccounting>().Registry.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals("JH_IsDisbursement is false because EnableElectronicProcessingChargeFunctionality is false", false, shipmentJob.JH_IsDisbursement);

			((BusinessObject)shipment).FillWithValidTestData();
			Factory.Save();

			shipmentJob.MarkAsInactive();
			Factory.Save();

			var enableElectronicProcessingChargeFunctionality = ObjectFactory.Get<IAccounting>().Registry.EnableElectronicProcessingChargeFunctionality;
			enableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ObjectFactory.Get<IAccounting>().Registry.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			shipmentJob = loader.TryCreate();
			AssertEquals("JH_IsDisbursement is false because job is in database.", false, shipmentJob.JH_IsDisbursement);

			(Parent.InvoicingSupporter as DummyJobHeaderParentJobInvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			var nonShipmentjob = Loader.TryCreate();
			AssertNotEquals("job type is not included in Electronic Processing Charge Configuration.", JobShipmentSchema.Constants.Prefix, nonShipmentjob.JH_ParentTableCode);
			AssertEquals("JH_IsDisbursement is false because job type is not included in Electronic Processing Charge Configuration.", false, nonShipmentjob.JH_IsDisbursement);

			var mock = new Mock<IAccounting>();
			mock.Setup(x => x.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			mockRegistry.Setup(x => x.EnableElectronicProcessingChargeFunctionality).Returns(enableElectronicProcessingChargeFunctionality);
			mock.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(mock.Object))
			{
				shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
				loader = new JobHeader.Loader(Factory, shipment);
				shipmentJob = loader.TryCreate();
				AssertEquals(true, shipmentJob.JH_IsDisbursement);
			}
		}

		public void TestJH_IsDisbursement_ActionFieldReadonly()
		{
			var readOnlyAction = ActionFieldAttribute.Get(typeof(JobHeader).GetProperty("JH_IsDisbursement")).ReadOnly;
			AssertEquals("JH_IsDisbursement is Readonly ActionField.", true, readOnlyAction);
		}

		#region Creating/Saving Job Without Mutex

		public void TestDispose_NotCollectInfo_WhenJobIsInDatabase()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving);

			var job = CreateJobForSavingWithoutMutexForTests(false, createWithMutex: true);
			Assert("Precondition: IsInDatabase", !job.IsInDatabase);
			AssertDispose_CollectsInfoForJobDisposedBeforeSaving(job, service);

			job.Factory.Save();
			Assert("Precondition: IsInDatabase", job.IsInDatabase);
			job.Dispose();
			AssertContains("Should not report error", "There is no data collected", service.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving));
		}

		public void TestDispose_NotCollectInfo_WhenJobIsDeleted()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving);

			var job = CreateJobForSavingWithoutMutexForTests(false, createWithMutex: true);
			Assert("Precondition: IsDeleted", !job.IsDeleted);
			AssertDispose_CollectsInfoForJobDisposedBeforeSaving(job, service);

			job.Delete();
			Assert("Precondition: IsDeleted", job.IsDeleted);
			job.Dispose();
			AssertContains("Should not report error", "There is no data collected", service.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving));
		}

		public void TestDispose_NotCollectInfo_WhenMutexIsNull()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving);

			var job = CreateJobForSavingWithoutMutexForTests(false, createWithMutex: true);
			var mutex = JobHeader.GetMutexForTest(job);
			AssertNotNull("Precondition: Mutex", mutex);
			AssertDispose_CollectsInfoForJobDisposedBeforeSaving(job, service);

			job.Delete();
			job = CreateJobForSavingWithoutMutexForTests(createWithoutMutexForTestOnly: true);
			mutex = JobHeader.GetMutexForTest(job);
			AssertNull("Precondition: Mutex", mutex);
			job.Dispose();
			AssertContains("Should not report error", "There is no data collected", service.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving));
		}

		public void TestDispose_NotCollectInfo_WhenMutexHasNoLock()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving);

			var job = CreateJobForSavingWithoutMutexForTests(false, createWithMutex: true);
			var mutex = JobHeader.GetMutexForTest(job);
			Assert("Precondition: Mutex.HasLock", mutex.HasLock);
			AssertDispose_CollectsInfoForJobDisposedBeforeSaving(job, service);

			mutex = JobHeader.GetMutexForTest(job);
			Assert("Precondition: Mutex.HasLock", !mutex.HasLock);
			job.Dispose();
			AssertContains("Should not report error", "There is no data collected", service.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving));
		}

		void AssertDispose_CollectsInfoForJobDisposedBeforeSaving(JobHeader jobHeader, CriticalValidationInfoCollectorService criticalValidationInfoCollectorService)
		{
			var mutex = JobHeader.GetMutexForTest(jobHeader);

			Assert("Precondition: IsInDatabase", !jobHeader.IsInDatabase);
			Assert("Precondition: IsDeleted", !jobHeader.IsDeleted);
			AssertNotNull("Precondition: Mutex", mutex);
			Assert("Precondition: Mutex.HasLock", mutex.HasLock);

			jobHeader.Dispose();

			AssertContains("Should report error", @"
JobDisposedBeforeSaving:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at Enterprise.MasterFiles.Business.JobHeader", criticalValidationInfoCollectorService.GetInfo(jobHeader.PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving));

			criticalValidationInfoCollectorService.ClearServiceCache();
		}

		[TestDate(2019, 12, 20, 11, 12, 13)]
		public void TestOnSaving_ReportsErrorWithStackTrace()
		{
			var job = CreateJobForSavingWithoutMutexForTests(createWithMutex: true);

			job.Dispose();
			job.OnSaving();
			AssertEquals("Should report error", "JobIsBeenSavedWithoutMutexForSavedParent_3", ErrorReporter.LastKeyReported);
			AssertContainsInOrder("Should report error"
				, ErrorReporter.LastMessageReported
				, @"JobDisposedBeforeSaving:
   at System.Environment.GetStackTrace"
				, "Properties:"
				, @"JobConstructorStackTrace:
Job Created Time: 2019-12-20 11:12:13.000
   at System.Environment.GetStackTrace");
			AssertEquals("Should report error", $@"
There are 2 rules for job mutexes:
1. Job either should be deleted or saved, you do not need to call Dispose() in this case.
2. Dispose() have to be called only if you never going to save its factory, like cancelling form or going out of scope of its factory.", ErrorReporter.LastExceptionReported.Message);

			ErrorReporter.Clear();
		}

		public void TestJobOnSave_WithMutexNotReportErrorAndWithoutMutexReportError()
		{
			var jobHeader1 = CreateJobForSavingWithoutMutexForTests(createWithMutex: true);
			Assert("Precondition: jobHeader1.IsInDatabase", !jobHeader1.IsInDatabase);
			AssertNotNull("Precondition: jobHeader1.Parent", jobHeader1.Parent);
			Assert("Precondition: jobHeader1.Parent.IsInDatabase", jobHeader1.Parent.IsInDatabase);

			var mutex = JobHeader.GetMutexForTest(jobHeader1);
			AssertNotNull("Precondition: Mutex", mutex);
			Assert("Precondition: Mutex.HasLock", mutex.HasLock);

			Factory.Save();
			AssertEquals("Should not report error", string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("Should not report error", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Should not report error", null, ErrorReporter.LastExceptionReported);

			var jobHeader2 = CreateJobForSavingWithoutMutexForTests(createWithMutex: false);
			Assert("Precondition: jobHeader2.IsInDatabase", !jobHeader2.IsInDatabase);
			AssertNotNull("Precondition: jobHeader2.Parent", jobHeader2.Parent);
			Assert("Precondition: jobHeader2.Parent.IsInDatabase", jobHeader2.Parent.IsInDatabase);

			mutex = JobHeader.GetMutexForTest(jobHeader2);
			AssertNull("Precondition: Mutex", mutex);

			Factory.Save();
			AssertEquals("Should report error", "JobIsBeenSavedWithoutMutexForSavedParent_3", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestOnSaving_JobIsBeenSavedWithoutMutexNotReported_WhenParentIsNull()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S00001111";
			AssertNull("Precondition: Parent", jobHeader.Parent);
			jobHeader.OnSaving();
			AssertEquals("When job parent is null", string.Empty, ErrorReporter.LastKeyReported);

			jobHeader = CreateJobForSavingWithoutMutexForTests();
			jobHeader.OnSaving();
			AssertEquals("When job parent is NOT null", "JobIsBeenSavedWithoutMutexForSavedParent_3", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestOnSaving_JobIsBeenSavedWithoutMutexNotReported_WhenParentIsNotInDatabase()
		{
			var jobHeader = CreateJobForSavingWithoutMutexForTests(saveParent: false);
			AssertEquals("Precondition: Parent.IsInDatabase", false, jobHeader.Parent.IsInDatabase);
			jobHeader.OnSaving();
			AssertEquals("When job parent is NOT in DB", string.Empty, ErrorReporter.LastKeyReported);

			jobHeader = CreateJobForSavingWithoutMutexForTests(saveParent: true);
			AssertEquals("Precondition: Parent.IsInDatabase", true, jobHeader.Parent.IsInDatabase);
			jobHeader.OnSaving();
			AssertEquals("When job parent is in DB", "JobIsBeenSavedWithoutMutexForSavedParent_3", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestOnSaving_JobIsBeenSavedWithoutMutexNotReported_WhenMutexHasLock()
		{
			var jobHeader = CreateJobForSavingWithoutMutexForTests(createWithMutex: true);
			var mutex = JobHeader.GetMutexForTest(jobHeader);
			AssertEquals("Precondition: Mutex.HasLock", true, mutex.HasLock);
			jobHeader.OnSaving();
			AssertEquals("When mutex has lock", string.Empty, ErrorReporter.LastKeyReported);

			jobHeader = CreateJobForSavingWithoutMutexForTests(createWithMutex: false);
			mutex = JobHeader.GetMutexForTest(jobHeader);
			AssertNull("Precondition: Mutex", mutex);
			jobHeader.OnSaving();
			AssertEquals("When mutex has no lock", "JobIsBeenSavedWithoutMutexForSavedParent_3", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestOnSaving_JobIsBeenSavedWithoutMutexNotReported_WhenJobHeaderIsInDatabase()
		{
			var jobHeader = CreateJobForSavingWithoutMutexForTests(createWithMutex: false);
			Factory.Save();
			AssertEquals("Precondition: Job is in DB", true, jobHeader.IsInDatabase);
			ErrorReporter.Clear();
			jobHeader.OnSaving();
			AssertEquals("When job is in DB", string.Empty, ErrorReporter.LastKeyReported);

			jobHeader = CreateJobForSavingWithoutMutexForTests(createWithMutex: false);
			AssertEquals("Precondition: Job is not in DB", false, jobHeader.IsInDatabase);
			jobHeader.OnSaving();
			AssertEquals("When job is not in DB", "JobIsBeenSavedWithoutMutexForSavedParent_3", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Dispose and Prevent Save

		public void TestDisposeAndPreventSave()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			AssertEquals(false, jobHeader.IsDisposed);
			AssertEquals(false, Factory.HasContext(BusinessContext.JobIsDisposedShouldPreventSave));

			jobHeader.DisposeAndPreventSave();

			AssertEquals(true, jobHeader.IsDisposed);
			AssertEquals(true, Factory.HasContext(BusinessContext.JobIsDisposedShouldPreventSave));
		}

		public void TestDisposeAndDeleteNew()
		{
			var newJobHeader = CreateValidJobHeaderForSave();
			AssertDisposeAndDeleteNew(newJobHeader, true);

			var persistedJobHeader = CreateValidJobHeaderForSave();
			Factory.Save();
			AssertDisposeAndDeleteNew(persistedJobHeader, false);
		}

		void AssertDisposeAndDeleteNew(JobHeader jobHeader, bool shouldBeDeleted)
		{
			AssertEquals(false, jobHeader.IsDisposed);
			AssertEquals(false, jobHeader.IsDeleted);

			jobHeader.DisposeAndDeleteNew();

			AssertEquals(true, jobHeader.IsDisposed);
			AssertEquals(shouldBeDeleted, jobHeader.IsDeleted);
		}

		JobHeader CreateValidJobHeaderForSave()
		{
			var jobHeaderParent = Factory.New<IForwardingShipment>();
			var job = Factory.NewWithValidTestData<DummyJobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = jobHeaderParent.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_JobNum = "S000001";

			return job;
		}

		public void TestCheckWhetherIsEligibleToSave()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.DisposeAndPreventSave();

			var exception = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
			AssertEquals("Cannot save the Invoicing Job Header because it's already disposed.\r\nThis may due to errors occurred before closing the form.\r\n\r\nPlease reload current form to continue.",
							exception.Message);
			AssertEquals("Cannot Save Invoicing Job Header", exception.Heading);
		}

		#endregion

		#region Test Classes

		class TestJobHeader : JobHeader
		{
			public TestJobHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
			{
				get { return base.UniqueIndexFailureHandlers; }
			}

			protected override bool IsChargesCollectionLoaded
			{
				get { return false; }
			}

			public override ZDecimal JH_TotalProfitRevenueMargin => throw new NotImplementedException();

			protected override ZGuid GetGSTIDCore(OrgHeader org, AccChargeCode chargeCode, GlbBranch branch, CostSell costOrSell, ILocation fixedPlaceOfSupplyLocation, ZString supplyType, out ZGuid overrideInvTaxMsg)
			{
				overrideInvTaxMsg = new ZGuid("1E8BE15B-D917-430E-9D0C-EEB5709F6C7B");
				return new ZGuid("56077822-3783-498B-9D8D-C99844B3C7CC");
			}
		}

		class TestNotificationHandler : INotificationHandler
		{
			public string LastErrorMessage;
			public string LastErrorCaption;
			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				LastErrorMessage = message;
				LastErrorCaption = caption;
			}

			public string LastInformationMessage;
			public string LastInformationCaption;
			void INotificationHandler.ReportInformation(string message, string caption)
			{
				LastInformationMessage = message;
				LastInformationCaption = caption;
			}
		}

		#endregion

		#region Implementation

		readonly TestNotificationHandler NotificationHandler = new TestNotificationHandler();

		JobHeader.Loader Loader
		{
			get { return loader ?? (loader = new JobHeader.Loader(Parent)); }
		}
		JobHeader.Loader loader;

		DummyJobHeaderParent Parent
		{
			get { return parent ?? (parent = Factory.New<DummyJobHeaderParent>()); }
		}
		DummyJobHeaderParent parent;

		ZGlobalMutex Mutex
		{
			get { return JobHeader.GetMutex_ForTestOnly(Parent.PK); }
		}

		protected virtual Type GetExpectedJobTypeToLoad()
		{
			return GetExpectedBusinessObjectType();
		}

		GlbStaff CreateStaff(string loginName, string password, string code, bool active, bool resource, bool isOperational = true, bool isController = false, bool canLogin = true)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "ABC";
			staff.GS_FullName = "ABC DEF";
			staff.GS_UserAddress1 = "GHI";
			staff.GS_IsResource = resource;
			staff.GS_CanLogin = canLogin;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.StaffPlainTextPassword = password;
			staff.GS_IsActive = active;
			staff.GS_IsOperational = isOperational;
			staff.GS_IsController = isController;
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(staff.PK);
			Factory.Save();
			return staff;
		}

		void SetPermission(GlbStaff staff, ZGuid companyPK, ZGuid branchPK, ZGuid deptPK, string displaytext, bool isAllowed)
		{
			foreach (SecurityCheckpoint checkPoint in Env.Security.AllLoadedCheckPoints)
			{
				if (checkPoint.DisplayText.ToString().Contains(displaytext))
				{
					GlbSecurity securityRecord = Factory.New<GlbSecurity>();
					securityRecord.GU_SecurityRight = checkPoint.Code;
					securityRecord.GU_ItemGUID = checkPoint.ItemGuid;
					securityRecord.GU_SecurityItemIsAllowed = isAllowed;
					securityRecord.GU_GS = staff.PK;
					securityRecord.GU_GC = companyPK;
					securityRecord.GU_GB = branchPK;
					securityRecord.GU_GE = deptPK;

					staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
				}
			}
		}

		#endregion

		JobHeader CreateJobForSavingWithoutMutexForTests(bool saveParent = true, bool createWithMutex = false, bool createWithoutMutexForTestOnly = false)
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			if (saveParent)
			{
				shipment.FillWithValidTestData();
				Factory.Save();
			}

			var loader = new JobHeader.Loader((IJobHeaderParent)shipment);
			var job = createWithMutex ? loader.TryCreateWithMutex() : (createWithoutMutexForTestOnly ? loader.TryCreateWithoutMutexForTestOnly() : loader.TryCreate());
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return job;
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => (accountingTestObjectCreator ??= new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
