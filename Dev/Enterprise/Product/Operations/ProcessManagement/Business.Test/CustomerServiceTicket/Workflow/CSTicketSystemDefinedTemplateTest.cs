using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestDate(2015, 7, 14)]
	class CSTicketSystemDefinedTemplateTest : TestCaseWithFactory
	{
		public void TestSystemDefinedTemplate_ShouldIncludeSampleFlow()
		{
			var expectedTriggerDetails = new[]
			{
				Tuple.Create(10, "ADD", "Ticket Raised"),
				Tuple.Create(20, "ATC", "Work to satisfy ticket in-progress"),
				Tuple.Create(30, "JCL", "Work to satisfy ticket completed"),
				Tuple.Create(31, "ATC", "Work to satisfy ticket completed"),
				Tuple.Create(40, "JCL", "Ticket Completed"),
				Tuple.Create(50, "JCL", "Work to satisfy ticket canceled"),
				Tuple.Create(51, "ATC", "Work to satisfy ticket canceled"),
			};

			var actualTriggerDetails = triggers.Select(trigger => Tuple.Create((int)trigger.Sequence, trigger.TriggerEventCode.ToString(), trigger.Description.ToString()));

			AssertSequencesEqual("If you add more triggers, please update these tests to also consider their business logic.", expectedTriggerDetails, actualTriggerDetails);
		}

		public void TestSystemDefinedTemplate_StandardProcessFlow()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			AssertSpecificTriggersHaveFired(triggers, ticket, 10);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ADD event, by trigger #10", new[] { "Farley@hunrath.com" },
@"The ticket CST00000001 has been raised.<br />
<br />
Client Name: See Dubyah<br />
Organization: THEBUNKER - Client<br />
<br />
<hr />
<strong>Incident Details</strong><br />
<br />
Hunrath: CW1<br />
Kaptar: CW2<br />
Maray: CW3<br />
Soria: CW4<br />
Diaspora: CW5<br />
Ticket Summary: Connect me damn battery<br />
Ticket Full Description: Who the devil are you?<br />");

			Env.ClearAllEmailsCreated();

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 20);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ATC event, by trigger #20", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that work for ticket CST00000001 is in progress now. We will be in further contact when work is completed.");

			Env.ClearAllEmailsCreated();

			workItem.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 20, 30);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for JCL event, by trigger #30", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that all work relating to ticket CST00000001 has now been completed.");
		}

		public void TestSystemDefinedTemplate_AttachedWorkItemIsCancelled()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			AssertSpecificTriggersHaveFired(triggers, ticket, 10);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ADD event, by trigger #10", new[] { "Farley@hunrath.com" },
@"The ticket CST00000001 has been raised.<br />
<br />
Client Name: See Dubyah<br />
Organization: THEBUNKER - Client<br />
<br />
<hr />
<strong>Incident Details</strong><br />
<br />
Hunrath: CW1<br />
Kaptar: CW2<br />
Maray: CW3<br />
Soria: CW4<br />
Diaspora: CW5<br />
Ticket Summary: Connect me damn battery<br />
Ticket Full Description: Who the devil are you?<br />");

			Env.ClearAllEmailsCreated();

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 20);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ATC event, by trigger #20", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that work for ticket CST00000001 is in progress now. We will be in further contact when work is completed.");

			Env.ClearAllEmailsCreated();

			workItem.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled);
			Factory.Save();

			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 20, 50);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for JCL event, by trigger #50", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that ticket CST00000001 has been canceled.");
		}

		public void TestSystemDefinedTemplate_WhenTicketTasksClosed()
		{
			AssertSpecificTriggersHaveFired(triggers, ticket, 10);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ADD event, by trigger #10", new[] { "Farley@hunrath.com" },
@"The ticket CST00000001 has been raised.<br />
<br />
Client Name: See Dubyah<br />
Organization: THEBUNKER - Client<br />
<br />
<hr />
<strong>Incident Details</strong><br />
<br />
Hunrath: CW1<br />
Kaptar: CW2<br />
Maray: CW3<br />
Soria: CW4<br />
Diaspora: CW5<br />
Ticket Summary: Connect me damn battery<br />
Ticket Full Description: Who the devil are you?<br />");

			Env.ClearAllEmailsCreated();

			TestDateAttribute.AddMinutes(1);

			ticket.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();
			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 40);

			AssertEmailSent("Email should have been triggered for JCL event, by trigger #40", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that ticket CST00000001 has now been completed.");
		}

		public void TestSystemDefinedTemplate_WhenTicketAttachedToAlreadyCompletedWorkItem()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			workItem.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertSpecificTriggersHaveFired(triggers, ticket, 10);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ADD event, by trigger #10", new[] { "Farley@hunrath.com" },
