using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business
{
	public class PickupAndDeliveryPair
	{
		public PickupAndDeliveryPair(IDocAddress pickupAddress, IDocAddress deliveryAddress)
		{
			PickupAddress = pickupAddress;
			DeliveryAddress = deliveryAddress;
		}

		public readonly IDocAddress PickupAddress;
		public readonly IDocAddress DeliveryAddress;

		#region GetHashCode

		public override int GetHashCode()
		{
			return (PickupKey + DeliveryKey).GetHashCode();
		}

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			var pickupAndDeliveryPair = obj as PickupAndDeliveryPair;

			return
				pickupAndDeliveryPair != null &&
				PickupKey == pickupAndDeliveryPair.PickupKey &&
				DeliveryKey == pickupAndDeliveryPair.DeliveryKey;
		}

		#endregion

		#region Keys

		public ZString PickupKey
		{
			get { return pickupKey ?? (pickupKey = GetAddressUniqueKey(PickupAddress)); }
		}

		string pickupKey;

		#endregion

		#region DeliveryKey

		public ZString DeliveryKey
		{
			get { return deliveryKey ?? (deliveryKey = GetAddressUniqueKey(DeliveryAddress)); }
		}

		string deliveryKey;

		#endregion

		#region GetAddressUniqueKey

		static string GetAddressUniqueKey(IDocAddress address)
		{
			return !address.E2_AddressOverride ? ((BusinessObject)address).PK.ToString() : address.GetAddressUniqueKey();
		}

		#endregion
	}
}
