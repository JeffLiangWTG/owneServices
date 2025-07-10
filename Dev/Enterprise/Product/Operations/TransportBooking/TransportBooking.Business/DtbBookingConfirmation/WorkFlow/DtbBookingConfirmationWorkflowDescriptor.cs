using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConfirmationWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportBookings|DtbBookingConfirmationWorkflowDescriptor|Description", "Transport Booking Confirmation"); }
		}

		public override ControllerID ControllerID
		{
			get { return null; } // Does not have controller ID
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbBookingConfirmation); }
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var confirmation = (DtbBookingConfirmation)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(confirmation.Booking.Address));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				foreach (var instruction in confirmation.Booking.Instructions.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNR && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				foreach (var instruction in confirmation.Booking.Instructions.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNE && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.NotifyParty)
			{
				var notifyParty = confirmation.Booking.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
				if (notifyParty != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(notifyParty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BillToParty)
			{
				var billToParty = confirmation.Booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
				if (billToParty != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(billToParty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				var bookedBy = confirmation.Booking.ConsolidationSingleJob.BookedByAddress;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(bookedBy));
			}
		}

		public override bool ParentSupportsWorkflowTriggerActionUniversalShipmentXML(IBusiness parent)
		{
			return parent is IWorkflowProvider;
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return !(triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument) && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger task, IBusiness parent)
		{
			return
				MessageRecipientPartyType.TransportCo |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.NotifyParty;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		public override bool SupportsWorkflowTemplates
		{
			get { return false; }
		}

		public override bool SupportsUniversalTemplates => false;

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			if (source.Action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML)
			{
				return GetWorkflowTriggerActionForXUS(source, queuedLog);
			}
			else
			{
				return base.GetWorkflowTriggerActionCore(source, queuedLog);
			}
		}

		DtbBookingWorkflowProcessor GetWorkflowTriggerActionForXUS(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var action = source.Action;
			var processTask = source.Action.ProcessTask;
			var processTaskWorkflowDescriptor = processTask.WorkflowDescriptorCore;
			var eventInfo = new EventInfoProvider(queuedLog, source.Trigger, processTask);
			var booking = (source.Job as DtbBookingConfirmation).Booking;

			var actionWrapper = new ActionWrapper(action, booking, source.EventProvider, queuedLog);

			var processor = new UniversalShipmentTriggerActionBuilder(processTaskWorkflowDescriptor, actionWrapper, eventInfo).GetUniversalWorkflowProcessor();

			return new DtbBookingWorkflowProcessor(processor, source.Job.Factory);
		}
	}
}
