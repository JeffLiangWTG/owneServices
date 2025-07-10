using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	using System.Linq;
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.NZ.Business;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.DocumentEngine.Scheduler.Business;
	using Enterprise.MasterFiles.Business;

	public class ProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestOrderAndHint()
		{
			var processor = new ProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		public void TestProcessEmptyMessageProperly()
		{
			Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Processor.ExecuteBatch();
			Message.Reload();
			AssertEquals(EDIMessage.Status.Error, Message.EM_Status);
		}

		public void TestProcessingDOResponseMessageForFormalEntry()
		{
			SetupMergedFormalEntry();
			SetupResponseMessage(ResponseMessageDO);

			Factory.Save();

			Processor.ExecuteBatch();

			Message.Reload();
			AssertEquals(EDIMessage.Status.Received, Message.EM_Status);

			Declaration.Reload();
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, Declaration.JE_EntryStatus);
		}

		public void TestProcessingResponseMessageWhenAutoPrintingDocumentThrowsNoExpections()
		{
			StmPrintQueue entryQueue = GetNewPrintQueue("Entry Printer");
			Factory.Save();
			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);

			SetupMergedFormalEntry();
			SetupResponseMessage(ResponseMessageDO);

			Factory.Save();

			Processor.ExecuteBatch();

			Message.Reload();
			AssertEquals(EDIMessage.Status.Received, Message.EM_Status);

			Declaration.Reload();
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, Declaration.JE_EntryStatus);

			StmPrintJobCollection entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 1, entryJobs[0].SP_Copies.ToZInt());
		}

		void SetupResponseMessage(string messageText)
		{
			Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Message.EM_Status = EDIMessage.Status.Queued;
			Message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
		}

		void SetupMergedFormalEntry()
		{
			Declaration.JE_DeclarationReference = "B01001001";
			DecCreator.SetupTestConsignmentDetails();
			DecCreator.SetupTestForAir();
			DecCreator.SetupTestForImportFromAU();
			DecCreator.SetupInvoiceGroup(111m, "NZD", 0m, "NZD");
			DecCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000m, "AU", "AU", "Q");
			DecCreator.SetupImportInvoiceLine("6505900023J", "PLASTIC BITS", "", "", "", 1000m);
			DecCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			DecCreator.MergeDeclaration();
		}

		#region ResponseMessageDO
		const string ResponseMessageDO = @"UNH+293602+CUSRES:D:96B:UN+B01001001'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+D:134:143'
UNT+8+293602'
";
		#endregion

		#region Implementation
		#region Processor
		Processor Processor
		{
			get
			{
				if (fProcessor == null)
				{
					fProcessor = new Processor();
				}
				return fProcessor;
			}
		}
		Processor fProcessor;
		#endregion

		#region Message
		NZCMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<NZCMessage>();
				}
				return fMessage;
			}
		}
		NZCMessage fMessage;
		#endregion

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.DisableDefaultPackingInformation = true;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region DecCreator
		TestFormalEntryCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = new TestFormalEntryCreator(Declaration);
				}
				return fDecCreator;
			}
		}
		TestFormalEntryCreator fDecCreator;
		#endregion

		#region GetPrintJobs
		StmPrintJobCollection GetPrintJobs(StmPrintQueue printQueue)
		{
			ZQuery filter = new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK);
			StmPrintJobCollection entryPrintJobs = new StmPrintJobCollection(Factory, filter);
			entryPrintJobs.Load();
			return entryPrintJobs;
		}
		#endregion

		#region GetNewPrintQueue
		StmPrintQueue GetNewPrintQueue(ZString displayName)
		{
			StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = true;
			printQueue.SQ_DisplayName = displayName;
			printQueue.SQ_QueueName = @"\\PrintServer\" + displayName;
			printQueue.SQ_PrintLanguage = "ESP";
			printQueue.SQ_Scale = 100m;
			printQueue.SQ_RowScale = 100m;
			printQueue.SQ_ColumnScale = 100m;
			printQueue.SQ_ServerName = "PRINTSERVER";
			return printQueue;
		}
		#endregion

		class ProcessorForTest : Processor
		{
			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}

		#endregion
	}
}
