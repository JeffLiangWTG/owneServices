using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	public class TWCInterchange : EDIInterchange
	{
		public const string TWCustomsForTest = "TWCustomsTest";
		public TWCInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.TaiwanCustoms;
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return true; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override Type GetMessageTypeToCreate(ZString messageText)
		{
			return typeof(TWMessage);
		}

		public override bool IsTestInterchange => EI_From.EndsWith("TEST", StringComparison.CurrentCultureIgnoreCase) || EI_To.EndsWith("TEST", StringComparison.CurrentCultureIgnoreCase);

		public bool CreateMessagesFromInterchageXml(LoggingInformation logger)
		{
			var result = true;
			var messageText = RemoveBOM(EI_BodyText);
			var validMessageInfo = TWMessageHelper.ValidMessageXML(messageText);
			if (!string.IsNullOrEmpty(validMessageInfo))
			{
				logger?.LogError(Res.GetString("D4D30DB6-ECB3-48BF-BC5B-D99168AC389E", "{0},the Message create failed", validMessageInfo));
				result = false;
			}
			if (result)
			{
				var interchangeNum = EI_InterchangeNum;
				var messageType = GetDefaultMessageType(EI_InterchangeType);
				var messageNum = ZString.Empty;

				if (messageType.IsEmpty)
				{
					var codeNumber = interchangeNum.Split(new char[] { '.' });
					if (codeNumber.Length > 0)
					{
						messageType = TWMessageHelper.GetMessageTypeByCode(codeNumber[0]);
					}
					if (messageType.IsEmpty)
					{
						messageType = TWMessageHelper.GetMessageTypeByXml(messageText);
						if (!messageType.IsEmpty)
						{
							logger?.Log(Res.GetString("F7488121-F079-4C24-9F41-BEB63D5BF180", "Interchange {0}: Determine Message Type as {1} from message content.", interchangeNum, messageType));
						}
					}
					else if (codeNumber.Length > 1)
					{
						messageNum = codeNumber[1];
					}
				}

				if (messageType.IsEmpty)
				{
					logger?.LogError(Res.GetString("B29B0B0E-ADBE-45EB-B102-E66D289A0B4D", "Interchange {0}: Can not determine Message Type for the Received Interchange", interchangeNum));
					result = false;
				}
				if (result)
				{
					var newEDIMessage = ContainedMessages.AddNew(GetMessageTypeToCreate(messageText));
					newEDIMessage.EM_IsTestMessage = IsTestInterchange;
					newEDIMessage.EM_ReceiveTransmit = Direction.Receive;
					newEDIMessage.EM_MessageText = messageText;
					newEDIMessage.EM_MessageType = messageType;
					newEDIMessage.EM_MessageNum = messageNum;
				}
			}
			return result;
		}

		ZString RemoveBOM(ZString str)
		{
			if (str.Length > 0 && str[0] == 65279)
			{
				str = str.Substring(1);
			}
			return str;
		}

		ZString GetDefaultMessageType(ZString interchangeType)
		{
			var result = ZString.Empty;
			switch (interchangeType)
			{
				case MessageTypeList.Codes.ECD:
				case MessageTypeList.Codes.ICD:
				case MessageTypeList.Codes.ADM:
				case MessageTypeList.Codes.IEA:
				case MessageTypeList.Codes.FCF:
				case MessageTypeList.Codes.FHM:
				case MessageTypeList.Codes.TRA:
				case MessageTypeList.Codes._101:
				case MessageTypeList.Codes._201:
				case MessageTypeList.Codes._207:
				case MessageTypeList.Codes._301:
				case MessageTypeList.Codes._31A:
				case MessageTypeList.Codes._31D:
				case MessageTypeList.Codes._401:
				case MessageTypeList.Codes._601:
				case MessageTypeList.Codes._603:
					result = interchangeType;
					break;
			}
			return result;
		}

		protected override ZString GetInterchangeNumber()
		{
			var messageOwner = ContainedMessages.Count > 0 ? ContainedMessages[0].EM_MessageOwner : ZString.Empty;
			if (messageOwner.IsEmpty)
			{
				messageOwner = "00000000";
			}
			return PopulateInterchangeNumber(messageOwner);
		}

		ZString PopulateInterchangeNumber(string messageOwner)
		{
			var today = ZDateTime.Today;
			var twYear = (today.Year - 1911) % 100;
			var prefix = ZString.Format("{0}00{1}{2}", messageOwner, twYear.ToString("D2", CultureInfo.InvariantCulture), today.ToString("MMdd", CultureInfo.InvariantCulture));
			var seqNo = Env.NumberFountains.GetTWCustomsInterchangeNumSequence(messageOwner).GetNext(Factory);
			return ZString.Format("{0}{1}", prefix, seqNo.ToString("D4", CultureInfo.InvariantCulture));
		}
	}
}
