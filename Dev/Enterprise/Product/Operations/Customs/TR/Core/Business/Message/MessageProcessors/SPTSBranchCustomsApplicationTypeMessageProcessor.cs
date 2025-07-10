using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.TR;

namespace Enterprise.Customs.TR.Business
{
	public class SPTSBranchCustomsApplicationTypeMessageProcessor : TRBranchCustomsApplicationTypeMessageProcessor<SPTSMessage>
	{
		public SPTSBranchCustomsApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is not a database field")]
		const string DateFormat = "dd/MM/yyyy HH:mm:ss";

		protected override string MessageFriendlyNameCore => Res.GetString("57944823-7F6C-4FD2-834B-0557102441E2", "TR SPTS Message Processor");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => (linkedObject as ICusInBondSPTSHeader)?.BH_GB ?? ZGuid.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		protected override bool ProcessMessageCore(SPTSMessage message)
		{
			var isSuccess = false;
			tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("B3119CD0-796C-4E37-BF19-E9CD7F5392ED", "SOAP Message") });

			if (message != null)
			{
				var header = message.EM_LinkedObject as ICusInBondSPTSHeader;

				if (header != null)
				{
					var temporaryQueryGUID = TRMessageHelper.GetNodeValue(message.EM_MessageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");
					if (!temporaryQueryGUID.IsEmpty)
					{
						message.EM_ApplicationReference = temporaryQueryGUID;
						var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
						if (originalMessage != null)
						{
							TRMessageSendingHelper.SendSPTSAutoReceiveResponseMessage(header, temporaryQueryGUID, originalMessage);
						}
						isSuccess = true;

						tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("C494820F-AA03-4EA8-A75D-4CBFA4310CC0", "Sent Message"), "" });
						tableCreator.WriteRow(Res.GetString("9B124E92-3277-4899-ABE2-8354CCF45A2C", "Query GUID:"), temporaryQueryGUID);
						var status = TRMessageHelper.GetNodeValue(message.EM_MessageText, "//x:Root/Response/Durum", "http://schemas.microsoft.com/BizTalk/2003/Any");
						tableCreator.WriteRow(Res.GetString("FDBD054C-7C8B-4C2C-BFDF-8A3401A4D076", "Status:"), status);

						var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
						message.EM_MessageInterpretation = CreateInterpretationContent(headerAttachee, isSuccess);
					}
					else
					{
						var shouldSendNotificationEmail = true;
						var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
						var messageText = message.EM_MessageText;

						ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/", (NoResString)"urn:schemas-microsoft-com:xml-diffgram-v1" };
						var xmlDataInXml = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:IslemSonucGetir2Response/B:IslemSonucGetir2Result/C:diffgram/NewDataSet/Sonuc/GidenXML", namespaceList);
						var errorMessageLists = TRMessageHelper.GetMultipleNodeValueList(xmlDataInXml, new List<ZString>() { "AktarmaSonucBilgisi", (NoResString)"Hatalar", "HataBilgisi" });

						if (errorMessageLists.Any())
						{
							tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("86F5503D-0DEE-405E-8A66-829004AED360", "Error Message") });
							headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Error;
							foreach (var item in errorMessageLists)
							{
								tableCreator.WriteRow(new string[] { item });
							}
						}
						else
						{
							tableCreator = new HtmlTableCreator("table", new string[] { Res.GetString("0CD3934D-01B2-4A38-A1EF-417C68F801BA", "Label"), Res.GetString("17D7F1F4-1761-4CA4-8A66-C5B03751E08A", "Value") });
							var successNoneLeftMessageInterpretation = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:IslemSonucGetir2Response/B:IslemSonucGetir2Result/C:diffgram/NewDataSet/Sonuc/GidenXML", namespaceList);
							var tescilNo = TRMessageHelper.GetNodeValue(successNoneLeftMessageInterpretation, "/x:AktarmaSonucBilgisi/x:TescilNo");

							if (!string.IsNullOrEmpty(tescilNo))
							{
								var tescilTarihi = TRMessageHelper.GetNodeValue(successNoneLeftMessageInterpretation, "/x:AktarmaSonucBilgisi/x:TescilTarihi");

								header.RegistrationNumber = tescilNo;
								ZDateTime registrationDate;
								if (ZDateTime.TryParseExact(tescilTarihi, out registrationDate, DateFormat))
								{
									header.RegistrationDate = new ZDateTime(registrationDate);
								}
								header.MessageStatus = TRMessageStatusCodeList.Codes.REG;
								headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.REG;

								tableCreator.WriteRow(Res.GetString("7DE85D7B-D856-4018-BD55-3DB827B67120", "Registration Number:"), header.RegistrationNumber);
								tableCreator.WriteRow(Res.GetString("8926CA96-4C4D-4993-967A-AB42C7E91084", "Issue Date:"), header.RegistrationDate);
								isSuccess = true;
							}
							else
							{
								var errorMessageText = TRMessageHelper.GetNodeValue(message.EM_MessageText, "/A:Envelope/A:Body", namespaceList);
								if (!string.IsNullOrEmpty(errorMessageText))
								{
									if (errorMessageText == responseErrorMessageForT1P)
									{
										var tspRcvMessage = TRInterchangeHelper.GetLastMessageWithApplicationReferenceByType(message, EDIMessage.Direction.Receive, TRMessageTypes.Codes.TSP);
										var origialMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
										TRMessageSendingHelper.SendSPTSAutoReceiveResponseMessage(header, tspRcvMessage.EM_ApplicationReference, origialMessage);
										shouldSendNotificationEmail = false;
									}
									else
									{
										header.MessageStatus = TRMessageStatusCodeList.Codes.Error;
										headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Error;
									}

									tableCreator.WriteRow(Res.GetString("57D4AC81-9D63-4B4A-AC59-137D5674A782", "Error Message:"), errorMessageText);
								}
								else
								{
									tableCreator.WriteRow(Res.GetString("6082BEEE-C94E-4F22-B08C-1AF584424E91", "Error Message: "), Res.GetString("8AC8486B-BA41-4BBC-9E99-E7764C5B62B7", "The message is invalid, please check the message text again."));
								}
							}
						}

