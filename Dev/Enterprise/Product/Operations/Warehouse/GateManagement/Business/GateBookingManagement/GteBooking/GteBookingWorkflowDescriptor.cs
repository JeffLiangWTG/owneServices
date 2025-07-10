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
	public class GteBookingWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GteBookingWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("GteBookingWorkflowDescriptor|Description", "Gate Booking");

		public override ControllerID ControllerID => ControllerIDs.GteBooking;

		public override Type WorkflowProviderType => typeof(GteBooking);

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool RequiresClient => false;

		public override bool RequiresBranch => false;

		public override bool RequiresWarehouse => true;

		public override ZString WarehouseName => Res.GetString("cedcf8c4-7edf-45fc-a496-b8fe994c9ed5", "Facility");

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
					new ProcessTemplateSubType(Res.GetString("045b5bc2-660e-4750-a52c-1d3e9e19a061", "Facility Type"), warehouseTypes)
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

			var booking = (GteBooking)bizObj;
			messageTriggerParties.AddNotNullAndNotDuplicatedItem(booking.GetMessageRecipientParty(partyType));
		}
	}
}
