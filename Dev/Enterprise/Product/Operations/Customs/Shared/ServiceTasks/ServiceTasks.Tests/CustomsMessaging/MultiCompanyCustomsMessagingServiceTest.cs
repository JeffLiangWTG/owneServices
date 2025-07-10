using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class MultiCompanyCustomsMessagingServiceTest : TestCaseWithFactory
	{
		[ExpectException(typeof(HostedServiceException))]
		public void TestWhenLegacyDirectorRunning()
		{
			SimulateServiceTasksActive();
			serviceTask.RunTask();
		}

		[ExpectNoExceptions]
		public void TestWhenLegacyDirectorNotRunning()
		{
			GlbCompany nzCompany = Factory.New<GlbCompany>();
			nzCompany.FillWithValidTestData();
			nzCompany.GC_Code = "NZ1";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			GlbBranch nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.NewZealand)).RL_Code;
			nzBranch.GB_Code = "NZ1";
			GlbCompany auCompany = Factory.New<GlbCompany>();
			auCompany.FillWithValidTestData();
			auCompany.GC_Code = "AU2";
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch auBranch = auCompany.Branches.AddNew();
			auBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia)).RL_Code;
			auBranch.GB_Code = "AU2";
			Factory.Save();
			SimulateServiceTasksInActive();
			serviceTask.RunTask();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "EDI", "AU2" }, serviceTask.CalledCompany);
		}

		ServiceManagerQuerierTester serviceManagerQuerier;
		MultiCompanyCustomsMessagingServiceTestHelper serviceTask;
		protected override void SetUp()
		{
			base.SetUp();
			serviceManagerQuerier = new ServiceManagerQuerierTester();
			ObjectFactory.Substitute<IServiceManagerQuerier>(serviceManagerQuerier);
			serviceTask = new MultiCompanyCustomsMessagingServiceTestHelper();
			serviceTask.ServiceLogger = new TestServiceLogger();
		}

		void SimulateServiceTasksActive()
		{
			serviceManagerQuerier.CurrentServiceTaskStatus = ServiceTaskStatus.AtLeastOneHostIsRunningHealthily;
		}

		void SimulateServiceTasksInActive()
		{
			serviceManagerQuerier.CurrentServiceTaskStatus = ServiceTaskStatus.ServiceTaskIsInactive;
		}
	}

	public class MultiCompanyCustomsMessagingServiceTestHelper : MultiCompanyCustomsMessagingService
	{
		protected override string RequiredCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override ZString LegacyBatchProcessorCode
		{
			get { return "XXX"; }
		}

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			return new BatchProcessHelper(this);
		}

		public List<ZString> CalledCompany
		{
			get
			{
				if (calledCompany == null)
				{
					calledCompany = new List<ZString>();
				}
				return calledCompany;
			}
		}
		List<ZString> calledCompany;
	}

	public class BatchProcessHelper : ICustomsServiceTaskProcess
	{
		public BatchProcessHelper(MultiCompanyCustomsMessagingServiceTestHelper helper)
		{
			this.helper = helper;
		}
		readonly MultiCompanyCustomsMessagingServiceTestHelper helper;

		public void ExecuteBatch(CancellationToken unused = new CancellationToken())
		{
			helper.CalledCompany.Add(Env.CurrentCompany.Code);
		}

		public void Dispose()
		{
		}

		public LoggingInformation Logger { get; set; }
	}

	public class ServiceManagerQuerierTester : IServiceManagerQuerier
	{
		internal ServiceTaskStatus CurrentServiceTaskStatus { get; set; }

		public ServiceTaskStatus CheckStateOfNamedServiceTask(string codeOfServiceTaskToCheck)
		{
			return CurrentServiceTaskStatus;
		}

		public bool TryGetServiceTaskNextRunTime(string code, out DateTimeOffset? nextRunTime) { throw new NotImplementedException(); }

		public bool TryGetServiceTaskScheduleState(string code, out string scheduleState) { throw new NotImplementedException(); }

		public bool TryGetServiceTaskBranchPK(string code, out Guid branchPK) { throw new NotImplementedException(); }

		public IEnumerable<string> GetServiceTasksByCategory(string category) { throw new NotImplementedException(); }
	}

	public abstract class BaseMultiCompanyCustomsMessagingServiceTest<T> : ServiceTaskTestCase<T> where T : MultiCompanyCustomsMessagingService, new()
	{
		protected MultiCompanyCustomsMessagingService testServiceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			testServiceTask = new T();
			testServiceTask.ServiceLogger = new TestServiceLogger();
		}
	}
}
