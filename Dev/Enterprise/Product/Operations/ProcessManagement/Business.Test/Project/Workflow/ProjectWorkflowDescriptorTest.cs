using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectWorkflowDescriptor))]
	class ProjectWorkflowDescriptorTest : WorkflowDescriptorTestCase<ProjectWorkflowDescriptor>
	{
		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.Project, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.Project.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.Project.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("4 sub types", 4, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1", "Project Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "Project Sub Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "Project Module", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Sub Type 4", "Priority", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[3].List);

			ProcessManagementRegistry.Instance.ProjectTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ONE");
			ProcessManagementRegistry.Instance.ProjectSubtypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TWO");
			ProcessManagementRegistry.Instance.ProjectModuleLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "THREE");
			ProcessManagementRegistry.Instance.ProjectPriorityLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOUR");
			AssertEquals("Sub Type 1", "ONE", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "TWO", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "THREE", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Sub Type 4", "FOUR", WorkflowDescriptor.SubTypeInformation[3].Description);

			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var info = WorkflowDescriptor.SubTypeInformation;
			AssertContainsExactElementsInAnyOrder("list 0",
				new CodeDescriptionComparer(),
				tree.GetParents(true).ToArray(),
				info[0].List.Cast<ICodeDescription>().ToArray());
			AssertContainsExactElementsInAnyOrder("list 1",
				new CodeDescriptionComparer(),
				tree.GetChildren(true, "").ToArray(),
				info[1].List.Cast<ICodeDescription>().ToArray());
			AssertContainsExactElementsInAnyOrder("list 2",
				new CodeDescriptionComparer(),
				tree.GetChildren(true, "", "").ToArray(),
				info[2].List.Cast<ICodeDescription>().ToArray());
			AssertContainsExactElementsInAnyOrder("list 3",
				new CodeDescriptionComparer(),
				tree.GetChildren(true, "", "", "").ToArray(),
				info[3].List.Cast<ICodeDescription>().ToArray());

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Project.Code;
			template.P0_SubType1 = "1AA";
			template.P0_SubType2 = "2AA";
			template.P0_SubType3 = "3AA";
			WorkflowDescriptor.LastProcessTaskTemplate = template;

			info = WorkflowDescriptor.SubTypeInformation;
			var expectedList = tree.GetChildren(true, template.P0_SubType1).ToArray();
			Assert("list.Length", expectedList.Length > 0);
			AssertContainsExactElementsInAnyOrder("list 1", new CodeDescriptionComparer(), expectedList, info[1].List.Cast<ICodeDescription>().ToArray());

			expectedList = tree.GetChildren(true, template.P0_SubType1, template.P0_SubType2).ToArray();
			Assert("list.Length", expectedList.Length > 0);
			AssertContainsExactElementsInAnyOrder("list 2", new CodeDescriptionComparer(), expectedList, info[2].List.Cast<ICodeDescription>().ToArray());

			expectedList = tree.GetChildren(true, template.P0_SubType1, template.P0_SubType2, template.P0_SubType3).ToArray();
			Assert("list.Length", expectedList.Length > 0);
			AssertContainsExactElementsInAnyOrder("list 3", new
				CodeDescriptionComparer(), expectedList, info[3].List.Cast<ICodeDescription>().ToArray());

			template.P0_SubType3 = "3S1";
			info = WorkflowDescriptor.SubTypeInformation;
			expectedList = tree.GetChildren(true, template.P0_SubType1, template.P0_SubType2, template.P0_SubType3).ToArray();
			Assert("list.Length", expectedList.Length > 0);
			AssertContainsExactElementsInAnyOrder("list 3", new CodeDescriptionComparer(), expectedList, info[3].List.Cast<ICodeDescription>().ToArray());
		}

		CodeDescriptionBoolTreeNodeCollection GetSetupProjectTypeTree()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
			return tree;
		}

		public void TestSubtypeList()
		{
			var tree = GetSetupProjectTypeTree();

			AssertContainsExactElementsInAnyOrder("Subtype list",
				new CodeDescriptionComparer(),
				tree.GetChildren(true, "").ToArray(),
				WorkflowDescriptor.SubTypeInformation[1].List.Cast<ICodeDescription>().ToArray());
		}

		public void TestModuleList()
		{
			var tree = GetSetupProjectTypeTree();

			AssertContainsExactElementsInAnyOrder("Module list",
				new CodeDescriptionComparer(),
				tree.GetChildren(true, "", "").ToArray(),
				WorkflowDescriptor.SubTypeInformation[2].List.Cast<ICodeDescription>().ToArray());
		}

		public override void TestRequiresPorts()
		{
			AssertEquals("RequiresPort1", false, WorkflowDescriptor.RequiresPort1);
			AssertEquals("RequiresPort2", false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals("RequiresClient", false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals("RequiresBranch", false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals("RequiresDepartment", false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var contact = ClientOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jan Michael Vincent";
			var project = Factory.New<Project>();
			project.WKP_OA_ClientAddress = ClientOrg.MainAddress.PK;
			project.WKP_OC_Contact = contact.PK;

			return new IWorkflowProvider[] { project };
		}

		public void TestForCustomisationSettings()
		{
			AssertEquals(typeof(ProjectFormCustomisationSettingsProvider), WorkflowDescriptor.FormCustomisationSettings.GetType());
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		[TestDate(2019, 1, 1)]
		public void TestNotificationEmail_ClientRecipient()
		{
			var project = ProcessMgmtTestHelper.CreateProject(Factory);
			var mode = project.ClientAddress.Header.EDICommunicationsModes.AddNew();

			mode.EK_Module = WorkflowDescriptors.ProjectWorkflowDescriptorCode;
			mode.EK_FileFormat = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "dinglebop@plumbus.com";

			var trigger = MasterFilesTestHelper.CreateTrigger(project, AutoEvents.CustomisableEvent69Code);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.Client);

			Factory.Save();

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);
			project.GetLogs().AddNew(AutoEvents.CustomisableEvent69);
			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);

			Factory.Save();

			AssertEquals(0, Env.AllEmailsCreated.Count());
			MasterFilesTestHelper.RunLogWalker();
			AssertEquals(1, Env.AllEmailsCreated.Count());

			AssertEquals("dinglebop@plumbus.com", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);
		}
	}

	class ProjectWorkflowDescriptorNonTransactionedTest : WorkflowDescriptorNonTransactionedTestCase<ProjectWorkflowDescriptor>
	{
		protected override IWorkflowProvider GetJobForTest()
		{
			return ProcessMgmtTestHelper.CreateProject(Factory);
		}
	}
}
