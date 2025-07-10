using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact.D08A.Elements;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class CompleteManifestDataConverterTest : TestCaseWithFactory
	{
		public void TestGetIdentificationCodeFromCrewACEIdType()
		{
			AssertEquals(Converter.IdentificationCodes.ACEId, Converter.GetIdentificationCodeFromCrewACEIdType(Factory, CrewACEIdTypes.Codes.Id));
			AssertEquals(Converter.IdentificationCodes.ACEProximityCardId, Converter.GetIdentificationCodeFromCrewACEIdType(Factory, CrewACEIdTypes.Codes.ProximityCardId));
			AssertEquals("BLA", Converter.GetIdentificationCodeFromCrewACEIdType(Factory, "BLA"));
			AssertEquals(CrewACEIdTypes.Codes.ProximityCardId, Converter.GetCrewACEIdTypeFromIdentificationCode(Factory, Converter.IdentificationCodes.ACEProximityCardId));
		}

		public void TestGetIdentificationCodePartyIdType()
		{
			AssertEquals(Converter.IdentificationCodes.ACEId, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.ACE));
			AssertEquals(Converter.IdentificationCodes.FAST, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.FAST));
			AssertEquals(Converter.IdentificationCodes.FilerCode, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.FilerCode));
			AssertEquals(Converter.IdentificationCodes.FIRMS, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.FIRMS));
			AssertEquals(Converter.IdentificationCodes.SSNOrEIN, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.SocialSecurityNumber));
			AssertEquals(Converter.IdentificationCodes.SSNOrEIN, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.EmployerIdentificationNumber));
			AssertEquals(Converter.IdentificationCodes.CustomsAssignedNumber, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.CustomsAssignedNumber));
			AssertEquals(Converter.IdentificationCodes.DUNS, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.DUNS));
			AssertEquals(Converter.IdentificationCodes.SCAC, Converter.GetIdentificationCodePartyIdType(Factory, PartyIdTypes.Codes.SCAC));
			AssertEquals(string.Empty, Converter.GetIdentificationCodePartyIdType(Factory, "BLA"));
			AssertEquals(PartyIdTypes.Codes.CustomsAssignedNumber, Converter.GetPartyIdTypeFromIdentificationCode(Factory, Converter.IdentificationCodes.CustomsAssignedNumber));
		}

		public void TestGetIdentificationCodeFromLocationCodeType()
		{
			AssertEquals(Converter.IdentificationCodes.ScheduleD, Converter.GetIdentificationCodeFromLocationCodeType(Factory, PortCodeTypes.Codes.ScheduleD));
			AssertEquals(Converter.IdentificationCodes.ScheduleK, Converter.GetIdentificationCodeFromLocationCodeType(Factory, PortCodeTypes.Codes.ScheduleK));
			AssertEquals(Converter.IdentificationCodes.InlandScheduleK, Converter.GetIdentificationCodeFromLocationCodeType(Factory, PortCodeTypes.Codes.InlandScheduleK));
			AssertEquals(Converter.IdentificationCodes.IATA, Converter.GetIdentificationCodeFromLocationCodeType(Factory, PortCodeTypes.Codes.IATA));
			AssertEquals(Converter.IdentificationCodes.FreeFormText, Converter.GetIdentificationCodeFromLocationCodeType(Factory, PortCodeTypes.Codes.LocationName));
			AssertEquals("BLA", Converter.GetIdentificationCodeFromLocationCodeType(Factory, "BLA"));
			AssertEquals(PortCodeTypes.Codes.ScheduleD, Converter.GetLocationCodeTypeFromIdentificationCode(Factory, Converter.IdentificationCodes.ScheduleD));
		}

		public void TestGetCrewPartyQualifier()
		{
			AssertEquals(PartyFunctionCodeQualifierList.Passenger, Converter.GetCrewPartyQualifier(Factory, CrewTypes.Codes.Passenger));
			AssertEquals(PartyFunctionCodeQualifierList.CrewMember, Converter.GetCrewPartyQualifier(Factory, CrewTypes.Codes.CrewMember));
			AssertEquals(PartyFunctionCodeQualifierList.ResponsibleParty, Converter.GetCrewPartyQualifier(Factory, CrewTypes.Codes.ResponsibleParty));
			AssertEquals(PartyFunctionCodeQualifierList.GetFromString("BLA"), Converter.GetCrewPartyQualifier(Factory, "BLA"));
			AssertNull(Converter.GetCrewPartyQualifier(Factory, null).ToString());
			AssertEquals(CrewTypes.Codes.CrewMember, Converter.GetCrewTypeFromQualifier(Factory, PartyFunctionCodeQualifierList.CrewMember));
			AssertEquals(string.Empty, Converter.GetCrewTypeFromQualifier(Factory, null));
		}

		public void TestGetPartyQualifier()
		{
			AssertEquals("AEB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.AirportAuthority).ToString());
			AssertEquals("AEE", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PortAuthority).ToString());
			AssertEquals("AM", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.AuthorizedOfficial).ToString());
			AssertEquals("BK", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.FinancialInstitution).ToString());
			AssertEquals("BNO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.BeneficialOwner).ToString());
			AssertEquals("BO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.BrokerOrSalesOffice).ToString());
			AssertEquals("BS", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.BillAndShipTo).ToString());
			AssertEquals("BT", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToBeBilledForOtherThanFreightBillTo).ToString());
			AssertEquals("BU", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ServiceBureau).ToString());
			AssertEquals("BY", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Buyer).ToString());
			AssertEquals("C1", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.InCareOfPartyNo1).ToString());
			AssertEquals("C2", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.InCareOfPartyNo2).ToString());
			AssertEquals("CA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Carrier).ToString());
			AssertEquals("CB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.CustomsBroker).ToString());
			AssertEquals("CEL", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ConsigneeToReceiveLargeParcelsAndFreight).ToString());
			AssertEquals("CGI", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.CarnetIssuer).ToString());
			AssertEquals("CL", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ContainerLocationParty).ToString());
			AssertEquals("CN", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Consignee).ToString());
			AssertEquals("COG", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Dispatcher).ToString());
			AssertEquals("CP", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveCertificateOfCompliance).ToString());
			AssertEquals("CPH", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.CopyReportTo).ToString());
			AssertEquals("CS", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Consolidator).ToString());
			AssertEquals("CU", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ContainerReturnCompany).ToString());
			AssertEquals("CZ", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Consignor).ToString());
			AssertEquals("DB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DistributorBranch).ToString());
			AssertEquals("DCA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DestinationCarrier).ToString());
			AssertEquals("DIS", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Distiller).ToString());
			AssertEquals("DIV", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Division).ToString());
			AssertEquals("DMF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DestinationMailFacility).ToString());
			AssertEquals("DO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DocumentRecipient).ToString());
			AssertEquals("DP", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DeliveryParty).ToString());
			AssertEquals("DS", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Distributor).ToString());
			AssertEquals("DSP", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DownstreamParty).ToString());
			AssertEquals("DTE", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DestinationTerminal).ToString());
			AssertEquals("EHB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Exhibitor).ToString());
			AssertEquals("EX", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Exporter).ToString());
			AssertEquals("FA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OperatorCommunicationChannel).ToString());
			AssertEquals("FSD", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.FinalScheduledDestination).ToString());
			AssertEquals("FW", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.FreightForwarder).ToString());
			AssertEquals("GA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.RoadCarrier).ToString());
			AssertEquals("GG", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Warehouse).ToString());
			AssertEquals("HA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyWhichDeliversConsignmentsToTheTerminal).ToString());
			AssertEquals("HB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyWhichPicksUpConsignmentsFromTheTerminal).ToString());
			AssertEquals("HDQ", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.CorporateOffice).ToString());
			AssertEquals("HR", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ShippingLineService).ToString());
			AssertEquals("HWF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DesignatedHazardousWasteFacility).ToString());
			AssertEquals("HWT", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.TransporterOfHazardousWaste).ToString());
			AssertEquals("IC", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.IntermediateConsignee).ToString());
			AssertEquals("IFF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.InternationalFreightForwarder).ToString());
			AssertEquals("IK", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.IntermediateCarrier).ToString());
			AssertEquals("IM", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Importer).ToString());
			AssertEquals("IPT", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.InterestedParty).ToString());
			AssertEquals("IV", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Invoice).ToString());
			AssertEquals("J3", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.AuthorizedEntity).ToString());
			AssertEquals("J6", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PowerOfAttorney).ToString());
			AssertEquals("LLE", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.LocationOfLoadExchangeExport).ToString());
			AssertEquals("MF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ManufacturerOfGoods).ToString());
			AssertEquals("MI", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PlanningScheduleMaterialReleaseIssuer).ToString());
			AssertEquals("MO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ReleaseDrayman).ToString());
			AssertEquals("MP", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ManufacturingUnit).ToString());
			AssertEquals("NI", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.NotifyParty).ToString());
			AssertEquals("OB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OrderedBy).ToString());
			AssertEquals("OE", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OwnerOfProperty).ToString());
			AssertEquals("OJ", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ThirdParty).ToString());
			AssertEquals("OO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OrderOfTheShipperParty).ToString());
			AssertEquals("OP", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OperatorOfPropertyOrEquipment).ToString());
			AssertEquals("OS", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Shipper).ToString());
			AssertEquals("OV", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OwnerOfMeansOfTransport).ToString());
			AssertEquals("PE", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Payee).ToString());
			AssertEquals("PF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveFreightBill).ToString());
			AssertEquals("PJ", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveCorrespondence).ToString());
			AssertEquals("PM", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceivePaperMemoOfInvoice).ToString());
			AssertEquals("PN", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveShippingNotice).ToString());
			AssertEquals("PR", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Payer).ToString());
			AssertEquals("PRN", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PierName).ToString());
			AssertEquals("PU", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyAtPickupLocation).ToString());
			AssertEquals("PUA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PickupAddress).ToString());
			AssertEquals("RD", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ResaleDealer).ToString());
			AssertEquals("RDI", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DestinationIntermodalRamp).ToString());
			AssertEquals("RO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OriginalIntermodalRamp).ToString());
			AssertEquals("SD", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.SoldToAndShipTo).ToString());
			AssertEquals("SE", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Seller).ToString());
			AssertEquals("SF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ShipFrom).ToString());
			AssertEquals("SM", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveShippingManifest).ToString());
			AssertEquals("SO", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.SoldToIfDifferentThanBillTo).ToString());
			AssertEquals("SP", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyFillingShippersOrder).ToString());
			AssertEquals("SR", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.SellerAgentRepresentative).ToString());
			AssertEquals("ST", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.ShipTo).ToString());
			AssertEquals("SU", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Supplier).ToString());
			AssertEquals("SY", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Surety).ToString());
			AssertEquals("T3", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.TerminalLocation).ToString());
			AssertEquals("T4", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.TransferPoint).ToString());
			AssertEquals("TH", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Attorney).ToString());
			AssertEquals("TR", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.TerminalOperator).ToString());
			AssertEquals("TRM", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Terminal).ToString());
			AssertEquals("TT", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.TransferTo).ToString());
			AssertEquals("UC", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.UltimateConsignee).ToString());
			AssertEquals("UD", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.UltimateCustomer).ToString());
			AssertEquals("UR", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.AffiliatedCompany).ToString());
			AssertEquals("UY", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.Subsidiary).ToString());
			AssertEquals("VA", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.JointOwner).ToString());
			AssertEquals("VB", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.JointVenture).ToString());
			AssertEquals("VY", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.OtherRelatedParty).ToString());
			AssertEquals("WN", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveOrderToSupply).ToString());
			AssertEquals("WQ", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.DoingBusinessAs).ToString());
			AssertEquals("Z1", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.PartyToReceiveStatus).ToString());
			AssertEquals("ZF", Converter.GetPartyQualifier(Factory, PartyTypes.Codes.BreakBulkPoint).ToString());
			AssertEquals(PartyFunctionCodeQualifierList.GetFromString("BLA"), Converter.GetPartyQualifier(Factory, "BLA"));
			AssertEquals(PartyTypes.Codes.Shipper, Converter.GetPartyTypeFromQualifier(Factory, PartyFunctionCodeQualifierList.OriginalShipper));
		}

		public void TestGetEntryTypeCode()
		{
			AssertEquals(Converter.EntryTypeCodes.LowValueEntriesWithFDA, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.LowValue, ZString.Empty, true));
			AssertEquals(Converter.EntryTypeCodes.LowValueEntriesWithoutFDA, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.LowValue, ZString.Empty, false));
			AssertEquals(string.Empty, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.PAPS, ZString.Empty, false));
			AssertEquals(Converter.EntryTypeCodes.GoodsAstray, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.GoodsAstray, ZString.Empty, false));
			AssertEquals(Converter.EntryTypeCodes.UnaccompaniedArticles, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.UnaccompaniedArticles, ZString.Empty, false));
			AssertEquals(Converter.EntryTypeCodes.FreeOfDuty, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.FreeOfDuty, ZString.Empty, false));
			AssertEquals(Converter.EntryTypeCodes.ReturnedGoods, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.ReturnedGoods, ZString.Empty, false));
			AssertEquals(Converter.EntryTypeCodes.ImmediateTransportation, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.Inbond, InbondTypes.Codes.ImmediateTransportation, false));
			AssertEquals(Converter.EntryTypeCodes.ImmediateExportation, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.Inbond, InbondTypes.Codes.ImmediateExportation, false));
			AssertEquals(Converter.EntryTypeCodes.TransportationAndExportation, Converter.GetEntryTypeCode(Factory, ShipmentTypes.Codes.Inbond, InbondTypes.Codes.TransportationAndExportation, false));
			AssertEquals(ShipmentTypes.Codes.LowValue, Converter.GetEntryTypeFromCode(Factory, Converter.EntryTypeCodes.LowValueEntriesWithFDA));
			AssertEquals(ShipmentTypes.Codes.LowValue, Converter.GetEntryTypeFromCode(Factory, Converter.EntryTypeCodes.LowValueEntriesWithoutFDA));
			AssertEquals(InbondTypes.Codes.ImmediateTransportation, Converter.GetEntryTypeFromCode(Factory, Converter.EntryTypeCodes.ImmediateTransportation));
		}

		public void TestGetTravelDocumentCode()
		{
			AssertEquals("39", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.Passport).ToString());
			AssertEquals("40", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.DrivingLicenseNational).ToString());
			AssertEquals("5K", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.CommercialDriversLicense).ToString());
			AssertEquals("6W", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.EnhancedDriversLicense).ToString());
			AssertEquals("989", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.VisaImmigrant).ToString());
			AssertEquals("AAG", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.MilitaryIdDocument).ToString());
			AssertEquals("ACU", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.PermanentResidentCard2).ToString());
			AssertEquals("AEF", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.USPassportCard).ToString());
			AssertEquals("AEW", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.NexusCard).ToString());
			AssertEquals("AGR", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.USAlienRegistrationCard1).ToString());
			AssertEquals("AGS", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.PermanentResidentCard1).ToString());
			AssertEquals("AGT", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.VisaNonImmigrant).ToString());
			AssertEquals("ALR", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.USAlienRegistrationCard2).ToString());
			AssertEquals("ALV", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.SentriCard).ToString());
			AssertEquals("ALX", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.USMerchantMarinerDocument).ToString());
			AssertEquals("ALY", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.NativeAmericanIndian).ToString());
			AssertEquals("BCN", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.BirthCertificate).ToString());
			AssertEquals("BCP", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.LaserVisaBorderCrossingCard).ToString());
			AssertEquals("CDN", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.CitizenshipDocumentNumber).ToString());
			AssertEquals("CON", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.CertificateOfNaturalization).ToString());
			AssertEquals("OTD", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.OtherTravelDocument).ToString());
			AssertEquals("REP", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.DHSReEntryPermit).ToString());
			AssertEquals("RTP", Converter.GetTravelDocumentCodeFromType(Factory, TravelDocumentTypes.Codes.DHSRefugeeTravelDocument).ToString());
			AssertEquals(TravelDocumentTypes.Codes.Passport, Converter.GetTravelDocumentTypeFromCode(Factory, DocumentNameCodeList.Passport));
		}

		public void TestGetTravelDocumentQualifier()
		{
			AssertEquals("ACE", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.EnhancedDriversLicense).ToString());
			AssertEquals("AFZ", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.NativeAmericanIndian).ToString());
			AssertEquals("AGF", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.MilitaryIdDocument).ToString());
			AssertEquals("AIG", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.Passport).ToString());
			AssertEquals("ALH", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.HazmatEndorsement).ToString());
			AssertEquals("ALR", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.USAlienRegistrationCard2).ToString());
			AssertEquals("ALV", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.SentriCard).ToString());
			AssertEquals("AQW", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.DrivingLicenseNational).ToString());
			AssertEquals("AQY", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.PermanentResidentCard1).ToString());
			AssertEquals("ARJ", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.USPassportCard).ToString());
			AssertEquals("ASX", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.USAlienRegistrationCard1).ToString());
			AssertEquals("AUB", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.VisaImmigrant).ToString());
			AssertEquals("AVP", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.USMerchantMarinerDocument).ToString());
			AssertEquals("CDN", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.CitizenshipDocumentNumber).ToString());
			AssertEquals("CON", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.CertificateOfNaturalization).ToString());
			AssertEquals("CR", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.LaserVisaBorderCrossingCard).ToString());
			AssertEquals("DM", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.NexusCard).ToString());
			AssertEquals("ET", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.PermanentResidentCard2).ToString());
			AssertEquals("GN", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.VisaNonImmigrant).ToString());
			AssertEquals("OTD", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.OtherTravelDocument).ToString());
			AssertEquals("REP", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.DHSReEntryPermit).ToString());
			AssertEquals("RTP", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.DHSRefugeeTravelDocument).ToString());
			AssertEquals("ZZZ", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.CommercialDriversLicense).ToString());
			AssertEquals("BCN", Converter.GetTravelDocumentQualifierFromType(Factory, TravelDocumentTypes.Codes.BirthCertificate).ToString());
		}

		public void TestGetMethodOfTransportation()
		{
			AssertEquals(Converter.TransportTypes.Road, Converter.GetMethodOfTransportationQualifier(Factory, TransportModes.Codes.Road));
			AssertEquals("BL", Converter.GetMethodOfTransportationQualifier(Factory, "BL"));
			AssertEquals(TransportModes.Codes.Road, Converter.GetMethodOfTransportationFromQualifier(Factory, Converter.TransportTypes.Road));
			AssertEquals("BL", Converter.GetMethodOfTransportationFromQualifier(Factory, "BL"));
		}
	}
}
