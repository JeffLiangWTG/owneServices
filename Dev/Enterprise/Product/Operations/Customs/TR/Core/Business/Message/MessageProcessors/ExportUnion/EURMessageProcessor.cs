using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class EURMessageProcessor : ExportUnionMessageProcessorBase
	{
		public EURMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessageWithDeclaration(ExportUnionMessage incomingMessage, CusEntryHeader entryHeader)
		{
			var isSuccess = false;
			var messageText = incomingMessage.EM_MessageText;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var nodeList = new List<ZString>() { Constants.SegmentType.DeclarationResponse };
				var crypto = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageSuccess.Crypto);
				if (!string.IsNullOrEmpty(crypto))
				{
					isSuccess = true;
					var unionRecordNumber = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageSuccess.UnionRecordNumber);
					var tps = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageSuccess.Tps);
					var currentLoan = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageSuccess.CurrentLoan);
					var totalPayment = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageSuccess.TotalPayment);
					var paymentType = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageSuccess.PaymentType);

					CreateOrUpdatePayInfo(entryHeader, crypto, unionRecordNumber, tps, currentLoan);
					CreateOrUpdateCharges(entryHeader, totalPayment, paymentType);

					MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
					MessageDetailHtmlTableCreator.WriteRow(Res.GetString("8F9B1528-3E08-4F6E-B3E5-C78BB21D6BA3", "Current Loan:"), currentLoan);
					MessageDetailHtmlTableCreator.WriteRow(Res.GetString("0BE6B1CD-9300-458C-98CE-791176C3D164", "Total Payment:"), totalPayment);
					MessageDetailHtmlTableCreator.WriteRow(Res.GetString("ECCA40CD-F55C-4D9D-A9E4-B090A9D117F4", "Payment Type:"), paymentType);
					MessageDetailHtmlTableCreator.WriteRow(Res.GetString("B226F712-B303-496C-919B-DCAAFE121CC9", "Union Rec.Num.:"), unionRecordNumber);
					MessageDetailHtmlTableCreator.WriteRow(Res.GetString("AA9B8D9E-B6A4-41B8-A07E-081EB4899254", "Union Appr.Code:"), crypto);
					MessageDetailHtmlTableCreator.WriteRow(Res.GetString("1CC977C3-FFFE-4802-A189-84EDCF9A31CD", "TPS Reference:"), tps);

					var title = Res.GetString("21F6E8D8-5D3D-4A1B-ADD2-5B1DFDC2ADA2", "Export Union message for job {0} has been accepted.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
					SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);
				}
				else
				{
					nodeList.Add(Constants.SegmentType.Error);
					var errorCode = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageError.Code);
					if (!string.IsNullOrEmpty(errorCode))
					{
						MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_ErrorCodeAndDescription);
						var errorDesc = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, Constants.MessageError.Description);
						MessageDetailHtmlTableCreator.WriteRow(errorCode, errorDesc);
					}

					var title = Res.GetString("7FFF2BA3-A1CA-44A7-A7EB-BBD05741400A", "Export Union message for job {0} has been error.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
					SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);
				}
			}

			return isSuccess;
		}

		public static void CreateOrUpdatePayInfo(CusEntryHeader entryHeader, ZString crypto, ZString unionRecordNumber, ZString tps, ZString currentLoan)
		{
			var payInfo = entryHeader.ExportUnionPayInfo ?? entryHeader.EntryPayInfos.AddNew();
			payInfo.C9_PaymentReference = unionRecordNumber;
			payInfo.C9_IncomingPayResponseNo = crypto;
			payInfo.C9_PaymentParty = Constants.ExporterUnion;
			payInfo.C9_BankAccount = tps;
			payInfo.C9_PaymentAmount = ZDecimal.TryParse(currentLoan, out var currentLoanValue) ? currentLoanValue : ZDecimal.Zero;
		}

		public static void CreateOrUpdateCharges(CusEntryHeader entryHeader, ZString totalPayment, ZString paymentType)
		{
			var charges = entryHeader.ExportUnionCharges;
			if (charges == null)
			{
				charges = entryHeader.Charges.AddNew();
				charges.C1_ChargeType = Constants.ExporterUnion;
			}
			charges.C1_MethodOfPayment = paymentType;
			charges.C1_ChargeAmount = ZDecimal.TryParse(totalPayment, out var totalPaymentValue) ? totalPaymentValue : ZDecimal.Zero;
		}

		protected override void UpdateStatusCore(ExportUnionMessage message, bool isSuccess)
		{
			if (message.EM_LinkedObject is IMessageAttachee header)
			{
				header.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				header.CustomsStatus = isSuccess ? header.GetEntryStatus(TRMessageTypes.Codes.EUR) : EntryStatusTypeList.Codes.ERR;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
		public static class Constants
		{
			public const string ExporterUnion = "EXU";

			public static class SegmentType
			{
				public const string DeclarationResponse = "Declaration_Response";
				public const string Error = "Error";
			}

			public static class MessageSuccess
			{
				public const string UnionRecordNumber = "UnionRecordNumber";
				public const string Crypto = "Crypto";
				public const string Tps = "TPS";
				public const string CurrentLoan = "CurrentLoan";
				public const string TotalPayment = "TotalPayment";
				public const string PaymentType = "PaymentType";
			}

			public static class MessageError
			{
				public const string Code = "ErrorCode";
				public const string Description = "ErrorDescription";
			}
		}
	}
}
