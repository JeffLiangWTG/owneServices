using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.DIS.Business
{
	class DocumentValidationResponseProcessor
	{
		public DocumentValidationResponseProcessor(LoggingInformation logger)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Process(EDIMessage message)
		{
			var requiredDocumentAddInfo = LinkAndGetJobRequiredDocumentAddInfo(message);

			if (requiredDocumentAddInfo == null)
			{
				var warningMessage = "This message with message # " + message.EM_MessageNum + " cannot be linked back to an originating DIS document.";
				logger.LogWarning(warningMessage);
				var nonDisBranch = ProcesserHelper.GetFallbackBranch(null, message.RelatedMessage, message) ?? GlbBranch.CurrentBranch;
				ProcesserHelper.SendNotification(message.Factory, warningMessage, message.EM_MessageText, nonDisBranch.GB_GC, nonDisBranch.PK);
				return;
			}

			var htmlTable = new HtmlTableCreator(new string[] { "Column", "Description" });

			var disHost = GetDISHost(requiredDocumentAddInfo);
			var disHostJobNumber = disHost != null ? disHost.JobNumber : ZString.Empty;
			var importerName = disHost != null ? (string)disHost.ImporterName : string.Empty;
			var documentID = GetDISDocumentID(message, disHostJobNumber);
			var failed = false;

			var documentLabel = message.MessageContent.Descendants().Where(x => x.Matches(EDIMessage.Constants.DocumentLabel)).Select(x => x.Value).FirstOrDefault();

			var xProcessingResult = message.MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentProcessingResult)) ?? message.MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.MessageProcessingResult));

			if (xProcessingResult != null)
			{
				htmlTable.WriteProcessingResultRows(xProcessingResult);

				failed = xProcessingResult.HasFailed();

				SetStatus(failed, requiredDocumentAddInfo, htmlTable);
				UpdateEntrySummaryNotificationStatus(failed, disHost, documentID);
			}

			var emailAddress = ZString.Empty;
			var outgoingMessage = message.RelatedMessage;
			var originalSender = outgoingMessage != null ? outgoingMessage.UserWhoQueuedThisRecord : null;
			if (originalSender != null)
			{
				emailAddress = originalSender.GS_EmailAddress;
			}

			var branch = ProcesserHelper.GetFallbackBranch(disHost, message.RelatedMessage, message) ?? GlbBranch.CurrentBranch;
			if (!emailAddress.IsEmpty)
			{
				string uri = disHost == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(disHost.ControllerIDProvider);
				SendEmail(uri, string.Format(CultureInfo.CurrentCulture, "{0}/{1}/{2}", disHostJobNumber, documentID, importerName), documentLabel, htmlTable.ToHtml(), failed, emailAddress, branch);
			}
			else
			{
				var warningMessage = "System could not locate an original sender for " + message.EM_MessageNum + " and could not send a notification email.";
				logger.LogWarning(warningMessage);
				ProcesserHelper.SendNotification(message.Factory, warningMessage, message.EM_MessageText, branch.GB_GC, branch.PK);
			}

			message.NullifyMessageContent();
		}

		JobRequiredDocumentAddInfo LinkAndGetJobRequiredDocumentAddInfo(EDIMessage message)
		{
			var outgoingMessage = message.RelatedMessage;
			if (outgoingMessage != null)
			{
				message.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
			}

			return message.RequiredDocumentAddInfo;
		}

		IDISHost GetDISHost(JobRequiredDocumentAddInfo requiredDocumentAddInfo)
		{
			var requiredDocument = requiredDocumentAddInfo.RequiredDocument;

			var disHostProvider = requiredDocument != null ? requiredDocument.Parent as IDISHostProvider : null;

			return disHostProvider != null ? disHostProvider.DISHost : null;
		}

		ZString GetDISDocumentID(EDIMessage message, string disHostJobNumber)
		{
			var result = ZString.Empty;

			var xDocumentID = message.MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentID));

			if (xDocumentID != null)
			{
				result = xDocumentID.Value;
			}

			if (result.IsEmpty && message.RelatedMessage != null)
			{
				result = DISDocument.FormatDocumentID(disHostJobNumber, message.RelatedMessage.EM_MessageOwner);
			}

			return result;
		}

		void SetStatus(bool failed, JobRequiredDocumentAddInfo requiredDocumentAddInfo, HtmlTableCreator htmlTable)
		{
			var status = string.Empty;
			switch (requiredDocumentAddInfo.EX_Status)
			{
				case StatusList.Codes.AOS:
					status = failed ? StatusList.Codes.EOS : StatusList.Codes.COS;
					break;
				case StatusList.Codes.ARS:
					status = failed ? StatusList.Codes.ERS : StatusList.Codes.CRS;
					break;
				case StatusList.Codes.AWS:
					status = failed ? StatusList.Codes.EWS : StatusList.Codes.CWS;
					break;
			}

			if (!string.IsNullOrEmpty(status))
			{
				requiredDocumentAddInfo.EX_Status = status;
				requiredDocumentAddInfo?.RequiredDocument?.Parent?.UltimateDocumentParent?.GetLogs()?.AddNew(Events.MessageStatusChange, "DIS " + status);
			}
			else
			{
				htmlTable.WriteRow("Orig. status", requiredDocumentAddInfo.EX_Status);
			}
		}

		void UpdateEntrySummaryNotificationStatus(bool failed, IDISHost disHost, ZString documentID)
		{
			if (!failed && disHost is JobDeclaration declaration)
			{
				var wrapper = new DISHostWrapper((IUSDISHost)disHost);
				var disDocument = wrapper.DISDocuments.OfType<DISDocument>().FirstOrDefault(x => x.DocumentID == documentID);
				var actionID = disDocument?.CBPRequest.ID;
				if (!string.IsNullOrEmpty(actionID))
				{
					declaration.UpdateEntrySummaryNotificationStatus(actionID.Value);
				}
			}
		}

		void SendEmail(string uri, string disHostJobNumber, string documentLabel, string emailBody, bool failed, ZString emailAddress, GlbBranch branch)
		{
			var emailGenerator = new HtmlResponseEmailGenerator();
			EmailDef emailDef;
			if (emailGenerator.TryGenerateEmail(uri, disHostJobNumber, string.Format(CultureInfo.CurrentCulture, "DIS {0} Validation", string.IsNullOrEmpty(documentLabel) ? "Document" : documentLabel), emailBody, failed, out emailDef, branch))
			{
				emailDef.AddRecipientForSystemCommunication(emailAddress);
				Env.OutgoingCustomsMailManager.CreateAndSave(emailDef);
			}
		}
	}
}
