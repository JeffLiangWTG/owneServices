using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105GoodsShipment_CustomsValuation))]
	sealed class NX5105GoodsShipment_CustomsValuationTest : TestCaseWithFactory
	{
		#region Exit To Entry Charge Amount
		[ExpectNoExceptions]
		public void TestCustomsValuation_ExitToEntryChargeAmount()
		{
			var entryHeader = GetEntryHeader();
			var charges = entryHeader.InvoiceHeaders().First().Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 120m, Core.Constants.CurrencyCodes.UnitedStates);
			ICustomsValuation customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			var expectedAmount = 120m;
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsValuation.ExitToEntryChargeAmount, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region Freight Charge Amount
		[ExpectNoExceptions]
		public void TestCustomsValuation_FreightChargeAmount()
		{
			var entryHeader = GetEntryHeader();
			var charges = entryHeader.InvoiceHeaders().FirstOrDefault().Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			ICustomsValuation customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			var expectedAmount = 100m;
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsValuation.FreightChargeAmount, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region Other Charge Deduction Amount
		[ExpectNoExceptions]
		public void TestCustomsValuation_OtherChargeDeductionAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			ICustomsValuation customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.OtherChargeDeductionAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "CustomsValuation.OtherChargeDeductionAmount should be");
			var invoiceLine1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.CL_ValueForVAT = 2560.47m;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine2.CL_ValueForVAT = 3546.78m;
			invoiceLine1.JI_VatPymntMthd = "CAS";
			invoiceLine2.JI_VatPymntMthd = "CAS";
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.OtherChargeDeductionAmount, NUnit.Framework.Is.EqualTo(6107m).Using(CustomComparers.TypeComparison), "CustomsValuation.OtherChargeDeductionAmount should be");
			invoiceLine2.JI_VatPymntMthd = "DEF";
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.OtherChargeDeductionAmount, NUnit.Framework.Is.EqualTo(2560m).Using(CustomComparers.TypeComparison), "CustomsValuation.OtherChargeDeductionAmount should be");
		}

		#endregion
		#region Party Relationship Code
		[ExpectNoExceptions]
		public void TestCustomsValuation_PartyRelationshipCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			ICustomsValuation customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceHeader.JZ_RelatedIndicator = ZString.Empty;
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.PartyRelationshipCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "CustomsValuation.PartyRelationshipCode should be");
			invoiceHeader.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.NoRelationship;
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.PartyRelationshipCode, NUnit.Framework.Is.EqualTo("135").Using(CustomComparers.TypeComparison), "CustomsValuation.PartyRelationshipCode should be");
			invoiceHeader.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicator;
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.PartyRelationshipCode, NUnit.Framework.Is.EqualTo("136").Using(CustomComparers.TypeComparison), "CustomsValuation.PartyRelationshipCode should be");
			invoiceHeader.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicatorNoEffect;
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.PartyRelationshipCode, NUnit.Framework.Is.EqualTo("137").Using(CustomComparers.TypeComparison), "CustomsValuation.PartyRelationshipCode should be");

			invoiceHeader.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicator138;
			entryLine.ResetTotalsAndCachedValues();
			entryLine.RefreshInvoiceLines();
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.ResetInvoiceHeadersAndLines();
			customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			NUnit.Framework.Assert.That(customsValuation.PartyRelationshipCode, NUnit.Framework.Is.EqualTo(MessageConstants.CustomsValuationPartyRelationshipCode._138).Using(CustomComparers.TypeComparison), "CustomsValuation.PartyRelationshipCode should be");
		}

		#endregion
		#region Other Charge Amount
		[ExpectNoExceptions]
		public void TestCustomsValuation_OtherChargeAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceCurrExRate = 30m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoice.JZ_InvoiceCurrExRateType = Customs.Business.ChargeExchangeRateTypeList.Codes.FixedRate;
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "0000.00.00.00Y";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var charges = invoice.Charges;
			var includedApplicableCharge = charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsDutiable = true;
			includedApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var includedNotApplicableCharge = charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsDutiable = false;
			includedNotApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var notIncludedApplicableCharge = charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 300m, Core.Constants.CurrencyCodes.UnitedStates);
			notIncludedApplicableCharge.J7_IsDutiable = true;
			notIncludedApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var discountNotApplicable = charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400m, Core.Constants.CurrencyCodes.UnitedStates);
			discountNotApplicable.J7_IsDutiable = false;
			discountNotApplicable.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			ICustomsValuation customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			var expectedAmount = 300m;
			NUnit.Framework.Assert.That(entryHeader.CH_TotalAdditionsInInvoiceCurrency, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsValuation.OtherChargeAmount, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region Other Deduction Amount
		[ExpectNoExceptions]
		public void TestCustomsValuation_OtherDeductionAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceCurrExRate = 30m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoice.JZ_InvoiceCurrExRateType = Customs.Business.ChargeExchangeRateTypeList.Codes.FixedRate;
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "0000.00.00.00Y";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var charges = invoice.Charges;
			var includedApplicableCharge = charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsDutiable = true;
			includedApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var includedNotApplicableCharge = charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsDutiable = false;
			includedNotApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var notIncludedApplicableCharge = charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 300m, Core.Constants.CurrencyCodes.UnitedStates);
			notIncludedApplicableCharge.J7_IsDutiable = true;
			notIncludedApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var discountNotApplicable = charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400m, Core.Constants.CurrencyCodes.UnitedStates);
			discountNotApplicable.J7_IsDutiable = false;
			discountNotApplicable.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			ICustomsValuation customsValuation = new NX5105GoodsShipment_CustomsValuation(entryHeader);
			var expectedAmount = 200m;
			NUnit.Framework.Assert.That(entryHeader.CH_TotalDeductionsInInvoiceCurrency, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsValuation.OtherDeductionAmount, NUnit.Framework.Is.EqualTo(expectedAmount).Using(CustomComparers.TypeComparison));
		}

		#endregion
		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105GoodsShipment_CustomsValuation(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CusEntryInstruction;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				new NX5105GoodsShipment_CustomsValuation(entryHeader);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation_IMP_CIF_OFTInITOT()
		{
			var date = new ZDateTime(2021, 01, 04);
			var jpy = Core.Constants.CurrencyCodes.Japan;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, jpy, 0.2776m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CG/ /10/418/05005";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 6097500m;
			invoice.JZ_RX_NKInvoice_Currency = jpy;
			invoice.JZ_IncoTerm = "CIF";

			var charges = invoice.Charges;
			var freight = charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 8100m, jpy);
			freight.J7_IsIncludedInITOT = true;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 15m;
			invoiceLine1.JI_EnteredUnitPrice = 7500m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 175m;
			invoiceLine2.JI_EnteredUnitPrice = 13500m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_InvoiceQuantity = 105m;
			invoiceLine3.JI_EnteredUnitPrice = 18500m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_InvoiceQuantity = 105m;
			invoiceLine4.JI_EnteredUnitPrice = 16000m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;
			var entryLine3 = invoiceLine3.CusEntryLine;
			var entryLine4 = invoiceLine4.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "CFR", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 6097500m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 6097500m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("FOB (17)", 6089400m, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				AssertEquals("Freight (18)", 8100m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (19)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (20)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (21)", 0m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("CIF (22)", 6097500m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("CIF TWD (22)", 1692666m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Item 1 Customs Value", 31230m, entryLine1.CL_CustomsValue);
				AssertEquals("Item 2 Customs Value", 655830m, entryLine2.CL_CustomsValue);
				AssertEquals("Item 3 Customs Value", 539238m, entryLine3.CL_CustomsValue);
				AssertEquals("Item 4 Customs Value", 466368m, entryLine4.CL_CustomsValue);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationImportCIF()
		{
			var date = new ZDateTime(2020, 12, 27);
			var jpy = Core.Constants.CurrencyCodes.Japan;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, jpy, 0.2776m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CG/ /10/418/05005";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 6097500m;
			invoice.JZ_RX_NKInvoice_Currency = jpy;
			invoice.JZ_IncoTerm = "CIF";

			var charges = invoice.Charges;
			var freight = charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 8100m, jpy);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 15m;
			invoiceLine1.JI_EnteredUnitPrice = 7500m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 175m;
			invoiceLine2.JI_EnteredUnitPrice = 13500m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_InvoiceQuantity = 105m;
			invoiceLine3.JI_EnteredUnitPrice = 18500m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_InvoiceQuantity = 105m;
			invoiceLine4.JI_EnteredUnitPrice = 16000m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;
			var entryLine3 = invoiceLine3.CusEntryLine;
			var entryLine4 = invoiceLine4.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "CFR", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Amount", 6097500m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("FOB", 6089400m, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				AssertEquals("Freight", 8100m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 0m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 6097500m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Customs Value (TWD)", 1692666m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Entry Line 1 Customs Value", 31230m, entryLine1.CL_CustomsValue);
				AssertEquals("Entry Line 2 Customs Value", 655830m, entryLine2.CL_CustomsValue);
				AssertEquals("Entry Line 3 Customs Value", 539238m, entryLine3.CL_CustomsValue);
				AssertEquals("Entry Line 4 Customs Value", 466368m, entryLine4.CL_CustomsValue);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationImportEXW()
		{
			var date = new ZDateTime(2021, 01, 03);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, usd, 34.98m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CA/ /10/418/05009";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 9577.44m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "EXW";

			var charges = invoice.Charges;
			var freight = charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 2389.5m, usd);
			var addition = charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 952.5m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 3m;
			invoiceLine1.JI_EnteredUnitPrice = 1230.9m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 2m;
			invoiceLine2.JI_EnteredUnitPrice = 1176.45m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_EnteredUnitPrice = 3531.84m;

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
				AssertEquals("Invoice Amount", 9577.44m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("FOB", 9577.44m, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				AssertEquals("Freight", 2389.5m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 952.5m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 0m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 12919.44m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Customs Value (TWD)", 451922m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Entry Line 1 Customs Value", 174244m, entryLine1.CL_CustomsValue);
				AssertEquals("Entry Line 2 Customs Value", 111024m, entryLine2.CL_CustomsValue);
				AssertEquals("Entry Line 3 Customs Value", 166654m, entryLine3.CL_CustomsValue);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationImportFOB()
		{
			var date = new ZDateTime(2020, 10, 29);
			var eur = Core.Constants.CurrencyCodes.EuropeanUnion;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, eur, 34.15m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "CA/ /09/C02/10740";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 36053.24m;
			invoice.JZ_RX_NKInvoice_Currency = eur;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var freight = charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1390m, eur);
			var deduction = charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 76.6m, eur);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 33500m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_EnteredUnitPrice = 970m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_EnteredUnitPrice = 960m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_InvoiceQuantity = 1m;
			invoiceLine4.JI_EnteredUnitPrice = 180m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction.PK;
			invoiceLine5.JI_InvoiceQuantity = 1m;
			invoiceLine5.JI_EnteredUnitPrice = 160m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction.PK;
			invoiceLine6.JI_InvoiceQuantity = 72m;
			invoiceLine6.JI_EnteredUnitPrice = 1.17m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_CEI = entryInstruction.PK;
			invoiceLine7.JI_InvoiceQuantity = 240m;
			invoiceLine7.JI_EnteredUnitPrice = 0.51m;

			var invoiceLine8 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_CEI = entryInstruction.PK;
			invoiceLine8.JI_InvoiceQuantity = 20m;
			invoiceLine8.JI_EnteredUnitPrice = 1.78m;

			var invoiceLine9 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine9.JI_CEI = entryInstruction.PK;
			invoiceLine9.JI_InvoiceQuantity = 20m;
			invoiceLine9.JI_EnteredUnitPrice = 1.37m;

			var invoiceLine10 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine10.JI_CEI = entryInstruction.PK;
			invoiceLine10.JI_InvoiceQuantity = 20m;
			invoiceLine10.JI_EnteredUnitPrice = 0.68m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;
			var entryLine3 = invoiceLine3.CusEntryLine;
			var entryLine4 = invoiceLine4.CusEntryLine;
			var entryLine5 = invoiceLine5.CusEntryLine;
			var entryLine6 = invoiceLine6.CusEntryLine;
			var entryLine7 = invoiceLine7.CusEntryLine;
			var entryLine8 = invoiceLine8.CusEntryLine;
			var entryLine9 = invoiceLine9.CusEntryLine;
			var entryLine10 = invoiceLine10.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Amount", 36053.24m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("FOB", 36053.24m, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				AssertEquals("Freight", 1390m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 76.6m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 37366.64m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Customs Value (TWD)", 1276071m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Entry Line 1 Customs Value", 1185702m, entryLine1.CL_CustomsValue);
				AssertEquals("Entry Line 2 Customs Value", 34332m, entryLine2.CL_CustomsValue);
				AssertEquals("Entry Line 3 Customs Value", 33978m, entryLine3.CL_CustomsValue);
				AssertEquals("Entry Line 4 Customs Value", 6371m, entryLine4.CL_CustomsValue);
				AssertEquals("Entry Line 5 Customs Value", 5663m, entryLine5.CL_CustomsValue);
				AssertEquals("Entry Line 6 Customs Value", 2982m, entryLine6.CL_CustomsValue);
				AssertEquals("Entry Line 7 Customs Value", 4332m, entryLine7.CL_CustomsValue);
				AssertEquals("Entry Line 8 Customs Value", 1260m, entryLine8.CL_CustomsValue);
				AssertEquals("Entry Line 9 Customs Value", 970m, entryLine9.CL_CustomsValue);
				AssertEquals("Entry Line 10 Customs Value", 481m, entryLine10.CL_CustomsValue);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationImportUPS()
		{
			var date = new ZDateTime(2025, 1, 25);
			var jpy = Core.Constants.CurrencyCodes.Japan;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, jpy, 32.965m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "UPS";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3499m;
			invoice.JZ_RX_NKInvoice_Currency = jpy;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			var freight = charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 780m, jpy);
			var deduction = charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 874.75m, jpy);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 3499m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Amount", 3499m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("FOB", 3499m, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				AssertEquals("Freight", 780m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 874.75m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 3404.25m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Customs Value (TWD)", 112221m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Entry Line 1 Customs Value", 112221m, actual: entryLine1.CL_CustomsValue);
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsValuationImportUPSGroupInvoice()
		{
			var date = new ZDateTime(2025, 1, 25);
			var jpy = Core.Constants.CurrencyCodes.Japan;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, jpy, 32.965m, date, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "UPS";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];
			groupInvoice.JZ_GroupInvoice = true;

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 3499m;
			invoice.JZ_RX_NKInvoice_Currency = jpy;
			invoice.JZ_IncoTerm = "FOB";

			var groupCharges = groupInvoice.Charges;
			var freight = groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 780m, jpy);
			var deduction = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 874.75m, jpy);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 3499m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Amount", 3499m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("FOB", 3499m, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				AssertEquals("Freight", 780m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions", 874.75m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("Customs Value", 3404.25m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Customs Value (TWD)", 112221m, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				AssertEquals("Entry Line 1 Customs Value", 112221m, actual: entryLine1.CL_CustomsValue);
			});
		}

		CusEntryHeader GetEntryHeader()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceCurrExRate = 30.945m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoice.JZ_InvoiceCurrExRateType = Customs.Business.ChargeExchangeRateTypeList.Codes.FixedRate;
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "0000.00.00.00Y";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var charges = invoice.Charges;
			var includedApplicableCharge = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsDutiable = true;
			includedApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var includedNotApplicableCharge = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsDutiable = false;
			includedNotApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var notIncludedApplicableCharge = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 300m, Core.Constants.CurrencyCodes.UnitedStates);
			notIncludedApplicableCharge.J7_IsDutiable = true;
			notIncludedApplicableCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var discountNotApplicable = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 400m, Core.Constants.CurrencyCodes.UnitedStates);
			discountNotApplicable.J7_IsDutiable = false;
			discountNotApplicable.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			return declaration.CustomsEntryHeaders[0];
		}
	}
}
