using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportBookings|DtbBookingConsolidationWorkflowDescriptor|Description", "Transport Booking Consolidation"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var consolidation = (DtbBookingConsolidation)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consolidation.Address));
			}
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return !(triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument) && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.TransportCo;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBookingConsolidation; }
		}

		public new BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;
	}
}
