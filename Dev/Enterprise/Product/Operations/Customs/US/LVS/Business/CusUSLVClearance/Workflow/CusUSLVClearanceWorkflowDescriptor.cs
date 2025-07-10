using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVClearanceWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("22d008b6-e1a4-49ee-8001-4afd60769f63", "US Customs Low Value Entries");

		public override ControllerID ControllerID => ControllerIDs.Customs.US.USLowValueEntries;

		public override Type WorkflowProviderType => typeof(CusUSLVClearance);

		public override bool SupportsEventTracking => true;

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness bizo)
		{
			yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) =>
			MessageRecipientPartyType.OrgProxy |
			MessageRecipientPartyType.Email |
			MessageRecipientPartyType.HVLVForwarder;

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.HVLVForwarder)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(GlbCompany.CurrentCompany.OrgProxy, ZString.Empty));
			}
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendReleaseMessage);
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
			var clearance = (CusUSLVClearance)source.Job;
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage)
			{
				if (clearance is IJobDeclarationAutoSendingMessageSupporter supporter)
				{
					return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
				}
			}

			return null;
		}
	}
}
