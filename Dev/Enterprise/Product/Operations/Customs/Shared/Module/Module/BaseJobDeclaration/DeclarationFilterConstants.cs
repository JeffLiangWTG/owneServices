namespace Enterprise.Customs.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constants")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Class is inherited and cannot be static")]
	public class DeclarationFilterConstants
	{
		public static class EntryStatus
		{
			public const string NotSentForFilter = Enterprise.Freight.Forwarding.Module.JobShipmentFilterBusinessObject.FilterStatus.NotSentCustomsStatusForFilter;
		}

		public const string FlightVoyageVessel = "Vessel and Flight/Voyage #";
		public const string CancelledJobs = "Cancelled (Inactive) Jobs";
		public const string Country = "Country/Region";
		public const string Show = "Show";
		public const string ShipmentType = "Shipment Type";
		public const string ShipmentSubType = "Shipment Sub-Type";
		public const string TransportMode = "Transport Mode";
		public const string ServiceLevel = "Service Level";
		public const string ShowJobsFCA_FIFTApportionment = "Jobs for FCA and FIFT apportionment";
		public const string WHSStatus = "WHS Status";
		public const string RelatedContainers = "Related Containers";
		public const string SubmitType = "Submit Type";
		public const string Product = "Product";
		public const string ImportClassification = "Import Classification";
		public const string EntryStatusText = "Entry Status";
		public const string PhaseStatusText = "Phase Status";
		public const string MessageStatusText = "Message Status";
		public const string ServiceType = "Service Type";
		public const string RelatedTransportBookings = "Related Transport Bookings";
		public const string RelatedShipmentSecurity = "Related Shipment Security";
		public const string RelatedShipment = "Related Shipment";

		#region NumberFilterTypes

		public class NumberFilterTypes
		{
			public const string Common = "Common Numbers and References";
			public const string AgentsReference = "Agents Reference";
			public const string ContainerModeCustoms = "Container Mode (Customs)";
			public const string ContainerNumber = "Container #";
			public const string DeclarationReference = "Job #";
			public const string EntryNumber = "Entry #";
			public const string HouseBill = "House Bill";
			public const string InvoiceAmount = "Invoice Amount";
			public const string InvoiceNumber = "Invoice #";
			public const string InvoiceLineProductCode = "Invoice Line Product Code";
			public const string MasterBill = "Master Bill";
			public const string OrderNumber = "Order #";
			public const string OrderNumberOwnersRef = "Order # / Owner's Reference";
			public const string PartAttribute1 = "Part Attribute 1";
			public const string PartAttribute2 = "Part Attribute 2";
			public const string PartAttribute3 = "Part Attribute 3";
			public const string PaymentAmount = "Payment Amount";
			public const string PaymentNumber = "Payment #";
			public const string SerialNumber = "Reference #";
			public const string TariffInvLine = "Tariff - Invoice Line";
			public const string DescriptionInvLine = "Description - Invoice Line";
			public const string TariffNumber = "Tariff Number";
			public const string LineNumber = "Line Number";
			public const string AdditionalReferenceNumber = "Additional Reference #";
			public const string ManifestNumber = "Manifest #";
		}

		#endregion

		#region DateFilterTypes

		public class DateFilterTypes
		{
			public const string DateOfExport = "Departure at Loading Port";
			public const string FirstArrival = "First Arrival";
			public const string DateOfArrival = "Arrival at Discharge Port";
			public const string CommercialInvoiceDate = "Commercial Invoice Date";
			public const string Submitted = "Submitted";
			public const string EntryPaymentDate = "Entry Payment Date";
			public const string CommercialInvoicePaymentDate = "Commercial Invoice Payment Date";
			public const string CartageAdvised = "Cartage Advised";
			public const string GoodsDelivered = "Goods Delivered";
			public const string EstimatedTimeOfArrival = "ETA";
			public const string ETDOfLoading = "ETD Of Loading(RT)";
			public const string ETAOfDischarge = "ETA Of Discharge(RT)";
			public const string Created = "Created";
			public const string ServiceDateBooked = "Service Date Booked";
			public const string ServiceCompleted = "Service Completed";
			public const string EntryReleaseDate = "Entry Release Date";
		}

		#endregion

		#region OrgFilterTypes

		public class OrgFilterTypes
		{
			public const string CartageCoordinator = "Cartage Coordinator";
			public const string DeliveryTo = "Delivery To";
			public const string DeliveryToName = "Delivery To Name";
			public const string ImporterSupplier = "Importer/Supplier";
			public const string Importer = "Importer";
			public const string ImporterName = "Importer Name";
			public const string Supplier = "Supplier";
			public const string SupplierName = "Supplier Name";
			public const string ShippingLineForwarder = "Shipping Line/Forwarder";
			public const string ShippingLine = "Shipping Line";
			public const string Forwarder = "Forwarder";
			public const string ClientAssignedStaff = "Client Assigned Staff";
			public const string ControllingCustomer = "Controlling Customer";
			public const string ControllingAgent = "Controlling Agent";
			public const string ExternalBroker = "External Broker";
			public const string PickupFrom = "Pickup From";
			public const string PickupFromName = "Pickup From Name";
			public const string PickupTransportCompany = "Pickup Transport Company"; // Pickup Transport Company = Filter types
			public const string DeliveryTransportCompany = "Delivery Transport Company"; // Delivery Transport Company = Filter types
			public const string Declarant = "Declarant";
		}

		#endregion

		#region PortFilterTypes

		public class PortFilterTypes
		{
			public const string OriginDestination = "Shipment Origin/Destination";
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string LoadDischarge = "Consol Load/Discharge";
			public const string Load = "Load";
			public const string Discharge = "Discharge";
			public const string PortOfFirstArrival = "Port of First Arrival";
		}

		#endregion

		#region AuditFilterTypes

		public static class LastAuditFilterTypes
		{
			public const string LastAuditDate = "Last Audit Date";
			public const string LastAuditReference = "Last Audit Reference";
			public const string LastAuditLogUser = "Last Audit User";
			public const string LastAuditLogUserName = "Last Audit User Name";
		}

		#endregion

		#region ModeFilterTypes

		public static class ModeFilterTypes
		{
			public const string PickupDropMode = "Pickup Drop Mode"; // Pickup Drop Mode = Filter types
			public const string DeliveryDropMode = "Delivery Drop Mode"; // Delivery Drop Mode = Filter types
		}

		#endregion
	}
}
