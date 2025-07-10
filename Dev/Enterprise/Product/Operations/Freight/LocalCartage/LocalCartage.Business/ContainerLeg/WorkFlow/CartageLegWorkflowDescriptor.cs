using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageLegWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.CartageLegWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("02bd2cde-ff2f-47e8-8e10-79883b6f73c4", "Port Transport Leg"); }
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var cartageLeg = (CommonCartageLeg)bizObj;
			var cartage = cartageLeg.Cartage;

			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				if (cartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageImporter)
				{
					if (cartageLeg.DeliverToDocAddress.Organisation != null)
					{
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(cartageLeg.DeliverToDocAddress));
					}
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				if (cartageLeg.PickupDocAddressType == DocAddressType.LocalCartageExporter)
				{
					if (cartageLeg.PickupFromDocAddress.Organisation != null)
					{
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(cartageLeg.PickupFromDocAddress));
					}
				}
			}
			else if (cartage != null)
			{
				if (partyType == MessageRecipientPartyTypeList.Codes.BillToParty)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(cartage.LocalClient, ZString.Empty));
				}
				else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(cartage.BookingParty, ZString.Empty));
				}
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Consignor
				| MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.BillToParty
				| MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.BookingParty;
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return !(triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument) && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new CartageLegWorkflowCondition2CodeList();
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.ContainerLeg }; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CommonCartageLeg); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.CartageLeg; }
		}

		public new BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
