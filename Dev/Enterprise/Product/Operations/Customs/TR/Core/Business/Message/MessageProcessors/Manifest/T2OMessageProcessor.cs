using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class T2OMessageProcessor : ManifestMessageProcessorBase
	{
		public T2OMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessMessage(TRManifestMessage message, IMessageAttachee headerAttachee)
		{
			var result = true;

			if (message.MessageObject.InnerMessageObjects.Cast<DiffGramGuidObject>().OrderByDescending(t2Object => t2Object.OptionTime).FirstOrDefault() is DiffGramGuidObject resultObject && !resultObject.Guid.IsEmpty)
			{
				NeedToSendEmail = NeedToUpdateStatus = false;

				var temporaryQueryGUID = resultObject.Guid;
				message.EM_ApplicationReference = temporaryQueryGUID.ToString();
				TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.T2O, message.EM_ApplicationReference);

				TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
				TableCreator.WriteRow(Res.GetString("8EBC6001-00C1-4487-85E0-8A968EB6072B", "Query GUID:"), temporaryQueryGUID);
			}

			return result;
		}

		protected override ZString GetInterpretationTitle(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess)
			=> Res.GetString("F8D7633B-BD15-4C3A-B129-E5F3DFF2F22A", "Manifest Message newest GUID for job {0} sent", messageAttacheeBO.JobReference);

		protected override bool ShouldHandleInvalidCredentialMessage => true;
	}
}
