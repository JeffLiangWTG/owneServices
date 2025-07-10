using System.Data;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class NLEDIMessage : EDIMessage, Integration.Customs.NL.IEDIMessage
{
	public new class Schema : AutoEDIMessage.Schema
	{
		public const string CustomsMessageRemarks = "CustomsMessageRemarks";
	}

	public NLEDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region SuppressResourceStringsCheckRegion
	public const string WCOTypePlaceHolder = "<<WCO TYPE PLACEHOLDER>>";
	public const string WCOTypePlaceHolderHtml = "&lt;&lt;WCO TYPE PLACEHOLDER&gt;&gt;";
	#endregion

	protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.NLMessageControlNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory) : EM_MessageNum;

	public new static readonly EDIMessageTypeDecider TypeDecider = new EDIMessageTypeDecider();

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;
	}

	protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;

	#region EntryStatus

	public ZString EntryStatus => entryStatus ??= EM_MessageType == NLEDIMessageTypes.Codes.DMS && EM_ReceiveTransmit == EDIMessage.Direction.Receive && EM_MessageSubType != NLIncomingMessageSubTypeList.Codes.Control
		? DMSResponseMessageHelper.GetMessageEntryStatus((CusEntryHeader)EM_LinkedObject, DMSResponseMessageHelper.CreateDMSIncomingDataProvider(EM_MessageText))
		: ZString.Empty;
	string entryStatus;

	#endregion

	#region EntryStatusDescription

	public ZString EntryStatusDescription => entryStatusDescription ?? (entryStatusDescription = EntryStatusList.GetDescriptionFromCode(EntryStatus));
	string entryStatusDescription;

	#endregion

	CodeDescriptionPairList EntryStatusList => NLRefCusCodeListTypes.GetCustomsStatusList(Factory);

	public ZString GetOutgoingMessageInterpretation() => !EM_MessageText.IsEmpty ? Prettier.GetFormatted() : base.EM_MessageInterpretation;

	NLEDIMessagePrettier Prettier => prettier ?? (prettier = GetNLEDIMessagePrettierCore());
	NLEDIMessagePrettier prettier;

	protected virtual NLEDIMessagePrettier GetNLEDIMessagePrettierCore() => new NLEDIMessagePrettier(this);

	public override ZString EM_Status { get => base.EM_Status; set => base.EM_Status = value; }

	protected override string SendersReferencePlaceHolderOverride => EDIMessage.SendersReferencePlaceHolderHtml;

	protected override string GetSendersReference() => EM_MessageNum;

	protected override string MessageNumberPlaceHolderOverride => EDIMessage.MessageNumberPlaceHolderHtml;

	[CargoWiseOne.ResourceStrings.ResourceStringData("73D1657D-AA98-4764-9036-51F0C14AC3A5", Caption = "Message")]
	public override ZString EM_MessageSubType { get => base.EM_MessageSubType; set => base.EM_MessageSubType = value; }

	protected override EDIMessageValidation GetNewValidation() => new NLEDIMessageValidation(this);

	public new NLEDIMessageValidation Validation => (NLEDIMessageValidation)base.Validation;

	#region CustomsMessageRemarks
	public ZString CustomsMessageRemarks
	{
		get
		{
			return CustomsMessageRemarksNoteManager.Value;
		}
		set
		{
			CustomsMessageRemarksNoteManager.Value = value;
			CustomsMessageRemarksInfo.RefreshBinding();
		}
	}

	ProxiedNotePropertyManager CustomsMessageRemarksNoteManager
	{
		get { return customsMessageRemarksNoteManager ?? (customsMessageRemarksNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.CustomsMessageRemarks)); }
	}
	ProxiedNotePropertyManager customsMessageRemarksNoteManager;

	public ZPropertyInfo CustomsMessageRemarksInfo
	{
		get { return GetZPropertyInfo(Schema.CustomsMessageRemarks); }
	}

	[CargoWiseOne.ResourceStrings.ResourceStringData("FD18411E-364A-4F1D-82AF-5AB5B340CB66", Caption = "Statement Type")]
	public ZString StatementType
	{
		get
		{
			var notes = CustomsMessageRemarks.Split(new char[] { '|' }, 2);
			var statementType = ZString.Empty;
			if (notes.Length > 0)
			{
				statementType = notes[0];
			}
			return statementType;
		}
	}

	[CargoWiseOne.ResourceStrings.ResourceStringData("7EB55C41-B92B-4519-97CA-9E12BF9BAF59", Caption = "Statement Description")]
	public ZString StatementDescription
	{
		get
		{
			var notes = CustomsMessageRemarks.Split(new char[] { '|' }, 2);
			var statementDescription = ZString.Empty;
			if (notes.Length == 2)
			{
				statementDescription = notes[1];
			}
			return statementDescription;
		}
	}
	#endregion
}

public class NLEDIMessage<TNLMessageDataObject> : NLEDIMessage
where TNLMessageDataObject : class
{
	public NLEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public TNLMessageDataObject MessageDataObject => messageDataObject ?? (messageDataObject = GetMessageDataObject(EM_MessageText));

	TNLMessageDataObject messageDataObject;

	static TNLMessageDataObject GetMessageDataObject(ZString messageText)
	{
		return XmlObjectSerializer.Deserialize<TNLMessageDataObject>(messageText);
	}
}