						message.EM_MessageInterpretation = CreateInterpretationContent(headerAttachee, isSuccess);
						if (shouldSendNotificationEmail)
						{
							SendNotificationEmailIfNeeded(headerAttachee, message, isSuccess);
						}
					}
				}
			}
			return isSuccess;
		}

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, SPTSMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				if (messageAttacheeBO.MessageStatus == TRMessageStatusCodeList.Codes.REG)
				{
					title = Res.GetString("03A8F967-3B6D-46A9-B201-83D86C0D0CCC", "SPTS Message for job {0} has been cleared. For details please follow the Link to the Manifest.", messageAttacheeBO.JobReference);
				}
				else
				{
					title = Res.GetString("6CC275C1-7A27-483B-8EA4-16B5CA8DB0F7", "SPTS Message for job {0} sent. For details please follow the Link to the Manifest.", messageAttacheeBO.JobReference);
				}
			}
			else
			{
				title = Res.GetString("CF3B9E6F-AADD-45AD-A933-DB73AB3E0479", "SPTS Message for job {0} has been rejected. For details please follow the Link to the Manifest.", messageAttacheeBO.JobReference);
			}

			htmlBody.Append("<br /><br />");
			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, SPTSMessage message) => Res.GetString("9A23E92B-35F2-41C9-8E3E-ADA9007898CA", "SPTS Status Message");

		ZString CreateInterpretationContent(IMessageAttachee messageAttacheeBO, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();

			if (isSuccess)
			{
				if (messageAttacheeBO.MessageStatus == TRMessageStatusCodeList.Codes.REG)
				{
					title = Res.GetString("F319CBCC-E251-4F96-8DB5-22AB4560A4BD", "SPTS Message for job {0} has been cleared.", messageAttacheeBO.JobReference);
				}
				else
				{
					title = Res.GetString("BECC43C4-F06D-44FB-93E3-706E6063B837", "SPTS Message for job {0} sent.", messageAttacheeBO.JobReference);
				}
			}
			else
			{
				title = Res.GetString("605ED5EF-88F0-4F3D-8FE3-762CC15B9390", "SPTS Message for job {0} has been rejected.", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		const string responseErrorMessageForT1P = "Bu referans ile tescil alınmış yada işlemdedir.";
	}
}
