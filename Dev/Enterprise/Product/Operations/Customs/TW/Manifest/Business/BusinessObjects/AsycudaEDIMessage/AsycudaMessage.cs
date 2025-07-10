using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Manifest.MessageProcessors;
using Enterprise.Environment;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaMessage : AsycudaEDIMessage
	{
		public AsycudaMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.FHM;
		}

		public new static readonly AsycudaMessageTypeDecider TypeDecider = new AsycudaMessageTypeDecider();

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_ApplicationCode", Caption = "Message Application Code", ShortCaption = "Msg. App. Code")]
		public override ZString EM_ApplicationCode { get => base.EM_ApplicationCode; set => base.EM_ApplicationCode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_MessageOwner", Caption = "Message Owner", ShortCaption = "Msg. Owner")]
		public override ZString EM_MessageOwner { get => base.EM_MessageOwner; set => base.EM_MessageOwner = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_MessageType", Caption = "Message Type", ShortCaption = "Msg. Type")]
		public override ZString EM_MessageType { get => base.EM_MessageType; set => base.EM_MessageType = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_MessageSubType", Caption = "Message Sub Type", ShortCaption = "Msg. Sub Type")]
		public override ZString EM_MessageSubType { get => base.EM_MessageSubType; set => base.EM_MessageSubType = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_ReceiveTransmit", Caption = "Message Direction", ShortCaption = "Msg. Dir")]
		public override ZString EM_ReceiveTransmit { get => base.EM_ReceiveTransmit; set => base.EM_ReceiveTransmit = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_MessageNum", Caption = "Message Number", ShortCaption = "Msg. No.")]
		public override ZString EM_MessageNum { get => base.EM_MessageNum; set => base.EM_MessageNum = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_ApplicationReference", Caption = "Application Reference", ShortCaption = "App. Ref.")]
		public override ZString EM_ApplicationReference { get => base.EM_ApplicationReference; set => base.EM_ApplicationReference = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_Status", Caption = "Message Status", ShortCaption = "Msg. Status")]
		public override ZString EM_Status { get => base.EM_Status; set => base.EM_Status = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_SendWithMessageErrors", Caption = "Send with error")]
		public override ZBool EM_SendWithMessageErrors { get => base.EM_SendWithMessageErrors; set => base.EM_SendWithMessageErrors = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_SystemCreateTimeUtc", Caption = "Message Create Time (UTC)", ShortCaption = "Msg. Create Time (UTC)")]
		public override ZDateTime EM_SystemCreateTimeUtc { get => base.EM_SystemCreateTimeUtc; set => base.EM_SystemCreateTimeUtc = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|SystemCreateTimeUtcPlusEight", Caption = "Message Create Time (UTC+8)", ShortCaption = "Msg. Create Time")]
		public ZDateTime SystemCreateTimeUtcPlusEight { get => EM_SystemCreateTimeUtc.IsEmpty ? ZDateTime.Empty : EM_SystemCreateTimeUtc.AddHours(8); }

		public ZPropertyInfo SystemCreateTimeUtcPlusEightInfo => GetZPropertyInfo(nameof(SystemCreateTimeUtcPlusEight));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_SystemCreateUser", Caption = "Message Create User", ShortCaption = "Msg. Create User")]
		public override ZString EM_SystemCreateUser { get => base.EM_SystemCreateUser; set => base.EM_SystemCreateUser = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_SystemLastEditTimeUtc", Caption = "Message Last Edit Time (UTC)", ShortCaption = "Msg. Last Edit Time (UTC)")]
		public override ZDateTime EM_SystemLastEditTimeUtc { get => base.EM_SystemLastEditTimeUtc; set => base.EM_SystemLastEditTimeUtc = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|SystemLastEditTimeUtcPlusEight", Caption = "Message Last Edit Time (UTC+8)", ShortCaption = "Msg. Last Edit Time")]
		public ZDateTime SystemLastEditTimeUtcPlusEight { get => EM_SystemLastEditTimeUtc.IsEmpty ? ZDateTime.Empty : EM_SystemLastEditTimeUtc.AddHours(8); }

		public ZPropertyInfo SystemLastEditTimeUtcPlusEightInfo => GetZPropertyInfo(nameof(SystemLastEditTimeUtcPlusEight));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|EM_SystemLastEditUser", Caption = "Message Last Edit User", ShortCaption = "Msg. Last Edit User")]
		public override ZString EM_SystemLastEditUser { get => base.EM_SystemLastEditUser; set => base.EM_SystemLastEditUser = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeApplicationCode", Caption = "Interchange Application Code", ShortCaption = "Int. App. Code")]
		public ZString InterchangeApplicationCode => Interchange?.EI_ApplicationCode ?? ZString.Empty;

		public ZPropertyInfo InterchangeApplicationCodeInfo => (Interchange?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(InterchangeApplicationCode)) : GetWrappedZPropertyInfo(nameof(InterchangeApplicationCode), x => Interchange.EI_ApplicationCodeInfo);

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeSender", Caption = "From")]
		public ZString InterchangeSender => EM_InterchangeSender;

		public ZPropertyInfo InterchangeSenderInfo => GetZPropertyInfo(nameof(InterchangeSender));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeReceiver", Caption = "To")]
		public ZString InterchangeReceiver => EM_InterchangeReceiver;

		public ZPropertyInfo InterchangeReceiverInfo => GetZPropertyInfo(nameof(InterchangeReceiver));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeStatus", Caption = "Interchange Status", ShortCaption = "Int. Status")]
		public ZString InterchangeStatus => EM_InterchangeStatus;

		public ZPropertyInfo InterchangeStatusInfo => GetZPropertyInfo(nameof(InterchangeStatus));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeNumber", Caption = "Interchange Number", ShortCaption = "Int. No.")]
		public ZString InterchangeNumber => EM_InterchangeNumber;

		public ZPropertyInfo InterchangeNumberInfo => GetZPropertyInfo(nameof(InterchangeNumber));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeDeliveredTime", Caption = "Delivered Time")]
		public ZDateTimeOffset InterchangeDeliveredTime => Interchange?.EI_DeliveredTime ?? ZDateTimeOffset.Empty;

		public ZPropertyInfo InterchangeDeliveredTimeInfo => (Interchange?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(InterchangeDeliveredTime)) : GetWrappedZPropertyInfo(nameof(InterchangeDeliveredTime), x => Interchange.EI_DeliveredTimeInfo);

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeCreateTime", Caption = "Interchange Create Time (UTC)", ShortCaption = "Int. Create Time (UTC)")]
		public ZDateTime InterchangeCreateTime => Interchange?.EI_SystemCreateTimeUtc ?? ZDateTime.Empty;

		public ZPropertyInfo InterchangeCreateTimeInfo => (Interchange?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(InterchangeCreateTime)) : GetWrappedZPropertyInfo(nameof(InterchangeCreateTime), x => Interchange.EI_SystemCreateTimeUtcInfo);

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeCreateTimePlusEight", Caption = "Interchange Create Time (UTC+8)", ShortCaption = "Int. Create Time")]
		public ZDateTime InterchangeCreateTimePlusEight => InterchangeCreateTime.IsEmpty ? ZDateTime.Empty : InterchangeCreateTime.AddHours(8);

		public ZPropertyInfo InterchangeCreateTimePlusEightInfo => GetZPropertyInfo(nameof(InterchangeCreateTimePlusEight));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeCreateUser", Caption = "Interchange Create User", ShortCaption = "Int. Create User")]
		public ZString InterchangeCreateUser => Interchange?.EI_SystemCreateUser ?? ZString.Empty;

		public ZPropertyInfo InterchangeCreateUserInfo => (Interchange?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(InterchangeCreateUser)) : GetWrappedZPropertyInfo(nameof(InterchangeCreateUser), x => Interchange.EI_SystemCreateUserInfo);

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeLastEditTime", Caption = "Interchange Last Edit Time (UTC)", ShortCaption = "Int. Last Edit Time (UTC)")]
		public ZDateTime InterchangeLastEditTime => Interchange?.EI_SystemLastEditTimeUtc ?? ZDateTime.Empty;

		public ZPropertyInfo InterchangeLastEditTimeInfo => (Interchange?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(InterchangeLastEditTime)) : GetWrappedZPropertyInfo(nameof(InterchangeLastEditTime), x => Interchange.EI_SystemLastEditTimeUtcInfo);

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeLastEditTimePlusEight", Caption = "Interchange Last Edit Time (UTC+8)", ShortCaption = "Int. Last Edit Time")]
		public ZDateTime InterchangeLastEditTimePlusEight => InterchangeLastEditTime.IsEmpty ? ZDateTime.Empty : InterchangeLastEditTime.AddHours(8);

		public ZPropertyInfo InterchangeLastEditTimePlusEightInfo => GetZPropertyInfo(nameof(InterchangeLastEditTimePlusEight));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaMessage|InterchangeLastEditUser", Caption = "Interchange Last Edit User", ShortCaption = "Int. Last Edit User")]
		public ZString InterchangeLastEditUser => Interchange?.EI_SystemLastEditUser ?? ZString.Empty;

		public ZPropertyInfo InterchangeLastEditUserInfo => (Interchange?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(InterchangeLastEditUser)) : GetWrappedZPropertyInfo(nameof(InterchangeLastEditUser), x => Interchange.EI_SystemLastEditUserInfo);

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.GetOutgoingTWCustomsMessageNumber().GetNextFormatted(Factory);
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			var functionalReferenceId = N5101HFunctionalReferenceIdGenerator.GetFunctionalReferenceId(Factory);
			Bills?.ForEach(b => b.EntryNumber = functionalReferenceId);
			ReplaceFunctionalReferenceId(functionalReferenceId);
			EM_MessageInterpretation = TWManifestMessageHelper.NewOutgoingHelper(this)?.ToHtml() ?? ZString.Empty;
		}

		public IEnumerable<AsycudaBill> Bills { get; set; }

		void ReplaceFunctionalReferenceId(ZString functionalReferenceId) => EM_MessageText = EM_MessageText.Replace(MessageConstants.FunctionalReferenceIDPlaceHolderHtml, functionalReferenceId);
	}
}
