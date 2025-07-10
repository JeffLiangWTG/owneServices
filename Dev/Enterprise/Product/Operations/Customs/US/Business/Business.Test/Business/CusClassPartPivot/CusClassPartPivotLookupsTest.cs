using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOMCDisclaimReasonList()
		{
			var tariff = "1234565432";
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = tariff;
			uscTariff.UE_PGACodes = "OM1";
			uscTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			Pivot.CI_ChildType = "HTI";
			Pivot.CI_TariffNum = tariff;
			AssertEquals(1, Lookups.OMCDisclaimReasonList.Count);
			AssertEquals("A", Lookups.OMCDisclaimReasonList.CodesAsString);
		}

		public void TestNOPDesclaimReasonList()
		{
			var tariff = "1234565789";
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = tariff;
			uscTariff.UE_PGACodes = "AM8";
			uscTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			Pivot.CI_ChildType = "HTI";
			Pivot.CI_TariffNum = tariff;
			AssertEquals(2, Lookups.OMCDisclaimReasonList.Count);
			AssertEquals("A, B", Lookups.OMCDisclaimReasonList.CodesAsString);
		}

		public void TestHFCDisclaimReasonList()
		{
			var tariff1 = "0000000001";
			var tariff2 = "0000000002";
			var uscTariff1 = Factory.New<USCTariff>();
			uscTariff1.UE_Tariff = tariff1;
			uscTariff1.UE_PGACodes = "EH1";
			uscTariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff1.UE_DateTo = ZDateTime.Now.AddYears(1);
			var uscTariff2 = Factory.New<USCTariff>();
			uscTariff2.UE_Tariff = tariff2;
			uscTariff2.UE_PGACodes = "EH2";
			uscTariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff2.UE_DateTo = ZDateTime.Now.AddYears(1);

			Pivot.CI_ChildType = "HTI";
			Pivot.CI_TariffNum = tariff1;
			AssertEquals(1, Lookups.HFCDisclaimReasonList.Count);
			AssertEquals("A", Lookups.HFCDisclaimReasonList.CodesAsString);

			Pivot.CI_TariffNum = tariff2;
			AssertEquals(2, Lookups.HFCDisclaimReasonList.Count);
			AssertEquals("A, B", Lookups.HFCDisclaimReasonList.CodesAsString);
		}

		public void TestParent()
		{
			AssertEquals(Pivot, Lookups.Parent);
		}

		public void TestClassificationTypes()
		{
			AssertEquals(typeof(ClassificationTypeList), Lookups.ClassificationTypes.GetType());
			CusClassPartPivot childPivot = Pivot.Children.AddNew();
			AssertEquals(typeof(ClassificationChildTypeList), childPivot.Lookups.ClassificationTypes.GetType());
		}

		public void TestTariffs()
		{
			var tariff = "9876.54.3210";
			Pivot.CI_FormattedTariffNum = tariff;
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(typeof(USCTariffCollection), Lookups.Tariffs.GetType());
			AssertHasDefaultFilter(USCTariff.FilterSchema.Tariff, tariff, Lookups.Tariffs);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(typeof(Universal.TariffViewCollection), Lookups.Tariffs.GetType());
			AssertHasDefaultFilter(Universal.Constants.RefCusTariffFilters.TariffCode, Pivot.CI_TariffNum, Lookups.Tariffs);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var tariffs = Lookups.Tariffs;
			AssertEquals(typeof(Universal.TariffViewCollection), tariffs.GetType());
			AssertHasDefaultFilter(Universal.Constants.RefCusTariffFilters.TariffCode, Pivot.CI_TariffNum, tariffs);
			AssertHasDefaultFilter(Universal.Constants.RefCusTariffFilters.TariffType, Core.Constants.CountryCodes.UnitedStates, tariffs, "1");
			AssertHasDefaultFilter(Universal.Constants.RefCusTariffFilters.TariffType, Universal.Constants.TariffTypes.Export, tariffs, "2");
		}

		public void TestZoneStatuses()
		{
			AssertNotNull(Lookups.ZoneStatuses);
		}

		public void TestManufacturers()
		{
			AssertNotNull(Lookups.Manufacturers);
		}

		public void TestExporters()
		{
			AssertNotNull(Lookups.Exporters);
		}

		public void TestClassification()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(typeof(ExportClassificationCollection), Lookups.Classifications.GetType());
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(typeof(ImportClassificationCollection), Lookups.Classifications.GetType());
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(typeof(ImportClassificationCollection), Lookups.Classifications.GetType());
		}

		public void TestCVDCaseNumberList()
		{
			var cvdDumpingCase = Factory.New<USCACCase>();
			cvdDumpingCase.U5_CaseNumber = "CXXAAABBB";
			cvdDumpingCase.U5_ISOCountryCode = Core.Constants.CountryCodes.Malaysia;
			cvdDumpingCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdDumpingCase.U5_CaseStatusDate = ZDateTime.Today;
			var cvdDumpingTariff = cvdDumpingCase.CaseTariffs.AddNew();
			cvdDumpingTariff.U9_TariffNumber = "5806321010";
			var cvdDumpingCase2 = Factory.New<USCACCase>();
			cvdDumpingCase2.U5_CaseNumber = "CXXAAA222";
			cvdDumpingCase2.U5_ISOCountryCode = Core.Constants.CountryCodes.Malaysia;
			cvdDumpingCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdDumpingCase2.U5_CaseStatusDate = ZDateTime.Today;
			var cvdDumpingTariff2 = cvdDumpingCase2.CaseTariffs.AddNew();
			cvdDumpingTariff2.U9_TariffNumber = "5806321221";
			var cvdDumpingCase3 = Factory.New<USCACCase>();
			cvdDumpingCase3.U5_CaseNumber = "CXXAAA333";
			cvdDumpingCase3.U5_ISOCountryCode = Core.Constants.CountryCodes.Malaysia;
			cvdDumpingCase3.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdDumpingCase3.U5_CaseStatusDate = ZDateTime.Today;
			var cvdDumpingTariff3 = cvdDumpingCase3.CaseTariffs.AddNew();
			cvdDumpingTariff3.U9_TariffNumber = "9912345678";
			Factory.Save();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "5806321221";
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Malaysia;
			USCACCaseCollection collection = pivot.Lookups.CVDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("1 Countervailing Case should be found for tariff '5806321221'", 1, collection.Count);
			pivot.CI_SupplementalTariff = "9912345678";
			collection = pivot.Lookups.CVDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("2 Countervailing Case should be found for tariffs '5806321221' and '9912345678'", 2, collection.Count);
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_SupplementalTariff = ZString.Empty;
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "5806321221";
			pivot.CI_CC = classification.PK;
			collection = pivot.Lookups.CVDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("Countervailing Case should be found for tariff '5806321221'", 1, collection.Count);
			pivot.CI_SupplementalTariff = "9912345678";
			collection = pivot.Lookups.CVDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("2 Countervailing Case should be found for tariffs '5806321221' and '9912345678'", 2, collection.Count);
		}

		public void TestADDCaseNumberList()
		{
			var addDumpingCase = Factory.New<USCACCase>();
			addDumpingCase.U5_CaseNumber = "AXXAAABBB";
			addDumpingCase.U5_ISOCountryCode = Core.Constants.CountryCodes.Malaysia;
			addDumpingCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addDumpingCase.U5_CaseStatusDate = ZDateTime.Today;
			var dumpingTariff = addDumpingCase.CaseTariffs.AddNew();
			dumpingTariff.U9_TariffNumber = "5806321010";
			var addDumpingCase2 = Factory.New<USCACCase>();
			addDumpingCase2.U5_CaseNumber = "A9085292";
			addDumpingCase2.U5_ISOCountryCode = Core.Constants.CountryCodes.Malaysia;
			addDumpingCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addDumpingCase2.U5_CaseStatusDate = ZDateTime.Today;
			var dumpingTariff2 = addDumpingCase2.CaseTariffs.AddNew();
			dumpingTariff2.U9_TariffNumber = "5806321221";
			var addDumpingCase3 = Factory.New<USCACCase>();
			addDumpingCase3.U5_CaseNumber = "AXXAAA333";
			addDumpingCase3.U5_ISOCountryCode = Core.Constants.CountryCodes.Malaysia;
			addDumpingCase3.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addDumpingCase3.U5_CaseStatusDate = ZDateTime.Today;
			var addDumpingTariff3 = addDumpingCase3.CaseTariffs.AddNew();
			addDumpingTariff3.U9_TariffNumber = "9912345678";
			Factory.Save();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "5806321221";
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Malaysia;
			USCACCaseCollection collection = pivot.Lookups.ADDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("AntiDumping Case should be found for tariff '5806321221'", 1, collection.Count);
			pivot.CI_SupplementalTariff = "9912345678";
			collection = pivot.Lookups.ADDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("2 AntiDumping Cases should be found for tariffs '5806321221' and '9912345678'", 2, collection.Count);
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_SupplementalTariff = ZString.Empty;
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "5806321221";
			pivot.CI_CC = classification.PK;
			collection = pivot.Lookups.ADDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("AntiDumping Case should be found for tariff '5806321221'", 1, collection.Count);
			pivot.CI_SupplementalTariff = "9912345678";
			collection = pivot.Lookups.ADDCaseNumberList;
			collection.RefreshFromDb();
			AssertEquals("2 AntiDumping Cases should be found for tariffs '5806321221' and '9912345678'", 2, collection.Count);
		}

		public void TestEPANetQtyUQList()
		{
			AssertEquals(2, Lookups.EPANetQtyUQList.Count);
			AssertEquals(true, Lookups.EPANetQtyUQList.ContainsCode(Core.Constants.Weight.Kilograms));
			AssertEquals(true, Lookups.EPANetQtyUQList.ContainsCode(Core.Constants.Volume.Litre));
		}

		public void TestAMSDisclaimProgramList()
		{
			AssertEquals(1, Lookups.AMSDisclaimProgramList.Count);
			AssertEquals(true, Lookups.AMSDisclaimProgramList.ContainsCode(AMSProgramList.Codes.MO8));
		}

		public void TestPSTDisclaimProgramList()
		{
			AssertEquals(typeof(PSTProductTypeList), Lookups.PSTDisclaimProgramList.GetType());
		}

		CusClassPartPivot pivot;
		CusClassPartPivot Pivot => pivot ?? (pivot = Factory.New<CusClassPartPivot>());

		CusClassPartPivotLookups Lookups => Pivot.Lookups;

		void AssertHasDefaultFilter(string filterName, string filterValue, BusinessObjectCollection collection, string propertySuffix = "")
		{
			var tariffFilterKey = filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property" + propertySuffix;
			Assert($"{filterName} filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(tariffFilterKey));
			AssertEquals(filterValue, collection.FilterBusinessObjectDefaults[tariffFilterKey].Value.ToString());
		}
	}
}
