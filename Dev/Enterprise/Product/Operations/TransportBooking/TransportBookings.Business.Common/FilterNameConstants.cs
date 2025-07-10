namespace Enterprise.TransportBookings.Shared
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant.")]
	public static class FilterNameConstants
	{
		// CO2
		public const string CO2e = "CO2e";

		// Consolidation or Booking
		public const string TransportCompany = "Transport Company";
		public const string TransportAdditionalReference = "Additional Reference #";

		// Consolidation
		public const string ConsolidationID = "Consolidation ID";
		public const string MultiBookingID = "Multi Booking ID";
		public const string ConsolidatedBookingID = "Consolidated Booking ID";
		public const string ConsolidationJobType = "ConsolidationJobType";
		public const string ConsolidationStatus = "Consolidation Status";
		public const string IsOverridden = "Is Overridden";
		public const string BookingConsolidationTemplate = "Booking Template";
		public const string BookingConsolidationJobDirection = "Booking Job Direction";

		// Booking
		public const string BookingConsolidated = "Consolidation Status";
		public const string BookingDirection = "Booking Direction";
		public const string BookingID = "Booking ID";
		public const string BookingIsHazardous = "Hazardous";
		public const string BookingRequiresRefrigeration = "Refrigeration";
		public const string BookingShowStandalone = "Show Standalone Bookings";
		public const string BookingStatus = "Booking Status";
		public const string BookingTransportReference = "Booking Transport Reference";
		public const string BookingTransportMode = "Booking Transport Mode";
		public const string BookingShowQuotes = "Show Quotes";
		public const string CarrierAccount = "Carrier Account";
		public const string CarrierBookingAgent = "Carrier Booking Agent";
		public const string BookingRequestedDate = "Booking Requested Date";
		public const string BookingIsMaster = "Is Master";
		public const string BookingIsSub = "Is Sub";

		// Instruction
		public const string InstructionType = "Instruction Type";
		public const string InstructionDropMode = "Instruction Drop Mode";
		public const string InstructionOrgType = "Instruction Organisation Type";
		public const string InstructionCompanyName = "Instruction Company Name";
		public const string InstructionCompanyCode = "InstructionCompanyCode";
		public const string InstructionCompanyRelatedPort = "InstructionCompanyRelatedPort";
		public const string InstructionStatus = "Instruction Status";
		public const string InstructionNotes = "Instruction Notes";

		// Instruction Addresses
		public const string InstructionAddressCity = "Instruction Address City";
		public const string InstructionAddressState = "Instruction Address State";
		public const string InstructionAddressPostCode = "Instruction Address Postcode";

		// Confirmation
		public const string ConfirmationType = "Confirmation Type";
		public const string ConfirmationReferenceNumber = "Confirmation Reference Number";
		public const string ConfirmationReceivedBy = "Confirmation Received By";
		public const string ConfirmationSlotReference = "Confirmation Slot Reference";
		public const string DeliveryEstimated = "Delivery Estimated";
		public const string DeliveryRequiredFrom = "Delivery Required From";
		public const string DeliveryRequiredTo = "Delivery Required To";
		public const string DeliveryActual = "Delivery Actual";
		public const string DeliverySlotDate = "Delivery Slot Date";
		public const string PickupEstimated = "Pickup Estimated";
		public const string PickupRequiredFrom = "Pickup Required From";
		public const string PickupRequiredTo = "Pickup Required To";
		public const string PickupActual = "Pickup Actual";
		public const string PickupSlotDate = "Pickup Slot Date";

		// Packages
		public const string PackageID = "Package ID/Container #";
		public const string PackageID_Assigned = "Package ID/Container # (Assigned)";
		public const string PackageType = "Package Type";
		public const string ContainerType = "Container Type";

		// Audit
		public const string CreateTime = "Create Time";
		public const string CreateUser = "Create User";

		// Parent Job (eg. Shipment)
		public const string ParentJobNumber = "Parent Job #";
		public const string ParentJobType = "Parent Job Type";
		public const string OriginDestination = "Origin / Destination";
		public const string ConsignorConsignee = "Consignor / Consignee";
		public const string VoyageVessel = "Vessel and Flight/Voyage #";
		public const string MasterBillNumber = "Master Bill #";
		public const string HouseBillNumber = "House Bill #";
		public const string ETD = "Load ETD";
		public const string ATD = "Load ATD";
		public const string ETA = "Discharge ETA";
		public const string ATA = "Discharge ATA";
		public const string FclReceival = "FCL Receival Commences";
		public const string FclCutOff = "FCL Cut Off";
		public const string FclDgReceival = "FCL DG Receival Commences";
		public const string FclDgCutOff = "FCL DG Cut Off";
		public const string FclAvailability = "FCL Availability";
		public const string FclStorage = "FCL Storage";
		public const string LclReceival = "LCL Receival Commences";
		public const string LclCutOff = "LCL Cut Off";
		public const string LclAvailability = "LCL Availability";
		public const string LclStorage = "LCL Storage";
		public const string LoadDischarge = "Load / Discharge";
		public const string Carrier = "Carrier";
		public const string BookedBy = "Booked By";
		public const string SendingReceivingAgent = "Sending / Receiving Agent";
		public const string ControllingCustomer = "Controlling Customer";
		public const string CustomsBroker = "Customs Broker";
		public const string LocalClient = "Local Client";
		public const string CartageCoordinator = "Cartage Coordinator";
		public const string CTOReference = "CTO Reference";
		public const string CYDReference = "CYD Reference";
		public const string ConsolNumber = "Consol #";
		public const string ShipmentNumber = "Shipment #";
	}
}
