using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Messages.IFTRIN;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Iftrin09bMessageProcessor : SGMessageProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1057:DoNotHardcodeMailserverName", Justification = "This is a code value")]
		public Iftrin09bMessageProcessor(LoggingInformation logger)
			: base(logger, "XCH", "Exchange Rates (IFTRIN D09B) message")
		{
		}

		#region Overridden

		protected override string DoProcessingReturningStatus(EDIMessage incomingMessage)
		{
			Logger.Log("Processing IFTRIN D09B CUSEXR Message");

			var result = EDIMessage.Status.Failed;
			try
			{
				this.incomingMessage = incomingMessage;
				IFTRINMessage iFTRIN = (IFTRINMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
				if (iFTRIN != null)
				{
					result = EDIMessage.Status.Received;
					RefDataRepoSender.SendIfNeeded(incomingMessage, IFTRINSource, RefDataRepoSender.ContentType.Edifact);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Iftrin09bMessageProcessor.DoProcessingReturningStatus", "IFTRIN D09B Message Processing Failure. Incoming message PK: " + this.incomingMessage.PK, e);
			}

			return result;
		}
		const string IFTRINSource = "IFTRIN";

		protected override string URN
		{
			get { throw new Exception("IFTRIN D09B has no URN"); }
		}

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;

		protected override void SetEntryStatus()
		{
			throw new Exception("IFTRIN D09B has no related CusEntryHeader");
		}

		#endregion
	}
}
