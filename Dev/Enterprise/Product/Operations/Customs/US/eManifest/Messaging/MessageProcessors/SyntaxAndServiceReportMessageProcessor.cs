using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.CONTRL;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using EDIMessage = Enterprise.Customs.US.eManifest.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	class SyntaxAndServiceReportMessageProcessor : ResponseMessageProcessor
	{
		public SyntaxAndServiceReportMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		#region Overrides of MessageProcessor

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var contrlMessage = ediMessage.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet()) as CONTRLMessage;
			if (contrlMessage != null)
			{
				var wrapper = new SyntaxAndServiceReportMessageWrapper(contrlMessage);
				foreach (var response in wrapper.MessageResponses)
				{
					var message = (EDIMessage)(ediMessage.EM_Status == EDIMessage.Status.Queued ? ediMessage : ediMessage.Clone(CloneArgs));
					SetMessageNumberAndTypeAndDefineStatusCalculator(message, response);
					SetBranchAndLinkedObjectAndItsReference(message);

					if (linkedObject != null)
					{
						using (DisposableEnvironment.ForBranch(message.EM_GB.ToGuid()))
						{
							if (response.IsAcknowledgement)
							{
								SetAcknowledgementResponseEmailOnMessage(message);
							}
							else if (response.IsSyntaxError)
							{
								var email = GetSyntaxErrorResponseEmailAndSetOnMessage(message, response);
								var messageSubType = statusCalculator.GetMessageSubType(linkedObject.MessageStatus);
								linkedObject.MessageStatus = statusCalculator.GetMessageRejectedStatus(messageSubType);
								linkedObject.JobStatus = statusCalculator.CalculatedJobStatus(linkedObject);
								SendErrorReport(EmailResponseLinkedObject, email);
							}
						}
						message.EM_Status = EDIMessage.Status.Received;
					}
					else
					{
						var exception = new CouldNotFindAssociatedTransmitMessageException(message, this);
						MessageProcessorErrorReporter.ProcessException(exception, true);
						message.EM_Status = EDIMessage.Status.Failed;
					}
				}
			}

			if (ediMessage.EM_Status == EDIMessage.Status.Queued)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}
			return ediMessage.EM_Status;
		}

		#region Set Message Number/Type/Linked Object/Branch

		void SetMessageNumberAndTypeAndDefineStatusCalculator(EDIMessage message, SyntaxAndServiceReportMessageWrapper.MessageResponse response)
		{
			message.EM_MessageNum = response.MessageNumber;
			message.EM_MessageType = response.IsSyntaxError ? MessageTypes.Codes.SyntaxError : MessageTypes.Codes.Acknowledgement;

			originalMessage = message.OriginalMessage;
			if (originalMessage != null)
			{
				message.EM_MessageSubType = originalMessage.EM_MessageType;
			}

			statusCalculator = new eManifestStatusCalculator(message.EM_MessageSubType);
		}

		void SetBranchAndLinkedObjectAndItsReference(EDIMessage message)
		{
			linkedObject = null;
			linkedObjectReference = ZString.Empty;

			originalMessage = message.OriginalMessage;
			if (originalMessage != null)
			{
				var trip = originalMessage.EM_LinkedObject as Trip;
				if (trip != null)
				{
					linkedObject = new eManifestMessageWrapper(trip, originalMessage.EM_MessageType);
					linkedObjectReference = linkedObject.JobIdentification;
					linkedObject.Messages.Add(message);
				}
				var branch = originalMessage.Branch;
				if (branch != null)
				{
					message.EM_GB = branch.PK;
				}
			}
			message.EM_GB = (message.Branch ?? GlbBranch.CurrentBranch).PK;
		}

		BusinessObjectCloneArgs CloneArgs
		{
			get
			{
				return cloneArgs ?? (cloneArgs = new BusinessObjectCloneArgs(
				new[]
					{
						EDIMessage.Schema.EM_MessageNum,
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_GB,
						EDIMessage.Schema.EM_LinkTable,
						EDIMessage.Schema.EM_LinkUniqueID
					}));
			}
		}

		BusinessObjectCloneArgs cloneArgs;

		#endregion

		#region Emails

		void SetAcknowledgementResponseEmailOnMessage(EDIMessage message)
		{
			var emailBuilder = new EmailDefBuilder(string.Empty, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.ValidatedResponse);
			emailBuilder.AddArgReplacementRange(GetJobLink(), linkedObjectReference, statusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			message.EM_MessageInterpretation = emailBuilder.ToString();
		}

		EmailDef GetSyntaxErrorResponseEmailAndSetOnMessage(EDIMessage message, SyntaxAndServiceReportMessageWrapper.MessageResponse response)
		{
			var sourceMessageText = message.OriginalMessage.EM_MessageText;
			var syntaxErrors = response.GetSyntaxErrors(sourceMessageText).ToList();
			var syntaxErrorsText = TableInterpretation.GetTableInterpretation(syntaxErrors, null, TableInterpretation.Attributes.AlignLeft);
			var sourceMessageTextWithErrorMarks = response.GetSourceMessageTextWithErrorMarks(sourceMessageText, syntaxErrors);

			var subject = string.Format("Syntax Error {0} Response for {1}", statusCalculator.MessageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_MessageText.Replace("'", "\r\n"), EmailDefBuilder.HtmlTemplates.ErrorResponse);
			emailBuilder.AddArgReplacementRange(GetJobLink(), linkedObjectReference, statusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, syntaxErrorsText);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, "Please report this to CargoWise.");
			message.EM_MessageInterpretation = emailBuilder.ToString();

			emailBuilder.AddAttachment("SourceMessageWithErrorMarks.txt", sourceMessageTextWithErrorMarks);
			return emailBuilder.ToEmail();
		}

		#endregion

		#endregion
	}
}
