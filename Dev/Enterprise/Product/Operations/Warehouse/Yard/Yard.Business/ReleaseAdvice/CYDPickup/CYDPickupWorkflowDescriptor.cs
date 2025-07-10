using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business;

public class CYDPickupWorkflowDescriptor : WorkflowDescriptor
{
	public override string Code => WorkflowDescriptors.CYDPickupWorkflowDescriptorCode;

	public override IMultilingualString Description => ResString.GetMultilingualString("CYDPickupWorkflowDescriptor|Description", "Container Yard Pick up");

	public override Type WorkflowProviderType => typeof(CYDPickup);

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

		if (bizObj is CYDPickup pickup)
		{
			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.GateManagement:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(pickup.LinkedYardUnit.CurrentYard.WarehouseAddress));
					break;
			}
		}
	}
}

