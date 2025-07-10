using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGlobalChargeCodeConsolidatorTest : TestCaseWithFactory
	{
		public void TestConsolidateToGlobal_PassedNothing()
		{
			DoConsolidation(System.Array.Empty<AccChargeCode>());
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_AccrualAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_CostAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_RevenueAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_WIPAccount);
			AssertEquals("Field takes default value", true, Result.AC_AllowDescriptionOvertype);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_ExpenseGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_SalesGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AX_TaxOverrideGroup);
			AssertEquals("Field takes default value", "NGC", Result.AC_ChargeGroup);
			AssertEquals("Field takes default value", "PRC", Result.AC_ChargeOtherGroups);
			AssertEquals("Field takes default value", "", Result.AC_ChargeSubGroup);
			AssertEquals("Field takes default value", "", Result.AC_ChargeType);
			AssertEquals("Field takes default value", "", Result.AC_Code);
			AssertEquals("Field takes default value", "ALL", Result.AC_DepartmentFilterList);
			AssertEquals("Field takes default value", "", Result.AC_Desc);
			AssertEquals("Field takes default value", "", Result.AC_ENettChargeCodeMap);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_GC);
			AssertEquals("Field takes default value", "SRV", Result.AC_GoodsServiceType);
			AssertEquals("Field takes default value", "", Result.AC_IATA_ChargeCodeMap);
			AssertEquals("Field takes default value", true, Result.AC_IsActive);
			AssertEquals("Field takes default value", false, Result.AC_IsCommissionable);
			AssertEquals("Field takes default value", false, Result.AC_IsGroupageCharge);
			AssertEquals("Field takes default value", "", Result.AC_LocalLanguageDescription);
			AssertEquals("Field takes default value", 0M, Result.AC_MarginPercentage);
			AssertEquals("Field takes default value", (short)0, Result.AC_PrintSequence);
			AssertEquals("Field takes default value", "", Result.AC_RateCalculator);
			AssertEquals("Field takes default value", true, Result.AC_ShowOnQuotation);
			AssertEquals("Field takes default value", false, Result.AC_SuppressOnQuoteIfZero);
			AssertEquals("Field takes default value", 0, Result.ChargeTypeOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.RevenueRecOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.GLPostingOverrides.Count);
		}

		public void TestConsolidateToGlobal_PassedOneCode_DefaultValues()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CC1";
			DoConsolidation(new AccChargeCode[] { chargeCode });
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_AccrualAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_CostAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_RevenueAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_WIPAccount);
			AssertEquals("Field takes default value", true, Result.AC_AllowDescriptionOvertype);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_ExpenseGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_SalesGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AX_TaxOverrideGroup);
			AssertEquals("Field takes default value", "NGC", Result.AC_ChargeGroup);
			AssertEquals("Field takes default value", "PRC", Result.AC_ChargeOtherGroups);
			AssertEquals("Field takes default value", "", Result.AC_ChargeSubGroup);
			AssertEquals("Field takes default value", "", Result.AC_ChargeType);
			AssertEquals("Field takes set value", "CC1", Result.AC_Code);
			AssertEquals("Field takes default value", "ALL", Result.AC_DepartmentFilterList);
			AssertEquals("Field takes default value", "", Result.AC_Desc);
			AssertEquals("Field takes default value", "", Result.AC_ENettChargeCodeMap);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_GC);
			AssertEquals("Field takes default value", "SRV", Result.AC_GoodsServiceType);
			AssertEquals("Field takes default value", "", Result.AC_IATA_ChargeCodeMap);
			AssertEquals("Field takes default value", true, Result.AC_IsActive);
			AssertEquals("Field takes default value", false, Result.AC_IsCommissionable);
			AssertEquals("Field takes default value", false, Result.AC_IsGroupageCharge);
			AssertEquals("Field takes default value", "", Result.AC_LocalLanguageDescription);
			AssertEquals("Field takes default value", 0M, Result.AC_MarginPercentage);
			AssertEquals("Field takes default value", (short)0, Result.AC_PrintSequence);
			AssertEquals("Field takes default value", "", Result.AC_RateCalculator);
			AssertEquals("Field takes default value", true, Result.AC_ShowOnQuotation);
			AssertEquals("Field takes default value", false, Result.AC_SuppressOnQuoteIfZero);
			AssertEquals("Field takes default value", 0, Result.ChargeTypeOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.RevenueRecOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.GLPostingOverrides.Count);
		}

		public void TestConsolidateToGlobal_PassedOneCode_OtherValues()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CC1";
			var costPK = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Cost);
			chargeCode.AC_AG_AccrualAccount = costPK;
			chargeCode.AC_Desc = "Desc001";
			chargeCode.AC_MarginPercentage = 33M;
			new AccChargeCodeValidationTest.ChargeTypeOverridesHelper().PopulateCollection(chargeCode);
			new AccChargeCodeValidationTest.TaxOverridesHelper().PopulateCollection(chargeCode);
			new AccChargeCodeValidationTest.BranchOverridesHelper().PopulateCollection(chargeCode);
			chargeCode.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			chargeCode.AC_AW_WithholdingTaxRate = Factory.NewWithValidTestData<AccWithholding>().PK;
			DoConsolidation(new AccChargeCode[] { chargeCode });

			AssertEquals("Field taken from local charge code", costPK, Result.AC_AG_AccrualAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_CostAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_RevenueAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_WIPAccount);
			AssertEquals("Field takes default value", true, Result.AC_AllowDescriptionOvertype);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_ExpenseGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_SalesGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AX_TaxOverrideGroup);
			AssertEquals("Field takes default value", "NGC", Result.AC_ChargeGroup);
			AssertEquals("Field takes default value", "PRC", Result.AC_ChargeOtherGroups);
			AssertEquals("Field takes default value", "", Result.AC_ChargeSubGroup);
			AssertEquals("Field takes default value", "", Result.AC_ChargeType);
			AssertEquals("Field takes set value", "CC1", Result.AC_Code);
			AssertEquals("Field takes default value", "ALL", Result.AC_DepartmentFilterList);
			AssertEquals("Field taken from local charge code", "Desc001", Result.AC_Desc);
			AssertEquals("Field takes default value", "", Result.AC_ENettChargeCodeMap);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_GC);
			AssertEquals("Field takes default value", "SRV", Result.AC_GoodsServiceType);
			AssertEquals("Field takes default value", "", Result.AC_IATA_ChargeCodeMap);
			AssertEquals("Field takes default value", true, Result.AC_IsActive);
			AssertEquals("Field takes default value", false, Result.AC_IsCommissionable);
			AssertEquals("Field takes default value", false, Result.AC_IsGroupageCharge);
			AssertEquals("Field takes default value", "", Result.AC_LocalLanguageDescription);
			AssertEquals("Field taken from local charge code", 33M, Result.AC_MarginPercentage);
			AssertEquals("Field takes default value", (short)0, Result.AC_PrintSequence);
			AssertEquals("Field takes default value", "", Result.AC_RateCalculator);
			AssertEquals("Field takes default value", true, Result.AC_ShowOnQuotation);
			AssertEquals("Field takes default value", false, Result.AC_SuppressOnQuoteIfZero);
			AssertEquals("Field taken from local charge code", 1, Result.ChargeTypeOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.RevenueRecOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.GLPostingOverrides.Count);
		}

		public void TestConsolidateToGlobal_PassedManyCodes_OneWayAround()
		{
			TestConsolidateToGlobal_PassedManyCodes(true);
		}

		public void TestConsolidateToGlobal_PassedManyCodes_TheOtherWayAround()
		{
			TestConsolidateToGlobal_PassedManyCodes(false);
		}

		void TestConsolidateToGlobal_PassedManyCodes(bool switchOrder)
		{
			var costPK = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Cost);

			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "CC1";
			chargeCode1.AC_AG_AccrualAccount = costPK;
			chargeCode1.AC_Desc = "Desc001";
			chargeCode1.AC_MarginPercentage = 33M;
			new AccChargeCodeValidationTest.ChargeTypeOverridesHelper().PopulateCollection(chargeCode1);
			new AccChargeCodeValidationTest.GLPostingOverridesHelper().PopulateCollection(chargeCode1);
			new AccChargeCodeValidationTest.TaxOverridesHelper().PopulateCollection(chargeCode1);
			new AccChargeCodeValidationTest.BranchOverridesHelper().PopulateCollection(chargeCode1);
			chargeCode1.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			chargeCode1.AC_AW_WithholdingTaxRate = Factory.NewWithValidTestData<AccWithholding>().PK;

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "CC2";
			chargeCode2.AC_AG_AccrualAccount = costPK;
			chargeCode2.AC_Desc = "Desc002";
			chargeCode2.AC_MarginPercentage = 33M;
			new AccChargeCodeValidationTest.ChargeTypeOverridesHelper().PopulateCollection(chargeCode2).PokeLocalChargeCode(chargeCode2);
			new AccChargeCodeValidationTest.GLPostingOverridesHelper().PopulateCollection(chargeCode2);
			new AccChargeCodeValidationTest.TaxOverridesHelper().PopulateCollection(chargeCode2);
			new AccChargeCodeValidationTest.BranchOverridesHelper().PopulateCollection(chargeCode2);
			chargeCode2.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			chargeCode2.AC_AW_WithholdingTaxRate = Factory.NewWithValidTestData<AccWithholding>().PK;

			var chargeCode3 = Factory.New<AccChargeCode>();
			chargeCode3.AC_Code = "CC1";
			chargeCode3.AC_AG_AccrualAccount = costPK;
			chargeCode3.AC_Desc = "Desc001";
			chargeCode3.AC_MarginPercentage = 33M;
			new AccChargeCodeValidationTest.ChargeTypeOverridesHelper().PopulateCollection(chargeCode3);
			new AccChargeCodeValidationTest.GLPostingOverridesHelper().PopulateCollection(chargeCode3);
			new AccChargeCodeValidationTest.TaxOverridesHelper().PopulateCollection(chargeCode3);
			new AccChargeCodeValidationTest.BranchOverridesHelper().PopulateCollection(chargeCode3);
			chargeCode3.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			chargeCode3.AC_AW_WithholdingTaxRate = Factory.NewWithValidTestData<AccWithholding>().PK;
			chargeCode3.AC_GC = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"))[0].PK;

			if (switchOrder)
			{
				DoConsolidation(new AccChargeCode[] { chargeCode2, chargeCode1, chargeCode3 });
			}
			else
			{
				DoConsolidation(new AccChargeCode[] { chargeCode1, chargeCode2, chargeCode3 });
			}

			AssertEquals("Field taken from local charge code", costPK, Result.AC_AG_AccrualAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_CostAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_RevenueAccount);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AG_WIPAccount);
			AssertEquals("Field takes default value", true, Result.AC_AllowDescriptionOvertype);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_ExpenseGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AR_SalesGroup);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_AX_TaxOverrideGroup);
			AssertEquals("Field takes default value", "NGC", Result.AC_ChargeGroup);
			AssertEquals("Field takes default value", "PRC", Result.AC_ChargeOtherGroups);
			AssertEquals("Field takes default value", "", Result.AC_ChargeSubGroup);
			AssertEquals("Field takes default value", "", Result.AC_ChargeType);
			AssertEquals("Field takes default value", "", Result.AC_Code);
			AssertEquals("Field takes default value", "ALL", Result.AC_DepartmentFilterList);
			AssertEquals("Field takes default value because values disagree", "", Result.AC_Desc);
			AssertEquals("Field takes default value", "", Result.AC_ENettChargeCodeMap);
			AssertEquals("Field takes default value", ZGuid.Empty, Result.AC_GC);
			AssertEquals("Field takes default value", "SRV", Result.AC_GoodsServiceType);
			AssertEquals("Field takes default value", "", Result.AC_IATA_ChargeCodeMap);
			AssertEquals("Field takes default value", true, Result.AC_IsActive);
			AssertEquals("Field takes default value", false, Result.AC_IsCommissionable);
			AssertEquals("Field takes default value", false, Result.AC_IsGroupageCharge);
			AssertEquals("Field takes default value", "", Result.AC_LocalLanguageDescription);
			AssertEquals("Field taken from local charge code", 33M, Result.AC_MarginPercentage);
			AssertEquals("Field takes default value", (short)0, Result.AC_PrintSequence);
			AssertEquals("Field takes default value", "", Result.AC_RateCalculator);
			AssertEquals("Field takes default value", true, Result.AC_ShowOnQuotation);
			AssertEquals("Field takes default value", false, Result.AC_SuppressOnQuoteIfZero);
			AssertEquals("Field takes default value because values disagree", 0, Result.ChargeTypeOverrides.Count);
			AssertEquals("Field takes default value", 0, Result.RevenueRecOverrides.Count);
			AssertEquals("Field takes default value", 1, Result.GLPostingOverrides.Count);
		}

		public void TestConsolidateToGlobal_IgnoreLocal_AC_ChargeSubGroup()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CC1";
			chargeCode.AC_ChargeGroup = "BRK";
			chargeCode.AC_ChargeSubGroup = "@#$"; // A subgroup that isn't accepted at a global level
			DoConsolidation(new AccChargeCode[] { chargeCode });
			AssertEquals("Field takes default value", "", Result.AC_ChargeSubGroup);
			chargeCode.AC_ChargeSubGroup = "XIN"; // A subgroup that is accepted at a global level
			DoConsolidation(new AccChargeCode[] { chargeCode });
			AssertEquals("Field taken from local charge code", "XIN", Result.AC_ChargeSubGroup);
		}

		#region Implementation

		AccChargeCode Result;

		void DoConsolidation(AccChargeCode[] accChargeCodes)
		{
			Result = new AccGlobalChargeCodeConsolidator(Factory).ConsolidateToGlobal(accChargeCodes);
			if (Result.AC_ChargeGroup == string.Empty)
			{
				Result.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			}
		}

		protected override void TearDown()
		{
			AssertEquals("Should always be global", true, Result.IsGlobal);
			AssertEquals("Should always allow the generated code to match existing local charge codes", true, Result.AllowCodeToMatchExisting);
			AssertEquals("Should never create tax overrides", 0, Result.TaxOverrides.Count);
			AssertEquals("Should never create branch overrides", 0, Result.BranchOverrides.Count);
			AssertEquals("Should never create GST tax", ZGuid.Empty, Result.AC_AT_GSTRate);
			AssertEquals("Should never create withholding tax", ZGuid.Empty, Result.AC_AW_WithholdingTaxRate);
			base.TearDown();
		}

		#endregion
	}
}
