using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ExportUnionItemsProviderTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "21340300IM123456";
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

#if NETFRAMEWORK
			var expectedMessage = "Value cannot be null.\r\nParameter name: cusEntryLine";
#else
			var expectedMessage = "Value cannot be null. (Parameter 'cusEntryLine')";
#endif
			AssertExceptionThrown<ArgumentNullException>("Null ExportUnionItemsProvider", expectedMessage,
			() => new ExportUnionItemsProvider(null, ZDateTime.Empty));
			AssertNoExceptionThrown("All ok", () => new ExportUnionItemsProvider(cusEntryLine, ZDateTime.Now));
		});

		public void TestExportUnionDeclarationMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				var entryLines = declaration.EntryLines.ToArray()[0];
				var exportUnionItems = entryLines.ExportUnionItems;

				CombineAssertions("Export Union Declaration Members | Case 1", () =>
				{
					AssertEquals("ContainerTypeCode", "A0001", exportUnionItems.ContainerTypeCode);
					AssertEquals("ContainerCount", 2, exportUnionItems.ContainerCount);
					AssertEquals("RegimeCode", "4000", exportUnionItems.RegimeCode);
					AssertEquals("ProductionYear", "2023", exportUnionItems.ProductionYear);
					AssertEquals("ThreadType", "A0002", exportUnionItems.ThreadType);
					AssertEquals("ItemAbroadExpenses", 120m, exportUnionItems.ItemAbroadExpenses);
					AssertEquals("ItemAbroadExpensesCurrencyCode", "USD", exportUnionItems.ItemAbroadExpensesCurrencyCode);
					AssertEquals("InternalExpensesAmount", 16m, exportUnionItems.InternalExpensesAmount);
					AssertEquals("Ecological", "H", exportUnionItems.Ecological);
					AssertEquals("SubjectToQuota", "H", exportUnionItems.SubjectToQuota);
					AssertEquals("SubjectToPricelessExport", "H", exportUnionItems.SubjectToPricelessExport);
					AssertEquals("TransporterTaxid", ZString.Empty, exportUnionItems.TransporterTaxid);
					AssertEquals("Companies", 1, exportUnionItems.Companies.Count);
				});

				var orgCarrier = Factory.New<OrgHeader>();
				orgCarrier.OH_Code = "xCarrier";
				orgCarrier.OH_FullName = "xCarrier Full Name";
				orgCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890034441");
				var addressCarrier = orgCarrier.MainAddress;
				addressCarrier.OA_OH = orgCarrier.PK;
				addressCarrier.CompanyName = "xCarrier Company Name";
				headerJobDeclaration.JE_OH_ShippingLine = orgCarrier.PK;

				var orgForwarder = Factory.New<OrgHeader>();
				orgForwarder.OH_Code = "xForwarder";
				orgForwarder.OH_FullName = "xForwarder Full Name";
				orgForwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890034442");
				var addressForwarder = orgForwarder.MainAddress;
				addressForwarder.OA_OH = orgForwarder.PK;
				addressForwarder.CompanyName = "xForwarder Company Name";
				headerJobDeclaration.JE_OH_Forwarder = orgForwarder.PK;

				var entryLine = headerJobDeclaration.CusEntryHeader.AllEntryLines[0];
				var randomInvoiceLine = (JobComInvoiceLine)entryLine.RandomLine;
				randomInvoiceLine.ZG_ExportUnionProductionYear = 0;
				randomInvoiceLine.ZG_ExportUnionEcological = true;
				randomInvoiceLine.ZG_PriceType = PriceTypeList.Codes._02;

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				entryLines = declaration.EntryLines.ToArray()[0];
				exportUnionItems = entryLines.ExportUnionItems;

				CombineAssertions("Export Union Declaration Members | Case 2", () =>
				{
					AssertEquals("ProductionYear", ZString.Empty, exportUnionItems.ProductionYear);
					AssertEquals("Ecological", "E", exportUnionItems.Ecological);
					AssertEquals("SubjectToPricelessExport", "E", exportUnionItems.SubjectToPricelessExport);
					AssertEquals("Carrier TransporterTaxid", "8890034441", exportUnionItems.TransporterTaxid);
				});

				orgCarrier.CustomsCodes.DeleteAll();
				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				entryLines = declaration.EntryLines.ToArray()[0];
				exportUnionItems = entryLines.ExportUnionItems;

				CombineAssertions("Export Union Declaration Members | Case 3", () =>
				{
					AssertEquals("Forwarder TransporterTaxid", "8890034442", exportUnionItems.TransporterTaxid);
				});
			}
		}
	}
}
