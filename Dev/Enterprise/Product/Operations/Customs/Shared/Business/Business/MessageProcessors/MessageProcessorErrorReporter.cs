using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors.ErrorReporting
{
	#region IErrorNotification

	public interface IErrorNotification
	{
		void SendError(EmailDef email);
		void SendErrorToPostMaster(EmailDef email);
		void LogError(string errorMessage);
		string MessageProcessorName { get; }
	}

	#endregion

	public static class MessageProcessorErrorReporter
	{
		public static void ProcessException(MessageProcessorException exception)
		{
			ProcessException(exception, false);
		}

		public static void ProcessException(MessageProcessorException exception, bool shouldSetEmailBodyOnMessage)
		{
			exception.EDIMessage.EM_Status = EDIMessage.Status.Failed;
			exception.MessageProcessor.LogError(Res.GetString("f475fd15-ca4d-482b-938c-c52a8c936ea1", "{0} Message Processor: {1}", exception.MessageProcessor.MessageProcessorName, exception.Message));

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var emailBuilder = exception.DoNotReferenceMessage ?
				new EmailDefBuilder(exception.Subject, EmailDefBuilder.HtmlTemplates.MailProblemResponse) :
				new EmailDefBuilder(exception.Subject, exception.EDIMessage.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.MailProblemResponse);
			emailBuilder.AddArgReplacement(string.Format("{0} ({1})", exception.EDIMessage.EM_MessageType, exception.EDIMessage.HumanReadableName));
			emailBuilder.AddArgReplacement(exception.Message);
			emailBuilder.AddArgReplacement(exception.DoNotReferenceMessage ? ZString.Empty : exception.EDIMessage.Report.Replace("\r\n", "<br>"));
			emailBuilder.AddArgReplacement(Db.Connection.ServerNameReportedByDatabase);
			emailBuilder.AddArgReplacement(Db.DatabaseName);
			emailBuilder.AddArgReplacement(registrationKey.EnterpriseCode);
			emailBuilder.AddArgReplacement(registrationKey.ServerCode);

			if (shouldSetEmailBodyOnMessage)
			{
				exception.EDIMessage.EM_MessageInterpretation = emailBuilder.ToString();
			}

			var email = emailBuilder.ToEmail();
			exception.MessageProcessor.SendError(email);
			exception.MessageProcessor.SendErrorToPostMaster(email);

			var criticalException = exception as CriticalMessageProcessorException;
			if (criticalException != null)
			{
				var errorKey = string.Format((NoResString)"Message Processor: {0}, Message Type: {1}, Error Type: {2}",
											 exception.MessageProcessor.MessageProcessorName,
											 criticalException.EDIMessage.EM_MessageType,
											 criticalException.ErrorType);

				ErrorReporter.ReportOnce(errorKey, string.Format("Error: {0}. Message: {1}.", exception.Message, exception.EDIMessage.EM_MessageText.SubstringSafe(0, 100)));
			}
		}
	}
}
