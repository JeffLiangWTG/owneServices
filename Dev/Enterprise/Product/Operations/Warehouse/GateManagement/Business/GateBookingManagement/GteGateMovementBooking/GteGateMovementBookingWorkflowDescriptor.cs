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
	public class GteGateMovementBookingWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GteGateMovementBookingWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("GteGateMovementBookingWorkflowDescriptor|Description", "Gate Movement Booking");

		public override ControllerID ControllerID => ControllerIDs.GteGateMovementBooking;

		public override Type WorkflowProviderType => typeof(GteGateMovementBooking);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool RequiresClient => false;

		public override bool RequiresBranch => false;

		public override bool RequiresWarehouse => true;

		public override ZString WarehouseName => Res.GetString("951dca33-4a0f-3f81-4806-597313f8b45d", "Facility");

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
					new ProcessTemplateSubType(Res.GetString("59265cf6-333e-1697-40e3-c7ff78aeffc8", "Facility Type"), warehouseTypes)
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

			var booking = ((GteGateMovementBooking)bizObj).Booking;
			messageTriggerParties.AddNotNullAndNotDuplicatedItem(booking.GetMessageRecipientParty(partyType));
		}
	}
}
