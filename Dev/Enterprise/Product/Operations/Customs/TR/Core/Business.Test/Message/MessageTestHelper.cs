using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public static class MessageTestHelper
	{
		public static EDIInterchange CreateInterchange(BusinessObjectFactory factory, ZString interchangeType, ZString direction, ZString status, ZGuid sessionGUID, ZString bodyText)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = status == EDIInterchange.Direction.Transmit ? "CW1" : "TR Customs Test";
			interchange.EI_To = status == EDIInterchange.Direction.Transmit ? "TR Customs Test" : "CW1";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;
			interchange.EI_BodyText = bodyText;

			return interchange;
		}

		public static EDIInterchange CreateInterchange(BusinessObjectFactory factory, ZString interchangeType, bool isErrorInterchange = false)
		{
			var interchange = CreateInterchange(factory,
				interchangeType,
				EDIInterchange.Direction.Receive,
				EDIInterchange.Status.Queued,
				ZGuid.Empty,
				ZString.Empty);

			const string bodyText = "Test Body";
			const string failHeaderText = "Test Fail Header";

			interchange.EI_InterchangeNum = "123654";
			interchange.EI_From = "TROCustoms";
			interchange.EI_To = "eHub";
			interchange.EI_HeaderText = failHeaderText;
			interchange.EI_BodyText = bodyText;

			if (interchangeType == TRMessageTypes.Codes.XER)
			{
				interchange.EI_BodyText = bodyText;
			}
			else
			{
				interchange.EI_BodyText = TRMessageTestHelper.GetBodyText(interchangeType, isErrorInterchange);
			}

			return interchange;
		}

		public static EDIInterchange CreateInterchange(BusinessObjectFactory factory, string bodyText, string headerText, string interChangeType, string from,
			string to)
		{
			var interchange = CreateInterchange(factory,
				interChangeType,
				EDIInterchange.Direction.Receive,
				EDIInterchange.Status.Queued,
				ZGuid.Empty,
				bodyText);

			interchange.EI_From = from;
			interchange.EI_To = to;
			interchange.EI_HeaderText = headerText;

			factory.Save();
			return interchange;
		}

		public static EDIMessage CreateMessage(BusinessObjectFactory factory, ZString messageType, ZString direction, ZString status, ZString linkTable, ZGuid linkID, ZString messageText, ZGuid interchangePK)
		{
			var message = factory.New<TRManifestMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageText;
			message.EM_LinkTable = linkTable;
			message.EM_LinkUniqueID = linkID;
			message.EM_EI = interchangePK;

			return message;
		}

		public static T CreateEdiMessage<T>(string messageType, string bodyText, string status, string direction, string linkTable , BusinessObjectFactory factory) where T : EDIMessage
		{
			var message = factory.New<T>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;
			message.EM_LinkTable = linkTable;

			return message;
		}

		public static CusPollingTransaction CreateCusPollingTransaction(BusinessObjectFactory factory, ZString type, ZString status, ZByte numberOfAttempts, ZDateTime earliestTimeOfNextAttempt, ZString transactionID, ZGuid parentID)
		{
			var pollingTransaction = factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = type;
			pollingTransaction.CPT_Status = status;
			pollingTransaction.CPT_NumberOfAttempts = numberOfAttempts;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttempt;
			pollingTransaction.CPT_TransactionID = transactionID;
			pollingTransaction.CPT_ParentID = parentID;

			return pollingTransaction;
		}

		public static string GetBodyText(this string input) => Regex.Match(input, @"\<body[\s\S]*\</body\>").Value;

		public static string GetBodyText(this ZString input) => GetBodyText(input.ToString());

		public static string RemoveLineBreakingsAndIndents(this string input) => Regex.Replace(input, @"(\r|\n|\r\n)\s*", string.Empty);

		public static string RemoveLineBreakingsAndIndents(this ZString input) => input.ToString().RemoveLineBreakingsAndIndents();
	}
}
