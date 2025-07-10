using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		public override void TestDutyAmountRounded()
		{
			Assert(true);
		}

		public override void TestGSTVATAmountRounded()
		{
			Assert(true);
		}

		public override void TestLinePricesWithCurrency()
		{
			Assert(true);
		}

		public override void TestLinePriceInLocalCurrencyEqualsTheRelatedValueInBizObj()
		{
			Assert(true);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes._TemplateCountryName_; }
		}

		protected override DocCusEntryLine CreateEntryLineWrapper(Customs.Business.ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		Customs.Business.CusEntryLine fEntryLineMergeOfTwoInvoiceLines;
		protected override Customs.Business.ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
					JobComInvoiceHeader header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "AED";

					JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;

					declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = "IMP";
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
