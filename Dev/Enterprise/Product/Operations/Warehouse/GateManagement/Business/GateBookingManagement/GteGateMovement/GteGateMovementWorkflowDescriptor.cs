using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteGateMovementWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("GteGateMovementWorkflowDescriptor|Description", "Gate Movement");

		public override ControllerID ControllerID => ControllerIDs.GteGateMovement;

		public override Type WorkflowProviderType => typeof(GteGateMovement);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool RequiresClient => false;

		public override bool RequiresBranch => false;

		public override bool RequiresWarehouse => true;

		public override ZString WarehouseName => Res.GetString("97c8dcb1-edfc-77bd-4100-cdb4ff7bf46c", "Facility");

		public override WarehouseCollectionType WarehouseType => WarehouseCollectionType.ProductWarehouse | WarehouseCollectionType.CYDWarehouse | WarehouseCollectionType.TransitWarehouse;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var warehouseTypes = new CodeDescriptionPairList();
				warehouseTypes.AddPair(WarehouseTypes.Codes.Product, WarehouseTypes.Descriptions.Product);
				warehouseTypes.AddPair(WarehouseTypes.Codes.ContainerYard, WarehouseTypes.Descriptions.ContainerYard);
				warehouseTypes.AddPair(WarehouseTypes.Codes.Transit, WarehouseTypes.Descriptions.Transit);

				var list = new List<ProcessTemplateSubType>(base.SubTypeInformation)
				{
					new ProcessTemplateSubType(Res.GetString("e1284b92-47dd-3797-4f88-0b2fcbd2b0e5", "Facility Type"), warehouseTypes)
				};

				return list.ToArray();
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.BookingParty
				| MessageRecipientPartyType.ArrivalTransitWarehouse
				| MessageRecipientPartyType.ContainerYard;
		}

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			return (string)recipient switch
			{
				MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse => [ServiceCodesList.Codes.TransitWarehouseReceive, ServiceCodesList.Codes.TransitWarehouseDispatch],
				_ => base.SupportedTriggerPartyServicesCore(recipient),
			};
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var booking = ((GteGateMovement)bizObj).GateMovementBooking.Booking;
			messageTriggerParties.AddNotNullAndNotDuplicatedItem(booking.GetMessageRecipientParty(partyType));
		}
	}
}
