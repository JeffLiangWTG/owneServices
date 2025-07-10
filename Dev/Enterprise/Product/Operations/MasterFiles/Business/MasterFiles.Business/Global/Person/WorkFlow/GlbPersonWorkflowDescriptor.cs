using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.GlbPersonDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("B60813BE-388C-4AAE-A652-FDC0EA13CFE7", "Person Intelligence"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(GlbPerson); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.GlbPerson; }
		}

		public override bool RequiresClient => false;

		#endregion

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email;
		}

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
			get { return new[] { BusinessContext.GlbPerson }; }
		}

		public override MessageRecipientPartyType SupportedSendDocumentMessageRecipientParties(ZString triggerAction)
		{
			return base.SupportedSendDocumentMessageRecipientParties(triggerAction) | MessageRecipientPartyType.PersonalEmail | MessageRecipientPartyType.PersonPrimaryWorkEmail | MessageRecipientPartyType.PersonalFallbackPrimaryWorkEmail;
		}
	}
}
