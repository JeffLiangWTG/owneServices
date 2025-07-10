using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class DeliveryDueDateCalculationLogHelper
	{
		public static ZStringBuilder InitCalculationLog(DeliveryDueDateCalculationContext context)
		{
			var calculationLogBuilder = new ZStringBuilder();

			calculationLogBuilder.AppendLine(Res.GetString("6ed3a915-10e2-4213-b626-5237867839a0", "Parameters used for this calculation:"));
			calculationLogBuilder.AppendLine(Res.GetString("799fe2ef-deed-41df-9f3d-e0740d3ca52a", "DTC: {0}", context.IsDTC));
			calculationLogBuilder.AppendLine(Res.GetString("ca16d4a3-aa72-4f11-9b8c-1e5bc2f27772", "Service Level: {0}",
				context.ServicelevelCode.IsEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.ServicelevelCode.ToString()));
			calculationLogBuilder.AppendLine(Res.GetString("ec64a963-5d90-46e5-b560-f30bd4876f7a", "HBL Dlv. Mode: {0}",
				context.HBLDeliveryMode.IsEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.HBLDeliveryMode.ToString()));
			var cfsPickupIsValid = context.CFSPickupAddress is IDocAddress pickupCFS && pickupCFS?.Organisation != null;
			calculationLogBuilder.AppendLine(Res.GetString("05d01fba-17cc-484e-92d7-d97648234c3f", "Pickup CFS/Transit Warehouse: {0}",
				cfsPickupIsValid ? (context.CFSPickupAddress as IDocAddress).Organisation.Code.ToString() : DeliveryDueDateCalculationHelper.EmptyValueSignForLog));
			var pickupAddressIsEmpty = context.PickupAddress?.AddressFull.IsEmpty ?? true;
			calculationLogBuilder.AppendLine(Res.GetString("bd6f7404-b479-4dc9-b677-e34ee53df815", "Pickup Address: {0}",
				pickupAddressIsEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.PickupAddress.AddressFull.ToString()));
			var cfsPickupAddressEmpty = context.CFSPickupAddress?.AddressFull.IsEmpty ?? true;
			calculationLogBuilder.AppendLine(Res.GetString("e0308b67-0fb9-4d90-9108-6c292e9a1a9a", "Pickup CFS Address: {0}",
				cfsPickupAddressEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.CFSPickupAddress.AddressFull.ToString()));
			var cfsDeliveryIsValid = context.CFSDeliveryAddress is IDocAddress deliveryCFS && deliveryCFS?.Organisation != null;
			calculationLogBuilder.AppendLine(Res.GetString("b02b861f-497a-4d90-8ca1-5c95fa7d374b", "Delivery CFS/Transit Warehouse: {0}",
				cfsDeliveryIsValid ? (context.CFSDeliveryAddress as IDocAddress).Organisation.Code.ToString() : DeliveryDueDateCalculationHelper.EmptyValueSignForLog));
			var deliveryAddressIsEmpty = context.DeliveryAddress?.AddressFull.IsEmpty ?? true;
			calculationLogBuilder.AppendLine(Res.GetString("74225e2b-fab6-4c79-aee9-a0f86fd7fbb0", "Delivery Address: {0}",
				deliveryAddressIsEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.DeliveryAddress.AddressFull.ToString()));
			var deliverAgentAddressEmpty = context.DeliveryAgentAddress?.AddressFull.IsEmpty ?? true;
			calculationLogBuilder.AppendLine(Res.GetString("a5ac1518-3176-4244-bbcb-2b5c631af7e8", "Delivery Agent Address: {0}",
				deliverAgentAddressEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.DeliveryAgentAddress.AddressFull.ToString()));
			calculationLogBuilder.AppendLine(Res.GetString("f10d3a3d-da7f-406c-aedf-3a1ff828a575", "Calculation Start Date: {0}",
				context.ReadyDate.IsEmpty ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : context.ReadyDate.ToString()));
			calculationLogBuilder.AppendLine(Res.GetString("a8b7dcd8-c034-4a53-874a-ba4c69bbd30d", "{0} has been selected as calculation start date",
				context.WhichDateSelectedAsReadyDate));

			return calculationLogBuilder;
		}
	}
}
