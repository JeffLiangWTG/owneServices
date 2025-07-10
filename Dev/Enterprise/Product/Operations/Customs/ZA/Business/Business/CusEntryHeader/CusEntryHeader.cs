using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.ZACusEntryHeader)]
	public partial class CusEntryHeader : AutoZACusEntryHeader,
		IComparable,
		IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider,
		Integration.Customs.ZA.ICusEntryHeader,
		ICusCodeDataTypeSupporter,
		IVOCAfterValues,
		IVOCBeforeValues,
		ICustomsChargeEntry,
		IInterchangeSenderIdProvider,
		IAutoRateDSBOnSaved,
		ICustomLabelsConfigOrgProvider,
		IAccInvoiceDataProvider
	{
		public new class Schema : AutoZACusEntryHeader.Schema
		{
			public const string ValuationCode = "ValuationCode";
			public const string ValueDeterminationNumber = "ValueDeterminationNumber";
			public const string ValueAddedTax = "ValueAddedTax";
			public const string CustomsDuty = "CustomsDuty";
			public const string CH_Calc_StatusEntryCode = "CH_Calc_StatusEntryCode";
			public const string EffectiveValuationCode = "EffectiveValuationCode";
			public const string UZ_ProvisionalPaymentSurety = "UZ_ProvisionalPaymentSurety";
			public const string CustomsProcedureCode = "CustomsProcedureCode";
			public const string CustomsProcedureInstructionDescription = "CustomsProcedureInstructionDescription";
			public const string Endorsements = "Endorsements";
			public const string CIFValueBefore = "CIFValueBefore";
			public const string CIFInLocalCurrencyRounded = "CIFInLocalCurrencyRounded";
			public const string CIFValueDifference = "CIFValueDifference";
			public const string CustomsValueBefore = "CustomsValueBefore";
			public const string CustomsValueDifference = "CustomsValueDifference";
			public const string CustomsDutyExcluding12BBefore = "CustomsDutyExcluding12BBefore";
			public const string CustomsDutyExcluding12BAfter = "CustomsDutyExcluding12BAfter";
			public const string CustomsDutyExcluding12BDifference = "CustomsDutyExcluding12BDifference";
			public const string S1P2BDutyBefore = "S1P2BDutyBefore";
			public const string S1P2BDutyAfter = "S1P2BDutyAfter";
			public const string S1P2BDutyDifference = "S1P2BDutyDifference";
			public const string ValueAddedTaxBefore = "ValueAddedTaxBefore";
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
			public const string DoNotClaimVATRefund = "DoNotClaimVATRefund";
			public const string UniqueConsignmentReference = "UniqueConsignmentReference";
			public const string EntryInstructionAssessmentDate = "EntryInstructionAssessmentDate";
			public const string CombinedUCREntryNumbers = "CombinedUCREntryNumbers";
			public const string IsOverwriteProvisionalPaymentAfterValues = "IsOverwriteProvisionalPaymentAfterValues";
		}

		#region IComparable

		public int CompareTo(object obj)
		{
			CusEntryHeader entryHeader = (CusEntryHeader)obj;
			return CH_BGMReference.CompareTo(entryHeader.CH_BGMReference);
		}

		#endregion

		#region Constructors

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Public Static

		public new static CusEntryHeader LoadForBGMReference(BusinessObjectFactory factory, ZString serialNumber)
		{
			CusEntryHeader mostRecent = null;
			ZQuery filter = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, serialNumber);
			CusEntryHeader[] entryHeaders = (CusEntryHeader[])factory.Load(typeof(CusEntryHeader), filter);
			foreach (CusEntryHeader entryHeader in entryHeaders)
			{
				if (entryHeader.Declaration.Branch.GB_GC == GlbCompany.CurrentCompany.PK || entryHeaders.Length == 1)
				{
					if (mostRecent == null)
					{
						mostRecent = entryHeader;
					}

					if (mostRecent != entryHeader
						&& mostRecent.Logs.AddedLog != null
						&& entryHeader.Logs.AddedLog != null
						&& mostRecent.Logs.AddedLog.SL_EventTime < entryHeader.Logs.AddedLog.SL_EventTime)
					{
						mostRecent = entryHeader;
					}
				}
			}
			return mostRecent;
		}

		#endregion

		#region Public Properties

		public ZString UniqueConsignmentReference
		{
			get { return DeclarationUCR; }
			set
			{
				if (!IsCopying && DeclarationUCR != value)
				{
					LoadOrCreateUCRNumber(value);
				}
			}
		}

		#region ImportControlNumber

		public ZString ImportControlNumber
		{
			get { return ImportControlCusEntryNumber != null ? ImportControlCusEntryNumber.CE_EntryNum : ZString.Empty; }
		}

		CusEntryNumber ImportControlCusEntryNumber
		{
			get { return importControlNumber ?? (importControlNumber = FindExistingCusEntryNum(CusEntryNumberTypes.Standard.ImportControlNumber)); }
		}
		CusEntryNumber importControlNumber;

		CusEntryNumber FindExistingCusEntryNum(ZString entryNumberType)
		{
			return CusEntryNumber.Load(this, entryNumberType, Core.Constants.CountryCodes.SouthAfrica);
		}

		#endregion

		public override bool HasBeenWithdrawn
		{
			get { return false; }
		}

		public bool Line1HasBondHolder => Factory.GetValue(ref line1HasBondHolderCached, () =>
		{
			return MergedLines.Any(x => x.AdditionalInformationCodes[UniversalReferenceConstants.AdditionalInformation.BondHolder] != null);
		});

		CachedProperty<bool> line1HasBondHolderCached;

		ZString GetRemoverUserCode(OrgHeader header)
		{
			return header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
		}

		public ZString RemoverLocalCustomsCarrierCode => Factory.GetValue(ref removerLocalCustomsCarrierCodeCached, () => GetRemoverUserCode(EntryInstruction?.Remover));

		CachedProperty<ZString> removerLocalCustomsCarrierCodeCached;

		public ZString SubContractorRemoverCarrierCode => Factory.GetValue(ref subContractorRemoverCarrierCodeCached, () => GetRemoverUserCode(EntryInstruction?.SubContractor));

		CachedProperty<ZString> subContractorRemoverCarrierCodeCached;

		public ZString RemoverCarrierCodeForEDI => (EntryInstruction?.CEI_RemoverEDI ?? true) ? RemoverLocalCustomsCarrierCode : SubContractorRemoverCarrierCode;

		public override ZInt PackagesCount
		{
			get { return CH_Packages; }
		}

		public ZString ReleaseAgentCodeAndHouseBill
		{
			get
			{
				ZString result = ZString.Empty;
				if (Declaration.IsImport && Declaration.IsSea && Declaration.Forwarder != null)
				{
					bool hasLCLOrFCG = false;
					foreach (CusContainer container in Containers)
					{
						hasLCLOrFCG |= (container.CO_FCL_LCL_AIR == CusContainer.ContainerModes.LessContainerLoad)
							|| (container.CO_FCL_LCL_AIR == "FCG");
					}
					if (hasLCLOrFCG)
					{
						result = Declaration.Forwarder.LocalReleaseAgentCode;
					}
				}

				Bill firstHouseBill = FirstHouseBill;
				if (firstHouseBill != null)
				{
					result += firstHouseBill.CU_BillNum;
				}
				return result;
			}
		}

		Bill FirstHouseBill
		{
			get
			{
				foreach (Bill bill in Bills)
				{
					if (bill.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.HouseBill)
					{
						return bill;
					}
				}
				return null;
			}
		}

		#region Is Properties

		public bool IsAwaitingResponse => ZAMessageStatusList.IsAwaiting(CH_Status);

		public bool IsVOCEntry => !(EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty).IsEmpty;

		bool IsNotVOCEntryOrNoMRNI => !IsVOCEntry || !MovementReferenceNumber.IsEmpty;

		bool IsProvisionalPaymentBeforeReadOnly => !IsOverwriteProvisionalPaymentAfterValues && IsNotVOCEntryOrNoMRNI;

		bool IsProvisionalPaymentAfterReadOnly => !IsOverwriteProvisionalPaymentAfterValues;

		bool isOverwriteProvisionalPaymentAfterValues;
		public ZBool IsOverwriteProvisionalPaymentAfterValues
		{
			get => isOverwriteProvisionalPaymentAfterValues || VoucherOfCorrectionValueAfters.Count > 0;
			set
			{
				if (IsOverwriteProvisionalPaymentAfterValues != value)
				{
					if (value)
					{
						if (Declaration.MessageInitiator.ShowUserConfirmation("Warning - Overwriting system generated totals may cause unexpected consequences",
							"Warning - Overwrite ePP Totals",
							"If you are certain you wish to overwrite ePP totals then please type:",
							"I wish to overwrite ePP Totals"))
						{
							isOverwriteProvisionalPaymentAfterValues = true;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							Logs.AddNew(AutoEvents.EditedARecord, "User acknowledged overwrite ePP Totals");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						}
					}
					else
					{
						isOverwriteProvisionalPaymentAfterValues = false;
						VoucherOfCorrectionValueAfters.RemoveAndDeleteAll();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(AutoEvents.EditedARecord, "User disabled overwrite ePP Totals");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					ProvisionalPaymentAmountBeforeInfo.RefreshBinding();
					ProvisionalPaymentAmountAfterInfo.RefreshBinding();
					PenaltyAmountBeforeInfo.RefreshBinding();
					PenaltyAmountAfterInfo.RefreshBinding();
				}
				IsOverwriteProvisionalPaymentAfterValuesInfo.RefreshBinding(value);
			}
		}

		public ZPropertyInfo IsOverwriteProvisionalPaymentAfterValuesInfo => GetZPropertyInfo(Schema.IsOverwriteProvisionalPaymentAfterValues);

		#endregion

		public ZString PortOfExit
		{
			get { return EntryInstruction?.CEI_PortOfExit ?? ZString.Empty; }
		}

		#region CustomsProcedureCode

		public virtual ZString CustomsProcedureCode => Factory.GetValue(ref cachedCustomsProcedureCode, () => RandomEntryLine?.CustomsProcedureCode ?? EntryInstruction?.CEI_Style ?? ZString.Empty);

		CachedProperty<ZString> cachedCustomsProcedureCode;

		public ZPropertyInfo CustomsProcedureCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsProcedureCode); }
		}

		#endregion

		#region CustomsProcedureInstructionDescription

		public ZString CustomsProcedureInstructionDescription => Factory.GetValue(ref cachedCustomsProcedureInstructionDescription, () => EntryInstruction?.CEI_Description ?? ZString.Empty);

		CachedProperty<ZString> cachedCustomsProcedureInstructionDescription;

		public ZPropertyInfo CustomsProcedureInstructionDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsProcedureInstructionDescription); }
		}

		#endregion

		#region ValueDeterminationNumber

		public ZString ValueDeterminationNumber
		{
			get { return RandomEntryLine != null ? RandomEntryLine.ValueDeterminationNumber : ZString.Empty; }
		}

		public ZPropertyInfo ValueDeterminationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ValueDeterminationNumber); }
		}

		#endregion

		#region Sum Bond Surety Amount
		public ZDecimal SumBondSuretyAmount
		{
			get
			{
				return MergedLines.OfType<CusEntryLine>()
					.SelectMany(x => x.AdditionalInformationCodes.Cast<AdditionalInformation>())
					.Where(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount)
					.Sum(x => x.Amount);
			}
		}
		#endregion

		#region Amount Due
		public ZDecimal AmountDueBefore => Factory.GetValue(ref amountDueBeforeCached, () => VOCBeforeValues.GetAmountDue());

		CachedProperty<ZDecimal> amountDueBeforeCached;

		public ZPropertyInfo AmountDueBeforeInfo => GetZPropertyInfo(Schema.AmountDueBefore);

		public ZDecimal AmountDueAfter => Factory.GetValue(ref amountDueAfterCached, () => VOCAfterValues.GetAmountDue());

		CachedProperty<ZDecimal> amountDueAfterCached;

		public ZPropertyInfo AmountDueAfterInfo => GetZPropertyInfo(Schema.AmountDueAfter);

		public ZDecimal AmountDueDifference => AmountDueAfter - AmountDueBefore;
		public ZPropertyInfo AmountDueDifferenceInfo => GetZPropertyInfo(Schema.AmountDueDifference);

		#endregion

		#region CIFValue
		[ReadOnlyMember(nameof(IsNotVOCEntryOrNoMRNI))]
		[DecimalPlaces(2)]
		public ZDecimal CIFValueBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.CIFValue); }
			set
			{
				VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.CIFValue, value, CIFValueBeforeInfo);
				CIFValueDifferenceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CIFValueBeforeInfo => GetZPropertyInfo(Schema.CIFValueBefore);

		public ZDecimal CIFInLocalCurrencyRounded
		{
			get { return CIFInLocalCurrency.Amount.RoundUsingCustomsValueRule(); }
		}

		public ZPropertyInfo CIFInLocalCurrencyRoundedInfo => GetZPropertyInfo(Schema.CIFInLocalCurrencyRounded);

		public ZDecimal CIFValueDifference
		{
			get { return new ZDecimal(CIFInLocalCurrencyRounded - CIFValueBefore).Round(2); }
		}

		public ZPropertyInfo CIFValueDifferenceInfo => GetZPropertyInfo(Schema.CIFValueDifference);
		#endregion

		#region CustomsValue
		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsNotVOCEntryOrNoMRNI))]
		public ZDecimal CustomsValueBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.CustomsValue); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.CustomsValue, value, CustomsValueBeforeInfo); }
		}

		public ZPropertyInfo CustomsValueBeforeInfo => GetZPropertyInfo(Schema.CustomsValueBefore);

		public ZDecimal CustomsValueDifference
		{
			get { return new ZDecimal(CustomsValue - CustomsValueBefore).Round(2); }
		}

		public ZPropertyInfo CustomsValueDifferenceInfo => GetZPropertyInfo(Schema.CustomsValueDifference);
		#endregion

		#region CustomsDuty
		[ReadOnlyMember(nameof(IsNotVOCEntryOrNoMRNI))]
		[DecimalPlaces(2)]
		public ZDecimal CustomsDutyExcluding12BBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.Duty); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.Duty, value, CustomsDutyExcluding12BBeforeInfo); }
		}

		public ZPropertyInfo CustomsDutyExcluding12BBeforeInfo => GetZPropertyInfo(Schema.CustomsDutyExcluding12BBefore);

		public ZDecimal CustomsDutyExcluding12BAfter
		{
			get { return GetCalcFeeValues().CustomsDutyExcluding12B; }
		}

		public ZPropertyInfo CustomsDutyExcluding12BAfterInfo => GetZPropertyInfo(Schema.CustomsDutyExcluding12BAfter);

		public ZDecimal CustomsDutyExcluding12BDifference
		{
			get { return new ZDecimal(CustomsDutyExcluding12BAfter - CustomsDutyExcluding12BBefore).Round(2); }
		}

		public ZPropertyInfo CustomsDutyExcluding12BDifferenceInfo => GetZPropertyInfo(Schema.CustomsDutyExcluding12BDifference);

		public ZDecimal CustomsDuty
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusEntryLine entryLine in MergedLines)
				{
					result += entryLine.CustomsDuty;
				}

				return result;
			}
		}

		public ZPropertyInfo CustomsDutyInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsDuty); }
		}

		#endregion

		#region S1P2BDuty
		[ReadOnlyMember(nameof(IsNotVOCEntryOrNoMRNI))]
		[DecimalPlaces(2)]
		public ZDecimal S1P2BDutyBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.S1P2BDuty); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.S1P2BDuty, value, S1P2BDutyBeforeInfo); }
		}

		public ZPropertyInfo S1P2BDutyBeforeInfo => GetZPropertyInfo(Schema.S1P2BDutyBefore);

		public ZDecimal S1P2BDutyAfter
		{
			get { return GetCalcFeeValues().S1P2BDuty; }
		}

		public ZPropertyInfo S1P2BDutyAfterInfo => GetZPropertyInfo(Schema.S1P2BDutyAfter);

		public ZDecimal S1P2BDutyDifference
		{
			get { return new ZDecimal(S1P2BDutyAfter - S1P2BDutyBefore).Round(2); }
		}

		public ZPropertyInfo S1P2BDutyDifferenceInfo => GetZPropertyInfo(Schema.S1P2BDutyDifference);
		#endregion

		#region ValueAddedTax
		[ReadOnlyMember(nameof(IsNotVOCEntryOrNoMRNI))]
		[DecimalPlaces(2)]
		public ZDecimal ValueAddedTaxBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.VAT); }
			set
			{
				VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.VAT, value, ValueAddedTaxBeforeInfo);
				RefreshDoNotClaimVATRefundIfNecessary();
			}
		}

		public ZPropertyInfo ValueAddedTaxBeforeInfo => GetZPropertyInfo(Schema.ValueAddedTaxBefore);

		public ZDecimal ValueAddedTax
		{
			get { return DoNotClaimVATRefund ? ValueAddedTaxBefore : GetCalcFeeValues().ValueAddedTax; }
		}

		public ZPropertyInfo ValueAddedTaxInfo
		{
			get { return GetZPropertyInfo(Schema.ValueAddedTax); }
		}

		public ZDecimal ValueAddedTaxDifference
		{
			get { return new ZDecimal(ValueAddedTax - ValueAddedTaxBefore).Round(2); }
		}

		public ZPropertyInfo ValueAddedTaxDifferenceInfo => GetZPropertyInfo(Schema.ValueAddedTaxDifference);
		#endregion

		#region ProvisionalPaymentAmount

		[ReadOnlyMember(nameof(IsProvisionalPaymentBeforeReadOnly))]
		[DecimalPlaces(2)]
		public ZDecimal ProvisionalPaymentAmountBefore
		{
			get => VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.ProvisionalPayment);
			set => VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.ProvisionalPayment, value, ProvisionalPaymentAmountBeforeInfo);
		}

		public ZPropertyInfo ProvisionalPaymentAmountBeforeInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountBefore);

		[ReadOnlyMember(nameof(IsProvisionalPaymentAfterReadOnly))]
		[DecimalPlaces(2)]
		public ZDecimal ProvisionalPaymentAmountAfter
		{
			get
			{
				var result = GetCalcFeeValues().ProvisionalPayment;
				if (!IsProvisionalPaymentAfterReadOnly)
				{
					if (!VoucherOfCorrectionValueAfters.ContainsCode(VOCValueTypeList.Codes.ProvisionalPayment))
					{
						ProvisionalPaymentAmountAfter = GetCalcFeeValues().ProvisionalPayment;
					}
					result = VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.ProvisionalPayment);
				}
				return result;
			}
			set => VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.ProvisionalPayment, value, ProvisionalPaymentAmountAfterInfo);
		}

		public ZPropertyInfo ProvisionalPaymentAmountAfterInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountAfter);

		public ZDecimal ProvisionalPaymentAmountDifference => new ZDecimal(ProvisionalPaymentAmountAfter - ProvisionalPaymentAmountBefore).Round(2);

		public ZPropertyInfo ProvisionalPaymentAmountDifferenceInfo => GetZPropertyInfo(Schema.ProvisionalPaymentAmountDifference);

		#endregion

		#region PenaltyAmount

		[ReadOnlyMember(nameof(IsProvisionalPaymentBeforeReadOnly))]
		[DecimalPlaces(2)]
		public ZDecimal PenaltyAmountBefore
		{
			get { return VoucherOfCorrectionValueBefores.GetValue(VOCValueTypeList.Codes.Penalty); }
			set { VoucherOfCorrectionValueBefores.SetValue(VOCValueTypeList.Codes.Penalty, value, PenaltyAmountBeforeInfo); }
		}

		public ZPropertyInfo PenaltyAmountBeforeInfo => GetZPropertyInfo(Schema.PenaltyAmountBefore);

		[ReadOnlyMember(nameof(IsProvisionalPaymentAfterReadOnly))]
		[DecimalPlaces(2)]
		public ZDecimal PenaltyAmountAfter
		{
			get
			{
				var result = GetCalcFeeValues().Penalty;
				if (!IsProvisionalPaymentAfterReadOnly)
				{
					if (!VoucherOfCorrectionValueAfters.ContainsCode(VOCValueTypeList.Codes.Penalty))
					{
						PenaltyAmountAfter = GetCalcFeeValues().Penalty;
					}
					result = VoucherOfCorrectionValueAfters.GetValue(VOCValueTypeList.Codes.Penalty);
				}
				return result;
			}
			set => VoucherOfCorrectionValueAfters.SetValue(VOCValueTypeList.Codes.Penalty, value, PenaltyAmountAfterInfo);
		}

		public ZPropertyInfo PenaltyAmountAfterInfo => GetZPropertyInfo(Schema.PenaltyAmountAfter);

		public ZDecimal PenaltyAmountDifference => new ZDecimal(PenaltyAmountAfter - PenaltyAmountBefore).Round(2);

		public ZPropertyInfo PenaltyAmountDifferenceInfo => GetZPropertyInfo(Schema.PenaltyAmountDifference);

		#endregion

		#region Endorsements

		[MaxLength(2000)]
		public ZString Endorsements
		{
			get
			{
				var result = ZString.Empty;
				var note = EndorsementsNote;
				if (note != null)
				{
					result = note.ST_NoteText;
				}
				return result;
			}
			set
			{
				var oldValue = Endorsements;
				var note = EndorsementsNote;
				if (note == null)
				{
					note = Notes.AddNew();
					note.ST_ParentID = PK;
					note.ST_Table = TableName;
					note.ST_Description = PredefinedNoteTypes.Instance.ZAEndorsement.Description;
				}
				CheckMaximumLength(EndorsementsInfo, value);
				note.ST_NoteText = value;
				HasChanges = oldValue != value;
				EndorsementsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndorsementsInfo
		{
			get { return GetZPropertyInfo(Schema.Endorsements); }
		}

		protected StmNote EndorsementsNote
		{
			get
			{
				var endorsementsNote = Notes.FindByDescription(PredefinedNoteTypes.Instance.ZAEndorsement.Description);
				return endorsementsNote.Length > 0 ? endorsementsNote[0] : null;
			}
		}

		#endregion

		#region Properties for Message Sender, Proxied From Declaration

		public virtual ZString AgentCode => Declaration?.AgentCode ?? ZString.Empty;

		public OrgHeader EffectiveAgent => OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.AgentCode, AgentCode, Core.Constants.CountryCodes.SouthAfrica);

		public virtual ZString AgentDualProfileCode => Declaration?.AgentDualProfileCode ?? ZString.Empty;

		public ZString SenderID => AgentCode + AgentDualProfileCode;

		#endregion

		#region
		public IEnumerable<ZString> UCREntryNumbers => Factory.GetValue(ref uCREntryNumbers, delegate
					{
						var entryNumbers = Factory.Load<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(PK, CountryCode, CusEntryNumberTypes.Standard.UniqueConsignementReference, IsInDatabase));
						return entryNumbers.Select(x => x.CE_EntryNum);
					});

		CachedProperty<IEnumerable<ZString>> uCREntryNumbers;

		public ZString CombinedUCREntryNumbers => Factory.GetValue(ref combinedUCREntryNumbers, () => ZString.Join(", ", UCREntryNumbers.ToArray()));

		CachedProperty<ZString> combinedUCREntryNumbers;

		#endregion

		#endregion

		#region Overrides

		internal bool IsImportByExternalBroker => Declaration?.IsImportByExternalBroker ?? false;

		public override ZString CH_BGMReference
		{
			get { return base.CH_BGMReference; }
			set
			{
				var oldValue = CH_BGMReference;
				base.CH_BGMReference = value;
				if (!oldValue.IsEmpty && oldValue != value && this.Messages.Count > 0)
				{
					this.MarkNeedsAutoRateDSB();
					this.temporaryOldUniqueNumberForAccounting = oldValue;
				}
			}
		}

		public override ZString CH_EntryStatus
		{
			get { return base.CH_EntryStatus; }
			set
			{
				var oldStatus = CH_EntryStatus;
				if (IsStatusChangingFromNotAcceptedToAccepted(oldStatus, value))
				{
					UpdateWhenStatusIsAboutToChangeToClear(oldStatus, value);
				}
				base.CH_EntryStatus = value;
			}
		}

		public override ZDateTime CH_EntryReleaseDate
		{
			get { return base.CH_EntryReleaseDate; }
			set
			{
				var oldValue = CH_EntryReleaseDate;
				base.CH_EntryReleaseDate = value;
				if (oldValue != CH_EntryReleaseDate)
				{
					DefaultCH_BondValidToDateIfNeeded();
					Validation.ValidateCH_BondValidToDate();
				}
			}
		}

		public override ZDateTime CH_EntrySubmittedDate
		{
			get => base.CH_EntrySubmittedDate;
			set
			{
				var oldValue = CH_EntrySubmittedDate;
				base.CH_EntrySubmittedDate = value;
				if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled && oldValue != CH_EntrySubmittedDate && !CH_EntrySubmittedDate.IsEmpty && !IsCopying && Declaration != null && CH_EntrySubmittedDate > Declaration.JE_ValuationDate)
				{
					if (Declaration.IsExport)
					{
						Declaration.JE_ValuationDate = CH_EntrySubmittedDate.AddDays(-1).Date;
					}
					else if (Declaration.IsExWarehouse)
					{
						Declaration.JE_ValuationDate = CH_EntrySubmittedDate.Date;
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString CH_RelPrintInd { get => base.CH_RelPrintInd; set => base.CH_RelPrintInd = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.PaymentMethodCodeList))]
		public override ZString CH_PaymentMethod { get => base.CH_PaymentMethod; set => base.CH_PaymentMethod = value; }

		void DefaultCH_BondValidToDateIfNeeded()
		{
			if (CH_EntryReleaseDate.IsValid && CH_BondValidToDate.IsEmpty)
			{
				var date = CalculateDefaultCH_BondValidToDate();
				if (date.IsValid)
				{
					CH_BondValidToDate = date;
				}
			}
		}

		internal ZDate CalculateDefaultCH_BondValidToDate()
		{
			var acquitByDateData = ZACustomsRegistry.Instance.CPCAcquitByDate.Value;
			return acquitByDateData != null ? CalculateDeferredDate(CH_EntryReleaseDate.Date, acquitByDateData) : ZDate.Invalid;
		}

		internal ZDate CalculateDefaultCH_BondValidToDate_OnDefaultRegistryValue()
		{
			return CalculateDeferredDate(CH_EntryReleaseDate.Date, new CPCAcquitByDate());
		}

		ZDate CalculateDeferredDate(ZDate startDate, CPCAcquitByDate deferParameter)
		{
			var date = deferParameter.Unit == "DAY(S)"
				? startDate.AddDays(deferParameter.Quantity)
				: startDate.AddMonths(deferParameter.Quantity);

			if (deferParameter.Quantity > 0)
			{
				switch (date.DayOfWeek)
				{
					case DayOfWeek.Sunday:
						date = date.AddDays(1);
						break;
					case DayOfWeek.Saturday:
						date = date.AddDays(-1);
						break;
				}
			}

			return date;
		}

		public override ZString DefaultStatusDescription
		{
			get { return ZString.Empty; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return (base.HumanReadableNameCore + " " + CH_BGMReference).TrimEnd(); }
		}

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			return new CusEntryHeaderLookups(this);
		}

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			var result = LoadCusEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber);
			if (result != null)
			{
				result.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				var declaration = Declaration;
				var entryType = declaration == null ? Customs.Business.JobMessageTypeList.Codes.Import : (string)declaration.JE_MessageType;
				result = LoadCusEntryNumber(entryType);
			}

			return result;
		}

		CusEntryNumber LoadCusEntryNumber(ZString entryType)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		protected override ZString EntryNumberType
		{
			get { return CusEntryNumberTypes.Standard.MovementReferenceNumber; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategy.CusEntryHeaderFetchStrategy(this);
		}

		public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPayment, ILogger logger)
		{
			var declaration = this.Declaration;
			var result = false;
			if (declaration != null)
			{
				if (declaration.IsDeclarationIntegrated)
				{
					result = declaration.JE_PaymentMethod == PaidByCodeList.Codes.BRK;
				}
				else
				{
					result = !GetImporterPays();
				}
			}
			return result;
		}

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			return !IsCleared(originalStatus) && IsCleared(newStatus);
		}

		bool IsCleared(ZString status)
		{
			return CustomsStatusAttributeHelper.IsStatusCleared(Factory, status, CountryCode, EntryInstructionAssessmentDate);
		}

		public ZString CustomsOffice => EntryInstruction?.CustomsOffice ?? Declaration?.JE_CustomsOffice ?? ZString.Empty;

		protected override ZString GetUniqueNumberForAccountingIntegrationCore()
		{
			var result = EntryNumber;
			if (result.IsEmpty)
			{
				result = GetAPInvoiceNumberFromLRN(CH_BGMReference);
			}
			return result;
		}

		protected override ZString GetPreviousUniqueNumberForAccountingIntegrationCore()
		{
			var result = GetAPInvoiceNumberFromLRN(temporaryOldUniqueNumberForAccounting);
			if (!EntryNumber.IsEmpty)
			{
				result = GetAPInvoiceNumberFromLRN(CH_BGMReference);
			}
			return result;
		}

		ZString temporaryOldUniqueNumberForAccounting;

		static ZString GetAPInvoiceNumberFromLRN(ZString input)
		{
			var result = ZString.Empty;
			var maxLength = JobCharge.Schema.JR_APInvoiceNumMaxLength;
			if (input.Length < maxLength)
			{
				result = input;
			}
			else
			{
				result = input.Right(14);
				if (result.KeepNumericCharacters() != result)
				{
					result = input.Right(maxLength);
				}
			}
			return result;
		}

		protected override Customs.Business.WeightUQCalculator GetWeightCalculator()
		{
			return new WeightUQCalculator(this);
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var declaration = Declaration;
			if (declaration != null && !declaration.IsImportByExternalBroker && (CH_BGMReference.IsEmpty || (NeedsNewBGMReference && IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected && Declaration.DoAgentHaveAValue && !CustomsOffice.IsEmpty)))
			{
				CH_BGMReference = declaration.LocalReferenceNumber(string.Format(CultureInfo.CurrentCulture, "{0:D6}", Env.NumberFountains.ZACustomsEDIFACTNumberFountain("M", "SARS").GetNext(Factory)), CustomsOffice);
				NeedsNewBGMReference = false;
			}
		}

		public ICusEntryLine[] AllMergedLines
		{
			get { return (ICusEntryLine[])MergedLines.ToArray(typeof(ICusEntryLine)); }
		}

		public virtual OrgHeader Supplier
		{
			get
			{
				OrgHeader result = null;
				if (!IsMultiSupplier && InvoiceHeaders.Length > 0)
				{
					result = InvoiceHeaders[0].Supplier;
				}

				return result;
			}
		}

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation()
		{
			return new CusEntryHeaderValidation(this);
		}

		public new CusEntryHeaderValidation Validation
		{
			get { return (CusEntryHeaderValidation)GetNewValidation(); }
		}

		protected override bool IsStatusClear(string status)
		{
			return CustomsStatusAttributeHelper.IsStatusCleared(Factory, status, CountryCode, EntryInstructionAssessmentDate);
		}

		ZString ICustomsChargeEntry.ReferenceNumber
		{
			get { return EntryNumber; }
		}

		bool SetAccountingChargesToZeroWhenCancelled => CustomsStatusAttributeHelper.IsStatusCancelled(Factory, CH_EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now);

		protected override ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPayment)
		{
			var result = 0m;
			if (!SetAccountingChargesToZeroWhenCancelled)
			{
				var chargeCode = chargeTypeElement.Code;
				var feeTypes = GetProvisionalPaymentTypes(chargeCode);
				if (feeTypes.Length != 0)
				{
					foreach (CusEntryLine entryLine in MergedLines)
					{
						foreach (var feeType in feeTypes)
						{
							result += entryLine.ProvisionalPayments.GetAmount(feeType);
						}
						if (entryLine.IsLine1)
						{
							var entryInstruction = EntryInstruction;
							if (entryInstruction != null && feeTypes.Contains(entryInstruction.CEI_ProvisionalPaymentType))
							{
								result += entryInstruction.CEI_ProvisionalPaymentAmount;
							}
						}
					}
				}
				else if (UniversalReferenceDataHelper.GetTaxTypeList(Factory, EntryInstructionAssessmentDate).ContainsCode(chargeCode))
				{
					result += this.ValueAddedTax;
				}
				else
				{
					result = base.GetTotalChargeValueFor(chargeTypeElement, methodOfPayment);
				}
			}
			return result;
		}

		public override ZDate CH_BondValidToDate
		{
			get => base.CH_BondValidToDate;
			set
			{
				if (value != CH_BondValidToDate)
				{
					bondValidToDateModified = true;
				}

				base.CH_BondValidToDate = value;
			}
		}

		bool bondValidToDateModified;

		ZString[] GetProvisionalPaymentTypes(ZString code)
		{
			return new List<ZString>(ProvisionalPaymentTypesHelper.GetTypesForRateType(code)).ToArray();
		}

		protected override bool ShouldLogCustomsClearedEvent()
		{
			return IsCustomsClearedEventSupported && IsEntryStatusChangedToClearSinceLoading;
		}

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment
		{
			get { return Declaration?.ActiveEntryHeaders.AreAllEntriesCleared ?? false; }
		}

		protected override bool ShouldPopulateReleaseDate => IsEntryStatusChangedToClearSinceLoading;

		protected override bool IsChangingToClearStatusForAccIntegration
		{
			get
			{
				return IsInDatabase && IsChangingToClearStatusForAccIntegrationCore((ZString)CH_EntryStatusInfo.OriginalValue, CH_EntryStatus)
					&& !IsPostingDateEmpty;
			}
		}

		bool IsPostingDateEmpty => ((IAccInvoiceDataProvider)this).InvoiceDate.IsEmpty;

		internal bool IsChangingToClearStatusForAccIntegrationCore(ZString originalStatus, ZString newStatus)
		{
			return CustomsStatusAttributeHelper.ShouldPostCustomsAPInvoice(Factory, newStatus, CountryCode, ZDateTime.Today)
					&& !CustomsStatusAttributeHelper.ShouldPostCustomsAPInvoice(Factory, originalStatus, CountryCode, ZDateTime.Today);
		}

		protected override ZDateTime GetReleaseDate()
		{
			var result = ZDateTime.Empty;
			var lastInterchange = Messages.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSRES, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK)?.Interchange;
			if (lastInterchange != null)
			{
				var unbSegment = new Edifact.Generic.UNBSegment();
				unbSegment.Parse(new BatchProcessor.ZACharacterSet(), lastInterchange.EI_HeaderText);
				ZDateTime.TryParseExact(unbSegment.DateTimeOfPreparation.Date, out result, "yyyyMMdd");
			}
			return result;
		}

		public bool HasResponses
		{
			get { return Messages.Cast<EDIMessage>().Any(m => m.EM_ApplicationCode == EDIMessage.ApplicationCodes.SouthAfricanCustoms && m.EM_ReceiveTransmit == EDIMessage.Direction.Receive); }
		}

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked
		{
			get { return !HasResponses; }
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded && bondValidToDateModified && !MovementReferenceNumber.IsEmpty)
			{
				UpdateWhsBondedWarehouseAttributeCustomsDeadline(MovementReferenceNumber, CH_BondValidToDate);
			}
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				this.AutoRateDSBOnSavedIfNecessary();
			}
		}

		void UpdateWhsBondedWarehouseAttributeCustomsDeadline(string mrn, ZDate deadline)
		{
			var inventories =
				Factory.Load<WhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_EntryKey, mrn));
			inventories.ForEach(i => i.WB_CustomsDeadline = deadline);
			Factory.Save();
		}

		protected override void AddExtraRequiredFieldsMessageError(ZStringBuilder messageErrors, bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			base.AddExtraRequiredFieldsMessageError(messageErrors, checkProduct, checkQuantity, checkEntryDetails);
			if (IsImportByExternalBroker)
			{
				var lines = InvoiceLines.OfType<JobComInvoiceLine>().ToList();
				if (lines.Any(x => x.JI_PreviousEntryLineNumber.IsEmpty))
				{
					messageErrors.Append(Res.GetString("99446BA0-B874-4A2C-A18C-A0AFC6FF2460", "An Invoice Line marked for Bonded Warehousing must have a WHS MRN line number; not all Invoice Lines marked for Bonded Warehousing have a WHS MRN line number specified."));
				}
				else
				{
					ZShort? duplicateNumber = null;
					foreach (var line in lines.ToArray())
					{
						lines.Remove(line);
						var entryLineNumber = line.JI_PreviousEntryLineNumber;
						if (lines.Any(x => x.JI_PreviousEntryLineNumber == entryLineNumber))
						{
							duplicateNumber = entryLineNumber;
							break;
						}
					}
					if (duplicateNumber.HasValue)
					{
						messageErrors.Append(Res.GetString("ADA0944E-D7B2-4D5F-A126-BBA37C31B6DD", "For an Import By External Broker job WHS MRN line numbers must be unique per Entry Instruction; WHS MRN Line '{0}' has been entered more than once.", duplicateNumber));
					}
				}
			}
		}

		#region IDocumentSupport Members

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

		#endregion

		#endregion

		#region Public Methods

		public void ClearVOCBeforeValuesIfMRNNotEntered()
		{
			if (MovementReferenceNumber.IsEmpty)
			{
				VoucherOfCorrectionValueBefores.Cast<VoucherOfCorrectionValueBefore>().ForEach(x => x.CY_Value = ZDecimal.Zero);
			}
		}

		public void ClearVOCAfterValuesIfMRNNotEntered()
		{
			if (MovementReferenceNumber.IsEmpty)
			{
				VoucherOfCorrectionValueAfters.Cast<VoucherOfCorrectionValueAfter>().ForEach(x => x.CY_Value = ZDecimal.Zero);
			}
		}

		internal void RefreshDoNotClaimVATRefundIfNecessary()
		{
			if (DoNotClaimVATRefund_ReadOnly)
			{
				SetDoNotClaimVATRefund(false, false);
			}
		}

		#endregion

		#region Bonded Warehouse Integration

		protected override bool IsOutwardBondedWarehousingEnabledCore
		{
			get { return HasLineComingOutOfABondedWarehouse; }
		}

		protected override bool IsInwardBondedWarehousingEnabledCore
		{
			get { return HasLineGoingIntoABondedWarehouse; }
		}

		#endregion

		public ZBool DoNotClaimVATRefund
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.DoNotClaimVATRefund); }
			set { SetDoNotClaimVATRefund(value, true); }
		}

		void SetDoNotClaimVATRefund(ZBool input, bool refreshVATBinding)
		{
			this.SetSystemDefinedValue(Schema.DoNotClaimVATRefund, input);
			if (refreshVATBinding)
			{
				this.ValueAddedTaxInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DoNotClaimVATRefunInfo
		{
			get { return GetZPropertyInfo(Schema.DoNotClaimVATRefund); }
		}

		public bool DoNotClaimVATRefund_ReadOnly
		{
			get { return !IsVOCEntry || ValueAddedTaxBefore < GetCalcFeeValues().ValueAddedTax; }
		}

		#region Collections

		public List<ResendableResponseInformation> ResendableResponseList
		{
			get
			{
				List<ResendableResponseInformation> result = new List<ResendableResponseInformation>();
				var message = Messages.GetLastMessage(MessageProcessor.MessageHelper.ApplicationCode, SARSEDIMessage.MessageTypes.CUSRES_REQDOC, Messaging.Business.EDIInterchange.Direction.Receive) as ZAMessage;
				if (message != null)
				{
					var helper = CUSRESMessageHelper.New(message);
					foreach (ResendableResponseInformation ftxHelper in helper.ResendableResponseInformationList(SenderID))
					{
						result.Add(ftxHelper);
					}
				}
				return result;
			}
		}

		[ChildEditable]
		public VoucherOfCorrectionValueBeforeCollection<CusEntryHeader> VoucherOfCorrectionValueBefores
		{
			get
			{
				if (voucherOfCorrectionValueBefores == null)
				{
					voucherOfCorrectionValueBefores = new VoucherOfCorrectionValueBeforeCollection<CusEntryHeader>(this);
					voucherOfCorrectionValueBefores.Load();
					RegisterEditableChildObject(voucherOfCorrectionValueBefores);
				}
				return voucherOfCorrectionValueBefores;
			}
		}
		VoucherOfCorrectionValueBeforeCollection<CusEntryHeader> voucherOfCorrectionValueBefores;

		[ChildEditable]
		public CusEntryHeaderVoucherOfCorrectionValueAfterCollection VoucherOfCorrectionValueAfters
		{
			get
			{
				if (voucherOfCorrectionValueAfters == null)
				{
					voucherOfCorrectionValueAfters = new CusEntryHeaderVoucherOfCorrectionValueAfterCollection(this);
					voucherOfCorrectionValueAfters.Load();
					RegisterEditableChildObject(voucherOfCorrectionValueAfters);
				}
				return voucherOfCorrectionValueAfters;
			}
		}
		CusEntryHeaderVoucherOfCorrectionValueAfterCollection voucherOfCorrectionValueAfters;

		public ProvisionalPaymentEntryPayInfoCollection ProvisionalPaymentPayInfos
		{
			get
			{
				if (provisionalPaymentPayInfos == null)
				{
					provisionalPaymentPayInfos = new ProvisionalPaymentEntryPayInfoCollection(Factory, new CusEntryHeader[] { this });
				}
				return provisionalPaymentPayInfos;
			}
		}
		ProvisionalPaymentEntryPayInfoCollection provisionalPaymentPayInfos;

		#endregion

		public bool NeedsNewBGMReference;

		#region Implementation

		protected override bool GetSupportsBondedWarehousingForEntry(Customs.Business.CusEntryInstruction entryInstruction)
		{
			var result = false;
			if (entryInstruction.HasBothOutOfAndIntoRegimeProcedure)
			{
				result = entryInstruction.ClientIsBondedWarehousing || entryInstruction.WarehouseIsBondedWarehousing || entryInstruction.Warehouse2IsBondedWarehousing || entryInstruction.OwnerIsBondedWarehousing;
			}
			else if (entryInstruction.HasIntoWarehouseProcedure)
			{
				result = entryInstruction.Warehouse2IsBondedWarehousing || entryInstruction.ClientIsBondedWarehousing;
			}
			else if (entryInstruction.HasOutOfWarehouseProcedure)
			{
				result = entryInstruction.WarehouseIsBondedWarehousing || entryInstruction.ClientIsBondedWarehousing;
			}
			return result;
		}

		internal void CopyMessageVOCAfterValuesToBefore(IVOCAfterValues afterValues)
		{
			CIFValueBefore = afterValues.CIFValue;
			CustomsValueBefore = afterValues.CustomsValue;
			CustomsDutyExcluding12BBefore = afterValues.CustomsDutyNoS1P2B;
			S1P2BDutyBefore = afterValues.S1P2BDuty;
			ValueAddedTaxBefore = afterValues.ValueAddedTax;
			PenaltyAmountBefore = afterValues.PenaltyAmount;
			ProvisionalPaymentAmountBefore = afterValues.ProvisionalPaymentAmount;
		}

		internal CalcFeeValues GetCalcFeeValues()
			=> Factory.GetValue(ref calcFeeValuesCached, () =>
				{
					var result = new CalcFeeValues();
					foreach (CusEntryLine entryLine in MergedLines)
					{
						var source = entryLine.GetCalcFeeValues();
						result.CustomsDutyExcluding12B += source.CustomsDutyExcluding12B;
						result.S1P2BDuty += source.S1P2BDuty;
						result.ValueAddedTax += source.ValueAddedTax;
						result.ProvisionalPayment += source.ProvisionalPayment;
						result.Penalty += source.Penalty;
						result.CustomsDutiesSchedule1P1andSchedule2 += source.CustomsDutiesSchedule1P1andSchedule2;
					}
					RefreshDoNotClaimVATRefundIfNecessary();
					return result;
				});
		CachedProperty<CalcFeeValues> calcFeeValuesCached;

		internal ZDecimal TotalDutiesAndTaxes
		{
			get
			{
				var result = ZDecimal.Zero;
				var calcFeeValues = GetCalcFeeValues();
				result += calcFeeValues.CustomsDutyExcluding12B;
				result += calcFeeValues.S1P2BDuty;
				result += calcFeeValues.ProvisionalPayment;
				result += calcFeeValues.Penalty;
				result += calcFeeValues.ValueAddedTax;
				return result;
			}
		}

		protected override void OnChangingCH_Status(ZString oldStatus, ZString newStatus)
		{
			if (oldStatus != newStatus)
			{
				var message = Messages.GetLastMessage(SARSEDIMessage.ApplicationCodes.SouthAfricanCustoms);
				if (message != null && message.EM_MessageType != SARSEDIMessage.MessageTypes.REQDOC)
				{
					if (ZAMessageStatusList.IsAwaiting(newStatus))
					{
						if (!CH_EntryStatus.IsEmpty)
						{
							CH_EntryStatus = ZString.Empty;
						}
					}
					else if (ZAMessageStatusList.IsError(newStatus))
					{
						ResetEntryStatusToLatestReceived();
					}
				}
			}
		}

		void ResetEntryStatusToLatestReceived()
		{
			foreach (var message in Messages.OfType<ZAMessage>().Where(x => x.EM_ApplicationCode == SARSEDIMessage.ApplicationCodes.SouthAfricanCustoms
				&& x.EM_MessageType == SARSEDIMessage.MessageTypes.CUSRES
				&& x.EM_ReceiveTransmit == SARSEDIMessage.Direction.Receive).OrderBy(x => GetMessageDate(x)))
			{
				var helper = CUSRESMessageHelper.New(message);
				if (helper != null && helper.DoesEntryStatusNeedsToBeUpdated(this))
				{
					CH_EntryStatus = helper.EntryStatus;
					break;
				}
			}
		}

		ZDateTime GetMessageDate(ZAMessage message)
		{
			var result = message.PreparationDate;
			return result.IsValid ? result : message.EM_DateTimeInterchangeSent;
		}

		protected override string GetDutyCode()
		{
			return (Declaration?.IsDeclarationIntegrated ?? false) ? Enterprise.Core.Constants.Customs.CusEntryFeeTypes.DutyAmount : string.Empty;
		}

		protected override string GetTaxCode()
		{
			return (Declaration?.IsDeclarationIntegrated ?? false) ? Enterprise.Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount : string.Empty;
		}

		public new CusEntryLine RandomEntryLine => (CusEntryLine)base.RandomEntryLine;

		public ZDateTime EntryInstructionAssessmentDate
		{
			get { return CusEntryInstruction.GetEffectiveAssessmentDate(EntryInstruction, Factory); }
		}

		#region AmendmentNotification Message Related

		public ZString LastReceivedCaseNumber
		{
			get
			{
				var result = ZString.Empty;
				if (LastReceivedCUSRESMessage != null)
				{
					result = LastReceivedCUSRESMessageHelper?.CaseNumber ?? ZString.Empty;
				}
				return result;
			}
		}

		public bool IsAmendmentNotificationReceived => LastReceivedCUSRESMessage != null && (LastReceivedCUSRESMessageHelper?.EntryStatus ?? ZString.Empty) == "26";

		public CUSRESMessageHelper LastReceivedCUSRESMessageHelper => Factory.GetValue(ref lastReceivedCUSRESMessageHelperCached, () => CUSRESMessageHelper.New(LastReceivedCUSRESMessage as ZAMessage));

		CachedProperty<CUSRESMessageHelper> lastReceivedCUSRESMessageHelperCached;

		EDIMessage LastReceivedCUSRESMessage => Factory.GetValue(ref lastReceivedCUSRESMessageCached, () => Messages.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSRES, EDIMessage.Direction.Receive));

		CachedProperty<EDIMessage> lastReceivedCUSRESMessageCached;

		#endregion

		public EDIMessage LastSentCUSDECMessage => Factory.GetValue(ref lastSentCUSDECMessageCached, () => Messages.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSDEC,
					EDIMessage.Direction.Transmit, Array.Empty<ZString>(), Array.Empty<ZString>(), new ZString[] { ZAMessage.Status.Discarded }, Array.Empty<ZString>()));

		CachedProperty<EDIMessage> lastSentCUSDECMessageCached;

		internal EDIFACTMessageStatusCalculator MessageStatusCalculator
		{
			get { return messageStatusCalculator ?? (messageStatusCalculator = new EDIFACTStatusCalculator(SARSEDIMessage.MessageTypeNames.CUSDEC)); }
		}
		EDIFACTMessageStatusCalculator messageStatusCalculator;

		public ZBool GetImporterPays()
		{
			var mapping = FinancialAccountNumberPortMappings.GetMappingFor(Declaration.Country, AgentCode, CustomsOffice, CH_PaymentMethod);
			return mapping?.ImporterPays ?? ZBool.False;
		}

		public FinancialAccountNumberPortMap GetFinancialAccountMapping()
		{
			if (Declaration != null)
			{
				return FinancialAccountNumberPortMappings.GetMappingFor(Declaration.Country, AgentCode, CustomsOffice, CH_PaymentMethod);
			}
			return null;
		}

		public ZString GetFinancialAccountNumber()
		{
			if (Declaration != null)
			{
				var mapping = GetFinancialAccountMapping();
				return mapping?.FinancialAccountNumber ?? ZString.Empty;
			}
			return ZString.Empty;
		}

		public ZGuid CreditorPK
		{
			get
			{
				if (!creditorPK.HasValue)
				{
					creditorPK = ZGuid.Empty;
					if (Declaration != null)
					{
						creditorPK = FinancialAccountNumberPortMappings.GetMappingFor(Declaration.Country, AgentCode, CustomsOffice, CH_PaymentMethod)?.CreditorPK ?? ZGuid.Empty;
					}
				}
				return creditorPK.Value;
			}
		}
		ZGuid? creditorPK;

		public ZGuid GetDefaultCreditorPK(EntryChargeType chargeType)
		{
			return CH_PaymentMethod == PaymentMethodCodeList.Codes.VATOnly && chargeType.ChargeCodeForRating != RateTypes.VATNormal
				? (FinancialAccountNumberPortMappings.OfType<FinancialAccountNumberPortMap>().FirstOrDefault(x => (x.Organization?.GetAgentCode(Declaration.Country) ?? ZString.Empty) == AgentCode && x.Cash)?.CreditorPK ?? ZGuid.Empty)
				: CreditorPK;
		}

		protected override ICustomsCharges GetCustomsChargesProvider()
		{
			return new InterfaceImplementations.CusEntryHeaderCustomsCharges(this);
		}

		public IEnumerable<AccTransactionHeader> RelatedARInvoices
		{
			get
			{
				if (relatedARInvoices == null)
				{
					var jobHeaderPK = Declaration?.Job?.PK ?? ZGuid.Empty;
					var entryCreditorPK = CreditorPK;
					if (jobHeaderPK.IsValid && entryCreditorPK.IsValid)
					{
						var jobChargeQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AL_ARLine);
						jobChargeQuery.AddToFilter(JobChargeSchema.JR_JH, jobHeaderPK);
						jobChargeQuery.AddToFilter(JobChargeSchema.JR_OH_CostAccount, entryCreditorPK);
						var accLineQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
						accLineQuery.AddSubQuery(jobChargeQuery, JoinCondition.And);
						var accHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
						accHeaderQuery.AddSubQuery(accLineQuery, JoinCondition.And);
						relatedARInvoices = Factory.Load<AccTransactionHeader>(accHeaderQuery)?.OrderBy(x => x.AH_TransactionNum);
					}
					if (relatedARInvoices == null)
					{
						relatedARInvoices = Array.Empty<AccTransactionHeader>();
					}
				}
				return relatedARInvoices;
			}
		}
		IEnumerable<AccTransactionHeader> relatedARInvoices;

		FinancialAccountNumberPortMapCollection FinancialAccountNumberPortMappings
		{
			get { return financialAccountNumberPortMappings ?? (financialAccountNumberPortMappings = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty)); }
		}
		FinancialAccountNumberPortMapCollection financialAccountNumberPortMappings;

		#endregion

		#region IVOCAfterValues

		IVOCAfterValues VOCAfterValues
		{
			get { return this; }
		}

		ZDecimal IVOCAfterValues.CIFValue
		{
			get { return CIFInLocalCurrencyRounded; }
		}

		ZDecimal IVOCAfterValues.CustomsValue
		{
			get { return CustomsValue; }
		}

		ZDecimal IVOCAfterValues.CustomsDutyNoS1P2B
		{
			get { return CustomsDutyExcluding12BAfter; }
		}

		ZDecimal IVOCAfterValues.S1P2BDuty
		{
			get { return S1P2BDutyAfter; }
		}

		ZDecimal IVOCAfterValues.ValueAddedTax
		{
			get { return ValueAddedTax; }
		}

		ZDecimal IVOCAfterValues.ProvisionalPaymentAmount
		{
			get { return ProvisionalPaymentAmountAfter; }
		}

		ZDecimal IVOCAfterValues.PenaltyAmount => PenaltyAmountAfter;

		#endregion

		#region IVOCBeforeValues

		IVOCBeforeValues VOCBeforeValues
		{
			get { return this; }
		}

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
			get { return CustomsDutyExcluding12BBefore; }
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

		ZDecimal IVOCBeforeValues.PenaltyAmount => PenaltyAmountBefore;

		#endregion

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.VOCValueBefore, typeof(VoucherOfCorrectionValueBefore));
			return result;
		}

		#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider

		public void AddMessage(EDIMessage message)
		{
			this.Messages.Add(message);
		}

		public ZString MessageStatus
		{
			get { return CH_Status; }
			set { CH_Status = value; }
		}

		public ZString JobStatus
		{
			set { CH_EntryStatus = value; }
			get { return CH_EntryStatus; }
		}

		public ZString JobIdentification => CH_BGMReference;

		public BusinessObject TopLevelBusinessObject => this;

		EDIFACTMessageStatusCalculator IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider.GetCalculator(string countryCode)
		{
			return countryCode == Core.Constants.CountryCodes.SouthAfrica ? MessageStatusCalculator : null;
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;
		#endregion

		#region IAutoRateDSBOnSaved

		public void MarkNeedsAutoRateDSB()
		{
			needsAutoRateDSBOnSaved = true;
		}

		ZBool needsAutoRateDSBOnSaved;

		public void ClearNeedsAutoRateDSB()
		{
			needsAutoRateDSBOnSaved = false;
		}

		public void AutoRateDSBOnSavedIfNecessary()
		{
			if (needsAutoRateDSBOnSaved
				&& (!Declaration?.IsDeclarationIntegrated ?? false)
				&& CustomsDataRegistry.Instance.CreditCheckOnMessageSend.GetValueWithoutFallback(Declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				new InvoicePostingAccountingIntegrator().IntegrateIfNecessary(new JobDeclarationIAccIntegrationDataProvider(ChargePosterBehaviours.AutoRateDSB, new ZGuid[] { this.PK }, Declaration.PK, true, Factory));
			}
			needsAutoRateDSBOnSaved = false;
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg => ((ICustomLabelsConfigOrgProvider)Declaration).ConfigOrg;

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add => ((ICustomLabelsConfigOrgProvider)Declaration).ConfigOrgChanged += value;
			remove => ((ICustomLabelsConfigOrgProvider)Declaration).ConfigOrgChanged -= value;
		}

		#endregion

		#region IAccInvoiceDataProvider
		ZDateTime IAccInvoiceDataProvider.InvoiceDate
		{
			get
			{
				var messageHelper = LastReceivedCUSRESMessageHelper;
				if (((string)messageHelper?.EntryStatus == "27" && messageHelper.PostingDate == ZDateTime.Empty)
					|| messageHelper == null)
				{
					return ZDateTime.Today;
				}
				return messageHelper.PostingDate;
			}
		}
		#endregion

		#region Constants

		public static class RateTypes
		{
			public const string VATNormal = "VAT";
		}

		#endregion

		public override ZInt GetPermitReferenceNumberLine() => CH_EntryNumber;

		public bool IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected
		{
			get
			{
				return MovementReferenceNumber.IsEmpty
					&& (MessageStatus == ZAMessageStatusList.Codes.NotSent
						|| CustomsStatusAttributeHelper.IsStatusRejected(Factory, JobStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today));
			}
		}

		public ZString HAWBOverride => (EntryInstruction?.CEI_HAWBOverride ?? ZString.Empty).IsEmpty ? Declaration.JE_HouseBill : EntryInstruction.CEI_HAWBOverride;

		public ZDateTime HawbDateOverride => (EntryInstruction?.CEI_HAWBDateOverride ?? ZDateTime.Empty).IsEmpty ? Declaration.HouseBillIssuedDate : EntryInstruction.CEI_HAWBDateOverride;

		public ZString CargoCarrierOverride => (EntryInstruction?.CEI_CargoCarrierOverride ?? ZString.Empty).IsEmpty ? Declaration.JE_CargoCarrier : EntryInstruction.CEI_CargoCarrierOverride;

		public void BackPopulateInvoiceLineTargetEntryLineNumberIfNeeded()
		{
			foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
			{
				var lineNumber = entryLine.CL_LineNumber;
				foreach (JobComInvoiceLine invLine in entryLine.InvoiceLines)
				{
					if (invLine.JI_TargetEntryLineNumber.IsEmpty)
					{
						using (invLine.GetValidationSuspender())
						{
							invLine.JI_TargetEntryLineNumber = lineNumber;
						}
					}
				}
			}
		}

		public void UpdateLRNIfNeeded(ZString newLRN, bool isLRNEditable)
		{
			var oldLRN = EntryHeader.CH_BGMReference;
			if (!newLRN.IsEmpty && isLRNEditable && newLRN != oldLRN)
			{
				EntryHeader.CH_BGMReference = newLRN;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				EntryHeader.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, ZString.Format("Local Reference Number updated:{0} => {1}", oldLRN, newLRN));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}
	}
}
