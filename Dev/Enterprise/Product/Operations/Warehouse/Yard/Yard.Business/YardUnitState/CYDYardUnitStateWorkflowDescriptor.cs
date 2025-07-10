using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStateWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CYDYardUnitStateWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("YardUnitStateWorkflowDescriptor|Description", "Container Yard Yard Unit State");

		public override ControllerID ControllerID => ControllerIDs.CYDYardUnitState;

		public override Type WorkflowProviderType => typeof(CYDYardUnitState);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Client
				| MessageRecipientPartyType.DeliveryCartage
				| MessageRecipientPartyType.PickupCartage
				| MessageRecipientPartyType.Warehouse;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			if (bizObj is CYDYardUnitState yardUnit)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.Client:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(yardUnit.ReceiveAdvice?.Client.Organisation, ZString.Empty));
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(yardUnit.ReleaseAdvice?.Client.Organisation, ZString.Empty));
						break;
					case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(yardUnit.ReceiveTransportationUnit?.TransportCompanyDocAddress.Organisation, ZString.Empty));
						break;
					case MessageRecipientPartyTypeList.Codes.PickupCartage:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(yardUnit.DispatchTransportationUnit?.TransportCompanyDocAddress.Organisation, ZString.Empty));
						break;
					case MessageRecipientPartyTypeList.Codes.Warehouse:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(yardUnit.CurrentYard.WarehouseAddress.Header, ZString.Empty));
						break;
				}
			}
		}
	}
}
