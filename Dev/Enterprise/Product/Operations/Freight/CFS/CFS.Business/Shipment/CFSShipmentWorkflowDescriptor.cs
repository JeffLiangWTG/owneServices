using System;
using System.Collections;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description / ControllerID

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.CFSShipment.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.CFSShipment.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ShipmentReceival; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				ArrayList list = new ArrayList();
				list.AddRange(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("b178c52e-9661-4158-93e1-55c08a6a8849", "Transport Mode"), TransportModeList));
				list.Add(new ProcessTemplateSubType(Res.GetString("65e46325-20df-4100-a09e-e5b63dec5c20", "Container / Packing Mode"), PackingModeList));

				return (ProcessTemplateSubType[])list.ToArray(typeof(ProcessTemplateSubType));
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		public CodeDescriptionPairList PackingModeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode); }
		}

		#endregion

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CFSShipmentReceival }; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CFSShipment); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.TransportCo
				| MessageRecipientPartyType.Forwarder
				| MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.Consignor;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var shipment = (CFSShipment)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.TransportCo:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.CartageCoAddr));
					break;
				case MessageRecipientPartyTypeList.Codes.Forwarder:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.HandledOnBehalfOfForwarder, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.Consignee:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsigneeDocumentaryAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.Consignor:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsignorDocumentaryAddress));
					break;
			}
		}
	}
}
