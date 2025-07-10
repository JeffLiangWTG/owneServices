using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IAMS1M
	{
		ZString CarrierCode { get; }
		ZString TransportationIndicator { get; }
		ZString CountryCodeOfImportingConveyance { get; }
		ZString ImportingConveyanceName { get; }
		ZInt NumberOfBillsOfLading { get; }
		ZString ManifestSequenceNumber { get; }
		ZString AMSMIBPaperlessParticipant { get; }
		ZString ManifestTypeCode { get; }
	}

	public interface IAMS2M
	{
		ZString CarrierAssignedBatchNumber { get; }
	}

	public interface IAMS1P
	{
		ZString DistrictPortOfUnlading { get; }
		ZDate OriginalScheduledDateOfArrival { get; }
		ZString Time { get; }
		ZString NumberOfBillsOfLadingForPort { get; }
		ZString FIRMSCode { get; }
	}

	public interface IAMS1A
	{
		ZString CarrierCode { get; }
		ZString CBPDistrictPort { get; }
		ZString ActionCode { get; }
		ZString Quantity { get; }
		ZString AmendmentCode { get; }
		ZString HouseBillNumber { get; }
		ZString CarrierCode1 { get; }
		ZString IssuerCode { get; }
	}

	public interface IAMS1B
	{
		ZString BillOfLading { get; }
		ZString ForeignPortOfLading { get; }
		ZDecimal ManifestQuantity { get; }
		ZString ManifestUnits { get; }
		ZDecimal Weight { get; }
		ZString WeightUnit { get; }
		ZString BillOfLadingStatusIndicator { get; }
		ZString MasterInBondIndicator { get; }
		ZString InBondEntryType { get; }
		ZString InBondPortOfDestination { get; }
	}

	public interface IAMS2B
	{
		ZDecimal Measurement { get; }
		ZString MeasurementUnit { get; }
		ZString PlaceOfReceiptByPrecarrier { get; }
		ZString SpaceCharterBLReference { get; }
		ZString CarrierCode { get; }
		ZString CarrierCode1 { get; }
	}

	public interface IAMS1J
	{
		ZString IssuerCode { get; }
	}

	public interface IAMS4B
	{
		ZString ReferenceNumber { get; }
		ZString ReferenceQualifier { get; }
	}

	public interface IAMS0N
	{
		ZString Name { get; }
		ZString EntityIDCode { get; }
		ZString CodeQualifier { get; }
		ZString IDCode { get; }
	}

	public interface IAMS2N
	{
		ZString EntityPartyAddress { get; }
		ZString EntityPartyAddress1 { get; }
	}

	public interface IAMS3N
	{
		ZString CityName { get; }
		ZString StateProvince { get; }
		ZString LocationIdentifier { get; }
		ZString PostalCode { get; }
		ZString CountryCode { get; }
	}

	public interface IAMS4N
	{
		ZString ContactName { get; }
		ZString CommNumberQualifier { get; }
		ZString CommunicationsNumber { get; }
	}

	public interface IAMS1I
	{
		ZString FDABTAConfirmationIndicator { get; }
		ZString ConventionalInBondNumber { get; }
		ZString InBondCarrierCode { get; }
		ZString ForeignDestination { get; }
		ZInt Value { get; }
		ZString BondedCarrierIDNumber { get; }
		ZString PaperlessInBond { get; }
		ZString ShipmentControlNumberInBond { get; }
	}

	public interface IAMS2I
	{
		ZString TransportationIndicator { get; }
		ZString VesselName { get; }
	}

	public interface IAMS1C
	{
		ZString LoadEmptyStatusCode { get; }
		ZString EquipmentInitial { get; }
		ZString EquipmentNumber { get; }
		ZString SealNumber1 { get; }
		ZString SealNumber2 { get; }
		ZString ContainerEquipmentDescriptionCode { get; }
	}

	public interface IAMS2C
	{
		ZString VIN { get; }
		ZString ForeignPort { get; }
		ZString FactoryCarOrderNumber { get; }
	}

	public interface IAMS0D
	{
		ZString HarmonizedNumber { get; }
		ZInt Value { get; }
		ZDecimal Weight { get; }
		ZString WeightUnit { get; }
	}

	public interface IAMS1D
	{
		ZString C4Number { get; }
		ZString Description { get; }
		ZDecimal PieceCount { get; }
		ZString ManifestUnitCode { get; }
		ZString CountryCode { get; }
	}

	public interface IAMS2D
	{
		ZString MarksAndNumbers { get; }
	}

	public interface IAMS1V
	{
		ZString HazardousMaterialCode { get; }
		ZString HazardousMaterialClass { get; }
		ZString HazardousMaterialCodeQualifier { get; }
		ZString HazardousMaterialDescription { get; }
		ZString HazardousMaterialContact { get; }
		ZString UNHazardousMaterialPage { get; }
	}

	public interface IAMS2V
	{
		ZInt FlashpointTemperature { get; }
		ZString UnitOfMeasureCode { get; }
		ZString NegativeIndicator { get; }
	}

	public interface IAMS3V
	{
		ZString HazardousMaterialDescription { get; }
		ZString HazardousMaterialClassification { get; }
	}
}
