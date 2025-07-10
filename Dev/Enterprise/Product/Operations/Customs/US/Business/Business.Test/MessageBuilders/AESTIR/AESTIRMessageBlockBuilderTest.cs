using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class AESTIRMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			Mock<IAESTIRMessageAttachee> attacheeMock = GetNewAttacheeMock(true);
			AESTIRMessageBlockBuilder builder = new AESTIRMessageBlockBuilder(attacheeMock.Object);
			List<MessageBlock> messageBlocks = new List<MessageBlock>(builder.Build(UpdateActionCode.Add));
			AssertEquals(28, messageBlocks.Count);
			AssertSC1Record((AESCommShipSC1XP)messageBlocks[0], "A");
			AssertSC2Record((AESCommShipSC2XP)messageBlocks[1]);

			AssertSC3Record((AESCommShipSC3XP)messageBlocks[2], "", "", "TRE1231231");
			AssertSC3Record((AESCommShipSC3XP)messageBlocks[3], "CTRU1235455", "5694", "");

			AssertAESParty((AESCommShipN01XP)messageBlocks[4], (AESCommShipN02XP)messageBlocks[5], (AESCommShipN03XP)messageBlocks[6], AESTIRPartyTypeList.Codes.USPPI, "E", "123456789", "", "");
			AssertAESParty((AESCommShipN01XP)messageBlocks[7], (AESCommShipN02XP)messageBlocks[8], (AESCommShipN03XP)messageBlocks[9], AESTIRPartyTypeList.Codes.ForwardingAgent, "", "", "", "");
			AssertAESParty((AESCommShipN01XP)messageBlocks[10], (AESCommShipN02XP)messageBlocks[11], (AESCommShipN03XP)messageBlocks[12], AESTIRPartyTypeList.Codes.UltimateConsignee, "", "", "N", UltimateConsigneeTypeList.Codes.GovernmentEntity);
			AssertAESParty((AESCommShipN01XP)messageBlocks[13], (AESCommShipN02XP)messageBlocks[14], (AESCommShipN03XP)messageBlocks[15], AESTIRPartyTypeList.Codes.IntermediateConsignee, "", "", "", "");

			AssertCommodityLine(1, (AESCommShipCL1XP)messageBlocks[16], (AESCommShipCL2XP)messageBlocks[17], (AESCommShipODTXP)messageBlocks[18], (AESCommShipEV1XP)messageBlocks[19], (AESCommShipEV1XP)messageBlocks[20]);
			AssertCommodityLine(2, (AESCommShipCL1XP)messageBlocks[22], (AESCommShipCL2XP)messageBlocks[23], (AESCommShipODTXP)messageBlocks[24], (AESCommShipEV1XP)messageBlocks[25], (AESCommShipEV1XP)messageBlocks[26]);

			attacheeMock.Setup(m => m.IsSoldEnRoute).Returns(ZBool.True);

			// Commodity Line Items
			List<IAESTIRCommodityLineItem> commodityLineItems = new List<IAESTIRCommodityLineItem>();
			commodityLineItems.Add(GetNewCommodityLine(1, false, false).Object);
			attacheeMock.Setup(m => m.CommodityLineItems).Returns(commodityLineItems);

			messageBlocks = new List<MessageBlock>(builder.Build(UpdateActionCode.Replace));
			AssertEquals(19, messageBlocks.Count);
			AssertSC1Record((AESCommShipSC1XP)messageBlocks[0], "R");
			AssertSC2Record((AESCommShipSC2XP)messageBlocks[1]);

			AssertSC3Record((AESCommShipSC3XP)messageBlocks[2], "", "", "TRE1231231");
			AssertSC3Record((AESCommShipSC3XP)messageBlocks[3], "CTRU1235455", "5694", "");

			AssertAESParty((AESCommShipN01XP)messageBlocks[4], (AESCommShipN02XP)messageBlocks[5], (AESCommShipN03XP)messageBlocks[6], AESTIRPartyTypeList.Codes.USPPI, "E", "123456789", "", "");
			AssertAESParty((AESCommShipN01XP)messageBlocks[7], (AESCommShipN02XP)messageBlocks[8], (AESCommShipN03XP)messageBlocks[9], AESTIRPartyTypeList.Codes.ForwardingAgent, "", "", "", "");
			AssertSoldEnRoute((AESCommShipN01XP)messageBlocks[10], (AESCommShipN02XP)messageBlocks[11], (AESCommShipN03XP)messageBlocks[12], "SYDNEY", "AUSTRALIA");
			AssertAESParty((AESCommShipN01XP)messageBlocks[13], (AESCommShipN02XP)messageBlocks[14], (AESCommShipN03XP)messageBlocks[15], AESTIRPartyTypeList.Codes.IntermediateConsignee, "", "", "", "");

			AssertCommodityLine(1, (AESCommShipCL1XP)messageBlocks[16], (AESCommShipCL2XP)messageBlocks[17], null, null, null);
		}

		public void TestSendAESPGAMessageNoExceptionThrowWhenShouldDataComesFromLastClearMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry.CH_BGMReference = "BHK3234232";
			var entryLine = entry.MergedLines.AddNew();

			var incomingMessage = entry.Messages.AddNew(typeof(MQEDIMessage));
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			incomingMessage.EM_MessageText =
"B  D0612456712E          US EXPORTER NAME 4                                     " +
"SC1Y12NZNYXXXAS40002510        ABAI WHO YUN HE         4 60167270120071101 Y    " +
"ES1974 A  SHIPMENT ADDED                          X20111130000105               " +
"Y  D0612456712E          US EXPORTER NAME 4";
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(4);
			incomingMessage.EM_MessageNum = "MSG4";

			entry.US_IsDeactivated = true;
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Load();
			invoice.Delete();
			var builder = new AESTIRMessageBlockBuilder(entry);
			AssertNoExceptionThrown(() => builder.Build(UpdateActionCode.Add));
		}

		public void TestBuildDoesNotIncludeBlankSC3Segments()
		{
			Mock<IAESTIRMessageAttachee> attacheeMock = GetNewAttacheeMock(false);
			var builder = new AESTIRMessageBlockBuilder(attacheeMock.Object);
			List<MessageBlock> messageBlocks = new List<MessageBlock>(builder.Build(UpdateActionCode.Add));
			AssertSC1Record((AESCommShipSC1XP)messageBlocks[0], "A");
			AssertSC2Record((AESCommShipSC2XP)messageBlocks[1]);

			AssertAESParty((AESCommShipN01XP)messageBlocks[2], (AESCommShipN02XP)messageBlocks[3], (AESCommShipN03XP)messageBlocks[4], AESTIRPartyTypeList.Codes.USPPI, "E", "123456789", "", "");
			AssertAESParty((AESCommShipN01XP)messageBlocks[5], (AESCommShipN02XP)messageBlocks[6], (AESCommShipN03XP)messageBlocks[7], AESTIRPartyTypeList.Codes.ForwardingAgent, "", "", "", "");
		}

		public void TestSC1BlockWithDestinationCountryMM()
		{
			Mock<IAESTIRMessageAttachee> attacheeMock = GetNewAttacheeMock(false, new ZDate(2019, 09, 01), "MM");

			var builder = new AESTIRMessageBlockBuilder(attacheeMock.Object);
			List<MessageBlock> messageBlocks = new List<MessageBlock>(builder.Build(UpdateActionCode.Add));
			var messageSC1 = (AESCommShipSC1XP)messageBlocks[0];
			AssertEquals("CountryOfUltimateDestinationCode", Core.Constants.CountryCodes.Myanmar, messageSC1.CountryOfUltimateDestinationCode);
			var n03xp = (AESCommShipN03XP)messageBlocks[10];
			AssertEquals(Core.Constants.CountryCodes.Myanmar, n03xp.CountryCode);

			attacheeMock = GetNewAttacheeMock(false, new ZDate(2019, 09, 21), "MM");
			builder = new AESTIRMessageBlockBuilder(attacheeMock.Object);
			messageBlocks = new List<MessageBlock>(builder.Build(UpdateActionCode.Add));
			messageSC1 = (AESCommShipSC1XP)messageBlocks[0];
			AssertEquals("CountryOfUltimateDestinationCode", Core.Constants.CountryCodes.Myanmar, messageSC1.CountryOfUltimateDestinationCode);
			n03xp = (AESCommShipN03XP)messageBlocks[10];
			AssertEquals(Core.Constants.CountryCodes.Myanmar, n03xp.CountryCode);
		}

		public void TestBuildDelete()
		{
			Mock<IAESTIRMessageAttachee> attacheeMock = GetNewAttacheeMock(true);
			AESTIRMessageBlockBuilder builder = new AESTIRMessageBlockBuilder(attacheeMock.Object);
			List<MessageBlock> messageBlocks = new List<MessageBlock>(builder.Build(UpdateActionCode.Delete));
			AssertEquals("SC1 is only block required when cancelling a shipment", 1, messageBlocks.Count);
			AssertSC1Record((AESCommShipSC1XP)messageBlocks[0], "X");
		}

		public void TestSendAESPGAMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_EPAConsentNumber = "288127374";
			invoiceLine.US_HazWasteTrackingNo = "123456789ABC";
			invoiceLine.US_EPANetQty = 123m;
			invoiceLine.US_EPANetQtyUQ = "KG";
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var builder = new AESTIRMessageBlockBuilder(entry);
			var messageBlocks = builder.Build(UpdateActionCode.Add);
			var cl1Block = messageBlocks.OfType<AESCommShipCL1XP>().FirstOrDefault();
			AssertNotNull(cl1Block);
			AssertEquals("Y", cl1Block.PGALicenseRequiredIndicator);
			var pgaBlock = messageBlocks.OfType<AESCommShipPGAXP>().FirstOrDefault();
			AssertNotNull(pgaBlock);
			AssertEquals("EP1", pgaBlock.PGAID);
			AssertEquals("Y288127374   123456789ABCKG 0000000123                                   ", pgaBlock.PGAData);

			invoiceLine.US_PSTIndicator = ZString.Empty;
			invoiceLine.US_ExportCertificateNo = "ABC1234567";
			messageBlocks = builder.Build(UpdateActionCode.Add);
			cl1Block = messageBlocks.OfType<AESCommShipCL1XP>().FirstOrDefault();
			AssertNotNull(cl1Block);
			AssertEquals(ZString.Empty, cl1Block.PGALicenseRequiredIndicator);
			pgaBlock = messageBlocks.OfType<AESCommShipPGAXP>().FirstOrDefault();
			AssertNull(pgaBlock);

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			messageBlocks = builder.Build(UpdateActionCode.Add);
			cl1Block = messageBlocks.OfType<AESCommShipCL1XP>().FirstOrDefault();
			AssertNotNull(cl1Block);
			AssertEquals("Y", cl1Block.PGALicenseRequiredIndicator);
			pgaBlock = messageBlocks.OfType<AESCommShipPGAXP>().FirstOrDefault();
			AssertNotNull(pgaBlock);
			AssertEquals("ABC1234567                                                               ", pgaBlock.PGAData);

			invoiceLine.US_AMSInd = ZString.Empty;
			messageBlocks = builder.Build(UpdateActionCode.Add);
			cl1Block = messageBlocks.OfType<AESCommShipCL1XP>().FirstOrDefault();
			AssertNotNull(cl1Block);
			AssertEquals(ZString.Empty, cl1Block.PGALicenseRequiredIndicator);
		}

		void AssertSC1Record(AESCommShipSC1XP messageBlock, ZString action)
		{
			AssertEquals("RelatedCompanyIndicator", "Y", messageBlock.RelatedCompanyIndicator);
			AssertEquals("ModeOfTransportationCodeMOT", "10", messageBlock.ModeOfTransportationCodeMOT);
			AssertEquals("CountryOfUltimateDestinationCode", "AU", messageBlock.CountryOfUltimateDestinationCode);
			AssertEquals("USStateOfOriginCode", "CA", messageBlock.USStateOfOriginCode);
			AssertEquals("CarrierIDSCACIATA", "ACAC", messageBlock.CarrierIDSCACIATA);
			AssertEquals("ShipmentFilingActionRequestIndicator", action, messageBlock.ShipmentFilingActionRequestIndicator);
			AssertEquals("ShipmentReferenceNumber", "SH00023424", messageBlock.ShipmentReferenceNumber);
			AssertEquals("ConveyanceNameCarrierName", "APL VESSEL", messageBlock.ConveyanceNameCarrierName);
			AssertEquals("FilingOptionIndicator", "3", messageBlock.FilingOptionIndicator);
			AssertEquals("AEIFilingType", "F", messageBlock.AEIFilingType);
			AssertEquals("PortOfUnladingCode", "AUSYD", messageBlock.PortOfUnladingCode);
			AssertEquals("PortOfExportationCode", "USLAX", messageBlock.PortOfExportationCode);
			AssertEquals("EstimatedDateOfExport", new ZDate(2009, 12, 15), messageBlock.EstimatedDateOfExport);
			AssertEquals("HazardousMaterialIndicatorHAZMAT", "N", messageBlock.HazardousMaterialIndicatorHAZMAT);
		}

		void AssertSC2Record(AESCommShipSC2XP messageBlock)
		{
			AssertEquals("InbondCode", "37", messageBlock.InbondCode);
			AssertEquals("EntryNumber", "XTN45665", messageBlock.EntryNumber);
			AssertEquals("ForeignTradeZoneIdentifier", "123FTZ123", messageBlock.ForeignTradeZoneIdentifier);
			AssertEquals("RoutedExportTransactionIndicator", "Y", messageBlock.RoutedExportTransactionIndicator);
			AssertEquals("OriginalITN", "X20170623123456", messageBlock.OriginalITN);
		}

		void AssertSC3Record(AESCommShipSC3XP messageBlock, ZString equipmentNumber, ZString sealNumber, ZString transportationReferenceNumber)
		{
			AssertEquals("EquipmentNumber", equipmentNumber, messageBlock.EquipmentNumber);
			AssertEquals("SealNumber", sealNumber, messageBlock.SealNumber);
			AssertEquals("TransportationReferenceNumber", transportationReferenceNumber, messageBlock.TransportationReferenceNumber);
		}

		void AssertSoldEnRoute(AESCommShipN01XP n01xp, AESCommShipN02XP n02xp, AESCommShipN03XP n03xp, ZString cityOfFirstPortOfCall, ZString countryOfFirstPortOfCall)
		{
			AssertEquals("PartyType", AESTIRPartyTypeList.Codes.UltimateConsignee, n01xp.PartyType);
			AssertEquals("PartyID", "", n01xp.PartyID);
			AssertEquals("PartyIDType", "", n01xp.PartyIDType);
			AssertEquals("PartyName", AESConstants.SoldEnRouteName, n01xp.PartyName);
			AssertEquals("ContactFirstName", "", n01xp.ContactFirstName);
			AssertEquals("ContactLastName", "", n01xp.ContactLastName);
			AssertEquals("ToBeSoldEnRouteIndicator", "Y", n01xp.ToBeSoldEnRouteIndicator);

			AssertEquals("AddressLine1", "", n02xp.AddressLine1);
			AssertEquals("AddressLine2", "", n02xp.AddressLine2);
			AssertEquals("ContactPhoneNumber", "", n02xp.ContactPhoneNumber);

			AssertEquals("City", cityOfFirstPortOfCall, n03xp.City);
			AssertEquals("StateCode", "", n03xp.StateCode);
			AssertEquals("CountryCode", countryOfFirstPortOfCall, n03xp.CountryCode);
			AssertEquals("PostalCode", "", n03xp.PostalCode);
			AssertEquals("USPPIIRSIDType", "", n03xp.USPPIIRSIDType);
			AssertEquals("USPPIIRSNumber", "", n03xp.USPPIIRSNumber);
		}

		void AssertAESParty(AESCommShipN01XP n01xp, AESCommShipN02XP n02xp, AESCommShipN03XP n03xp, ZString partyType, ZString uSPPIIRSIDType, ZString uSPPIIRSNumber, ZString toBeSoldEnRouteIndicator, ZString ultimateConsigneeType)
		{
			AssertEquals("PartyType", partyType, n01xp.PartyType);
			AssertEquals("PartyID", partyType + "12345678", n01xp.PartyID);
			AssertEquals("PartyIDType", partyType, n01xp.PartyIDType);
			AssertEquals("PartyName", partyType + "NAME", n01xp.PartyName);
			AssertEquals("ContactFirstName", partyType + "FIRST NAME", n01xp.ContactFirstName);
			AssertEquals("ContactLastName", partyType + "LAST NAME", n01xp.ContactLastName);
			AssertEquals("ToBeSoldEnRouteIndicator", toBeSoldEnRouteIndicator, n01xp.ToBeSoldEnRouteIndicator);

			var prefix = partyType == AESTIRPartyTypeList.Codes.USPPI ? (ZString)"P" : partyType;

			AssertEquals("AddressLine1", prefix + "ADDRESS 1", n02xp.AddressLine1);
			AssertEquals("AddressLine2", prefix + "ADDRESS 2", n02xp.AddressLine2);
			AssertEquals("ContactPhoneNumber", partyType + "123456789", n02xp.ContactPhoneNumber);

			AssertEquals("City", prefix + " CITY", n03xp.City);
			AssertEquals("StateCode", prefix + "S", n03xp.StateCode);
			AssertEquals("CountryCode", prefix + "C", n03xp.CountryCode);
			AssertEquals("PostalCode", prefix + "12345678", n03xp.PostalCode);
			AssertEquals(ultimateConsigneeType, n03xp.UltimateConsigneeType);
			AssertEquals("USPPIIRSIDType", uSPPIIRSIDType, n03xp.USPPIIRSIDType);
			AssertEquals("USPPIIRSNumber", uSPPIIRSNumber, n03xp.USPPIIRSNumber);
		}

		void AssertCommodityLine(ZInt lineNumber, AESCommShipCL1XP cl1xp, AESCommShipCL2XP cl2xp, AESCommShipODTXP odtxp, AESCommShipEV1XP ev1xp1, AESCommShipEV1XP ev1xp2)
		{
			AssertEquals("ExportInformationCode", ExportInformationCodeList.Codes.CH, cl1xp.ExportInformationCode);
			AssertEquals("LineNumber", lineNumber, cl1xp.LineNumber);
			AssertEquals("CommodityDescription", "DESCRIPTION", cl1xp.CommodityDescription);
			AssertEquals("LicenseCodeLicenseExemptionCode", USAESLicenseCode.Codes.C30, cl1xp.LicenseCodeLicenseExemptionCode);
			AssertEquals("ForeignDomesticOriginIndicator", AESOriginIndicatorList.Codes.Domestic, cl1xp.ForeignDomesticOriginIndicator);

			AssertEquals("ScheduleBHTSNumber", "1010101010", cl2xp.ScheduleBHTSNumber);
			AssertEquals("UnitOfMeasure1", AESUnitOfMeasureList.Codes.Barrels, cl2xp.UnitOfMeasure1);
			AssertEquals("Quantity1", 10m, cl2xp.Quantity1);
			AssertEquals("ValueOfGoods", 1500m, cl2xp.ValueOfGoods);
			AssertEquals("UnitOfMeasure2", AESUnitOfMeasureList.Codes.Carats, cl2xp.UnitOfMeasure2);
			AssertEquals("Quantity2", 20.50m, cl2xp.Quantity2);
			AssertEquals("ShippingWeight", 130m, cl2xp.ShippingWeight);
			AssertEquals("ExportControlClassificationNumberECCN", "ECCN", cl2xp.ExportControlClassificationNumberECCN);
			AssertEquals("ExportLicenseNumberCFRCitationAuthorizationSymbolKCP", "LC2342", cl2xp.ExportLicenseNumberCFRCitationAuthorizationSymbolKCPACM);

			if (odtxp != null)
			{
				AssertEquals("DDTCITARExemptionNumber", "DTCEN1234", odtxp.DDTCITARExemptionNumber);
				AssertEquals("DDTCRegistrationNumber", "DTCRN1234", odtxp.DDTCRegistrationNumber);
				AssertEquals("DDTCSignificantMilitaryEquipmentSMEIndicator", "Y", odtxp.DDTCSignificantMilitaryEquipmentSMEIndicator);
				AssertEquals("DDTCEligiblePartyCertificationIndicator", "N", odtxp.DDTCEligiblePartyCertificationIndicator);
				AssertEquals("DDTCUSMLCategoryCode", "123.11B", odtxp.DDTCUSMLCategoryCode);
				AssertEquals("DDTCUnitOfMeasureCode", DDTCUnitOfMeasureList.Codes.Bags, odtxp.DDTCUnitOfMeasureCode);
				AssertEquals("DDTCQuantity", 53m, odtxp.DDTCQuantity);
				AssertEquals("DDTCCategoryXXIDeterminationNumber", "CJ1234567", odtxp.DDTCCategoryXXIDeterminationNumber);
			}

			if (ev1xp1 != null)
			{
				AssertEV1(ev1xp1, VehicleIDTypeList.Codes.VIN);
			}

			if (ev1xp2 != null)
			{
				AssertEV1(ev1xp2, VehicleIDTypeList.Codes.ProductID);
			}
		}

		void AssertEV1(AESCommShipEV1XP ev1xp, ZString vehicleID)
		{
			AssertEquals("VehicleIdentificationNumberVINProductID", "VIN123", ev1xp.VehicleIdentificationNumberVINProductID);
			AssertEquals("VehicleIDQualifier", vehicleID, ev1xp.VehicleIDQualifier);
			AssertEquals("VehicleTitleNumber", "TITLE", ev1xp.VehicleTitleNumber);
			AssertEquals("VehicleTitleStateCode", "CA", ev1xp.VehicleTitleStateCode);
		}

		Mock<IAESTIRMessageAttachee> GetNewAttacheeMock(bool includeTransportDetail) => GetNewAttacheeMock(includeTransportDetail, new ZDate(2009, 12, 15), "AU");

		Mock<IAESTIRMessageAttachee> GetNewAttacheeMock(bool includeTransportDetail, ZDateTime dateOfExport, string destinationCode)
		{
			var attacheeMock = new Mock<IAESTIRMessageAttachee>();

			// SC1 Record
			attacheeMock.Setup(m => m.RelatedCompanyIndicator).Returns("Y");
			attacheeMock.Setup(m => m.ModeOfTransportationCodeMOT).Returns("10");
			attacheeMock.Setup(m => m.CountryOfUltimateDestinationCode).Returns(destinationCode);
			attacheeMock.Setup(m => m.USStateOfOriginCode).Returns("CA");
			attacheeMock.Setup(m => m.CarrierIDSCACIATA).Returns("ACAC");
			attacheeMock.Setup(m => m.ShipmentReferenceNumber).Returns("SH00023424");
			attacheeMock.Setup(m => m.ConveyanceNameCarrierName).Returns("APL VESSEL");
			attacheeMock.Setup(m => m.FilingOptionIndicator).Returns("3");
			attacheeMock.Setup(m => m.AEIFilingType).Returns("F");
			attacheeMock.Setup(m => m.PortOfUnladingCode).Returns("AUSYD");
			attacheeMock.Setup(m => m.PortOfExportationCode).Returns("USLAX");
			attacheeMock.Setup(m => m.EstimatedDateOfExport).Returns(new ZDate(dateOfExport));
			attacheeMock.Setup(m => m.HazardousMaterialIndicatorHAZMAT).Returns("N");
			attacheeMock.Setup(m => m.Factory).Returns(Factory);

			// SC2 Record
			attacheeMock.Setup(m => m.InbondCode).Returns("37");
			attacheeMock.Setup(m => m.EntryNumber).Returns("XTN45665");
			attacheeMock.Setup(m => m.ForeignTradeZoneIdentifier).Returns("123FTZ123");
			attacheeMock.Setup(m => m.RoutedExportTransactionIndicator).Returns("Y");
			attacheeMock.Setup(m => m.OriginalITN).Returns("X20170623123456");

			// SC3 Records
			var transportationDetails = new List<IAESTIRTransportationDetail>();
			var transportationDetailMock1 = new Mock<IAESTIRTransportationDetail>();
			transportationDetailMock1.Setup(m => m.EquipmentNumber).Returns("");
			transportationDetailMock1.Setup(m => m.SealNumber).Returns("");
			if (includeTransportDetail)
			{
				transportationDetailMock1.Setup(m => m.TransportationReferenceNumber).Returns("TRE1231231");
			}
			else
			{
				transportationDetailMock1.Setup(m => m.TransportationReferenceNumber).Returns("");
			}
			transportationDetails.Add(transportationDetailMock1.Object);

			var transportationDetailMock2 = new Mock<IAESTIRTransportationDetail>();
			if (includeTransportDetail)
			{
				transportationDetailMock2.Setup(m => m.EquipmentNumber).Returns("CTRU1235455");
				transportationDetailMock2.Setup(m => m.SealNumber).Returns("5694");
			}
			else
			{
				transportationDetailMock2.Setup(m => m.EquipmentNumber).Returns("");
				transportationDetailMock2.Setup(m => m.SealNumber).Returns("");
			}
			transportationDetailMock2.Setup(m => m.TransportationReferenceNumber).Returns("");
			transportationDetails.Add(transportationDetailMock2.Object);

			attacheeMock.Setup(m => m.TransportationDetails).Returns(transportationDetails);

			// Parties
			var uSPPIMock = GetNewAESParty(AESTIRPartyTypeList.Codes.USPPI);
			attacheeMock.Setup(m => m.USPPI).Returns(uSPPIMock.Object);
			attacheeMock.Setup(m => m.USPPIIRSNumber).Returns("123456789");
			attacheeMock.Setup(m => m.USPPIIRSIDType).Returns("E");

			var forwardingAgentMock = GetNewAESParty(AESTIRPartyTypeList.Codes.ForwardingAgent);
			attacheeMock.Setup(m => m.ForwardingAgent).Returns(forwardingAgentMock.Object);

			var ultimateConsigneeMock = GetNewAESParty(AESTIRPartyTypeList.Codes.UltimateConsignee, destinationCode);
			attacheeMock.Setup(m => m.UltimateConsignee).Returns(ultimateConsigneeMock.Object);
			attacheeMock.Setup(m => m.IsSoldEnRoute).Returns(ZBool.False);
			attacheeMock.Setup(m => m.CityOfFirstPortOfCall).Returns("SYDNEY");
			attacheeMock.Setup(m => m.CountryOfFirstPortOfCall).Returns("AUSTRALIA");
			attacheeMock.Setup(m => m.UltimateConsigneeType).Returns(UltimateConsigneeTypeList.Codes.GovernmentEntity);

			var intermediateConsigneeMock = GetNewAESParty(AESTIRPartyTypeList.Codes.IntermediateConsignee);
			attacheeMock.Setup(m => m.IntermediateConsignee).Returns(intermediateConsigneeMock.Object);

			// Commodity Line Items
			var commodityLineItems = new List<IAESTIRCommodityLineItem>();
			commodityLineItems.Add(GetNewCommodityLine(1, true, true).Object);
			commodityLineItems.Add(GetNewCommodityLine(2, true, true).Object);
			attacheeMock.Setup(m => m.CommodityLineItems).Returns(commodityLineItems);
			return attacheeMock;
		}

		Mock<IAESTIRParty> GetNewAESParty(ZString prefix, string countryCode = "AU")
		{
			var partyMock = new Mock<IAESTIRParty>();
			var addressPrefix = prefix == AESTIRPartyTypeList.Codes.USPPI ? (ZString)"P" : prefix;

			// N01 Record
			partyMock.Setup(m => m.PartyID).Returns(prefix + "12345678");
			partyMock.Setup(m => m.PartyIDType).Returns(prefix);
			partyMock.Setup(m => m.PartyName).Returns(prefix + "NAME");
			partyMock.Setup(m => m.ContactFirstName).Returns(prefix + "FIRST NAME");
			partyMock.Setup(m => m.ContactLastName).Returns(prefix + "LAST NAME");

			// N02 Record
			partyMock.Setup(m => m.AddressLine1).Returns(addressPrefix + "ADDRESS 1");
			partyMock.Setup(m => m.AddressLine2).Returns(addressPrefix + "ADDRESS 2");
			partyMock.Setup(m => m.ContactPhoneNumber).Returns(prefix + "123456789");

			// N03 Record
			partyMock.Setup(m => m.City).Returns(addressPrefix + " CITY");
			partyMock.Setup(m => m.StateCode).Returns(addressPrefix + "S");
			partyMock.Setup(m => m.CountryCode).Returns(countryCode == "AU" ? (addressPrefix + "C") : countryCode);
			partyMock.Setup(m => m.PostalCode).Returns(addressPrefix + "12345678");
			return partyMock;
		}

		Mock<IAESTIRCommodityLineItem> GetNewCommodityLine(ZInt lineNumber, bool includeDDTC, bool includeUseVehicles)
		{
			var lineMock = new Mock<IAESTIRCommodityLineItem>();
			// CL1 Record
			lineMock.Setup(m => m.ExportInformationCode).Returns(ExportInformationCodeList.Codes.CH);
			lineMock.Setup(m => m.LineNumber).Returns(lineNumber);
			lineMock.Setup(m => m.CommodityDescription).Returns("DESCRIPTION");
			lineMock.Setup(m => m.LicenseCodeLicenseExemptionCode).Returns(USAESLicenseCode.Codes.C30);
			lineMock.Setup(m => m.ForeignDomesticOriginIndicator).Returns(AESOriginIndicatorList.Codes.Domestic);
			lineMock.Setup(m => m.LicenseValue).Returns(980m);
			lineMock.Setup(m => m.AMSIndicator).Returns(OGAIndicatorList.Codes.Declared);
			lineMock.Setup(m => m.EPAIndicator).Returns(ZString.Empty);
			lineMock.Setup(m => m.NMFSIndicator).Returns(ZString.Empty);
			lineMock.Setup(m => m.ATFIndicator).Returns(ZString.Empty);
			lineMock.Setup(m => m.DEAIndicator).Returns(ZString.Empty);
			lineMock.Setup(m => m.FWSIndicator).Returns(ZString.Empty);
			lineMock.Setup(m => m.TTBIndicator).Returns(ZString.Empty);

			var exportAMSMock = new Mock<IAESAMS>();
			exportAMSMock.Setup(m => m.ExportCertificateNo).Returns("ABC1234567");
			lineMock.Setup(m => m.ExportAMS).Returns(exportAMSMock.Object);

			// CL2 Record
			lineMock.Setup(m => m.ScheduleBHTSNumber).Returns("1010101010");
			lineMock.Setup(m => m.UnitOfMeasure1).Returns(AESUnitOfMeasureList.Codes.Barrels);
			lineMock.Setup(m => m.Quantity1).Returns(10m);
			lineMock.Setup(m => m.ValueOfGoods).Returns(1500m);
			lineMock.Setup(m => m.UnitOfMeasure2).Returns(AESUnitOfMeasureList.Codes.Carats);
			lineMock.Setup(m => m.Quantity2).Returns(20.50m);
			lineMock.Setup(m => m.ShippingWeight).Returns(130m);
			lineMock.Setup(m => m.ExportControlClassificationNumberECCN).Returns("ECCN");
			lineMock.Setup(m => m.ExportLicenseNumberCFRCitationAuthorizationSymbolKCP).Returns("LC2342");

			if (includeDDTC)
			{
				// ODT Record
				lineMock.Setup(m => m.DDTCITARExemptionNumber).Returns("DTCEN1234");
				lineMock.Setup(m => m.DDTCRegistrationNumber).Returns("DTCRN1234");
				lineMock.Setup(m => m.DDTCSignificantMilitaryEquipmentSMEIndicator).Returns("Y");
				lineMock.Setup(m => m.DDTCEligiblePartyCertificationIndicator).Returns("N");
				lineMock.Setup(m => m.DDTCUSMLCategoryCode).Returns("123.11B");
				lineMock.Setup(m => m.DDTCUnitOfMeasureCode).Returns(DDTCUnitOfMeasureList.Codes.Bags);
				lineMock.Setup(m => m.DDTCQuantity).Returns(53m);
				lineMock.Setup(m => m.DDTCCommodityJurisdictionNumber).Returns("CJ1234567");
			}
			else
			{
				// ODT Record
				lineMock.Setup(m => m.DDTCITARExemptionNumber).Returns("");
				lineMock.Setup(m => m.DDTCRegistrationNumber).Returns("");
				lineMock.Setup(m => m.DDTCSignificantMilitaryEquipmentSMEIndicator).Returns("");
				lineMock.Setup(m => m.DDTCEligiblePartyCertificationIndicator).Returns("");
				lineMock.Setup(m => m.DDTCUSMLCategoryCode).Returns("");
				lineMock.Setup(m => m.DDTCUnitOfMeasureCode).Returns("");
				lineMock.Setup(m => m.DDTCQuantity).Returns(0m);
				lineMock.Setup(m => m.DDTCCommodityJurisdictionNumber).Returns("");
			}

			if (includeUseVehicles)
			{
				// Used Vehicles
				List<IAESTIRUsedVehicle> usedVehicles = new List<IAESTIRUsedVehicle>();
				usedVehicles.Add(GetNewUseVehicle(VehicleIDTypeList.Codes.VIN).Object);
				usedVehicles.Add(GetNewUseVehicle(VehicleIDTypeList.Codes.ProductID).Object);
				lineMock.Setup(m => m.UsedVehicles).Returns(usedVehicles);
			}

			return lineMock;
		}

		Mock<IAESTIRUsedVehicle> GetNewUseVehicle(ZString vehicleID)
		{
			var useVehicleMock = new Mock<IAESTIRUsedVehicle>();
			// EV1 Record
			useVehicleMock.Setup(m => m.VehicleIdentificationNumberVINProductID).Returns("VIN123");
			useVehicleMock.Setup(m => m.VehicleIDQualifier).Returns(vehicleID);
			useVehicleMock.Setup(m => m.VehicleTitleNumber).Returns("TITLE");
			useVehicleMock.Setup(m => m.VehicleTitleStateCode).Returns("CA");

			return useVehicleMock;
		}
	}
}
