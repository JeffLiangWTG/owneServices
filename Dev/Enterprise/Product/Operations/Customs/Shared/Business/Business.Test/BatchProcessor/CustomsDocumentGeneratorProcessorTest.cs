using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class CustomsDocumentGeneratorProcessorTest : TestCaseWithFactory
	{
		public void TestGenerateCustomsInvoiceDocumentForCACustoms()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B00001001";
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = "B3C";
				var queue = CustomsStmProcessQueueLoader.New(entry, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "ABC");
				Factory.Save();

				var processor = new CustomsDocumentGeneratorProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var logs = new ZStringBuilder();
				var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				var newFactory = new BusinessObjectFactory();
				var queueLoaded = newFactory.Load<StmProcessQueue>(queue.PK);
				AssertNull(queueLoaded);
				AssertContains(@"System is unable to generate  document for Job B00001001 due to following error:
ABC is not supported for document generator in CA customs.", logs.ToStringWithDelimiterBetweenAppends("\r\n"));
				var loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				var storageMain = loadedDeclaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "DEC");
				AssertEquals(0, storageMain.Files.Count);

				queue = CustomsStmProcessQueueLoader.New(entry, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "CCI");
				Factory.Save();

				processor = new CustomsDocumentGeneratorProcessor(new LoggingInformation());
				processor.ExecuteBatch();
				logs.Clear();
				enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				newFactory = new BusinessObjectFactory();
				queueLoaded = newFactory.Load<StmProcessQueue>(queue.PK);
				AssertNull(queueLoaded);
				AssertContains(@"System is generating CA Customs Invoice document for Job B00001001.
System is unable to generate CA Customs Invoice document for Job B00001001, becasue there is no invoice header found for this job.", logs.ToStringWithDelimiterBetweenAppends("\r\n"));
				loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				storageMain = loadedDeclaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "DEC");
				AssertEquals(0, storageMain.Files.Count);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				queue = CustomsStmProcessQueueLoader.New(entry, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "CCI");
				Factory.Save();

				processor = new CustomsDocumentGeneratorProcessor(new LoggingInformation());
				processor.ExecuteBatch();
				logs.Clear();
				enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				newFactory = new BusinessObjectFactory();
				queueLoaded = newFactory.Load<StmProcessQueue>(queue.PK);
				AssertNull(queueLoaded);
				AssertContains(@"System is generating CA Customs Invoice document for Job B00001001.
CA Customs Invoice document has been generated successfully for Job B00001001.", logs.ToStringWithDelimiterBetweenAppends("\r\n"));
				loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				storageMain = loadedDeclaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "DEC");
				AssertEquals(1, storageMain.Files.Count);
			}
		}
	}
}
