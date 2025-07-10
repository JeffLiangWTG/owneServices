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
	public class ETradeQueryInspectionLineMessageProcessor : ETradeMessageProcessor
	{
		public ETradeQueryInspectionLineMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
			tableCreator = new HtmlTableCreator();
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("740A76A8-16E4-447A-994D-4D976C7D47B7", "E-Trade Inspection Line Message");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		protected override bool ProcessMessageCore(ETradeEDIMessage message)
		{
			var errorCode = Res.GetString("F99B1CB0-B439-476B-B19A-5B5A1DD700A6", "Error Code");
			var errorMessage = Res.GetString("61E43533-E552-4B44-A97B-74B00E765381", "Error Message");

			var isSuccess = false;

			if (message != null)
			{
				var messageText = message.EM_MessageText;
				var header = message.EM_LinkedObject as IAsycudaManifestHeader;
				var headerAttachee = message.EM_LinkedObject as IMessageAttachee;

				if (header != null)
				{
					var errorMessageTextList = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { (NoResString)"xml", (NoResString)"Hatalar", "Hata" }, "HataKodu", "HataAciklamasi");
					if (errorMessageTextList.Any())
					{
						tableCreator = new HtmlTableCreator("table", new string[] { errorCode, errorMessage });
						isSuccess = false;
						foreach (var item in errorMessageTextList)
						{
							tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
						}
					}
					else
					{
						var successMessageTextList = TRMessageHelper.GetChildNodesFromElement(messageText, "Table1", "TASIMASENEDINO", "HAT", "ONAYDURUMU");
						if (successMessageTextList.Any())
						{
							isSuccess = true;

							tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("7C28036A-BE47-4CAA-BA01-C584708D516E", "Bill Number"), Res.GetString("0BDDF028-AC68-4D9C-8A08-3B7D66510810", "Inspection Line"), Res.GetString("6A05F6DA-6103-4EE4-A4EB-046800D2A9E5", "Approval Status") });
							foreach (var item in successMessageTextList)
							{
								var billNumber = item[0];
								var billStatus = item[1];
								var cargoStatus = item[2].Replace("Ş", "S");

								var bill = header.Bills.OfType<IAsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == billNumber);
								if (bill != null)
								{
									bill.ABL_BillStatus = billStatus == "SARI" ? BillStatusList.Codes.YEL : billStatus == "KIRMIZI" ? BillStatusList.Codes.RED : string.Empty;
									bill.ABL_CargoStatus = cargoStatus == ApprovalStatusCodeList.Codes.APPROVED ? TRETradeCargoStatusList.Codes.APP : cargoStatus == ApprovalStatusCodeList.Codes.NOTAPPROVED ? TRETradeCargoStatusList.Codes.NAP : string.Empty;
									tableCreator.WriteRow(item);
								}
								else
								{
									tableCreator.WriteRow(new string[] { item[0], Res.GetString("C2ACBF72-6A12-44BA-A966-632DC53A1F27", "This bill could not be found"), Res.GetString("56204D27-CE43-49FC-9D22-7E17C4C7ABF9", "This bill could not be found") });
								}
							}
						}

						else
						{
							isSuccess = false;
							tableCreator = new HtmlTableCreator(new string[] { errorCode, errorMessage });
							tableCreator.WriteRow(Res.GetString("99AE336E-1B22-47CD-8375-9E977583F5E9", "Error Message: "), Res.GetString("25A3B365-A326-48CD-94C8-6243C638C9B3", "The message is invalid, please check the message text again!"));
						}
					}

					message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
					SendNotificationEmailIfNeeded(headerAttachee, message, isSuccess);
				}
			}
			return isSuccess;
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRL;

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("C6397643-1CEE-4929-AD40-CC8E95825EF8", "E-Trade Inspection Line Message for job {0} has been cleared. For details please follow the Link to the E-Trade.", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("83A98343-7211-4948-8266-527F8EC87E3A", "E-Trade Inspection Line Message for job {0} has been rejected. For details please follow the Link to the E-Trade.", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message)
		{
			return Res.GetString("A359EA69-9AC8-4AD7-910B-4975C7E9DEE0", "E-Trade Inspection Line Message Status");
		}

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();

			if (isSuccess)
			{
				title = Res.GetString("BB0D7C7D-DC4F-4721-BE63-C5A808BA738B", "E-Trade Inspection Line Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("79046196-0EBB-4EE1-A71B-7849DE96F0BA", "E-Trade Inspection Line Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}
	}
}
