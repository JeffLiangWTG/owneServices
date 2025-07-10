using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestAddInfoSynchronisation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKCountryOfExport = "AU";
			Factory.Save();

			var addInfoHash = AddInfoParser.CreateDictionaryWithAddInfoString(invoice.JZ_AddInfo);
			AssertEquals("Country of Export", "AU", addInfoHash["RN_NKCountryOfExport"]);
		}

		public void TestSetSupplierGSTNumberWhenSupplierChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(ZString.Empty, invoice.JZ_SupplierGSTNumber);

			var supplier4 = Factory.New<OrgHeader>();
			supplier4.OH_Code = "SP4";
			var orgCusCode4 = supplier4.CustomsCodes.AddNew();
			orgCusCode4.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			orgCusCode4.OK_CustomsRegNo = "1234567890123456";
			orgCusCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			AssertNoExceptionThrown(delegate
			{ invoice.JZ_OH_Supplier = supplier4.PK; });
			AssertEquals("", invoice.JZ_SupplierGSTNumber);

			var supplier5 = Factory.New<OrgHeader>();
			supplier5.OH_Code = "SP5";
			var orgCusCode5 = supplier5.CustomsCodes.AddNew();
			orgCusCode5.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			orgCusCode5.OK_CustomsRegNo = "123456789012345";
			orgCusCode5.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			invoice.JZ_OH_Supplier = supplier5.PK;
			AssertEquals("", invoice.JZ_SupplierGSTNumber);

			var supplier6 = Factory.New<OrgHeader>();
			supplier6.OH_Code = "SP6";
			var orgCusCode6 = supplier6.CustomsCodes.AddNew();
			orgCusCode6.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			orgCusCode6.OK_CustomsRegNo = "123456785";
			orgCusCode6.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			invoice.JZ_OH_Supplier = supplier6.PK;
			AssertEquals("123456785", invoice.JZ_SupplierGSTNumber);

			invoice.JZ_SupplierGSTNumber = "ZZZ000";

			var supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_Code = "SP3";
			var orgCusCode3 = supplier3.CustomsCodes.AddNew();
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			orgCusCode3.OK_CustomsRegNo = "123456785";
			orgCusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			invoice.JZ_OH_Supplier = supplier3.PK;
			AssertEquals("ZZZ000", invoice.JZ_SupplierGSTNumber);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			customsChargeTypeList = new CustomsChargeTypeList(true);
			customsChargeTypeList.Sort();
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			commonInvoice = Factory.New<JobComInvoiceHeader>();
			customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestExportJob_DefaultCurrencyIndicator()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var declaration = InvoiceHeader.JobDeclaration;

			var invoiceHeader1 = InvoiceHeader;
			AssertEquals("invoiceHeader1 currency is NZD", Core.Constants.CurrencyCodes.NewZealand, invoiceHeader1.JZ_RX_NKInvoice_Currency);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			AssertEquals("New added invoice default currency is the local currency", JobDeclaration.LocalCurrencyConstantCode, invoiceHeader2.JZ_RX_NKInvoice_Currency);
			AssertEquals("New added invoice default currency is NZD", Core.Constants.CurrencyCodes.NewZealand, invoiceHeader2.JZ_RX_NKInvoice_Currency);

			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			AssertEquals("Invoice 3 currency indicator is defaulted to the same as invoice 2", invoiceHeader2.JZ_ExchangeRateIndicator, invoiceHeader3.JZ_ExchangeRateIndicator);

			var invoiceHeader4 = declaration.Invoices.AddNew();
			invoiceHeader4.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals("invoice 4 currency indicator is defaulted to NZD", ExchangeRateIndicatorList.Codes.NZD, invoiceHeader4.JZ_ExchangeRateIndicator);
		}

		public void TestNonExportJob_DefaultCurrencyIndicator()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration = InvoiceHeader.JobDeclaration;

			var invoiceHeader1 = InvoiceHeader;
			AssertEquals("invoiceHeader1 currency is NZD", Core.Constants.CurrencyCodes.NewZealand, invoiceHeader1.JZ_RX_NKInvoice_Currency);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			AssertEquals("New added invoice default currency is the local currency", JobDeclaration.LocalCurrencyConstantCode, invoiceHeader2.JZ_RX_NKInvoice_Currency);
			AssertEquals("New added invoice default currency is NZD", Core.Constants.CurrencyCodes.NewZealand, invoiceHeader2.JZ_RX_NKInvoice_Currency);

			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			Assert("Invoice 3 currency indicator is defaulted to empty for an import job", invoiceHeader3.JZ_ExchangeRateIndicator.IsEmpty);

			var invoiceHeader4 = declaration.Invoices.AddNew();
			invoiceHeader4.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			Assert("Invoice 4 currency indicator is defaulted to empty for an import job", invoiceHeader3.JZ_ExchangeRateIndicator.IsEmpty);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.NewZealand;

		public override void TestNotSupportedExceptionOnSupplierNameSetter()
		{
			AssertNoExceptionThrown(delegate
			{ InvoiceHeader.SupplierName = "FERD"; });
		}

		public void TestUseForwardCoverExchangeRateEntered()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = Declaration.Invoices.AddNew();
			AssertEquals("GetExchangeRate - will be local currency @ 1.00", 1.00m, invoice.EffectiveExchangeRateForInvoiceCurr);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			invoice.JZ_InvoiceCurrExRate = 1.37m;
			AssertEquals("GetExchangeRate should now pick up fixed rate value", 1.37m, invoice.EffectiveExchangeRateForInvoiceCurr);
		}

		public void TestMaxSupplierNameLengthIsNotGreaterThanAddInfoField()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("invoiceHeader.SupplierNameInfo.MaxLength >= org.OH_FullNameInfo.MaxLength", true, invoiceHeader.SupplierNameInfo.MaxLength >= org.OH_FullNameInfo.MaxLength);
		}

		public override void TestIWeightApportioneeRoundingIssue()
		{
			RefCurrency foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, JobDeclaration.LocalCurrencyConstantCode));
			foreignCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.8119m);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice1.JZ_InvoiceAmount = 8000m;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice2.JZ_InvoiceAmount = 2000m;

			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);
		}

		public void TestAdditionalSupplierNameFunctionality()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;
			OrgHeader miscOrganisation = Factory.Load<OrgHeader>(miscOrgPK);
			AssertNotNull("Precondition: miscOrganisation", miscOrganisation);

			JobComInvoiceHeader invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			invoice.JZ_OH_Supplier = miscOrgPK;
			invoice.SupplierName = "MISC Supplier";
			AssertEquals("invoice.SupplierName", miscOrganisation.OH_FullName, invoice.SupplierName);
			AssertEquals("invoice.SupplierNameInfo.ReadOnly", true, invoice.SupplierNameInfo.ReadOnly);
			AssertEquals("invoice.MiscSupplierName", "", invoice.MiscSupplierName);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			invoice.SupplierName = "MISC Supplier";
			AssertEquals("invoice.SupplierName", "MISC SUPPLIER", invoice.SupplierName);
			AssertEquals("invoice.SupplierNameInfo.ReadOnly", false, invoice.SupplierNameInfo.ReadOnly);
			AssertEquals("invoice.MiscSupplierName", "MISC SUPPLIER", invoice.MiscSupplierName);

			OrgHeader testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "GUNNING DOWN MARGHERITAS";
			invoice.JZ_OH_Supplier = testOrg.PK;
			AssertEquals("invoice.SupplierName", "GUNNING DOWN MARGHERITAS", invoice.SupplierName);
			AssertEquals("invoice.SupplierNameInfo.ReadOnly", true, invoice.SupplierNameInfo.ReadOnly);
			AssertEquals("invoice.MiscSupplierName", "", invoice.MiscSupplierName);
		}

		public void TestMiscSupplier()
		{
			ZGuid miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OH_Supplier = miscOrgPK;
			invoice.MiscSupplierName = "MISC Supplier";

			AssertEquals(miscOrgPK, invoice.JZ_OH_Supplier);
			AssertEquals("MISC SUPPLIER", invoice.MiscSupplierName);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("", invoice.MiscSupplierName);
		}

		public void TestNullDeclarationDoesntCauseDescriptionsToBlow()
		{
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertNoExceptionThrown(delegate
			{ string dummy = invoiceHeader.EffectiveIsZeroRatedDutyAsString; });
			AssertNoExceptionThrown(delegate
			{ string dummy = invoiceHeader.EffectiveIsZeroRatedExciseAsString; });
			AssertNoExceptionThrown(delegate
			{ string dummy = invoiceHeader.EffectiveIsZeroRatedGSTAsString; });
			AssertNoExceptionThrown(delegate
			{ string dummy = invoiceHeader.EffectiveIsZeroRatedLeviesAsString; });
		}

		public void TestSettingTheInvoiceCurrencyDefaultsTheCurrencyIndicator()
		{
			RefCurrency currencyNZ = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.NewZealand);
			RefCurrency currencyAU = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ZString.Empty, InvoiceHeader.JZ_ExchangeRateIndicator);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZ.RX_Code;
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ExchangeRateIndicatorList.Codes.NZD, InvoiceHeader.JZ_ExchangeRateIndicator);

			InvoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZ.RX_Code;
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ExchangeRateIndicatorList.Codes.ForwardCover, InvoiceHeader.JZ_ExchangeRateIndicator);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = "XXX";
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ExchangeRateIndicatorList.Codes.ForwardCover, InvoiceHeader.JZ_ExchangeRateIndicator);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyAU.RX_Code;
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ExchangeRateIndicatorList.Codes.ForwardCover, InvoiceHeader.JZ_ExchangeRateIndicator);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZ.RX_Code;
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ExchangeRateIndicatorList.Codes.NZD, InvoiceHeader.JZ_ExchangeRateIndicator);
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyAU.RX_Code;
			AssertEquals("InvoiceHeader.JZ_ExchangeRateIndicator", ZString.Empty, InvoiceHeader.JZ_ExchangeRateIndicator);
		}

		public void TestLookupsIsRightTypeEvenIfTheInvoiceHEaderIsCastedBackToBase()
		{
			AssertEquals("InvoiceHeader.Lookups.GetType()", typeof(JobComInvoiceHeaderLookups), InvoiceHeader.Lookups.GetType());
			AssertEquals("((BaseJobComInvoiceHeader)InvoiceHeader).Lookups.GetType()", typeof(JobComInvoiceHeaderLookups), ((Customs.Business.BaseJobComInvoiceHeader)InvoiceHeader).Lookups.GetType());
		}

		public void TestIsECIWriteOff()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("InvoiceHeader.IsECIWriteOff", true, InvoiceHeader.IsECIWriteOff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("InvoiceHeader.IsECIWriteOff", false, InvoiceHeader.IsECIWriteOff);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("InvoiceHeader.JZ_RX_NKInvoice_Currency", JobDeclaration.LocalCurrencyConstantCode, InvoiceHeader.JZ_RX_NKInvoice_Currency);
		}

		public void TestCurrencyConverter()
		{
			AssertEquals("CurrencyConverter.GetType()", typeof(CurrencyConverterWithFixedExchangeRatesDataProvider), InvoiceHeader.CurrencyConverter.GetType());
		}

		public void TestChangingJZ_RN_NKDefaultOriginValidatesInvoiceLines()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CountryOfOrigin = ZString.Empty;
			AssertHasMessageError(invoiceLine1.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);

			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = ZString.Empty;
			AssertHasMessageError(invoiceLine2.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);

			InvoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertNoMessageError(invoiceLine1.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);
			AssertNoMessageError(invoiceLine2.JI_CountryOfOriginInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfOrigin);
		}

		public void TestChangingJZ_RN_NKDefaultExportValidatesInvoiceLines()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertHasMessageError(invoiceLine1.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);

			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertHasMessageError(invoiceLine2.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);

			InvoiceHeader.JZ_RN_NKDefaultExport = "AU";
			AssertNoMessageError(invoiceLine1.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);
			AssertNoMessageError(invoiceLine2.JI_RN_NKCountryOfExportInfo, JobComInvoiceLineValidation.MessageErrorEnterAValidCountryOfExport);
		}

		public void TestChangingJZ_DefaultQualifiesForPrefDutyValidatesInvoiceLines()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_QualifiesForPreferentialDuty = ZString.Empty;
			AssertHasMessageError(invoiceLine1.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);

			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_QualifiesForPreferentialDuty = ZString.Empty;
			AssertHasMessageError(invoiceLine2.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);

			InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertNoMessageError(invoiceLine1.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
			AssertNoMessageError(invoiceLine2.JI_QualifiesForPreferentialDutyInfo, JobComInvoiceLineValidation.MessageErrorEnterValidQualForPrefDutyFlag);
		}

		public void TestJZ_RN_NKDefaultExport()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RN_NKDefaultExport = "";
			AssertEquals("", InvoiceHeader.JZ_RN_NKDefaultExport);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultExportInfo);
			InvoiceHeader.JZ_RN_NKDefaultExport = "AU";
			AssertEquals("AU", InvoiceHeader.JZ_RN_NKDefaultExport);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultExportInfo);
			InvoiceHeader.JZ_RN_NKDefaultExport = "ZZ";
			AssertEquals("ZZ", InvoiceHeader.JZ_RN_NKDefaultExport);
			AssertHasMessageError(InvoiceHeader.JZ_RN_NKDefaultExportInfo, JobComInvoiceHeaderValidation.MessageErrorRemoveOrFixDefaultCountryOfExport);
		}

		public void TestJZ_RN_NKDefaultOrigin()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RN_NKDefaultOrigin = "";
			AssertEquals("", InvoiceHeader.JZ_RN_NKDefaultOrigin);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultOriginInfo);
			InvoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertEquals("AU", InvoiceHeader.JZ_RN_NKDefaultOrigin);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultOriginInfo);
			InvoiceHeader.JZ_RN_NKDefaultOrigin = "ZZ";
			AssertEquals("ZZ", InvoiceHeader.JZ_RN_NKDefaultOrigin);
			AssertHasMessageError(InvoiceHeader.JZ_RN_NKDefaultOriginInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorRemoveOrFixDefaultCountryOfOrigin);
		}

		public void TestJZ_DefaultOriginRegion()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_DefaultOriginRegion = "";
			AssertEquals("", InvoiceHeader.JZ_DefaultOriginRegion);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultOriginInfo);
			InvoiceHeader.JZ_DefaultOriginRegion = "XX";
			AssertEquals("XX", InvoiceHeader.JZ_DefaultOriginRegion);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultOriginInfo);
			InvoiceHeader.JZ_DefaultOriginRegion = "ZZ";
			AssertEquals("ZZ", InvoiceHeader.JZ_DefaultOriginRegion);
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultOriginInfo);
		}

		public void TestAddInfoOnLoaded()
		{
			AssertEquals("InvoiceHeader HasChanges", false, InvoiceHeader.HasChanges);
			InvoiceHeader.JZ_RelationshipIndicator = "Y";
			Factory.Save();
			var invoiceHeaderLoaded = Factory.Load<JobComInvoiceHeader>(InvoiceHeader.PK);
			Assert("JZ_AddInfo:Precondition", invoiceHeaderLoaded.JZ_AddInfo.IndexOf("RelationshipIndicator=Y") >= 0);
			AssertEquals("RelationshipIndicator", "Y", invoiceHeaderLoaded.JZ_RelationshipIndicator);
		}

		public void TestAddInfoOnSaving()
		{
			Assert("PreCondition:JZ_AddInfo doesnt have uplift", InvoiceHeader.JZ_AddInfo.IndexOf("RelationshipIndicator=Y") < 0);
			InvoiceHeader.JZ_RelationshipIndicator = "Y";
			Factory.Save();
			Assert("JZ_AddInfo has RelationshipIndicator", InvoiceHeader.JZ_AddInfo.IndexOf("RelationshipIndicator=Y") >= 0);
		}

		public void TestHasChangesOfAddInfo()
		{
			AssertEquals("HasChanges", false, InvoiceHeader.HasChanges);
			InvoiceHeader.JZ_RelationshipIndicator = "Y";
			AssertEquals("HasChanges", true, InvoiceHeader.HasChanges);
		}

		public void TestFOBValueForCPTIncoterm()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				InvoiceHeader.JZ_InvoiceAmount = 15000m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, JobDeclaration.LocalCurrencyConstantCode);
				GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, JobDeclaration.LocalCurrencyConstantCode);
				InvoiceHeader.JZ_IncoTerm = Enterprise.Customs.NZ.Business.IncoTermList.Codes.CarriagePaidTo;

				ZDecimal expectedLineTotal = 15000 - 500;
				ZDecimal expectedFOB = 15000 - 500;
				ZDecimal expectedCIF = 15000 + 50;

				Declaration.ResumeApportionment();

				AssertEquals("Line total", expectedLineTotal, InvoiceHeader.InvoiceLineTotal);
				AssertEquals("FOB calculated", expectedFOB, InvoiceHeader.JZ_Calc_FOBAmount);
				AssertEquals("CIF calculated", expectedCIF, InvoiceHeader.JZ_Calc_CIFAmount);
			}
		}

		public void TestFOBValueForCPTIncoterm2()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				InvoiceHeader.JZ_InvoiceAmount = 15000m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				InvoiceCharge oFT = InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, JobDeclaration.LocalCurrencyConstantCode);
				oFT.J7_IsIncludedInITOT = true;
				GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, JobDeclaration.LocalCurrencyConstantCode);
				InvoiceHeader.JZ_IncoTerm = Enterprise.Customs.NZ.Business.IncoTermList.Codes.CarriagePaidTo;

				ZDecimal expectedLineTotal = 15000;
				ZDecimal expectedFOB = 15000 - 500;
				ZDecimal expectedCIF = 15000 + 50;
				Declaration.ResumeApportionment();

				AssertEquals("Line total", expectedLineTotal, InvoiceHeader.InvoiceLineTotal);
				AssertEquals("FOB calculated", expectedFOB, InvoiceHeader.JZ_Calc_FOBAmount);
				AssertEquals("CIF calculated", expectedCIF, InvoiceHeader.JZ_Calc_CIFAmount);
			}
		}

		public void TestDefaultRelationshipIndicator()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Precondition: InvoiceHeader.JZ_RelationshipIndicator Default Value", "", InvoiceHeader.JZ_RelationshipIndicator);

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "S1";
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "B1";
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;

			InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", "", InvoiceHeader.JZ_RelationshipIndicator);

			OrgSupplierBuyerLink link = buyer.SupplierLinks.AddNew();
			link.OL_OH_Supplier = supplier.PK;

			InvoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", "", InvoiceHeader.JZ_RelationshipIndicator);
			InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", "", InvoiceHeader.JZ_RelationshipIndicator);

			link.OL_RelatedParty = Enterprise.MasterFiles.Business.Customs.NZ.RelatedPartyList.Codes.Unrelated;

			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", "", InvoiceHeader.JZ_RelationshipIndicator);
			InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", "", InvoiceHeader.JZ_RelationshipIndicator);
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", RelationshipIndicatorList.Codes.NotRelated, InvoiceHeader.JZ_RelationshipIndicator);

			link.OL_RelatedParty = Enterprise.MasterFiles.Business.Customs.NZ.RelatedPartyList.Codes.Related;

			InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", RelationshipIndicatorList.Codes.NotRelated, InvoiceHeader.JZ_RelationshipIndicator);
			InvoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", RelationshipIndicatorList.Codes.NotRelated, InvoiceHeader.JZ_RelationshipIndicator);
			InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", RelationshipIndicatorList.Codes.Related, InvoiceHeader.JZ_RelationshipIndicator);

			InvoiceHeader.JZ_RelationshipIndicator = "";
			InvoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			link.OL_RelatedParty = Enterprise.MasterFiles.Business.Customs.NZ.RelatedPartyList.Codes.RelatedDoesNotAffectPrice;

			AssertEquals("InvoiceHeader.JZ_RelationshipIndicator", "", InvoiceHeader.JZ_RelationshipIndicator);
			InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			AssertEquals("Related but does not affect price should have defaulted from Organisation set-up", RelationshipIndicatorList.Codes.RelatedDoesNotAffectPrice, InvoiceHeader.JZ_RelationshipIndicator);
		}

		public void TestEffectiveZeroRatedDutyAsString()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			CombineAssertions("ZeroRatedAll = Yes", () =>
			{
				declaration.JE_IsZeroRatedAll = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedDutyDescription);

				invoiceHeader.JZ_IsZeroRatedDuty = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedDutyDescription);

				invoiceHeader.JZ_IsZeroRatedDuty = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedDutyDescription);
			});

			CombineAssertions("ZeroRatedAll = No", () =>
			{
				invoiceHeader.JZ_IsZeroRatedDuty = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedDutyDescription);

				declaration.JE_IsZeroRatedAll = "N";
				invoiceHeader.JZ_IsZeroRatedDuty = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedDutyDescription);

				invoiceHeader.JZ_IsZeroRatedDuty = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
			});

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedDuty = "";
			AssertEquals("", invoiceHeader.EffectiveIsZeroRatedDutyAsString);
			AssertEquals("", invoiceHeader.ZeroRatedDutyDescription);
		}

		public void TestEffectiveZeroRatedExciseAsString()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			CombineAssertions("ZeroRatedAll = Yes", () =>
			{
				declaration.JE_IsZeroRatedAll = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedExciseDescription);

				invoiceHeader.JZ_IsZeroRatedExcise = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedExciseDescription);

				invoiceHeader.JZ_IsZeroRatedExcise = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedExciseDescription);
			});

			CombineAssertions("ZeroRatedAll = No", () =>
			{
				invoiceHeader.JZ_IsZeroRatedExcise = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedExciseDescription);

				declaration.JE_IsZeroRatedAll = "N";
				invoiceHeader.JZ_IsZeroRatedExcise = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedExciseDescription);

				invoiceHeader.JZ_IsZeroRatedExcise = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
			});

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedExcise = "";
			AssertEquals("", invoiceHeader.EffectiveIsZeroRatedExciseAsString);
			AssertEquals("", invoiceHeader.ZeroRatedExciseDescription);
		}

		public void TestEffectiveZeroRatedGSTAsString()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			CombineAssertions("ZeroRatedAll = Yes", () =>
			{
				declaration.JE_IsZeroRatedAll = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedGSTDescription);

				invoiceHeader.JZ_IsZeroRatedGST = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedGSTDescription);

				invoiceHeader.JZ_IsZeroRatedGST = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedGSTDescription);
			});

			CombineAssertions("ZeroRatedAll = No", () =>
			{
				invoiceHeader.JZ_IsZeroRatedGST = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedGSTDescription);

				declaration.JE_IsZeroRatedAll = "N";
				invoiceHeader.JZ_IsZeroRatedGST = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedGSTDescription);

				invoiceHeader.JZ_IsZeroRatedGST = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
			});

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedGST = "";
			AssertEquals("", invoiceHeader.EffectiveIsZeroRatedGSTAsString);
			AssertEquals("", invoiceHeader.ZeroRatedGSTDescription);
		}

		public void TestEffectiveZeroRatedLeviesAsString()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			CombineAssertions("ZeroRatedAll = Yes", () =>
			{
				declaration.JE_IsZeroRatedAll = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedLeviesDescription);

				invoiceHeader.JZ_IsZeroRatedLevies = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedLeviesDescription);

				invoiceHeader.JZ_IsZeroRatedLevies = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedLeviesDescription);
			});

			CombineAssertions("ZeroRatedAll = No", () =>
			{
				invoiceHeader.JZ_IsZeroRatedLevies = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
				AssertEquals("No", invoiceHeader.ZeroRatedLeviesDescription);

				declaration.JE_IsZeroRatedAll = "N";
				invoiceHeader.JZ_IsZeroRatedLevies = "Y";
				AssertEquals("Y", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
				AssertEquals("Yes", invoiceHeader.ZeroRatedLeviesDescription);

				invoiceHeader.JZ_IsZeroRatedLevies = "N";
				AssertEquals("N", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
			});

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedLevies = "";
			AssertEquals("", invoiceHeader.EffectiveIsZeroRatedLeviesAsString);
			AssertEquals("", invoiceHeader.ZeroRatedLeviesDescription);
		}

		public override void TestMarkApportionmentDirtyOnInvoiceCurrExRateTypeChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRateType = ExchangeRateIndicatorList.Codes.NZD;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRateType = ExchangeRateIndicatorList.Codes.ForwardCover;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			invoice.JZ_InvoiceCurrExRateType = ExchangeRateIndicatorList.Codes.NZD;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);
		}

		public void TestSellerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			seller.OH_FullName = "YIN SUNG ENTERPRISES MALAYSIA";
			var sellerLocation = seller.Addresses.AddNew();
			sellerLocation.OA_Address1 = "1570 SIDHULAN AVE";
			sellerLocation.OA_Address2 = "KOMA SUR";
			sellerLocation.OA_City = "KUALA LUMPUR";

			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			invoice.JZ_FOBValue = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = "NZD";
			invoice.SupplierName = "MISC Supplier";
			invoice.JZ_OA_SellerAddress = sellerLocation.PK;
			AssertEquals("invoice.SellerAddress", sellerLocation, invoice.SellerAddress);
			AssertEquals("invoice.SellerAddress.OA_Address1", "1570 SIDHULAN AVE", invoice.SellerAddress.OA_Address1);
			AssertEquals("invoice.SellerAddress.OA_Address1", "KOMA SUR", invoice.SellerAddress.OA_Address2);
			AssertEquals("invoice.SellerAddress.OA_Address1", "KUALA LUMPUR", invoice.SellerAddress.OA_City);
			AssertEquals("JZ_OA_SellerAddress", sellerLocation.PK, invoice.JZ_OA_SellerAddress);
		}

		public void TestDefaultCurrencyForStandaloneInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			_ = new Customs.Business.FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			AssertEquals(JobDeclaration.LocalCurrencyConstantCode, invoice.JZ_RX_NKInvoice_Currency);
		}

		#region Implementation
		protected override void SetIsJZ_InvoiceCurrExRateUserEnterable(Customs.Business.BaseJobComInvoiceHeader invoice, bool value)
		{
			JobComInvoiceHeader nzInvoice = invoice as JobComInvoiceHeader;
			nzInvoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			nzInvoice.JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			nzInvoice.JZ_ExchangeRateIndicator = value ? ExchangeRateIndicatorList.Codes.ForwardCover : ExchangeRateIndicatorList.Codes.NZD;
		}

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.groupHeader; }
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.invoiceHeader; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			return testDec.Invoices.AddNew();
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
		#endregion
	}

	public class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		public void TestSettingEDITransmitDateSetsCurrencyConverterDate()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 8, 14);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 14), invoiceHeader.CurrencyConverter.DateForRate);
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 8, 15);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 15), invoiceHeader.CurrencyConverter.DateForRate);
		}

		public void TestSettingValuationDateOverrideOnInvoiceHasNoNetEffectInNZ()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 8, 14);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 14), invoiceHeader.CurrencyConverter.DateForRate);
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 15);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 14), invoiceHeader.CurrencyConverter.DateForRate);
		}

		public void TestIsJZ_InvoiceCurrExRateUserEnterable()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("IsExport", true, Declaration.IsExport);

			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			AssertEquals("PreCondition: JZ_InvoiceCurrExRate is not user-enterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);

			invoice.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			AssertEquals("JZ_InvoiceCurrExRate is user-enterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);

			invoice.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			AssertEquals("JZ_InvoiceCurrExRate is not user-enterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);

			invoice.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("JZ_InvoiceCurrExRate is not user-enterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
		}

		public void TestApportionmentShouldNotUpdateUserEnteredExchangeRate()
		{
			TestCaseHelper.ClearTable("RefExchangeRate");
			var aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefExchangeRate rate1 = aUDCurrency.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "CUS";
			rate1.RE_StartDate = new ZDateTime(1999, 8, 13);
			rate1.RE_ExpiryDate = new ZDateTime(1999, 8, 13);
			rate1.RE_SellRate = 1.3m;

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			AssertEquals("JZ_InvoiceCurrExRate is user-enterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);

			Declaration.JE_EDITransmitDate = new ZDateTime(1999, 8, 14);

			AssertEquals("Precondition: Currency set to a local currency", JobDeclaration.LocalCurrencyConstantCode, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("Exchange Rate set", 1m, invoice.JZ_InvoiceCurrExRate);

			invoice.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			AssertEquals("Exchange Rate does not change", 1m, invoice.JZ_InvoiceCurrExRate);

			invoice.JZ_InvoiceCurrExRate = 1.35m;
			AssertEquals("User-entered ex-rate stays", 1.35m, invoice.JZ_InvoiceCurrExRate);

			Declaration.ResumeApportionment();
			AssertEquals("User-entered ex-rate stays", 1.35m, invoice.JZ_InvoiceCurrExRate);

			RefExchangeRate rate2 = aUDCurrency.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "CUS";
			rate2.RE_StartDate = new ZDateTime(1999, 8, 14);
			rate2.RE_ExpiryDate = new ZDateTime(1999, 8, 14);
			rate2.RE_SellRate = 1.4m;

			Declaration.ResumeApportionment();
			AssertEquals("User-entered Exrate should stay", 1.35m, invoice.JZ_InvoiceCurrExRate);
		}

		#region Implementation
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
		#endregion

	}

	public class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}

	public class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}

	public class JobComInvoiceHeaderTestForDocumentWrapper : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		public void TestEffectiveValuationDate()
		{
			ZDateTime exportDate = new ZDateTime(2004, 12, 12);
			((JobDeclaration)invoice.JobDeclaration).JE_EDITransmitDate = exportDate;
			AssertEquals("Export Date", exportDate, invoice.EffectiveValuationDate);

			ZDateTime valuationDate = new ZDateTime(2004, 12, 15);
			invoice.JZ_ValuationDateOverride = valuationDate;
			AssertEquals("Valuation Date", valuationDate, invoice.EffectiveValuationDate);
		}
	}
}
