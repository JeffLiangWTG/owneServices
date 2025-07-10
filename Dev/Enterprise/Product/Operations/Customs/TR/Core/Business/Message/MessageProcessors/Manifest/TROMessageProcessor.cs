using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class TROMessageProcessor : ManifestMessageProcessorBase
	{
		public TROMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessMessage(TRManifestMessage message, IMessageAttachee headerAttachee)
		{
			var result = true;

			var defaultObject = message.MessageObject.InnerMessageObjects.FirstOrDefault();

			if (defaultObject is SOAPLevelRefIDAndGuidObject troResult)
			{
				var temporaryQueryGUID = troResult.Guid.ToString();
				TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.TRO, temporaryQueryGUID);

				TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
				TableCreator.WriteRow(Res.GetString("189CBEB2-E2EA-4CC5-AAA4-716B3358179B", "Query GUID:"), temporaryQueryGUID);
			}
			else
			{
				result = false;
			}

			return result;
		}

		protected override void HandleErrorMessage(TRManifestMessage message, SOAPLevelErrorObject errorObject)
		{
			if (errorObject.ErrorMessage == TRMessageConstants.RegisteredOrInProcessWithThisReferenceMessage)
			{
				HasEesponseErrorMessageForT2O = true;
				var header = message.EM_LinkedObject as Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;
				var origialMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
				TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForT2O(header, origialMessage);
			}
		}

		protected override ZString GetInterpretationTitle(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess)
		{
			ZString title = ZString.Empty;
			if (HasEesponseErrorMessageForT2O)
			{
				title = Res.GetString("E642F9FE-7630-4F60-BEA8-3A4C84A0245F", "Manifest Message for job {0} has been rejected.", messageAttacheeBO.JobReference);
			}
			else if (isSuccess)
			{
				title = Res.GetString("FCF6DAF0-83D6-4635-98A6-3F36F25845B2", "Manifest Message for job {0} sent", messageAttacheeBO.JobReference);
			}

			return title;
		}

		protected override bool ShouldHandleInvalidCredentialMessage => true;

		bool HasEesponseErrorMessageForT2O { get; set; }
	}
}
