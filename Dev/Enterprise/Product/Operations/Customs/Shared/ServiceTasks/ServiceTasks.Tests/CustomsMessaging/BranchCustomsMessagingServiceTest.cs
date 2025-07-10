using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class BranchCustomsMessagingServiceTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.FillWithValidTestData();
			company1.GC_Code = "NZ1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var company1Branch1 = company1.Branches.AddNew();
			company1Branch1.GB_RL_NKHomePort = "NZAKL";
			company1Branch1.GB_Code = "NZ1";
			var company1Branch2 = company1.Branches.AddNew();
			company1Branch2.GB_RL_NKHomePort = "NZAKL";
			company1Branch2.GB_Code = "NZ2";
			var company2 = Factory.New<GlbCompany>();
			company2.FillWithValidTestData();
			company2.GC_Code = "AU2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var company2Branch1 = company2.Branches.AddNew();
			company2Branch1.GB_RL_NKHomePort = "AUSYD";
			company2Branch1.GB_Code = "AB1";
			var company2Branch2 = company2.Branches.AddNew();
			company2Branch2.GB_RL_NKHomePort = "AUSYD";
			company2Branch2.GB_Code = "AB2";
			var company2Branch3 = company2.Branches.AddNew();
			company2Branch3.GB_RL_NKHomePort = "AUSYD";
			company2Branch3.GB_Code = "AB3";
			Factory.Save();
			using (Enterprise.ZArchitecture.Environment.DataRegistry.Instance.RawRegistry.EnableCustomsDiagnostics.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = new BranchCustomsMessagingServiceTestHelper(new Guid[] { company1Branch2.PK.ToGuid(), company2Branch2.PK.ToGuid() });
				var logger = new TestServiceLogger();
				serviceTask.ServiceLogger = logger;
				serviceTask.RunTask();
				AssertContainsExactElementsInAnyOrder(new[] { company1Branch2.PK, company2Branch2.PK }, serviceTask.ProcessedBranchPKs.ToArray());
				AssertContains("Processing company 'AU2' branch 'AB2'...", logger.ToString());
			}
		}
	}

	class BranchCustomsMessagingServiceTestHelper : BranchCustomsMessagingService
	{
		public BranchCustomsMessagingServiceTestHelper(Guid[] branchPKs)
		{
			this.branchPKs = branchPKs;
		}
		readonly Guid[] branchPKs;
		public List<ZGuid> ProcessedBranchPKs => processedBranchPKs ?? (processedBranchPKs = new List<ZGuid>());
		List<ZGuid> processedBranchPKs;

		protected override void Process(CancellationToken token)
		{
			ProcessedBranchPKs.Add(GlbBranch.CurrentBranch.PK);
			base.Process(token);
		}

		protected override ICustomsServiceTaskProcess GetNewProcess() => new CustomsServiceTaskProcessTestHelper();
		protected override Guid[] GetBranchPKsHavingDataToProcess() => branchPKs;
	}

	class CustomsServiceTaskProcessTestHelper : ICustomsServiceTaskProcess
	{
		public LoggingInformation Logger { get; set; }

		public void Dispose()
		{
		}

		public void ExecuteBatch(CancellationToken token)
		{
		}
	}
}
