using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchConsignmentWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.TransitDispatchConsignment; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get
			{
				return ResString.GetMultilingualString("WhsItemDispatchConsignment|WhsItemDispatchConsignmentWorkflowDescriptor|Description",
					"Transit Dispatch Consignment");
			}
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsTransitDispatchConsignment; }
		}

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.TransitDspConsignmnt }; }
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
			var action = source.Action;
			var parent = source.Job;

			if (processor == null && parent is WhsItemDispatchConsignment dcn)
			{
				if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage)
				{
					processor = new TransitCRESAMessageProcessor<WhsItemDispatchConsignment>(dcn);
				}
				else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message)
				{
					processor = new TransitCIN750MessageProcessor<WhsItemDispatchConsignment>(dcn);
				}
			}
			return processor;
		}

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.Forwarder |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		#endregion

		#region SupportsBufferManagement

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var dispatchConsignment = (WhsItemDispatchConsignment)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(dispatchConsignment.ConsigneeDocAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty || partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(dispatchConsignment.BookingPartyDocAddress));
			}
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(WhsItemDispatchConsignment); }
		}

		#endregion

		#region IWorkflowParentWithLines Memebers

		IEnumerable<string> IWorkflowParentWithLines.SupportedTriggerLineTypes
		{
			get
			{
				return new[] { TriggerLineTypes.Codes.Service };
			}
		}

		#endregion
	}
}
