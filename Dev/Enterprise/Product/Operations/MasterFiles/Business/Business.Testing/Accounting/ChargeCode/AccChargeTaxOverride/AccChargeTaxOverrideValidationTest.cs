using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeTaxOverrideValidationForTaxFrameworkTaxOverrideGroup_NotInDBTest : AccChargeTaxOverrideValidationForTaxFrameworkTaxOverrideGroupTest
	{
		protected override void SetupForTaxFramework(AccChargeTaxOverride chargeTaxOverride)
		{
			Assert("Precondition: IsInDatabase", !chargeTaxOverride.TaxOverrideGroup.IsInDatabase);
			Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
		}
	}

	sealed class AccChargeTaxOverrideValidationForTaxFrameworkTaxOverrideGroup_InDBTest : AccChargeTaxOverrideValidationForTaxFrameworkTaxOverrideGroupTest
	{
		protected override void SetupForTaxFramework(AccChargeTaxOverride chargeTaxOverride)
		{
			var pivot = chargeTaxOverride.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			pivot.FillWithValidTestData();
			chargeTaxOverride.TaxOverrideGroup.FillWithValidTestData();
			chargeTaxOverride.FillWithValidTestData();
			chargeTaxOverride.AO_AT = ZGuid.Empty;
			pivot.Factory.Save();
		}

		protected override bool UseNewFactoryAsNewObjectWillBeSavedInDB => true;
	}

	public abstract class AccChargeTaxOverrideValidationForTaxFrameworkTaxOverrideGroupTest : AccChargeTaxOverrideValidationForTaxOverrideGroupTest
	{
		protected sealed override void AdditionalSetupForNewChargeTaxOverride(AccChargeTaxOverride chargeTaxOverride)
		{
			base.AdditionalSetupForNewChargeTaxOverride(chargeTaxOverride);
			SetupForTaxFramework(chargeTaxOverride);
			Assert("Precondition: IsTaxFrameworkRelated", chargeTaxOverride.TaxOverrideGroup.IsTaxFrameworkRelated);
		}

		protected abstract void SetupForTaxFramework(AccChargeTaxOverride chargeTaxOverride);

		public override void TestCheckAO_AT()
		{
			var newFactory = new BusinessObjectFactory();

			var taxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_Code = "ACTIVE";
			taxRate.AT_IsActive = true;
			taxRate.AT_TaxSystemCode = "test";
			newFactory.Save();

			var pivots = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots;
			BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.DeleteAll();
			AssertEquals("Precondition: pivot collection count is 0", 0, pivots.Count);
			AssertEquals("Precondition:", ZGuid.Empty, BizO.AO_AT);
			BizO.Validation.ValidateAO_AT();
			AssertHasError(BizO.AO_ATInfo, "Please enter a Tax ID.");

			BizO.AO_AT = Guid.NewGuid();
			AssertHasError(BizO.AO_ATInfo, "Enter a valid Tax ID.");

			BizO.AO_AT = taxRate.PK;
			AssertNoErrors(BizO.AO_ATInfo);

			BizO.AO_AT = ZGuid.Empty;
			var pivot1 = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			AssertEquals("Precondition: Pivot collection count is one", 1, pivots.Count);
			AssertEquals("Precondition:", ZGuid.Empty, pivot1.AXP_AT_TaxID);
			BizO.Validation.ValidateAO_AT();
			AssertHasError(BizO.AO_ATInfo, "Please enter a Tax ID.");

			pivot1.AXP_AT_TaxID = taxRate.PK;
			BizO.Validation.ValidateAO_AT();
			AssertNoErrors(BizO.AO_ATInfo);

			BizO.AO_AT = taxRate.PK;
			AssertHasError(BizO.AO_ATInfo, "Tax ID must be empty here as the Tax ID is already been entered in the Tax Framework Configuration grid.");

			pivot1.AXP_AT_TaxID = ZGuid.Empty;
			BizO.Validation.ValidateAO_AT();
			AssertNoErrors(BizO.AO_ATInfo);

			var pivot2 = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			AssertEquals("Precondition: Pivot collection count more than one", 2, pivots.Count);
			AssertNotEquals("Precondition:", ZGuid.Empty, BizO.AO_AT);
			BizO.Validation.ValidateAO_AT();
			AssertHasError(BizO.AO_ATInfo, "Tax ID must be empty here since more than one Tax Configuration has been added to the Tax Framework Configuration grid.");

			pivot1.AXP_AT_TaxID = taxRate.PK;
			pivot2.AXP_AT_TaxID = taxRate.PK;
			BizO.AO_AT = ZGuid.Empty;
			AssertNoErrors(BizO.AO_ATInfo);
		}

		public override void TestCheckAO_ATWithDefaultingRuleIsSUM()
		{
			var newFactory = new BusinessObjectFactory();

			var nonZeroTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			nonZeroTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			nonZeroTaxRate.AT_Code = "NonZero";
			nonZeroTaxRate.AT_IsActive = true;
			nonZeroTaxRate.AT_ReferenceRateType = "STD";
			nonZeroTaxRate.AT_TaxSystemCode = "test";

			var zeroTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			zeroTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			zeroTaxRate.AT_Code = "Zero";
			zeroTaxRate.AT_IsActive = true;
			zeroTaxRate.AT_ReferenceRateType = "ZERO";
			zeroTaxRate.AT_ReferenceExtraRateType = "";
			zeroTaxRate.AT_TaxSystemCode = "test";

			newFactory.Save();

			AssertNotNull("Precondition", BizO.TaxOverrideGroup);
			Assert("Precondition", !nonZeroTaxRate.IsZeroTaxRate);
			Assert("Precondition", zeroTaxRate.IsZeroTaxRate);
			Assert("Precondition", !BizO.AO_ATInfo.ReadOnly);

			var pivots = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots;
			BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.DeleteAll();
			AssertEquals("Precondition: pivot collection count is 0", 0, pivots.Count);
			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount;
			BizO.AO_AT = nonZeroTaxRate.PK;
			AssertHasError(BizO.AO_ATInfo, "Tax ID's Tax Rate must equal 0 when Defaulting Rule is 'SUM'.");

			var pivot1 = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			AssertEquals("Precondition: Pivot collection count is one", 1, pivots.Count);
			AssertEquals("Precondition: ", ZGuid.Empty, pivot1.AXP_AT_TaxID);
			AssertHasError(BizO.AO_ATInfo, "Tax ID's Tax Rate must equal 0 when Defaulting Rule is 'SUM'.");

			pivot1.AXP_AT_TaxID = nonZeroTaxRate.PK;
			BizO.AO_AT = ZGuid.Empty;
			AssertNoErrors(BizO.AO_ATInfo);

			pivot1.AXP_AT_TaxID = ZGuid.Empty;
			BizO.AO_AT = zeroTaxRate.PK;
			BizO.Validation.ValidateAO_AT();
			AssertNoErrors(BizO.AO_ATInfo);
		}

		public void TestCheckAO_ATTaxSystemValidation()
		{
			var newFactory = new BusinessObjectFactory();

			var taxConfiguration = newFactory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TEST";

			var taxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_Code = "ACTIVE";
			taxRate.AT_TaxSystemCode = "TEST";

			var taxRate1 = newFactory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.AT_Code = "AAABBB";
			taxRate1.AT_TaxSystemCode = "TSTTST";

			newFactory.Save();

			Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			var pivots = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots;
			BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.DeleteAll();
			AssertEquals("Precondition: pivot collection count is 0", 0, pivots.Count);
			BizO.AO_AT = taxRate1.PK;
			AssertNoErrors(BizO.AO_ATInfo);

			BizO.AO_AT = taxRate.PK;
			AssertNoErrors(BizO.AO_ATInfo);

			var pivot1 = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			pivot1.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertEquals("Precondition: Pivot collection count is one", 1, pivots.Count);
			AssertEquals("Precondition:", ZGuid.Empty, pivot1.AXP_AT_TaxID);

			BizO.AO_AT = taxRate1.PK;
			AssertHasError(BizO.AO_ATInfo, "Enter a valid Tax ID.");

			BizO.AO_AT = taxRate.PK;
			AssertNoErrors(BizO.AO_ATInfo);
		}

		protected override bool IsTaxConfigured => true;
	}

	sealed class AccChargeTaxOverrideValidationForVATTaxOverrideGroupTest : AccChargeTaxOverrideValidationForTaxOverrideGroupTest
	{
	}

	public abstract class AccChargeTaxOverrideValidationForTaxOverrideGroupTest : AccChargeTaxOverrideValidationTest
	{
		protected override bool IsChargeTypeApplicable => false;

		protected sealed override AccChargeTaxOverride GetNewChargeTaxOverride()
		{
			var factoryForNewBizo = UseNewFactoryAsNewObjectWillBeSavedInDB ? new BusinessObjectFactory() : Factory;
			var bizO = factoryForNewBizo.New<AccChargeTaxOverride>();
			bizO.AO_ParentID = factoryForNewBizo.New<AccTaxOverrideGroup>().PK;
			bizO.AO_ParentTableCode = AccTaxOverrideGroupSchema.Constants.Prefix;

			AdditionalSetupForNewChargeTaxOverride(bizO);

			return UseNewFactoryAsNewObjectWillBeSavedInDB ? Factory.Load<AccChargeTaxOverride>(bizO.PK) : bizO;
		}

		protected virtual void AdditionalSetupForNewChargeTaxOverride(AccChargeTaxOverride chargeTaxOverride) { }

		protected virtual bool UseNewFactoryAsNewObjectWillBeSavedInDB => false;
	}

	public class AccChargeTaxOverrideValidationForChargeCodeTest : AccChargeTaxOverrideValidationTest
	{
		protected override bool IsChargeTypeApplicable => true;

		protected override AccChargeTaxOverride GetNewChargeTaxOverride()
		{
			var bizO = Factory.New<AccChargeTaxOverride>();
			bizO.AO_ParentID = Factory.New<AccChargeCode>().PK;
			bizO.AO_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;

			return bizO;
		}
	}

	public abstract class AccChargeTaxOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAO_Origin()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			ZQuery findOtherCountryQuery = new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, currentCountry.PK);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(findOtherCountryQuery);

			SetValuesAndAssertHasErrors(BizO, "ALL", ZString.Empty, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertHasErrors(BizO, "ALL", "XX", ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", currentCountry.Code, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", otherCountry.Code, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", "EUN", ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", "ALL", ZString.Empty, BizO.AO_OriginInfo);

			SetValuesAndAssertHasErrors(BizO, IMP, ZString.Empty, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertHasErrors(BizO, IMP, "XX", ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertHasErrors(BizO, IMP, currentCountry.Code, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, IMP, otherCountry.Code, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, IMP, "EUN", ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, IMP, "ALL", ZString.Empty, BizO.AO_OriginInfo);

			SetValuesAndAssertHasErrors(BizO, EXP, ZString.Empty, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertHasErrors(BizO, EXP, "XX", ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertNoErrors(BizO, EXP, currentCountry.Code, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertHasErrors(BizO, EXP, otherCountry.Code, ZString.Empty, BizO.AO_OriginInfo);
			SetValuesAndAssertHasErrors(BizO, EXP, "EUN", ZString.Empty, BizO.AO_OriginInfo);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_OriginInfo, false);
			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Disbursement);
			SetValuesAndAssertHasErrors(BizO, EXP, "ALL", ZString.Empty, BizO.AO_OriginInfo);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_OriginInfo, false);
			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_OriginInfo, true);
			SetValues(BizO, EXP, ZString.Empty, ZString.Empty);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_OriginInfo, false);
		}

		public void TestCheckAO_Destination()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			ZQuery findOtherCountryQuery = new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, currentCountry.PK);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(findOtherCountryQuery);

			SetValuesAndAssertHasErrors(BizO, "ALL", ZString.Empty, ZString.Empty, BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, "ALL", ZString.Empty, "XX", BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", ZString.Empty, currentCountry.Code, BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", ZString.Empty, otherCountry.Code, BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", ZString.Empty, "EUN", BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, "ALL", ZString.Empty, "ALL", BizO.AO_DestinationInfo);

			SetValuesAndAssertHasErrors(BizO, IMP, ZString.Empty, ZString.Empty, BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, IMP, ZString.Empty, "XX", BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, IMP, ZString.Empty, currentCountry.Code, BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, IMP, ZString.Empty, otherCountry.Code, BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, IMP, ZString.Empty, "EUN", BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, IMP, ZString.Empty, "ALL", BizO.AO_DestinationInfo);

			SetValuesAndAssertHasErrors(BizO, EXP, ZString.Empty, ZString.Empty, BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, EXP, ZString.Empty, "XX", BizO.AO_DestinationInfo);
			SetValuesAndAssertHasErrors(BizO, EXP, ZString.Empty, currentCountry.Code, BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, EXP, ZString.Empty, otherCountry.Code, BizO.AO_DestinationInfo);
			SetValuesAndAssertNoErrors(BizO, EXP, ZString.Empty, "EUN", BizO.AO_DestinationInfo);
			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Disbursement);
			SetValuesAndAssertNoErrors(BizO, EXP, ZString.Empty, "ALL", BizO.AO_DestinationInfo);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_DestinationInfo, false);
			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_DestinationInfo, true);
			SetValues(BizO, EXP, ZString.Empty, ZString.Empty);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_DestinationInfo, false);
		}

		public void TestCheckAO_TaxRegCntry()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			ZQuery findOtherCountryQuery = new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, currentCountry.PK);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(findOtherCountryQuery);

			BizO.AO_TaxRegCntryOrGroup = ZString.Empty;
			BizO.Validation.ValidateAO_TaxRegCntryOrGroup();
			AssertHasErrors(BizO.AO_TaxRegCntryOrGroupInfo);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_TaxRegCntryOrGroupInfo, false);
			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_TaxRegCntryOrGroupInfo, false);
			BizO.AO_TaxRegCntryOrGroup = "SG";
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_TaxRegCntryOrGroupInfo, true);
			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Disbursement);

			BizO.AO_TaxRegCntryOrGroup = "XX";
			BizO.Validation.ValidateAO_TaxRegCntryOrGroup();
			AssertHasErrors(BizO.AO_TaxRegCntryOrGroupInfo);

			BizO.AO_TaxRegCntryOrGroup = currentCountry.RN_Code;
			BizO.Validation.ValidateAO_TaxRegCntryOrGroup();
			AssertNoErrors(BizO.AO_TaxRegCntryOrGroupInfo);

			BizO.AO_TaxRegCntryOrGroup = otherCountry.RN_Code;
			BizO.Validation.ValidateAO_TaxRegCntryOrGroup();
			AssertNoErrors(BizO.AO_TaxRegCntryOrGroupInfo);

			BizO.AO_TaxRegCntryOrGroup = "ALL";
			BizO.Validation.ValidateAO_TaxRegCntryOrGroup();
			AssertNoErrors(BizO.AO_TaxRegCntryOrGroupInfo);

			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Margin);
			BizO.AO_TaxRegCntryOrGroup = "EUN";
			BizO.Validation.ValidateAO_TaxRegCntryOrGroup();
			AssertNoErrors(BizO.AO_TaxRegCntryOrGroupInfo);
		}

		public virtual void TestCheckAO_AT()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				AccTaxRate activeTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
				activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				activeTaxRate.AT_Code = "ACTIVE";
				activeTaxRate.AT_IsActive = true;

				AccTaxRate reverseTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
				reverseTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reverseTaxRate.AT_Code = "GSTREV";
				reverseTaxRate.AT_IsActive = true;

				newFactory.Save();

				BizO.AO_AT = ZGuid.Empty;
				BizO.Validation.ValidateAO_AT();
				AssertHasErrors(BizO.AO_ATInfo);

				BizO.AO_AT = activeTaxRate.PK;
				BizO.Validation.ValidateAO_AT();
				AssertNoErrors(BizO.AO_ATInfo);

				BizO.AO_AT = reverseTaxRate.PK;
				BizO.AO_CostSellAll = "COS";
				BizO.Validation.ValidateAO_AT();
				AssertNoErrors(BizO.AO_ATInfo);

				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_ATInfo, true);
				BizO.AO_AT = ZGuid.Empty;
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_ATInfo, false);
				BizO.AO_AT = reverseTaxRate.PK;
				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Disbursement);
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_ATInfo, false);
			}
		}

		public void TestCheckAO_AT_TaxConfigurationPivots()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccTaxRate activeTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			activeTaxRate.AT_Code = "ACTIVE";
			activeTaxRate.AT_IsActive = true;

			if (IsTaxConfigured)
			{
				var pivots = BizO.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots;
				activeTaxRate.AT_TaxSystemCode = pivots.Count > 0 ? pivots[0].TaxConfiguration.ETC_TaxSystemCode.ToString() : "TEST";
			}

			newFactory.Save();

			AssertValidation(true);
			AssertValidation(false);

			void AssertValidation(bool activated)
			{
				if (activated)
				{
					BizO.AO_CreateTaxRecord = false;
					BizO.AO_AT = ZGuid.Empty;
					BizO.Validation.ValidateAO_AT();
					AssertNoErrors(BizO.AO_ATInfo);

					BizO.AO_CreateTaxRecord = true;
					BizO.AO_AT = activeTaxRate.PK;
					BizO.Validation.ValidateAO_AT();
					AssertNoErrors(BizO.AO_ATInfo);

					BizO.AO_CreateTaxRecord = true;
					BizO.AO_AT = ZGuid.Empty;
					BizO.Validation.ValidateAO_AT();
					AssertHasError(BizO.AO_ATInfo, "Please enter a Tax ID.");
				}
				else
				{
					BizO.AO_AT = ZGuid.Empty;
					BizO.Validation.ValidateAO_AT();
					AssertHasError(BizO.AO_ATInfo, "Please enter a Tax ID.");

					BizO.AO_AT = activeTaxRate.PK;
					BizO.Validation.ValidateAO_AT();
					AssertNoErrors(BizO.AO_ATInfo);
				}
			}
		}

		public void TestCheckAO_A9_DefaultVATClass()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				RefCountry unitedKingdom = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = unitedKingdom.Code;

				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				AccTaxRate activeTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
				activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				activeTaxRate.AT_Code = "ACTIVE";
				activeTaxRate.AT_IsActive = true;

				AccTaxRate reverseTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
				reverseTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reverseTaxRate.AT_Code = "GSTREV";
				reverseTaxRate.AT_IsActive = true;

				AccInvMsg message1 = newFactory.NewWithValidTestData<AccInvMsg>();
				message1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				newFactory.Save();

				BizO.AO_AT = ZGuid.Empty;
				BizO.AO_A9_DefaultVATClass = ZGuid.Empty;
				BizO.Validation.ValidateAO_A9_DefaultVATClass();
				AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);

				BizO.AO_AT = activeTaxRate.PK;
				BizO.Validation.ValidateAO_A9_DefaultVATClass();
				AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);

				BizO.AO_AT = ZGuid.Empty;
				BizO.AO_A9_DefaultVATClass = message1.PK;
				BizO.Validation.ValidateAO_A9_DefaultVATClass();
				AssertHasErrors(BizO.AO_A9_DefaultVATClassInfo);

				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_A9_DefaultVATClassInfo, true);
				BizO.AO_A9_DefaultVATClass = ZGuid.Empty;
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_A9_DefaultVATClassInfo, false);
				BizO.AO_A9_DefaultVATClass = message1.PK;
				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Disbursement);
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_A9_DefaultVATClassInfo, false);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		public void TestCheckAO_CustomsStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				BizO.AO_CustomsStatus = "";
				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
				AssertNoErrors(BizO.AO_CustomsStatusInfo);
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_CustomsStatusInfo, false);

				BizO.AO_CustomsStatus = "T1";
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_CustomsStatusInfo, true);

				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Disbursement);
				BizO.Validation.ValidateAO_CustomsStatus();
				AssertNoErrors(BizO.AO_CustomsStatusInfo);
				AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_CustomsStatusInfo, false);

				BizO.AO_CustomsStatus = "GGG";
				AssertHasErrors(BizO.AO_CustomsStatusInfo);

				BizO.AO_CustomsStatus = "X";
				AssertNoErrors(BizO.AO_CustomsStatusInfo);
			}
		}

		public void TestCheckAO_TransportMode()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();

			var expectedBrokerageTransportModes = new[]
				{
					"ALL", "AIR", "FIX", "IWT", "OWN", "MAI", "RAI", "ROA", "SEA"
				};

			foreach (JobInvoicingConsumerType jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				@override.AO_JobType = jobType.Code;
				if (@override.JobType.IsTransportModeSupported)
				{
					@override.AO_TransportMode = ZString.Empty;
					AssertHasErrors(@override.AO_TransportModeInfo);

					@override.AO_TransportMode = "ALL";
					AssertNoErrors(@override.AO_TransportModeInfo);

					@override.AO_TransportMode = "ZZZ";
					AssertHasErrors(@override.AO_TransportModeInfo);

					@override.AO_TransportMode = "SEA";
					AssertNoErrors(@override.AO_TransportModeInfo);

					if (jobType.Code == JobInvoicingConsumerTypes.Brokerage.Code)
					{
						foreach (var mode in expectedBrokerageTransportModes)
						{
							@override.AO_TransportMode = mode;
							AssertNoErrors($"JobType:BRK TransportMode: {mode}", @override.AO_TransportModeInfo);
						}
					}
				}
				else
				{
					@override.AO_TransportMode = ZString.Empty;
					AssertHasErrors(@override.AO_TransportModeInfo);
				}
			}
		}

		public void TestCheckAO_Direction()
		{
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			var taxOverride = charge.TaxOverrides.AddNew();

			foreach (JobInvoicingConsumerType jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				taxOverride.AO_JobType = jobType.Code;

				if (taxOverride.JobType.IsDirectionSupported)
				{
					taxOverride.AO_Direction = ZString.Empty;
					AssertHasErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertNoRowErrors(getAssertionMsg(), taxOverride);

					taxOverride.AO_Direction = "ALL";
					AssertNoErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertNoRowErrors(getAssertionMsg(), taxOverride);

					taxOverride.AO_Direction = "ZZZ";
					AssertHasErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertNoRowErrors(getAssertionMsg(), taxOverride);

					taxOverride.AO_Direction = "EXP";
					AssertNoErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertNoRowErrors(getAssertionMsg(), taxOverride);
				}
				else
				{
					taxOverride.AO_Direction = ZString.Empty;
					AssertHasErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertHasRowError(getAssertionMsg(), taxOverride, $"The '{jobType.Code}' Job Type does not support '' Direction. Please, delete an invalid tax override.");

					taxOverride.AO_Direction = "ALL";
					AssertNoErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertNoRowErrors(getAssertionMsg(), taxOverride);

					taxOverride.AO_Direction = "ZZZ";
					AssertHasErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertHasRowError(getAssertionMsg(), taxOverride, $"The '{jobType.Code}' Job Type does not support 'ZZZ' Direction. Please, delete an invalid tax override.");

					taxOverride.AO_Direction = "EXP";
					AssertNoErrors(getAssertionMsg(), taxOverride.AO_DirectionInfo);
					AssertHasRowError(getAssertionMsg(), taxOverride, $"The '{jobType.Code}' Job Type does not support 'EXP' Direction. Please, delete an invalid tax override.");
				}

				string getAssertionMsg() => $"Job Type: '{taxOverride.AO_JobType}' with DirectionSupport: '{taxOverride.JobType.IsDirectionSupported}' and Direction: '{taxOverride.AO_Direction}'.";
			}
		}

		public void TestCheckAO_OrganisationCategory()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();

			@override.AO_OrganisationCategory = "";
			AssertHasErrors(@override.AO_OrganisationCategoryInfo);
			@override.AO_OrganisationCategory = "GOV";
			AssertNoErrors(@override.AO_OrganisationCategoryInfo);
			@override.AO_OrganisationCategory = "XXX";
			AssertHasErrors(@override.AO_OrganisationCategoryInfo);
		}

		public void TestCheckAO_SplitPaymentVATOrganisation()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();

			@override.AO_CostSellAll = "REV";
			@override.AO_SplitPaymentVATOrganisation = true;
			AssertNoErrors(@override.AO_SplitPaymentVATOrganisationInfo);
			@override.AO_CostSellAll = "CST";
			@override.AO_SplitPaymentVATOrganisation = true;
			AssertHasError("expect error", @override.AO_SplitPaymentVATOrganisationInfo, AccChargeTaxOverrideValidation.ErrorMessageSplitPaymentVATOrganisationIsOnlyForSellCharge);
		}

		public virtual void TestCheckAO_ATWithDefaultingRuleIsSUM()
		{
			var newFactory = new BusinessObjectFactory();

			var nonZeroTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			nonZeroTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			nonZeroTaxRate.AT_Code = "NonZero";
			nonZeroTaxRate.AT_IsActive = true;
			nonZeroTaxRate.AT_ReferenceRateType = "STD";

			var zeroTaxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			zeroTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			zeroTaxRate.AT_Code = "Zero";
			zeroTaxRate.AT_IsActive = true;
			zeroTaxRate.AT_ReferenceRateType = "ZERO";
			zeroTaxRate.AT_ReferenceExtraRateType = "";

			newFactory.Save();

			if (IsChargeTypeApplicable)
			{
				AssertNotNull("Precondition", BizO.ChargeCode);
				AssertNotEquals("Precondition", Core.Constants.ChargeType.Comment, BizO.ChargeCode.AC_ChargeType);
			}
			else
			{
				AssertNotNull("Precondition", BizO.TaxOverrideGroup);
			}
			Assert("Precondition", !nonZeroTaxRate.IsZeroTaxRate);
			Assert("Precondition", zeroTaxRate.IsZeroTaxRate);
			Assert("Precondition", !BizO.AO_ATInfo.ReadOnly);

			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount;
			BizO.AO_AT = ZGuid.Empty;
			BizO.Validation.ValidateAO_AT();
			AssertHasError(BizO.AO_ATInfo, "Please enter a Tax ID.");

			BizO.AO_AT = nonZeroTaxRate.PK;
			BizO.Validation.ValidateAO_AT();
			AssertHasError(BizO.AO_ATInfo, "Tax ID's Tax Rate must equal 0 when Defaulting Rule is 'SUM'.");

			BizO.AO_AT = zeroTaxRate.PK;
			BizO.Validation.ValidateAO_AT();
			AssertNoErrors(BizO.AO_ATInfo);
		}

		public void TestCheckAO_AT_WhenInViewMode_NoErrorContains()
		{
			PrepareTaxInfo();
			BizO.AO_AT = taxRate1.PK;

			if (IsChargeTypeApplicable)
			{
				BizO.AO_CreateTaxRecord = true;
				SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
				Assert("When we are not in the view mode, it would show error about the comment type", !BizO.AO_ATInfo.ReadOnly);
				BizO.Validation.ValidateAO_AT();
				AssertHasError("There should be has error in AO_ATInfo", BizO.AO_ATInfo, "This data is invalid. There is no sense in it if the charge type is 'CMT'");
			}

			BizO.AO_CreateTaxRecord = false;
			Assert("We should be in the view mode", BizO.AO_ATInfo.ReadOnly);
			BizO.Validation.ValidateAO_AT();
			AssertNoErrors("There should be no error in AO_ATInfo", BizO.AO_ATInfo);
		}

		public void TestCheckAO_A9_DefaultVATClassWithDefaultingRuleIsART()
		{
			var message = Factory.NewWithValidTestData<AccInvMsg>();

			if (IsChargeTypeApplicable)
			{
				AssertNotNull("Precondition", BizO.ChargeCode);
				AssertNotEquals("Precondition", Core.Constants.ChargeType.Comment, BizO.ChargeCode.AC_ChargeType);
			}
			else
			{
				AssertNotNull("Precondition", BizO.TaxOverrideGroup);
			}

			AssertNotEquals("Precondition", Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount, BizO.AO_DefaultingRule);
			AssertEquals("Precondition", ZGuid.Empty, BizO.AO_AT);

			BizO.AO_A9_DefaultVATClass = ZGuid.Empty;
			BizO.Validation.ValidateAO_A9_DefaultVATClass();
			AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);

			BizO.AO_A9_DefaultVATClass = message.PK;
			BizO.Validation.ValidateAO_A9_DefaultVATClass();
			AssertHasError(BizO.AO_A9_DefaultVATClassInfo, "You must have a Tax ID before you can choose a Tax Message");

			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount;
			BizO.Validation.ValidateAO_A9_DefaultVATClass();
			AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);
		}

		public void TestCheckAO_DefaultingRule()
		{
			if (IsChargeTypeApplicable)
			{
				AssertNotNull("Precondition", BizO.ChargeCode);
				AssertNotEquals("Precondition", Core.Constants.ChargeType.Comment, BizO.ChargeCode.AC_ChargeType);
			}

			AssertNotEquals("Precondition", Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport, BizO.AO_TransactionContext);

			BizO.AO_DefaultingRule = ZString.Empty;
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertHasError(BizO.AO_DefaultingRuleInfo, "Please enter a Defaulting Rule.");

			BizO.AO_DefaultingRule = "XXX";
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertHasError(BizO.AO_DefaultingRuleInfo, "Enter a valid Defaulting Rule.");

			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_DefaultingRuleInfo, true);

			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Margin);
			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable;
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_DefaultingRuleInfo, false);

			BizO.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable;
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertHasError(BizO.AO_DefaultingRuleInfo, "Defaulting Rule must be 'ART' or 'SUM' when Transaction Context is 'INT'.");

			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount;
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertNoErrors(BizO.AO_DefaultingRuleInfo);

			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount;
			BizO.Validation.ValidateAO_DefaultingRule();
			AssertNoErrors(BizO.AO_DefaultingRuleInfo);
		}

		public void TestCheckAO_TransactionContext()
		{
			if (IsChargeTypeApplicable)
			{
				AssertNotNull("Precondition", BizO.ChargeCode);
				AssertNotEquals("Precondition", Core.Constants.ChargeType.Comment, BizO.ChargeCode.AC_ChargeType);
			}

			AssertNotEquals("Precondition", Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport, BizO.AO_TransactionContext);

			BizO.AO_TransactionContext = ZString.Empty;
			BizO.Validation.ValidateAO_TransactionContext();
			AssertHasError(BizO.AO_TransactionContextInfo, "Please enter a Transaction Context.");

			BizO.AO_TransactionContext = "XXX";
			BizO.Validation.ValidateAO_TransactionContext();
			AssertHasError(BizO.AO_TransactionContextInfo, "Enter a valid Transaction Context.");

			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
			BizO.Validation.ValidateAO_TransactionContext();
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_TransactionContextInfo, true);

			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Margin);
			BizO.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			BizO.Validation.ValidateAO_TransactionContext();
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_TransactionContextInfo, false);
			var expectedINTRegistryError = "'INT' option can only be selected when registry 'Accounting -> Payable Defaults -> Default Settings -> Use Import Company Charge Code's Tax Overrides for Intercompany Invoice Import' is set to 'Yes'.";
			if (IsTaxConfigured)
			{
				AssertHasError(BizO.AO_TransactionContextInfo, "'INT' option cannot be selected for Tax Configuration Override Group");
				AssertCollectionNotContains(expectedINTRegistryError, BizO.AO_TransactionContextInfo.Notifications.Select(x => x.Message));
			}
			else
			{
				AssertHasError(BizO.AO_TransactionContextInfo, expectedINTRegistryError);
				AssertCollectionContains(expectedINTRegistryError, BizO.AO_TransactionContextInfo.Notifications.Select(x => x.Message));
			}

			using (AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				BizO.Validation.ValidateAO_TransactionContext();
				if (IsTaxConfigured)
				{
					AssertHasError(BizO.AO_TransactionContextInfo, "'INT' option cannot be selected for Tax Configuration Override Group");
				}
				else
				{
					AssertNoErrors(BizO.AO_TransactionContextInfo);
				}
			}

			BizO.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.Standard;
			BizO.Validation.ValidateAO_TransactionContext();
			AssertNoErrors(BizO.AO_TransactionContextInfo);

			BizO.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.All;
			BizO.Validation.ValidateAO_TransactionContext();
			AssertNoErrors(BizO.AO_TransactionContextInfo);
		}

		public void TestCheckAO_HomeCountryOrZone()
		{
			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable;
			BizO.AO_HomeCountryOrZone = "AAA";
			BizO.Validation.ValidateAO_HomeCountryOrZone();
			AssertHasError(BizO.AO_HomeCountryOrZoneInfo, "Enter a valid FPOS/AR/AP Location.");

			foreach (var homeCountryOrZone in BizO.Lookups.Locations.GetAllCodes())
			{
				BizO.AO_HomeCountryOrZone = homeCountryOrZone;
				AssertNoErrors("AO_HomeCountryOrZone should not contain errors", BizO.AO_HomeCountryOrZoneInfo);
			}

			BizO.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount;
			BizO.AO_HomeCountryOrZone = "US";
			AssertHasError(BizO.AO_HomeCountryOrZoneInfo, "'FPOS/AR/AP Location' must be in the same country/region as the current login company when Defaulting Rule is 'ART'.");

			BizO.AO_HomeCountryOrZone = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			BizO.Validation.ValidateAO_HomeCountryOrZone();
			AssertNoErrors(BizO.AO_HomeCountryOrZoneInfo);
		}

		public void TestAO_SupplyType()
		{
			BizO.AO_SupplyType = ZString.Empty;
			BizO.Validation.ValidateAO_SupplyType();
			AssertNoErrors(BizO.AO_SupplyTypeInfo);

			CombineAssertions("When value is in lookup like, it should not be error", () =>
			{
				foreach (var supplyType in BizO.Lookups.SupplyTypes.GetAllCodes())
				{
					BizO.AO_SupplyType = supplyType;
					AssertNoErrors(BizO.AO_SupplyTypeInfo);
				}
			});

			AssertEquals("PreCondtion, AAA is invalid.", false, BizO.Lookups.SupplyTypes.GetAllCodes().Contains("AAA"));
			BizO.AO_SupplyType = "AAA";
			AssertHasError(BizO.AO_SupplyTypeInfo, "Enter a valid Supply Type.");
		}

		public void TestAO_DebtorRole()
		{
			BizO.AO_DebtorRole = ZString.Empty;
			BizO.Validation.ValidateAO_DebtorRole();
			AssertNoErrors(BizO.AO_DebtorRoleInfo);

			CombineAssertions("When value is in lookup like, it should not be error", () =>
			{
				foreach (var debtorRole in BizO.Lookups.DebtorRoleList.GetAllCodes())
				{
					BizO.AO_DebtorRole = debtorRole;
					if (Factory.HasContext(AccTaxOverrideGroup.BusinessContext.TaxFramework))
					{
						AssertHasError("except in tax framework", BizO.AO_DebtorRoleInfo, "The Debtor Role must be empty on Tax Configuration Override Groups. Please delete and recreate this record.");
					}
					else
					{
						AssertNoErrors(BizO.AO_DebtorRoleInfo);
					}
				}
			});

			AssertEquals("PreCondtion, AAA is invalid.", false, BizO.Lookups.DebtorRoleList.GetAllCodes().Contains("AAA"));
			BizO.AO_DebtorRole = "AAA";
			if (Factory.HasContext(AccTaxOverrideGroup.BusinessContext.TaxFramework))
			{
				AssertHasError(BizO.AO_DebtorRoleInfo, "The Debtor Role must be empty on Tax Configuration Override Groups. Please delete and recreate this record.");
			}
			else
			{
				AssertHasError(BizO.AO_DebtorRoleInfo, "Enter a valid Debtor Role.");
			}
		}

		#region Test for Tax Id And Tax Message Mapping Validation

		public void TestTaxIDAndTaxMessageMappingValidation_ForTypeCost()
		{
			PrepareTaxInfo();

			BizO.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			BizO.AO_AT = taxRate1.PK;
			var helper = ObjectFactory.Get<ITaxIdAndTaxMessageMappingHelper>();
			var lineType = new[] { TransactionLineTypes.Cost };
			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate1, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_AT = taxRate1.PK;
			taxOverride.AO_A9_DefaultVATClass = taxMsg2.PK;
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), helper.ValidateMappingForTaxOverride(lineType, taxOverride));

			BizO.AO_A9_DefaultVATClass = taxMsg2.PK;
			AssertEquals(!IsTaxConfigured, BizO.AO_A9_DefaultVATClassInfo.HasError(GetErrorMsg("CST", "TaxRate01", "TaxMsg02")));

			BizO.AO_A9_DefaultVATClass = taxMsg1.PK;
			AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);
		}

		public void TestTaxIDAndTaxMessageMappingValidation_ForTypeRevenue()
		{
			PrepareTaxInfo();

			BizO.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			BizO.AO_AT = taxRate1.PK;
			var helper = ObjectFactory.Get<ITaxIdAndTaxMessageMappingHelper>();
			var lineType = new[] { TransactionLineTypes.Revenue };
			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, taxRate1, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_AT = taxRate1.PK;
			taxOverride.AO_A9_DefaultVATClass = taxMsg2.PK;
			AssertEquals(GetErrorMsg("REV", "TaxRate01", "TaxMsg02"), helper.ValidateMappingForTaxOverride(lineType, taxOverride));

			BizO.AO_A9_DefaultVATClass = taxMsg2.PK;
			AssertEquals(!IsTaxConfigured, BizO.AO_A9_DefaultVATClassInfo.HasError(GetErrorMsg("REV", "TaxRate01", "TaxMsg02")));

			BizO.AO_A9_DefaultVATClass = taxMsg1.PK;
			AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);
		}

		public void TestTaxIDAndTaxMessageMappingValidation_ForTypeALL()
		{
			PrepareTaxInfo();

			BizO.AO_CostSellAll = AccChargeTaxOverrideLookups.ALL;
			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Cost, taxRate1, taxMsg1),
				(TransactionLineTypes.Revenue, taxRate1, taxMsg1),
				(TransactionLineTypes.Cost, taxRate2, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			BizO.AO_AT = taxRate1.PK;
			BizO.AO_A9_DefaultVATClass = taxMsg2.PK;
			AssertEquals(!IsTaxConfigured, BizO.AO_A9_DefaultVATClassInfo.HasError(GetErrorMsgForTypeALL("CST", "REV", "TaxRate01", "TaxMsg02")));

			BizO.AO_A9_DefaultVATClass = taxMsg1.PK;
			AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);

			BizO.AO_AT = taxRate2.PK;
			BizO.AO_A9_DefaultVATClass = taxMsg1.PK;
			AssertNoErrors(BizO.AO_A9_DefaultVATClassInfo);
		}

		public void TestTaxIDAndTaxMessageMappingValidation_WhenHasError()
		{
			PrepareTaxInfo();

			BizO.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			BizO.AO_AT = taxRate1.PK;
			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, taxRate1, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			SetParentChargeToSpecificChargeType(Core.Constants.ChargeType.Comment);
			BizO.AO_A9_DefaultVATClass = taxMsg2.PK;
			AssertHasInvalidDataForCMTChargeTypeError(BizO.AO_A9_DefaultVATClassInfo, true);
			AssertEquals(!IsChargeTypeApplicable && !IsTaxConfigured, BizO.AO_A9_DefaultVATClassInfo.HasError(GetErrorMsg("REV", "TaxRate01", "TaxMsg02")));
		}

		void PrepareTaxInfo()
		{
			taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TaxRate01";
			taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TaxRate02";
			taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			taxMsg3 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg3.A9_Code = "TaxMsg03";
			Factory.Save();
		}

		#endregion

		void SetValuesAndAssertHasErrors(AccChargeTaxOverride bizO, ZString direction, ZString origin, ZString destination, ZPropertyInfo infoToCheck)
		{
			SetValues(bizO, direction, origin, destination);
			AssertHasErrors(infoToCheck);
		}

		void SetValuesAndAssertNoErrors(AccChargeTaxOverride bizO, ZString direction, ZString origin, ZString destination, ZPropertyInfo infoToCheck)
		{
			SetValues(bizO, direction, origin, destination);
			AssertNoErrors(infoToCheck);
		}

		void SetValues(AccChargeTaxOverride bizO, ZString direction, ZString origin, ZString destination)
		{
			bizO.AO_Direction = direction;
			bizO.Validation.ValidateAO_Direction();

			bizO.AO_Origin = origin;
			bizO.Validation.ValidateAO_Origin();

			bizO.AO_Destination = destination;
			bizO.Validation.ValidateAO_Destination();
		}

		protected void SetParentChargeToSpecificChargeType(ZString chargeType)
		{
			if (IsChargeTypeApplicable)
			{
				BizO.ChargeCode.AC_ChargeType = chargeType;
			}
		}

		void AssertHasInvalidDataForCMTChargeTypeError(ZPropertyInfo infoToCheck, bool hasError)
		{
			BizO.Validation.ValidateAll();
			AssertEquals(IsChargeTypeApplicable && hasError, infoToCheck.HasError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment));
		}

		protected abstract bool IsChargeTypeApplicable { get; }

		protected virtual bool IsTaxConfigured => false;

		protected AccChargeTaxOverride BizO => bizO ?? (bizO = GetNewChargeTaxOverride());
		AccChargeTaxOverride bizO;
		AccTaxRate taxRate1, taxRate2;
		AccInvMsg taxMsg1, taxMsg2, taxMsg3;
		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		protected abstract AccChargeTaxOverride GetNewChargeTaxOverride();

		string EXP
		{
			get { return Core.Constants.Sales.Mode.Export; }
		}

		string IMP
		{
			get { return Core.Constants.Sales.Mode.Import; }
		}

		string GetErrorMsg(string lineType, string taxId, string taxMsg) => $@"Tax Override cannot be saved due to invalid Tax ID and Tax Message combination.
Line Type={lineType}, Tax ID={taxId}, Tax Message={taxMsg}";

		string GetErrorMsgForTypeALL(string lineType1, string lineType2, string taxId, string taxMsg) => $@"Tax Override cannot be saved due to invalid Tax ID and Tax Message combination.
Line Type={lineType1}, Tax ID={taxId}, Tax Message={taxMsg}
Line Type={lineType2}, Tax ID={taxId}, Tax Message={taxMsg}";
	}
}
