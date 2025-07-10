using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), "CustomsEntryHeaders")]
	public abstract class CusEntryHeader : AutoNZCusEntryHeader, IServiceLocator, Integration.Customs.NZ.ICusEntryHeader
	{
		public static readonly new TypeDecider TypeDecider = new CusEntryHeaderTypeDecider();

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : Customs.Business.CusEntryHeader.Schema
		{
			public const string CH_IsActive = "CH_IsActive";
			public const string CH_IsEntryCancelled = "CH_IsEntryCancelled";
			public const string CH_RecordAdded = "CH_RecordAdded";
			public const string CH_LastEntryStyle = "CH_LastEntryStyle";
			public const string CH_LastNumberOfLinesSentToCustoms = "CH_LastNumberOfLinesSentToCustoms";
			public const string CH_EDITransmitDate = "CH_EDITransmitDate";
			public const string CH_LastResponseVersionNumber = "CH_LastResponseVersionNumber";
			public const string CH_TotalAmountReturned = "CH_TotalAmountReturned";
			public const string CH_IsRestored = "CH_IsRestored";
			public const string CH_MPIFoodResponseTime = "CH_MPIFoodResponseTime";
			public const string CH_MPIFoodStatus = "CH_MPIFoodStatus";
			public const string CH_MPIBioResponseTime = "CH_MPIBioResponseTime";
			public const string CH_MPIBioStatus = "CH_MPIBioStatus";
			public const string CH_NZCSResponseTime = "CH_NZCSResponseTime";
			public const string CH_NZCSStatus = "CH_NZCSStatus";
			public const string CH_EntryChargeWaived = "CH_EntryChargeWaived";
			public const string DutyTotalInMergedLines = "DutyTotalInMergedLines";
			public const string TotalMisc = "TotalMisc";
			public const string TotalEntryFeeAmount = "TotalEntryFeeAmount";
			public const string TotalAmountPayableIncludingEntryFee = "TotalAmountPayableIncludingEntryFee";
			public const string CH_MPIBioMovementStatus = "CH_MPIBioMovementStatus";
			public const string CH_MPIBioMovementStatusTime = "CH_MPIBioMovementStatusTime";
			public const string CH_NZCSMovementStatus = "CH_NZCSMovementStatus";
			public const string CH_NZCSMovementStatusTime = "CH_NZCSMovementStatusTime";
		}
		#endregion

		#region Messages

		[ChildEditable(true)]
		public new NZCMessageCollection Messages
		{
			get { return (NZCMessageCollection)base.Messages; }
		}

		protected override EDIMessageCollection GetNewMessageCollection()
		{
			return new NZCMessageCollection(this);
		}

		#endregion

		#region MergedLines

		public new CusEntryLineCollection<CusEntryLine> MergedLines
		{
			get { return (CusEntryLineCollection<CusEntryLine>)base.MergedLines; }
		}

		protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
		{
			return new CusEntryLineCollection<CusEntryLine>(this);
		}

		#endregion

		public new JobDeclaration Declaration
		{
			get { return base.Declaration as JobDeclaration; }
		}

		#region Charges and Associated Fields/Overrides

		public override bool IsFeePaidByBroker(string feeCode, ZString paymentMethod, ILogger logger)
		{
			return !ShouldSendPaymentMethodInMessage//then broker is assumed to pay for entries by customs
				|| PaymentMethodList.IsPaidByBroker(Declaration.JE_PaymentMethod);
		}

		protected internal bool ShouldSendPaymentMethodInMessage
		{
			get { return Declaration.IsImport || HasAmountToPayOrRefundOtherThanEntryFee; }
		}

		protected virtual bool HasAmountToPayOrRefundOtherThanEntryFee
		{
			get { return !TotalAmountPayable.IsEmpty; }
		}

		protected override ZDecimal GetTotalChargeValueFor(Enterprise.Registry.Business.Customs.EntryChargeType chargeType, ZString methodOfPayment)
		{
			if (Declaration.IsExport
				&& (chargeType.Code == Registry.EntryChargeTypeList.Codes.EntryFee || chargeType.Code == Registry.EntryChargeTypeList.Codes.EntryFeeGST)
				&& NZCustomsDataRegistry.Instance.ExportEntryFeeChargeCode.GetValueWithoutFallback(RegistryCompanyPK, Guid.Empty, Guid.Empty) == Guid.Empty)
			{
				return ZDecimal.Zero;
			}
			return base.GetTotalChargeValueFor(chargeType, methodOfPayment);
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetEntryChargeTypeList()
		{
			return Factory.GetCachedValue<EntryChargeTypeList>();
		}

		[ChildEditable(true)]
		public new ICusEntryHeaderChargesCollection<CusEntryHeaderCharge> Charges => (CusEntryHeaderChargesCollection<CusEntryHeaderCharge>)base.Charges;

		protected override ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharge>(this);

		public ZDecimal EntryFeeAmount
		{
			get { return Charges.GetAmount(Registry.EntryChargeTypeList.Codes.EntryFee); }
			set { Charges[Registry.EntryChargeTypeList.Codes.EntryFee].C1_ChargeAmount = value; }
		}

		public ZDecimal EntryFeeGST
		{
			get { return Charges.GetAmount(Registry.EntryChargeTypeList.Codes.EntryFeeGST); }
			set { Charges[Registry.EntryChargeTypeList.Codes.EntryFeeGST].C1_ChargeAmount = value; }
		}

		protected override ZString GetUniqueNumberForAccountingIntegrationCore()
		{
			var result = base.GetUniqueNumberForAccountingIntegrationCore();
			if (!result.IsEmpty && Declaration is { } declaration && ConsolidatedDeclaration.IsConsolidated(declaration))
			{
				result += $"-{CH_ConsolidatedEntryMemberID}";
			}
			return result;
		}

		#endregion

		#region AddInfo

		#region CH_IsActive

		public override ZBool CH_IsActive
		{
			get => base.CH_IsActive;
			set
			{
				var oldValue = CH_IsActive;
				base.CH_IsActive = value;
				if (!isDefaultingInProgress && !IsCopying && oldValue != CH_IsActive)
				{
					((IAddInfoManager)this).AddInfo?.UpdateRelatedPropertyInfo();
					MarkAsNeedingValidation();
					Declaration?.MarkAsNeedingValidation();

					if (oldCH_IsActiveForLogging.HasValue && oldCH_IsActiveForLogging.Value == CH_IsActive)
					{
						oldCH_IsActiveForLogging = null;
					}
					else
					{
						oldCH_IsActiveForLogging = oldValue;
					}
				}
			}
		}

		#endregion

		#region CH_IsEntryCancelled

		public override ZBool CH_IsEntryCancelled
		{
			get => base.CH_IsEntryCancelled;
			set
			{
				var oldValue = CH_IsEntryCancelled;
				base.CH_IsEntryCancelled = value;
				if (!IsCopying && oldValue != CH_IsEntryCancelled && CH_IsEntryCancelled)
				{
					Logs.AddNew(Events.Cancelled, EntryNumber);
				}
			}
		}

		#endregion

		#region CH_EDITransmitDate

		public override ZDateTime CH_EDITransmitDate
		{
			get => base.CH_EDITransmitDate;
			set
			{
				var oldValue = CH_EDITransmitDate;
				base.CH_EDITransmitDate = value;
				if (!IsCopying && oldValue != CH_EDITransmitDate)
				{
					((IAddInfoManager)this).AddInfo?.UpdateRelatedPropertyInfo();
				}
			}
		}

		#endregion

		#region TSW Response Values

		#region CH_MPIFoodResponse

		public ZString CH_MPIFoodStatusDesc
		{
			get { return CH_MPIFoodStatus.IsEmpty ? string.Empty : Lookups.TSWMessageStatusList.GetDescriptionFromCode(CH_MPIFoodStatus); }
		}

		#endregion

		#region CH_MPIBioResponse

		public ZString CH_MPIBioStatusDesc
		{
			get { return CH_MPIBioStatus.IsEmpty ? string.Empty : Lookups.TSWMessageStatusList.GetDescriptionFromCode(CH_MPIBioStatus); }
		}

		#endregion

		#region CH_MPIBioMovementStatus

		public ZString CH_MPIBioMovementStatusDesc
		{
			get { return CH_MPIBioMovementStatus.IsEmpty ? string.Empty : Lookups.TSWMovementStatusList.GetDescriptionFromCode(CH_MPIBioMovementStatus); }
		}

		#endregion

		#region CH_NZCSResponse

		public ZString CH_NZCSStatusDesc
		{
			get { return CH_NZCSStatus.IsEmpty ? string.Empty : Lookups.TSWMessageStatusList.GetDescriptionFromCode(CH_NZCSStatus); }
		}

		#endregion

		#region CH_NZCSMovementStatus

		public ZString CH_NZCSMovementStatusDesc
		{
			get { return CH_NZCSMovementStatus.IsEmpty ? string.Empty : Lookups.TSWMovementStatusList.GetDescriptionFromCode(CH_NZCSMovementStatus); }
		}

		#endregion

		#endregion

		#region CH_EntryChargeWaived

		public override ZBool CH_EntryChargeWaived
		{
			get => base.CH_EntryChargeWaived;
			set
			{
				base.CH_EntryChargeWaived = value;
				if (value)
				{
					foreach (CusEntryHeaderCharge charge in Charges.ToList())
					{
						if (charge.IsWaivable)
						{
							Charges.RemoveAndDelete(charge);
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region CH_EntryStatus

		public override ZString CH_EntryStatus
		{
			get { return base.CH_EntryStatus; }
			set
			{
				if (base.CH_EntryStatus != value)
				{
					if (base.CH_EntryStatus == FormalEntryStatusList.Codes.QueuedForSending &&
						value == FormalEntryStatusList.Codes.NotSentToCustoms)
					{
						CancelDeclarationAmendedPermitEvents();
					}

					base.CH_EntryStatus = value;
				}
			}
		}

		void CancelDeclarationAmendedPermitEvents()
		{
			var declarationAmendedPermitLogs = GetUncancelledDeclarationAmendedPermitEvents();

			foreach (var log in declarationAmendedPermitLogs)
			{
				log.Cancel();
			}
		}

		public StmALog[] GetUncancelledDeclarationAmendedPermitEvents()
		{
			var acknowledgementLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationAmendedPermitApproved.Code);
			acknowledgementLogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
			var logs = Logs.Find(acknowledgementLogQuery);
			return logs;
		}

		#endregion

		protected override void SetDefaultValues()
		{
			try
			{
				isDefaultingInProgress = true;
				base.SetDefaultValues();
				SetMessagingStatusToNotSent();
				CH_IsActive = true;
				CH_RecordAdded = ZDateTime.Now;
			}
			finally
			{
				isDefaultingInProgress = false;
			}
		}
		bool isDefaultingInProgress;

		public override void OnSaving()
		{
			var declaration = Declaration;
			if (!IsInDatabase && IsEntryHeaderCurrentDeclarationType(declaration))
			{
				// querycache has to be cleared so that entry headers created out of this factory can be loaded
				declaration.Factory.ClearQueryCache(CusEntryHeader.Schema.TableName);
				// reload only new entry headers
				declaration.CustomsEntryHeaders.Reload(false);
				var appropriateEntryHeader = declaration.GetAppropriateCusEntryHeaderIfExists();
				if (appropriateEntryHeader != null && !IsDeleted && appropriateEntryHeader.PK != PK)
				{
					var errorMessage = "Duplicated entry header. Declaration: " + declaration.JE_DeclarationReference + ", JE_MessageSubType: " + declaration.JE_MessageSubType + ", CH_MessageType: " + CH_MessageType;
					ErrorReporter.ReportOnce("4029E6ED-483D-4890-B5B9-F5911D476930", errorMessage);
#if DEBUG
					throw new InvalidOperationException(errorMessage);
#endif
				}
			}
			base.OnSaving();
			PopulateCH_BGMReferenceIfNeeded();
			if (oldCH_IsActiveForLogging.HasValue && IsInDatabase)
			{
				activeLog = Logs.AddNew(!oldCH_IsActiveForLogging.Value ? Events.SetToActive : Events.SetToInactive, EntryNumber);
			}
		}
		bool? oldCH_IsActiveForLogging;
		StmALog activeLog;

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment
		{
			get { return false; }
		}

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked => !IsNotCancelledIPIEntry && base.ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked;

		protected override bool IsCustomsImpedimentReceivedEventSupported
		{
			get { return Declaration != null && !Declaration.IsTSWImportDeclaration; }  // NZ creates CIP events for MPI Food & MPI Biosecurity messages, (i.e. Import entries), internally.
		}

		public void PopulateCH_BGMReferenceIfNeeded()
		{
			var declaration = Declaration;
			if (!IsDeleted && !IsDeleting && declaration != null)
			{
				var referencePlaceHolder = CH_BGMReference == TSWConstants.SendersReferencePlaceHolder;
				declaration.PopulateJE_DeclarationReferenceIfNeeded();
				try
				{
					if (referencePlaceHolder)
					{
						CH_BGMReference = ZString.Empty;
					}
					PopulateNumberPropertyIfRequired(CH_BGMReferenceInfo, factory => GetUniqueReference(declaration));
				}
				finally
				{
					if (referencePlaceHolder && CH_BGMReference.IsEmpty)
					{
						CH_BGMReference = TSWConstants.SendersReferencePlaceHolder;
					}
				}
			}
		}

		ZString GetUniqueReference(JobDeclaration declaration)
		{
			var result = declaration.JE_DeclarationReference;
			if (declaration.IsTSWDeclaration)
			{
				if (Declaration.IsCompletion)   // 1) TSW Completion entry requires a unique reference, so we cannot use the same jobno (or generated ref) value already used for the Sight or Temporary entry.
				{
					var orginalEntry = Declaration.EntryHeaderForOriginalEntryNumber;
					if (orginalEntry != null && !orginalEntry.CH_BGMReference.IsEmpty)
					{
						result = orginalEntry.CH_BGMReference;
					}

					if (CH_EntryStatus != FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms)
					{
						if (!result.EndsWith("C", StringComparison.Ordinal))
						{
							result += "C";
						}
					}
				}
				else if (Declaration.IsWriteOffChangedToFormal)
				{
					var writeOffEntry = Declaration.EntryHeaderForWriteOffEntryNumber;
					if (writeOffEntry != null && !writeOffEntry.CH_BGMReference.IsEmpty)
					{
						CH_BGMReference = writeOffEntry.CH_BGMReference;
					}

					if (!result.EndsWith("F", StringComparison.Ordinal))
					{
						result += "F";
					}
				}
				else if (Declaration.IsTSW_IPI_Declaration)
				{
					if (!result.EndsWith("I", StringComparison.Ordinal))
					{
						result += "I";
					}
				}

				if (result.Length > 13)   // 2) TSW Senders Reference Max length is an..14, if job number is too large we need to generate this reference number within the message generation
				{
					var refPrefix = !Declaration.JE_MessageSubType.IsEmpty ? Declaration.JE_MessageSubType : new ZString("TSW");
					result = refPrefix + GetMessageSendersReferenceNumber(refPrefix);
				}
			}

			return result;
		}

		protected string GetMessageSendersReferenceNumber(string refPrefix)
		{
			return Env.NumberFountains.NZTSWSenderReferenceNumberFountain(refPrefix).GetNextFormatted(Factory);
		}

		public virtual bool LastCustomsStatusIsImpediment
		{
			get { return Declaration != null && Declaration.LastCustomsStatusIsImpediment; }
		}

		public override bool IsFormalEntry
		{
			get { return CH_MessageType == EntryHeaderTypes.NZ.FormalEntry; }
		}

		public bool IsIPIEntryHeader
		{
			get { return CH_MessageType == EntryHeaderTypes.NZ.PrimaryIndustries; }
		}

		bool IsNotCancelledIPIEntry
		{
			get { return IsIPIEntryHeader && !HasBeenWithdrawn; }
		}

		public abstract bool IsECIWriteOff { get; }
		public abstract void SetDeclarationStatusesWhenSetToCurrent(JobDeclaration declaration);

		public override bool IsActive
		{
			get { return CH_IsActive; }
			set { CH_IsActive = value; }
		}

		public override bool HasBeenWithdrawn
		{
			get
			{
				//'CAN' is used throughout all types of entries
				return CH_EntryStatus == FormalEntryStatusList.Codes.EntryCancelled;
			}
		}

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			return !HasBeenWithdrawn;
		}

		public bool IsEntryHeaderCurrentDeclarationType(JobDeclaration declaration)
		{
			if (declaration == null)
			{
				return false;
			}
			else
			{
				if (!CH_IsActive && !declaration.IsTSWCancellation)
				{
					return false;
				}
				else
				{
					return GetType() == declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings;
				}
			}
		}

		internal virtual void DeleteIfContainsNoValuableData()
		{
			JobDeclaration declaration = Declaration;
			if (declaration != null
				&& (!CH_IsActive || !IsEntryHeaderCurrentDeclarationType(declaration))
				&& CountryCode == Core.Constants.CountryCodes.NewZealand
				&& Messages.Count == 0
				&& EntryNumber.IsEmpty
				&& !ConsolidatedDeclaration.IsConsolidated(declaration))
			{
				Delete();
			}
		}

		protected override void OnFactorySaving()
		{
			if (!IsInDatabase)
			{
				((IAddInfoManager)this).AddInfo.UpdateRelatedPropertyInfo();
			}
			base.OnFactorySaving();
			DeleteIfContainsNoValuableData();
		}

		public NZCMessage[] CancelQueuedMessages()
		{
			var cancelledMessages = new List<NZCMessage>();

			foreach (NZCMessage message in Messages)
			{
				if (message.IsQueuedToBeSentLater)
				{
					message.EM_Status = NZCMessage.Status.Cancelled;
					cancelledMessages.Add(message);
				}
			}

			return cancelledMessages.ToArray();
		}

		public ZDateTime DateMessageQueuedToBeSentOn
		{
			get
			{
				ZDateTime result = CH_EDITransmitDate;
				foreach (NZCMessage message in Messages)
				{
					if (message.IsQueuedToBeSentLater)
					{
						result = message.EM_HeldUntilDate.ToLocalBranchTime(Factory);
					}
				}
				return result;
			}
		}

		public bool HasNonCancelledMessages
		{
			get
			{
				bool result = false;
				foreach (NZCMessage message in Messages)
				{
					if (message.EM_Status != NZCMessage.Status.Cancelled)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public void SetMessagingStatusToNotSent()
		{
			SetMessagingStatusToNotSentInternal();
		}

		protected abstract void SetMessagingStatusToNotSentInternal();

		protected override bool IsExportCore()
		{
			JobDeclaration declaration = Declaration;
			return declaration != null && declaration.IsExport;
		}

		[ReadOnly(true)]
		public override ZString CH_CustomsDeliveryInstructions
		{
			get { return base.CH_CustomsDeliveryInstructions; }
			set { base.CH_CustomsDeliveryInstructions = value; }
		}

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation()
		{
			return new CusEntryHeaderValidation(this);
		}

		#region Totals Aggregated from MergedLines

		public ZDecimal DutyTotalInMergedLines
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (CusEntryLine line in MergedLines)
				{
					result += line.DutyAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo DutyTotalInMergedLinesInfo
		{
			get { return GetZPropertyInfo(Schema.DutyTotalInMergedLines); }
		}

		public ZDecimal VFDWholeNZD
		{
			get
			{
				ZDecimal result = 0m;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					result += entryLine.VFDWholeNZD;
				}
				return result;
			}
		}

		public ZDecimal DutyCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.DutyCreditAmount;
				}
				return result;
			}
		}

		public ZDecimal GSTCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.GSTCreditAmount;
				}
				return result;
			}
		}

		public ZDecimal DutyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.DutyAmount;
				}
				return result;
			}
		}

		public ZDecimal TotalMisc
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.TotalMisc;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalMiscInfo
		{
			get { return GetZPropertyInfo(Schema.TotalMisc); }
		}

		public ZInt NumberOfEntryLines
		{
			get { return MergedLines.Count; }
		}

		public override ZDecimal TotalAmountPayable
		{
			get
			{
				ZDecimal result = CH_TotalPaid;
				if (result.IsEmpty)
				{
					foreach (CusEntryLine mergedLine in MergedLines)
					{
						result += mergedLine.TotalAmountPayable;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalEntryFeeAmount
		{
			get
			{
				return EntryFeeAmount + EntryFeeGST;
			}
		}

		public ZPropertyInfo TotalEntryFeeAmountInfo
		{
			get { return GetZPropertyInfo(Schema.TotalEntryFeeAmount); }
		}

		public ZDecimal TotalAmountPayableIncludingEntryFee
		{
			get
			{
				var declaration = Declaration;
				if (declaration != null && declaration.IsExport && (declaration.IsDrawback || declaration.IsCompletion))
				{
					return TotalAmountPayable - TotalEntryFeeAmount;
				}
				else
				{
					return TotalAmountPayable + TotalEntryFeeAmount;
				}
			}
		}

		public ZPropertyInfo TotalAmountPayableIncludingEntryFeeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalAmountPayableIncludingEntryFee); }
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				oldCH_IsActiveForLogging = null;
			}
			else
			{
				activeLog?.Delete();
				if (IsInDatabase)
				{
					CH_EntryStatus = (ZString)CH_EntryStatusInfo.OriginalValue;
				}
				else
				{
					SetMessagingStatusToNotSent();
					CH_BGMReference = ZString.Empty;
				}
			}
			activeLog = null;
		}

		object IServiceLocator.GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new InterfaceImplementations.CusEntryHeaderCustomsCharges(this);
			}
			return null;
		}

		protected override bool IsChangingToClearStatusForAccIntegration
		{
			get
			{
				var isFormalEntry = CH_MessageType == EntryHeaderTypes.NZ.FormalEntry;
				var declaration = Declaration;
				return isFormalEntry && declaration != null && !IsClearedEntryStatus((ZString)declaration.JE_EntryStatusInfo.OriginalValue) && IsClearedEntryStatus(declaration.JE_EntryStatus);
			}
		}

		ZBool IsClearedEntryStatus(ZString entryStatus)
		{
			return FormalEntryStatusList.IsDeliveryStatusCleared(entryStatus);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("3B0EF767-0704-4704-939E-517CEAB73E6B", "ECI Manifesting {0}", CH_BGMReference);

		#region ICustomsCharges members
		protected override ICustomsCharges GetCustomsChargesProvider()
		{
			return new InterfaceImplementations.CusEntryHeaderCustomsCharges(this);
		}
		#endregion

		#region Lookups

		public new CusEntryHeaderLookups Lookups
		{
			get { return (CusEntryHeaderLookups)base.Lookups; }
		}

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			return new CusEntryHeaderLookups(this);
		}

		#endregion

	}
}
