using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.TCODEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Tcodec09bMessageProcessor : SGMessageProcessor
	{
		public const string MessageType = "DCI";

		public Tcodec09bMessageProcessor(LoggingInformation logger)
			: base(logger, Tcodec09bMessageProcessor.MessageType, "Certificate Of Origin (TCODEC D09B) message")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			Logger.Log("Processing TCODEC D09B Message");
			var result = EDIMessage.Status.Failed;
			incomingMessage = message;

			tCODEC = ((SGEDIMessage)incomingMessage).TradeNetMessage as TCODECMessage;
			if (tCODEC != null)
			{
				Logger.Log("URN Number : " + URN);
				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;
					CompileCertificateOfOriginEmail();
					SendAcknowledgementReport(entry, responseEmail);
				}
			}
			else
			{
				responseEmail.Subject = "ERROR PROCESSING";
				responseEmail.Body = "FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText.Replace("'", "'" + System.Environment.NewLine);
				SendErrorReport(null, responseEmail);
			}

			return result;
		}
		TCODECMessage tCODEC;

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;

		protected override string URN
		{
			get { return tCODEC.BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		protected override void SetEntryStatus()
		{
			if (tCODEC.UNH[0].CommonAccessReference == CommonAccessReferenceCodeList.Codes.COODCI)
			{
				entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				entry.CertificateNumber = CertificateNumber;
			}
		}

		ZString CertificateNumber
		{
			get
			{
				foreach (SegmentGroup1 group1 in tCODEC.Group1)
				{
					foreach (RFFSegment rFF in group1.RFF)
					{
						if (rFF.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.OriginalCertificateNumber)
						{
							return rFF.Reference.ReferenceIdentifier;
						}
					}
				}
				return string.Empty;
			}
		}

		ZString CertificateApprovalDate
		{
			get
			{
				foreach (SegmentGroup1 group1 in tCODEC.Group1)
				{
					foreach (DTMSegment dtm in group1.DTM)
					{
						if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.AuthorizationDate)
						{
							return dtm.DateTimePeriod.DateOrTimeOrPeriodText;
						}
					}
				}
				return ZString.Empty;
			}
		}

		void CompileCertificateOfOriginEmail()
		{
			HtmlTableCreator creator = null;
			if (tCODEC.Group1.Count > 0)
			{
				creator = new HtmlTableCreator(new string[] { "Type", "Condition Code", "Description" });
				foreach (SegmentGroup1 group1 in tCODEC.Group1)
				{
					foreach (FTXSegment fTX in group1.FTX)
					{
						var textLiteral = string.Join(" ", fTX.TextLiteral.FreeText1.Trim(), fTX.TextLiteral.FreeText2.Trim(), fTX.TextLiteral.FreeText3.Trim(), fTX.TextLiteral.FreeText4.Trim(), fTX.TextLiteral.FreeText5.Trim()).Trim();
						creator.WriteRow(fTX.TextSubjectCodeQualifier, fTX.TextReference.FreeTextDescriptionCode, textLiteral);
					}
				}
				foreach (SegmentGroup4 group4 in tCODEC.Group4)
				{
					foreach (FTXSegment fTX in group4.FTX)
					{
						var textLiteral = string.Join(" ", fTX.TextLiteral.FreeText1.Trim(), fTX.TextLiteral.FreeText2.Trim(), fTX.TextLiteral.FreeText3.Trim(), fTX.TextLiteral.FreeText4.Trim(), fTX.TextLiteral.FreeText5.Trim()).Trim();
						creator.WriteRow(fTX.TextSubjectCodeQualifier, fTX.TextReference.FreeTextDescriptionCode, textLiteral);
					}
				}
			}

			string emailTemplateHtml;
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.Messaging.CustomsMessaging.MessageProcessors.HtmlTemplates.Tcodec.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}
			emailTemplateHtml = ZString.Format(emailTemplateHtml, entry.Declaration.JE_DeclarationReference, CertificateApprovalDate, CertificateNumber, entry.CH_Status, URN);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator?.ToHtml() ?? string.Empty);

			var emailSender = new HtmlNotificationEmailSender();
			responseEmail = emailSender.CreateEmail("Certificate of Origin for " + entry.Declaration.JE_DeclarationReference, emailTemplateHtml);
		}
	}
}
