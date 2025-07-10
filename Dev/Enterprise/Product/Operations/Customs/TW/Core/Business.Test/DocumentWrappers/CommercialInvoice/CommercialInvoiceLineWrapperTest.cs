using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class CommercialInvoiceLineWrapperTest : DocumentWrapperTest
	{
		JobComInvoiceLine GenerateJobComInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "TWD";
			invoice.JZ_InvoiceCurrExRate = 1m;
			invoice.JZ_RelatedIndicator = "N";
			invoice.JZ_MarksAndNumbers = "TEST MARKS";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 88m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 50m;
			return invoiceLine;
		}

		CommercialInvoiceLineWrapper GetCommercialInvoiceLineWrapper(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
		{
			return CommercialInvoiceLineWrapper.New(invoiceLine, factory);
		}

		public void TestLineNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var wrapper = GetCommercialInvoiceLineWrapper(invoice.JobComInvoiceLines.AddNew(), Factory);
			AssertEquals((ZShort)1, wrapper.LineNo);

			wrapper = GetCommercialInvoiceLineWrapper(invoice.JobComInvoiceLines.AddNew(), Factory);
			AssertEquals((ZShort)2, wrapper.LineNo);
		}

		public void TestUnitPrice()
		{
			var invoiceLine = GenerateJobComInvoiceLine();
			invoiceLine.AddInfoChild.TWL_DocumentaryUnitPrice = 2689.35m;
			var wrapper = GetCommercialInvoiceLineWrapper(invoiceLine, Factory);
			AssertEquals(2689.35m, wrapper.UnitPrice);
		}

		public void TestShowFOC()
		{
			var invoiceLine = GenerateJobComInvoiceLine();
			var wrapper = GetCommercialInvoiceLineWrapper(invoiceLine, Factory);

			Assert(!wrapper.ShowFOC);

			invoiceLine.JI_Procedure = "04";
			Assert(wrapper.ShowFOC);

			invoiceLine.JI_Procedure = "94";
			Assert(wrapper.ShowFOC);

			invoiceLine.JI_Procedure = "14";
			Assert(!wrapper.ShowFOC);
		}

		public void TestTwGroup()
		{
			var invoiceLine = GenerateJobComInvoiceLine();
			var wrapper = GetCommercialInvoiceLineWrapper(invoiceLine, Factory);

			AssertEquals("", wrapper.TwGroup);

			invoiceLine.JI_Group = "test group";
			AssertEquals("test group", wrapper.TwGroup);

			invoiceLine.JI_Group = @"

";
			AssertEquals("", wrapper.TwGroup);
		}

		public void TestGoodsDescription()
		{
			var invoiceLine = GenerateJobComInvoiceLine();
			invoiceLine.JI_DeclarationGoodsDescription = @"1234567890123456789012345678901234567890
1
2
3
4";
			var wrapper = GetCommercialInvoiceLineWrapper(invoiceLine, Factory);

			AssertEquals(@"1234567890123456789012345678901234567890
1
2
3
4", wrapper.GoodsDescription);
		}
	}
}
