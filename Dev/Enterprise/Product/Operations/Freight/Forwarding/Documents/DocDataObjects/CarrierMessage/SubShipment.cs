using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class SubShipment : DocDataObject
	{
		public SubShipment(object identifier)
		: base(identifier)
		{
		}

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentNumberInfo);
				}
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region ShippersRef

		public ZString ShippersRef
		{
			get => shippersRef;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersRefInfo, ref shippersRef, value))
				{
					Validate(ShippersRefInfo);
				}
			}
		}
		ZString shippersRef;

		public ZPropertyInfo ShippersRefInfo => GetZPropertyInfo(nameof(ShippersRef));

		#endregion

		#region MessageRef

		public ZString MessageRef
		{
			get => messageRef;
			set
			{
				if (SetNonPersistentPropertyValue(MessageRefInfo, ref messageRef, value))
				{
					Validate(MessageRefInfo);
				}
			}
		}
		ZString messageRef;

		public ZPropertyInfo MessageRefInfo => GetZPropertyInfo(nameof(MessageRef));

		#endregion

		#region Addresses

		public Address BookingParty
		{
			get => bookingParty;
			set => bookingParty = SetChild(bookingParty, value);
		}
		Address bookingParty;

		#endregion

		#region Ports

		public Unloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		Unloco origin;

		public Unloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		Unloco destination;

		#endregion
	}
}
