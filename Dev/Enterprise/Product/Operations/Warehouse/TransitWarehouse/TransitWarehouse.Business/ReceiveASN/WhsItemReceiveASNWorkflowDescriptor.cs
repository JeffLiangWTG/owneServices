using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveASNWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.TransitReceiveASN; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get
			{
				return ResString.GetMultilingualString("WhsItemReceiveASN|WhsItemReceiveASNWorkflowDescriptor|Description",
					"Transit Receive ASN");
			}
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsItemReceiveASN; }
		}

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.TransitReceiveASN }; }
		}

		#endregion

		#region IncludeWorkflowTriggerActionXMLDebtorBalance

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance
		{
			get { return false; }
		}

		#endregion

		#region RequiresClient

		public override bool RequiresClient
		{
			get { return true; }
		}

		#endregion

		#region RequiresWarehouse

		public override bool RequiresWarehouse
		{
			get { return true; }
		}

		#endregion

		#region WarehouseType

		public override WarehouseCollectionType WarehouseType => WarehouseCollectionType.TransitWarehouse;

		#endregion

		#region SupportsBufferManagement

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		#endregion

		#region SupportsEventTracking

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#endregion

		#region SupportsSetFieldTriggerAction

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return true;
		}

		#endregion

		#region SupportsWorkflowTriggerActionUniversalEventXML

		protected override bool SupportsWorkflowTriggerActionUniversalEventXML
		{
			get { return true; }
		}

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.AirCargoResponsibleParty |
				MessageRecipientPartyType.Email;
		}

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var receiveASN = (WhsItemReceiveASN)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receiveASN.BookingPartyDocAddress));
			}
			if (partyType == MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(GlbCompany.CurrentCompany.OrgProxy.MainAddress));
			}
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(WhsItemReceiveASN); }
		}

		#endregion
	}
}
