using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLinesForEntryLineCollection))]
	sealed class InvoiceLinesForEntryLineCollectionTest : Customs.Business.Testing.InvoiceLinesForEntryLineCollectionTest
	{
		public void TestGetEntryLineFor()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var aaaEntry = declaration.CustomsEntryHeaders.AddNew();
			aaaEntry.CH_MessageType = "AAA";
			var aaaEntryLine = aaaEntry.MergedLines.AddNew();

			var bbbEntry = declaration.CustomsEntryHeaders.AddNew();
			bbbEntry.CH_MessageType = "BBB";
			var bbbEntryLine = bbbEntry.MergedLines.AddNew();

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = aaaEntryLine.PK;
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(bbbEntryLine);

			AssertEquals("GetEntryLineFor aaa", aaaEntryLine, invoiceLine.GetEntryLineFor("AAA", false));
			AssertEquals("GetEntryLineFor bbb", bbbEntryLine, invoiceLine.GetEntryLineFor("BBB", false));
		}

		public void TestRemoveCollectionRelationship()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry1 = declaration.CustomsEntryHeaders[0];
			var entry2 = declaration.CustomsEntryHeaders[1];

			AssertEquals("PreCondition:invoiceLine is contained", true, new List<Customs.Business.BaseJobComInvoiceLine>(entry1.InvoiceLines).Contains(invoiceLine));
			AssertEquals("PreCondition:invoiceLine is contained", true, new List<Customs.Business.BaseJobComInvoiceLine>(entry2.InvoiceLines).Contains(invoiceLine));
			Customs.Business.AdditionalInvoiceLineEntryLineLink link = invoiceLine.AdditionalEntryLineLinks.GetPivotFor(entry2.MergedLines[0]);

			declaration.US_EnableCRL = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("it should have deleted the pivot", true, link.IsDeleted);
			AssertEquals("JI_CL is retained", true, invoiceLine.JI_CL.IsValid);
		}

		public void TestInvoiceLines()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry1 = declaration.CustomsEntryHeaders[0];
			var entry2 = declaration.CustomsEntryHeaders[1];

			AssertEquals("PreCondition:invoiceLine is contained", true, new List<Customs.Business.BaseJobComInvoiceLine>(entry1.InvoiceLines).Contains(invoiceLine));
			AssertEquals("PreCondition:invoiceLine is contained", true, new List<Customs.Business.BaseJobComInvoiceLine>(entry2.InvoiceLines).Contains(invoiceLine));

			Assert(entry1.MergedLines[0].InvoiceLines.Contains(invoiceLine));
			Assert(entry1.MergedLines[1].InvoiceLines.Contains(invoiceLine2));
			Assert(entry2.MergedLines[0].InvoiceLines.Contains(invoiceLine));
			Assert(entry2.MergedLines[1].InvoiceLines.Contains(invoiceLine2));

			var ensEntryLineToDelete = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition:JI_CL are identical", invoiceLine.JI_CL, invoiceLine2.JI_CL);
			AssertEquals("PreCondition:AdditionalLink are identical", invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false), invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false));
			AssertEquals(true, ensEntryLineToDelete.IsDeleted);
		}

		public void TestMergeAndEntryLineInvoiceLinesForUS()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Two headers", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[1].MergedLines[0].InvoiceLines.Count);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Two headers", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[1].MergedLines[0].InvoiceLines.Count);

			Factory.Save();

			declaration.Reload();
			AssertEquals("Two headers", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[1].MergedLines[0].InvoiceLines.Count);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Two headers", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for each EntryLine", 1, declaration.CustomsEntryHeaders[1].MergedLines[0].InvoiceLines.Count);

			declaration.US_EnableENS = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLinesForEntryLineCollection((CusEntryLine)EntryLine);
	}
}
