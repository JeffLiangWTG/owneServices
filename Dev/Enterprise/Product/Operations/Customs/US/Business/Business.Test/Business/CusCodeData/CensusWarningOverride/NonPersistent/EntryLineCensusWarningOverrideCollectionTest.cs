using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryCensusWarningOverrideCollection))]
	sealed class EntryLineCensusWarningOverrideCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryCensusWarningOverrideCollection>
	{
		public void TestCopyToInvoiceLineWithSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98";
			var one = invoiceLine.CensusWarningOverrides.AddNew();
			one.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			one.CY_Data = CensusOverrideCodeList.Codes._01;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98";
			invoiceLine2.JI_Tariff = "9101.11.4010";
			AssertEquals("PreCondition", 3, invoiceLine2.SecondaryTariffLines.Count());
			var line3 = invoiceLine2.SecondaryTariffLines.ElementAt(1);
			var two = line3.CensusWarningOverrides.AddNew();
			two.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			two.CY_Data = CensusOverrideCodeList.Codes._01;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new EntryCensusWarningOverrideCollection(entry);
			AssertEquals(2, collection.Count);
			collection.CopyToEntryLines();
			AssertEquals(1, invoiceLine.CensusWarningOverrides.Count);
			AssertEquals(1, line3.CensusWarningOverrides.Count);
		}

		public void TestCopyToInvoiceLine_SupTariffIsNotEmptyButThereIsNoSupEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98";
			invoiceLine.JI_Tariff = "9101.11.4010";
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertNoExceptionThrown(() =>
			{
				var collection = new EntryCensusWarningOverrideCollection(entry);
				collection.CopyToEntryLines();
			});
		}

		public void TestBuildCollection()
		{
			EntryLine.US_CWOs = CensusWarningCodeList.Codes.GrossWeightAir;
			var collection = new EntryCensusWarningOverrideCollection(EntryLine.Header);
			collection.DefaultCustomsCWOs();
			AssertEquals(1, collection.Count);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightAir, collection[0].ConditionCode);
			EntryLine.US_CWOs = CensusWarningCodeList.Codes.GrossWeightAir + CensusWarningCodeList.Codes.GrossWeightVessel;
			collection.DefaultCustomsCWOs();
			AssertEquals(2, collection.Count);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightAir, collection[0].ConditionCode);
			Assert(!collection[0].ConditionCodeInfo.HasMessageErrors());
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, collection[1].ConditionCode);
			Assert(!collection[1].ConditionCodeInfo.HasMessageErrors());
		}

		public void TestBuildCollection_InvoiceLineWithSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98";
			var one = invoiceLine.CensusWarningOverrides.AddNew();
			one.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			one.CY_Data = CensusOverrideCodeList.Codes._01;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new EntryCensusWarningOverrideCollection(entry);
			AssertEquals(1, collection.Count);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, collection[0].ConditionCode);
		}

		public void TestCopyToEntryLines()
		{
			EntryLine.US_CWOs = CensusWarningCodeList.Codes.GrossWeightAir + CensusWarningCodeList.Codes.GrossWeightVessel;
			var coll = new EntryCensusWarningOverrideCollection(EntryLine.Header);
			coll.DefaultCustomsCWOs();
			coll[0].OverrideCode = CensusOverrideCodeList.Codes._03;
			coll.Remove(coll[1]);
			var newElement = coll.AddNew();
			newElement.ConditionCode = CensusWarningCodeList.Codes.ChargesDividedByValue;
			newElement.OverrideCode = CensusOverrideCodeList.Codes._02;
			newElement.EntryLinePK = EntryLine.PK;
			coll.CopyToEntryLines();
			AssertEquals("Copied", 2, EntryLine.InvoiceLines[0].CensusWarningOverrides.Count);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightAir, EntryLine.InvoiceLines[0].CensusWarningOverrides[0].CY_Code);
			AssertEquals(CensusOverrideCodeList.Codes._03, EntryLine.InvoiceLines[0].CensusWarningOverrides[0].CY_Data);
			AssertEquals(CensusWarningCodeList.Codes.ChargesDividedByValue, EntryLine.InvoiceLines[0].CensusWarningOverrides[1].CY_Code);
			AssertEquals(CensusOverrideCodeList.Codes._02, EntryLine.InvoiceLines[0].CensusWarningOverrides[1].CY_Data);
		}

		public void TestCoptyToEntryLinesWhenCWOForOneEntryLineDeleted()
		{
			var declaration = EntryLine.Declaration;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine2 = invoiceLine.CusEntryLine;
			EntryLine.US_CWOs = CensusWarningCodeList.Codes.GrossWeightAir + CensusWarningCodeList.Codes.GrossWeightVessel;
			var collection = new EntryCensusWarningOverrideCollection(EntryLine.Header);
			collection.DefaultCustomsCWOs();
			AssertEquals(2, collection.Count);
			collection.Remove(collection[1]);
			collection.CopyToEntryLines();
			AssertEquals(1, EntryLine.InvoiceLines[0].CensusWarningOverrides.Count);
			AssertEquals("This is removed from collection", 0, entryLine2.InvoiceLines[0].CensusWarningOverrides.Count);
		}

		public void TestDefaultCustomsCWOs()
		{
			var declaration = EntryLine.Declaration;
			EntryLine.RandomLine.CensusWarningOverrides.AddNew("AAA", "11");
			EntryLine.RandomLine.CensusWarningOverrides.AddNew("CCC", "22");
			EntryLine.US_CWOs = "BBB";
			var collection = new EntryCensusWarningOverrideCollection(EntryLine.Header);
			AssertEquals(2, collection.Count);
			AssertEquals("AAA", collection[0].ConditionCode);
			AssertEquals("11", collection[0].OverrideCode);
			AssertEquals("CCC", collection[1].ConditionCode);
			AssertEquals("22", collection[1].OverrideCode);
			collection.DefaultCustomsCWOs();
			AssertEquals(3, collection.Count);
			AssertEquals("AAA", collection[0].ConditionCode);
			AssertEquals("It retains what was entered by users", "11", collection[0].OverrideCode);
			AssertEquals("CCC", collection[1].ConditionCode);
			AssertEquals("It retains what was entered by users", "22", collection[1].OverrideCode);
			AssertEquals("BBB", collection[2].ConditionCode);
			AssertEquals("", collection[2].OverrideCode);
		}

		public void TestDefaultCustomsCWOsWithTwoEntryLinesWithIdenticalCWO()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = "NON";
			declaration.Invoices.AddNew();
			var line1 = declaration.InvoiceLines.AddNew();
			var line2 = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(line1.CusEntryLine, line2.CusEntryLine);
			line1.CusEntryLine.US_CWOs = "27D";
			line2.CusEntryLine.US_CWOs = "27D";
			var collection = new EntryCensusWarningOverrideCollection(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			collection.DefaultCustomsCWOs();
			AssertEquals(2, collection.Count);
		}

		protected override EntryCensusWarningOverrideCollection GetCollectionToTest() => new EntryCensusWarningOverrideCollection(EntryLine.Header);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryCensusWarningOverride(EntryLine.Header, new EntryCensusWarningOverrideCollection(EntryLine.Header));

		CusEntryLine entryLine;
		CusEntryLine EntryLine
		{
			get
			{
				if (entryLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.ImportEntryNumber = "ENT32432";
					declaration.Invoices.AddNew();
					declaration.InvoiceLines.AddNew();
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
					entryLine = declaration.InvoiceLines[0].CusEntryLine;
				}

				return entryLine;
			}
		}
	}
}
