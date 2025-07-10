using System;
using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.DEBADV;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Debadv09bMessageProcessor : SGMessageProcessor
	{
		public const string MessageType = "FEE";

		public Debadv09bMessageProcessor(LoggingInformation logger)
			: base(logger, Debadv09bMessageProcessor.MessageType, "Fee Advice (DEBADV D09B) message")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage incomingMessage)
		{
			Logger.Log("Processing DEBADV D09B Message");
			var result = EDIMessage.Status.Failed;
			this.incomingMessage = incomingMessage;
			dEBADV = (DEBADVMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
			if (dEBADV != null)
			{
				Logger.Log("URN Number : " + URN);
				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;
					CompileFeeAdviceEmail();
					SendAcknowledgementReport(entry, responseEmail);
				}
			}

			return result;
		}
		DEBADVMessage dEBADV;

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;

		protected override string URN
		{
			get { return dEBADV.BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		protected override void SetEntryStatus()
		{
			//status of entry is not affected by this message
		}

		#region email generation

		void CompileFeeAdviceEmail()
		{
			var permitNumber = ZString.Empty;
			var licenceNumber = ZString.Empty;
			var feeAmount = ZDecimal.Zero;
			var settlementDate = ZString.Empty;
			var remittanceInfo = ZString.Empty;

			if (dEBADV.Group1.Count > 0)
			{
				foreach (SegmentGroup1 sg1 in dEBADV.Group1)
				{
					foreach (RFFSegment rff in sg1.RFF)
					{
						if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms)
						{
							permitNumber = rff.Reference.ReferenceIdentifier;
						}
						else if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.DocumentIdentifier)
						{
							licenceNumber = rff.Reference.ReferenceIdentifier;
						}
					}
				}
			}

			if (dEBADV.Group3.Count > 0)
			{
				foreach (SegmentGroup3 sg3 in dEBADV.Group3)
				{
					foreach (MOASegment moa in sg3.MOA)
					{
						var value = moa.MonetaryAmount.MonetaryAmount;
						if (!string.IsNullOrEmpty(value))
						{
							feeAmount = Convert.ToDecimal(value);
						}
					}

					foreach (DTMSegment dtm in sg3.DTM)
					{
						settlementDate = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
						if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.SettlementDate)
						{
							settlementDate = FormattedDateTime(dtm.DateTimePeriod.DateOrTimeOrPeriodText);
						}
					}
				}
			}

			if (dEBADV.FTX.Count > 0)
			{
				var ftx = dEBADV.FTX[0];
				remittanceInfo = ftx.TextLiteral.FreeText1 + ftx.TextLiteral.FreeText2;
			}

			string emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.Messaging.CustomsMessaging.MessageProcessors.HtmlTemplates.Debadv.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{0}", entry.Declaration.JE_DeclarationReference);
			emailTemplateHtml = emailTemplateHtml.Replace("{1}", URN);
			emailTemplateHtml = emailTemplateHtml.Replace("{2}", permitNumber);
			emailTemplateHtml = emailTemplateHtml.Replace("{3}", licenceNumber);
			emailTemplateHtml = emailTemplateHtml.Replace("{4}", feeAmount.ToString(2));
			emailTemplateHtml = emailTemplateHtml.Replace("{5}", settlementDate);
			emailTemplateHtml = emailTemplateHtml.Replace("{6}", remittanceInfo);

			var emailSender = new HtmlNotificationEmailSender();
			var subject = string.Format("Licence fee payment advice for " + entry.Declaration.JE_DeclarationReference);
			responseEmail = emailSender.CreateEmail(subject, emailTemplateHtml);
		}

		ZString FormattedDateTime(ZString messageDate)
		{
			return messageDate.SubstringSafe(6, 2) + "-" + GetMonthAbbrev(messageDate.SubstringSafe(4, 2)) + "-" + messageDate.SubstringSafe(0, 4);
		}

		ZString GetMonthAbbrev(string month)
		{
			switch (month)
			{
				case "01":
					return "JAN";
				case "02":
					return "FEB";
				case "03":
					return "MAR";
				case "04":
					return "APR";
				case "05":
					return "MAY";
				case "06":
					return "JUN";
				case "07":
					return "JUL";
				case "08":
					return "AUG";
				case "09":
					return "SEP";
				case "10":
					return "OCT";
				case "11":
					return "NOV";
				case "12":
					return "DEC";
				default:
					return "";
			}
		}
		#endregion
	}
}
