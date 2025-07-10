using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkflowLinkTemplateApplicator))]
	public class WorkflowLinkTemplateApplicatorTest : TestCaseWithFactory
	{
		public void TestApplyTemplates()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.CreateSystem(Factory, "WKI");
			bmsTestHelper.CreateSystem(Factory, "WKP");
			bmsTestHelper.EnableBMSInRegistry();
			var workItemPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var projectPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItemCode;
			projectPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.ProjectCode;
			workItemPartialTemplate.P0_Description = "WKI PT";
			projectPartialTemplate.P0_Description = "WKP PT";
			workItemPartialTemplate.P0_IsPartialTemplate = projectPartialTemplate.P0_IsPartialTemplate = true;
			var workItemTemplateWorkflowHeader = workItemPartialTemplate.ProcessHeaders.AddNew();
			var projectTemplateHeader = projectPartialTemplate.ProcessHeaders.AddNew();
			var task1 = workItemPartialTemplate.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workItemTemplateWorkflowHeader.PK;
			var task2 = projectPartialTemplate.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = projectTemplateHeader.PK;
			workItemTemplateWorkflowHeader.FH_CompletionStatement = "ESC AAA";
			projectTemplateHeader.FH_CompletionStatement = "ESC AAA";

			var templateLink = (IProcessHeaderLink)workItemPartialTemplate.ProcessHeaderLinks.AddNew();
			templateLink.FP_FH_HeaderFrom = workItemTemplateWorkflowHeader.PK;
			templateLink.FP_FH_HeaderTo = projectTemplateHeader.PK;
			templateLink.FP_LinkType = "DEP";
			templateLink.FromWorkflowExternalTemplatePK = projectPartialTemplate.PK;

			var workItem = Factory.New<WorkItem>();
			var project = Factory.New<Project>();
			workItem.RelatedItems.Add(project);

			var workItemPartialTemplateTrigger = workItem.WorkflowItems.Triggers.AddNew();
			workItemPartialTemplateTrigger.P9_Description = "WI Trigger";
			workItemPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var projectPartialTemplateTrigger = project.WorkflowItems.Triggers.AddNew();
			projectPartialTemplateTrigger.P9_Description = "WI Trigger";
			projectPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var workItemAction = workItemPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			workItemAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			workItemAction.PQ_P0_WorkflowTemplate = workItemPartialTemplate.PK;
			var projectAction = projectPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			projectAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			projectAction.PQ_P0_WorkflowTemplate = projectPartialTemplate.PK;

			Factory.Save();

			AssertEquals("Precondition: No tasks should have been created yet", 0, workItem.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: No tasks should have been created yet", 0, project.WorkflowItems.Tasks.Count);
			workItem.Logs.AddNew(AutoEvents.CustomisableEvent00);
			project.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			var workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);
			var projectReloaded = newFactory.Load<Project>(project.PK);

			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, workItemReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, projectReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", workItemPartialTemplate.PK, workItemReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", projectPartialTemplate.PK, projectReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);

			var headerLinks = workItemReloaded.WorkflowItems.Tasks[0].ProcessHeader.Links.ToArray();
			AssertEquals("Should create links between workflows from different jobs since the templates have a link", 1, headerLinks.Length);
			AssertEquals("Should create links between workflows from different jobs since the templates have a link", workItemReloaded.WorkflowItems.Tasks[0].ProcessHeader.PK, headerLinks[0].FP_FH_HeaderFrom);
			AssertEquals("Should create links between workflows from different jobs since the templates have a link", projectReloaded.WorkflowItems.Tasks[0].ProcessHeader.PK, headerLinks[0].FP_FH_HeaderTo);
		}

		public void TestApplyTemplates_LinkOnTo()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.CreateSystem(Factory, "WKI");
			bmsTestHelper.CreateSystem(Factory, "WKP");
			bmsTestHelper.EnableBMSInRegistry();
			var workItemPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var projectPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItemCode;
			projectPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.ProjectCode;
			workItemPartialTemplate.P0_Description = "WKI PT";
			projectPartialTemplate.P0_Description = "WKP PT";
			workItemPartialTemplate.P0_IsPartialTemplate = projectPartialTemplate.P0_IsPartialTemplate = true;
			var workItemTemplateWorkflowHeader = workItemPartialTemplate.ProcessHeaders.AddNew();
			var projectTemplateHeader = projectPartialTemplate.ProcessHeaders.AddNew();
			var task1 = workItemPartialTemplate.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workItemTemplateWorkflowHeader.PK;
			var task2 = projectPartialTemplate.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = projectTemplateHeader.PK;
			workItemTemplateWorkflowHeader.FH_CompletionStatement = "ESC AAA";
			projectTemplateHeader.FH_CompletionStatement = "ESC AAA";

			var templateLink = (IProcessHeaderLink)projectPartialTemplate.ProcessHeaderLinks.AddNew();
			templateLink.FP_FH_HeaderFrom = projectTemplateHeader.PK;
			templateLink.FP_FH_HeaderTo = workItemTemplateWorkflowHeader.PK;
			templateLink.FP_LinkType = "DEP";
			templateLink.ToWorkflowExternalTemplatePK = workItemPartialTemplate.PK;

			var workItem = Factory.New<WorkItem>();
			var project = Factory.New<Project>();
			workItem.RelatedItems.Add(project);

			var workItemPartialTemplateTrigger = workItem.WorkflowItems.Triggers.AddNew();
			workItemPartialTemplateTrigger.P9_Description = "WI Trigger";
			workItemPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var projectPartialTemplateTrigger = project.WorkflowItems.Triggers.AddNew();
			projectPartialTemplateTrigger.P9_Description = "WI Trigger";
			projectPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var workItemAction = workItemPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			workItemAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			workItemAction.PQ_P0_WorkflowTemplate = workItemPartialTemplate.PK;
			var projectAction = projectPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			projectAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			projectAction.PQ_P0_WorkflowTemplate = projectPartialTemplate.PK;

			Factory.Save();

			AssertEquals("Precondition: No tasks should have been created yet", 0, workItem.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: No tasks should have been created yet", 0, project.WorkflowItems.Tasks.Count);
			workItem.Logs.AddNew(AutoEvents.CustomisableEvent00);
			project.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			var workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);
			var projectReloaded = newFactory.Load<Project>(project.PK);

			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, workItemReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, projectReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", workItemPartialTemplate.PK, workItemReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", projectPartialTemplate.PK, projectReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);

			var headerLinks = workItemReloaded.WorkflowItems.Tasks[0].ProcessHeader.Links.ToArray();
			AssertEquals("Should create links between workflows from different jobs since the templates have a link", 1, headerLinks.Length);
			AssertEquals("Should create links between workflows from different jobs since the templates have a link", projectReloaded.WorkflowItems.Tasks[0].ProcessHeader.PK, headerLinks[0].FP_FH_HeaderFrom);
			AssertEquals("Should create links between workflows from different jobs since the templates have a link", workItemReloaded.WorkflowItems.Tasks[0].ProcessHeader.PK, headerLinks[0].FP_FH_HeaderTo);
		}

		public void TestApplyTemplates_ShouldOnlyApplyOnce()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.CreateSystem(Factory, "WKI");
			bmsTestHelper.CreateSystem(Factory, "WKP");
			bmsTestHelper.EnableBMSInRegistry();
			var workItemPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var projectPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var workItemPartialTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItemCode;
			projectPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.ProjectCode;
			workItemPartialTemplate2.P0_ProcessType = JobInvoicingConsumerTypes.WorkItemCode;
			workItemPartialTemplate.P0_Description = "WKI PT";
			projectPartialTemplate.P0_Description = "WKP PT";
			workItemPartialTemplate2.P0_Description = "WKI PT2";
			workItemPartialTemplate.P0_IsPartialTemplate = projectPartialTemplate.P0_IsPartialTemplate = workItemPartialTemplate2.P0_IsPartialTemplate = true;
			var workItemTemplateWorkflowHeader = workItemPartialTemplate.ProcessHeaders.AddNew();
			var projectTemplateHeader = projectPartialTemplate.ProcessHeaders.AddNew();
			var workItemTemplateWorkflowHeader2 = workItemPartialTemplate2.ProcessHeaders.AddNew();
			var task1 = workItemPartialTemplate.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workItemTemplateWorkflowHeader.PK;
			var task2 = projectPartialTemplate.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = projectTemplateHeader.PK;
			var task3 = workItemPartialTemplate2.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = workItemTemplateWorkflowHeader2.PK;
			workItemTemplateWorkflowHeader.FH_CompletionStatement = "ESC AAA";
			projectTemplateHeader.FH_CompletionStatement = "ESC AAA";
			workItemTemplateWorkflowHeader2.FH_CompletionStatement = "ESC AAA";

			var templateLink = (IProcessHeaderLink)workItemPartialTemplate.ProcessHeaderLinks.AddNew();
			templateLink.FP_FH_HeaderFrom = workItemTemplateWorkflowHeader.PK;
			templateLink.FP_FH_HeaderTo = projectTemplateHeader.PK;
			templateLink.FP_LinkType = "DEP";
			templateLink.FromWorkflowExternalTemplatePK = projectPartialTemplate.PK;

			var workItem = Factory.New<WorkItem>();
			var project = Factory.New<Project>();
			workItem.RelatedItems.Add(project);

			var workItemPartialTemplateTrigger = workItem.WorkflowItems.Triggers.AddNew();
			workItemPartialTemplateTrigger.P9_Description = "WI Trigger";
			workItemPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var projectPartialTemplateTrigger = project.WorkflowItems.Triggers.AddNew();
			projectPartialTemplateTrigger.P9_Description = "PRJ Trigger";
			projectPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var workItemOtherPartialTemplateTrigger = workItem.WorkflowItems.Triggers.AddNew();
			workItemOtherPartialTemplateTrigger.P9_Description = "WI Trigger 2";
			workItemOtherPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z01";

			var workItemAction = workItemPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			workItemAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			workItemAction.PQ_P0_WorkflowTemplate = workItemPartialTemplate.PK;
			var projectAction = projectPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			projectAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			projectAction.PQ_P0_WorkflowTemplate = projectPartialTemplate.PK;
			var workItemAction2 = workItemOtherPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			workItemAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			workItemAction2.PQ_P0_WorkflowTemplate = workItemPartialTemplate2.PK;

			Factory.Save();

			AssertEquals("Precondition: No tasks should have been created yet", 0, workItem.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: No tasks should have been created yet", 0, project.WorkflowItems.Tasks.Count);
			workItem.Logs.AddNew(AutoEvents.CustomisableEvent00);
			project.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			var workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);
			var projectReloaded = newFactory.Load<Project>(project.PK);

			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, workItemReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, projectReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", workItemPartialTemplate.PK, workItemReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", projectPartialTemplate.PK, projectReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);

			var firstCreatedTask = workItemReloaded.WorkflowItems.Tasks[0];
			var headerLinks = workItemReloaded.WorkflowItems.Tasks[0].ProcessHeader.Links.ToArray();
			AssertEquals("Precondition: Should create links between workflows from different jobs since the templates have a link", 1, headerLinks.Length);

			workItem.Logs.AddNew(AutoEvents.CustomisableEvent00);
			project.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			newFactory = new BusinessObjectFactory();
			workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);

			headerLinks = workItemReloaded.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.PK == firstCreatedTask.PK).ProcessHeader.Links.ToArray();
			AssertEquals("Should not create links again on originally added WF since the workflows already have a link", 1, headerLinks.Length);

			headerLinks[0].Delete();
			newFactory.Save();

			workItem.Logs.AddNew(AutoEvents.CustomisableEvent01);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			newFactory = new BusinessObjectFactory();
			workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);

			AssertEquals("Should not create links again on originally added WF since it was deliberately deleted", 0, workItemReloaded.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.PK == firstCreatedTask.PK).ProcessHeader.Links.Count());
		}

		public void TestApplyTemplates_ShouldOnlyApplyOnce_JobLevelProcessHeaderLink()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.CreateSystem(Factory, "WKI");
			bmsTestHelper.CreateSystem(Factory, "WKP");
			bmsTestHelper.EnableBMSInRegistry();
			var workItemPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var projectPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var workItemPartialTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workItemPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItemCode;
			projectPartialTemplate.P0_ProcessType = JobInvoicingConsumerTypes.ProjectCode;
			workItemPartialTemplate2.P0_ProcessType = JobInvoicingConsumerTypes.WorkItemCode;
			workItemPartialTemplate.P0_Description = "WKI PT";
			projectPartialTemplate.P0_Description = "WKP PT";
			workItemPartialTemplate2.P0_Description = "WKI PT2";
			workItemPartialTemplate.P0_IsPartialTemplate = projectPartialTemplate.P0_IsPartialTemplate = workItemPartialTemplate2.P0_IsPartialTemplate = true;
			var workItemJobLevelHeader = workItemPartialTemplate.ProcessHeaders[0];
			var projectJobLevelHeader = projectPartialTemplate.ProcessHeaders[0];
			var workItemTemplateWorkflowHeader = workItemPartialTemplate.ProcessHeaders.AddNew();
			var projectTemplateHeader = projectPartialTemplate.ProcessHeaders.AddNew();
			var workItemTemplateWorkflowHeader2 = workItemPartialTemplate2.ProcessHeaders.AddNew();
			var task1 = workItemPartialTemplate.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workItemTemplateWorkflowHeader.PK;
			var task2 = projectPartialTemplate.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = projectTemplateHeader.PK;
			var task3 = workItemPartialTemplate2.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = workItemTemplateWorkflowHeader2.PK;
			workItemTemplateWorkflowHeader.FH_CompletionStatement = "ESC AAA";
			projectTemplateHeader.FH_CompletionStatement = "ESC AAA";
			workItemTemplateWorkflowHeader2.FH_CompletionStatement = "ESC AAA";

			var templateLink = (IProcessHeaderLink)workItemPartialTemplate.ProcessHeaderLinks.AddNew();
			templateLink.FP_FH_HeaderFrom = workItemJobLevelHeader.PK;
			templateLink.FP_FH_HeaderTo = projectJobLevelHeader.PK;
			templateLink.FP_LinkType = "DEP";
			templateLink.FromWorkflowExternalTemplatePK = projectPartialTemplate.PK;

			var workItem = Factory.New<WorkItem>();
			var project = Factory.New<Project>();
			workItem.RelatedItems.Add(project);

			var workItemPartialTemplateTrigger = workItem.WorkflowItems.Triggers.AddNew();
			workItemPartialTemplateTrigger.P9_Description = "WI Trigger";
			workItemPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var projectPartialTemplateTrigger = project.WorkflowItems.Triggers.AddNew();
			projectPartialTemplateTrigger.P9_Description = "PRJ Trigger";
			projectPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var workItemOtherPartialTemplateTrigger = workItem.WorkflowItems.Triggers.AddNew();
			workItemOtherPartialTemplateTrigger.P9_Description = "WI Trigger 2";
			workItemOtherPartialTemplateTrigger.TriggerConditions.TriggerEventCode = "Z01";

			var workItemAction = workItemPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			workItemAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			workItemAction.PQ_P0_WorkflowTemplate = workItemPartialTemplate.PK;
			var projectAction = projectPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			projectAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			projectAction.PQ_P0_WorkflowTemplate = projectPartialTemplate.PK;
			var workItemAction2 = workItemOtherPartialTemplateTrigger.ProcessTaskNotifications.AddNew();
			workItemAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			workItemAction2.PQ_P0_WorkflowTemplate = workItemPartialTemplate2.PK;

			Factory.Save();

			AssertEquals("Precondition: No tasks should have been created yet", 0, workItem.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: No tasks should have been created yet", 0, project.WorkflowItems.Tasks.Count);
			workItem.Logs.AddNew(AutoEvents.CustomisableEvent00);
			project.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			var workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);
			var projectReloaded = newFactory.Load<Project>(project.PK);

			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, workItemReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", 1, projectReloaded.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", workItemPartialTemplate.PK, workItemReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);
			AssertEquals("Precondition: Tasks should have been created from the partial templates", projectPartialTemplate.PK, projectReloaded.WorkflowItems.Tasks[0].SourceTemplatePK);

			var firstCreatedTask = workItemReloaded.WorkflowItems.Tasks[0];
			var headerLinks = workItemReloaded.WorkflowItems.Tasks[0].JobHeader.Links.ToArray();
			AssertEquals("Precondition: Should create links between workflows from different jobs since the templates have a link", 1, headerLinks.Length);

			workItem.Logs.AddNew(AutoEvents.CustomisableEvent00);
			project.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			newFactory = new BusinessObjectFactory();
			workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);

			headerLinks = workItemReloaded.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.PK == firstCreatedTask.PK).JobHeader.Links.ToArray();
			AssertEquals("Should not create links again on originally added WF since the workflows already have a link", 1, headerLinks.Length);

			headerLinks[0].Delete();
			newFactory.Save();

			workItem.Logs.AddNew(AutoEvents.CustomisableEvent01);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			newFactory = new BusinessObjectFactory();
			workItemReloaded = newFactory.Load<WorkItem>(workItem.PK);

			AssertEquals("Should not create links again on originally added WF since it was deliberately deleted", 0, workItemReloaded.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.PK == firstCreatedTask.PK).JobHeader.Links.Count());
		}
	}
}
