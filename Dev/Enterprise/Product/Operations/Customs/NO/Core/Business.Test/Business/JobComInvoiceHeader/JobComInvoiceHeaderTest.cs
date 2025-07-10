using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public void TestGetFetchStrategyCore()
	{
		AssertType<JobComInvoiceHeaderFetchStrategy>(invoice.FetchStrategy);
	}

	public override void TestLocalCurrencyCodeCoreOverride()
	{
		AssertEquals("LocalCurrencyCode", Core.Constants.CurrencyCodes.Norway, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
	}

	public void TestJZ_Calc_ChargesAmount()
	{
		using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
		{
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = GetLocalCurrencyCode();

			CombineAssertions(() =>
			{
				var chargeCodeOFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("With OFT", 200m, invoice.JZ_Calc_ChargesAmount);

				var chargeCodeONS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("With ONS", 250m, invoice.JZ_Calc_ChargesAmount);

				chargeCodeONS.J7_IsDutiable = true;
				Factory.Save();
				AssertEquals("When ONS.IsDutiable=true", 250m, invoice.JZ_Calc_ChargesAmount);

				var groupCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("When group charge", 250m, invoice.JZ_Calc_ChargesAmount);

				var deduction1 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 22m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("When DED as invoice charge", 228m, invoice.JZ_Calc_ChargesAmount);

				var deduction2 = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 11m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("When DED as group charge", 228m, invoice.JZ_Calc_ChargesAmount);

				deduction1.Delete();
				Factory.Save();
				AssertEquals("After delete DED invoicecharge", 250m, invoice.JZ_Calc_ChargesAmount);
			});
		}
	}

	public void TestJZ_Calc_ChargesAmountPercent()
	{
		using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
		{
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = GetLocalCurrencyCode();

			CombineAssertions(() =>
			{
				var chargeCodeOFT = groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("With OFT", 100m, invoice.JZ_Calc_ChargesAmount);

				var chargeCodeONS = groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
				chargeCodeONS.J7_Percentage = 15m;
				Factory.Save();
				AssertEquals("With ONS", 250m, invoice.JZ_Calc_ChargesAmount);

				var chargeCodeOTH = groupCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges);
				chargeCodeOTH.J7_Percentage = 20m;
				Factory.Save();
				AssertEquals("With OTH", 450m, invoice.JZ_Calc_ChargesAmount);

				var deduction1 = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 10m, GetLocalCurrencyCode());
				Factory.Save();
				AssertEquals("With DED amount", 450m, invoice.JZ_Calc_ChargesAmount);

				var deduction2 = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge);
				deduction2.J7_Percentage = 15m;
				Factory.Save();
				AssertEquals("With DED percent", 450m, invoice.JZ_Calc_ChargesAmount);
			});
		}
	}

	public void TestJZ_InvoiceCurrExRateType_ReadOnly()
	{
		CombineAssertions(() =>
		{
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("ReadOnly", true, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);

			invoice.JZ_InvoiceCurrExRateType = Customs.Business.ChargeExchangeRateTypeList.Codes.FixedRate;
			AssertEquals("Editable", false, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);
		});
	}

	public void TestJZ_ValuationCodeList()
	{
		invoice.JZ_ValuationCode = "01";
		AssertEquals("ReadOnly", false, invoice.JZ_ValuationCodeInfo.ReadOnly);
	}

	public void TestJZ_ValuationCodeList_ResourceStringData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoice.JZ_ValuationCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "[24] Tran. Nature", resourceStringData.Caption);
			AssertEquals("Full description", "The nature of the transaction.", resourceStringData.FullDescription);
		});
	}

	public void TestJZ_ValuationCodeDefaultValue()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertEquals("JZ_ValuationCode Default Value should be - 01", "01", invoiceHeader.JZ_ValuationCode);
	}

	public void TestChargeVGEisAddedWhenProcedureIs6021()
	{
		var dec = invoice.JobDeclaration;
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var groupHeader = dec.JobComInvoiceGroupHeaders[0];
		var entry = dec.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			entry.CEI_Procedure = "6021";
			Assert("Charge VGE should exist", groupHeader.Charges.HasChargeWithThisKey(new Common.ChargeCodeChargeKey(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, false, false)));

			entry.CEI_Procedure = "6000";
			Assert("Charge VGE should not exist", !groupHeader.Charges.HasChargeWithThisKey(new Common.ChargeCodeChargeKey(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, false, false)));
		});
	}

	public override void TestChargeTypeList()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		ICommonInvoice commonInvoice = dec.Invoices.AddNew();
		var chargeTypeList1 = commonInvoice.ChargeTypeList;
		var chargeTypeList2 = commonInvoice.ChargeTypeList;
		CombineAssertions(() =>
		{
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.AddPair(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, NOInvoiceChargeTypesImport.Descriptions.ValueOfGoodsExported);
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		});
	}

	public override void TestInvoiceLineChargeAffectsBalanceCalculation()
	{
		var declaration = base.Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_IncoTerm = "FOB";
		invoiceHeader.JZ_InvoiceAmount = 25000m;
		invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var invoiceLine = declaration.InvoiceLines.AddNew();
		invoiceLine.JI_LinePrice = 10000m;
		var invoiceLine2 = declaration.InvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 20000m;
		declaration.ResumeApportionment();
		CombineAssertions(() =>
		{
			AssertEquals(0m, invoiceHeader.JZ_Calc_ChargesExcludedFromITOT);
			AssertNotEquals("Not balanced yet", 0m, invoiceHeader.JZ_Calc_Balance);
			var invoiceLineCharge = invoiceLine2.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			invoiceLineCharge.J7_IsDutiable = false;
			invoiceLineCharge.J7_IsGSTApplicable = false;
			invoiceLineCharge.J7_ChargeDescription = GetDiscountChargeDescriptionForTest();
			declaration.ResumeApportionment();
			AssertEquals("line level Discount", -5000m, invoiceHeader.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Now balanced", 0m, invoiceHeader.JZ_Calc_Balance);
			var invoiceCharge = invoiceHeader.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			invoiceCharge.J7_IsDutiable = false;
			invoiceCharge.J7_IsGSTApplicable = false;
			invoiceCharge.J7_ChargeDescription = GetDiscountChargeDescriptionForTest();
			declaration.ResumeApportionment();
			AssertEquals("Invoice Level Discount. Line level Discount is disregarded", -5000m, invoiceHeader.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Still balanced", 0m, invoiceHeader.JZ_Calc_Balance);
		});
	}

	protected override string GetDiscountChargeCodeForTest() => Common.CustomsChargeTypeList.Codes.Discount;

	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Norway;

	public void TestCurrencyConverterType() => CombineAssertions(() =>
	{
		invoice.IsJZ_InvoiceCurrExRateUserEnterable = (ZBool)false;
		AssertType<MasterFiles.Business.CurrencyConverterWithFixedExchangeRatesDataProvider>("When IsJZ_InvoiceCurrExRateUserEnterable is false", invoice.CurrencyConverter);

		invoice.IsJZ_InvoiceCurrExRateUserEnterable = (ZBool)true;
		AssertType<CurrencyConverterWithFixedExchangeRatesDataProvider>("When IsJZ_InvoiceCurrExRateUserEnterable is true", invoice.CurrencyConverter);
	});

	[TestDate(2025, 3, 1)]
	public void TestEffectiveValuationDate() => CombineAssertions(() =>
	{
		var invoiceLine = invoice.InvoiceLines.AddNew();
		AssertEquals("No date specified, use current date", ZDate.Today, invoice.EffectiveValuationDate);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_DateForDuty = new ZDate(2025, 2, 1);

		AssertEquals("Use date from CEI_DateForDuty if exist", new ZDate(2025, 2, 1), invoice.EffectiveValuationDate);

		entryInstruction.CEI_DateForDuty = ZDate.Empty;
		AssertEquals("Entry exists, but no date specified, use current date", ZDate.Today, invoice.EffectiveValuationDate);
	});

	public void TestAllLines()
	{
		var lineOne = invoice.InvoiceLines.AddNew();
		var lineTwo = invoice.InvoiceLines.AddNew();

		var allInvoiceLines = invoice.AllLines.ToArray();
		AssertContainsExactElementsInAnyOrder("AllLines", [lineOne.PK, lineTwo.PK], allInvoiceLines.Select(l => l.PK));
	}

	#region Overseas carges (OFT/ONS) are dutiable in NO

	public override void TestCFRCalculationWithFreightAdjustedFlag()
	{
		var declaration = this.invoice.JobDeclaration;
		declaration.JE_ExportDate = new ZDateTime(2004, 10, 30);
		var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoice.JZ_IncoTerm = "CFR";
		invoice.JZ_InvoiceAmount = 10_500m;
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

		var invoiceCharge1 = invoice.Charges.AddNew();
		invoiceCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
		invoiceCharge1.J7_Amount = 500m;
		invoiceCharge1.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
		invoiceCharge1.J7_IsDutiable = true;
		invoiceCharge1.J7_IsIncludedInITOT = false;
		var invoiceCharge2 = invoice.Charges.AddNew();
		invoiceCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
		invoiceCharge2.J7_Amount = 300m;
		invoiceCharge2.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
		invoiceCharge2.J7_AdjustedCharge = true;
		invoiceCharge2.J7_IsIncludedInITOT = false;
		CombineAssertions(() =>
		{
			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10_000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10_500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10_500m, invoice.JZ_Calc_CIFAmount);
			invoiceCharge1.J7_IsIncludedInITOT = true;
			invoiceCharge2.J7_IsIncludedInITOT = true;
			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10_500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10_500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10_500m, invoice.JZ_Calc_CIFAmount);
		});
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		groupCharges = declaration.TopGroupInvoice.Charges;
		invoice = declaration.Invoices.AddNew();
	}

	Common.JobComInvChargeCollection<GroupInvoiceCharge> groupCharges;
	JobComInvoiceHeader invoice;

	protected override Type ExpectedTypeOfGroupCharges => typeof(Common.JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<InvoiceCharge>);
}

