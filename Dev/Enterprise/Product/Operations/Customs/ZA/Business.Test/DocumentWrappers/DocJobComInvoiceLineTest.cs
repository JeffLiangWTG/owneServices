using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		#region Override Fields

		public void TestTariffInformation()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		public override void TestLinePriceCurr()
		{
			AssertEquals("LinePriceCurr", InvoiceLineWrapper.LinePriceCurr.ToString(), "ZAR");
		}

		#endregion

		#region ZDecimal Fields

		/// <summary> Incident#I00020827 </summary>
		public void TestMarkupPercentIsWrapped()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_RL_NKClosestPort = "ZAAAM";
			OrgSupplierBuyerLink link = importer.SupplierLinks.AddNew(supplier);
			link.OL_ValuationBasisMarkupPercent = 20.01m;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			header.JZ_OH_Supplier = supplier.PK;

			DocJobComInvoiceLine docInvoiceLine = DocJobComInvoiceLine.New(line, Factory);
			AssertEquals(0.2001m, docInvoiceLine.MarkupPercent);
		}

		public void TestImportDutyPaid()
		{
			InvoiceLine.JI_ImportDutyPaid = 100.0m;
			AssertEquals(100.0m, InvoiceLineWrapper.ImportDutyPaid);
		}

		public void TestImportSch1P2BPaid()
		{
			InvoiceLine.DA63AdditionalDuties.AddOrUpdate("12B", 25.77m);
			AssertEquals(25.77m, InvoiceLineWrapper.ImportSch1P2BPaid);
		}

		public void TestCustomsValueFromRelatedImportDeclaration()
		{
			InvoiceLine.JI_ImportCustomsValue = 312m;
			AssertEquals(312.0m, InvoiceLineWrapper.CustomsValueFromRelatedImportDeclaration);
		}

		public void TestVATFromRelatedImportDeclaration()
		{
			InvoiceLine.JI_ImportVATPaid = 141.23m;
			AssertEquals(141.23m, InvoiceLineWrapper.VATFromRelatedImportDeclaration);
		}

		public void TestLineCustomsValue()
		{
			MoqInvoiceLine.Setup(m => m.ApportionedCustomsValue).Returns(new ZDecimal(150m));
			AssertEquals("Apportioned Customs Value", 150m, MoqInvoiceLineWrapper.LineCustomsValue);
		}

		public void TestConversionFactor()
		{
			InvoiceLine.InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			InvoiceLine.JI_LinePrice = 1000m;
			AssertEquals(1m, InvoiceLineWrapper.ConversionFactor);
		}

		public void TestCustomsDuty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.SouthAfrica, "EX1");
			helper.LoadOrCreateNewCusRateCode(Factory, "12B", rateType.PK);
			Factory.Save();

			MoqInvoiceLine.Setup(m => m.JI_Calc_DutyAmount).Returns(new ZDecimal(150m));
			MoqInvoiceLine.Setup(m => m.ApportionedDutySch1P2B).Returns(new ZDecimal(20m));
			AssertEquals("Customs Duty", 130m, MoqInvoiceLineWrapper.CustomsDuty);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes._40;

			var cusEntryLine = Factory.New<CusEntryLine>();

			MoqInvoiceLine.Setup(m => m.JI_CEI).Returns(entryInstruction.PK);
			MoqInvoiceLine.Setup(m => m.JI_CL).Returns(cusEntryLine.PK);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			MoqInvoiceLine.Setup(m => m.JI_JZ).Returns(invoiceHeader.PK);

			var fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "DUT";
			fee.CF_ChargeAmount = 10m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "12B";
			fee.CF_ChargeAmount = 20m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "VAT";
			fee.CF_ChargeAmount = 40m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "DTY";
			fee.CF_ChargeAmount = 80m;
			fee.CF_IsLandedCostOnly = true;

			AssertEquals("Customs Duty", 60m, MoqInvoiceLineWrapper.CustomsDuty);
		}

		public void TestTotalDuty()
		{
			MoqInvoiceLine.Setup(m => m.JI_Calc_DutyAmount).Returns(new ZDecimal(150m));
			AssertEquals("Total Duty", 150m, MoqInvoiceLineWrapper.TotalDuty);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes._40;

			var cusEntryLine = Factory.New<CusEntryLine>();

			MoqInvoiceLine.Setup(m => m.JI_CEI).Returns(entryInstruction.PK);
			MoqInvoiceLine.Setup(m => m.JI_CL).Returns(cusEntryLine.PK);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			MoqInvoiceLine.Setup(m => m.JI_JZ).Returns(invoiceHeader.PK);

			var fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "DUT";
			fee.CF_ChargeAmount = 10m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "12B";
			fee.CF_ChargeAmount = 20m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "VAT";
			fee.CF_ChargeAmount = 40m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "DTY";
			fee.CF_ChargeAmount = 80m;
			fee.CF_IsLandedCostOnly = true;
			AssertEquals("Total Duty", 80m, MoqInvoiceLineWrapper.TotalDuty);
		}

		public void TestSch12BDuty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.SouthAfrica, "EX1");
			helper.LoadOrCreateNewCusRateCode(Factory, "12B", rateType.PK);
			Factory.Save();

			MoqInvoiceLine.Setup(m => m.ApportionedDutySch1P2B).Returns(new ZDecimal(20m));
			AssertEquals("12B", 20m, MoqInvoiceLineWrapper.Sch12BDuty);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes._40;

			var cusEntryLine = Factory.New<CusEntryLine>();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			MoqInvoiceLine.Setup(m => m.JI_CEI).Returns(entryInstruction.PK);
			MoqInvoiceLine.Setup(m => m.JI_CL).Returns(cusEntryLine.PK);
			MoqInvoiceLine.Setup(m => m.JI_JZ).Returns(invoiceHeader.PK);

			var fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "DUT";
			fee.CF_ChargeAmount = 10m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "12B";
			fee.CF_ChargeAmount = 20m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "VAT";
			fee.CF_ChargeAmount = 40m;
			fee.CF_IsLandedCostOnly = true;

			AssertEquals("12B", 20m, MoqInvoiceLineWrapper.Sch12BDuty);
		}

		public void TestCustomsVAT()
		{
			MoqInvoiceLine.Protected().Setup<ZDecimal>("GetGSTVATAmountCore").Returns(new ZDecimal(20m));
			AssertEquals("Customs VAT", 20m, MoqInvoiceLineWrapper.CustomsVAT);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes._40;

			var cusEntryLine = Factory.New<CusEntryLine>();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			MoqInvoiceLine.Setup(m => m.JI_CEI).Returns(entryInstruction.PK);
			MoqInvoiceLine.Setup(m => m.JI_CL).Returns(cusEntryLine.PK);
			MoqInvoiceLine.Setup(m => m.JI_JZ).Returns(invoiceHeader.PK);
			MoqInvoiceLine.Setup(m => m.JI_ZZF_NKTaxType).Returns("VAT");

			var fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "DUT";
			fee.CF_ChargeAmount = 10m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "12B";
			fee.CF_ChargeAmount = 20m;
			fee.CF_IsLandedCostOnly = true;

			fee = MoqInvoiceLine.Object.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = "VAT";
			fee.CF_ChargeAmount = 40m;
			fee.CF_IsLandedCostOnly = true;

			AssertEquals("Customs VAT", 40m, MoqInvoiceLineWrapper.CustomsVAT);
		}

		public void TestDiscountInInvoiceCurrency()
		{
			InvoiceLine.Charges.RemoveAll();
			InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 20m);
			InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 10m);
			AssertEquals("Discount", "30.00", InvoiceLineWrapper.DiscountInInvoiceCurrency);
		}

		public void TestValuationMarkup()
		{
			InvoiceLine.Declaration.JE_MessageType = "IMP";
			InvoiceLine.JI_LinePrice = 200m;
			InvoiceLine.JI_ValuationMarkup = 10m;
			AssertEquals("ValuationMarkup", 20m, InvoiceLineWrapper.ValuationMarkup);
		}

		public void TestCustomsValueAdjustment()
		{
			InvoiceLine.Declaration.JE_MessageType = "IMP";
			InvoiceLine.JI_LinePrice = 200m;
			InvoiceLine.JI_CustomsValueOverride = 100m;
			AssertEquals("CustomsValueAdjustment", -100m, InvoiceLineWrapper.CustomsValueAdjustment);

			InvoiceLine.JI_CustomsValueOverride = 0m;
			InvoiceLine.JI_ValuationMarkup = 10;
			AssertEquals("CustomsValueAdjustment", 0m, InvoiceLineWrapper.CustomsValueAdjustment);
		}

		public void TestTariffFormula()
		{
			MoqInvoiceLine.Setup(m => m.DutyFormulaDescription).Returns("FREE");
			AssertEquals("TariffFormula", "FREE", MoqInvoiceLineWrapper.TariffFormula);
		}

		public void TestIncoTerm()
		{
			InvoiceLine.InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("INCOTERM", Core.Constants.IncoTerms.DeliveredDutyPaid, InvoiceLineWrapper.IncoTerm);
		}

		public void TestCustomsValueInLocalCurrency()
		{
			MoqInvoiceLine.Setup(m => m.JI_CustomsValue).Returns(20m);
			AssertEquals("CustomsValueInLocalCurrency", 20m, MoqInvoiceLineWrapper.CustomsValueInLocalCurrency);
		}

		#region

		public void TestDiscountNotIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.DiscountNotIncludedInLines).Returns(money);
			AssertEquals("DiscountNotIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["DiscountNotIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestDiscountNotIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.DiscountNotIncludedInLines).Returns(money);
			AssertEquals("DiscountNotIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["DiscountNotIncludedInLines" + "CurrencyCode"]);
		}

		public void TestDiscountNotIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.DiscountNotIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("DiscountNotIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["DiscountNotIncludedInLines" + "ExRate"]);
		}

		#endregion

		#region DutiableChargesIncludedInLines

		public void TestDutiableChargesIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.DutiableChargesIncludedInLines).Returns(money);
			AssertEquals("DutiableChargesIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["DutiableChargesIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestDutiableChargesIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.DutiableChargesIncludedInLines).Returns(money);
			AssertEquals("DutiableChargesIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["DutiableChargesIncludedInLines" + "CurrencyCode"]);
		}

		public void TestDutiableChargesIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.DutiableChargesIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("DutiableChargesIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["DutiableChargesIncludedInLines" + "ExRate"]);
		}

		#endregion

		#region DutiableChargesNotIncludedInLines

		public void TestDutiableChargesNotIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.DutiableChargesNotIncludedInLines).Returns(money);
			AssertEquals("DutiableChargesNotIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["DutiableChargesNotIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestDutiableChargesNotIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.DutiableChargesNotIncludedInLines).Returns(money);
			AssertEquals("DutiableChargesNotIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["DutiableChargesNotIncludedInLines" + "CurrencyCode"]);
		}

		public void TestDutiableChargesNotIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.DutiableChargesNotIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("DutiableChargesNotIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["DutiableChargesNotIncludedInLines" + "ExRate"]);
		}

		#endregion

		#region NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance

		public void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance).Returns(money);
			AssertEquals("NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance" + "InForeignCurrency"]);
		}

		public void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance).Returns(money);
			AssertEquals("NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance" + "CurrencyCode"]);
		}

		public void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance" + "ExRate", 0.5m, MoqInvoiceLineWrapper["NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance" + "ExRate"]);
		}

		#endregion

		#region OverseasFreightIncludedInLines

		public void TestOverseasFreightIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasFreightIncludedInLines).Returns(money);
			AssertEquals("OverseasFreightIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["OverseasFreightIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestOverseasFreightIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasFreightIncludedInLines).Returns(money);
			AssertEquals("OverseasFreightIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["OverseasFreightIncludedInLines" + "CurrencyCode"]);
		}

		public void TestOverseasFreightIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.OverseasFreightIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("OverseasFreightIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["OverseasFreightIncludedInLines" + "ExRate"]);
		}

		#endregion

		#region OverseasFreightNotIncludedInLines

		public void TestOverseasFreightNotIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasFreightNotIncludedInLines).Returns(money);
			AssertEquals("OverseasFreightNotIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["OverseasFreightNotIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestOverseasFreightNotIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasFreightNotIncludedInLines).Returns(money);
			AssertEquals("OverseasFreightNotIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["OverseasFreightNotIncludedInLines" + "CurrencyCode"]);
		}

		public void TestOverseasFreightNotIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.OverseasFreightNotIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("OverseasFreightNotIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["OverseasFreightNotIncludedInLines" + "ExRate"]);
		}

		#endregion

		#region OverseasInsuranceIncludedInLines

		public void TestOverseasInsuranceIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasInsuranceIncludedInLines).Returns(money);
			AssertEquals("OverseasInsuranceIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["OverseasInsuranceIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestOverseasInsuranceIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasInsuranceIncludedInLines).Returns(money);
			AssertEquals("OverseasInsuranceIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["OverseasInsuranceIncludedInLines" + "CurrencyCode"]);
		}

		public void TestOverseasInsuranceIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));

			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.OverseasInsuranceIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("OverseasInsuranceIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["OverseasInsuranceIncludedInLines" + "ExRate"]);
		}

		#endregion

		#region OverseasInsuranceNotIncludedInLines

		public void TestOverseasInsuranceNotIncludedInLinesInForeignCurrency()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasInsuranceNotIncludedInLines).Returns(money);
			AssertEquals("OverseasInsuranceNotIncludedInLines" + "InForeignCurrency", 20m, MoqInvoiceLineWrapper["OverseasInsuranceNotIncludedInLines" + "InForeignCurrency"]);
		}

		public void TestOverseasInsuranceNotIncludedInLinesCurrencyCode()
		{
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, GlbCompany.CurrentCompany.LocalCurrency);
			MoqInvoiceLine.Setup(m => m.OverseasInsuranceNotIncludedInLines).Returns(money);
			AssertEquals("OverseasInsuranceNotIncludedInLines" + "CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.Code, MoqInvoiceLineWrapper["OverseasInsuranceNotIncludedInLines" + "CurrencyCode"]);
		}

		public void TestOverseasInsuranceNotIncludedInLinesExRate()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var declaration = InvoiceLine.Declaration;
			declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, 0.5m, declaration.JE_ExportDate.AddDays(-1));
			
			ZDecimal testAmount = 20m;
			var money = new Money(testAmount, currency);
			MoqInvoiceLine.Setup(m => m.OverseasInsuranceNotIncludedInLines).Returns(money);
			MoqInvoiceLine.Protected().Setup<ZString>("JI_RX_NKLinePriceCurrCore").Returns(InvoiceLine.InvoiceHeader.Invoice_Currency.Code);
			AssertEquals("OverseasInsuranceNotIncludedInLines" + "ExRate", 0.5m, MoqInvoiceLineWrapper["OverseasInsuranceNotIncludedInLines" + "ExRate"]);
		}

		#endregion

		#endregion

		#region ZString Fields

		public void TestCountableQty()
		{
			InvoiceLine.JI_BondedWhsQuantity = 1.99M;
			AssertEquals("CountableQty", "1", InvoiceLineWrapper.CountableQty);
			InvoiceLine.JI_BondedWhsQuantity = 1.00M;
			AssertEquals("CountableQty", "1", InvoiceLineWrapper.CountableQty);
			InvoiceLine.JI_BondedWhsQuantity = 88888888888888.999M;
			AssertEquals("CountableQty", "88888888888888", InvoiceLineWrapper.CountableQty);
		}

		public void TestPermitNumber()
		{
			InvoiceLine.JI_PermitNumber = "VIC";
			AssertEquals("PermitNumber", "VIC", InvoiceLineWrapper.PermitNumber);
		}

		public void TestROOCert()
		{
			InvoiceLine.JI_ROOCert = "VIC";
			AssertEquals("ROOCert", "VIC", InvoiceLineWrapper.ROOCert);
		}

		public void TestPrevMRN()
		{
			InvoiceLine.JI_PreviousEntryNumber = "VIC";
			AssertEquals("PrevMRN", "VIC", InvoiceLineWrapper.PrevMRN);
		}

		public void TestCPCAndPPC()
		{
			InvoiceLine.JI_Procedure = "VIC";
			AssertEquals("CPCAndPPC", "VIC", InvoiceLineWrapper.CPCAndPPC);
		}

		public void TestAddtionalTariffs()
		{
			AssertEquals("AddtionalTariffs", "", InvoiceLineWrapper.AddtionalTariffs);
			InvoiceLine.CusLineTariffDetails.AddNew("12B", "1260313");
			InvoiceLine.CusLineTariffDetails.AddNew("13D", "1510113");
			InvoiceLine.CusLineTariffDetails.AddNew("13E", "1530322");
			AssertEquals("AddtionalTariffs", "12B:1260313,13D:1510113,13E:1530322", InvoiceLineWrapper.AddtionalTariffs);
		}

		public void TestVIN()
		{
			InvoiceLine.JI_VIN = "VIC";
			AssertEquals("VIN", "VIC", InvoiceLineWrapper.VIN);
		}

		public void TestEngineNumber()
		{
			InvoiceLine.JI_EngineNumber = "VIC";
			AssertEquals("EngineNumber", "VIC", InvoiceLineWrapper.EngineNumber);
		}

		public void TestColour()
		{
			InvoiceLine.JI_Colour = "RED";
			AssertEquals("Colour", "RED", InvoiceLineWrapper.Colour);
		}

		public void TestMake()
		{
			InvoiceLine.JI_Make = "VIC";
			AssertEquals("Make", "VIC", InvoiceLineWrapper.Make);
		}

		public void TestModel()
		{
			InvoiceLine.JI_Model = "VIC";
			AssertEquals("Model", "VIC", InvoiceLineWrapper.Model);
		}

		public void TestVehicleFormat()
		{
			InvoiceLine.JI_VehicleFormat = "VIC";
			AssertEquals("VehicleFormat", "VIC", InvoiceLineWrapper.VehicleFormat);
		}

		public void TestVehicleType()
		{
			InvoiceLine.JI_VehicleType = "VIC";
			AssertEquals("VehicleType", "VIC", InvoiceLineWrapper.VehicleType);
		}

		public void TestYoM()
		{
			InvoiceLine.JI_YearOfManufacture = "2015";
			AssertEquals("YoM", "2015", InvoiceLineWrapper.YoM);
		}

		public void TestEffectiveCountryOfOrigin()
		{
			MoqInvoiceLine.Setup(m => m.EffectiveCountryOfOrigin).Returns(new ZString("AU"));
			AssertEquals("EffectiveCountryOfOrigin", "AU", MoqInvoiceLineWrapper.EffectiveCountryOfOrigin);
		}

		public void TestJI_PrimaryPreference()
		{
			InvoiceLine.JI_PrimaryPreference = "STANDARD";
			AssertEquals(InvoiceLine.JI_PrimaryPreference, InvoiceLineWrapper.TradeAgreement);
		}

		public void TestTariffSchedule1P2A()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		public void TestRawTariffSchedule1P2A2B()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		public void TestTariffSchedule1P2B()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		public void TestTariffSchedule5And6()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		public void TestRebateItem()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		public void TestTakeUpInTradeStatistics()
		{
			InvoiceLine.JI_TakeUpInTradeStatistics = ZBool.True;
			AssertEquals("TakeUpInTradeStatistics", "1", InvoiceLineWrapper.TakeUpInTradeStatistics);

			InvoiceLine.JI_TakeUpInTradeStatistics = ZBool.False;
			AssertEquals("TakeUpInTradeStatistics", "2", InvoiceLineWrapper.TakeUpInTradeStatistics);
		}

		public void TestRebateDescription()
		{
			// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
			Assert(true);
		}

		#endregion

		#region ZInt Fields

		public void TestEntryLineNumber()
		{
			InvoiceLine.CusEntryLine.CL_LineNumber = 3;
			AssertEquals("Entry line number for the invoice line", 3, InvoiceLineWrapper.EntryLineNumber);
		}

		#endregion

		#region ZShort Fields

		public void TestEngineCC()
		{
			InvoiceLine.JI_EngineCapacity = 1;
			AssertEquals("EngineCC", 1, InvoiceLineWrapper.EngineCC);
		}

		public void TestPrevMRNLine()
		{
			InvoiceLine.JI_PreviousEntryLineNumber = Convert.ToByte(3);
			AssertEquals("PrevMRNLine", Convert.ToByte(3), InvoiceLineWrapper.PrevMRNLine);
		}

		public void TestInvoiceSequenceNum()
		{
			InvoiceLine.InvoiceHeader.JZ_InvoiceDisplaySequence = Convert.ToByte(1);
			AssertEquals("InvoiceSequenceNum", Convert.ToByte(1), InvoiceLineWrapper.InvoiceSequenceNum);
		}

		#endregion

		#region Implementation

		Mock<JobComInvoiceLine> MoqInvoiceLine
		{
			get
			{
				if (fMoqInvoiceLine == null)
				{
					fMoqInvoiceLine = Factory.NewMoq<JobComInvoiceLine>();
				}

				return fMoqInvoiceLine;
			}
		}
		Mock<JobComInvoiceLine> fMoqInvoiceLine;

		DocJobComInvoiceLine MoqInvoiceLineWrapper
		{
			get
			{
				if (fMoqInvoiceLineWrapper == null)
				{
					fMoqInvoiceLineWrapper = DocJobComInvoiceLine.New(MoqInvoiceLine.Object, Factory);
				}
				return fMoqInvoiceLineWrapper;
			}
		}
		DocJobComInvoiceLine fMoqInvoiceLineWrapper;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		protected override JobComInvoiceLine GetNewInvoiceLine()
		{
			var invoiceLine = base.GetNewInvoiceLine();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			return invoiceLine;
		}

		#endregion
	}
}
