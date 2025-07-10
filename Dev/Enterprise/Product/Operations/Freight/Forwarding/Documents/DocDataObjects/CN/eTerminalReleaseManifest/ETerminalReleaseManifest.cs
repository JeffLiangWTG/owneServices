using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ETerminalReleaseManifest : DocDataObject, IDataSourceProvider, IAdditionalReferenceProvider
	{
		public ETerminalReleaseManifest(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region IDataSourceProvider members

		public ZString SourceType { get; }
		public ZString SourceID { get; }
		public ZString DocumentName { get; }

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

		#region UseMasterBillAsMasterSO

		public ZBool UseMasterBillAsMasterSO
		{
			get => useMasterBillAsMasterSO;
			set
			{
				if (SetNonPersistentPropertyValue(UseMasterBillAsMasterSOInfo, ref useMasterBillAsMasterSO, value))
				{
					Validate(UseMasterBillAsMasterSOInfo);
				}
			}
		}

		ZBool useMasterBillAsMasterSO;

		public ZPropertyInfo UseMasterBillAsMasterSOInfo => GetZPropertyInfo(nameof(UseMasterBillAsMasterSO));

		#endregion

		#region UseBkgRefAsMasterSO

		public ZBool UseBkgRefAsMasterSO
		{
			get => useBkgRefAsMasterSO;
			set
			{
				if (SetNonPersistentPropertyValue(UseBkgRefAsMasterSOInfo, ref useBkgRefAsMasterSO, value))
				{
					Validate(UseBkgRefAsMasterSOInfo);
				}
			}
		}

		ZBool useBkgRefAsMasterSO;

		public ZPropertyInfo UseBkgRefAsMasterSOInfo => GetZPropertyInfo(nameof(UseBkgRefAsMasterSO));

		#endregion

		#region ReleaseType

		public CodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(releaseType, value);
		}

		CodeDescription releaseType;

		#endregion

		#region NumberOfOriginals

		public ZInt NumberOfOriginals
		{
			get => numberOfOriginals;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfOriginalsInfo, ref numberOfOriginals, value))
				{
				}
			}
		}

		ZInt numberOfOriginals;

		public ZPropertyInfo NumberOfOriginalsInfo => GetZPropertyInfo(nameof(NumberOfOriginals));

		#endregion

		#region NumberOfCopies

		public ZInt NumberOfCopies
		{
			get => numberOfCopies;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value))
				{
				}
			}
		}

		ZInt numberOfCopies;

		public ZPropertyInfo NumberOfCopiesInfo => GetZPropertyInfo(nameof(NumberOfCopies));

		#endregion

		#region RequestedDateOfIssue

		public ZDateTime RequestedDateOfIssue
		{
			get => requestedDateOfIssue;
			set
			{
				if (SetNonPersistentPropertyValue(RequestedDateOfIssueInfo, ref requestedDateOfIssue, value))
				{
				}
			}
		}

		ZDateTime requestedDateOfIssue;

		public ZPropertyInfo RequestedDateOfIssueInfo => GetZPropertyInfo(nameof(RequestedDateOfIssue));

		#endregion

		#region PlaceOfIssue

		public Unloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}

		Unloco placeOfIssue;

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

		#region PlaceOfReceipt

		public Unloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}

		Unloco placeOfReceipt;

		#endregion

		#region PlaceOfDelivery

		public Unloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}

		Unloco placeOfDelivery;

		#endregion

		#region OperationalPort

		public Unloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		Unloco operationalPort;

		#endregion

		#region ContainerMode

		public CodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		CodeDescription containerMode;

		#endregion

		#region IsDoorPickup

		public ZBool IsDoorPickup
		{
			get => isDoorPickup;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorPickupInfo, ref isDoorPickup, value))
				{
				}
			}
		}

		ZBool isDoorPickup;

		public ZPropertyInfo IsDoorPickupInfo => GetZPropertyInfo(nameof(IsDoorPickup));

		#endregion

		#region IsDoorDelivery

		public ZBool IsDoorDelivery
		{
			get => isDoorDelivery;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorDeliveryInfo, ref isDoorDelivery, value))
				{
				}
			}
		}

		ZBool isDoorDelivery;

		public ZPropertyInfo IsDoorDeliveryInfo => GetZPropertyInfo(nameof(IsDoorDelivery));

		#endregion

		#region SpecialInstructions

		public ZString SpecialInstructions
		{
			get => specialInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(SpecialInstructionsInfo, ref specialInstructions, value))
				{
					Validate(SpecialInstructionsInfo);
				}
			}
		}

		ZString specialInstructions;

		public ZPropertyInfo SpecialInstructionsInfo => GetZPropertyInfo(nameof(SpecialInstructions));

		#endregion

		#region BillOfLadingNumber

		public ZString BillOfLadingNumber
		{
			get => billOfLadingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingNumberInfo, ref billOfLadingNumber, value))
				{
				}
			}
		}

		ZString billOfLadingNumber;

		public ZPropertyInfo BillOfLadingNumberInfo => GetZPropertyInfo(nameof(BillOfLadingNumber));

		#endregion

		#region ShipperReference

		public ZString ShipperReference
		{
			get => shipperReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceInfo, ref shipperReference, value))
				{
				}
			}
		}

		ZString shipperReference;

		public ZPropertyInfo ShipperReferenceInfo => GetZPropertyInfo(nameof(ShipperReference));

		#endregion

		#region FreightForwarderReference

		public ZString FreightForwarderReference
		{
			get => freightForwarderReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceInfo, ref freightForwarderReference, value))
				{
				}
			}
		}

		ZString freightForwarderReference;

		public ZPropertyInfo FreightForwarderReferenceInfo => GetZPropertyInfo(nameof(FreightForwarderReference));

		#endregion

		#region CarrierContractNumber

		public ZString CarrierContractNumber
		{
			get => carrierContractNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierContractNumberInfo, ref carrierContractNumber, value))
				{
				}
			}
		}

		ZString carrierContractNumber;

		public ZPropertyInfo CarrierContractNumberInfo => GetZPropertyInfo(nameof(CarrierContractNumber));

		#endregion

		#region MasterBookingNumber

		public ZString MasterBookingNumber
		{
			get => masterBookingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(MasterBookingNumberInfo, ref masterBookingNumber, value))
				{
				}
			}
		}

		ZString masterBookingNumber;

		public ZPropertyInfo MasterBookingNumberInfo => GetZPropertyInfo(nameof(MasterBookingNumber));

		#endregion

		#region QuotationNumber

		[MaxLength(NotificationTypes.Warning, 30)]
		public ZString QuotationNumber
		{
			get => quotationNumber;
			set
			{
				if (SetNonPersistentPropertyValue(QuotationNumberInfo, ref quotationNumber, value))
				{
				}
			}
		}

		ZString quotationNumber;

		public ZPropertyInfo QuotationNumberInfo => GetZPropertyInfo(nameof(QuotationNumber));

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion

		#region Shipper

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		Address shipper;

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region Consignee

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		Address consignee;

		#endregion

		#region NotifyParty

		public Address NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}

		Address notifyParty;

		#endregion

		#region NotifyParty2

		public Address NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}

		Address notifyParty2;

		#endregion

		#region Forwarder

		public Address Forwarder
		{
			get => forwarder;
			set => forwarder = SetChild(forwarder, value);
		}

		Address forwarder;

		#endregion

		#region CurrentUser

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		Address currentUser;

		#endregion

		#region Transports

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}

		Transports transports;

		#endregion

		#region FreightPayableAt

		public Unloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}

		Unloco freightPayableAt;

		#endregion

		#region IsFreightPrepaid

		public ZBool IsFreightPrepaid
		{
			get => isFreightPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreightPrepaidInfo, ref isFreightPrepaid, value))
				{
					Validate(IsFreightPrepaidInfo);
					Validate(IsFreightCollectInfo);
				}
			}
		}

		ZBool isFreightPrepaid;

		public ZPropertyInfo IsFreightPrepaidInfo => GetZPropertyInfo(nameof(IsFreightPrepaid));

		#endregion

		#region IsFreightCollect

		public ZBool IsFreightCollect
		{
			get => isFreightCollect;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreightCollectInfo, ref isFreightCollect, value))
				{
					Validate(IsFreightPrepaidInfo);
					Validate(IsFreightCollectInfo);
				}
			}
		}

		ZBool isFreightCollect;

		public ZPropertyInfo IsFreightCollectInfo => GetZPropertyInfo(nameof(IsFreightCollect));

		#endregion

		#region OtherCharges

		public OtherCharges OtherCharges
		{
			get => otherCharges;
			set => otherCharges = SetChild(otherCharges, value);
		}

		OtherCharges otherCharges;

		#endregion

		#region Bookings

		public IReadOnlyCollection<Booking> Bookings
		{
			get => bookings;
			set => bookings = SetChildCollection(bookings, value);
		}
		IReadOnlyCollection<Booking> bookings;

		#endregion

		public IReadOnlyCollection<Container> Containers { get; set; }
	}
}
