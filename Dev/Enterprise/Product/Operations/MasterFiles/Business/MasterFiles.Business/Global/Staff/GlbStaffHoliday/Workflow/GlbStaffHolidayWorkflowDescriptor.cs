using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description
		public const string WorkflowDescriptorGlbStaffHolidayDescriptorCode = WorkflowDescriptors.GlbStaffHolidayDescriptorCode;
		public override string Code => WorkflowDescriptorGlbStaffHolidayDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("D507E05A-0EDA-44B1-867D-37828EDB5799", "Staff Holiday");

		public override ControllerID ControllerID => ControllerIDs.GlbStaffHoliday;

		public override Type WorkflowProviderType => typeof(GlbStaffHoliday);

		#endregion

		#region Capabilities

		public override bool RequiresClient => false;

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsScreenLayout => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.FirstApprovalTask;
		protected internal override bool IsDirectEmailRecipient(string triggerParty)
		{
			return base.IsDirectEmailRecipient(triggerParty)
				|| triggerParty == MessageRecipientPartyTypeList.Codes.FirstApprovalTask;
		}

		protected override string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent)
		{
			switch (action.PQ_Calc_TriggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.FirstApprovalTask:
					var holiday = (parent as GlbStaffHoliday);
					var emailAddress = FirstMeaningfulApprovalTasksEmailAddress.GetEmail(holiday, this, Factory);
					// fallback if empty
					return emailAddress.Any() ? emailAddress : new[] { action.PQ_EmailAddr.ToString() };
				default:
					return base.GetNotificationEmailAddresses(action, parent);
			}
		}

		public new BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
		#endregion

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypesList == null)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					GlbStaffHoliday holiday = factory.New<GlbStaffHoliday>();

					subTypesList = new ProcessTemplateSubType[] {
						new ProcessTemplateSubType(Res.GetString("a55d3f02-0506-4737-b972-3c6cb15ebb70", "Leave Type"), SystemDataRegistry.Instance.StaffLeaveTypes.Value),
						new ProcessTemplateSubType(Res.GetString("0adcf13a-f9b2-4e8f-9e48-9d7e97844d94", "Approval Status"), holiday.Lookups.Statuses),
					};
				}
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;
	}

	class FirstMeaningfulApprovalTasksEmailAddress
	{
		public static string[] GetEmail(IWorkflowProvider workflowProvider, WorkflowDescriptor descriptor, BusinessObjectFactory factory)
		{
			var registryItem = new WorkflowTaskTypesRegistryItem(
				"ProcessManagerTaskTypes",
				RawDataRegistry.Categories.WorkflowManager,
				null,
				null,
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
			var collection = registryItem.Value;

			var approvalTasks = collection.GetTaskTypesFromWorkflowCode(descriptor.Code)
				.Cast<WorkflowTaskType>()
				.Where(t => t.IsApprovalTask)
				.ToArray();

			workflowProvider.WorkflowItems.Load();

			var staffQuery = workflowProvider.WorkflowItems
				.Where(w => !w.P9_GS_NKAssignedStaffMember.IsEmpty
					&& (w.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned || w.P9_Status == ProcessTaskStatusCodeList.Codes.Closed || w.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled)
					&& approvalTasks.Any(a => a.Code == w.P9_Type))
				.Select(w => w.P9_GS_NKAssignedStaffMember)
				.Distinct()
				.ToArray();

			if (!staffQuery.Any())
			{
				return Array.Empty<string>();
			}

			var combinedQuery = new ZQuery(GlbStaffSchema.GS_Code, staffQuery);

			var staff = factory.Load<GlbStaff>(combinedQuery);
			if (staff == null || !staff.Any())
			{
				return Array.Empty<string>();
			}
			return staff.Select(s => s.GS_EmailAddress.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
		}
	}
}
