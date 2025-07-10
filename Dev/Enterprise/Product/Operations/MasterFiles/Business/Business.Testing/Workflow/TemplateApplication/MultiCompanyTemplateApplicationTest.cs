using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MultiCompanyTemplateApplicationTest : TemplateApplicationTestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
		}

		public CompanyTestProvider SetupCompanies(int count)
		{
			return new CompanyTestProvider(Factory, count);
		}

		#endregion

		void TestAreTasksShared(int expectedTasksPerCompany, int totalTaskCount, bool isGlobal, int companyCount)
		{
			var companies = SetupCompanies(companyCount);
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			if (isGlobal)
			{
				var template = MakeTemplate(isGlobal: isGlobal);
				var task1 = MakeTask(template, isShared: true, udfCondition: "\"1\"==\"1\"");
				var task2 = MakeTask(template, isShared: false, udfCondition: "\"1\"==\"1\"");
				Factory.Save();
			}
			else
			{
				companies.ForAllCompanies(_ =>
				{
					var template = MakeTemplate(isGlobal: isGlobal);
					var task1 = MakeTask(template, isShared: true, udfCondition: "\"1\"==\"1\"");
					var task2 = MakeTask(template, isShared: false, udfCondition: "\"1\"==\"1\"");
					Factory.Save();
				});
			}

			companies.ForAllCompanies(_ =>
			{
				var newFactory = Factory.CreateNewFactory();
				var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
				loadedDummy.ApplyWorkflowTemplates();
				newFactory.Save();
			});

			companies.ForAllCompanies(_ =>
			{
				var newFactory = Factory.CreateNewFactory();
				var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
				AssertEquals(expectedTasksPerCompany, loadedDummy.WorkflowItems.Tasks.Count);
			});

			AssertEquals(totalTaskCount, Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, dummy.PK)).Length);
		}

		public void TestTaskVisibility_Shared_NonGlobal()
		{
			TestAreTasksShared(expectedTasksPerCompany: 5, totalTaskCount: 8, isGlobal: false, companyCount: 4);
		}

		public void TestTaskVisibility_Shared_Global()
		{
			TestAreTasksShared(expectedTasksPerCompany: 2, totalTaskCount: 5, isGlobal: true, companyCount: 4);
		}

		public void TestEmptyFallback_AfterPartialApplication_DoesNotFallBackWhenMultiCompany()
		{
			var companies = SetupCompanies(2);
			Factory.Save();

			var template1 = MakeTemplate(isGlobal: true);
			template1.P0_TriggerFallbackMethod = FallbackTypeList.Codes.EmptyFallback;
			template1.P0_SubType1 = "FOO";
			// Add a trigger we expect to apply
			var trigger1 = MakeTrigger(template1, Events.CustomisableEvent01Code, udfCondition: "\"1\"==\"1\"");

			// And a trigger that should never apply
			var template2 = MakeTemplate(isGlobal: true);
			var trigger2 = MakeTrigger(template2, Events.CustomisableEvent03Code, udfCondition: "\"1\"==\"1\"");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.SubType1 = "FOO";
			dummy.ApplyWorkflowTemplates();
			Factory.Save();

			companies.ForAllCompanies(_ =>
			{
				var newFactory = Factory.CreateNewFactory();
				var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
				loadedDummy.SubType1 = "FOO";
				loadedDummy.ApplyWorkflowTemplates();
				newFactory.Save();
			});

			var finalFactory = Factory.CreateNewFactory();
			var finalDummy = finalFactory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals(1, finalDummy.WorkflowItems.Triggers.Count);
		}

		public void TestEmptyFallback_AfterPartialApplication_DoesFallBackWhenUDFDoesNotMatch()
		{
			var companies = SetupCompanies(2);
			Factory.Save();

			var template1 = MakeTemplate(isGlobal: true);
			template1.P0_TriggerFallbackMethod = FallbackTypeList.Codes.EmptyFallback;
			template1.P0_SubType1 = "FOO";

			// Add a trigger that we need to fall past.
			MakeTrigger(template1, Events.CustomisableEvent01Code, udfCondition: "\"1\"==\"2\"");

			// Add a trigger to always apply
			var template2 = MakeTemplate(isGlobal: true);
			MakeTrigger(template2, Events.CustomisableEvent03Code, udfCondition: "\"1\"==\"1\"");

			// Add two triggers that only apply in a specific company.
			MakeTrigger(template2, Events.CustomisableEvent04Code, udfCondition: "\"<CompanyCode>\"==\"" + companies.Companies.First().GC_Code + "\"");
			MakeTrigger(template2, Events.CustomisableEvent05Code, udfCondition: "\"<CompanyCode>\"==\"" + companies.Companies.First().GC_Code + "\"");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.SubType1 = "FOO";
			dummy.ApplyWorkflowTemplates();
			AssertEquals("It should have only created one of the triggers", 1, dummy.WorkflowItems.Triggers.Count);
			Factory.Save();

			companies.ForAllCompanies(_ =>
			{
				var newFactory = Factory.CreateNewFactory();
				var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
				loadedDummy.SubType1 = "FOO";
				loadedDummy.ApplyWorkflowTemplates();
				newFactory.Save();
			});

			var finalFactory = Factory.CreateNewFactory();
			var finalDummy = finalFactory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals("It should have created the company specific trigger in addition to the previous triggers", 3, finalDummy.WorkflowItems.Triggers.Count);
		}

		[TestDate(2018, 7, 3)]
		public void TestFireWorkflow_CompanySwitchDuringEdit()
		{
			AssertFire(dummy => dummy.Logs.AddNew(Events.CustomisableEvent01));
		}

		[TestDate(2018, 7, 3)]
		public void TestFireWorkflow_CompanySwitchDuringEdit_ExternalLogSource()
		{
			AssertFire(_ => Factory.NewWithValidTestData<DummyWithWorkflow>().Logs.AddNew(Events.CustomisableEvent01));
		}

		void AssertFire(Func<DummyWithWorkflow, StmALog> logSource)
		{
			var companies = SetupCompanies(2);
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var relatedDummy = logSource(dummy);
			var trigger = MakeTrigger(dummy, Events.CustomisableEvent01Code);
			MakeNotification(trigger);

			var log = relatedDummy;
			((IBaseTrigger)trigger).Fire(null, log);

			var wteQuery = new ZQuery(StmALogSchema.SL_Parent, trigger.PK)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			AssertEquals("Should only have triggered once.", 1, Factory.Load<StmALog>(wteQuery).Length);

			companies.ForAllCompanies(c =>
			{
				((IBaseTrigger)trigger).Fire(null, log);
			});

			AssertEquals("Should only have triggered once.", 1, Factory.Load<StmALog>(wteQuery).Length);
		}

		public void TestApplyTemplateInTheFuture()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var companies = SetupCompanies(2);
			var c1 = companies[0];
			var c2 = companies[1];

			c1.Branch.GB_RN_NKCountryCode = "AU";
			c1.Branch.GB_RL_NKHomePort = "AUGFN"; // Grafton (NSW) Port

			c2.Branch.GB_RL_NKHomePort = "US";
			c2.Branch.GB_RL_NKHomePort = "USNYC"; // New York port

			Factory.Save();

			DummyWithWorkflow dummy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, c1.Branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var factory = Factory.CreateNewFactory();
				dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, c2.Branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var factory2 = Factory.CreateNewFactory();
				var template = MakeTemplate(factory2);
				var m1 = MakeMilestone(template, Events.AddedARecordToTheSystemCode);
				MakeNotification(m1);
				factory2.Save();

				dummy = factory2.Load<DummyWithWorkflow>(dummy.PK);
				dummy.ApplyWorkflowTemplates();
				factory2.Save();
				AssertEquals(true, dummy.WorkflowItems.Milestones[0].P9_ActualDateForBinding.IsValid);
			}
		}
	}
}
