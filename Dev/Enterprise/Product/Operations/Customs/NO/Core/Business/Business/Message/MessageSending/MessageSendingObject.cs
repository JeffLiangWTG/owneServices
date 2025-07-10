using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public class MessageSendingObject : JobDeclarationMessageSendingObject
{
	public MessageSendingObject(CusEntryHeader header) : base(header)
	{
		if (!header.CH_ToCustomsControllingUnit.IsEmpty)
		{
			CustomsOffice = header.CH_ToCustomsControllingUnit;
		}
	}

	public new class Schema : AutoJobDeclarationMessageSendingObject.Schema
	{
		public const string Description = nameof(MessageSendingObject.Description);
		public const string Procedure = nameof(MessageSendingObject.Procedure);
		public const string CustomsOffice = nameof(MessageSendingObject.CustomsOffice);
		public const string EntryNumber = nameof(MessageSendingObject.EntryNumber);
		public const string PaymentMethod = nameof(MessageSendingObject.PaymentMethod);
	}

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new MessageSendingObjectValidation(this);

	public MessageSendingObjectLookups Lookups
	{
		get
		{
			if (fLookups == null || !IsLookupsCachedInBase)
			{
				fLookups = GetNewLookups();
			}
			return fLookups;
		}
	}
	MessageSendingObjectLookups fLookups;

	MessageSendingObjectLookups GetNewLookups()
	{
		return IsImport ? new ImportMessageSendingObjectLookups(this) : new ExportMessageSendingObjectLookups(this);
	}

	[ResourceStringData("9C795479-DE95-4974-B73B-CD2DB135AF1D", Caption = "Declaration Type")]
	public override ZString DeclarationType => base.DeclarationType;

	[ResourceStringData("539B3CEA-2B14-449E-B3D5-7A25A09622B2", Caption = "Description")]
	public ZString Description => Header?.EntryInstruction?.CEI_Description ?? ZString.Empty;

	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

	[ResourceStringData("D4B97F5A-C28C-4718-A7EC-50E538FF4EEF", Caption = "Procedure")]
	public ZString Procedure => Header?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;

	public ZPropertyInfo ProcedureInfo => GetZPropertyInfo(Schema.Procedure);

	[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.MessageTypeList))]
	[ResourceStringData("AD0997C1-9F56-4FC0-A01C-4C50D02A1144", Caption = "Message Type")]
	public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

	protected override ZString GetDefaultMessageType()
	{
		if (IsDigitollDeclaration())
		{
			return MessageSendingMessageTypes.Codes.Correction;
		}

		return Lookups.MessageTypeList.GetAllCodesZString().FirstOrDefault();
	}

	protected override bool MessageType_ReadOnly => false;

	[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.CustomsOfficeList))]
	[ResourceStringData("26A7057F-940D-486C-80DC-EAF4529EFF2A", Caption = "To Customs Office/Controlling Unit")]
	public ZString CustomsOffice
	{
		get { return customsOffice; }
		set { SetNonPersistentPropertyValue(CustomsOfficeInfo, ref customsOffice, value); }
	}
	ZString customsOffice;

	public ZPropertyInfo CustomsOfficeInfo => GetZPropertyInfo(Schema.CustomsOffice);

	[ResourceStringData("F0DCABEA-D2F4-45BE-852A-1768D9F2FE75", Caption = "Entry Status")]
	public override ZString EntryStatus => base.EntryStatus;

	[ResourceStringData("39939607-BCD6-4E2E-A236-8D0465833643", Caption = "Entry Number")]
	public ZString EntryNumber => Header?.EntryNumber ?? ZString.Empty;

	public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(Schema.EntryNumber);

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		ShouldSend = ShouldSendMessageToCustomCore();
		if (string.IsNullOrWhiteSpace(CustomsOffice))
		{
			CustomsOffice = Lookups.CustomsOfficeList.DefaultCode;
		}
	}

	[ResourceStringData("A2419C37-A01C-84A7-42C2-DE659318D010", Caption = "Payment Method", ShortCaption = "Pay")]
	public ZString PaymentMethod => Header.CH_PaymentMethod;

	public ZPropertyInfo PaymentMethodInfo => GetZPropertyInfo(Schema.PaymentMethod);

	public bool IsImport => Header?.Declaration?.IsImport ?? false;

	public bool IsFirstMessageToSend => Header.CH_BGMReference.IsEmpty;

	protected override bool ShouldSend_ReadOnly => !ShouldSendMessageToCustomCore();

	bool ShouldSendMessageToCustomCore()
	{
		return IsDigitollDeclaration() || !statusNotAllowedToSendMessageToCustom.Value.Contains(Header.CH_EntryStatus);
	}

	static readonly Lazy<ImmutableHashSet<string>> statusNotAllowedToSendMessageToCustom = new(() => ImmutableHashSet.Create(
		UniversalReferenceConstants.CusEntryStatus.MEM,
		UniversalReferenceConstants.CusEntryStatus.MEG,
		UniversalReferenceConstants.CusEntryStatus.MED,
		UniversalReferenceConstants.CusEntryStatus.TKR
	));

	bool IsDigitollDeclaration() =>
		Header.CH_EntryStatus == UniversalReferenceConstants.CusEntryStatus.MEM && GetHasDigitollGoodsNumber();

	bool GetHasDigitollGoodsNumber() =>
		Header.Declaration?.HasDigitollGoodsNumber ?? ZBool.False;
}
