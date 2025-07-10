using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProcessManagementRegistry))]
	class ProcessManagementRegistryTest : RegistryItemSetTestCaseWithFactory<ProcessManagementRegistry>
	{
		#region Work Item Types

		public void TestWorkItemTypeTree()
		{
			CodeDescriptionBoolRegistryEditorInfo expectedParentListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null, false);
			CodeDescriptionBoolRegistryEditorInfo expectedChildListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null, false);
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo expectedEditorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
				(NoResString)"Product", (NoResString)"Work Item Area",
				expectedParentListEditorInfo, expectedChildListEditorInfo, false);

			var regItem = ItemSet.WorkItemTypeTree;
			TestGenericRegistryItem(regItem, "WorkItemTypeTree", ProcessManagementRegistry.Categories.ProductivityTools_WorkItem, "Selection Criteria Values",
				"Defines a list of values that can be selected in the relevant Selection Criteria fields. The values can be filtered to be available hierarchically. For example, the values shown in Selection Criterion 2 can be different depending on the value chosen in Selection Criterion 1. Alternatively, the values can be the same regardless of the other Selection Criteria values by leaving the 'All Selection Criterion * Values' row selected.",
				RegistryStorageFlags.System);

			var defaultValue = regItem.Value;
			AssertEquals(5, defaultValue.MaxDepth);
			AssertNotNull("AllDescriptions", defaultValue.AllDescriptions);
			var parentToAllMap = new Dictionary<ZGuid, CodeDescriptionBoolTreeNode>();
			foreach (CodeDescriptionBoolTreeNode item in defaultValue)
			{
				if (item.Code == CodeDescriptionBoolTreeNode.AllCode)
				{
					parentToAllMap.Add(item.ParentID, item);
				}
			}

			foreach (CodeDescriptionBoolTreeNode item in defaultValue)
			{
				if (item.Code != CodeDescriptionBoolTreeNode.AllCode)
				{
					int depth = defaultValue.GetDepth(item);
					if (depth < defaultValue.MaxDepth - 1)
					{
						AssertEquals("Has ALL child " + item.Code + " (" + item.Description + ") depth " + depth, true, parentToAllMap.ContainsKey(item.PK));
					}
				}
			}
		}

		public void TestWorkItemGenericCaptionAttributes()
		{
			TestRegistryItem(
				ItemSet.WorkItemTypeLabel,
				"WorkItemTypeLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Selection Criterion 1 Caption",
				"Allows you to further categorize your work items, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Type", "12345678901234567890");

			TestRegistryItem(
				ItemSet.WorkItemAreaLabel,
				"WorkItemAreaLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Selection Criterion 2 Caption",
				"Allows you to further categorize your work items, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Area", "12345678901234567890");

			TestRegistryItem(
				ItemSet.WorkItemActivityTypeLabel,
				"WorkItemActivityTypeLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Selection Criterion 3 Caption",
				"Allows you to further categorize your work items, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Activity Type", "12345678901234567890");

			TestRegistryItem(
				ItemSet.WorkItemActivitySubTypeLabel,
				"WorkItemActivitySubTypeLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Selection Criterion 4 Caption",
				"Allows you to further categorize your work items, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Activity Sub Type", "12345678901234567890");

			TestRegistryItem(
				ItemSet.WorkItemPriorityLabel,
				"WorkItemPriorityLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Selection Criterion 5 Caption",
				"Allows you to further categorize your work items, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Priority", "12345678901234567890");

			TestRegistryItem(
				ItemSet.DefectIntroducedInWorkItemLabel,
				"DefectIntroducedInWorkItemLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Defect Introduced in WI Label",
				"Allows you to customize the label for Defect Introduced in WI.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Defect Introduced in WI", "123456789012345678901234567890");

			TestRegistryItem(
				ItemSet.DefectIntroducedInTaskLabel,
				"DefectIntroducedInTaskLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"Defect Introduced in Task Label",
				"Allows you to customize the label for Defect Introduced in Task.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Defect Introduced in Task", "123456789012345678901234567890");

			TestRegistryItem(
				ItemSet.FirstCBThatMissedDefectLabel,
				"FirstCBThatMissedDefectLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
				"First CB that Missed Defect Label",
				"Allows you to customize the label for First CB that Missed Defect.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"First CB that Missed Defect", "123456789012345678901234567890");
		}

		public void TestWorkItemMandatoryWorkItemTypes()
		{
			TestRegistryItem(
				ItemSet.WorkItemTypeMandatory,
				"WorkItemTypeMandatory",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
				"Selection Criterion 1 Mandatory",
				"Determines whether the Selection Criterion 1 field is mandatory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);

			TestRegistryItem(
				ItemSet.WorkItemAreaMandatory,
				"WorkItemAreaMandatory",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
				"Selection Criterion 2 Mandatory",
				"Determines whether the Selection Criterion 2 field is mandatory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);

			TestRegistryItem(
				ItemSet.WorkItemActivityTypeMandatory,
				"WorkItemActivityTypeMandatory",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
				"Selection Criterion 3 Mandatory",
				"Determines whether the Selection Criterion 3 field is mandatory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);

			TestRegistryItem(
				ItemSet.WorkItemActivitySubTypeMandatory,
				"WorkItemActivitySubTypeMandatory",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
				"Selection Criterion 4 Mandatory",
				"Determines whether the Selection Criterion 4 field is mandatory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);

			TestRegistryItem(
				ItemSet.WorkItemPriorityMandatory,
				"WorkItemPriorityMandatory",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
				"Selection Criterion 5 Mandatory",
				"Determines whether the Selection Criterion 5 field is mandatory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Defect Cause Identification

		public void TestIdentifyDefectCauseTaskType()
		{
			TestRegistryItem(
				ItemSet.IdentifyDefectCauseTaskType,
				"IdentifyDefectCauseTaskType",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem,
				"Identify Defect Cause Task Type",
				"This setting is to specify the task type that corresponds to 'Identify Defect Cause' concept.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				WorkflowDataRegistryHelper.GetTaskTypeList(WorkflowDescriptors.WorkItemWorkflowDescriptorCode),
				"IDC");
		}

		public void TestDefectWorkItemTypes()
		{
			TestGenericRegistryItem(
				ItemSet.DefectWorkItemTypes,
				"DefectWorkItemTypes",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem,
				"Defect Activity Sub Types",
				"List of activity sub types which identify a Work Item as a Defect. The Defect Introduced fields will only be enabled for these values.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		public void TestDefectIntroducedTaskTypes()
		{
			TestGenericRegistryItem(
				ItemSet.DefectIntroducedTaskTypes,
				"DefectIntroducedTaskTypes",
				ProcessManagementRegistry.Categories.ProductivityTools_WorkItem,
				"Defect Introduced Task Types",
				"List of task types which can cause a defect. The Defect Introduced In Task field will only allow these types. Leave empty to include all task types.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		#endregion

		#region Cop-outs

		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			//Task Types items based on Workflow Types populated from the global workflow descriptors list
			//where Description has a resource string inside it's implementation, but returns 'string' type. This is a reason why test failed.
			//Terefore we need to override this test and do not run.
			//eventually Bret will do a refactoring and return type for WorkflowDescriptor -> Description will be MultilingualResourceString
			Assert(true);
		}

		#endregion

		#region Project Types

		public void TestProjectTypeTree()
		{
			CodeDescriptionBoolRegistryEditorInfo expectedParentListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null, false);
			CodeDescriptionBoolRegistryEditorInfo expectedChildListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null, false);
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo expectedEditorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
				(NoResString)"Product", (NoResString)"Project Module",
				expectedParentListEditorInfo, expectedChildListEditorInfo, false);

			var regItem = ItemSet.ProjectTypeTree;
			TestGenericRegistryItem(regItem, "ProjectTypeTree", ProcessManagementRegistry.Categories.ProductivityTools_Project, "Selection Criteria Values",
				"Defines a list of values that can be selected in the relevant Selection Criteria fields. The values can be filtered to be available hierarchically. For example, the values shown in Selection Criterion 2 can be different depending on the value chosen in Selection Criterion 1. Alternatively, the values can be the same regardless of the other Selection Criteria values by leaving the 'All Selection Criterion * Values' row selected.",
				RegistryStorageFlags.System);

			var defaultValue = regItem.Value;
			AssertEquals(4, defaultValue.MaxDepth);
			var parentToAllMap = new Dictionary<ZGuid, CodeDescriptionBoolTreeNode>();
			foreach (CodeDescriptionBoolTreeNode item in defaultValue)
			{
				if (item.Code == CodeDescriptionBoolTreeNode.AllCode)
				{
					parentToAllMap.Add(item.ParentID, item);
				}
			}

			foreach (CodeDescriptionBoolTreeNode item in defaultValue)
			{
				if (item.Code != CodeDescriptionBoolTreeNode.AllCode)
				{
					int depth = defaultValue.GetDepth(item);
					if (depth < defaultValue.MaxDepth - 1)
					{
						AssertEquals("Has ALL child " + item.Code + " (" + item.Description + ") depth " + depth, true, parentToAllMap.ContainsKey(item.PK));
					}
				}
			}
		}

		public void TestProjectFromEmailAddress()
		{
			TestRegistryItem(
				ItemSet.ProjectDefaultFromEmailAddress,
				"ProjectDefaultFromEmailAddress",
				ProcessManagementRegistry.Categories.ProductivityTools_Project,
				"Project Default From Email Address",
				"This is the 'From' address that will appear on emails sent via the Project module.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				"");
		}

		public void TestProjectGenericCaptionAttributes()
		{
			TestRegistryItem(
				ItemSet.ProjectTypeLabel,
				"ProjectTypeLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_Project_GenericCaptionAttributes,
				"Selection Criterion 1 Caption",
				"Allows you to further categorize your projects, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Project Type", "12345678901234567890");

			TestRegistryItem(
				ItemSet.ProjectSubtypeLabel,
				"ProjectSubtypeLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_Project_GenericCaptionAttributes,
				"Selection Criterion 2 Caption",
				"Allows you to further categorize your projects, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Project Sub Type", "12345678901234567890");

			TestRegistryItem(
				ItemSet.ProjectModuleLabel,
				"ProjectModuleLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_Project_GenericCaptionAttributes,
				"Selection Criterion 3 Caption",
				"Allows you to further categorize your projects, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Project Module", "12345678901234567890");

			TestRegistryItem(
				ItemSet.ProjectPriorityLabel,
				"ProjectPriorityLabel",
				ProcessManagementRegistry.Categories.ProductivityTools_Project_GenericCaptionAttributes,
				"Selection Criterion 4 Caption",
				"Allows you to further categorize your projects, by an identifier with customizable label.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Priority", "12345678901234567890");
		}

		public void TestProjectMandatoryProjectTypes()
		{
			TestRegistryItem(
				ItemSet.ProjectTypeMandatory,
				"ProjectTypeMandatory",
				ProcessManagementRegistry.Categories.ProductivityTools_Project_MandatoryProjectTypes,
				"Selection Criterion 1 Mandatory",
				"Determines whether the Selection Criterion 1 field is mandatory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);

			TestRegistryItem(
			   ItemSet.ProjectSubTypeMandatory,
			   "ProjectSubTypeMandatory",
			   ProcessManagementRegistry.Categories.ProductivityTools_Project_MandatoryProjectTypes,
			   "Selection Criterion 2 Mandatory",
			   "Determines whether the Selection Criterion 2 field is mandatory.",
			   RegistryStorageFlags.System,
			   RegistryOptions.Default,
			   false);

			TestRegistryItem(
			   ItemSet.ProjectModuleMandatory,
			   "ProjectModuleMandatory",
			   ProcessManagementRegistry.Categories.ProductivityTools_Project_MandatoryProjectTypes,
			   "Selection Criterion 3 Mandatory",
			   "Determines whether the Selection Criterion 3 field is mandatory.",
			   RegistryStorageFlags.System,
			   RegistryOptions.Default,
			   false);

			TestRegistryItem(
			   ItemSet.ProjectPriorityMandatory,
			   "ProjectPriorityMandatory",
			   ProcessManagementRegistry.Categories.ProductivityTools_Project_MandatoryProjectTypes,
			   "Selection Criterion 4 Mandatory",
			   "Determines whether the Selection Criterion 4 field is mandatory.",
			   RegistryStorageFlags.System,
			   RegistryOptions.Default,
			   false);
		}

		#endregion

		#region Customer Service Ticket

		public void TestSelectionCriteriaCaptions()
		{
			TestRegistryItem(
				ItemSet.SelectionCriterion1Caption,
				"SelectionCriterion1Caption",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
				"Selection Criterion 1 Caption",
				"Overrides the default caption for Selection Criterion 1 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Selection Criterion 1");

			TestRegistryItem(
				ItemSet.SelectionCriterion2Caption,
				"SelectionCriterion2Caption",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
				"Selection Criterion 2 Caption",
				"Overrides the default caption for Selection Criterion 2 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Selection Criterion 2");

			TestRegistryItem(
				ItemSet.SelectionCriterion3Caption,
				"SelectionCriterion3Caption",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
				"Selection Criterion 3 Caption",
				"Overrides the default caption for Selection Criterion 3 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Selection Criterion 3");

			TestRegistryItem(
				ItemSet.SelectionCriterion4Caption,
				"SelectionCriterion4Caption",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
				"Selection Criterion 4 Caption",
				"Overrides the default caption for Selection Criterion 4 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Selection Criterion 4");

			TestRegistryItem(
				ItemSet.SelectionCriterion5Caption,
				"SelectionCriterion5Caption",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
				"Selection Criterion 5 Caption",
				"Overrides the default caption for Selection Criterion 5 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"Selection Criterion 5");
		}

		public void TestSelectionCriteriaDescriptions()
		{
			TestRegistryItem(
				ItemSet.SelectionCriterion1Description,
				"SelectionCriterion1Description",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
				"Selection Criterion 1 Description",
				"Overrides the default long description for Selection Criterion 1 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"The first criterion for deciding which Workflow Template to use for this Customer Service Ticket.");

			TestRegistryItem(
				ItemSet.SelectionCriterion2Description,
				"SelectionCriterion2Description",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
				"Selection Criterion 2 Description",
				"Overrides the default long description for Selection Criterion 2 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"The second criterion for deciding which Workflow Template to use for this Customer Service Ticket.");

			TestRegistryItem(
				ItemSet.SelectionCriterion3Description,
				"SelectionCriterion3Description",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
				"Selection Criterion 3 Description",
				"Overrides the default long description for Selection Criterion 3 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"The third criterion for deciding which Workflow Template to use for this Customer Service Ticket.");

			TestRegistryItem(
				ItemSet.SelectionCriterion4Description,
				"SelectionCriterion4Description",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
				"Selection Criterion 4 Description",
				"Overrides the default long description for Selection Criterion 4 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"The fourth criterion for deciding which Workflow Template to use for this Customer Service Ticket.");

			TestRegistryItem(
				ItemSet.SelectionCriterion5Description,
				"SelectionCriterion5Description",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
				"Selection Criterion 5 Description",
				"Overrides the default long description for Selection Criterion 5 in Customer Service Tickets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"The fifth criterion for deciding which Workflow Template to use for this Customer Service Ticket.");
		}

		public void TestSelectionCriteriaValues()
		{
			TestRegistryItem(
				ItemSet.SelectionCriterion1Values,
				"SelectionCriterion1Values",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				"Selection Criterion 1 Values",
				"The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form.",
				RegistryStorageFlags.System,
				expectedDefaultValueCount: 0);

			TestRegistryItem(
				ItemSet.SelectionCriterion2Values,
				"SelectionCriterion2Values",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				"Selection Criterion 2 Values",
				"The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form.",
				RegistryStorageFlags.System,
				expectedDefaultValueCount: 0);

			TestRegistryItem(
				ItemSet.SelectionCriterion3Values,
				"SelectionCriterion3Values",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				"Selection Criterion 3 Values",
				"The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form.",
				RegistryStorageFlags.System,
				expectedDefaultValueCount: 0);

			TestRegistryItem(
				ItemSet.SelectionCriterion4Values,
				"SelectionCriterion4Values",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				"Selection Criterion 4 Values",
				"The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form.",
				RegistryStorageFlags.System,
				expectedDefaultValueCount: 0);

			TestRegistryItem(
				ItemSet.SelectionCriterion5Values,
				"SelectionCriterion5Values",
				ProcessManagementRegistry.Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				"Selection Criterion 5 Values",
				"The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form.",
				RegistryStorageFlags.System,
				expectedDefaultValueCount: 0);
		}

		public void TestCustomerServiceTicketNotificationGroup()
		{
			AssertEquals(RawDataRegistry.Categories.Notification, ItemSet.CustomerServiceTicketNotificationGroup.Category);
			AssertEquals("Customer Service Ticket Notification Group", ItemSet.CustomerServiceTicketNotificationGroup.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, ItemSet.CustomerServiceTicketNotificationGroup.Storage);
			AssertEquals("The staff group that will receive notifications when configured to do so through Workflow triggers using the NGP Recipient.", ItemSet.CustomerServiceTicketNotificationGroup.Hint);

			var defaultValue = Factory.Load<GlbGroup>(ItemSet.CustomerServiceTicketNotificationGroup.DefaultValue);

			AssertEquals(GlbGroup.PostMastersGroupCode, defaultValue?.GG_Code ?? ZString.Empty);
		}

		public void TestEmailRecipientDeterminationForNonSubscribedCSTickets_PersistenceWorks()
		{
			var defaultValue = ItemSet.RecipientDeterminationFallbackForNonSubscribedCSTickets.Value;

			AssertSourceTypes(defaultValue, RecipientSourceTypeList.Codes.LastCompletedTaskResource, RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup, RecipientSourceTypeList.Codes.NotificationGroup);

			var newValue = new RecipientSourceFallbackHeader();
			newValue.SourceCollection.AddNew().SourceType = RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup;
			newValue.SourceCollection.AddNew().SourceType = RecipientSourceTypeList.Codes.NotificationGroup;
			newValue.SourceCollection.AddNew().SourceType = RecipientSourceTypeList.Codes.LastCompletedTaskResource;

			ItemSet.RecipientDeterminationFallbackForNonSubscribedCSTickets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			AssertSourceTypes(ItemSet.RecipientDeterminationFallbackForNonSubscribedCSTickets.Value, RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup, RecipientSourceTypeList.Codes.NotificationGroup, RecipientSourceTypeList.Codes.LastCompletedTaskResource);
		}

		public void TestEmailRecipientDeterminationForNonSubscribedCSTickets_ValidationWorks()
		{
			var newValue = new RecipientSourceFallbackHeader();
			var source1 = newValue.SourceCollection.AddNew();
			var source2 = newValue.SourceCollection.AddNew();
			var source3 = newValue.SourceCollection.AddNew();

			source1.ValidateSourceType();
			source2.ValidateSourceType();
			source3.ValidateSourceType();

			BusinessObjectValidationTestCase.AssertMandatoryValidationError(source1.SourceTypeInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(source2.SourceTypeInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(source3.SourceTypeInfo, isExpectingError: true);

			source1.SourceType = RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup;
			source2.SourceType = RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup;
			source3.SourceType = RecipientSourceTypeList.Codes.NotificationGroup;

			source1.RunPreSaveValidation(); // Poke this one because it was set after the duplicate.

			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source1.SourceTypeInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source2.SourceTypeInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source3.SourceTypeInfo, isExpectingError: false);

			source2.Delete();

			source1.RunPreSaveValidation();

			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source1.SourceTypeInfo, isExpectingError: false);
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source3.SourceTypeInfo, isExpectingError: false);

			source1.FallbackSequence = 69;
			source3.FallbackSequence = 69;

			source1.RunPreSaveValidation();

			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source1.FallbackSequenceInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source3.FallbackSequenceInfo, isExpectingError: true);

			source3.FallbackSequence = 68;

			source1.RunPreSaveValidation();

			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source1.FallbackSequenceInfo, isExpectingError: false);
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(source3.FallbackSequenceInfo, isExpectingError: false);
		}

		static void AssertSourceTypes(RecipientSourceFallbackHeader header, params string[] expectedSourceTypes)
		{
			AssertEquals(expectedSourceTypes.Length, header.SourceCollection.Count);

			for (var i = 0; i < header.SourceCollection.Count; i++)
			{
				AssertEquals(i + 1, header.SourceCollection[i].FallbackSequence);
				AssertEquals(expectedSourceTypes[i], header.SourceCollection[i].SourceType);
			}
		}

		#endregion

		#region Jira Integration

		public void TestJiraIntegrationRegistryItems()
		{
			TestGenericRegistryItem(ItemSet.JiraSiteUrls, "JiraSiteUrls", ProcessManagementRegistry.Categories.ProductivityTools_Project_JiraIntegration, "Jira Site URLs", $"The URL for each Jira system to which {Core.Constants.ProductName} will be integrated. 'System Code' is any unique 3-character code that will be used to identify the Jira system associated with the specified URL. Please note that the System Code cannot be changed once items are imported from the specified system.", RegistryStorageFlags.System, RegistryOptions.MustOverrideDefaultValue, new CodeDescriptionPairList());

			TestGenericRegistryItem(ItemSet.DependencyLinkTypeMapping, "DependencyLinkTypeMapping", ProcessManagementRegistry.Categories.ProductivityTools_Project_JiraIntegration_DataMapping_LinkType, "Dependency Link Type Mapping", "The Jira Issue Link types that will become Dependency Links between Work Items when imported.", RegistryStorageFlags.System);
			TestGenericRegistryItem(ItemSet.ParentChildLinkTypeMapping, "ParentChildLinkTypeMapping", ProcessManagementRegistry.Categories.ProductivityTools_Project_JiraIntegration_DataMapping_LinkType, "Parent/Child Link Type Mapping", "The Jira Issue Link types that will become Parent/Child Links between Work Items when imported.", RegistryStorageFlags.System);

			TestGenericRegistryItem(ItemSet.CompletedIssueStatusTypesMapping, "CompletedIssueStatusTypesMapping", ProcessManagementRegistry.Categories.ProductivityTools_Project_JiraIntegration_DataMapping_IssueStatus, "Completed Issue Status Types Mapping", "The Jira Issue Status types that represent completed issues. This can be used to filter the issues that are imported from Jira. Note that this Registry Item uses the Jira Status, not the Jira Status Category.", RegistryStorageFlags.System);
			TestGenericRegistryItem(ItemSet.WorkInProgressIssueStatusTypesMapping, "WorkInProgressIssueStatusTypesMapping", ProcessManagementRegistry.Categories.ProductivityTools_Project_JiraIntegration_DataMapping_IssueStatus, "Work-in-Progress Issue Status Types Mapping", "The Jira Issue Status types that represent work-in-progress issues. This can be used to filter the issues that are imported from Jira. Note that this Registry Item uses the Jira Status, not the Jira Status Category.", RegistryStorageFlags.System);
		}

		public void TestJiraSiteUrls_Options_MustOverrideDefaultValue()
		{
			var registryItem = ProcessManagementRegistry.Instance.JiraSiteUrls;
			AssertEquals("MustOverrideDefaultValue must be true in order to stop users from unticking the Override checkbox and deleting all the systems when they might have links in them. SAD!", true, registryItem.HasOption(RegistryOptions.MustOverrideDefaultValue));
		}

		public void TestJiraIssueImportBatchSize()
		{
			TestGenericRegistryItem(
				ItemSet.JiraIssueImportBatchSize,
				"JiraIssueImportBatchSize",
				ProcessManagementRegistry.Categories.ProductivityTools_Project_JiraIntegration,
				"Work Item Creation Batch Size",
				"The number of Work Items to create per batch when importing issues for Jira projects.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				500);

			AssertEquals(500, ItemSet.JiraIssueImportBatchSize.DefaultValue);
			AssertEquals(50d, ((NumericRegistryDataType<int>)ItemSet.JiraIssueImportBatchSize.DataType).LowerBound);
			AssertEquals(2000d, ((NumericRegistryDataType<int>)ItemSet.JiraIssueImportBatchSize.DataType).UpperBound);
		}

		public void TestJiraIssueTypesMapping()
		{
			var map = new IssueTypeMap();

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivitySubtype,
				SelectionCriterionFieldValue = "FIX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Feature",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivityType,
				SelectionCriterionFieldValue = "PRD",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Epic",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_Priority,
				SelectionCriterionFieldValue = "WTF",
			});

			ProcessManagementRegistry.Instance.JiraIssueTypesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var retrievedMap = ProcessManagementRegistry.Instance.JiraIssueTypesMapping.Value;

			AssertEquals(3, retrievedMap.JiraClassificationMap.Count);

			AssertIssueTypeMap(retrievedMap.JiraClassificationMap[0], "Bug", WorkItemSchema.Constants.WKI_ActivitySubtype, "FIX");
			AssertIssueTypeMap(retrievedMap.JiraClassificationMap[1], "Feature", WorkItemSchema.Constants.WKI_ActivityType, "PRD");
			AssertIssueTypeMap(retrievedMap.JiraClassificationMap[2], "Epic", WorkItemSchema.Constants.WKI_Priority, "WTF");

			void AssertIssueTypeMap(IssueTypeMapItem item, string expectedJiraIssueType, string expectedSelectionCriterionFieldName, string expectedSelectionCriterionFieldValue)
			{
				CombineAssertions(() =>
				{
					AssertEquals("JiraEntityName", expectedJiraIssueType, item.JiraEntityName);
					AssertEquals("SelectionCriterionFieldName", expectedSelectionCriterionFieldName, item.SelectionCriterionFieldName);
					AssertEquals("SelectionCriterionFieldValue", expectedSelectionCriterionFieldValue, item.SelectionCriterionFieldValue);
				});
			}
		}

		public void TestJiraIssueTypesMapping_LookupsList()
		{
			ItemSet.WorkItemTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Bloobles");
			ItemSet.WorkItemAreaLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cramies");
			ItemSet.WorkItemActivityTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Glorpoids");
			ItemSet.WorkItemActivitySubTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Archedains");
			ItemSet.WorkItemPriorityLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Flimflams");

			var map = ProcessManagementRegistry.Instance.JiraIssueTypesMapping.Value;
			var mapItem = map.JiraClassificationMap.AddNew();
			var fieldNames = mapItem.SelectionCriterionFieldNames.Cast<ICodeDescription>().Select(cdp => Tuple.Create(cdp.Code, cdp.Description));

			AssertSequencesEqual(new[]
			{
				Tuple.Create(WorkItemSchema.Constants.WKI_WorkItemType, "Bloobles"),
				Tuple.Create(WorkItemSchema.Constants.WKI_WorkItemArea, "Cramies"),
				Tuple.Create(WorkItemSchema.Constants.WKI_ActivityType, "Glorpoids"),
				Tuple.Create(WorkItemSchema.Constants.WKI_ActivitySubtype, "Archedains"),
				Tuple.Create(WorkItemSchema.Constants.WKI_Priority, "Flimflams"),
			}, fieldNames);
		}

		public void TestJiraProjectCategoriesMapping()
		{
			var map = new ProjectCategoryMap();

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Service Desk",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_Type,
				SelectionCriterionFieldValue = "CST",
			});

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "PAVE",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_SubType,
				SelectionCriterionFieldValue = "PAV",
			});

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Something else that isn't a project",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_Module,
				SelectionCriterionFieldValue = "WTF",
			});

			ProcessManagementRegistry.Instance.JiraProjectCategoriesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var retrievedMap = ProcessManagementRegistry.Instance.JiraProjectCategoriesMapping.Value;

			AssertEquals(3, retrievedMap.JiraClassificationMap.Count);

			AssertProjectCategoryMap(retrievedMap.JiraClassificationMap[0], "Service Desk", WorkProjectSchema.Constants.WKP_Type, "CST");
			AssertProjectCategoryMap(retrievedMap.JiraClassificationMap[1], "PAVE", WorkProjectSchema.Constants.WKP_SubType, "PAV");
			AssertProjectCategoryMap(retrievedMap.JiraClassificationMap[2], "Something else that isn't a project", WorkProjectSchema.Constants.WKP_Module, "WTF");

			void AssertProjectCategoryMap(ProjectCategoryMapItem item, string expectedJiraIssueType, string expectedSelectionCriterionFieldName, string expectedSelectionCriterionFieldValue)
			{
				CombineAssertions(() =>
				{
					AssertEquals("JiraEntityName", expectedJiraIssueType, item.JiraEntityName);
					AssertEquals("SelectionCriterionFieldName", expectedSelectionCriterionFieldName, item.SelectionCriterionFieldName);
					AssertEquals("SelectionCriterionFieldValue", expectedSelectionCriterionFieldValue, item.SelectionCriterionFieldValue);
				});
			}
		}

		public void TestJiraProjectCategoriesMapping_LookupsList()
		{
			ItemSet.ProjectTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Frompies");
			ItemSet.ProjectSubtypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Grimeys");
			ItemSet.ProjectModuleLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Zooters");
			ItemSet.ProjectPriorityLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Frangipanzee");

			var map = ProcessManagementRegistry.Instance.JiraProjectCategoriesMapping.Value;
			var mapItem = map.JiraClassificationMap.AddNew();
			var fieldNames = mapItem.SelectionCriterionFieldNames.Cast<ICodeDescription>().Select(cdp => Tuple.Create(cdp.Code, cdp.Description));

			AssertSequencesEqual(new[]
			{
				Tuple.Create(WorkProjectSchema.Constants.WKP_Type, "Frompies"),
				Tuple.Create(WorkProjectSchema.Constants.WKP_SubType, "Grimeys"),
				Tuple.Create(WorkProjectSchema.Constants.WKP_Module, "Zooters"),
				Tuple.Create(WorkProjectSchema.Constants.WKP_Priority, "Frangipanzee"),
			}, fieldNames);
		}

		#endregion

		#region ConditionallyVisibleRegistryItems

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				var list = new List<string>(base.ConditionallyVisibleRegistryItems);
				list.Add("SelectionCriterion1Caption");
				list.Add("SelectionCriterion2Caption");
				list.Add("SelectionCriterion3Caption");
				list.Add("SelectionCriterion4Caption");
				list.Add("SelectionCriterion5Caption");
				list.Add("SelectionCriterion1Description");
				list.Add("SelectionCriterion2Description");
				list.Add("SelectionCriterion3Description");
				list.Add("SelectionCriterion4Description");
				list.Add("SelectionCriterion5Description");
				return list;
			}
		}

		#endregion
	}
}
