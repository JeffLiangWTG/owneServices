using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class ConsolidationAdvice : DocDataObject, IDataSourceProvider
	{
		public ConsolidationAdvice(ZString sourceType, ZString sourceID)
		{
			SourceType = sourceType;
			SourceID = sourceID;
		}

		#region IDataSourceProvider members

		public ZString SourceID
		{
			get => sourceID;
			set
			{
				sourceID = value;
				SourceIDInfo.RefreshBinding();
			}
		}
		ZString sourceID;

		public ZPropertyInfo SourceIDInfo => GetZPropertyInfo(nameof(SourceID));

		public ZString SourceType { get; }

		#endregion IDataSourceProvider members

		#region Addresses

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		Address currentUser;

		public Address BookingParty
		{
			get => bookingParty;
			set => bookingParty = SetChild(bookingParty, value);
		}
		Address bookingParty;

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}
		Address sendingForwarder;

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}
		Address receivingForwarder;

		#endregion

		#region Estimated Time of Departure

		public ZDateTime EstimatedTimeDeparture
		{
			get => estimatedTimeDeparture;
			set
			{
				if (SetNonPersistentPropertyValue(EstimatedTimeDepartureInfo, ref estimatedTimeDeparture, value))
				{
					Validate(EstimatedTimeDepartureInfo);
				}
			}
		}
		ZDateTime estimatedTimeDeparture;

		public ZPropertyInfo EstimatedTimeDepartureInfo => GetZPropertyInfo(nameof(EstimatedTimeDeparture));

		#endregion

		#region Estimated Time of Arrival

		public ZDateTime EstimatedTimeArrival
		{
			get => estimatedTimeArrival;
			set
			{
				if (SetNonPersistentPropertyValue(EstimatedTimeArrivalInfo, ref estimatedTimeArrival, value))
				{
					Validate(EstimatedTimeArrivalInfo);
				}
			}
		}
		ZDateTime estimatedTimeArrival;

		public ZPropertyInfo EstimatedTimeArrivalInfo => GetZPropertyInfo(nameof(EstimatedTimeArrival));

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

		public Unloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		Unloco portOfLoading;

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		Unloco portOfDischarge;

		#endregion

		#region VesselName

		public ZString VesselName
		{
			get => vesselName;
			set
			{
				if (SetNonPersistentPropertyValue(VesselNameInfo, ref vesselName, value))
				{
					Validate(VesselNameInfo);
				}
			}
		}
		ZString vesselName;

		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(nameof(VesselName));

		#endregion

		#region LloydsIMO

		public ZString LloydsIMO
		{
			get => lloydsIMO;
			set
			{
				if (SetNonPersistentPropertyValue(LloydsIMOInfo, ref lloydsIMO, value))
				{
					Validate(LloydsIMOInfo);
				}
			}
		}
		ZString lloydsIMO;

		public ZPropertyInfo LloydsIMOInfo => GetZPropertyInfo(nameof(LloydsIMO));

		#endregion

		#region VoyageNumber

		public ZString VoyageNumber
		{
			get => voyageNumber;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageNumberInfo, ref voyageNumber, value))
				{
					Validate(VoyageNumberInfo);
				}
			}
		}
		ZString voyageNumber;

		public ZPropertyInfo VoyageNumberInfo => GetZPropertyInfo(nameof(VoyageNumber));

		#endregion

		#region MasterBillNumber

		public ZString MasterBillNumber
		{
			get => masterBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(MasterBillNumberInfo, ref masterBillNumber, value))
				{
					Validate(MasterBillNumberInfo);
				}
			}
		}
		ZString masterBillNumber;

		public ZPropertyInfo MasterBillNumberInfo => GetZPropertyInfo(nameof(MasterBillNumber));

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get => bookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(BookingReferenceInfo, ref bookingReference, value))
				{
					Validate(BookingReferenceInfo);
				}
			}
		}
		ZString bookingReference;

		public ZPropertyInfo BookingReferenceInfo => GetZPropertyInfo(nameof(BookingReference));

		#endregion

		#region ContractNumber

		public ZString ContractNumber
		{
			get => contractNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContractNumberInfo, ref contractNumber, value))
				{
					Validate(ContractNumberInfo);
				}
			}
		}

		ZString contractNumber;

		public ZPropertyInfo ContractNumberInfo => GetZPropertyInfo(nameof(ContractNumber));

		#endregion

		#region Shipments

		public IReadOnlyCollection<SubShipment> SubShipments
		{
			get => subShipments;
			set => subShipments = SetChildCollection(subShipments, value);
		}

		IReadOnlyCollection<SubShipment> subShipments;

		#endregion

		#region Transport

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		Transports transports;

		#endregion

		#region BookingConfirmationNotes

		public ZString BookingConfirmationNotes
		{
			get => bookingConfirmationNotes;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationNotesInfo, ref bookingConfirmationNotes, value))
				{
					Validate(BookingConfirmationNotesInfo);
				}
			}
		}
		ZString bookingConfirmationNotes;

		public ZPropertyInfo BookingConfirmationNotesInfo => GetZPropertyInfo(nameof(BookingConfirmationNotes));

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region TransportMode

		public ICodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(TransportMode, value);
		}
		ICodeDescription transportMode;

		#endregion

		#region Total

		public ZInt TotalQuantity
		{
			get => totalQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(TotalQuantityInfo, ref totalQuantity, value))
				{
				}
			}
		}
		ZInt totalQuantity;

		public ZPropertyInfo TotalQuantityInfo => GetZPropertyInfo(nameof(TotalQuantity));

		public ZDecimal TotalWeight
		{
			get => totalWeight;
			set
			{
				if (SetNonPersistentPropertyValue(TotalWeightInfo, ref totalWeight, value))
				{
				}
			}
		}
		ZDecimal totalWeight;

		public ZPropertyInfo TotalWeightInfo => GetZPropertyInfo(nameof(TotalWeight));

		public ZDecimal TotalVolume
		{
			get => totalVolume;
			set
			{
				if (SetNonPersistentPropertyValue(TotalVolumeInfo, ref totalVolume, value))
				{
				}
			}
		}
		ZDecimal totalVolume;

		public ZPropertyInfo TotalVolumeInfo => GetZPropertyInfo(nameof(TotalVolume));

		#endregion

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

		#endregion
	}
}
