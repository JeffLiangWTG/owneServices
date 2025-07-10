using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.UY.Manifest.Business
{
	internal static class UYMessageHelper
	{
		internal static AsycudaBill LookForAsycudaBill(IResponse response, AsycudaManifestHeader manifestHeader)
		{
			AsycudaBill bill = null;

			var referenceBill = response.References?.Where(x => x.Code == UYMessageConstants.HBLNumber).FirstOrDefault();
			if (referenceBill != null)
			{
				bill = manifestHeader?.Bills?.OfType<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == referenceBill.Value.Trim());
			}

			if (bill == null)
			{
				var nroSequence = response.References?.Where(x => x.Code == UYMessageConstants.HBLSequenceNumber).FirstOrDefault();
				bill = manifestHeader?.Bills?.OfType<AsycudaBill>().FirstOrDefault(x => x.CustomsEntryNumber.ToString() == nroSequence.Value.Trim());
			}
			return bill;
		}

		internal static void UpdateBOWhenBillNotFound(AsycudaManifestHeader header, IEnumerable<AsycudaBill> bills)
		{
			var hasOriginal = false;

			foreach (var item in bills)
			{
				item.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;

				if (item.ABL_BillStatus != CustomsStatusList.Codes.ACP)
				{
					item.ABL_BillStatus = CustomsStatusList.Codes.ERR;
					hasOriginal = true;
				}
			}

			header.AMA_MessageStatus = hasOriginal ? (ZString)MessageStatusCodeList.Codes.Error : (ZString)MessageStatusCodeList.Codes.Accepted;
		}

		internal static ZString MessageInterpretation(IReadOnlyCollection<IResponse> responses)
		{
			var messageText = new ZStringBuilder();
			foreach (var response in responses)
			{
				var responseNumber = response.References?.Where(x => x.Code == UYMessageConstants.HBLNumber).FirstOrDefault();
				var responseSequence = response.References?.Where(x => x.Code == UYMessageConstants.HBLSequenceNumber).FirstOrDefault();
				var responseDNA = response.References?.Where(x => x.Code == UYMessageConstants.DNANumber).FirstOrDefault();

				messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.Result, response.Description));
				messageText.AppendLine();

				messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.Description, response.Help));
				messageText.AppendLine();
				messageText.AppendLine();

				messageText.Append(ZString.Format("{0}", UYMessageConstants.References));
				messageText.AppendLine();
				messageText.AppendLine();

				if (responseDNA != null)
				{
					messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.DNANumberForView, responseDNA.Value));
					messageText.AppendLine();
				}
				if (responseSequence != null)
				{
					messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.Sequence, responseSequence.Value));
					messageText.AppendLine();
				}
				if (responseNumber != null)
				{
					messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.HBLNumberForView, responseNumber.Value));
					messageText.AppendLine();
				}

				messageText.AppendLine();
			}

			return messageText.ToString();
		}

		internal static ZString MessageInterpretationForWrongCredentials(Dictionary<string, string> headerTextDictionary)
		{
			var messageText = new ZStringBuilder();

			var errorType = headerTextDictionary.TryGetValue("custom.ErrorType", out var errorTypeValue) ? errorTypeValue : string.Empty;
			var notificationTime = headerTextDictionary.TryGetValue("custom.NotificationTime", out var notificationTimeValue) ? notificationTimeValue : string.Empty;
			var notificationType = headerTextDictionary.TryGetValue("custom.NotificationType", out var notificationTypeValue) ? notificationTypeValue : string.Empty;
			var errorDescription = headerTextDictionary.TryGetValue("custom.ErrorDescription", out var errorDescriptionValue) ? errorDescriptionValue : string.Empty;

			messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.Result, errorType));
			messageText.AppendLine();

			messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.Description, errorDescription));
			messageText.AppendLine();

			messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.NotificationTime, notificationTime));
			messageText.AppendLine();

			messageText.Append(ZString.Format("{0}{1}", UYMessageConstants.NotificationType, notificationType));
			messageText.AppendLine();

			return messageText.ToString();
		}

		internal static ZString EnvelopeSignedMessage(ZString signedXMLMessage)
		{
			var processedMessage = GetExpectedMessageXML(signedXMLMessage);

			return processedMessage;
		}

		internal static List<Tuple<string, string>> GetResponseInformation(ZString status, ZString refNumber)
		{
			var valueList = new List<Tuple<string, string>>();
			valueList.Add(new Tuple<string, string>((NoResString)"Status", status)); // only used in HTML Body
			valueList.Add(new Tuple<string, string>((NoResString)"Ref. Number", refNumber)); // only used in HTML Body
			return valueList;
		}

		internal static EDIInterchange GetSentInterchangeWithTrackingId(ZGuid eHubTrackingID, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, eHubTrackingID);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, UYCInterchange.Direction.Transmit);
			return factory.LoadTop1<EDIInterchange>(query);
		}

		internal static EDIMessage GetOriginalMessage(ZGuid interchangePK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchangePK);
			return factory.LoadTop1<EDIMessage>(query);
		}

		static ZString GetExpectedMessageXML(ZString signedXMLMessage)
		{
			var messageWithOutXMlDeclaration = signedXMLMessage.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ZString.Empty);

			var messageTextEncoded = WebUtility.HtmlEncode(messageWithOutXMlDeclaration);

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.UY.Manifest.Business.Message.Templates.UYMessageFullRequestContent.xml"))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd().Replace("{BodyText}", messageTextEncoded).Replace("&quot;", "\"");
			}
		}
	}
}
