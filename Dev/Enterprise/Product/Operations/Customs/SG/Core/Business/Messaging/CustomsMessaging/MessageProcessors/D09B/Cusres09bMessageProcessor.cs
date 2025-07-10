using System.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Messages.CUSRES;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Cusres09bMessageProcessor : SGMessageProcessor
	{
		public const string MessageType = CUSDECEDIMessage.Cancellation;

		public Cusres09bMessageProcessor(LoggingInformation logger)
			: base(logger, Cusres09bMessageProcessor.MessageType, "Cancellation Approval (CUSRES D09B) message")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage incomingMessage)
		{
			Logger.Log("Processing CUSRES D09B Message");
			var result = EDIMessage.Status.Failed;
			this.incomingMessage = incomingMessage;
			cUSRES = (CUSRESMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
			if (cUSRES != null)
			{
				Logger.Log("URN Number : " + URN);
				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;
					CompileCancellationResponseEmail();
					SendAcknowledgementReport(entry, responseEmail);
				}
			}
			else
			{
				responseEmail.Subject = "ERROR PROCESSING";
				responseEmail.Body = "FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + incomingMessage.EM_FormattedMessageText.Replace("'", "'" + System.Environment.NewLine);

				SendErrorReport(null, responseEmail);
			}

			return result;
		}
		CUSRESMessage cUSRES;

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;

		protected override string URN
		{
			get { return cUSRES.BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		protected override void SetEntryStatus()
		{
			if (cUSRES.UNH[0].CommonAccessReference == CommonAccessReferenceCodeList.Codes.STATUS)
			{
				entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			}
		}

		void CompileCancellationResponseEmail()
		{
			HtmlTableCreator creator = null;
			if (cUSRES.FTX.Count > 0)
			{
				creator = new HtmlTableCreator(new string[] { "Reason" });
				foreach (FTXSegment fTX in cUSRES.FTX)
				{
					creator.WriteRow(fTX.TextLiteral.FreeText1 + " " +
						fTX.TextLiteral.FreeText2);
				}
			}

			string emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.Messaging.CustomsMessaging.MessageProcessors.HtmlTemplates.Cusres.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}
			emailTemplateHtml = emailTemplateHtml.Replace("{0}", entry.Declaration.JE_DeclarationReference);
			emailTemplateHtml = emailTemplateHtml.Replace("{1}", entry.CH_Status);
			emailTemplateHtml = emailTemplateHtml.Replace("{2}", URN);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator != null ? creator.ToHtml() : string.Empty);

			var emailSender = new HtmlNotificationEmailSender();
			var subject = string.Format("Cancellation for " + entry.Declaration.JE_DeclarationReference);
			responseEmail = emailSender.CreateEmail(subject, emailTemplateHtml);
		}
	}
}
