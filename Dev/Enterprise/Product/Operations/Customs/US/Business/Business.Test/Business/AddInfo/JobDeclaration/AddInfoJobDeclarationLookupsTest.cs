using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInsuranceDispositionList()
		{
			var cachedList = Factory.GetCachedValue<InsuranceDispositionCodeList>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var dispositionList = jobDeclaration.AddInfoLookups.InsuranceDispositionList;
			AssertSame("Should get the cached value.", cachedList, dispositionList);
			AssertEquals(typeof(InsuranceDispositionCodeList), dispositionList.GetType());
		}

		public void TestInsuranceAgentList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "IAA", "UnitedStates Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "IAC", "Canada Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var cachedCollection = GlbExternalPasswordHelper.GetInsuranceAgents(Factory);
			var jobDeclaration = Factory.New<JobDeclaration>();
			var collection = jobDeclaration.AddInfoLookups.InsuranceAgentList;
			AssertSame("Should get the cached value.", cachedCollection, collection);
			var expectedCodes = new[] { "IAA" };
			var actualCodes = collection.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("Should only get codes from United States.", expectedCodes, actualCodes);
		}

		public void TestECCNNumberListOnDeclaration()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C35, USAESLicenseCode.Codes.C36 });

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

			declaration.US_LicenseType = USAESLicenseCode.Codes.C35;
			var eccnList = declaration.AddInfoLookups.US_ECCNList;
			eccnList.Load();
			AssertEquals("ECCN Numbers with LicenseType C35", 3, eccnList.Count);
			var item1 = declaration.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "6A002").FirstOrDefault();
			var item2 = declaration.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "4A003").FirstOrDefault();
			var item3 = declaration.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "5A002").FirstOrDefault();
			AssertEquals("ECCNList with LicenseType C35 -6A002", "6A002 Description", item1.ZZD_Description);
			AssertEquals("ECCNList with LicenseType C35 -4A003", "4A003 Description", item2.ZZD_Description);
			AssertEquals("ECCNList with LicenseType C35 -5A002", "5A002 Description", item3.ZZD_Description);

			declaration.US_LicenseType = USAESLicenseCode.Codes.C36;
			var eccnList2 = declaration.AddInfoLookups.US_ECCNList;
			eccnList2.Load();
			AssertEquals("ECCN Numbers with LicenseType C36", 1, eccnList2.Count);
			var item4 = declaration.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "3A003").FirstOrDefault();
			AssertEquals("ECCNList with LicenseType C36 -3A003", "3A003 Description", item4.ZZD_Description);
			declaration.US_LicenseType = USAESLicenseCode.Codes.C37;

			var eccnList3 = declaration.AddInfoLookups.US_ECCNList;
			eccnList3.Load();
			AssertEquals("ECCN Numbers with LicenseType C37", 0, eccnList3.Count);
		}

		public void TestUS_SplitShipmentReleaseCodeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(SplitShipmentReleaseCodeList), declaration.AddInfoLookups.US_SplitShipmentReleaseCodeList.GetType());
		}

		public void TestUS_BondDispositionCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(BondDispositionCodeList), declaration.AddInfoLookups.US_BondDispositionCodeList.GetType());
		}

		public void TestPortOfExports()
		{
			AssertEquals(typeof(RefUNLOCOCollection), Declaration.AddInfoLookups.PortOfExports.GetType());
			var prPort = Factory.New<RefUNLOCO>();
			prPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			Assert("Contain PR ports", Declaration.AddInfoLookups.PortOfExports.Contains(prPort));

			var viPort = Factory.New<RefUNLOCO>();
			viPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			Assert("Contain VI ports", Declaration.AddInfoLookups.PortOfExports.Contains(viPort));
		}

		public void TestUS_CommodityFilingOptions()
		{
			var list = Declaration.AddInfoLookups.US_CommodityFilingOptions;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode(AESCommodityFilingOptionList.Codes._2Predeparture));
			AssertEquals(true, list.ContainsCode(AESCommodityFilingOptionList.Codes._4Postdeparture));
		}

		public void TestImporterOfRecordList()
		{
			AssertNotNull(Declaration.AddInfoLookups.ImporterOfRecordList);
		}

		public void TestBondWaiverReasonCodes()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals(2, Declaration.AddInfoLookups.BondWaiverReasonCodes.Count);
			Assert(Declaration.AddInfoLookups.BondWaiverReasonCodes.ContainsCode(BondWaiverReasonCodeList.Codes._998));
			Assert(Declaration.AddInfoLookups.BondWaiverReasonCodes.ContainsCode(BondWaiverReasonCodeList.Codes._999));
			Declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals(5, Declaration.AddInfoLookups.BondWaiverReasonCodes.Count);
		}

		public void TestUS_ConsolidatedInformalList()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Assert(Declaration.IsACE);
			AssertEquals(2, Declaration.AddInfoLookups.US_ConsolidatedInformalList.Count);
			Assert(!Declaration.AddInfoLookups.US_ConsolidatedInformalList.ContainsCode(ConsolidatedInformalList.Codes.Consolidated));
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(3, Declaration.AddInfoLookups.US_ConsolidatedInformalList.Count);
		}

		public void TestDischargeSchDList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4001", "4001 TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4002", "4002 TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5001", "5001 TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5002", "5002 TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5003", "5003 TEST NAME", startDate, endDate);
			Factory.Save();

			CreateLocoMapIfNotExists("60001", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("60002", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("60003", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("4001", "USLAX", USLocoMapSystemUsageList.Codes.Sea, true);
			CreateLocoMapIfNotExists("4002", "USLAX", USLocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMapIfNotExists("4003", "USLAX", USLocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMapIfNotExists("5001", "USLAX", USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMapIfNotExists("5002", "USLAX", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMapIfNotExists("5003", "USLAX", USLocoMapSystemUsageList.Codes.Air, false);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("DischargeSchDList - Import", typeof(ZZRefCusCodeListCombinedCollection), Declaration.AddInfoLookups.DischargeSchDList.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("DischargeSchDList - Export", typeof(ZZRefCusCodeListCombinedCollection), Declaration.AddInfoLookups.DischargeSchDList.GetType());
			Declaration.JE_MessageType = "";
			AssertEquals("DischargeSchDList - undefined", typeof(ZZRefCusCodeListCombinedCollection), Declaration.AddInfoLookups.DischargeSchDList.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.US_SchDLoading = "4901";
			AssertEquals("DischargeSchDList - Export from Puerto Rico should return USCForeignAndRegionPortCollection as for Import", typeof(USCForeignAndRegionPortCollection), Declaration.AddInfoLookups.DischargeSchDList.GetType());

			Declaration.JE_RL_NKPortOfArrival = "USLAX";
			var dischargeSchDList = Declaration.AddInfoLookups.DischargeSchDList;
			AssertEquals("DischargeSchDList - Export and US_SchDArrivalTypeIsDropEdit is true", typeof(ZZRefCusCodeListCombinedCollection), dischargeSchDList.GetType());
			AssertEquals(2, dischargeSchDList.Count);
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60002"));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_RL_NKPortOfArrival = "USLAX";
			dischargeSchDList = Declaration.AddInfoLookups.DischargeSchDList;
			AssertEquals("DischargeSchDList - Import and US_SchDArrivalTypeIsDropEdit is false", typeof(ZZRefCusCodeListCombinedCollection), dischargeSchDList.GetType());
			AssertEquals(0, dischargeSchDList.Count);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.JE_RL_NKPortOfArrival = "USLAX";
			dischargeSchDList = Declaration.AddInfoLookups.DischargeSchDList;
			AssertEquals("DischargeSchDList - Import and sea and US_SchDArrivalTypeIsDropEdit is true", typeof(ZZRefCusCodeListCombinedCollection), dischargeSchDList.GetType());
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4001"));
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4002"));

			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_RL_NKPortOfArrival = "USLAX";
			dischargeSchDList = Declaration.AddInfoLookups.DischargeSchDList;
			AssertEquals("DischargeSchDList - Import and Air and US_SchDArrivalTypeIsDropEdit is true", typeof(ZZRefCusCodeListCombinedCollection), dischargeSchDList.GetType());
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "5001"));
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "5002"));
			Assert(dischargeSchDList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "5003"));
		}

		public void TestSchDExportList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "60001", "60001 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "60002", "60002 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "60004", "60004 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode3.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "60005", "60005 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode4.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "60003", "60003 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			CreateLocoMapIfNotExists("60001", "USLAX", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("60002", "USLAX", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("60003", "USLAX", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("60005", "USLAX", USLocoMapSystemUsageList.Codes.Sea);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_RL_NKPortOfExport = "USLAX";
			var schDExportList = Declaration.AddInfoLookups.SchDExportList;
			AssertEquals(2, schDExportList.Count);
			Assert(schDExportList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			Assert(schDExportList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60005"));
		}

		public void TestBillIssuers()
		{
			AssertEquals("BillIssuers", typeof(BillIssuerOrganisationFindBoxCollection), Declaration.AddInfoLookups.BillIssuers.GetType());
		}

		public void TestUS_TariffTypeList()
		{
			AssertEquals("US_TariffTypeList", typeof(TariffTypeList), Declaration.AddInfoLookups.US_TariffTypeList.GetType());
		}

		public void TestOtherReconIssueList()
		{
			AssertEquals("OtherReconIssueList", typeof(ReconIssueCodeList), Declaration.AddInfoLookups.OtherReconIssueList.GetType());
			AssertEquals("not existing ReconIssueCodeList.Codes.FTA", false, Declaration.AddInfoLookups.OtherReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
		}

		public void TestCargoReleaseTypeList()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert(Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.CR));
			Assert(Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.BCR));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.SE));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.ACE));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.ACS));
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.CR));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.BCR));
			Assert(Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.SE));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.ACE));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.ACS));
			Declaration.US_EnableCRL = false;
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.CR));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.BCR));
			Assert(!Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.SE));
			Assert(Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.ACE));
			Assert(Declaration.AddInfoLookups.CargoReleaseTypeList.ContainsCode(CargoReleaseTypeList.Codes.ACS));
		}

		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		public void TestBondTypeList()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			var list = Declaration.AddInfoLookups.US_BondTypeList;
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.NoBondRequired));
			AssertContains("Bond Waived", list.GetDescriptionFromCode(BondTypeList.Codes.NoBondRequired));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.ContinuousBond));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.SingleTransactionBond));
			Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			list = Declaration.AddInfoLookups.US_BondTypeList;
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.NoBondRequired));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.ContinuousBond));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.SingleTransactionBond));
			Declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			list = Declaration.AddInfoLookups.US_BondTypeList;
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.NoBondRequired));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.ContinuousBond));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.SingleTransactionBond));
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			list = Declaration.AddInfoLookups.US_BondTypeList;
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.NoBondRequired));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.ContinuousBond));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.SingleTransactionBond));
		}

		public void TestBondTypeListForAGovernmentImporter()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.GovernmentDutiable;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(importer).ZO_ImporterType = ImporterTypeList.Codes.USGovernment;
			Factory.Save();
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals(true, Declaration.IsGovernmentImporter);
			var list = Declaration.AddInfoLookups.US_BondTypeList;
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.NoBondRequired));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.ContinuousBond));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.SingleTransactionBond));
		}

		public void TestBondTypeListForDrawback()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var list = Declaration.AddInfoLookups.US_BondTypeList;
			AssertEquals("3 bond types for drawbacks", 3, list.Count);
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.NoBondRequired));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.ContinuousBond));
			AssertEquals("BondTypeList", true, list.ContainsCode(BondTypeList.Codes.SingleTransactionBond));
		}

		public void TestEntryTypeForINB()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EnableINB = true;
			AssertEquals("Inbond", true, Declaration.IsInBond);
			var result = Declaration.AddInfoLookups.US_InbondType_List;
			AssertEquals("there should be three items only", 3, result.Count);
			AssertEquals("IT is there", true, result.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals("TE is there", true, result.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals("IE is there", true, result.ContainsCode(EntryTypeList.Codes.ImmediateExportation));
		}

		public void TestPaymentTypeIsSorted()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var list = Declaration.AddInfoLookups.US_PaymentTypeList;
			AssertEquals("first payment type after sorted", PaymentTypeList.Codes.IndividualBasis, list[0].Code);
		}

		public void TestEntryType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableINB = true;
			AssertEquals("Inbond", true, Declaration.IsInBond);
			var list = Declaration.AddInfoLookups.US_EntryTypeList;
			Assert("list should be a full list", list.Count > 3);
			AssertEquals("ExWarehouse EntryType does not exist", true, list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
		}

		public void TestUS_NAFTACountryCodeList()
		{
			Assert("contains Canada", Declaration.AddInfoLookups.US_NAFTACountryCodeList.ContainsCode(Core.Constants.CountryCodes.Canada));
			Assert("contains Mexico", Declaration.AddInfoLookups.US_NAFTACountryCodeList.ContainsCode(Core.Constants.CountryCodes.Mexico));
			Assert("does not contain US", !Declaration.AddInfoLookups.US_NAFTACountryCodeList.ContainsCode(Core.Constants.CountryCodes.UnitedStates));
			Assert("does not contain AU", !Declaration.AddInfoLookups.US_NAFTACountryCodeList.ContainsCode(Core.Constants.CountryCodes.Australia));
		}

		readonly ZString[] claimPorts = new ZString[] { "1001", "3901", "5301", "2809" };
		readonly ZString[] teamNos = new ZString[] { "2DB", "3DR", "6D0", "7D7" };
		public void TestUS_ClaimPortCodeList()
		{
			foreach (var claimPort in claimPorts)
			{
				Assert("contains claim port " + claimPort, Declaration.AddInfoLookups.US_ClaimPortCodeList.ContainsCode(claimPort));
			}

			Assert("does not contains claim port 9999", !Declaration.AddInfoLookups.US_ClaimPortCodeList.ContainsCode("9999"));
		}

		public void TestUS_TeamNoCodeList()
		{
			foreach (var teamNo in teamNos)
			{
				Assert("contains team no " + teamNo, Declaration.AddInfoLookups.US_TeamNoForDrawbackCodeList.ContainsCode(teamNo));
			}

			Assert("does not contains team no 999", !Declaration.AddInfoLookups.US_TeamNoForDrawbackCodeList.ContainsCode("999"));
		}

		public void TestValidClaimPortTeamNos()
		{
			foreach (var claimPort in claimPorts)
			{
				Assert("contains key (claim port) " + claimPorts, Declaration.AddInfoLookups.ValidClaimPortTeamNos.ContainsKey(claimPort));
			}

			Assert("does not contains claim port 9999", !Declaration.AddInfoLookups.ValidClaimPortTeamNos.ContainsKey("9999"));
			foreach (var teamNo in teamNos)
			{
				Assert("contains value (team no) " + teamNo, Declaration.AddInfoLookups.ValidClaimPortTeamNos.ContainsValue(teamNo));
			}

			Assert("does not contains team no 999", !Declaration.AddInfoLookups.ValidClaimPortTeamNos.ContainsValue("999"));
		}

		public void TestUS_DRWFilingMethodCodeList()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var list = Declaration.AddInfoLookups.US_DRWFilingMethodCodeList;
			AssertEquals("2 Filing Method codes for drawbacks", 2, list.Count);
			AssertEquals("A", list[0].Code);
			AssertEquals("M", list[1].Code);
		}

		public void TestSEBCalcCodes()
		{
			AssertNotNull(Declaration.AddInfoLookups.SEBCalcCodes);
			AssertEquals(typeof(SEBCalculationList), Declaration.AddInfoLookups.SEBCalcCodes.GetType());
		}

		public void TestReleaseStatusList()
		{
			var list = Declaration.AddInfoLookups.ReleaseStatusList;
			AssertEquals(11, list.Count);
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.CAN));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.DEL));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.EXM));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.HLD));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.NRL));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.REL));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.ADM));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.NRT));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.DOC));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.NRC));
			AssertEquals(true, list.ContainsCode(CRLReleaseStatusList.Codes.RVW));
		}

		public void TestEntryModes()
		{
			EntryModeList list = Declaration.AddInfoLookups.EntryModes;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.Paired));
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.RLF));
			Declaration.US_EntryDate = new ZDateTime(2011, 01, 27);
			list = Declaration.AddInfoLookups.EntryModes;
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.Paired));
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.RLF));
			Declaration.US_EntryDate = new ZDateTime(2011, 01, 28);
			list = Declaration.AddInfoLookups.EntryModes;
			AssertEquals(1, list.Count);
			AssertEquals(false, list.ContainsCode(EntryModeList.Codes.Paired));
			AssertEquals(true, list.ContainsCode(EntryModeList.Codes.RLF));
		}

		public void TestUS_DRWPurposeCodeList()
		{
			AssertEquals("There are 3 types of drawback purpose", 3, Declaration.AddInfoLookups.US_DRWPurposeCodeList.Count);
		}

		public void TestUS_BondDesignationCodeList()
		{
			var list = Declaration.AddInfoLookups.US_BondDesignationCodeList;
			AssertEquals(3, list.Count);
			AssertEquals(true, list.ContainsCode(BondDesignationCodeList.Codes.BasicBond));
			AssertEquals(true, list.ContainsCode(BondDesignationCodeList.Codes.SubstitutionBond));
			AssertEquals(true, list.ContainsCode(BondDesignationCodeList.Codes.SupersedingBond));
			AssertEquals(false, list.ContainsCode(BondDesignationCodeList.Codes.TerminateContinuousBond));
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			list = Declaration.AddInfoLookups.US_BondDesignationCodeList;
			AssertEquals(1, list.Count);
			AssertEquals(true, list.ContainsCode(BondDesignationCodeList.Codes.BasicBond));
			AssertEquals(false, list.ContainsCode(BondDesignationCodeList.Codes.SubstitutionBond));
			AssertEquals(false, list.ContainsCode(BondDesignationCodeList.Codes.SupersedingBond));
			AssertEquals(false, list.ContainsCode(BondDesignationCodeList.Codes.TerminateContinuousBond));
		}

		public void TestFTZLists()
		{
			AssertEquals(typeof(FTZAdmissionTypeCodeList), Declaration.AddInfoLookups.US_FTZAdmissionTypeList.GetType());
			AssertEquals(typeof(FTZDeliveryCodeList), Declaration.AddInfoLookups.FTZDeliveryCodeList.GetType());
		}

		public void TestEntryDateElectionCodeList()
		{
			var list = Declaration.AddInfoLookups.EntryDateElectionCodeList;
			AssertEquals(2, list.Count);
			AssertEquals(EntryDateElectionCodeList.Descriptions.ArrivalDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.ArrivalDate));
			AssertEquals(EntryDateElectionCodeList.Descriptions.PresentationDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.PresentationDate));
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			list = Declaration.AddInfoLookups.EntryDateElectionCodeList;
			AssertEquals(2, list.Count);
			AssertEquals(EntryDateElectionCodeList.Descriptions.ArrivalDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.ArrivalDate));
			AssertEquals(EntryDateElectionCodeList.Descriptions.PresentationDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.PresentationDate));
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			list = Declaration.AddInfoLookups.EntryDateElectionCodeList;
			AssertEquals(2, list.Count);
			AssertEquals(EntryDateElectionCodeList.Descriptions.ArrivalDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.ArrivalDate));
			AssertEquals(EntryDateElectionCodeList.Descriptions.PresentationDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.PresentationDate));
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			list = Declaration.AddInfoLookups.EntryDateElectionCodeList;
			AssertEquals(2, list.Count);
			AssertEquals(EntryDateElectionCodeList.Descriptions.ArrivalDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.ArrivalDate));
			AssertEquals(EntryDateElectionCodeList.Descriptions.PresentationDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.PresentationDate));
			Declaration.US_EnableCRL = true;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			list = Declaration.AddInfoLookups.EntryDateElectionCodeList;
			AssertEquals(1, list.Count);
			AssertEquals(EntryDateElectionCodeList.Descriptions.WeeklyEstimateFilingDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate));
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			list = Declaration.AddInfoLookups.EntryDateElectionCodeList;
			AssertEquals(2, list.Count);
			AssertEquals(EntryDateElectionCodeList.Descriptions.NonWeeklyEstimateFilingDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate));
			AssertEquals(EntryDateElectionCodeList.Descriptions.WeeklyEstimateFilingDate, list.GetDescriptionFromCode(EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate));
		}

		public void TestUS_LicenseType_List()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
			var declaration = Factory.New<JobDeclaration>();
			var uS_LicenseType_List = declaration.AddInfoLookups.US_LicenseType_List;
			uS_LicenseType_List.Load();
			AssertEquals(USAESLicenseCode.Codes.C30, uS_LicenseType_List.Cast<ZZRefCusCodeListCombined>().FirstOrDefault().ZZD_Code);
		}

		public void TestDestructionResultCodesList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(DrawbackDestructionResultCodes), jobDeclaration.AddInfoLookups.DestructionResultCodesList.GetType());
			AssertEquals("D, N, W", jobDeclaration.AddInfoLookups.DestructionResultCodesList.CodesAsString);
		}

		public void TestACEDrawbackProcessingPortCodeList()
		{
			AssertEquals(5, Declaration.AddInfoLookups.ACEDrawbackProcessingPortCodeList.Count);
			AssertEquals(true, Declaration.AddInfoLookups.ACEDrawbackProcessingPortCodeList.ContainsCode("1001"));
			AssertEquals(true, Declaration.AddInfoLookups.ACEDrawbackProcessingPortCodeList.ContainsCode("3901"));
			AssertEquals(true, Declaration.AddInfoLookups.ACEDrawbackProcessingPortCodeList.ContainsCode("5301"));
			AssertEquals(true, Declaration.AddInfoLookups.ACEDrawbackProcessingPortCodeList.ContainsCode("2809"));
			AssertEquals(true, Declaration.AddInfoLookups.ACEDrawbackProcessingPortCodeList.ContainsCode("9900"));
		}

		public void TestImportEstablishments()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("ImportEstablishments", lookups.ImportEstablishments);
				AssertContainsExactElementsInExactOrder("ImportEstablishments.DataGroupingCode", new[] { Core.Constants.CountryCodes.UnitedStates }, lookups.ImportEstablishments.DataGroupingCodes);
				AssertCollectionContains("ImportEstablishments.CodeTypes", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers, lookups.ImportEstablishments.CodeTypes);
			});
		}

		void CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = true)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}
		}
	}
}
