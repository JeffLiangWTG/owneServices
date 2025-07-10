using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.TransitReceiveConsignment; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get
			{
				return ResString.GetMultilingualString("WhsItemReceiveConsignment|WhsItemReceiveConsignmentWorkflowDescriptor|Description",
					"Transit Receive Consignment");
			}
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsTransitReceiveConsignment; }
		}

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.TransitRcvConsignmnt }; }
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
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Forwarder |
				MessageRecipientPartyType.CustomsOutturnAgent |
				MessageRecipientPartyType.Email;
		}

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var receiveConsignment = (WhsItemReceiveConsignment)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receiveConsignment.BookingPartyDocAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receiveConsignment.ConsignorDocAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receiveConsignment.ConsigneeDocAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receiveConsignment.BookingPartyDocAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent)
			{
				var airManifestLineAddresses = TransitWarehouseHelper.GetMatchingOrgAddresses(bizObj, UniversalDataBuss.Integration.DataContextType.AirManifestLine);
				var underBondAddresses = TransitWarehouseHelper.GetMatchingOrgAddresses(bizObj, UniversalDataBuss.Integration.DataContextType.UnderBond);
				var addresses = airManifestLineAddresses.Concat(underBondAddresses);

				if (addresses.Any())
				{
					foreach (var address in addresses)
					{
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(address));
					}
				}
				else
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(GlbCompany.CurrentCompany.OrgProxy.MainAddress));
				}
			}
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(WhsItemReceiveConsignment); }
		}

		#endregion

		#region GetAdditionalWorkflowTriggerActionTypeList

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var additionalTriggerActionType = new CodeDescriptionPairList();
			additionalTriggerActionType.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendCRESAMessage);
			additionalTriggerActionType.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message, WorkflowTriggerActionTypeConstants.Descriptions.SendCIN750Message);

			return additionalTriggerActionType;
		}

		#endregion

		#region GetWorkflowTriggerActionCore

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var processor = base.GetWorkflowTriggerActionCore(source);
			if (processor == null)
			{
				var action = source.Action;
				var parent = source.Job;

				if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage
					&& parent is WhsItemReceiveConsignment)
				{
					processor = new TransitCRESAMessageProcessor<WhsItemReceiveConsignment>(parent as WhsItemReceiveConsignment);
				}
				else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message && parent is WhsItemReceiveConsignment rcn)
				{
					processor = new TransitCIN750MessageProcessor<WhsItemReceiveConsignment>(rcn);
				}
			}
			return processor;
		}

		#endregion

		#region IWorkflowParentWithLines Memebers

		IEnumerable<string> IWorkflowParentWithLines.SupportedTriggerLineTypes
		{
			get
			{
				return new[] { TriggerLineTypes.Codes.PkgPackage, TriggerLineTypes.Codes.Service };
			}
		}

		#endregion
	}
}
