using System;
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

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("E7A86269-7578-44C7-B41B-878B071D0387", "Accreditation Attempt");

		public override Type WorkflowProviderType => typeof(GlbAccreditationAttempt);

		public override ControllerID ControllerID => ControllerIDs.GlbAccreditationAttempt;

		public override bool RequiresClient => false;

		public override bool SupportsBufferManagement => false;

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match GlbAccreditationAttempt.GetTemplateSelectionCriteria
			// and GlbAccreditationAttemptFormCustomisationSettingProvider.GetPropertiesThatAffectWorkflow
			get
			{
				if (subTypesList == null)
				{
					subTypesList = new[]
					{
						new ProcessTemplateSubType(Res.GetString("f024cf18-cda1-4e24-84c6-434aa604600d", "Accreditation Code"), AccreditationList),
					};
				}
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;

		CodeDescriptionPairList AccreditationList
		{
			get
			{
				if (accreditationList == null)
				{
					accreditationList = new CodeDescriptionPairList();
					var factory = new BusinessObjectFactory();
					var accreditations = factory.Load<GlbAccreditation>(new ZQuery());
					foreach (var accreditation in accreditations)
					{
						accreditationList.AddPair(accreditation.HAC_Code, accreditation.HAC_Description);
					}
					accreditationList.Sort();
				}
				return accreditationList;
			}
		}

		CodeDescriptionPairList accreditationList;

		#endregion

		#region Trigger

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificAction(ZString triggerAction)
		{
			if (triggerAction == GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument)
			{
				return MessageRecipientPartyType.Email | MessageRecipientPartyType.PersonPrimaryWorkEmail | MessageRecipientPartyType.PersonalFallbackPrimaryWorkEmail;
			}

			return base.SupportedMessageRecipientPartiesForSpecificAction(triggerAction);
		}

		public override MessageRecipientPartyType SupportedSendDocumentMessageRecipientParties(ZString triggerAction)
		{
			return base.SupportedSendDocumentMessageRecipientParties(triggerAction) | MessageRecipientPartyType.PersonPrimaryWorkEmail | MessageRecipientPartyType.PersonalFallbackPrimaryWorkEmail;
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return base.IsMessagingOrEmailNotificationTriggerAction(triggerAction) || triggerAction == GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;
		}

		public override bool IsEmailSendNotificationTriggerAction(ZString triggerAction)
		{
			return base.IsEmailSendNotificationTriggerAction(triggerAction) || triggerAction == GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;
		}

		public override bool IsDocumentTriggerAction(ZString triggerAction)
		{
			return base.IsDocumentTriggerAction(triggerAction) || triggerAction == GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument, GlbAccreditationAttemptWorkflowTriggerActionTypeList.Descriptions.SendAccreditationDocument);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var action = source.Action;
			if (action.PQ_TriggerType == GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument)
			{
				return new GlbAccreditationAttemptDocumentSender(action, source.Job, source.Event);
			}
			else
			{
				return base.GetWorkflowTriggerActionCore(source, queuedLog);
			}
		}

		#endregion

		protected override string GetSubjectForEmailParty(ProcessTaskNotification action)
		{
			return action.Parent.GetDescriptionWithReference();
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.AccreditationAttempt }; }
		}
	}
}
