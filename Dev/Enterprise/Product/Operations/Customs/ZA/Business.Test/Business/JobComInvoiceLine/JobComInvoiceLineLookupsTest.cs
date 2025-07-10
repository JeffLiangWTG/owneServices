using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPermits()
		{
			var permit1 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_Type = PermitTypeList.Codes.IMP;
			var permit2 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit2.CPH_Type = PermitTypeList.Codes.EXP;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var impPermits = invoiceLine.Lookups.Permits;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var expPermits = invoiceLine.Lookups.Permits;
			AssertEquals("Import Permit", "IMP", impPermits.FilterBusinessObjectDefaults[CusPermitHeaderCollection.FilterConstants.PermitTypeSubType + ":Property1"].Value);
			AssertEquals("Export Permit", "EXP", expPermits.FilterBusinessObjectDefaults[CusPermitHeaderCollection.FilterConstants.PermitTypeSubType + ":Property1"].Value);
		}

		delegate ICollection GetICollectionDelegate(JobComInvoiceLineLookups lookups);
		public void TestLookupsListAreCached()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceUQList = dec.Invoices.AddNew().JobComInvoiceLines.AddNew().Lookups.InvoiceUQList;
			AssertListIsCached(ZAJobMessageTypeList.Codes.Import, new KeyValuePair<ICollection, GetICollectionDelegate>(invoiceUQList, x => x.InvoiceUQList));
			AssertListIsCached(ZAJobMessageTypeList.Codes.Export, new KeyValuePair<ICollection, GetICollectionDelegate>(invoiceUQList, x => x.InvoiceUQList));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				AssertEquals(expected: true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today));
				var invoiceUQUNE20CodeList = dec.Invoices.AddNew().JobComInvoiceLines.AddNew().Lookups.InvoiceUQUNE20CodeList;
				AssertListIsCached(ZAJobMessageTypeList.Codes.Import, new KeyValuePair<ICollection, GetICollectionDelegate>(invoiceUQUNE20CodeList, x => x.InvoiceUQUNE20CodeList));
				AssertListIsCached(ZAJobMessageTypeList.Codes.Export, new KeyValuePair<ICollection, GetICollectionDelegate>(invoiceUQUNE20CodeList, x => x.InvoiceUQUNE20CodeList));
			}
		}

		public void TestTradeAgreements()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var preference = testHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", startDate, endDate);
			var tariff2 = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304060", startDate, endDate);
			var rateType_ZA_REB = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = testHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var tradeGroup1 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, description: "Standard Agreement");
			testHelper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var tradeGroup2 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, description: "SADC Trade Agreement 2000");
			testHelper.AddCountry(tradeGroup2, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Factory.Save();
			var tariff1Rate = testHelper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			Factory.Save();
			var testApplicability1 = testHelper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability2 = testHelper.CreateCusApplicability(tariff1Rate, tradeGroup2, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.JI_Tariff = "1020304050";
			var list1 = invoiceLine.Lookups.ROOTypeOrPreferenceList;
			AssertEquals("Pref List contains correct item count", list1.Count, 1);
			AssertEquals("100", ((ReadOnlyCodeDescriptionPairList)list1).GetAllCodes()[0]);
			AssertEquals("STANDARD", ((ReadOnlyCodeDescriptionPairList)list1).GetDescriptionFromCode("100"));
		}

		public void TestInvoiceUQListHasNX()
		{
			JobComInvoiceLine line = Factory.New<JobComInvoiceLine>();
			AssertEquals(true, line.Lookups.InvoiceUQList.ContainsCode("NX"));
		}

		public void TestInvoiceUQListIsSortedAfterAddingNX()
		{
			JobComInvoiceLine line = Factory.New<JobComInvoiceLine>();
			int nXIndex = line.Lookups.InvoiceUQList.IndexOfCode("NX");
			string oneBeforeNX = line.Lookups.InvoiceUQList[nXIndex - 1].Code;
			string oneAfterNX = line.Lookups.InvoiceUQList[nXIndex + 1].Code;
			AssertEquals(1, "NX".CompareTo(oneBeforeNX));
			AssertEquals(-1, "NX".CompareTo(oneAfterNX));
		}

		public void TestInvoiceUQUNE20CodeList()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				AssertEquals(expected: true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today));

				var helper = new ZAUniversalReferenceTestDataHelper(Factory);
				_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "INVUQ", "2I", "British thermal unit (international table) per hour", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				var line = Factory.New<JobComInvoiceLine>();
				var lookups = new JobComInvoiceLineLookups(line);
				AssertType<ZZRefCusCodeListCombinedCollection>("InvoiceUQUNE20CodeList Lookup Type", lookups.InvoiceUQUNE20CodeList);

				var codeList = (ZZRefCusCodeListCombinedCollection)lookups.InvoiceUQUNE20CodeList;
				codeList.Load();

				AssertEquals("InvoiceUQUNE20CodeList contains single ZA INVUQ item", 1, codeList.Count);
				AssertEquals("InvoiceUQUNE20CodeList item", "2I", codeList[0].ZZD_Code);
			}
		}

		public void TestCustomsProcedureCodesAndPreviousProcedureCodes()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "00", "", "", "IMP,EXW", "");
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "40", "", "", "IMP,EXW", "");
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "13", "40", "", "", "IMP,EXP,EXW", "");
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "13", "44", "", "", "IMP,EXW", "");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "13";
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = "XX";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var testLookups = invLine.Lookups;
			CombineAssertions("TestCPC", () =>
			{
				AssertEquals("Test Count", 3, testLookups.CustomsProcedureCodes.Count);
				AssertEquals("Test 1", true, testLookups.CustomsProcedureCodes.ContainsCode("11"));
				AssertEquals("Test 2", true, testLookups.CustomsProcedureCodes.ContainsCode("13"));
				AssertEquals("Test 3", true, testLookups.CustomsProcedureCodes.ContainsCode("XX"));
			});
			CombineAssertions("Test PPC Import", () =>
			{
				invLine.JI_CEI = testInstruction1.PK;
				AssertEquals("TEST 1", 2, testLookups.Procedures.Count);
				invLine.JI_CEI = testInstruction2.PK;
				AssertEquals("TEST 2", 2, testLookups.Procedures.Count);
				invLine.JI_CEI = testInstruction3.PK;
				AssertEquals("TEST 3", 0, testLookups.Procedures.Count);
			});
			declaration.JE_MessageType = declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			CombineAssertions("Test PPC Exbond", () =>
			{
				invLine.JI_CEI = testInstruction1.PK;
				AssertEquals("TEST 1", 2, testLookups.Procedures.Count);
				invLine.JI_CEI = testInstruction2.PK;
				AssertEquals("TEST 2", 2, testLookups.Procedures.Count);
				invLine.JI_CEI = testInstruction3.PK;
				AssertEquals("TEST 3", 0, testLookups.Procedures.Count);
			});
		}

		public void TestCPCList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "13", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "XX", "444", "Four", "EXP", group: "EFD");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var testLookups = invLine.Lookups;
			var dummyInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			dummyInstruction.CEI_Style = "XX";
			invLine.JI_CEI = dummyInstruction.PK;
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertNoExceptionThrown(() => testLookups.CPCList.ToList());
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "13";
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = "XX";
			AssertEquals("Test 1", true, testLookups.CPCList.Contains(procedure1));
			AssertEquals("Test 2", true, testLookups.CPCList.Contains(procedure2));
			AssertEquals("Test 3", true, testLookups.CPCList.Contains(procedure3));
		}

		public void TestROOTypeOrPreferenceList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AdditionalInformation");
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var codeList1 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "XX", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList1.PK, RefCusCodeListAttributeTypes.Codes.ROOType, "");
			var codeList2 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "YY", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList2.PK, RefCusCodeListAttributeTypes.Codes.ROOType, "");
			testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "ZZ", startDate, endDate);
			testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SS", startDate, endDate);
			testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "TT", startDate, endDate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var testLookups = invLine.Lookups;
			CombineAssertions("Test ROOTypeOrPreferenceList Export", () =>
			{
				Assert("Test 1", testLookups.ROOTypeOrPreferenceList.ContainsCode("XX"));
				Assert("Test 2", testLookups.ROOTypeOrPreferenceList.ContainsCode("YY"));
				Assert("Test 3 Not", !testLookups.ROOTypeOrPreferenceList.ContainsCode("ZZ"));
			});
			var preference = testHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", startDate, endDate);
			var rateType_ZA_REB = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = testHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var tradeGroup1 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, description: "Standard Agreement");
			testHelper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Factory.Save();
			var tariff1Rate = testHelper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			Factory.Save();
			testHelper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invLine.JI_PrimaryPreference = "100";
			invLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invLine.JI_Tariff = "1020304050";
			CombineAssertions("Test ROOTypeOrPreferenceList Import", () =>
			{
				Assert("Test 4", !testLookups.ROOTypeOrPreferenceList.ContainsCode("XX"));
				Assert("Test 5", !testLookups.ROOTypeOrPreferenceList.ContainsCode("YY"));
				Assert("Test 6", testLookups.ROOTypeOrPreferenceList.ContainsCode("100"));
			});
		}

		[TestDate(2016, 02, 01)]
		public void TestTaxOrFeeCodeList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Zero Rated");
			testHelper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Normal");
			testHelper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Exempt");
			testHelper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Zero Rated");
			testHelper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Normal");
			testHelper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Exempt");
			var newTaxOrFeeCode = Factory.NewWithValidTestData<RefCusTaxOrFee>();
			newTaxOrFeeCode.ZZF_StartDate = new ZDateTime(2016, 01, 15);
			newTaxOrFeeCode.ZZF_EndDate = new ZDateTime(2016, 02, 15);
			newTaxOrFeeCode.ZZF_ZZZ_NKDataGrouping = "ZA";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 01, 01);
			AssertEquals(4, invoiceLine.Lookups.TaxOrFeeCodeList.Count);
			declaration.JE_TransportMode = "SEA";
			AssertEquals(4, invoiceLine.Lookups.TaxOrFeeCodeList.Count);
		}

		public void TestDefaultRelationForNewProduct()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "sup";
			supplier.MainAddress.OA_Address1 = "supaddr";
			supplier.CountryData.OV_MakePartsBothImportAndExport = false;
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "buy";
			importer.MainAddress.OA_Address1 = "buyaddr";
			importer.CountryData.OV_MakePartsBothImportAndExport = true;
			importer.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.AccrualBasis.Code;
			importer.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			importer.OH_RL_NKClosestPort = "AUSYD";
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_OH_Buyer = importer.PK;
			link.OL_ProductRelation = OrgRelationTypeList.Codes.Importer;
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var partsList = invoiceLine.Lookups.PartsList;
			var part = partsList.AddNew();
			part.OP_PartNum = "00001";
			var relatedOrganisations = part.RelatedOrganisations;
			AssertEquals("IMP", 1, relatedOrganisations.Count);
			AssertEquals("IMP: Importer", OrgPartRelation.RelationshipTypes.Both, relatedOrganisations.FindFirstByOrganisationPK(declaration.JE_OH_Importer).OU_Relationship);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			partsList = invoiceLine.Lookups.PartsList;
			part = partsList.AddNew();
			part.OP_PartNum = "00002";
			relatedOrganisations = part.RelatedOrganisations;
			AssertEquals("EXP", 1, relatedOrganisations.Count);
			AssertEquals("EXP: Importer", OrgPartRelation.RelationshipTypes.Both, relatedOrganisations.FindFirstByOrganisationPK(declaration.JE_OH_Importer).OU_Relationship);
			link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			partsList = invoiceLine.Lookups.PartsList;
			part = partsList.AddNew();
			part.OP_PartNum = "00003";
			relatedOrganisations = part.RelatedOrganisations;
			AssertEquals("EXW", 1, relatedOrganisations.Count);
			AssertEquals("EXW: Supplier", OrgPartRelation.RelationshipTypes.Supplier, relatedOrganisations.FindFirstByOrganisationPK(declaration.JE_OH_Supplier).OU_Relationship);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
			partsList = invoiceLine.Lookups.PartsList;
			part = partsList.AddNew();
			part.OP_PartNum = "00004";
			relatedOrganisations = part.RelatedOrganisations;
			AssertEquals("MSC", 1, relatedOrganisations.Count);
			AssertEquals("MSC: Blank", ZString.Empty, relatedOrganisations.FindFirstByOrganisationPK(ZGuid.Empty).OU_Relationship);
		}

		void AssertListIsCached(ZString messageType, params KeyValuePair<ICollection, GetICollectionDelegate>[] listMatchTypes)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = messageType;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var lookup1 = new JobComInvoiceLineLookups(invoice.JobComInvoiceLines.AddNew());
			var lookup2 = new JobComInvoiceLineLookups(invoice.JobComInvoiceLines.AddNew());
			var lookup3 = new JobComInvoiceLineLookups(invoice.JobComInvoiceLines.AddNew());
			var lookup4 = new JobComInvoiceLineLookups(invoice.JobComInvoiceLines.AddNew());
			foreach (var pair in listMatchTypes)
			{
				var cachedList = pair.Key;
				var getList = pair.Value;
				AssertSame("lookup1", cachedList, getList(lookup1));
				AssertSame("lookup2", cachedList, getList(lookup2));
				AssertSame("lookup3", cachedList, getList(lookup3));
				AssertSame("lookup4", cachedList, getList(lookup4));
			}
		}

		public void TestVehicleFormats()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertSame(Factory.GetCachedValue<VehicleFormatList>(), invoiceLine.Lookups.VehicleFormats);
		}

		public void TestVehicleTypes()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertSame(Factory.GetCachedValue<VehicleTypeList>(), invoiceLine.Lookups.VehicleTypes);
		}

		public void TestYearList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Last n Years only", 40, invoiceLine.Lookups.YearList.Count);
			Assert("this year", invoiceLine.Lookups.YearList.ContainsCode(ZDateTime.Today.Year.ToString()));
			Assert("oldest year", invoiceLine.Lookups.YearList.ContainsCode((ZDateTime.Today.Year - 39).ToString()));
		}

		public void TestGoodsTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(typeof(GoodsTypeList), invoiceLine.Lookups.GoodsTypeList.GetType());
			Assert(invoiceLine.Lookups.GoodsTypeList.ContainsCode(GoodsTypeList.Codes.N));
			Assert(invoiceLine.Lookups.GoodsTypeList.ContainsCode(GoodsTypeList.Codes.S));
			Assert(invoiceLine.Lookups.GoodsTypeList.ContainsCode(GoodsTypeList.Codes.U));
		}

		public void TestCustomsValueCurrencyOverrideList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { "ZAR" }, invoiceLine.Lookups.CustomsValueCurrencyOverrideList.GetAllCodes());
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			AssertContainsExactElementsInAnyOrder(new[] { "USD", "ZAR" }, invoiceLine.Lookups.CustomsValueCurrencyOverrideList.GetAllCodes());
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "XXX";
			AssertContainsExactElementsInAnyOrder(new[] { "ZAR" }, invoiceLine.Lookups.CustomsValueCurrencyOverrideList.GetAllCodes());
		}
	}
}
