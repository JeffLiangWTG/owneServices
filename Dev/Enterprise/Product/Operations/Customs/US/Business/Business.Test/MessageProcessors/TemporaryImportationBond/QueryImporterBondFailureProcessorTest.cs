using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class QueryImporterBondFailureProcessorTest : ABIProcessorTest<QueryImporterBondFailureProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = new ImporterNumberRequester(Factory).RequestImporterBond(declaration, "43-789432897");
			Factory.Save();

			outgoingmessage.EM_MessageNum = "YASYUSPRD_70018";

			var message = GetMessageToProcess();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.QueryImporterBond;
			message.EM_MessageText =
"B00                                                        B                    " +
"X0 BLOCK       1 REF ID:      286    KI YASYUSPRD_70018                         " +
"X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR                              " +
"X1RF999   BATCH REJECTED                                                        " +
"Y           00003";

			message.EM_MessageNum = "B";

			Factory.Save();

			Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
