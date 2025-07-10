using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Messages.CLASET;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Claset09bMessageProcessor : SGMessageProcessor
	{
		public const string MessageType = "REF";

		public Claset09bMessageProcessor(LoggingInformation logger)
			: base(logger, Claset09bMessageProcessor.MessageType, "Reference Data (CLASET D09B) message")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage incomingMessage)
		{
			Logger.Log("Processing CLASET D09B CUSCOM Message");

			var result = EDIMessage.Status.Failed;
			try
			{
				this.incomingMessage = incomingMessage;
				cLASET = (CLASETMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
				if (cLASET != null)
				{
					RefDataRepoSender.SendIfNeeded(incomingMessage, CLASETSource, RefDataRepoSender.ContentType.Edifact);
					result = EDIMessage.Status.Received;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Claset09bMessageProcessor.DoProcessingReturningStatus", "CLASET D09B Message Processing Failure. Incoming message PK: " + this.incomingMessage.PK, e);
			}

			return result;
		}
		CLASETMessage cLASET;
		const string CLASETSource = "CLASET";

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;

		protected override string URN
		{
			get { throw new Exception("CLASET D09B has no URN - should not be accessing this property."); }
		}

		protected override void SetEntryStatus()
		{
			throw new Exception("CLASET D09B is not related to any CusEntryHeader - should not be accessing this method.");
		}
	}
}
