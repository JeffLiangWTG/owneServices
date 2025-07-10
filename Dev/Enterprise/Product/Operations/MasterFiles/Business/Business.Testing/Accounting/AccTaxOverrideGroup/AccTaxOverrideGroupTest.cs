using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccTaxOverrideGroup;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroup))]
	sealed class AccTaxOverrideGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestDefaultValues()
		{
			AssertEquals("Current cuntry must be set as default.", Env.CurrentCompany.Country.Code, TestTaxOverrideGroup.AX_RN_NKCountry);
		}

		public void TestGroupType()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = taxConfiguration1.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_Code = "TAXCONFIG1";
			taxConfiguration1.ETC_Code = "TAXCONFIG2";

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("GST", taxOverrideGroup.AX_GroupType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				AssertEquals("IVA", taxOverrideGroup.AX_GroupType);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				AssertEquals("VAT", taxOverrideGroup.AX_GroupType);
			}

			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertEquals("TAXCONFIG1", taxOverrideGroup.AX_GroupType);

			var taxOverrideGroupTaxConfigurationPivot1 = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot1.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			AssertEquals("TAXCONFIG1, TAXCONFIG2", taxOverrideGroup.AX_GroupType);
		}

		public void TestTemplateCopy()
		{
			AccChargeTaxOverride taxOverride = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride.AO_JobType = "SHP";
			taxOverride.AO_Direction = "IMP";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_Origin = "ZA";
			taxOverride.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_TaxRegCntryOrGroup = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_AT = ZGuid.NewZGuid();
			AssertEquals("Precondition: ", TestTaxOverrideGroup.PK, taxOverride.AO_ParentID);
			AssertEquals("Precondition: ", TestTaxOverrideGroup.TablePrefix, taxOverride.AO_ParentTableCode);

			AccChargeTaxOverride taxOverride2 = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride2.AO_JobType = "SHP";
			taxOverride2.AO_Direction = "DOM";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride2.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride2.AO_TaxRegCntryOrGroup = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride2.AO_AT = ZGuid.NewZGuid();
			AssertEquals("Precondition: ", TestTaxOverrideGroup.PK, taxOverride2.AO_ParentID);
			AssertEquals("Precondition: ", TestTaxOverrideGroup.TablePrefix, taxOverride2.AO_ParentTableCode);

			AccTaxOverrideGroup newTaxOverrideGroup = (AccTaxOverrideGroup)((ITemplateCopyable)TestTaxOverrideGroup).TemplateCopy();
			AssertEquals(newTaxOverrideGroup.AX_Code, TestTaxOverrideGroup.AX_Code);
			AssertEquals(newTaxOverrideGroup.AX_Description, TestTaxOverrideGroup.AX_Description);
			AssertEquals(newTaxOverrideGroup.AX_RN_NKCountry, TestTaxOverrideGroup.AX_RN_NKCountry);
			AssertEquals("TaxOverrides.Count", TestTaxOverrideGroup.TaxOverrides.Count, newTaxOverrideGroup.TaxOverrides.Count);
			AssertNotEquals("ChargeCode PKs different", TestTaxOverrideGroup.PK, newTaxOverrideGroup.PK);

			AccChargeTaxOverride copiedTaxOverride = newTaxOverrideGroup.TaxOverrides[0];
			AssertEquals(taxOverride.AO_JobType, copiedTaxOverride.AO_JobType);
			AssertEquals(taxOverride.AO_Direction, copiedTaxOverride.AO_Direction);
			AssertEquals(taxOverride.AO_IncoTerm, copiedTaxOverride.AO_IncoTerm);
			AssertEquals(taxOverride.AO_Origin, copiedTaxOverride.AO_Origin);
			AssertEquals(taxOverride.AO_Destination, copiedTaxOverride.AO_Destination);
			AssertEquals(taxOverride.AO_TaxRegCntryOrGroup, copiedTaxOverride.AO_TaxRegCntryOrGroup);
			AssertEquals(taxOverride.AO_AT, copiedTaxOverride.AO_AT);
			AssertEquals(taxOverride.AO_ParentTableCode, copiedTaxOverride.AO_ParentTableCode);
			AssertEquals(newTaxOverrideGroup.PK, copiedTaxOverride.AO_ParentID);
			AssertNotEquals("TaxOverride PKs different", taxOverride.PK, copiedTaxOverride.PK);

			AccChargeTaxOverride copiedTaxOverride2 = newTaxOverrideGroup.TaxOverrides[1];
			AssertEquals(taxOverride2.AO_JobType, copiedTaxOverride2.AO_JobType);
			AssertEquals(taxOverride2.AO_Direction, copiedTaxOverride2.AO_Direction);
			AssertEquals(taxOverride2.AO_IncoTerm, copiedTaxOverride2.AO_IncoTerm);
			AssertEquals(taxOverride2.AO_Origin, copiedTaxOverride2.AO_Origin);
			AssertEquals(taxOverride2.AO_Destination, copiedTaxOverride2.AO_Destination);
			AssertEquals(taxOverride2.AO_TaxRegCntryOrGroup, copiedTaxOverride2.AO_TaxRegCntryOrGroup);
			AssertEquals(taxOverride2.AO_AT, copiedTaxOverride2.AO_AT);
			AssertEquals(taxOverride2.AO_ParentTableCode, copiedTaxOverride2.AO_ParentTableCode);
			AssertEquals(newTaxOverrideGroup.PK, copiedTaxOverride2.AO_ParentID);
			AssertNotEquals("TaxOverride PKs different", taxOverride2.PK, copiedTaxOverride2.PK);
		}

		public void TestTemplateCopyDoesNotUseDefaultingRules()
		{
			AccChargeTaxOverride taxOverride = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "EUN";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = ZGuid.NewZGuid();
			AssertEquals("Precondition: ", TestTaxOverrideGroup.PK, taxOverride.AO_ParentID);
			AssertEquals("Precondition: ", TestTaxOverrideGroup.TablePrefix, taxOverride.AO_ParentTableCode);

			AccChargeTaxOverride taxOverride2 = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride2.AO_JobType = "ALL";
			taxOverride2.AO_Direction = "EXP";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_Origin = "AU";
			taxOverride2.AO_Destination = "ALX";
			taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride2.AO_AT = ZGuid.NewZGuid();
			AssertEquals("Precondition: ", TestTaxOverrideGroup.PK, taxOverride2.AO_ParentID);
			AssertEquals("Precondition: ", TestTaxOverrideGroup.TablePrefix, taxOverride2.AO_ParentTableCode);

			AccTaxOverrideGroup newTaxOverrideGroup = (AccTaxOverrideGroup)((ITemplateCopyable)TestTaxOverrideGroup).TemplateCopy();
			AssertEquals(newTaxOverrideGroup.AX_Code, TestTaxOverrideGroup.AX_Code);
			AssertEquals(newTaxOverrideGroup.AX_Description, TestTaxOverrideGroup.AX_Description);
			AssertEquals(newTaxOverrideGroup.AX_RN_NKCountry, TestTaxOverrideGroup.AX_RN_NKCountry);
			AssertEquals("TaxOverrides.Count", TestTaxOverrideGroup.TaxOverrides.Count, newTaxOverrideGroup.TaxOverrides.Count);
			AssertNotEquals("ChargeCode PKs different", TestTaxOverrideGroup.PK, newTaxOverrideGroup.PK);

			AccChargeTaxOverride copiedTaxOverride = newTaxOverrideGroup.TaxOverrides[0];
			AssertEquals(taxOverride.AO_JobType, copiedTaxOverride.AO_JobType);
			AssertEquals(taxOverride.AO_Direction, copiedTaxOverride.AO_Direction);
			AssertEquals(taxOverride.AO_IncoTerm, copiedTaxOverride.AO_IncoTerm);
			AssertEquals(taxOverride.AO_Origin, copiedTaxOverride.AO_Origin);
			AssertEquals(taxOverride.AO_Destination, copiedTaxOverride.AO_Destination);
			AssertEquals(taxOverride.AO_TaxRegCntryOrGroup, copiedTaxOverride.AO_TaxRegCntryOrGroup);
			AssertEquals(taxOverride.AO_AT, copiedTaxOverride.AO_AT);
			AssertEquals(taxOverride.AO_ParentTableCode, copiedTaxOverride.AO_ParentTableCode);
			AssertEquals(newTaxOverrideGroup.PK, copiedTaxOverride.AO_ParentID);
			AssertNotEquals("TaxOverride PKs different", taxOverride.PK, copiedTaxOverride.PK);

			AccChargeTaxOverride copiedTaxOverride2 = newTaxOverrideGroup.TaxOverrides[1];
			AssertEquals(taxOverride2.AO_JobType, copiedTaxOverride2.AO_JobType);
			AssertEquals(taxOverride2.AO_Direction, copiedTaxOverride2.AO_Direction);
			AssertEquals(taxOverride2.AO_IncoTerm, copiedTaxOverride2.AO_IncoTerm);
			AssertEquals(taxOverride2.AO_Origin, copiedTaxOverride2.AO_Origin);
			AssertEquals(taxOverride2.AO_Destination, copiedTaxOverride2.AO_Destination);
			AssertEquals(taxOverride2.AO_TaxRegCntryOrGroup, copiedTaxOverride2.AO_TaxRegCntryOrGroup);
			AssertEquals(taxOverride2.AO_AT, copiedTaxOverride2.AO_AT);
			AssertEquals(taxOverride2.AO_ParentTableCode, copiedTaxOverride2.AO_ParentTableCode);
			AssertEquals(newTaxOverrideGroup.PK, copiedTaxOverride2.AO_ParentID);
			AssertNotEquals("TaxOverride PKs different", taxOverride2.PK, copiedTaxOverride2.PK);
		}

		public void TestIDocManagerSupport()
		{
			IDocManagerSupport docManagerSupport = TestTaxOverrideGroup;
			AssertNotNull("IDocManagerSupport must be implemented", docManagerSupport);
			AssertEquals("DocManagerCode must be TaxOverrideGroup code.", Core.Constants.DocManagerCodes.TaxOverrideGroup, docManagerSupport.DocManagerInfo.DocManagerCode);
		}

		public void TestCanDelete()
		{
			AccChargeCode chargeCode_inDB = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode_inDB.AC_Code = "AAA";
			chargeCode_inDB.AC_AX_TaxOverrideGroup = TestTaxOverrideGroup.PK;
			Factory.Save();
			AccChargeCode chargeCode_notInDB = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode_notInDB.AC_Code = "BBB";

			AssertEquals("CanDelete", false, TestTaxOverrideGroup.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "The Tax Override Group cannot be deleted because it is used in charge codes: 'AAA'.", TestTaxOverrideGroup.ReasonForNotAbleToDelete);

			chargeCode_notInDB.AC_AX_TaxOverrideGroup = TestTaxOverrideGroup.PK;
			AssertEquals("CanDelete", false, TestTaxOverrideGroup.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "The Tax Override Group cannot be deleted because it is used in charge codes: 'AAA', 'BBB'.", TestTaxOverrideGroup.ReasonForNotAbleToDelete);

			chargeCode_inDB.AC_AX_TaxOverrideGroup = ZGuid.Empty;
			chargeCode_notInDB.AC_AX_TaxOverrideGroup = ZGuid.Empty;
			AssertEquals("CanDelete", true, TestTaxOverrideGroup.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", TestTaxOverrideGroup.ReasonForNotAbleToDelete);

			var chargeCode_inDB2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode_inDB2.AC_Code = "CCC";
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			var taxOverrideGroupChargeCodePivot = Factory.New<AccTaxOverrideGroupChargeCodePivot>();
			taxOverrideGroupChargeCodePivot.ACP_AC_ChargeCode = chargeCode_inDB2.PK;
			taxOverrideGroupChargeCodePivot.ACP_AX_TaxOverrideGroup = taxOverrideGroup.PK;

			Factory.Save();

			AssertEquals("CanDelete", false, taxOverrideGroup.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "The Tax Override Group cannot be deleted because it is used in charge codes: 'CCC'.", taxOverrideGroup.ReasonForNotAbleToDelete);
		}

		public void TestDelete()
		{
			AccChargeTaxOverride taxOverride1 = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride1.FillWithValidTestData();
			AccChargeTaxOverride taxOverride2 = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride2.FillWithValidTestData();
			Factory.Save();

			AssertEquals("Precondition: ", TestTaxOverrideGroup.PK, taxOverride1.AO_ParentID);
			AssertEquals("Precondition: ", TestTaxOverrideGroup.PK, taxOverride2.AO_ParentID);

			TestTaxOverrideGroup.Delete();

			AssertEquals(0, TestTaxOverrideGroup.TaxOverrides.Count);

			Factory.Save();
			Assert(taxOverride1.IsDeleted);
			Assert(taxOverride2.IsDeleted);
			Assert(TestTaxOverrideGroup.IsDeleted);
		}

		public void TestDeleteDuplicateTaxOverridesInChargeCodes()
		{
			AccChargeTaxOverride taxOverrideForGroup = TestTaxOverrideGroup.TaxOverrides.AddNew();
			taxOverrideForGroup.AO_Direction = "ALL";
			taxOverrideForGroup.AO_IncoTerm = "ALL";
			taxOverrideForGroup.AO_JobType = "ALL";
			taxOverrideForGroup.AO_Origin = "ALL";
			taxOverrideForGroup.AO_Destination = "ALL";
			taxOverrideForGroup.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForGroup.AO_CustomsStatus = "ALL";

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_AX_TaxOverrideGroup = TestTaxOverrideGroup.PK;
			AccChargeTaxOverride taxOverrideForChargeCode1_1 = chargeCode1.TaxOverrides.AddNew();
			taxOverrideForChargeCode1_1.AO_Direction = "ALL";
			taxOverrideForChargeCode1_1.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode1_1.AO_JobType = "ALL";
			taxOverrideForChargeCode1_1.AO_Origin = "ALL";
			taxOverrideForChargeCode1_1.AO_Destination = "ALL";
			taxOverrideForChargeCode1_1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode1_1.AO_CustomsStatus = "ALL";
			AccChargeTaxOverride taxOverrideForChargeCode1_2 = chargeCode1.TaxOverrides.AddNew();
			taxOverrideForChargeCode1_2.AO_Direction = "ALL";
			taxOverrideForChargeCode1_2.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode1_2.AO_JobType = "CST";
			taxOverrideForChargeCode1_2.AO_Origin = "ALL";
			taxOverrideForChargeCode1_2.AO_Destination = "ALL";
			taxOverrideForChargeCode1_2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode1_2.AO_CustomsStatus = "ALL";

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_AX_TaxOverrideGroup = TestTaxOverrideGroup.PK;
			AccChargeTaxOverride taxOverrideForChargeCode2_1 = chargeCode2.TaxOverrides.AddNew();
			taxOverrideForChargeCode2_1.AO_Direction = "IMP";
			taxOverrideForChargeCode2_1.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode2_1.AO_JobType = "ALL";
			taxOverrideForChargeCode2_1.AO_Origin = "ALL";
			taxOverrideForChargeCode2_1.AO_Destination = "ALL";
			taxOverrideForChargeCode2_1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode2_1.AO_CustomsStatus = "ALL";
			AccChargeTaxOverride taxOverrideForChargeCode2_2 = chargeCode2.TaxOverrides.AddNew();
			taxOverrideForChargeCode2_2.AO_Direction = "ALL";
			taxOverrideForChargeCode2_2.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode2_2.AO_JobType = "ALL";
			taxOverrideForChargeCode2_2.AO_Origin = "ALL";
			taxOverrideForChargeCode2_2.AO_Destination = "ALL";
			taxOverrideForChargeCode2_2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode2_2.AO_CustomsStatus = "ALL";

			AccChargeCode chargeCode3 = Factory.New<AccChargeCode>();
			chargeCode3.AC_AX_TaxOverrideGroup = TestTaxOverrideGroup.PK;
			AccChargeTaxOverride taxOverrideForChargeCode3_1 = chargeCode3.TaxOverrides.AddNew();
			taxOverrideForChargeCode3_1.AO_Direction = "IMP";
			taxOverrideForChargeCode3_1.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode3_1.AO_JobType = "ALL";
			taxOverrideForChargeCode3_1.AO_Origin = "ALL";
			taxOverrideForChargeCode3_1.AO_Destination = "ALL";
			taxOverrideForChargeCode3_1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode3_1.AO_CustomsStatus = "ALL";
			AccChargeTaxOverride taxOverrideForChargeCode3_2 = chargeCode3.TaxOverrides.AddNew();
			taxOverrideForChargeCode3_2.AO_Direction = "ALL";
			taxOverrideForChargeCode3_2.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode3_2.AO_JobType = "CST";
			taxOverrideForChargeCode3_2.AO_Origin = "ALL";
			taxOverrideForChargeCode3_2.AO_Destination = "ALL";
			taxOverrideForChargeCode3_2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode3_2.AO_CustomsStatus = "ALL";

			AssertEquals("Precondition: TestTaxOverrideGroup.TaxOverrides.Count", 1, TestTaxOverrideGroup.TaxOverrides.Count);
			AssertEquals("Precondition: chargeCode1.TaxOverrides.Count", 2, chargeCode1.TaxOverrides.Count);
			AssertEquals("Precondition: chargeCode2.TaxOverrides.Count", 2, chargeCode2.TaxOverrides.Count);
			AssertEquals("Precondition: chargeCode3.TaxOverrides.Count", 2, chargeCode3.TaxOverrides.Count);

			TestTaxOverrideGroup.DeleteDuplicateTaxOverridesInChargeCodes();
			AssertEquals("TestTaxOverrideGroup.TaxOverrides.Count", 1, TestTaxOverrideGroup.TaxOverrides.Count);
			AssertEquals("chargeCode1.TaxOverrides.Count", 1, chargeCode1.TaxOverrides.Count);
			AssertCollectionContains("chargeCode1.TaxOverrides must contain not duplicated tax override.", taxOverrideForChargeCode1_2, chargeCode1.TaxOverrides);
			AssertEquals("chargeCode2.TaxOverrides.Count", 1, chargeCode2.TaxOverrides.Count);
			AssertCollectionContains("chargeCode2.TaxOverrides must contain not duplicated tax override.", taxOverrideForChargeCode2_1, chargeCode2.TaxOverrides);
			AssertEquals("chargeCode3.TaxOverrides.Count", 2, chargeCode3.TaxOverrides.Count);
		}

		public void TestHumanReadableNameCore()
		{
			var newTaxOverrideGroup = (AccTaxOverrideGroup)((ITemplateCopyable)TestTaxOverrideGroup).TemplateCopy();
			newTaxOverrideGroup.AX_Code = "TAX";
			newTaxOverrideGroup.AX_Description = "No Tax";

			AssertEquals("Tax Override Groups - TAX - No Tax", newTaxOverrideGroup.HumanReadableName);
		}

		public void TestValidation()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AssertEquals("Precondition: ", 0, taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.Count);
			Assert("Precondition:", !taxOverrideGroup.IsTaxFrameworkRelated);
			AssertType(typeof(AccTaxOverrideGroupValidation), taxOverrideGroup.Validation);

			taxOverrideGroup.SetContext(BusinessContext.TaxFramework);
			Assert("Precondition:", taxOverrideGroup.IsTaxFrameworkRelated);
			AssertType(typeof(TaxFrameworkAccTaxOverrideGroupValidation), taxOverrideGroup.Validation);

			taxOverrideGroup.RemoveContext(BusinessContext.TaxFramework);
			Assert("Precondition:", !taxOverrideGroup.IsTaxFrameworkRelated);
			AssertType(typeof(AccTaxOverrideGroupValidation), taxOverrideGroup.Validation);

			taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			Assert("Precondition: ", taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.Count > 0);
			AssertType(typeof(TaxFrameworkAccTaxOverrideGroupValidation), taxOverrideGroup.Validation);
		}

		public void TestChargeCodesLinkedToTaxOverrideGroup()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			Assert("IsRegisteredEditableChildObject", taxOverrideGroup.IsRegisteredEditableChildObject(taxOverrideGroup.ChargeCodes));
			AssertEquals("Need not require to load the collection when we run validation as we skip validation", false, ChildEditableAttribute.GetValue(TypeDescriptor.GetProperties(typeof(AccTaxOverrideGroup))["ChargeCodes"]));
		}

		public void TestChargeCodesLinkedToTaxFrameworkConfiguration()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			Assert("IsRegisteredEditableChildObject", taxOverrideGroup.IsRegisteredEditableChildObject(taxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration));
			AssertEquals("Need not require to load the collection when we run validation as we skip validation", false, ChildEditableAttribute.GetValue(TypeDescriptor.GetProperties(typeof(AccTaxOverrideGroup))["ChargeCodesLinkedToTaxFrameworkConfiguration"]));
		}

		public void TestTaxOverrideGroupTaxConfigurationPivots()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			Assert("IsRegisteredEditableChildObject", taxOverrideGroup.IsRegisteredEditableChildObject(taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots));
		}

		public void TestIsTaxFrameworkRelated_DoesNotLoadPivotsUnnesesarily()
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var accountingTestObjectCreatorInNewFactory = new AccountingTestObjectCreator(newFactory);
			var taxConfigurationInNewFactory = accountingTestObjectCreatorInNewFactory.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
			var taxOverrideGroupInNewFactory = accountingTestObjectCreatorInNewFactory.CreateTaxOverrideGroup(GlbCompany.CurrentCompany, null);
			Assert(!taxOverrideGroupInNewFactory.IsTaxFrameworkRelated);
			accountingTestObjectCreatorInNewFactory.CreateTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroupInNewFactory, taxConfigurationInNewFactory);
			Assert(taxOverrideGroupInNewFactory.IsTaxFrameworkRelated);
			newFactory.Save();

			var taxOverrideGroup = Factory.Load<AccTaxOverrideGroup>(taxOverrideGroupInNewFactory.PK);
			var loadFromLocalCacheQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			AssertEquals(0, Factory.Load<AccTaxOverrideGroupTaxConfigurationPivot>(loadFromLocalCacheQuery).Length);
			Assert(taxOverrideGroup.IsTaxFrameworkRelated);
			AssertEquals(0, Factory.Load<AccTaxOverrideGroupTaxConfigurationPivot>(loadFromLocalCacheQuery).Length);
		}

		public void TestIsTaxFrameworkRelated_ChecksLocalCache()
		{
			var taxOverrideGroup = AccountingTestObjectCreator.CreateTaxOverrideGroup(GlbCompany.CurrentCompany, null);
			Assert(!taxOverrideGroup.IsTaxFrameworkRelated);

			var taxOverrideGroupTaxConfigurationPivot = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			Assert(taxOverrideGroup.IsTaxFrameworkRelated);
		}

		public void TestIsTaxFrameworkRelated_BusinessContext()
		{
			var taxOverrideGroup = AccountingTestObjectCreator.CreateTaxOverrideGroup(GlbCompany.CurrentCompany, null);
			AssertEquals("Precondition: ", 0, taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.Count);
			Assert(!taxOverrideGroup.IsTaxFrameworkRelated);

			taxOverrideGroup.SetContext(BusinessContext.TaxFramework);
			Assert(taxOverrideGroup.IsTaxFrameworkRelated);

			taxOverrideGroup.RemoveContext(BusinessContext.TaxFramework);
			Assert(!taxOverrideGroup.IsTaxFrameworkRelated);
		}

		public void TestSkipsValidationOnChargeCodesCollection()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			using (chargeCode.GetValidationSuspender())
			{
				chargeCode.FillWithValidTestData();
				chargeCode.AC_Desc = ZString.Empty;
			}
			AssertNoErrors("Precondition: charge code", chargeCode);

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			chargeCode.AC_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			AssertCollectionContains("Precondition: Collection Contains Charge Code which has error", chargeCode, taxOverrideGroup.ChargeCodes);

			taxOverrideGroup.RunPreSaveValidation();
			AssertNoErrors("Tax Override Group should not run each Charge Code validation", taxOverrideGroup);

			chargeCode.RunPreSaveValidation();
			Assert("Post Condition: But Charge Code actually has an error and it is registered on tax override group", taxOverrideGroup.HasErrors);
		}

		public void TestSkipsValidationOnChargeCodesLinkedToTaxFrameworkConfigurationCollection()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			using (chargeCode.GetValidationSuspender())
			{
				chargeCode.FillWithValidTestData();
				chargeCode.AC_Desc = ZString.Empty;
			}
			AssertNoErrors("Precondition: charge code", chargeCode);

			var taxFrameworkTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroupChargeCodePivot = Factory.NewWithValidTestData<AccTaxOverrideGroupChargeCodePivot>();
			taxOverrideGroupChargeCodePivot.ACP_AC_ChargeCode = chargeCode.PK;
			taxOverrideGroupChargeCodePivot.ACP_AX_TaxOverrideGroup = taxFrameworkTaxOverrideGroup.PK;
			AssertCollectionContains("Precondition: Collection Contains Charge Code which has error", chargeCode, taxFrameworkTaxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration);

			taxFrameworkTaxOverrideGroup.RunPreSaveValidation();
			AssertNoErrors("Tax Override Group should not run each Charge Code validation", taxFrameworkTaxOverrideGroup);

			chargeCode.RunPreSaveValidation();
			Assert("Post Condition: But Charge Code actually has an error and it is registered on tax override group", taxFrameworkTaxOverrideGroup.HasErrors);
		}

		public void TestTriggerExceptionOnDifferentLedgers_InsertThenUpdateTaxConfiguration()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var taxConfigurationAP = AccountingTestObjectCreator.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAP2 = AccountingTestObjectCreator.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAR = AccountingTestObjectCreator.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxOverrideGroup = AccountingTestObjectCreator.CreateTaxOverrideGroup(GlbCompany.CurrentCompany, null);
			var taxOverrideGroupTaxConfigurationPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfigurationAP.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_AT_TaxID = taxRate.PK;
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taxOverrideGroupInNewFactory1 = factory1.Load<AccTaxOverrideGroup>(taxOverrideGroup.PK);
			var taxOverrideGroupInNewFactory2 = factory2.Load<AccTaxOverrideGroup>(taxOverrideGroup.PK);
			var pivot1_InNewFactory1 = factory1.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot.PK);
			var pivot_InNewFactory2 = factory2.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot.PK);

			var pivot2_InNewFactory1 = taxOverrideGroupInNewFactory1.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			pivot2_InNewFactory1.AXP_ETC_TaxConfiguration = taxConfigurationAP2.PK;
			pivot2_InNewFactory1.AXP_AT_TaxID = taxRate.PK;

			pivot_InNewFactory2.AXP_ETC_TaxConfiguration = taxConfigurationAR.PK;

			AssertNoExceptionThrown(() => factory1.Save());
			AssertTriggerPreventsSave(factory2);
		}

		public void TestTriggerExceptionOnDifferentLedgers_UpdateThenInsertTaxConfiguration()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var taxConfigurationAP = AccountingTestObjectCreator.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAP2 = AccountingTestObjectCreator.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAR = AccountingTestObjectCreator.CreateTaxConfiguration(AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxOverrideGroup = AccountingTestObjectCreator.CreateTaxOverrideGroup(GlbCompany.CurrentCompany, null);
			var taxOverrideGroupTaxConfigurationPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfigurationAP.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_AT_TaxID = taxRate.PK;
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taxOverrideGroupInNewFactory1 = factory1.Load<AccTaxOverrideGroup>(taxOverrideGroup.PK);
			var taxOverrideGroupInNewFactory2 = factory2.Load<AccTaxOverrideGroup>(taxOverrideGroup.PK);
			var pivot_InNewFactory1 = factory1.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot.PK);
			var pivot1_InNewFactory2 = factory2.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot.PK);

			pivot_InNewFactory1.AXP_ETC_TaxConfiguration = taxConfigurationAR.PK;

			var pivot2_InNewFactory2 = taxOverrideGroupInNewFactory2.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			pivot2_InNewFactory2.AXP_ETC_TaxConfiguration = taxConfigurationAP2.PK;
			pivot2_InNewFactory2.AXP_AT_TaxID = taxRate.PK;

			AssertNoExceptionThrown(() => factory1.Save());
			AssertTriggerPreventsSave(factory2);
		}

		#region Tax Overrides

		public void TestNoAuditLogsWithAttibute_TaxOverrides()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var postingOverride = taxOverrideGroup.TaxOverrides.AddNew();

			postingOverride.AO_CostSellAll = "ALL";
			postingOverride.AO_Direction = "OTH";
			postingOverride.AO_JobType = "ALL";
			postingOverride.AO_Origin = AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry;
			postingOverride.AO_Destination = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
			postingOverride.AO_IncoTerm = "ALL";
			postingOverride.AO_TaxRegCntryOrGroup = "ALL";
			postingOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxOverrideGroup.AX_Code = "TESTGROUP";
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, taxOverrideGroup.PK);
			AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
			taxOverrideGroup.AX_Description = "Audit Log Test";
			Factory.Save();
			AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		AccTaxOverrideGroup TestTaxOverrideGroup_innerValue;
		AccTaxOverrideGroup TestTaxOverrideGroup => TestTaxOverrideGroup_innerValue ?? (TestTaxOverrideGroup_innerValue = Factory.NewWithValidTestData<AccTaxOverrideGroup>());

		AccountingTestObjectCreator accountingTestObjectCreator;
		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));

		void AssertTriggerPreventsSave(BusinessObjectFactory factory)
		{
			bool triggerExceptionThrown = false;
			try
			{
				factory.Save();
			}
			catch (ZSaveException e)
			{
				AssertEquals("Tax Override Group cannot have Tax configurations of different Ledgers.", e.InnerException.InnerException.Message);
				triggerExceptionThrown = true;
			}

			AssertEquals("Trigger should have prevented save.", true, triggerExceptionThrown);
		}

		#endregion
	}
}
