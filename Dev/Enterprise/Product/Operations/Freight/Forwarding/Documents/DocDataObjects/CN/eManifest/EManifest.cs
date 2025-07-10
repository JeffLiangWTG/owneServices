using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class EManifest : DocDataObject, IEManifest
	{
		public EManifest(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region IDataSourceProvider memebers

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

		#endregion

		#region Numbers

		public IReadOnlyCollection<IReferenceNumber> Numbers
		{
			get => numbers;
			set => numbers = SetChildCollection(numbers, value);
		}

		IReadOnlyCollection<IReferenceNumber> numbers;

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
				}
			}
		}

		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

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
					Validate(RequestedDateOfIssueInfo);
				}
			}
		}

		ZDateTime requestedDateOfIssue;

		public ZPropertyInfo RequestedDateOfIssueInfo => GetZPropertyInfo(nameof(RequestedDateOfIssue));

		#endregion

		#region IsDoorPickup

		public ZBool IsDoorPickup
		{
			get => isDoorPickup;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorPickupInfo, ref isDoorPickup, value))
				{
					if (PickupFrom is IAdHocValidationSupporter pickupFromValidator)
					{
						pickupFromValidator.ValidateAllIncludingChildren();
					}

					if (DeliverTo is IAdHocValidationSupporter deliverToValidator)
					{
						deliverToValidator.ValidateAllIncludingChildren();
					}
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
					if (PickupFrom is IAdHocValidationSupporter pickupFromValidator)
					{
						pickupFromValidator.ValidateAllIncludingChildren();
					}

					if (DeliverTo is IAdHocValidationSupporter deliverToValidator)
					{
						deliverToValidator.ValidateAllIncludingChildren();
					}
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

		#region SendAllBookings

		[IgnoreChanges]
		public ZBool SendAllBookings
		{
			get => sendAllBookings;
			set
			{
				if (isSettingSendAll)
				{
					return;
				}

				isSettingSendAll = true;

				if (SetNonPersistentPropertyValue(SendAllBookingsInfo, ref sendAllBookings, value)
					&& sendAllBookings
					&& bookings != null)
				{
					Validate(SendAllBookingsInfo);

					foreach (var booking in bookings)
					{
						booking.Send = true;
					}
				}

				isSettingSendAll = false;
			}
		}

		bool isSettingSendAll;

		ZBool sendAllBookings;

		public ZPropertyInfo SendAllBookingsInfo => GetZPropertyInfo(nameof(SendAllBookings));

		#endregion

		#region IsDirect

		public ZBool IsDirect
		{
			get => isDirect;
			set
			{
				if (SetNonPersistentPropertyValue(IsDirectInfo, ref isDirect, value))
				{
				}
			}
		}

		ZBool isDirect;

		public ZPropertyInfo IsDirectInfo => GetZPropertyInfo(nameof(IsDirect));

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

		#region IsChargesFreighted

		public ZBool IsChargesFreighted
		{
			get => isChargesFreighted;
			set
			{
				if (SetNonPersistentPropertyValue(IsChargesFreightedInfo, ref isChargesFreighted, value))
				{
				}
			}
		}

		ZBool isChargesFreighted;

		public ZPropertyInfo IsChargesFreightedInfo => GetZPropertyInfo(nameof(IsChargesFreighted));

		#endregion

		#region ReleaseType

		public ICodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(releaseType, value);
		}

		ICodeDescription releaseType;

		#endregion

		public ICodeDescription ContainerMode { get; set; }

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		public IUnloco PortOfLoad { get; set; }

		#region PortOfDischarge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		IUnloco portOfDischarge;

		#endregion

		#region PlaceOfIssue

		public IUnloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}

		IUnloco placeOfIssue;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		public IUnloco PlaceOfReceipt { get; set; }
		public IUnloco PlaceOfDelivery { get; set; }

		#region FreightPayableAt

		public IUnloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}

		IUnloco freightPayableAt;

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region Transports

		public ITransports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}

		ITransports transports;

		#endregion

		public IOtherCharges OtherCharges { get; set; }

		#region SendingAgent

		public IAddress SendingAgent
		{
			get => sendingAgent;
			set => sendingAgent = SetChild(sendingAgent, value);
		}

		IAddress sendingAgent;

		#endregion

		#region Carrier

		public IAddress Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		IAddress carrier;

		#endregion

		#region ReceivingAgent

		public IAddress ReceivingAgent
		{
			get => receivingAgent;
			set => receivingAgent = SetChild(receivingAgent, value);
		}

		IAddress receivingAgent;

		#endregion

		#region CarrierHandlingAgent

		public IAddress CarrierHandlingAgent
		{
			get => carrierHandlingAgent;
			set => carrierHandlingAgent = SetChild(carrierHandlingAgent, value);
		}

		IAddress carrierHandlingAgent;

		#endregion

		#region CarrierBookingAgent

		public IAddress CarrierBookingAgent
		{
			get => carrierBookingAgent;
			set => carrierBookingAgent = SetChild(carrierBookingAgent, value);
		}

		IAddress carrierBookingAgent;

		#endregion

		#region NotifyParty

		public IAddress NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}

		IAddress notifyParty;

		#endregion

		#region NotifyParty2

		public IAddress NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}

		IAddress notifyParty2;

		#endregion

		#region Forwarder

		public IAddress Forwarder
		{
			get => forwarder;
			set => forwarder = SetChild(forwarder, value);
		}

		IAddress forwarder;

		#endregion

		public IAddress PickupFrom { get; set; }
		public IAddress DeliverTo { get; set; }
		public IAddress CurrentUser { get; set; }

		public IReadOnlyCollection<IContainer> Containers { get; set; }
		IReadOnlyCollection<Booking> IEManifest.Bookings => Bookings;

		public IReadOnlyCollection<Booking> Bookings
		{
			get => bookings;
			set
			{
				if (bookings != null)
				{
					foreach (var booking in bookings)
					{
						booking.SendInfo.ValueChanged -= RefreshSendAll;
					}
				}

				if (value != null)
				{
					foreach (var booking in value)
					{
						booking.SendInfo.ValueChanged += RefreshSendAll;
					}
				}

				void RefreshSendAll(object sender, EventArgs args)
				{
					SendAllBookings = bookings
						.Where(b => b.MessageStatus.AllowSendOriginal || b.MessageStatus.AllowSendAmendment)
						.All(b => b.Send);
				}

				SetChildCollection(bookings, value);
				bookings = value;
			}
		}

		#region UseBkgRefAsMasterSO

		public ZBool UseBkgRefAsMasterSO
		{
			get => useBkgRefAsMasterSO;
			set
			{
				if (SetNonPersistentPropertyValue(UseBkgRefAsMasterSOInfo, ref useBkgRefAsMasterSO, value))
				{
					if (value && UseMasterBillAsMasterSO)
					{
						UseMasterBillAsMasterSO = !value;
					}

					Validate(UseMasterBillAsMasterSOInfo);
					Validate(UseBkgRefAsMasterSOInfo);
				}
			}
		}

		ZBool useBkgRefAsMasterSO;

		public ZPropertyInfo UseBkgRefAsMasterSOInfo => GetZPropertyInfo(nameof(UseBkgRefAsMasterSO));

		#endregion

		#region UseMasterBillAsMasterSO

		public ZBool UseMasterBillAsMasterSO
		{
			get => useMasterBillAsMasterSO;
			set
			{
				if (SetNonPersistentPropertyValue(UseMasterBillAsMasterSOInfo, ref useMasterBillAsMasterSO, value))
				{
					if (value && UseBkgRefAsMasterSO)
					{
						UseBkgRefAsMasterSO = !value;
					}

					Validate(UseMasterBillAsMasterSOInfo);
					Validate(UseBkgRefAsMasterSOInfo);
				}
			}
		}

		ZBool useMasterBillAsMasterSO;

		public ZPropertyInfo UseMasterBillAsMasterSOInfo => GetZPropertyInfo(nameof(UseMasterBillAsMasterSO));

		#endregion

		#region SendingAgentTaxInfo

		public TaxInfo SendingAgentTaxInfo
		{
			get => sendingAgentTaxInfo;
			set => sendingAgentTaxInfo = SetChild(sendingAgentTaxInfo, value);
		}

		TaxInfo sendingAgentTaxInfo;

		#endregion

		#region ReceivingAgentTaxInfo

		public TaxInfo ReceivingAgentTaxInfo
		{
			get => receivingAgentTaxInfo;
			set => receivingAgentTaxInfo = SetChild(receivingAgentTaxInfo, value);
		}

		TaxInfo receivingAgentTaxInfo;

		#endregion

		#region NotifyPartyTaxInfo

		public TaxInfo NotifyPartyTaxInfo
		{
			get => notifyPartyTaxInfo;
			set => notifyPartyTaxInfo = SetChild(notifyPartyTaxInfo, value);
		}

		TaxInfo notifyPartyTaxInfo;

		#endregion

		#region NotifyParty2TaxInfo

		public TaxInfo NotifyParty2TaxInfo
		{
			get => notifyParty2TaxInfo;
			set => notifyParty2TaxInfo = SetChild(notifyParty2TaxInfo, value);
		}

		TaxInfo notifyParty2TaxInfo;

		#endregion

		IReadOnlyCollection<Booking> bookings;

		#region IsRequiredSendAttachment

		public ZBool IsRequiredSendAttachment
		{
			get => isRequiredSendAttachment;
			set
			{
				if (SetNonPersistentPropertyValue(IsRequiredSendAttachmentInfo, ref isRequiredSendAttachment, value))
				{
					Validate(IsRequiredSendAttachmentInfo);
				}
			}
		}
		ZBool isRequiredSendAttachment;

		public ZPropertyInfo IsRequiredSendAttachmentInfo => GetZPropertyInfo(nameof(IsRequiredSendAttachment));

		#endregion
	}
}
