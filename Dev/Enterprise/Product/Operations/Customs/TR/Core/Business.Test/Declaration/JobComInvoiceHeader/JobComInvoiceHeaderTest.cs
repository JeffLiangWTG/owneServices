using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestJobComInvoiceLines()
		{
			var invoiceHeader = InvoiceHeader;
			CombineAssertions(() =>
			{
				AssertType<JobComInvoiceLineViewCollection>("Type", invoiceHeader.JobComInvoiceLines);
				AssertSame("InvoiceLine same", invoiceHeader.JobComInvoiceLines, invoiceHeader.InvoiceLines);
			});
		}

		public void TestLookups_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceHeaderLookups>(InvoiceHeader.Lookups);
		}

		public void TestLookups_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceHeaderLookups>(InvoiceHeader.Lookups);
		}

		public void TestLookups_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceHeaderLookups>(InvoiceHeader.Lookups);
		}

		public void TestValidation_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceHeaderValidation>(InvoiceHeader.Validation);
		}

		public void TestValidation_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceHeaderValidation>(InvoiceHeader.Validation);
		}

		public void TestValidation_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceHeaderValidation>(InvoiceHeader.Validation);
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			Assert("TODO, We will calculate CIF in future work item. becasue TR is not EU, we cannot use EuCustomsValuationCalculator and EUIncoTermAndCustomsChargeFactory.", true);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Turkey;

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		public void TestCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<InvoiceChargeCollection>(invoice.Charges);
		}

		public override void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			Assert("The method is not supported： User can't enter any charges", true);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;

			AssertContainsExactElementsInAnyOrder(
				new[] { "COM", "DEM", "INT", "LBC", "LCC", "LDC", "LEC", "LOT", "LPC", "LRU", "LSC", "LTC", "OBS", "OFT", "ONS", "OTH", "ROY", "SUR", "TFC" },
				chargeTypeList1.GetAllCodes()
			);
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;

			ICommonInvoice commonInvoice2 = dec.TopGroupInvoice;
			var chargeTypeList3 = commonInvoice2.ChargeTypeList;
			AssertContainsExactElementsInAnyOrder(
				new[] { "COM", "DEM", "INT", "LBC", "LCC", "LDC", "LEC", "LOT", "LPC", "LRU", "LSC", "LTC", "OBS", "OFT", "ONS", "OTH", "ROY", "SUR", "TFC" },
				chargeTypeList3.GetAllCodes()
			);
		}

		public override void TestIWeightApportioneeRoundingIssue()
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "XYZ";

			var cusRate = foreignCurrency.ExchangeRates.AddNew();
			cusRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			cusRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusRate.RE_SellRate = 0.8m;

			var cusSecRate = foreignCurrency.ExchangeRates.AddNew();
			cusSecRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
			cusSecRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusSecRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusSecRate.RE_SellRate = 0.7m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ValuationDate = ZDate.Today;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice1.JZ_InvoiceAmount = 8000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice2.JZ_InvoiceAmount = 2000m;

			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, true);
			invoice1.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 0.25m : 4m;

			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4m : 0.25m;
			AssertEquals("Weight", 2000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 8000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, false);
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, false);
			declaration.ApportionInvoiceWeight(null);
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 5000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 5000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);
		}

		public override void TestMarkApportionmentDirtyOnInvoiceCurrExRateTypeChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRateType = "BSF";
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			invoice.JZ_InvoiceCurrExRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRateType = "BSF";
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			declaration.ResumeApportionment();
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);
		}

		public void TestJobComInvoiceHeaderFieldsValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_PaymentAmount = 100;
			invoice.JZ_PaymentNo = "12345678901234567890";
			Factory.Save();

			CombineAssertions("JobComInvoiceHeaderFieldsValue", () =>
			{
				AssertEquals(100m, invoice.JZ_PaymentAmount);
				AssertEquals("12345678901234567890", invoice.JZ_PaymentNo);
			});
		}

		public void TestAddInfoJobComInvoiceHeaderFieldsValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_CommercialPaymentCode = "1";
			Factory.Save();

			var newInvoice = NewFactory().Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("1", newInvoice.ZG_CommercialPaymentCode);
		}

		public void TestGetAllActiveCusSupportingInfoTypes()
		{
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceHeader).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceHeader).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(InvoiceHeader.AdditionalInfos);
		}

		public void TestLocalCharges()
		{
			var invoiceHeader = InvoiceHeader;
			AssertEquals("LocalChargesExpected, default 0.", 0m, invoiceHeader.LocalChargesExpected);
			AssertEquals("LocalChargesEntered, default 0.", 0m, invoiceHeader.LocalChargesEntered);
			AssertEquals("LocalChargesBalance, default 0.", 0m, invoiceHeader.LocalChargesBalance);
			AssertEquals("LocalChargesCurrency, default empty.", ZGuid.Empty, invoiceHeader.LocalChargesCurrency);

			invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalTotalCharges).J7_Amount = 100;
			invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge).J7_Amount = 90;
			AssertEquals("LocalChargesExpected, 100 TRY.", 100M, invoiceHeader.LocalChargesExpected);
			AssertEquals("LocalChargesEntered, 90 TRY.", 90M, invoiceHeader.LocalChargesEntered);
			AssertEquals("LocalChargesBalance, 10 TRY.", 10M, invoiceHeader.LocalChargesBalance);
			AssertEquals("LocalChargesCurrency, PK of TRY.", Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Turkey).PK, invoiceHeader.LocalChargesCurrency);
		}

		public void TestForeignCharges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ValuationDate = ZDate.Today;
			var invoiceHeader = declaration.Invoices.AddNew();

			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.Turkey, 1, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 19.33, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedKingdom, 33.16, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.EuropeanUnion, 21.41, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			declaration.Company.GC_IsReciprocal = true;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Inv's EffectiveValuationDateCore is today when not EntryInstruction", ZDateTime.Today, invoiceHeader.EffectiveValuationDate);
				AssertEquals("ForeignChargesExpected, default 0.", 0M, invoiceHeader.ForeignChargesExpected);
				AssertEquals("ForeignChargesEntered, default 0.", 0M, invoiceHeader.ForeignChargesEntered);
				AssertEquals("ForeignChargesBalance, default 0.", 0M, invoiceHeader.ForeignChargesBalance);
				AssertEquals("ForeignChargesCurrency, default empty.", ZGuid.Empty, invoiceHeader.ForeignChargesCurrency);
			});

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.TotalForeignCharges).J7_Amount = 100;
			var obvCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.Observation);
			obvCharge.J7_Amount = 10;
			obvCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var demCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.DEM);
			demCharge.J7_Amount = 10;
			demCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;

			CombineAssertions("Foreign Charges", () =>
			{
				AssertEquals("ForeignChargesExpected, 100USD(1922TRY).", 100M, invoiceHeader.ForeignChargesExpected);
				AssertEquals("ForeignChargesEntered, 10EUR(214.1TRY) + 10GBP(331.6TRY) = 545.7TRY = 28.23USD.", 28.23M, invoiceHeader.ForeignChargesEntered);
				AssertEquals("ForeignChargesBalance, (100-28.23)=71.77, USD", 71.77M, invoiceHeader.ForeignChargesBalance);
				AssertEquals("ForeignChargesCurrency, PK of USD.",
					Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates).PK,
					invoiceHeader.ForeignChargesCurrency
				);
			});
		}

		public void TestCaptions()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.JZ_RelatedIndicator), false,
				x => x.Caption == "Seller/Buyer Relation Code" && x.ShortCaption == "S/B Rel.Code"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.LocalChargesExpected), false,
				x => x.Caption == "Total Expected"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.LocalChargesEntered), false,
				x => x.Caption == "Lines Entered"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.LocalChargesBalance), false,
				x => x.Caption == "Balance"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.LocalChargesCurrency), false,
				x => x.Caption == "Local Currency"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.ForeignChargesExpected), false,
				x => x.Caption == "Total Expected"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.ForeignChargesEntered), false,
				x => x.Caption == "Lines Entered"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.ForeignChargesBalance), false,
				x => x.Caption == "Balance"
			);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(
				typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.ForeignChargesCurrency), false,
				x => x.Caption == "Foreign Currency"
			);
		}

		public void TestEffectiveValuationDateCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ValuationDate = new ZDate(2024, 8, 7);
			var invoice = declaration.Invoices.AddNew();
			Factory.Save();

			var newInvoice = NewFactory().Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals(new ZDate(2024, 8, 7), newInvoice.EffectiveValuationDate);

			AssertEquals(ZDate.Today, InvoiceHeader.EffectiveValuationDate);
		}

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection);

		JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.invoiceHeader;
	}
}
