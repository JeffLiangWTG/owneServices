using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeImportComplementaryDeclarationProcessor : ETradeMessageProcessor
	{
		public ETradeImportComplementaryDeclarationProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("93495E9E-78F3-4365-9CC6-C915EDED205D", "E-Trade Import Send for Complementary Declaration Message");

		bool needToUpdateStatus = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded, datetimeformat")]
		protected override bool ProcessMessageCore(ETradeEDIMessage message)
		{
			var isSuccess = false;
			if (message != null)
			{
				var header = message.EM_LinkedObject as AsycudaManifestHeader;
				if (header != null)
				{
					var messageText = message.EM_MessageText;
					var temporaryQueryGUID = SoapMessageTextHelper.GetOutputMessageGuid(messageText);
					if (!temporaryQueryGUID.IsEmpty)
					{
						var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
						if (originalMessage != null)
						{
							TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(header, temporaryQueryGUID, originalMessage);
						}
						isSuccess = true;

						tableCreator = new HtmlTableCreator("table", SoapMessageTextHelper.RowHeaders_Labelvalue);
						tableCreator.WriteRow(Res.GetString("189CBEB2-E2EA-4CC5-AAA4-716B3358179B", "Query GUID:"), temporaryQueryGUID);
						message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
					}
					else
					{
						var parentNodeListError = new List<ZString>() { "ETicaretSoapOut", (NoResString)"Record", (NoResString)"Sonuc", "Hatalar", "Hata", "HataAciklamasi" };
						ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
						var xmlData = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
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
							var isError = !string.IsNullOrEmpty(TRMessageHelper.GetNodeValue(messageText, new List<ZString>() { "xml", "Hatalar", "Hata" }, "HataKodu"));
							if (isError)
							{
								tableCreator = new HtmlTableCreator("table", SoapMessageTextHelper.RowHeaders_ErrorCodeAndDescription);

								var errorMessageText = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { "xml", "Hatalar", "Hata" }, "HataKodu", "HataAciklamasi");
								foreach (var item in errorMessageText)
								{
									tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
								}
							}
							else
							{
								tableCreator = new HtmlTableCreator("table", SoapMessageTextHelper.RowHeaders_Labelvalue);
								var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
								var registrationNumber = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
								if (!registrationNumber.IsEmpty)
								{
									if (registrationNumber.Length <= 38)
									{
										header.ClosureNo = registrationNumber;
										var registrationDate = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi").SubstringSafe(0, 19);
										if (ZDateTime.TryParseExact(registrationDate, out var registrationDateOut, "yyyy-MM-ddTHH:mm:ss"))
										{
											header.ClosureNoDate = registrationDateOut;
										}
									}
									tableCreator.WriteRow(Res.GetString("3A392DD7-9015-4DB5-9E45-98148D24357B", "Complementary Declaration Registration Number: "), header.ClosureNo);
									tableCreator.WriteRow(Res.GetString("0110D7BE-4BBF-4FF3-8799-B715598AB084", "Complementary Declaration Registration Date: "), header.ClosureNoDate);
									isSuccess = true;
								}
								else
								{
									var incomingStatusMessage = SoapMessageTextHelper.GetOutputMessageStatus(messageText).ToString();
									var messageCaption = Res.GetString("4CD5941E-162C-4E25-9685-870629BB5F15", "Error Message: ");

									if (incomingStatusMessage == SoapMessageTextHelper.Constants.OutputMessage.ProcessHasStarted)
									{
										needToUpdateStatus = false;
										isSuccess = true;
										messageCaption = Res.GetString("0359035F-03E4-46B2-A6C7-1520F6747240", "Message: ");
									}

									tableCreator.WriteRow(messageCaption,
									!string.IsNullOrEmpty(incomingStatusMessage)
									? incomingStatusMessage
									: Res.GetString("C567F8FC-4CB3-4411-806E-6B71AB7E42B3", "The message missed the Complementary Declaration"));
								}
							}
						}

						message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
						SendNotificationEmailIfNeeded(message.EM_LinkedObject as IMessageAttachee, message, isSuccess);
					}
				}
			}
			return isSuccess;
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TCD;

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("444DD2B4-B2D1-4D15-B1B0-00C2EF4DFA32", "E-Trade Import Send for Complementary Declaration Message for job {0} has been cleared. For details please follow the Link to the E-Trade.", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("A9167D01-713E-4F62-B5E0-3A627BDF9C2D", "E-Trade Import Send for Complementary Declaration Message for job {0} has been rejected. For details please follow the Link to the E-Trade.", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message)
		{
			return Res.GetString("36993A9E-0BA0-46E7-9C68-E0C5E0366CFB", "E-Trade Import Send for Complementary Declaration Message Status");
		}

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();

			if (isSuccess)
			{
				title = Res.GetString("C12C260C-69EE-48ED-B536-F2CF6FA69C2E", "E-Trade Import Send for Complementary Declaration Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("58AA8FCC-F8A8-4984-8436-36BD15877BCA", "E-Trade Import Send for Complementary Declaration Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override void UpdateStatusCore(ETradeEDIMessage message, bool isSuccess)
		{
			if (needToUpdateStatus)
			{
				base.UpdateStatusCore(message, isSuccess);
			}
		}
	}
}
