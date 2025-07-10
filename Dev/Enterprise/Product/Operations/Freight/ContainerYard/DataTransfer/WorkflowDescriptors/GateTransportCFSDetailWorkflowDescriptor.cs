using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Freight.ContainerYard.DataTransfer.Res;
using ResString = Enterprise.Freight.ContainerYard.DataTransfer.ResString;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GateTransportCFSWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Freight|GateTransportCFSDetailWorkflowDescriptor|Description", "Gate Transport CFS");

		public override Type WorkflowProviderType => typeof(GateTransportCFSDetail);

		public override ControllerID ControllerID => null;

		public override bool SupportsTasks => false;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		//will be supported in later work item.
		//protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;

		public override bool RequiresClient => true;

		public override bool RequiresWarehouse => true;

		public override bool RequiresBranch => true;

		public override ZString WarehouseName => Res.GetString("{99F2E565-3FE8-4564-BD02-7627A38D99A4}", "Facility");

		//public override SchemaColumn[] WorkflowTriggerFieldColumns
		//{
		//	get
		//	{
		//		return new SchemaColumn[]
		//		{
		//			GateTransportCFSDetailSchema.GTF_IsPickup,
		//		};
		//	}
		//}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Client | MessageRecipientPartyType.TransportCo;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var detail = (GateTransportCFSDetail)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(detail.Owner.MainAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				var transportCoAddr = detail.GateTransport?.TransportCompanyDocumentaryAddress;
				if (transportCoAddr != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(transportCoAddr));
				}
				else
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(detail.Owner.MainAddress));
				}
			}
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.GateTransportCFSDet }; }
		}
	}
}
