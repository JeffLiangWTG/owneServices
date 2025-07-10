using System;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class OrdersOnShipmentLimitsHelperTest : CollectionLimitHelperForPotentialHVLVTest<ForwardingShipment>
	{
		protected override IDisposable SetLimit(int limit)
		{
			return FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, limit);
		}

		protected override IDisposable SetLimitIntroductionTime(DateTime time)
		{
			return FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, time);
		}

		protected override ForwardingShipment GetNewParentBizO()
		{
			return Factory.New<ForwardingShipment>();
		}

		protected override CollectionLimitHelperForPotentialHVLV GetNewHelper(ForwardingShipment shipment)
		{
			return new OrdersOnShipmentLimitHelper(shipment);
		}

		protected override void AddNewCollectionElement(ForwardingShipment shipment)
		{
			shipment.AttachedOrders.AddNew();
		}

		protected override string NotificationForLimitOfFour
		{
			get
			{
				return @"The number of Orders on a Shipment is limited for performance and database management reasons to 4 Orders. Above 2 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";
			}
		}
	}
}