@"The ticket CST00000001 has been raised.<br />
<br />
Client Name: See Dubyah<br />
Organization: THEBUNKER - Client<br />
<br />
<hr />
<strong>Incident Details</strong><br />
<br />
Hunrath: CW1<br />
Kaptar: CW2<br />
Maray: CW3<br />
Soria: CW4<br />
Diaspora: CW5<br />
Ticket Summary: Connect me damn battery<br />
Ticket Full Description: Who the devil are you?<br />");

			Env.ClearAllEmailsCreated();

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();
			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 31);

			AssertEmailSent("Email should have been triggered for ATC event, by trigger #31", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that all work relating to ticket CST00000001 has now been completed.");
		}

		public void TestSystemDefinedTemplate_WhenTicketAttachedToAlreadyCancelledWorkItem()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			Factory.Save();

			workItem.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled);
			Factory.Save();

			AssertSpecificTriggersHaveFired(triggers, ticket, 10);
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailSent("Email should have been triggered for ADD event, by trigger #10", new[] { "Farley@hunrath.com" },
@"The ticket CST00000001 has been raised.<br />
<br />
Client Name: See Dubyah<br />
Organization: THEBUNKER - Client<br />
<br />
<hr />
<strong>Incident Details</strong><br />
<br />
Hunrath: CW1<br />
Kaptar: CW2<br />
Maray: CW3<br />
Soria: CW4<br />
Diaspora: CW5<br />
Ticket Summary: Connect me damn battery<br />
Ticket Full Description: Who the devil are you?<br />");

			Env.ClearAllEmailsCreated();

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();
			AssertSpecificTriggersHaveFired(triggers, ticket, 10, 51);

			AssertEmailSent("Email should have been triggered for ATC event, by trigger #51", new[] { "JosefJanssen@hunrath.com" },
@"This is an automatic email notification to advise you that ticket CST00000001 has been canceled.");
		}

		static void AssertSpecificTriggersHaveFired(ITemplateTrigger[] triggers, IWorkflowProvider job, params int[] triggerSequencesWhichShouldHaveFired)
		{
			for (var i = 0; i < triggers.Length; i++)
			{
				var trigger = triggers[i];
				var shouldHaveFired = triggerSequencesWhichShouldHaveFired.Contains(trigger.Sequence);

				MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, shouldHaveFired);
			}
		}

		static void AssertEmailSent(string message, string[] recipients, string textContainedInBody)
		{
			foreach (var recipient in recipients)
			{
				var email = Env.AllEmailsCreated.SingleOrDefault(e => e.Recipients.Cast<RecipientDef>().Any(r => r.Email == recipient));

				CombineAssertions(message + ", recipient " + recipient, () =>
				{
					AssertNotNull("An email should have been sent", email);
					AssertContains("Body", textContainedInBody.Replace(" ", ""), email.Body.Replace(" ", ""));
				});
			}
		}

		ITemplateTrigger[] triggers;
		WorkRequest ticket;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "Farley@hunrath.com";
			group.Staff.Add(staff);

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			ProcessManagementRegistry.Instance.SelectionCriterion1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Hunrath");
			ProcessManagementRegistry.Instance.SelectionCriterion2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Kaptar");
			ProcessManagementRegistry.Instance.SelectionCriterion3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Maray");
			ProcessManagementRegistry.Instance.SelectionCriterion4Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Soria");
			ProcessManagementRegistry.Instance.SelectionCriterion5Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Diaspora");

			var ticketTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);
			var ticketTemplateWorkflow = BMSTestHelper.CreateWorkflow(ticketTaskTemplate);
			BMSTestHelper.CreateTask(ticketTaskTemplate, ticketTemplateWorkflow);

			var workItemTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var workItemTemplateWorkflow = BMSTestHelper.CreateWorkflow(workItemTaskTemplate);
			BMSTestHelper.CreateTask(workItemTaskTemplate, workItemTemplateWorkflow);

			Factory.Save();

			ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Connect me damn battery", "CW1", "CW2", "CW3", "CW4", "CW5");

			ticket.WKR_Description = "Who the devil are you?";
			ticket.Client.OC_ContactName = "See Dubyah";
			ticket.Client.ParentOrg.OH_Code = "THEBUNKER";

			Factory.Save();

			var universalTemplate = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode).AddToFilter(ProcessTaskTemplateSchema.P0_IsSystem, true)).Single();
			triggers = universalTemplate.TemplateTriggers.Cast<ITemplateTrigger>().OrderBy(t => t.Sequence).ToArray();
		}

		IBMTestHelper BMSTestHelper { get; } = ObjectFactory.Get<IBMTestHelper>();
	}
}
