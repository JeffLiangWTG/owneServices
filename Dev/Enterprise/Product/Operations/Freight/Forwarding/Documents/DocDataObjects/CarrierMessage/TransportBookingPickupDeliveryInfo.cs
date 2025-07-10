using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Integration.Packing;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class TransportBookingPickupDeliveryInfo : DocDataObject
	{
		public TransportBookingPickupDeliveryInfo()
		{
		}

		#region Address

		public IAddress Address
		{
			get => address;
			set => address = SetChild(address, value);
		}

		IAddress address;

		#endregion

		#region AddressETD

		public ZDateTime AddressETD
		{
			get => addressETD;
			set
			{
				if (SetNonPersistentPropertyValue(AddressETDInfo, ref addressETD, value))
				{
					Validate(AddressETDInfo);
				}
			}
		}

		ZDateTime addressETD;

		public ZPropertyInfo AddressETDInfo => GetZPropertyInfo(nameof(AddressETD));

		#endregion

		#region Packages

		public IEnumerable<IPkgPackage> Packages { get; set; }

		#endregion

		#region Type

		public ZString Type
		{
			get => type;
			set
			{
				if (SetNonPersistentPropertyValue(TypeInfo, ref type, value))
				{
					Validate(TypeInfo);
				}
			}
		}

		ZString type;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		#endregion
	}
}
