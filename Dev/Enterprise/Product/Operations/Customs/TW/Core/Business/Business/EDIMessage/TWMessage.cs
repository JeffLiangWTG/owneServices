using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWMessage : EDIMessage
	{
		public TWMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema
		public new abstract class Schema : EDIMessage.Schema
		{
			public const string EM_Calc_MessageTypeCode = "EM_Calc_MessageTypeCode";
			public const string EM_Calc_MessageTypeDescription = "EM_Calc_MessageTypeDescription";
			public const string InterchangeeHubID = "Interchange+eHubID";

			protected Schema() : base() { }
		}
		#endregion

		#region override Properties
		public new static readonly TWMessageTypeDecider TypeDecider = new TWMessageTypeDecider();
		public override ZString EM_MessageText
		{
			get
			{
				return base.EM_MessageText;
			}
			set
			{
				var oldValue = EM_MessageText;
				base.EM_MessageText = value;
				if (EM_MessageText != oldValue)
				{
					incomingMessageKeyInfomation = null;
				}
				EM_MessageTextInfo.RefreshBinding();
			}
		}

		public override ZString EM_MessageType
		{
			get => base.EM_MessageType;
			set
			{
				var oldValue = EM_MessageType;
				base.EM_MessageType = value;
				if (EM_MessageType != oldValue)
				{
					incomingMessageKeyInfomation = null;
				}
			}
		}
		#endregion

		#region New Properties
		CusEntryHeader fCusEntryHeader;
		public CusEntryHeader CusEntryHeader
		{
			get
			{
				if (fCusEntryHeader == null && EM_LinkedObject is CusEntryHeader linkedObject)
				{
					fCusEntryHeader = linkedObject;
				}
				return fCusEntryHeader;
			}
		}

		public ZDateTime ReleaseDateTime => IncomingMessageKeyInfomation.ReleaseDateTime;

		public ZDateTime DutyDueDateForTPC => IncomingMessageKeyInfomation.DutyDueDateForTPC;

		public ZString EntryNumber => IncomingMessageKeyInfomation.DeclarationID;

		public ZString FunctionalReferenceID => IncomingMessageKeyInfomation.FunctionalReferenceID;

		public ZString ErrorText => IncomingMessageKeyInfomation.ErrorText;

		public ZString EventType => IncomingMessageKeyInfomation.EventType;

		public ZString InterchangeNumber => IncomingMessageKeyInfomation.InterchangeNumber;

		public ZBool IsTranshipment => IncomingMessageKeyInfomation.IsTranshipment;

		public ZString StatusNameCode => IncomingMessageKeyInfomation.StatusNameCode;

		public bool IsCustomsDeliveryNotification
		{
			get
			{
				var result = false;
				switch (EM_MessageType)
				{
					case MessageTypeList.Codes.ECD:
					case MessageTypeList.Codes.ICD:
					case MessageTypeList.Codes.ADM:
					case MessageTypeList.Codes.IEA:
						result = true;
						break;
				}
				return result;
			}
		}

		public bool IsManifestDeliveryNotification => manifestDeliveryNotificationtTypes.Contains(EM_MessageType);

		static readonly ImmutableHashSet<ZString> manifestDeliveryNotificationtTypes = new HashSet<ZString>()
		{
			MessageTypeList.Codes.FCF,
			MessageTypeList.Codes.FHM
		}.ToImmutableHashSet();

		public bool IsManifestMessage => EM_MessageType == MessageTypeList.Codes.FHR;

		public bool IsTransferApplication => EM_MessageType == MessageTypeList.Codes.TRA;

		public bool IsLicensingMessageDeliveryNotification => internalLicensingMessageDeliveryNotification.Contains(EM_MessageType);

		public bool IsLicensingMessageResponse => licensingMessageResponse.Contains(EM_MessageType);

		readonly HashSet<ZString> internalLicensingMessageDeliveryNotification = new HashSet<ZString>()
		{
			MessageTypeList.Codes._101,
			MessageTypeList.Codes._201,
			MessageTypeList.Codes._207,
			MessageTypeList.Codes._301,
			MessageTypeList.Codes._31A,
			MessageTypeList.Codes._31D,
			MessageTypeList.Codes._401,
			MessageTypeList.Codes._601,
			MessageTypeList.Codes._603
		};

		readonly HashSet<ZString> licensingMessageResponse = new HashSet<ZString>()
		{
			MessageTypeList.Codes._102,
			MessageTypeList.Codes._202,
			MessageTypeList.Codes._302,
			MessageTypeList.Codes._32A,
			MessageTypeList.Codes._32D,
			MessageTypeList.Codes._402,
			MessageTypeList.Codes._602,
			MessageTypeList.Codes._901,
			MessageTypeList.Codes._902,
			MessageTypeList.Codes._903
		};

		internal TWIncomingMessageKeyInfomation IncomingMessageKeyInfomation => incomingMessageKeyInfomation ??= new TWIncomingMessageKeyInfomation(EM_MessageType, EM_MessageText);
		TWIncomingMessageKeyInfomation incomingMessageKeyInfomation;

		public ZString EntryStatus => IncomingMessageKeyInfomation.EntryStatus;

		public ZString ClearanceStatus => IncomingMessageKeyInfomation.ClearanceStatus;

		public ZString EntryType => IncomingMessageKeyInfomation.EntryType;

		public ZString ReleaseStatus => IncomingMessageKeyInfomation.ReleaseStatus;

		public ZString MessageCode => EM_Calc_MessageTypeCode;

		public ZString MessageTypeDescription => TWMessageHelper.GetMessageTypeDescription(EM_MessageType);

		public List<ZString> GoodsShipmentValidationCode => IncomingMessageKeyInfomation.GoodsShipmentValidationCode;

		public List<ZString> GoodsShipmentNameCode => IncomingMessageKeyInfomation.GoodsShipmentNameCode;

		public List<ZString> StatementCode => IncomingMessageKeyInfomation.StatementCode;

		#region EM_Calc_MessageTypeCode
		[ResourceStringData("Enterprise.Customs.TW.Business.TWMessage|EM_Calc_MessageTypeCode", Caption = "Msg. Type Code")]
		public ZString EM_Calc_MessageTypeCode => MessageTypeCodeList.GetDescriptionFromCode(EM_MessageType);

		MessageTypeCodeList MessageTypeCodeList => Factory.GetCachedValue<MessageTypeCodeList>();
		#endregion

		#region EM_Calc_MessageTypeDescription
		[ResourceStringData("Enterprise.Customs.TW.Business.TWMessage|EM_Calc_MessageTypeDescription", Caption = "Msg. Type Desc.")]
		public ZString EM_Calc_MessageTypeDescription => MessageTypeDescriptionList.GetDescriptionFromCode(MessageTypeDescriptionCode);

		ZString MessageTypeDescriptionCode => EM_MessageType + EM_ReceiveTransmit;

		MessageTypeDescriptionList MessageTypeDescriptionList => Factory.GetCachedValue<MessageTypeDescriptionList>();
		#endregion
		#endregion
		#region Override Implementation
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.TaiwanCustoms;
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
		{
			return true;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.GetOutgoingTWCustomsMessageNumber().GetNextFormatted(Factory);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateMessageNumberIfNeeded();
		}

		internal void PopulateMessageNumberIfNeeded()
		{
			if (!IsTransmitMessage)
			{
				PopulateNumberPropertyIfRequired<ZString>(EM_MessageNumInfo, x => Env.NumberFountains.GetIncomingTWCustomsMessageNumber().GetNextFormatted(x));
			}
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(CusInBondHeader);
			yield return typeof(CusTWControllingMessageHeader);
			yield return typeof(CusEntryNumber);
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			var isTransmitTaiwanCustoms = IsTransmitTaiwanCustoms;
			if (isTransmitTaiwanCustoms)
			{
				var cusEntryHeader = CusEntryHeader;
				if (cusEntryHeader == null && ShouldLicensingMessageAllocateEntryNumber(EM_MessageType) && EM_LinkedObject is CusTWControllingMessageHeader messageHeader)
				{
					cusEntryHeader = messageHeader.Declaration.EntryHeader;
				}

				if (cusEntryHeader != null)
				{
					cusEntryHeader.AllocateEntryNumber();
					TWMessageHelper.FillMessagePlaceHolder(this, MessageConstants.EntryNumberPlaceHolder, cusEntryHeader.EntryNumber);
				}
			}
			switch (EM_MessageType)
			{
				case MessageTypeList.Codes.IEA:
				case MessageTypeList.Codes.ADM:
					SetFunctionalReferenceID();
					break;
				case MessageTypeList.Codes.TRA:
					FillInPlaceHoldersTranshipmentMessage();
					break;
				case MessageTypeList.Codes._101:
				case MessageTypeList.Codes._201:
				case MessageTypeList.Codes._207:
				case MessageTypeList.Codes._301:
				case MessageTypeList.Codes._31A:
				case MessageTypeList.Codes._31D:
				case MessageTypeList.Codes._401:
				case MessageTypeList.Codes._601:
				case MessageTypeList.Codes._603:
					SetFunctionalReferenceIDForLicensingMessage();
					break;
				case MessageTypeList.Codes.ICD:
					SetCMLicensingReferenceIDForLicensingMessage();
					break;
			}
			if (isTransmitTaiwanCustoms)
			{
				EM_MessageInterpretation = TWMessageHelper.NewOutgoingHelper(this)?.ToHtml() ?? ZString.Empty;
			}
		}

		void FillInPlaceHoldersTranshipmentMessage()
		{
			if (EM_LinkedObject is CusInBondHeader cusInBondHeader)
			{
				cusInBondHeader.AllocateEntryNumber();
				TWMessageHelper.FillMessagePlaceHolder(this, MessageConstants.EntryNumberPlaceHolder, cusInBondHeader.EntryNumber);
			}
		}

		protected override IStreamFormatter MessageStreamFormatter => new EDIMessageStreamFormatterForXml();

		bool IsTransmitTaiwanCustoms => EM_ApplicationCode == EDIMessage.ApplicationCodes.TaiwanCustoms && EM_ReceiveTransmit == Direction.Transmit;

		public static bool ShouldLicensingMessageAllocateEntryNumber(ZString messageType)
		{
			switch (messageType)
			{
				case MessageTypeList.Codes._301:
				case MessageTypeList.Codes._31A:
				case MessageTypeList.Codes._31D:
				case MessageTypeList.Codes._401:
				case MessageTypeList.Codes._601:
				case MessageTypeList.Codes._603:
					return true;
				default:
					return false;
			}
		}

		void SetFunctionalReferenceID()
		{
			var messageText = EM_MessageText;

			if (messageText.IndexOf(MessageConstants.FunctionalReferenceIDPlaceHolderHtml, StringComparison.Ordinal) != -1)
			{
				var functionalReferenceID = FunctionalReferenceIDNumberFountain.GetNext(Factory, CusEntryHeader.EntryNumber);
				messageText = messageText.Replace(MessageConstants.FunctionalReferenceIDPlaceHolderHtml, functionalReferenceID);
				EM_MessageText = messageText;
			}
		}

		void SetFunctionalReferenceIDForLicensingMessage()
		{
			if (EM_LinkedObject is CusTWControllingMessageHeader controllingMessageHeader)
			{
				var messageText = EM_MessageText;
				if (messageText.IndexOf(MessageConstants.FunctionalReferenceIDPlaceHolderHtml, StringComparison.Ordinal) != -1)
				{
					var functionalReferenceID = LicensingMessageFunctionalReferenceIDFountain.GetNext(Factory, controllingMessageHeader.VATNumber);
					EM_MessageText = messageText.Replace(MessageConstants.FunctionalReferenceIDPlaceHolderHtml, functionalReferenceID);

					controllingMessageHeader.TW1_FunctionalReferenceId = functionalReferenceID;
				}
			}
		}

		void SetCMLicensingReferenceIDForLicensingMessage()
		{
			if (EM_LinkedObject is CusEntryHeader entryHeader && entryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				var messageText = EM_MessageText;
				var hasChanged = false;

				foreach (var licensingHeader in entryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>())
				{
					var referenceIDPlaceHolder = SharedHelper.GetFunctionalReferenceIDPlaceHolderWithPKHtml(licensingHeader.PK);
					if (messageText.IndexOf(referenceIDPlaceHolder, StringComparison.Ordinal) != -1)
					{
						var functionalReferenceID = LicensingMessageFunctionalReferenceIDFountain.GetNext(Factory, licensingHeader.VATNumber, true);
						messageText = messageText.Replace(referenceIDPlaceHolder, functionalReferenceID);
						licensingHeader.TW1_FunctionalReferenceId = functionalReferenceID;
						hasChanged = true;
					}
				}

				if (hasChanged)
				{
					EM_MessageText = messageText;
				}
			}
		}
		#endregion
	}
}
