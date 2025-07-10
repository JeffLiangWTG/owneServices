using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRunSheetInstructionWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetInstructionWorkflowDescriptorCode; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("DtbConsignmentRunSheetWorkflowDescriptor|Description", "Consignment Run Sheet Instruction"); }
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return null; } // Does not have Controller ID
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbConsignmentRunSheetInstruction); }
		}

		#endregion

		#region Flags

		public override bool SupportsEventTracking => false;

		public override bool SupportsBufferManagement => false;

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsApplyWorkflowTemplate => false;

		public override bool SupportsUniversalTemplates => false;

		protected override bool SupportsWorkflowTriggerActionUniversalShipmentXML => true;

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var instruction = bizObj as DtbConsignmentRunSheetInstruction;

			if (instruction != null)
			{
				var actionType = partyType == MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp;

				if (ConsignmentRunSheetHelper.IsDepotInstruction(instruction, actionType) &&
					(partyType == MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse || partyType == MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ConsignmentRunSheetHelper.GetFirstAction(instruction, actionType).ConsignmentAddress.Address));
				}
			}
		}

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.ArrivalTransitWarehouse |
				MessageRecipientPartyType.DepartureTransitWarehouse;
		}

		#endregion

		#region SupportedTriggerPartyServicesCore

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseDispatch };

				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.SupportedTriggerPartyServicesCore(recipient);
			}
		}

		#endregion
	}
}
