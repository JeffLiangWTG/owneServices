using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using ExportSecurityTypeList = Enterprise.Customs.EU.Business.ExportSecurityTypeList;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObject(EU.Business.Declaration.CusEntryHeader header)
	: EU.Business.JobDeclarationMessageSendingObject(header)
{
#pragma warning disable CA1052
	public static class PLSchema
#pragma warning restore CA1052
	{
		#region SuppressResourceStringsCheckRegion

		public const string MessageStatus = nameof(BaseMessageSendingObject.MessageStatus);
		public const string EntryNumber = nameof(BaseMessageSendingObject.EntryNumber);
		public const string Action = nameof(BaseMessageSendingObject.Action);
		public const string ReferenceNumber = nameof(BaseMessageSendingObject.ReferenceNumber);
		public const string DeclarationDate = nameof(BaseMessageSendingObject.DeclarationDate);
		public const string AmendmentInvalidationReason = nameof(BaseMessageSendingObject.AmendmentInvalidationReason);
		public const string ShouldSend = nameof(BaseMessageSendingObject.ShouldSend);
		public const string EntryDescription = nameof(BaseMessageSendingObject.EntryDescription);
		public const string ResponseMessage = nameof(BaseMessageSendingObject.ResponseMessage);
		#endregion

		public const int AmendmentInvalidationReasonMaxLen = 512;
	}

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		EntryNumber = MovementReferenceNumber;
		LocalReferenceNumber = Header.CH_BGMReference;
		var entryStatus = Header.CH_EntryStatus;
		ShouldSend = entryStatus.IsEmpty || entryStatus == PLEntryStatusList.Codes.NPP;
		DeclarationDate = new ZDate(Header.EntryInstruction?.CEI_DateForDuty);
	}

	public override ZBool ShouldSend
	{
		get => base.ShouldSend;
		set
		{
			base.ShouldSend = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateDeclarationDate();
				Validation.ValidateEntryNumber();
			}
		}
	}

	[ReadOnly(true)]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|EntryDescription", Caption = "Entry Description")]
	public ZString EntryDescription
	{
		get
		{
			if (Header.EntryInstruction is CusEntryInstruction instruction)
			{
				return $"{instruction.CEI_SubStyle} : {instruction.DescriptionForDisplay}";
			}
			return ZString.Empty;
		}
	}
	public ZPropertyInfo EntryDescriptionInfo => GetZPropertyInfo(PLSchema.EntryDescription);

	[ReadOnly(true)]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|MessageStatus", Caption = "Status")]
	public ZString MessageStatus => Header.CH_Status;
	public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(PLSchema.MessageStatus);

	[ReadOnly(true)]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|EntryStatus", Caption = "Entry Status")]
	public override ZString EntryStatus => Header.CH_EntryStatus;

	[MaxLength(nameof(EntryNumberMaxLength))]
	[ReadOnlyMember(nameof(EntryNumberReadOnly))]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|EntryNumber", Caption = "MRN")]
	public virtual ZString EntryNumber
	{
		get => entryNumber;
		set
		{
			SetNonPersistentPropertyValue(EntryNumberInfo, ref entryNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateEntryNumber();
			}
		}
	}
	ZString entryNumber;

	public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(PLSchema.EntryNumber);

	[List(nameof(ActionList))]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|Action", Caption = "Action")]
	public ZString Action
	{
		get => action;
		set
		{
			if (action != value && GetMessagesForResponse(value).Take(2).ToArray() is var twoMessages)
			{
				ResponseMessage = twoMessages.Length == 1 ? twoMessages[0] : ZString.Empty;
			}

			SetNonPersistentPropertyValue(ActionInfo, ref action, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAction();
				Validation.ValidateDeclarationDate();
			}
			Security = SetDefaultSecurity(Header.Declaration);
		}
	}

	ZString action;
	public ZPropertyInfo ActionInfo => GetZPropertyInfo(PLSchema.Action);

	[ReadOnly(true)]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|ReferenceNumber", Caption = "Ref No.")]
	public ZString ReferenceNumber
	{
		get => referenceNumber;
		set => SetNonPersistentPropertyValue(ReferenceNumberInfo, ref referenceNumber, value);
	}
	ZString referenceNumber;
	public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(PLSchema.ReferenceNumber);

	[ResourceStringData("PLJobDeclarationMessageSendingObject|DeclarationDate", Caption = "Declaration Date")]
	[ReadOnly(true)]
	public ZDate DeclarationDate
	{
		get => declarationDate;
		set
		{
			SetNonPersistentPropertyValue(DeclarationDateInfo, ref declarationDate, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateDeclarationDate();
			}
		}
	}

	ZDate declarationDate;
	public ZPropertyInfo DeclarationDateInfo => GetZPropertyInfo(PLSchema.DeclarationDate);

	[MaxLength(PLSchema.AmendmentInvalidationReasonMaxLen)]
	public ZString AmendmentInvalidationReason
	{
		get => amendmentInvalidationReason;
		set
		{
			SetNonPersistentPropertyValue(AmendmentInvalidationReasonInfo, ref amendmentInvalidationReason, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAmendmentInvalidationReason();
			}
		}
	}

	ZString amendmentInvalidationReason;
	public ZPropertyInfo AmendmentInvalidationReasonInfo => GetZPropertyInfo(PLSchema.AmendmentInvalidationReason);

	public virtual CodeDescriptionPairList ActionList => new CodeDescriptionPairList();

	public new BaseMessageSendingObjectValidation Validation => (BaseMessageSendingObjectValidation)base.Validation;

	[ResourceStringData("PLJobDeclarationMessageSendingObject|Security", Caption = "Security")]
	[List(nameof(Lookups) + "." + nameof(BaseMessageSendingObjectLookups.SecurityList))]
	public ZString Security
	{
		get => security;
		set
		{
			SetNonPersistentPropertyValue(SecurityInfo, ref security, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateSecurity();
			}
		}
	}
	ZString security;

	public ZPropertyInfo SecurityInfo => GetZPropertyInfo(nameof(Security));

	[ResourceStringData("PLJobDeclarationMessageSendingObject|CorrectionAcceptance", Caption = "Correction Acceptance")]
	[List(nameof(Lookups) + "." + nameof(BaseMessageSendingObjectLookups.CorrectionAcceptanceList))]
	public ZString CorrectionAcceptance
	{
		get => correctionAcceptance;
		set
		{
			SetNonPersistentPropertyValue(CorrectionAcceptanceInfo, ref correctionAcceptance, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateCorrectionAcceptance();
			}
		}
	}
	ZString correctionAcceptance;

	public ZPropertyInfo CorrectionAcceptanceInfo => GetZPropertyInfo(nameof(CorrectionAcceptance));

	[MaxLength(AcceptanceCommentMaxLength)]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|AcceptanceComment", Caption = "Acceptance Comment")]
	public virtual ZString AcceptanceComment
	{
		get => acceptanceComment;
		set
		{
			SetNonPersistentPropertyValue(AcceptanceCommentInfo, ref acceptanceComment, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAcceptanceComment();
			}
		}
	}
	ZString acceptanceComment;

	public ZPropertyInfo AcceptanceCommentInfo => GetZPropertyInfo(nameof(AcceptanceComment));

	[List(nameof(Lookups) + "." + nameof(BaseMessageSendingObjectLookups.MessageNumList))]
	[ResourceStringData("PLJobDeclarationMessageSendingObject|ResponseMessage", Caption = "Response to Message No.")]
	public ZString ResponseMessage
	{
		get => responseMessage;
		set
		{
			SetNonPersistentPropertyValue(ResponseMessageInfo, ref responseMessage, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateResponseMessage();
			}
		}
	}

	ZString responseMessage;
	public ZPropertyInfo ResponseMessageInfo => GetZPropertyInfo(nameof(ResponseMessage));

	public BaseMessageSendingObjectLookups Lookups
	{
		get
		{
			if (lookups == null || !IsLookupsCachedInBase)
			{
				lookups = GetNewLookups();
			}

			return lookups;
		}
	}
	BaseMessageSendingObjectLookups lookups;

	public IEnumerable<ZString> GetMessagesForResponse(ZString action) => action.ToString() switch
	{
		ExportMessageSendingObjectActionList.Codes.CC566 => GetMessageNums(AESMessageCodes.Descriptions.CC560),
		ExportMessageSendingObjectActionList.Codes.CC583 => GetMessageNums(AESMessageCodes.Descriptions.CC582),
		_ => [],
	};

	IEnumerable<ZString> GetMessageNums(string messageSubType)
		=> Header.Messages.Where(m => m.EM_MessageSubType == messageSubType).OrderBy(m => m.EM_SystemCreateTimeUtc).Select(m => m.EM_MessageNum);

	protected virtual BaseMessageSendingObjectLookups GetNewLookups() => new BaseMessageSendingObjectLookups(this);

	protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() =>
		new BaseMessageSendingObjectValidation(this);

	protected virtual bool EntryNumberReadOnly => !Header?.MovementReferenceNumber.IsEmpty ?? false;

	protected virtual int EntryNumberMaxLength => 35;

	public int ExpectedEntryNumberLength => GetExpectedEntryNumberLengthCore();

	protected virtual int GetExpectedEntryNumberLengthCore() => 35;

	const int AcceptanceCommentMaxLength = 512;

	ZString SetDefaultSecurity(JobDeclaration declaration) => declaration switch
	{
		null => ZString.Empty,
		_ when BaseMessageSendingObjectSecurityValidationHelper.IsRuleR0095Valid(declaration) => ExportSecurityTypeList.Codes.EXS,
		_ => ZString.Empty,
	};
}
