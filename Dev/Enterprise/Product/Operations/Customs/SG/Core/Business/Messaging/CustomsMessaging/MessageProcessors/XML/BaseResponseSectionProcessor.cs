using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	abstract class BaseResponseSectionProcessor<T> : SGMessageProcessor where T : ITradeNetOutSection
	{
		protected BaseResponseSectionProcessor(LoggingInformation logger, T section, string messageType, string messageName)
			: base(logger, messageType, messageName)
		{
			Section = section;
		}

		protected T Section { get; }

		public ZString GetReferenceNumber(UniqueReferenceNumber uniqueReferenceNumber)
		{
			return uniqueReferenceNumber?.GetReferenceNumber();
		}

		protected override EDIMessage IncomingMessage => incomingMessage;
		protected internal EDIMessage incomingMessage;
		string messageInterpretation;

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			var result = DoProcessingReturningStatusCore(message);

			if (string.IsNullOrEmpty(messageInterpretation))
			{
				messageInterpretation = responseEmail?.Body;
			}
			if (!string.IsNullOrEmpty(messageInterpretation))
			{
				message.EM_MessageInterpretation = messageInterpretation;
			}

			return result;
		}

		protected abstract string GetCommonAccessReference();

		protected abstract string DoProcessingReturningStatusCore(EDIMessage message);

		protected void CreateResponseEmail(string subjectPrefix, string templateType, HtmlTableCreator creator, params object[] htmlArgs)
		{
			CreateResponseEmail(subjectPrefix, templateType, creator?.ToHtml() ?? string.Empty, htmlArgs);
		}

		protected void CreateResponseEmail(string subjectPrefix, string templateType, string htmlContent, params object[] htmlArgs)
		{
			var emailTemplateHtml = string.Empty;
			var templateResource = string.Concat(CultureInfo.InvariantCulture, "Enterprise.Customs.SG.V4.Business.Messaging.CustomsMessaging.MessageProcessors.HtmlTemplates.", templateType);

			var assembly = Assembly.GetAssembly(typeof(BaseResponseSectionProcessor<>));
			using (var stream = assembly.GetManifestResourceStream(templateResource))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = string.Format(CultureInfo.InvariantCulture, emailTemplateHtml, htmlArgs);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", htmlContent ?? string.Empty);

			messageInterpretation = emailTemplateHtml;
			var emailSender = new HtmlNotificationEmailSender();

			var subject = string.Concat(CultureInfo.InvariantCulture, subjectPrefix, entry.Declaration.JE_DeclarationReference);
			responseEmail = emailSender.CreateEmail(subject, emailTemplateHtml);
		}
	}
}
