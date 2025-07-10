using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[SystemDefinedValues]
	public class NZCMessage : EDIMessage, Integration.Customs.NZ.INZCMessage
	{
		public const string MsgTransModeConstant = "MsgTransMode";
		public static readonly new TypeDecider TypeDecider = new NZCMessageTypeDecider();

		public static class MessageTypes
		{
			public const string ResponseMsg = "RES";

			public static class ECIWriteOff
			{
				public const string MessageType = "CAR"; // CUSCAR
				public static class MessageSubTypes
				{
					public const string Original = Business.MessageSubTypeList.Codes.Original;
					public const string ReplaceHeader = Business.MessageSubTypeList.Codes.ReplaceHeader;
					public const string ReplaceLines = Business.MessageSubTypeList.Codes.ReplaceLines;
					public const string Cancellation = Business.MessageSubTypeList.Codes.Cancellation;
					public const string Replacement = Business.MessageSubTypeList.Codes.Replacement;
				}
			}

			public static class FormalEntry
			{
				public const string MessageType = "DEC"; // CUSDEC
				public static class MessageSubTypes
				{
					public const string Original = Business.MessageSubTypeList.Codes.Original;
					public const string ReplaceHeader = Business.MessageSubTypeList.Codes.ReplaceHeader;
					public const string ReplaceLines = Business.MessageSubTypeList.Codes.ReplaceLines;
					public const string AddLine = Business.MessageSubTypeList.Codes.AddLine;
					public const string CancelLine = Business.MessageSubTypeList.Codes.CancelLine;
					public const string Replacement = Business.MessageSubTypeList.Codes.Replacement;
					public const string Completion = Business.MessageSubTypeList.Codes.Completion;
					public const string Cancellation = Business.MessageSubTypeList.Codes.Cancellation;
				}
			}

			public static class OutwardReport
			{
				public const string MessageType = "ORM";
				public static class MessageSubTypes
				{
					public const string Original = Business.MessageSubTypeList.Codes.Original;
					public const string Replacement = Business.MessageSubTypeList.Codes.Replacement;
					public const string Cancellation = Business.MessageSubTypeList.Codes.Cancellation;
				}
			}

			public static class InwardCargoReport
			{
				public const string MessageType = MessageTypeList.Codes.ICR;
				public static class MessageSubTypes
				{
					public const string Original = Business.MessageSubTypeList.Codes.Original;
					public const string Replacement = Business.MessageSubTypeList.Codes.Replacement;
					public const string Cancellation = Business.MessageSubTypeList.Codes.Cancellation;
				}
			}
		}

		public NZCMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected string Sender
		{
			get { return NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant(); }
		}

		protected string Receiver
		{
			get { return EM_IsTestMessage ? EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox : EDIInterchange.InterchangePartyIDs.NZCustomsLiveMailbox; }
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", Sender, Receiver).GetNextFormatted(Factory);
		}

		protected override string GetSendersReference()
		{
			ZString result = "";
			if (EntryHeader != null)
			{
				EntryHeader.PopulateCH_BGMReferenceIfNeeded();
				result = EntryHeader.CH_BGMReference;
			}
			return result;
		}

		protected override string GetContainedChecksum(string messageText)
		{
			string result = "PF.NOUSER";

			MessageBuilders.FormalEntry.CurrentUsersPin usersPin;
			if (UserWhoQueuedThisRecord != null)
			{
				usersPin = new MessageBuilders.FormalEntry.CurrentUsersPin(UserWhoQueuedThisRecord, EM_IsTestMessage);
			}
			else
			{
				usersPin = new MessageBuilders.FormalEntry.CurrentUsersPin(Factory, EM_IsTestMessage);
			}

			PinBuilder pinBuilder = new PinBuilder(usersPin.DecryptedPinCode);
			pinBuilder.GenerateBlocksFromEDIFACTMessageText(messageText);
			result = pinBuilder.GetMAC();
			return result;
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null && EntryHeader != null)
				{
					fDeclaration = EntryHeader.Declaration;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		CusEntryHeader fEntryHeader;
		public CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null && !EM_LinkUniqueID.IsEmpty)
				{
					fEntryHeader = Factory.Load<CusEntryHeader>(EM_LinkUniqueID);
				}
				return fEntryHeader;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.NewZealandCustoms;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_Status = Status.Queued;
			EM_IsTestMessage = true;
		}

		CUSRESMessage fMessageAsCUSRESD98A;
		public CUSRESMessage MessageAsCUSRESD98A
		{
			get
			{
				if (fMessageAsCUSRESD98A == null)
				{
					MessageProcessors.CusResD98AConverter converter = new MessageProcessors.CusResD98AConverter(this);
					fMessageAsCUSRESD98A = converter.CusResD98A;
				}
				return fMessageAsCUSRESD98A;
			}
		}

		Edifact.D96B.Messages.CUSRES.CUSRESMessage fMessageAsCUSRESD96B;
		public Edifact.D96B.Messages.CUSRES.CUSRESMessage MessageAsCUSRESD96B
		{
			get
			{
				if (fMessageAsCUSRESD96B == null)
				{
					MessageProcessors.CusResD96BConverter converter = new MessageProcessors.CusResD96BConverter(this);
					fMessageAsCUSRESD96B = converter.CusResD96B;
				}
				return fMessageAsCUSRESD96B;
			}
		}

		public bool IsQueuedToBeSentLater
		{
			get { return IsTransmitMessage && EM_Status == NZCMessage.Status.Queued; }
		}

		public virtual ZString GetJobNumber()
		{
			ZString result = ZString.Empty;
			if (EntryHeader != null)
			{
				result = EntryHeader.CH_BGMReference;
				if (result.IsEmpty && EntryHeader.Declaration != null)
				{
					result = EntryHeader.Declaration.JE_DeclarationReference;
				}
			}
			else
			{
				CusMAWB mawb = EM_LinkedObject as CusMAWB;
				if (mawb != null)
				{
					result = mawb.CM_MessageReference;
				}
			}
			return result.IsEmpty ? new ZString("(unknown)") : result;
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList MessageSubTypeList
		{
			get { return Factory.GetCachedValue<MessageSubTypeList>(); }
		}

		public virtual ZString MsgTransMode
		{
			get { return this.GetSystemDefinedValue<ZString>(MsgTransModeConstant); }
			set { this.SetSystemDefinedValue(MsgTransModeConstant, value); }
		}

		internal void ClearMsgTransMode()
		{
			MsgTransMode = ZString.Empty;
		}
	}
}
