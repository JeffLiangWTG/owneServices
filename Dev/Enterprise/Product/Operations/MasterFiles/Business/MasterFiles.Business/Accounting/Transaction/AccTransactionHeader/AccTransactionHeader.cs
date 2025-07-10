using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[DisableWorkflowSettingPropertiesAfterOnSaving]
	[DescriptionProperty(AutoAccTransactionHeader.Schema.AH_Desc)]
	[CodeProperty(AutoAccTransactionHeader.Schema.AH_TransactionNum)]
	public class AccTransactionHeader : AutoAccTransactionHeader,
		IAccTransactionHeader,
		IDocManagerSupport,
		ISupportCriticalValidation,
		IHaveConstructorStackTrace,
		ICanApplyDataRefresh,
		IDataVersionLoggingSupported,
		IEInvoicingEligibilityLiteTransaction,
		IPropertyChecker
	{
		public abstract new class Schema : AutoAccTransactionHeader.Schema
		{
			public const string Index_FK_RX__AH_JH = "FK_RX__AH_JH";
		}

		public abstract class TransactionCountConstants
		{
			public const byte BankTransferFromRow = 1;
			public const byte BankTransferFromRowWhenReversing = 5;
			public const byte BankTransferToRow = 2;
			public const byte BankTransferToRowWhenReversing = 4;
			public const byte BankTransferExchangeDiff = 7;
			public const byte BankTransferExchangeDiffWhenReversing = 8;
		}

		public AccTransactionHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_InvoicePrinted), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_Ledger), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_TransactionType), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_FullyPaidDate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_OutstandingAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_IsCancelled), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_AH_InvoiceStatement), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_ReceiptBatchNo), ConcurrencyPolicy.Strict);
			this.SetConstructorStackTrace();
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged
		{
			get { return true; }
		}

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter
		{
			get { return this.GetDefaultDataVersionLogFormatter(); }
		}

		#endregion

		public sealed override void OnSaving()
		{
			OnSavingBeforeBase();
			base.OnSaving();
			OnSavingCore();
			OnSavingFinalize();
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
		}

		#region ICriticalValidation

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return fCriticalValidation ?? (fCriticalValidation = GetCriticalValidation()); }
		}

		AccTransactionHeaderCriticalValidation fCriticalValidation;

		protected virtual AccTransactionHeaderCriticalValidation GetCriticalValidation()
		{
			return new AccTransactionHeaderCriticalValidation(this);
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		#region IPropertyCheckHelper

		public bool CanUpdateTransactionNumber()
		{
			return (
					AH_Ledger == LedgerTypes.IncompleteTransactions ||    //IN
					AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||    //PA
					(ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.IncompleteTransactions ||    //IN to AP
					(AH_Ledger == LedgerTypes.UnapprovedPayableTransactions && AH_IsCancelled && AH_IsCancelledInfo.HasChanges) ||    // Reject UA
					this.HasContext(BusinessContext.PostUnapprovedCreditNoteForApproveClaim) ||     //Approve Claim
					(AH_Ledger == LedgerTypes.AccountsPayable &&
						(AH_TransactionType.Equals(TransactionTypes.Invoice) ||
						AH_TransactionType.Equals(TransactionTypes.CreditNote) ||
						AH_TransactionType.Equals(TransactionTypes.AdjustmentNote)))
				   );
		}

		public bool IsPropertyUpdatableViaXueAdditionalFields(PropertyInfo propertyInfo, object proposedValue, out string errorMessage)
		{
			errorMessage = null;
			if (propertyInfo.Name == AccTransactionHeaderSchema.Constants.AH_TransactionNum)
			{
				var result = CanUpdateTransactionNumber();
				errorMessage = result ? errorMessage
					: Res.GetString(
						"54db97e8-11a5-42ce-a3ea-ea27d50bea61",
						"Field [{0}.{1}] in {2} could not be modified because it's already saved in database.",
						GetType().Name,
						AccTransactionHeaderSchema.Constants.AH_TransactionNum,
						HumanReadableName);

				return result;
			}
			return true;
		}

		#endregion

		#region Can Apply Data Refresh

		bool ICanApplyDataRefresh.CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher)
		{
			bool result;

			var isDataRefreshGoingToUpdateLedgerOrTransactionType = publisher.IsDeleted || ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().IsStrictPropertyChangedOnSubscriberOrPublisher(publisher, AH_LedgerInfo, AH_TransactionTypeInfo);
			if (isDataRefreshGoingToUpdateLedgerOrTransactionType)
			{
				this.SetContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged);
				result = false;
			}
			else
			{
				result = ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(action, this, publisher, GetPropertiesWithStrictConcurrency());
			}

			if (!result)
			{
				UnmanageRegisteredEditableChildObjectForDataRefresh();
			}

			return result;
		}

		protected virtual void UnmanageRegisteredEditableChildObjectForDataRefresh()
		{
		}

		protected virtual ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			return new ZPropertyInfo[] {
				AH_LedgerInfo,
				AH_TransactionTypeInfo,
				AH_FullyPaidDateInfo,
				AH_OutstandingAmountInfo,
				AH_IsCancelledInfo,
				AH_AH_InvoiceStatementInfo,
				AH_ReceiptBatchNoInfo
			};
		}

		#endregion

		protected virtual void OnSavingCore()
		{
		}

		protected virtual void OnSavingFinalize()
		{
		}

		protected virtual void OnSavingBeforeBase()
		{
		}

		public bool IsAlreadyReversedByOtherUser()
		{
			var dbCommand = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM {0} WHERE {1} = @PK AND {2} = @IsCancelled", AccTransactionHeaderSchema.Constants.TableName, AccTransactionHeaderSchema.Constants.PK, AccTransactionHeaderSchema.Constants.AH_IsCancelled, 1));
			dbCommand.AddParameter("@PK", SqlDbType.UniqueIdentifier, PK.ToGuid());
			dbCommand.AddParameter("@IsCancelled", SqlDbType.Bit, true);
			var count = dbCommand.ExecuteScalar();
			return Convert.ToInt32(count) > 0;
		}

		#region IHaveConstructorStackTrace member

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }

		#endregion

		#region Business object overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Bound Lists

		OrgHeaderCollection fAH_OHList;

		public OrgHeaderCollection AH_OHList
		{
			get
			{
				if (fAH_OHList == null)
				{
					fAH_OHList = new OrganisationsFindBoxCollection(Factory);
				}
				return fAH_OHList;
			}
		}

		#endregion

		#region Calculated Properties

		public ZBool IsVietnamGovtTaxInvoice
		{
			get
			{
				return
					AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
					AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice &&
					!AH_ComplianceSubType.IsEmpty &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.VietNam;
			}
		}

		public ZBool IsIndonesianGovtComplianceInvoice
		{
			get
			{
				return
					AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
					AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice &&
					AH_GSTAmount != 0 &&
					!AH_ComplianceSubType.IsEmpty &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Indonesia;
			}
		}

		public ZBool IsGovtTaxInvoice
		{
			get { return IsVietnamGovtTaxInvoice || IsIndonesianGovtComplianceInvoice; }
		}

		#region AH_InvoiceTermDescriptionCalc

		InvoiceTermsList invoiceTermsList;

		public ZString AH_InvoiceTermDescriptionCalc
		{
			get
			{
				if (invoiceTermsList == null)
				{
					invoiceTermsList = AH_Ledger == LedgerTypes.AccountsReceivable ? new ARInvoiceTermsList() : new APInvoiceTermsList();
				}

				return invoiceTermsList.GetDescriptionFromCode(AH_InvoiceTerm);
			}
		}

		public ZPropertyInfo AH_InvoiceTermDescriptionCalcInfo
		{
			get { return GetZPropertyInfo(nameof(AH_InvoiceTermDescriptionCalc)); }
		}

		#endregion

		#region AdditionalCompanyName

		public ZString AdditionalCompanyName
		{
			get
			{
				if (AH_Ledger == LedgerTypes.AccountsPayable)
				{
					return Header?.GetAdditionalCompanyName(OrgAddressType.Payables) ?? string.Empty;
				}

				if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					return Header?.GetAdditionalCompanyName(OrgAddressType.Receivables) ?? string.Empty;
				}

				return string.Empty;
			}
		}

		#endregion

		#region JobNumber

		public ZString JobNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (!AH_JH.IsEmpty)
				{
					JobHeader job = (JobHeader)Factory.Load(JobTypeForDeterminingJobNumber, AH_JH);
					if (job != null)
					{
						result = job.JH_JobNum;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JobNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JobNumber)); }
		}

		protected virtual Type JobTypeForDeterminingJobNumber
		{
			get { return typeof(JobHeader); }
		}

		#endregion

		#region JobLocalReference

		public ZString JobLocalReference
		{
			get
			{
				ZString result = ZString.Empty;
				if (!AH_JH.IsEmpty)
				{
					JobHeader job = (JobHeader)Factory.Load(JobTypeForDeterminingJobNumber, AH_JH);
					if (job != null)
					{
						result = job.JH_JobLocalReference;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JobLocalReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(JobLocalReference)); }
		}

		#endregion

		public virtual ZBool AH_IsDisbursementCalc
		{
			get { return IsDisbursementInvoiceType(AH_TransactionCategory); }
		}

		public ZPropertyInfo AH_IsDisbursementCalcInfo
		{
			get { return GetZPropertyInfo(nameof(AH_IsDisbursementCalc)); }
		}

		public static string[] DisbursementInvoiceTypes
		{
			get
			{
				return new string[]
				{
					InvoiceTypesList.Codes.DisbursementInForeignCurrency,
					InvoiceTypesList.Codes.DisbursementInvoice,
					InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
					InvoiceTypesList.Codes.DisbursementInvoice_Batching
				};
			}
		}

		public static bool IsDisbursementInvoiceType(string invoiceType)
		{
			return Array.IndexOf(DisbursementInvoiceTypes, invoiceType) > -1;
		}

		public ZBool IsIncompleteTransaction => AH_Ledger == LedgerTypes.IncompleteTransactions &&
			(AH_TransactionType == TransactionTypes.IncompleteInvoice ||
			AH_TransactionType == TransactionTypes.IncompleteCreditNote ||
			AH_TransactionType == TransactionTypes.IncompleteAdjustmentNote);

		public ZBool IsAPTransactionConvertedFromIncompleteTransaction =>
			AH_Ledger == LedgerTypes.AccountsPayable && IsInvoiceCreditNoteOrAdjustmentNote &&
			(ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.IncompleteTransactions &&
			AccTransactionHeaderCompatibilityMatrix.IsLedgerCompatibleWithTransactionType(LedgerTypes.IncompleteTransactions, (ZString)AH_TransactionTypeInfo.OriginalValue);

		public ZBool IsAPTransactionConvertedFromUnapprovedTransaction =>
			AH_Ledger == LedgerTypes.AccountsPayable && IsInvoiceCreditNoteOrAdjustmentNote &&
			(ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.UnapprovedPayableTransactions &&
			AccTransactionHeaderCompatibilityMatrix.IsLedgerCompatibleWithTransactionType(LedgerTypes.UnapprovedPayableTransactions, (ZString)AH_TransactionTypeInfo.OriginalValue);

		public ZBool IsAPTransactionConvertedFromTransactionPendingAllocation =>
			AH_Ledger == LedgerTypes.AccountsPayable && IsInvoiceCreditNoteOrAdjustmentNote &&
			(ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.TransactionsPendingAllocation &&
			AccTransactionHeaderCompatibilityMatrix.IsLedgerCompatibleWithTransactionType(LedgerTypes.TransactionsPendingAllocation, (ZString)AH_TransactionTypeInfo.OriginalValue);

		#endregion

		#region IDocManagerSupport Members

		public virtual DocManagerInfo DocManagerInfo
		{
			get { return new DocManagerInfo(this, AH_Ledger); }
		}

		#endregion

		#region Govt Tax Invoice Display

		#region Organisation Code

		[ReadOnly(true)]
		public ZString GovtTaxInvoiceDisplay_OrganisationCode
		{
			get { return Header != null ? Header.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo GovtTaxInvoiceDisplay_OrganisationCodeInfo
		{
			get { return GetZPropertyInfo(nameof(GovtTaxInvoiceDisplay_OrganisationCode)); }
		}

		#endregion

		#region Post Date

		[ReadOnly(true)]
		public ZDateTime GovtTaxInvoiceDisplay_PostDate
		{
			get { return AH_PostDate; }
		}

		public ZPropertyInfo GovtTaxInvoiceDisplay_PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(GovtTaxInvoiceDisplay_PostDate)); }
		}

		#endregion

		#region Invoice Date

		[ReadOnly(true)]
		public ZDateTime GovtTaxInvoiceDisplay_InvoiceDate
		{
			get { return AH_InvoiceDate; }
		}

		public ZPropertyInfo GovtTaxInvoiceDisplay_InvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(GovtTaxInvoiceDisplay_InvoiceDate)); }
		}

		#endregion

		#region Transaction Number

		[ReadOnly(true)]
		public ZString GovtTaxInvoiceDisplay_TransactionNum
		{
			get { return AH_TransactionNum; }
		}

		public ZPropertyInfo GovtTaxInvoiceDisplay_TransactionNumInfo
		{
			get { return GetZPropertyInfo(nameof(GovtTaxInvoiceDisplay_TransactionNum)); }
		}

		#endregion

		#region Transaction Type

		[ReadOnly(true)]
		public ZString GovtTaxInvoiceDisplay_TransactionType
		{
			get { return AH_TransactionType; }
		}

		public ZPropertyInfo GovtTaxInvoiceDisplay_TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(GovtTaxInvoiceDisplay_TransactionType)); }
		}

		#endregion

		#endregion

		#region Properties

		[List("Lookups.Headers")]
		public override ZGuid AH_OH
		{
			get
			{
				return base.AH_OH;
			}
			set
			{
				var oldValue = AH_OH;
				base.AH_OH = value;
				if (AH_OH != oldValue)
				{
					AH_OA_InvoiceAddressOverride = ZGuid.Empty;
					AH_OC_InvoiceContactOverride = ZGuid.Empty;
				}
			}
		}

		public override ZGuid AH_GB
		{
			get { return base.AH_GB; }
			set
			{
				base.AH_GB = value;
				if (Branch != null)
				{
					AH_GC = Branch.GB_GC;
				}

				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateAH_GE();
				}

				if (this.IsInDatabase && AH_GBInfo.HasChanges && (AH_LedgerInfo.HasChanges || AH_TransactionTypeInfo.HasChanges))
				{
					var lineQuery = new ZQuery(AccTransactionLinesSchema.AL_AH, this.PK);
					var lines = Factory.Load<AccTransactionLines>(lineQuery);
					if (lines.Any(x => x.AL_GB != AH_GB))
					{
						var lineBranch = lines.FirstOrDefault(x => x.AL_GB != AH_GB)?.Branch;
						var originalBranch = Factory.Load<GlbBranch>(new ZGuid(AH_GBInfo.OriginalValue));
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(this.PK,
						CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderBranchChanged, (() => FormattableString.Invariant($@"AH_GB: Line Branch: {lineBranch.GB_Code}, Header Old Branch: {originalBranch.GB_Code}, Header New Branch:{Branch.GB_Code} , StackTrace ->\r\n {System.Environment.StackTrace}")));
					}
				}
			}
		}

		public override ZString AH_Ledger
		{
			get { return base.AH_Ledger; }
			set
			{
				ZString oldValue = base.AH_Ledger;
				base.AH_Ledger = value;

				if (oldValue != value)
				{
					invoiceTermsList = null;

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges, () =>
					{
						if (IsInDatabase && (ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.IncompleteTransactions && AH_Ledger == LedgerTypes.AccountsPayable)
						{
							return new StackTrace().ToString();
						}
						return null;
					});
				}
			}
		}

		public override ZString AH_ConsolidatedInvoiceRef
		{
			get { return base.AH_ConsolidatedInvoiceRef; }
			set
			{
				var oldValue = base.AH_ConsolidatedInvoiceRef;
				base.AH_ConsolidatedInvoiceRef = value;

				if (oldValue != value
					&& (AH_Ledger == LedgerTypes.AccountsReceivable)
					&& (AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote))
				{
					AH_JobNumber = value.Contains("/") && value != JobNumber
						? value.Substring(0, value.LastIndexOf("/"))
						: value;
				}
			}
		}

		#region Outstanding Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AH_OSOutstandingAmount
		{
			get { return base.AH_OSOutstandingAmount; }
			set
			{
				base.AH_OSOutstandingAmount = value;
			}
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal AH_OutstandingAmount
		{
			get { return base.AH_OutstandingAmount; }
			set
			{
				ZDecimal oldValue = base.AH_OutstandingAmount;

				base.AH_OutstandingAmount = value;

				if ((AH_Ledger == LedgerTypes.AccountsReceivable || AH_Ledger == LedgerTypes.AccountsPayable) &&
					AH_TransactionType == TransactionTypes.ExchangeDifference && AH_OutstandingAmount != 0)
				{
					MiscTransactionAH_OutstandingAmountStackTrace = System.Environment.StackTrace;
				}

				if (oldValue != value)
				{
					TryAddSignErrorLog();

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.AH_OutstandingAmountLastSet, () =>
					{
						var info = new ZStringBuilder();
						info.AppendLine(FormattableString.Invariant($"AH_OutstandingAmount = {AH_OutstandingAmount}, AH_OutstandingAmount Old Value = {oldValue}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					});
				}
			}
		}

		public void MakeOSOutstandingAmountApplicable(ZDecimal osOutstandingAmount)
		{
			base.AH_IsOSOutstandingAmountApplicable = true;
			AH_OSOutstandingAmount = osOutstandingAmount;
		}

		#endregion

		void TryAddSignErrorLog()
		{
			if (IsARReceiptOrAPPayment() && AH_OutstandingAmount != 0 && AH_LocalTotal != 0 && Math.Sign(AH_OutstandingAmount) != Math.Sign(AH_LocalTotal))
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoOnceWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderDifferentSignBetweenOutstandingAmountAndInvoiceAmount, () =>
				{
					return FormattableString.Invariant($@"AH_OutstandingAmount: {AH_OutstandingAmount}
AH_LocalTotal: {AH_LocalTotal}
Stack trace for sign error:
{new StackTrace()}");
				}, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			}

			bool IsARReceiptOrAPPayment()
				=> (AH_Ledger == LedgerTypes.AccountsReceivable && AH_TransactionType == TransactionTypes.Receipt)
				|| (AH_Ledger == LedgerTypes.AccountsPayable && AH_TransactionType == TransactionTypes.Payment);
		}

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AH_OSTotal
		{
			get => base.AH_OSTotal;
			set => base.AH_OSTotal = value;
		}

		#region AH_LocalTotal

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalTotal => AH_InvoiceAmount + AH_GSTAmount + AH_LocalTaxAmountOtherTaxes;
		public static ZString AH_LocalTotalSQLFormula => "(([AH_InvoiceAmount]+[AH_GSTAmount])+[AH_LocalTaxAmountOtherTaxes])";

		public ZPropertyInfo AH_LocalTotalInfo => GetZPropertyInfo(Schema.AH_LocalTotal);

		#endregion

		public override ZGuid AH_AB
		{
			get => base.AH_AB;
			set
			{
				if (base.AH_AB != value && IsInDatabase)
				{
					var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
					infoCollector.AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderBankAccountWasModifiedAfterBeingSaved,
						() => string.Format(CultureInfo.InvariantCulture, (NoResString)"Old Bank Account: {0}, New Bank Account: {1}, Transaction Header: {2}, Stack Trace:\n {3}", base.AH_AB, value, PK, System.Environment.StackTrace));
				}
				base.AH_AB = value;
			}
		}

		internal string MiscTransactionAH_OutstandingAmountStackTrace { get; private set; }

		public override ZString AH_TransactionNum
		{
			set
			{
				var oldValue = AH_TransactionNum;

				base.AH_TransactionNum = value;

				if (IsInDatabase && oldValue != AH_TransactionNum)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberChanged,
						() => new ZStringBuilder().AppendLine("AH_TransactionNum changed after being saved:").AppendLine(new StackTrace().ToString()).ToString());
				}
				else if (oldValue != ZString.Empty && AH_TransactionNum == ZString.Empty)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberSetToEmpty,
						() => new ZStringBuilder().AppendLine("AH_TransactionNum of a TransactionHeader is set as empty:").AppendLine(new StackTrace().ToString()).ToString());
				}
			}
			get
			{
				return base.AH_TransactionNum;
			}
		}

		#endregion

		#region ComplianceSubType

		[List("ComplianceSubTypeInLocalLanguageList")]
		public override ZString AH_ComplianceSubType
		{
			get
			{
				return base.AH_ComplianceSubType;
			}
			set
			{
				if (value != AH_ComplianceSubType)
				{
					base.AH_ComplianceSubType = value;
					Validation.ValidateAH_OH();
				}
			}
		}

		public ICodeDescriptionPairList ComplianceSubTypeList
			=> AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(GlbCompany.CurrentCompany.Country.Code, AH_Ledger, AH_TransactionType, GetTransactionCreatingMode());

		public ICodeDescriptionPairList ComplianceSubTypeInLocalLanguageList
			=> AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.Country.Code, AH_Ledger, AH_TransactionType, GetTransactionCreatingMode());

		protected virtual TransactionCreatingMode? GetTransactionCreatingMode() => null;

		#endregion

		#region BankCurrencyDecimals

		public ZInt BankCurrencyDecimals => BankAccount?.AccountCurrency?.Decimals ?? LocalCurrencyDecimals;

		public ZPropertyInfo BankCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(BankCurrencyDecimals)); }
		}

		#endregion

		#region DecimalOverrides

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ZUnbindableProperty()]
		public override ZDecimal AH_GSTAmount
		{
			get => base.AH_GSTAmount;
			set => base.AH_GSTAmount = value;
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal AH_InvoiceAmount
		{
			get => base.AH_InvoiceAmount;
			set
			{
				var oldValue = base.AH_InvoiceAmount;

				base.AH_InvoiceAmount = value;

				if (oldValue != value)
				{
					TryAddSignErrorLog();
				}
			}
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ZUnbindableProperty()]
		public override ZDecimal AH_WithholdingTax
		{
			get => base.AH_WithholdingTax;
			set => base.AH_WithholdingTax = value;
		}

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public override ZDecimal AH_ExchangeRate
		{
			get => base.AH_ExchangeRate;
			set => base.AH_ExchangeRate = value;
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AH_OSTaxAmountOtherTaxes
		{
			get => base.AH_OSTaxAmountOtherTaxes;
			set => base.AH_OSTaxAmountOtherTaxes = Utilities.Round(value, OSCurrencyDecimals);
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal AH_LocalTaxAmountOtherTaxes
		{
			get => base.AH_LocalTaxAmountOtherTaxes;
			set => base.AH_LocalTaxAmountOtherTaxes = Utilities.Round(value, LocalCurrencyDecimals);
		}

		#endregion

		#region DecimalPlaces

		public virtual int LocalCurrencyDecimals => Company.GetLocalDecimals();

		public virtual int OSCurrencyDecimals => TransactionCurrency != null ? TransactionCurrency.Decimals : LocalCurrencyDecimals;

		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		public int BankCurrencyDecimalsAsInt => BankCurrencyDecimals;

		#endregion

		#region IsSelfBillingInvoice

		[BusinessObjectTestExclude]
		public virtual ZBool IsSelfBillingInvoice
		{
			get
			{
				return AH_TransactionCategory == Constants.TransactionCategory.Codes.SelfBilling ||
						 AH_TransactionCategory == InvoiceTypesList.Codes.SelfBillingInvoice ||
						 AH_TransactionCategory == InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
			}
			set
			{
				IsSelfBillingInvoiceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSelfBillingInvoiceInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelfBillingInvoice)); }
		}

		#endregion

		public bool ShouldUseLoginBranchForComplianceSequence
		{
			get
			{
				var complianceDocumentNumberAllocationRule = ComplianceDocumentNumberAllocationRuleTypes.LBD.Code;
				if (!AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
				{
					if (AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						complianceDocumentNumberAllocationRule = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.Value;
					}
					else if (AH_Ledger == LedgerTypes.AccountsPayable)
					{
						complianceDocumentNumberAllocationRule = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRulePayables.Value;
					}
				}

				return complianceDocumentNumberAllocationRule == ComplianceDocumentNumberAllocationRuleTypes.LBD.Code;
			}
		}

		ZGuid BranchForComplianceSequence
			=> ShouldUseLoginBranchForComplianceSequence
				? GlbBranch.CurrentBranch.PK
				: BranchForComplianceSequenceThisTransaction;

		ZGuid BranchForComplianceSequenceThisTransaction
			=> AccountingMasterFilesUtils.IsTaxBranchApplicableForTransaction(this)
				? AH_GB_TaxBranch
				: AH_GB;

		ZGuid DepartmentForComplianceSequence
			=> ShouldUseLoginBranchForComplianceSequence
				? GlbDepartment.CurrentDepartment.PK
				: Department.PK;

		#region IEInvoicingEligibilityLiteTransaction

		ZString IEInvoicingEligibilityLiteTransaction.CountryCode => Company?.GC_RN_NKCountryCode ?? ZString.Empty;

		ZString IEInvoicingEligibilityLiteTransaction.Ledger => AH_Ledger;

		ZString IEInvoicingEligibilityLiteTransaction.TransactionType => AH_TransactionType;

		ZString IEInvoicingEligibilityLiteTransaction.ComplianceSubType => AH_ComplianceSubType;

		ZString IEInvoicingEligibilityLiteTransaction.ComplianceNumber => AH_TransactionReference;

		ZString IEInvoicingEligibilityLiteTransaction.TransactionCategory => AH_TransactionCategory;

		ZString IEInvoicingEligibilityLiteTransaction.TransactionNumber => AH_TransactionNum;

		ZString IEInvoicingEligibilityLiteTransaction.PlaceOfSupply => AH_PlaceOfSupply;

		ZBool IEInvoicingEligibilityLiteTransaction.IsCancelled => AH_IsCancelled;

		IEInvoicingEligibilityLiteOrgHeader IEInvoicingEligibilityLiteTransaction.OrgHeader
			=> (Header as IEInvoicingEligibilityLiteOrgHeader) ?? OrgHeader.EmptyIEInvoicingEligibilityLiteOrgHeader.Value;

		IEInvoicingEligibilityLiteOrgHeader IEInvoicingEligibilityLiteTransaction.BranchOrgProxy
			=> (Branch?.OrgProxy as IEInvoicingEligibilityLiteOrgHeader) ?? OrgHeader.EmptyIEInvoicingEligibilityLiteOrgHeader.Value;

		IEInvoicingEligibilityLiteOrgHeader IEInvoicingEligibilityLiteTransaction.CompanyOrgProxy
			=> (Company?.OrgProxy as IEInvoicingEligibilityLiteOrgHeader) ?? OrgHeader.EmptyIEInvoicingEligibilityLiteOrgHeader.Value;

		IEInvoicingEligibilityLiteOrgAddress IEInvoicingEligibilityLiteTransaction.InvoiceOrgAddressOverride
			=> (InvoiceAddressOverride as IEInvoicingEligibilityLiteOrgAddress) ?? OrgAddress.EmptyIEInvoicingEligibilityLiteOrgAddress.Value;

		ZGuid IEInvoicingEligibilityLiteTransaction.CompanyPK => AH_GC;

		ZGuid IEInvoicingEligibilityLiteTransaction.BranchPK => AH_GB;

		ZDateTime IEInvoicingEligibilityLiteTransaction.InvoiceDate => AH_InvoiceDate;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IEInvoicingEligibilityLiteTransaction.InvoiceAmount => AH_InvoiceAmount;

		IEInvoicingEligibilityLiteTransaction IEInvoicingEligibilityLiteTransaction.OriginalTransactionIfExists => GetOriginalTransactionIfTransactionIsReversed();

		ZString IEInvoicingEligibilityLiteTransaction.GovernmentAllocatedID => AH_GovernmentAllocatedID;

		IReadOnlyCollection<IEInvoicingEligibilityLiteTransactionLine> IEInvoicingEligibilityLiteTransaction.Lines
		{
			get
			{
				var lines = Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AH, PK));

				return lines.Length > 0 ? lines.Select(x => x as IEInvoicingEligibilityLiteTransactionLine).ToArray() : Array.Empty<IEInvoicingEligibilityLiteTransactionLine>();
			}
		}

		#endregion

		/// <summary>
		/// Gets the compliance allocation method for AR ledger, based on branch and compliance sub type.
		/// </summary>
		public ZString GetComplianceAllocationMethodAR()
			=> GetComplianceAllocationMethodARInternal(BranchForComplianceSequence);

		/// <summary>
		/// Gets the compliance allocation method for AR ledger, based on branch and compliance sub type.
		/// Ignores current branch and uses transaction branch (good for service tasks).
		/// </summary>
		public ZString GetComplianceAllocationMethodARThisTransaction()
			=> GetComplianceAllocationMethodARInternal(BranchForComplianceSequenceThisTransaction);

		ZString GetComplianceAllocationMethodARInternal(ZGuid branchPK)
		{
			var subTypeDependencies = Factory.GetCachedValue(
							nameof(AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration),
							() => AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.Value,
							CacheStalenessPolicy.StaleOnFactorySave
						);
			var parentSubTypeOrEmpty = subTypeDependencies.FindParentSubTypeInCollection(Company.GC_RN_NKCountryCode, AH_ComplianceSubType);
			var finalSubType = parentSubTypeOrEmpty.IsEmpty ? AH_ComplianceSubType : parentSubTypeOrEmpty;

			var rules = Factory.GetCachedValue(
							nameof(AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables),
							() => AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.Value,
							CacheStalenessPolicy.StaleOnFactorySave
						);
			var matchingRule = rules.GetMatchingConfiguration(branchPK, finalSubType);
			return matchingRule != null
				? matchingRule.AllocationMethod
				: (ZString)AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value;
		}

		public AccComplianceSequence ComplianceSequenceFromSubType => ComplianceSequenceRetriever.GetComplianceSequenceFromSubType(AH_ComplianceSubType,
			(Company ?? GlbCompany.CurrentCompany).PK, BranchForComplianceSequence, DepartmentForComplianceSequence, ComplianceNumberAllocationDateWithFallbackValue);

		public string ComplianceNumberAllocationDateOption
		{
			get
			{
				var validLedger = AH_Ledger == LedgerTypes.AccountsReceivable ? ComplianceNumberAllocationDateValidLedgerEnum.AR
					: AH_Ledger == LedgerTypes.AccountsPayable
						|| AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
						|| AH_Ledger == LedgerTypes.IncompleteTransactions
						|| AH_Ledger == LedgerTypes.TransactionsPendingAllocation
					? ComplianceNumberAllocationDateValidLedgerEnum.AP
					: ComplianceNumberAllocationDateValidLedgerEnum.None;

				return validLedger != ComplianceNumberAllocationDateValidLedgerEnum.None
					? AccountingMasterFilesRegistry.Instance.GetComplianceNumberAllocationDateRegistryValue(validLedger, AH_GC)
					: ComplianceNumberAllocationDateOptions.NoControl.Code;
			}
		}

		public ZDateTime ComplianceNumberAllocationDate
		{
			get
			{
				var complianceNumberAllocationDateOption = ComplianceNumberAllocationDateOption;

				return complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? AH_InvoiceDate
					: complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.PostDate.Code ? AH_PostDate
					: ZDateTime.Empty;
			}
		}

		public ZDateTime ComplianceNumberAllocationDateWithFallbackValue
		{
			get
			{
				var complianceNumberAllocationDate = ComplianceNumberAllocationDate;
				return complianceNumberAllocationDate.IsValid ? complianceNumberAllocationDate : ZDateTime.Now;
			}
		}

		public bool IsComplianceNumberAllocationMandatory => ComplianceNumberAllocationDateOption != ComplianceNumberAllocationDateOptions.NoControl.Code;

		public SchemaDateTimeColumn ComplianceNumberAllocationDateColumn
		{
			get
			{
				var complianceNumberAllocationDateOption = ComplianceNumberAllocationDateOption;

				return complianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? AccTransactionHeaderSchema.AH_InvoiceDate
					: complianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code ? AccTransactionHeaderSchema.AH_PostDate
					: null;
			}
		}

		public ZDateTime GetLastDateUsedInComplianceBook(AccComplianceSequence sequence)
		{
			var transactionsUsedInBook = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_XD_ComplianceBook, sequence.PK)
				.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK));

			if (transactionsUsedInBook != null)
			{
				foreach (var transaction in transactionsUsedInBook.OrderByDescending(t => t.AH_TransactionReference))
				{
					if (transaction.ComplianceNumberAllocationDate.IsValid)
					{
						return transaction.ComplianceNumberAllocationDate;
					}
				}
			}

			return ZDateTime.Empty;
		}

		public bool IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(AccComplianceSequence sequence)
		{
			var allocationDate = ComplianceNumberAllocationDate;
			var lastDateUsedInComplianceBook = GetLastDateUsedInComplianceBook(sequence);
			return allocationDate.IsValid && lastDateUsedInComplianceBook.IsValid && allocationDate.Date < lastDateUsedInComplianceBook.Date;
		}

		public AccComplianceSequence ComplianceSequenceFromTransactionReference
		{
			get { return ComplianceSequenceRetriever.GetComplianceSequenceFromTransactionReference(AH_ComplianceSubType, AH_TransactionReference, AH_GC, Factory); }
		}

		public AccTransactionHeader GetOriginalTransactionIfTransactionIsReversed()
		{
			if (!AH_TransactionBelongsToGroup.IsEmpty)
			{
				var originalTransaction = Factory.Load<AccTransactionHeader>(AH_TransactionBelongsToGroup);

				return originalTransaction;
			}

			return null;
		}

		public AccComplianceSequence ComplianceSequence
		{
			get
			{
				if (!AH_XD_ComplianceBook.IsEmpty)
				{
					return ComplianceBook;
				}
				else
				{
					return AH_TransactionReference.IsEmpty ? ComplianceSequenceFromSubType : ComplianceSequenceFromTransactionReference;
				}
			}
		}

		IComplianceSequenceRetriever ComplianceSequenceRetriever
		{
			get
			{
				return complianceSequenceRetriever_internal ?? (complianceSequenceRetriever_internal = new ComplianceSequenceRetriever());
			}
		}
		IComplianceSequenceRetriever complianceSequenceRetriever_internal;

