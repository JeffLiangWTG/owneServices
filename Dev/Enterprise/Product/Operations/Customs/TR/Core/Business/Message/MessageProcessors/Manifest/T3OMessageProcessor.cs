using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Integration.Customs.ASYCUDA.TRManifest;

namespace Enterprise.Customs.TR.Business
{
	public class T3OMessageProcessor : ManifestMessageProcessorBase
	{
		public T3OMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessage(TRManifestMessage message, IMessageAttachee headerAttachee)
		{
			var isSuccess = true;
			TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass);

			var defaultMessageObject = message.MessageObject.InnerMessageObjects.FirstOrDefault();

			var status = TRMessageSendingHelper.ProcessCusPollingTransaction(message, TRMessageTypes.Codes.T2O, defaultMessageObject != null);
			if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR)
			{
				isSuccess = false;
				TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass);
				TableCreator.WriteRow(SoapMessageTextHelper.Row_EmptyResponse);
			}
			else if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS || defaultMessageObject != null)
			{
				if (defaultMessageObject is InnerXmlRegistrationNumberObject registrationNumberObject && !registrationNumberObject.RegistrationNumber.IsEmpty)
				{
					isSuccess = true;
					var registrationNumber = registrationNumberObject.RegistrationNumber;

					var header = message.EM_LinkedObject as IAsycudaManifestHeader;
					header.SuspendValidation();
					header.RegistrationNumber = registrationNumber;
					header.RegistrationDate = registrationNumberObject.RegistrationTime;
					header.AMA_GS_NKCustomsAgent = MessageProcessorHelper.GetMessageSignedBy(header);

					TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
					TableCreator.WriteRow(SoapMessageTextHelper.RowHeader_RegistrationNumber, registrationNumber);
					TableCreator.WriteRow(SoapMessageTextHelper.RowHeader_IssueDate, registrationNumberObject.RegistrationTime.ToString(InnerMessageObjectBase.Constants.DefaultDateTimeFormat));

					header.CalculateStampDuties();
					header.CreateManifestStatement();

					if (!registrationNumber.IsEmpty)
					{
						var origialMessage = TRInterchangeHelper.GetMainMessageByType(message, TRMessageTypes.Codes.TRO);
						TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForTRM(header, origialMessage);
					}
				}

				else if (defaultMessageObject is InnerXmlErrorMessagesObject errorMessagesObject)
				{
					TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, new[] { SoapMessageTextHelper.TableHeaderWithErrorMessage });
					foreach (var errorMessage in errorMessagesObject.ErrorMessages)
					{
						TableCreator.WriteRow(new string[] { errorMessage });
					}
					isSuccess = false;
				}
				else if (defaultMessageObject is InnerMessageObjectBase innerObject)
				{
					TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, new[] { SoapMessageTextHelper.TableHeaderWithErrorMessage });

					if (!innerObject.MessageText.IsEmpty())
					{
						TableCreator.WriteRow(new string[] { innerObject.MessageText });
					}

					isSuccess = false;
				}
			}
			else if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN)
			{
				NeedToSendEmail = NeedToUpdateStatus = false; // if status still is OPN, do nothing, will send query again.
			}

			return isSuccess;
		}
	}
}
