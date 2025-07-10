using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceLineLookups))]
sealed class JobComInvoiceLineLookupsBaseOnlyTest : JobComInvoiceLineLookupsAbstractTest<JobComInvoiceLineLookups>
{
	protected override string MessageType => SharedJobMessageTypeList.Codes.MiscellaneousCustoms;

	public void TestJI_Procedure()
	{
		var codeList = lookups.Procedures;
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.Procedures, codeList);
			AssertType<CodeDescriptionPairList>("Type", codeList);
		});
	}

	public void TestJI_ValuationCode()
	{
		var codeList = invoiceLine.Lookups.ValuationCodeList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", invoiceLine.Lookups.ValuationCodeList, codeList);
			AssertType<ValuationMethodList>("Type", codeList);
			AssertEquals(6, codeList.Count);
		});
	}

	public void TestJI_ValuationCode_NorwegianTranslation()
	{
		using (Res.TemporarilySwitchLanguage(Constants.Languages.Norwegian))
		{
			CombineAssertions(() =>
			{
				AssertEquals("Varens transaksjonsverdi (tolloven §7-10)", ValuationMethodList.Descriptions.ValueOfImportedGoods);
				AssertEquals("Transaksjonsverdien av lignende vare (tolloven §7-12)", ValuationMethodList.Descriptions.ValueOfSimilarGoods);
			});
		}
	}

	public void TestJI_StateOrRegionOfOrigin()
	{
		invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Norway;
		var countyList = lookups.NOStateOrRegionOfOrigin;
		CombineAssertions("When Country of Origin is NO", () =>
		{
			AssertSame("Cached", lookups.NOStateOrRegionOfOrigin, countyList);
			AssertContainsInOrder("Does not contain extra codes for NO", countyList.CodesAsString, "23", "29", "99");
			AssertEquals("Default code", null, countyList.DefaultCode);
		});

		CombineAssertions("When Country of Origin is not NO", () =>
		{
			invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Denmark;
			countyList = lookups.NOStateOrRegionOfOrigin;
			AssertContainsInOrder("CountyList", countyList.CodesAsString, "91");
			AssertEquals("Default code", "91", countyList.DefaultCode);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			countyList = lookups.NOStateOrRegionOfOrigin;
			AssertContainsInOrder("Empty country: CountyList", countyList.CodesAsString, "91");
			AssertEquals("Empty country: Default code", "91", countyList.DefaultCode);
		});
	}

	public void TestJI_ZZF_NKTaxType_PrivateCodesAvailable()
	{
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(Factory);
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);

		var tariffTest1 = tariffTestHelper.CreateImportTariff("77777777");
		tariffTestHelper.AddVatApplicability(tariffTest1, RefCusTaxOrFee.MV1);

		var tariffTest2 = tariffTestHelper.CreateImportTariff("88888888");
		tariffTestHelper.AddVatApplicability(tariffTest2, RefCusTaxOrFee.MV1);
		tariffTestHelper.AddVatApplicability(tariffTest2, RefCusTaxOrFee.MV2);

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariffTest1.ZZ1_TariffCode;
			AssertContainsExactElementsInAnyOrder("Does not contain private tax codes", new string[] { RefCusTaxOrFee.MV1, RefCusTaxOrFee.MVF, RefCusTaxOrFee.MVK }, lookups.TaxOrFeeCodeList.GetAllCodes());

			invoiceLine.JI_Tariff = tariffTest2.ZZ1_TariffCode;
			AssertContainsExactElementsInAnyOrder("Does not contain private tax codes", new string[] { RefCusTaxOrFee.MV1, RefCusTaxOrFee.MV2, RefCusTaxOrFee.MVF, RefCusTaxOrFee.MVK }, lookups.TaxOrFeeCodeList.GetAllCodes());
		});
	}

	public void TestCustomsUQList_Cached() => AssertCached(() => lookups.CustomsUQList);

	public void TestCustomsUQList_Type() => AssertType<CodeDescriptionPairList>("Type", lookups.CustomsUQList);

	public void TestCustomsUQList() => CombineAssertions(() =>
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		var codeList = lookups.CustomsUQList;

		invoiceLine.JI_Tariff = "33333333";
		AssertContainsExactElementsInAnyOrder("Codes from list", ["KGM"], lookups.CustomsUQList.GetAllCodes());
	});

	public void TestCustomsUnitQtyList_Cached() => AssertCached(() => lookups.CustomsUnitQtyList);

	public void TestCustomsUnitQtyList_Type() => AssertType<CodeDescriptionPairList>("Type", lookups.CustomsUnitQtyList);

	public void TestCustomsUnitQtyList() => CombineAssertions(() =>
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		var codeList = lookups.CustomsUnitQtyList;

		AssertSame("Cached", lookups.CustomsUnitQtyList, codeList);
		AssertType<CodeDescriptionPairList>("Type", codeList);

		invoiceLine.JI_Tariff = "11111111";
		AssertEquals("Codes from list", "KGM", lookups.CustomsUnitQtyList.CodesAsString);
	});

	public void TestCustomsSecondUnitQtyList_Cached() => AssertCached(() => lookups.CustomsSecondUnitQtyList);

	public void TestCustomsSecondUnitQtyList_Type() => AssertType<CodeDescriptionPairList>("Type", lookups.CustomsSecondUnitQtyList);

	public void TestCustomsSecondUnitQtyList() => CombineAssertions(() =>
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		var codeList = lookups.CustomsSecondUnitQtyList;
		invoiceLine.JI_Tariff = "11111111";
		AssertEquals("When CU2 code available", "LTR", lookups.CustomsSecondUnitQtyList.CodesAsString);

		invoiceLine.JI_Tariff = "22222222";
		AssertEquals("When CU2 code is not available", "NMB", lookups.CustomsSecondUnitQtyList.CodesAsString);
	});

	public void TestCustomsTypeByTariff()
	{
		SetupTariffs();
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = "US";
		invoiceLine.JI_Tariff = TariffConstants.StTariff;
		var codeList = lookups.CustomsOverrideTypeList;
		CombineAssertions(() =>
		{
			AssertEquals(1, codeList.Count);
			AssertEquals(RateTypeCodeList.Codes.Piece, codeList[0].Code);
		});
	}

	public void TestCustomsTypeByMultiRateTariff()
	{
		SetupTariffs();
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = "US";
		invoiceLine.JI_Tariff = TariffConstants.KgLtrTariff;
		var codeList = lookups.CustomsOverrideTypeList;
		var codes = codeList.GetAllCodes();
		CombineAssertions(() =>
		{
			AssertEquals(2, codeList.Count);
			AssertCollectionContains(RateTypeCodeList.Codes.Kilogram, codes);
			AssertCollectionContains(RateTypeCodeList.Codes.Liter, codes);
		});
	}

	public void TestFormulaUnitToRateType()
	{
		CombineAssertions(() =>
		{
			assertFormulaUnitToRateType(NOCustomsFormulaUnitCodeList.Codes.GRM, RateTypeCodeList.Codes.Gram, NOCustomsFormulaUnitCodeList.Descriptions.GRM);
			assertFormulaUnitToRateType(NOCustomsFormulaUnitCodeList.Codes.KGM, RateTypeCodeList.Codes.Kilogram, NOCustomsFormulaUnitCodeList.Descriptions.KGM);
			assertFormulaUnitToRateType(NOCustomsFormulaUnitCodeList.Codes.LTR, RateTypeCodeList.Codes.Liter, NOCustomsFormulaUnitCodeList.Descriptions.LTR);
			assertFormulaUnitToRateType(NOCustomsFormulaUnitCodeList.Codes.MTQ, RateTypeCodeList.Codes.CubicMeter, NOCustomsFormulaUnitCodeList.Descriptions.MTQ);
			assertFormulaUnitToRateType(NOCustomsFormulaUnitCodeList.Codes.NMB, RateTypeCodeList.Codes.Piece, NOCustomsFormulaUnitCodeList.Descriptions.NMB);
			assertFormulaUnitToRateType(NOCustomsFormulaUnitCodeList.Codes.VFD, RateTypeCodeList.Codes.PercentSign, NOCustomsFormulaUnitCodeList.Descriptions.VFD);
		});
		void assertFormulaUnitToRateType(ZString formulaUnit, ZString expectedCode, ZString expectedDescription)
		{
			var codePair = JobComInvoiceLineLookups.FormulaUnitToRateType(formulaUnit);
			AssertEquals($"{formulaUnit} Code", expectedCode, codePair.Code);
			AssertEquals($"{formulaUnit} Description", expectedDescription, codePair.Description);
		}
	}

	public void TestAdditionalCodesList() => CombineAssertions(() =>
	{
		SupplementaryCodeTestHelper.SetupTariffAndCusCodeList(Factory);

		invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Botswana;
		invoiceLine.JI_Tariff = "DUMMYTRF";

		var additionalCodesListAll = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType is Empty, Only Excise codes should be added to the list",
			new ZString[] { "FA400", "MA400", "MB400", "MP400", "GA400", "GB400", "GP400" },
			additionalCodesListAll.GetAllCodesZString());
		AssertEquals("Description for FA400", "FA400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("FA400"));
		AssertEquals("Description for MA400", "MA400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("MA400"));
		AssertEquals("Description for MB400", "MB400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("MB400"));
		AssertEquals("Description for MP400", "MP400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("MP400"));
		AssertEquals("Description for GA400", "GA400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("GA400"));
		AssertEquals("Description for GB400", "GB400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("GB400"));
		AssertEquals("Description for GP400", "GP400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("GP400"));

		invoiceLine.JI_PackageType = "A";
		var additionalCodesListPackageTypeA = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType == A", new ZString[] { "FA400", "MA400", "GA400" }, additionalCodesListPackageTypeA.GetAllCodesZString());

		invoiceLine.JI_PackageType = "B";
		var additionalCodesListPackageTypeB = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType == B", new ZString[] { "FA400", "MB400", "GB400" }, additionalCodesListPackageTypeB.GetAllCodesZString());

		invoiceLine.JI_PackageType = "P";
		var additionalCodesListPackageTypeP = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType == P", new ZString[] { "FA400", "MP400", "GP400" }, additionalCodesListPackageTypeP.GetAllCodesZString());
	});

	public void TestReducedCustomsFlagList()
	{
		var codeList = lookups.ReducedCustomsFlagList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.ReducedCustomsFlagList, codeList);
			AssertType<ReducedCustomsFlagList>("Type", codeList);
			AssertEquals(2, codeList.Count);
		});
	}

	public void TestCustomsOverrideTypeList_Cached()
	{
		var codeList = lookups.CustomsOverrideTypeList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.CustomsOverrideTypeList, codeList);
			AssertType<CodeDescriptionPairList>("Type", codeList);
		});
	}

	public void TestCustomsOverrideTypeList()
	{
		SetupTariffs();
		invoiceLine.JI_Tariff = TariffConstants.KgLtrTariff;
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = "US";

		AssertContainsExactElementsInAnyOrder("Codes from list", ["K", "L"], lookups.CustomsOverrideTypeList.GetAllCodes());
	}

	public void TestPackageTypeList()
	{
		var codeList = lookups.PackageTypeList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.PackageTypeList, codeList);
			AssertType<NOPackageTypes>("Type", codeList);
			AssertContains("Codes", "A, B, G, P", codeList.CodesAsString);
		});
	}

	void SetupTariffs()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var vfdTariff = tariffTestHelper.CreateImportTariff(TariffConstants.StTariff);
		tariffTestHelper.AddRate(vfdTariff, PrimaryPreferenceCodeList.Codes.N, "1 * [NMB]", Constants.CountryCodes.UnitedStates);
		var kgTariff = tariffTestHelper.CreateImportTariff(TariffConstants.KgLtrTariff);
		tariffTestHelper.AddRate(kgTariff, PrimaryPreferenceCodeList.Codes.N, "2 * [KGM]", Constants.CountryCodes.UnitedStates);
		tariffTestHelper.AddRate(kgTariff, PrimaryPreferenceCodeList.Codes.N, "2.5 * [LTR]", Constants.CountryCodes.UnitedStates);
	}

	class TariffConstants
	{
		public const string StTariff = "10001000";
		public const string KgLtrTariff = "10002000";
	}
}
