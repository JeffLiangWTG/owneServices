using System.Collections;
using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	enum GeneratedMessage
	{
		None,
		Send,
		Withdraw
	}

	public class CMDGenerator
	{
		public abstract class Constants
		{
			public const string TDB = "TDB";
			public const string GHA = "GHA";
		}

		public CMDGenerator(CMDShipmentWrapper shipmentWrapper, ZString recipient)
		{
			this.shipmentWrapper = shipmentWrapper;
			this.shipmentWrapper.ShowActiveMessagesOnly = true;
			this.recipient = recipient.Left(4);
		}

		public CMDGenerator(CMDShipmentWrapper shipmentWrapper)
			: this(shipmentWrapper, "")
		{
		}

		#region Send Messages

		/// <summary>
		/// The new messages are compared with the currently active ones.
		/// The comparison rules are as follow:
		/// - New CMD message that do not exist in the active message list should be sent with the Action Code "Add"
		/// - New CMD message that has an exact same content as one of the messages in the active message list should not be re-sent.
		/// - New CMD message that has a different content as one of the messages in the active message list but has the same Application
		///   Reference (Consol No.) should be sent with the Action Code "Modify"
		/// - The remaining active messages should be re-sent with the Action Code "Delete" (since they are not relevant to the shipment anymore)			
		/// </summary>
		public virtual void GenerateSendMessages()
		{
			lastMessageAttemptedToGenerate = GeneratedMessage.Send;
			ResetCachedProperties();

			if (IsValidSendMessage)
			{
				for (int i = 0; i < ConsolMessagePairs.Count; i++)
				{
					CMDEDIMessage[] messages = GetCurrentCMDMessagesToGHA(ConsolMessagePairs.Keys[i]);
					if (messages.Length == 0)
					{
						CreateNewMessage(ConsolMessagePairs.Keys[i], ConsolMessagePairs[i]);
					}
					else
					{
						UpdateMessage(ConsolMessagePairs.Keys[i], ConsolMessagePairs[i], messages[0]);

						for (int j = 1; j < messages.Length; j++)
						{
							SetMessageInactive(messages[j]);
						}
					}
				}

				DeleteIrrelevantMessages();
			}
		}

		void SetMessageInactive(CMDEDIMessage message)
		{
			message.EM_IsActive = false;
			SetFirstOccurenceOfCurrentTDBMessageInactive(message);
		}

		void UpdateMessage(ZString consolID, ZString newMessageText, CMDEDIMessage message)
		{
			bool differentRecipient = !recipient.IsEmpty && message.Reply != null && message.Reply.Sender != recipient;
			if (message.EM_MessageText != newMessageText || differentRecipient)
			{
				CMDParser oldParser = new CMDParser(message.EM_MessageText);
				if ((message.Reply == null || message.Reply.IsErrorMessage) && oldParser.ActionCode == CMD.ActionCodes.Add)
				{
					message.EM_IsActive = false;
					CreateNewMessage(consolID, newMessageText);
				}
				else
				{
					CreateModifyMessage(oldParser, message, newMessageText);
				}

				if (!message.EM_IsActive)
				{
					SetCurrentTDBMessageInactive(message, differentRecipient);
				}
			}
			else
			{
				message.HasChanges = true;
			}
		}

		#endregion

		#region Withdraw Messages

		/// <summary>
		/// Send a CMD Delete Message for every active message with action code "Add" or "Modify".
		/// </summary>
		public virtual void GenerateWithdrawMessages()
		{
			lastMessageAttemptedToGenerate = GeneratedMessage.Withdraw;

			if (IsValidWithdrawMessage)
			{
				foreach (CMDEDIMessage message in GetCMDMessages())
				{
					CMDParser parser = new CMDParser(message.EM_MessageText);
					if (parser.IsValid && parser.ActionCode != CMD.ActionCodes.Delete)
					{
						message.EM_IsActive = false;
						CreateDeleteMessage(message);
					}
				}
			}
		}

		#endregion

		public int NoOfMessagesSent
		{
			get { return fNoOfMessagesSent; }
		}

		int fNoOfMessagesSent;
		GeneratedMessage lastMessageAttemptedToGenerate = GeneratedMessage.None;

		#region Implementation

		#region Message Creation

		protected void InsertMessage(string consolID, string messageSubType, string messageText, string masterBillNum)
		{
			ZString routingInfo = GetRoutingInfo(messageSubType, masterBillNum);
			string headerText = string.Concat(EDIMessage.ApplicationCodes.SingaporeCMD, "\r\n", routingInfo, "\r\n");
			InsertMessageWithHeaderText(consolID, messageSubType, messageText, headerText);
		}

		protected void CreateInterchange(string consolID, CMDEDIMessage message, ZString headerText)
		{
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, consolID));
			var departureFlight = consol?.GetTransportByPlanningType(Core.Constants.TransportPlanningType.Flight1);
			var airlineCode = departureFlight?.JW_VoyageFlight.Left(2) ?? ZString.Empty;
			var hawb = Shipment.JS_HouseBill;
			var mawb = consol?.JK_MasterBillNum ?? ZString.Empty;

			var sendViaEHub = SGCustomsDataRegistry.Instance.SendViaEHub.Value;
			var header = sendViaEHub ? CargoIMPInterchangeGenerator.CreateHubHeader(message, "QK", airlineCode, hawb, mawb) : headerText;
			var footer = sendViaEHub ? CargoIMPInterchangeGenerator.CreateHubFooter() : new ZString(((char)4).ToString());

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = header;
			interchange.EI_BodyText = message.EM_MessageText;
			interchange.EI_FooterText = footer;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = (registrationKey.EnterpriseCode.Length == 0 ? (string)GlbCompany.CurrentCompany.GC_Code : registrationKey.EnterpriseCode) + registrationKey.ServerCode;
			interchange.EI_Status = sendViaEHub ? EDIInterchange.Status.eHubQueued : EDIInterchange.Status.Queued;
			interchange.EI_To = sendViaEHub ? "eHubAirService" : "CCN CMD Processor";
			interchange.EI_ApplicationCode = message.EM_ApplicationCode;
			interchange.EI_InterchangeType = interchange.EI_ApplicationCode;
			message.EM_EI = interchange.PK;

			if (sendViaEHub)
			{
				message.EM_Status = EDIMessage.Status.Sent;
			}
		}

		void CreateNewMessage(string consolID, string messageText)
		{
			CMDParser parser = new CMDParser(messageText);
			InsertMessage(consolID, Constants.GHA, messageText, parser.MasterBillNumber);

			if (parser.IsLate)
			{
				InsertMessage(consolID, Constants.TDB, messageText, parser.MasterBillNumber);
			}
		}

		void CreateModifyMessage(CMDParser oldParser, CMDEDIMessage message, string messageText)
		{
			CMDParser parser = new CMDParser(messageText);
			if (parser.ActionCode != CMD.ActionCodes.Delete)
			{
				parser.ActionCode = (oldParser.ActionCode == CMD.ActionCodes.Delete) ? CMD.ActionCodes.Add : CMD.ActionCodes.Modify;
			}

			if (parser.ModifiedMessageText != message.EM_MessageText)
			{
				message.EM_IsActive = false;
				string newMessageText = parser.ModifiedMessageText;
				InsertMessage(message.EM_ApplicationReference, Constants.GHA, newMessageText, parser.MasterBillNumber);

				if (parser.IsLate)
				{
					InsertMessage(message.EM_ApplicationReference, Constants.TDB, newMessageText, parser.MasterBillNumber);
				}
			}
			else
			{
				message.HasChanges = true;
			}
		}

		void CreateDeleteMessage(CMDEDIMessage message)
		{
			CMDParser parser = new CMDParser(message.EM_MessageText);
			if (parser.ActionCode != CMD.ActionCodes.Delete)
			{
				message.EM_IsActive = false;
				parser.ActionCode = CMD.ActionCodes.Delete;
				var interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				if (interchange != null)
				{
					InsertMessageWithHeaderText(message.EM_ApplicationReference, message.EM_MessageSubType, parser.ModifiedMessageText, interchange.EI_HeaderText);
				}
				else
				{
					InsertMessage(message.EM_ApplicationReference, message.EM_MessageSubType, parser.ModifiedMessageText, parser.MasterBillNumber);
				}
			}
		}

		void InsertMessageWithHeaderText(string consolID, string messageSubType, string messageText, string headerText)
		{
			CMDEDIMessage message = shipmentWrapper.Messages.AddNew();
			message.EM_ApplicationReference = consolID;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = messageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			CreateInterchange(consolID, message, headerText);
			fNoOfMessagesSent++;
		}

		#endregion

		#region Validation

		public virtual ZString ErrorMessage
		{
			get
			{
				ZString result = "";

				if (lastMessageAttemptedToGenerate == GeneratedMessage.Send)
				{
					if (CMDData.ExemptionCode.IsEmpty && CMDData.TDBPermitNos.Length == 0)
					{
						result = "There is no Exemption Code or Permit Numbers entered to send.";
					}
					else if (ConsolMessagePairs.Count == 0)
					{
						result = "There is no Consol to send the CMD Messages for.";
					}
				}
				else if (lastMessageAttemptedToGenerate == GeneratedMessage.Withdraw)
				{
					if (shipmentWrapper.Messages.Count == 0)
					{
						result = "There is no previously sent CMD Messages to be deleted.";
					}
				}

				return result;
			}
		}

		protected virtual bool IsValidSendMessage
		{
			get { return (!CMDData.ExemptionCode.IsEmpty || CMDData.TDBPermitNos.Length > 0) && ConsolMessagePairs.Count > 0; }
		}

		protected virtual bool IsValidWithdrawMessage
		{
			get { return shipmentWrapper.Messages.Count > 0; }
		}

		#endregion

		ForwardingShipment Shipment
		{
			get { return shipmentWrapper.Shipment; }
		}

		CMDData CMDData
		{
			get
			{
				if (fCMDData == null)
				{
					fCMDData = new CMDData(shipmentWrapper);
				}
				return fCMDData;
			}
		}

		NameValueCollection ConsolMessagePairs
		{
			get
			{
				if (fConsolMessagePairs == null)
				{
					fConsolMessagePairs = GetNewMessagePairs();
				}
				return fConsolMessagePairs;
			}
		}

		ZString OriginCity
		{
			get { return Shipment.JS_RL_NKOrigin.Right(3); }
		}

		BusinessObjectFactory Factory
		{
			get { return shipmentWrapper.Factory; }
		}

		protected virtual string GetCMDString(ForwardingConsol consol)
		{
			return new CMD(consol, CMDData).ToString();
		}

		protected virtual bool IsImportOrExport(ForwardingConsol consol)
		{
			return (consol.IsImport() || consol.IsExport());
		}

		string GetDestCodeFromMasterBillNum(ZString masterBillNum)
		{
			string result = "";
			if (masterBillNum.Length > 2)
			{
				RefAirline airline = RefAirline.LoadFromAirlinePrefix(Factory, masterBillNum.Left(3));
				result = airline != null ? airline.RM_TwoCharacterCode : ZString.Empty;
			}
			return result;
		}

		NameValueCollection GetNewMessagePairs()
		{
			NameValueCollection consolMessagePairs = new NameValueCollection();
			ForwardingConsol[] consols = GetValidConsols();
			foreach (ForwardingConsol consol in consols)
			{
				consolMessagePairs.Add(consol.JK_UniqueConsignRef, GetCMDString(consol));
			}
			return consolMessagePairs;
		}

		ForwardingConsol[] GetValidConsols()
		{
			ArrayList list = new ArrayList();
			foreach (ForwardingConsol consol in Shipment.Consols)
			{
				if (consol.IsAir && IsImportOrExport(consol))
				{
					list.Add(consol);
				}
			}
			return (ForwardingConsol[])list.ToArray(typeof(ForwardingConsol));
		}

		void SetFirstOccurenceOfCurrentTDBMessageInactive(CMDEDIMessage gHAMessage)
		{
			SetCurrentTDBMessageInactive(gHAMessage, true);
		}

		void SetCurrentTDBMessageInactive(CMDEDIMessage gHAMessage, bool firstOccurrenceOnly)
		{
			ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageSubType, Constants.TDB);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationReference, gHAMessage.EM_ApplicationReference);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, Constants.TDB);
			filter.AddToFilter(EDIMessageSchema.EM_MessageText, gHAMessage.EM_MessageText);
			CMDEDIMessage[] tDBMessages = (CMDEDIMessage[])shipmentWrapper.Messages.Find(filter);

			foreach (CMDEDIMessage message in tDBMessages)
			{
				message.EM_IsActive = false;
				if (firstOccurrenceOnly)
				{
					break;
				}
			}
		}

		CMDEDIMessage[] GetCurrentCMDMessagesToGHA(ZString applicationReference)
		{
			ZQuery filter = new ZQuery(EDIMessageSchema.EM_ApplicationReference, applicationReference);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessage.ApplicationCodes.SingaporeCMD);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, Constants.GHA);

			return (CMDEDIMessage[])shipmentWrapper.Messages.Find(filter);
		}

		CMDEDIMessage[] GetCMDMessages()
		{
			ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIInterchange.ApplicationCodes.SingaporeCMD);
			return (CMDEDIMessage[])shipmentWrapper.Messages.Find(filter);
		}

		void DeleteIrrelevantMessages()
		{
			shipmentWrapper.RefreshMessageList();
			foreach (CMDEDIMessage message in GetCMDMessages())
			{
				if (!message.HasChanges && message.EM_MessageSubType == Constants.GHA)
				{
					CreateDeleteMessage(message);
				}
			}
		}

		void ResetCachedProperties()
		{
			fCMDData = null;
			fConsolMessagePairs = null;
			fNoOfMessagesSent = 0;
		}

		ZString GetRoutingInfo(ZString messageSubType, ZString masterBillNum)
		{
			ZString result;

			if (messageSubType == Constants.TDB)
			{
				result = Constants.TDB;
			}
			else if (!recipient.IsEmpty)
			{
				result = recipient;
			}
			else
			{
				result = string.Concat(GetDestCodeFromMasterBillNum(masterBillNum), OriginCity);
			}

			return result;
		}

		readonly CMDShipmentWrapper shipmentWrapper;
		CMDData fCMDData;
		NameValueCollection fConsolMessagePairs;
		readonly ZString recipient;

		#endregion
	}
}
