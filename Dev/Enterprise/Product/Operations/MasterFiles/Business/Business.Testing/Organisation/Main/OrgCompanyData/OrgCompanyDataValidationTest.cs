using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using AuthorisationRequirementCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgCompanyDataValidationTest : BusinessObjectValidationTestCase
	{
		#region Tax Configuration Template

		public void TestCheckOB_OCT_ARTaxTemplate()
		{
			var templateAR = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAR.OCT_IsReceivable = true;

			var templateARUnactive = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateARUnactive.OCT_IsReceivable = true;
			templateARUnactive.OCT_IsActive = false;

			var templateAP = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAP.OCT_IsReceivable = false;
			Factory.Save();

			CompanyData.OB_OCT_ARTaxTemplate = templateARUnactive.PK;
			AssertHasError(CompanyData.OB_OCT_ARTaxTemplateInfo, "This AR Tax Configuration Template is inactive - it may not be used.");

			CompanyData.OB_OCT_ARTaxTemplate = templateAR.PK;
			AssertNoErrors(CompanyData.OB_OCT_ARTaxTemplateInfo);

			CompanyData.OB_OCT_ARTaxTemplate = templateAP.PK;
			AssertHasError(CompanyData.OB_OCT_ARTaxTemplateInfo, "Enter a valid AR Tax Configuration Template.");
		}

		public void TestCheckOB_OCT_APTaxTemplate()
		{
			var templateAR = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAR.OCT_IsReceivable = true;

			var templateAP = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAP.OCT_IsReceivable = false;

			var templateAPUnactive = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAPUnactive.OCT_IsReceivable = false;
			templateAPUnactive.OCT_IsActive = false;

			CompanyData.OB_OCT_APTaxTemplate = templateAPUnactive.PK;
			AssertHasError(CompanyData.OB_OCT_APTaxTemplateInfo, "This AP Tax Configuration Template is inactive - it may not be used.");

			CompanyData.OB_OCT_APTaxTemplate = templateAP.PK;
			AssertNoErrors(CompanyData.OB_OCT_APTaxTemplateInfo);

			CompanyData.OB_OCT_APTaxTemplate = templateAR.PK;
			AssertHasError(CompanyData.OB_OCT_APTaxTemplateInfo, "Enter a valid AP Tax Configuration Template.");
		}

		#endregion

		public void TestCheckOB_RX_NKAPDefltCurrency()
		{
			CompanyData.OB_IsCreditor = true;
			CompanyData.OB_RX_NKAPDefltCurrency = "AUD";
			AssertNoErrors(CompanyData.OB_RX_NKAPDefltCurrencyInfo);

			CompanyData.OB_RX_NKAPDefltCurrency = "XXX";
			AssertHasErrors(CompanyData.OB_RX_NKAPDefltCurrencyInfo);

			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_RX_NKAPDefltCurrency = "XXX";
			AssertNoErrors(CompanyData.OB_RX_NKAPDefltCurrencyInfo);
		}

		public void TestCheckOB_RX_NKARDDefltCurrency()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_RX_NKARDDefltCurrency = "AUD";
			AssertNoErrors(CompanyData.OB_RX_NKARDDefltCurrencyInfo);

			CompanyData.OB_RX_NKARDDefltCurrency = "XXX";
			AssertHasErrors(CompanyData.OB_RX_NKARDDefltCurrencyInfo);

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_RX_NKARDDefltCurrency = "XXX";
			AssertNoErrors(CompanyData.OB_RX_NKARDDefltCurrencyInfo);
		}

		public void TestCheckOB_ARConsolidatedAccountingCategory()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			AssertNoErrors(CompanyData.OB_ARConsolidatedAccountingCategoryInfo);

			CompanyData.OB_ARConsolidatedAccountingCategory = "XXX";
			AssertHasErrors(CompanyData.OB_ARConsolidatedAccountingCategoryInfo);

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARConsolidatedAccountingCategory = "XXX";
			AssertNoErrors(CompanyData.OB_ARConsolidatedAccountingCategoryInfo);
		}

		public void TestExternalDebtorCodeValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Is Debtor shouldn't be true", !org.OH_IsDebtor);
			org.OH_IsCreditor = true;
			org.RunPreSaveValidation();
			Assert("Shouldn't be an error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());
			org.OH_IsDebtor = true;
			org.RunPreSaveValidation();
			Assert("Still shouldn't be an error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());
			OrganisationsDataRegistry.Instance.MakeExternalDebtorCodeMandatory.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			org.RunPreSaveValidation();
			Assert("Should be an error on external debtor code", org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			org.CompanyData.OB_ARExternalDebtorCode = "TEST123";
			org.RunPreSaveValidation();
			Assert("Should be no error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			org.CompanyData.OB_ARExternalDebtorCode = ZString.Empty;
			org.RunPreSaveValidation();
			Assert("Should now be an error on external debtor code", org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, "TEST4321");
			org.RunPreSaveValidation();
			Assert("Should now be no error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			OrganisationsDataRegistry.Instance.MakeExternalDebtorCodeMandatory.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false);
			org.CustomsCodes.RemoveAndDelete(org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, GlbCompany.CurrentCompany.Country));
			org.RunPreSaveValidation();
			Assert("Should now be no error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			OrganisationsDataRegistry.Instance.MakeExternalDebtorCodeMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			org.RunPreSaveValidation();
			Assert("Should be an error on external debtor code", org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			org.CompanyData.OB_ARExternalDebtorCode = "TEST123";
			org.RunPreSaveValidation();
			Assert("Should be no error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			org.CompanyData.OB_ARExternalDebtorCode = ZString.Empty;
			org.RunPreSaveValidation();
			Assert("Should now be an error on external debtor code", org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, "TEST4321");
			org.RunPreSaveValidation();
			Assert("Should now be no error on external debtor code", !org.CompanyData.OB_ARExternalDebtorCodeInfo.HasErrors());
		}

		public void TestExternalCreditorCodeValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Is Debtor shouldn't be true", !org.OH_IsDebtor);
			org.OH_IsCreditor = true;
			org.RunPreSaveValidation();
			Assert("Shouldn't be an error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());
			org.OH_IsDebtor = true;
			org.RunPreSaveValidation();
			Assert("Still shouldn't be an error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());
			Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.MakeExternalCreditorCodeMandatory.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			org.RunPreSaveValidation();
			Assert("Should be an error on external debtor code", org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			org.CompanyData.OB_APExternalCreditorCode = "TEST123";
			org.RunPreSaveValidation();
			Assert("Should be no error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			org.CompanyData.OB_APExternalCreditorCode = ZString.Empty;
			org.RunPreSaveValidation();
			Assert("Should now be an error on external debtor code", org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "TEST4321");
			org.RunPreSaveValidation();
			Assert("Should now be no error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.MakeExternalCreditorCodeMandatory.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false);
			org.CustomsCodes.RemoveAndDelete(org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, GlbCompany.CurrentCompany.Country));
			org.RunPreSaveValidation();
			Assert("Should be no error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.MakeExternalCreditorCodeMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			org.RunPreSaveValidation();
			Assert("Should be an error on external debtor code", org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			org.CompanyData.OB_APExternalCreditorCode = "TEST123";
			org.RunPreSaveValidation();
			Assert("Should be no error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			org.CompanyData.OB_APExternalCreditorCode = ZString.Empty;
			org.RunPreSaveValidation();
			Assert("Should now be an error on external debtor code", org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "TEST4321");
			org.RunPreSaveValidation();
			Assert("Should now be no error on external debtor code", !org.CompanyData.OB_APExternalCreditorCodeInfo.HasErrors());
		}

		public void TestOrgTypeValidation_AtleastOne()
		{
			var org = Factory.New<OrgHeader>();
			org.ExpectedAtleastOneOrgTypeInfos.Add(org.CompanyData.OB_IsDebtorInfo);
			org.ExpectedAtleastOneOrgTypeInfos.Add(org.CompanyData.OB_IsCreditorInfo);

			org.CompanyData.RunPreSaveValidation();
			AssertHasError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
			AssertHasError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");

			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.RunPreSaveValidation();
			AssertNoError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
			AssertNoError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");

			org.CompanyData.OB_IsCreditor = false;
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.RunPreSaveValidation();
			AssertNoError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
			AssertNoError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
		}

		public void TestOrgTypeValidation_Mandatory()
		{
			var org = Factory.New<OrgHeader>();
			org.ExpectedOrgTypeInfos.Add(org.CompanyData.OB_IsDebtorInfo);
			org.ExpectedOrgTypeInfos.Add(org.CompanyData.OB_IsCreditorInfo);

			org.CompanyData.RunPreSaveValidation();
			AssertHasError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization Type of Receivables selected.");
			AssertHasError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization Type of Payables selected.");

			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.RunPreSaveValidation();
			AssertNoError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization Type of Payables selected.");
			AssertHasError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization Type of Receivables selected.");

			org.CompanyData.OB_IsCreditor = false;
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.RunPreSaveValidation();
			AssertNoError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization Type of Receivables selected.");
			AssertHasError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization Type of Payables selected.");

			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.RunPreSaveValidation();
			AssertNoError(org.CompanyData.OB_IsDebtorInfo, "An Organization selected from here must have an Organization Type of Receivables selected.");
			AssertNoError(org.CompanyData.OB_IsCreditorInfo, "An Organization selected from here must have an Organization Type of Payables selected.");
		}

		#region Payables (OB_IsCreditor)

		public void TestCheckOB_OG_APCreditorGroup()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_OG_APCreditorGroup = ZGuid.Empty;
			Assert("Company not Creditor, so no errors", !CompanyData.OB_OG_APCreditorGroupInfo.HasNotifications());

			CompanyData.OB_IsCreditor = true;
			CompanyData.OB_OG_APCreditorGroup = ZGuid.Empty;
			Assert("Company is Creditor, so has errors", CompanyData.OB_OG_APCreditorGroupInfo.HasNotifications());

			CompanyData.OB_OG_APCreditorGroup = ZGuid.NewZGuid();
			Assert("Company is Creditor, OM_OG_APCreditorGroup is entered", !CompanyData.OB_OG_APCreditorGroupInfo.HasNotifications());
		}

		public void TestCheckOB_APCreditLimit()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APCreditLimit = -5;
			Assert("Company is not creditor, no notifications", !CompanyData.OB_APCreditLimitInfo.HasNotifications());

			CompanyData.OB_IsCreditor = true;
			CompanyData.OB_APCreditLimit = -4;
			Assert("Company is Creditor, OM_APCreditLimit < 0, has errors", CompanyData.OB_APCreditLimitInfo.HasErrors());

			CompanyData.OB_APCreditLimit = 5;
			Assert("Company is Creditor, OM_APCreditLimit > 0, no errors", !CompanyData.OB_APCreditLimitInfo.HasErrors());
		}

		public void TestCheckOB_APPaymentTermsDisplaysWarningWhenSHPIsUsedOnAP()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromShipmentDate.Code;
			Assert("Company is not creditor, no notifications", !CompanyData.OB_APPaymentTermsInfo.HasNotifications());

			CompanyData.OB_IsCreditor = true;
			CompanyData.Validation.ValidateOB_APPaymentTerms();
			AssertHasWarning("Should show warning", CompanyData.OB_APPaymentTermsInfo, "This AP Invoice Term is for reference purposes only and can be used to record the invoice term that your supplier has given to you. However this term does not default the due date based on the shipment date and rather defaults based on the user entered invoice date, because it is common to receive AP Invoices that span multiple jobs with different shipment dates.");
		}

		public void TestCheckOB_APPaymentTerms()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APPaymentTerms = "RRR";
			Assert("Company is not creditor, no notifications", !CompanyData.OB_APPaymentTermsInfo.HasNotifications());

			CompanyData.OB_IsCreditor = true;
			CompanyData.Validation.ValidateOB_APPaymentTerms();
			Assert("Company is Creditor, invalid code, has errors", CompanyData.OB_APPaymentTermsInfo.HasErrors());

			CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromInvoiceDate;
			Assert("Company is Creditor, valid code, no errors", !CompanyData.OB_APPaymentTermsInfo.HasErrors());

			CompanyData.OB_APPaymentTerms = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertHasErrors("'DEF' term can't be set if AP Settlement Group is empty.", CompanyData.OB_APPaymentTermsInfo);
			AssertEquals(false, CompanyData.Validation.CanDefaultAPTermBeSet);

			Org.APSettlementGroupPK = Org.PK;
			CompanyData.MarkAsNeedingValidation();
			CompanyData.RunPreSaveValidation();
			AssertHasErrors("'DEF' term can't be set if AR Settlement Group is set to itself.", CompanyData.OB_APPaymentTermsInfo);
			AssertEquals(false, CompanyData.Validation.CanDefaultAPTermBeSet);

			Org.APSettlementGroupPK = Factory.New<OrgHeader>().PK;
			CompanyData.MarkAsNeedingValidation();
			CompanyData.RunPreSaveValidation();
			AssertNoErrors(CompanyData.OB_APPaymentTermsInfo);
			AssertEquals(true, CompanyData.Validation.CanDefaultAPTermBeSet);

			Org.APSettlementGroup.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			CompanyData.MarkAsNeedingValidation();
			CompanyData.RunPreSaveValidation();
			AssertHasErrors("'DEF' term can't be set if settlement organization also has 'DEF' term.", CompanyData.OB_APPaymentTermsInfo);
			AssertEquals(false, CompanyData.Validation.CanDefaultAPTermBeSet);

			Org.APSettlementGroup.CompanyData.OB_IsCreditor = true;
			Org.APSettlementGroup.CompanyData.MarkAsNeedingValidation();
			Org.APSettlementGroup.CompanyData.RunPreSaveValidation();
			AssertHasErrors("'DEF' term can't be set if this organization is settlement for another one.", Org.APSettlementGroup.CompanyData.OB_APPaymentTermsInfo);
		}

		public void TestCheckOB_APCategory()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APCategory = "RRR";
			Assert("Company is not creditor, no notifications", !CompanyData.OB_APCategoryInfo.HasNotifications());

			CompanyData.OB_IsCreditor = true;
			CompanyData.Validation.ValidateOB_APCategory();
			Assert("Company is Creditor, invalid code, has errors", CompanyData.OB_APCategoryInfo.HasErrors());

			CompanyData.OB_APCategory = "STD";
			Assert("Company is Creditor, valid code, no errors", !CompanyData.OB_APCategoryInfo.HasErrors());
		}

		#endregion

		#region Receivables (OB_IsDebtor)

		public void TestCheckOB_ARTreatDisbursementsAsStandardValue()
		{
			CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_ARTreatDisbursementsAsStandardValue = 0;
			AssertNoErrors(Org.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo);

			Org.CompanyData.OB_ARTreatDisbursementsAsStandardValue = -1;
			AssertHasErrors(Org.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo);

			Org.CompanyData.OB_ARTreatDisbursementsAsStandardValue = 1;
			AssertNoErrors(Org.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo);

			CompanyData.OB_IsDebtor = false;
			Org.CompanyData.OB_ARTreatDisbursementsAsStandardValue = 1;
			AssertNoErrors(Org.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo);
		}

		public void TestCheckOB_ARCreditCardType()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditCardType = "DDD";
			AssertHasError(CompanyData.OB_ARCreditCardTypeInfo, "Enter a valid " + CompanyData.OB_ARCreditCardTypeInfo.Description + ".");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.Mastercard;
			AssertNoError(CompanyData.OB_ARCreditCardTypeInfo, "Enter a valid " + CompanyData.OB_ARCreditCardTypeInfo.Description + ".");

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARCreditCardType = "YYY";
			AssertNoError(CompanyData.OB_ARCreditCardTypeInfo, "Enter a valid " + CompanyData.OB_ARCreditCardTypeInfo.Description + ".");
		}

		public void TestCheckOB_ARCreditCardNum()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditCardNum = "DDD";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be numerical.");

			CompanyData.OB_ARCreditCardNum = "23623623";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be 13-19 digits length.");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.Mastercard;
			CompanyData.OB_ARCreditCardNum = "512362335688767";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be 16 digits length.");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.AmExpress;
			CompanyData.OB_ARCreditCardNum = "3423623356887674";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be 15 digits length.");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.DinClub;
			CompanyData.OB_ARCreditCardNum = "3003623356887674";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be 14 digits length.");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.Visa;
			CompanyData.OB_ARCreditCardNum = "462362335688764";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be 13 or 16 digits length.");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.ChinaUnionPay;
			CompanyData.OB_ARCreditCardNum = "622126335688764";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number should be 16-19 digits length.");

			CompanyData.OB_ARCreditCardType = CreditCardTypeList.Codes.Visa;
			CompanyData.OB_ARCreditCardNum = "4874120020928563";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Credit Card Number invalid.");

			CompanyData.OB_ARCreditCardNum = "4874120020928561";
			AssertNoErrors(CompanyData.OB_ARCreditCardNumInfo);

			CompanyData.OB_ARCreditCardType = "BLH";
			CompanyData.OB_ARCreditCardNum = "";
			AssertHasError(CompanyData.OB_ARCreditCardNumInfo, "Please enter a " + CompanyData.OB_ARCreditCardNumInfo.Description + ".");

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARCreditCardType = "BLH";
			CompanyData.OB_ARCreditCardNum = "";
			AssertNoErrors(CompanyData.OB_ARCreditCardNumInfo);
		}

		public void TestCheckOB_ARCreditCardHolder()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditCardNum = "2362362335688767";
			CompanyData.OB_ARCreditCardHolder = "BLAH";
			AssertNoErrors(CompanyData.OB_ARCreditCardHolderInfo);

			CompanyData.OB_ARCreditCardHolder = "";
			AssertHasError(CompanyData.OB_ARCreditCardHolderInfo, "Please enter a " + CompanyData.OB_ARCreditCardHolderInfo.Description + ".");

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARCreditCardHolder = "";
			AssertNoErrors(CompanyData.OB_ARCreditCardHolderInfo);
		}

		public void TestCheckOB_ARCreditCardExpire()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditCardNum = "2362362335688767";
			CompanyData.OB_ARCreditCardExpire = "0112";
			AssertNoErrors(CompanyData.OB_ARCreditCardExpireInfo);

			CompanyData.OB_ARCreditCardExpire = "";
			AssertHasError(CompanyData.OB_ARCreditCardExpire_MonthInfo, "Please enter a value.");
			AssertHasError(CompanyData.OB_ARCreditCardExpire_YearInfo, "Please enter a value.");

			CompanyData.OB_IsDebtor = false;
			CompanyData.Validation.ValidateAll();
			AssertNoErrors(CompanyData.OB_ARCreditCardExpire_MonthInfo);
			AssertNoErrors(CompanyData.OB_ARCreditCardExpire_YearInfo);
		}

		public void TestCheckOB_ARBuyersConsolInvoicingStyle()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARBuyersConsolInvoicingStyle = "XXX";
			AssertHasErrors(CompanyData.OB_ARBuyersConsolInvoicingStyleInfo);

			CompanyData.OB_ARBuyersConsolInvoicingStyle = "DEF";
			AssertNoErrors(CompanyData.OB_ARBuyersConsolInvoicingStyleInfo);

			CompanyData.OB_ARBuyersConsolInvoicingStyle = "";
			AssertHasErrors(CompanyData.OB_ARBuyersConsolInvoicingStyleInfo);

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARBuyersConsolInvoicingStyle = "YYY";
			AssertNoErrors(CompanyData.OB_ARBuyersConsolInvoicingStyleInfo);
		}

		public void TestCheckOB_ARCreditAgreedPaymentMethod()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditAgreedPaymentMethod = "XXX";
			AssertHasErrors(CompanyData.OB_ARCreditAgreedPaymentMethodInfo);

			CompanyData.OB_ARCreditAgreedPaymentMethod = "CHK";
			AssertNoErrors(CompanyData.OB_ARCreditAgreedPaymentMethodInfo);

			CompanyData.OB_ARCreditAgreedPaymentMethod = "";
			AssertNoErrors(CompanyData.OB_ARCreditAgreedPaymentMethodInfo);

			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARCreditAgreedPaymentMethod = "YYY";
			AssertNoErrors(CompanyData.OB_ARCreditAgreedPaymentMethodInfo);
		}

		public void TestCheckOB_ARCreditAgreedPaymentMethod_UsesPreferredPaymentMethod()
		{
			var expectedWarningMessage = "Please consider using Credit Card as payment method.";
			var mockPreferredPaymentMethod = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IPreferredPaymentMethod>();
			mockPreferredPaymentMethod.Setup(x => x.GetPreferredPaymentMethodWarning(It.IsAny<string>(), It.IsAny<string>()))
				.Returns((string orgCategory, string paymentMethod) =>
				{
					return (paymentMethod != OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard) ? expectedWarningMessage : null;
				});

			CompanyData.OB_IsDebtor = true;

			CompanyData.OB_ARCreditAgreedPaymentMethod = "";
			AssertNoWarnings("No warning expected when no payment method is set", CompanyData.OB_ARCreditAgreedPaymentMethodInfo);

			CompanyData.OB_ARCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			AssertHasWarning(CompanyData.OB_ARCreditAgreedPaymentMethodInfo, expectedWarningMessage);

			CompanyData.OB_ARCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;
			AssertNoWarnings("No warning expected when preferred payment method is selected", CompanyData.OB_ARCreditAgreedPaymentMethodInfo);
		}

		public void TestCheckOB_APCreditAgreedPaymentMethod()
		{
			CompanyData.OB_IsCreditor = true;
			CompanyData.OB_APCreditAgreedPaymentMethod = "XXX";
			AssertHasErrors(CompanyData.OB_APCreditAgreedPaymentMethodInfo);

			CompanyData.OB_APCreditAgreedPaymentMethod = "CHK";
			AssertNoErrors(CompanyData.OB_APCreditAgreedPaymentMethodInfo);

			CompanyData.OB_APCreditAgreedPaymentMethod = "";
			AssertNoErrors(CompanyData.OB_APCreditAgreedPaymentMethodInfo);

			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APCreditAgreedPaymentMethod = "YYY";
			AssertNoErrors(CompanyData.OB_APCreditAgreedPaymentMethodInfo);
		}

		public void TestCheckOB_ARWarehouseRatingPeriod()
		{
			Org.OH_IsDebtor = false;
			CompanyData.OB_ARWarehouseRatingPeriod = "XXX";
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);

			Org.OH_IsDebtor = true;
			CompanyData.OB_ARWarehouseRatingPeriod = "YYY";
			AssertHasError(CompanyData.OB_ARWarehouseRatingPeriodInfo, "Enter a valid Warehouse Rating Period.");

			CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);

			CompanyData.OB_ARWarehouseRatingPeriod = "";
			AssertHasError(CompanyData.OB_ARWarehouseRatingPeriodInfo, "Please enter a Warehouse Rating Period.");

			CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Default;
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);
		}

		public void TestCheckOB_ARWarehouseRatingPeriod_SplitPeriodBilling()
		{
			Org.OH_IsDebtor = true;
			CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			CompanyData.OB_ARWarehouseRatingPeriod = "YYY";
			AssertHasError(CompanyData.OB_ARWarehouseRatingPeriodInfo, "For Split Period Billing, you can only specify a rating/billing period of Daily, Weekly or Monthly.");

			CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);

			CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);

			CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);

			CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			AssertHasError(CompanyData.OB_ARWarehouseRatingPeriodInfo, "For Split Period Billing, you can only specify a rating/billing period of Daily, Weekly or Monthly.");

			CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;
			AssertNoErrors(CompanyData.OB_ARWarehouseRatingPeriodInfo);
		}

		CreditTemporaryIncreaseAuthorisationSettingsRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold as CreditTemporaryIncreaseAuthorisationSettingsRegistryItem; }
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_NoValidationIfNotDebtor()
		{
			CompanyData.OB_IsDebtor = false;
			AssertEquals("Precondition, registry defaults to having no settings", 0, CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Value.Count);
			CompanyData.OB_ARCreditLimit = 100M;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 0M;
			AssertNoErrors("No validation performed because IsDebtor is false", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 1M;
			AssertNoErrors("No validation performed because IsDebtor is false", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 2M;
			AssertHasErrors(CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_NoRegistry()
		{
			CompanyData.OB_IsDebtor = true;
			AssertEquals("Precondition, registry defaults to having no settings", 0, CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Value.Count);
			CompanyData.OB_ARCreditLimit = 100M;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 0M;
			AssertNoErrors("No validation performed based on registry or security if override left as 0", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 1M;
			AssertHasError(CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, "To enter a temporary credit limit increase you must first configure the registry setting: 'Temporary Credit Limit Increase Threshold'");
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_WithChangeToZero()
		{
			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(5, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(5, 0, RangeCodes.Above, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 5);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditLimit = 100M;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 100M;
			CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Now;
			Assert(!CompanyData.HasNotifications());
			Factory.Save();

			collection.RemoveAndDeleteAll();
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = false;

			CompanyData.OB_ARTemporaryCreditLimitIncrease = 0M;
			CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Empty;
			Assert(!CompanyData.HasNotifications());
			AssertNoErrors("No validation performed based on registry or security if override changed to 0", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_Security_With_Amounts()
		{
			CompanyData.OB_IsDebtor = true;

			Env.Security.OrgReceivablesTemporaryCreditAdjustment.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = false;

			CompanyData.OB_ARCreditLimit = 100M;

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(50, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(150, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.CreateCreditTemporaryIncreaseRequirement(150, 0, RangeCodes.Above, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, 20);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var expectedMesage = @"You do not have the appropriate security rights to set this adjustment amount.

Please ask your administrator to change either your Staff or Group Security Rights to allow the appropriate access via:

Maintain -> Master Data -> Organization -> Edit -> Receivables -> Modify Credit Control and Settlement -> Modify Credit Control -> Temporary Credit Adjustment";

			CompanyData.OB_ARTemporaryCreditLimitIncrease = 50M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 51M;
			AssertHasError("50.01-100 requires first level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 101M;
			AssertHasError("100.01-150 requires second level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 151M;
			AssertHasError("150.01+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);

			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 50M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 51M;
			AssertNoErrors("50.01-100 requires first level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 101M;
			AssertHasError("100.01-150 requires second level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 151M;
			AssertHasError("150.01+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);

			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 50M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 51M;
			AssertNoErrors("50.01-100 requires first level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 101M;
			AssertNoErrors("100.01-150 requires second level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 151M;
			AssertHasError("150.01+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);

			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 50M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 51M;
			AssertNoErrors("50.01-100 requires first level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 101M;
			AssertNoErrors("100.01-150 requires second level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 151M;
			AssertNoErrors("150.01+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);

			Factory.Save();
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 151M;
			AssertNoErrors("No error due to no change to DB value", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 152M;
			AssertNoErrors("150.01+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_Security_With_Percentages()
		{
			CompanyData.OB_IsDebtor = true;

			Env.Security.OrgReceivablesTemporaryCreditAdjustment.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = false;

			CompanyData.OB_ARCreditLimit = 100M;

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 20, RangeCodes.UpTo, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 30, RangeCodes.UpTo, AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 30, RangeCodes.Above, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, 20);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var expectedMesage = @"You do not have the appropriate security rights to set this adjustment amount.

Please ask your administrator to change either your Staff or Group Security Rights to allow the appropriate access via:

Maintain -> Master Data -> Organization -> Edit -> Receivables -> Modify Credit Control and Settlement -> Modify Credit Control -> Temporary Credit Adjustment";

			CompanyData.OB_ARTemporaryCreditLimitIncrease = 10M;
			AssertNoErrors("Registry configured so any user can increase up to 10%, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 11M;
			AssertHasError("10.01-20% requires first level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 21M;
			AssertHasError("20.01-30% requires second level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 31M;
			AssertHasError("30%+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);

			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 10M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 11M;
			AssertNoErrors("10.01-20% requires first level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 21M;
			AssertHasError("20.01-30% requires second level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 31M;
			AssertHasError("30%+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);

			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 10M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 11M;
			AssertNoErrors("10.01-20% requires first level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 21M;
			AssertNoErrors("20.01-30% requires second level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 31M;
			AssertHasError("30%+  requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);

			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 10M;
			AssertNoErrors("Registry configured so any user can increase up to 50, so no error", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 11M;
			AssertNoErrors("10.01-20% requires first level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 21M;
			AssertNoErrors("20.01-30% requires second level approval, so is allowed", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
			CompanyData.OB_ARTemporaryCreditLimitIncrease = 31M;
			AssertNoErrors("30%+ requires third level approval, so security error shown", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_Security_When_Credit_Limit_Zero()
		{
			CompanyData.OB_IsDebtor = true;

			Env.Security.OrgReceivablesTemporaryCreditAdjustment.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

			CompanyData.OB_ARCreditLimit = 0M;

			foreach (bool usePercentages in new[] { true, false })
			{
				var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();

				if (usePercentages)
				{
					collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
					collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.Above, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
				}
				else
				{
					collection.CreateCreditTemporaryIncreaseRequirement(10, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
					collection.CreateCreditTemporaryIncreaseRequirement(10, 0, RangeCodes.Above, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
				}

				collection.RunPreSaveValidation();
				AssertEquals(false, collection.HasNotifications());
				CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var expectedMesage = @"You cannot enter a Temporary Credit Limit Increase because the Credit Limit is not set.";

				CompanyData.OB_ARTemporaryCreditLimitIncrease = 0.01M;
				AssertHasError("You can only set an increase if the original limit is set to something", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
				CompanyData.OB_ARTemporaryCreditLimitIncrease = 0M;
			}
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_Security_When_CrediOnHold()
		{
			CompanyData.OB_IsDebtor = true;

			Env.Security.OrgReceivablesTemporaryCreditAdjustment.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

			CompanyData.OB_ARCreditLimit = 10M;
			CompanyData.OB_ARCreditApproved = false;

			foreach (bool usePercentages in new[] { true, false })
			{
				var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();

				if (usePercentages)
				{
					collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
					collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.Above, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
				}
				else
				{
					collection.CreateCreditTemporaryIncreaseRequirement(10, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
					collection.CreateCreditTemporaryIncreaseRequirement(10, 0, RangeCodes.Above, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
				}

				collection.RunPreSaveValidation();
				AssertEquals(false, collection.HasNotifications());
				CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var expectedMesage = @"You cannot enter a Temporary Credit Limit Increase because credit is not approved.";

				CompanyData.OB_ARTemporaryCreditLimitIncrease = 0.01M;
				AssertHasError("You can only set an increase if credit is approved", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
				CompanyData.OB_ARTemporaryCreditLimitIncrease = 0M;
			}
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_NegativeValue()
		{
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARTemporaryCreditLimitIncrease = -1M;
			AssertHasError(CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, "Temporary Credit Limit Increase cannot be negative.");
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_WhenUsingWebService()
		{
			CompanyData.OB_IsDebtor = true;

			Env.Security.OrgReceivablesTemporaryCreditAdjustment.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

			CompanyData.OB_ARCreditLimit = 10M;

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 20, RangeCodes.UpTo, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 30, RangeCodes.UpTo, AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 30, RangeCodes.Above, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, 20);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CompanyData.OB_ARTemporaryCreditLimitIncrease = 0.01M;

			var expectedMesage = "This setting cannot be used when the 'Use Web Service for Credit Limit' registry item is turned on.";

			CompanyData.OB_ARTemporaryCreditLimitIncrease = 0.01M;
			AssertHasError("Due to use of web service for credit limit you cannot set a temporary increase", CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, expectedMesage);
		}

		public void TestCheckOB_ARTemporaryCreditLimitIncrease_SecurityLevels()
		{
			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(50, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(150, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.CreateCreditTemporaryIncreaseRequirement(150, 0, RangeCodes.Above, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, 20);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Env.Security.OrgReceivablesTemporaryCreditAdjustment.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = false;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = false;

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 3, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 2, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 1, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 0, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 3, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 2, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 1, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 0, 20, 30, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 3, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 2, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 1, 20, 30, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 0, 20, 30, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 3, 20, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 2, 20, 30, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 1, 20, 30, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 0, 20, 30, false, false);

			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 3, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 2, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 1, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 0, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 3, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 2, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 1, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 0, 20, 0, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 3, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 2, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 1, 20, 0, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 0, 20, 0, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 3, 20, 0, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 2, 20, 0, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 1, 20, 0, false, false);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 0, 20, 0, false, false);

			// Don't disallow change based on levels if increase is 0
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 3, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 2, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 1, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 0, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 3, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 2, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 1, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 0, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 3, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 2, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 1, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 0, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 3, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 2, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 1, 0, 30, false, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 0, 0, 30, false, true);

			// Don't disallow change based on levels if date has expired
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 3, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 2, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 1, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 0, 0, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 3, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 2, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 1, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 1, 0, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 3, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 2, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 1, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 2, 0, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 3, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 2, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 1, 20, 30, true, true);
			SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(authorizingUser, 3, 0, 20, 30, true, true);
		}

		void SetTemporaryCreditLimitIncreaseViaUserLevel(GlbStaff user, int level, decimal tempCreditLimitIncrease, bool setAsExpired)
		{
			TestDateAttribute.Date = TestDateAttribute.Date.AddMilliseconds(10);
			OrgCompanyDataTest.SetSecurityForUser(user.PK, Env.Security.CreditLimitAdjustmentFirstLevel, level == 1);
			OrgCompanyDataTest.SetSecurityForUser(user.PK, Env.Security.CreditLimitAdjustmentSecondLevel, level == 2);
			OrgCompanyDataTest.SetSecurityForUser(user.PK, Env.Security.CreditLimitAdjustmentThirdLevel, level == 3);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMilliseconds(10);
			using (Env.SetTemporaryUserContext(user.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				// Because SetTemporaryUserContext doesn't seem to update the data in the cache based
				// on what is in the DB
				Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = level == 1;
				Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = level == 2;
				Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = level == 3;

				if (CompanyData.OB_ARTemporaryCreditLimitIncrease == tempCreditLimitIncrease)
				{
					// Keeping it the same won't be seen as a change by this user. So lets change it to something else then back
					CompanyData.OB_ARTemporaryCreditLimitIncrease = tempCreditLimitIncrease + 1;
					AssertNoErrors(CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
					Factory.Save();
				}

				CompanyData.OB_ARTemporaryCreditLimitIncrease = tempCreditLimitIncrease;

				AssertNoErrors(CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo);
				Factory.Save();
				if (setAsExpired)
				{
					CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Now.AddDays(-1);
				}
				Factory.Save();
			}
		}

		int setupAndAssertTemporaryCreditLimitIncreaseSecurityValidation_OrgSequence;

		void SetupAndAssertTemporaryCreditLimitIncreaseSecurityValidation(
			GlbStaff previousUser, int previousUserLevel, int thisUserLevel, decimal previousUserTCLI,
			decimal thisUserTCLI, bool setAsExpired, bool expectedIsAllowedToEdit)
		{
			ResetOrgAndCompanyData(string.Format("O{0}", setupAndAssertTemporaryCreditLimitIncreaseSecurityValidation_OrgSequence++));
			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreditLimit = 100M;
			Factory.Save();
			SetTemporaryCreditLimitIncreaseViaUserLevel(previousUser, previousUserLevel, previousUserTCLI, setAsExpired);
			var freshFactory = new BusinessObjectFactory();
			var companyDataReloaded = freshFactory.Load<OrgCompanyData>(CompanyData.PK);
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = thisUserLevel == 1;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = thisUserLevel == 2;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = thisUserLevel == 3;
			companyDataReloaded.OB_ARTemporaryCreditLimitIncrease = thisUserTCLI;
			if (expectedIsAllowedToEdit)
			{
				AssertNoErrors("Sufficient access to change temp increase", companyDataReloaded.OB_ARTemporaryCreditLimitIncreaseInfo);
			}
			else
			{
				AssertHasError("Anyone without enough access cannot change temp increase", companyDataReloaded.OB_ARTemporaryCreditLimitIncreaseInfo,
					"You do not have the appropriate security rights to change the Temporary Credit Limit Increase. This is because the Temporary Credit Limit Increase has been set by Adam Brian Carlson (JVL) with higher approval right.");
			}
			companyDataReloaded.OB_ARTemporaryCreditLimitIncrease = 20;
			AssertNoErrors("No error when kept as is", companyDataReloaded.OB_ARTemporaryCreditLimitIncreaseInfo);
		}

		public void TestCheckOB_ARWhsStorageCalcMethod()
		{
			Org.OH_IsDebtor = false;
			CompanyData.OB_ARWhsStorageCalcMethod = "RRR";
			Assert("Company is not debtor, no notifications", !CompanyData.OB_ARWhsStorageCalcMethodInfo.HasNotifications());

			Org.OH_IsDebtor = true;
			CompanyData.OB_ARWhsStorageCalcMethod = "RRR";
			Assert("Company is debtor, invalid code, has errors", CompanyData.OB_ARWhsStorageCalcMethodInfo.HasErrors());

			CompanyData.OB_ARWhsStorageCalcMethod = "";
			Assert("Company is debtor, no code, has errors", CompanyData.OB_ARWhsStorageCalcMethodInfo.HasErrors());

			CompanyData.OB_ARWhsStorageCalcMethod = "MAX";
			Assert("Company is not debtor, valid code, no errors", !CompanyData.OB_ARWhsStorageCalcMethodInfo.HasErrors());
		}

		public void TestCheckOB_OJ_ARDebtorGroup()
		{
			Org.OH_IsDebtor = false;
			CompanyData.OB_OJ_ARDebtorGroup = ZGuid.Empty;
			Assert("Company not debtor, so no errors", !CompanyData.OB_OJ_ARDebtorGroupInfo.HasNotifications());

			Org.OH_IsDebtor = true;
			CompanyData.OB_OJ_ARDebtorGroup = ZGuid.Empty;
			Assert("Company is debtor, so has errors", CompanyData.OB_OJ_ARDebtorGroupInfo.HasNotifications());

			CompanyData.OB_OJ_ARDebtorGroup = ZGuid.NewZGuid();
			Assert("Company is debtor, OB_OJ_ARDebtorGroup is entered", !CompanyData.OB_OJ_ARDebtorGroupInfo.HasNotifications());
		}

		public void TestCheckOB_AB_PayToAccount()
		{
			AccBankAccount acc = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();
			group.OJ_Code = "XXX";
			group.OJ_Desc = "XXX";
			group.DefaultBankAccountPK = acc.PK;
			Factory.Save();

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_IsDebtor = true;
			testHeader.CompanyData.OB_OJ_ARDebtorGroup = group.PK;
			testHeader.CompanyData.OverrideBankAccountFromDebtorGroup = true;

			AssertEquals("Pay To Account not set", ZGuid.Empty, testHeader.CompanyData.OB_AB_ARPayToAccount);

			testHeader.CompanyData.OB_AB_ARPayToAccount = acc.PK;

			AssertEquals("Pay To Account Set", acc.PK, testHeader.CompanyData.OB_AB_ARPayToAccount);
			AssertNoErrors("Pay To Account Valid", testHeader.CompanyData.OB_AB_ARPayToAccountInfo);

			testHeader.CompanyData.OB_AB_ARPayToAccount = ZGuid.NewZGuid();
			AssertHasErrors("Pay To Account should be invalid", testHeader.CompanyData.OB_AB_ARPayToAccountInfo);

			testHeader.CompanyData.OverrideBankAccountFromDebtorGroup = false;
			testHeader.CompanyData.OB_AB_ARPayToAccount = ZGuid.Empty;
			AssertNoErrors("Pay To Account Valid", testHeader.CompanyData.OB_AB_ARPayToAccountInfo);
		}

		public void TestCheckARBankAccountToDisplay()
		{
			AccBankAccount acc = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();
			group.OJ_Code = "XXX";
			group.OJ_Desc = "XXX";
			group.DefaultBankAccountPK = acc.PK;
			Factory.Save();

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_IsDebtor = true;
			testHeader.CompanyData.OB_OJ_ARDebtorGroup = group.PK;

			AssertEquals("Pay To Account not set", ZGuid.Empty, testHeader.CompanyData.OB_AB_ARPayToAccount);
			AssertNoErrors("Pay To Account should be valid (empty) when not overridden", testHeader.CompanyData.ARBankAccountToDisplayInfo);

			testHeader.CompanyData.OverrideBankAccountFromDebtorGroup = true;
			testHeader.CompanyData.ARBankAccountToDisplay = ZGuid.Empty;
			AssertHasErrors("Pay To Account should be invalid when empty and overridden", testHeader.CompanyData.ARBankAccountToDisplayInfo);

			testHeader.CompanyData.ARBankAccountToDisplay = acc.PK;
			AssertEquals("Pay To Account Set", acc.PK, testHeader.CompanyData.OB_AB_ARPayToAccount);
			AssertNoErrors("Pay To Account Valid", testHeader.CompanyData.ARBankAccountToDisplayInfo);

			testHeader.CompanyData.ARBankAccountToDisplay = ZGuid.Empty;
			AssertHasErrors("Pay To Account should be invalid when empty and overridden", testHeader.CompanyData.ARBankAccountToDisplayInfo);
		}
		#endregion

		#region Controlling Branch

		public void TestControllingBranchBelongsToCurrentCompany()
		{
			Org.OH_IsDebtor = true;

			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_OH_OrgProxy = Org.PK;

			GlbBranch branch2 = Factory.New<GlbBranch>();

			Org.CompanyData.OB_GB_ControllingBranch = GlbCompany.CurrentCompany.Branches[0].PK;
			Assert("No Error because branch is set to member of current company", !Org.CompanyData.OB_GB_ControllingBranchInfo.HasWarnings());

			Org.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			Assert("Warning because Branch is not member current company", Org.CompanyData.OB_GB_ControllingBranchInfo.HasWarnings());

			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, true, false);
			SetRequiredFields(fields);

			Org.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			Assert("No Error as branch is not mandatory", !Org.CompanyData.OB_GB_ControllingBranchInfo.HasNotifications());

			fields = new OrgRequiredFields(false, true, false, false, false, false, false, false, false, true, false);
			SetRequiredFields(fields); // Require branch set as mandatory

			Org.CompanyData.OB_GB_ControllingBranch = branch1.PK;
			branch1.GB_OH_OrgProxy = ZGuid.Empty;
			Org.CompanyData.RunPreSaveValidation();
			Assert("Error as branch IS mandatory and org is not org proxy", Org.CompanyData.OB_GB_ControllingBranchInfo.HasNotifications());

			branch1.GB_OH_OrgProxy = Org.PK;
			Org.CompanyData.RunPreSaveValidation();
			Assert("Warning as Branch is not member current company ", Org.CompanyData.OB_GB_ControllingBranchInfo.HasWarnings());

			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			Org.CompanyData.RunPreSaveValidation();
			Assert("No Error as branch IS mandatory, but org is an org proxy", !Org.CompanyData.OB_GB_ControllingBranchInfo.HasNotifications());
		}

		protected void SetRequiredFields(OrgRequiredFields fields)
		{
			Env.Registry.SetTempOrgConsigneeRequiredFields(fields);
			Env.Registry.SetTempOrgConsignorRequiredFields(fields);
			Env.Registry.SetTempOrgCreditorRequiredFields(fields);
			Env.Registry.SetTempOrgDebtorRequiredFields(fields);
			Env.Registry.SetOrgBrokerRequiredFields(fields);
			Env.Registry.SetOrgCarrierRequiredFields(fields);
			Env.Registry.SetOrgCompetitorRequiredFields(fields);
			Env.Registry.SetOrgConsigneeRequiredFields(fields);
			Env.Registry.SetOrgConsignorRequiredFields(fields);
			Env.Registry.SetOrgContainerYardRequiredFields(fields);
			Env.Registry.SetOrgCreditorRequiredFields(fields);
			Env.Registry.SetOrgCTORequiredFields(fields);
			Env.Registry.SetOrgDebtorRequiredFields(fields);
			Env.Registry.SetOrgForwarderRequiredFields(fields);
			Env.Registry.SetOrgPackDepotRequiredFields(fields);
			Env.Registry.SetOrgSalesLeadRequiredFields(fields);
			Env.Registry.SetOrgTransportClientRequiredFields(fields);
			Env.Registry.SetOrgWarehouseRequiredFields(fields);
		}

		#endregion

		public void TestCheckOB_ARUseSettlementGroupCreditLimit()
		{
			CompanyData.OB_IsDebtor = false;
			Org.ARSettlementGroupPK = ZGuid.Empty;
			CompanyData.OB_ARUseSettlementGroupCreditLimit = true;

			AssertEquals("CompanyData.OB_ARUseSettlementGroupCreditLimit", true, CompanyData.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("CompanyData.OB_IsDebtor", false, CompanyData.OB_IsDebtor);
			AssertNoErrors(CompanyData.OB_ARUseSettlementGroupCreditLimitInfo);

			CompanyData.OB_IsDebtor = true;
			Org.ARSettlementGroupPK = ZGuid.Empty;
			CompanyData.OB_ARUseSettlementGroupCreditLimit = true;

			AssertEquals("CompanyData.OB_ARUseSettlementGroupCreditLimit", true, CompanyData.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("CompanyData.OB_IsDebtor", true, CompanyData.OB_IsDebtor);
			AssertHasError(CompanyData.OB_ARUseSettlementGroupCreditLimitInfo, "Settlement Group Credit Limit must not be ticked if the Settlement Group is empty");

			CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
			AssertEquals("CompanyData.OB_ARUseSettlementGroupCreditLimit", false, CompanyData.OB_ARUseSettlementGroupCreditLimit);
			AssertNoErrors(CompanyData.OB_ARUseSettlementGroupCreditLimitInfo);
		}

		public void TestCheckOB_RateSecurityGroup()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC Description");
			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(list));

			CompanyData.OB_RateSecurityGroup = "XXX";
			AssertHasError(CompanyData.OB_RateSecurityGroupInfo, "Enter a valid Rate's Security.");

			CompanyData.OB_RateSecurityGroup = "ABC";
			AssertNoErrors(CompanyData.OB_RateSecurityGroupInfo);
		}

		public void TestCheckOB_ARVATConfig()
		{
			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARVATConfig = "";
			AssertNoErrors(CompanyData.OB_ARVATConfigInfo);

			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARVATConfig = "X";
			AssertHasError(CompanyData.OB_ARVATConfigInfo, "Enter a valid selection.");

			CompanyData.OB_ARVATConfig = "";
			AssertHasError(CompanyData.OB_ARVATConfigInfo, "Please enter a value.");

			Action assertNoErrorsForAllCodes = () =>
			{
				foreach (CodeDescriptionPair type in new AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes())
				{
					CompanyData.OB_ARVATConfig = type.Code;
					AssertNoErrors(CompanyData.OB_ARVATConfigInfo);
				}
			};

			Action<string> assertOverrideErrorsForSomeCodes = (string validationError) =>
			{
				foreach (CodeDescriptionPair type in new AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes())
				{
					CompanyData.OB_ARVATConfig = type.Code;
					if (type.Code == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code ||
						type.Code == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code)
					{
						AssertNoErrors(CompanyData.OB_ARVATConfigInfo);
					}
					else
					{
						AssertHasError(CompanyData.OB_ARVATConfigInfo, validationError);
					}
				}
			};

			GlbCompany.CurrentCompany.GC_IsGSTCashBasis = true;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(It.IsAny<string>()))
				.Returns<string>((ledger) => ledger == LedgerTypes.AccountsReceivable);
			using (ObjectFactory.Substitute(mock.Object))
			{
				assertNoErrorsForAllCodes();

				GlbCompany.CurrentCompany.GC_IsGSTCashBasis = false;
				assertOverrideErrorsForSomeCodes("Override is not permitted. All Tax for your company is reported on an Accrual Basis.");
			}
			mock = new Mock<IAccounting>();
			mock.Setup(m => m.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(It.IsAny<string>()))
				.Returns<string>((ledger) => ledger != LedgerTypes.AccountsReceivable);
			using (ObjectFactory.Substitute(mock.Object))
			{
				assertOverrideErrorsForSomeCodes("AR Organization specific Overrides are not Permitted by your Login Company’s Tax Recognition Defaulting Rules registry.");
			}
		}

		public void TestCheckOB_ARCreateVATComplianceDocumentOnPosting()
		{
			CompanyData.OB_IsDebtor = false;
			CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "";
			AssertNoErrors(CompanyData.OB_ARCreateVATComplianceDocumentOnPostingInfo);

			CompanyData.OB_IsDebtor = true;
			CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "X";
			AssertHasError(CompanyData.OB_ARCreateVATComplianceDocumentOnPostingInfo, "Enter a valid selection.");

			CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "";
			AssertHasError(CompanyData.OB_ARCreateVATComplianceDocumentOnPostingInfo, "Please enter a value.");

			CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge.Code;
			AssertHasError(CompanyData.OB_ARCreateVATComplianceDocumentOnPostingInfo, "This value must be set 'NON' if Debtor is Not Applicable to Tax.");

			CompanyData.OB_ARVATConfig = ZString.Empty;
			Action assertNoErrorsForAllCodes = () =>
			{
				var arCreateVATComplianceDocumentOnPostingList = new AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes();
				arCreateVATComplianceDocumentOnPostingList.RemoveCode(Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber);
				foreach (CodeDescriptionPair type in new AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes())
				{
					CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = type.Code;
					AssertNoErrors(CompanyData.OB_ARCreateVATComplianceDocumentOnPostingInfo);
				}
			};
		}

		public void TestCheckOB_APCreateVATComplianceDocumentOnPosting()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "";
			AssertNoErrors(CompanyData.OB_APCreateVATComplianceDocumentOnPostingInfo);

			CompanyData.OB_IsCreditor = true;
			CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "X";
			AssertHasError(CompanyData.OB_APCreateVATComplianceDocumentOnPostingInfo, "Enter a valid selection.");

			CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "";
			AssertHasError(CompanyData.OB_APCreateVATComplianceDocumentOnPostingInfo, "Please enter a value.");

			CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			CompanyData.OB_APCreateVATComplianceDocumentOnPosting = AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge.Code;
			AssertHasError(CompanyData.OB_APCreateVATComplianceDocumentOnPostingInfo, "This value must be set 'NON' if Creditor is Not Applicable to Tax.");

			CompanyData.OB_APVATConfig = ZString.Empty;
			Action assertNoErrorsForAllCodes = () =>
			{
				foreach (CodeDescriptionPair type in new AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes())
				{
					CompanyData.OB_APCreateVATComplianceDocumentOnPosting = type.Code;
					AssertNoErrors(CompanyData.OB_APCreateVATComplianceDocumentOnPostingInfo);
				}
			};
		}

		public void TestCheckOB_APVATConfig()
		{
			CompanyData.OB_IsCreditor = false;
			CompanyData.OB_APVATConfig = "";
			AssertNoErrors(CompanyData.OB_APVATConfigInfo);

			CompanyData.OB_IsCreditor = true;
			CompanyData.OB_APVATConfig = "X";
			AssertHasError(CompanyData.OB_APVATConfigInfo, "Enter a valid selection.");

			CompanyData.OB_APVATConfig = "";
			AssertHasError(CompanyData.OB_APVATConfigInfo, "Please enter a value.");

			Action assertNoErrorsForAllCodes = () =>
			{
				foreach (CodeDescriptionPair type in new AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes())
				{
					CompanyData.OB_APVATConfig = type.Code;
					AssertNoErrors(CompanyData.OB_APVATConfigInfo);
				}
			};

			Action<string> assertOverrideErrorsForSomeCodes = (string validationError) =>
			{
				foreach (CodeDescriptionPair type in new AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes())
				{
					CompanyData.OB_APVATConfig = type.Code;
					if (type.Code == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code ||
						type.Code == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code)
					{
						AssertNoErrors(CompanyData.OB_APVATConfigInfo);
					}
					else
					{
						AssertHasError(CompanyData.OB_APVATConfigInfo, validationError);
					}
				}
			};

			GlbCompany.CurrentCompany.GC_IsGSTCashBasis = true;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(It.IsAny<string>()))
				.Returns<string>((ledger) => ledger == LedgerTypes.AccountsPayable);
			using (ObjectFactory.Substitute(mock.Object))
			{
				assertNoErrorsForAllCodes();

				GlbCompany.CurrentCompany.GC_IsGSTCashBasis = false;
				assertOverrideErrorsForSomeCodes("Override is not permitted. All Tax for your company is reported on an Accrual Basis.");
			}
			mock = new Mock<IAccounting>();
			mock.Setup(m => m.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(It.IsAny<string>()))
				.Returns<string>((ledger) => ledger != LedgerTypes.AccountsPayable);
			using (ObjectFactory.Substitute(mock.Object))
			{
				assertOverrideErrorsForSomeCodes("AP Organization specific Overrides are not Permitted by your Login Company’s Tax Recognition Defaulting Rules registry.");
			}
		}

		public void TestCheckOB_AROnCreditHoldWaringForUserWithoutRights()
		{
			AssertCheckOB_AROnCreditHoldWaringForUserWithoutRights(false);
		}

		public void TestCheckOB_AROnCreditHoldWaringForUserWithoutRights_NoLogs()
		{
			AssertCheckOB_AROnCreditHoldWaringForUserWithoutRights(true);
		}

		void AssertCheckOB_AROnCreditHoldWaringForUserWithoutRights(bool noLogs)
		{
			CompanyData.OB_IsDebtor = true;
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "John C";
			authorizingUser.GS_FullName = "John Clark";
			authorizingUser.GS_Code = "JC";
			Factory.Save();
			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldFirstLevel, false);
			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldSecondLevel, false);
			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldThirdLevel, false);

			var expectedWarningForCurrentUser =
@"You ticked 'AR on Credit Hold' check box without any approval right.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.";

			var expectedWarningForNonCurrentUser =
@"User John Clark (JC) without any approval right ticked 'AR on Credit Hold' check box.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.";

			using (Env.SetTemporaryUserContext(authorizingUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition", false, CompanyData.OB_AROnCreditHold);
				CompanyData.OB_AROnCreditHold = true;
				AssertHasWarning(CompanyData.OB_AROnCreditHoldInfo, expectedWarningForCurrentUser);
				Factory.Save();
				if (noLogs)
				{
					var creditControlsModifiedEvents = CompanyData.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
					foreach (var creditControlsModifiedEvent in creditControlsModifiedEvents)
					{
						using (creditControlsModifiedEvent.LockForUpdatingKeyFieldsForTesting())
						{
							creditControlsModifiedEvent.SL_Reference = ZString.Empty;
						}
					}
					Factory.Save();
				}

				CompanyData.Validation.ValidateOB_AROnCreditHold();
				if (noLogs)
				{
					AssertNoWarnings(CompanyData.OB_AROnCreditHoldInfo);
				}
				else
				{
					AssertHasWarning(CompanyData.OB_AROnCreditHoldInfo, expectedWarningForCurrentUser);
				}
			}

			CompanyData.OB_AROnCreditHold = false;
			AssertNoWarnings(CompanyData.OB_AROnCreditHoldInfo);

			CompanyData.OB_AROnCreditHold = true;
			if (noLogs)
			{
				AssertNoWarnings(CompanyData.OB_AROnCreditHoldInfo);
			}
			else
			{
				AssertHasWarning(CompanyData.OB_AROnCreditHoldInfo, expectedWarningForNonCurrentUser);
			}

			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldSecondLevel, true);
			CompanyData.Validation.ValidateOB_AROnCreditHold();
			AssertNoWarnings(CompanyData.OB_AROnCreditHoldInfo);
		}

		#region TestCheckOB_AROnCreditHold

		[TestDate(2014, 02, 19, 12, 00, 00)]
		public void TestCheckOB_AROnCreditHold()
		{
			CompanyData.OB_IsDebtor = true;
			AssertEquals("Precondition", false, CompanyData.OB_AROnCreditHold);
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "Adam BC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			authorizingUser.GS_Code = "ABC";
			Factory.Save();
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 0);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, true, isUserWithoutRights: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, false, isUserWithoutRights: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, false, isUserWithoutRights: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, false, isUserWithoutRights: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, noLogs: true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 0);
			using (Env.SetTemporaryUserContext(authorizingUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupAndAssertAROnCreditHoldSecurityValidation(3, true, isUserWithoutRights: true, currentUserHasNoRights: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(2, true, isUserWithoutRights: true, currentUserHasNoRights: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(1, true, isUserWithoutRights: true, currentUserHasNoRights: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(0, true, isUserWithoutRights: true, currentUserHasNoRights: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(3, noLogs: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(2, noLogs: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(1, noLogs: true);
				SetupAndAssertAROnCreditHoldSecurityValidation(0, noLogs: true);
			}
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 1);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, noLogs: true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 2);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, false);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, noLogs: true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 3);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, false);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, false);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, noLogs: true);

			CompanyData.OB_IsDebtor = false;
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 0);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false, noLogs: true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 1);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false, noLogs: true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 2);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false, noLogs: true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 3);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false);
			SetupAndAssertAROnCreditHoldSecurityValidation(3, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(2, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(1, isDebtor: false, noLogs: true);
			SetupAndAssertAROnCreditHoldSecurityValidation(0, isDebtor: false, noLogs: true);
		}

		void SetupAndAssertAROnCreditHoldSecurityValidation(int level, bool isAllowedToTurnOff = false, bool isDebtor = true, bool isUserWithoutRights = false, bool noLogs = false, bool currentUserHasNoRights = false)
		{
			if (noLogs)
			{
				var creditControlsModifiedEvents = CompanyData.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
				foreach (var creditControlsModifiedEvent in creditControlsModifiedEvents)
				{
					using (creditControlsModifiedEvent.LockForUpdatingKeyFieldsForTesting())
					{
						creditControlsModifiedEvent.SL_Reference = ZString.Empty;
					}
				}
				Factory.Save();
			}

			var freshFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var companyDataReloaded = freshFactory.Load<OrgCompanyData>(CompanyData.PK);
			Env.Security.OrgCreditOnHoldFirstLevel.IsAllowed = level == 1;
			Env.Security.OrgCreditOnHoldSecondLevel.IsAllowed = level == 2;
			Env.Security.OrgCreditOnHoldThirdLevel.IsAllowed = level == 3;

			companyDataReloaded.OB_AROnCreditHold = false;

			if (isDebtor && !noLogs)
			{
				if (isAllowedToTurnOff)
				{
					AssertNoErrors("Sufficient access to take it off hold", companyDataReloaded.OB_AROnCreditHoldInfo);
				}
				else
				{
					if (isUserWithoutRights)
					{
						AssertHasError(companyDataReloaded.OB_AROnCreditHoldInfo, "You do not have the appropriate security rights to un-tick the 'AR on Credit Hold' check box. This is because the credit on hold has been set by Adam Brian Carlson (ABC) without any approval right and so only the same user or user with approval level 3 can un-tick it.");
					}
					else
					{
						AssertHasError(companyDataReloaded.OB_AROnCreditHoldInfo, "You do not have the appropriate security rights to un-tick the 'AR on Credit Hold' check box. This is because the credit on hold has been approved by Adam Brian Carlson (ABC) with higher approval right.");
					}
				}
			}
			else
			{
				AssertNoErrors("No Validation if not debtor", companyDataReloaded.OB_AROnCreditHoldInfo);
			}
			AssertNoWarnings("No warnings when to take it off hold", companyDataReloaded.OB_AROnCreditHoldInfo);

			companyDataReloaded.OB_AROnCreditHold = true;
			if (isDebtor && !noLogs)
			{
				AssertNoErrors("No error when kept as is", companyDataReloaded.OB_AROnCreditHoldInfo);
				if (isUserWithoutRights)
				{
					if (currentUserHasNoRights)
					{
						AssertHasWarning(companyDataReloaded.OB_AROnCreditHoldInfo,
@"You ticked 'AR on Credit Hold' check box without any approval right.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.");
					}
					else
					{
						AssertHasWarning(companyDataReloaded.OB_AROnCreditHoldInfo,
@"User Adam Brian Carlson (ABC) without any approval right ticked 'AR on Credit Hold' check box.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.");
					}
				}
				else
				{
					AssertNoWarnings("No warnings when kept as is", companyDataReloaded.OB_AROnCreditHoldInfo);
				}
			}
			else
			{
				AssertNoErrors("No Validation if not debtor", companyDataReloaded.OB_AROnCreditHoldInfo);
			}
		}

		#endregion

		public void TestCheckOB_CRIsShipsAgencyPrincipal()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			AssertHasError(org.CompanyData.OB_CRIsShipsAgencyPrincipalInfo, "To mark an Organization as Principal it must also be marked as Shipping Line.");

			org.OH_IsShippingLine = true;
			org.RunPreSaveValidation();

			AssertNoErrors(org.CompanyData.OB_CRIsShipsAgencyPrincipalInfo);
		}

		public void TestCheckOB_APTransactionCreationRestriction()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_APTransactionCreationRestriction = ZString.Empty;
			AssertHasError(org.CompanyData.OB_APTransactionCreationRestrictionInfo, "Please enter a Transaction Creation Restriction.");

			org.CompanyData.OB_APTransactionCreationRestriction = "123";
			AssertHasError(org.CompanyData.OB_APTransactionCreationRestrictionInfo, "Enter a valid Transaction Creation Restriction.");

			org.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.None;
			AssertNoErrors(org.CompanyData.OB_APTransactionCreationRestrictionInfo);
		}

		public void TestCheckOB_ARApplicableSurcharges()
		{
			var surcharge1 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surcharge1.ASC_Code = "AAA";
			surcharge1.ASC_GC_Company = GlbCompany.CurrentCompany.PK;

			var surcharge2 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surcharge2.ASC_Code = "BBB";
			surcharge2.ASC_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARApplicableSurcharges = ZString.Empty;
			AssertHasError(org.CompanyData.OB_ARApplicableSurchargesInfo, "Please enter a value.");

			org.CompanyData.OB_ARApplicableSurcharges = "ALL";
			AssertNoErrors(org.CompanyData.OB_ARApplicableSurchargesInfo);

			org.CompanyData.OB_ARApplicableSurcharges = "NON";
			AssertNoErrors(org.CompanyData.OB_ARApplicableSurchargesInfo);

			org.CompanyData.OB_ARApplicableSurcharges = "AAA";
			AssertNoErrors(org.CompanyData.OB_ARApplicableSurchargesInfo);

			org.CompanyData.OB_ARApplicableSurcharges = "BBB";
			AssertNoErrors(org.CompanyData.OB_ARApplicableSurchargesInfo);

			org.CompanyData.OB_ARApplicableSurcharges = "AAA,BBB";
			AssertNoErrors(org.CompanyData.OB_ARApplicableSurchargesInfo);

			org.CompanyData.OB_ARApplicableSurcharges = "CCC";
			AssertHasError(org.CompanyData.OB_ARApplicableSurchargesInfo, "CCC is not a valid Surcharge Code.");

			org.CompanyData.OB_ARApplicableSurcharges = "AAA, ALL";
			AssertHasError(org.CompanyData.OB_ARApplicableSurchargesInfo, "ALL is not a valid Surcharge Code.");

			org.CompanyData.OB_ARApplicableSurcharges = "BBB, NON";
			AssertHasError(org.CompanyData.OB_ARApplicableSurchargesInfo, "NON is not a valid Surcharge Code.");

			org.CompanyData.OB_ARApplicableSurcharges = "AAA?BBB";
			AssertHasError(org.CompanyData.OB_ARApplicableSurchargesInfo, "Applicable Surcharges must be ALL,NON or a comma separated list of valid Surcharge Codes.");

			org.CompanyData.OB_ARApplicableSurcharges = "AAA, BBB, AAA, BBB";
			AssertHasError(org.CompanyData.OB_ARApplicableSurchargesInfo, "Please remove duplicate surcharges: AAA, BBB.");
		}

		public void TestCheckOB_ARTransactionCreationRestriction()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARTransactionCreationRestriction = ZString.Empty;
			AssertHasError(org.CompanyData.OB_ARTransactionCreationRestrictionInfo, "Please enter a Transaction Creation Restriction.");

			org.CompanyData.OB_ARTransactionCreationRestriction = "123";
			AssertHasError(org.CompanyData.OB_ARTransactionCreationRestrictionInfo, "Enter a valid Transaction Creation Restriction.");

			org.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.None;
			AssertNoErrors(org.CompanyData.OB_ARTransactionCreationRestrictionInfo);
		}

		public void TestCheckOB_WhsAllowCreateInvoiceWithNoTransactionsOrStock()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Precondition, Default value.", false, org.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Precondition, Default value.", false, org.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertNoErrors(org.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo);

			org.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = true;
			AssertHasError(org.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo, "Cannot enable Allow Creation of Periodic Invoice jobs without current transactions or existing Inventory if this Organization does not automatically Create Periodic Invoices.");

			org.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = false;
			AssertNoErrors(org.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo);
		}

		public void TestCheckOB_WhsAutoPostPeriodicInvoice()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Precondition, Default value.", false, org.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Precondition, Default value.", false, org.CompanyData.OB_WhsAutoPostPeriodicInvoice);
			AssertNoErrors(org.CompanyData.OB_WhsAutoPostPeriodicInvoiceInfo);

			org.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			AssertHasError(org.CompanyData.OB_WhsAutoPostPeriodicInvoiceInfo, "Cannot enable Auto Post Periodic Invoices if this Organization does not automatically Create Periodic Invoices.");

			org.CompanyData.OB_WhsAutoPostPeriodicInvoice = false;
			AssertNoErrors(org.CompanyData.OB_WhsAutoPostPeriodicInvoiceInfo);
		}

		public void TestCheckOB_WhsAutoDeliverPeriodicInvoice()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Precondition, Default value.", false, org.CompanyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Precondition, Default value.", false, org.CompanyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertNoErrors(org.CompanyData.OB_WhsAutoDeliverPeriodicInvoiceInfo);

			org.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;
			AssertHasError(org.CompanyData.OB_WhsAutoDeliverPeriodicInvoiceInfo, "Cannot enable Auto Deliver Periodic Invoices if this Organization does not automatically Post Periodic Invoices.");

			org.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = false;
			AssertNoErrors(org.CompanyData.OB_WhsAutoDeliverPeriodicInvoiceInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ResetOrgAndCompanyData();
		}

		void ResetOrgAndCompanyData(string code = "XXXYYY")
		{
			Org = Factory.New<OrgHeader>();
			Org.OH_Code = code;
			CompanyData = Org.CompanyData;
		}

		OrgHeader Org;
		OrgCompanyData CompanyData;

		#endregion
	}
}
