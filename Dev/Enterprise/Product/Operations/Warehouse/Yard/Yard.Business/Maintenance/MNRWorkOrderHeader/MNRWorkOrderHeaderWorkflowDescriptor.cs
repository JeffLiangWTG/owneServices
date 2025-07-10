using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.MNRWorkOrderHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("MNRWorkOrderHeaderWorkflowDescriptor|Description", "Container Yard MNR Work Order Header");

		public override ControllerID ControllerID => ControllerIDs.MNRWorkOrder;

		public override Type WorkflowProviderType => typeof(MNRWorkOrderHeader);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Client
				| MessageRecipientPartyType.ControllingCustomer;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var yardUnitPK = (bizObj as MNRWorkOrderHeader)?.MWO_ParentID;
			var relatetedReceiveAdvice = bizObj.Factory.Load<CYDYardUnitState>(yardUnitPK.Value)?.ReceiveAdvice;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.Client:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(relatetedReceiveAdvice?.Client?.Address));
					break;
				case MessageRecipientPartyTypeList.Codes.ControllingCustomer:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(relatetedReceiveAdvice?.Lessee?.Address));
					break;
			}
		}
	}
}
