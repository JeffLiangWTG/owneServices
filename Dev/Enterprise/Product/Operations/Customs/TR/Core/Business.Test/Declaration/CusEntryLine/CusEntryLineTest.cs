using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestGetReadOnlySupportingDocumentsWhenJobDeclarationSupportingDocumentsHasItemsWillNotThrowExceptions()
		{
			AssertNoExceptionThrown("Get ReadOnlySupportingDocuments when JobDeclaration.SupportingDocuments has items", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.SupportingDocuments.AddNew();
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				var entryLine = entryHeader.AllEntryLines.AddNew();
				_ = entryLine.ReadOnlySupportingDocuments;
			});
		}

		public void TestEffectiveDescriptionAlwaysReturnJI_NDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "220860990000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Muhtevası 2 litreyi geçen kaplarda olanlar");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_Description = "English Description";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals("EffectiveDescription should always return JI_NDescription rather than JI_Description.", "Muhtevası 2 litreyi geçen kaplarda olanlar", entryLine.EffectiveDescription);
		}

		public void TestCPDecs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var cpDec1 = Factory.New<CusEntryCPDec>();
			var cpDec2 = Factory.New<CusEntryCPDec>();
			cpDec1.ON_ParentID = entryLine.PK;
			cpDec2.ON_ParentID = entryLine.PK;

			CombineAssertions("This test is for CPDecs related with CusEntryLine", () =>
			{
				AssertEquals(2, entryLine.CPDecCollection.Count);
				AssertEquals(true, entryLine.CPDecCollection.Contains(cpDec1));
				AssertEquals(true, entryLine.CPDecCollection.Contains(cpDec2));
			});
		}

		public override void TestDutyRateDescription()
		{
			var cusEntryLine = Factory.New<CusEntryLineForTest>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = cusEntryLine.PK;
			AssertEquals(ZDecimal.Zero, cusEntryLine.GSTRate);
			AssertEquals(ZString.Empty, cusEntryLine.DutyRateDescription);
			AssertEquals(ZString.Empty, invoiceLine.DutyAmountsAsString);
			var b00Fee = cusEntryLine.Fees.AddNew();
			b00Fee.CF_Rate = 17.5;
			b00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			var a30Fee = cusEntryLine.Fees.AddNew();
			a30Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			a30Fee.CF_ChargeAmount = 202m;
			a30Fee.CF_BaseValue = 404m;
			a30Fee.CF_Rate = 50;
			var a00Fee = cusEntryLine.Fees.AddNew();
			a00Fee.CF_ChargeAmount = 33m;
			a00Fee.CF_BaseValue = 100m;
			a00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			a00Fee.CF_Rate = 33;
			AssertEquals("GetGSTRate(): GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:33.00\r\nA30:202.00", invoiceLine.DutyAmountsAsString);
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(new ZDateTime(2005, 6, 1), new ZDateTime(2005, 6, 5), RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_CEI = instruction.PK;
			line2.JI_CEI = instruction.PK;
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			var line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line1OFT.J7_Amount = 10.0m;
			var line2ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			var line2OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line2OFT.J7_Amount = 20.0m;

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("FOB", 690.0m, entryLine.FOBInLocalCurrency.Amount);
				AssertEquals("CIF", ZDecimal.Zero, entryLine.CIFInLocalCurrency.Amount);
				AssertEquals("Overseas Freight", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
				AssertEquals("Overseas Insurance", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
				AssertEquals("T and I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
			});
		}

		protected override bool RatesAreReciprocal => true;

		protected override Type ExpectedTypeOfFees => typeof(EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		sealed class CusEntryLineForTest : CusEntryLine
		{
			public CusEntryLineForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public ZString GetDutyRateDescriptionForTest() => GetDutyRateDescription();

			public ZDecimal GetGstRateForTest() => GetGSTRate();
		}
	}
}
