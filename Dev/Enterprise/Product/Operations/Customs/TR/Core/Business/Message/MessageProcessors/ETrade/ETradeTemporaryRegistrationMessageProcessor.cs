using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeTemporaryRegistrationMessageProcessor : ETradeMessageProcessor
	{
		public ETradeTemporaryRegistrationMessageProcessor(LoggingInformation logger)
		   : base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("8BF92F3F-BA24-4428-B755-530BA5C6433B", "E-Trade Temporary Registration Message");

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message) => Res.GetString("EA8152E5-ED37-4275-8B89-AE0D79795C61", "E-Trade Temporary Registration Message Status");

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			if (tableCreator == null)
			{
				tableCreator = new HtmlTableCreator();
			}
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("D916A38E-16BD-4167-89B0-5500087BC39C", "E-Trade Temporary Registration Message for job {0} has been cleared. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("C5A6E6F9-287A-4EFF-B8BB-3F2D24266C2A", "E-Trade Temporary Registration Message for job {0} has been rejected. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded, datetimeformat")]
		protected override bool ProcessMessageCore(ETradeEDIMessage message)
		{
			bool isSuccess = false, isContinue = true;
			var errorMessage = Res.GetString("FCE3A4FF-E220-419E-949B-2371B3336257", "Error Message: ");
			if (message != null)
			{
				if (message.EM_LinkedObject is AsycudaManifestHeader header)
				{
					var messageText = message.EM_MessageText;
					if (message.EM_MessageType == TRMessageTypes.Codes.T1E)
					{
						var status = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, TRMessageTypes.Codes.TRE);
						if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR)
						{
							tableCreator = new HtmlTableCreator("table", new string[] { SoapMessageTextHelper.TableHeaderWithErrorMessage });
							tableCreator.WriteRow(errorMessage, Res.GetString("B53BB70D-6984-42A8-B11B-77667F614DED", "The message text sent back from Customs is empty, please try to resend original message"));
							SendNotificationEmailIfNeeded(message.EM_LinkedObject as IMessageAttachee, message, isSuccess);
							isContinue = false;
						}
						else if (status == Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN)
						{
							isSuccess = true;
							isContinue = false;
						}
					}
					else if (message.EM_MessageType == TRMessageTypes.Codes.TRE)
					{
						var temporaryQueryGUID = SoapMessageTextHelper.GetOutputMessageGuid(messageText);
						if (!temporaryQueryGUID.IsEmpty)
						{
							message.EM_ApplicationReference = temporaryQueryGUID;
							TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.TRE, temporaryQueryGUID);

							isSuccess = true;
							isContinue = false;

							tableCreator = new HtmlTableCreator("table", SoapMessageTextHelper.RowHeaders_Labelvalue);
							tableCreator.WriteRow(Res.GetString("AA18386C-FE7C-41B8-B41E-F335520CE1AA", "Query GUID:"), temporaryQueryGUID);
							message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
						}
					}

					if (isContinue)
					{
						var parentNodeListSuccess = new List<ZString>() { "ETicaretSoapOut", (NoResString)"Record", (NoResString)"Sonuc" };
						var parentNodeListError = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc", "Hatalar", "Hata", "HataAciklamasi" };
						ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
						var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
						var errorMessages = TRMessageHelper.GetMultipleNodeValueList(xmlData, parentNodeListError);

						if (errorMessages.Any())
						{
							tableCreator = new HtmlTableCreator("table", new string[] { SoapMessageTextHelper.TableHeaderWithErrorMessage });
							foreach (var item in errorMessages)
							{
								tableCreator.WriteRow(new string[] { item });
							}
						}
						else
						{
							tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("9D7E224C-FA3E-4EA4-AE9B-94662D216B0D", "Label"), Res.GetString("01BDA206-3A04-4037-9485-01AA30EAECB6", "Value") });
							var registrationNumber = TRMessageHelper.GetNodeValue(xmlData, parentNodeListSuccess, "KayitNo");
							if (!registrationNumber.IsEmpty)
							{
								header.TempRegNo = registrationNumber;
								var registrationDate = TRMessageHelper.GetNodeValue(xmlData, parentNodeListSuccess, "KayitTarihi");
								if (ZDateTime.TryParseExact(registrationDate, out var registrationDateOut, "yyyy-MM-ddTHH:mm:ss"))
								{
									header.TempRegNoDate = registrationDateOut;
								}
								tableCreator.WriteRow(Res.GetString("C8EE6AEC-2B3F-4B10-B080-1B39CE1BC8F9", "Temporary Registration Number: "), header.TempRegNo);
								tableCreator.WriteRow(Res.GetString("C55B0DC8-80C5-408B-A309-769AD166D76E", "Temporary Registration Date: "), header.TempRegNoDate);
								isSuccess = true;
							}
							else
							{
								var outputMessage = SoapMessageTextHelper.GetOutputMessageStatus(messageText);
								tableCreator.WriteRow(errorMessage,
									outputMessage == SoapMessageTextHelper.Constants.OutputMessage.SigningCardAndUserInfoIncompatible
									? SoapMessageTextHelper.Constants.OutputMessage.SigningCardAndUserInfoIncompatible
									: Res.GetString("28D3076E-ACEA-4AE9-8E9F-A7D0561D6A07", "The message missed the Temporary Registration Number"));
							}
						}

						message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
						SendNotificationEmailIfNeeded(message.EM_LinkedObject as IMessageAttachee, message, isSuccess);
					}
				}
			}
			return isSuccess;
		}

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("D235A83C-0ACD-40D0-9C2D-6574F3E7B9DC", "E-Trade Temporary Registration Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("C34F8DE5-1778-4347-8C8C-CBF8C0AB815A", "E-Trade Temporary Registration Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRE;
		protected override ControllerID ControllerIDForemail => ControllerIDs.Customs.TR.ETrade;
	}
}

