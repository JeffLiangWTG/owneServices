using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.TR.MessageDefinitions.NCTS.CC015B_RES;
using CargoWise.Customs.TR.MessageDefinitions.NCTS.CTRINFDEP;
using CargoWise.Customs.TR.MessageDefinitions.NCTS.GUAINF;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSDownloadMessageByIndexResponseMessageProcessor : NCTSMessageProcessor
	{
		public NCTSDownloadMessageByIndexResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => ResString.GetMultilingualString("449B5FD1-ADE0-4095-8D5B-8B683168C982", "NCTS Download Message By Index Response Message Processor");

		protected override bool ProcessMessageCore(NCTSMessage message)
		{
			if (message == null)
			{
				return false;
			}

			if (message.EM_LinkedObject is not Integration.Customs.TR.ICusInBondHeader header
				|| message.EM_LinkedObject is not IMessageAttachee headerAttachee)
			{
				return false;
			}

			var isSuccess = false;
			var messageText = message.EM_MessageText;

			var processingResult = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, TRMessageTypes.Codes.T1N);
			TRInterchangeHelper.SetMessageOwnerByMainMessage(message);
			if (processingResult == Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS)
			{
				isSuccess = ProcessCLSMessage(message, messageText, header, headerAttachee);
			}
			else if (processingResult == Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR)
			{
				tableCreator = new HtmlTableCreator();
				tableCreator.WriteRow(ResString.GetMultilingualString("85E2FA4D-99A9-47F5-B3C3-BC043E5BC4DF", "Error Message:"), ResString.GetMultilingualString("700321FF-4DBE-4503-943D-E3430C1EBA4F", "The message text sent back from Customs is empty, please try to resend original message"));
				SendNotificationEmailIfNeeded(headerAttachee, message, false);
				headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Error;
				header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
			}
			else if (processingResult == Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN)
			{
				isSuccess = true;
			}

			return isSuccess;
		}

		bool ProcessCLSMessage(NCTSMessage message,
			ZString messageText,
			Integration.Customs.TR.ICusInBondHeader header,
			IMessageAttachee headerAttachee)
		{
			var isSuccess = false;
			var lrnNumber = ZString.Empty;
			var lrnDate = ZString.Empty;
			var mrnNumber = ZString.Empty;
			var mrnDate = ZString.Empty;
			var messageInterpretation = new List<string>();
			var errorCode = TRMessageHelper.GetXmlNode(messageText, new ZString[] { (NoResString)"Envelope", (NoResString)"Body", "downloadmessagebyindexResponse", (NoResString)"return", (NoResString)"error" });
			var node = TRMessageHelper.GetXmlNode(messageText, new ZString[] { (NoResString)"Envelope", (NoResString)"Body", "downloadmessagebyindexResponse", (NoResString)"return", "msgContent" });
			node.InnerXml = TRMessageHelper.DecodeHtmlIfNecessary(node.InnerXml);
			var content = node?.FirstChild;
			if (content != null)
			{
				switch (content.Name)
				{
					case NCTSP4PhaseDefinitionList.Codes.CC060A:
						messageInterpretation.Add(NCTSMovementHeaderCustomsStatusList.Descriptions.CTR);
						header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.CTR;

						mrnNumber = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC060A", "HEAHEA" }, "DocNumHEA5");
						mrnDate = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC060A", "HEAHEA" }, "DatOfConNotHEA148");
						AssignRegisterInfo(header, lrnNumber, lrnDate, mrnNumber, mrnDate);

						isSuccess = true;
						break;
					case NCTSP4PhaseDefinitionList.Codes.CC028A:
						messageInterpretation.Add(NCTSMovementHeaderCustomsStatusList.Descriptions.MRN);
						header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.MRN;

						lrnNumber = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC028A", "HEAHEA" }, "RefNumHEA4");
						mrnNumber = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC028A", "HEAHEA" }, "DocNumHEA5");
						mrnDate = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC028A", "HEAHEA" }, "AccDatHEA158");
						AssignRegisterInfo(header, lrnNumber, lrnDate, mrnNumber, mrnDate);

						isSuccess = true;
						break;
					case NCTSP4PhaseDefinitionList.Codes.CC029B:
						messageInterpretation.Add(NCTSMovementHeaderCustomsStatusList.Descriptions.REL);
						header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.REL;
												
						lrnNumber = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC029B", "HEAHEA" }, "RefNumHEA4");
						mrnNumber = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC029B", "HEAHEA" }, "DocNumHEA5");
						mrnDate = TRMessageHelper.GetNodeValue(content.OuterXml, new List<ZString>() { "CC029B", "HEAHEA" }, "AccDatHEA158");
						AssignRegisterInfo(header, lrnNumber, lrnDate, mrnNumber, mrnDate);

						isSuccess = true;
						break;
					case NCTSP4PhaseDefinitionList.Codes.CC015B_RES:
						{
							var outerXml = content.OuterXml;
							if (outerXml.Contains("CC015B_RES"))
							{
								var messageObjectRES = XmlObjectSerializer.Deserialize<TCc015BRes>(outerXml);
								lrnNumber = messageObjectRES.Lrn;
								lrnDate = messageObjectRES.Tmstmp != null ? messageObjectRES.Tmstmp.Replace("-", "/") : ZString.Empty;
								if (!lrnNumber.IsEmpty)
								{
									header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.LRN;
									headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.REG;
									messageInterpretation.Add(ResString.GetMultilingualString("2474D654-C445-4033-891F-D01A6261850D", "Registration No: {0}", lrnNumber));
									AssignRegisterInfo(header, lrnNumber, lrnDate, mrnNumber, mrnDate);
								}
								else
								{
									header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
									var tecNodes = TRMessageHelper.GetNodeValues(outerXml, new List<ZString>() { "CC015B_RES", "ERR" }, "TEC");
									if (tecNodes != null)
									{
										messageInterpretation.Add(string.Join(System.Environment.NewLine, tecNodes));
									}

									var procNodes = TRMessageHelper.GetNodeValues(outerXml, new List<ZString>() { "CC015B_RES", "ERR" }, "PROC");
									if (procNodes != null)
									{
										messageInterpretation.Add(string.Join(System.Environment.NewLine, procNodes));
									}

									var valMainNodes = TRMessageHelper.GetNodeValues(outerXml, new List<ZString>() { "CC015B_RES", "ERR", "VAL" }, "VAL_MAIN");
									if (valMainNodes != null)
									{
										messageInterpretation.Add(string.Join(System.Environment.NewLine, valMainNodes));
									}

									var valGuaNodes = TRMessageHelper.GetNodeValues(outerXml, new List<ZString>() { "CC015B_RES", "ERR", "VAL" }, "VAL_GUA");
									if (valGuaNodes != null)
									{
										messageInterpretation.Add(string.Join(System.Environment.NewLine, valGuaNodes));
									}
								}
							}

							isSuccess = true;
						}
						break;
					case NCTSP4PhaseDefinitionList.Codes.CTRINFDEP:
						{
							var messageObject = XmlObjectSerializer.Deserialize<TCtrinfdep>(content.OuterXml);
							messageInterpretation.Add(ResString.GetMultilingualString("58533280-7E2C-47AA-89B0-7BF32B5A84B1", "Inspection Clerk: {0}", messageObject.Muamem)
													  + System.Environment.NewLine + ResString.GetMultilingualString("2D2200C8-F4CE-47EA-AE69-873AA024DDD8", "Inspection Line: {0}", messageObject.Muatur));
							isSuccess = true;
						}
						break;
					case NCTSP4PhaseDefinitionList.Codes.GUAINF:
						{
							var messageObject = XmlObjectSerializer.Deserialize<TGuainf>(content.OuterXml);
							var guagua = messageObject.Guagua.FirstOrDefault()?.Guarefref;
							messageInterpretation.Add(guagua == null ? ResString.GetMultilingualString("894D9443-B835-487F-A9C8-5AC7E4AA4A03", "Guarantee Amount information not found") : ResString.GetMultilingualString("8FE27E49-72DA-4B94-8BD3-AC8851F27445", "Guarantee Amount: {0} {1}", guagua.AmoConRef7, guagua.CurRef8));
							isSuccess = true;
						}
						break;
				}
			}

			var messageCode = errorCode == null ? string.Empty : errorCode.FirstChild.OuterXml;
			if (isSuccess)
			{
				tableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderT2NRecieveFields);
				var msgDesc = message.Factory.GetErrorDescByCode(messageCode);
				tableCreator.WriteRow(messageCode, msgDesc, string.Join(System.Environment.NewLine, messageInterpretation));
				message.EM_MessageInterpretation = CreateInterpretationContent(headerAttachee, true);
				SendNotificationEmailIfNeeded(headerAttachee, message, true);
			}
			else
			{
				tableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderT2NRecieveFields);
				var msgDesc = message.Factory.GetErrorDescByCode(messageCode);
				tableCreator.WriteRow(messageCode, msgDesc, ResString.GetMultilingualString("5F41B4BF-DB98-4644-AA53-880A451D6A06", "Processing failed, unknown XML format"));
				message.EM_MessageInterpretation = CreateInterpretationContent(headerAttachee, true);
				SendNotificationEmailIfNeeded(headerAttachee, message, false);
				headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Error;
				header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
			}

			return isSuccess;
		}

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, NCTSMessage message, bool isSuccess)
		{
			var htmlBody = new ZStringBuilder();
			htmlBody.Append(ResString.GetMultilingualString("85E45FE8-A4BC-4BE7-B1EC-249602A3B88D", "NCTS Download Message By Index Response error for job {0}.", messageAttacheeBO.JobReference));
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, NCTSMessage message) => ResString.GetMultilingualString("5BD06364-7B4B-49EB-9177-C462FBB39F65", "NCTS Download Message By Index Response");

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			ZString title;

			if (isSuccess)
			{
				title = ResString.GetMultilingualString("1B9E4D06-35DE-4CBF-914B-EA7D5094FD2D", "NCTS Download Message for job {0} By Index Response", header.JobReference);
			}
			else
			{
				title = ResString.GetMultilingualString("994BC181-0757-414A-BE0E-A4C70786CE1F", "NCTS Download Message for job {0} By Index Response Error", header.JobReference);
			}

			var htmlBody = string.Empty;
			if (tableCreator != null)
			{
				htmlBody = tableCreator.ToHtml().Replace("<th>", @"<th class=""th"">");
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody);
		}

		void AssignRegisterInfo(Integration.Customs.TR.ICusInBondHeader header, ZString lrnNumber, ZString lrnDate, ZString mrnNumber, ZString mrnDate)
		{
			if (!lrnNumber.IsEmpty && header.LrnRegistrationNumber.IsEmpty)
			{
				header.LrnRegistrationNumber = lrnNumber;
			}

			ZDateTime lrnDateOut;
			if (!lrnDate.IsEmpty && ZDateTime.TryParseExact(lrnDate, out lrnDateOut, "dd/MMM/yy"))
			{
				header.LrnRegistrationDate = new ZDateTime(lrnDateOut);
			}

			if (!mrnNumber.IsEmpty && header.ArrivalMrnFromUser.IsEmpty)
			{
				header.ArrivalMrnFromUser = mrnNumber;
			}

			ZDateTime mrnDateOut;
			if (!mrnDate.IsEmpty && ZDateTime.TryParseExact(mrnDate, out mrnDateOut, "yyyyMMdd"))
			{
				header.MrnIssueDateFromUser = new ZDateTime(mrnDateOut);
			}
		}
	}
}
