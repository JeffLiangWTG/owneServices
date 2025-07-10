using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripCloneStrategyTest : TestCaseWithFactory
	{
		[TestDate(2015, 08, 22, 14, 00, 00)]
		public void TestCloneTripAndEquipment()
		{
			//Trip
			AssertEquals("BH_JobReference", "MAN0000001", clonedTrip.BH_JobReference);
			AssertEquals("BH_ImportTransportMode", TransportModes.Codes.Road, clonedTrip.BH_ImportTransportMode);
			AssertEquals("BH_CarrierSCAC", "AAGC", clonedTrip.BH_CarrierSCAC);
			AssertEquals("BH_RL_NKPortUnlading", "USBUF", clonedTrip.BH_RL_NKPortUnlading);
			AssertEquals("BH_PortUnladingDCode", "0901", clonedTrip.BH_PortUnladingDCode);
			AssertEquals("BH_ETA", new ZDateTime(2015, 08, 22, 16, 00, 00), clonedTrip.BH_ETA);
			AssertEquals("BH_TransitDirection", TransitDirectionCodes.Codes.Importation, clonedTrip.BH_TransitDirection);
			AssertEquals("Notes.Count", 1, clonedTrip.Notes.GetAllNotes().Count);
			var clonedNote = (StmNote)clonedTrip.Notes.GetAllNotes().ToArray()[0];
			AssertEquals("ST_IsCustomDescription", true, clonedNote.ST_IsCustomDescription);
			AssertEquals("ST_Description", "Description", clonedNote.ST_Description);
			AssertEquals("ST_NoteDataAsText", "Note text", clonedNote.ST_NoteDataAsText);
			//Conveyance
			var clonedConveyance = clonedTrip.Conveyance;
			AssertEquals("BJ_RQ_Equipment", conveyance.BJ_RQ_Equipment, clonedConveyance.BJ_RQ_Equipment);
			AssertEquals("BJ_IITEntityIndicators", ZString.Empty, clonedConveyance.BJ_IITEntityIndicators);
			AssertEquals("BJ_InsuranceName", ZString.Empty, clonedConveyance.BJ_InsuranceName);
			AssertEquals("BJ_InsurancePolicyNumber", ZString.Empty, clonedConveyance.BJ_InsurancePolicyNumber);
			AssertEquals("BJ_InsuranceYearPolicyIssue", ZInt.Zero, clonedConveyance.BJ_InsuranceYearPolicyIssue);
			AssertEquals("BJ_InsuranceAmount", ZDecimal.Zero, clonedConveyance.BJ_InsuranceAmount);
			AssertEquals("BJ_SealNumbers", ZString.Empty, clonedConveyance.BJ_SealNumbers);
			//Equipment
			AssertEquals("Equipment.Count", 2, clonedTrip.Equipment.Count);
			var clonedEquipment = clonedTrip.Equipment.SingleOrDefault(x => x.BJ_RQ_Equipment == equipment1.BJ_RQ_Equipment);
			AssertNotNull("equipment1 cloned", clonedEquipment);
			AssertEquals("BJ_IITEntityIndicators", ZString.Empty, clonedEquipment.BJ_IITEntityIndicators);
			AssertEquals("BJ_SealNumbers", ZString.Empty, clonedEquipment.BJ_SealNumbers);
			clonedEquipment = clonedTrip.Equipment.SingleOrDefault(x => x.BJ_RQ_Equipment == equipment2.BJ_RQ_Equipment);
			AssertNotNull("equipment2 cloned", clonedEquipment);
			AssertEquals("BJ_IITEntityIndicators", ZString.Empty, clonedEquipment.BJ_IITEntityIndicators);
			AssertEquals("BJ_SealNumbers", ZString.Empty, clonedEquipment.BJ_SealNumbers);
		}

		public void TestCloneCrewMembers()
		{
			AssertEquals("CrewMembers.Count", 3, clonedTrip.CrewMembers.Count);
			var clonedCrew = clonedTrip.CrewMembers.SingleOrDefault(x => x.CP_Type == CrewTypes.Codes.ResponsibleParty);
			AssertNotNull("PR CrewMember cloned", clonedCrew);
			AssertEquals("CP_FullName", "CHRIS AOMAD", clonedCrew.CP_FullName);
			AssertEquals("CP_DateOfBirth", new ZDate(1935, 09, 19), clonedCrew.CP_DateOfBirth);
			AssertEquals("CP_Gender", Constants.Genders.Man, clonedCrew.CP_Gender);
			AssertEquals("CP_RN_NKNationality", Constants.CountryCodes.UnitedStates, clonedCrew.CP_RN_NKNationality);
			AssertEquals("Certificates.Count", 2, clonedCrew.Certificates.Count);
			var clonedCertificate = clonedCrew.Certificates.SingleOrDefault(x => x.XZ_Type == TravelDocumentTypes.Codes.CommercialDriversLicense);
			AssertNotNull("CDL Certificate cloned", clonedCrew);
			AssertEquals("XZ_RefNumber", "P100971204141", clonedCertificate.XZ_RefNumber);
			AssertEquals("XZ_ExpiryOrDueDate", new ZDateTime(2014, 03, 27), clonedCertificate.XZ_ExpiryOrDueDate);
			AssertEquals("XZ_RN_NKCountryOfIssuance", Constants.CountryCodes.UnitedStates, clonedCertificate.XZ_RN_NKCountryOfIssuance);
			AssertEquals("XZ_StateOrProvinceOfIssuance", USStatesList.Codes.Virginia, clonedCertificate.XZ_StateOrProvinceOfIssuance);
			clonedCertificate = clonedCrew.Certificates.SingleOrDefault(x => x.XZ_Type == TravelDocumentTypes.Codes.Passport);
			AssertNotNull("PAS Certificate cloned", clonedCrew);
			AssertEquals("XZ_RefNumber", "15504141", clonedCertificate.XZ_RefNumber);
			AssertEquals("XZ_ExpiryOrDueDate", new ZDateTime(2014, 03, 27), clonedCertificate.XZ_ExpiryOrDueDate);
			AssertEquals("XZ_RN_NKCountryOfIssuance", Constants.CountryCodes.UnitedStates, clonedCertificate.XZ_RN_NKCountryOfIssuance);
			AssertEquals("XZ_StateOrProvinceOfIssuance", ZString.Empty, clonedCertificate.XZ_StateOrProvinceOfIssuance);
			var clonedAddress = clonedCrew.USAddress;
			AssertEquals("E2_AddressOverride", true, clonedAddress.E2_AddressOverride);
			AssertEquals("E2_Address1", "11107 SUNSET HILLS ROAD", clonedAddress.E2_Address1);
			AssertEquals("E2_City", "RESTON", clonedAddress.E2_City);
			AssertEquals("E2_State", USStatesList.Codes.Virginia, clonedAddress.E2_State);
			AssertEquals("E2_RN_NKCountryCode", Constants.CountryCodes.UnitedStates, clonedAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_Postcode", "20190", clonedAddress.E2_Postcode);
			clonedCrew = clonedTrip.CrewMembers.SingleOrDefault(x => x.CP_Type == CrewTypes.Codes.CrewMember);
			clonedAddress = clonedCrew.USAddress;
			AssertNotNull("CM CrewMember cloned", clonedCrew);
			AssertEquals("CP_HasHazmatEndorsment", true, clonedCrew.CP_HasHazmatEndorsment);
			AssertEquals("CP_OC_Contact", crew.CP_OC_Contact, clonedCrew.CP_OC_Contact);
			AssertEquals("USAddress.OrganisationPK", crew.USAddress.OrganisationPK, clonedAddress.OrganisationPK);
			clonedCrew = clonedTrip.CrewMembers.SingleOrDefault(x => x.CP_Type == CrewTypes.Codes.Passenger);
			clonedAddress = clonedCrew.USAddress;
			AssertNotNull("PS CrewMember cloned", clonedCrew);
			AssertEquals("CP_HasHazmatEndorsment", false, clonedCrew.CP_HasHazmatEndorsment);
			AssertEquals("CP_GS_NKStaff", passenger.CP_GS_NKStaff, clonedCrew.CP_GS_NKStaff);
			AssertEquals("USAddress.IsEmpty", true, clonedAddress.IsEmpty);
		}

		public void TestCloneShipments()
		{
			AssertEquals("Shipments.Count", 3, clonedTrip.Shipments.Count);
			//Shipment
			var clonedShipment = clonedTrip.Shipments.SingleOrDefault(x => x.B0_ShipmentType == ShipmentTypes.Codes.BRASS);
			AssertNotNull("RAS Shipment Cloned", clonedShipment);
			clonedShipment = clonedTrip.Shipments.SingleOrDefault(x => x.B0_ShipmentType == ShipmentTypes.Codes.LowValue);
			AssertNotNull("SEC Shipment Cloned", clonedShipment);
			clonedShipment = clonedTrip.Shipments.SingleOrDefault(x => x.B0_ShipmentType == ShipmentTypes.Codes.Inbond);
			AssertNotNull("INB Shipment Cloned", clonedShipment);
			AssertEquals("B0_MasterBillNumber", ZString.Empty, clonedShipment.B0_MasterBillNumber);
			AssertEquals("B0_ReferenceID", ZString.Empty, clonedShipment.B0_ReferenceID);
			AssertEquals("B0_RL_NKPortOfLading", "CATOR", clonedShipment.B0_RL_NKPortOfLading);
			AssertEquals("B0_PortOfLadingKCode", "01535", clonedShipment.B0_PortOfLadingKCode);
			AssertEquals("B0_PlaceOfReceipt", "Tahsis", clonedShipment.B0_PlaceOfReceipt);
			AssertEquals("B0_ServiceType", ServiceTypes.Codes.CollectOnDelivery, clonedShipment.B0_ServiceType);
			AssertEquals("B0_Firms", "A000", clonedShipment.B0_Firms);
			AssertEquals("B0_ManifestQty", ZInt.Zero, clonedShipment.B0_ManifestQty);
			AssertEquals("B0_ManifestUQ", Constants.PkgUnit.Piece, clonedShipment.B0_ManifestUQ);
			AssertEquals("B0_BoardedQuantity", ZInt.Zero, clonedShipment.B0_BoardedQuantity);
			AssertEquals("B0_Weight", ZDecimal.Zero, clonedShipment.B0_Weight);
			AssertEquals("B0_WeightUQ", Constants.Weight.Kilograms, clonedShipment.B0_WeightUQ);
			AssertEquals("B0_Volume", ZDecimal.Zero, clonedShipment.B0_Volume);
			AssertEquals("B0_VolumeUQ", Constants.Volume.CubicMetres, clonedShipment.B0_VolumeUQ);
			AssertEquals("B0_DateOfExport", ZDateTime.Empty, clonedShipment.B0_DateOfExport);
			AssertEquals("B0_WasOutOfUSFor45DaysOrLess", true, clonedShipment.B0_WasOutOfUSFor45DaysOrLess);
			AssertEquals("B0_IsFDAFreight", true, clonedShipment.B0_IsFDAFreight);
			//Parties
			var clonedParty = clonedShipment.Consignee;
			AssertEquals("E2_AddressOverride", true, clonedParty.E2_AddressOverride);
			AssertEquals("E2_AddressType", PartyTypes.Codes.Consignee, clonedParty.E2_AddressType);
			AssertEquals("E2_CompanyName", PartyTypes.Descriptions.Consignee, clonedParty.E2_CompanyName);
			AssertEquals("E2_GovRegNum", "54654", clonedParty.E2_GovRegNum);
			AssertEquals("E2_GovRegNumType", PartyIdTypes.Codes.ACE, clonedParty.E2_GovRegNumType);
			AssertEquals("E2_Phone", "+3 (126) 4846516", clonedParty.E2_Phone);
			AssertEquals("E2_Email", "party@test.net", clonedParty.E2_Email);
			AssertEquals("E2_Address1", "12 Party Street", clonedParty.E2_Address1);
			AssertEquals("E2_City", "Chicago", clonedParty.E2_City);
			AssertEquals("E2_State", USStatesList.Codes.Illinois, clonedParty.E2_State);
			AssertEquals("E2_RN_NKCountryCode", Constants.CountryCodes.UnitedStates, clonedParty.E2_RN_NKCountryCode);
			AssertEquals("E2_Postcode", "2009", clonedParty.E2_Postcode);
			clonedParty = clonedShipment.Shipper;
			AssertEquals("E2_AddressType", PartyTypes.Codes.Shipper, clonedParty.E2_AddressType);
			AssertEquals("Shipper.OrganisationPK", party2.OrganisationPK, clonedParty.OrganisationPK);
			AssertEquals("Parties.Count", 1, clonedShipment.Parties.Count);
			clonedParty = clonedShipment.Parties[0];
			AssertEquals("E2_AddressType", PartyTypes.Codes.CustomsBroker, clonedParty.E2_AddressType);
			AssertEquals("CustomsBroker.OrganisationPK", party3.OrganisationPK, clonedParty.OrganisationPK);
			//InBond
			var clonedInBond = clonedShipment.InBond;
			AssertEquals("BM_InBondEntryType", InbondTypes.Codes.ImmediateExportation, clonedInBond.BM_InBondEntryType);
			AssertEquals("BM_RL_NKDestinationPort", "USBUF", clonedInBond.BM_RL_NKDestinationPort);
			AssertEquals("BM_DestinationPortCode", "0901", clonedInBond.BM_DestinationPortCode);
			AssertEquals("BM_OnwardCarrier", "AAGC", clonedInBond.BM_OnwardCarrier);
			AssertEquals("BM_InBondCarrierID", "61-059874300", clonedInBond.BM_InBondCarrierID);
			AssertEquals("InBondNumber", ZString.Empty, clonedInBond.InBondNumber);
			AssertEquals("BM_TransferCarrier", "61-059874300", clonedInBond.BM_TransferCarrier);
			AssertEquals("BM_RL_NKForeignDestPort", "CATOR", clonedInBond.BM_RL_NKForeignDestPort);
			AssertEquals("BM_ForeignDestPortKCode", "01535", clonedInBond.BM_ForeignDestPortKCode);
			AssertEquals("BM_ExportDate", ZDateTime.Empty, clonedInBond.BM_ExportDate);
			AssertEquals("BM_PedimentoNumber", "98765413265478", clonedInBond.BM_PedimentoNumber);
		}

		public void TestCloneCommodities()
		{
			AssertEquals("Shipments.Count", 3, clonedTrip.Shipments.Count);
			var clonedShipment = clonedTrip.Shipments[0];
			AssertEquals("Commodities.Count", trip.Shipments[0].Commodities.Count, clonedShipment.Commodities.Count);
			var clonedEquipments = clonedTrip.AllEquipmentIncludingMainConveyance;
			var clonedCommodity = clonedShipment.FirstCommodity;
			AssertEquals("BY_C4Codes", "13213213, 21646544", clonedCommodity.BY_C4Codes);
			clonedShipment = clonedTrip.Shipments[1];
			AssertEquals("Commodities.Count", trip.Shipments[1].Commodities.Count, clonedShipment.Commodities.Count);
			clonedCommodity = clonedShipment.FirstCommodity;
			AssertEquals("BY_RN_NKCountryOfOrigin", Constants.CountryCodes.France, clonedCommodity.BY_RN_NKCountryOfOrigin);
			clonedShipment = clonedTrip.Shipments[2];
			AssertEquals("Commodities.Count", trip.Shipments[2].Commodities.Count, clonedShipment.Commodities.Count);
			clonedCommodity = clonedShipment.Commodities.First(x => x.BY_Description == "FRENCH MILK CHOCOLATE");
			AssertEquals("BY_PieceCount", ZInt.Zero, clonedCommodity.BY_PieceCount);
			AssertEquals("BY_ManifestUnitCode", Constants.PkgUnit.Box, clonedCommodity.BY_ManifestUnitCode);
			AssertEquals("BY_GrossWeight", ZDecimal.Zero, clonedCommodity.BY_GrossWeight);
			AssertEquals("BY_GrossWeightUnit", Constants.Weight.Tonnes, clonedCommodity.BY_GrossWeightUnit);
			AssertEquals("BY_Description", "FRENCH MILK CHOCOLATE", clonedCommodity.BY_Description);
			AssertEquals("BY_BJ_Equipment", clonedEquipments.FirstOrDefault().PK, clonedCommodity.BY_BJ_Equipment);
			AssertEquals("BY_MarksAndNumbers", "MARKS", clonedCommodity.BY_MarksAndNumbers);
			AssertEquals("BY_HarmonizedNumbers", "6601.10.00 00, 6602.00.10 00", clonedCommodity.BY_HarmonizedNumbers);
			AssertEquals("BY_VehicleIdentificationNumbers", ZString.Empty, clonedCommodity.BY_VehicleIdentificationNumbers);
			AssertEquals("BY_C4Codes", ZString.Empty, clonedCommodity.BY_C4Codes);
			AssertEquals("BY_MonetaryValue", ZDecimal.Zero, clonedCommodity.BY_MonetaryValue);
			AssertEquals("BY_RN_NKCountryOfOrigin", ZString.Empty, clonedCommodity.BY_RN_NKCountryOfOrigin);
			AssertEquals("UNDGs.Count", 2, clonedCommodity.UNDGs.Count);
			var clonedUndg132 = clonedCommodity.UNDGs.SingleOrDefault(x => x.UNDGSubstance.DG_Code == "132");
			AssertNotNull(clonedUndg132);
			AssertEquals("DI_OC_DGContact", undgContact.PK, clonedUndg132.DI_OC_DGContact);
			var clonedUndg456 = clonedCommodity.UNDGs.SingleOrDefault(x => x.UNDGSubstance.DG_Code == "456");
			AssertNotNull(clonedUndg456);
			AssertEquals("DI_OC_DGContact", undgContact.PK, clonedUndg456.DI_OC_DGContact);
			clonedCommodity = clonedShipment.Commodities.First(x => x.BY_Description == "FRENCH DARK CHOCOLATE");
			AssertEquals("BY_PieceCount", ZInt.Zero, clonedCommodity.BY_PieceCount);
			AssertEquals("BY_ManifestUnitCode", Constants.PkgUnit.Box, clonedCommodity.BY_ManifestUnitCode);
			AssertEquals("BY_GrossWeight", ZDecimal.Zero, clonedCommodity.BY_GrossWeight);
			AssertEquals("BY_GrossWeightUnit", Constants.Weight.Tonnes, clonedCommodity.BY_GrossWeightUnit);
			AssertEquals("BY_Description", "FRENCH DARK CHOCOLATE", clonedCommodity.BY_Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			#region Trip
			trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0001001";
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_CarrierSCAC = "AAGC";
			trip.BH_RL_NKPortUnlading = "USBUF";
			trip.BH_PortUnladingDCode = "0901";
			trip.BH_ETA = ZDateTime.Now;
			trip.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			trip.Notes.AddNew(true, "Description", "Note text");
			#endregion
			#region Conveyance / Equipment
			conveyance = trip.Conveyance;
			conveyance.BJ_RQ_Equipment = Factory.NewWithValidTestData<RefEquipment>().PK;
			conveyance.BJ_EmptyIITsCoveredByCarrier = true;
			conveyance.BJ_InsuranceName = "Hazmat Shipment Insurance";
			conveyance.BJ_InsurancePolicyNumber = "1234567890";
			conveyance.BJ_InsuranceYearPolicyIssue = 2011;
			conveyance.BJ_InsuranceAmount = 2000000;
			conveyance.SealNumbers.AddNew().CY_Data = "12345";
			equipment1 = trip.Equipment.AddNew();
			equipment1.SealNumbers.AddNew().CY_Data = "56484";
			equipment1.BJ_RQ_Equipment = Factory.NewWithValidTestData<RefEquipment>().PK;
			equipment2 = trip.Equipment.AddNew();
			equipment2.BJ_EmptyIITsCoveredByImporter = true;
			equipment2.BJ_RQ_Equipment = Factory.NewWithValidTestData<RefEquipment>().PK;
			#endregion
			#region Crew
			var responsibleParty = trip.CrewMembers.AddNew();
			responsibleParty.CP_Type = CrewTypes.Codes.ResponsibleParty;
			responsibleParty.CP_FullName = "CHRIS AOMAD";
			responsibleParty.CP_DateOfBirth = new ZDate(1935, 09, 19);
			responsibleParty.CP_Gender = Constants.Genders.Man;
			responsibleParty.CP_RN_NKNationality = Constants.CountryCodes.UnitedStates;
			AddDocOrNumber(responsibleParty.Certificates, TravelDocumentTypes.Codes.CommercialDriversLicense, "P100971204141", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates, USStatesList.Codes.Virginia);
			AddDocOrNumber(responsibleParty.Certificates, TravelDocumentTypes.Codes.Passport, "15504141", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates);
			var address = responsibleParty.USAddress;
			address.E2_AddressOverride = true;
			address.E2_Address1 = "11107 SUNSET HILLS ROAD";
			address.E2_City = "RESTON";
			address.E2_State = USStatesList.Codes.Virginia;
			address.E2_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			address.E2_Postcode = "20190";
			crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.CrewMember;
			crew.CP_HasHazmatEndorsment = true;
			crew.CP_OC_Contact = Factory.NewWithValidTestData<OrgContact>().PK;
			crew.USAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			passenger = trip.CrewMembers.AddNew();
			passenger.CP_Type = CrewTypes.Codes.Passenger;
			passenger.CP_GS_NKStaff = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			#endregion
			#region Shipments
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			var commodity1 = shipment.Commodities.AddNew();
			commodity1.C4Codes.AddNew().CY_Data = "13213213";
			commodity1.C4Codes.AddNew().CY_Data = "21646544";
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			var commodity2 = shipment1.Commodities.AddNew();
			commodity2.BY_RN_NKCountryOfOrigin = Constants.CountryCodes.France;
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment2.B0_MasterBillNumber = "AAGC123456789012";
			shipment2.B0_ReferenceID = "shipment2 1";
			shipment2.B0_RL_NKPortOfLading = "CATOR";
			shipment2.B0_PortOfLadingKCode = "01535";
			shipment2.B0_PlaceOfReceipt = "Tahsis";
			shipment2.B0_ServiceType = ServiceTypes.Codes.CollectOnDelivery;
			shipment2.B0_Firms = "A000";
			shipment2.B0_ManifestQty = 500;
			shipment2.B0_ManifestUQ = Constants.PkgUnit.Piece;
			shipment2.B0_BoardedQuantity = 100;
			shipment2.B0_Weight = 200;
			shipment2.B0_WeightUQ = Constants.Weight.Kilograms;
			shipment2.B0_Volume = 100;
			shipment2.B0_VolumeUQ = Constants.Volume.CubicMetres;
			shipment2.B0_DateOfExport = ZDateTime.Now;
			shipment2.B0_WasOutOfUSFor45DaysOrLess = true;
			shipment2.B0_IsFDAFreight = true;
			#endregion
			#region Parties
			var party1 = shipment2.Consignee;
			party1.E2_AddressOverride = true;
			party1.E2_AddressType = PartyTypes.Codes.Consignee;
			party1.E2_CompanyName = PartyTypes.Descriptions.Consignee;
			party1.E2_GovRegNum = "54654";
			party1.E2_GovRegNumType = PartyIdTypes.Codes.ACE;
			party1.E2_Phone = "+3 (126) 4846516";
			party1.E2_Email = "party@test.net";
			party1.E2_Address1 = "12 Party Street";
			party1.E2_City = "Chicago";
			party1.E2_State = USStatesList.Codes.Illinois;
			party1.E2_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			party1.E2_Postcode = "2009";
			party2 = shipment2.Shipper;
			party2.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			party3 = shipment2.Parties.AddNew();
			party3.E2_AddressType = PartyTypes.Codes.CustomsBroker;
			party3.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			#endregion
			#region Commodities
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "132";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "456";
			subs2.DG_Variant = "";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var commodity3 = shipment2.Commodities.AddNew();
			commodity3.BY_PieceCount = 100;
			commodity3.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			commodity3.BY_GrossWeight = 2.65;
			commodity3.BY_GrossWeightUnit = Constants.Weight.Tonnes;
			commodity3.BY_Description = "FRENCH MILK CHOCOLATE";
			commodity3.BY_BJ_Equipment = ((Equipment)commodity3.Lookups.Equipment[1]).PK;
			commodity3.BY_MarksAndNumbers = "MARKS";
			commodity3.HarmonizedNumbers.AddNew().CY_Data = "6601100000";
			commodity3.HarmonizedNumbers.AddNew().CY_Data = "6602001000";
			commodity3.VehicleIdentificationNumbers.AddNew().CY_Data = "64987465456";
			commodity3.VehicleIdentificationNumbers.AddNew().CY_Data = "4654879874";
			commodity3.BY_MonetaryValue = 2500;
			undgContact = Factory.NewWithValidTestData<OrgContact>();
			var hazmat = commodity3.UNDGs.AddNew();
			hazmat.DI_DG = subs.PK;
			hazmat.LinkDefault(subs);
			hazmat.DI_OC_DGContact = undgContact.PK;
			hazmat = commodity3.UNDGs.AddNew();
			hazmat.DI_DG = subs2.PK;
			hazmat.LinkDefault(subs2);
			hazmat.DI_OC_DGContact = undgContact.PK;
			var commodity4 = shipment2.Commodities.AddNew();
			commodity4.BY_PieceCount = 100;
			commodity4.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			commodity4.BY_GrossWeight = 2.65;
			commodity4.BY_GrossWeightUnit = Constants.Weight.Tonnes;
			commodity4.BY_Description = "FRENCH DARK CHOCOLATE";
			#endregion
			#region InBond
			var inbond = shipment2.InBond;
			inbond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			inbond.BM_RL_NKDestinationPort = "USBUF";
			inbond.BM_DestinationPortCode = "0901";
			inbond.BM_OnwardCarrier = "AAGC";
			inbond.BM_InBondCarrierID = "61-059874300";
			inbond.InBondNumber = "VVV34567890";
			inbond.BM_TransferCarrier = "61-059874300";
			inbond.BM_RL_NKForeignDestPort = "CATOR";
			inbond.BM_ForeignDestPortKCode = "01535";
			inbond.BM_ExportDate = ZDateTime.Now;
			inbond.BM_PedimentoNumber = "98765413265478";
			#endregion
			clonedTrip = (Trip)((ITemplateCopyable)trip).TemplateCopy();
			Factory.Save();
			clonedTrip = new BusinessObjectFactory().Load<Trip>(clonedTrip.PK);
		}

		void AddDocOrNumber(GenRegCertAccredMaintListCollection certificates, string type, string number, ZDateTime? expiry = null, string country = null, string state = null)
		{
			var cert = certificates.AddNew();
			cert.XZ_Type = type;
			cert.XZ_RefNumber = number;
			cert.XZ_ExpiryOrDueDate = expiry.GetValueOrDefault();
			cert.XZ_RN_NKCountryOfIssuance = country;
			cert.XZ_StateOrProvinceOfIssuance = state;
		}

		Trip trip;
		Equipment conveyance;
		Equipment equipment1;
		Equipment equipment2;
		Trip clonedTrip;
		CrewMember crew;
		CrewMember passenger;
		Party party2;
		Party party3;
		OrgContact undgContact;
	}
}
