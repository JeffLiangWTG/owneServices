using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		public void TestRoundDownValueIfLessThanCriticalValueRoundUpOtherwisef_ValueIs9999999999()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			var declaration = (JobDeclaration)GetNewDeclaration();
			var header = declaration.Invoices.AddNew();
			header.JZ_InvoiceNumber = "1";
			header.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 9999999999m;
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 1;

			declaration.MessageInitiator = new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = "IMP";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			var entryLine = invoiceLine.CusEntryLine;
			var docCusEntryLine = DocCusEntryLine.New(entryLine, Factory);
			AssertNoExceptionThrown(() =>
			{
				_ = docCusEntryLine.ValueInWholeUSDollars;
			});
		}

		public override void TestDutyAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
		}

		public override void TestGSTVATAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
		}

		public override void TestLinePricesWithCurrency()
		{
			AssertEquals(DocEntryLineMergeOfTwoInvoiceLines.LinePricesWithCurrency, "200.05 USD");
		}

		public override void TestCustomsValue()
		{
			EntryLineInternal.CL_CustomsValue = 12.34M;
			AssertEquals("CustomsValue", 12m, EntryLineWrapperInternal.CustomsValue);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override DocCusEntryLine CreateEntryLineWrapper(Enterprise.Customs.Business.ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		Enterprise.Customs.Business.CusEntryLine fEntryLineMergeOfTwoInvoiceLines;
		protected override Enterprise.Customs.Business.ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
					JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
					JobComInvoiceHeader header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "USD";

					JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;

					declaration.MessageInitiator = new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = "IMP";
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					declaration.DoMerge();
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
					fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLineMergeOfTwoInvoiceLines;
			}
		}

		protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
		}

		#endregion
	}
}
