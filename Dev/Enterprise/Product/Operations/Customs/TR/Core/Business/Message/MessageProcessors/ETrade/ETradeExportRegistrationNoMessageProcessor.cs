using System.Collections.Generic;
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
	public class ETradeExportRegistrationNoMessageProcessor : ETradeMessageProcessor
	{
		public ETradeExportRegistrationNoMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("8BF92F3F-BA24-f428-B755-530BA5C6433B", "E-Trade Export Registration No Message");

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message) => Res.GetString("EA8152E5-ED37-4275-8B89-AE0D79095C61", "E-Trade Export Registration No Message Status");

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("D916A38E-16BD-4167-89B0-5504f87BC39C", "E-Trade Export Registration No Message for job {0} has been cleared. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("C5A6E6F9-287A-4EFF-1234-3F2D24266C2A", "E-Trade Export Registration No Message for job {0} has been rejected. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			htmlBody.Append("<br /><br />");
			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

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
					var temporaryQueryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");
					if (!temporaryQueryGUID.IsEmpty)
					{
						var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
						if (originalMessage != null)
						{
							TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(header, temporaryQueryGUID, originalMessage);
						}
						isSuccess = true;

						tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("0F958592-EEC1-463C-9283-E00CC2EA8AFD", "Label"), Res.GetString("0955561C-2571-4F49-AECA-2ED1C0C505E7", "Value") });
						tableCreator.WriteRow(Res.GetString("189CBEB2-E2EA-4CC5-AAA4-716B3358179B", "Query GUID:"), temporaryQueryGUID);
						message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
					}
					else
					{
						var parentNodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
						var errorMessage = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "HataAciklamasi");
						if (!string.IsNullOrEmpty(errorMessage))
						{
							tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("A4BE9547-2361-4E7E-91FC-E9CF2DC0A4FD", "Error Code"), Res.GetString("7609E380-2180-4245-AC0D-CBFEA03CFB87", "Error Message") });
							var errorMessages = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar" }, "HataKodu", "HataAciklamasi");
							foreach (var item in errorMessages)
							{
								tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
							}
						}
						else
						{
							tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("E6E32BF3-6C89-4912-AD8C-98CA9F412A34", "Label"), Res.GetString("F80508AE-A914-4DA2-A62A-7AFFF129104B", "Value") });

							var registrationNumber = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
							if (!registrationNumber.IsEmpty)
							{
								header.RegistrationNumber = registrationNumber;
								var registrationDate = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");
								if (ZDateTime.TryParseExact(registrationDate, out var registrationDateOut, "yyyy-MM-ddTHH:mm:ss"))
								{
									header.RegistrationDate = registrationDateOut;
								}
								tableCreator.WriteRow(Res.GetString("C8EE6AEC-233F-4B10-B080-1B39CE1BC8F9", "Export Registration No Number: "), header.RegistrationNumber);
								tableCreator.WriteRow(Res.GetString("C55B0DC8-83C5-408B-A309-769AD166D76E", "Export Registration No Date: "), header.RegistrationDate);
								isSuccess = true;
							}
							else
							{
								tableCreator.WriteRow(Res.GetString("1BBE9547-2361-4E7E-91FC-E9CF2D00A4FD", "Error Message: "), Res.GetString("28D3076E-ACEA-4AE9-8E9F-A7D0501D6A07", "The message missed the Export Registration No Number"));
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
				title = Res.GetString("E4025874-BA8F-43F0-BE0C-EF787459818E", "E-Trade Export Registration No Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("799BF734-A327-4765-8F83-7D0EBCF3A390", "E-Trade Export Registration No Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRS;
		protected override ControllerID ControllerIDForemail => ControllerIDs.Customs.TR.ETrade;
	}
}

