using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccChargeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGLAccountsOnGlobalChargeCode()
		{
			AccGLHeader globalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			globalGLHeader.AG_IsGlobal = true;
			AccGLHeader nonGlobalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader.AG_IsGlobal = false;

			TestChargeCode.AC_GC = ZGuid.Empty;
			TestChargeCode.AC_AG_RevenueAccount = TestChargeCode.AC_AG_CostAccount = TestChargeCode.AC_AG_WIPAccount = TestChargeCode.AC_AG_AccrualAccount = TestChargeCode.AC_AG_DisbursementSurplusAccount = TestChargeCode.AC_AG_DisbursementShortfallAccount = nonGlobalGLHeader.PK;
			AssertHasError(TestChargeCode.AC_AG_RevenueAccountInfo, "This account must be a Global GL Account.");
			AssertHasError(TestChargeCode.AC_AG_CostAccountInfo, "This account must be a Global GL Account.");
			AssertHasError(TestChargeCode.AC_AG_WIPAccountInfo, "This account must be a Global GL Account.");
			AssertHasError(TestChargeCode.AC_AG_AccrualAccountInfo, "This account must be a Global GL Account.");
			AssertHasError(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo, "This account must be a Global GL Account.");
			AssertHasError(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo, "This account must be a Global GL Account.");

			TestChargeCode.AC_AG_RevenueAccount = TestChargeCode.AC_AG_CostAccount = TestChargeCode.AC_AG_WIPAccount = TestChargeCode.AC_AG_AccrualAccount = TestChargeCode.AC_AG_DisbursementSurplusAccount = TestChargeCode.AC_AG_DisbursementShortfallAccount = globalGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_RevenueAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_CostAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_WIPAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_AccrualAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);
		}

		public void TestInactiveGLAccountsCreatesValidationError()
		{
			AccGLHeader activelGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader inActiveGlobalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			inActiveGlobalGLHeader.AG_IsActive = false;

			TestChargeCode.AC_GC = ZGuid.Empty;
			TestChargeCode.AC_AG_RevenueAccount = TestChargeCode.AC_AG_CostAccount = TestChargeCode.AC_AG_WIPAccount = TestChargeCode.AC_AG_AccrualAccount = TestChargeCode.AC_AG_DisbursementSurplusAccount = TestChargeCode.AC_AG_DisbursementShortfallAccount = inActiveGlobalGLHeader.PK;
			AssertHasError(TestChargeCode.AC_AG_RevenueAccountInfo, "This Revenue Account is inactive - it may not be used.");
			AssertHasError(TestChargeCode.AC_AG_CostAccountInfo, "This Cost GL Account is inactive - it may not be used.");
			AssertHasError(TestChargeCode.AC_AG_WIPAccountInfo, "This WIP GL Account is inactive - it may not be used.");
			AssertHasError(TestChargeCode.AC_AG_AccrualAccountInfo, "This Accrual Account is inactive - it may not be used.");
			AssertHasError(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo, "This Disbursement Surplus GL Account is inactive - it may not be used.");
			AssertHasError(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo, "This Disbursement Shortfall GL Account is inactive - it may not be used.");

			TestChargeCode.AC_AG_RevenueAccount = TestChargeCode.AC_AG_CostAccount = TestChargeCode.AC_AG_WIPAccount = TestChargeCode.AC_AG_AccrualAccount = TestChargeCode.AC_AG_DisbursementSurplusAccount = TestChargeCode.AC_AG_DisbursementShortfallAccount = activelGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_RevenueAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_CostAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_WIPAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_AccrualAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);
		}

		public void TestCheckAC_IsActive_OnGlobalChargeCode_WhenIsUsedForElectronicProcessingChargeCode()
		{
			(var globalChargeCode, var localChargeCode) = CreateGlobalChargeCodeAndLocalChargeCode();

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			AssertEquals("PreCondition: AC_IsActive is true.", true, localChargeCode.AC_IsActive);
			AssertNoErrors("PreCondition: AC_IsActive should has no errors.", localChargeCode.AC_IsActiveInfo);

			localChargeCode.AC_IsActive = false;

			AssertHasError("AC_IsActive should has errors.", localChargeCode.AC_IsActiveInfo, "This is a system defined Charge Code used in Electronic Processing Fee management and cannot be set to inactive. Please set it to active.");
		}

		public void TestValidateAC_ChargeOtherGroups()
		{
			TestChargeCode.AC_ChargeOtherGroups = "XXX";
			AssertHasError("invalid value", TestChargeCode.AC_ChargeOtherGroupsInfo, "Enter a valid " + TestChargeCode.AC_ChargeOtherGroupsInfo.Description + ".");

			TestChargeCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Agent;
			AssertNoNotifications("valid value", TestChargeCode.AC_ChargeOtherGroupsInfo);

			TestChargeCode.AC_ChargeOtherGroups = "";
			AssertHasError("missing value", TestChargeCode.AC_ChargeOtherGroupsInfo, "Please enter a " + TestChargeCode.AC_ChargeOtherGroupsInfo.Description + ".");
		}

		public void TestValidateAC_Code()
		{
			AccChargeCode code2 = Factory.New(typeof(AccChargeCode)) as AccChargeCode;
			code2.AC_Code = "###";
			code2.AC_GC = ZGuid.NewZGuid();

			TestChargeCode.AC_Code = ")))";
			TestChargeCode.AC_GC = code2.AC_GC;
			Assert("Different codes - no errors", !TestChargeCode.AC_CodeInfo.HasErrors());

			TestChargeCode.AC_Code = "###";
			TestChargeCode.AC_GC = (ZGuid)Env.CurrentCompany.PK;
			TestChargeCode.Validation.ValidateAC_Code();
			Assert("Same code but different company so should not give errors", !TestChargeCode.AC_CodeInfo.HasErrors());

			code2.AC_GC = (ZGuid)Env.CurrentCompany.PK;
			TestChargeCode.Validation.ValidateAC_Code();
			Assert("Same code and same company so should give errors", TestChargeCode.AC_CodeInfo.HasErrors());

			AccChargeCode global = Factory.New(typeof(AccChargeCode)) as AccChargeCode;
			global.AC_Code = "!!!";
			global.AC_GC = ZGuid.Empty;

			TestChargeCode.AC_Code = ")))";
			TestChargeCode.AC_GC = ZGuid.Empty;
			Assert("Different codes - no errors", !TestChargeCode.AC_CodeInfo.HasErrors());

			TestChargeCode.AC_Code = "!!!";
			TestChargeCode.AC_GC = ZGuid.Empty;
			TestChargeCode.Validation.ValidateAC_Code();
			Assert("Same code should give errors", TestChargeCode.AC_CodeInfo.HasErrors());
			AssertEquals("Same code should give errors", "Code must be unique", TestChargeCode.AC_CodeInfo.GetErrors().GetFirst().Message);
		}

		public void TestCheckAC_CodeOnGlobalChargeCode_WhenItIsElectronicProcessingChargeCode()
		{
			(var globalChargeCode, var localChargeCode) = CreateGlobalChargeCodeAndLocalChargeCode();

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			AssertNoErrors("PreCondition: AC_Code should has no errors.", localChargeCode.AC_CodeInfo);

			localChargeCode.AC_Code = "test1";

			AssertHasError("AC_Code should has errors.", localChargeCode.AC_CodeInfo, "This is a system defined Charge Code used in Electronic Processing Fee management and must equal the Global Charge Code test as defined in 'Electronic Processing Charge Code' registry.");
		}

		public void TestCheckAC_AT_GSTRate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsGSTRegistered = true;
			TestChargeCode.AC_GC = company.PK;
			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;

			AssertNull("This company does not support tax rate config", AccTaxRate.Helper.FindTaxRate(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, company.PK.ToGuid()));
			Assert("No error if the tax rate is not supported", !TestChargeCode.AC_AT_GSTRateInfo.HasErrors());

			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_Code = "GSTx";
			rate.AT_Type = AccTaxRate.Types.Rated;
			rate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
			rate.AT_Description = "GST Description";

			var item = Factory.New<StmData>();
			item.SD_Name = AccTaxRate.Helper.MainGSTTaxRegistryID;
			item.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
			item.SD_GuidValue = rate.PK;
			item.SD_DepartmentGuid = company.PK;

			Factory.Save();

			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;

			AssertNotNull("This company support tax rate config", AccTaxRate.Helper.FindTaxRate(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, company.PK.ToGuid()));
			Assert("Need error if the tax rate is supported", TestChargeCode.AC_AT_GSTRateInfo.HasErrors());
		}

		public void TestCheckAC_Code_DoesNotSearchDuplicateForLoginCompany()
		{
			AccChargeCode codeForLoginCompany = Factory.NewWithValidTestData<AccChargeCode>();
			codeForLoginCompany.AC_Code = "###";
			codeForLoginCompany.AC_GC = (ZGuid)Env.CurrentCompany.PK;
			Factory.Save();

			AccChargeCode newCode = Factory.New(typeof(AccChargeCode)) as AccChargeCode;
			newCode.AC_Code = "###";
			newCode.AC_GC = ZGuid.NewZGuid();
			newCode.Validation.ValidateAC_Code();
			Assert("should not give errors as codes are belong to different companies", !newCode.AC_CodeInfo.HasErrors());
		}

		public void TestValidateAC_MarginPercentage()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			TestChargeCode.AC_MarginPercentage = -10M;
			Assert("Error expected (-10)", TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 50M;
			Assert("No errors expected (50)", !TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 110M;
			Assert("Error expected (110)", TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 0.01M;
			Assert("No errors expected (0.01 - Margin)", !TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 0M;
			Assert("Error expected (0 - Margin)", !TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 100M;
			Assert("No errors expected (100)", !TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.AC_MarginPercentage = 0.01M;
			Assert("Error expected (0.01 - Revenue)", TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 0M;
			Assert("No errors expected (0 - Revenue)", !TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			TestChargeCode.AC_MarginPercentage = 0.01M;
			Assert("Error expected (0.01 - Manual Job Accrual)", TestChargeCode.AC_MarginPercentageInfo.HasErrors());
			TestChargeCode.AC_MarginPercentage = 0M;
			Assert("No errors expected (0 - Manual Job Accrual)", !TestChargeCode.AC_MarginPercentageInfo.HasErrors());
		}

		public void TestValidateAC_ChargeType()
		{
			TestChargeCode.AC_ChargeType = "xyz";
			Assert("Error expected (xyz)", TestChargeCode.AC_ChargeTypeInfo.HasErrors());

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("No errors expected (Margin)", !TestChargeCode.AC_ChargeTypeInfo.HasErrors());
			TestChargeCode.AC_ChargeType = "";
			Assert("Error expected (blank)", TestChargeCode.AC_ChargeTypeInfo.HasErrors());

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("No errors expected (Revenue)", !TestChargeCode.AC_ChargeTypeInfo.HasErrors());

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestChargeCode.PK.ToGuid());
			TestChargeCode.AC_ChargeType = "DSB";
			Assert("No errors expected (Disbursement)", !TestChargeCode.AC_ChargeTypeInfo.HasErrors());

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Expect error - charge code is referenced in registry and should be disbursement", TestChargeCode.AC_ChargeTypeInfo.HasErrors());

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Expect error - charge code is referenced in registry and should be disbursement", !TestChargeCode.AC_ChargeTypeInfo.HasErrors());
		}

		public void TestValidateAC_RateCalculator()
		{
			TestChargeCode.AC_RateCalculator = "xyz";
			Assert("Error expected (xyz)", TestChargeCode.AC_RateCalculatorInfo.HasErrors());
			TestChargeCode.AC_RateCalculator = "AGY";
			Assert("No errors expected (AGY)", !TestChargeCode.AC_RateCalculatorInfo.HasErrors());

			TestChargeCode.AC_RateCalculator = "EXL";
			AssertHasError(TestChargeCode.AC_RateCalculatorInfo, "Exclude from Company Tariffs Calculator can't be used as Rate Calculator as such calculator only applicable to Client Rates setup.");
			TestChargeCode.AC_RateCalculator = "AGY";
			AssertNoErrors(TestChargeCode.AC_RateCalculatorInfo);
		}

		public void TestValidateAC_AT_GSTRate()
		{
			TestChargeCode.AC_AT_GSTRate = ZGuid.NewZGuid();
			Assert("No errors expected", !TestChargeCode.AC_AT_GSTRateInfo.HasErrors());
			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;
			Assert("Error expected if Current Company is GST registered", TestChargeCode.AC_AT_GSTRateInfo.HasErrors() || !Env.CurrentCompany.IsGSTRegistered);

			// Global charge codes 
			TestChargeCode.AC_GC = ZGuid.Empty;
			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;
			Assert("No errors expected - due to global charge code", !TestChargeCode.AC_AT_GSTRateInfo.HasErrors());
		}

		public void TestValidateAC_AW_WithholdingTaxRate()
		{
			TestChargeCode.Company.GC_IsWHTRegistered = true;
			Assert("Precondition: withholding tax applicable", TestChargeCode.Company.GC_IsWHTRegistered);
			AssertNoErrorOnAC_AW_WithholdingTaxRate();

			TestChargeCode.Company.GC_IsWHTRegistered = false;
			Assert("Precondition: withholding tax not applicable", !TestChargeCode.Company.GC_IsWHTRegistered);
			AssertNoErrorOnAC_AW_WithholdingTaxRate();

			void AssertNoErrorOnAC_AW_WithholdingTaxRate()
			{
				TestChargeCode.AC_AW_WithholdingTaxRate = ZGuid.NewZGuid();
				Assert("No errors expected", !TestChargeCode.AC_AW_WithholdingTaxRateInfo.HasErrors());
				TestChargeCode.AC_AW_WithholdingTaxRate = ZGuid.Empty;
				Assert("No errors expected even if withholding tax is empty", !TestChargeCode.AC_AW_WithholdingTaxRateInfo.HasErrors());
			}
		}

		public void TestValidateAC_AW_WithholdingTaxRateUsesChargeCodeCompany()
		{
			TestChargeCode.Company.GC_IsWHTRegistered = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			TestChargeCode.AC_AW_WithholdingTaxRate = new ZGuid();
			Assert("Should have no error as WHT does not become mandatory even if the Charge Code Company is WHT Registered", !TestChargeCode.AC_AW_WithholdingTaxRateInfo.HasErrors());
		}

		public void TestChargeTypeDependentPropertyValidation()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			Assert("No errors expected on RevenueAccount (Revenue)", !TestChargeCode.AC_AG_RevenueAccountInfo.HasErrors());
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.Empty;
			Assert("Error expected on RevenueAccount (Revenue)", TestChargeCode.AC_AG_RevenueAccountInfo.HasErrors());
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();
			Assert("No errors expected on CostAccount (Revenue)", !TestChargeCode.AC_AG_CostAccountInfo.HasErrors());
			TestChargeCode.AC_AG_CostAccount = ZGuid.Empty;
			Assert("No errors expected on CostAccount (Revenue)", !TestChargeCode.AC_AG_CostAccountInfo.HasErrors());

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			Assert("No errors expected on RevenueAccount (Overhead)", !TestChargeCode.AC_AG_RevenueAccountInfo.HasErrors());
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.Empty;
			Assert("No errors expected on RevenueAccount (Overhead)", !TestChargeCode.AC_AG_RevenueAccountInfo.HasErrors());
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();
			Assert("No errors expected on CostAccount (Overhead)", !TestChargeCode.AC_AG_CostAccountInfo.HasErrors());
			TestChargeCode.AC_AG_CostAccount = ZGuid.Empty;
			Assert("Error expected on CostAccount (Overhead)", TestChargeCode.AC_AG_CostAccountInfo.HasErrors());
		}

		public void TestValidateAC_DepartmentFilterList()
		{
			GlbDepartment testDepartment = Factory.New<GlbDepartment>();
			testDepartment.GE_Code = "AAA";

			TestChargeCode.AC_DepartmentFilterList = "";
			AssertEquals("Empty values should not be accepted into Department Filter List.", true, TestChargeCode.AC_DepartmentFilterListInfo.HasErrors());

			TestChargeCode.AC_DepartmentFilterList = ",";
			Assert("Error expected - " + TestChargeCode.AC_DepartmentFilterList, TestChargeCode.AC_DepartmentFilterListInfo.HasErrors());
			TestChargeCode.AC_DepartmentFilterList = "ALL";
			Assert("No errors expected - " + TestChargeCode.AC_DepartmentFilterList, !TestChargeCode.AC_DepartmentFilterListInfo.HasErrors());
			TestChargeCode.AC_DepartmentFilterList = "***,###," + testDepartment.GE_Code;
			Assert("Error expected - " + TestChargeCode.AC_DepartmentFilterList, TestChargeCode.AC_DepartmentFilterListInfo.HasErrors());
			TestChargeCode.AC_DepartmentFilterList = "None";
			Assert("Error expected - " + TestChargeCode.AC_DepartmentFilterList, TestChargeCode.AC_DepartmentFilterListInfo.HasErrors());
			TestChargeCode.AC_DepartmentFilterList = testDepartment.GE_Code;
			Assert("No errors expected - " + TestChargeCode.AC_DepartmentFilterList, !TestChargeCode.AC_DepartmentFilterListInfo.HasErrors());
		}

		public void TestValidateAC_DepartmentFilterList_WithWipsAndAccruals()
		{
			GlbDepartment testDepartmentAAA = Factory.New<GlbDepartment>();
			testDepartmentAAA.GE_Code = "AAA";
			GlbDepartment testDepartmentBBB = Factory.New<GlbDepartment>();
			testDepartmentBBB.GE_Code = "BBB";
			TestChargeCode.AC_DepartmentFilterList = testDepartmentAAA.GE_Code;
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertNoErrors(TestChargeCode.AC_DepartmentFilterListInfo);

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionLines wip = Factory.New<AccTransactionLines>();
			wip.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			wip.AL_GB = GlbBranch.CurrentBranch.PK;
			wip.AL_GE = testDepartmentBBB.PK;
			wip.AL_AC = TestChargeCode.PK;
			wip.AL_PostDate = ZDateTime.Now;
			wip.AL_AH = header.PK;
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertNoErrors(TestChargeCode.AC_DepartmentFilterListInfo);

			wip.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertHasErrors(TestChargeCode.AC_DepartmentFilterListInfo);

			wip.AL_ReverseDate = ZDateTime.Now;
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertNoErrors(TestChargeCode.AC_DepartmentFilterListInfo);
		}

		public void TestValidateAC_DepartmentFilterList_WithDepartmentCharges()
		{
			GlbDepartment testDepartmentAAA = Factory.New<GlbDepartment>();
			testDepartmentAAA.GE_Code = "AAA";
			GlbDepartment testDepartmentBBB = Factory.New<GlbDepartment>();
			testDepartmentBBB.GE_Code = "BBB";
			TestChargeCode.AC_DepartmentFilterList = testDepartmentAAA.GE_Code;
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertNoErrors(TestChargeCode.AC_DepartmentFilterListInfo);

			GlbDeptCharges deptCharge = testDepartmentBBB.DeptCharges.AddNew();
			deptCharge.GD_AC = TestChargeCode.PK;
			deptCharge.GD_GC = GlbCompany.CurrentCompany.PK;
			deptCharge.GD_GE = testDepartmentBBB.PK;
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertHasErrors(TestChargeCode.AC_DepartmentFilterListInfo);

			deptCharge.Delete();
			Factory.Save();

			TestChargeCode.Validation.ValidateAC_DepartmentFilterList();
			AssertNoErrors(TestChargeCode.AC_DepartmentFilterListInfo);
		}

		public void TestValidateAC_IATA_ChargeCodeMap()
		{
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "X0X";
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_IATA_ChargeCodeMap = "";
			Assert("No errors expected (blank)", !chargeCode1.AC_IATA_ChargeCodeMapInfo.HasErrors());

			chargeCode1.AC_IATA_ChargeCodeMap = "--";
			AssertEquals("Error expected (--)", 1, chargeCode1.AC_IATA_ChargeCodeMapInfo.GetErrors().Count());
			AssertEquals("Error expected (--)", "Enter a valid IATA Code.", chargeCode1.AC_IATA_ChargeCodeMapInfo.GetErrors().GetFirstMessage());

			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			Assert("No errors expected (AC)", !chargeCode1.AC_IATA_ChargeCodeMapInfo.HasErrors());

			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.TX;
			Assert("No errors expected (TX)", !chargeCode1.AC_IATA_ChargeCodeMapInfo.HasErrors());
		}

		public void TestValidateAC_GovtChargeCode()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
				chargeCode1.AC_Code = "X0X";
				chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				chargeCode1.AC_GovtChargeCode = "S987.098.47";
				AssertNoError(chargeCode1.AC_GovtChargeCodeInfo, "Please enter a value.");

				chargeCode1.AC_GovtChargeCode = "";
				AssertHasError(chargeCode1.AC_GovtChargeCodeInfo, "Please enter a value.");

				chargeCode1.AC_GC = ZGuid.Empty;
				Assert("Precondition", chargeCode1.IsGlobal);
				chargeCode1.AC_GovtChargeCode = "S987.098.47";
				AssertNoError(chargeCode1.AC_GovtChargeCodeInfo, "Please enter a value.");

				chargeCode1.AC_GovtChargeCode = "";
				AssertNoError(chargeCode1.AC_GovtChargeCodeInfo, "Please enter a value.");

				var chargeCode2 = Factory.New<AccChargeCode>();
				chargeCode2.AC_Code = "X0X";
				chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				chargeCode2.AC_GovtChargeCode = ZString.Empty;
				Assert("Precondition", chargeCode2.IsLinkedToGlobalChargeCode);

				AssertNoError(chargeCode2.AC_GovtChargeCodeInfo, "Please enter a value.");
				AssertHasWarning(chargeCode2.AC_GovtChargeCodeInfo, "In your login company recording Government Charge Code is mandatory. Since 'X0X' is linked to Global Charge Code, the mandatory validation is not enforced but you might want to set a Government Charge Code.");

				chargeCode2.AC_GovtChargeCode = "fsdf";
				AssertNoError(chargeCode2.AC_GovtChargeCodeInfo, "Please enter a value.");
				AssertNoWarning(chargeCode2.AC_GovtChargeCodeInfo, "In your login company recording Government Charge Code is mandatory. Since 'X0X' is linked to Global Charge Code, the mandatory validation is not enforced but you might want to set a Government Charge Code.");
			}
		}

		public void TestValidateAC_IATA_ChargeCodeMap_OnlyForValidChargeTypes()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "X0X";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			chargeCode.AC_IATA_ChargeCodeMap = "";
			Assert("No errors expected (blank)", !chargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());

			chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			AssertEquals("Error expected, not a valid charge type (Comment)", 1, chargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().Count());
			AssertEquals(string.Format("Cannot assign IATA code for charge code with charge type '{0}'.", Core.Constants.ChargeType.Comment), chargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().GetFirstMessage());

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertEquals("No errors expected, charge type Disbursement", false, chargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("No errors expected, charge type Margin", false, chargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertEquals("Error expected, not a valid charge type (Non-Accruals)", 1, chargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().Count());
			AssertEquals(string.Format("Cannot assign IATA code for charge code with charge type '{0}'.", Core.Constants.ChargeType.NonAccrual), chargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().GetFirstMessage());

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			AssertEquals("Error expected, not a valid charge type (Overhead)", 1, chargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().Count());
			AssertEquals(string.Format("Cannot assign IATA code for charge code with charge type '{0}'.", Core.Constants.ChargeType.Overhead), chargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().GetFirstMessage());

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals("No errors expected, charge type Revenue", false, chargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());
		}

		public void TestValidateAC_IsActive()
		{
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestChargeCode.PK.ToGuid());
			TestChargeCode.AC_IsActive = true;
			Assert("No errors expected", !TestChargeCode.AC_IsActiveInfo.HasErrors());

			TestChargeCode.AC_IsActive = false;
			Assert("Error expected - charge code is referenced in the registry", TestChargeCode.AC_IsActiveInfo.HasErrors());
		}

		public void TestValidateAC_PrintSequence()
		{
			TestChargeCode.AC_PrintSequence = 0;
			Assert("Not expecting errors", !TestChargeCode.AC_PrintSequenceInfo.HasErrors());

			TestChargeCode.AC_PrintSequence = -1;
			Assert("Error: print sequence should be between 0 and 999", TestChargeCode.AC_PrintSequenceInfo.HasErrors());

			TestChargeCode.AC_PrintSequence = 1000;
			Assert("Error: print sequence should be between 0 and 999", TestChargeCode.AC_PrintSequenceInfo.HasErrors());
		}

		public void TestValidateAC_LocalLanguageDescription()
		{
			TestChargeCode.AC_Desc = "Crap";

			TestChargeCode.AC_LocalLanguageDescription = "";
			AssertNoErrors(TestChargeCode.AC_LocalLanguageDescriptionInfo);
			AssertNoWarnings(TestChargeCode.AC_LocalLanguageDescriptionInfo);

			TestChargeCode.AC_LocalLanguageDescription = "CRAP";
			AssertHasErrors(TestChargeCode.AC_LocalLanguageDescriptionInfo);
			AssertNoWarnings(TestChargeCode.AC_LocalLanguageDescriptionInfo);

			TestChargeCode.AC_LocalLanguageDescription = "CrapTwo";
			AssertNoErrors(TestChargeCode.AC_LocalLanguageDescriptionInfo);
			AssertNoWarnings(TestChargeCode.AC_LocalLanguageDescriptionInfo);

			TestChargeCode.AC_Desc = "";
			TestChargeCode.AC_LocalLanguageDescription = "";
			AssertNoErrors(TestChargeCode.AC_LocalLanguageDescriptionInfo);
			AssertNoWarnings(TestChargeCode.AC_LocalLanguageDescriptionInfo);
		}

		public void TestValidateAC_AllowDescriptionOvertype()
		{
			TestChargeCode.AC_AllowDescriptionOvertype = true;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestChargeCode.Validation.ValidateAC_AllowDescriptionOvertype();
			AssertNoErrors(TestChargeCode.AC_AllowDescriptionOvertypeInfo);

			TestChargeCode.AC_AllowDescriptionOvertype = false;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestChargeCode.Validation.ValidateAC_AllowDescriptionOvertype();
			AssertHasErrors(TestChargeCode.AC_AllowDescriptionOvertypeInfo);

			TestChargeCode.AC_AllowDescriptionOvertype = false;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			TestChargeCode.Validation.ValidateAC_AllowDescriptionOvertype();
			AssertNoErrors(TestChargeCode.AC_AllowDescriptionOvertypeInfo);
		}

		public void TestValidateAC_DefaultCommissionProduct()
		{
			TestChargeCode.AC_IsCommissionable = true;
			using (CommissionLookupsForTest.TemporarilyOverrideListsForTesting(new[] { "XXX" }, new[] { "XXX" }, new[] { "XXX" }))
			{
				TestChargeCode.AC_DefaultCommissionProduct = "AAA";
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionProductInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionProductInfo, true);

				TestChargeCode.AC_DefaultCommissionProduct = "XXX";
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionProductInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionProductInfo, false);

				TestChargeCode.AC_DefaultCommissionProduct = ZString.Empty;
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionProductInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionProductInfo, false);

				TestChargeCode.AC_DefaultCommissionService = "XXX";
				TestChargeCode.Validation.ValidateAC_DefaultCommissionProduct();
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionProductInfo, true);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionProductInfo, false);

				TestChargeCode.AC_IsCommissionable = false;
				TestChargeCode.Validation.ValidateAC_DefaultCommissionProduct();
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionProductInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionProductInfo, false);
			}
		}

		public void TestValidateAC_DefaultCommissionService()
		{
			TestChargeCode.AC_IsCommissionable = true;
			using (CommissionLookupsForTest.TemporarilyOverrideListsForTesting(new[] { "XXX" }, new[] { "XXX" }, new[] { "XXX" }))
			{
				TestChargeCode.AC_DefaultCommissionService = "AAA";
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionServiceInfo, true);

				TestChargeCode.AC_DefaultCommissionService = "XXX";
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);

				TestChargeCode.AC_DefaultCommissionService = ZString.Empty;
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);

				TestChargeCode.AC_DefaultCommissionSubModule = "XXX";
				TestChargeCode.Validation.ValidateAC_DefaultCommissionService();
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionServiceInfo, true);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);

				TestChargeCode.AC_IsCommissionable = false;
				TestChargeCode.Validation.ValidateAC_DefaultCommissionService();
				AssertMandatoryValidationError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionServiceInfo, false);
			}
		}

		public void TestValidateAC_DefaultCommissionSubModule()
		{
			TestChargeCode.AC_IsCommissionable = true;
			using (CommissionLookupsForTest.TemporarilyOverrideListsForTesting(new[] { "XXX" }, new[] { "XXX" }, new[] { "XXX" }))
			{
				TestChargeCode.AC_DefaultCommissionSubModule = "AAA";
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionSubModuleInfo, true);

				TestChargeCode.AC_DefaultCommissionSubModule = "XXX";
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionSubModuleInfo, false);

				TestChargeCode.AC_DefaultCommissionSubModule = ZString.Empty;
				AssertListValidationInvalidCodeError(TestChargeCode.AC_DefaultCommissionSubModuleInfo, false);
			}
		}

		public void TestValidateDuplicateTaxOverrides()
		{
			TestChargeCode.AC_AX_TaxOverrideGroup = Factory.New<AccTaxOverrideGroup>().PK;
			AccChargeTaxOverride taxOverride1 = TestChargeCode.TaxOverrides.AddNew();
			AccChargeTaxOverride taxOverride2 = TestChargeCode.TaxOverrides.AddNew();
			AccChargeTaxOverride taxOverride_fromGroup = TestChargeCode.TaxOverrideGroup.TaxOverrides.AddNew();

			taxOverride1.AO_Direction = "ALL";
			taxOverride1.AO_IncoTerm = "ALL";
			taxOverride1.AO_JobType = "ALL";
			taxOverride1.AO_Origin = "ALL";
			taxOverride1.AO_Destination = "ALL";
			taxOverride1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride1.AO_CustomsStatus = "ALL";

			taxOverride2.AO_Direction = "ALL";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_JobType = "ALL";
			taxOverride2.AO_Origin = "ALL";
			taxOverride2.AO_Destination = "ALL";
			taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride2.AO_CustomsStatus = "ALL";

			TestChargeCode.Validation.ValidateDuplicateTaxOverrides();
			AssertNoRowErrors(taxOverride1);
			AssertHasRowError(taxOverride2, "You cannot have identical tax overrides.");

			taxOverride2.AO_JobType = "CST";
			TestChargeCode.Validation.ValidateAll();
			AssertNoRowErrors(taxOverride1);
			AssertNoRowErrors(taxOverride2);

			taxOverride_fromGroup.AO_Direction = "ALL";
			taxOverride_fromGroup.AO_IncoTerm = "ALL";
			taxOverride_fromGroup.AO_JobType = "ALL";
			taxOverride_fromGroup.AO_Origin = "ALL";
			taxOverride_fromGroup.AO_Destination = "ALL";
			taxOverride_fromGroup.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride_fromGroup.AO_CustomsStatus = "ALL";
			TestChargeCode.Validation.ValidateDuplicateTaxOverrides();
			AssertHasRowError(taxOverride1, "You cannot have identical tax overrides with tax override group.");
			AssertNoRowErrors(taxOverride2);

			taxOverride_fromGroup.AO_JobType = "CST";
			TestChargeCode.Validation.ValidateAll();
			AssertNoRowErrors(taxOverride1);
			AssertHasRowError(taxOverride2, "You cannot have identical tax overrides with tax override group.");

			taxOverride_fromGroup.AO_Destination = "DOM";
			TestChargeCode.Validation.ValidateDuplicateTaxOverrides();
			AssertNoRowErrors(taxOverride1);
			AssertNoRowErrors(taxOverride2);
		}

		[ExpectNoExceptions]
		public void TestValidateDuplicateComplianceDescriptions()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var expectedErrorMessage = "The combination of Job Type, Transport Mode and Sell Supply Type must be unique.";

			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var duplicateValidationHelperMock = new Mock<IDuplicateValidationHelper>();

			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);
			jobConfigurationHelperFactoryMock.Setup(x => x.GetDuplicateValidationHelper()).Returns(duplicateValidationHelperMock.Object);

			chargeCode.Validation.ValidateAll();
			duplicateValidationHelperMock.Verify(x => x.CheckDuplicates(It.IsAny<IDuplicateValidationCollectionProvider<AccChargeComplianceDescription>>(), It.IsAny<string>()), Times.Once);
			duplicateValidationHelperMock.Verify(x => x.CheckDuplicates(chargeCode, expectedErrorMessage));
		}

		public void TestCheckAC_ChargeType_ValidateCommentChargeType()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.ChargeComplianceDescriptions.AddNew();
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			var errorMessage = "Sell Compliance Description rules must be empty on Comment Charge Code.";

			Assert("Precondition: IsComment", chargeCode.IsComment);
			AssertEquals("Precondition: Sell Compliance Description", 1, chargeCode.ChargeComplianceDescriptions.Count);
			AssertHasError(chargeCode.AC_ChargeTypeInfo, errorMessage);

			chargeCode.AC_ChargeType = Constants.ChargeType.Revenue;
			Assert("Precondition: IsComment", !chargeCode.IsComment);
			AssertNoErrors(chargeCode.AC_ChargeTypeInfo);

			chargeCode.ChargeComplianceDescriptions.DeleteAll();
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			AssertEquals("Precondition: Sell Compliance Description Count", 0, chargeCode.ChargeComplianceDescriptions.Count);
			AssertNoErrors(chargeCode.AC_ChargeTypeInfo);
		}

		public void TestCheckAC_ChargeType_OnGlobalChargeCode_WhenIsUsedForElectronicProcessingChargeCode()
		{
			(var globalChargeCode, var localChargeCode) = CreateGlobalChargeCodeAndLocalChargeCode();

			AssertEquals("Precondition: The charge type of local charge code is DSB.", Constants.ChargeType.Disbursement, localChargeCode.AC_ChargeType);
			AssertEquals("Precondition: The charge type of global charge code is DSB.", Constants.ChargeType.Disbursement, globalChargeCode.AC_ChargeType);
			AssertNoErrors("Precondition: local AC_ChargeTypeInfo should has no errors.", localChargeCode.AC_ChargeTypeInfo);
			AssertNoErrors("Precondition: global AC_ChargeTypeInfo should has no errors.", globalChargeCode.AC_ChargeTypeInfo);

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			localChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			AssertHasError("AC_ChargeTypeInfo should has errors.", localChargeCode.AC_ChargeTypeInfo, "This charge code is used in Electronic Processing Fee Management and can only be set to 'MRG - Margin' or 'DSB - Disbursement'.");

			globalChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			AssertHasError("AC_ChargeTypeInfo should has errors.", globalChargeCode.AC_ChargeTypeInfo, "This charge code is used in Electronic Processing Fee Management and can only be set to 'MRG - Margin' or 'DSB - Disbursement'.");
		}

		#region TestGlobalVsLocalValidationOnLocalChargeCode

		List<string> testGlobalVsLocalValidationOnLocalChargeCode_FieldsTested;

		public void TestGlobalVsLocalValidationOnLocalChargeCode()
		{
			testGlobalVsLocalValidationOnLocalChargeCode_FieldsTested = new List<string>();

			// Fields that are copied from global charge code
			var cost2 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Cost2);
			var rev2 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Revenue2);
			var cost3 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Cost3);
			var rev3 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Revenue3);

			var costClearing2 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_CostClearing2);
			var revClearing2 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_RevenueClearing2);
			var costClearing3 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_CostClearing3);
			var revClearing3 = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_RevenueClearing3);

			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_CostClearingAccountInfo, costClearing2, costClearing3, true, false, AccChargeCodeTest.GLHeader_CostClearing, null, null, Constants.ChargeType.NonAccrual);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_RevenueClearingAccountInfo, revClearing2, revClearing3, true, false, AccChargeCodeTest.GLHeader_RevenueClearing, null, null, Constants.ChargeType.NonAccrual);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_AccrualAccountInfo, cost2, cost3, true, false, AccChargeCodeTest.GLHeader_Cost);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_CostAccountInfo, cost2, cost3, true, false, AccChargeCodeTest.GLHeader_Cost);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_RevenueAccountInfo, rev2, rev3, true, false, AccChargeCodeTest.GLHeader_Revenue);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_WIPAccountInfo, rev2, rev3, true, false, AccChargeCodeTest.GLHeader_Revenue);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_DisbursementShortfallAccountInfo, rev2, rev3, true, false, AccChargeCodeTest.GLHeader_Revenue, null, null, Constants.ChargeType.Disbursement);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AG_DisbursementSurplusAccountInfo, rev2, rev3, true, false, AccChargeCodeTest.GLHeader_Revenue, null, null, Constants.ChargeType.Disbursement);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AllowDescriptionOvertypeInfo, ZBool.False, ZBool.False, true, false, "Y");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AR_ExpenseGroupInfo, Factory.NewWithValidTestData<AccGroups>().PK, Factory.NewWithValidTestData<AccGroups>().PK, true, false, "Empty");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AR_SalesGroupInfo, Factory.NewWithValidTestData<AccGroups>().PK, Factory.NewWithValidTestData<AccGroups>().PK, true, false, "Empty");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_ChargeGroupInfo, (ZString)"ORG", (ZString)"ORG", true, false, "BRK");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_ChargeOtherGroupsInfo, (ZString)ChargeOtherGroupsList.Codes.Agent, (ZString)ChargeOtherGroupsList.Codes.Agent, true, false, "PRC");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_ChargeSubGroupInfo, (ZString)"STG", (ZString)"STG", true, false, "LBR");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_ChargeTypeInfo, (ZString)Constants.ChargeType.Revenue, (ZString)Constants.ChargeType.Disbursement, true, false, "MRG");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_DepartmentFilterListInfo, (ZString)Env.CurrentDepartment.Code, (ZString)Env.CurrentDepartment.Code, true, false, "ALL");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_DescInfo, new ZString("Aaa Bee See One"), new ZString("Aaa Bee See Two"), true, false, "CC2 Desc");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_ENettChargeCodeMapInfo, new ZString("Whatever"), new ZString("Whatever2"), true, false, "Empty");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_GoodsServiceTypeInfo, (ZString)GoodServiceTypes.Codes.GDS, (ZString)GoodServiceTypes.Codes.GDS, true, false, "SRV");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_IATA_ChargeCodeMapInfo, (ZString)"AC", (ZString)"AS", true, false, "Empty");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_IsActiveInfo, ZBool.False, ZBool.False, true, false, "Y");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_IsCommissionableInfo, ZBool.False, ZBool.False, true, false, "Y");

			using (CommissionLookupsForTest.TemporarilyOverrideListsForTesting(new[] { "XXX" }, new[] { "XXX" }, new[] { "XXX" }))
			{
				TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_DefaultCommissionProductInfo, (ZString)"XXX", (ZString)"XXX", true, false, "Empty");

				Action<AccChargeCode> fillDefaultCommissionProductDelegate = (x) => x.AC_DefaultCommissionProduct = "XXX";
				Action<AccChargeCode> fillDefaultCommissionProductAndServiceDelegate = (x) => { x.AC_DefaultCommissionProduct = "XXX"; x.AC_DefaultCommissionService = "XXX"; };
				TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_DefaultCommissionServiceInfo, (ZString)"XXX", (ZString)"XXX", true, false, "Empty", fillDefaultCommissionProductDelegate, fillDefaultCommissionProductDelegate);
				TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_DefaultCommissionSubModuleInfo, (ZString)"XXX", (ZString)"XXX", true, false, "Empty", fillDefaultCommissionProductAndServiceDelegate, fillDefaultCommissionProductAndServiceDelegate);
			}

			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_IsGroupageChargeInfo, ZBool.True, ZBool.True, true, false, "N");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_IsAdhocServiceChargeInfo, ZBool.True, ZBool.True, true, false, "N");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_MarginPercentageInfo, new ZDecimal(66M), new ZDecimal(33M), true, false, "100");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_PrintSequenceInfo, new ZShort(4), new ZShort(3), true, false, "0");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_RateCalculatorInfo, (ZString)"AGY", (ZString)"CMB", true, false, "Empty");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_ShowOnQuotationInfo, ZBool.False, ZBool.False, true, false, "Y");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_SuppressOnQuoteIfZeroInfo, ZBool.True, ZBool.True, true, false, "N");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_EnergySourceGroupInfo, (ZString)"FUL", (ZString)"FUL", true, false, "Empty");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AC_RevenueChargeCodeInfo, Factory.NewWithValidTestData<AccChargeCode>().PK, Factory.NewWithValidTestData<AccChargeCode>().PK, true, false, "Empty");

			// Fields not copied
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AW_WithholdingTaxRateInfo, Factory.NewWithValidTestData<AccWithholding>().PK, Factory.NewWithValidTestData<AccWithholding>().PK, false, false, "");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AX_TaxOverrideGroupInfo, Factory.NewWithValidTestData<AccTaxOverrideGroup>().PK, Factory.NewWithValidTestData<AccTaxOverrideGroup>().PK, false, false, "");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_AT_GSTRateInfo, Factory.NewWithValidTestData<AccTaxRate>().PK, Factory.NewWithValidTestData<AccTaxRate>().PK, false, false, "");
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_InputGSTVATRecoverableInfo, new ZDecimal(0.5M), new ZDecimal(0.6M), false, false, "", c => c.AC_ChargeType = Constants.ChargeType.Overhead, null);
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_GovtChargeCodeInfo, (ZString)"S0098", (ZString)"S0097", false, false, "");

			// TestShowDifferenceWarnings
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(c => c.AC_MarginPercentageInfo, new ZDecimal(66M), new ZDecimal(33M), true, true, "100", c => c.ShowDifferenceWarnings = true, c => c.ShowDifferenceWarnings = true);

			// Check we have tested all fields
			// AC_GC not tested here as not user editable, PK not relevant to validation, AC_Code tested elsewhere as it is to do with linking, and audit fields not tested
			var ignoreList = new List<string> { "AC_PK", "AC_GC", "AC_Code", "AC_LocalLanguageDescription", "AC_SystemCreateTimeUtc", "AC_SystemCreateUser", "AC_SystemLastEditTimeUtc", "AC_SystemLastEditUser" };
			int columnsChecked = 0;
			foreach (DataColumn column in ((INeedRow)Factory.NewWithValidTestData<AccChargeCode>()).Row.Table.Columns)
			{
				if (!ignoreList.Contains(column.ColumnName))
				{
					columnsChecked++;
					Assert(string.Format("We have tested {0}", column.ColumnName), testGlobalVsLocalValidationOnLocalChargeCode_FieldsTested.Contains(column.ColumnName));
				}
			}

			AssertEquals("Number of columns checked. Change only if you add another field to the DB", 39, columnsChecked);
		}

		void TestGlobalVsLocalValidationOnLocalChargeCode_Field(
			Func<AccChargeCode, ZPropertyInfo> propertyInfoForCharge,
			IZType newValue,
			IZType newValue2,
			bool expectWarningWhenChanged,
			bool expectWarningWhenSame,
			string expectedStringForField)
		{
			TestGlobalVsLocalValidationOnLocalChargeCode_Field(propertyInfoForCharge, newValue, newValue2, expectWarningWhenChanged, expectWarningWhenSame, expectedStringForField, null, null);
		}

		void TestGlobalVsLocalValidationOnLocalChargeCode_Field(
			Func<AccChargeCode, ZPropertyInfo> propertyInfoForCharge,
			IZType newValue,
			IZType newValue2,
			bool expectWarningWhenChanged,
			bool expectWarningWhenSame,
			string expectedStringForField,
			Action<AccChargeCode> additionalChargeCodeSetup,
			Action<AccChargeCode> additionalGlobalChargeCodeSetup,
			string chargeType = "")
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, true, false, "CC1", "CC2", chargeType);

			if (additionalChargeCodeSetup != null)
			{
				additionalChargeCodeSetup(normalChargeCodeLinked);
			}

			if (additionalGlobalChargeCodeSetup != null)
			{
				additionalGlobalChargeCodeSetup(globalChargeCode);
			}

			var propertyInfo = propertyInfoForCharge(normalChargeCodeLinked);
			var globalPropertyInfo = propertyInfoForCharge(globalChargeCode);

			// Local Side
			var warnings = propertyInfo.GetWarnings();
			AssertEquals(string.Format("{0} same as global, so no warnings.", propertyInfo.Name), 0, warnings.Count());

			propertyInfo.Value = newValue;
			warnings = propertyInfo.GetWarnings();
			if (expectWarningWhenChanged)
			{
				AssertEquals(string.Format("{0} - Different to global, so 1 warning", propertyInfo.Name), 1, warnings.Count());
				AssertEquals(
					string.Format("{0} - Warning message", propertyInfo.Name),
					string.Format("This has a different value to that set on the Global Charge Code. This field will not be updated when the Global Charge Code is changed. To rectify this, set the value to be the same as the global value, i.e. '{0}'", expectedStringForField),
					warnings.GetFirst().Message);
			}
			else
			{
				AssertEquals(string.Format("{0} - No warnings. Not synchrnoised with global", propertyInfo.Name), 0, warnings.Count());
			}

			Factory.Save();
			normalChargeCodeLinked.Validation.ValidateAll();
			warnings = propertyInfo.GetWarnings();

			if (expectWarningWhenSame)
			{
				AssertEquals(string.Format("{0} - Different to global, so 1 warning", propertyInfo.Name), 1, warnings.Count());
				AssertEquals(
					string.Format("{0} - Warning message", propertyInfo.Name),
					string.Format("This has a different value to that set on the Global Charge Code. This field will not be updated when the Global Charge Code is changed. To rectify this, set the value to be the same as the global value, i.e. '{0}'", expectedStringForField),
					warnings.GetFirst().Message);
			}
			else
			{
				AssertEquals(string.Format("{0} - No warnings after save (due to no changes)", propertyInfo.Name), 0, warnings.Count());
			}

			testGlobalVsLocalValidationOnLocalChargeCode_FieldsTested.Add(propertyInfo.Name);

			// Global Side
			warnings = globalPropertyInfo.GetWarnings();
			AssertEquals(string.Format("{0} same as local, so no warnings.", globalPropertyInfo.Name), 0, warnings.Count());

			if (expectWarningWhenChanged)
			{
				globalPropertyInfo.Value = newValue2;
				globalChargeCode.Validation.ValidateAll();
				warnings = globalPropertyInfo.GetWarnings();
				AssertEquals(string.Format("{0} - Different to local, so 1 warning", globalPropertyInfo.Name), 1, warnings.Count());
			}
			else
			{
				AssertEquals(string.Format("{0} - No warnings. Not synchrnoised with local", globalPropertyInfo.Name), 0, warnings.Count());
			}

			Factory.Save();
			globalChargeCode.Validation.ValidateAll();
			warnings = globalPropertyInfo.GetWarnings();

			if (expectWarningWhenSame)
			{
				AssertEquals(string.Format("{0} - Different to local, so 1 warning", globalPropertyInfo.Name), 1, warnings.Count());
			}
			else
			{
				AssertEquals(string.Format("{0} - No warnings after save (due to no changes)", globalPropertyInfo.Name), 0, warnings.Count());
			}

			globalChargeCode.Delete();
			Factory.Save();
		}

		#endregion

		public void TestValidateAC_AX_TaxOverrideGroup()
		{
			TestChargeCode.Validation.ValidateAll();
			AssertEquals("Precondition:", ZGuid.Empty, TestChargeCode.AC_AX_TaxOverrideGroup);
			AssertNoErrors(TestChargeCode.AC_AX_TaxOverrideGroupInfo);

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			TestChargeCode.AC_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			Assert("Precondition: NOT Tax Framework related", !taxOverrideGroup.IsTaxFrameworkRelated);
			TestChargeCode.Validation.ValidateAll();
			AssertNoErrors(TestChargeCode.AC_AX_TaxOverrideGroupInfo);

			taxOverrideGroup.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			Assert("Precondition: Is Tax Framework related", taxOverrideGroup.IsTaxFrameworkRelated);
			TestChargeCode.Validation.ValidateAll();
			AssertHasError(TestChargeCode.AC_AX_TaxOverrideGroupInfo, "This is an invalid selection. Please select a valid ‘Tax Override Group’ from the list.");

			taxOverrideGroup.RemoveContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			Assert("Precondition: NOT TaxFramework related", !taxOverrideGroup.IsTaxFrameworkRelated);
			TestChargeCode.Validation.ValidateAll();
			AssertNoErrors(TestChargeCode.AC_AX_TaxOverrideGroupInfo);

			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			Assert("Precondition:", taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.Count > 0);
			Assert("Precondition: is TaxFramework related", taxOverrideGroup.IsTaxFrameworkRelated);
			TestChargeCode.Validation.ValidateAll();
			AssertHasError(TestChargeCode.AC_AX_TaxOverrideGroupInfo, "This is an invalid selection. Please select a valid ‘Tax Override Group’ from the list.");
		}

		public void TestValidateA0_DebtorRole()
		{
			var overrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			TestChargeCode.AC_AX_TaxOverrideGroup = overrideGroup.PK;
			var taxOverride_fromGroup = TestChargeCode.TaxOverrideGroup.TaxOverrides.AddNew();

			taxOverride_fromGroup.AO_DebtorRole = "ABB";
			AssertHasError(taxOverride_fromGroup.AO_DebtorRoleInfo, "Enter a valid " + taxOverride_fromGroup.AO_DebtorRoleInfo.Description + ".");

			taxOverride_fromGroup.TaxOverrideGroup.Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			taxOverride_fromGroup.AO_DebtorRole = "AGT";
			TestChargeCode.Validation.ValidateAll();
			AssertHasError(taxOverride_fromGroup.AO_DebtorRoleInfo, "The Debtor Role must be empty on Tax Configuration Override Groups. Please delete and recreate this record.");

			taxOverride_fromGroup.TaxOverrideGroup.Factory.RemoveContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			taxOverride_fromGroup.AO_DebtorRole = "AGT";
			TestChargeCode.Validation.ValidateAll();
			AssertNoErrors(taxOverride_fromGroup.AO_DebtorRoleInfo);
		}

		#region TestGlobalVsLocalValidationOnLocalChargeCode_ChildCollections and helpers

		public void TestGlobalVsLocalValidationOnLocalChargeCode_ChildCollections()
		{
			var testedProperties = new List<string>();

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			// Sync collections
			var syncCollections = new ITestGlobalVsLocalValidationOnLocalChargeCodeHelper[] {
				new ChargeTypeOverridesHelper(), new RevenueRecOverridesHelper(), new GLPostingOverridesHelper(), new ApportionmentMethodOverridesHelper(), new CreditorOverridesHelper() };

			foreach (var helper in syncCollections)
			{
				helper.PopulateCollection(globalChargeCode);
				Factory.Save();
				AssertEquals(string.Format("{0} - Precondition values copied to local charge code", helper.CollectionPropertyName), 1, helper.GetCollection(normalChargeCodeLinked).Count);
				normalChargeCodeLinked.Validation.ValidateAll();
				globalChargeCode.Validation.ValidateAll();
				AssertEquals(string.Format("{0} - [Local] Overrides same in both, so no warning", helper.CollectionPropertyName), 0, helper.GetCollection(normalChargeCodeLinked).ToArray()[0].RowWarnings.Count());
				AssertEquals(string.Format("{0} - [Global] Overrides same in both, so no warning", helper.CollectionPropertyName), 0, helper.GetCollection(globalChargeCode).ToArray()[0].RowWarnings.Count());
				helper.PokeLocalChargeCode(normalChargeCodeLinked);
				normalChargeCodeLinked.Validation.ValidateAll();
				globalChargeCode.Validation.ValidateAll();
				AssertEquals(string.Format("{0} - [Local] Overrides different and changes so warning shown", helper.CollectionPropertyName), 1, helper.GetCollection(normalChargeCodeLinked).ToArray()[0].RowWarnings.Count());
				AssertEquals(string.Format("{0} - [Global] Overrides different and changes so warning shown", helper.CollectionPropertyName), 1, helper.GetCollection(globalChargeCode).ToArray()[0].RowWarnings.Count());
				testedProperties.Add(helper.CollectionPropertyName);
			}

			// Non-Sync collections
			var nonSyncCollections = new ITestGlobalVsLocalValidationOnLocalChargeCodeHelper[] {
				new TaxOverridesHelper(), new BranchOverridesHelper(), new ChargeComplianceDescriptionsHelper(), new AirlineIATACodeOverridesHelper(), new UniversalChargeCodeMappingsCollectionHelper(), new GovtChargeCodeOverridesHelper(), new SupplyTypeOverridesHelper() };

			foreach (var helper in nonSyncCollections)
			{
				AssertEquals(string.Format("{0} - Precondition values not copied to local charge code", helper.CollectionPropertyName), 0, helper.GetCollection(normalChargeCodeLinked).Count);
				helper.PopulateCollection(normalChargeCodeLinked);
				helper.PokeLocalChargeCode(normalChargeCodeLinked);
				normalChargeCodeLinked.Validation.ValidateAll();
				globalChargeCode.Validation.ValidateAll();
				AssertContainsExactElementsInAnyOrder
				(
					$"{helper.CollectionPropertyName} - [Local] We don't sync this collection, so no warning",
					Array.Empty<string>(),
					helper.GetCollection(normalChargeCodeLinked).ToArray()[0].RowWarnings.Select(x => x.Message)
				);
				testedProperties.Add(helper.CollectionPropertyName);
			}

			// Check for collections we haven't tested
			var properties = normalChargeCode.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			testedProperties.Add("ChildChargeCodes"); // Ignore - This is used for business logic only. This is not bound to the form.
			testedProperties.Add("DataVersionLogs"); // Ignore - This is architecture collection. No validation
			testedProperties.Add(nameof(AccChargeCode.PlaceOfSupplyConfigurations)); // Ignore - Collection is disabled and unavailable for global charge codes.

			foreach (var property in properties)
			{
				if (property.PropertyType.GetInterface("IBusinessObjectCollection") != null)
				{
					if (!testedProperties.Contains(property.Name))
					{
						Fail(string.Format("There is no test for the collection: {0}", property.Name));
					}
				}
			}
		}

		public interface ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c);
			ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c);
			IBusinessObjectCollection GetCollection(AccChargeCode c);
			string CollectionPropertyName { get; }
		}

		public class ChargeTypeOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.ChargeTypeOverrides.AddNew();
				o.AN_JobDirection = o.Lookups.DirectionList[0].Code;
				o.AN_JobType = o.Lookups.JobTypes[0].Code;
				o.AN_ChargeType = o.Lookups.AC_ChargeType_List[0].Code;
				o.AN_InvoiceType = o.Lookups.InvoiceTypes[0].Code;
				o.AN_MarginPercentage = 50;
				o.AN_MarginPercentage = 50M;
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.ChargeTypeOverrides[0].AN_MarginPercentage = 51M; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.ChargeTypeOverrides; }
			public string CollectionPropertyName { get { return "ChargeTypeOverrides"; } }
		}

		public class RevenueRecOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.RevenueRecOverrides.AddNew();
				o.AE_JobType = o.JobTypeList[0].Code;
				o.AE_Direction = o.DirectionList[0].Code;
				o.AE_Mode = o.ModeList[0].Code;
				o.AE_BrokerType = o.BrokerList[0].Code;
				o.AE_RecognitionType = o.RecognitionDateOptionList[0].Code;
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.RevenueRecOverrides[0].AE_RecognitionType = c.RevenueRecOverrides[0].RecognitionDateOptionList[1].Code; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.RevenueRecOverrides; }
			public string CollectionPropertyName { get { return "RevenueRecOverrides"; } }
		}

		public class GLPostingOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.GLPostingOverrides.AddNew();
				o.Y1_GE = GlbDepartment.CurrentDepartment.PK;
				o.Y1_AG_ACR = AccChargeCodeTest.AccGLHeaderPK(c.Factory, AccChargeCodeTest.GLHeader_Cost);
				o.Y1_AG_CST = AccChargeCodeTest.AccGLHeaderPK(c.Factory, AccChargeCodeTest.GLHeader_Cost);
				o.Y1_AG_REV = AccChargeCodeTest.AccGLHeaderPK(c.Factory, AccChargeCodeTest.GLHeader_Revenue);
				o.Y1_AG_WIP = AccChargeCodeTest.AccGLHeaderPK(c.Factory, AccChargeCodeTest.GLHeader_Revenue);
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.GLPostingOverrides[0].Y1_AG_ACR = AccChargeCodeTest.AccGLHeaderPK(c.Factory, AccChargeCodeTest.GLHeader_Cost2); return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.GLPostingOverrides; }
			public string CollectionPropertyName { get { return "GLPostingOverrides"; } }
		}

		public class TaxOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.TaxOverrides.AddNew();
				o.AO_AT = AccTaxRate.GetNOTREPORTTaxID(c.Factory, c.Company).PK;
				o.AO_CostSellAll = "ALL";
				o.AO_Destination = "ALL";
				o.AO_Direction = "ALL";
				o.AO_IncoTerm = "ALL";
				o.AO_JobType = "ALL";
				o.AO_Origin = "ALL";
				o.FillWithValidTestData();
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.TaxOverrides[0].AO_CostSellAll = "CST"; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.TaxOverrides; }
			public string CollectionPropertyName { get { return "TaxOverrides"; } }
		}

		public class SupplyTypeOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.SupplyTypeOverrides.AddNew();
				o.ACS_JobType = o.JobTypeList[0].Code;
				o.ACS_Direction = o.DirectionList[0].Code;
				o.ACS_TransportMode = o.ModeList[0].Code;
				o.ACS_IncoTerm = o.IncotermList[0].Code;
				o.ACS_GE = GlbDepartment.CurrentDepartment.PK;
				o.ACS_SupplyType = o.SupplyTypeList[0].Code;
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.SupplyTypeOverrides[0].ACS_SupplyType = c.SupplyTypeOverrides[0].SupplyTypeList[1].Code; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.SupplyTypeOverrides; }
			public string CollectionPropertyName { get { return "SupplyTypeOverrides"; } }
		}

		public class GovtChargeCodeOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.GovtChargeCodeOverrides.AddNew();
				o.ACG_JobType = "ALL";
				o.ACG_Direction = "ALL";
				o.ACG_TransportMode = "ALL";
				o.ACG_GovtChargeCode = "123455";
				o.FillWithValidTestData();
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.GovtChargeCodeOverrides[0].ACG_GovtChargeCode = "123466"; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.GovtChargeCodeOverrides; }
			public string CollectionPropertyName { get { return "GovtChargeCodeOverrides"; } }
		}

		public class ApportionmentMethodOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.ApportionmentMethodOverrides.AddNew();
				o.FillWithValidTestData();
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.ApportionmentMethodOverrides[0].AAM_ApportionmentMethod = "SHP"; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.ApportionmentMethodOverrides; }
			public string CollectionPropertyName { get { return "ApportionmentMethodOverrides"; } }
		}

		public class BranchOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.BranchOverrides.AddNew();
				o.FillWithValidTestData();
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.BranchOverrides[0].YA_DefaultingRule = "ABC"; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.BranchOverrides; }
			public string CollectionPropertyName { get { return "BranchOverrides"; } }
		}

		public class ChargeComplianceDescriptionsHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.ChargeComplianceDescriptions.AddNew();
				o.ADE_JobType = "ALL";
				o.ADE_TransportMode = "ALL";
				o.ADE_SupplyType = "LOC";
				o.ADE_Description = "TEST123455";
				o.FillWithValidTestData();
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.ChargeComplianceDescriptions[0].ADE_Description = "TEST55"; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.ChargeComplianceDescriptions; }
			public string CollectionPropertyName { get { return "ChargeComplianceDescriptions"; } }
		}

		public class AirlineIATACodeOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.AccChargeCodeCarrierIataMappings.AddNew();
				o.FillWithValidTestData();
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.AccChargeCodeCarrierIataMappings[0].ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.AccChargeCodeCarrierIataMappings; }
			public string CollectionPropertyName { get { return "AccChargeCodeCarrierIataMappings"; } }
		}

		public class CreditorOverridesHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var o = c.CreditorOverrides.AddNew();
				o.FillWithValidTestData();
				o.ACC_TransportMode = "ALL";
				o.ACC_PaymentTerm = Constants.PaymentType.Prepaid;
				o.ACC_Direction = Constants.FreightShipmentDirection.Code.Import;
				o.ACC_OH_Creditor = ZGuid.Empty;
				o.ACC_CreditorRole = DocAddressTypes.Codes.OverseasAgent;
				return this;
			}
			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c) { c.CreditorOverrides[0].ACC_PaymentTerm = Constants.PaymentType.Collect; return this; }
			public IBusinessObjectCollection GetCollection(AccChargeCode c) { return c.CreditorOverrides; }
			public string CollectionPropertyName { get { return "CreditorOverrides"; } }
		}

		public class UniversalChargeCodeMappingsCollectionHelper : ITestGlobalVsLocalValidationOnLocalChargeCodeHelper
		{
			public string CollectionPropertyName => "UniversalChargeCodeMappingsCollection";

			public IBusinessObjectCollection GetCollection(AccChargeCode c)
			{
				return c.UniversalChargeCodeMappingsCollection;
			}

			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PokeLocalChargeCode(AccChargeCode c)
			{
				c.UniversalChargeCodeMappingsCollection[0].AUP_Code = "UNC";
				return this;
			}

			public ITestGlobalVsLocalValidationOnLocalChargeCodeHelper PopulateCollection(AccChargeCode c)
			{
				var mapping = c.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.FillWithValidTestData();
				return this;
			}
		}

		#endregion

		public void TestGlobalVsLocalAC_CodeValidation_Local_New()
		{
			Env.Security.ChargeCodesNew.IsAllowed = true;
			Env.Security.ChargeCodesLTGNew.IsAllowed = true;

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			normalChargeCodeLinked.Delete();
			Factory.Save();

			var newChargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var notifications = newChargeCode.AC_CodeInfo.GetWarnings();
			AssertEquals("Start off with no warnings.", 0, notifications.Count());
			notifications = newChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("Start off with no errors.", 0, notifications.Count());

			newChargeCode.AC_Code = globalChargeCode.AC_Code;

			newChargeCode.AC_Code = globalChargeCode.AC_Code;
			notifications = newChargeCode.AC_CodeInfo.GetWarnings();
			AssertEquals("Warning due to linking", 1, notifications.Count());
			AssertEquals("Warning message", "This change will link this code to the Global Charge Code 'CC2'", notifications.GetFirst().Message);

			Env.Security.ChargeCodesLTGNew.IsAllowed = false;
			newChargeCode.Validation.ValidateAll(); // Need to revalidate becuase no change has been made. [This is done by the charge code form]
			notifications = newChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("Error due to linking", 1, notifications.Count());
			AssertEquals("Error message", @"This change will link this code to the Global Charge Code 'CC2'. To do this you require the following security permission:

Maintain -> Account -> Charge Codes -> If Linked To Global Charge Code -> New", notifications.GetFirst().Message);
			Env.Security.ChargeCodesLTGNew.IsAllowed = true;
		}

		public void TestGlobalVsLocalAC_CodeValidation_Local_Existing()
		{
			Env.Security.ChargeCodesNew.IsAllowed = true;
			Env.Security.ChargeCodesLTGNew.IsAllowed = true;

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var notifications = normalChargeCode.AC_CodeInfo.GetWarnings();
			AssertEquals("Start off with no warnings.", 0, notifications.Count());
			notifications = normalChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("Start off with no errors.", 0, notifications.Count());

			normalChargeCodeLinked.AC_Code = "1";
			notifications = normalChargeCodeLinked.AC_CodeInfo.GetWarnings();
			AssertEquals("Warning due to unlinking", 1, notifications.Count());
			AssertEquals("Warning message", "This change will remove the link from this code to the Global Charge Code 'CC2'", notifications.GetFirst().Message);

			Env.Security.ChargeCodesNew.IsAllowed = false;
			normalChargeCodeLinked.Validation.ValidateAll(); // Need to revalidate becuase no change has been made. [This is done by the charge code form]
			notifications = normalChargeCodeLinked.AC_CodeInfo.GetErrors();
			AssertEquals("Error due to unlinking", 1, notifications.Count());
			AssertEquals("Error message", @"This change will remove the link from this code to the Global Charge Code 'CC2'.  To do this you require the following security permission:

Maintain -> Account -> Charge Codes -> New", notifications.GetFirst().Message);
			Env.Security.ChargeCodesNew.IsAllowed = true;

			Factory.Save();
			normalChargeCodeLinked.Validation.ValidateAll(); // Need to revalidate becuase no change has been made. [This is done by the charge code form]
			notifications = normalChargeCodeLinked.AC_CodeInfo.GetWarnings();
			AssertEquals("No warning due to no long being in a 'changing' state", 0, notifications.Count());

			normalChargeCode.AC_Code = globalChargeCode.AC_Code;
			notifications = normalChargeCode.AC_CodeInfo.GetWarnings();
			AssertEquals("Warning due to linking", 1, notifications.Count());
			AssertEquals("Warning message", "This change will link this code to the Global Charge Code 'CC2'", notifications.GetFirst().Message);

			Env.Security.ChargeCodesLTGNew.IsAllowed = false;
			normalChargeCode.Validation.ValidateAll(); // Need to revalidate becuase no change has been made. [This is done by the charge code form]
			notifications = normalChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("Error due to linking", 1, notifications.Count());
			AssertEquals("Error message", @"This change will link this code to the Global Charge Code 'CC2'. To do this you require the following security permission:

Maintain -> Account -> Charge Codes -> If Linked To Global Charge Code -> New", notifications.GetFirst().Message);
			Env.Security.ChargeCodesLTGNew.IsAllowed = true;

			Factory.Save();
			normalChargeCode.Validation.ValidateAll(); // Need to revalidate becuase no change has been made. [This is done by the charge code form]
			notifications = normalChargeCode.AC_CodeInfo.GetWarnings();
			AssertEquals("No warning due to no long being in a 'changing' state", 0, notifications.Count());
		}

		public void TestGlobalVsLocalAC_CodeValidation_Global()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var errors = normalChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("Start off with no warnings.", 0, errors.Count());

			globalChargeCode.AC_Code = "1";
			errors = globalChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("It is ok to change the code normally.", 0, errors.Count());

			globalChargeCode.AC_Code = normalChargeCode.AC_Code;
			errors = globalChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("But you can't change it to one that will clash", 1, errors.Count());
			AssertEquals("Error message", "There is a charge code in company 'Eagle Datamation International' with the Code 'CC1'. Please choose another Code.", errors.GetFirst().Message);

			globalChargeCode.AllowCodeToMatchExisting = true;
			globalChargeCode.Validation.ValidateAll();
			errors = globalChargeCode.AC_CodeInfo.GetErrors();
			AssertEquals("No error now because AllowCodeToMatchExisting is set", 0, errors.Count());
		}

		public void TestValidateAC_ChargeSubGroup_Global()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var warehouseJobServicesGlobal = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServicesGlobal.Add("BAD", (NoResString)"Bad", false);
			warehouseJobServicesGlobal.Add("GUD", (NoResString)"Good", false);
			warehouseJobServicesGlobal.SetDefaultCode("GUD", true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, warehouseJobServicesGlobal);

			var warehouseJobServicesCompany = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServicesCompany.Add("GUD", (NoResString)"Good", false);
			warehouseJobServicesCompany.SetDefaultCode("GUD", true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, warehouseJobServicesCompany);

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			globalChargeCode.AC_ChargeGroup = "WIN";
			globalChargeCode.AC_ChargeSubGroup = "GUD";
			globalChargeCode.Validation.ValidateAll();
			var errors = globalChargeCode.AC_ChargeSubGroupInfo.GetErrors();
			AssertEquals("No error", 0, errors.Count());

			globalChargeCode.AC_ChargeSubGroup = "BAD";
			globalChargeCode.Validation.ValidateAll();
			errors = globalChargeCode.AC_ChargeSubGroupInfo.GetErrors();
			AssertEquals("Error due to not being abailable in company", 1, errors.Count());
			AssertEquals("Error due to not being abailable in company", "This Service Type is not valid in company 'EDI' (Eagle Datamation International)", errors.GetFirst().Message);
		}

		public void TestValidateAC_IsAdhocServiceCharge()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.Validation.ValidateAll();

			AssertEquals("Pre-condition: defaults to false", false, chargeCode.AC_IsAdhocServiceCharge);
			AssertNoErrors("No errors expected", chargeCode.AC_IsAdhocServiceChargeInfo);

			chargeCode.AC_IsAdhocServiceCharge = true;

			AssertHasErrors("Cannot be selected without Charge Group and SubGroup", chargeCode.AC_IsAdhocServiceChargeInfo);

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			AssertNoErrors("No errors expected", chargeCode.AC_IsAdhocServiceChargeInfo);

			chargeCode.AC_ChargeSubGroup = "";

			AssertHasErrors("Missing sub charge group", chargeCode.AC_IsAdhocServiceChargeInfo);

			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Washing;

			AssertNoErrors("No errors expected", chargeCode.AC_IsAdhocServiceChargeInfo);
		}

		public void TestValidateAC_IsAdhocServiceCharge_UniquePerCompanyChargeGroupAndSubChargeGroup()
		{
			AccChargeCodeTest.EnsureAllGSTRegisteredCompaniesHaveRatedGST(Factory);

			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "SRVCHRG1";
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode1.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode1.AC_IsAdhocServiceCharge = true;

			AssertNoErrors("No errors expected", chargeCode1.AC_IsAdhocServiceChargeInfo);

			Factory.Save();

			var expectedError = ZString.Format(@"Only one charge code can be selected as the Ad Hoc Service Charge per Company, Charge Group and Service Type combination. Please either clear this tick or all of the following");

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "SRVCHRG2";
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode2.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode2.AC_IsAdhocServiceCharge = true;

			AssertHasErrorContaining(chargeCode2.AC_IsAdhocServiceChargeInfo, expectedError);
			AssertExceptionThrown("Unique Index (NR_UX__AC_GC_AC_ChargeGroup_AC_ChargeSubGroup) won't allow this. Validation should always ensure users don't see this message", typeof(ZSaveException), () => Factory.Save());

			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			AssertNoErrors("No longer violates unique index, should be valid and saveable", chargeCode2.AC_IsAdhocServiceChargeInfo);
			Factory.Save();

			var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			var anotherBranch = Factory.LoadTop1<GlbBranch>(branchQuery);
			var context = new TemporaryUserContext { BranchPK = anotherBranch.PK.ToGuid() };

			using (context.Set())
			{
				var chargeCode3 = Factory.New<AccChargeCode>();
				chargeCode3.AC_Code = "SRVCHRG3";
				chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
				chargeCode3.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
				chargeCode3.AC_IsAdhocServiceCharge = true;

				AssertNoErrors("In another company and no global charge exists so no error expected.", chargeCode3.AC_IsAdhocServiceChargeInfo);
				Factory.Save();
			}

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_ChargeType = Constants.ChargeType.ManualJobAccrual;
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "GLBSRV1";
			globalChargeCode.AC_Desc = "Global Service Charge";
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			globalChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			globalChargeCode.AC_IsAdhocServiceCharge = true;

			AssertNoErrors("This isn't ideal but when the users populate normal charge codes from global charge codes, it'll fail", globalChargeCode.AC_IsAdhocServiceChargeInfo);
			AssertExceptionThrown("Unique Index (NR_UX__AC_GC_AC_ChargeGroup_AC_ChargeSubGroup) won't allow this", typeof(AccChargeCode.ValidationOnLocalChargeCodeException), () => Factory.Save());

			globalChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.ExtraInspection;

			AssertNoErrors("Global charges can be saved as Ad Hoc Service Charge if they don't violate any unique indexes", globalChargeCode.AC_IsAdhocServiceChargeInfo);

			var globalChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode2.AC_ChargeType = Constants.ChargeType.ManualJobAccrual;
			globalChargeCode2.AC_GC = ZGuid.Empty;
			globalChargeCode2.AC_Code = "GLBSRV2";
			globalChargeCode2.AC_Desc = "Global Service Charge Again";
			globalChargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			globalChargeCode2.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.ExtraInspection;
			globalChargeCode2.AC_IsAdhocServiceCharge = true;

			var duplicateGlobalErrorMessage = @"Only one charge code can be selected as the Ad Hoc Service Charge per Company, Charge Group and Service Type combination. Please either clear this tick or all of the following:
- Global Charge Code 'GLBSRV1'";

			AssertHasErrorContaining(globalChargeCode2.AC_IsAdhocServiceChargeInfo, duplicateGlobalErrorMessage);
		}

		public void TestValidateAC_GoodsServiceType()
		{
			Action<ZPropertyInfo, ZString> assert = (property, validValue) =>
			{
				property.Value = ZString.Empty;
				var expectedError = "Please enter a Class.";
				AssertHasError(property, expectedError);

				using (TestChargeCode.GetValidationSuspender())
				{
					property.Value = (ZString)"WWW";
				}
				AssertHasError("The property should not be revalidated as validation was suspended.", property, expectedError);

				TestChargeCode.RunPreSaveValidation();
				AssertHasError(property, "Enter a valid Class.");

				property.Value = validValue;
				AssertNoErrors(property);
			};

			foreach (CodeDescriptionPair value in TestChargeCode.Lookups.GoodServiceTypes)
			{
				assert(TestChargeCode.AC_GoodsServiceTypeInfo, value.Code);
			}
		}

		protected AccChargeCode TestChargeCode;

		public void TestValidateAllChangesContextIfNecessary()
		{
			#region Getting data for test

			var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Env.CurrentCompany.Country.Code);
			taxQuery.AddToFilter(AccTaxRateSchema.AT_IsActive, true);
			var taxRate1 = Factory.LoadTop1<AccTaxRate>(taxQuery);
			AssertNotNull("taxRate1", taxRate1);
			taxQuery.AddToFilter(AccTaxRateSchema.PK, SQLComparisonOperator.NotEqual, taxRate1.PK);
			var taxRate2 = Factory.LoadTop1<AccTaxRate>(taxQuery);
			AssertNotNull("taxRate2", taxRate2);

			var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.DoesNotStartWith, Env.CurrentCompany.Country.Code);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

			var differentBranch = Factory.LoadTop1<GlbBranch>(branchQuery);
			AssertNotNull("differentBranch", differentBranch);

			#endregion

			#region Set up test data

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();

			TestChargeCode.AC_GC = Env.CurrentCompany.PK;
			TestChargeCode.AC_Code = "TST";
			TestChargeCode.AC_Desc = "Test Charge";
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			TestChargeCode.AC_AT_GSTRate = taxRate1.PK;
			TestChargeCode.AC_AG_AccrualAccount = glHeader.PK;
			TestChargeCode.AC_AG_CostAccount = glHeader.PK;
			TestChargeCode.AC_AG_RevenueAccount = glHeader.PK;
			TestChargeCode.AC_AG_WIPAccount = glHeader.PK;

			AccChargeTaxOverride taxOverride = TestChargeCode.TaxOverrides.AddNew();
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_TransportMode = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_AT = taxRate2.PK;

			TestChargeCode.RunPreSaveValidation();
			AssertEquals("Has no errors", false, TestChargeCode.HasErrors);
			Factory.Save();

			#endregion

			using (new TemporaryUserContext() { BranchPK = differentBranch.PK.ToGuid() }.Set())
			{
				var testChargeCode = new BusinessObjectFactory().Load<AccChargeCode>(TestChargeCode.PK);
				testChargeCode.RunPreSaveValidation();
				AssertEquals("One Tax Override", 1, testChargeCode.TaxOverrides.Count);
				AssertNoErrors("No errors expected on Tax Rate", testChargeCode.TaxOverrides[0].AO_ATInfo);
				AssertNoErrors("No errors expected on Tax Rate", testChargeCode.AC_AT_GSTRateInfo);
				AssertNoErrors("Has no errors caused by CurrentCompany different to the Context Company", testChargeCode);
			}
		}

		public void TestValidateAllChangesContextIfNecessaryNoExceptionForNewCompany()
		{
			#region Getting data for test

			var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Env.CurrentCompany.Country.Code);
			taxQuery.AddToFilter(AccTaxRateSchema.AT_IsActive, true);
			var taxRate1 = Factory.LoadTop1<AccTaxRate>(taxQuery);
			AssertNotNull("taxRate1", taxRate1);
			taxQuery.AddToFilter(AccTaxRateSchema.PK, SQLComparisonOperator.NotEqual, taxRate1.PK);
			var taxRate2 = Factory.LoadTop1<AccTaxRate>(taxQuery);
			AssertNotNull("taxRate2", taxRate2);

			#endregion

			#region Set up test data
			var newFactory = new BusinessObjectFactory();
			var newCompany = newFactory.New<GlbCompany>();
			newCompany.GC_IsActive = true;
			newCompany.GC_RN_NKCountryCode = "UA";
			newCompany.GC_Code = "XYZ";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_IsActive = true;
			newBranch.GB_Code = "XYZ";
			newFactory.Save();

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();

			TestChargeCode.AC_GC = newCompany.PK;
			TestChargeCode.AC_Code = "TST";
			TestChargeCode.AC_Desc = "Test Charge";
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			TestChargeCode.AC_AT_GSTRate = taxRate1.PK;
			TestChargeCode.AC_AG_AccrualAccount = glHeader.PK;
			TestChargeCode.AC_AG_CostAccount = glHeader.PK;
			TestChargeCode.AC_AG_RevenueAccount = glHeader.PK;
			TestChargeCode.AC_AG_WIPAccount = glHeader.PK;

			AccChargeTaxOverride taxOverride = TestChargeCode.TaxOverrides.AddNew();
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_AT = taxRate2.PK;

			#endregion

			AssertNoExceptionThrown(TestChargeCode.RunPreSaveValidation);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestValidateAC_ChargeType_IsChargeTypeValidIfPreviousTypeIsRevenue()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);
			Factory.Save();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = TestChargeCode.PK;
			line.AL_GC = TestChargeCode.AC_GC;

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was REV, new charge type must be MRG, DSB or MJA because there are accrued / actual revenue lines associated with this charge code.");
		}

		public void TestValidateAC_ChargeType_IsChargeTypeValidIfPreviousTypeIsOverhead()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);
			Factory.Save();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = TestChargeCode.PK;
			line.AL_GC = TestChargeCode.AC_GC;

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was OVR, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs related to this Charge Code");
		}

		public void TestValidateAC_ChargeType_IsChargeTypeValidIfPreviousTypeIsNonAccrual()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);
			Factory.Save();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line.AL_AC = TestChargeCode.PK;
			line.AL_GC = TestChargeCode.AC_GC;

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was NON, charge type must be MRG, DSB, NON or MJA because there are actual costs and revenues associated with this Charge Code");

			line.AL_LineType = "CST";
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was NON, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs associated with this Charge Code");

			line.AL_LineType = "REV";
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was NON, charge type must be MRG, DSB, NON, REV or MJA because there is actual revenue associated with this Charge Code");
		}

		public void TestValidateAC_ChargeType_IsChargeTypeValidIfPreviousTypeIsDisbursement()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);
			Factory.Save();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = TestChargeCode.PK;
			line.AL_GC = TestChargeCode.AC_GC;

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was DSB and cannot be changed because there are posted charges relating to operation job(s)");
		}

		public void TestValidateAC_ChargeType_IsChargeTypeValidIfPreviousTypeIsMargin()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);
			Factory.Save();

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = TestChargeCode.PK;
			line.AL_GC = TestChargeCode.AC_GC;
			line.AL_JH = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MRG, charge type must be MRG, DSB or MJA because there are accrued/actual costs and revenue associated with this Charge Code");

			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MRG, charge type must be MRG, DSB, REV or MJA because there is actual revenue associated with this Charge Code");

			line.AL_JH = ZGuid.Empty;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MRG, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs associated with this Charge Code");

			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MRG, type must be MRG, DSB, NON or MJA because there are actual/accrued costs or revenue associated with this Charge Code");
		}

		public void TestValidateAC_ChargeType_IsChargeTypeValidPreviousTypeIsManualJobAccrual()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			AssertNoErrors(TestChargeCode.AC_ChargeTypeInfo);
			Factory.Save();

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = TestChargeCode.PK;
			line.AL_GC = TestChargeCode.AC_GC;
			line.AL_JH = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MJA, charge type must be MRG, DSB or MJA because there are accrued/actual costs and revenue associated with this Charge Code");

			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MJA, charge type must be MRG, DSB, REV or MJA because there is actual revenue associated with this Charge Code");

			line.AL_JH = ZGuid.Empty;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MJA, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs associated with this Charge Code");

			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.RunPreSaveValidation();
			AssertHasError(TestChargeCode.AC_ChargeTypeInfo, "Previous type was MJA, type must be MRG, DSB, NON or MJA because there are actual/accrued costs or revenue associated with this Charge Code");
		}

		public void TestHelperMethods()
		{
			Action<string, bool> assertGetter = (name, value) =>
			{
				var result = (bool)TestChargeCode.Validation.GetType()
				.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(TestChargeCode.Validation, null);
				AssertEquals(value, result);
			};

			Action<string, string, bool> assertMethod = (name, param, value) =>
			{
				var result = (bool)TestChargeCode.Validation.GetType()
				.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(TestChargeCode.Validation, new object[] { param });
				AssertEquals(value, result);
			};

			//IsChargeCodeUsedByLinesAlready
			assertGetter("IsChargeCodeUsedByLinesAlready", false);
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			assertGetter("IsChargeCodeUsedByLinesAlready", false);
			line.AL_AC = TestChargeCode.PK;
			assertGetter("IsChargeCodeUsedByLinesAlready", false);
			line.AL_GC = TestChargeCode.AC_GC;
			assertGetter("IsChargeCodeUsedByLinesAlready", true);

			//IsChargeCodeUsedByLinesWithJobsAlready
			line.AL_AC = ZGuid.Empty;
			line.AL_GC = ZGuid.Empty;
			assertGetter("IsChargeCodeUsedByLinesWithJobsAlready", false);
			line.AL_AC = TestChargeCode.PK;
			assertGetter("IsChargeCodeUsedByLinesWithJobsAlready", false);
			line.AL_GC = TestChargeCode.AC_GC;
			assertGetter("IsChargeCodeUsedByLinesWithJobsAlready", false);
			line.AL_JH = ZGuid.NewZGuid();
			assertGetter("IsChargeCodeUsedByLinesWithJobsAlready", true);

			//IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready
			line.AL_AC = ZGuid.Empty;
			line.AL_GC = ZGuid.Empty;
			line.AL_JH = ZGuid.Empty;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", false);
			line.AL_AC = TestChargeCode.PK;
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", false);
			line.AL_GC = TestChargeCode.AC_GC;
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", false);
			line.AL_JH = ZGuid.NewZGuid();
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", false);
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", true);
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", true);
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			assertGetter("IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready", true);

			//IsChargeCodeUsedByLinesButThisLineTypeOnly
			line.AL_AC = ZGuid.Empty;
			line.AL_GC = ZGuid.Empty;
			line.AL_JH = ZGuid.Empty;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			assertMethod("IsChargeCodeUsedByLinesButThisLineTypeOnly", "CST", false);
			line.AL_AC = TestChargeCode.PK;
			assertMethod("IsChargeCodeUsedByLinesButThisLineTypeOnly", "CST", false);
			line.AL_GC = TestChargeCode.AC_GC;
			assertMethod("IsChargeCodeUsedByLinesButThisLineTypeOnly", "CST", true);

			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			assertMethod("IsChargeCodeUsedByLinesButThisLineTypeOnly", "CST", false);
			assertMethod("IsChargeCodeUsedByLinesButThisLineTypeOnly", "REV", true);
		}

		public void TestCodeOfChargeCodeCannotBeChangedInPortugalCompanyOnceATransactionIsPosted()
		{
			var ptBranch1 = CreateCompanyWithBranch(Constants.CountryCodes.Portugal);
			var ptBranch2 = CreateCompanyWithBranch(Constants.CountryCodes.Portugal);
			CreateChargeCode(ZGuid.Empty, ("CC1", "CC1 Description"), ("CC2", "CC2 Description"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cc1 = LoadChargeCode("CC1", ptBranch1.GB_GC, new BusinessObjectFactory());

				CreateARInvoiceWithLine(cc1);

				cc1.AC_Code = "CC1new";
				AssertHasError(cc1.AC_CodeInfo, "You cannot edit this code. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code.");
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cc1 = LoadChargeCode("CC1", ptBranch2.GB_GC);
				var cc2 = LoadChargeCode("CC2", ptBranch2.GB_GC);

				cc1.AC_Code = "CC1new2";
				AssertNoErrors("no error expected if transaction are posted in another company with this charge code", cc1.AC_CodeInfo);

				CreateARInvoiceWithLine(cc1);

				var cc1Reload = new BusinessObjectFactory().Load<AccChargeCode>(cc1.PK);
				cc1Reload.AC_Code = "CC1new3";
				AssertHasError(cc1Reload.AC_CodeInfo, "You cannot edit this code. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code.");

				var cc2Reload = new BusinessObjectFactory().Load<AccChargeCode>(cc2.PK);
				cc2Reload.AC_Code = "CC2new";
				AssertNoErrors(cc2Reload.AC_CodeInfo);
			}

			AssertNotEquals("PreCond: current company is not Portugal", Constants.CountryCodes.Portugal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cc1AU = LoadChargeCode("CC1", GlbCompany.CurrentCompany.PK);
			CreateARInvoiceWithLine(cc1AU);
			cc1AU.AC_Code = "CC1new";
			AssertNoErrors(cc1AU.AC_CodeInfo);
		}

		public void TestDescriptionOfChargeCodeCannotBeChangedInPortugalCompanyOnceATransactionIsPosted()
		{
			var ptBranch = CreateCompanyWithBranch(Core.Constants.CountryCodes.Portugal);
			CreateChargeCode(ZGuid.Empty, ("CC1", "CC1 Description"), ("CC2", "CC2 Description"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cc1 = LoadChargeCode("CC1", ptBranch.GB_GC);
				var cc2 = LoadChargeCode("CC2", ptBranch.GB_GC);

				CreateARInvoiceWithLine(cc1);

				cc1.AC_Desc = "Description Modified";
				AssertHasError(cc1.AC_DescInfo, "You cannot edit this description. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code.");

				cc2.AC_Desc = "Description Modified";
				AssertNoErrors(cc2.AC_DescInfo);
			}
		}

		public void TestChangeInTheDescriptionOfGlobalChargeCodeDoesNotTriggerChangedInPortugalCompanyChargeCodeOnceATransactionIsPosted()
		{
			var ptBranch = CreateCompanyWithBranch(Core.Constants.CountryCodes.Portugal);

			CreateChargeCode(ZGuid.Empty, ("CC1", "CC1 Description"), ("CC2", "CC2 Description"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cc1 = LoadChargeCode("CC1", ptBranch.GB_GC);
				var cc2 = LoadChargeCode("CC2", ptBranch.GB_GC);
				var cc1Gbl = LoadChargeCode("CC1", ZGuid.Empty);
				var cc2Gbl = LoadChargeCode("CC2", ZGuid.Empty);

				CreateARInvoiceWithLine(cc1);

				cc1Gbl.AC_Desc = "Description Modified CC1";
				cc2Gbl.AC_Desc = "Description Modified CC2";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var cc1GblInNewFactory = LoadChargeCode("CC1", ptBranch.GB_GC, newFactory);

				foreach (var localChargeCode in cc1GblInNewFactory.ChildChargeCodes)
				{
					if (localChargeCode.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Portugal)
					{
						AssertEquals("CC1 Description", cc1.AC_Desc);
					}
					else
					{
						AssertEquals("Description Modified CC1", cc1.AC_Desc);
					}
				}

				AssertEquals("Description Modified CC2", cc2.AC_Desc);
			}
		}

		public void TestAccChargeCodeValidationCacheIsNotUsedInPreSaveValidation()
		{
			var ptBranch = CreateCompanyWithBranch(Core.Constants.CountryCodes.Portugal);
			CreateChargeCode(ZGuid.Empty, ("CC1", "CC1 Description"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cc1 = LoadChargeCode("CC1", ptBranch.GB_GC);
				var cc1InNewFactory = LoadChargeCode("CC1", ptBranch.GB_GC, new BusinessObjectFactory());

				cc1InNewFactory.AC_Desc = "Description Modified in a new factory";
				AssertNoErrors("No Error, as no transaction is posted yet", cc1InNewFactory.AC_DescInfo);

				CreateARInvoiceWithLine(cc1);

				cc1InNewFactory.AC_Desc = "Description Modified in a new factory";
				AssertNoErrors("Still no error, as validation process is pulling data from cache", cc1InNewFactory.AC_DescInfo);

				cc1InNewFactory.RunPreSaveValidation();
				AssertHasError("Now error should be reported, as PreSaveValidation process does not use cache", cc1InNewFactory.AC_DescInfo, "You cannot edit this description. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code.");
			}
		}

		void CreateChargeCode(ZGuid companyPK, params (ZString code, ZString desc)[] chargeCodesWithDescription)
		{
			foreach (var chargeCodeWithDescription in chargeCodesWithDescription)
			{
				var ccGbl = Factory.NewWithValidTestData<AccChargeCode>();
				ccGbl.AC_Code = chargeCodeWithDescription.code;
				ccGbl.AC_Desc = chargeCodeWithDescription.desc;
				ccGbl.AC_GC = companyPK;
				ccGbl.AC_ChargeType = "MRG";
			}
			Factory.Save();
		}

		AccChargeCode LoadChargeCode(ZString code, ZGuid companyPK, BusinessObjectFactory newFactory = null)
		{
			var ccQuery = new ZQuery(AccChargeCodeSchema.AC_Code, code);
			ccQuery.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK.IsValid ? companyPK : null);
			var cc = (newFactory ?? Factory).LoadTop1<AccChargeCode>(ccQuery);
			return cc;
		}

		void CreateARInvoiceWithLine(AccChargeCode chargeCode)
		{
			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;

			var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine.AL_AH = arInvoice.PK;
			arInvoiceLine.AL_AT = ZGuid.Empty;
			arInvoiceLine.AL_AC = chargeCode.PK;
			arInvoiceLine.AL_LineAmount = 44;

			Factory.Save();
		}

		GlbBranch CreateCompanyWithBranch(ZString companyCountryCode)
		{
			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = companyCountryCode;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;

			Factory.Save();

			return ptBranch;
		}

		public void TestCheckAC_AG_DisbursementSurplusAccount()
		{
			var profitAndLossGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			profitAndLossGLHeader.AG_AccountType = Constants.AccountType.ProfitAndLossAccount;
			var balanceSheetAccountGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			balanceSheetAccountGLHeader.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			var noteAccountGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			noteAccountGLHeader.AG_AccountType = Constants.AccountType.Note;
			Factory.Save();

			TestChargeCode.AC_AG_DisbursementSurplusAccount = profitAndLossGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);
			TestChargeCode.AC_AG_DisbursementSurplusAccount = noteAccountGLHeader.PK;
			AssertHasErrors("Enter a valid Disbursement Surplus GL Account.", TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);
			TestChargeCode.AC_AG_DisbursementSurplusAccount = balanceSheetAccountGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);

			TestChargeCode.AC_AG_DisbursementShortfallAccount = profitAndLossGLHeader.PK;

			TestChargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			TestChargeCode.AC_AG_DisbursementSurplusAccount = ZGuid.Empty;
			AssertHasErrors("This GL Account must be specified if either Disbursement Surplus/ Shortfall GL Account has been specified.", TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);

			TestChargeCode.AC_AG_DisbursementSurplusAccount = profitAndLossGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);

			TestChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			TestChargeCode.AC_AG_DisbursementSurplusAccount = ZGuid.Empty;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);

			TestChargeCode.AC_AG_DisbursementSurplusAccount = profitAndLossGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo);
		}

		public void TestCheckAC_AG_DisbursementShortfallAccount()
		{
			var profitAndLossGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			profitAndLossGLHeader.AG_AccountType = Constants.AccountType.ProfitAndLossAccount;
			var balanceSheetAccountGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			balanceSheetAccountGLHeader.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			var noteAccountGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			noteAccountGLHeader.AG_AccountType = Constants.AccountType.Note;
			Factory.Save();

			TestChargeCode.AC_AG_DisbursementShortfallAccount = profitAndLossGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);
			TestChargeCode.AC_AG_DisbursementShortfallAccount = noteAccountGLHeader.PK;
			AssertHasErrors("Enter a valid Disbursement Shortfall GL Account.", TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);
			TestChargeCode.AC_AG_DisbursementShortfallAccount = balanceSheetAccountGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);

			TestChargeCode.AC_AG_DisbursementSurplusAccount = profitAndLossGLHeader.PK;

			TestChargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			TestChargeCode.AC_AG_DisbursementShortfallAccount = ZGuid.Empty;
			AssertHasErrors("This GL Account must be specified if either Disbursement Surplus/ Shortfall GL Account has been specified.", TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);

			TestChargeCode.AC_AG_DisbursementShortfallAccount = profitAndLossGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);

			TestChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			TestChargeCode.AC_AG_DisbursementShortfallAccount = ZGuid.Empty;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);

			TestChargeCode.AC_AG_DisbursementShortfallAccount = profitAndLossGLHeader.PK;
			AssertNoErrors(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo);
		}

		public void TestCheckAC_AG_DisbursementSurplusAccount_NotAllowEdit()
		{
			TestAccChargeCodeDisbursementAccountsNotAllowEditCore(TestChargeCode.AC_AG_DisbursementSurplusAccountInfo,
				"Cannot edit Disbursement Surplus GL Account because relative DSB Job Close batch was created. The DSB Surplus Account must be set to '{0}'.");
		}

		public void TestCheckAC_AG_DisbursementShortfallAccount_NotAllowEdit()
		{
			TestAccChargeCodeDisbursementAccountsNotAllowEditCore(TestChargeCode.AC_AG_DisbursementShortfallAccountInfo,
				"Cannot edit Disbursement Shortfall GL Account because relative DSB Job Close batch was created. The DSB Shortfall Account must be set to '{0}'.");
		}

		void TestAccChargeCodeDisbursementAccountsNotAllowEditCore(ZPropertyInfo disbursementAccountInfo, string expectedErrorMsg)
		{
			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader1.AG_AccountType = Constants.AccountType.ProfitAndLossAccount;
			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader2.AG_AccountType = Constants.AccountType.ProfitAndLossAccount;
			Factory.Save();

			TestChargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			TestChargeCode.AC_AG_DisbursementShortfallAccount = glHeader1.PK;
			TestChargeCode.AC_AG_DisbursementSurplusAccount = glHeader1.PK;
			Factory.Save();

			AssertNoErrors(disbursementAccountInfo);

			var batchId = CreateDsbJobCloseBatch();

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "WU6NQJXWCZQQSS7YWYHT8M4APKDLQULUHT2";

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = job.PK;
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_JH = job.PK;
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			line.AL_AG = glHeader.PK;
			line.AL_AC = TestChargeCode.PK;
			line.AL_JBB = batchId;
			line.AL_LineType = "CST";
			line.AL_JH = job.PK;
			line.AL_PostDate = ZDateTime.Today;
			charge1.JR_AL_APLine = line.PK;
			Factory.Save();

			disbursementAccountInfo.Value = glHeader2.PK;

			AssertEquals("Precondition", true, disbursementAccountInfo.HasChanges);
			AssertHasError(disbursementAccountInfo, string.Format(CultureInfo.InvariantCulture, expectedErrorMsg, glHeader1.AccountNum));
		}

		(AccChargeCode globalChargeCode, AccChargeCode localChargeCode) CreateGlobalChargeCodeAndLocalChargeCode()
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Desc = "Global DSB Charge Code";
			globalChargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			globalChargeCode.AC_Code = "test";

			Factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_Code == "test");

			return (globalChargeCode, localChargeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
		}

		Guid CreateDsbJobCloseBatch()
		{
			var batchId = Guid.NewGuid();

			Db.Connection.ExecuteNonQuery(@"
INSERT INTO [DsbJobCloseBatch]
([JBB_PK]
,[JBB_BatchNumber]
,[JBB_GC]
,[JBB_BatchStatus]
,[JBB_SystemCreateTimeUtc]
,[JBB_SystemCreateUser]
,[JBB_SystemLastEditTimeUtc]
,[JBB_SystemLastEditUser])
VALUES
(@BatchPK
,'B001'
,@CompanyPK
,'OPN'
,getdate()
,'E'
,getdate()
,'E')",
			(x) =>
			{
				x.AddParameter("@BatchPK", SqlDbType.UniqueIdentifier, batchId);
				x.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			});

			return batchId;
		}
	}
}
