using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public partial class JobDeclarationMessageSendingObject : WCOJobDeclarationMessageSendingObject
{
	public JobDeclarationMessageSendingObject(EU.Business.Declaration.CusEntryHeader header) : base(header)
	{
	}
	public static class NLSchema
	{
		public const string Variant = nameof(Variant);
		public const string Description = nameof(Description);
		public const string ExportCustomsOffice = nameof(ExportCustomsOffice);
		public const string ExitCustomsOffice = nameof(ExitCustomsOffice);
		public const string SubStyle = nameof(SubStyle);
		public const string Date = nameof(Date);
		public const string Update = nameof(Update);
		public const string ReasonForInvalidation = nameof(ReasonForInvalidation);
		public const int ReasonForInvalidationMaxLength = 512;
	}

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public JobDeclaration Declaration => Header.Declaration;

	protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationMessageSendingObjectValidation(this);

	public new JobDeclarationMessageSendingObjectValidation Validation => (JobDeclarationMessageSendingObjectValidation)base.Validation;

	public JobDeclarationMessageSendingObjectLookups Lookups => new JobDeclarationMessageSendingObjectLookups(this);

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		MessageType = GetDefaultMessageType();
	}

	public bool SubStyleDEF => SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA || SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB || SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;

	bool StyleI2C2 => Style.In<ZString>(DeclarationTypeList.Codes.I2, DeclarationTypeList.Codes.C2);

	public ZString MessageTypeDescription => MessageTypesList.GetDescriptionFromCode(MessageType);

	protected override bool IsAmendCore => MessageType == ExportSendMessageTypes.Codes.AMD;

	#region MovementReferenceNumber
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|MovementReferenceNumber", Caption = "MRN")]
	public override ZString MovementReferenceNumber
	{
		get { return base.MovementReferenceNumber; }
	}
	#endregion

	#region Type
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|Type", Caption = "Type")]
	public override ZString DeclarationType => base.DeclarationType;
	#endregion

	#region Variant
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|Variant", Caption = "Variant")]
	public ZString Variant => Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

	public ZPropertyInfo VariantInfo => GetZPropertyInfo(NLSchema.Variant);
	#endregion

	#region Description
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|Description", Caption = "Description")]
	public ZString Description => Header.EntryInstruction?.CEI_Description ?? ZString.Empty;

	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(NLSchema.Description);
	#endregion

	#region EntryStatus
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|EntryStatus", Caption = "Entry Status")]
	public override ZString EntryStatus => base.EntryStatus;
	#endregion

	#region ExportCustomsOffice
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|ExportCustomsOffice", Caption = "Export Customs Office")]
	public ZString ExportCustomsOffice => Declaration?.JE_CustomsOffice ?? ZString.Empty;

	public ZPropertyInfo ExportCustomsOfficeInfo => GetZPropertyInfo(NLSchema.ExportCustomsOffice);
	#endregion

	#region ExitCustomsOffice
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|ExitCustomsOffice", Caption = "Exit Customs Office")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationMessageSendingObjectLookups.ExitCustomsOfficeList))]
	public ZString ExitCustomsOffice => Declaration?.CustomsOffices?.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit && x.CY_Type == EU.Business.CusCodeDataTypeList.Codes.OfficeCode && x.CY_Data != "")?.CY_Data ?? ZString.Empty;

	public ZPropertyInfo ExitCustomsOfficeInfo => GetZPropertyInfo(NLSchema.ExitCustomsOffice);
	#endregion

	#region SubStyle
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|SubStyle", Caption = "Sub Style")]
	public ZString SubStyle => Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

	ZString Style => Header.EntryInstruction?.CEI_Style ?? ZString.Empty;

	public ZPropertyInfo SubStyleInfo => GetZPropertyInfo(NLSchema.SubStyle);
	#endregion

	#region Date
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|Date", Caption = "Date")]
	public ZDate Date => Header?.CH_EntrySubmittedDate.Date ?? ZDate.Empty;

	public ZPropertyInfo DateInfo => GetZPropertyInfo(NLSchema.Date);
	#endregion

	#region Update
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|Update", Caption = "Update")]
	[ReadOnlyMember(nameof(Update_ReadOnly))]
	public ZBool Update
	{
		get
		{
			return update;
		}
		set
		{
			SetNonPersistentPropertyValue(UpdateInfo, ref update, value);
		}
	}
	ZBool update;

	public ZPropertyInfo UpdateInfo => GetZPropertyInfo(NLSchema.Update);

	bool Update_ReadOnly => IsMessageTypeDEC || IsMessageTypeSUP || IsMessageTypeAMD || IsMessageTypeFBK || IsMessageTypeCRE || IsMessageTypeCAN;
	#endregion

	#region MessageType

	[ResourceStringData("3C558FBD-8F8E-44CB-B812-D3769FCE3A44", ShortCaption = "Msg. Type", Caption = "Message Type")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationMessageSendingObjectLookups.MessageTypeList))]
	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			var oldValue = MessageType;
			base.MessageType = value;
			if (!oldValue.IsEmpty && oldValue != MessageType)
			{
				ShouldSend = false;
				Header.CH_EntrySubmittedDate = ZDateTime.Empty;
			}

			if (IsMessageTypeDEC || IsMessageTypeFBK || IsMessageTypeCAN)
			{
				Update = false;
			}
			else if (IsMessageTypeSUP || IsMessageTypeAMD || IsMessageTypeCRE)
			{
				Update = true;
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateReasonForInvalidation();
			}
			MessageTypeInfo.RefreshBinding();
		}
	}

	protected override bool MessageType_ReadOnly => false;

	bool IsMessageTypeDEC => MessageType == ExportSendMessageTypes.Codes.DEC;

	bool IsMessageTypeSUP => MessageType == ExportSendMessageTypes.Codes.SUP;

	bool IsMessageTypeAMD => MessageType == ExportSendMessageTypes.Codes.AMD;

	bool IsMessageTypeFBK => MessageType == ExportSendMessageTypes.Codes.FBK;

	bool IsMessageTypeCRE => MessageType == ExportSendMessageTypes.Codes.CRE;

	public bool IsMessageTypeCAN => MessageType == ExportSendMessageTypes.Codes.CAN;

	protected override ZString GetDefaultMessageType()
	{
		var result = ZString.Empty;
		if (Header.CH_EntrySubmittedDate.IsEmpty || Header.IsFailedFromTransmission)
		{
			result = StyleI2C2 || (SubStyleDEF &&
				(Header.IsImport || (Header.IsExport && Header.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes.REG && Header.CH_EntryStatus == EntryStatusNew.PreLodged)))
				? ExportSendMessageTypes.Codes.PRE : ExportSendMessageTypes.Codes.DEC;
		}

		if (Lookups.MessageTypeList.Count == 1)
		{
			result = Lookups.MessageTypeList[0].Code;
		}

		return result;
	}

	public override ZBool ShouldSend
	{
		get => base.ShouldSend;
		set
		{
			base.ShouldSend = value;
			Validation.ValidateAll();
		}
	}

	public CodeDescriptionPairList MessageTypesList => Header.IsImport ? new ImportSendMessageTypes() : new ExportSendMessageTypes();
	#endregion

	#region ReasonForInvalidation
	[ReadOnlyMember(nameof(ReasonForInvalidationReadOnly))]
	[MaxLength(NLSchema.ReasonForInvalidationMaxLength)]
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|ReasonForInvalidation", Caption = "Reason for Invalidation")]
	public ZString ReasonForInvalidation
	{
		get
		{
			return reasonForInvalidation;
		}
		set
		{
			SetNonPersistentPropertyValue(ReasonForInvalidationInfo, ref reasonForInvalidation, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateReasonForInvalidation();
			}
			MessageTypeInfo.RefreshBinding();
		}
	}
	ZString reasonForInvalidation;

	public ZPropertyInfo ReasonForInvalidationInfo => GetZPropertyInfo(nameof(ReasonForInvalidation));

	bool ReasonForInvalidationReadOnly => !IsMessageTypeCAN;
	#endregion

	#region ExitType
	[MaxLength(1)]
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|ExitType", Caption = "Exit Type")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationMessageSendingObjectLookups.ExitTypeList))]
	public ZString ExitType
	{
		get { return exitType; }
		set
		{
			SetNonPersistentPropertyValue(ExitTypeInfo, ref exitType, value);
			ExitTypeInfo.RefreshBinding();
		}
	}
	ZString exitType;

	public ZPropertyInfo ExitTypeInfo => GetZPropertyInfo(nameof(ExitType));
	#endregion

	#region ExitDate
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|ExitDate", Caption = "Exit Date")]
	public ZDateTime ExitDate
	{
		get { return exitDate; }
		set
		{
			SetNonPersistentPropertyValue(ExitDateInfo, ref exitDate, value);
			ExitDateInfo.RefreshBinding();
		}
	}
	ZDateTime exitDate;

	public ZPropertyInfo ExitDateInfo => GetZPropertyInfo(nameof(ExitDate));
	#endregion

	#region ZG_TypeOfSecurity
	[MaxLength(2)]
	[ResourceStringData("NPBO:Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject|ZG_TypeOfSecurity", Caption = "Security")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationMessageSendingObjectLookups.SecurityTypeList))]
	public ZString ZG_TypeOfSecurity
	{
		get { return Declaration.ZG_TypeOfSecurity; }
		set
		{
			CheckMaximumLength(ZG_TypeOfSecurityInfo, value);
			Declaration.ZG_TypeOfSecurity = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateZG_TypeOfSecurity();
			}
			ZG_TypeOfSecurityInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo ZG_TypeOfSecurityInfo => GetZPropertyInfo(nameof(ZG_TypeOfSecurity));
	#endregion

	#region Annotation
	[MaxLength(512)]
	public ZString Annotation
	{
		get { return annotation; }
		set
		{
			SetNonPersistentPropertyValue(AnnotationInfo, ref annotation, value);
		}
	}
	ZString annotation;

	public ZPropertyInfo AnnotationInfo => GetZPropertyInfo(nameof(Annotation));
	#endregion

	#region AlternativeEvidence
	[ChildEditable(true)]
	public AlternativeEvidenceCollection AlternativeEvidences
	{
		get
		{
			if (alternativeEvidences == null)
			{
				alternativeEvidences = new AlternativeEvidenceCollection(this);
				RegisterEditableChildObject(alternativeEvidences);
			}
			return alternativeEvidences;
		}
	}
	AlternativeEvidenceCollection alternativeEvidences;

	public bool HasEvidencesExitProof => alternativeEvidences.Any(x => ((AlternativeEvidence)x).EvidenceType.Equals(AAZAlternativeEvidenceTypeList.Codes.EXITPROOF));
	#endregion
}
