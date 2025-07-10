using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	[TestedType(typeof(AutoSendCustomsMessagingBatchProcessor))]
	sealed class AutoSendCustomsMessagingBatchProcessorTest : TestCaseWithFactory
	{
		public void TestSendCustomsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_DeclarationReference = "B00001001";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1234567890";

				CustomsStmProcessQueueLoader.New(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SEM");
				CustomsStmProcessQueueLoader.New(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SRM");

				var srmProcessQueue = Factory.New<StmProcessQueue>();
				srmProcessQueue.SW_ApplicationCode = "ASC";
				srmProcessQueue.SW_JobTypeCode = "CUS";
				srmProcessQueue.SW_ActionCode = "SRM";
				srmProcessQueue.SW_ReferenceID = declaration.PK;
				srmProcessQueue.SW_ReferenceTableCode = "JE";
				srmProcessQueue.SW_PostedTimeUtc = ZDateTime.Today;

				Factory.Save();

				var processor = new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var loadedJob = new BusinessObjectFactory().Load<BaseJobDeclaration>(declaration.PK);
				AssertEquals(1, loadedJob.ActiveEntryHeaders.Count);
				AssertEquals(1, loadedJob.ActiveEntryHeaders[0].Messages.Count);

				var foundFirstMessage = false;
				var foundSecondMessage = false;
				foreach (var log in processor.Logger.UserLogStrings)
				{
					if (!foundFirstMessage && log.Contains("SED message has been sent to customs for Job:B00001001"))
					{
						foundFirstMessage = true;
					}
					if (!foundSecondMessage && log.Contains("Unable to send message for Declaration B00001001. The Send Release Message trigger is only supported for import declaration and message mode is 'ACE' and cargo release is enabled."))
					{
						foundSecondMessage = true;
					}
					if (foundFirstMessage && foundSecondMessage)
					{
						break;
					}
				}
				Assert(@"Log should contain [SED message has been sent to customs for Job:B00001001]", foundFirstMessage);
				Assert(@"Log should contain [Unable to send message for Declaration B00001001. The Send Release Message trigger is only supported for import declaration and message mode is 'ACE' and cargo release is enabled.]", foundSecondMessage);
			}
		}

		public void TestSendExitReportTransferMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var exitHeader = Factory.New<Integration.Customs.DEExitControl.ICusExitHeader>();
				var exitConsignment = Factory.New<Integration.Customs.DEExitControl.ICusExitConsignment>();
				exitConsignment.CXC_CXH_Header = exitHeader.PK;
				exitConsignment.CXC_MovementReference = "MRN";
				var exitReport = Factory.New<Integration.Customs.DEExitControl.ICusExitReport>();
				exitReport.CER_CXH_Header = exitHeader.PK;
				exitReport.CER_CXC_Consignment = exitConsignment.PK;
				var stmProcessQueue = Factory.New<StmProcessQueue>();
				stmProcessQueue.SW_ApplicationCode = "ASC";
				stmProcessQueue.SW_JobTypeCode = "CUS";
				stmProcessQueue.SW_ActionCode = "TRA";
				stmProcessQueue.SW_ReferenceID = exitReport.PK;
				stmProcessQueue.SW_ReferenceTableCode = CusExitReportSchema.Constants.Prefix;
				stmProcessQueue.SW_PostedTimeUtc = ZDateTime.Today;

				Factory.Save();
				var processor = new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				AssertCollectionContains("Logger.UserLogStrings", processor.Logger.UserLogStrings.Cast<string>(), new Predicate<string>((x) => x.Contains("'EXTINF' message has been sent to customs for Job:")));
			}
		}

		public void TestEmmaMessageGeneration_ForNorway()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				var stmProcessQueue = Factory.New<StmProcessQueue>();
				stmProcessQueue.SW_ApplicationCode = "ASC";
				stmProcessQueue.SW_JobTypeCode = "CUS";
				stmProcessQueue.SW_ActionCode = WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator;
				stmProcessQueue.SW_ReferenceID = entryHeader.PK;
				stmProcessQueue.SW_ReferenceTableCode = CusEntryHeaderSchema.Constants.Prefix;
				stmProcessQueue.SW_PostedTimeUtc = ZDateTime.Today;

				Factory.Save();

				var processor = new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var messages = entryHeader.Messages;
				AssertEquals("Messages Count", 1, messages.Count);

				var message = messages[0];

				AssertContainsExactElementsInAnyOrder("Logger.UserLogStrings",
					expected: [
						"Processing: CusEntryHeader",
						$"'EMMA' Message generation started for EntryHeader with PK [{entryHeader.PK}].",
						$"'EMMA' Message with PK [{message.PK}] generated for EntryHeader with PK [{entryHeader.PK}]."
					],
					actual: processor.Logger.UserLogStrings.Cast<string>().WhereNotNull().Select(t => t.TrimStart('\t')));
			}
		}
	}
}
