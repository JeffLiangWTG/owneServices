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
	public class ETradeImportDischargeListMessageProcessor : ETradeMessageProcessor
	{
		public ETradeImportDischargeListMessageProcessor(LoggingInformation loggingInformation) : base(loggingInformation)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("FB9D8174-4E13-4389-9017-4BF3388D988C", "E-Trade Import Discharge List Message");

		bool needToUpdateStatus = true;

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("DF5A0417-9F65-46E1-BCD6-7A773D655451", "E-Trade Import Discharge List Message for job {0} has been cleared. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("413F6B9E-DFE0-4F0B-859A-026CF9E8325C", "E-Trade Import Discharge List Message for job {0} has been rejected. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message) => Res.GetString("379ECB20-E55F-4B17-AD4C-BF0C489E9334", "E-Trade Import Discharge List Message Status");

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
					var parentNodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "OutputMessage", "Record", "Sonuc" };
					var temporaryQueryGUID = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "GUID");
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
						parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
						var errorMessage = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "HataAciklamasi");
						if (!string.IsNullOrEmpty(errorMessage))
						{
							tableCreator = new HtmlTableCreator("table", SoapMessageTextHelper.RowHeaders_ErrorCodeAndDescription);
							var errorMessages = TRMessageHelper.GetMultipleNodeValueList(messageText, parentNodeList, "HataKodu", "HataAciklamasi");
							foreach (var item in errorMessages)
							{
								tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
							}
						}
						else
						{
							var parentNodeListError = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc", "Hatalar", "Hata", "HataAciklamasi" };
							ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
							var xmlData = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
							var errorMessages = TRMessageHelper.GetMultipleNodeValueList(xmlData, parentNodeListError);

							if (errorMessages.Any())
							{
								var dischargeListNo = TRMessageHelper.GetAlreadyRegisteredDischargeListNo(xmlData);
								var tableHeader = SoapMessageTextHelper.TableHeaderWithErrorMessage;

								if (!dischargeListNo.IsEmpty)
								{
									header.DischargeRecordNo = dischargeListNo;
									isSuccess = true;
									needToUpdateStatus = false;
									tableHeader = string.Join(string.Empty, SoapMessageTextHelper.TableHeaderSoapMessage);
								}

								tableCreator = new HtmlTableCreator("table", new string[] { tableHeader });
								foreach (var item in errorMessages)
								{
									tableCreator.WriteRow(new string[] { item });
								}
							}
							else
							{
								tableCreator = new HtmlTableCreator("table", SoapMessageTextHelper.RowHeaders_Labelvalue);

								var dischargeRecordNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
								if (!dischargeRecordNo.IsEmpty)
								{
									header.DischargeRecordNo = dischargeRecordNo;
									var dischargeRecordNoDate = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");
									ZDateTime registrationDateOut;
									if (ZDateTime.TryParseExact(dischargeRecordNoDate, out registrationDateOut, "yyyy-MM-ddTHH:mm:ss"))
									{
										header.DischargeRecordNoDate = new ZDateTime(dischargeRecordNoDate);
									}
									tableCreator.WriteRow(Res.GetString("623572C7-91EC-4AAC-902E-A03DE7F6A8BF", "Import Discharge List Number: "), header.DischargeRecordNo);
									tableCreator.WriteRow(Res.GetString("25FC8A2B-24FA-4946-9114-52B05AB85309", "Import Discharge List Date: "), header.DischargeRecordNoDate);
									isSuccess = true;
								}
								else
								{
									var incomingTRDStatusMessage = SoapMessageTextHelper.GetOutputMessageStatus(messageText).ToString();
									var errorMessageCaption = Res.GetString("9D86C385-2432-4DDF-BF12-B41C2EC2C53A", "Error Message: ");

									if (incomingTRDStatusMessage == SoapMessageTextHelper.Constants.OutputMessage.ProcessHasStarted)
									{
										needToUpdateStatus = false;
										isSuccess = true;
									}
									else
									{
										tableCreator.WriteRow(errorMessageCaption,
											incomingTRDStatusMessage == SoapMessageTextHelper.Constants.OutputMessage.SigningCardAndUserInfoIncompatible
											? incomingTRDStatusMessage
											: Res.GetString("63CA97E1-4B2D-44DB-835D-AC9CEF27BE85", "The message missed the Import Discharge List Number"));
									}
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

		protected override void UpdateStatusCore(ETradeEDIMessage message, bool isSuccess)
		{
			if (needToUpdateStatus)
			{
				base.UpdateStatusCore(message, isSuccess);
			}
		}

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();

			if (isSuccess)
			{
				title = Res.GetString("7F954335-B1D2-4F98-996F-24C5A6D12215", "E-Trade Import Discharge List Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("6A6B22C9-4964-4B02-90FB-7B7D1034F59C", "E-Trade Import Discharge List Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRD;

		protected override ControllerID ControllerIDForemail => ControllerIDs.Customs.TR.ETrade;
	}
}

