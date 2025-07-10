using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class COODECTest : TestCaseWithFactory
	{
		public void TestGenerateDeclarantElements()
		{
			var testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test SG Broker";
			testDeclarant.Phone = "64 85721111";
			testDeclarant.Code = "v13t001";
			DataProvider.Declarant = testDeclarant;
			Message.BuildHeader(COODEC);
			Message.BuildParty(COODEC);
			AssertEquals("DeclarationIndicator", true, COODEC.Header.DeclarationIndicator);
			AssertEquals("Declarant Code", "V13T001", COODEC.Party.DeclarantParty.PersonInformation.CodeValue);
			AssertEquals("Declarant Name", "TEST SG BROKER", COODEC.Party.DeclarantParty.PersonInformation.Name);
			AssertEquals("Declarant Phone No.", "64 85721111", COODEC.Party.DeclarantParty.Telephone);
		}

		public void TestDeclaringAgent()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Message.BuildParty(COODEC);
			AssertEquals("Declaring Agent Identification", "AAA374M", COODEC.Party.DeclaringAgentParty.PartyIdentification.ID);
			AssertEquals("Declaring Agent Name", "EAGLE DATAMATION INTERNATIONAL", COODEC.Party.DeclaringAgentParty.PartyName[0]);
		}

		public void TestExporterName()
		{
			var exporter = new OrganisationTestClass();
			exporter.UEN = "200102977K";
			exporter.Name = "HYDRO ALUMINIUM MALAYSIA SDN BHD C/O OIA GLOBAL LOGISTICS (S) PTE LTD";
			DataProvider.Exporter = exporter;
			Message.BuildParty(COODEC);
			AssertEquals("Exporter Party Identification", "200102977K", COODEC.Party.ExporterParty.PartyDetail.PartyIdentification.ID);
			AssertEquals("Exporter Name (line1)", "HYDRO ALUMINIUM MALAYSIA SDN BHD C/", COODEC.Party.ExporterParty.PartyDetail.PartyName[0]);
			AssertEquals("Exporter Name (line2)", "O OIA GLOBAL LOGISTICS (S) PTE LTD", COODEC.Party.ExporterParty.PartyDetail.PartyName[1]);
		}

		public void TestCOAddressExcludesAdditionalAddressInformation()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "HEPL";
			consignee.OH_FullName = "HANDY ENTERPRISES PTE. LTD.";
			var mainAddress = consignee.Addresses[0];
			mainAddress.OA_Address1 = "AVENUE KING CHARLES II";
			mainAddress.OA_Address2 = "";
			mainAddress.PrimaryOrgAddressAdditionalInfoDetail = "C/- JOHN SMITH, FIRST FLOOR";
			mainAddress.OA_City = "SINGAPORE";
			mainAddress.OA_PostCode = "59200";
			Factory.Save();
			DataProvider.Consignee = new EntryOrganisationsInfo(consignee);
			Message.BuildParty(COODEC);
			var consigneeParty = Message.CusDec.Consignee;
			AssertEquals("HANDY ENTERPRISES PTE. LTD.", consigneeParty.Name);
			AssertEquals("C/- JOHN SMITH, FIRST FLOOR AVENUE KING CHARLES II SINGAPORE 59200", consigneeParty.Address.FullAddress);
			AssertEquals("Exporter message address (line1)", "C/- JOHN SMITH, FIRST FLOOR AVENUE", COODEC.Party.ConsigneeParty.AddressLine[0]);
			AssertEquals("Exporter message address (line2)", "KING CHARLES II SINGAPORE 59200", COODEC.Party.ConsigneeParty.AddressLine[1]);
			mainAddress.OA_Address1 = "AVENUE KING CHARLES II, SPECIAL DIPLOMATIC AREA";
			mainAddress.OA_Address2 = "SOUTH CENTRAL HARBOURSIDE";
			Factory.Save();
			Message.BuildParty(COODEC);
			consigneeParty = Message.CusDec.Consignee;
			AssertEquals("HANDY ENTERPRISES PTE. LTD.", consigneeParty.Name);
			AssertEquals("Long address should not include Additional Address Information when Address exceeds message capacity", "AVENUE KING CHARLES II, SPECIAL DIPLOMATIC AREA SOUTH CENTRAL HARBOURSIDE SINGAPORE 59200", consigneeParty.Address.FullAddressWithoutAdditionalInfo);
			AssertEquals("Exporter message address (line1)", "AVENUE KING CHARLES II, SPECIAL", COODEC.Party.ConsigneeParty.AddressLine[0]);
			AssertEquals("Exporter message address (line2)", "DIPLOMATIC AREA SOUTH CENTRAL", COODEC.Party.ConsigneeParty.AddressLine[1]);
			AssertEquals("Exporter message address (line3)", "HARBOURSIDE SINGAPORE 59200", COODEC.Party.ConsigneeParty.AddressLine[2]);
		}

		public void TestCountryOfFinalDestination()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.IsForStorage = true;
			DataProvider.CountryOfFinalDestination = "HK";
			Message.BuildTransport(COODEC);
			var outwardTransport = COODEC.Transport.OutwardTransport;
			AssertEquals("Country of Final Dest. is mandatory for outward transport", "HK", outwardTransport.FinalDestinationCountry);
		}

		public void TestCertificateSendsSGD()
		{
			SetCOTestData(DataProvider);
			DataProvider.CertificateType1 = "1";
			DataProvider.CurrencyCode = "SGD";
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = "15 Packages";
			DataProvider.CertItems = items;
			Message.BuildCertificate(COODEC);
			var certificateCurrency = COODEC.Certificate;
			AssertEquals("Certificate Currency", "SGD", certificateCurrency.CurrencyCode);
		}

		public void TestSeaTransportElements()
		{
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardTransportIdentifier = "OV010101";
			DataProvider.OutwardJourneyIdentifier = "V55";
			DataProvider.OutwardVesselNationality = "SG";
			Message.BuildTransport(COODEC);
			var outwardTransport = COODEC.Transport.OutwardTransport;
			AssertEquals(1, outwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("V55", outwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
			AssertEquals("OV010101", outwardTransport.TransportMeans.TransportMode.TransportIdentifier);
		}

		public void TestAirTransportElements()
		{
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardJourneyIdentifier = "SQ190";
			Message.BuildTransport(COODEC);
			var outwardTransport = COODEC.Transport.OutwardTransport;
			AssertEquals(4, outwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("SQ190", outwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
		}

		public void TestGenerateAttachmentElements()
		{
			AttachmentsTestClass[] attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = "ATTDOC_0001";
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = "ATTDOC_0002";
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType999;
			SGCUSDECTestClass.ImplementsAdditionalMessageInformation addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			DataProvider.OutwardMasterBill = "5555";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			Message.BuildSupportingDocumentReference(COODEC);
			var supportingDocuments = COODEC.SupportingDocumentReference;
			AssertEquals("2 attached document elements should have been generated", 2, supportingDocuments.Length);
			AssertEquals("DocType001", "001", supportingDocuments[0].DocumentID);
			AssertEquals("Attached document file name", "ATTDOC_0001", supportingDocuments[0].Filename);
			AssertEquals("DocType999", "999", supportingDocuments[1].DocumentID);
			AssertEquals("Attached document file name", "ATTDOC_0002", supportingDocuments[1].Filename);
		}

		public void TestMessageTypeAndSubType()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.COODEC, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, Message.MessageSubType);
		}

		public void TestCertificateItemWhereCarriageReturnUsed()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = @"(505 CTNS) CONTAINING 8059 CPS OF
PRINTED BOOKS
""CIF LE HAVRE""
01 TITLE: WHAT KNOT? (EQUMWHKT) - 1ST FEBRUARY 2008
02 TITLE: WHAT KNOT? (EQUMWHKT) - 1ST FEBRUARY 2008
(LOGISTA FRANCE)
03 TITLE: WHAT KNOT? (EQUMWHKT) - 1ST FEBRUARY 2008
(HACHETTE-LIVRE)
DELIVERY ADDRESS:
___________
CEPL SOLADIS
RUE DE COURCY
27380 FLEURY LES ANDELLES
FRANCE
CONTACT:M.LEVRAULT
TEL: +33(0) 1 30 662288
EMAIL:RDVRECEPTION@HACHETTE.FR
NOTIFY PARTY:
GEODIS OVERSEAS
9 RUE FERRER
BP 1345, 76065 LE HAVRE CEDEX
FRANCE CONTACT:ROZEN GODEY
TEL: +33(0)235 538929
FAX: +33(0)235 538925
CONSIGNEE EMAIL: PCOULAND@HACHETTE-LIVRE.FR";
			DataProvider.Items = items;
			DataProvider.CertItems = items;
			DataProvider.CertificateType1 = "1";
			Message.BuildItem(COODEC);
			var certItem = COODEC.Item[0].ItemCertificate;
			AssertEquals("ItemCertificateDescription - line 1", "(505 CTNS) CONTAINING 8059 CPS OF", certItem.ItemCertificateDescription[0].Line[0]);
			AssertEquals("ItemCertificateDescription - line 2", "PRINTED BOOKS", certItem.ItemCertificateDescription[0].Line[1]);
			AssertEquals("ItemCertificateDescription - line 3", "\"CIF LE HAVRE\"", certItem.ItemCertificateDescription[0].Line[2]);
			AssertEquals("ItemCertificateDescription - line 4", "01 TITLE: WHAT KNOT? (EQUMWHKT) -", certItem.ItemCertificateDescription[0].Line[3]);
			AssertEquals("ItemCertificateDescription - line 5", "1ST FEBRUARY 2008", certItem.ItemCertificateDescription[0].Line[4]);
			AssertEquals("ItemCertificateDescription - line 6", "02 TITLE: WHAT KNOT? (EQUMWHKT) -", certItem.ItemCertificateDescription[1].Line[0]);
			AssertEquals("ItemCertificateDescription - line 7", "1ST FEBRUARY 2008", certItem.ItemCertificateDescription[1].Line[1]);
			AssertEquals("ItemCertificateDescription - line 8", "(LOGISTA FRANCE)", certItem.ItemCertificateDescription[1].Line[2]);
			AssertEquals("ItemCertificateDescription - line 9", "03 TITLE: WHAT KNOT? (EQUMWHKT) -", certItem.ItemCertificateDescription[1].Line[3]);
			AssertEquals("ItemCertificateDescription - line 10", "1ST FEBRUARY 2008", certItem.ItemCertificateDescription[1].Line[4]);
			AssertEquals("ItemCertificateDescription - line 11", "(HACHETTE-LIVRE)", certItem.ItemCertificateDescription[2].Line[0]);
			AssertEquals("ItemCertificateDescription - line 12", "DELIVERY ADDRESS:", certItem.ItemCertificateDescription[2].Line[1]);
			AssertEquals("ItemCertificateDescription - line 13", "___________", certItem.ItemCertificateDescription[2].Line[2]);
			AssertEquals("ItemCertificateDescription - line 14", "CEPL SOLADIS", certItem.ItemCertificateDescription[2].Line[3]);
			AssertEquals("ItemCertificateDescription - line 15", "RUE DE COURCY", certItem.ItemCertificateDescription[2].Line[4]);
			AssertEquals("ItemCertificateDescription - line 16", "27380 FLEURY LES ANDELLES", certItem.ItemCertificateDescription[3].Line[0]);
			AssertEquals("ItemCertificateDescription - line 17", "FRANCE", certItem.ItemCertificateDescription[3].Line[1]);
			AssertEquals("ItemCertificateDescription - line 18", "CONTACT:M.LEVRAULT", certItem.ItemCertificateDescription[3].Line[2]);
			AssertEquals("ItemCertificateDescription - line 19", "TEL: +33(0) 1 30 662288", certItem.ItemCertificateDescription[3].Line[3]);
			AssertEquals("ItemCertificateDescription - line 20", "EMAIL:RDVRECEPTION@HACHETTE.FR", certItem.ItemCertificateDescription[3].Line[4]);
			AssertEquals("ItemCertificateDescription - line 21", "NOTIFY PARTY:", certItem.ItemCertificateDescription[4].Line[0]);
			AssertEquals("ItemCertificateDescription - line 22", "GEODIS OVERSEAS", certItem.ItemCertificateDescription[4].Line[1]);
			AssertEquals("ItemCertificateDescription - line 23", "9 RUE FERRER", certItem.ItemCertificateDescription[4].Line[2]);
			AssertEquals("ItemCertificateDescription - line 24", "BP 1345, 76065 LE HAVRE CEDEX", certItem.ItemCertificateDescription[4].Line[3]);
			AssertEquals("ItemCertificateDescription - line 25", "FRANCE CONTACT:ROZEN GODEY", certItem.ItemCertificateDescription[4].Line[4]);
			AssertEquals("ItemCertificateDescription - line 26", "TEL: +33(0)235 538929", certItem.ItemCertificateDescription[5].Line[0]);
			AssertEquals("ItemCertificateDescription - line 27", "FAX: +33(0)235 538925", certItem.ItemCertificateDescription[5].Line[1]);
			AssertEquals("ItemCertificateDescription - line 28", "CONSIGNEE EMAIL:", certItem.ItemCertificateDescription[5].Line[2]);
			AssertEquals("ItemCertificateDescription - line 29", "PCOULAND@HACHETTE-LIVRE.FR", certItem.ItemCertificateDescription[5].Line[3]);
		}

		public void TestItemDescRevertsToSimpleSplittingIfCannotBeFormatted()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = @"PURE BRED BREEDING HORSES ASSES MULES & HINNIES (NMB)
ADD SOME FOMATTING
ADD SOME MORE TEXT

=====================

AND DO SOME MORE STUFF AND DO SOME MORE STUFF AND DO SOME MORE STUFF

AND MORE

AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND
AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REREST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE THIS IS THE LAST PIECE OF TEXT";
			DataProvider.Items = items;
			DataProvider.CertItems = items;
			DataProvider.CertificateType1 = "1";
			Message.BuildItem(COODEC);
			var certItem = COODEC.Item[0].ItemCertificate;
			AssertEquals("ItemCertificateDescription - line 1", "PURE BRED BREEDING HORSES ASSES MUL", certItem.ItemCertificateDescription[0].Line[0]);
			AssertEquals("ItemCertificateDescription - line 2", "ES & HINNIES (NMB) ADD SOME FOMATTI", certItem.ItemCertificateDescription[0].Line[1]);
			AssertEquals("ItemCertificateDescription - line 3", "NG ADD SOME MORE TEXT  ============", certItem.ItemCertificateDescription[0].Line[2]);
			AssertEquals("ItemCertificateDescription - line 4", "=========  AND DO SOME MORE STUFF A", certItem.ItemCertificateDescription[0].Line[3]);
			AssertEquals("ItemCertificateDescription - line 5", "ND DO SOME MORE STUFF AND DO SOME M", certItem.ItemCertificateDescription[0].Line[4]);
			AssertEquals("ItemCertificateDescription - line 6", "ORE STUFF  AND MORE  AND THEN USE U", certItem.ItemCertificateDescription[1].Line[0]);
			AssertEquals("ItemCertificateDescription - line 7", "P THE REST OF THE CHARS AND THEN US", certItem.ItemCertificateDescription[1].Line[1]);
			AssertEquals("ItemCertificateDescription - line 8", "E UP THE REST OF THE CHARS AND AND ", certItem.ItemCertificateDescription[1].Line[2]);
			AssertEquals("ItemCertificateDescription - line 9", "THEN USE UP THE REST OF THE CHARS A", certItem.ItemCertificateDescription[1].Line[3]);
			AssertEquals("ItemCertificateDescription - line 10", "ND THEN USE UP THE REST OF THE CHAR", certItem.ItemCertificateDescription[1].Line[4]);
			AssertEquals("ItemCertificateDescription - line 11", "S AND AAAAAAAAAAAAAAAAAAAAAAAAAAAAA", certItem.ItemCertificateDescription[2].Line[0]);
			AssertEquals("ItemCertificateDescription - line 12", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", certItem.ItemCertificateDescription[2].Line[1]);
			AssertEquals("ItemCertificateDescription - line 13", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", certItem.ItemCertificateDescription[2].Line[2]);
			AssertEquals("ItemCertificateDescription - line 14", "AA AND THEN USE UP THE REST OF THE ", certItem.ItemCertificateDescription[2].Line[3]);
			AssertEquals("ItemCertificateDescription - line 15", "CHARS AND THEN USE UP THE REST OF T", certItem.ItemCertificateDescription[2].Line[4]);
			AssertEquals("ItemCertificateDescription - line 16", "HE CHARS AND THEN USE UP THE REST O", certItem.ItemCertificateDescription[3].Line[0]);
			AssertEquals("ItemCertificateDescription - line 17", "F THE CHARS AND THEN USE UP THE RES", certItem.ItemCertificateDescription[3].Line[1]);
			AssertEquals("ItemCertificateDescription - line 18", "T OF THE CHARS AND THEN USE UP THE ", certItem.ItemCertificateDescription[3].Line[2]);
			AssertEquals("ItemCertificateDescription - line 19", "REST OF THE CHARS AND THEN USE UP T", certItem.ItemCertificateDescription[3].Line[3]);
			AssertEquals("ItemCertificateDescription - line 20", "HE REST OF THE CHARS AND THEN USE U", certItem.ItemCertificateDescription[3].Line[4]);
			AssertEquals("ItemCertificateDescription - line 21", "P THE REST OF THE CHARS AND THEN US", certItem.ItemCertificateDescription[4].Line[0]);
			AssertEquals("ItemCertificateDescription - line 22", "E UP THE REST OF THE CHARS AND THEN", certItem.ItemCertificateDescription[4].Line[1]);
			AssertEquals("ItemCertificateDescription - line 23", " USE UP THE REST OF THE CHARS AND T", certItem.ItemCertificateDescription[4].Line[2]);
			AssertEquals("ItemCertificateDescription - line 24", "HEN USE UP THE REST OF THE CHARS AN", certItem.ItemCertificateDescription[4].Line[3]);
			AssertEquals("ItemCertificateDescription - line 25", "D THEN USE UP THE REST OF THE CHARS", certItem.ItemCertificateDescription[4].Line[4]);
			AssertEquals("ItemCertificateDescription - line 26", " AND THEN USE UP THE REST OF THE CH", certItem.ItemCertificateDescription[5].Line[0]);
			AssertEquals("ItemCertificateDescription - line 27", "ARS AND THEN USE UP THE REST OF THE", certItem.ItemCertificateDescription[5].Line[1]);
			AssertEquals("ItemCertificateDescription - line 28", " CHARS AND THEN USE UP THE REREST O", certItem.ItemCertificateDescription[5].Line[2]);
			AssertEquals("ItemCertificateDescription - line 29", "F THE CHARS AND THEN USE UP THE RES", certItem.ItemCertificateDescription[5].Line[3]);
			AssertEquals("ItemCertificateDescription - line 30", "T OF THE CHARS AND THEN USE UP THE ", certItem.ItemCertificateDescription[5].Line[4]);
			AssertEquals("ItemCertificateDescription - line 31", "REST OF THE CHARS AND THEN USE UP T", certItem.ItemCertificateDescription[6].Line[0]);
			AssertEquals("ItemCertificateDescription - line 32", "HE REST OF THE CHARS AND THEN USE U", certItem.ItemCertificateDescription[6].Line[1]);
			AssertEquals("ItemCertificateDescription - line 33", "P THE REST OF THE CHARS AND THEN US", certItem.ItemCertificateDescription[6].Line[2]);
			AssertEquals("ItemCertificateDescription - line 34", "E UP THE REST OF THE CHARS AND THEN", certItem.ItemCertificateDescription[6].Line[3]);
			AssertEquals("ItemCertificateDescription - line 35", " USE UP THE REST OF THE CHARS AND T", certItem.ItemCertificateDescription[6].Line[4]);
			AssertEquals("ItemCertificateDescription - line 36", "HEN USE UP THE REST OF THE CHARS AN", certItem.ItemCertificateDescription[7].Line[0]);
			AssertEquals("ItemCertificateDescription - line 37", "D THEN USE UP THE REST OF THE CHARS", certItem.ItemCertificateDescription[7].Line[1]);
			AssertEquals("ItemCertificateDescription - line 38", " AND THEN USE UP THE REST OF THE CH", certItem.ItemCertificateDescription[7].Line[2]);
			AssertEquals("ItemCertificateDescription - line 39", "ARS AND THEN USE UP THE REST OF THE", certItem.ItemCertificateDescription[7].Line[3]);
			AssertEquals("ItemCertificateDescription - line 40", " CHARS AND THEN USE UP THE REST OF ", certItem.ItemCertificateDescription[7].Line[4]);
			AssertEquals("ItemCertificateDescription - line 41", "THE CHARS AND THEN USE UP THE REST ", certItem.ItemCertificateDescription[8].Line[0]);
			AssertEquals("ItemCertificateDescription - line 42", "OF THE CHARS AND THEN USE UP THE RE", certItem.ItemCertificateDescription[8].Line[1]);
			AssertEquals("ItemCertificateDescription - line 43", "ST OF THE CHARS AND THEN USE UP THE", certItem.ItemCertificateDescription[8].Line[2]);
			AssertEquals("ItemCertificateDescription - line 44", " REST OF THE CHARS AND THEN USE UP ", certItem.ItemCertificateDescription[8].Line[3]);
			AssertEquals("ItemCertificateDescription - line 45", "THE REST OF THE CHARS AND THEN USE ", certItem.ItemCertificateDescription[8].Line[4]);
			AssertEquals("ItemCertificateDescription - line 46", "THIS IS THE LAST PIECE OF TEXT", certItem.ItemCertificateDescription[9].Line[0]);
		}

		public void TestItemDescRevertsToNonFormattedToFitAllData()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = @"THIS SPECIFICATION PROVIDES THE DEFINITION OF THE TRADE DECLARATION AND APPLICATION OF CERTIFICATE OF ORIGIN IE. TCODEC
(COODEC) MESSAGE TO BE USED IN ELECTRONIC DATA INTERCHANGE (EDI) BETWEEN TRADING PARTNERS INVOLVED IN ADMINISTRATION, COMMERCE
AND TRANSPORT.
TCODEC IS A LOCAL MESSAGE, PENDING SUBMISSION TO UN FOR APPROVAL AS A NEW UN STANDARD MESSAGE (UNSM).
THIS DOCUMENT SHALL BE USED AS A BASELINE FOR THE INTERFACE SOFTWARE DESIGN AND SHALL BE AGREED UPON BY REPRESENTATIVE FROM IE
SINGAPORE AND CRIMSONLOGIC PTE LTD.

THE DOCUMENT SHALL PROVIDE THE FOLLOWING PRINCIPLES AND DETAILS FOR THE FOLLOWING:
- CRITERIA FOR PASSING INFORMATION FROM TRADERS, FREIGHT FORWARDERS, CARGO AGENTS AND SHIPPING AGENTS TO IESGP, CA OR CHAMBERS.
- MESSAGE FORMAT.
THE SEGMENTS, COMPOSITE DATA ELEMENTS, DATA ELEMENTS AND CODES USED IN THIS DOCUMENT ARE BASED ON THE RESPECTIVE DIRECTORIES IN
THE UN/EDIFACT D.05B DIRECTORY. REFER TO REFERENCES 5 TO 16 FOR MORE DETAILS.

THE MESSAGE SPECIFICATION PROVIDED IN THIS DOCUMENT ARE INTENDED FOR USE FOR THE EXCHANGE OF INFORMATION BETWEEN THE TRADING
PARTNERS IN INTERNATIONAL TRADE. THIS MESSAGE MAY BE APPLIED FOR BOTH NATIONAL AND INTERNATIONAL TRADE. IT IS BASED ON
UNIVERSAL PRACTICE AND IS NOT DEPENDENT ON THE TYPE OF BUSINESS OR INDUSTRY.

THE MESSAGE SPECIFICATION PROVIDED IN THIS DOCUMENT ARE INTENDED FOR USE FOR THE EXCHANGE OF INFORMATION BETWEEN THE TRADING
PARTNERS IN INTERNATIONAL TRADE. THIS MESSAGE MAY BE APPLIED FOR BOTH NATIONAL AND INTERNATIONAL TRADE. IT IS BASED ON
UNIVERSAL PRACTICE AND IS NOT DEPENDENT ON THE TYPE OF BUSINESS OR INDUSTRY.THIS MESSAGE INCORPORATES THE NECESSARY TRADE, TRANSPORT, STATISTICAL AND APPLICANT/DECLARANT INFORMATION ON THE APPLICATION.";
			DataProvider.Items = items;
			DataProvider.CertItems = items;
			DataProvider.CertificateType1 = "1";
			Message.BuildItem(COODEC);
			var certItem = COODEC.Item[0].ItemCertificate;
			AssertEquals("ItemCertificateDescription - line 1", "THIS SPECIFICATION PROVIDES THE DEF", certItem.ItemCertificateDescription[0].Line[0]);
			AssertEquals("ItemCertificateDescription - line 2", "INITION OF THE TRADE DECLARATION AN", certItem.ItemCertificateDescription[0].Line[1]);
			AssertEquals("ItemCertificateDescription - line 3", "D APPLICATION OF CERTIFICATE OF ORI", certItem.ItemCertificateDescription[0].Line[2]);
			AssertEquals("ItemCertificateDescription - line 4", "GIN IE. TCODEC (COODEC) MESSAGE TO ", certItem.ItemCertificateDescription[0].Line[3]);
			AssertEquals("ItemCertificateDescription - line 5", "BE USED IN ELECTRONIC DATA INTERCHA", certItem.ItemCertificateDescription[0].Line[4]);
			AssertEquals("ItemCertificateDescription - line 6", "NGE (EDI) BETWEEN TRADING PARTNERS ", certItem.ItemCertificateDescription[1].Line[0]);
			AssertEquals("ItemCertificateDescription - line 7", "INVOLVED IN ADMINISTRATION, COMMERC", certItem.ItemCertificateDescription[1].Line[1]);
			AssertEquals("ItemCertificateDescription - line 8", "E AND TRANSPORT. TCODEC IS A LOCAL ", certItem.ItemCertificateDescription[1].Line[2]);
			AssertEquals("ItemCertificateDescription - line 9", "MESSAGE, PENDING SUBMISSION TO UN F", certItem.ItemCertificateDescription[1].Line[3]);
			AssertEquals("ItemCertificateDescription - line 10", "OR APPROVAL AS A NEW UN STANDARD ME", certItem.ItemCertificateDescription[1].Line[4]);
			AssertEquals("ItemCertificateDescription - line 11", "SSAGE (UNSM). THIS DOCUMENT SHALL B", certItem.ItemCertificateDescription[2].Line[0]);
			AssertEquals("ItemCertificateDescription - line 12", "E USED AS A BASELINE FOR THE INTERF", certItem.ItemCertificateDescription[2].Line[1]);
			AssertEquals("ItemCertificateDescription - line 13", "ACE SOFTWARE DESIGN AND SHALL BE AG", certItem.ItemCertificateDescription[2].Line[2]);
			AssertEquals("ItemCertificateDescription - line 14", "REED UPON BY REPRESENTATIVE FROM IE", certItem.ItemCertificateDescription[2].Line[3]);
			AssertEquals("ItemCertificateDescription - line 15", " SINGAPORE AND CRIMSONLOGIC PTE LTD", certItem.ItemCertificateDescription[2].Line[4]);
			AssertEquals("ItemCertificateDescription - line 16", ".  THE DOCUMENT SHALL PROVIDE THE F", certItem.ItemCertificateDescription[3].Line[0]);
			AssertEquals("ItemCertificateDescription - line 17", "OLLOWING PRINCIPLES AND DETAILS FOR", certItem.ItemCertificateDescription[3].Line[1]);
			AssertEquals("ItemCertificateDescription - line 18", " THE FOLLOWING: - CRITERIA FOR PASS", certItem.ItemCertificateDescription[3].Line[2]);
			AssertEquals("ItemCertificateDescription - line 19", "ING INFORMATION FROM TRADERS, FREIG", certItem.ItemCertificateDescription[3].Line[3]);
			AssertEquals("ItemCertificateDescription - line 20", "HT FORWARDERS, CARGO AGENTS AND SHI", certItem.ItemCertificateDescription[3].Line[4]);
			AssertEquals("ItemCertificateDescription - line 21", "PPING AGENTS TO IESGP, CA OR CHAMBE", certItem.ItemCertificateDescription[4].Line[0]);
			AssertEquals("ItemCertificateDescription - line 22", "RS. - MESSAGE FORMAT. THE SEGMENTS,", certItem.ItemCertificateDescription[4].Line[1]);
			AssertEquals("ItemCertificateDescription - line 23", " COMPOSITE DATA ELEMENTS, DATA ELEM", certItem.ItemCertificateDescription[4].Line[2]);
			AssertEquals("ItemCertificateDescription - line 24", "ENTS AND CODES USED IN THIS DOCUMEN", certItem.ItemCertificateDescription[4].Line[3]);
			AssertEquals("ItemCertificateDescription - line 25", "T ARE BASED ON THE RESPECTIVE DIREC", certItem.ItemCertificateDescription[4].Line[4]);
			AssertEquals("ItemCertificateDescription - line 26", "TORIES IN THE UN/EDIFACT D.05B DIRE", certItem.ItemCertificateDescription[5].Line[0]);
			AssertEquals("ItemCertificateDescription - line 27", "CTORY. REFER TO REFERENCES 5 TO 16 ", certItem.ItemCertificateDescription[5].Line[1]);
			AssertEquals("ItemCertificateDescription - line 28", "FOR MORE DETAILS.  THE MESSAGE SPEC", certItem.ItemCertificateDescription[5].Line[2]);
			AssertEquals("ItemCertificateDescription - line 29", "IFICATION PROVIDED IN THIS DOCUMENT", certItem.ItemCertificateDescription[5].Line[3]);
			AssertEquals("ItemCertificateDescription - line 30", " ARE INTENDED FOR USE FOR THE EXCHA", certItem.ItemCertificateDescription[5].Line[4]);
			AssertEquals("ItemCertificateDescription - line 31", "NGE OF INFORMATION BETWEEN THE TRAD", certItem.ItemCertificateDescription[6].Line[0]);
			AssertEquals("ItemCertificateDescription - line 32", "ING PARTNERS IN INTERNATIONAL TRADE", certItem.ItemCertificateDescription[6].Line[1]);
			AssertEquals("ItemCertificateDescription - line 33", ". THIS MESSAGE MAY BE APPLIED FOR B", certItem.ItemCertificateDescription[6].Line[2]);
			AssertEquals("ItemCertificateDescription - line 34", "OTH NATIONAL AND INTERNATIONAL TRAD", certItem.ItemCertificateDescription[6].Line[3]);
			AssertEquals("ItemCertificateDescription - line 35", "E. IT IS BASED ON UNIVERSAL PRACTIC", certItem.ItemCertificateDescription[6].Line[4]);
			AssertEquals("ItemCertificateDescription - line 36", "E AND IS NOT DEPENDENT ON THE TYPE ", certItem.ItemCertificateDescription[7].Line[0]);
			AssertEquals("ItemCertificateDescription - line 37", "OF BUSINESS OR INDUSTRY.  THE MESSA", certItem.ItemCertificateDescription[7].Line[1]);
			AssertEquals("ItemCertificateDescription - line 38", "GE SPECIFICATION PROVIDED IN THIS D", certItem.ItemCertificateDescription[7].Line[2]);
			AssertEquals("ItemCertificateDescription - line 39", "OCUMENT ARE INTENDED FOR USE FOR TH", certItem.ItemCertificateDescription[7].Line[3]);
			AssertEquals("ItemCertificateDescription - line 40", "E EXCHANGE OF INFORMATION BETWEEN T", certItem.ItemCertificateDescription[7].Line[4]);
			AssertEquals("ItemCertificateDescription - line 41", "HE TRADING PARTNERS IN INTERNATIONA", certItem.ItemCertificateDescription[8].Line[0]);
			AssertEquals("ItemCertificateDescription - line 42", "L TRADE. THIS MESSAGE MAY BE APPLIE", certItem.ItemCertificateDescription[8].Line[1]);
			AssertEquals("ItemCertificateDescription - line 43", "D FOR BOTH NATIONAL AND INTERNATION", certItem.ItemCertificateDescription[8].Line[2]);
			AssertEquals("ItemCertificateDescription - line 44", "AL TRADE. IT IS BASED ON UNIVERSAL ", certItem.ItemCertificateDescription[8].Line[3]);
			AssertEquals("ItemCertificateDescription - line 45", "PRACTICE AND IS NOT DEPENDENT ON TH", certItem.ItemCertificateDescription[8].Line[4]);
			AssertEquals("ItemCertificateDescription - line 46", "E TYPE OF BUSINESS OR INDUSTRY.THIS", certItem.ItemCertificateDescription[9].Line[0]);
			AssertEquals("ItemCertificateDescription - line 47", " MESSAGE INCORPORATES THE NECESSARY", certItem.ItemCertificateDescription[9].Line[1]);
			AssertEquals("ItemCertificateDescription - line 48", " TRADE, TRANSPORT, STATISTICAL AND ", certItem.ItemCertificateDescription[9].Line[2]);
			AssertEquals("ItemCertificateDescription - line 49", "APPLICANT/DECLARANT INFORMATION ON ", certItem.ItemCertificateDescription[9].Line[3]);
			AssertEquals("ItemCertificateDescription - line 50", "THE APPLICATION.", certItem.ItemCertificateDescription[9].Line[4]);
		}

		public void TestWith2Certificates()
		{
			SetCOTestData(DataProvider);
			DataProvider.CertificateType1 = "9";
			DataProvider.CertificateType2 = "16";
			DataProvider.ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			DataProvider.CurrencyCode = "USD";
			DataProvider.AdditionalInformation = "MALAYSIA FORM D NO.KL2007/2/2155 DATED 10/06/2020";
			DataProvider.AdditionalDetails1 = "Certificate Additional Details - first certificate";
			DataProvider.AdditionalDetails2 = "Additional Details - second certificate";
			DataProvider.TransportDetails1 = "Certificate Transport Details - first certificate";
			DataProvider.NumberOfCopies1 = 3;
			DataProvider.NumberOfCopies2 = 0;
			DataProvider.DonorCountryCode = "CA";
			DataProvider.YearOfEntry = 2020;
			var cooMessage = Message.BuildDeclaration();
			AssertEquals("Message Header", "COODEC", cooMessage.Header.CommonAccessReference);
			AssertEquals("Message Header", "COO", cooMessage.Header.ApplicationType);
			AssertEquals("Message Header", SGXmlEDIMessage.SendersReferencePlaceHolderXml, cooMessage.Header.DeclarantID);
			AssertEquals("CertificateAdditionalInformation - split line 1", "MALAYSIA FORM D NO.KL2007/2/2155", cooMessage.Header.CertificateAdditionalInformation[0].Line[0]);
			AssertEquals("CertificateAdditionalInformation - split line 2", "DATED 10/06/2020", cooMessage.Header.CertificateAdditionalInformation[0].Line[1]);
			var certificate = cooMessage.Certificate;
			AssertEquals("ApplicationProductType", "TX", certificate.ApplicationProductType);
			AssertEquals("EntryYear", 2020, certificate.EntryYear);
			AssertEquals("GSPDonorCountry", "CA", certificate.GSPDonorCountry);
			AssertEquals("Certificate Cmwlth Pref. Content is only required for certificate type 5", 0, certificate.PreferenceContentPercent);
			AssertEquals("Certificate Reference Currency", "USD", certificate.CurrencyCode);
			AssertEquals("AdditionalCertificateDetails", @"CERTIFICATE ADDITIONAL DETAILS -
FIRST CERTIFICATE", string.Join(System.Environment.NewLine, certificate.AdditionalCertificateDetails));
			AssertEquals("TransportDetails", @"CERTIFICATE TRANSPORT DETAILS -
FIRST CERTIFICATE", string.Join(System.Environment.NewLine, certificate.TransportDetails));
			var cert1 = certificate.CertificateDetail[0];
			AssertEquals("First certificate details", 1, Utilities.ConvertToInt32(cert1.SequenceNumeric));
			AssertEquals("CertificateType", "9", cert1.CertificateType);
			AssertEquals("CopiesNumeric", 3, cert1.CopiesNumeric);
			var cert2 = certificate.CertificateDetail[1];
			AssertEquals("Second certificate details", 2, Utilities.ConvertToInt32(cert2.SequenceNumeric));
			AssertEquals("CertificateType", "16", cert2.CertificateType);
			AssertEquals("CopiesNumeric", 0, cert2.CopiesNumeric);
			AssertEquals("Port of Discharge", "USLAX", cooMessage.Transport.OutwardTransport.DischargePort);
			AssertEquals("Destination Country", "US", cooMessage.Transport.OutwardTransport.FinalDestinationCountry);
			AssertEquals("Departure Date", "20200616", cooMessage.Transport.OutwardTransport.DepartureDate);
			AssertEquals("Declaring Agent Identification", "AAA374M", cooMessage.Party.DeclaringAgentParty.PartyIdentification.ID);
			AssertEquals("Declaring Agent Name", "EAGLE DATAMATION INTERNATIONAL", cooMessage.Party.DeclaringAgentParty.PartyName[0]);
			AssertEquals("Declarant Identification", "V13T001", cooMessage.Party.DeclarantParty.PersonInformation.CodeValue);
			AssertEquals("Declarant Name", "CHOW LEW WEE", cooMessage.Party.DeclarantParty.PersonInformation.Name);
			AssertEquals("Consignee Name", "JOHN SMITH", cooMessage.Party.ConsigneeParty.PartyName[0]);
			AssertEquals("Consignee Address", "1556 SMITH ST LOS ANGELES", cooMessage.Party.ConsigneeParty.AddressLine[0]);
			AssertEquals("Exporter Name", "SINGAPORE EXPORTERS PTE LTD", cooMessage.Party.ExporterParty.PartyDetail.PartyName[0]);
			AssertEquals("Exporter Address", "GWANG ZO BUILDING C", cooMessage.Party.ExporterParty.AddressLine[0]);
			AssertEquals("Carrier Agent Name", "SINGAPORE PACIFIC LINE", cooMessage.Party.OutwardCarrierAgentParty.PartyName[0]);
			AssertEquals("NumberOfItems", 3, cooMessage.Summary.NumberOfItems);
		}

		public void TestWithMultipleItemLines()
		{
			SetCOTestData(DataProvider);
			DataProvider.CertificateType1 = "16";
			DataProvider.ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			DataProvider.SendInvoiceDetails = true;
			DataProvider.Items = DataProvider.CertItems;
			var cooMessage = Message.BuildDeclaration();
			var items = cooMessage.Item;
			AssertEquals("There should have been 3 item lines generated", 3, items.Length);
			var item1 = items[0];
			AssertEquals("ItemSequence", 1, item1.ItemSequenceNumeric);
			AssertEquals("ItemHarmonizedSystemCode - item 1", "48063000", item1.ItemHarmonizedSystemCode);
			AssertEquals("HarmonizedSystemQuantity.Value - item 1", 144m, item1.HarmonizedSystemQuantity.Value);
			AssertEquals("HarmonizedSystemQuantity.unitCode - item 1", "PKT", item1.HarmonizedSystemQuantity.unitCode);
			AssertEquals("OriginCountry - item 1", "SG", item1.OriginCountry);
			AssertEquals("ItemCIFFOBValue - item 1", 25000m, item1.ItemCIFFOBValue);
			var shippingMarks = item1.ShippingMarksInformation[0];
			AssertEquals("ShippingMarksInformation - item 1, line 1", "MARKS AND NUMBERS", shippingMarks.ShippingMarks[0]);
			AssertEquals("ShippingMarksInformation - item 1, line 2", "FOR CERTIFICATE", shippingMarks.ShippingMarks[1]);
			AssertEquals("ShippingMarksInformation - item 1, line 3", "OF ORIGIN - LINE", shippingMarks.ShippingMarks[2]);
			AssertEquals("ShippingMarksInformation - item 1, line 4", "1", shippingMarks.ShippingMarks[3]);
			AssertEquals("ItemCertificateQuantity.Value - item 1", 144m, item1.ItemCertificate.ItemCertificateQuantity.Value);
			AssertEquals("ItemCertificateQuantity.unitCode - item 1", "TNE", item1.ItemCertificate.ItemCertificateQuantity.unitCode);
			AssertEquals("ItemValue - item 1", 750m, item1.ItemCertificate.ItemValue);
			AssertEquals("ItemCertificateDescription - item 1", "TRACING PAPERS", item1.ItemCertificate.ItemCertificateDescription[0].Line[0]);
			AssertEquals("ManufacturingCostDate - item 1", "20191015", item1.ItemCertificate.ManufacturingCostDate);
			AssertEquals("TextileCategoryCode - item 1", "15", item1.ItemCertificate.TextileCategoryCode);
			AssertEquals("TextileQuotaQuantity.Value - item 1", 0m, item1.ItemCertificate.TextileQuotaQuantity.Value);
			AssertEquals("TextileQuotaQuantity.unitCode - item 1", "", item1.ItemCertificate.TextileQuotaQuantity.unitCode);
			AssertEquals("ItemInvoiceNumber - item 1", "INV01031", item1.ItemCertificate.ItemInvoiceNumber);
			AssertEquals("ItemInvoiceDate - item 1", "20200420", item1.ItemCertificate.ItemInvoiceDate);
			AssertEquals("OriginCriterion1 - item 1", "SINGLE", item1.ItemCertificate.OriginCriterion[0]);
			AssertEquals("OriginCriterion2 - item 1", "COUNTRY", item1.ItemCertificate.OriginCriterion[1]);
			AssertEquals("OriginCriterion3 - item 1", "CONTENT", item1.ItemCertificate.OriginCriterion[2]);
			AssertEquals("HarmonizedSystemCode - item 1", "480630", item1.ItemCertificate.HarmonizedSystemCode);
			AssertEquals("ContentPercent - item 1", 35, item1.ItemCertificate.ContentPercent);
			var item2 = items[1];
			AssertEquals("ItemSequence", 2, item2.ItemSequenceNumeric);
			AssertEquals("ItemHarmonizedSystemCode - item 2", "48083000", item2.ItemHarmonizedSystemCode);
			AssertEquals("HarmonizedSystemQuantity.Value - item 2", 250m, item2.HarmonizedSystemQuantity.Value);
			AssertEquals("HarmonizedSystemQuantity.unitCode - item 2", "BOX", item2.HarmonizedSystemQuantity.unitCode);
			AssertEquals("OriginCountry - item 2", "SG", item2.OriginCountry);
			AssertEquals("ItemCIFFOBValue - item 2", 25000m, item2.ItemCIFFOBValue);
			shippingMarks = item2.ShippingMarksInformation[0];
			AssertEquals("ShippingMarksInformation - item 2, line 1", "MARKS AND NUMBERS", shippingMarks.ShippingMarks[0]);
			AssertEquals("ShippingMarksInformation - item 2, line 2", "FOR CERTIFICATE", shippingMarks.ShippingMarks[1]);
			AssertEquals("ShippingMarksInformation - item 2, line 3", "OF ORIGIN - LINE", shippingMarks.ShippingMarks[2]);
			AssertEquals("ShippingMarksInformation - item 2, line 4", "2", shippingMarks.ShippingMarks[3]);
			AssertEquals("ItemCertificateQuantity.Value - item 2", 250m, item2.ItemCertificate.ItemCertificateQuantity.Value);
			AssertEquals("ItemCertificateQuantity.unitCode - item 2", "TNE", item2.ItemCertificate.ItemCertificateQuantity.unitCode);
			AssertEquals("ItemValue - item 2", 1095m, item2.ItemCertificate.ItemValue);
			AssertEquals("ItemCertificateDescription - item 2, line 1", "OTHER KRAFT PAPER CREPED OR", item2.ItemCertificate.ItemCertificateDescription[0].Line[0]);
			AssertEquals("ItemCertificateDescription - item 2, line 2", "CRINKLED", item2.ItemCertificate.ItemCertificateDescription[0].Line[1]);
			AssertEquals("ManufacturingCostDate - item 2", "20191015", item2.ItemCertificate.ManufacturingCostDate);
			AssertEquals("TextileCategoryCode - item 2", "15", item2.ItemCertificate.TextileCategoryCode);
			AssertEquals("TextileQuotaQuantity.Value - item 2", 0m, item2.ItemCertificate.TextileQuotaQuantity.Value);
			AssertEquals("TextileQuotaQuantity.unitCode - item 2", "", item2.ItemCertificate.TextileQuotaQuantity.unitCode);
			AssertEquals("ItemInvoiceNumber - item 2", "INV01031", item2.ItemCertificate.ItemInvoiceNumber);
			AssertEquals("ItemInvoiceDate - item 2", "20200420", item2.ItemCertificate.ItemInvoiceDate);
			AssertEquals("OriginCriterion1 - item 2", "SINGLE", item2.ItemCertificate.OriginCriterion[0]);
			AssertEquals("OriginCriterion2 - item 2", "COUNTRY", item2.ItemCertificate.OriginCriterion[1]);
			AssertEquals("OriginCriterion3 - item 2", "CONTENT", item2.ItemCertificate.OriginCriterion[2]);
			AssertEquals("HarmonizedSystemCode - item 2", "480830", item2.ItemCertificate.HarmonizedSystemCode);
			AssertEquals("ContentPercent - item 2", 35, item2.ItemCertificate.ContentPercent);
			var item3 = items[2];
			AssertEquals("ItemSequence", 3, item3.ItemSequenceNumeric);
			AssertEquals("ItemHarmonizedSystemCode - item 3", "48091010", item3.ItemHarmonizedSystemCode);
			AssertEquals("HarmonizedSystemQuantity.Value - item 3", 500m, item3.HarmonizedSystemQuantity.Value);
			AssertEquals("HarmonizedSystemQuantity.unitCode - item 3", "RLL", item3.HarmonizedSystemQuantity.unitCode);
			AssertEquals("OriginCountry - item 3", "SG", item3.OriginCountry);
			AssertEquals("ItemCIFFOBValue - item 3", 25000m, item3.ItemCIFFOBValue);
			shippingMarks = item3.ShippingMarksInformation[0];
			AssertEquals("ShippingMarksInformation - item 3, line 1", "MARKS AND NUMBERS", shippingMarks.ShippingMarks[0]);
			AssertEquals("ShippingMarksInformation - item 3, line 2", "FOR CERTIFICATE", shippingMarks.ShippingMarks[1]);
			AssertEquals("ShippingMarksInformation - item 3, line 3", "OF ORIGIN - LINE", shippingMarks.ShippingMarks[2]);
			AssertEquals("ShippingMarksInformation - item 3, line 4", "3", shippingMarks.ShippingMarks[3]);
			AssertEquals("ItemCertificateQuantity.Value - item 3", 500m, item3.ItemCertificate.ItemCertificateQuantity.Value);
			AssertEquals("ItemCertificateQuantity.unitCode - item 3", "TNE", item3.ItemCertificate.ItemCertificateQuantity.unitCode);
			AssertEquals("ItemValue - item 3", 1575m, item3.ItemCertificate.ItemValue);
			AssertEquals("ItemCertificateDescription - item 3", "CARBON PAPER IN ROLLS OR SHEETS", item3.ItemCertificate.ItemCertificateDescription[0].Line[0]);
			AssertEquals("ManufacturingCostDate - item 3", "20191015", item3.ItemCertificate.ManufacturingCostDate);
			AssertEquals("TextileCategoryCode - item 3", "15", item3.ItemCertificate.TextileCategoryCode);
			AssertEquals("TextileQuotaQuantity.Value - item 3", 0m, item3.ItemCertificate.TextileQuotaQuantity.Value);
			AssertEquals("TextileQuotaQuantity.unitCode - item 3", "", item3.ItemCertificate.TextileQuotaQuantity.unitCode);
			AssertEquals("ItemInvoiceNumber - item 3", "INV01031", item3.ItemCertificate.ItemInvoiceNumber);
			AssertEquals("ItemInvoiceDate - item 3", "20200420", item3.ItemCertificate.ItemInvoiceDate);
			AssertEquals("OriginCriterion1 - item 3", "SINGLE", item3.ItemCertificate.OriginCriterion[0]);
			AssertEquals("OriginCriterion2 - item 3", "COUNTRY", item3.ItemCertificate.OriginCriterion[1]);
			AssertEquals("OriginCriterion3 - item 3", "CONTENT", item3.ItemCertificate.OriginCriterion[2]);
			AssertEquals("HarmonizedSystemCode - item 3", "480910", item3.ItemCertificate.HarmonizedSystemCode);
			AssertEquals("ContentPercent - item 3", 35, item3.ItemCertificate.ContentPercent);
		}

		public void TestCmwlthContentPercent()
		{
			SetCOTestData(DataProvider);
			DataProvider.CertificateType1 = "1";
			DataProvider.ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			DataProvider.CurrencyCode = "USD";
			DataProvider.NumberOfCopies1 = 1;
			DataProvider.PercCommContent1 = 15;
			DataProvider.YearOfEntry = 2019;
			var cooMessage = Message.BuildDeclaration();
			var certificate = cooMessage.Certificate;
			AssertEquals("ApplicationProductType", "TX", certificate.ApplicationProductType);
			AssertEquals("GSPDonorCountry", null, certificate.GSPDonorCountry);
			AssertEquals("Certificate Cmwlth Pref. Content is mandatory for certificate type 5", 15, certificate.PreferenceContentPercent);
		}

		public void TestSummaryForCOODEC()
		{
			SetCOTestData(DataProvider);
			DataProvider.CertificateType1 = "1";
			var cooMessage = Message.BuildDeclaration();
			// summary for CoO should only have the number of items eg.  "<coo:Summary><cbc:NumberOfItems>3</cbc:NumberOfItems></coo:Summary>"
			AssertEquals("NumberOfItems", 3, cooMessage.Summary.NumberOfItems);
			AssertNull(cooMessage.Summary.TotalGrossWeight);
			AssertNull(cooMessage.Summary.TotalOuterPack);
		}

		#region Implementation
		COODEC Message => message ?? (message = new COODEC(DataProvider));
		COODEC message;
		CusCertificateTestClass DataProvider => dataProvider ?? (dataProvider = new CusCertificateTestClass());
		CusCertificateTestClass dataProvider;
		CertificateOfOrigin COODEC
		{
			get
			{
				if (cOODECMessage == null)
				{
					var tradenetDeclaration = new TradenetDeclaration();
					var inboundMessage = tradenetDeclaration.InboundMessage = new TradenetDeclarationInboundMessage();
					cOODECMessage = inboundMessage.CertificateOfOrigin = new CertificateOfOrigin();
				}

				return cOODECMessage;
			}
		}

		CertificateOfOrigin cOODECMessage;

		public CusCertificateTestClass SetCOTestData(CusCertificateTestClass cofODataProvider)
		{
			cofODataProvider.DeclarantId = "V13T001";
			AgentInfoTestClass declarant = new AgentInfoTestClass();
			declarant.Name = "Chow Lew Wee";
			declarant.Code = "V13T001";
			declarant.Phone = "+64 9760 2847";
			cofODataProvider.Declarant = declarant;
			cofODataProvider.CargoPackingType = "1";
			cofODataProvider.PortOfLoading = "SGSIN";
			cofODataProvider.PortOfDischarge = "USLAX";
			cofODataProvider.OutwardMasterBill = "OBL00384758";
			cofODataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			cofODataProvider.OutwardTransportIdentifier = "PACIFIC STAR";
			cofODataProvider.OutwardJourneyIdentifier = "18W";
			cofODataProvider.HasOutwardTransport = true;
			cofODataProvider.IsImport = false;
			cofODataProvider.IsSea = true;
			cofODataProvider.DepartureDate = new ZDate(2020, 06, 16);
			cofODataProvider.TotalGrossWeight = 85.75m;
			cofODataProvider.TotalGrossWeightUnitOfQty = Constants.Weight.Tonnes;
			cofODataProvider.TotalOuterPack = 4;
			cofODataProvider.TotalOuterPackUnitOfQty = Constants.PkgUnit.Carton;
			cofODataProvider.TradersRemarksForMessage = new ZString[] { "Test Declaration Message: exch rate 0.9565" };
			cofODataProvider.PreviousPermitNumber = "BA97985267";
			cofODataProvider.CountryOfFinalDestination = "US";
			OrganisationTestClass consignee = new OrganisationTestClass();
			consignee.Address = new OrganisationAddressTestClass("1556 smith st Los angeles");
			consignee.Name = "John Smith";
			cofODataProvider.Consignee = consignee;
			OrganisationTestClass exporter = new OrganisationTestClass();
			exporter.Address = new OrganisationAddressTestClass("Gwang Zo building c");
			exporter.UEN = "12093857483R";
			exporter.Name = "Singapore Exporters Pte Ltd";
			cofODataProvider.Exporter = exporter;
			OrganisationTestClass carrierAgent = new OrganisationTestClass();
			carrierAgent.Address = new OrganisationAddressTestClass("Keppel Warves");
			carrierAgent.UEN = "16003857483R";
			carrierAgent.Name = "Singapore Pacific Line";
			cofODataProvider.OutwardCarrierAgent = carrierAgent;
			ItemsTestClass[] invoices = new ItemsTestClass[1];
			invoices[0] = new ItemsTestClass();
			invoices[0].InvoicePK = ZGuid.NewZGuid();
			invoices[0].InvoiceCurrency = "SGD";
			invoices[0].IncoTerm = "FOB";
			invoices[0].InvoiceDate = new ZDate(2020, 04, 20);
			OrgHeader supplier = new BusinessObjectFactory().New<OrgHeader>();
			supplier.OH_FullName = "PACECO INDUSTRIAL SUPPLIES PTE LTD";
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "14242880000R");
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "14242880000R");
			invoices[0].Supplier = new EntryOrganisationsInfo(supplier);
			invoices[0].InvoiceNumber = "INV01031";
			invoices[0].InvoiceCurrExchangeRate = 1;
			invoices[0].InvoiceTotalAmount = 23012;
			cofODataProvider.Invoices = invoices;
			serialNumber = 0;
			ItemsTestClass[] items = new ItemsTestClass[3];
			items[0] = new ItemsTestClass();
			InitItem(items[0], "48063000", "TRACING PAPERS", 144, "PKT", 750);
			items[1] = new ItemsTestClass();
			InitItem(items[1], "48083000", "OTHER KRAFT PAPER CREPED OR CRINKLED", 250, "BOX", 1095);
			items[2] = new ItemsTestClass();
			InitItem(items[2], "48091010", "CARBON PAPER IN ROLLS OR SHEETS", 500, "RLL", 1575);
			cofODataProvider.CertItems = items;
			return cofODataProvider;
		}

		void InitItem(ItemsTestClass item, string certHSCode, string description, int qty, string qtyUnit, decimal unitPrice)
		{
			item.BrandName = "Spicers";
			item.CountryOfOriginCode = "SG";
			item.DutyAmount = 12;
			item.DutyUnitRate = 7;
			item.DutyUnitRateUnit = "PCE";
			item.DGIndicator = "N";
			item.E_SDNPIndicator = "Y";
			item.GSTPayable = 111;
			item.HSCode = certHSCode;
			item.CertHSCode = ((ZString)certHSCode).Left(6);
			item.DateOfManufacturingCost = new ZDate(2019, 10, 15);
			item.HSQuantity = qty;
			item.HSQuantityUnitType = qtyUnit;
			item.InvoiceNumber = "INV01031";
			item.InvoiceDate = new ZDate(2020, 04, 20);
			item.IsBasedOnRates = true;
			item.IsDangerous = false;
			item.IsLiquor = false;
			item.IsMotorVehicle = false;
			item.IsTobacco = false;
			serialNumber++;
			item.MarksAndNumbers = "Marks and numbers for certificate of Origin - line " + serialNumber.ToString();
			item.TextileCategoryCode = "15";
			item.TotalDutiableQuantity = 13;
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.SerialNumber = serialNumber.ToString();
			item.ItemQuantityUnitType = UnitOfQuantityCodeList.Codes.TNE;
			item.ItemQuantity = qty;
			item.UnitDutiableQuantity = 17;
			item.CustomsValue = 25000m;
			item.LSPValue = 2.28m;
			item.ItemValue = unitPrice;
			item.ItemDescription = description;
			item.PercentageContent = 35;
			item.OriginCriterion1 = "SINGLE";
			item.OriginCriterion2 = "COUNTRY";
			item.OriginCriterion3 = "CONTENT";
		}

		int serialNumber;
		#endregion
	}
}
