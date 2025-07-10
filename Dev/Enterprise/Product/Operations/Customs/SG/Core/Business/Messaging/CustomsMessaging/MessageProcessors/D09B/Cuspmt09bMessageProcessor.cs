using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSPMT;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class Cuspmt09bMessageProcessor : SGMessageProcessor
	{
		public const string MessageType = "PMT";

		public Cuspmt09bMessageProcessor(LoggingInformation logger)
			: base(logger, Cuspmt09bMessageProcessor.MessageType, "Permit (CUSPMT D09B) message")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage incomingMessage)
		{
			Logger.Log("Processing CUSPMT D09B Message");

			var result = EDIMessage.Status.Failed;
			this.incomingMessage = incomingMessage;
			cUSPMT = (CUSPMTMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
			if (cUSPMT != null)
			{
				Logger.Log("URN Number : " + URN);
				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;
					CompilePermitResponseEmail();
					SendAcknowledgementReport(entry, responseEmail);
					if (TotalPayable > 0)
					{
						AssignValuesToCusEntryPayInfo();
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
		CUSPMTMessage cUSPMT;

		void AssignValuesToCusEntryPayInfo()
		{
			CusEntryPayInfo payInfo = entry.EntryPayInfos.GetItemByMessageNum(MessageReferenceNumber)
				?? entry.EntryPayInfos.AddNew();

			payInfo.C9_IncomingPayResponseNo = MessageReferenceNumber;
			payInfo.C9_PaymentAmount = TotalPayable;
			payInfo.C9_TransactionType = DocumentName;
			payInfo.C9_PaymentDate = AuthorisationDate;
			payInfo.C9_PaymentReference = PermitNumber;
			payInfo.C9_PaymentParty = PaymentParty;
		}

		protected override EDIMessage IncomingMessage
		{
			get { return incomingMessage; }
		}
		EDIMessage incomingMessage;

		protected override string URN
		{
			get { return cUSPMT.BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		protected ZString DocumentNameCode
		{
			get { return cUSPMT.BGM[0].DocumentMessageName.DocumentNameCode.ToString(); }
		}

		protected ZString DocumentName
		{
			get { return cUSPMT.BGM[0].DocumentMessageName.DocumentName; }
		}

		protected override void SetEntryStatus()
		{
			switch (cUSPMT.UNH[0].CommonAccessReference)
			{
				case CommonAccessReferenceCodeList.Codes.INPPMT:
				case CommonAccessReferenceCodeList.Codes.IPTPMT:
				case CommonAccessReferenceCodeList.Codes.OUTPMT:
				case CommonAccessReferenceCodeList.Codes.TNPPMT:
					entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
					attachPermitPrint = true;
					break;
				case CommonAccessReferenceCodeList.Codes.INPUPT:
				case CommonAccessReferenceCodeList.Codes.IPTUPT:
				case CommonAccessReferenceCodeList.Codes.OUTUPT:
				case CommonAccessReferenceCodeList.Codes.TNPUPT:
					entry.CH_Status = (entry.CH_Status == Core.SGConstants.DeclarationStatus.RefundSent) || (entry.CH_Status == Core.SGConstants.DeclarationStatus.RefundPermitReceived) ? Core.SGConstants.DeclarationStatus.RefundPermitReceived : Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
					attachPermitPrint = DocumentNameCode == DocumentNameCodeList.DocumentResponseCustoms && entry.CH_Status != Core.SGConstants.DeclarationStatus.RefundPermitReceived;
					break;
			}

			entry.EntryNumber = PermitNumber;
			entry.CertificateNumber = CertificateNumber;
			entry.Declaration.JE_EntryAuthorisationDate = AuthorisationDate;
		}

		ZString MessageReferenceNumber
		{
			get { return cUSPMT.UNH[0].MessageReferenceNumber.Replace("WTG", ""); }
		}

		ZString PermitNumber
		{
			get
			{
				foreach (SegmentGroup1 group1 in cUSPMT.Group1)
				{
					foreach (RFFSegment rFF in group1.RFF)
					{
						if (rFF.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms)
						{
							return rFF.Reference.ReferenceIdentifier;
						}
					}
				}

				return string.Empty;
			}
		}

		ZString CertificateNumber
		{
			get
			{
				foreach (SegmentGroup1 group1 in cUSPMT.Group1)
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

		ZDateTime AuthorisationDate
		{
			get
			{
				var result = ZDateTime.Now;

				foreach (SegmentGroup1 group1 in cUSPMT.Group1)
				{
					foreach (DTMSegment dtm in group1.DTM)
					{
						if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.GoodsDeclarationDocumentAcceptanceDateTime)
						{
							var authDate = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
							if (!string.IsNullOrEmpty(authDate) && dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode == DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmmsszzz)
							{
								result = new ZDateTime(Convert.ToInt32(authDate.Substring(0, 4)), Convert.ToInt32(authDate.Substring(4, 2)), Convert.ToInt32(authDate.Substring(6, 2)), Convert.ToInt32(authDate.Substring(8, 2)), Convert.ToInt32(authDate.Substring(10, 2)), Convert.ToInt32(authDate.Substring(12, 2)));
							}
						}
					}
				}

				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		ZDecimal TotalPayable
		{
			get
			{
				foreach (SegmentGroup51 group51 in cUSPMT.Group51)
				{
					foreach (MOASegment mOA in group51.MOA)
					{
						if (mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.AmountDueAmountPayable)
						{
							return Convert.ToDecimal(mOA.MonetaryAmount.MonetaryAmount);
						}
					}
				}

				return ZDecimal.Zero;
			}
		}

		ZString PaymentParty
		{
			get
			{
				foreach (SegmentGroup1 group1 in cUSPMT.Group1)
				{
					foreach (FTXSegment fTX in group1.FTX)
					{
						if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.CustomsClearanceInstructions)
						{
							if (fTX.TextReference.FreeTextDescriptionCode == "G7")
							{
								return "BRK";
							}

							if (fTX.TextReference.FreeTextDescriptionCode == "GF")
							{
								return "IMP";
							}
						}
					}
				}

				return "DEF";
			}
		}

		void CompilePermitResponseEmail()
		{
			HtmlTableCreator creator = null;
			if (cUSPMT.Group1.Count > 0)
			{
				creator = new HtmlTableCreator(new string[] { "Type", "Condition Code", "Description" });
				foreach (SegmentGroup1 group1 in cUSPMT.Group1)
				{
					foreach (FTXSegment fTX in group1.FTX)
					{
						var textLiteral = Regex.Replace(fTX.TextLiteral.FreeText1, " {2,}", " ").Trim();
						creator.WriteRow(fTX.TextSubjectCodeQualifier, fTX.TextReference.FreeTextDescriptionCode, textLiteral);
					}
				}
			}

			string emailTemplateHtml;
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.Messaging.CustomsMessaging.MessageProcessors.HtmlTemplates.Cuspmt.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}
			emailTemplateHtml = ZString.Format(emailTemplateHtml, entry.Declaration.JE_DeclarationReference, entry.EntryNumber, entry.CertificateNumber, entry.CH_Status, URN);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator?.ToHtml() ?? string.Empty);

			var emailSender = new HtmlNotificationEmailSender();
			responseEmail = emailSender.CreateEmail("Customs Permit for " + entry.Declaration.JE_DeclarationReference, emailTemplateHtml);
			if (attachPermitPrint && SGCustomsDataRegistry.Instance.AttachPermitToAcknowledgementEmail.GetFallBackValueAtAllLevels(entry.Declaration.CompanyPK.ToGuid(), entry.Declaration.Branch.PK.ToGuid(), Guid.Empty))
			{
				var printPermitProcessor = new PrintPermitProcessor();
				var pdfAttachment = printPermitProcessor.GenerateDocumentToPDFAttachment(incomingMessage);
				if (pdfAttachment != null)
				{
					responseEmail.Attachments.Add(pdfAttachment);
				}
			}
		}

		ZBool attachPermitPrint;
	}
}
