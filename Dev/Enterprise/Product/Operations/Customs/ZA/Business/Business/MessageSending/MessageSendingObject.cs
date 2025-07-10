using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class MessageSendingObject : JobDeclarationMessageSendingObject, IVOCAfterValues, IVOCBeforeValues, IJobDeclarationSendingObjectWarehouseProvider
	{
		public new class Schema : JobDeclarationMessageSendingObject.Schema
		{
			public const string CaseNumber = "CaseNumber";
			public const int CaseNumberMaxLength = 35;
			public const string DocumentMessageSource = "DocumentMessageSource";
			public const int DocumentMessageSourceMaxLength = 35;

			public const string CustomsProcedureCode = "CustomsProcedureCode";
			public const string UniqueConsignmentReferenceNumber = "UniqueConsignmentReferenceNumber";
			public const string MessageStatus = "MessageStatus";
			public const string PaymentMethod = "PaymentMethod";

			public const string CIFValueBefore = "CIFValueBefore";
			public const string CIFValueAfter = "CIFValueAfter";
			public const string CIFValueDifference = "CIFValueDifference";
			public const string CustomsValueBefore = "CustomsValueBefore";
			public const string CustomsValueAfter = "CustomsValueAfter";
			public const string CustomsValueDifference = "CustomsValueDifference";
			public const string CustomsDutyNoS1P2BBefore = "CustomsDutyNoS1P2BBefore";
			public const string CustomsDutyNoS1P2BAfter = "CustomsDutyNoS1P2BAfter";
			public const string CustomsDutyNoS1P2BDifference = "CustomsDutyNoS1P2BDifference";
			public const string S1P2BDutyBefore = "S1P2BDutyBefore";
			public const string S1P2BDutyAfter = "S1P2BDutyAfter";
			public const string S1P2BDutyDifference = "S1P2BDutyDifference";
			public const string ValueAddedTaxBefore = "ValueAddedTaxBefore";
			public const string ValueAddedTaxAfter = "ValueAddedTaxAfter";
			public const string ValueAddedTaxDifference = "ValueAddedTaxDifference";
			public const string ProvisionalPaymentAmountBefore = "ProvisionalPaymentAmountBefore";
			public const string ProvisionalPaymentAmountAfter = "ProvisionalPaymentAmountAfter";
			public const string ProvisionalPaymentAmountDifference = "ProvisionalPaymentAmountDifference";
			public const string PenaltyAmountBefore = "PenaltyAmountBefore";
			public const string PenaltyAmountAfter = "PenaltyAmountAfter";
			public const string PenaltyAmountDifference = "PenaltyAmountDifference";
			public const string AmountDueBefore = "AmountDueBefore";
			public const string AmountDueAfter = "AmountDueAfter";
			public const string AmountDueDifference = "AmountDueDifference";
		}

		public MessageSendingObject(CusEntryHeader header)
			: base(header)
		{
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		#region Override Properties

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new MessageSendingObjectValidation(this);
		}

		public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

		#region ShouldSend

		public override ZBool ShouldSend
		{
			get
			{
				return base.ShouldSend;
			}
			set
			{
				base.ShouldSend = value;
				if (!ShouldSend)
				{
					VOCReason = ZString.Empty;
				}
			}
		}

		internal ZDateTime LastMessageSentTime
		{
			get
			{
				if (!lastMessageSentTime.HasValue)
				{
					lastMessageSentTime = Header.Messages.LastOutgoingMessage?.EM_SystemCreateTimeUtc ?? ZDateTime.Empty;
				}
				return lastMessageSentTime.Value;
			}
		}
		ZDateTime? lastMessageSentTime;

		#endregion

		#region MessageType

		[List(nameof(MessageTypesList))]
		public override ZString MessageType
		{
			get { return base.MessageType; }
			set
			{
				var oldValue = MessageType;
				base.MessageType = value;
				MessageKeyFactor.MessageType = value;
				if (oldValue != MessageType)
				{
					if (!MessageDataProviderInstruction.ShouldOutputCaseNumber(MessageKeyFactor))
					{
						CaseNumber = string.Empty;
					}
					else if (CaseNumber.IsEmpty)
					{
						DefaultCaseNumberFromEntry();
					}

					if (MessageType == MessageSubTypeCodes.Codes.Original || MessageType == MessageSubTypeCodes.Codes.Replace)
					{
						VOCReason = ZString.Empty;
					}

					if (!IsLRNEditable)
					{
						LocalReferenceNumber = Header.CH_BGMReference;
					}

					if (MessageDataProviderInstruction.ShouldOutputMRNToBeReplaced(MessageKeyFactor) && Header.MovementReferenceNumber.IsEmpty)
					{
						MovementReferenceNumber = Header.EntryInstruction?.CEI_MRNToBeReplaced ?? ZString.Empty;
					}
					else if (!IsMRNEditable)
					{
						MovementReferenceNumber = Header.MovementReferenceNumber;
					}

					ChangeAcknowledgementIndicator = MessageDataProviderInstruction.ShouldOutputChangeAcknowledgementIndicator(MessageKeyFactor) ? ChangeAcknowledgementIndicator : ZString.Empty;
				}
			}
		}

		protected override bool MessageType_ReadOnly => false;

		#endregion

		#region CaseNumber

		ZString caseNumber;
		[List(nameof(CaseNumbers))]
		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|CaseNumber", Caption = "Case Number")]
		[ReadOnlyMember(nameof(CaseNumber_ReadOnly))]
		public ZString CaseNumber
		{
			get => caseNumber;
			set
			{
				CheckMaximumLength(CaseNumberInfo, value);
				SetNonPersistentPropertyValue(CaseNumberInfo, ref caseNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCaseNumber();
				}
			}
		}

		protected bool CaseNumber_ReadOnly => !MessageDataProviderInstruction.ShouldOutputCaseNumber(MessageKeyFactor);

		public ZPropertyInfo CaseNumberInfo => GetZPropertyInfo(Schema.CaseNumber);

		public CodeDescriptionPairList CaseNumbers
		{
			get
			{
				var entryInstruction = Header.EntryInstruction ?? Factory.GetNull<CusEntryInstruction>();

				var caseNumberList = new List<CusCodeData>();
				entryInstruction.CaseNumbers.CopyToList(caseNumberList);
				caseNumberList = caseNumberList.Where(x => x.CY_Date.IsEmpty).ToList();

				var activeCaseNumbers = new CodeDescriptionPairList();
				activeCaseNumbers.AddRange(caseNumberList);
				return activeCaseNumbers;
			}
		}

		#endregion

		#region ChangeAcknowledgementIndicator

		[List(nameof(ChangeAcknowledgementIndicatorList))]
		public override ZString ChangeAcknowledgementIndicator
		{
			get { return base.ChangeAcknowledgementIndicator; }
			set { base.ChangeAcknowledgementIndicator = value; }
		}

		protected override bool ChangeAcknowledgementIndicator_ReadOnly => !MessageDataProviderInstruction.ShouldOutputChangeAcknowledgementIndicator(MessageKeyFactor);

		#endregion

		#region VOCReason

		protected override bool VOCReason_ReadOnly
		{
			get
			{
				return !(ShouldSend && (MessageType.Equals(MessageSubTypeCodes.Codes.Change) || MessageType.Equals(MessageSubTypeCodes.Codes.Cancellation)));
			}
		}

		#endregion

		#region LocalReferenceNumber

		protected override bool LocalReferenceNumber_ReadOnly => !IsLRNEditable;

		public void RefreshLocalReferenceNumber()
		{
			LocalReferenceNumber = Header.CH_BGMReference;
		}

		#endregion

		#region MovementReferenceNumber

		protected override bool MovementReferenceNumber_ReadOnly => !IsMRNEditable;

		#endregion

		#region DocumentMessageSource
		[ReadOnlyMember(nameof(DocumentMessageSource_ReadOnly))]
		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|DocumentMessageSource", ShortCaption = "DMS", Caption = "Document Message Source")]
		[MaxLength(Schema.DocumentMessageSourceMaxLength)]
		public ZString DocumentMessageSource
		{
			get => documentMessageSource;
			set
			{
				CheckMaximumLength(DocumentMessageSourceInfo, value);
				SetNonPersistentPropertyValue(DocumentMessageSourceInfo, ref documentMessageSource, value);
			}
		}
		public ZPropertyInfo DocumentMessageSourceInfo => GetZPropertyInfo(Schema.DocumentMessageSource);

		protected bool DocumentMessageSource_ReadOnly => false;

		ZString documentMessageSource;
		#endregion

		#region DeclarationType

		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|DeclarationType", Caption = "Declaration Type")]
		[MaxLength(Schema.DeclarationTypeMaxLength)]
		[List(nameof(DeclarationTypeList))]
		[ReadOnlyMember(nameof(DeclarationType_ReadOnly))]
		public new ZString DeclarationType
		{
			get => declarationType;
			set
			{
				CheckMaximumLength(DeclarationTypeInfo, value);
				SetNonPersistentPropertyValue(DeclarationTypeInfo, ref declarationType, value);
				MessageKeyFactor.DeclarationType = ((ICUSDECMessageDataProvider)this).DeclarationType;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeclarationType();
				}
			}
		}
		ZString declarationType;

		bool DeclarationType_ReadOnly => !TwoStepDeclarationHelper.IsTwoStepClearingValid || Declaration.IsExWarehouse;

		#endregion

		#endregion

		#region New Properties

		#region CustomsProcedureCode

		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|CustomsProcedureCode", ShortCaption = "CPC", Caption = "Customs Procedure Code")]
		public ZString CustomsProcedureCode => Header.CustomsProcedureCode;

		public ZPropertyInfo CustomsProcedureCodeInfo => GetZPropertyInfo(Schema.CustomsProcedureCode);

		#endregion

		#region UniqueConsignmentReferenceNumber

		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|UniqueConsignmentReferenceNumber", ShortCaption = "UCR", Caption = "Unique Consignment Reference Number")]
		public ZString UniqueConsignmentReferenceNumber => Header.UniqueConsignmentReference;

		public ZPropertyInfo UniqueConsignmentReferenceNumberInfo => GetZPropertyInfo(Schema.UniqueConsignmentReferenceNumber);

		#endregion

		#region MessageStatus

		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|MessageStatus", ShortCaption = "Msg. Status", Caption = "Message Status")]
		public ZString MessageStatus => Header.CH_Status;

		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(Schema.MessageStatus);

		#endregion

		#region PaymentMethod

		[ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|PaymentMethod", ShortCaption = "Pay Code", Caption = "Payment Code")]
		public ZString PaymentMethod => Header.CH_PaymentMethod;

		public ZPropertyInfo PaymentMethodInfo => GetZPropertyInfo(Schema.PaymentMethod);

		public bool PaymentMethodIsDeferOrVAT => PaymentMethod == PaymentMethodCodeList.Codes.Defer || PaymentMethod == PaymentMethodCodeList.Codes.VATOnly;

		#endregion

		#region SubmissionDate

		public ZDateTime SubmissionDate { get; set; }

		#endregion

		#region CIFValue

		IVOCAfterValues VOCAfterValues
		{
			get
			{
				if (MessageType == MessageSubTypeCodes.Codes.Cancellation ||
					DeclarationType == ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration ||
					DeclarationType == ZA.Business.DeclarationTypeList.Codes.RegularProvisionalDeclaration)
				{
					return new EmptyVOCAfterValues();
				}

				return Header;
			}
		}

		IVOCBeforeValues VOCBeforeValues
		{
			get { return Header; }
		}

		public ZDecimal CIFValueBefore
		{
			get { return VOCBeforeValues.CIFValue; }
		}

		public ZPropertyInfo CIFValueBeforeInfo => GetZPropertyInfo(Schema.CIFValueBefore);

		public ZDecimal CIFValueAfter
		{
			get { return VOCAfterValues.CIFValue; }
		}

		public ZPropertyInfo CIFValueAfterInfo => GetZPropertyInfo(Schema.CIFValueAfter);

		public ZDecimal CIFValueDifference
		{
			get { return new ZDecimal(CIFValueAfter - CIFValueBefore).Round(2); }
		}

		public ZPropertyInfo CIFValueDifferenceInfo => GetZPropertyInfo(Schema.CIFValueDifference);
		#endregion

		#region CustomsValue
		public ZDecimal CustomsValueBefore
		{
			get { return VOCBeforeValues.CustomsValue; }
		}

		public ZPropertyInfo CustomsValueBeforeInfo => GetZPropertyInfo(Schema.CustomsValueBefore);

		public ZDecimal CustomsValueAfter
		{
			get { return VOCAfterValues.CustomsValue; }
		}

		public ZPropertyInfo CustomsValueAfterInfo => GetZPropertyInfo(Schema.CustomsValueAfter);

		public ZDecimal CustomsValueDifference
		{
			get { return new ZDecimal(CustomsValueAfter - CustomsValueBefore).Round(2); }
		}

		public ZPropertyInfo CustomsValueDifferenceInfo => GetZPropertyInfo(Schema.CustomsValueDifference);
		#endregion

		#region CustomsDutyNoS1P2B
		public ZDecimal CustomsDutyNoS1P2BBefore
		{
			get { return VOCBeforeValues.CustomsDutyNoS1P2B; }
		}

		public ZPropertyInfo CustomsDutyNoS1P2BBeforeInfo => GetZPropertyInfo(Schema.CustomsDutyNoS1P2BBefore);

		public ZDecimal CustomsDutyNoS1P2BAfter
		{
			get { return VOCAfterValues.CustomsDutyNoS1P2B; }
		}

		public ZPropertyInfo CustomsDutyNoS1P2BAfterInfo => GetZPropertyInfo(Schema.CustomsDutyNoS1P2BAfter);

		public ZDecimal CustomsDutyNoS1P2BDifference
		{
			get { return new ZDecimal(CustomsDutyNoS1P2BAfter - CustomsDutyNoS1P2BBefore).Round(2); }
		}

		public ZPropertyInfo CustomsDutyNoS1P2BDifferenceInfo => GetZPropertyInfo(Schema.CustomsDutyNoS1P2BDifference);
		#endregion

		#region S1P2BDuty
		public ZDecimal S1P2BDutyBefore
		{
			get { return VOCBeforeValues.S1P2BDuty; }
		}

		public ZPropertyInfo S1P2BDutyBeforeInfo => GetZPropertyInfo(Schema.S1P2BDutyBefore);

		public ZDecimal S1P2BDutyAfter
		{
			get { return VOCAfterValues.S1P2BDuty; }
		}

		public ZPropertyInfo S1P2BDutyAfterInfo => GetZPropertyInfo(Schema.S1P2BDutyAfter);

		public ZDecimal S1P2BDutyDifference
		{
			get { return new ZDecimal(S1P2BDutyAfter - S1P2BDutyBefore).Round(2); }
		}

		public ZPropertyInfo S1P2BDutyDifferenceInfo => GetZPropertyInfo(Schema.S1P2BDutyDifference);
		#endregion

		#region ValueAddedTax
		public ZDecimal ValueAddedTaxBefore
		{
			get { return VOCBeforeValues.ValueAddedTax; }
		}

		public ZPropertyInfo ValueAddedTaxBeforeInfo => GetZPropertyInfo(Schema.ValueAddedTaxBefore);

		public ZDecimal ValueAddedTaxAfter
		{
			get { return Header.DoNotClaimVATRefund ? VOCBeforeValues.ValueAddedTax : VOCAfterValues.ValueAddedTax; }
		}

		public ZPropertyInfo ValueAddedTaxAfterInfo => GetZPropertyInfo(Schema.ValueAddedTaxAfter);

		public ZDecimal ValueAddedTaxDifference
		{
			get { return new ZDecimal(ValueAddedTaxAfter - ValueAddedTaxBefore).Round(2); }
		}

		public ZPropertyInfo ValueAddedTaxDifferenceInfo => GetZPropertyInfo(Schema.ValueAddedTaxDifference);
		#endregion

		#region ProvisionalPaymentAmount

		public ZDecimal ProvisionalPaymentAmountBefore
		{
			get { return VOCBeforeValues.ProvisionalPaymentAmount; }
		}

		public ZPropertyInfo ProvisionalPaymentAmountBeforeInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountBefore);

		public ZDecimal ProvisionalPaymentAmountAfter
		{
			get { return VOCAfterValues.ProvisionalPaymentAmount; }
		}

		public ZPropertyInfo ProvisionalPaymentAmountAfterInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountAfter);

		public ZDecimal ProvisionalPaymentAmountDifference
		{
			get { return new ZDecimal(ProvisionalPaymentAmountAfter - ProvisionalPaymentAmountBefore).Round(2); }
		}

		public ZPropertyInfo ProvisionalPaymentAmountDifferenceInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountDifference);

		#endregion

		#region PenaltyAmount

		public ZDecimal PenaltyAmountBefore
		{
			get { return VOCBeforeValues.PenaltyAmount; }
		}

		public ZPropertyInfo PenaltyAmountBeforeInfo => GetZPropertyInfo(Schema.PenaltyAmountBefore);

		public ZDecimal PenaltyAmountAfter
		{
			get { return VOCAfterValues.PenaltyAmount; }
		}

		public ZPropertyInfo PenaltyAmountAfterInfo => GetZPropertyInfo(Schema.PenaltyAmountAfter);

		public ZDecimal PenaltyAmountDifference
		{
			get { return new ZDecimal(PenaltyAmountAfter - PenaltyAmountBefore).Round(2); }
		}

		public ZPropertyInfo PenaltyAmountDifferenceInfo => GetZPropertyInfo(Schema.PenaltyAmountDifference);

		#endregion

		#region Amount Due

		public ZDecimal AmountDueBefore
		{
			get { return CustomsDutyNoS1P2BBefore + S1P2BDutyBefore + ValueAddedTaxBefore + ProvisionalPaymentAmountBefore + PenaltyAmountBefore; }
		}

		public ZPropertyInfo AmountDueBeforeInfo => GetZPropertyInfo(Schema.AmountDueBefore);

		public ZDecimal AmountDueAfter
		{
			get { return CustomsDutyNoS1P2BAfter + S1P2BDutyAfter + ValueAddedTaxAfter + ProvisionalPaymentAmountAfter + PenaltyAmountAfter; }
		}

		public ZPropertyInfo AmountDueAfterInfo => GetZPropertyInfo(Schema.AmountDueAfter);

		public ZDecimal AmountDueDifference => AmountDueAfter - AmountDueBefore;

		public ZPropertyInfo AmountDueDifferenceInfo => GetZPropertyInfo(Schema.AmountDueDifference);

		public bool IsDutiable => AmountDueAfter > 0;

		#endregion

		#endregion

		#region Lookup Lists

		public CodeDescriptionPairList MessageTypesList
		{
			get
			{
				var messageTypesList = new CodeDescriptionPairList();
				if (Declaration.IsImport && Header.MovementReferenceNumber.IsEmpty)
				{
					messageTypesList = Factory.GetCachedValue("MessageSubTypeCodesList_ZA_Import", () =>
					{
						var result = new MessageSubTypeCodes();
						result.RemoveCode(MessageSubTypeCodes.Codes.Undefined);
						return result;
					});
				}
				else
				{
					messageTypesList = Factory.GetCachedValue("MessageSubTypeCodesList_ZA", () =>
					{
						var result = new MessageSubTypeCodes();
						result.RemoveCode(MessageSubTypeCodes.Codes.Replace);
						result.RemoveCode(MessageSubTypeCodes.Codes.Undefined);
						return result;
					});
				}

				if (IsEntryAlreadyRegistered)
				{
					var messageTypesListExcludeORG = new CodeDescriptionPairList();
					messageTypesListExcludeORG.AddRange(messageTypesList);
					messageTypesListExcludeORG.RemoveCode(MessageSubTypeCodes.Codes.Original);
					messageTypesList = messageTypesListExcludeORG;
				}
				return messageTypesList;
			}
		}

		public CodeDescriptionPairList ChangeAcknowledgementIndicatorList => Factory.GetCachedValue<ChangeAcknowledgementIndicator>();

		public CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				var isTwoStepClearingValid = TwoStepDeclarationHelper.IsTwoStepClearingValid;
				var isExWarehouse = Declaration.IsExWarehouse;
				var lastAcceptedSendDeclarationType = LastAcceptedSendDeclarationType;
				var messageType = MessageType;
				var key = System.FormattableString.Invariant($"{isTwoStepClearingValid}_{isExWarehouse}_{lastAcceptedSendDeclarationType}_{messageType}");

				return Factory.GetCachedValue(key, () =>
				{
					var result = new CodeDescriptionPairList();
					if (!isTwoStepClearingValid || isExWarehouse || lastAcceptedSendDeclarationType == ZA.Business.DeclarationTypeList.Codes.RegularCompleteDeclarationDefault)
					{
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularCompleteDeclarationDefault, ZA.Business.DeclarationTypeList.Descriptions.RegularCompleteDeclarationDefault);
					}
					else if (lastAcceptedSendDeclarationType == ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration
								|| lastAcceptedSendDeclarationType == ZA.Business.DeclarationTypeList.Codes.RegularProvisionalDeclaration)
					{
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularSupplementaryDeclaration, ZA.Business.DeclarationTypeList.Descriptions.RegularSupplementaryDeclaration);
						result.AddPair(lastAcceptedSendDeclarationType,
							lastAcceptedSendDeclarationType == ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration ?
							ZA.Business.DeclarationTypeList.Descriptions.RegularIncompleteDeclaration :
							ZA.Business.DeclarationTypeList.Descriptions.RegularProvisionalDeclaration);
					}
					else if (messageType == MessageSubTypeCodes.Codes.Original)
					{
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularCompleteDeclarationDefault, ZA.Business.DeclarationTypeList.Descriptions.RegularCompleteDeclarationDefault);
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration, ZA.Business.DeclarationTypeList.Descriptions.RegularIncompleteDeclaration);
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularProvisionalDeclaration, ZA.Business.DeclarationTypeList.Descriptions.RegularProvisionalDeclaration);
					}
					else
					{
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularCompleteDeclarationDefault, ZA.Business.DeclarationTypeList.Descriptions.RegularCompleteDeclarationDefault);
						result.AddPair(ZA.Business.DeclarationTypeList.Codes.RegularSupplementaryDeclaration, ZA.Business.DeclarationTypeList.Descriptions.RegularSupplementaryDeclaration);
					}

					return result;
				});
			}
		}

		public ZString LastAcceptedSendDeclarationType => LastAcceptedSendMessage?.DeclarationType ?? ZString.Empty;

		#endregion

		#region Implementation

		protected override void SetMessageSendingObjectDefaultValues()
		{
			var source = Header;
			ShouldSend = !source.IsEntryStatusCleared || source.IsAmendmentNotificationReceived;
			if ((!source.EntryInstruction?.CEI_MRNToBeReplaced.IsEmpty ?? false) && Header.MovementReferenceNumber.IsEmpty)
			{
				MessageType = MessageSubTypeCodes.Codes.Replace;
			}
			else
			{
				MessageType = IsEntryAlreadyRegistered ? MessageSubTypeCodes.Codes.Change : MessageSubTypeCodes.Codes.Original;
			}

			DeclarationType = DeclarationTypeList[0].Code;
		}

		void DefaultCaseNumberFromEntry()
		{
			var instructionPK = Header.EntryInstruction?.PK ?? ZGuid.Empty;
			if (!instructionPK.IsEmpty && MessageDataProviderInstruction.ShouldOutputCaseNumber(MessageKeyFactor))
			{
				var query = new ZQuery(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CaseNumber);
				query.AddToFilter(CusCodeDataSchema.CY_ParentID, instructionPK);
				query.AddToFilter(CusCodeDataSchema.CY_Date, ZDateTime.Empty);
				CaseNumber = Factory.LoadTop1<CaseNumber>(query)?.CY_Data ?? ZString.Empty;
			}
		}

		internal bool IsLRNEditable
		{
			get
			{
				switch (MessageType)
				{
					case MessageSubTypeCodes.Codes.Change:
						return IsImpliedVOCForEntryOutsideCW1;
					case MessageSubTypeCodes.Codes.Cancellation:
						return Header.MovementReferenceNumber.IsEmpty;
					default:
						return false;
				}
			}
		}

		internal bool IsMRNEditable
		{
			get { return MessageType != MessageSubTypeCodes.Codes.Replace && IsLRNEditable; }
		}

		bool IsImpliedVOCForEntryOutsideCW1 => Header.MovementReferenceNumber.IsEmpty && (Header.EntryInstruction?.CEI_DateForDuty.IsValid ?? false);

		bool IsEntryAlreadyRegistered => !Header.MovementReferenceNumber.IsEmpty || IsImpliedVOCForEntryOutsideCW1;

		#endregion

		#region IVOCAfterValues

		ZDecimal IVOCAfterValues.CIFValue
		{
			get { return CIFValueAfter; }
		}

		ZDecimal IVOCAfterValues.CustomsValue
		{
			get { return CustomsValueAfter; }
		}

		ZDecimal IVOCAfterValues.CustomsDutyNoS1P2B
		{
			get { return CustomsDutyNoS1P2BAfter; }
		}

		ZDecimal IVOCAfterValues.S1P2BDuty
		{
			get { return S1P2BDutyAfter; }
		}

		ZDecimal IVOCAfterValues.ValueAddedTax
		{
			get { return ValueAddedTaxAfter; }
		}

		ZDecimal IVOCAfterValues.ProvisionalPaymentAmount
		{
			get { return ProvisionalPaymentAmountAfter; }
		}

		ZDecimal IVOCAfterValues.PenaltyAmount
		{
			get { return PenaltyAmountAfter; }
		}

		#endregion

		#region IVOCBeforeValues

		ZDecimal IVOCBeforeValues.CIFValue
		{
			get { return CIFValueBefore; }
		}

		ZDecimal IVOCBeforeValues.CustomsValue
		{
			get { return CustomsValueBefore; }
		}

		ZDecimal IVOCBeforeValues.CustomsDutyNoS1P2B
		{
			get { return CustomsDutyNoS1P2BBefore; }
		}

		ZDecimal IVOCBeforeValues.S1P2BDuty
		{
			get { return S1P2BDutyBefore; }
		}

		ZDecimal IVOCBeforeValues.ValueAddedTax
		{
			get { return ValueAddedTaxBefore; }
		}

		ZDecimal IVOCBeforeValues.ProvisionalPaymentAmount
		{
			get { return ProvisionalPaymentAmountBefore; }
		}

		ZDecimal IVOCBeforeValues.PenaltyAmount
		{
			get { return PenaltyAmountBefore; }
		}

		#endregion

		#region Diamond Levy Amount For Exports
		public ZDecimal DiamondLevyAmount
		{
			get
			{
				var result = 0m;

				if (Declaration.IsExport)
				{
					result = Header.MergedLines.OfType<CusEntryLine>().SelectMany(el => el.ProvisionalPayments.OfType<ProvisionalPaymentAmountCodeData>().Where(pp => pp.CY_Code == LineLevelProvisionalPaymentsForExports.Codes.DLA).Select(x => (decimal)x.CY_Value)).Sum();
				}

				return result;
			}
		}
		#endregion

		public MessageAction GetMessageAction()
		{
			var type = MessageSubTypeCodes.TranslateToMessageSubType(MessageType);
			switch (type)
			{
				case MessageSubTypes.Create:
					return MessageAction.Original;
				case MessageSubTypes.Withdraw:
					return MessageAction.Withdrawal;
				default:
					return MessageAction.Amendment;
			}
		}

		bool IJobDeclarationSendingObjectWarehouseProvider.ShouldProcessWarehouse
		{
			get
			{
				var messageAction = GetMessageAction();

				return (messageAction == MessageAction.Original || ((messageAction == MessageAction.Withdrawal || messageAction == MessageAction.Amendment) && Header.HasWHSTransaction()));
			}
		}
	}
}