#if DEBUG
		public void SubstituteComplianceSequenceRetriever_ForTestOnly(IComplianceSequenceRetriever replacement)
		{
			complianceSequenceRetriever_internal = replacement;
		}

		public IComplianceSequenceRetriever ComplianceSequenceRetriever_ExposedForTestOnly => ComplianceSequenceRetriever;
#endif

		public ZGuid ComplianceSequencePrinterPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				AccComplianceSequence sequence;
				if (Branch.Country.SupportComplianceSubType && GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.China)
				{
					sequence = ComplianceSequence;
					if (sequence != null && !sequence.XD_SQ_DocumentPrintQueue.IsEmpty)
					{
						result = sequence.XD_SQ_DocumentPrintQueue;
					}
				}
				return result;
			}
		}

		#region New Properties

		public void PrepareClassAInvoiceForEditing()
		{
			ClassAInvoiceIsEditing = true;
		}

		bool ClassAInvoiceIsEditing;

		protected virtual bool AH_PostDate_ReadOnly
		{
			get { return ClassAInvoiceIsEditing; }
		}

		protected virtual bool AH_InvoiceDate_ReadOnly
		{
			get { return ClassAInvoiceIsEditing; }
		}

		protected virtual bool AH_TransactionNum_ReadOnly
		{
			get { return ClassAInvoiceIsEditing; }
		}

		protected virtual bool AH_TransactionType_ReadOnly
		{
			get { return ClassAInvoiceIsEditing; }
		}

		public ZString TransactionNumberPrefixed
		{
			get { return InvoiceTransactionNumberPrefix + AH_TransactionNum; }
		}

		ZString InvoiceTransactionNumberPrefix
		{
			get
			{
				ZString result = ZString.Empty;
				switch (AH_Ledger)
				{
					case LedgerTypes.AccountsPayable:
						if (IsInvoiceCreditNoteAdjustmentNoteOrBatch && AH_TransactionCategory == Core.Constants.TransactionCategory.Codes.SelfBilling)
						{
							result = ObjectFactory.Get<IAccounting>().SelfBillingInvoiceTransactionNumberPrefix(AH_GC.ToGuid());
						}
						break;
					case LedgerTypes.AccountsReceivable:
						if (IsInvoiceCreditNoteAdjustmentNoteOrBatch)
						{
							result = ObjectFactory.Get<IAccounting>().InvoiceTransactionNumberPrefix(AH_GC.ToGuid());
						}
						break;
				}

				return result;
			}
		}

		bool IsInvoiceCreditNoteOrAdjustmentNote =>
			AH_TransactionType == TransactionTypes.Invoice ||
			AH_TransactionType == TransactionTypes.CreditNote ||
			AH_TransactionType == TransactionTypes.AdjustmentNote;

		bool IsInvoiceCreditNoteAdjustmentNoteOrBatch => IsInvoiceCreditNoteOrAdjustmentNote ||
			AH_TransactionType == TransactionTypes.InvoiceBatch;

		public bool IsAPInvoice => AH_Ledger == LedgerTypes.AccountsPayable && AH_TransactionType == TransactionTypes.Invoice;

		public bool IsARCreditNote => AH_Ledger == LedgerTypes.AccountsReceivable && AH_TransactionType == TransactionTypes.CreditNote;

		public ZString HeaderFullName
		{
			get { return Header != null ? Header.OH_FullNameTruncated : ZString.Empty; }
		}

		public virtual ZString RelatedTransactionDebtorsAsString
		{
			get { return ZString.Empty; }
		}

		public ZString CheckRequesterUserFullName
		{
			get
			{
				return Creator != null ? Creator.GS_FullName : ZString.Empty;
			}
		}

		public GlbStaff Creator
		{
			get
			{
				return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, AH_SystemCreateUser));
			}
		}

		public bool IsCreatedByENett
		{
			get { return this.HasContext(BusinessContext.ImportingENettTransaction); }
			set
			{
				if (value)
				{
					this.SetContext(BusinessContext.ImportingENettTransaction);
				}
				else
				{
					this.RemoveContext(BusinessContext.ImportingENettTransaction);
				}
			}
		}

		protected virtual bool AH_ChequeOrReference_ReadOnly { get; set; }

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AH_GC.IsEmpty)
			{
				AH_GC = GlbCompany.CurrentCompany.PK; //do this before base call to avoid creating new GlbCompany
			}
			if (AH_GB.IsEmpty)
			{
				AH_GB = GlbBranch.CurrentBranch.PK;
			}
			if (AH_Ledger.IsEmpty)
			{
				// AccTransactionHeader is not valid with empty AH_Ledger.
				AH_Ledger = LedgerTypes.AccountsReceivable;
			}
			if (AH_TransactionType.IsEmpty)
			{
				// AccTransactionHeader is not valid with empty AH_TransactionType.
				AH_TransactionType = TransactionTypes.Journal;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
