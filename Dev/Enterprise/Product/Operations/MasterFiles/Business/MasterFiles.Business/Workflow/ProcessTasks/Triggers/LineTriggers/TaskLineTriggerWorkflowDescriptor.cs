using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TaskLineTriggerWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public override string Code => ProcessTasksLookups.TaskLineTriggerCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("MasterFiles|TaskLineTriggerWorkflowDescriptor|Description", "Task Line Trigger");

		public override ControllerID ControllerID => ControllerIDs.ProcessTasks;

		public override Type WorkflowProviderType => typeof(ProcessTask);

		#endregion

		#region Feature Inclusion

		public override bool SupportsTasks => false;

		public override bool SupportsBufferManagement => false;

		protected override bool SupportsTaskLineTriggersCore => false;

		public override bool SupportsWorkflowTemplates => false;

		public override bool RequiresClient => false;

		public override bool SupportsWorkflowTriggerActionUniversalEventCollectionXML => false;

		internal protected override bool SupportsWorkflowTriggerActionUniversalShipmentXML => false;

		public override MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificAction(ZString triggerAction)
		{
			if (WorkflowTriggerActionTypeConstants.IsNotificationEmail(triggerAction))
			{
				return MessageRecipientPartyType.AssignedStaff
					| MessageRecipientPartyType.RequiredCapabilityMembers
					| MessageRecipientPartyType.AssignedGroupMembers
					| MessageRecipientPartyType.Email;
			}

			if (triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML)
			{
				return MessageRecipientPartyType.OrgProxy; // This is specific to XUE due to the special rule in regards to communication modes.
			}

			return MessageRecipientPartyType.None;
		}

		public override bool SupportsAssignStaffAndEmail(IBaseTrigger trigger, IBusiness parent) => parent is ProcessTaskTemplate && ((ProcessTaskTemplate)parent).P0_ProcessType == WorkflowDescriptors.GlbStaffHolidayDescriptorCode;

		protected internal override bool SupportsWorkflowTypeFiltersCore => false;

		public override bool ParentSupportsWorkflowTriggerActionUniversalShipmentXML(IBusiness parent) => LineTriggerWorkflowDescriptorHelper.ParentSupportsWorkflowTriggerActionUniversalShipmentXML(parent);

		#endregion

		#region Trigger Action Overrides

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog) =>
			source.Action.IsTriggerTypeXUE_or_XUS ? source.GetWorkflowTriggerActionForXUE_or_XUS(queuedLog) : base.GetWorkflowTriggerActionCore(source, queuedLog);

		protected override BusinessObject[] GetUDFMacroDataContextCore(ITriggerConditions workflowItem, BusinessObject parent) => LineTriggerWorkflowDescriptorHelper.GetUDFMacroDataContext(workflowItem, parent);

		#endregion

		#region Email Notifications

		protected internal override bool IsDirectEmailRecipient(string triggerParty)
		{
			return base.IsDirectEmailRecipient(triggerParty)
				|| triggerParty == MessageRecipientPartyTypeList.Codes.AssignedStaff
				|| triggerParty == MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers
				|| triggerParty == MessageRecipientPartyTypeList.Codes.AssignedGroupMembers;
		}

		protected override string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent)
		{
			if (action.PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.AssignedStaff)
			{
				return LineTriggerWorkflowDescriptorHelper.GetEmailAddresses(parent, t => new[] { t.AssignedStaffMember });
			}

			if (action.PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers)
			{
				return LineTriggerWorkflowDescriptorHelper.GetEmailAddresses(parent, task =>
				{
					var capability = task.RequiredCapability;

					if (capability == null)
					{
						return Array.Empty<GlbStaff>();
					}

					var resources = capability.ResourcesWithCapability.Cast<GlbStaff>();

					if (capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope)
					{
						return resources;
					}

					GlbGroup group = null;

					if (task.P9_GG_AssignedGroup.IsValid)
					{
						group = task.AssignedGroup;
					}
					else if (task.P9_FH_ProcessHeader.IsValid)
					{
						group = task.ProcessHeader?.ReleaseGroup as GlbGroup;
					}

					if (group != null)
					{
						var groupMembers = group.Staff.Cast<GlbStaff>().ToHashSet();

						return resources.Where(s => groupMembers.Contains(s));
					}

					return resources;
				});
			}

			if (action.PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.AssignedGroupMembers)
			{
				return LineTriggerWorkflowDescriptorHelper.GetEmailAddresses(parent, t => t.AssignedGroup?.Staff.Cast<GlbStaff>());
			}

			return base.GetNotificationEmailAddresses(action, parent);
		}
	}

	#endregion
}

