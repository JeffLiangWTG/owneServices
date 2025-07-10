using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business;

public class CYDDeliveryWorkflowDescriptor : WorkflowDescriptor
{
	public override string Code => WorkflowDescriptors.CYDDeliveryWorkflowDescriptorCode;

	public override IMultilingualString Description => ResString.GetMultilingualString("CYDDeliveryWorkflowDescriptor|Description", "Container Yard Delivery");

	public override Type WorkflowProviderType => typeof(CYDDelivery);

	public override ControllerID ControllerID => null;

	public override bool SupportsWorkflowTemplates => false;

	public override bool SupportsEventTracking => true;

	public override bool SupportsBufferManagement => false;

	public override bool AreTasksCompanySpecific => false;

	public override bool SupportsUniversalTemplates => false;

	protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;

	public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
	{
		return MessageRecipientPartyType.GateManagement;
	}

	protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
	{
		base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

		if (bizObj is CYDDelivery delivery)
		{
			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.GateManagement:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(delivery.LinkedYardUnit.CurrentYard.WarehouseAddress));
					break;
			}
		}
	}
}

