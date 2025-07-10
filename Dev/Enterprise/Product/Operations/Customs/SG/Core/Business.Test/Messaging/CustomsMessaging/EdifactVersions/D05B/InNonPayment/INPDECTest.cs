using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class INPDECTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestGenerateLOCSegments()
		{
			DataProvider.CargoPackingType = CargoPackingTypeCodeList.Codes.PackingType3;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = false;
			DataProvider.Is2bStoredBWCY = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.PlaceOfRelease = CreateTestPlace("O2", "O", "Release Location Very common location address of Discharge", true, true);
			DataProvider.InwardVesselBerth = CreateTestPlace("R1", "SY", "Inward Vessel Location Very common location address of Berth", true, true);
			DataProvider.PlaceOfStorage = "locgoods";
			DataProvider.PlaceOfReceipt = CreateTestPlace("SC1", "SC", "Receipt Location Very common location address of Receipt", true, true);
			DataProvider.OutwardVesselBerth = CreateTestPlace("O1", "O", "Out Vssl Location Very common location address of Outward Vssl Berth", true, true);
			DataProvider.PortOfDischarge = "audis";
			DataProvider.NextPortOfCall = "aucal";
			DataProvider.PortOfLoading = "auloa";
			DataProvider.FinalPortOfCall = "auudc";
			DataProvider.CountryOfFinalDestination = "usdes";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("LOC Segments expected", 5, msg.LOC.Count);
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+9+AULOA"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+SY:::INWARD VESSEL LOCATION VERY COMMON LOCATION ADDRESS OF BERTH"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+14+LOCGOODS"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+11+O:::RELEASE LOCATION VERY COMMON LOCATION ADDRESS OF DISCHARGE"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+88+SC:::RECEIPT LOCATION VERY COMMON LOCATION ADDRESS OF RECEIPT"));
			ResetMessageBuilder();
			DataProvider.HasInwardTransport = false;
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasLiquorOrTobacco = true;
			msg = MessageBuilder.CusdecMessage;
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+61+AUCAL"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+130+AUUDC"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+SY:::INWARD VESSEL LOCATION VERY COMMON LOCATION ADDRESS OF BERTH"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+234+O:::OUT VSSL LOCATION VERY COMMON LOCATION ADDRESS OF OUTWARD VSSL BERTH"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+14+LOCGOODS"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+11+O:::RELEASE LOCATION VERY COMMON LOCATION ADDRESS OF DISCHARGE"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+88+SC:::RECEIPT LOCATION VERY COMMON LOCATION ADDRESS OF RECEIPT"));
			ResetMessageBuilder();
			DataProvider.IsSeaStoreDeclaration = false;
			msg = MessageBuilder.CusdecMessage;
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+12+AUDIS"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+SY:::INWARD VESSEL LOCATION VERY COMMON LOCATION ADDRESS OF BERTH"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+234+O:::OUT VSSL LOCATION VERY COMMON LOCATION ADDRESS OF OUTWARD VSSL BERTH"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+36+USDES"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+11+O:::RELEASE LOCATION VERY COMMON LOCATION ADDRESS OF DISCHARGE"));
			Assert(msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+88+SC:::RECEIPT LOCATION VERY COMMON LOCATION ADDRESS OF RECEIPT"));
		}

		public void TestGenerateOutwardLOCSegment()
		{
			DataProvider.CargoPackingType = CargoPackingTypeCodeList.Codes.PackingType3;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SHO;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.InwardVesselBerth = CreateTestPlace("R1", "SY", "Inward Vessel Location Very common location address of Berth", true, true);
			DataProvider.OutwardVesselBerth = CreateTestPlace("O1", "O", "Out Vssl Location Very common location address of Outward Vssl Berth", true, true);
			CUSDECMessage shutOutINPUPDMsg = MessageBuilder.CusdecMessage;
			AssertEquals("LOC Segments expected", 2, shutOutINPUPDMsg.LOC.Count);
			Assert(shutOutINPUPDMsg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+SY:::INWARD VESSEL LOCATION VERY COMMON LOCATION ADDRESS OF BERTH"));
			Assert("Out Vessel Location if entered should be created regardless of declaration type.", shutOutINPUPDMsg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+234+O:::OUT VSSL LOCATION VERY COMMON LOCATION ADDRESS OF OUTWARD VSSL BERTH"));
			ResetMessageBuilder();
			DataProvider.OutwardVesselBerth = null;
			shutOutINPUPDMsg = MessageBuilder.CusdecMessage;
			AssertEquals("LOC Segments expected - Outward vessel Location should not be created as not entered.", 1, shutOutINPUPDMsg.LOC.Count);
			Assert(shutOutINPUPDMsg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+164+SY:::INWARD VESSEL LOCATION VERY COMMON LOCATION ADDRESS OF BERTH"));
		}

		public void TestGenerateDTMSegments()
		{
			DataProvider.HasInwardTransport = true;
			DataProvider.ArrivalDate = ZDate.Today;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("DTM+178:" + ZDate.Today.ToString("yyyyMMdd") + ":102'", msg.DTM.ToString(new UNOASGCharacterSet()));
			DataProvider.HasInwardTransport = false;
			DataProvider.HasOutwardTransport = true;
			DataProvider.DepartureDate = ZDate.Today.AddDays(1);
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("DTM+136:" + ZDate.Today.AddDays(1).ToString("yyyyMMdd") + ":102'", msg.DTM.ToString(new UNOASGCharacterSet()));
			DataProvider.IsTemporaryConsignment = true;
			DataProvider.DepartureDate = ZDate.Today.AddDays(1);
			DataProvider.EndDateOfTemporaryImport = ZDate.Today.AddDays(2);
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("DTM+136:" + ZDate.Today.AddDays(1).ToString("yyyyMMdd") + ":102'DTM+206:" + ZDate.Today.AddDays(2).ToString("yyyyMMdd") + ":102'", msg.DTM.ToString(new UNOASGCharacterSet()));
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

		public void TestGenerateSegmentGroup1()
		{
			DataProvider.DeclarantId = "ORGSG00001";
			LicencesAndDocumentsTestClass[] documents = new LicencesAndDocumentsTestClass[2];
			documents[0] = new LicencesAndDocumentsTestClass();
			documents[0].LicenceNumber = "LIC123-3456-1";
			documents[1] = new LicencesAndDocumentsTestClass();
			documents[1].LicenceNumber = "LIC123-3456-2";
			DataProvider.LicencesAndDocuments = documents;
			DataProvider.DeclarationType = "GTR";
			DataProvider.SupplyIndicator = "Y";
			DataProvider.PreviousPermitNumber = "PERM001/1-2";
			ZString[] additionals = new ZString[1];
			additionals[0] = new ZString("ADD12034");
			DataProvider.AdditionalRecipients = additionals;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("RFF+MS:ORGS.ORGSG00001'RFF+DM:LIC123-3456-1'RFF+DM:LIC123-3456-2'RFF+TN:Y'RFF+ACE:PERM001/1-2'RFF+MR:ADD12034'", msg.Group1.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup4()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SHO;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("SHO declaration should not have transport segment", 0, msg.Group4.Count);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			DataProvider.IsReleasedInLicensedPremiseExclBWCY = true;
			SGCPlaceTestClass receipt = new SGCPlaceTestClass();
			receipt.Code = SGCPlaces.Constants.MajorExporterScheme;
			DataProvider.PlaceOfReceipt = receipt;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("APS declaration with goods release into a licenced premise under the Major Exporter scheme should not have transport segment", 0, msg.Group4.Count);
			receipt = new SGCPlaceTestClass();
			receipt.Code = "JZ";
			DataProvider.PlaceOfReceipt = receipt;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Road;
			DataProvider.HasInwardTransport = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Should now generate Inward Transport", 1, msg.Group4.Count);
			AssertEquals("TDT+3++3'", msg.Group4.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKN;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsForStorage = true;
			DataProvider.IsReleasedInLicensedPremiseExclBWCY = false;
			DataProvider.InwardTransportIdentifier = "Hyogo Maru";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("TDT+3++1+++++NA:::HYOGO MARU'", msg.Group4.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.InwardJourneyIdentifier = "QF220";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardVesselNationality = "LR";
			DataProvider.OutwardTransportIdentifier = "Asia Star";
			DataProvider.OutwardJourneyIdentifier = "V555555";
			DataProvider.OutwardVesselType = "CV";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("TDT+3++4+++++QF220'TDT+12++1+:::CV++++V555555:::ASIA STAR'", msg.Group4.ToString(new UNOASGCharacterSet()));
			DataProvider.TowingVesselName = "Little Red Tug";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("TDT+3++4+++++QF220'TDT+12++1+:::CV++++V555555:::ASIA STAR'TDT+24+++++++NA:::LITTLE RED TUG'", msg.Group4.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup6()
		{
			AgentInfoTestClass testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test SG Broker";
			testDeclarant.Phone = "64 85721111";
			testDeclarant.Code = "v13t001";
			DataProvider.Declarant = testDeclarant;
			OrganisationTestClass testClaimant = new OrganisationTestClass();
			testClaimant.Name = "SG Claimant";
			DataProvider.Claimant = testClaimant;
			OrganisationTestClass inwardCarrierAgentTestOrg = new OrganisationTestClass();
			inwardCarrierAgentTestOrg.Name = "Inward Carrier Agent";
			inwardCarrierAgentTestOrg.Address = new OrganisationAddressTestClass("Inward Carrier Agent Address, Inward Carrier Agent City, Inward Carrier Agent PostCode");
			inwardCarrierAgentTestOrg.UEN = "12345678901A";
			DataProvider.InwardCarrierAgent = inwardCarrierAgentTestOrg;
			OrganisationTestClass outwardCarrierAgentTestOrg = new OrganisationTestClass();
			outwardCarrierAgentTestOrg.Name = "Outward Carrier Agent";
			outwardCarrierAgentTestOrg.Address = new OrganisationAddressTestClass("Outward Carrier Agent Address, Outward Carrier Agent City, Outward Carrier Agent PostCode");
			outwardCarrierAgentTestOrg.UEN = "12345678901B";
			DataProvider.OutwardCarrierAgent = outwardCarrierAgentTestOrg;
			OrganisationTestClass importerTestOrg = new OrganisationTestClass();
			importerTestOrg.Name = "Importer";
			importerTestOrg.Address = new OrganisationAddressTestClass("Importer Address, Importer City, Importer PostCode");
			importerTestOrg.UEN = "12345678901C";
			DataProvider.Importer = importerTestOrg;
			OrganisationTestClass exporterTestOrg = new OrganisationTestClass();
			exporterTestOrg.Name = "Exporter";
			exporterTestOrg.Address = new OrganisationAddressTestClass("Exporter Address, Exporter City, Exporter PostCode");
			exporterTestOrg.UEN = "12345678901D";
			DataProvider.Exporter = exporterTestOrg;
			OrganisationTestClass consigneeTestOrg = new OrganisationTestClass();
			consigneeTestOrg.Name = "Consignee";
			consigneeTestOrg.Address = new OrganisationAddressTestClass("Consignee Address, Consignee City, Consignee PostCode");
			consigneeTestOrg.UEN = "12345678901E";
			DataProvider.Consignee = consigneeTestOrg;
			OrganisationTestClass freightForwarderTestOrg = new OrganisationTestClass();
			freightForwarderTestOrg.Name = "FreightForwarder";
			freightForwarderTestOrg.Address = new OrganisationAddressTestClass("FreightForwarder Address, FreightForwarder City, FreightForwarder PostCode");
			freightForwarderTestOrg.UEN = "12345678901F";
			DataProvider.FreightForwarder = freightForwarderTestOrg;
			DataProvider.DeclarantId = "ORGSG00001";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GTR;
			DataProvider.BGIndicator = "D";
			DataProvider.ConsigneeAddress = "Con Home str 1";
			DataProvider.InwardTransportCode = 2;
			DataProvider.HasInwardTransport = true;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("INPDEC SG6", "NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'NAD+DT++TEST SG BROKER'CTA+IC+:V13T001'COM+64 85721111:TE'NAD+CG+12345678901A++INWARD CARRIER AGENT'NAD+CA+12345678901B++OUTWARD CARRIER AGENT'NAD+IM+12345678901C++IMPORTER'NAD+EX+12345678901D++EXPORTER'NAD+CN+++CONSIGNEE+CONSIGNEE ADDRESS, CONSIGNEE CITY,:CONSIGNEE POSTCODE'NAD+FW+12345678901F++FREIGHTFORWARDER'NAD+CC+++SG CLAIMANT'CTA+IC'NAD+BB'RFF+DAN:D'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCE;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("INPDEC SG6", "NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'NAD+DT++TEST SG BROKER'CTA+IC+:V13T001'COM+64 85721111:TE'NAD+CG+12345678901A++INWARD CARRIER AGENT'NAD+CA+12345678901B++OUTWARD CARRIER AGENT'NAD+IM+12345678901C++IMPORTER'NAD+EX+12345678901D++EXPORTER'NAD+CN+++CONSIGNEE+CONSIGNEE ADDRESS, CONSIGNEE CITY,:CONSIGNEE POSTCODE'NAD+FW+12345678901F++FREIGHTFORWARDER'NAD+CC+++SG CLAIMANT'CTA+IC'NAD+BB'RFF+DAN:D'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("INPDEC SG6", "NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'NAD+DT++TEST SG BROKER'CTA+IC+:V13T001'COM+64 85721111:TE'NAD+CG+12345678901A++INWARD CARRIER AGENT'NAD+CA+12345678901B++OUTWARD CARRIER AGENT'NAD+IM+12345678901C++IMPORTER'NAD+EX+12345678901D++EXPORTER'NAD+CN+++CONSIGNEE+CONSIGNEE ADDRESS, CONSIGNEE CITY,:CONSIGNEE POSTCODE'NAD+FW+12345678901F++FREIGHTFORWARDER'NAD+CC+++SG CLAIMANT'CTA+IC'NAD+BB'RFF+DAN:D'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SFZ;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasOutwardTransport = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("INPDEC SG6", "NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'NAD+DT++TEST SG BROKER'CTA+IC+:V13T001'COM+64 85721111:TE'NAD+CG+12345678901A++INWARD CARRIER AGENT'NAD+CA+12345678901B++OUTWARD CARRIER AGENT'NAD+IM+12345678901C++IMPORTER'NAD+EX+12345678901D++EXPORTER'NAD+CN+++CONSIGNEE+CONSIGNEE ADDRESS, CONSIGNEE CITY,:CONSIGNEE POSTCODE'NAD+FW+12345678901F++FREIGHTFORWARDER'NAD+CC+++SG CLAIMANT'CTA+IC'NAD+BB'RFF+DAN:D'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup10()
		{
			ItemsTestClass[] invoices = new ItemsTestClass[2];
			invoices[0] = new ItemsTestClass();
			invoices[0].InvoicePK = ZGuid.NewZGuid();
			invoices[0].InvoiceCurrency = Core.Constants.CurrencyCodes.Singapore;
			invoices[0].InvoiceDate = new ZDate(2006, 11, 29);
			invoices[0].Supplier = CreateSupplier("SUP Twelve", "SUP012");
			invoices[0].InvoiceNumber = "INV0103101.9";
			invoices[0].InvoiceCurrExchangeRate = 1;
			invoices[0].InvoiceTotalAmount = 23012;
			invoices[0].IncoTerm = "FOB";
			invoices[0].FreightCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 100m, 0m);
			invoices[0].InsuranceCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 200m, 0m);
			invoices[0].OtherCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.UnitedStates, 110m, 100m, 10m);
			invoices[1] = new ItemsTestClass();
			invoices[1].InvoicePK = ZGuid.NewZGuid();
			invoices[1].InvoiceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			invoices[1].InvoiceDate = new ZDate(2006, 11, 30);
			invoices[1].InvoiceNumber = "INV0103101.10";
			invoices[1].InvoiceCurrExchangeRate = 112;
			invoices[1].InvoiceTotalAmount = 3000;
			invoices[1].Supplier = CreateSupplier("SUP Eleven", "SUP011");
			invoices[1].IncoTerm = "FOB";
			invoices[1].FreightCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 400m, 0m);
			invoices[1].InsuranceCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 500m, 0m);
			invoices[1].OtherCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.UnitedStates, 110m, 150m, 10m);
			DataProvider.Invoices = invoices;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("", "DMS+INVOICE DETAILS'MOA+39:23012.00:SGD'TOD+++FOB'NAD+SU++SUP012+SUP TWELVE'DOC+380+INV0103101.9'DTM+3:20061129:102'ALC+C'MOA+64:100.00:SGD'ALC+C'MOA+70:200.00:SGD'ALC+C'MOA+304:100.00:USD'PCD+5:10.000'CUX+++110.000000'DMS+INVOICE DETAILS'MOA+39:3000.00:USD'CUX+++112.000000'TOD+++FOB'NAD+SU++SUP011+SUP ELEVEN'DOC+380+INV0103101.10'DTM+3:20061130:102'ALC+C'MOA+64:400.00:SGD'ALC+C'MOA+70:500.00:SGD'ALC+C'MOA+304:150.00:USD'PCD+5:10.000'CUX+++110.000000'", msg.Group10.ToString(new UNOASGCharacterSet()), '\'');
		}

		IOrganisation CreateSupplier(string supplierName, string uEN)
		{
			OrgHeader supplier = new BusinessObjectFactory().New<OrgHeader>();
			supplier.OH_FullName = supplierName;
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, uEN);
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, uEN);
			return new EntryOrganisationsInfo(supplier);
		}

		public void TestGenerateSegmentGroup30()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			InitItem(items[0]);
			items[0].InwardHAWB = "In-HB";
			DataProvider.Items = items;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCE;
			DataProvider.IsImport = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			string str1 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+7'LOC+249+3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:17.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+144+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'PCI+28+MARKSANDNOS'MOA+63:0.00'RFF+IV:INV100023'RFF+AEA:CS00001'DOC+703+IN-HB'TAX+5+:::PRI+++KGM:::7.0000'MOA+55:12.00'TAX+5++++KGM:::7.5000'MOA+161:270.38'TAX+7++++:::5'MOA+124:111.00'";
			AssertMultilineEquals("Group 30", str1, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			ResetMessageBuilder();
			string str2 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+7'LOC+249+3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:17.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+144+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'PCI+28+MARKSANDNOS'MOA+63:0.00'RFF+IV:INV100023'RFF+AEA:CS00001'DOC+703+IN-HB'TAX+5+:::PRI+++KGM:::7.0000'MOA+55:12.00'TAX+5++++KGM:::7.5000'MOA+161:270.38'TAX+7++++:::5'MOA+124:111.00'";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("", str2, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			ResetMessageBuilder();
			string str3 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+7'LOC+249+3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:17.0000'MEA+ABA++KGM:2.0000'PAC+12+3+CRT'PAC+144+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'PCI+28+MARKSANDNOS'MOA+63:0.00'RFF+IV:INV100023'RFF+AEA:CS00001'";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			DataProvider.InwardTransportCode = 2;
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("", str3, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
			ResetMessageBuilder();
			items[0].ProductCodes = null;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 15;
			DataProvider.VoyageDuration = 22;
			msg = MessageBuilder.CusdecMessage;
			string str4 = "CST+1+01011000'FTX+AAA+++MISC THINGS'FTX+PRD+++SONIC:SRFT123FG'FTX+AAC+++Y'LOC+27+AU'LOC+18+7'LOC+249+3'MEA+AAF++KGM:5.0000'MEA+AAE++KGM:13.0000'MEA+AAG++LPA:40.000'MEA+AAI++KGM:17.0000'PAC+12+3+CRT'PAC+144+2+BOX'PAC+24+1+CTN'PAC+40+5+STK'PCI+19+Y'PCI+28+MARKSANDNOS'MOA+63:0.00'RFF+IV:INV100023'RFF+AEA:SEASTORE'GIN+AV+15+22'";
			AssertMultilineEquals("", str4, msg.Group30.ToString(new UNOASGCharacterSet()), '\'');
		}

		#region Group 35 tests
		public void TestGenerateSegmentGroup35()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			InitItem(items[0]);
			InitItemMVDetails(items[0]);
			items[0].InwardHAWB = "In-HB";
			items[0].ProductCodes = null;
			DataProvider.Items = items;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCE;
			DataProvider.IsImport = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			ResetMessageBuilder();
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			string expectedResult = "RFF+IV:INV100023'RFF+AVM'GIN+AV+11111+222222+333333'GIN+AV+444444+555555+666666'RFF+SE'DTM+375:20051123:102'IMD++8'FTX+PRD+++1600.00:CC'";
			AssertMultilineEquals("Group 35", expectedResult, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35Mixed()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			items[0].HSCode = "87042233";
			items[0].HSQuantity = 3.5;
			items[0].HSQuantityUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].IsMotorVehicle = true;
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial application";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.GOV;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
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
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SFZ;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'DTM+375:20051123:102'IMD++8'FTX+PRD+++4800.00:CC'";
			AssertMultilineEquals("Group 35 - Strategic Motor Vehicle", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35Strategic()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial aircraft spares";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.NGU;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'"; // no strategic goods for INP message
			AssertMultilineEquals("Group 35 - s/b NO Strategic Goods segments", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35MotorVehicles()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
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
			CASCCode1 engineNo = Factory.New<CASCCode1>();
			engineNo.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(engineNo);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 chassisNo = Factory.New<CASCCode2>();
			chassisNo.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(chassisNo);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'DTM+375:20051123:102'IMD++8'FTX+PRD+++4800.00:CC'";
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
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'RFF+AEA:SEASTORE'GIN+AV+8+15'";
			AssertMultilineEquals("Group 35 - Seastores", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35NoProductCode()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "5";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "50";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'RFF+AVM'GIN+AV+5+50'";
			AssertMultilineEquals("Group 35 - Chemical Concentration limit", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		public void TestGenerateSegmentGroup49()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GTR;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.TotalDutyPayable = 3400;
			DataProvider.TotalExcisePayable = 700;
			DataProvider.TotalGSTPayable = 12009;
			DataProvider.TotalPayable = 17034;
			DataProvider.TotalCustomsValue = 230;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString str1 = "TAX+5'MOA+55:3400.00'TAX+5'MOA+161:700.00'TAX+1'MOA+63:230.00'TAX+2'MOA+9:17034.00'TAX+7'MOA+124:12009.00'";
			AssertEquals(str1, msg.Group49.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			DataProvider.IsDG = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			ZString str2 = "";
			AssertEquals(str2, msg.Group49.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			DataProvider.IsDG = false;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals(str1, msg.Group49.ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		INPDECTestClass DataProvider
		{
			get
			{
				if (fDataProvider == null)
				{
					fDataProvider = new INPDECTestClass();
				}

				return fDataProvider;
			}
		}

		INPDECTestClass fDataProvider;
		INPDEC MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new INPDEC(DataProvider);
				}

				return fMessageBuilder;
			}
		}

		INPDEC fMessageBuilder;
		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}

		void InitItem(ItemsTestClass item)
		{
			DataProvider.IsStoredInLicensedPremise = true;
			item.InvoiceCurrency = Core.Constants.CurrencyCodes.Singapore;
			item.BrandName = "Sonic";
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "CS00001";
			cASC[0].ProductCodeQty = 2;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.ProductCodes = cASC;
			item.CountryOfOriginCode = "AU";
			item.CurrentLotNumber = "7";
			item.DutyAmount = 12;
			item.DutyUnitRate = 7;
			item.DutyUnitRateUnit = UnitOfQuantityCodeList.Codes.KGM;
			item.DGIndicator = "Y";
			item.UnitDutiableQuantity = 2;
			item.UnitDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.E_SDNPIndicator = "Y";
			item.ExciseAmount = 270.38;
			item.ExciseUnitRate = 7.5;
			item.DutyRateUnit = UnitOfQuantityCodeList.Codes.KGM;
			item.GoodsDescription = "Misc things";
			item.GSTPayable = 111;
			item.GSTRate = 5;
			item.HSCode = "01011000";
			item.HSQuantity = 5;
			item.HSQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.InvoiceNumber = "INV100023";
			item.IsBasedOnRates = true;
			item.IsDangerous = true;
			item.IsLiquor = true;
			item.IsMotorVehicle = false;
			item.PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRI;
			item.IsTobacco = true;
			item.MarksAndNumbers = "marksandnos";
			item.ModelDescription = "srft123fg";
			item.PackInmostQuantity = 40;
			item.PackInmostUnitType = "STK";
			item.PackInnerQuantity = 24;
			item.PackInnerUnitType = "CTN";
			item.PackInQuantity = 144;
			item.PackInUnitType = "BOX";
			item.PackOuterQuantity = 12;
			item.PackOuterUnitType = "CRT";
			item.PercentageOfAlcohol = 40;
			item.PercentageOfAlcoholUnitType = "LPA";
			item.PreviousLotNumber = "3";
			item.SerialNumber = "1";
			item.TotalDutiableQuantity = 13;
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.UnitDutiableQuantity = 17;
		}

		void InitItemMVDetails(ItemsTestClass item)
		{
			item.IsMotorVehicle = true;
			item.DateOfFirstRegistration = new ZDate(2005, 11, 23);
			item.EngineCapacity = 1600;
			item.EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			item.RegistrationNumberSG = "SG12345";
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 code1_1 = Factory.New<CASCCode1>();
			code1_1.CY_Data = "11111";
			testCodes1.Add(code1_1);
			CASCCode1 code1_2 = Factory.New<CASCCode1>();
			code1_2.CY_Data = "444444";
			testCodes1.Add(code1_2);
			item.CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 code2_1 = Factory.New<CASCCode2>();
			code2_1.CY_Data = "222222";
			testCodes2.Add(code2_1);
			CASCCode2 code2_2 = Factory.New<CASCCode2>();
			code2_2.CY_Data = "555555";
			testCodes2.Add(code2_2);
			item.CASCCodes2 = testCodes2;
			CASCCode3Collection testCodes3 = new CASCCode3Collection(InvoiceLine);
			CASCCode3 code3_1 = Factory.New<CASCCode3>();
			code3_1.CY_Data = "333333";
			testCodes3.Add(code3_1);
			CASCCode3 code3_2 = Factory.New<CASCCode3>();
			code3_2.CY_Data = "666666";
			testCodes3.Add(code3_2);
			item.CASCCodes3 = testCodes3;
		}

		SGCPlaceTestClass CreateTestPlace(string placeCode, string placeType, string placeNA, bool isSCSYO, bool isNonSystemNonLicenced = false)
		{
			SGCPlaceTestClass testLocation = new SGCPlaceTestClass();
			testLocation.Code = placeCode;
			testLocation.Type = placeType;
			testLocation.NameAndAddress = placeNA;
			testLocation.AddressRequired = isSCSYO;
			testLocation.IsNonSystemNonLicenced = isNonSystemNonLicenced;
			Factory.Save();
			return testLocation;
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
