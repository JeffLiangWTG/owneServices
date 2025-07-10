using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestFlags()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertEquals("IsEntrySummaryValidationMode", false, invoice.Validation.IsEntrySummaryValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", false, invoice.Validation.IsCargoReleaseValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", false, invoice.Validation.IsFTZAdmissionValidationMode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals("IsEntrySummaryValidationMode", false, invoice.Validation.IsEntrySummaryValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", false, invoice.Validation.IsCargoReleaseValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", true, invoice.Validation.IsFTZAdmissionValidationMode);
		}

		public void TestCheckJZ_OA_ConsigneeAddress()
		{
			declaration.US_EnableAII = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var ultConsignee = Factory.New<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(ultConsignee);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			invoice.JZ_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;

			CombineAssertions(() =>
			{
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertHasMessageError(invoice.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_OA_ConsigneeAddress = Factory.New<OrgHeader>().MainAddress.PK;
				invoice.JZ_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertHasMessageError(invoice.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				invoiceLine.JI_OA_ConsigneeAddress = ZGuid.Empty;
				ultConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
				invoice.JZ_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertNoMessageError(invoice.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				declaration.US_EnableCRL = true;
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertNoMessageError(invoice.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				declaration.US_EntryType = EntryTypeList.Codes.LowValue;
				var newConsignee = Factory.New<OrgHeader>();
				declaration.JE_OA_ConsigneeAddress = newConsignee.MainAddress.PK;
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertHasMessageError(invoice.JZ_OA_ConsigneeAddressInfo, JobComInvoiceHeaderValidation.ConsigneeNotMatchingMessageText);

				declaration.JE_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertNoMessageError(invoice.JZ_OA_ConsigneeAddressInfo, JobComInvoiceHeaderValidation.ConsigneeNotMatchingMessageText);
			});
		}

		public void TestCheckJZ_OA_ConsigneeAddressWithoutENSInInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var ultimateConsignee = Factory.New<OrgHeader>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_OA_ConsigneeAddress = ultimateConsignee.PK;

			CombineAssertions(() =>
			{
				AssertHasMessageError(invoiceHeader.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "89745387");
				invoiceHeader.JZ_OA_ConsigneeAddress = ultimateConsignee.PK;
				AssertNoMessageError(invoiceHeader.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				declaration.US_EnableENS = false;
				declaration.US_EnableCRL = true;
				ultimateConsignee.CustomsCodes.RemoveAndDeleteAll();
				invoiceHeader.JZ_OA_ConsigneeAddress = ZGuid.Empty;
				invoiceHeader.JZ_OA_ConsigneeAddress = ultimateConsignee.PK;
				AssertNoMessageError(invoiceHeader.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "89745387");
				invoiceHeader.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertNoMessageError(invoiceHeader.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);
			});
		}

		public void TestCheckJZ_OA_ConsigneeAddress_WhenCreatedFromUSLowValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableAII = true;
			declaration.US_EnableENS = true;

			var ultConsignee = Factory.New<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(ultConsignee);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;

			CombineAssertions(() =>
			{
				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertHasMessageError("Not created from USLV", invoice.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);

				declaration.Logs.AddNew(AutoEvents.Transferred,
				[
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "USLV"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "LV001")
				]);

				invoice.Validation.ValidateJZ_OA_ConsigneeAddress();
				AssertNoMessageError("Created from USLV", invoice.JZ_OA_ConsigneeAddressInfo, EIN_SSN_CBNCodeRequiredMessageError);
			});
		}

		public void TestCheckJZ_OH_ConsigneeOrganisationCountryForMyanmar()
		{
			var org1 = CreateNewOrg("ORG1", "ORG1 ADDRESS", "MARY1", "AU");
			var org2 = CreateNewOrg("ORG2", "ORG2 ADDRESS", "MARY2", "MM");
			var org3 = CreateNewOrg("ORG3", "ORG3 ADDRESS", "MARY3", "BU");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = new ZDateTime(2019, 09, 01);
			declaration.JE_OH_Consignee = org1.PK;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OH_Consignee = org2.PK;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertNoWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			declaration.JE_OH_Consignee = org2.PK;
			declaration.Validation.ValidateJE_OH_Consignee();
			AssertHasWarning(declaration.JE_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			invoice1.Validation.ValidateJZ_OH_Consignee();
			AssertHasWarning(invoice1.JZ_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			declaration.US_DateOfExport = new ZDateTime(2019, 09, 21);
			invoice1.Validation.ValidateJZ_OH_Consignee();
			AssertNoWarning(invoice1.JZ_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			invoice1.JZ_OH_Consignee = org3.PK;
			invoice1.Validation.ValidateJZ_OH_Consignee();
			AssertHasWarning(invoice1.JZ_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			declaration.US_DateOfExport = new ZDateTime(2019, 09, 01);
			invoice1.Validation.ValidateJZ_OH_Consignee();
			AssertNoWarning(invoice1.JZ_OH_ConsigneeInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
		}

		public void TestCheckJZ_IncoTermPlace()
		{
			invoiceHeader.JZ_IncoTerm = TermsOfDeliveryList.Codes.DAP;
			invoiceHeader.JZ_IncoTermPlace = "123";
			AssertHasWarning(invoiceHeader.JZ_IncoTermPlaceInfo, JobComInvoiceHeaderValidation.ApplyForAgreedPlaceMessageText);
			invoiceHeader.JZ_IncoTermPlace = "";
			AssertNoWarning(invoiceHeader.JZ_IncoTermPlaceInfo, JobComInvoiceHeaderValidation.ApplyForAgreedPlaceMessageText);
			invoiceHeader.JZ_IncoTerm = TermsOfDeliveryList.Codes.CPT;
			invoiceHeader.JZ_IncoTermPlace = "123";
			AssertNoWarning(invoiceHeader.JZ_IncoTermPlaceInfo, JobComInvoiceHeaderValidation.ApplyForAgreedPlaceMessageText);
			invoiceHeader.JZ_IncoTerm = TermsOfDeliveryList.Codes.CIP;
			invoiceHeader.JZ_IncoTermPlace = "123";
			AssertNoWarning(invoiceHeader.JZ_IncoTermPlaceInfo, JobComInvoiceHeaderValidation.ApplyForAgreedPlaceMessageText);
		}

		public override void TestValidateJZ_CU_RelatedHouseBill()
		{
			var houseBill = invoiceHeader.JobDeclaration.Bills.AddNew();
			invoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			var entry1 = invoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			var entry2 = invoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			invoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			Assert(!invoiceHeader.JZ_CU_RelatedHouseBillInfo.HasMessageErrors());
		}

		public void TestExchangeRateValidation()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			audCurr.ExchangeRates.DeleteAll();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_DateOfExport = new ZDateTime(2012, 1, 1);
			Assert(declaration.ValuationDatesChanged);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = audCurr.RX_Code;
			Assert(!invoice.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
			declaration.ResumeApportionment();
			Assert(invoice.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
			audCurr.SetUpExchangeRates(new ZDateTime(2011, 12, 31), 1.05m);
			declaration.RefreshExchangeRates();
			Assert(!invoice.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
		}

		public void TestCheckJZ_CU_RelatedHouseBill()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
			AssertHasMessageErrorContaining(invoice.JZ_CU_RelatedHouseBillInfo, MandatoryValidation.YouHaveNotEntered);
			var bill = declaration.Bills.AddNew();
			var childBill = bill.ChildBills.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			AssertNoMessageErrorContaining(invoice.JZ_CU_RelatedHouseBillInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			declaration.Bills.RemoveAndDeleteAll();
			var masterBillOne = declaration.Bills.CreatePrimaryBill(BillTypeList.Codes.MasterBill);
			masterBillOne.CU_BillNum = "1111111";
			invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
			AssertNoMessageErrorContaining(invoice.JZ_CU_RelatedHouseBillInfo, MandatoryValidation.YouHaveNotEntered);
			var masterBillTwo = declaration.Bills.CreatePrimaryBill(BillTypeList.Codes.MasterBill);
			masterBillTwo.CU_BillNum = "2222222";
			invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
			AssertHasMessageErrorContaining(invoice.JZ_CU_RelatedHouseBillInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckEffectiveValuationDate()
		{
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "JJJ";
			currency.RX_Desc = "Dummy Currency";
			currency.SetUpExchangeRates(new ZDateTime(2012, 3, 12), 1.05m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_LatestRateDate = new ZDateTime(2012, 03, 07);
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 13);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";
			currency.SetUpExchangeRates(new ZDateTime(2012, 3, 13), 1.05m);
			invoice.Validation.ValidateEffectiveValuationDate();
			AssertHasWarning(invoice.EffectiveValuationDateInfo, string.Format(JobComInvoiceHeaderValidation.ThereAreMoreRecentRatesAvailable, new ZDateTime(2012, 3, 12).ToString("dd-MM-yy"), new ZDateTime(2012, 3, 13).ToString("dd-MM-yy")));
			declaration.RefreshExchangeRates();
			invoice.Validation.ValidateEffectiveValuationDate();
			AssertNoWarning(invoice.EffectiveValuationDateInfo, string.Format(JobComInvoiceHeaderValidation.ThereAreMoreRecentRatesAvailable, new ZDateTime(2012, 3, 12).ToString("dd-MM-yy"), new ZDateTime(2012, 3, 13).ToString("dd-MM-yy")));
		}

		public void TestCheckJZ_Calc_TNI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			declaration.ResumeApportionment();
			AssertHasMessageError(invoice.JZ_Calc_TNIInfo, string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoice.EnteredValueThresholdForCharges));
			declaration.TopGroupInvoice.Charges.AddNew("OFT", 10m, "USD");
			declaration.ResumeApportionment();
			AssertNoMessageError(invoice.JZ_Calc_TNIInfo, string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoice.EnteredValueThresholdForCharges));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.TopGroupInvoice.Charges.RemoveAll();
			declaration.ResumeApportionment();
			AssertNoMessageError(invoice.JZ_Calc_TNIInfo, string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoice.EnteredValueThresholdForCharges));
			invoice.JZ_InvoiceAmount = 2500m;
			declaration.ResumeApportionment();
			AssertNoMessageError(invoice.JZ_Calc_TNIInfo, string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoice.EnteredValueThresholdForCharges));
			invoice.JZ_InvoiceAmount = 2501m;
			declaration.ResumeApportionment();
			AssertHasMessageError(invoice.JZ_Calc_TNIInfo, string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoice.EnteredValueThresholdForCharges));
			declaration.TopGroupInvoice.Charges.AddNew("OFT", 1m, "USD");
			declaration.ResumeApportionment();
			AssertNoMessageError(invoice.JZ_Calc_TNIInfo, string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoice.EnteredValueThresholdForCharges));
		}

		public void TestDuplicateInvoiceNumber()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			GlbCompany nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "~NZ";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			GlbBranch nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "~NZ";
			GlbCompany usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "~US";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			GlbBranch usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "~US";
			BaseJobDeclaration nzDeclaration = Factory.New<BaseJobDeclaration>();
			nzDeclaration.JE_GB = nzBranch.PK;
			nzDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			nzDeclaration.JE_OH_Supplier = supplier.PK;
			BaseJobComInvoiceHeader nzInvoice = nzDeclaration.Invoices.AddNew();
			nzInvoice.JZ_InvoiceNumber = "-234";
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = usBranch.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			declaration.JE_OH_Supplier = supplier.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "-234";
			AssertNoWarnings(invoice.JZ_InvoiceNumberInfo);
			Factory.Save();
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.US_EnableAII = true;
			declaration2.JE_GB = usBranch.PK;
			JobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_OH_Supplier = supplier.PK;
			invoice2.JZ_InvoiceNumber = "-234";
			AssertHasWarningContaining(invoice2.JZ_InvoiceNumberInfo, Enterprise.Customs.Business.Testing.BaseInvoiceHeaderValidationTest.DuplicateInvoiceNumberWithOtherDec);
			OrgHeader newSupplier = Factory.New<OrgHeader>();
			invoice2.JZ_OH_Supplier = newSupplier.PK;
			invoice2.JZ_InvoiceNumber = "-234";
			AssertNoWarningContaining(invoice2.JZ_InvoiceNumberInfo, Enterprise.Customs.Business.Testing.BaseInvoiceHeaderValidationTest.DuplicateInvoiceNumberWithOtherDec);
			declaration2.JE_OH_Supplier = supplier.PK;
			invoice2.JZ_OH_Supplier = ZGuid.Empty;
			invoice2.JZ_InvoiceNumber = "-234";
			AssertHasWarningContaining(invoice2.JZ_InvoiceNumberInfo, Enterprise.Customs.Business.Testing.BaseInvoiceHeaderValidationTest.DuplicateInvoiceNumberWithOtherDec);
		}

		public void TestCheckJZ_WeightUQ()
		{
			invoice.JZ_WeightUQ = "~";
			AssertHasMessageError(invoice.JZ_WeightUQInfo, JobComInvoiceHeaderValidation.WeightUQShouldBeInList);
			invoice.JZ_WeightUQ = invoice.Lookups.JZ_WeightUQ_List[0].Code;
			AssertNoMessageError(invoice.JZ_WeightUQInfo, JobComInvoiceHeaderValidation.WeightUQShouldBeInList);
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1".PadRight(18, '1');
			AssertNoWarnings(invoice.JZ_InvoiceNumberInfo);
			invoice.JZ_InvoiceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JZ_InvoiceNumber = "1";
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestValidateBalance()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100;
			invoice.JZ_InvoiceAmount = 1000m;
			declaration.ResumeApportionment();
			AssertHasWarning(invoice.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
			invoice.JZ_InvoiceAmount = 100m;
			declaration.ResumeApportionment();
			AssertNoWarning(invoice.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
			invoice.JZ_InvoiceAmount = 0m;
			declaration.ResumeApportionment();
			AssertHasWarning(invoice.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_InvoiceAmount = 1000m;
			declaration.ResumeApportionment();
			AssertHasMessageError(invoice.JZ_Calc_BalanceInfo, Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
		}

		public void TestPOARequiresForExport()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			string countrySpecificNameForPOA = new AuthorityToActValidator().CountrySpecificNameForPOA;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertHasWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			JobRequiredDocument poaDocument = supplier.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			poaDocument.EQ_ValidToDate = ZDate.Today.AddMonths(10);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			AssertNoMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			JobRequiredDocAttrib attrib = poaDocument.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertHasWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
			poaDocument.Delete();
			poaDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			poaDocument.Delete();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
		}

		public override void TestValidateAbsenceOfOFTOrONS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.RunPreSaveValidation();
			AssertEquals(false, invoice.JZ_Calc_CIFAmountInfo.HasNotifications());
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_InvoiceAmount = 1000m;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_InvoiceAmount = 0m;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice.US_PaymentTerms = PaymentTermsTypeList.Codes.NoCharge;
			AssertEquals("IsNoChargeInvoice", true, invoice.IsNoCharge);
			invoice.JZ_InvoiceAmount = 0m;
			invoice.RunPreSaveValidation();
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoice.JZ_InvoiceAmount = 0m;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.US_PaymentTerms = PaymentTermsTypeList.Codes.EndOfMonth;
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoice.JZ_InvoiceAmount = 0m;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.PuertoRico;
			invoice.JZ_InvoiceAmount = 1m;
			invoice.JZ_InvoiceAmount = 0m;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = false;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoice.JZ_InvoiceAmount = 0m;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestAddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStaleReturnsMessageErrorForUS()
		{
			var jjjCurrency = Factory.NewWithValidTestData<RefCurrency>();
			jjjCurrency.RX_Code = "JJJ";
			jjjCurrency.RX_Desc = "Dummy Currency";
			var exchRate1 = jjjCurrency.ExchangeRates.AddNew();
			exchRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate1.RE_RX_NKExCurrency = jjjCurrency.RX_Code;
			exchRate1.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_SellRate = 1.5555m;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";
			AssertEquals("JZ_InvoiceCurrExRate", 1.5555m, invoice.JZ_InvoiceCurrExRate);
			AssertNoMessageError(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");
			var exchRate2 = jjjCurrency.ExchangeRates.AddNew();
			exchRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate2.RE_RX_NKExCurrency = jjjCurrency.RX_Code;
			exchRate2.RE_StartDate = ZDateTime.Today;
			exchRate2.RE_ExpiryDate = ZDateTime.Today;
			exchRate2.RE_SellRate = 1.6111m;
			declaration.JE_ExportDate = ZDateTime.Today;
			invoice.JZ_InvoiceCurrExRate = 1.5555m;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertHasMessageError(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");
			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";
			AssertEquals("JZ_InvoiceCurrExRate", 1.6111m, invoice.JZ_InvoiceCurrExRate);
			AssertNoMessageError("Exchange rate should have been updated to today's rate", invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");
		}

		[TestDate(2009, 12, 1)]
		public void TestCheckJZ_OH_Supplier()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, "Enter a valid USPPI.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, "Enter a valid USPPI.");
			invoice.USPPIDocAddress.E2_AddressOverride = true;
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, "Enter a valid USPPI.");
			invoice.USPPIDocAddress.E2_AddressOverride = false;
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, "Enter a valid USPPI.");
			invoice.JZ_OH_Supplier = ZGuid.Invalid;
			AssertHasErrorContaining(invoice.JZ_OH_SupplierInfo, ListValidation.InvalidCodeError);

			AssertUSPPIMustHaveEIN(Core.Constants.CountryCodes.UnitedStates);
			AssertUSPPIMustHaveEIN(Core.Constants.CountryCodes.PuertoRico);
			AssertUSPPIMustHaveEIN(Core.Constants.CountryCodes.VirginIslands);

			AssertUSPPIMustHaveEitherEINOrFRNOrDuns(Core.Constants.CountryCodes.Australia);
			AssertUSPPIMustHaveEitherEINOrFRNOrDuns(Core.Constants.CountryCodes.Canada);
			AssertUSPPIMustHaveEitherEINOrFRNOrDuns(Core.Constants.CountryCodes.SouthAfrica);
		}

		void AssertUSPPIMustHaveEIN(ZString countryCode)
		{
			var consignor = new DeclarationTestHelper(Factory).Consignor;
			consignor.MainAddress.OA_RN_NKCountryCode = countryCode;
			consignor.CustomsCodes.RemoveAndDeleteAll();
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoErrorContaining(invoice.JZ_OH_SupplierInfo, ListValidation.InvalidCodeError);
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
			var cusCode = consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignRegistrationNumber;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
			consignor.CustomsCodes.RemoveAndDeleteAll();
			cusCode = consignor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
			consignor.CustomsCodes.RemoveAndDeleteAll();
			consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
			consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "12345678910");
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
		}

		void AssertUSPPIMustHaveEitherEINOrFRNOrDuns(ZString countryCode)
		{
			var consignor = new DeclarationTestHelper(Factory).Consignor;
			consignor.MainAddress.OA_RN_NKCountryCode = countryCode;
			consignor.CustomsCodes.RemoveAndDeleteAll();
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoErrorContaining(invoice.JZ_OH_SupplierInfo, ListValidation.InvalidCodeError);
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			var cusCode = consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignRegistrationNumber;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			consignor.CustomsCodes.RemoveAndDeleteAll();
			cusCode = consignor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			consignor.CustomsCodes.RemoveAndDeleteAll();
			consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			invoice.JZ_OH_Supplier = consignor.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
		}

		public void TestSLIGetValidatedFirstBeforeOrgPOA()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			var supplier = Factory.New<OrgHeader>();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = dec.Invoices.AddNew();
			var poa = supplier.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poa.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poa.EQ_DateReceived = ZDateTimeOffset.Today.AddYears(-1);
			poa.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
			var sli = dec.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.ShippersLetterOfInstruction);
			sli.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			sli.EQ_DateReceived = ZDateTimeOffset.Today;
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertHasWarning(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAButHasShippersLetterOfInstruction);
		}

		public void TestValidateHasShippersLetterOfInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			var header = Factory.New<JobComInvoiceHeader>();
			header.JZ_JE = declaration.PK;
			header.JZ_OH_Supplier = supplier.PK;
			using (header.SuspendValidationTesting())
			{
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				JobRequiredDocument poaDocument1 = header.Supplier.RequiredDocuments.AddNew("POA");
				poaDocument1.EQ_DocDescription = "Power of Attorney";
				poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument1.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument1.EQ_ValidToDate = ZDateTime.Today;
				header.Validation.ValidateJZ_OH_Supplier();
				AssertNoWarning(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAButHasShippersLetterOfInstruction);
				AssertNoMessageError(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
				header.ClearAllNotifications();
				var attrib = poaDocument1.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
				header.Validation.ValidateJZ_OH_Supplier();
				AssertNoWarning(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAButHasShippersLetterOfInstruction);
				AssertHasMessageError(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
				header.ClearAllNotifications();
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
				header.Validation.ValidateJZ_OH_Supplier();
				AssertNoWarning(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAButHasShippersLetterOfInstruction);
				AssertNoMessageError(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
				AssertNoMessageError(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.InvalidPOAAndNoShippersLetterOfInstruction);
				header.ClearAllNotifications();
				header.Supplier.RequiredDocuments.RemoveAndDeleteAll();
				header.Validation.ValidateJZ_OH_Supplier();
				AssertNoWarning(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAButHasShippersLetterOfInstruction);
				AssertHasMessageError(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
				header.ClearAllNotifications();
				Freight.Forwarding.Business.ForwardingShipment shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
				JobRequiredDocument sliDocument1 = declaration.Shipment.DocsAndCartage.RequiredDocuments.AddNew("SLI");
				sliDocument1.EQ_DocType = "SLI";
				sliDocument1.EQ_DocDescription = "Shippers Letter of Instruction 1";
				sliDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				sliDocument1.EQ_DateReceived = ZDateTimeOffset.Empty;
				sliDocument1.EQ_ValidToDate = ZDateTime.Today;
				JobRequiredDocument sliDocument2 = declaration.DocsAndCartage.RequiredDocuments.AddNew("SLI");
				sliDocument2.EQ_DocType = "SLI";
				sliDocument2.EQ_DocDescription = "Shippers Letter of Instruction 2";
				sliDocument2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				sliDocument2.EQ_DateReceived = ZDateTimeOffset.Empty;
				sliDocument2.EQ_ValidToDate = ZDateTime.Today;
				header.Validation.ValidateJZ_OH_Supplier();
				AssertHasWarning(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAButHasShippersLetterOfInstruction);
				AssertNoMessageError(header.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.NoPOAAndNoShippersLetterOfInstruction);
				header.ClearAllNotifications();
			}
		}

		public void TestCheckJZ_OH_Buyer()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JZ_OH_Buyer = ZGuid.Invalid;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			invoice.JZ_OH_Buyer = ZGuid.Invalid;
			AssertHasMessageError(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.UltimateConsigneeEnteredWhenSoldOnRoute);
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;
			invoice.JZ_OH_Buyer = ZGuid.Invalid;
			AssertNoMessageError(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.UltimateConsigneeEnteredWhenSoldOnRoute);
			invoice.UltimateConsigneeDocAddress.E2_AddressOverride = true;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			var factory = new BusinessObjectFactory();
			declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableSPN = true;
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			invoice = declaration.Invoices.AddNew();
			FDAOrganisationValidatorTest.AssertOrganisation(invoice.JZ_OH_BuyerInfo, (ZGuid organisationPK) => invoice.JZ_OH_Buyer = organisationPK);

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Export;
			var validation = invoiceHeader.Validation;
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateJZ_OH_Buyer();
			});
		}

		public void TestCheckJZ_OH_SupplierWithCountryCodeMM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 1);
			var org1 = CreateNewOrg("ORG1", "ORG1 ADDRESS", "MARY1", "MM");
			var org2 = CreateNewOrg("ORG2", "ORG2 ADDRESS", "MARY2", "BU");
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = org1.PK;
			AssertHasWarning(invoice.JZ_OH_SupplierInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 21);
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertNoWarning(invoice.JZ_OH_SupplierInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			invoice.JZ_OH_Supplier = org2.PK;
			AssertHasWarning(invoice.JZ_OH_SupplierInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 1);
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertNoWarning(invoice.JZ_OH_SupplierInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 21);
			invoice.USPPIDocAddress.E2_AddressOverride = false;
			Assert(!invoice.JZ_OH_Supplier.IsEmpty);
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertHasWarning(invoice.JZ_OH_SupplierInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			invoice.USPPIDocAddress.E2_AddressOverride = true;
			Assert(invoice.JZ_OH_Supplier.IsEmpty);
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertNoWarning(invoice.JZ_OH_SupplierInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
		}

		public void TestCheckJZ_OH_BuyerWithCountryCodeMM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 1);
			var org1 = CreateNewOrg("ORG1", "ORG1 ADDRESS", "MARY1", "MM");
			var org2 = CreateNewOrg("ORG2", "ORG2 ADDRESS", "MARY2", "BU");
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = org1.PK;
			AssertHasWarning(invoice.JZ_OH_BuyerInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 21);
			invoice.Validation.ValidateJZ_OH_Buyer();
			AssertNoWarning(invoice.JZ_OH_BuyerInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);
			invoice.JZ_OH_Buyer = org2.PK;
			AssertHasWarning(invoice.JZ_OH_BuyerInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 1);
			invoice.Validation.ValidateJZ_OH_Buyer();
			AssertNoWarning(invoice.JZ_OH_BuyerInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);
		}

		public void TestCheckJZ_OA_ManufacturerAddress()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			AssertJZ_OA_ManufacturerAddress(declaration);
		}

		public void TestCheckJZ_OA_ManufacturerAddressForPostalCodeRequiredForMF()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				manufacturer.MainAddress.OA_PostCode = ZString.Empty;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = false;
				invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_EnableCRL = true;
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_CertifyCargoRelease = true;
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "123456";
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "ABCDEF";
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				manufacturer.MainAddress.OA_PostCode = ZString.Empty;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = false;
				invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_EnableCRL = true;
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertHasMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_CertifyCargoRelease = true;
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertHasMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "123456";
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "ABCDEF";
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertHasMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
			}
		}

		public void TestCheckJZ_OA_ManufacturerAddressForBorderCargoRelease()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			AssertEquals("IsCargoReleaseValidationMode", true, declaration.IsCargoReleaseValidationMode);
			AssertJZ_OA_ManufacturerAddress(declaration);
		}

		public void TestValidateJZ_InvoiceNumberForDuplicateNo()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "ORG1";
			var declarationForReleaseEntry = Factory.NewWithValidTestData<JobDeclaration>();
			declarationForReleaseEntry.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationForReleaseEntry.JE_DeclarationReference = "B001";
			declarationForReleaseEntry.JE_OH_Supplier = org1.PK;
			declarationForReleaseEntry.US_EntryFilerCode = "SV9";
			declarationForReleaseEntry.ImportEntryNumber = "123";
			var header1 = declarationForReleaseEntry.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "INV001";
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B002";
			declaration.JE_OH_Supplier = org1.PK;
			var header2 = declaration.Invoices.AddNew();
			header2.JZ_InvoiceNumber = "INV001";
			AssertEquals("Warning Exists", true, header2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("This invoice number already exists in job(s): "));
			declaration.Invoices.RemoveAll();
			var declarationsForReleaseEntries = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationsForReleaseEntries.Add(declarationForReleaseEntry);
			new ReleaseEntryInvoiceRetriever(declaration).ImportDeclarations(declarationsForReleaseEntries);
			var header3 = (JobComInvoiceHeader)declaration.Invoices.FirstOrDefault();
			AssertEquals("INV001", header3.JZ_InvoiceNumber);
			header3.Validation.ValidateJZ_InvoiceNumber();
			AssertEquals("Warning Exists", false, header3.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("This invoice number already exists in job(s): "));
		}

		public void TestCheckJZ_OA_SupplierAddress_ForRoutedTransaction()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			helper.ShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000", Core.Constants.CountryCodes.UnitedStates);
			var orgHeader = Factory.New<OrgHeader>();
			OrgAddress orgAddress = helper.ShippingLine.MainAddress;
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_RN_NKCountryCode = "US";
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_RL_NKRelatedPortCode = "USLAX";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "12345";
			orgAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			invoice.USPPIDocAddress.E2_Contact = "TestSupplierCOntact Name";
			invoice.USPPIDocAddress.E2_Phone_Formatted = "61 5798-4578";
			OrgAddress orgAddress1 = helper.ShippingLine.Addresses.AddNew();
			orgAddress1.OA_OH = orgHeader.PK;
			orgAddress1.OA_RN_NKCountryCode = "AU";
			orgAddress1.OA_Address1 = "AU ADDRESS 1";
			orgAddress1.OA_City = "SYDNEY";
			orgAddress1.OA_RL_NKRelatedPortCode = "AUNSW";
			orgAddress1.OA_State = "NSW";
			orgAddress1.OA_PostCode = "66574";
			invoice.JZ_OA_SupplierAddress = orgAddress.PK;
			orgAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			orgAddress.OA_RN_NKCountryCode = "US";
			invoice.JZ_OA_SupplierAddress = orgAddress.PK;
			Assert(!invoice.JZ_OA_SupplierAddressInfo.HasMessageErrors());
		}

		public override void TestValidateJZ_OH_Supplier()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			BaseJobDeclaration declaration = invoiceHeader.JobDeclaration;
			AssertNotNull("Precondition: invoiceHeader.JobDeclaration", declaration);
			invoiceHeader.JZ_JE = ZGuid.Empty;
			new FakeDeclarationCreatorForInvoice(invoiceHeader);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasErrorContaining(invoiceHeader.JZ_OH_SupplierInfo, MandatoryValidation.MustBeEntered);
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertNoErrorContaining(invoiceHeader.JZ_OH_SupplierInfo, MandatoryValidation.MustBeEntered);
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoError(invoiceHeader.JZ_OH_SupplierInfo, InvoiceHeaderValidation.ErrorCannotUseMISCOnCommercialInvoiceHeader);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasError(invoiceHeader.JZ_OH_SupplierInfo, InvoiceHeaderValidation.ErrorCannotUseMISCOnCommercialInvoiceHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			invoice = declaration.Invoices.AddNew();
			helper = new DeclarationTestHelper(Factory);
		}

		new JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		DeclarationTestHelper helper;

		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		OrgHeader CreateNewOrg(ZString companyName, ZString address, ZString contactName, ZString countryCode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = companyName;
			result.MainAddress.OA_Address1 = address;
			result.MainAddress.OA_RN_NKCountryCode = countryCode;
			result.OH_RL_NKClosestPort = countryCode + "PT";
			var contact = result.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			return result;
		}

		void AssertJZ_OA_ManufacturerAddress(JobDeclaration declaration)
		{
			invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			OrgHeader party = Factory.New<OrgHeader>();
			OrgAddress mainAddress = party.MainAddress;
			invoice.JZ_OA_ManufacturerAddress = mainAddress.PK;
			string messageError = string.Format(OrganisationValidation.ManufacturerIDMissing, mainAddress.OA_Code);
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);
			OrgCusCode cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoice.JZ_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, messageError);
			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XY;
			invoice.US_UC_NKCountryOfExport = "CA";
			invoice.JZ_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(invoice.JZ_OA_ManufacturerAddressInfo, ManufacturerIDValidator.Constants.Canadian);
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XB;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XY;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			cusCode.OK_CustomsRegNo = "XY1";
			invoice.JZ_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
		}

		string EIN_SSN_CBNCodeRequiredMessageError => string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee");
	}
}
