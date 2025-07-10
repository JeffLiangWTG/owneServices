
using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	[TestedType(typeof(NZDocsMAFCoverSheet))]
	sealed class NZDocsMAFCoverSheetTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultAccountHolder()
		{
			declaration.FillWithValidTestData();
			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;

			GlbCompany.CurrentCompany.GC_Name = "UNHOLY WINES";
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("UNHOLY WINES", coverSheet.D0_AccountHolder);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "PURE WINE";
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("UNHOLY WINES", coverSheet.D0_AccountHolder);

			var code = importer.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.NZCodeTypes.MAFCoverSheetQE;
			code.OK_CustomsRegNo = "43214125";

			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("PURE WINE", coverSheet.D0_AccountHolder);
		}

		public void TestDefaultTransitionalFacility()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "NEVER SHOW";
			org.OH_FullName = "SHOW ME";
			org.MainAddress.OA_Address1 = "THE";
			org.MainAddress.OA_Address2 = "FRICKIN";
			org.MainAddress.OA_City = "MONEY";
			org.MainAddress.OA_PostCode = "BRAIN";
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;

			var atfCode = org.CustomsCodes.AddNew();
			atfCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			atfCode.OK_RN_NKCodeCountry = "NZ";
			atfCode.OK_CustomsRegNo = "CODE1";
			atfCode.OK_OA_PremisesAddress = org.MainAddress.PK;

			declaration.JE_OH_Importer = org.PK;
			var maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("SHOW ME, THE, FRICKIN, MONEY BRAIN", maf.D0_TransitionalFacility);

			var depot = Factory.New<OrgHeader>();
			depot.OH_FullName = "LA-LA-LAND";
			atfCode = depot.CustomsCodes.AddNew();
			atfCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			atfCode.OK_RN_NKCodeCountry = "NZ";
			atfCode.OK_CustomsRegNo = "CODE1";
			atfCode.OK_OA_PremisesAddress = depot.MainAddress.PK;

			var depotAddress = depot.MainAddress;
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			depotAddress.OA_Address1 = "THIS IS LINE 1";
			depotAddress.OA_Address2 = "THIS IS LINE 2";
			depotAddress.OA_City = "THIS IS THE CITY";
			depotAddress.OA_RN_NKCountryCode = ZString.Empty;

			maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("LA-LA-LAND, THIS IS LINE 1, THIS IS LINE 2, THIS IS THE CITY", maf.D0_TransitionalFacility);
		}

		public void TestDefaultTransitionalFacilityWithATFCode()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var deliveryAddress = Factory.New<OrgAddress>();
			declaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			deliveryAddress.OA_OH = importer.PK;
			deliveryAddress.OA_Address1 = "YOKOHAMASHI";
			deliveryAddress.OA_Address2 = "KANAZAWAKU";
			deliveryAddress.OA_City = "KAMARIYANISHI 1-63-27";
			deliveryAddress.OA_RN_NKCountryCode = ZString.Empty;

			var wrongAddress = Factory.New<OrgAddress>();
			wrongAddress.OA_OH = importer.PK;
			wrongAddress.OA_Address1 = "SENDAISHI";
			wrongAddress.OA_Address2 = "WAKAYAMAKU";
			wrongAddress.OA_City = "MINAMIKOIZUMI 4-24-6";

			var atfCode1 = importer.CustomsCodes.AddNew();
			atfCode1.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			atfCode1.OK_RN_NKCodeCountry = "NZ";
			atfCode1.OK_CustomsRegNo = "67890";
			atfCode1.OK_OA_PremisesAddress = wrongAddress.PK;

			var atfCode2 = importer.CustomsCodes.AddNew();
			atfCode2.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			atfCode2.OK_RN_NKCodeCountry = "NZ";
			atfCode2.OK_CustomsRegNo = "12345";
			atfCode2.OK_OA_PremisesAddress = deliveryAddress.PK;

			CodeDataPair codeDataPair = declaration.OtherInfos.AddNew();
			codeDataPair.ZO_Code = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			codeDataPair.ZO_Data = atfCode2.OK_CustomsRegNo;

			var maf = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals("YOKOHAMASHI, KANAZAWAKU, KAMARIYANISHI 1-63-27", maf.D0_TransitionalFacility);
		}

		public void TestTariffsThatRequirePermitCodes()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3502.20.00.00C";

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00.00Z";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("maf.D0_EDITariffCodes", "3502.20.00.00C", maf.D0_EDITariffCodes);
		}

		public void TestComoditiesOnlyIncludesTariffItemsNeedingPermits()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3502.20.00.00C";

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00.00Z";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("maf.Commodities.Count", 1, maf.Commodities.Count);
		}

		public void TestCustomsQuantityWithUnitFormatting()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3502.20.00.00C";
			invoiceLine.JI_CustomsQuantity = 3000;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals(maf.Commodities[0].D1_QuantityWithUnit, "3000 KGM");

			invoiceLine.JI_CustomsQuantity = 1.1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			maf = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals(maf.Commodities[0].D1_QuantityWithUnit, "1.1 KGM");

			invoiceLine.JI_CustomsQuantity = 1.0001;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			maf = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals(maf.Commodities[0].D1_QuantityWithUnit, "1.0001 KGM");

			invoiceLine.JI_CustomsQuantity = 1.10000;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			maf = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals(maf.Commodities[0].D1_QuantityWithUnit, "1.1 KGM");
		}

		public void TestMeasureWithUnit()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3502.20.00.00C";

			invoiceLine.JI_Weight = 3000;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("3000 KG", maf.Commodities[0].D1_MeasureWithUnit);

			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_WeightUQ = ZString.Empty;
			invoiceLine.JI_Volume = 9000;
			invoiceLine.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("9000 M3", maf.Commodities[0].D1_MeasureWithUnit);

			invoiceLine.JI_Weight = 3000;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Volume = 9000;
			invoiceLine.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			maf = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("3000 KG", maf.Commodities[0].D1_MeasureWithUnit);
		}

		public void TestDefaultMAFQENumber()
		{
			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("D0_PayByAccount", true, coverSheet.D0_PayByAccount);
			AssertEquals("D0_PayBeCash", false, coverSheet.D0_PayBeCash);
			AssertEquals("D0_PayBeCheque", false, coverSheet.D0_PayBeCheque);
			AssertEquals("D0_QENumber", string.Empty, coverSheet.D0_QENumber);
			AssertEquals("D0_AccountHolder", "Eagle Datamation International", coverSheet.D0_AccountHolder);

			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Cash;
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("D0_PayByAccount", false, coverSheet.D0_PayByAccount);
			AssertEquals("D0_PayBeCash", true, coverSheet.D0_PayBeCash);
			AssertEquals("D0_PayBeCheque", false, coverSheet.D0_PayBeCheque);
			AssertEquals("D0_QENumber", string.Empty, coverSheet.D0_QENumber);
			AssertEquals("D0_AccountHolder", string.Empty, coverSheet.D0_AccountHolder);

			var code = declaration.Branch.OrgProxy.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.NZCodeTypes.MAFCoverSheetQE;
			code.OK_CustomsRegNo = "77884561";
			mafMessaging.ZX_PaymentMethod = ZString.Empty;
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("D0_PayByAccount", true, coverSheet.D0_PayByAccount);
			AssertEquals("D0_PayBeCash", false, coverSheet.D0_PayBeCash);
			AssertEquals("D0_PayBeCheque", false, coverSheet.D0_PayBeCheque);
			AssertEquals("D0_QENumber", "77884561", coverSheet.D0_QENumber);
			AssertEquals("D0_AccountHolder", GlbBranch.CurrentBranch.OrgProxy.OH_FullName, coverSheet.D0_AccountHolder);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "TEST IMPORTER";
			code = importer.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.NZCodeTypes.MAFCoverSheetQE;
			code.OK_CustomsRegNo = "43214125";
			declaration.JE_OH_Importer = importer.PK;

			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("D0_PayByAccount", true, coverSheet.D0_PayByAccount);
			AssertEquals("D0_QENumber", "43214125", coverSheet.D0_QENumber);
			AssertEquals("D0_AccountHolder", importer.OH_FullName, coverSheet.D0_AccountHolder);

			mafMessaging.ZX_AccountHolder = "Account Holder";
			mafMessaging.ZX_AccountNumber = "123456789";
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("D0_PayByAccount", true, coverSheet.D0_PayByAccount);
			AssertEquals("D0_QENumber", "123456789", coverSheet.D0_QENumber);
			AssertEquals("D0_AccountHolder", "Account Holder", coverSheet.D0_AccountHolder);
		}

		public void TestLoadDefaultMAFOfficeFromDeclaration()
		{
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForImportFromAU();
			declaration.JE_RL_NKProcessingPort = MAFOfficesList_DescriptionsOnly.Descriptions.Gisborne;

			decCreator.SetupTestForAir();
			AssertEquals("D0_ToMAFQuarantineService", MAFOfficesList_DescriptionsOnly.Codes.AucklandAirCargo, new NZDocsMAFCoverSheet(mafMessaging).D0_ToMAFQuarantineService);

			decCreator.SetupTestForSea();
			AssertEquals("D0_ToMAFQuarantineService", MAFOfficesList_DescriptionsOnly.Codes.Auckland, new NZDocsMAFCoverSheet(mafMessaging).D0_ToMAFQuarantineService);

			declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			AssertEquals("D0_ToMAFQuarantineService", MAFOfficesList_DescriptionsOnly.Codes.Auckland, new NZDocsMAFCoverSheet(mafMessaging).D0_ToMAFQuarantineService);

			declaration.JE_RL_NKPortOfArrival = "NZGIS";
			AssertEquals("D0_ToMAFQuarantineService", MAFOfficesList_DescriptionsOnly.Codes.Gisborne, new NZDocsMAFCoverSheet(mafMessaging).D0_ToMAFQuarantineService);

			mafMessaging.ZX_ProcessingOffice = MAFProcessingOfficeList.Codes.Tauranga;
			AssertEquals("D0_ToMAFQuarantineService", MAFOfficesList_DescriptionsOnly.Codes.Tauranga, new NZDocsMAFCoverSheet(mafMessaging).D0_ToMAFQuarantineService);
		}

		public void TestLoadDefaultDataFromDeclaration()
		{
			var user = GlbStaff.CurrentUser;
			user.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			user.GS_FullName = "Test User";
			user.GS_EmailAddress = "test@cargowise.com";
			user.GS_FaxNum = "0255559999";
			user.GS_WorkPhone = "0211113333";

			declaration.JE_DeclarationReference = "B01001001";
			declaration.JE_OwnerRef = "OWN12345";
			declaration.CusEntryHeader.EntryNumber = "78945612";
			var decCreator = new TestFormalEntryCreator(declaration);

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals("D0_AgentCompanyName", "Eagle Datamation International", coverSheet.D0_AgentCompanyName);
			AssertEquals("D0_AgentContactName", "Test User", coverSheet.D0_AgentContactName);
			AssertEquals("D0_AgentEmail", "test@cargowise.com", coverSheet.D0_AgentEmail);
			AssertEquals("D0_AgentFaxNumber", "0255559999", coverSheet.D0_AgentFaxNumber);
			AssertEquals("D0_AgentFaxNumber_Formatted", "0255559999", coverSheet.D0_AgentFaxNumber_Formatted);
			AssertEquals("D0_AgentPhoneNumber", "0211113333", coverSheet.D0_AgentPhoneNumber);
			AssertEquals("D0_AgentPhoneNumber_Formatted", "0211113333", coverSheet.D0_AgentPhoneNumber_Formatted);
			AssertEquals("D0_ClientReference", "B01001001 / OWN12345", coverSheet.D0_ClientReference);
			AssertEquals("D0_EntryNumber", "78945612", coverSheet.D0_EntryNumber);

			AssertEquals("D0_ImporterName", "ADULT BOOKS LTD", coverSheet.D0_ImporterName);
			AssertEquals("D0_ExporterName", "TEST SUPPLIER AU", coverSheet.D0_ExporterName);
			AssertEquals("D0_ShippingOrAirLine", "DHL INTERNATIONAL LTD", coverSheet.D0_ShippingOrAirLine);
			AssertEquals("D0_HouseBill", "HOUSETEST123, HOUSEBILL1", coverSheet.D0_HouseBill);
			AssertEquals("D0_MasterBill", "OBL123456", coverSheet.D0_MasterBill);
			AssertEquals("D0_RL_NKDestination", "NZAKL", coverSheet.D0_RL_NKDestination);
			AssertEquals("D0_RL_NKPortOfDischarge", "NZAKL", coverSheet.D0_RL_NKPortOfDischarge);
			AssertEquals("D0_RN_NKCountryOfOrigin", "AU", coverSheet.D0_RN_NKCountryOfOrigin);
			AssertEquals("D0_DateOfArrival", new ZDateTime(2004, 1, 1), coverSheet.D0_DateOfArrival);
			AssertEquals("D0_Vessel", "BUNGA BIDARA", coverSheet.D0_Vessel);
			AssertEquals("D0_VoyageOrFlight", "109", coverSheet.D0_VoyageOrFlight);

			AssertEquals("D0_EDITariffCodes", "", coverSheet.D0_EDITariffCodes);

			AssertEquals("D0_SuppliedBillOfLading", false, coverSheet.D0_SuppliedBillOfLading);
			AssertEquals("D0_SuppliedCertificates", false, coverSheet.D0_SuppliedCertificates);
			AssertEquals("D0_SuppliedComplianceAgreement", false, coverSheet.D0_SuppliedComplianceAgreement);
			AssertEquals("D0_SuppliedComplianceCheckCompleted", false, coverSheet.D0_SuppliedComplianceCheckCompleted);
			AssertEquals("D0_SuppliedIHS", false, coverSheet.D0_SuppliedIHS);
			AssertEquals("D0_SuppliedImportPermit", false, coverSheet.D0_SuppliedImportPermit);
			AssertEquals("D0_SuppliedOtherDocumentation", "", coverSheet.D0_SuppliedOtherDocumentation);
			AssertEquals("D0_SuppliedQuarantineDeclaration", false, coverSheet.D0_SuppliedQuarantineDeclaration);
			AssertEquals("D0_SuppliedRelevantInvoices", false, coverSheet.D0_SuppliedRelevantInvoices);

			AssertEquals("D0_TransitionalFacility", "", coverSheet.D0_TransitionalFacility);
			AssertEquals("D0_TreatmentSupplier", "", coverSheet.D0_TreatmentSupplier);

			AssertEquals("D0_PayBeCash", true, coverSheet.D0_PayBeCash);
			AssertEquals("D0_PayBeCheque", false, coverSheet.D0_PayBeCheque);
			AssertEquals("D0_PayByAccount", false, coverSheet.D0_PayByAccount);
			AssertEquals("D0_AccountHolder", "", coverSheet.D0_AccountHolder);
			AssertEquals("D0_QENumber", "", coverSheet.D0_QENumber);

			AssertEquals("D0_SignatoryCompanyName", "Eagle Datamation International", coverSheet.D0_SignatoryCompanyName);
			AssertEquals("D0_SignatoryName", GlbStaff.CurrentUser.GS_FullName, coverSheet.D0_SignatoryName);
			AssertEquals("D0_SignatoryPhoneNumber", "0211113333", coverSheet.D0_SignatoryPhoneNumber);
			AssertEquals("D0_DateSigned", declaration.CachedTodaysDate, coverSheet.D0_DateSigned);
		}

		public void TestLoadDefaultDataFromConsol()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01001001";
			consol.JK_AgentsReference = "OWN12345";
			new TestDataBuilder(Factory).PopulateConsolThatPassesValidation(consol);
			mafMessaging = TestDataBuilder.GetMAFMessaging(consol);
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals("D0_AgentCompanyName", "CARGOWISE BROKERS", coverSheet.D0_AgentCompanyName);
			AssertEquals("D0_AgentContactName", GlbStaff.CurrentUser.GS_FullName, coverSheet.D0_AgentContactName);
			AssertEquals("D0_AgentEmail", "broker@brokers.cargowise.com", coverSheet.D0_AgentEmail);
			AssertEquals("D0_AgentFaxNumber", "99887766", coverSheet.D0_AgentFaxNumber);
			AssertEquals("D0_AgentPhoneNumber", "44556677", coverSheet.D0_AgentPhoneNumber);
			AssertEquals("D0_ClientReference", "C01001001 / OWN12345", coverSheet.D0_ClientReference);
			AssertEquals("D0_EntryNumber", string.Empty, coverSheet.D0_EntryNumber);

			AssertEquals("D0_ImporterName", "IMPORTER INCORPORATED", coverSheet.D0_ImporterName);
			AssertEquals("D0_ExporterName", "SUPPLIER PTY LTD", coverSheet.D0_ExporterName);
			AssertEquals("D0_ShippingOrAirLine", "IMPORTER INCORPORATED", coverSheet.D0_ShippingOrAirLine);
			AssertEquals("D0_HouseBill", "BOL01010101, BOL01010102", coverSheet.D0_HouseBill);
			AssertEquals("D0_MasterBill", "BR298032", coverSheet.D0_MasterBill);
			AssertEquals("D0_RL_NKDestination", "NZNPE", coverSheet.D0_RL_NKDestination);
			AssertEquals("D0_RL_NKPortOfDischarge", "NZCHC", coverSheet.D0_RL_NKPortOfDischarge);
			AssertEquals("D0_RN_NKCountryOfOrigin", "AU", coverSheet.D0_RN_NKCountryOfOrigin);
			AssertEquals("D0_DateOfArrival", new DateTime(2009, 2, 1), coverSheet.D0_DateOfArrival);
			AssertEquals("D0_Vessel", "BUNGA DELIMA", coverSheet.D0_Vessel);
			AssertEquals("D0_VoyageOrFlight", "3599", coverSheet.D0_VoyageOrFlight);

			var commodity = coverSheet.Commodities[0];
			AssertEquals("D1_CommodityOrSpecies", "FAK", commodity.D1_CommodityOrSpecies);
			AssertEquals("D1_CommodityOrSpecies", "24 UNT", commodity.D1_QuantityWithUnit);
			AssertEquals("D1_CommodityOrSpecies", "22 KG", commodity.D1_MeasureWithUnit);

			var container = coverSheet.Containers[0];
			AssertEquals("D2_ContainerNumber", "OOCL0000023", container.D2_ContainerNumber);
			AssertEquals("D2_IsFCL", true, container.D2_IsFCL);
			AssertEquals("D2_IsLCL", false, container.D2_IsLCL);

			AssertEquals("D0_EDITariffCodes", "", coverSheet.D0_EDITariffCodes);

			AssertEquals("D0_SuppliedBillOfLading", false, coverSheet.D0_SuppliedBillOfLading);
			AssertEquals("D0_SuppliedCertificates", false, coverSheet.D0_SuppliedCertificates);
			AssertEquals("D0_SuppliedComplianceAgreement", false, coverSheet.D0_SuppliedComplianceAgreement);
			AssertEquals("D0_SuppliedComplianceCheckCompleted", false, coverSheet.D0_SuppliedComplianceCheckCompleted);
			AssertEquals("D0_SuppliedIHS", false, coverSheet.D0_SuppliedIHS);
			AssertEquals("D0_SuppliedImportPermit", false, coverSheet.D0_SuppliedImportPermit);
			AssertEquals("D0_SuppliedOtherDocumentation", "Exporter Declaration", coverSheet.D0_SuppliedOtherDocumentation);
			AssertEquals("D0_SuppliedQuarantineDeclaration", false, coverSheet.D0_SuppliedQuarantineDeclaration);
			AssertEquals("D0_SuppliedRelevantInvoices", false, coverSheet.D0_SuppliedRelevantInvoices);

			AssertEquals("D0_TransitionalFacility", "TRANSITIONAL FACILITY, 45 TRANSITIONAL ROAD, FACILITY TOPS, TRANSITIONAL 4389, NZ", coverSheet.D0_TransitionalFacility);
			AssertEquals("D0_TreatmentSupplier", "", coverSheet.D0_TreatmentSupplier);

			AssertEquals("D0_PayBeCash", false, coverSheet.D0_PayBeCash);
			AssertEquals("D0_PayBeCheque", false, coverSheet.D0_PayBeCheque);
			AssertEquals("D0_PayByAccount", true, coverSheet.D0_PayByAccount);
			AssertEquals("D0_AccountHolder", "EDI CUSTOMS BROKERS", coverSheet.D0_AccountHolder);
			AssertEquals("D0_QENumber", "AB123", coverSheet.D0_QENumber);

			AssertEquals("D0_SignatoryCompanyName", "CARGOWISE BROKERS", coverSheet.D0_SignatoryCompanyName);
			AssertEquals("D0_SignatoryName", GlbStaff.CurrentUser.GS_FullName, coverSheet.D0_SignatoryName);
			AssertEquals("D0_SignatoryPhoneNumber", "44556677", coverSheet.D0_SignatoryPhoneNumber);
			AssertEquals("D0_DateSigned", declaration.CachedTodaysDate, coverSheet.D0_DateSigned);
		}

		public void TestTreatmentSupplier()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			new TestDataBuilder(declaration).PopulateDeclarationForAir();
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
			AssertEquals("D0_TreatmentSupplier", "TREATMENT PROVIDER, 15 TREATMENT ROAD, BACK OF TREATMENT, TREATMENT 2323, NZ", coverSheet.D0_TreatmentSupplier);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSuppliedDocumentation()
		{
			var fileNameWithPath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\SampleBitmap.bmp";
			var fileNameWithPath1 = BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\Sample2PageTIFF.TIF";
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.BillofLading);
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.Invoice);
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.PermitToImportSeed);
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.CertificateOfOrigin);
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.QuarantineDeclaration);
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.ExporterDeclaration);
			AddFileOrDocument(fileNameWithPath, DocumentTypeList.Codes.Other);
			AddFileOrDocument(fileNameWithPath1, DocumentTypeList.Codes.Other);
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);

			AssertEquals("D0_SuppliedBillOfLading", true, coverSheet.D0_SuppliedBillOfLading);
			AssertEquals("D0_SuppliedCertificates", true, coverSheet.D0_SuppliedCertificates);
			AssertEquals("D0_SuppliedComplianceAgreement", false, coverSheet.D0_SuppliedComplianceAgreement);
			AssertEquals("D0_SuppliedComplianceCheckCompleted", false, coverSheet.D0_SuppliedComplianceCheckCompleted);
			AssertEquals("D0_SuppliedIHS", false, coverSheet.D0_SuppliedIHS);
			AssertEquals("D0_SuppliedImportPermit", true, coverSheet.D0_SuppliedImportPermit);
			AssertEquals("D0_SuppliedQuarantineDeclaration", true, coverSheet.D0_SuppliedQuarantineDeclaration);
			AssertEquals("D0_SuppliedRelevantInvoices", true, coverSheet.D0_SuppliedRelevantInvoices);
			AssertEquals("D0_SuppliedOtherDocumentation", "Exporter Declaration, SampleBitmap, Sample2PageTIFF", coverSheet.D0_SuppliedOtherDocumentation);
		}

		public void TestCommodities()
		{
			var commodity = coverSheet.Commodities.AddNew();
			AssertEquals(commodity, coverSheet.Commodities[0]);
		}

		public void TestContainers()
		{
			var container = coverSheet.Containers.AddNew();
			AssertEquals(container, coverSheet.Containers[0]);
		}

		#region Implementation

		void AddFileOrDocument(string path, string documentType)
		{
			using (var stream = File.OpenRead(path))
			{
				var contents = new byte[stream.Length];
				stream.Read(contents, 0, (int)stream.Length);
				var fileName = Path.GetFileName(path);

				AddFileOrDocument(contents, fileName, documentType);
			}
		}

		void AddFileOrDocument(byte[] contents, string fileName, string documentType)
		{
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(contents, fileName, documentType);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_DocumentType = eDoc.DocType;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			file.ZF_FileName = fileName;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return coverSheet;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			coverSheet = new NZDocsMAFCoverSheet(mafMessaging);
		}

		JobDeclaration declaration;
		MAFMessagingBO mafMessaging;
		NZDocsMAFCoverSheet coverSheet;

		#endregion
	}
}
