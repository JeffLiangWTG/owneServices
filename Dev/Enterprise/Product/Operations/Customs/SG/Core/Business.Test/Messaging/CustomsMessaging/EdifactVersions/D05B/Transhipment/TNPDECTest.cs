using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Messages.CUSDEC;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class TNPDECTest : TestCaseWithFactory
	{
		public void TestNextPortOfCall()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NextPortOfCall = "THBKK";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			Assert("Next port of call", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+61+THBKK'"));
		}

		public void TestFinalPortOfCall()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.HasLiquorOrTobacco = true;
			DataProvider.FinalPortOfCall = "MYPTK";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			Assert("Final Port of Call", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+130+MYPTK'"));
		}

		public void TestInwardVesselBerth()
		{
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			SGCPlaceTestClass inwardVesselBerth = new SGCPlaceTestClass();
			inwardVesselBerth.Code = "KW";
			inwardVesselBerth.Type = "KW";
			DataProvider.InwardVesselBerth = inwardVesselBerth;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			Assert("Inward Vessel Berth", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+KW'"));
			ResetMessageBuilder();
			inwardVesselBerth = new SGCPlaceTestClass();
			inwardVesselBerth.Code = "MYTEST";
			inwardVesselBerth.Type = SGCPlaces.Constants.PremiseType.Others;
			inwardVesselBerth.AddressRequired = true;
			inwardVesselBerth.NameAndAddress = "My vessel Location Keppel Warves";
			inwardVesselBerth.IsNonSystemNonLicenced = true;
			DataProvider.InwardVesselBerth = inwardVesselBerth;
			msg = MessageBuilder.CusdecMessage;
			Assert("Inward Vessel Berth", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+O:::MY VESSEL LOCATION KEPPEL WARVES'"));
		}

		public void TestOutwardVesselBerth()
		{
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			SGCPlaceTestClass outwardVesselBerth = new SGCPlaceTestClass();
			outwardVesselBerth.Code = "JW";
			outwardVesselBerth.Type = "JW";
			DataProvider.OutwardVesselBerth = outwardVesselBerth;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			Assert("Outward Vessel Berth", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+234+JW'"));
		}

		public void TestCountryOfFinalDestination()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.CountryOfFinalDestination = "HK";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			Assert("S/b no Country of Final Dest. (Seastores)", !msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+36+HK'"));
			ResetMessageBuilder();
			DataProvider.IsSeaStoreDeclaration = false;
			DataProvider.IsForStorage = true;
			msg = MessageBuilder.CusdecMessage;
			Assert("S/b no Country of Final Dest. (For Storage)", !msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+36+HK'"));
			ResetMessageBuilder();
			DataProvider.IsForStorage = false;
			msg = MessageBuilder.CusdecMessage;
			Assert("Country of Final Dest.", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+36+HK'"));
		}

		public void TestPlaceOfStorage()
		{
			DataProvider.IsStorageInFTZ = true;
			DataProvider.PlaceOfStorage = "EXAREA";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			Assert("Outward Vessel Berth", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+14+EXAREA'"));
		}

		public void TestGenerateDTMSegments()
		{
			DataProvider.HasInwardTransport = true;
			DataProvider.ArrivalDate = ZDate.Today;
			DataProvider.StartDateOfCargoRemoval = ZDate.Today;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("DTM+178:" + ZDate.Today.ToString("yyyyMMdd") + ":102'", msg.DTM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateEQDAndSELSegments()
		{
			ContainersTestClass[] containers = new ContainersTestClass[2];
			containers[0] = new ContainersTestClass();
			containers[1] = new ContainersTestClass();
			containers[0].ContainerNumber = "101";
			containers[0].ContainerType = "FCL";
			containers[0].ContainerSize = 13;
			containers[0].ContainerWeight = 913;
			containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1].ContainerNumber = "202";
			containers[1].ContainerType = "LCL";
			containers[1].ContainerSize = 77;
			containers[1].ContainerWeight = 517;
			containers[1].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1].SealNumber = "SHP1233445";
			DataProvider.Containers = containers;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("EQD+CN+101:1+:::FCL13913'EQD+CN+202:2+:::LCL77517'", msg.EQD.ToString(new UNOASGCharacterSet()));
			AssertEquals("SEL+NA'SEL+SHP1233445'", msg.SEL.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup4()
		{
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.InwardTransportIdentifier = "IV2232324";
			DataProvider.OutwardTransportIdentifier = "OV010101";
			DataProvider.OutwardJourneyIdentifier = "V555555";
			DataProvider.OutwardVesselType = "CV";
			DataProvider.OutwardVesselNationality = "SG";
			DataProvider.TowingVesselName = "TW00101";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("TDT+3++1+++++NA:::IV2232324'TDT+12++1+:::CV++++V555555:::OV010101'TDT+24+++++++NA:::TW00101'", msg.Group4.ToString(new UNOASGCharacterSet()));
			DataProvider.InwardJourneyIdentifier = "220E";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTF;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardJourneyIdentifier = "SQ190";
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.HasLiquorOrTobacco = false;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("TDT+3++1+++++220E:::IV2232324'TDT+12++4+++++SQ190'", msg.Group4.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasLiquorOrTobacco = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("TDT+3++1+++++220E:::IV2232324'TDT+12++1+:::CV++++SQ190:::OV010101'TPL+::::SG'TDT+24+++++++NA:::TW00101'", msg.Group4.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup5()
		{
			AttachmentsTestClass[] attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = "ATTDOC_0001";
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = "ATTDOC_0002";
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType002;
			SGCUSDECTestClass.ImplementsAdditionalMessageInformation addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			DataProvider.InwardMasterBill = "77777";
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardMasterBill = "5555";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("DOC+704+77777'DOC+741+5555'DOC+:::DOCUMENT ATTACHMENT+001::ATTDOC_0001'DOC+:::DOCUMENT ATTACHMENT+002::ATTDOC_0002'", msg.Group5.ToString(new UNOASGCharacterSet()));
			DataProvider.InwardMasterBill = "77777";
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardMasterBill = "5555";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			fMessageBuilder = null;
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("DOC+704+77777'DOC+741+5555'DOC+:::DOCUMENT ATTACHMENT+001::ATTDOC_0001'DOC+:::DOCUMENT ATTACHMENT+002::ATTDOC_0002'", msg.Group5.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup6()
		{
			AgentInfoTestClass agent = new AgentInfoTestClass();
			agent.Name = "Declarer";
			agent.Passport = "DEC0012SA";
			agent.EntityIdentifier = "D0001";
			agent.Phone = "322223";
			OrganisationTestClass inCarrierAgent = new OrganisationTestClass();
			inCarrierAgent.Name = "Inward Carrier";
			inCarrierAgent.UEN = "1235552223B";
			OrganisationTestClass outCarrierAgent = new OrganisationTestClass();
			outCarrierAgent.Name = "Outward Carrier";
			outCarrierAgent.UEN = "4321567891C";
			OrganisationTestClass importerOrg = new OrganisationTestClass();
			importerOrg.Name = "Importer Organisation";
			importerOrg.UEN = "1234567890C";
			OrganisationTestClass forwarderOrg = new OrganisationTestClass();
			forwarderOrg.Name = "Forwarder Org";
			forwarderOrg.UEN = "1234567890J";
			OrganisationTestClass handlingAgent = new OrganisationTestClass();
			handlingAgent.Name = "Handling Agent";
			handlingAgent.UEN = "2224567890D";
			OrganisationTestClass consignee = new OrganisationTestClass();
			consignee.Name = "Consignee Name";
			consignee.Address = new OrganisationAddressTestClass("Address, Address, Postcode");
			DataProvider.InwardCarrierAgent = inCarrierAgent;
			DataProvider.OutwardCarrierAgent = outCarrierAgent;
			DataProvider.Importer = importerOrg;
			DataProvider.Consignee = consignee;
			DataProvider.Declarant = agent;
			DataProvider.FreightForwarder = forwarderOrg;
			DataProvider.HandlingAgent = handlingAgent;
			DataProvider.EndUser = consignee;
			DataProvider.DeclarantId = "ORGSG00001";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BRE;
			DataProvider.BGIndicator = "I";
			DataProvider.InwardTransportCode = 2;
			DataProvider.HasInwardTransport = true;
			DataProvider.OutwardTransportCode = 2;
			DataProvider.HasOutwardTransport = true;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("TNPDEC SG6", "NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'NAD+DT++DECLARER'CTA+IC+:DEC0012SA'COM+322223:TE'NAD+CG+1235552223B++INWARD CARRIER'NAD+CA+4321567891C++OUTWARD CARRIER'NAD+IM+1234567890C++IMPORTER ORGANISATION'NAD+AH+2224567890D++HANDLING AGENT'NAD+UC+++CONSIGNEE NAME+ADDRESS, ADDRESS, POSTCODE'NAD+CN+++CONSIGNEE NAME+ADDRESS, ADDRESS, POSTCODE'NAD+FW+1234567890J++FORWARDER ORG'NAD+BB'RFF+DAN:I'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.IGM;
			DataProvider.BGIndicator = "D";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("TNPDEC SG6", "NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'NAD+DT++DECLARER'CTA+IC+:DEC0012SA'COM+322223:TE'NAD+CG+1235552223B++INWARD CARRIER'NAD+CA+4321567891C++OUTWARD CARRIER'NAD+IM+1234567890C++IMPORTER ORGANISATION'NAD+AH+2224567890D++HANDLING AGENT'NAD+UC+++CONSIGNEE NAME+ADDRESS, ADDRESS, POSTCODE'NAD+CN+++CONSIGNEE NAME+ADDRESS, ADDRESS, POSTCODE'NAD+FW+1234567890J++FORWARDER ORG'NAD+BB'RFF+DAN:D'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup10()
		{
			DataProvider.DeclarantId = "ORGSG00001";
			ItemsTestClass[] invoices = new ItemsTestClass[2];
			invoices[0] = new ItemsTestClass();
			invoices[0].InvoicePK = ZGuid.NewZGuid();
			invoices[0].InvoiceCurrency = "SGD";
			invoices[0].InvoiceDate = new ZDate(2006, 11, 29);
			invoices[0].InvoiceNumber = "INV0103101.9";
			invoices[0].InvoiceCurrExchangeRate = 1;
			invoices[0].InvoiceTotalAmount = 23012;
			invoices[1] = new ItemsTestClass();
			invoices[1].InvoiceCurrency = "USD";
			invoices[1].InvoiceDate = new ZDate(2006, 11, 30);
			invoices[1].InvoiceNumber = "INV0103101.10";
			invoices[1].InvoiceCurrExchangeRate = 112;
			invoices[1].InvoiceTotalAmount = 3000;
			DataProvider.Invoices = invoices;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Should be no SG10 in TNPDEC", "", msg.Group10.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup30()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			InitItem(items[0]);
			items[0].InwardHAWB = "In-hawb";
			items[0].OutwardHAWB = "Out-hawb";
			items[0].InwardMAWB = "MAW1";
			items[0].OutwardMAWB = "MAW2";
			DataProvider.Items = items;
			DataProvider.DeclarationType = "BRE";
			DataProvider.IsImport = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsCASCProductCodeNeeded = true;
			DataProvider.IsStoredInLicensedPremise = true;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString str1 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+AA7'LOC+249+BB3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:2.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+256+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'MOA+63:0.00'RFF+AEA:CS00001'DOC+703+IN-HAWB'DOC+714+OUT-HAWB'";
			AssertMultilineEquals("Group 30", str1, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			ResetMessageBuilder();
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTI;
			ZString str2 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+AA7'LOC+249+BB3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:2.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+256+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'RFF+AEA:CS00001'DOC+704+MAW1'DOC+741+MAW2'DOC+703+IN-HAWB'DOC+714+OUT-HAWB'";
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("", str2, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			ResetMessageBuilder();
			DataProvider.DeclarationType = "APS";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Mail;
			DataProvider.IsStoredInLicensedPremise = true;
			ZString str3 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+AA7'LOC+249+BB3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:2.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+256+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'RFF+AEA:CS00001'DOC+703+IN-HAWB'";
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("", str3, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			ResetMessageBuilder();
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 15;
			DataProvider.VoyageDuration = 22;
			msg = MessageBuilder.CusdecMessage;
			ZString str5 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+AA7'LOC+249+BB3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:2.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+256+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'RFF+AEA:CS00001'RFF+AEA:SEASTORE'GIN+AV+15+22'DOC+703+IN-HAWB'";
			AssertMultilineEquals("", str5, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			DataProvider.IsSeaStoreDeclaration = false;
			ResetMessageBuilder();
			DataProvider.DeclarationType = "BRE";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			items[0].IsMotorVehicle = true;
			InitItemMVDetails(items[0]);
			DataProvider.Items = items;
			ZString str4 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+AA7'LOC+249+BB3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:2.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+256+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'MOA+63:0.00'RFF+AEA:CS00001'GIN+AV+CS11111+CS22222+CS33333'GIN+AV+444444+555555+666666'RFF+SE'IMD++8'FTX+PRD+++1600.00:CC'DOC+703+IN-HAWB'DOC+714+OUT-HAWB'";
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Motor Vehicles", str4, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
		}

		#region Group 35 Tests
		public void TestGenerateSegmentGroup35()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			//Sea Stores
			DataProvider.Items = items;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 8;
			DataProvider.VoyageDuration = 15;
			//MV
			items[0].IsMotorVehicle = true;
			items[0].DateOfFirstRegistration = new ZDate(2005, 11, 23);
			items[0].EngineCapacity = 4800;
			items[0].EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			items[0].RegistrationNumberSG = "SG12345";
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 engineNumber = Factory.New<CASCCode1>();
			engineNumber.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(engineNumber);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 chassisNumber = Factory.New<CASCCode2>();
			chassisNumber.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(chassisNumber);
			items[0].CASCCodes2 = testCodes2;
			//strategic
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial application";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = "GOV";
			items[0].EndUseCode3 = "NMD";
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'IMD++8'FTX+PRD+++4800.00:CC'RFF+AEA:SEASTORE'GIN+AV+8+15'RFF+AEA:XY09348TFZ'GIN+AV+NMU+GOV+NMD'IMD++5'FTX+AFG+++COMMERCIAL APPLICATION'";
			AssertMultilineEquals("Group 35 - Strategic / Seastores / Motor Vehicle", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35Strategic()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial aircraft spares";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.NGU;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+AEA:XY09348TFZ'GIN+AV+NMU+NGU+NMD'IMD++5'FTX+AFG+++COMMERCIAL AIRCRAFT SPARES";
			AssertMultilineEquals("Group 35 - Strategic Goods", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35MotorVehicles()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].IsMotorVehicle = true;
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			items[0].DateOfFirstRegistration = new ZDate(2005, 11, 23);
			items[0].EngineCapacity = 4800;
			items[0].EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			items[0].RegistrationNumberSG = "SG12345";
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 code1_1 = Factory.New<CASCCode1>();
			code1_1.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(code1_1);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 code2_1 = Factory.New<CASCCode2>();
			code2_1.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(code2_1);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'IMD++8'FTX+PRD+++4800.00:CC'";
			AssertMultilineEquals("Group 35 - Motor Vehicle", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35Seastores()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			DataProvider.Items = items;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 8;
			DataProvider.VoyageDuration = 15;
			AssertMultilineEquals("Group 35 - Seastores", "RFF+AEA:SEASTORE'GIN+AV+8+15'", MessageBuilder.CusdecMessage.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35NoProductCode()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 code1_1 = Factory.New<CASCCode1>();
			code1_1.CY_Data = "5";
			testCodes1.Add(code1_1);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 code2_1 = Factory.New<CASCCode2>();
			code2_1.CY_Data = "50";
			testCodes2.Add(code2_1);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 35 - Chemical Concentration limit", "RFF+AVM'GIN+AV+5+50'", msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35NoProductCodeButOtherG35()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 code1_1 = Factory.New<CASCCode1>();
			code1_1.CY_Data = "5";
			testCodes1.Add(code1_1);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 code2_1 = Factory.New<CASCCode2>();
			code2_1.CY_Data = "50";
			testCodes2.Add(code2_1);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 8;
			DataProvider.VoyageDuration = 15;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 35 - No Product code but with other CASC values & Seastores", "RFF+AVM'GIN+AV+5+50'RFF+AEA:SEASTORE'GIN+AV+8+15'", msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestInvoiceNumberIsNotIncluded()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			DataProvider.Items = items;
			AssertMultilineEquals("Group 35 - should be empty", "", MessageBuilder.CusdecMessage.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		public void TestGenerateSegmentGroup49()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BRE;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.TotalDutyPayable = 3400;
			DataProvider.TotalExcisePayable = 700;
			DataProvider.TotalGSTPayable = 12009;
			DataProvider.TotalPayable = 17034;
			DataProvider.TotalCustomsValue = 230;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString str1 = "TAX+1'MOA+63:230.00'";
			AssertEquals(str1, msg.Group49.ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		TNPDECTestClass DataProvider
		{
			get
			{
				if (fDataProvider == null)
				{
					fDataProvider = new TNPDECTestClass();
				}

				return fDataProvider;
			}
		}

		TNPDECTestClass fDataProvider;
		TNPDEC MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new TNPDEC(DataProvider);
				}

				return fMessageBuilder;
			}
		}

		TNPDEC fMessageBuilder;
		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}

		void InitItem(ItemsTestClass item)
		{
			item.BrandName = "Sonic";
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "CS00001";
			cASC[0].ProductCodeQty = 2;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.ProductCodes = cASC;
			item.CountryOfOriginCode = "AU";
			item.CurrentLotNumber = "AA7";
			item.DutyAmount = 12;
			item.DutyUnitRate = 7;
			item.DGIndicator = "Y";
			item.UnitDutiableQuantity = 2;
			item.UnitDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.E_SDNPIndicator = "Y";
			item.ExciseAmount = 5;
			item.GoodsDescription = "Misc things";
			item.GSTPayable = 111;
			item.HSCode = "01011000";
			item.HSQuantity = 5;
			item.HSQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.InvoiceNumber = "INV100023";
			item.IsBasedOnRates = true;
			item.IsDangerous = true;
			item.IsLiquor = true;
			item.PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRI;
			item.IsTobacco = true;
			item.ModelDescription = "srft123fg";
			item.PackInmostQuantity = 40;
			item.PackInmostUnitType = "STK";
			item.PackInnerQuantity = 24;
			item.PackInnerUnitType = "CTN";
			item.PackInQuantity = 256;
			item.PackInUnitType = "BOX";
			item.PackOuterQuantity = 12;
			item.PackOuterUnitType = "CRT";
			item.PercentageOfAlcohol = 40;
			item.PercentageOfAlcoholUnitType = "LAL";
			item.PreviousLotNumber = "BB3";
			item.SerialNumber = "1";
			item.TotalDutiableQuantity = 13;
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.UnitDutiableQuantity = 2;
			item.DateOfFirstRegistration = new ZDate(2005, 11, 23);
			item.EngineCapacity = 1600;
			item.EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			item.RegistrationNumberSG = "SG12345";
		}

		void InitItemMVDetails(ItemsTestClass item)
		{
			item.DateOfFirstRegistration = new ZDate(2005, 11, 23);
			item.EngineCapacity = 1600;
			item.EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			item.RegistrationNumberSG = "SG12345";
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 code1_1 = Factory.New<CASCCode1>();
			code1_1.CY_Data = "CS11111";
			testCodes1.Add(code1_1);
			CASCCode1 code1_2 = Factory.New<CASCCode1>();
			code1_2.CY_Data = "444444";
			testCodes1.Add(code1_2);
			item.CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 code2_1 = Factory.New<CASCCode2>();
			code2_1.CY_Data = "CS22222";
			testCodes2.Add(code2_1);
			CASCCode2 code2_2 = Factory.New<CASCCode2>();
			code2_2.CY_Data = "555555";
			testCodes2.Add(code2_2);
			item.CASCCodes2 = testCodes2;
			CASCCode3Collection testCodes3 = new CASCCode3Collection(InvoiceLine);
			CASCCode3 code3_1 = Factory.New<CASCCode3>();
			code3_1.CY_Data = "CS33333";
			testCodes3.Add(code3_1);
			CASCCode3 code3_2 = Factory.New<CASCCode3>();
			code3_2.CY_Data = "666666";
			testCodes3.Add(code3_2);
			item.CASCCodes3 = testCodes3;
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
	}
}
