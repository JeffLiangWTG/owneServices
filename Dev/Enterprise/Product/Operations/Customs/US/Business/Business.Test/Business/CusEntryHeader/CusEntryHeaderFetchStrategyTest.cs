using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadChildEditableObjects()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry1 = AddEntry(dec);
			var entry2 = AddEntry(dec);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			dec = newFactory.Load<JobDeclaration>(dec.PK);
			dec.LoadChildEditableObjects();
			AssertEquals(2, dec.CustomsEntryHeaders[0].US7501DocPrintingData.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[1].US7501DocPrintingData.Count);
			var tableSelects = newFactory.TableSelects;
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for CusEntryHeader", 1, newFactory.GetTableHitCount(CusEntryHeaderSchema.Constants.TableName));
		}

		public void TestFetchForValidate()
		{
			var dec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry1 = dec.OriginalEntries.AddNew();
			entry1.CH_OrigEntryReference = "ABC1";
			var entry2 = dec.OriginalEntries.AddNew();
			entry2.CH_OrigEntryReference = "ABC2";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			dec = new ReconDeclaration(newFactory.Load<JobDeclaration>(dec.PK));
			AssertEquals(2, dec.OriginalEntries.Count); // cause CustomsEntryHeaders to be register as child of dec
			dec.RunPreSaveValidationWithFetchHints();
			AssertEquals("FetchForValidate Hints should be added for CusEntryHeader", 1, newFactory.GetTableHitCount(CusEntryHeaderSchema.Constants.TableName));
		}

		public void TestFetchForValidateForNormalDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			entry1.CH_BGMReference = "";

			var normalDec = Factory.New<JobDeclaration>();
			normalDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			normalDec.US_EnableENS = true;
			var entry2 = normalDec.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.CH_BGMReference = "";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			normalDec = newFactory.Load<JobDeclaration>(normalDec.PK);
			AssertEquals(1, normalDec.CustomsEntryHeaders.Count); // cause CustomsEntryHeaders to be register as child of dec
			((IBusiness)normalDec).RunPreSaveValidationFetch(true);//should execute fetch and load records into DataSet

			Assert("Should not load a recon original entry with an empty BGM reference", !((INeedRow)normalDec.CustomsEntryHeaders[0]).Row.Table.Rows.Contains(entry1.PK));
		}

		public void TestFetchForRecon()
		{
			JobDeclaration declaration1 = GetImportMergedDeclaration();

			JobDeclaration declaration2 = GetImportMergedDeclaration();

			JobDeclaration declaration3 = GetImportMergedDeclaration();

			Factory.Save();

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration1, declaration2, declaration3 });

			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			reconDeclaration = new ReconDeclaration(factory.Load<JobDeclaration>(reconDeclaration.JE_PK));
			var originalDeclarationAccessed = reconDeclaration.OriginalEntries[0].CH_OriginalDeclarationReference;
			originalDeclarationAccessed = reconDeclaration.OriginalEntries[1].CH_OriginalDeclarationReference;

			AssertEquals(2, factory.GetTableHitCount(CusEntryHeaderSchema.Constants.TableName));//Load ReconOriginalEntry, then OriginalEntry
			AssertEquals(2, factory.GetTableHitCount(JobDeclarationSchema.Constants.TableName)); // 1 for loading of ReconDeclaration and 1 for loading of all original declarations
		}

		CusEntryHeader AddEntry(JobDeclaration dec)
		{
			var entry = dec.CustomsEntryHeaders.AddNew();
			var printing1 = entry.US7501DocPrintingData.AddNew();
			printing1.US_LineNo = 1;
			var printing2 = entry.US7501DocPrintingData.AddNew();
			printing2.US_LineNo = 2;
			return entry;
		}

		JobDeclaration GetImportMergedDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}
	}
}
