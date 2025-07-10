using System.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Messages.APERAK;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Aperak09bMessageProcessor : SGMessageProcessor
	{
		public const string MessageType = "ERR";
		const string ControllingAgencyQuery = "AQR";
		const string SingaporeCustomsQuery = "CQR";

		public Aperak09bMessageProcessor(LoggingInformation logger)
			: base(logger, Aperak09bMessageProcessor.MessageType, "Response Status (APERAK D09B) message")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage incomingMessage)
		{
			Logger.Log("Processing APERAK D09B Message");
			var result = EDIMessage.Status.Failed;
			this.incomingMessage = incomingMessage;
			aPERAK = (APERAKMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
			if (aPERAK != null)
			{
				Logger.Log("URN Number : " + URN);

				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;

					bool sendErrorEmail = true;
					if (IsDuplicateDeclarationError && !syntaxError)
					{
						sendErrorEmail = false;
					}

					if (sendErrorEmail)
					{
						CompileErrorEmail();
						if (queryResponse)
						{
							SendImpedimentReport(entry, responseEmail);
						}
						else
						{
							SendErrorReport(entry, responseEmail);
						}
					}
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
		APERAKMessage aPERAK;
		bool syntaxError;
		bool queryResponse;

		bool IsDuplicateDeclarationError
		{
			get { return aPERAK.ToString(new UNOASGCharacterSet()).Contains("ERC+E17001"); }
		}

		protected override void SetEntryStatus()
		{
			syntaxError = false;
			queryResponse = false;

			if (aPERAK.UNH[0].CommonAccessReference == CommonAccessReferenceCodeList.Codes.ERRORM)
			{
				if (entry.IsOriginalPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
					syntaxError = true;
				}
				else if (entry.IsAmendmentPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
					syntaxError = true;
				}
				else if (entry.IsRefundPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
					syntaxError = true;
				}
				else if (entry.IsCancellationPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
					syntaxError = true;
				}
			}
			else  // Aperak is Status APERAK
			{
				if (aPERAK.BGM[0].DocumentMessageName.DocumentName == Aperak09bMessageProcessor.ControllingAgencyQuery ||
					aPERAK.BGM[0].DocumentMessageName.DocumentName == Aperak09bMessageProcessor.SingaporeCustomsQuery)
				{
					queryResponse = true;

					if (entry.IsOriginalPending)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationQuery;
					}
					else if (entry.IsAmendmentPending)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentQuery;
					}
					else if (entry.IsRefundPending)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundQuery;
					}
					else if (entry.IsCancellationPending)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationQuery;
					}
				}
				else
				{
					if (entry.IsOriginalPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.DeclarationQuery)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
					}
					else if (entry.IsAmendmentPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.AmendmentQuery)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
					}
					else if (entry.IsRefundPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.RefundQuery)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms;
					}
					else if (entry.IsCancellationPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.CancellationQuery)
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
					}
				}
			}
		}

		void CompileErrorEmail()
		{
			HtmlTableCreator creator = null;
			if (aPERAK.Group4.Count > 0)
			{
				if (syntaxError)
				{
					creator = new HtmlTableCreator(new string[] { "Error Code", "Description", "Segment Group", "Segment Tag", "Data Element" });
					foreach (SegmentGroup4 group4 in aPERAK.Group4)
					{
						creator.WriteRow(group4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode,
							group4.FTX[0].TextLiteral.FreeText1,
							group4.FTX[0].TextLiteral.FreeText2,
							group4.FTX[0].TextLiteral.FreeText3,
							group4.FTX[0].TextLiteral.FreeText4);
					}
				}
				else if (queryResponse)
				{
					creator = new HtmlTableCreator(new string[] { "Query Detail", "Query Description" });
					foreach (SegmentGroup4 group4 in aPERAK.Group4)
					{
						creator.WriteRow(group4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode,
							group4.FTX[0].TextLiteral.FreeText1 + " " + group4.FTX[0].TextLiteral.FreeText2);
					}
				}
				else
				{
					creator = new HtmlTableCreator(new string[] { "Error Code", "Line Item Reference Number", "Description" });
					foreach (SegmentGroup4 group4 in aPERAK.Group4)
					{
						if (group4.Group5.Count > 0)
						{
							foreach (SegmentGroup5 group5 in group4.Group5)
							{
								creator.WriteRow(group4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode,
								group5.RFF[0].Reference.ReferenceIdentifier,
								group4.FTX[0].TextLiteral.FreeText1 + group4.FTX[0].TextLiteral.FreeText2);
							}
						}
						else
						{
							creator.WriteRow(group4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode,
							"0",
							group4.FTX[0].TextLiteral.FreeText1 + group4.FTX[0].TextLiteral.FreeText2);
						}
					}
				}
			}

			string emailTemplateHtml;
			string templateType = queryResponse ? "AperakQuery.htm" : "Aperak.htm";
			string templateResource = string.Format("Enterprise.Customs.SG.V4.Business.Messaging.CustomsMessaging.MessageProcessors.HtmlTemplates." + templateType);

			using (Stream stream = GetType().Assembly.GetManifestResourceStream(templateResource))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{0}", entry.Declaration.JE_DeclarationReference);
			emailTemplateHtml = emailTemplateHtml.Replace("{1}", entry.CH_Status);
			emailTemplateHtml = emailTemplateHtml.Replace("{2}", URN);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator != null ? creator.ToHtml() : string.Empty);

			var emailSender = new HtmlNotificationEmailSender();
			var subjectType = queryResponse ? "Impediment Query from Customs for " : "Error for ";
			var subject = string.Format(subjectType + entry.Declaration.JE_DeclarationReference);

			responseEmail = emailSender.CreateEmail(subject, emailTemplateHtml);
		}

		protected override string URN
		{
			get { return aPERAK.BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;
	}
}
