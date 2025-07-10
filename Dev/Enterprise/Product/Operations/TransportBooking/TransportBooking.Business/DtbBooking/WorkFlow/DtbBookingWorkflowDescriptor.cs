using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DtbBookingWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportBookings|DtbBookingWorkflowDescriptor|Description", "Transport Booking"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbBooking); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBooking; }
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypeInformation == null)
				{
					subTypeInformation =
					[
						new ProcessTemplateSubType(Res.GetString("b11207fd-2a59-4dad-8413-a95fc8cbae23", "Transport Mode"), BookingTransportModes),
						new ProcessTemplateSubType(Res.GetString("955922e5-76eb-4970-babb-cb123b730632", "Job Direction"), BindToLists.BookingConsolidationJobDirections),
						new ProcessTemplateSubType(Res.GetString("f73d4d07-4b2a-45c9-acaf-477819881b68", "Booking Template"), GetBookingTemplates()),
					];
				}

				return subTypeInformation;
			}
		}
		ProcessTemplateSubType[] subTypeInformation;

		CodeDescriptionPairList BookingTransportModes
		{
			get
			{
				var defaultValue = new CodeDescriptionPairList();

				defaultValue.AddPair(Constants.TransportModes.Road, ResString.GetMultilingualString("D89981E1-38FF-446D-9BEA-1D4EFEC15993", "Road"));
				defaultValue.AddPair(Constants.TransportModes.Rail, ResString.GetMultilingualString("63CBCBAC-D529-44D5-A17A-0A791AB3D82F", "Rail"));
				defaultValue.AddPair(Constants.TransportModes.InlandWaterwayTransport, ResString.GetMultilingualString("3AF6C2B0-DC99-497F-9B3D-B20270B19560", "Inland Waterway"));

				return defaultValue;
			}
		}

		CodeDescriptionPairList GetBookingTemplates()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();

			foreach (var template in BindToLists.BookingTemplates)
			{
				codeDescriptionPairList.Add(template);
			}

			return codeDescriptionPairList;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var transportBooking = (DtbBooking)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(transportBooking.Address));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				foreach (var instruction in transportBooking.Instructions.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNR && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				foreach (var instruction in transportBooking.Instructions.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNE && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.NotifyParty)
			{
				var notifyParty = transportBooking.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
				if (notifyParty != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(notifyParty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BillToParty)
			{
				var billToParty = transportBooking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
				if (billToParty != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(billToParty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				var bookedBy = transportBooking.ConsolidationSingleJob.BookedByAddress;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(bookedBy));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.CarrierBookingAgent)
			{
				var carrierBookingAgent = transportBooking.DocAddresses.FindByDocAddressType(DocAddressType.CarrierBookingAgent);
				if (carrierBookingAgent != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(carrierBookingAgent));
				}
			}
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return !(triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument) && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.TransportCo |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.NotifyParty |
				MessageRecipientPartyType.CarrierBookingAgent;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificAction(ZString triggerAction)
		{
			if (WorkflowTriggerActionTypeConstants.IsCreateTransportJob(triggerAction))
			{
				return MessageRecipientPartyType.TransportJobRegistry;
			}
			return base.SupportedMessageRecipientPartiesForSpecificAction(triggerAction);
		}

		public override bool SupportsCreateTransportJob
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.DtbBooking }; }
		}

		IEnumerable<string> IWorkflowParentWithLines.SupportedTriggerLineTypes
		{
			get { return new[] { TriggerLineTypes.Codes.DtbBookingConfirmation }; }
		}

		BindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new BindToLists(Factory)); }
		}

		BindToLists bindToLists;

		protected override bool SupportsSendCalculateCO2EmissionRequestCore => ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled;

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var wrappedProcessor = base.GetWorkflowTriggerActionCore(source, queuedLog);
			return new DtbBookingWorkflowProcessor(wrappedProcessor, source.Job.Factory);
		}

		public new BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;
	}
}
