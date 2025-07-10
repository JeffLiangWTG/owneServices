using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business
{
	public static class TRInterchangeHelper
	{
		public static ZString GetInterchangeTo(ZString messageType, ZBool isTestMessage)
		{
			var interchangeTo = messageType == TRMessageTypes.Codes.EUT
				? (isTestMessage ? TRMessageConstants.ExportUnionTest : TRMessageConstants.ExportUnion)
				: (isTestMessage ? TRMessageConstants.TRRecipientTest : TRMessageConstants.TRRecipient);

			return System.FormattableString.Invariant($"{messageType}{interchangeTo}");
		}

		public static EDIMessage GetOriginalMessageByTrackingId(EDIMessage currentMessage)
		{
			EDIMessage originalMessage = null;

			var interchange = currentMessage.Interchange;
			var trackingId = interchange?.EI_SessionGUID ?? ZGuid.Empty;

			if (trackingId.IsValid)
			{
				var factory = currentMessage.Factory;

				var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				var originalInterchange = factory.LoadTop1<EDIInterchange>(query);

				if (originalInterchange != null)
				{
					originalMessage = originalInterchange.ContainedMessages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				}
			}

			return originalMessage;
		}

		public static EDIMessage GetMainMessageByType(EDIMessage currentMessage, ZString messageTypeCode)
		{
			Argument.NotNull(currentMessage, "currentMessage cannot be null");

			ZQuery messageFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, currentMessage.EM_LinkUniqueID);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, messageTypeCode);
			EDIMessage mainMessage = currentMessage.Factory.Load<EDIMessage>(messageFilter).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			return mainMessage;
		}

		public static ZString GetMessageTypeByMain(ZString currentMessageType)
		{
			var mainMessageType = ZString.Empty;
			switch (currentMessageType)
			{
				case TRMessageTypes.Codes.TRO
					or TRMessageTypes.Codes.T1O
					or TRMessageTypes.Codes.T2O
					or TRMessageTypes.Codes.T3O
					or TRMessageTypes.Codes.TRM:
					mainMessageType = TRMessageTypes.Codes.TRO;
					break;
				case TRMessageTypes.Codes.TRE
					or TRMessageTypes.Codes.TRQ
					or TRMessageTypes.Codes.TRL
					or TRMessageTypes.Codes.TRI
					or TRMessageTypes.Codes.TRS
					or TRMessageTypes.Codes.TRB
					or TRMessageTypes.Codes.TRD
					or TRMessageTypes.Codes.TCD
					or TRMessageTypes.Codes.T1D
					or TRMessageTypes.Codes.T2D
					or TRMessageTypes.Codes.T1S
					or TRMessageTypes.Codes.T1E:
					mainMessageType = TRMessageTypes.Codes.TRE;
					break;
				case TRMessageTypes.Codes.TSP
					or TRMessageTypes.Codes.T1P:
					mainMessageType = TRMessageTypes.Codes.TSP;
					break;
				case TRMessageTypes.Codes.TRN
					or TRMessageTypes.Codes.T1N
					or TRMessageTypes.Codes.T2N:
					mainMessageType = TRMessageTypes.Codes.TRN;
					break;
				case TRMessageTypes.Codes.DKO
					or TRMessageTypes.Codes.DK1:
					mainMessageType = TRMessageTypes.Codes.DKO;
					break;
				case TRMessageTypes.Codes.DTE
					or TRMessageTypes.Codes.DT1
					or TRMessageTypes.Codes.DT2
					or TRMessageTypes.Codes.DT3:
					mainMessageType = TRMessageTypes.Codes.DTE;
					break;
				case TRMessageTypes.Codes.EUT
					or TRMessageTypes.Codes.EUR:
					mainMessageType = TRMessageTypes.Codes.EUT;
					break;
			}

			return mainMessageType;
		}

		public static void SetMessageOwnerByMainMessage(EDIMessage currentMessage)
		{
			var mainMessageType = GetMessageTypeByMain(currentMessage.EM_MessageType);
			var mainMessage = GetMainMessageByType(currentMessage, mainMessageType);
			if (mainMessage != null)
			{
				var systemCreateUser = mainMessage.EM_SystemCreateUser;
				if (!systemCreateUser.IsEmpty)
				{
					currentMessage.EM_MessageOwner = systemCreateUser;
					var user = mainMessage.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, currentMessage.EM_MessageOwner));
					var languageCode = user?.Language ?? ZString.Empty;
					if (languageCode != ZString.Empty)
					{
						GlbStaff.CurrentUser.GS_WorkingLanguage = languageCode;
					}
				}
			}
		}

		public static EDIMessage GetLastMessageWithApplicationReferenceByType(EDIMessage currentMessage, ZString transmitTypeCode, ZString messageTypeCode)
		{
			var messageFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, currentMessage.EM_LinkUniqueID);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, transmitTypeCode);
			messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, messageTypeCode);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			return currentMessage.Factory.Load<EDIMessage>(messageFilter).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		public static ZString CreateHeaderTextForEUTInterchange(ZGuid guid)
		{
			var fileName = guid.ToString() + TRMessageConstants.ExportUnionFtpFileNameExtension;
			var headerText = new Dictionary<string, string>
			{
				{ TRMessageConstants.ExportUnionFtpFileNameKey, fileName }
			};

			return System.Text.Json.JsonSerializer.Serialize(headerText);
		}
	}
}
