using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DIS.Business
{
	class DocumentReviewResponseProcessor
	{
		public DocumentReviewResponseProcessor(LoggingInformation logger)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;
		const string RejectedStatus = "REJECTED";

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Process(EDIMessage message)
		{
			JobRequiredDocumentAddInfo requiredDocumentAddInfo = null;
			DISDocument disDocument = null;
			IDISHost disHost = null;
			var disHostJobNumber = ZString.Empty;
			var documentID = GetDISDocumentID(message);

			if (!documentID.IsEmpty)
			{
				disHostJobNumber = documentID.SubstringSafe(0, documentID.IndexOf("_DIS", StringComparison.OrdinalIgnoreCase));
				disHost = GetDISHOst(message, disHostJobNumber);
				if (disHost != null)
				{
					var wrapper = new DISHostWrapper((IUSDISHost)disHost);
					disDocument = wrapper.DISDocuments.OfType<DISDocument>().FirstOrDefault(x => x.DocumentID == documentID);
					if (disDocument != null)
					{
						requiredDocumentAddInfo = disDocument.RequiredDocumentAddInfo;
						if (requiredDocumentAddInfo != null)
						{
							message.EM_LinkedObject = requiredDocumentAddInfo;
						}
					}
				}
			}

			var branch = ProcesserHelper.GetFallbackBranch(disHost, message.RelatedMessage, message) ?? GlbBranch.CurrentBranch;

			if (requiredDocumentAddInfo == null)
			{
				var warningMessage = "This message with message # " + message.EM_MessageNum + " cannot be linked back to an originating DIS document.";
				logger.LogWarning(warningMessage);
				ProcesserHelper.SendNotification(message.Factory, warningMessage, message.EM_MessageText, branch.GB_GC, branch.PK);
				return;
			}

			var htmlTable = new HtmlTableCreator(new string[] { "Column", "Description" });

			var importerName = disHost != null ? (string)disHost.ImporterName : string.Empty;

			var documentReviewStatus = string.Empty;

			var documentLabel = message.MessageContent.Descendants().Where(x => x.Matches(EDIMessage.Constants.DocumentLabel)).Select(x => x.Value).FirstOrDefault();

			var documentReviewResult = message.MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewResult));

			if (documentReviewResult != null)
			{
				htmlTable.WriteProcessingResultRows(documentReviewResult);
				documentReviewStatus = documentReviewResult.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewStatus))?.Value.ToUpper();

				if (!string.IsNullOrEmpty(documentReviewStatus))
				{
					SetStatus(documentReviewStatus, requiredDocumentAddInfo);
				}
			}

			var emailAddress = ZString.Empty;
			EDIMessage outgoingMessage = null;

			if (disDocument != null)
			{
				outgoingMessage = disDocument.Messages.Cast<EDIMessage>().Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit).OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();
			}

			var originalSender = outgoingMessage != null ? outgoingMessage.UserWhoQueuedThisRecord : null;

			if (originalSender != null)
			{
				emailAddress = originalSender.GS_EmailAddress;
			}

			if (!emailAddress.IsEmpty)
			{
				string uri = disHost == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(disHost.ControllerIDProvider);
				SendEmail(uri, string.Format(CultureInfo.CurrentCulture, "{0}/{1}/{2}", disHostJobNumber, documentID, importerName), documentLabel, htmlTable.ToHtml(), documentReviewStatus == RejectedStatus, emailAddress, branch);
			}
			else
			{
				var warningMessage = "System could not locate an original sender for " + message.EM_MessageNum + " and could not send a notification email.";
				logger.LogWarning(warningMessage);
				ProcesserHelper.SendNotification(message.Factory, warningMessage, message.EM_MessageText, branch.GB_GC, branch.PK);
			}

			message.NullifyMessageContent();
		}

		IDISHost GetDISHOst(EDIMessage message, ZString disHostJobNumber)
		{
			ZQuery query;
			IDISHost disHost = null;

			if (IsISF(message))
			{
				query = new ZQuery(CusISFHeaderSchema.BF_JobReference, disHostJobNumber);
				query.AddToFilter(CusISFHeaderSchema.BF_IsCancelled, false);
				var isfHeader = message.Factory.LoadTop1<Integration.Customs.US.ISF.ICusISFHeader>(query);
				if (isfHeader != null)
				{
					disHost = (IDISHost)isfHeader;
				}
			}
			else
			{
				query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, disHostJobNumber);
				query.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				query.AddToFilter(JobDeclarationSchema.JE_IsCancelled, false);
				var declaration = message.Factory.LoadTop1<Integration.Customs.US.IJobDeclaration>(query);
				if (declaration != null)
				{
					disHost = (IDISHost)declaration;
				}
			}

			return disHost;
		}

		ZString GetDISDocumentID(EDIMessage message)
		{
			var result = ZString.Empty;

			var xDocumentID = message.MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentID));

			if (xDocumentID != null)
			{
				result = xDocumentID.Value;
			}

			return result;
		}

		bool IsISF(EDIMessage message)
		{
			return message.MessageContent.Descendants().Any(x => x.Matches(EDIMessage.Constants.ISFNumber));
		}

		void SetStatus(string status, JobRequiredDocumentAddInfo requiredDocumentAddInfo)
		{
			var code = Common.US.DIS.StatusList.GetCodeByDocumentReviewStatus(status);
			requiredDocumentAddInfo.EX_Status = code;
			requiredDocumentAddInfo.RequiredDocument?.Parent?.UltimateDocumentParent?.GetLogs()?.AddNew(Events.MessageStatusChange, "DIS " + code);
		}

		void SendEmail(string uri, string disHostJobNumber, string documentLabel, string emailBody, bool failed, ZString emailAddress, GlbBranch branch)
		{
			var emailGenerator = new HtmlResponseEmailGenerator();
			EmailDef emailDef;

			if (emailGenerator.TryGenerateEmail(uri, disHostJobNumber, string.Format(CultureInfo.CurrentCulture, "DIS {0} Document Review", string.IsNullOrEmpty(documentLabel) ? "Document" : documentLabel), emailBody, failed, out emailDef, branch))
			{
				emailDef.AddRecipientForSystemCommunication(emailAddress);
				Env.OutgoingCustomsMailManager.CreateAndSave(emailDef);
			}
		}
	}
}
