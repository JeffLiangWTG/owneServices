using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVConsignmentWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("eTail|HVLVConsignmentWorkflowDescriptor|Description", "HVLV Consignment");

		public override Type WorkflowProviderType => typeof(HVLVConsignment);

		public override ControllerID ControllerID => ControllerIDs.HVLVConsignment;

		public override bool SupportsWorkflowTemplates => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsCreateTransportBooking => true;

		public override bool RequiresClient => true;

		public override ZString ClientName => Res.GetString("78d2b643-5a70-4afd-b210-31e657dfe43a", "eTailer");

		public override bool RequiresBranch => true;

		public override bool RequiresPort1 => true;

		public override ZString Port1Name => Res.GetString("1068f5f2-9fe3-4454-9b6f-133cc161fc86", "Dispatch UNLOCO");

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateStandAloneDeclaration, WorkflowTriggerActionTypeConstants.Descriptions.CreateStandAloneDeclaration);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source) =>
			source.Action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateStandAloneDeclaration ?
				new CreateStandAloneDeclarationProcessor((HVLVConsignment)source.Job) :
				base.GetWorkflowTriggerActionCore(source);

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(HVLVBookingHeaderWorkflowDescriptor.GetServiceLevelSubType());

				return list.ToArray();
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			var consignment = (HVLVConsignment)bizObj;

			if (consignment != null)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.Consignor:
						var cnrRecipient = GetSingleRecipientFromShipments(consignment, x => x.ConsignorDocumentaryAddress?.Address);

						if (cnrRecipient != null)
						{
							messageRecipientParties.AddNotNullAndNotDuplicatedItem(cnrRecipient);
						}
						else if (consignment.BookingHeader != null)
						{
							var headerConsignor = consignment.BookingHeader?.BillToParty;

							if (headerConsignor != null)
							{
								messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(headerConsignor));
							}
						}

						break;

					case MessageRecipientPartyTypeList.Codes.Consignee:
						var cneRecipient = GetSingleRecipientFromShipments(consignment, x => x.ConsigneeDocumentaryAddress?.Address);

						if (cneRecipient != null)
						{
							messageRecipientParties.AddNotNullAndNotDuplicatedItem(cneRecipient);
						}

						break;
				}
			}
		}

		MessageRecipientParty GetSingleRecipientFromShipments(HVLVConsignment consignment, Func<ForwardingShipment, OrgAddress> selector)
		{
			var shipmentAddresses = consignment.Items.Cast<HVLVItem>()
				.Select(x => x.Shipment)
				.Where(x => x != null)
				.Select(selector)
				.Where(x => x != null)
				.Distinct();

			return shipmentAddresses.IsCountEqualTo(1) ? new MessageRecipientParty(shipmentAddresses.First()) : null;
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.HVLVConsignment };
	}
}
