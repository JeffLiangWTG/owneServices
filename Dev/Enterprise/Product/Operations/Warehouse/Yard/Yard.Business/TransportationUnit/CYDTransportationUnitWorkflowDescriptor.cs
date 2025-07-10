using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		public override string Code => WorkflowDescriptors.CYDTransportationUnitWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("TransportationUnitWorkflowDescriptor|Description", "Container Yard Transportation Unit");

		public override ControllerID ControllerID => ControllerIDs.CYDTransportationUnit;

		public override Type WorkflowProviderType => typeof(CYDTransportationUnit);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override ZString ClientName => Res.GetString("{e0c75ac6-41b4-44eb-b4e9-1749eceffa35}", "Transport Provider");

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.GateManagement;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			if (bizObj is CYDTransportationUnit tpu)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.GateManagement:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(tpu.Yard.WarehouseAddress));
						break;
				}
			}
		}

		#region IWorkflowParentWithLines Members

		IEnumerable<string> IWorkflowParentWithLines.SupportedTriggerLineTypes => [TriggerLineTypes.Codes.CYDDelivery, TriggerLineTypes.Codes.CYDPickup];

		#endregion
	}
}
