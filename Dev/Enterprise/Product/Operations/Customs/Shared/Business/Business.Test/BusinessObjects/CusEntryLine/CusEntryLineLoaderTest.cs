using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryLine.Loader))]
	sealed class CusEntryLineLoaderTest : LoaderTestCase
	{
		public void TestFindByDeclarationAndLineNumbers()
		{
			BaseJobDeclaration declaration1 = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice1 = declaration1.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "B12345678";
			CusEntryHeader entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = "IMP";
			CusEntryLine entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			entryLine1.CL_LineNumber = 2;
			invoiceLine1.JI_CL = entryLine1.PK;
			entryHeader1.EntryNumber = "AAABBBCCC";

			Factory.Save();
			CusEntryLine.Loader loader = new CusEntryLine.Loader(Factory);
			AssertNull("GetEntryLineFromDecAndLineNumbers", loader.FindByDeclarationAndLineNumbers("AAABBBCCC", 1));
			AssertNull("GetEntryLineFromDecAndLineNumbers", loader.FindByDeclarationAndLineNumbers("AAABBBCCX", 2));
			AssertEquals("GetEntryLineFromDecAndLineNumbers", entryLine1, loader.FindByDeclarationAndLineNumbers("AAABBBCCC", 2));

			BaseJobDeclaration declaration2 = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration2.JE_DeclarationReference = "B12345679";
			CusEntryHeader entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = "EXP";
			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();
			entryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			entryHeader2.EntryNumber = "AAABBBCCC";

			Factory.Save();
			AssertEquals("GetEntryLineFromDecAndLineNumbers", entryLine1, loader.FindByDeclarationAndLineNumbers("AAABBBCCC", "IMP", 2));
			AssertEquals("GetEntryLineFromDecAndLineNumbers", entryLine2, loader.FindByDeclarationAndLineNumbers("AAABBBCCC", "EXP", 2));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusEntryLine.Loader(Factory);
		}
	}
}
