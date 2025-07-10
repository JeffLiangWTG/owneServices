using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ
{
	sealed class ExportPreAdviceNotification : DocDataObject, IDataSourceProvider
	{
		public ExportPreAdviceNotification(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<Container> containers;

		#endregion

		#region Shipper

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		Address shipper;

		#endregion

		#region ShipperCode

		public ZString ShipperCode
		{
			get => shipperCode;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperCodeInfo, ref shipperCode, value))
				{
					Validate(ShipperCodeInfo);
				}
			}
		}
		ZString shipperCode;

		public ZPropertyInfo ShipperCodeInfo => GetZPropertyInfo(nameof(ShipperCode));

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CarrierCode

		public ZString CarrierCode
		{
			get => carrierCode;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierCodeInfo, ref carrierCode, value))
				{
					Validate(CarrierCodeInfo);
				}
			}
		}
		ZString carrierCode;

		public ZPropertyInfo CarrierCodeInfo => GetZPropertyInfo(nameof(CarrierCode));

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
					Validate(CarrierBookingReferenceInfo);
				}
			}
		}
		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region FreightForwardersReference

		public ZString FreightForwardersReference
		{
			get => freightForwardersReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwardersReferenceInfo, ref freightForwardersReference, value))
				{
					Validate(FreightForwardersReferenceInfo);
				}
			}
		}

		ZString freightForwardersReference;

		public ZPropertyInfo FreightForwardersReferenceInfo => GetZPropertyInfo(nameof(FreightForwardersReference));

		#endregion

		#region Vessel

		public Vessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}

		Vessel vessel;

		#endregion

		#region Voyage

		public ZString Voyage
		{
			get => voyage;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageInfo, ref voyage, value))
				{
					Validate(VoyageInfo);
				}
			}
		}
		ZString voyage;

		public ZPropertyInfo VoyageInfo => GetZPropertyInfo(nameof(Voyage));

		#endregion

		#region PortOfLoad

		public Unloco PortOfLoad
		{
			get => portOfLoad;
			set => portOfLoad = SetChild(portOfLoad, value);
		}

		Unloco portOfLoad;

		#endregion

		#region PortOfDischarge

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		Unloco portOfDischarge;

		#endregion

		#region PreCarriageMode

		public ICodeDescription PreCarriageMode
		{
			get => preCarriageMode;
			set => preCarriageMode = SetChild(preCarriageMode, value);
		}

		ICodeDescription preCarriageMode;

		#endregion

		#region Origin

		public Unloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		Unloco origin;

		#endregion

		#region DepartureCTOAddress

		public Address DepartureCTOAddress
		{
			get => departureCTOAddress;
			set => departureCTOAddress = SetChild(departureCTOAddress, value);
		}

		Address departureCTOAddress;

		#endregion

		#region LoadPortFacility

		public ZString LoadPortFacility
		{
			get => loadPortFacility;
			set
			{
				if (SetNonPersistentPropertyValue(LoadPortFacilityInfo, ref loadPortFacility, value))
				{
					Validate(LoadPortFacilityInfo);
				}
			}
		}
		ZString loadPortFacility;

		public ZPropertyInfo LoadPortFacilityInfo => GetZPropertyInfo(nameof(LoadPortFacility));

		#endregion

		#region OperationalPort

		public Unloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		Unloco operationalPort;

		#endregion

		#region BookingConfirmationReference

		public ZString BookingConfirmationReference
		{
			get => bookingConfirmationReference;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationReferenceInfo, ref bookingConfirmationReference, value))
				{
					Validate(BookingConfirmationReferenceInfo);
				}
			}
		}
		ZString bookingConfirmationReference;

		public ZPropertyInfo BookingConfirmationReferenceInfo => GetZPropertyInfo(nameof(BookingConfirmationReference));

		#endregion
	}
}
