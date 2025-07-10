using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal abstract class ForwardersCargoReceipt : DocDataObject
	{
		#region Address

		public Address Buyer
		{
			get => buyer;
			set => buyer = SetChild(buyer, value);
		}
		Address buyer;

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		Address shipper;

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}
		Address carrier;

		public Address NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}
		Address notifyParty;

		public Address NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}
		Address notifyParty2;

		public Address NotifyParty3
		{
			get => notifyParty3;
			set => notifyParty3 = SetChild(notifyParty3, value);
		}
		Address notifyParty3;

		public Address ControllingCustomer
		{
			get => controllingCustomer;
			set => controllingCustomer = SetChild(controllingCustomer, value);
		}
		Address controllingCustomer;

		public Address Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		Address destination;

		#endregion

		#region Port

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

		public Unloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		Unloco origin;

		#endregion

		#region IncotermCodeDescription

		public ZString IncotermDescription
		{
			get => incotermDescription;
			set
			{
				if (SetNonPersistentPropertyValue(IncotermDescriptionInfo, ref incotermDescription, value))
				{
					Validate(IncotermDescriptionInfo);
				}
			}
		}

		ZString incotermDescription;

		public ZPropertyInfo IncotermDescriptionInfo => GetZPropertyInfo(nameof(IncotermDescription));

		public ICodeDescription IncotermCodeDescription
		{
			get => incotermCodeDescription;
			set => incotermCodeDescription = SetChild(incotermCodeDescription, value);
		}

		ICodeDescription incotermCodeDescription;

		#endregion

		#region Transport Mode

		public ZString TransportMode
		{
			get => transportMode;
			set
			{
				if (SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value))
				{
					Validate(TransportModeInfo);
				}
			}
		}

		ZString transportMode;

		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(nameof(TransportMode));

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

		#region Vessel Name

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

		#region Transports

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		Transports transports;

		#endregion

		#region Wooden Package Declaration

		public ZBool WoodenPackageDeclaration
		{
			get => woodenPackageDeclaration;
			set
			{
				if (SetNonPersistentPropertyValue(WoodenPackageDeclarationInfo, ref woodenPackageDeclaration, value))
				{
					Validate(WoodenPackageDeclarationInfo);
				}
			}
		}
		ZBool woodenPackageDeclaration;

		public ZPropertyInfo WoodenPackageDeclarationInfo => GetZPropertyInfo(nameof(WoodenPackageDeclaration));

		#endregion

		#region Cargo Receipt Date

		public ZDateTime CargoReceiptDate
		{
			get => cargoReceiptDate;
			set
			{
				if (SetNonPersistentPropertyValue(CargoReceiptDateInfo, ref cargoReceiptDate, value))
				{
					Validate(CargoReceiptDateInfo);
				}
			}
		}
		ZDateTime cargoReceiptDate;

		public ZPropertyInfo CargoReceiptDateInfo => GetZPropertyInfo(nameof(CargoReceiptDate));

		#endregion

		#region On Board Date

		public ZDateTime OnBoardDate
		{
			get => onBoardDate;
			set
			{
				if (SetNonPersistentPropertyValue(OnBoardDateInfo, ref onBoardDate, value))
				{
					Validate(OnBoardDateInfo);
				}
			}
		}
		ZDateTime onBoardDate;

		public ZPropertyInfo OnBoardDateInfo => GetZPropertyInfo(nameof(OnBoardDate));

		#endregion

		#region FCR Instructions

		public ZString FCRInstructions
		{
			get => fcrInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(FCRInstructionsInfo, ref fcrInstructions, value))
				{
					Validate(FCRInstructionsInfo);
				}
			}
		}
		ZString fcrInstructions;

		public ZPropertyInfo FCRInstructionsInfo => GetZPropertyInfo(nameof(FCRInstructions));

		#endregion

		#region SupplierBookingNumber

		public ZString SupplierBookingNumber
		{
			get => supplierBookingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(SupplierBookingNumberInfo, ref supplierBookingNumber, value))
				{
					Validate(SupplierBookingNumberInfo);
				}
			}
		}
		ZString supplierBookingNumber;

		public ZPropertyInfo SupplierBookingNumberInfo => GetZPropertyInfo(nameof(SupplierBookingNumber));

		#endregion

		#region ContainerSummary

		public ZString ContainerSummary
		{
			get => containerSummary;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerSummaryInfo, ref containerSummary, value))
				{
					Validate(ContainerSummaryInfo);
				}
			}
		}
		ZString containerSummary;

		public ZPropertyInfo ContainerSummaryInfo => GetZPropertyInfo(nameof(ContainerSummary));

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion

		#region Delivered

		public ZBool Delivered
		{
			get => delivered;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveredInfo, ref delivered, value))
				{
					Validate(DeliveredInfo);
				}
			}
		}
		ZBool delivered;
		public ZPropertyInfo DeliveredInfo => GetZPropertyInfo(nameof(Delivered));

		#endregion
	}
}
