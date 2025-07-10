using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1M : MessageBlock, IAMS1M
	{
		#region IAMS1M Members

		ZString IAMS1M.CarrierCode
		{
			get { return CarrierCode; }
		}

		ZString IAMS1M.TransportationIndicator
		{
			get { return TransportationIndicator; }
		}

		ZString IAMS1M.CountryCodeOfImportingConveyance
		{
			get { return CountryCodeOfImportingConveyance; }
		}

		ZString IAMS1M.ImportingConveyanceName
		{
			get { return ImportingConveyanceName; }
		}

		ZInt IAMS1M.NumberOfBillsOfLading
		{
			get { return NumberOfBillsOfLading; }
		}

		ZString IAMS1M.ManifestSequenceNumber
		{
			get { return ManifestSequenceNumber; }
		}
		ZString IAMS1M.AMSMIBPaperlessParticipant
		{
			get { return AMSMIBPaperlessParticipant; }
		}
		ZString IAMS1M.ManifestTypeCode
		{
			get { return ManifestTypeCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2M : MessageBlock, IAMS2M
	{
		#region IAMS2M Members

		ZString IAMS2M.CarrierAssignedBatchNumber
		{
			get { return CarrierAssignedBatchNumber; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1P : MessageBlock, IAMS1P
	{
		#region IAMS1P Members

		ZString IAMS1P.DistrictPortOfUnlading
		{
			get { return DistrictPortOfUnlading; }
		}

		ZDate IAMS1P.OriginalScheduledDateOfArrival
		{
			get { return OriginalScheduledDateOfArrival; }
		}

		ZString IAMS1P.Time
		{
			get { return Time; }
		}

		ZString IAMS1P.NumberOfBillsOfLadingForPort
		{
			get { return NumberOfBillsOfLadingForPort; }
		}

		ZString IAMS1P.FIRMSCode
		{
			get { return FIRMSCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1J : MessageBlock, IAMS1J
	{
		#region IAMS1J Members

		ZString IAMS1J.IssuerCode
		{
			get { return IssuerCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1B : MessageBlock, IAMS1B
	{
		#region IAMS1B Members

		ZString IAMS1B.BillOfLading
		{
			get { return BillOfLading; }
		}

		ZString IAMS1B.ForeignPortOfLading
		{
			get { return ForeignPortOfLading; }
		}

		ZDecimal IAMS1B.ManifestQuantity
		{
			get { return ManifestQuantity; }
		}

		ZString IAMS1B.ManifestUnits
		{
			get { return ManifestUnits; }
		}

		ZDecimal IAMS1B.Weight
		{
			get { return Weight; }
		}

		ZString IAMS1B.WeightUnit
		{
			get { return WeightUnit; }
		}

		ZString IAMS1B.BillOfLadingStatusIndicator
		{
			get { return BillOfLadingStatusIndicator; }
		}

		ZString IAMS1B.MasterInBondIndicator
		{
			get { return MasterInBondIndicator; }
		}

		ZString IAMS1B.InBondEntryType
		{
			get { return InBondEntryType; }
		}

		ZString IAMS1B.InBondPortOfDestination
		{
			get { return InBondPortOfDestination; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2B : MessageBlock, IAMS2B
	{
		#region IAMS2B Members

		ZDecimal IAMS2B.Measurement
		{
			get { return Measurement; }
		}

		ZString IAMS2B.MeasurementUnit
		{
			get { return MeasurementUnit; }
		}

		ZString IAMS2B.PlaceOfReceiptByPrecarrier
		{
			get { return PlaceOfReceiptByPrecarrier; }
		}

		ZString IAMS2B.SpaceCharterBLReference
		{
			get { return SpaceCharterBLReference; }
		}

		ZString IAMS2B.CarrierCode
		{
			get { return CarrierCode; }
		}

		ZString IAMS2B.CarrierCode1
		{
			get { return CarrierCode1; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS0N : MessageBlock, IAMS0N
	{
		#region IAMS0N members

		ZString IAMS0N.Name { get { return Name; } }
		ZString IAMS0N.EntityIDCode { get { return EntityIDCode; } }
		ZString IAMS0N.CodeQualifier { get { return CodeQualifier; } }
		ZString IAMS0N.IDCode { get { return IDCode; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1C : MessageBlock, IAMS1C
	{
		#region IAMS1C members

		ZString IAMS1C.LoadEmptyStatusCode { get { return LoadEmptyStatusCode; } }
		ZString IAMS1C.EquipmentInitial { get { return EquipmentInitial; } }
		ZString IAMS1C.EquipmentNumber { get { return EquipmentNumber; } }
		ZString IAMS1C.SealNumber1 { get { return SealNumber1; } }
		ZString IAMS1C.SealNumber2 { get { return SealNumber2; } }
		ZString IAMS1C.ContainerEquipmentDescriptionCode { get { return ContainerEquipmentDescriptionCode; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1D : MessageBlock, IAMS1D
	{
		#region IAMS1D members

		ZString IAMS1D.C4Number { get { return C4Number; } }
		ZString IAMS1D.Description { get { return Description; } }
		ZDecimal IAMS1D.PieceCount { get { return PieceCount; } }
		ZString IAMS1D.ManifestUnitCode { get { return ManifestUnitCode; } }
		ZString IAMS1D.CountryCode { get { return CountryCode; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2D : MessageBlock, IAMS2D
	{
		#region IAMS2D members

		ZString IAMS2D.MarksAndNumbers { get { return MarksAndNumbers; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMSNS30 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1A : MessageBlock, IAMS1A
	{
		#region IAMS1A Members

		ZString IAMS1A.CarrierCode
		{
			get { return CarrierCode; }
		}

		ZString IAMS1A.CBPDistrictPort
		{
			get { return CBPDistrictPort; }
		}

		ZString IAMS1A.ActionCode
		{
			get { return ActionCode; }
		}

		ZString IAMS1A.Quantity
		{
			get { return Quantity; }
		}

		ZString IAMS1A.AmendmentCode
		{
			get { return AmendmentCode; }
		}

		ZString IAMS1A.HouseBillNumber
		{
			get { return HouseBillNumber; }
		}

		ZString IAMS1A.CarrierCode1
		{
			get { return CarrierCode1; }
		}
		ZString IAMS1A.IssuerCode
		{
			get { return IssuerCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1I : MessageBlock, IAMS1I
	{
		#region IAMS1I members

		ZString IAMS1I.FDABTAConfirmationIndicator { get { return FDABTAConfirmationIndicator; } }
		ZString IAMS1I.ConventionalInBondNumber { get { return ConventionalInBondNumber; } }
		ZString IAMS1I.InBondCarrierCode { get { return InBondCarrierCode; } }
		ZString IAMS1I.ForeignDestination { get { return ForeignDestination; } }
		ZInt IAMS1I.Value { get { return Value; } }
		ZString IAMS1I.BondedCarrierIDNumber { get { return BondedCarrierIDNumber; } }
		ZString IAMS1I.PaperlessInBond { get { return PaperlessInBond; } }
		ZString IAMS1I.ShipmentControlNumberInBond { get { return ShipmentControlNumberInBond; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2I : MessageBlock, IAMS2I
	{
		#region IAMS2I members

		ZString IAMS2I.TransportationIndicator { get { return TransportationIndicator; } }
		ZString IAMS2I.VesselName { get { return VesselName; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2C : MessageBlock, IAMS2C
	{
		#region IAMS2C members

		ZString IAMS2C.VIN { get { return VIN; } }
		ZString IAMS2C.ForeignPort { get { return ForeignPort; } }
		ZString IAMS2C.FactoryCarOrderNumber { get { return FactoryCarOrderNumber; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS0D : MessageBlock, IAMS0D
	{
		#region IAMS0D members

		ZString IAMS0D.HarmonizedNumber { get { return HarmonizedNumber; } }
		ZInt IAMS0D.Value { get { return Value; } }
		ZDecimal IAMS0D.Weight { get { return Weight; } }
		ZString IAMS0D.WeightUnit { get { return WeightUnit; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS1V : MessageBlock, IAMS1V
	{
		#region IAMS1V members

		ZString IAMS1V.HazardousMaterialCode { get { return HazardousMaterialCode; } }
		ZString IAMS1V.HazardousMaterialClass { get { return HazardousMaterialClass; } }
		ZString IAMS1V.HazardousMaterialCodeQualifier { get { return HazardousMaterialCodeQualifier; } }
		ZString IAMS1V.HazardousMaterialDescription { get { return HazardousMaterialDescription; } }
		ZString IAMS1V.HazardousMaterialContact { get { return HazardousMaterialContact; } }
		ZString IAMS1V.UNHazardousMaterialPage { get { return UNHazardousMaterialPage; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2V : MessageBlock, IAMS2V
	{
		#region IAMS2V members

		ZInt IAMS2V.FlashpointTemperature { get { return FlashpointTemperature; } }
		ZString IAMS2V.UnitOfMeasureCode { get { return UnitOfMeasureCode; } }
		ZString IAMS2V.NegativeIndicator { get { return NegativeIndicator; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS3V : MessageBlock, IAMS3V
	{
		#region IAMS3V members

		ZString IAMS3V.HazardousMaterialDescription { get { return HazardousMaterialDescription; } }
		ZString IAMS3V.HazardousMaterialClassification { get { return HazardousMaterialClassification; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMSNS05 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMSNS10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMSNS40 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMSNS50 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMSNS60 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS4B : MessageBlock, IAMS4B
	{
		#region IAMS4B members

		ZString IAMS4B.ReferenceNumber { get { return ReferenceNumber; } }
		ZString IAMS4B.ReferenceQualifier { get { return ReferenceQualifier; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS2N : MessageBlock, IAMS2N
	{
		#region IAMS2N members

		ZString IAMS2N.EntityPartyAddress { get { return EntityPartyAddress; } }
		ZString IAMS2N.EntityPartyAddress1 { get { return EntityPartyAddress1; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS3N : MessageBlock, IAMS3N
	{
		#region IAMS3N members

		ZString IAMS3N.CityName { get { return CityName; } }
		ZString IAMS3N.StateProvince { get { return StateProvince; } }
		ZString IAMS3N.LocationIdentifier { get { return LocationIdentifier; } }
		ZString IAMS3N.PostalCode { get { return PostalCode; } }
		ZString IAMS3N.CountryCode { get { return CountryCode; } }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	public partial class AMS4N : MessageBlock, IAMS4N
	{
		#region IAMS4N members

		ZString IAMS4N.ContactName { get { return ContactName; } }
		ZString IAMS4N.CommNumberQualifier { get { return CommNumberQualifier; } }
		ZString IAMS4N.CommunicationsNumber { get { return CommunicationsNumber; } }

		#endregion
	}
}