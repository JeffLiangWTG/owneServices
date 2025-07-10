using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs.US.USAMS;

namespace Enterprise.Customs.US.AMS.Business
{
	public class USAMSWorkflowDescriptor : WorkflowDescriptor
	{
		#region Overrides of WorkflowDescriptor

		public override string Code
		{
			get { return WorkflowDescriptors.USAMSWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("USAMSWorkflowDescriptor|Description", "US AMS"); }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return ""; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.CusInBondHeader }; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy;
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendManifestMessage);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			if (result != null)
			{
				return result;
			}

			var action = source.Action;
			if (source.Job is ICustomsManifestMessageSupporter supporter)
			{
				if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage)
				{
					return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
				}
			}

			return null;
		}

		#endregion

		#region Criteria Requirements

		public override bool RequiresBranch { get { return true; } }
		public override bool RequiresPort1 { get { return true; } }
		public override bool RequiresPort2 { get { return true; } }
		public override bool RequiresClient { get { return false; } }

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CusInBondHeader); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.AMS; }
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
		}
	}
}
