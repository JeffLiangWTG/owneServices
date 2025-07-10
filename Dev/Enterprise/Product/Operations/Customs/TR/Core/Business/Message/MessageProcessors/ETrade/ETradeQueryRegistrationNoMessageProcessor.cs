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
	public class ETradeQueryRegistrationNoMessageProcessor : ETradeMessageProcessor
	{
		public ETradeQueryRegistrationNoMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("9FEBACB1-DF23-47C3-B594-3BA1589FCE5C", "E-Trade Query Registration No Message");

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("BF3656A9-F023-4A79-9410-A0F33D433D6F", "E-Trade Query Registration No Message for job {0} has been cleared. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("A3E5F5EF-BBC5-4829-AE60-8ABC888D6601", "E-Trade Query Registration No Message for job {0} has been rejected. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message) => Res.GetString("4C758F88-1D74-498A-B99A-4463755C8BEA", "E-Trade Query Registration No Message Status");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		protected override bool ProcessMessageCore(ETradeEDIMessage message)
		{
			var isSuccess = false;
			if (message != null)
			{
				var messageText = message.EM_MessageText;

				var parentNodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "GeciciTescildenTescilNoSorgulaResponse" };
				var resultValue = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "GeciciTescildenTescilNoSorgulaResult");
				var header = message.EM_LinkedObject as AsycudaManifestHeader;
				var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
				if (header != null)
				{
					if (string.IsNullOrEmpty(resultValue))
					{
						tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("9383B730-CF3C-4BE3-B006-D2C6A8C18D82", "Error Code"), Res.GetString("5881215C-0293-4A15-9BD1-C038FEB9CBC4", "Error Message") });

						var errorMessages = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { "xml", "Hatalar", "Hata" }, "HataKodu", "HataAciklamasi");
						foreach (var item in errorMessages)
						{
							tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
						}
					}
					else
					{
						tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("E6E32BF3-6C89-4912-AD8C-98CA9F412A34", "Label"), Res.GetString("F80508AE-A914-4DA2-A62A-7AFFF129104B", "Value") });
						if (!resultValue.IsEmpty)
						{
							var prefixNumber = resultValue.SubstringSafe(0, 2);
							if (prefixNumber.Length == 2 && prefixNumber.IsNumbersOnlyOrEmpty)
							{
								header.RegistrationNumber = resultValue;
								tableCreator.WriteRow(Res.GetString("4C3481A1-C11F-4736-AC64-2E29B1636115", "Query Registration Number: "), header.RegistrationNumber);
								isSuccess = true;
							}
							else
							{
								shouldUpdateMessageMode = false;
								tableCreator.WriteRow(Res.GetString("E7EC270D-B2C1-46A7-B71A-6C0DA070BC86", "Error Message: "), resultValue);
							}
						}
						else
						{
							tableCreator.WriteRow(Res.GetString("25E5B580-4B31-4ECE-8A18-0F4C508CD6E4", "Error Message: "), Res.GetString("9D9F7246-30C8-4911-85F5-BAA748D5F2C9", "The message missed the Query Registration Number"));
						}
					}

					message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
					SendNotificationEmailIfNeeded(headerAttachee, message, isSuccess);
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
				title = Res.GetString("900063F8-73B8-443C-81D1-813DCF337A46", "E-Trade Query Registration No Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("EAE84A5E-4A35-4350-B2D7-9C30D5E55D24", "E-Trade Query Registration No Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRQ;
		protected override ControllerID ControllerIDForemail => ControllerIDs.Customs.TR.ETrade;
	}
}
