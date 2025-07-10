using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ShippingOrder : DocDataObject, IShippingOrder
	{
		public ShippingOrder(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region IDataSourceProvider memebers

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

		#region NumberOfOriginals

		public ZInt NumberOfOriginals
		{
			get => numberOfOriginals;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfOriginalsInfo, ref numberOfOriginals, value))
				{
					Validate(NumberOfOriginalsInfo);
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
					Validate(NumberOfCopiesInfo);
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
					Validate(IsDoorPickupInfo);
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
					Validate(IsDoorDeliveryInfo);
				}
			}
		}

		ZBool isDoorDelivery;

		public ZPropertyInfo IsDoorDeliveryInfo => GetZPropertyInfo(nameof(IsDoorDelivery));

		#endregion

		#region ForwardingInstructions

		public ZString ForwardingInstructions
		{
			get => forwardingInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(ForwardingInstructionsInfo, ref forwardingInstructions, value))
				{
					Validate(ForwardingInstructionsInfo);
				}
			}
		}

		ZString forwardingInstructions;

		public ZPropertyInfo ForwardingInstructionsInfo => GetZPropertyInfo(nameof(ForwardingInstructions));

		#endregion

		#region GoodsHandlingInstructions

		public ZString GoodsHandlingInstructions
		{
			get => goodsHandlingInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsHandlingInstructionsInfo, ref goodsHandlingInstructions, value))
				{
					Validate(GoodsHandlingInstructionsInfo);
				}
			}
		}

		ZString goodsHandlingInstructions;

		public ZPropertyInfo GoodsHandlingInstructionsInfo => GetZPropertyInfo(nameof(GoodsHandlingInstructions));

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
					Validate(BillOfLadingNumberInfo);
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
					Validate(ShipperReferenceInfo);
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
					Validate(FreightForwarderReferenceInfo);
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
					Validate(CarrierContractNumberInfo);
				}
			}
		}

		ZString carrierContractNumber;

		public ZPropertyInfo CarrierContractNumberInfo => GetZPropertyInfo(nameof(CarrierContractNumber));

		#endregion

		#region CarrierContractNumberIsQuotationNumber

		public ZBool CarrierContractNumberIsQuotationNumber
		{
			get => carrierContractNumberIsQuotationNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierContractNumberIsQuotationNumberInfo, ref carrierContractNumberIsQuotationNumber, value))
				{
					Validate(CarrierContractNumberIsQuotationNumberInfo);
				}
			}
		}

		ZBool carrierContractNumberIsQuotationNumber;

		public ZPropertyInfo CarrierContractNumberIsQuotationNumberInfo => GetZPropertyInfo(nameof(CarrierContractNumberIsQuotationNumber));

		#endregion

		#region ContractNamedAccount

		public ZString ContractNamedAccount
		{
			get => contractNamedAccount;
			set
			{
				if (SetNonPersistentPropertyValue(ContractNamedAccountInfo, ref contractNamedAccount, value))
				{
					Validate(ContractNamedAccountInfo);
				}
			}
		}

		ZString contractNamedAccount;

		public ZPropertyInfo ContractNamedAccountInfo => GetZPropertyInfo(nameof(ContractNamedAccount));

		#endregion

		#region LetterOfCredit

		public ZString LetterOfCredit
		{
			get => letterOfCredit;
			set
			{
				if (SetNonPersistentPropertyValue(LetterOfCreditInfo, ref letterOfCredit, value))
				{
					Validate(LetterOfCreditInfo);
				}
			}
		}

		ZString letterOfCredit;

		public ZPropertyInfo LetterOfCreditInfo => GetZPropertyInfo(nameof(LetterOfCredit));

		#endregion

		#region IsDischargeInCanadaUSOrUSTerritory

		public ZBool IsDischargeInCanadaUSOrUSTerritory
		{
			get => isDischargeInCanadaUSOrUSTerritory;
			set
			{
				if (SetNonPersistentPropertyValue(IsDischargeInCanadaUSOrUSTerritoryInfo, ref isDischargeInCanadaUSOrUSTerritory, value))
				{
				}
			}
		}

		ZBool isDischargeInCanadaUSOrUSTerritory;

		public ZPropertyInfo IsDischargeInCanadaUSOrUSTerritoryInfo => GetZPropertyInfo(nameof(IsDischargeInCanadaUSOrUSTerritory));

		#endregion

		#region USCanadaManifestSelfFilerID

		public ZString USCanadaManifestSelfFilerID
		{
			get => usCanadaManifestSelfFilerID;
			set
			{
				if (SetNonPersistentPropertyValue(USCanadaManifestSelfFilerIDInfo, ref usCanadaManifestSelfFilerID, value))
				{
					Validate(USCanadaManifestSelfFilerIDInfo);
				}
			}
		}

		ZString usCanadaManifestSelfFilerID;

		public ZPropertyInfo USCanadaManifestSelfFilerIDInfo => GetZPropertyInfo(nameof(USCanadaManifestSelfFilerID));

		#endregion

		#region CarrierBookingPrefix

		public ZString CarrierBookingPrefix
		{
			get => carrierBookingPrefix;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingPrefixInfo, ref carrierBookingPrefix, value))
				{
				}
			}
		}

		ZString carrierBookingPrefix;

		public ZPropertyInfo CarrierBookingPrefixInfo => GetZPropertyInfo(nameof(CarrierBookingPrefix));

		#endregion

		#region IsFreightPrepaid

		public ZBool IsFreightPrepaid
		{
			get => isFreightPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreightPrepaidInfo, ref isFreightPrepaid, value))
				{
					isFreightCollect = false;

					IsFreightPrepaidInfo.RefreshBinding();
					IsFreightCollectInfo.RefreshBinding();
				}

				Validate(IsFreightPrepaidInfo);
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
					isFreightPrepaid = false;

					IsFreightPrepaidInfo.RefreshBinding();
					IsFreightCollectInfo.RefreshBinding();
				}

				Validate(IsFreightCollectInfo);
			}
		}

		ZBool isFreightCollect;

		public ZPropertyInfo IsFreightCollectInfo => GetZPropertyInfo(nameof(IsFreightCollect));

		#endregion

		#region IssueFreightedBillOfLading

		public ZBool IssueFreightedBillOfLading
		{
			get => issueFreightedBillOfLading;
			set
			{
				if (SetNonPersistentPropertyValue(IssueFreightedBillOfLadingInfo, ref issueFreightedBillOfLading, value))
				{
					Validate(IssueFreightedBillOfLadingInfo);
				}
			}
		}

		ZBool issueFreightedBillOfLading;

		public ZPropertyInfo IssueFreightedBillOfLadingInfo => GetZPropertyInfo(nameof(IssueFreightedBillOfLading));

		#endregion

		#region VoyageFlightNumber

		public ZString VoyageFlightNumber
		{
			get => voyageFlightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNumberInfo, ref voyageFlightNumber, value))
				{
					Validate(VoyageFlightNumberInfo);
				}
			}
		}

		ZString voyageFlightNumber;

		public ZPropertyInfo VoyageFlightNumberInfo => GetZPropertyInfo(nameof(VoyageFlightNumber));

		#endregion

		#region NVOCCReference

		public RegistrationNumber NVOCCReference
		{
			get => nvoccReference;
			set => nvoccReference = SetChild(nvoccReference, value);
		}

		RegistrationNumber nvoccReference;

		#endregion

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		public ICodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(releaseType, value);
		}

		ICodeDescription releaseType;

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		public IVessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}

		IVessel vessel;

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

		public IUnloco CarrierBookingOffice
		{
			get => carrierBookingOffice;
			set => carrierBookingOffice = SetChild(carrierBookingOffice, value);
		}

		IUnloco carrierBookingOffice;

		public Unloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}

		Unloco placeOfIssue;

		public Unloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}

		Unloco placeOfReceipt;

		public Unloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}

		Unloco placeOfDelivery;

		public IUnloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}

		IUnloco freightPayableAt;

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#region Transports

		public ITransports Transports
		{
			get => transports;
			set => transports = (ITransports)SetChildCollection(transports, value);
		}

		ITransports transports;

		#endregion

		#region OtherCharges

		public IOtherCharges OtherCharges
		{
			get => otherCharges;
			set => otherCharges = SetChild(otherCharges, value);
		}

		IOtherCharges otherCharges;

		#endregion

		#region Optional Charges

		public IOptionalCharge OptionalChargeDestinationHaulage
		{
			get => optionalChargeDestinationHaulage;
			set => optionalChargeDestinationHaulage = SetChild(optionalChargeDestinationHaulage, value);
		}

		IOptionalCharge optionalChargeDestinationHaulage;

		public IOptionalCharge OptionalChargeDestinationPort
		{
			get => optionalChargeDestinationPort;
			set => optionalChargeDestinationPort = SetChild(optionalChargeDestinationPort, value);
		}

		IOptionalCharge optionalChargeDestinationPort;

		public IOptionalCharge OptionalChargeOriginHaulage
		{
			get => optionalChargeOriginHaulage;
			set => optionalChargeOriginHaulage = SetChild(optionalChargeOriginHaulage, value);
		}

		IOptionalCharge optionalChargeOriginHaulage;

		public IOptionalCharge OptionalChargeOriginPort
		{
			get => optionalChargeOriginPort;
			set => optionalChargeOriginPort = SetChild(optionalChargeOriginPort, value);
		}

		IOptionalCharge optionalChargeOriginPort;

		#endregion

		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		IAddress shipper;

		#endregion

		#region Carrier

		public IAddress Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		IAddress carrier;

		#endregion

		#region RecipientType

		public ZString RecipientType
		{
			get => recipientType;
			set
			{
				if (SetNonPersistentPropertyValue(RecipientTypeInfo, ref recipientType, value))
				{
					Validate(RecipientTypeInfo);
				}
			}
		}
		ZString recipientType;

		public ZPropertyInfo RecipientTypeInfo => GetZPropertyInfo(nameof(RecipientType));

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		IAddress consignee;

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

		#region PickupFrom

		public IAddress PickupFrom
		{
			get => pickupFrom;
			set => pickupFrom = SetChild(pickupFrom, value);
		}

		IAddress pickupFrom;

		#endregion

		#region DeliverTo

		public IAddress DeliverTo
		{
			get => deliverTo;
			set => deliverTo = SetChild(deliverTo, value);
		}

		IAddress deliverTo;

		#endregion

		#region CurrentUser

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		IAddress currentUser;

		#endregion

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set
			{
				containers = SetChildCollection(containers, value);

				foreach (var container in containers.OfType<Container>())
				{
					SetChildCollection(null, container.PackingLines);
				}
			}
		}

		IReadOnlyCollection<Container> containers;

		#endregion

		#region ErrorPlaceholder

		public ZString ErrorPlaceholder
		{
			get => errorPlaceholder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceholderInfo, ref errorPlaceholder, value))
				{
					Validate(ErrorPlaceholderInfo);
				}
			}
		}

		ZString errorPlaceholder;

		public ZPropertyInfo ErrorPlaceholderInfo => GetZPropertyInfo(nameof(ErrorPlaceholder));

		#endregion

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

		#region PackageGrouping

		public CodeDescription PackageGrouping
		{
			get => packageGrouping;
			set => packageGrouping = SetChild(packageGrouping, value);
		}
		CodeDescription packageGrouping;

		#endregion

		#region Shipments

		public IReadOnlyCollection<Shipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}

		IReadOnlyCollection<Shipment> shipments;

		#endregion
	}
}
