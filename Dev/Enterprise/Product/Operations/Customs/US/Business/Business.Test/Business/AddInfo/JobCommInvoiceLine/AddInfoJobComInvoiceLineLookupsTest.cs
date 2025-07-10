using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLoadingSchDList_RegionDistrictPortsForSpecificLoco_ForeignPortsForSpecificLoco()
		{
			var line = Factory.New<JobComInvoiceLine>();
			AssertNoExceptionThrown("There is no exception when the declaration is null", () =>
			{
				_ = line.AddInfoLookups.LoadingSchDList;
			});
		}

		public void TestOMCDisclaimReasonList()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2020202020";
			tariff.UE_PGACodes = "OM1";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.AddInfoLookups.OMCDisclaimReasonList);
			AssertEquals("AMSDisclaimReasonList", 4, invoiceLine.AddInfoLookups.OMCDisclaimReasonList.Count);
			invoiceLine.JI_Tariff = "2020202020";
			AssertEquals("OMCDisclaimReasonList", 1, invoiceLine.AddInfoLookups.OMCDisclaimReasonList.Count);
		}

		public void TestECCNNumberList()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C35 });

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber);
			var cusCode6A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "6A002", "6A002 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var cusCode5A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "5A002", "5A002 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var cusCode4A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "4A003", "4A003 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var cusCode3A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3A003", "3A003 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode6A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode5A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode4A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C36);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C35;

			var eccnCandidates = invoiceLine.AddInfoLookups.US_ECCNList;
			eccnCandidates.Load();
			AssertEquals("ECCN Numbers with LicenseType C35", 3, eccnCandidates.Count);
			var item1 = invoiceLine.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "6A002").FirstOrDefault();
			var item2 = invoiceLine.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "4A003").FirstOrDefault();
			var item3 = invoiceLine.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "5A002").FirstOrDefault();
			AssertEquals("ECCNList with LicenseType C35 -6A002", "6A002 Description", item1.ZZD_Description);
			AssertEquals("ECCNList with LicenseType C35 -4A003", "4A003 Description", item2.ZZD_Description);
			AssertEquals("ECCNList with LicenseType C35 -5A002", "5A002 Description", item3.ZZD_Description);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C36;
			var eccnCandidates2 = invoiceLine.AddInfoLookups.US_ECCNList;
			eccnCandidates2.Load();
			AssertEquals("ECCN Numbers with LicenseType C36", 1, eccnCandidates2.Count);
			var item4 = invoiceLine.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "3A003").FirstOrDefault();
			AssertEquals("ECCNList with LicenseType C36 -3A003", "3A003 Description", item4.ZZD_Description);
		}

		[TestDate(2018, 10, 22)]
		public void TestSPIListForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine1.US_UC_NKCountryOfExport = "CN";
			invoiceLine1.US_SupTariff = "9802.00.4020";
			var spiList = invoiceLine1.AddInfoLookups.SPIList;
			AssertEquals(4, spiList.Count);
			Assert(spiList.ContainsCode("N/A"));
			Assert(spiList.ContainsCode("C"));
			Assert(spiList.ContainsCode("K"));
			Assert(spiList.ContainsCode("L"));
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = "9802.00.4020";
			invoiceLine2.JI_Tariff = "8803.30.0060";
			invoiceLine2.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine2.US_UC_NKCountryOfExport = "CN";
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			spiList = invoiceLine1.AddInfoLookups.SPIList;
			AssertEquals(4, spiList.Count);
			Assert(spiList.ContainsCode("N/A"));
			Assert(spiList.ContainsCode("C"));
			Assert(spiList.ContainsCode("K"));
			Assert(spiList.ContainsCode("L"));
			spiList = invoiceLine2.AddInfoLookups.SPIList;
			AssertEquals(4, spiList.Count);
			Assert(spiList.ContainsCode("N/A"));
			Assert(spiList.ContainsCode("C"));
			Assert(spiList.ContainsCode("K"));
			Assert(spiList.ContainsCode("L"));
		}

		public void TestManifestUQListForFTZWeeklyEstimateIntegrationEnabled()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = false;
			var orgAddress1 = importer.Addresses.AddNew();
			var cusCode1 = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "A000", "US");
			cusCode1.OK_OA_PremisesAddress = orgAddress1.PK;
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_IsWarehouseClient = true;
			var orgAddress2 = warehouseOrg.Addresses.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_EntryDateElectionCode = ZString.Empty;
			declaration.JE_OH_Importer = importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = orgAddress2.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var list = invoiceLine.AddInfoLookups.ManifestUQList;
			AssertNotNull(list);
			AssertEquals(typeof(CodeDescriptionPairList), list.GetType());
			AssertEquals(RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory), list);
		}

		public void TestManifestUQListNotForFTZWeeklyEstimateIntegrationEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.AddInfoLookups.ManifestUQList);
		}

		public void TestPGADisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.AddInfoLookups.PGADisclaimReasonList);
		}

		public void TestAMSDisclaimReasonList()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1111111111";
			tariff.UE_PGACodes = "AM1";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.AddInfoLookups.AMSDisclaimReasonList);
			AssertEquals("AMSDisclaimReasonList", 4, invoiceLine.AddInfoLookups.AMSDisclaimReasonList.Count);
			invoiceLine.JI_Tariff = "1111111111";
			AssertEquals("AMSDisclaimReasonList", 2, invoiceLine.AddInfoLookups.AMSDisclaimReasonList.Count);
		}

		public void TestNOPDisclaimReasonList()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1111111111";
			tariff.UE_PGACodes = "AM7";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.AddInfoLookups.NOPDisclaimReasonList);
			AssertEquals("NOPDisclaimReasonList", 4, invoiceLine.AddInfoLookups.NOPDisclaimReasonList.Count);
			invoiceLine.JI_Tariff = "1111111111";
			AssertEquals("NOPDisclaimReasonList", 1, invoiceLine.AddInfoLookups.NOPDisclaimReasonList.Count);
		}

		public void TestNMFSDisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("NMFSDisclaimReasonList", invoiceLine.AddInfoLookups.NMFSHMSDisclaimReasonList);
			AssertNotNull("NMFSDisclaimReasonList", invoiceLine.AddInfoLookups.NMFS370DisclaimReasonList);
			AssertNotNull("NMFSDisclaimReasonList", invoiceLine.AddInfoLookups.NMFSAMRDisclaimReasonList);
		}

		public void TestTTBDisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("TTBDisclaimReasonList: US_TTBDisclaimReason exposed in invoice line grid", invoiceLine.AddInfoLookups.TTBDisclaimReasonList);
		}

		public void TestAPHISDisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("APHISDisclaimReasonList: US_APHISDisclaimReason exposed in invoice line grid", invoiceLine.AddInfoLookups.APHISDisclaimReasonList);
		}

		public void TestFWSDisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("FWSDisclaimReasonList: US_FWSDisclaimReason exposed in invoice line grid", invoiceLine.AddInfoLookups.FWSDisclaimReasonList);
		}

		public void TestDEADisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("DEADisclaimReasonList: US_DEADisclaimReason exposed in invoice line grid", invoiceLine.AddInfoLookups.DEADisclaimReasonList);
		}

		public void TestPSTDisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("PSTDisclaimReasonList: US_PSTDisclaimReason exposed in invoice line grid", invoiceLine.AddInfoLookups.PSTDisclaimReasonList);
		}

		public void TestHFCDisclaimReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull("HFCDisclaimReasonList: US_HFCDisclaimReason exposed in invoice line grid", invoiceLine.AddInfoLookups.HFCDisclaimReasonList);
		}

		public void TestTaxCodeList()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = "016";
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "000000002";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = "018";
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			AssertEquals(2, invoiceLine.AddInfoLookups.TaxCodeList.Count);
			//016 and 017 are interchangeable
			Assert(invoiceLine.AddInfoLookups.TaxCodeList.ContainsCode("016"));
			Assert(invoiceLine.AddInfoLookups.TaxCodeList.ContainsCode("017"));
			invoiceLine.JI_Tariff = "000000002";
			AssertEquals(1, invoiceLine.AddInfoLookups.TaxCodeList.Count);
			Assert(invoiceLine.AddInfoLookups.TaxCodeList.ContainsCode("018"));
		}

		public void TestTaxRateList_ProductClaimC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			AssertEquals(1, invoiceLine.AddInfoLookups.TaxRateList.Count);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertEquals(0, invoiceLine.AddInfoLookups.TaxRateList.Count);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals(24, invoiceLine.AddInfoLookups.TaxRateList.Count);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			AssertEquals(2, invoiceLine.AddInfoLookups.TaxRateList.Count);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			AssertEquals(5, invoiceLine.AddInfoLookups.TaxRateList.Count);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertEquals(9, invoiceLine.AddInfoLookups.TaxRateList.Count);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals(7, invoiceLine.AddInfoLookups.TaxRateList.Count);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			AssertEquals(2, invoiceLine.AddInfoLookups.TaxRateList.Count);
		}

		public void TestTaxRateList()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = "016";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.11111m;
			dutyRate.UD_TaxFeeFlag = "1";
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "000000002";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = "022";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
			dutyRate2.UD_TaxFeeAdvalorem = 0.25m;
			dutyRate2.UD_TaxFeeFlag = "1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			AssertEquals(3, invoiceLine.AddInfoLookups.TaxRateList.Count);
			Assert(invoiceLine.AddInfoLookups.TaxRateList.ContainsCode(invoiceLine.ImportTariff.GetTaxFeeRateDescription("016", "")));
			Assert(invoiceLine.AddInfoLookups.TaxRateList.ContainsCode(AppendixBTaxRateList.Codes.DistilledSpirits));
			Assert(invoiceLine.AddInfoLookups.TaxRateList.ContainsCode(AppendixBTaxRateList.Codes.Specify));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(3, invoiceLine.AddInfoLookups.TaxRateList.Count);
			Assert(invoiceLine.AddInfoLookups.TaxRateList.ContainsCode(invoiceLine.ImportTariff.GetTaxFeeRateDescription("016", "")));
			Assert(invoiceLine.AddInfoLookups.TaxRateList.ContainsCode(AppendixBTaxRateList.Codes.DistilledSpirits));
			Assert(invoiceLine.AddInfoLookups.TaxRateList.ContainsCode(AppendixBTaxRateList.Codes.Specify));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.JI_Tariff = "000000002";
			AssertEquals(6, invoiceLine.AddInfoLookups.TaxRateList.Count);
		}

		public void TestCBMATaxRateList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_TaxApply = "Y";
			AssertEquals(0, invoiceLine.AddInfoLookups.CBMATaxRateList.Count);

			invoiceLine.US_TaxCode = "017";
			AssertEquals(24, invoiceLine.AddInfoLookups.CBMATaxRateList.Count);
			AssertArrayEqualsByElements(new string[] { "W01010", "W01020", "W01030" }, invoiceLine.AddInfoLookups.CBMATaxRateList.Cast<ICodeDescription>().Select(s => s.Code).Take(3).ToArray());

			invoiceLine.US_TaxCode = "017";
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_5;
			AssertEquals(3, invoiceLine.AddInfoLookups.CBMATaxRateList.Count);
			AssertArrayEqualsByElements(new string[] { "W07010", "W07020", "W07030" }, invoiceLine.AddInfoLookups.CBMATaxRateList.Cast<ICodeDescription>().Select(s => s.Code).ToArray());

			invoiceLine.US_TaxCode = "017";
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			AssertEquals(24, invoiceLine.AddInfoLookups.CBMATaxRateList.Count);
			AssertArrayEqualsByElements(new string[] { "W01010", "W01020", "W01030" }, invoiceLine.AddInfoLookups.CBMATaxRateList.Cast<ICodeDescription>().Select(s => s.Code).Take(3).ToArray());
		}

		public void TestACEAD_CVDList()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A462105011";
			acCase.U5_CaseStatus = "AC";
			acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caseTariff = acCase.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "98170090";
			var suspension = Factory.New<USCACCaseLiqSuspension>();
			suspension.UN_CaseNumber = acCase.U5_CaseNumber;
			suspension.UN_EffectiveDate = ZDateTime.Now.AddDays(-1);
			suspension.UN_Action = "START";
			suspension.UN_AddedDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9817.00.9040";
			invoiceLine.JI_Tariff = "7201.10.0000";
			invoiceLine.US_ADDCaseNo = "A462105011"; // relevant for sup. tariff
			ZQuery query = invoiceLine.AddInfoLookups.ADDCaseNumberList.CompleteFilter;
			Assert(invoiceLine.AntidumpingDutyCase.MatchesFilter(query));
		}

		[TestDate(2009, 1, 1)]
		public void TestNAIsNotAnOptionWhenSPIBecomesMandatoryWithSup()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99100466";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.JI_Tariff = "1806327000";
			AssertEquals(1, invoiceLine.AddInfoLookups.SPIList.Count);
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("SG"));
			Assert(!invoiceLine.AddInfoLookups.SPIList.ContainsCode("N/A"));
		}

		public void TestWhenParentTariffDoesNotAllowSPIWithSup()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.NoSPIRequired;
			tariffRule.U1_Tariff = "00000000";
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7017103000";
			invoiceLine.US_SupTariff = "00000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			AssertEquals("SG should not appear in the list: Parent", 1, invoiceLine.AddInfoLookups.SPIList.Count);
			AssertEquals("SG should not appear in the list: Secondary Line", 1, invoiceLine.AddInfoLookups.SPIList.Count);
		}

		[TestDate(2009, 6, 1)]
		public void TestSPIFor99117704WithSup()
		{
			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("99117704", ZDateTime.Today);
			IRateWrapper rates = DutyRateWrapper.GetWrapper(ZDateTime.Today, tariff, "CL", "CL", "CL");
			//PreCondition
			AssertNotNull("tariff exists for 99117704", tariff);
			Assert("Tariff has a duty rate of 'Do not declare duty'", rates.IsInvalidDutyRate());
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9911.77.04";
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.JI_Tariff = "0811.10.0050";
			AssertEquals("It should contain 'CL'", true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(SpecialProgramList.Codes.CL));
		}

		[TestDate(2009, 12, 12)]
		public void TestListRefreshedForStandAloneAndSecondaryTariffLines()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00778899";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXSG";
			USCCountry cr = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "CR");
			cr.UC_MiscellaneousSPIIndicator = "P";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9915.61.01";
			invoiceLine.US_UC_NKCountryOfOrigin = "NI";
			invoiceLine.US_UC_NKCountryOfExport = "CR";
			invoiceLine.JI_Tariff = "00778899";
			AssertEquals("P should not be a valid SPI in the list for a secondary line of CAFTA TPL parent line", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode("P"));
			JobComInvoiceLine standAloneLine = declaration.InvoiceLines.AddNew();
			standAloneLine.US_UC_NKCountryOfOrigin = "NI";
			standAloneLine.US_UC_NKCountryOfExport = "CR";
			standAloneLine.JI_Tariff = "00778899";
			AssertEquals("P should be a valid SPI in the list for this stand-alone line", true, standAloneLine.AddInfoLookups.SPIList.ContainsCode("P"));
			Factory.Save();
			//to load invoice lines in HasChanges-free environment
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
			JobComInvoiceLine secondaryLine2 = (JobComInvoiceLine)declarationLoaded.InvoiceLines.FindByPK(invoiceLine.PK);
			AssertEquals("P for the secondary line", false, secondaryLine2.AddInfoLookups.SPIList.ContainsCode("P"));
			JobComInvoiceLine standAloneLine2 = (JobComInvoiceLine)declarationLoaded.InvoiceLines.FindByPK(standAloneLine.PK);
			AssertEquals("P for the stand-alone line", true, standAloneLine2.AddInfoLookups.SPIList.ContainsCode("P"));
		}

		public void TestSPIList()
		{
			//more extensive tests in SPICompleteList
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXP+SG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoExceptionThrown(() => _ = invoiceLine.AddInfoLookups.SPIList);
			invoiceLine.JI_Tariff = "0000";
			AssertNoExceptionThrown(() => _ = invoiceLine.AddInfoLookups.SPIList);
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			CodeDescriptionPairList list = invoiceLine.AddInfoLookups.SPIList;
			AssertEquals("List should have 4 elements including N/A", 4, list.Count);
			AssertEquals("Should contain AU", true, list.ContainsCode(SpecialProgramList.Codes.AU));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
		}

		public void TestLists()
		{
			AssertEquals("US_HazMatQualifierList", typeof(HazMatQualifierList), lookups.US_HazMatQualifierList.GetType());
			AssertEquals("US_CargoStorageCodeList", typeof(CargoStorageCodeList), lookups.US_CargoStorageCodeList.GetType());
			AssertEquals("ReconFeeAndChargeList", typeof(CodeDescriptionPairList), lookups.ReconFeeAndChargeList.GetType());
			AssertEquals("USCarrierList", typeof(USCarrierCombinedCollection), lookups.USCarrierList.GetType());
		}

		public void TestADDCaseNumberList()
		{
			lookups.Parent.InvoiceLine.JI_Tariff = "2203.00.60";
			USCACCaseCollection collection = (USCACCaseCollection)lookups.ADDCaseNumberList;
			AssertEquals("22030060", collection.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
			collection.RefreshFromDb();
			AssertEquals(0, collection.Count);
			USCACCase caseNumber = Factory.New<USCACCase>();
			caseNumber.U5_CaseNumber = "ADD111";
			caseNumber.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			USCACCase caseNumber2 = Factory.New<USCACCase>();
			caseNumber2.U5_CaseNumber = "CVD111";
			caseNumber2.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber2.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			Factory.Save();
			USCACCaseCollection collection2 = (USCACCaseCollection)lookups.ADDCaseNumberList;
			collection2.RefreshFromDb();
			AssertEquals(1, collection2.Count);
		}

		public void TestCVDCaseNumberList()
		{
			lookups.Parent.InvoiceLine.JI_Tariff = "2203.00.60";
			USCACCaseCollection collection = (USCACCaseCollection)lookups.CVDCaseNumberList;
			AssertEquals("22030060", collection.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
			collection.RefreshFromDb();
			AssertEquals(0, collection.Count);
			USCACCase caseNumber = Factory.New<USCACCase>();
			caseNumber.U5_CaseNumber = "CVD111";
			caseNumber.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			Factory.Save();
			USCACCaseCollection collection2 = (USCACCaseCollection)lookups.CVDCaseNumberList;
			collection2.RefreshFromDb();
			AssertEquals(1, collection2.Count);
		}

		public void TestTariffDefaultedForADDList()
		{
			lookups.Parent.InvoiceLine.US_SupTariff = "9803.80.30";
			USCACCaseCollection collection = (USCACCaseCollection)lookups.ADDCaseNumberList;
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tariff Number:Property"));
			AssertEquals("98038030", collection.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
			lookups.Parent.InvoiceLine.JI_Tariff = "2203.00.60";
			collection = (USCACCaseCollection)lookups.ADDCaseNumberList;
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tariff Number:Property"));
			AssertEquals("22030060", collection.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
		}

		public void TestTariffDefaultedForCVDList()
		{
			lookups.Parent.InvoiceLine.US_SupTariff = "9803.80.30";
			USCACCaseCollection collection = (USCACCaseCollection)lookups.CVDCaseNumberList;
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tariff Number:Property"));
			AssertEquals("98038030", collection.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
			lookups.Parent.InvoiceLine.JI_Tariff = "2203.00.60";
			collection = (USCACCaseCollection)lookups.CVDCaseNumberList;
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tariff Number:Property"));
			AssertEquals("22030060", collection.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
		}

		[TestDate(2008, 7, 1)]
		public void TestSPIListForOriginalReconLine()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_SPICode = "AUSG";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 12, 31);
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000000";
			tariff2.UE_SPICode = "AU";
			tariff2.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2008, 12, 31);
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2007, 12, 31);
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_R_OrigTariff = "0000000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			AssertEquals("SG should be in the list", true, invoiceLine.AddInfoLookups.ReconOrigSPIList.ContainsCode("SG"));
		}

		public void TestLoadingSchDList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), invoiceLine.AddInfoLookups.LoadingSchDList.GetType());
			declaration.US_SchDLoading = "2704";
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), invoiceLine.AddInfoLookups.LoadingSchDList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), invoiceLine.AddInfoLookups.LoadingSchDList.GetType());
			declaration.US_SchDLoading = ZString.Empty;
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), invoiceLine.AddInfoLookups.LoadingSchDList.GetType());
		}

		public void TestEPANetQtyUQList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.AddInfoLookups.EPANetQtyUQList);
		}

		public void TestUS_LicenseType_List()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "STL", "STL Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "ALU", "ALU Description", new ZDateTime(1900, 1, 1), ZDateTime.Today.AddYears(-1));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var uS_LicenseType_ListForExp = (USAESLicenseCodeCollection)invoiceLine.AddInfoLookups.US_LicenseType_List;
			uS_LicenseType_ListForExp.Load();
			AssertEquals(USAESLicenseCode.Codes.C30, uS_LicenseType_ListForExp.Cast<ZZRefCusCodeListCombined>().FirstOrDefault().ZZD_Code);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var uS_LicenseType_ListForFTZ = (CodeDescriptionPairList)invoiceLine.AddInfoLookups.US_LicenseType_List;
			AssertEquals(1, uS_LicenseType_ListForFTZ.Count);
			AssertEquals("STL", uS_LicenseType_ListForFTZ[0].Code);
			AssertEquals("STL Description", uS_LicenseType_ListForFTZ[0].Description);
		}

		public void TestProductExclusionList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(2, invoiceLine.AddInfoLookups.ProductExclusionList.Count);
			invoiceLine.JI_Tariff = "1001011001";
			AssertEquals(2, invoiceLine.AddInfoLookups.ProductExclusionList.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._02, invoiceLine.AddInfoLookups.ProductExclusionList[0].Code);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._03, invoiceLine.AddInfoLookups.ProductExclusionList[1].Code);
			invoiceLine.JI_Tariff = "7201011001";
			AssertEquals(1, invoiceLine.AddInfoLookups.ProductExclusionList.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._02, invoiceLine.AddInfoLookups.ProductExclusionList[0].Code);
			invoiceLine.JI_Tariff = "7301011001";
			AssertEquals(1, invoiceLine.AddInfoLookups.ProductExclusionList.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._02, invoiceLine.AddInfoLookups.ProductExclusionList[0].Code);
			invoiceLine.JI_Tariff = "7601011001";
			AssertEquals(1, invoiceLine.AddInfoLookups.ProductExclusionList.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._03, invoiceLine.AddInfoLookups.ProductExclusionList[0].Code);
			invoiceLine.JI_Tariff = "7801011001";
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._02, invoiceLine.AddInfoLookups.ProductExclusionList[0].Code);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._03, invoiceLine.AddInfoLookups.ProductExclusionList[1].Code);
		}

		public void TestControlledGroupNames()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Wrapper = OrgHeaderWrapper.New(org1);
			var org1GroupName = org1Wrapper.ImportersControlledGroupNames.AddNew();
			org1GroupName.US_GroupName = "NAME1";
			org1GroupName = org1Wrapper.ImportersControlledGroupNames.AddNew();
			org1GroupName.US_GroupName = "NAME2";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Wrapper = OrgHeaderWrapper.New(org2);
			var org2GroupName = org2Wrapper.ImportersControlledGroupNames.AddNew();
			org2GroupName.US_GroupName = "NAME3";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.AddInfoLookups.ControlledGroupNames.Count);
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals(2, invoiceLine.AddInfoLookups.ControlledGroupNames.Count);
			AssertEquals("NAME1, NAME2", string.Join(", ", invoiceLine.AddInfoLookups.ControlledGroupNames.GetAllCodes()));
			declaration.JE_OH_Importer = org2.PK;
			AssertEquals(1, invoiceLine.AddInfoLookups.ControlledGroupNames.Count);
			AssertEquals("NAME3", invoiceLine.AddInfoLookups.ControlledGroupNames[0].Code);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(0, invoiceLine.AddInfoLookups.ControlledGroupNames.Count);
		}

		public void TestForeignProducerIdentifiers()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode1 = manufacturer.MainAddress.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "FPI";
			cusCode1.OK_CustomsRegNo = "TTB-FP-0123456";

			var cusCode2 = manufacturer.MainAddress.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "FPB";
			cusCode2.OK_CustomsRegNo = "BABCWIN150522";

			var cusCode3 = manufacturer.MainAddress.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "FPS";
			cusCode3.OK_CustomsRegNo = "SABCWIN150521";

			var cusCode4 = manufacturer.MainAddress.CustomsCodes.AddNew();
			cusCode4.OK_CodeType = "FPW";
			cusCode4.OK_CustomsRegNo = "WABCWIN150521";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Wrapper = OrgHeaderWrapper.New(org1);

			var org1QtyPerFPI1 = org1Wrapper.AllocationQuantityPerFPIs.AddNew();
			org1QtyPerFPI1.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			org1QtyPerFPI1.US_ForeignProducerIdentifier = "BABCWIN150522";

			var org1QtyPerFPI2 = org1Wrapper.AllocationQuantityPerFPIs.AddNew();
			org1QtyPerFPI2.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			org1QtyPerFPI2.US_ForeignProducerIdentifier = "SABCWIN150521";

			var org1QtyPerFPI3 = org1Wrapper.AllocationQuantityPerFPIs.AddNew();
			org1QtyPerFPI3.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			org1QtyPerFPI3.US_ForeignProducerIdentifier = "WABCWIN150521";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org1.PK;
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			AssertEquals("IsCBMA23Effective", true, invoiceLine.IsCBMA23Effective);
			AssertEquals(1, invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.Count);
			AssertCollectionContains("TTB-FP-0123456", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(4, invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.Count);
			AssertCollectionContains("TTB-FP-0123456", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
			AssertCollectionContains("BABCWIN150522", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
			AssertCollectionContains("SABCWIN150521", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
			AssertCollectionContains("WABCWIN150521", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			AssertEquals("IsCBMA23Effective", false, invoiceLine.IsCBMA23Effective);
			AssertEquals(3, invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.Count);
			AssertCollectionContains("BABCWIN150522", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
			AssertCollectionContains("SABCWIN150521", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
			AssertCollectionContains("WABCWIN150521", invoiceLine.AddInfoLookups.ForeignProducerIdentifiers.GetAllCodes());
		}

		public void TestNoExceptionWhenNoInvoiceHeader_Issue01468281()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNoExceptionThrown("There should be no exception when calling the list getter if there is no InvoiceHeader attached to an InvoiceLine", () => _ = new AddInfoJobComInvoiceLineLookups(invoiceLine.GetAddInfo()).TSCADisclaimReasonList);
		}

		public void TestTaxRateTypeList()
		{
			CreateTestDateForTaxRateTypeListAndOrigTaxRateTypeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertTaxRateTypeList(false, invoiceLine.AddInfoLookups.TaxRateTypeList);
			invoiceLine.JI_Tariff = "20230106";
			AssertTaxRateTypeList(true, invoiceLine.AddInfoLookups.TaxRateTypeList);
			invoiceLine.US_TaxCode = "107";
			AssertTaxRateTypeList(false, invoiceLine.AddInfoLookups.TaxRateTypeList);
			invoiceLine.US_TaxCode = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertTaxRateTypeList(false, invoiceLine.AddInfoLookups.TaxRateTypeList);
		}

		public void TestOrigTaxRateTypeList()
		{
			CreateTestDateForTaxRateTypeListAndOrigTaxRateTypeList();
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			var invoice = entry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertTaxRateTypeList(false, invoiceLine.AddInfoLookups.OrigTaxRateTypeList);
			invoiceLine.US_R_OrigTariff = "20230106";
			AssertTaxRateTypeList(true, invoiceLine.AddInfoLookups.OrigTaxRateTypeList);
			invoiceLine.US_R_OrigTaxCode = "107";
			AssertTaxRateTypeList(false, invoiceLine.AddInfoLookups.OrigTaxRateTypeList);
			invoiceLine.US_R_OrigTaxCode = ZString.Empty;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			AssertTaxRateTypeList(false, invoiceLine.AddInfoLookups.OrigTaxRateTypeList);
		}

		void AssertTaxRateTypeList(bool descriptionHasRateValue, CodeDescriptionPairList actualValue)
		{
			if (!descriptionHasRateValue)
			{
				AssertEquals(2, actualValue.Count);
				AssertEquals(RateTypeList.Descriptions.Primary, actualValue.GetDescriptionFromCode(RateTypeList.Codes.Primary));
				AssertEquals(RateTypeList.Descriptions.Secondary, actualValue.GetDescriptionFromCode(RateTypeList.Codes.Secondary));
			}
			else
			{
				AssertEquals(2, actualValue.Count);
				AssertEquals(RateTypeList.Descriptions.Primary + " - 0.1", actualValue.GetDescriptionFromCode(RateTypeList.Codes.Primary));
				AssertEquals(RateTypeList.Descriptions.Secondary + " - 0.2", actualValue.GetDescriptionFromCode(RateTypeList.Codes.Secondary));
			}
		}

		void CreateTestDateForTaxRateTypeListAndOrigTaxRateTypeList()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "20230106";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate.UD_TaxFeeSpecificRate = 0.1m;
			dutyRate.UD_TaxFeeAdvalorem = 0.2m;
		}

		AddInfoJobComInvoiceLineLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineAddInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceLineLookups(invoiceLineAddInfo);
		}
	}
}
