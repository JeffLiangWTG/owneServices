using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseContainer : DocDataObject
	{
		#region Ctor

		public PortbaseContainer(object identifier = default)
			: base(identifier)
		{
		}

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

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}
		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region CarrierBookingRef

		public ZString CarrierBookingRef
		{
			get => carrierBookingRef;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingRefInfo, ref carrierBookingRef, value))
				{
					Validate(CarrierBookingRefInfo);
				}
			}
		}
		ZString carrierBookingRef;

		public ZPropertyInfo CarrierBookingRefInfo => GetZPropertyInfo(nameof(CarrierBookingRef));

		#endregion

		#region GrossWeight

		public Measurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}
		Measurement grossWeight;

		#endregion

		#region Shipments

		public IReadOnlyCollection<PortbaseShipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}
		IReadOnlyCollection<PortbaseShipment> shipments;

		#endregion

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion
	}
}
