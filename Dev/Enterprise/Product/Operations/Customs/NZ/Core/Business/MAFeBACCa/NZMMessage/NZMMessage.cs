using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	[SystemDefinedValues]
	public class NZMMessage : EDIMessage
	{
		public NZMMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region MessageTypes

		public static class MessageTypes
		{
			public static class Transmit
			{
				public const string MessagingRequest = "EBR";
				public static class MessageSubTypes
				{
					public const string Original = CodedLists.MessageSubTypeList.Codes.Original;
					public const string Replacement = CodedLists.MessageSubTypeList.Codes.Replacement;
				}
			}

			public static class Receive
			{
				public const string SOAPError = "EBE";
				public const string Acknowledgement = "EBA";
				public const string Notification = "EBN";
				public static class MessageSubTypes
				{
					public const string SOAPError = CodedLists.MessageSubTypeList.Codes.SOAPError;
					public const string Acknowledgement = CodedLists.MessageSubTypeList.Codes.Acknowledgement;
					public const string ErrorMessage = CodedLists.MessageSubTypeList.Codes.ErrorMessage;
					public const string NotifyCRN = CodedLists.MessageSubTypeList.Codes.NotifyCRN;
					public const string RequestMoreInfo = CodedLists.MessageSubTypeList.Codes.RequestMoreInfo;
					public const string Cancellation = CodedLists.MessageSubTypeList.Codes.Cancellation;
				}
			}
		}

		#endregion

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			var sender = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();
			var receiver = EM_IsTestMessage ? EDIInterchange.InterchangePartyIDs.NZMAFeBACCaTestMailbox : EDIInterchange.InterchangePartyIDs.NZMAFeBACCaLiveMailbox;
			return Env.NumberFountains.EDIFACTNumberFountain("M", sender, receiver).GetNextFormatted(Factory);
		}

		protected override string GetSendersReference()
		{
			var declaration = Parent as JobDeclaration;
			if (declaration != null)
			{
				declaration.PopulateJE_DeclarationReferenceIfNeeded();
				return declaration.JE_DeclarationReference;
			}
			var consol = Parent as ForwardingConsol;
			if (consol != null)
			{
				consol.PopulateJK_UniqueConsignRefIfNeeded();
				return consol.JK_UniqueConsignRef;
			}
			return string.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.NewZealandMAFeBACCa;
			EM_ReceiveTransmit = Direction.Transmit;
			EM_Status = Status.Queued;
			EM_IsTestMessage = true;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				var declaration = Parent as JobDeclaration;
				if (declaration != null && declaration.JE_GB.IsValid)
				{
					EM_GB = declaration.JE_GB;
				}
			}
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return Factory.GetCachedValue<MessageSubTypeList>(); }
		}

		protected override string MessageNumberPlaceHolderOverride
		{
			get { return MessageNumberPlaceHolder; }
		}

		protected override string SendersReferencePlaceHolderOverride
		{
			get { return SendersReferencePlaceHolder; }
		}

		public new const string MessageNumberPlaceHolder = "||-MESSAGE NUMBER PLACE HOLDER-||";
		public new const string SendersReferencePlaceHolder = "||-SENDERS REFERENCE PLACE HOLDER-||";

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(MessageInterpretationNoteManager.Value);
				if (!result.IsEmpty)
				{
					result.Append("");
					result.Append("");
					result.Append("Raw Message Text:");
					result.Append("-----------------");
					result.Append("");
				}

				return new NZMMessageStreamFormatter(result.ToStringWithNewLineBetweenAppends());
			}
		}

		public ZString MsgTransMode
		{
			get { return this.GetSystemDefinedValue<ZString>(NZCMessage.MsgTransModeConstant); }
			set { this.SetSystemDefinedValue(NZCMessage.MsgTransModeConstant, value); }
		}

		internal void ClearMsgTransMode()
		{
			MsgTransMode = ZString.Empty;
		}

		#endregion

		#region MAFMessaging

		internal MAFMessagingBO MAFMessaging
		{
			get
			{
				if (mafMessaging == null && Parent != null)
				{
					var declaration = Parent as JobDeclaration;
					if (declaration != null)
					{
						mafMessaging = new MAFMessagingBO(new MAFPlugInSupportDeclarationWrapper(declaration));
					}
					else
					{
						var consol = Parent as ForwardingConsol;
						if (consol != null)
						{
							mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(consol));
						}
					}
				}
				return mafMessaging;
			}
		}
		MAFMessagingBO mafMessaging;

		#endregion

		#region Parent

		BusinessObject Parent
		{
			get
			{
				if (parent == null && !EM_LinkUniqueID.IsEmpty)
				{
					parent = (BusinessObject)Factory.Load<JobDeclaration>(EM_LinkUniqueID)
							 ?? Factory.Load<ForwardingConsol>(EM_LinkUniqueID);
				}
				return parent;
			}
		}
		BusinessObject parent;

		#endregion
	}
}
