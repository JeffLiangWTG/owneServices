using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class MessageSendingObject : NctsHeaderMessageSendingObject
{
	public MessageSendingObject(NctsHeader nctsHeader) : base(Argument.NotNull(nctsHeader, nameof(nctsHeader)))
	{
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			SetMessageSendingDefaultValues();
		}
	}

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public new class Schema : NctsHeaderMessageSendingObject.Schema
	{
		public const string MovementReferenceNumber = nameof(MessageSendingObject.MovementReferenceNumber);
		public const string Description = nameof(MessageSendingObject.Description);
		public const string AmendmentType = nameof(MessageSendingObject.AmendmentType);
		public const string PresentationDateTime = nameof(MessageSendingObject.PresentationDateTime);
		public const string TirPageNumber = nameof(MessageSendingObject.TirPageNumber);
		public const string TirUnloadingNumber = nameof(MessageSendingObject.TirUnloadingNumber);
		public const string GoodsLocation = nameof(MessageSendingObject.GoodsLocation);
		public const string TransportIdentification = nameof(MessageSendingObject.TransportIdentification);
		public const string PlaceOfLoading = nameof(MessageSendingObject.PlaceOfLoading);
	}

	[ResourceStringData("PLNCTSMessageSendingObject|MovementReferenceNumber", Caption = "Movement Reference Number", ShortCaption = "MRN")]
	[ReadOnlyMember(nameof(MovementReferenceNumber_ReadOnly))]
	[MaxLength(AutoNctsHeaderMessageSendingObject.Schema.MRNMaxLength)]
	public ZString MovementReferenceNumber
	{
		get => movementReferenceNumber;
		set
		{
			SetNonPersistentPropertyValue(MovementReferenceNumberInfo, ref movementReferenceNumber, value);
			MovementReferenceNumberInfo.RefreshBinding();
		}
	}
	ZString movementReferenceNumber;

	public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

	bool MovementReferenceNumber_ReadOnly => !(MessageType == DepartureMessageSendingObjectTypeList.Codes.AMD && MRN.IsEmpty);

	protected override bool DepartureOfficeOfEnquiry_ReadOnly => base.DepartureOfficeOfEnquiry_ReadOnly || MessageType != DepartureMessageSendingObjectTypeList.Codes.RNM;

	[ResourceStringData("PLNCTSMessageSendingObject|Description", Caption = "Description", ShortCaption = "Descr.")]
	public ZString Description
	{
		get => description;
		private set
		{
			SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
		}
	}
	ZString description;

	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

	[ResourceStringData("CB904C67-2A0B-444B-B227-084223813E68", Caption = "Location of Goods")]
	public ZString GoodsLocation => NctsHeader.MovementHeader?.GoodsLocationDescription ?? ZString.Empty;

	public ZPropertyInfo GoodsLocationInfo => GetZPropertyInfo(Schema.GoodsLocation);

	public ZString PlaceOfLoading => NctsHeader.MovementHeader.BM_PortOfPresentationCode;

	public ZPropertyInfo PlaceOfLoadingInfo => GetZPropertyInfo(Schema.PlaceOfLoading);

	[ResourceStringData("19DDCAE4-2550-45A4-A51E-CE9AD00A135C", Caption = "Transport Identification")]
	public ZString TransportIdentification => NctsHeader.MovementHeader?.BM_TransportAtDeparture ?? ZString.Empty;

	public ZPropertyInfo TransportIdentificationInfo => GetZPropertyInfo(Schema.TransportIdentification);

	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			var oldValue = MessageType;
			if (!IsCopying && oldValue != value)
			{
				base.MessageType = value;
				Description = Lookups.MessageTypeList.GetDescriptionFromCode(value);
				OnMessageTypeChangedDefaults();
			}
		}
	}

	protected override bool Justification_ReadOnly => false;

	[ResourceStringData("5640E5C6-8C6F-4445-BE56-ACD0D01E7CD1", Caption = "Amendment Type")]
	[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.AmendmentTypeList))]
	public ZString AmendmentType
	{
		get => amendmentType;
		set
		{
			SetNonPersistentPropertyValue(AmendmentTypeInfo, ref amendmentType, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAmendmentType();
			}
			AmendmentTypeInfo.RefreshBinding();
		}
	}
	ZString amendmentType;

	public ZPropertyInfo AmendmentTypeInfo => GetZPropertyInfo(Schema.AmendmentType);

	[ResourceStringData("3F386907-F86B-4DF6-8375-0F8B91190F25", Caption = "Presentation Date and Time")]
	public ZDateTime PresentationDateTime
	{
		get => presentationDateTime;
		set
		{
			SetNonPersistentPropertyValue(PresentationDateTimeInfo, ref presentationDateTime, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidatePresentationDateTime();
			}
			PresentationDateTimeInfo.RefreshBinding();
		}
	}
	ZDateTime presentationDateTime;

	public ZPropertyInfo PresentationDateTimeInfo => GetZPropertyInfo(Schema.PresentationDateTime);

	public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)GetNewValidation();

	protected override NctsHeaderMessageSendingObjectValidation GetNewValidation() => new MessageSendingObjectValidation(this);

	public new MessageSendingObjectLookups Lookups => (MessageSendingObjectLookups)base.Lookups;

	protected override NctsHeaderMessageSendingObjectLookups GetNewLookups() => new MessageSendingObjectLookups(this);

	void SetMessageSendingDefaultValues()
	{
		Description = ZString.Empty;
		OnMessageTypeChangedDefaults();
	}

	void OnMessageTypeChangedDefaults()
	{
		Justification = ZString.Empty;
		MovementReferenceNumber = MRN;
		TirPageNumber = ZString.Empty;
		TirUnloadingNumber = ZString.Empty;
	}

	[MaxLength(2)]
	[ResourceStringData("24FF2BE9-91D6-4D87-82F4-1EADA099106A", Caption = "TIR Page Number")]
	[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.TirPageNumberTypeList))]
	[BusinessObjectTestExclude]
	public ZString TirPageNumber
	{
		get => tirPageNumber;
		set
		{
			SetNonPersistentPropertyValue(TirPageNumberInfo, ref tirPageNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateTirPageNumber();
			}
			TirPageNumberInfo.RefreshBinding();
		}
	}
	ZString tirPageNumber;

	public ZPropertyInfo TirPageNumberInfo => GetZPropertyInfo(Schema.TirPageNumber);

	[MaxLength(2)]
	[ResourceStringData("EB32CB09-85E6-4C4D-B698-7F3EF26D5294", Caption = "TIR Unloading Number")]
	[List(nameof(Lookups) + "." + nameof(MessageSendingObjectLookups.TirUnloadingNumberTypeList))]
	public ZString TirUnloadingNumber
	{
		get => tirUnloadingNumber;
		set
		{
			SetNonPersistentPropertyValue(TirUnloadingNumberInfo, ref tirUnloadingNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateTirUnloadingNumber();
			}
			TirUnloadingNumberInfo.RefreshBinding();
		}
	}
	ZString tirUnloadingNumber;

	public ZPropertyInfo TirUnloadingNumberInfo => GetZPropertyInfo(Schema.TirUnloadingNumber);
}
