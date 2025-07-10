using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetAdditionalRootType_Template()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template.P0_IsUniversal = true;

			var workItem = template.WorkflowItems.AddNew();
			workItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			CombineAssertions(() =>
			{
				AssertEquals(true, workItem.IsTemplate);
				var rootTypes = workItem.GetRootTypes();
				AssertEquals(2, rootTypes.Length);
				AssertEquals("Enterprise.Freight.Forwarding.Business.ForwardingShipment", rootTypes[0].FullName);
				AssertEquals("Enterprise.MasterFiles.Business.TemplateProcessTask", rootTypes[1].FullName);

				var triggerConditions = workItem as IBaseTrigger;
				triggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;

				var templateConditions = new TemplateConditionsViewModel(workItem, template);
				templateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached;
				templateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

				rootTypes = workItem.GetRootTypes();
				AssertEquals(3, rootTypes.Length);
				AssertEquals("Enterprise.Freight.Forwarding.Business.ForwardingShipment", rootTypes[0].FullName);
				AssertEquals("Enterprise.Customs.Business.BaseJobDeclaration", rootTypes[1].FullName);
				AssertEquals("Enterprise.MasterFiles.Business.TemplateProcessTask", rootTypes[2].FullName);
			});
		}

		public void TestGetAdditionalRootType_Job()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = Factory.New<DummyTaskWithShipmentType>();
			dummy.WorkflowItems.Tasks.Add(task);
			task.P9_ParentID = dummy.PK;
			task.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template.P0_IsUniversal = true;

			CombineAssertions(() =>
			{
				AssertEquals(false, task.IsTemplate);
				AssertArrayEqualsByElements("No additional type", new[] { dummy.GetType(), task.GetType() }, task.GetRootTypes());

				var triggerConditions = task as IBaseTrigger;
				triggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
				AssertEquals("PreCondition: company is set as default as AU when new task", "AU", triggerConditions.GetCompany().GC_RN_NKCountryCode);

				var templateConditions = new TemplateConditionsViewModel(task, template);
				templateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached;
				templateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				AssertArrayEqualsByElements("Has additional type", new[] { dummy.GetType().FullName, task.GetType().FullName, "Enterprise.Customs.AU.Declaration.Business.JobDeclaration" }, task.GetRootTypes().Select(x => x.FullName).ToArray());
			});
		}

		sealed class DummyTaskWithShipmentType : DummyTask
		{
			public DummyTaskWithShipmentType(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string WorkflowTypeCore => "SHP"; // mock shipment workflow in order to get NON-null GetAdditionalRootTypeForTemplate() in ForwardingShipmentWorkflowDescriptor as ForwardingShipment type can't be referred directly
		}

		public void TestGetCountrySpecificTypeIfApplicable()
		{
			var task = Factory.New<DummyTask>();
			task.P9_LineTriggerType = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;
			AssertEquals("No CountrySpecificType", "Enterprise.ProcessManagement.Business.WorkItem", task.GetCountrySpecificTypeIfApplicable().ToString());

			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry("CA");
			AssertEquals("No CountrySpecificType", "Enterprise.ProcessManagement.Business.WorkItem", task.GetCountrySpecificTypeIfApplicable().ToString());

			task.P9_LineTriggerType = WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;
			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry("AU");
			AssertEquals("Has CountrySpecificType", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration", task.GetCountrySpecificTypeIfApplicable().ToString());

			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry("CA");
			AssertEquals("Has CountrySpecificType", "Enterprise.Customs.CA.Business.JobDeclaration", task.GetCountrySpecificTypeIfApplicable().ToString());
		}

		public void TestCancelCollection_IfCollectionIsModified_ExceptionIsNotThrown()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = Factory.New<DummyTask>();

			dummy.WorkflowItems.Tasks.Add(task);
			task.P9_ParentID = dummy.PK;

			AssertNoExceptionThrown(() => WorkflowExtensionMethods.CancelNonStartedTasksAndClosePartiallyCompletedTasks(dummy));
		}

		public void TestIsTask()
		{
			var job = Factory.New<OrgHeader>();

			var task = job.WorkflowItems.Tasks.AddNew();
			var standaloneTask = Factory.New<ProcessTask>();

			var milestone = job.WorkflowItems.Milestones.AddNew();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(true, task.IsTask());
			AssertEquals(true, standaloneTask.IsTask());

			AssertEquals(false, milestone.IsTask());
			AssertEquals(false, trigger.IsTask());
			AssertEquals(false, exception.IsTask());
		}

		public void TestIsQualityContainmentBarrierTask()
		{
			ObjectFactory.Get<IBMTestHelper>().EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "WKI");

			var parent1 = (IWorkflowProvider)Factory.New<IWorkItem>();
			var parent2 = (IWorkflowProvider)Factory.New<IProject>();

			var task1_1 = parent1.WorkflowItems.Tasks.AddNew();
			var task1_2 = parent1.WorkflowItems.Tasks.AddNew();
			var task2_1 = parent2.WorkflowItems.Tasks.AddNew();
			var task2_2 = parent2.WorkflowItems.Tasks.AddNew();

			task1_1.P9_Type = "QCB";
			task2_1.P9_Type = "QCB";

			task1_2.P9_Type = "UDF";
			task2_2.P9_Type = "UDF";

			AssertEquals(true, task1_1.IsQualityContainmentBarrierTask());
			AssertEquals(false, task1_2.IsQualityContainmentBarrierTask());

			AssertEquals(false, task2_1.IsQualityContainmentBarrierTask());
			AssertEquals(false, task2_2.IsQualityContainmentBarrierTask());
		}

		public void TestDoNotLoadUnRelatedRows()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template.P0_IsUniversal = true;

			var trig1 = template.TemplateTriggers.AddNew();
			trig1[ProcessTemplateTriggerSchema.P9T_Description] = "JAPAN";
			trig1[ProcessTemplateTriggerSchema.P9T_SE_NKTriggerEvent] = Events.CustomisableEvent00Code;
			var trig1Action = (ProcessTaskNotification)((ITemplateTrigger)trig1).TriggerActions.AddNew();
			trig1Action.PQ_TriggerType = "NTF";
			trig1Action.PQ_Calc_TriggerParty = "EML";
			trig1Action.PQ_EmailAddr = "JAPAN@ItsJapansTime.ToGetEmail";

			Factory.Save();

			for (int i = 0; i < 20; i++)
			{
				var d = Factory.New<DummyWithWorkflow>();
				d.Logs.AddNew(Events.CustomisableEvent00);
			}
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var newDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals(1, newDummy.WorkflowItems.TriggersIncludingRelated.Count);
			newFactory.Load<IProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.PK, ZGuid.NewZGuid()));
			AssertEquals(1, newFactory.Load<IProcessJobTriggerLink>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		class DummyTask : DummyProcessTask
		{
			public DummyTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString P9_Status
			{
				get => base.P9_Status;
				set
				{
					Parent?.WorkflowItems.Tasks.Add(Factory.New<DummyProcessTask>());
					base.P9_Status = value;
				}
			}
		}
	}
}
