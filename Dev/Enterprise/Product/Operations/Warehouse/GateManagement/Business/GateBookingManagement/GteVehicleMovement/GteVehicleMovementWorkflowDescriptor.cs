using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteVehicleMovementWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("GteVehicleMovementWorkflowDescriptor|Description", "Vehicle Movement");

		public override ControllerID ControllerID => ControllerIDs.GteVehicleMovement;

		public override Type WorkflowProviderType => typeof(GteVehicleMovement);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool RequiresClient => false;

		public override bool RequiresBranch => false;

		public override bool RequiresWarehouse => false;

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
					new ProcessTemplateSubType(Res.GetString("88c5f2ea-f1bc-43a7-4a4c-c6d9be6e4470", "Facility Type"), warehouseTypes)
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

			var bookings = ((GteVehicleMovement)bizObj).GateMovements.Select(x => x.GateMovementBooking.Booking).DistinctBy(x => x.PK);
			messageTriggerParties.AddRangeNotNullAndNotDuplicatedItems(Enumerable.Select(bookings, x => x.GetMessageRecipientParty(partyType)).ToArray());
		}
	}
}
