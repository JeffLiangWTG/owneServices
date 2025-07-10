using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummary7501Invoice))]
	sealed class EntrySummary7501InvoiceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFactoryPassedInFromInvoiceHeaderIsUsedToLoadOtherObjects()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "I+*/NV-T#$#ST1";
			invoiceHeader.US_TransactionsRelated = "Y";

			var invoiceHeaderWrapper = new EntrySummary7501Invoice(invoiceHeader, 1, false);

			AssertEquals("Wrapper should use factory from line to avoid complete Dec being reloaded in a new factory for every Invoice Header"
				, invoiceHeader.Factory
				, invoiceHeaderWrapper.Factory);
		}

		public void TestEntrySummary7501Invoice()
		{
			JobComInvoiceHeader invoiceHdr = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoiceHdr.JZ_InvoiceNumber = "I+*/NV-T#$#ST1";
			invoiceHdr.US_TransactionsRelated = "Y";
			EntrySummary7501Invoice invoice = new EntrySummary7501Invoice(invoiceHdr, 1, true);

			AssertEquals("InvoiceNo", "001/INV-TST1", invoice.InvoiceNo);
			AssertEquals("Related transactions for mixed relationship invoices print blank at the invoice level", "", invoice.TransactionsRelatedIndicator);
		}

		public void TestTransactionsRelated()
		{
			var invoiceHdr = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoiceHdr.JZ_InvoiceNumber = "I8429";
			var invLine = invoiceHdr.JobComInvoiceLines.AddNew();
			invLine.US_TransactionsRelated = "Y";
			var invoice = new EntrySummary7501Invoice(invoiceHdr, 1, false);
			AssertEquals("When not a mixed relationship invoice, related transactions indicator needs to be determined from invoice lines", "Y", invoice.TransactionsRelatedIndicator);
		}

		public void TestExcludeZeroAmountCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "DDP";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			invoice.Charges.AddNew();
			invoice.Charges.AddNew();
			invoice.Charges.AddNew();

			declaration.TopGroupInvoice.Charges.AddNew("OFT", 2033.05m, JobDeclaration.LocalCurrencyConstantCode);
			declaration.TopGroupInvoice.Charges.AddNew("ONS", 17.58m, JobDeclaration.LocalCurrencyConstantCode);
			declaration.TopGroupInvoice.Charges.AddNew("DED", 15.58m, JobDeclaration.LocalCurrencyConstantCode);

			declaration.ResumeApportionment();

			var invoicePrint = new EntrySummary7501Invoice(invoice, 1, false);

			Assert(HasThisCharge(invoicePrint, 2033.05m));
			Assert(!HasThisCharge(invoicePrint, 17.58m));
			Assert(HasThisCharge(invoicePrint, 15.58m));
		}

		public void TestTransactionsRelatedIndicator()
		{
			JobComInvoiceHeader invoiceHdr = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoiceHdr.JZ_InvoiceNumber = "Inv_001";
			invoiceHdr.US_TransactionsRelated = "Y";
			EntrySummary7501Invoice invoice = new EntrySummary7501Invoice(invoiceHdr, 1, true);

			AssertEquals("Related transactions should be blank on Invoice level when there are mixed relationships on the entry", "", invoice.TransactionsRelatedIndicator);
		}

		public void TestChargeAdjustments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			invoice.Charges.AddNew("OFT", 2033.05m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;
			invoice.Charges.AddNew("ONS", 17.58m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[1].J7_IsNotIncludedInInvoice = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("invoiceLine should have apportioned charges", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1USD", "(-) International Freight", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1USD", 2033.05m, invoice7501Summary.Adjustment1USD);

			AssertEquals("AdjustmentItem2USD", "(-) International Insurance", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2USD", 17.58m, invoice7501Summary.Adjustment2USD);

			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestInvoiceAdjustmentsWithDutiableCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			foreach (CodeDescriptionPair chargeType in new USCustomsChargeTypeList())
			{
				if (chargeType.Code != USCustomsChargeTypeList.Codes.Discount)
				{
					AssertInvoiceAdjustmentsWithDutiableCharges(declaration, chargeType);
				}
			}
		}

		public void TestChargeAdjustmentsWithDiscount2()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EnableENS = true;
				declaration.US_EntryFilerCode = "XJ5";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV-CHG1";
				invoice.JZ_IncoTerm = "CAF";
				invoice.JZ_InvoiceAmount = 10433.67m;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
				invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
				invoice.JZ_InvoiceCurrExRate = 1.1194m;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 9159.55m;

				invoice.Charges.RemoveAndDeleteAll();

				var oft = invoice.Charges.AddNew();
				oft.J7_ChargeType = "OFT";
				oft.J7_Amount = 1640.50m;
				oft.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				oft.IsJ7_ExchangeRateUserEnterable = true;
				oft.J7_ExchangeRate = 1.1194m;
				oft.J7_IsNotIncludedInInvoice = false;
				oft.J7_IsIncludedInITOT = false;

				var discount = invoice.Charges.AddNew();
				discount.J7_ChargeType = "DIS";
				discount.J7_Amount = 366.38m;
				discount.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				discount.IsJ7_ExchangeRateUserEnterable = true;
				discount.J7_ExchangeRate = 1.1194m;
				discount.J7_IsNotIncludedInInvoice = false;
				discount.J7_IsIncludedInITOT = false;

				declaration.ResumeApportionment();

				var invoicePrint = new EntrySummary7501Invoice(invoice, 1, false);

				AssertEquals("AdjustmentItem1USD", "(-) International Freight USD", invoicePrint.AdjustmentItem1USD);
				AssertEquals("Adjustment1USD", 1836.38m, invoicePrint.Adjustment1USD);

				AssertEquals("AdjustmentItem2USD", "(-) Discount USD", invoicePrint.AdjustmentItem2USD);
				AssertEquals("Adjustment2USD", 410.13m, invoicePrint.Adjustment2USD);

				AssertEquals("InvoiceValueInUSD", 12089.58m, invoicePrint.InvoiceValueInUSD);
			}
		}

		public void TestInvoiceAdjustmentsWithMultipleDeductionsInSameCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			invoice.Charges.AddNew("OFT", 2033.05m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;
			invoice.Charges.AddNew("ONS", 17.58m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[1].J7_IsNotIncludedInInvoice = false;

			invoice.Charges.AddNew("DED", 32000m, Core.Constants.CurrencyCodes.Japan);
			invoice.Charges[2].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[2].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[2].J7_ExchangeRate = 0.012591m;
			invoice.Charges.AddNew("DED", 2000m, Core.Constants.CurrencyCodes.Japan);
			invoice.Charges[3].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[3].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[3].J7_ExchangeRate = 0.012591m;

			invoice.Charges.AddNew("DED", 464m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[4].J7_IsNotIncludedInInvoice = false;
			invoice.Charges.AddNew("DED", 25m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[5].J7_IsNotIncludedInInvoice = false;

			invoice.Charges.AddNew("DED", 5000m, Core.Constants.CurrencyCodes.China);
			invoice.Charges[6].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[6].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[6].J7_ExchangeRate = 0.375m;
			invoice.Charges.AddNew("DED", 5000m, Core.Constants.CurrencyCodes.China);
			invoice.Charges[7].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[7].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[7].J7_ExchangeRate = 0.375m;
			invoice.Charges.AddNew("DED", 2000m, Core.Constants.CurrencyCodes.China);
			invoice.Charges[8].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[8].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[8].J7_ExchangeRate = 0.375m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("Pre-condition - charges set up", 9, invoice.Charges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1USD", "(-) International Freight", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1USD", 2033.05m, invoice7501Summary.Adjustment1USD);

			AssertEquals("AdjustmentItem2USD", "(-) International Insurance", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2USD", 17.58m, invoice7501Summary.Adjustment2USD);

			AssertEquals("AdjustmentItem3", "Deduction Charge JPY", invoice7501Summary.AdjustmentItem3);
			AssertEquals("Adjustment3", 34000m, invoice7501Summary.Adjustment3);
			AssertEquals("Adjustment3ExchangeRate", 0.012591m, invoice7501Summary.Adjustment3ExchangeRate);
			AssertEquals("AdjustmentItem3USD", "(-) Deduction Charge USD", invoice7501Summary.AdjustmentItem3USD);
			AssertEquals("Adjustment3USD", 428.09m, invoice7501Summary.Adjustment3USD);

			AssertEquals("AdjustmentItem4", "Deduction Charge", invoice7501Summary.AdjustmentItem4);
			AssertEquals("AdjustmentIte43USD", "(-) Deduction Charge", invoice7501Summary.AdjustmentItem4USD);
			AssertEquals("Adjustment4USD", 489.00m, invoice7501Summary.Adjustment4USD);

			AssertEquals("AdjustmentItem3", "Deduction Charge CNY", invoice7501Summary.AdjustmentItem5);
			AssertEquals("Adjustment3", 12000m, invoice7501Summary.Adjustment5);
			AssertEquals("Adjustment3ExchangeRate", 0.375m, invoice7501Summary.Adjustment5ExchangeRate);
			AssertEquals("AdjustmentItem3USD", "(-) Deduction Charge USD", invoice7501Summary.AdjustmentItem5USD);
			AssertEquals("Adjustment3USD", 4500m, invoice7501Summary.Adjustment5USD);

			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestInvoiceAdjustmentsWithMixedMultipleAdditionsAndDeductionsInSameCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "MIXEDCHGS";
			invoice.JZ_IncoTerm = "CIP";
			invoice.JZ_RN_NKDefaultOrigin = "JP";
			invoice.JZ_Weight = 9290m;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_InvoiceAmount = 37000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9801001012";
			invoiceLine.JI_Weight = 9290m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.US_UC_NKCountryOfExport = "JP";
			invoiceLine.JI_LinePrice = 37000m;

			invoice.Charges[0].J7_Amount = 4950m;
			invoice.Charges[0].J7_IsGSTApplicable = true;

			invoice.Charges[1].J7_Amount = 375.75m;
			invoice.Charges[1].J7_IsGSTApplicable = true;

			invoice.Charges.AddNew("ADD", 300m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[2].J7_IsDutiable = true;
			invoice.Charges[2].J7_IsGSTApplicable = true;
			invoice.Charges[2].J7_IsNotIncludedInInvoice = true;

			invoice.Charges.AddNew("ADD", 250m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[3].J7_IsDutiable = true;
			invoice.Charges[3].J7_IsGSTApplicable = true;
			invoice.Charges[3].J7_IsNotIncludedInInvoice = true;

			invoice.Charges.AddNew("ADD", 2000m, Core.Constants.CurrencyCodes.Japan);
			invoice.Charges[4].J7_IsDutiable = true;
			invoice.Charges[4].J7_IsGSTApplicable = true;
			invoice.Charges[4].J7_IsNotIncludedInInvoice = true;
			invoice.Charges[4].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[4].J7_ExchangeRate = 0.012591m;

			invoice.Charges.AddNew("DED", 5000m, Core.Constants.CurrencyCodes.China);
			invoice.Charges[5].J7_IsIncludedInITOT = true;
			invoice.Charges[5].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[5].J7_ExchangeRate = 0.157513m;

			invoice.Charges.AddNew("DED", 2500m, Core.Constants.CurrencyCodes.China);
			invoice.Charges[6].J7_IsIncludedInITOT = true;
			invoice.Charges[6].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[6].J7_ExchangeRate = 0.157513m;

			invoice.Charges.AddNew("ADD", 24000m, Core.Constants.CurrencyCodes.Japan);
			invoice.Charges[7].J7_IsDutiable = true;
			invoice.Charges[7].J7_IsGSTApplicable = true;
			invoice.Charges[7].J7_IsNotIncludedInInvoice = true;
			invoice.Charges[7].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[7].J7_ExchangeRate = 0.012591m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("Pre-condition - charges set up", 8, invoice.Charges.Count);
			AssertEquals("InvoiceNo", "001/MIXEDCHGS", invoice7501Summary.InvoiceNo);
			AssertEquals("InvoiceValue", 37000.00m, invoice7501Summary.InvoiceValueInUSD);

			AssertEquals("AdjustmentItem1", "(-) International Freight", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 4950m, invoice7501Summary.Adjustment1USD);

			AssertEquals("AdjustmentItem2", "(-) International Insurance", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2", 375.75m, invoice7501Summary.Adjustment2USD);

			AssertEquals("AdjustmentItem3USD", "(+) Additional Charge", invoice7501Summary.AdjustmentItem3USD);
			AssertEquals("Adjustment3USD", 550m, invoice7501Summary.Adjustment3USD);

			AssertEquals("AdjustmentItem4", "Additional Charge JPY", invoice7501Summary.AdjustmentItem4);
			AssertEquals("Adjustment4", 26000m, invoice7501Summary.Adjustment4);
			AssertEquals("Adjustment4ExchangeRate", 0.012591m, invoice7501Summary.Adjustment4ExchangeRate);
			AssertEquals("AdjustmentItem4USD", "(+) Additional Charge USD", invoice7501Summary.AdjustmentItem4USD);
			AssertEquals("Adjustment4USD", 327.37m, invoice7501Summary.Adjustment4USD);

			AssertEquals("AdjustmentItem5", "Deduction Charge CNY", invoice7501Summary.AdjustmentItem5);
			AssertEquals("Adjustment5", 7500m, invoice7501Summary.Adjustment5);
			AssertEquals("Adjustment5ExchangeRate", 0.157513m, invoice7501Summary.Adjustment5ExchangeRate);
			AssertEquals("AdjustmentItem5USD", "(-) Deduction Charge USD", invoice7501Summary.AdjustmentItem5USD);
			AssertEquals("Adjustment5USD", 1181.35m, invoice7501Summary.Adjustment5USD);
		}

		public void TestMixedInvoiceAdjustmentsInMultipleCurrencies()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "MIXEDCHGS";
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 100000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3501101000";
			invoiceLine.JI_Weight = 150m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.JI_LinePrice = 100000m;

			//OFT
			invoice.Charges[0].J7_Amount = 1000m;
			invoice.Charges[0].J7_IsGSTApplicable = true;

			//INS
			invoice.Charges[1].J7_Amount = 500m;
			invoice.Charges[1].J7_IsGSTApplicable = true;

			invoice.Charges.AddNew("ADD", 200m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[2].J7_IsDutiable = true;
			invoice.Charges[2].J7_IsGSTApplicable = true;

			invoice.Charges.AddNew("DED", 1000m, Core.Constants.CurrencyCodes.China);
			invoice.Charges[3].J7_IsIncludedInITOT = true;
			invoice.Charges[3].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[3].J7_ExchangeRate = 0.157513m;

			invoice.Charges.AddNew("ADD", 1500m, Core.Constants.CurrencyCodes.Japan);
			invoice.Charges[4].J7_IsDutiable = true;
			invoice.Charges[4].J7_IsGSTApplicable = true;
			invoice.Charges[4].J7_IsNotIncludedInInvoice = true;
			invoice.Charges[4].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[4].J7_ExchangeRate = 0.012591m;

			invoice.Charges.AddNew("ADD", 3000m, Core.Constants.CurrencyCodes.Japan);
			invoice.Charges[5].J7_IsDutiable = true;
			invoice.Charges[5].J7_IsGSTApplicable = true;
			invoice.Charges[5].J7_IsNotIncludedInInvoice = true;
			invoice.Charges[5].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[5].J7_ExchangeRate = 0.012591m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("Pre-condition - charges set up", 6, invoice.Charges.Count);
			AssertEquals("InvoiceNo", "001/MIXEDCHGS", invoice7501Summary.InvoiceNo);
			AssertEquals("InvoiceValue", 100000.00m, invoice7501Summary.InvoiceValueInUSD);

			AssertEquals("AdjustmentItem1", "(-) International Freight", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 1000m, invoice7501Summary.Adjustment1USD);

			AssertEquals("AdjustmentItem2", "(-) International Insurance", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2", 500m, invoice7501Summary.Adjustment2USD);

			AssertEquals("AdjustmentItem3", "Deduction Charge CNY", invoice7501Summary.AdjustmentItem3);
			AssertEquals("Adjustment3", 1000m, invoice7501Summary.Adjustment3);
			AssertEquals("Adjustment3ExchangeRate", 0.157513m, invoice7501Summary.Adjustment3ExchangeRate);
			AssertEquals("AdjustmentItem3USD", "(-) Deduction Charge USD", invoice7501Summary.AdjustmentItem3USD);
			AssertEquals("Adjustment5USD", 157.51m, invoice7501Summary.Adjustment3USD);

			AssertEquals("AdjustmentItem4", "Additional Charge JPY", invoice7501Summary.AdjustmentItem4);
			AssertEquals("Adjustment4", 4500m, invoice7501Summary.Adjustment4);
			AssertEquals("Adjustment4ExchangeRate", 0.012591m, invoice7501Summary.Adjustment4ExchangeRate);
			AssertEquals("AdjustmentItem4USD", "(+) Additional Charge USD", invoice7501Summary.AdjustmentItem4USD);
			AssertEquals("Adjustment4USD", 56.66m, invoice7501Summary.Adjustment4USD);
		}

		public void TestNoExceptionThrownOnTEVWhenDeclarationNull()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertEquals(null, invoice.JobDeclaration);
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);
			AssertEquals("No exception thrown", 0m, invoice7501Summary.TEV);
		}

		public void TestTotalEnteredValueOfInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 87525.20m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.Charges.AddNew("DIS", 875.35m, "USD");

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 12246.55m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 15048.00m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 11454.30m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 12662.55m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 36123.80m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine6 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);
			AssertEquals("Total Entered value", 86660m, invoice7501Summary.TEV);
		}

		public void TestTotalEnteredValueOfInvoiceWhenDDP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceCurrLandedCostExRate = 1.000000000;
			invoice.JZ_InvoiceAmount = 143640.00m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.JZ_Weight = 1000m;
			invoice.JZ_WeightUQ = "KG";

			var ddd = invoice.Charges.AddNew("DDD", 2062.5600m, "USD");
			ddd.J7_IsIncludedInITOT = true;
			ddd.J7_Calc_IsIncludedInInvoiceAmount = true;
			ddd.J7_ExchangeRate = 1.0m;

			var lch = invoice.Charges.AddNew("LCH", 5000.00m, "USD");
			lch.J7_IsIncludedInITOT = true;
			lch.J7_Calc_IsIncludedInInvoiceAmount = true;

			var oft = invoice.Charges.AddNew("OFT", 5850.00m, "USD");
			oft.J7_IsIncludedInITOT = true;
			oft.J7_Calc_IsIncludedInInvoiceAmount = true;
			oft.J7_IsGSTApplicable = true;

			var ons = invoice.Charges.AddNew("ONS", 379.21m, "USD");
			ons.J7_IsIncludedInITOT = true;
			ons.J7_Calc_IsIncludedInInvoiceAmount = true;
			ons.J7_IsGSTApplicable = true;

			AssertEquals("OFT of invoice", 6229.21m, invoice.JZ_Calc_TNI);
			AssertEquals("Customs FOB Amoun of Invoice", 130348.23m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF Amount of Invoice", 136577.44m, invoice.JZ_Calc_CIFAmount);
			AssertEquals("Invoice Total Amoun of Invoice", 143640.00m, invoice.InvoiceLineTotal);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.SupTariffFormatted = "9802005060";
			invoiceLine1.US_98ValueInvCurr = 124800.00m;
			invoiceLine1.JI_Weight = 868.839m;
			invoiceLine1.JI_WeightUQ = "KG";

			AssertEquals("Customes Value", 124800.00m, invoiceLine1.JI_CustomsValue);
			AssertEquals("Total Expected Value", 143640.00m, invoiceLine1.JI_Calc_LinesTotal);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 16330m;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Weight = 131.161m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_LinePrice = 18840.0000m;
			invoiceLine2.JI_Tariff = "0712908580";
			invoiceLine2.JI_CustomsQuantity = 16330m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 18840.00m;
			invoiceLine2.SupTariffFormatted = "99038803";
			invoiceLine2.US_Duty = 473.43m;

			var fee501 = invoiceLine2.FeeCusCodes.AddNew("501");
			fee501.CY_FeeAmount = 163.13m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);
			AssertEquals("Total Entered value", 130348.00m, invoice7501Summary.TEV);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Total Entered value", 130348.00m, entry.TotalEnteredValue);
			AssertEquals("US Total Entered value", 130348.00m, declaration.US_TotalEnteredValue);
		}

		public void TestUSOriginalGoodsValues()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.US_SupTariff = "9802004040";
			invoiceLine.US_98GoodsValue = 60000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("USOriginalGoodsValue", 60000m, invoice7501Summary.USOriginalGoodsValue);
			AssertEquals("TEV", 60600m, invoice7501Summary.TEV);

			invoiceLine.US_98ValueInvCurr = 10m;

			AssertEquals("InvoiceValueInUSD", 60000m, invoice7501Summary.USOriginalGoodsValue);
			AssertEquals("TEV", 60600m, invoice7501Summary.TEV);
		}

		public void TestTotalEnteredValueOfInvoiceWithXAndVLines()
		{
			//CS00174232
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = "01";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CS00174232";
			invoice.JZ_InvoiceAmount = 8757.60m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.US_UC_NKCountryOfExport = "CH";
			invoice.US_UC_NKCountryOfOrigin = "CH";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew("OFT", 150m, "USD");

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6211.43.0020";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "DOZ";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_DestinationState = "IL";
			invoiceLine.JI_Weight = 83m;
			invoiceLine.JI_WeightUQ = "KG";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6211.43.0020";
			invoiceLine2.JI_CustomsQuantity = 82m;
			invoiceLine2.JI_CustomsUnitQty = "DOZ";
			invoiceLine2.JI_CustomsSecondQuantity = 82m;
			invoiceLine2.JI_CustomsSecondUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 7409.52m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6505.00.2060";
			invoiceLine3.JI_CustomsQuantity = 82m;
			invoiceLine3.JI_CustomsUnitQty = "DOZ";
			invoiceLine3.JI_CustomsSecondQuantity = 15m;
			invoiceLine3.JI_CustomsSecondUnitQty = "KG";
			invoiceLine3.JI_LinePrice = 1348.08m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("InvoiceLine1 Customs Value", 8757.60m, invoiceLine.JI_CustomsValue);
			AssertEquals("InvoiceLine2 Customs Value", 7409.52m, invoiceLine2.JI_CustomsValue);
			AssertEquals("InvoiceLine3 Customs Value", 1348.08m, invoiceLine3.JI_CustomsValue);

			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);
			AssertEquals("Total Entered value should not include X line Customs Value if any", 8758m, invoice7501Summary.TEV);
		}

		public void TestChargeAdjustments3()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ApportionmentDirty = false;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG3";
			invoice.JZ_IncoTerm = "CFR";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			invoice.Charges.AddNew("OFT", 0m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsDutiable = false;
			invoice.Charges[0].J7_IsGSTApplicable = true;
			invoice.Charges[0].J7_IsIncludedInITOT = false;
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[0].J7_IsApportionedCharge = false;
			invoice.Charges[0].J7_DistributeBy = "VAL";
			invoice.Charges[0].J7_ExchangeRate = 0m;
			invoice.Charges[0].J7_RX_NKCurrency = "";
			invoice.Charges[0].J7_FullOrPartialApportionment = "PAA";

			invoice.Charges.AddNew("ONS", 0m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[1].J7_IsDutiable = false;
			invoice.Charges[1].J7_IsGSTApplicable = true;
			invoice.Charges[1].J7_IsIncludedInITOT = false;
			invoice.Charges[1].J7_IsNotIncludedInInvoice = false;
			invoice.Charges[1].J7_IsApportionedCharge = false;
			invoice.Charges[1].J7_DistributeBy = "VAL";
			invoice.Charges[1].J7_ExchangeRate = 0m;
			invoice.Charges[1].J7_RX_NKCurrency = "";
			invoice.Charges[1].J7_FullOrPartialApportionment = "PAA";

			var charge = declaration.TopGroupInvoice.Charges.AddNew("PAC", 2260.00m, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsNotIncludedInInvoice = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_ExchangeRate = 1m;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_FullOrPartialApportionment = "PAA";

			charge = declaration.TopGroupInvoice.Charges.AddNew("OFT", 3920.00m, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsIncludedInITOT = true;
			charge.J7_IsNotIncludedInInvoice = false;
			charge.J7_DistributeBy = "VAL";
			charge.J7_ExchangeRate = 1m;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_FullOrPartialApportionment = "PAA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceNo", "001/INV-CHG3", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem2USD", "(+) Packing Costs", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2USD", 2260.00m, invoice7501Summary.Adjustment2USD);

			AssertEquals("AdjustmentItem3USD", "(-) International Freight", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment3USD", 3920.00m, invoice7501Summary.Adjustment1USD);

			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestChargeAdjustmentsWhenChargeAtLineLevel()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG2";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			invoiceLine.Charges.AddNew("OFT", 2033.05m, JobDeclaration.LocalCurrencyConstantCode);
			invoiceLine.Charges[0].J7_IsNotIncludedInInvoice = false;
			invoiceLine.Charges.AddNew("ONS", 17.58m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine.Charges.AddNew("ADD", 350m, JobDeclaration.LocalCurrencyConstantCode);
			invoiceLine.Charges[2].J7_IsNotIncludedInInvoice = true;
			invoiceLine.Charges[2].J7_IsDutiable = true;
			invoiceLine.Charges[2].J7_FullOrPartialApportionment = "PAA";
			invoiceLine.Charges[2].J7_IsApportionedCharge = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceNo", "001/INV-CHG2", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1USD", "(-) International Freight", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment1USD", 2033.05m, invoice7501Summary.Adjustment2USD);

			AssertEquals("AdjustmentItem2USD", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment2USD, $350 Additional charges", 350m, invoice7501Summary.Adjustment1USD);

			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestForeignCurrencyChargeAdjustments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			invoice.Charges.AddNew("OFT", 2950m, "AUD");
			invoice.Charges[0].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[0].J7_ExchangeRate = 0.815m;
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;
			invoice.Charges.AddNew("ONS", 35.75m, "AUD");
			invoice.Charges[1].IsJ7_ExchangeRateUserEnterable = true;
			invoice.Charges[1].J7_ExchangeRate = 0.815m;
			invoice.Charges[1].J7_IsNotIncludedInInvoice = false;
			declaration.ResumeApportionment();
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("invoiceLine should have apportioned charges", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1", "International Freight AUD", invoice7501Summary.AdjustmentItem1);
			AssertEquals("Adjustment1", 2950m, invoice7501Summary.Adjustment1);
			AssertEquals("ExchangeRate1", 0.815m, invoice7501Summary.Adjustment1ExchangeRate);
			AssertEquals("AdjustmentItem1USD", "(-) International Freight USD", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1USD", 2404.25m, invoice7501Summary.Adjustment1USD);

			AssertEquals("AdjustmentItem2", "International Insurance AUD", invoice7501Summary.AdjustmentItem2);
			AssertEquals("Adjustment2", 35.75m, invoice7501Summary.Adjustment2);
			AssertEquals("ExchangeRate2", 0.815m, invoice7501Summary.Adjustment2ExchangeRate);
			AssertEquals("AdjustmentItem2USD", "(-) International Insurance USD", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2USD", 29.14m, invoice7501Summary.Adjustment2USD.Round(2));

			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestChargeAdjustmentsWithDiscount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 119868m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 119868m;

			invoice.Charges.AddNew("OFT", 3500m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = true;

			var groupCharge = declaration.TopGroupInvoice.Charges.AddNew("DIS", 1198.68m, JobDeclaration.LocalCurrencyConstantCode);
			groupCharge.J7_IsNotIncludedInInvoice = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("invoice should have 1 group charge", 1, invoice.GroupCharges.Count);
			AssertEquals("invoice should have 1 apportioned charge", 1, invoice.Charges.Count);

			AssertEquals("invoiceLine should have 2 apportioned charge", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1", "(-) Discount", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 1198.68m, invoice7501Summary.Adjustment1USD);

			AssertEquals("InvoiceValueInUSD", 119868m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestDiscountIncludedInInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoice.Charges.AddNew("DIS", 750m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10750m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(-) Discount", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 750m, invoice7501Summary.Adjustment1USD);
		}

		public void TestDiscountNotIncludedInInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoice.Charges.AddNew("DIS", 750m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = true;

			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(-) Discount", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 750m, invoice7501Summary.Adjustment1USD);
		}

		public void TestLineLevelDiscountIncluded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 9700m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.Charges.AddNew("DIS", 300m, JobDeclaration.LocalCurrencyConstantCode);
			invoiceLine.Charges[0].J7_IsNotIncludedInInvoice = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice should have 1 group charge", 1, invoice.GroupCharges.Count);

			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(-) Discount", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 300m, invoice7501Summary.Adjustment1USD);
			AssertEquals("TEV rounded", 9700m, invoice7501Summary.TEV);
		}

		public void TestLineLevelDiscountNotIncluded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.Charges.AddNew("DIS", 750m, JobDeclaration.LocalCurrencyConstantCode);
			invoiceLine.Charges[0].J7_IsNotIncludedInInvoice = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice should have 1 group charge", 1, invoice.GroupCharges.Count);

			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(-) Discount", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 750m, invoice7501Summary.Adjustment1USD);
			AssertEquals("TEV rounded", 9250m, invoice7501Summary.TEV);
		}

		public void TestAMMVIncludedInInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoice.Charges.AddNew("ADD", 750m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10000m, invoice7501Summary.InvoiceValueInUSD);
		}

		public void TestAMMVNotIncludedInInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoice.Charges.AddNew("ADD", 750m, JobDeclaration.LocalCurrencyConstantCode);
			invoice.Charges[0].J7_IsNotIncludedInInvoice = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 750m, invoice7501Summary.Adjustment1USD);
			AssertEquals("TEV rounded", 10750m, invoice7501Summary.TEV);
		}

		public void TestLineLevelAMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "KGS";
			invoiceLine.US_AMMVPerUnit = 2.15m;

			AssertEquals("invoice line should have 1 ADD charge", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("charge should be AMMV value", 215m, invoiceLine.ApportionedCharges[0].J7_Amount);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 10000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 215m, invoice7501Summary.Adjustment1USD);
		}

		public void TestMultiLineLevelAMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 30000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "KGS";
			invoiceLine1.US_AMMVPerUnit = 2.15m;

			AssertEquals("invoice line 1 should have 1 ADD charge", 1, invoiceLine1.ApportionedCharges.Count);
			AssertEquals("charge should be AMMV value", 215m, invoiceLine1.ApportionedCharges[0].J7_Amount);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_InvoiceQuantity = 100m;
			invoiceLine2.JI_InvoiceUQ = "KGS";

			AssertEquals("invoice line 2 should not have ADD charge", 0, invoiceLine2.ApportionedCharges.Count);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 10000m;
			invoiceLine3.JI_InvoiceQuantity = 100m;
			invoiceLine3.JI_InvoiceUQ = "KGS";
			invoiceLine3.US_AMMVPercentage = 7.5m;

			AssertEquals("invoice line 3 should have ADD charge", 1, invoiceLine3.ApportionedCharges.Count);
			AssertEquals("charge should be 150 AMMV value", 750m, invoiceLine3.ApportionedCharges[0].J7_Amount);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("InvoiceValueInUSD", 30000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("AdjustmentItem1", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1 should combine AMMV charges from line 1 & line 3", 965m, invoice7501Summary.Adjustment1USD);
		}

		public void TestMultiInvoiceMultiLineLevelAMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 30000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var charge = invoice1.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			charge.J7_Amount = 1290;

			invoice1.Charges.AddNew(USCustomsChargeTypeList.Codes.AdditionCharge, 100m);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_InvoiceAmount = 15750m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "KGS";
			invoiceLine1.US_AMMVPerUnit = 2.15m;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_InvoiceQuantity = 100m;
			invoiceLine2.JI_InvoiceUQ = "KGS";

			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 10000m;
			invoiceLine3.JI_InvoiceQuantity = 100m;
			invoiceLine3.JI_InvoiceUQ = "KGS";
			invoiceLine3.US_AMMVPercentage = 7.5m;
			declaration.ResumeApportionment();
			AssertEquals("invoice1 line 1 should have 3 charges", 3, invoiceLine1.ApportionedCharges.Count);
			AssertEquals("Apportioned Freight charge", 430m, invoiceLine1.ApportionedCharges[1].J7_Amount);
			AssertEquals("ADD (AMMV) charge - line", 215m, invoiceLine1.ApportionedCharges[0].J7_Amount);

			var apportionedADDAmount1 = invoiceLine1.ApportionedCharges[2].J7_Amount;
			declaration.ResumeApportionment();
			Assert("Apportioned ADD charge - Invoice", apportionedADDAmount1 == 33.33m || apportionedADDAmount1 == 33.34m);

			AssertEquals("invoice1 line 2 should have 2 charges", 2, invoiceLine2.ApportionedCharges.Count);
			AssertEquals("Apportioned Freight charge", 430m, invoiceLine2.ApportionedCharges[0].J7_Amount);

			var apportionedADDAmount2 = invoiceLine2.ApportionedCharges[1].J7_Amount;
			declaration.ResumeApportionment();
			Assert("Apportioned ADD charge - Invoice", apportionedADDAmount2 == 33.33m || apportionedADDAmount2 == 33.34m);

			AssertEquals("invoice1 line 3 should have 3 charges", 3, invoiceLine3.ApportionedCharges.Count);
			invoiceLine3.ApportionedCharges.Sort("J7_Amount");
			AssertEquals("Apportioned Freight charge", 430m, invoiceLine3.ApportionedCharges[1].J7_Amount);
			AssertEquals("ADD (AMMV) charge - line", 750m, invoiceLine3.ApportionedCharges[2].J7_Amount);

			var apportionedADDAmount3 = invoiceLine3.ApportionedCharges[0].J7_Amount;
			declaration.ResumeApportionment();
			Assert("Apportioned ADD charge - Invoice", apportionedADDAmount3 == 33.33m || apportionedADDAmount3 == 33.34m);

			AssertEquals("Total Apportioned ADD Amount", 100m, apportionedADDAmount1 + apportionedADDAmount2 + apportionedADDAmount3);

			var inv2_Line1 = invoice2.JobComInvoiceLines.AddNew();
			inv2_Line1.JI_LinePrice = 15750m;
			inv2_Line1.JI_InvoiceQuantity = 1000m;
			inv2_Line1.JI_InvoiceUQ = "NMB";
			inv2_Line1.US_AMMVPerUnit = 1.8m;
			declaration.ResumeApportionment();
			AssertEquals("inv2_Line1 should have 1 ADD charge", 1, inv2_Line1.ApportionedCharges.Count);
			AssertEquals("charge should be AMMV value", 1800m, inv2_Line1.ApportionedCharges[0].J7_Amount);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice1, 1, false);

			AssertEquals("Invoice1 InvoiceValueInUSD", 30000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("Invoice1 AdjustmentItem1", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Invoice1 Adjustment1 should combine AMMV charges from line 1 & line 3 and Invoice", 965m, invoice7501Summary.Adjustment1USD);

			invoice7501Summary = new EntrySummary7501Invoice(invoice2, 2, false);

			AssertEquals("Invoice2 InvoiceValueInUSD", 15750m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("Invoice2 AdjustmentItem1", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Invoice2 Adjustment1 should combine AMMV charges from line 1 & line 3 and Invoice", 1800m, invoice7501Summary.Adjustment1USD);
		}

		public void TestMultiInvoiceForeignCurrencyADDWithNoLineLevelAMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 30000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var freightCharge = invoice1.Charges.AddNew();
			freightCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge.J7_AdjustedCharge = true;
			freightCharge.J7_Amount = 1290;

			var gPBCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom);
			gPBCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddYears(1), 1.5738m);
			var addCharge = invoice1.Charges.AddNew(USCustomsChargeTypeList.Codes.AdditionCharge, 500m, Core.Constants.CurrencyCodes.UnitedKingdom);
			addCharge.J7_IsNotIncludedInInvoice = true;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_InvoiceAmount = 15750m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var addChargeInv2 = invoice2.Charges.AddNew(USCustomsChargeTypeList.Codes.AdditionCharge, 320m, Core.Constants.CurrencyCodes.UnitedKingdom);
			addChargeInv2.J7_IsNotIncludedInInvoice = true;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "KGS";

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_InvoiceQuantity = 100m;
			invoiceLine2.JI_InvoiceUQ = "KGS";

			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 10000m;
			invoiceLine3.JI_InvoiceQuantity = 100m;
			invoiceLine3.JI_InvoiceUQ = "KGS";
			declaration.ResumeApportionment();

			AssertEquals("invoice1 line 1 should have 2 charges", 2, invoiceLine1.ApportionedCharges.Count);
			AssertEquals("Apportioned Freight charge", 430m, invoiceLine1.ApportionedCharges[0].J7_Amount);

			var apportionedADDAmount1 = invoiceLine1.ApportionedCharges[1].J7_Amount;
			Assert("Apportioned ADD charge - Invoice", apportionedADDAmount1 == 166.67m || apportionedADDAmount1 == 166.66m);

			AssertEquals("invoice1 line 2 should have 2 charges", 2, invoiceLine2.ApportionedCharges.Count);
			AssertEquals("Apportioned Freight charge", 430m, invoiceLine2.ApportionedCharges[0].J7_Amount);

			var apportionedADDAmount2 = invoiceLine2.ApportionedCharges[1].J7_Amount;
			Assert("Apportioned ADD charge - Invoice", apportionedADDAmount2 == 166.67m || apportionedADDAmount2 == 166.66m);

			AssertEquals("invoice1 line 3 should have 2 charges", 2, invoiceLine3.ApportionedCharges.Count);
			AssertEquals("Apportioned Freight charge", 430m, invoiceLine3.ApportionedCharges[0].J7_Amount);

			var apportionedADDAmount3 = invoiceLine3.ApportionedCharges[1].J7_Amount;
			Assert("Apportioned ADD charge - Invoice", apportionedADDAmount3 == 166.67m || apportionedADDAmount3 == 166.66m);

			AssertEquals("Apportioned amount summed up to the original amount", 500m, apportionedADDAmount1 + apportionedADDAmount2 + apportionedADDAmount3);

			var inv2_Line1 = invoice2.JobComInvoiceLines.AddNew();
			inv2_Line1.JI_LinePrice = 15750m;
			inv2_Line1.JI_InvoiceQuantity = 1000m;
			inv2_Line1.JI_InvoiceUQ = "NMB";

			declaration.ResumeApportionment();
			AssertEquals("inv2_Line1 should have 1 ADD charge", 1, inv2_Line1.ApportionedCharges.Count);
			AssertEquals("charge should be Apportioned ADD value", 320m, inv2_Line1.ApportionedCharges[0].J7_Amount);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice1, 1, false);

			AssertEquals("Invoice1 InvoiceValueInUSD", 30000m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("Invoice1 AdjustmentItem1", "Additional Charge GBP", invoice7501Summary.AdjustmentItem1);
			AssertEquals("Invoice1 AdjustmentItem1", 500m, invoice7501Summary.Adjustment1);
			AssertEquals("Invoice1 AdjustmentItem1", 1.5738m, invoice7501Summary.Adjustment1ExchangeRate);
			AssertEquals("Invoice1 AdjustmentItem1", "(+) Additional Charge USD", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Invoice1 Adjustment1 should show USD value of ADD charge", 786.9m, invoice7501Summary.Adjustment1USD);

			invoice7501Summary = new EntrySummary7501Invoice(invoice2, 2, false);

			AssertEquals("Invoice2 InvoiceValueInUSD", 15750m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("Invoice1 AdjustmentItem1", "Additional Charge GBP", invoice7501Summary.AdjustmentItem1);
			AssertEquals("Invoice1 AdjustmentItem1", 320m, invoice7501Summary.Adjustment1);
			AssertEquals("Invoice1 AdjustmentItem1", 1.5738m, invoice7501Summary.Adjustment1ExchangeRate);
			AssertEquals("Invoice2 AdjustmentItem1", "(+) Additional Charge USD", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Invoice2 Adjustment1 should show USD value of ADD charge", 503.62m, invoice7501Summary.Adjustment1USD);
		}

		public void TestInvoiceValueInUSDAndTEVCalculateUsingInvoiceExRateNotSimplyByGettingInvoiceAmountInLocalCurrency()
		{
			var testCurrency = Factory.NewWithValidTestData<RefCurrency>();
			testCurrency.RX_Code = "JJJ";
			testCurrency.RX_Desc = "Test Currency";

			var exchRate1 = testCurrency.ExchangeRates.AddNew();
			exchRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate1.RE_RX_NKExCurrency = testCurrency.RX_Code;
			exchRate1.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_SellRate = 1.2824m;

			var declaration = Factory.New<JobDeclaration>();
			var branch = declaration.Branch;
			var company = branch.Company;
			company.GC_IsReciprocal = true;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 41860m;
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 41860m;

			invoice.Charges.AddNew("OFT", 2950m, "JJJ");
			invoice.Charges[0].J7_IsNotIncludedInInvoice = false;
			invoice.Charges.AddNew("ONS", 35.75m, "JJJ");
			invoice.Charges[1].J7_IsNotIncludedInInvoice = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			ZDecimal exportDateInvoiceValueInUSD = invoice.JZ_InvoiceAmount * invoice.JZ_InvoiceCurrExRate;
			AssertEquals("Pre-condition: JZ_InvoiceAmountInLocalCurrency should be equal to the calculated Invoice Value in USD", exportDateInvoiceValueInUSD.Round(2), invoice.JZ_InvoiceAmountInLocalCurrency);

			var exchRate2 = testCurrency.ExchangeRates.AddNew();
			exchRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate2.RE_RX_NKExCurrency = testCurrency.RX_Code;
			exchRate2.RE_StartDate = ZDateTime.Today;
			exchRate2.RE_ExpiryDate = ZDateTime.Today;
			exchRate2.RE_SellRate = 1.2885m;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var reloadedDeclaration = anotherFactory.Load<JobDeclaration>(declaration.PK);

			invoice = reloadedDeclaration.Invoices[0];

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("invoiceLine should have apportioned charges", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1", "International Freight JJJ", invoice7501Summary.AdjustmentItem1);
			AssertEquals("Adjustment1", 2950m, invoice7501Summary.Adjustment1);
			AssertEquals("ExchangeRate1", 1.2824m, invoice7501Summary.Adjustment1ExchangeRate);
			AssertEquals("AdjustmentItem1USD", "(-) International Freight USD", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1USD", 3783.08m, invoice7501Summary.Adjustment1USD.Round(2));

			AssertEquals("AdjustmentItem2", "International Insurance JJJ", invoice7501Summary.AdjustmentItem2);
			AssertEquals("Adjustment2", 35.75m, invoice7501Summary.Adjustment2);
			AssertEquals("ExchangeRate2", 1.2824m, invoice7501Summary.Adjustment2ExchangeRate);
			AssertEquals("AdjustmentItem2USD", "(-) International Insurance USD", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2USD", 45.85m, invoice7501Summary.Adjustment2USD.Round(2));

			AssertEquals("InvoiceValueInUSD", 53681.26m, invoice7501Summary.InvoiceValueInUSD.Round(2));
		}

		public void TestRelEntryNo()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.US_ReleaseEntryNumber = "SV912345678";
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);
			AssertEquals("SV912345678", invoice7501Summary.RelEntryNo);

			invoice.US_ReleaseEntryNumber = "12345678901234567890";
			AssertEquals("12345678901234", invoice7501Summary.RelEntryNo);
		}
		protected override BusinessObject GetNewBusinessObject() => new EntrySummary7501Invoice(Factory.New<JobComInvoiceHeader>(), 1, false);

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.SetCountry("US");

			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ6";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
		}

		bool HasThisCharge(EntrySummary7501Invoice invoicePrint, ZDecimal amount)
		{
			return invoicePrint.Adjustment1USD == amount
				|| invoicePrint.Adjustment2USD == amount
				|| invoicePrint.Adjustment3USD == amount
				|| invoicePrint.Adjustment4USD == amount
				|| invoicePrint.Adjustment5USD == amount;
		}

		void AssertInvoiceAdjustmentsWithDutiableCharges(JobDeclaration declaration, CodeDescriptionPair chargeType)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;

			var charge1 = invoice.Charges.AddNew(chargeType.Code, 500m, JobDeclaration.LocalCurrencyConstantCode);
			charge1.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge1.J7_IsIncludedInITOT = false;
			charge1.J7_IsDutiable = true;

			var charge2 = invoice.Charges.AddNew(chargeType.Code, 600m, JobDeclaration.LocalCurrencyConstantCode);
			charge2.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_IsDutiable = true;

			var charge3 = invoice.Charges.AddNew(chargeType.Code, 700m, JobDeclaration.LocalCurrencyConstantCode);
			charge3.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge3.J7_IsIncludedInITOT = true;
			charge3.J7_IsDutiable = true;

			var charge4 = invoice.Charges.AddNew(chargeType.Code, 800m, JobDeclaration.LocalCurrencyConstantCode);
			charge4.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge4.J7_IsIncludedInITOT = true;
			charge4.J7_IsDutiable = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			var description = new EntrySummary7501Invoice.ChargeWrapper().GetChargeDescription(chargeType.Description, chargeType.Code);
			AssertEquals("AdjustmentItem1", "(+) " + description, invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1", 1100m, invoice7501Summary.Adjustment1USD);
			AssertEquals("AdjustmentItem2", "(-) " + description, invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2", 800m, invoice7501Summary.Adjustment2USD);
		}
	}
}
