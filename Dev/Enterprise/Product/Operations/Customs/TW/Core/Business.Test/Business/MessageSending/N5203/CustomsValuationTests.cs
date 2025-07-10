using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CustomsValuation))]
	sealed class CustomsValuationTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCustomsValuation_EXP_FOB_DEDAsGroupCharge()
		{
			var date = new ZDateTime(2020, 01, 01);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "B07252327";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];
			var groupCharges = groupInvoice.Charges;

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 14096m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var deduction = groupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 704.8m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 14096m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 14096m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 14096m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("Declaration Invoice Total (16)", 13391.2m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 704.8m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 14096m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation_EXP_FOB_DED()
		{
			var date = new ZDateTime(2020, 01, 01);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "B07252327";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 14096m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var deduction = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 704.8m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 14096m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 14096m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 14096m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("Declaration Invoice Amount (16)", 13391.2m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 704.8m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 14096m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation_EXP_FOB_DEDAsGroupCharge2()
		{
			var date = new ZDateTime(2020, 01, 01);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CW/ /09/C02/36150";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];
			var groupCharges = groupInvoice.Charges;

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 6988.2m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var deduction = groupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 69.43m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 2436m;
			invoiceLine1.JI_EnteredUnitPrice = 2.85m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 16m;
			invoiceLine2.JI_EnteredUnitPrice = 2.85m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 6988.2m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 6988.2m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("Declaration Invoice Total (16)", 6918.77m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 69.43m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 6988.2m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation_EXP_FOB_DED2()
		{
			var date = new ZDateTime(2020, 01, 01);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CW/ /09/C02/36150";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 6988.2m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var deduction = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 69.43m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 2436m;
			invoiceLine1.JI_EnteredUnitPrice = 2.85m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 16m;
			invoiceLine2.JI_EnteredUnitPrice = 2.85m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 6988.2m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 6988.2m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("Declaration Invoice Total (16)", 6918.77m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 69.43m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 6988.2m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation_EXP_DHL()
		{
			var date = new ZDateTime(2020, 01, 01);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "DHL";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;
			
			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 5300m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "CIF";

			var charges = invoice.Charges;
			var freight = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 260m, usd);
			freight.J7_IsIncludedInITOT = false;
			var insurance = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 40m, usd);
			insurance.J7_IsIncludedInITOT = false;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 5000m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 5300m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 5000m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("Declaration Invoice Total (16)", 5300m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 260m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 40m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 0m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 5000m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationExportFCA()
		{
			var date = new ZDateTime(2024, 10, 07);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, usd, 31.79m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CW/  /13/784/14153";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 23740m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var addition = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 50m, usd);
			var deduction = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 1661.8, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 10500m;
			invoiceLine1.JI_EnteredUnitPrice = 1.28m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 4000m;
			invoiceLine2.JI_EnteredUnitPrice = 1.41m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_InvoiceQuantity = 2000m;
			invoiceLine3.JI_EnteredUnitPrice = 2.33m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;
			var entryLine3 = invoiceLine3.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Amount", 23740m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Amount", 22128.2m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 50m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 1661.8m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 23740m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Customs Value (TWD)", 754695m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation_EXP_EXW()
		{
			var date = new ZDateTime(2020, 03, 06);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, usd, 30.29m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CW/ /09/47B/34166";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 41925m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "EXW";

			var charges = invoice.Charges;
			var exw = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 130m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 165m;
			invoiceLine1.JI_EnteredUnitPrice = 185m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 50m;
			invoiceLine2.JI_EnteredUnitPrice = 21m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_InvoiceQuantity = 450m;
			invoiceLine3.JI_EnteredUnitPrice = 23m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;
			var entryLine3 = invoiceLine3.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "EXW", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 41925m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("EXW (16)", 41925m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 130m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 0m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 42055m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("FOB TWD (22)", 1273846m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Entry Line 1 Customs Value", 927469m, entryLine1.CL_CustomsValue);
				AssertEquals("Entry Line 2 Customs Value", 31903m, entryLine2.CL_CustomsValue);
				AssertEquals("Entry Line 3 Customs Value", 314474m, entryLine3.CL_CustomsValue);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationExportCIF()
		{
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "DHL";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5300m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "CIF";

			var charges = invoice.Charges;
			var freight = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 260m, usd);
			freight.J7_IsIncludedInITOT = false;
			var insurance = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 40m, usd);
			insurance.J7_IsIncludedInITOT = false;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 2m;
			invoiceLine1.JI_EnteredUnitPrice = 2500m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Amount", 5300m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Amount", 5300m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight", 260m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 40m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 0m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 5000m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
			});
		}
	}
}
