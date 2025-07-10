using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestMergedLinesOrderByInvoiceDisplaySequence()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2021, 11, 26);

			invoice1.JZ_InvoiceAmount = 30m;
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_InvoiceDisplaySequence = 2;
			invoice2.JZ_InvoiceAmount = 50m;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_InvoiceDisplaySequence = 1;

			invoiceLine1.JI_Tariff = "0101.1000/21";
			invoiceLine1.JI_LinePrice = 1m;
			invoiceLine1.JI_LineNo = 1;
			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0101.1000/22";
			invoiceLine3.JI_LinePrice = 2m;
			invoiceLine3.JI_LineNo = 2;
			var invoiceLine4 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "0101.1000/23";
			invoiceLine4.JI_LinePrice = 3m;
			invoiceLine4.JI_LineNo = 3;

			invoiceLine2.JI_Tariff = "0101.1000/24";
			invoiceLine2.JI_LinePrice = 1m;
			invoiceLine2.JI_LineNo = 4;
			var invoiceLine5 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "0101.1000/25";
			invoiceLine5.JI_LinePrice = 2m;
			invoiceLine5.JI_LineNo = 5;
			var invoiceLine6 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "0101.1000/26";
			invoiceLine6.JI_LinePrice = 3m;
			invoiceLine6.JI_LineNo = 6;

			declaration.DoMerge();

			var mergedLines = declaration.CustomsEntryHeaders[0].MergedLines.Cast<CusEntryLine>().ToList();
			CombineAssertions(() =>
			{
				AssertEquals("One entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("Merged Lines should include 6 lines.", 6, mergedLines.Count);
				AssertEquals((ZShort)1, mergedLines[0].CL_LineNumber);
				AssertEquals("0101100024", mergedLines[0].CL_AdValoremTariff);
				AssertEquals((ZShort)2, mergedLines[1].CL_LineNumber);
				AssertEquals("0101100025", mergedLines[1].CL_AdValoremTariff);
				AssertEquals((ZShort)3, mergedLines[2].CL_LineNumber);
				AssertEquals("0101100026", mergedLines[2].CL_AdValoremTariff);
				AssertEquals((ZShort)4, mergedLines[3].CL_LineNumber);
				AssertEquals("0101100021", mergedLines[3].CL_AdValoremTariff);
				AssertEquals((ZShort)5, mergedLines[4].CL_LineNumber);
				AssertEquals("0101100022", mergedLines[4].CL_AdValoremTariff);
				AssertEquals((ZShort)6, mergedLines[5].CL_LineNumber);
				AssertEquals("0101100023", mergedLines[5].CL_AdValoremTariff);
			});
		}

		public void TestRefreshIncotermAndChargeFactoryForEXWWhenIncotermChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CFI";
			var cfiHashCode = invoice.IncoTermAndChargeFactory.GetHashCode();
			invoice.JZ_IncoTerm = "EXW";
			var exwHashCode = invoice.IncoTermAndChargeFactory.GetHashCode();
			AssertNotEquals(cfiHashCode, exwHashCode);
		}

		public override void TestAllowNonWesternEuropeanCharacterForMarksAndNumbers()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			AssertEquals(true, header.AllowNonWesternEuropeanCharacterForMarksAndNumbers);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		[ExpectNoExceptions]
		public void TestJZ_RelatedIndicator_Caption()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(header.JZ_RelatedIndicatorInfo, "Related Indicator", "Indicates the special relationship between buyer and provider, which affects the trading price.");
		}

		[ExpectNoExceptions]
		public void TestTW_MarksAndNumbers_Caption()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(header.TW_MarksAndNumbersInfo, "Marks & Numbers", "The marks and numbers printed on the outer package of the cargo.");
		}

		[ExpectNoExceptions]
		public void TestJZ_Remarks_Caption()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(header.JZ_RemarksInfo, "Remarks", "The remarks printed on the commercial invoice.");
		}

		public void TestJZ_IncoTermPlace_Caption()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			AssertEquals("Agreed Place", DataBoundResourceStrings.GetDataForProperty(header.JZ_IncoTermPlaceInfo).Caption);
		}

		public void TestJZ_Description_Caption()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			AssertEquals("Goods Description", DataBoundResourceStrings.GetDataForProperty(header.JZ_DescriptionInfo).Caption);
		}

		public void TestDefaultValues()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			AssertEquals(RelationshipIndicatorList.Codes.NoRelationship, header.JZ_RelatedIndicator);
		}

		public void TestSetIncoTermPlaceIfRequired()
		{
			this.declaration.JE_RL_NKOrigin = "CNSHA";
			this.declaration.JE_RL_NKFinalDestination = "TWKEL";

			invoiceHeader.JZ_IncoTermPlace = "XX201103";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			AssertEquals("XX201103", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("China", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			AssertEquals("China", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			AssertEquals("China", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("Taiwan Keelung (Chilung)", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals("Taiwan Keelung (Chilung)", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			AssertEquals("Taiwan Keelung (Chilung)", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrier;
			AssertNullOrEmpty(invoiceHeader.JZ_IncoTermPlace);

			this.declaration.JE_RL_NKOrigin = "CNZ99";
			this.declaration.JE_RL_NKFinalDestination = "TWZ99";
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("China", invoiceHeader.JZ_IncoTermPlace);

			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("Taiwan", invoiceHeader.JZ_IncoTermPlace);

			this.declaration.JE_RL_NKFinalDestination = "TWZ99";
			var declaration = this.declaration as JobDeclaration;
			declaration.JE_Z99FinalDestination = "3/11/2020";
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals("Taiwan 3/11/2020", invoiceHeader.JZ_IncoTermPlace);

			declaration.JE_Z99FinalDestination = ZString.Replicate('X', 35);
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			AssertEquals("Taiwan XXXXXXXXXXXXXXXXXXXXXXXXXXXX", invoiceHeader.JZ_IncoTermPlace);
			AssertEquals(JobComInvoiceHeader.Schema.JZ_IncoTermPlaceMaxLength, invoiceHeader.JZ_IncoTermPlace.Length);
		}

		public void TestJZ_InvoiceCurrExRate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			AssertEquals("JZ_InvoiceCurrExRateInfo readonly is false when the JZ_InvoiceCurrExRateType is true", false, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = false;
			AssertEquals("JZ_InvoiceCurrExRateInfo readonly is true when the JZ_InvoiceCurrExRateType is false", true, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertNullOrEmpty("ChargeTypeList not has 'EXW'", chargeTypeList1.GetDescriptionFromCode("EXW"));
			AssertNullOrEmpty("ChargeTypeList not has 'OTH'", chargeTypeList1.GetDescriptionFromCode("OTH"));

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertNullOrEmpty("ChargeTypeList not has 'EXW'", chargeTypeList1.GetDescriptionFromCode("EXW"));
			AssertNullOrEmpty("ChargeTypeList not has 'OTH'", chargeTypeList1.GetDescriptionFromCode("OTH"));
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;
			adjustedOFT.J7_IsDutiable = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10200m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10200m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		[TestDate(2019, 3, 1, 13, 13, 13)]
		public void TestEffectiveValuationDateCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();
			var today = new ZDateTime(2019, 3, 1);
			AssertEquals("Inv's EffectiveValuationDateCore is today when not EntryInstruction", today, invoice.EffectiveValuationDate);

			var entryInst = declaration.CusEntryInstruction;
			entryInst.CEI_DateForDuty = ZDateTime.Empty;
			Factory.Save();

			invoice = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("Inv's EffectiveValuationDateCore is today when CEI_DateForDuty of declaration.CustomsEntryInstructions is not Valid", today, invoice.EffectiveValuationDate);

			entryInst.CEI_DateForDuty = new ZDateTime(2019, 4, 5);
			Factory.Save();
			AssertEquals("Inv's EffectiveValuationDateCore is today when CEI_DateForDuty of declaration.CustomsEntryInstructions is not only one", new ZDateTime(2019, 4, 5), invoice.EffectiveValuationDate);

			entryInst.CEI_DateForDuty = new ZDateTime(2019, 5, 5);
			Factory.Save();
			AssertEquals("Inv's EffectiveValuationDateCore is CEI_DateForDuty when CEI_DateForDuty of declaration.CustomsEntryInstructions is only one", new ZDateTime(2019, 5, 5), invoice.EffectiveValuationDate);

			entryInst.CEI_DateForDuty = new ZDateTime(2019, 4, 5);
			invoiceLine.JI_CEI = entryInst.PK;
			Factory.Save();
			AssertEquals("Inv's EffectiveValuationDateCore is CEI_DateForDuty when the CEI_DateForDuty of CusEntryInstructions is only one", new ZDateTime(2019, 4, 5), invoice.EffectiveValuationDate);
		}

		public void TestEffectiveCurrencyCode()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var entry = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_JZ = invoice1.PK;
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoice2.PK;

			AssertEquals("USD", invoice1.EffectiveCurrencyCode);
			AssertEquals("USD", invoice2.EffectiveCurrencyCode);

			invoice2.JZ_RX_NKInvoice_Currency = "CNY";
			AssertEquals("USD", invoice1.EffectiveCurrencyCode);
			AssertEquals("USD", invoice2.EffectiveCurrencyCode);

			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			AssertEquals("EUR", invoice1.EffectiveCurrencyCode);
			AssertEquals("EUR", invoice2.EffectiveCurrencyCode);
		}

		public override void TestSettingJZ_JEByFakeDeclarationShouldNotChangeData()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			supplier.OH_FullName = "DUMMY COMP";
			supplier.MainAddress.OA_Address1 = "Address 1";

			var invoice = Factory.New<JobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (JobDeclaration)fakeDeclaration.HeaderData;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_AutoWeightApportion = true;
			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_Weight = 20m;
			AssertEquals(supplier.PK, invoice.JZ_OH_Supplier);
			Factory.Save();
			AssertEquals(supplier.PK, invoice.JZ_OH_Supplier);
			AssertEquals(20m, invoice.JZ_Weight);
		}

		public override void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			Assert("The method is not supported： User can't enter any charges", true);
		}

		protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO)
		{
			return base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO) || bO is Common.CusEntryNumber;
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new JobComInvoiceHeaderLightValidationTester(bizObjToTest);
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

		public void TestExchangeRateWhenShipmentTypeChanged()
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
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;

			AssertEquals(0.7m, invoice1.JZ_InvoiceCurrExRate);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(0.8m, invoice1.JZ_InvoiceCurrExRate);
		}

		public void TestTW_MarksAndNumbers()
		{
			var aLength512 = new ZString('A', 512);
			var aLength514 = new ZString('A', 514);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.TW_MarksAndNumbers = aLength512;
			Factory.Save();

			var noteQuery = new ZQuery(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, invoice.PK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, "Marks & Numbers Overflow");
			noteQuery.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.Equal, "JobComInvoiceHeader");

			var loadInvoice = Factory.LoadTop1<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.Equal, invoice.PK));
			AssertEquals(aLength512, loadInvoice.TW_MarksAndNumbers);
			AssertEquals(aLength512, loadInvoice.JZ_MarksAndNumbers);
			AssertNullOrEmpty(loadInvoice.TW_MarksAndNumbersOverFlow);

			var note = Factory.LoadTop1<StmNote>(noteQuery);
			AssertNull(note);

			invoice.TW_MarksAndNumbers = aLength514;
			Factory.Save();
			loadInvoice = Factory.LoadTop1<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.Equal, invoice.PK));
			AssertEquals(aLength514, loadInvoice.TW_MarksAndNumbers);
			AssertEquals(aLength512, loadInvoice.JZ_MarksAndNumbers);
			AssertEquals("AA", loadInvoice.TW_MarksAndNumbersOverFlow);

			note = Factory.LoadTop1<StmNote>(noteQuery);
			AssertEquals("AA", note.ST_NoteText);
			Assert(note.ST_GC_RelatedCompany.IsEmpty);
		}

		public void TestJustAddedByDataObjectReader()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoice.JustAddedByDataObjectReader = true;
			Factory.Save();
			Assert(!invoice.JustAddedByDataObjectReader);
		}

		protected override Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (fExpectedDocAddressTypes == null)
				{
					fExpectedDocAddressTypes = base.ExpectedDocAddressTypes;
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.SupplierDocumentaryAddress, DocAddressType.SupplierDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.BuyerDocumentaryAddress, DocAddressType.BuyerDocumentaryAddress);
				}
				return fExpectedDocAddressTypes;
			}
		}
		Hashtable fExpectedDocAddressTypes;

		public void TestSupplierDocumentaryAddress()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ORG";
			supplier.OH_FullName = "DUMMY COMP";
			supplier.MainAddress.OA_Address1 = "Address 1";

			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var supplierDocAddr = invoice.SupplierDocumentaryAddress;
			AssertEquals("DocAddressType should be SupplierDocumentaryAddress", DocAddressType.SupplierDocumentaryAddress, supplierDocAddr.DocAddressType);

			Assert(invoice.JZ_OH_Supplier.IsEmpty);
			supplierDocAddr.OrganisationPK = supplier.PK;
			AssertEquals("JZ_OH_Supplier should be set.", supplier.PK, invoice.JZ_OH_Supplier);
		}

		public void TestJZ_LetterOfCreditNumber()
		{
			var header = GetNewBusinessObject() as JobComInvoiceHeader;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(header.JZ_LetterOfCreditNumberInfo);
			AssertEquals("JZ_LetterOfCreditNumber Caption", "L/C Number", resourceStringData.Caption);
		}

		public void TestJZ_LetterOfCreditDate()
		{
			var header = GetNewBusinessObject() as JobComInvoiceHeader;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(header.JZ_LetterOfCreditDateInfo);
			AssertEquals("JZ_LetterOfCreditDate Caption", "L/C Date", resourceStringData.Caption);
		}

		public void TestBuyerDocumentaryAddress()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG";
			importer.OH_FullName = "DUMMY COMP";
			importer.MainAddress.OA_Address1 = "Address 1";

			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var buyerDocAddr = invoice.BuyerDocumentaryAddress;
			buyerDocAddr.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType should be BuyerDocumentaryAddress", DocAddressType.BuyerDocumentaryAddress, buyerDocAddr.DocAddressType);
				AssertType<BuyerAddressRequirement>(buyerDocAddr.Requirement);
				AssertType<BuyerLocalAddressRequirement>(buyerDocAddr.LocalAddress.Requirement);
			});

			Assert(invoice.JZ_OH_Buyer.IsEmpty);
			buyerDocAddr.OrganisationPK = importer.PK;
			AssertEquals("JZ_OH_Buyer should be set.", importer.PK, invoice.JZ_OH_Buyer);
		}

		public void TestLoadCusPackingList()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var loadedPackingList = invoice.LoadCusPackingList(Factory);
			AssertNull(loadedPackingList);

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var invoiceOtherFactory = otherFactory.Load<JobComInvoiceHeader>(invoice.PK);
			loadedPackingList = invoiceOtherFactory.LoadCusPackingList(Factory);
			AssertNotNull(loadedPackingList);
			AssertType<CusPackingList>(loadedPackingList);
		}

		public void TestCreateCusPackingList()
		{
			var expectedDesc = "Goods Description";
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_Description = expectedDesc;
			invoice.JZ_InvoiceDate = new ZDateTime(2022, 03, 20);
			var createdPackingList = invoice.CreateCusPackingList(Factory);
			AssertEquals(expectedDesc, createdPackingList.CUL_Description);
			AssertEquals(new ZDate(2022, 03, 20), createdPackingList.CUL_PackingListDate);
			AssertType<CusPackingList>(createdPackingList);
		}

		public void TestHasCusPackingList()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			Assert("Packing List should not exist", !invoice.HasCusPackingList);

			Factory.Save();
			invoice.CreateCusPackingList(Factory);
			Assert("Packing List should not exist", !invoice.HasCusPackingList);

			Factory.Save();
			Assert("invoice has a Packing List", invoice.HasCusPackingList);
		}

		public void TestHasInternationFreightAmount()
		{
			var decl = Factory.New<JobDeclaration>();
			var invoice = decl.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			Assert("HasInternationFreightAmount is false", !invoice.HasInternationFreightAmount);

			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			Assert("HasInternationFreightAmount is false", !invoice.HasInternationFreightAmount);
			charge.J7_Amount = 0.01m;
			Assert("HasInternationFreightAmount is true", invoice.HasInternationFreightAmount);

			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			Assert("HasInternationFreightAmount is false", !invoice.HasInternationFreightAmount);

			var groupCharge = invoice.GroupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			groupCharge.J7_Amount = 0.01m;
			Assert("HasInternationFreightAmount is true", invoice.HasInternationFreightAmount);

			groupCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			Assert("HasInternationFreightAmount is false", !invoice.HasInternationFreightAmount);
		}

		public void TestGetTWInvoiceLineMaxDecimalPlaces()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfoChild.TWL_DocumentaryQty = 2.254m;
			invoiceLine.AddInfoChild.TWL_DocumentaryUQ = "PK";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfoChild.TWL_DocumentaryQty = 2.2545422222m;
			invoiceLine2.AddInfoChild.TWL_DocumentaryUQ = "PK";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.AddInfoChild.TWL_DocumentaryQty = 2m;
			invoiceLine3.AddInfoChild.TWL_DocumentaryUQ = "PK";
			AssertEquals(4, invoiceHeader.GetTWInvoiceLineMaxDecimalPlaces(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name));

			invoiceLine2.AddInfoChild.TWL_DocumentaryQty = 2.255m;
			AssertEquals(3, invoiceHeader.GetTWInvoiceLineMaxDecimalPlaces(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name));
		}

		class JobComInvoiceHeaderLightValidationTester : LightValidationTester
		{
			public JobComInvoiceHeaderLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != Common.AutoCusEntryNum.Schema.CE_Category
					&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryIsSystemGenerated
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryNum
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryType
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_ParentID
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_ParentTable
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_RN_NKCountryCode
						&& propertyName != JobComInvoiceLine.Schema.JI_JZ
						&& propertyName != JobComInvoiceLine.Schema.JI_AddInfo
						&& propertyName != JobComInvoiceLine.Schema.JI_NAddInfo;
			}
		}

		#region Implementation

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Taiwan;

		#endregion

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}
}
