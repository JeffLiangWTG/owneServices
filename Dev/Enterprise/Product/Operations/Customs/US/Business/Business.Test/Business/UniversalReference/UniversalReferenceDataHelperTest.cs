using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class UniversalReferenceDataHelperTest : TestCaseWithFactory
	{
		public void TestGetTariffs()
		{
			var shb = Universal.Constants.TariffTypes.ScheduleB;
			var collection = Factory.GetTariffs(shb, "1020", ZDateTime.BrettsBirthday);
			var tariffCode = collection.FilterBusinessObjectDefaults[Universal.Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", "1020", tariffCode.Value);
			var effectiveDate = collection.FilterBusinessObjectDefaults[Universal.Constants.RefCusTariffFilters.EffectiveDate + ":Property1"];
			AssertEquals("effectiveDate", ZDateTime.BrettsBirthday, effectiveDate.Value);
			var tariffTypeCountry = collection.FilterBusinessObjectDefaults[Universal.Constants.RefCusTariffFilters.TariffType + ":Property1"];
			AssertEquals("tariffTypeCountry", (ZString)Core.Constants.CountryCodes.UnitedStates, tariffTypeCountry.Value);
			var tariffTypeType = collection.FilterBusinessObjectDefaults[Universal.Constants.RefCusTariffFilters.TariffType + ":Property2"];
			AssertEquals("tariffTypeType", shb, tariffTypeType.Value);
			AssertEquals(false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Universal.Constants.RefCusTariffFilters.TariffRestriction + ":Property1"));
		}

		public void TestGetTariff()
		{
			var shbType = Helper.CreateTariffType(Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var shbTariff = Helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, shbType.PK, "99999999", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "SHB Description");

			USCTariff impTariff = Factory.New<USCTariff>();
			impTariff.UE_Tariff = "1111101000";
			impTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			impTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			impTariff.UE_ShortDescription = "IMP Description";
			Factory.Save();

			var iSHBTariff = Factory.GetTariff(Universal.Constants.TariffTypes.ScheduleB, "99999999", ZDateTime.Today);
			var iIMPTariff = Factory.GetTariff(Universal.Constants.TariffTypes.HarmonizedSystem, "1111101000", ZDateTime.Today);

			AssertEquals("Tariff 99999999 should be Universal.TariffView", typeof(Universal.TariffView), iSHBTariff.GetType());
			AssertEquals("Tariff 1111101000 should be USCTariff", typeof(USCTariff), iIMPTariff.GetType());
			AssertEquals("Tariff 99999999 description", "SHB Description", iSHBTariff.Description);
			AssertEquals("Tariff 1111101000 description", "IMP Description", iIMPTariff.Description);
		}

		[TestDate(2011, 1, 2)]
		public void TestGetEffectiveSPIForDutyCalculation()
		{
			Helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, "Expired Special Program Indicator");
			Helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, PrimarySpecProgramIndicatorList.Codes.A, "A", ZDateTime.Today, ZDateTime.Today.AddDays(5), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, PrimarySpecProgramIndicatorList.Codes.D);
			Factory.Save();

			var countryOfOrigin = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "ID");
			AssertEquals("'D' is not eligible", ZString.Empty, UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(Factory, PrimarySpecProgramIndicatorList.Codes.A, ZDateTime.Today, countryOfOrigin));
			countryOfOrigin = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "ZA");
			AssertEquals("'D' is eligible", PrimarySpecProgramIndicatorList.Codes.A, UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(Factory, PrimarySpecProgramIndicatorList.Codes.A, ZDateTime.Today, countryOfOrigin));
		}

		public void TestGetDispositionCodeDescriptionList()
		{
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var dataGrouping = Core.Constants.CountryCodes.UnitedStates;
			var so50CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode;
			var so60CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO60RecordDispCode;

			Helper.CreateNewOrGetExistingCusCodeType(so50CodeType, "SO50RecordDispCode", dataGrouping);
			Helper.CreateNewOrGetExistingCusCodeList(dataGrouping, so50CodeType, "51", "MANIFEST HOLD CBP", startDate, endDate);

			Helper.CreateNewOrGetExistingCusCodeType(so60CodeType, "SO60RecordDispCode", dataGrouping);
			Helper.CreateNewOrGetExistingCusCodeList(dataGrouping, so60CodeType, "01", "ONEUSG", startDate, endDate);
			Factory.Save();

			var so50DispositionCodes = UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, so50CodeType);
			AssertEquals(typeof(CodeDescriptionPairList), so50DispositionCodes.GetType());
			AssertEquals(2, so50DispositionCodes.Count);
			AssertEquals("MANIFEST HOLD CBP", so50DispositionCodes.GetDescriptionFromCode("51"));
			AssertEquals(SEBillProcessingResultList.BillStatusHoldOrExamDesc, so50DispositionCodes.GetDescriptionFromCode(SEBillProcessingResultList.BillStatusHoldOrExam));

			var so60DispositionCodes = UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, so60CodeType);
			AssertEquals(typeof(CodeDescriptionPairList), so60DispositionCodes.GetType());
			AssertEquals(1, so60DispositionCodes.Count);
			AssertEquals("ONEUSG", so60DispositionCodes.GetDescriptionFromCode("01"));
		}

		[TestDate(2011, 1, 2)]
		public void TestGetRefCusCodeList()
		{
			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var dataGrouping = Core.Constants.CountryCodes.UnitedStates;
			var nmfsCategoryCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NMFSCategoryCode;

			Helper.CreateNewOrGetExistingCusCodeType(nmfsCategoryCodeType, "NMFSCategoryCode", dataGrouping);
			Helper.CreateNewOrGetExistingCusCodeList(dataGrouping, nmfsCategoryCodeType, "BBF", "Baitboat: Freezer", startDate, endDate);
			Helper.CreateNewOrGetExistingCusCodeList(dataGrouping, nmfsCategoryCodeType, "BBI", "Baitboat: Ice-well", startDate, endDate);
			Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, nmfsCategoryCodeType, "BB", "Baitboat", startDate, endDate);
			Helper.CreateNewOrGetExistingCusCodeList(dataGrouping, nmfsCategoryCodeType, "BLL", "Longline: Bottom or Deep longliners", startDate, date.AddDays(-5));
			Factory.Save();

			var nmfsCategoryCodeList = UniversalReferenceDataHelper.GetRefCusCodeList(Factory, nmfsCategoryCodeType);
			AssertEquals(typeof(CodeDescriptionPairList), nmfsCategoryCodeList.GetType());
			AssertEquals(2, nmfsCategoryCodeList.Count);
			AssertEquals("Baitboat: Freezer", nmfsCategoryCodeList.GetDescriptionFromCode("BBF"));
			AssertEquals("Baitboat: Ice-well", nmfsCategoryCodeList.GetDescriptionFromCode("BBI"));

			Helper.CreateNewOrGetExistingCusCodeList(dataGrouping, nmfsCategoryCodeType, "TES", "Test", startDate, endDate);
			Factory.Save();

			nmfsCategoryCodeList = UniversalReferenceDataHelper.GetRefCusCodeList(Factory, nmfsCategoryCodeType);
			AssertEquals(2, nmfsCategoryCodeList.Count);

			Factory.ClearCachedValue<CodeDescriptionPairList>("GetRefCusCodeList" + nmfsCategoryCodeType + date.ToString("MMddyy"));
			Factory.ReloadAll<Universal.ZZRefCusCodeListCombined>();
			nmfsCategoryCodeList = UniversalReferenceDataHelper.GetRefCusCodeList(Factory, nmfsCategoryCodeType);
			AssertEquals(3, nmfsCategoryCodeList.Count);
			AssertEquals("Test", nmfsCategoryCodeList.GetDescriptionFromCode("TES"));
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
