using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	abstract class BasePermitSectionProcessor<T> : BaseResponseSectionProcessor<T> where T : ITradeNetOutPermitSection
	{
		public const string MessageType = "PMT";
		public const string MessageReferencePrefix = "WTG";

		protected BasePermitSectionProcessor(LoggingInformation logger, T section, string messageName)
			: base(logger, section, MessageType, messageName)
		{
		}

		protected override string URN => GetReferenceNumber(Header?.UniqueReferenceNumber);

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void SetEntryStatus()
		{
			var commonAccessReference = GetCommonAccessReference();
			switch (commonAccessReference)
			{
				case CommonAccessReferenceCodeList.Codes.INPPMT:
				case CommonAccessReferenceCodeList.Codes.IPTPMT:
				case CommonAccessReferenceCodeList.Codes.OUTPMT:
				case CommonAccessReferenceCodeList.Codes.TNPPMT:
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
						ShouldAttachPermitPrint = true;
						break;
					}

				case CommonAccessReferenceCodeList.Codes.INPUPT:
				case CommonAccessReferenceCodeList.Codes.OUTUPT:
				case CommonAccessReferenceCodeList.Codes.TNPUPT:
					{
						entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
						ShouldAttachPermitPrint = true;
						break;
					}

				case CommonAccessReferenceCodeList.Codes.IPTUPT:
					{
						entry.CH_Status = NeedToUpdateRefund
								? Core.SGConstants.DeclarationStatus.RefundPermitReceived
								: Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;

						ShouldAttachPermitPrint = !NeedToUpdateRefund;
						break;
					}
			}

			entry.EntryNumber = PermitNumber;
			entry.CertificateNumber = CertificateNumber;
			entry.Declaration.JE_EntryAuthorisationDate = AuthorisationDate;
		}

		protected override string DoProcessingReturningStatusCore(EDIMessage incomingMessage)
		{
			Logger.Log(FormattableString.Invariant($"Processing {MessageFriendlyName} Message"));

			var result = EDIMessage.Status.Failed;
			this.incomingMessage = incomingMessage;

			if (Permit != null)
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
				responseEmail.Body = "FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + incomingMessage.EM_FormattedMessageText;

				SendErrorReport(null, responseEmail);
			}

			return result;
		}

		#region Implement

		void CompilePermitResponseEmail()
		{
			var htmlContent = GetHtmlContent();
			var declaration = entry.Declaration;

			CreateResponseEmail("Customs Permit for ", "Cuspmt.htm", htmlContent, EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), entry.EntryNumber, entry.CertificateNumber, entry.CH_Status, URN);

			if (ShouldAttachPermitPrint && SGCustomsDataRegistry.Instance.AttachPermitToAcknowledgementEmail.GetFallBackValueAtAllLevels(entry.Declaration.CompanyPK.ToGuid(), entry.Declaration.Branch.PK.ToGuid(), Guid.Empty))
			{
				var printPermitProcessor = new PrintPermitProcessor();
				var pdfAttachment = printPermitProcessor.GenerateDocumentToPDFAttachment(incomingMessage);
				if (pdfAttachment != null)
				{
					responseEmail.Attachments.Add(pdfAttachment);
				}
			}
		}

		string GetHtmlContent()
		{
			var result = new ZStringBuilder();

			var permit = Permit;
			var permitValidityPeriod = permit.PermitValidityPeriod;

			if (permitValidityPeriod != null)
			{
				var creator = new HtmlTableCreator(new[] { "Permit Validity Period" }) { EnableHTMLEncoding = false };

				var tableInterpretation = new FieldValueTableInterpretation(false);
				tableInterpretation.Add("Start Date", permitValidityPeriod.StartDate);
				tableInterpretation.Add("End Date", permitValidityPeriod.EndDate);

				creator.WriteRow(tableInterpretation.ToHtml());
				result.AppendLine(creator.ToHtml());
			}

			var caApprovalCondition = permit.CAApprovalCondition;
			var scApprovalCondition = permit.SCApprovalCondition;

			if ((caApprovalCondition?.Any() ?? false) || (scApprovalCondition?.Any() ?? false))
			{
				var creator = new HtmlTableCreator(new string[] { "Type", "Code", "Description" });

				if (caApprovalCondition != null)
				{
					foreach (var approvalCondition in caApprovalCondition)
					{
						creator.WriteRow(approvalCondition.AgencyCode ?? string.Empty, approvalCondition.ConditionCode ?? string.Empty, approvalCondition.ConditionDescription ?? string.Empty);
					}
				}

				if (scApprovalCondition != null)
				{
					foreach (var approvalCondition in scApprovalCondition)
					{
						creator.WriteRow(approvalCondition.AgencyCode ?? string.Empty, approvalCondition.ConditionCode ?? string.Empty, approvalCondition.ConditionDescription ?? string.Empty);
					}
				}

				result.AppendLine(creator.ToHtml());
			}

			return result.ToString();
		}

		void AssignValuesToCusEntryPayInfo()
		{
			var messageReferenceNumber = Header?.MessageReference?.Replace(MessageReferencePrefix, string.Empty) ?? string.Empty;

			var payInfo = entry.EntryPayInfos.GetItemByMessageNum(messageReferenceNumber)
				?? entry.EntryPayInfos.AddNew();

			payInfo.C9_IncomingPayResponseNo = messageReferenceNumber;
			payInfo.C9_PaymentAmount = TotalPayable;
			payInfo.C9_TransactionType = DocumentName;
			payInfo.C9_PaymentDate = AuthorisationDate;
			payInfo.C9_PaymentReference = PermitNumber;
			payInfo.C9_PaymentParty = PaymentParty;
		}

		ZDateTime ConvertApprovalDatetime(ZString dateTime)
		{
			return dateTime.IsEmpty
				? ZDateTime.Empty
				: new ZDateTime(Convert.ToInt32(dateTime.SubstringSafe(0, 4)), Convert.ToInt32(dateTime.SubstringSafe(4, 2)), Convert.ToInt32(dateTime.SubstringSafe(6, 2)), Convert.ToInt32(dateTime.SubstringSafe(8, 2)), Convert.ToInt32(dateTime.SubstringSafe(10, 2)), Convert.ToInt32(dateTime.SubstringSafe(12, 2)));
		}

		#endregion

		#region Properties

		protected ITradeNetInSection Declaration => Section.Declaration;

		protected Permit Permit => Section.Permit;

		protected Header Header => Declaration?.Header;

		protected Summary Summary => Declaration?.Summary;

		protected virtual bool NeedToUpdateRefund => true;

		protected bool ShouldAttachPermitPrint { get; set; }

		protected string PermitNumber => Permit?.PermitNumber ?? string.Empty;

		protected ZDateTime AuthorisationDate => ConvertApprovalDatetime(Permit?.PermitApprovalDatetime ?? ZString.Empty);

		protected string PaymentParty
		{
			get
			{
				var conditions = Permit?.SCApprovalCondition ?? Enumerable.Empty<PermitSCApprovalCondition>();

				foreach (var condition in conditions)
				{
					if (condition.ConditionCode == "G7")
					{
						return "BRK";
					}

					if (condition.ConditionCode == "GF")
					{
						return "IMP";
					}
				}

				return "DEF";
			}
		}

		protected string CertificateNumber => Permit?.CertificateNumber ?? string.Empty;

		protected string DocumentName => Header?.DeclarationType ?? string.Empty;

		protected ZDecimal TotalPayable => Summary?.TotalTariff?.TotalAmountPayable ?? 0m;

		#endregion
	}
}
