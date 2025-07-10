using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using IAsycudaBill = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaBill;
using IAsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeQueryRemainingBillsForImportMessageProcessor : ETradeMessageProcessor
	{
		public ETradeQueryRemainingBillsForImportMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("1A63DAAD-9FF4-4138-B439-ECDA1DCF6AB9", "E-Trade Query Remaining Bills for Import Declaration Message");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		protected override bool ProcessMessageCore(ETradeEDIMessage message)
		{
			var isSuccess = false;
			if (message != null)
			{
				var messageText = message.EM_MessageText;
				var header = message.EM_LinkedObject as IAsycudaManifestHeader;
				var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
				tableCreator = new HtmlTableCreator();

				if (header != null)
				{
					var errorMessageTextList = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { (NoResString)"xml", (NoResString)"Hatalar", "Hata" }, "HataKodu", "HataAciklamasi");
					if (errorMessageTextList.Any())
					{
						tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("7CBFD6C1-52DA-43B3-A78B-8067DFAFCA2E", "Error Code"), Res.GetString("BD10A56A-A3FE-4479-A9DA-7EDD7057F0CD", "Error Message") });
						foreach (var item in errorMessageTextList)
						{
							tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
						}
					}
					else
					{
						var commonMessageText = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { "Envelope", "Body", "BeyannameDurumSorgulaResponse", "BeyannameDurumSorgulaResult", "diffgram", "NewDataSet", "Hata" }, "Hata_x0020_Aciklamasi", "").FirstOrDefault();
						if (commonMessageText != null)
						{
							tableCreator.WriteRow(Res.GetString("EB82ABF0-4784-4D65-BD5A-6FBD58C26099", "Error Message"), commonMessageText.Item1);
						}
						else
						{
							var successNoneLeftMessageText = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { "Sonuc", "Hatalar", "Hata" }, "HataAciklamasi", "").FirstOrDefault();
							if (successNoneLeftMessageText != null)
							{
								tableCreator.WriteRow(Res.GetString("BD518292-4724-4C34-9659-D190E8854B5F", "Remaining Bills Information"), successNoneLeftMessageText.Item1);
							}
							else
							{
								var successMessageTextList = TRMessageHelper.GetMultipleNodeValueListForQueryRemainingBills(messageText);
								if (successMessageTextList.Any())
								{
									isSuccess = true;

									tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("E873702D-3D3F-41F8-9FDC-879B9AB7CEC6", "Remaining Bills Number"), Res.GetString("C92BB7D8-3AA1-4C71-BB04-82BE01EE06C5", "Remaining Bills Reason") });
									foreach (var item in successMessageTextList)
									{
										var bill = header.Bills.OfType<IAsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == item.Item1);
										if (bill != null)
										{
											bill.ABL_BillStatus = TRMessageStatusCodeList.Codes.REM;
											tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
										}
										else
										{
											tableCreator.WriteRow(new string[] { item.Item1, Res.GetString("C2ACBF72-6A12-44BA-A966-632DC53A1F27", "This bill could not be found") });
										}
									}
								}
								else
								{
									ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
									var messageContent = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:AyrilanTasimaSenediSorgulaResponse", namespaceList);
									var messageList = TRMessageHelper.GetMultipleNodeValueList(messageContent, new List<ZString>() { "Sonuc", "Hatalar", "Hata" });
									var errorMesasge = Res.GetString("2A07DF55-53A3-4FF7-9711-C3328530450B", "Error Message");

									if (messageList.Any())
									{
										if (messageList.Contains(reservedShippingNoteMessage))
										{
											isSuccess = true;
											tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("F84E782E-9C3A-4B7E-9504-FDCF19FB1523", "Message") });
											foreach (var item in messageList)
											{
												tableCreator.WriteRow(new string[] { item });
											}
										}
										else
										{
											tableCreator = new HtmlTableCreator("table", new string[] { errorMesasge });
											foreach (var item in messageList)
											{
												tableCreator.WriteRow(new string[] { item });
											}
										}
									}
									else
									{
										tableCreator = new HtmlTableCreator(new string[] { Res.GetString("7CBFD6C1-52DA-43B3-A78B-8067DFAFCA2E", "Error Code"), errorMesasge });
										tableCreator.WriteRow(Res.GetString("5B5CA0CF-4E3B-4DF4-9295-5CC8A5AB36EA", "Error Message: "), Res.GetString("949EB5AE-9942-44D0-812E-45D131738DD5", "The message is invalid, please check the message text again!"));
									}
								}
							}
						}
					}

					message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
					SendNotificationEmailIfNeeded(headerAttachee, message, isSuccess);
				}
			}
			return isSuccess;
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRB;

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("3BA959DB-A4F3-401B-813B-9D31152D53F0", "E-Trade Query Remaining Bills for Import Declaration Message for job {0} has been cleared. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("F5C16471-1DFB-4331-99E8-7EBE2D91F9B7", "E-Trade Query Remaining Bills for Import Declaration Message for job {0} has been rejected. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message)
		{
			return Res.GetString("E86874E8-ABDD-4388-A492-B76477F94668", "E-Trade Query Remaining Bills for Import Declaration Message Status");
		}

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();

			if (isSuccess)
			{
				title = Res.GetString("CED9FC4A-3F67-434F-A2EA-C5E0334C755B", "E-Trade Query Remaining Bills for Import Declaration Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("3FC14BEA-B52E-47F9-82BC-BAF0C88BA51D", "E-Trade Query Remaining Bills for Import Declaration Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised string")]
		const string reservedShippingNoteMessage = "Ayrılmış taşıma senedi bulunamadı!";
	}
}
