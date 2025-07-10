using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestedType(typeof(CustomsDocumentGeneratorServiceTask))]
	class CustomsDocumentGeneratorServiceTaskTest : ServiceTaskTestCase<CustomsDocumentGeneratorServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestGenerateCustomsInvoiceDocumentForCACustoms()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B00001001";
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = "B3C";
				var invoice = declaration.Invoices.AddNew();
				invoice.JobComInvoiceLines.AddNew();
				CustomsStmProcessQueueLoader.New(entry, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "CCI");
				Factory.Save();

				var serviceTask = new CustomsDocumentGeneratorServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				AssertContains("serviceTask.ServiceLogger", $"CA Customs Invoice document has been generated successfully for Job B00001001.", serviceTask.ServiceLogger.ToString());
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var declarationLoaded = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				var storageMain = declarationLoaded.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declarationLoaded, "DEC");
				AssertEquals(1, storageMain.Files.Count);
				AssertEquals("CACustomsInvoice.pdf", storageMain.Files[0].FileName);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StmProcessQueueSchema.Constants.TableName,
						"Customs Document Generator",
						StmProcessQueueSchema.Constants.SW_ApplicationCode + "=" + CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode,
						StmProcessQueueSchema.Constants.SW_JobTypeCode + "=" + CustomsStmProcessQueueLoader.Constants.JobTypeCode),
				};
			}
		}
	}
}
