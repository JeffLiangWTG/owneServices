using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemWorkflowDescriptor))]
	class WorkItemWorkflowDescriptorTest : WorkflowDescriptorTestCase<WorkItemWorkflowDescriptor>
	{
		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.WorkItem, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.WorkItem.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.WorkItem.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("5 sub types", 5, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1", "Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "Area", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "Activity Type", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Sub Type 4", "Activity Sub Type", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("Sub Type 5", "Priority", WorkflowDescriptor.SubTypeInformation[4].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[3].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[4].List);

			ProcessManagementRegistry.Instance.WorkItemTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ONE");
			ProcessManagementRegistry.Instance.WorkItemAreaLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TWO");
			ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "THREE");
			ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOUR");
			ProcessManagementRegistry.Instance.WorkItemPriorityLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FIVE");
			AssertEquals("Sub Type 1", "ONE", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "TWO", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "THREE", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Sub Type 4", "FOUR", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("Sub Type 5", "FIVE", WorkflowDescriptor.SubTypeInformation[4].Description);

			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

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
			AssertContainsExactElementsInAnyOrder("list 4",
				new CodeDescriptionComparer(),
				tree.GetChildren(true, "", "", "", "").ToArray(),
				info[4].List.Cast<ICodeDescription>().ToArray());

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;
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
			AssertContainsExactElementsInAnyOrder("list 3", new CodeDescriptionComparer(), expectedList, info[3].List.Cast<ICodeDescription>().ToArray());

			template.P0_SubType3 = "3S1";
			info = WorkflowDescriptor.SubTypeInformation;
			expectedList = tree.GetChildren(true, template.P0_SubType1, template.P0_SubType2, template.P0_SubType3).ToArray();
			Assert("list.Length", expectedList.Length > 0);
			AssertContainsExactElementsInAnyOrder("list 3", new CodeDescriptionComparer(), expectedList, info[3].List.Cast<ICodeDescription>().ToArray());
		}

		public override void TestRequiresPorts()
		{
			AssertEquals("RequiresPort1", true, WorkflowDescriptor.RequiresPort1);
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
			AssertEquals("RequiresDepartment", true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			WorkItem workItem = Factory.New<WorkItem>();
			return new IWorkflowProvider[] { workItem };
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => new SchemaColumn[]
		{
			WorkItemSchema.WKI_ActivitySubtype,
			WorkItemSchema.WKI_ActivityType,
			WorkItemSchema.WKI_GB_AssignedBranch,
			WorkItemSchema.WKI_GC_AssignedCompany,
			WorkItemSchema.WKI_GE_AssignedDepartment,
			WorkItemSchema.WKI_PortOrCountry,
			WorkItemSchema.WKI_Priority,
			WorkItemSchema.WKI_Status,
			WorkItemSchema.WKI_Summary,
			WorkItemSchema.WKI_WorkItemArea,
			WorkItemSchema.WKI_WorkItemType,
		};
	}

	class WorkItemWorkflowDescriptorNonTransactionedTest : WorkflowDescriptorNonTransactionedTestCase<WorkItemWorkflowDescriptor>
	{
		protected override IWorkflowProvider GetJobForTest()
		{
			return ProcessMgmtTestHelper.CreateWorkItem(Factory);
		}
	}
}
