using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.AccChargeGLPostingOverrideLookups;

#if DEBUG
using Enterprise.MasterFiles.Business.Testing;
#endif

namespace Enterprise.MasterFiles.Business
{
	[DisableWorkflowSettingPropertiesAfterOnSaving]
	public class AccTransactionLines : AutoAccTransactionLines, IAccTransactionLines, ISupportCriticalValidation, IHaveConstructorStackTrace, IEInvoicingEligibilityLiteTransactionLine
	{
		public new abstract class Schema : AutoAccTransactionLines.Schema
		{
			public const string JobNumber = "JobNumber";
		}

		public AccTransactionLines(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_RevRecognitionType), ConcurrencyPolicy.Strict);
			this.SetConstructorStackTrace();
		}

		internal static bool IsRevenueLine(AccTransactionLines line)
		{
			return line != null && IsRevenueLine(line.AL_LineType);
		}

		internal static bool IsRevenueLine(ZString type)
		{
			return RevenueLineTypes.Contains(type);
		}

		internal static bool IsCostLine(AccTransactionLines line)
		{
			return line != null && (CostLineTypes.Contains(line.AL_LineType));
		}

		public static List<string> RevenueLineTypes
		{
			get
			{
				return new List<string>() { TransactionLineTypes.Revenue };
			}
		}

		public static List<string> CostLineTypes
		{
			get
			{
				return new List<string>() { TransactionLineTypes.Cost, TransactionLineTypes.UnapprovedCost };
			}
		}

		#region IHaveConstructorStackTrace member

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }

		#endregion

		#region Override

		public override ZDateTime AL_ReverseDate
		{
			get
			{
				return base.AL_ReverseDate;
			}
			set
			{
				bool shouldSave = true;
				if (IsInDatabase)
				{
					if (AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP || AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual ||
						AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue ||
						(AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost &&
						TransactionHeader != null && TransactionHeader.AH_LedgerInfo.OriginalValue.ToString() != LedgerTypes.UnapprovedPayableTransactions))
					{
						if (base.AL_ReverseDate.IsValid && AL_ReverseDateInfo.OriginalValue.IsValid)
						{
							if (((ZDateTime)AL_ReverseDateInfo.OriginalValue).Date != value.Date)
							{
								if (!this.HasContext(BusinessContext.WipAccrualReversing))
								{
									string message = ResetReverseDateErrorMessageTrap(value);
									DeveloperExceptionsToBeSentAfterSavingService.QueueReport(this, ResetReverseDateErrorKey, message, new Exception(message));
									shouldSave = false;
								}
							}
						}
					}
				}

				if (shouldSave)
				{
					base.AL_ReverseDate = value;
					ReportReversedJobRelatedLineErrorMessage();
				}
			}
		}

		#region ReversedJobRelatedLineError

		void ReportReversedJobRelatedLineErrorMessage()
		{
#if DEBUG
			if (Globals.IsTest && SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.IsActive)
			{
				return;
			}
#endif
			if ((AL_LineType == TransactionLineTypes.WIP || AL_LineType == TransactionLineTypes.Accrual)
				&& !SetReverseDateBeforeUnlinkChargeErrorSuspender.IsSuspended && base.AL_ReverseDate.IsValid)
			{
				var relatedJobCharge = this.LoadRelatedJobCharge();
				if (relatedJobCharge != null)
				{
					string message = Res.GetString("511ec8db-0395-45d1-bec7-b4837ff7bc31", "A reversed ACR/WIP has an associated job charge. other details : {0}\r\nRelated Job Charge details : {1}",
					this.GetTransactionLineInfo(),
					relatedJobCharge.GetJobChargeInfo());

					ErrorReporter.ReportOnce("ReversedWIPACRAssociatedToJobCharge", message);
				}
			}
		}

		#endregion

		public FunctionalitySuspender SetReverseDateBeforeUnlinkChargeErrorSuspender
		{
			get { return setReverseDateBeforeUnlinkChargeErrorSuspender ?? (setReverseDateBeforeUnlinkChargeErrorSuspender = new FunctionalitySuspender(ReportReversedJobRelatedLineErrorMessage)); }
		}
		FunctionalitySuspender setReverseDateBeforeUnlinkChargeErrorSuspender;

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AL_LineAmount
		{
			get { return base.AL_LineAmount; }
			set
			{
				var oldValue = AL_LineAmount;
				base.AL_LineAmount = RoundAmountToLocalDecimals(value);
				CollectDeveloperInfoInAL_LineAmountSetter(oldValue);
			}
		}

		void CollectDeveloperInfoInAL_LineAmountSetter(ZDecimal oldValue)
		{
			if (AL_LineAmount != oldValue)
			{
				Func<string> createMessage = () =>
				{
					var info = new ZStringBuilder();
					info.AppendLine(FormattableString.Invariant($"AL_LineAmount = {AL_LineAmount}, AL_LineAmount Old Value = {oldValue}"));
					info.AppendLine(new StackTrace().ToString());
					return info.ToString();
				};

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenPostingReceivableCharges, () =>
				{
					if (Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel))
					{
						return createMessage();
					}

					return null;
				});

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenImportInvoice, () =>
				{
					if (AL_LineType == TransactionLineTypes.Cost && Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting))
					{
						return createMessage();
					}

					return null;
				});

				Action<ZStringBuilder> addNewValueAndOldValueInfo = (infoBuilder) =>
				{
					infoBuilder.Append(FormattableString.Invariant($"AL_LineAmount = {AL_LineAmount}, AL_LineAmount Old Value = {oldValue}, AL_OSAmount = {AL_OSAmount}, AL_GSTVAT = {AL_GSTVAT}"));
				};
				CollectStackTraceWithOSAmountIncorrect(addNewValueAndOldValueInfo);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge, () =>
				{
					var relatedJobCharge = this.LoadRelatedJobCharge(useLocalCacheOnly: true);

					ZDecimal chargeInfoLocalAmount = 0;
					if (relatedJobCharge != null)
					{
						chargeInfoLocalAmount = new JobChargeCriticalValidation.LineRelatedChargeInfo(relatedJobCharge.JR_AL_ARLineInfo).LocalAmount;
					}

					if (relatedJobCharge != null
					&& relatedJobCharge.IsRevenuePosted
					&& AL_LineAmount != chargeInfoLocalAmount)
					{
						return FormattableString.Invariant(
	$@"{nameof(AL_LineAmount)} has been changed from {oldValue} to {AL_LineAmount} after Job charge creation.
JobCharge LocalAmount: {chargeInfoLocalAmount}

StackTrace:
{System.Environment.StackTrace}");
					}

					return null;
				});
			}
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal AL_OSAmount
		{
			get { return base.AL_OSAmount; }
			set
			{
				var oldValue = AL_OSAmount;
				base.AL_OSAmount = value;
				CollectDeveloperInfoInAL_OSAmountSetter(oldValue);
			}
		}

		void CollectDeveloperInfoInAL_OSAmountSetter(ZDecimal oldValue)
		{
			if (AL_OSAmount != oldValue)
			{
				Action<ZStringBuilder> addNewValueAndOldValueInfo = (infoBuilder) =>
				{
					infoBuilder.Append(FormattableString.Invariant($"AL_OSAmount = {AL_OSAmount}, AL_OSAmount Old Value = {oldValue}, AL_LineAmount = {AL_LineAmount}, AL_GSTVAT = {AL_GSTVAT}"));
				};
				CollectStackTraceWithOSAmountIncorrect(addNewValueAndOldValueInfo);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge, () =>
				{
					var relatedJobCharge = this.LoadRelatedJobCharge(useLocalCacheOnly: true);

					ZDecimal chargeInfoOSAmount = 0;
					ZString chargeInfoCurrency = default;

					if (relatedJobCharge != null)
					{
						var lineRelatedChargeInfo = new JobChargeCriticalValidation.LineRelatedChargeInfo(relatedJobCharge.JR_AL_ARLineInfo);
						chargeInfoOSAmount = lineRelatedChargeInfo.OSAmount;
						chargeInfoCurrency = lineRelatedChargeInfo.CurrencyCode;
					}

					if (relatedJobCharge != null
					&& relatedJobCharge.IsRevenuePosted
					&& AL_RX_NKTransactionCurrency == chargeInfoCurrency
					&& AL_OSAmount != chargeInfoOSAmount)
					{
						return FormattableString.Invariant(
	$@"{nameof(AL_OSAmount)} has been changed from {oldValue} to {AL_OSAmount} after Job charge creation.
JobCharge OSAmount: {chargeInfoOSAmount}

StackTrace:
{System.Environment.StackTrace}");
					}

					return null;
				});
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AL_GSTVAT
		{
			get { return base.AL_GSTVAT; }
			set
			{
				var oldValue = AL_GSTVAT;
				base.AL_GSTVAT = RoundAmountToLocalDecimals(value);

				if (AL_GSTVAT != oldValue)
				{
					Action<ZStringBuilder> addNewValueAndOldValueInfo = (infoBuilder) =>
					{
						infoBuilder.Append(FormattableString.Invariant($"AL_GSTVAT = {AL_GSTVAT}, AL_GSTVAT Old Value = {oldValue}, AL_LineAmount = {AL_LineAmount}, AL_OSAmount = {AL_OSAmount}"));
					};
					CollectStackTraceWithOSAmountIncorrect(addNewValueAndOldValueInfo);
				}
			}
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal AL_GSTVATExtra
		{
			get => base.AL_GSTVATExtra;
			set => base.AL_GSTVATExtra = value;
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AL_WithholdingTax
		{
			get { return base.AL_WithholdingTax; }
			set
			{
				base.AL_WithholdingTax = RoundAmountToLocalDecimals(value);
			}
		}

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal AL_ExchangeRate
		{
			get => base.AL_ExchangeRate;
			set
			{
				var oldValue = AL_ExchangeRate;
				base.AL_ExchangeRate = value;

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineExChangeRateChangeWhenPostingReceivableCharges, () =>
				{
					if (AL_ExchangeRate != oldValue && Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel))
					{
						var info = new ZStringBuilder();
						info.AppendLine(System.FormattableString.Invariant($"AL_ExchangeRate = {AL_ExchangeRate}, AL_ExchangeRate Old Value = {oldValue}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					}

					return null;
				});

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineExRateShouldBeOneWhenLocalCompanyCurrencyEqualsLineCurrency, () =>
				{
					if ((!IsInDatabase || AL_RX_NKTransactionCurrencyInfo.HasChanges || AL_ExchangeRateInfo.HasChanges)
						&& AL_ExchangeRate != 0 && Company.GC_RX_NKLocalCurrency == AL_RX_NKTransactionCurrency && AL_ExchangeRate != 1m)
					{
						var info = new ZStringBuilder();
						info.AppendLine(System.FormattableString.Invariant($"AL_ExchangeRate = {AL_ExchangeRate}, AL_ExchangeRate Old Value = {oldValue}, AL_RX_NKTransactionCurrency = {AL_RX_NKTransactionCurrency}, AH_RX_NKTransactionCurrency = {TransactionHeader.AH_RX_NKTransactionCurrency}, AH_PostedToEFT = {TransactionHeader.AH_PostedToEFT}, AH_ExchangeRate = {TransactionHeader.AH_ExchangeRate}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					}

					return null;
				});
			}
		}

		public override ZString AL_RX_NKTransactionCurrency
		{
			get { return base.AL_RX_NKTransactionCurrency; }
			set
			{
				if (AL_RX_NKTransactionCurrency != value)
				{
					var oldValue = AL_RX_NKTransactionCurrency;
					base.AL_RX_NKTransactionCurrency = value;

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineCurrencyChangeWhenPostingReceivableCharges, () =>
					{
						if (Factory.HasContext(Enterprise.Integration.Accounting.BusinessContext.PostingReceivableChargesForFactoryLevel))
						{
							var info = new ZStringBuilder();
							info.AppendLine(System.FormattableString.Invariant($"AL_RX_NKTransactionCurrency = {AL_RX_NKTransactionCurrency}, AL_RX_NKTransactionCurrency Old Value = {oldValue}"));
							info.AppendLine(new StackTrace().ToString());
							return info.ToString();
						}

						return null;
					});
				}
			}
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal AL_OSUnitPrice
		{
			get => base.AL_OSUnitPrice;
			set => base.AL_OSUnitPrice = value;
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AL_UnitPrice
		{
			get => base.AL_UnitPrice;
			set => base.AL_UnitPrice = value;
		}

		bool FactoryWithValidContext
		{
			get
			{
				return !(Factory.HasContext(BusinessContext.IncompleteInvoiceSaving) && Factory.HasContext(BusinessContext.PreviewInvoice));
			}
		}

		public sealed override void OnSaving()
		{
			base.OnSaving();
			OnSavingCore();

			DeveloperExceptionsToBeSentAfterSavingService.RegisterObjectIsSaving(Factory, PK);
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
		}

		public virtual void OnSavingCore()
		{
		}

		protected sealed override void OnFactorySavingBeforeTransactionCore()
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySavingBeforeTransactionCore();

				OnFactorySavingBeforeTransactionCore2();
			}
		}

		protected virtual void OnFactorySavingBeforeTransactionCore2()
		{
		}

		protected sealed override void OnFactorySaving()
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaving();

				OnFactorySavingCore();
			}
		}

		protected virtual void OnFactorySavingCore()
		{
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaved(saveSucceeded);

				OnFactorySavedCore(saveSucceeded);
			}
		}

		protected virtual void OnFactorySavedCore(bool saveSucceeded)
		{
		}

		public override ZGuid AL_GB
		{
			get
			{
				return base.AL_GB;
			}
			set
			{
				base.AL_GB = value;

				if (Branch != null)
				{
					this.AL_GC = Branch.GB_GC;
				}

				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateAL_GE();
				}
			}
		}

		public override ZGuid AL_GE
		{
			get { return base.AL_GE; }
			set
			{
				base.AL_GE = value;

				DefaultGLAccounts();
			}
		}

		public override ZGuid AL_AC
		{
			get { return base.AL_AC; }
			set
			{
				base.AL_AC = value;

				DefaultGLAccounts();
			}
		}

		public override ZGuid AL_OH
		{
			get { return base.AL_OH; }
			set
			{
				var oldValue = AL_OH;
				base.AL_OH = value;

				if (AL_OH != oldValue)
				{
					CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(this, oldValue);
				}

				DefaultGLAccounts();
			}
		}

		public override ZGuid AL_JH
		{
			get
			{
				return base.AL_JH;
			}
			set
			{
				base.AL_JH = value;

				DefaultGLAccounts();
			}
		}

		protected virtual void DefaultGLAccounts()
		{
			if (ChargeCode != null
				&& (AL_LineType == TransactionLineTypes.Accrual
					|| AL_LineType == TransactionLineTypes.Cost
					|| AL_LineType == TransactionLineTypes.UnapprovedCost
					|| AL_LineType == TransactionLineTypes.Revenue
					|| AL_LineType == TransactionLineTypes.WIP)
				&& ChargeCode.AC_ChargeType != Core.Constants.ChargeType.Comment)
			{
				var glPostingAccounts = GetDefaultGLPostingAccounts();
				var glAccount = GetGLAccountPKFromChargeCodeAccounts(glPostingAccounts);

				if (!glAccount.IsEmpty)
				{
					AL_AG = glAccount;
				}
			}

			Validation.ValidateAL_AG();
		}

		public virtual AccChargeCode.GLPostingAccounts GetDefaultGLPostingAccounts()
		{
			var jobType = JobTypeAdditionalCodes.All;
			var direction = Constants.FreightShipmentDirection.Code.All;
			var transportMode = TransportModeAdditionalCodes.All;
			var consolContainerMode = ConsolContainerModeAdditionalCodes.All;
			var matserPaymentType = MasterPaymentTypeAdditionalCodes.All;
			var housePaymentType = HousePaymentTypeAdditionalCodes.All;

			return ChargeCode?.GetGLPostingAccounts(Department, TransactionHeader?.Header ?? this.Header, jobType, direction, transportMode, consolContainerMode, matserPaymentType, housePaymentType) ?? default(AccChargeCode.GLPostingAccounts);
		}

		protected virtual ZGuid GetGLAccountPKFromChargeCodeAccounts(AccChargeCode.GLPostingAccounts glPostingAccounts)
		{
			var glAccountPK = ZGuid.Empty;
			switch (AL_LineType)
			{
				case TransactionLineTypes.Accrual:
					glAccountPK = glPostingAccounts.AccrualAccount;
					break;
				case TransactionLineTypes.Cost:
				case TransactionLineTypes.UnapprovedCost:
					glAccountPK = glPostingAccounts.CostAccount;
					break;
				case TransactionLineTypes.Revenue:
					glAccountPK = glPostingAccounts.RevenueAccount;
					break;
				case TransactionLineTypes.WIP:
					glAccountPK = glPostingAccounts.WIPAccount;
					break;
			}

			return glAccountPK;
		}

		public override RefCurrency TransactionCurrency
		{
			get
			{
				if (AL_RX_NKTransactionCurrency.IsEmpty)
				{
					return null;
				}

				if (transactionCurrency_cached == null || transactionCurrency_cached.IsDeleted || transactionCurrency_cached.RX_Code != AL_RX_NKTransactionCurrency)
				{
					transactionCurrency_cached = base.TransactionCurrency;
				}

				return transactionCurrency_cached;
			}
		}
		RefCurrency transactionCurrency_cached;

		public override ZGuid AL_AT
		{
			get => base.AL_AT;
			set
			{
				var oldRate = AL_TaxRateCalc;
				var oldExtraRate = AL_TaxExtraRateCalc;
				var oldExtraTaxRateType = TaxRate?.AT_ExtraTaxRateType ?? ZString.Empty;

				var hasChanged = base.AL_AT != value;
				base.AL_AT = value;

				if (hasChanged)
				{
					if (!AL_AT.IsValid)
					{
						SetBaseAL_TaxDate(ZDate.Empty);
					}
					else if (AL_TaxDate.IsEmpty)
					{
						SetBaseAL_TaxDate(ZDate.Today);
					}

					SetRates(oldRate, oldExtraRate, oldExtraTaxRateType);
				}

				Validation.ValidateAL_A9_VATClass();

				if (hasChanged && IsInDatabase && ShouldRecordChangingStackTraceAfterBeingPosted)
				{
					CriticalValidationInfoCollectorService
						.GetOrCreateService(Factory)
						.AddLastInfoWhenAllowed(PK
							, CriticalValidationInfoCollectorServiceKeyType.TransactionLineHasChangedAfterItIsPosted_TaxId
							, () => System.Environment.StackTrace
							, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE
						);
				}
			}
		}

		public override ZGuid AL_A9_VATClass
		{
			get => base.AL_A9_VATClass;
			set
			{
				var hasChanged = base.AL_A9_VATClass != value;
				base.AL_A9_VATClass = value;

				if (hasChanged && IsInDatabase && ShouldRecordChangingStackTraceAfterBeingPosted)
				{
					CriticalValidationInfoCollectorService
						.GetOrCreateService(Factory)
						.AddLastInfoWhenAllowed(PK
							, CriticalValidationInfoCollectorServiceKeyType.TransactionLineHasChangedAfterItIsPosted_TaxMessage
							, () => System.Environment.StackTrace
							, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE
						);
				}
			}
		}

		bool ShouldRecordChangingStackTraceAfterBeingPosted
			=> AL_LineType == TransactionLineTypes.Cost
			&& (string)TransactionHeader?.AH_Ledger == LedgerTypes.AccountsPayable
			&& (string)TransactionHeader?.AH_TransactionType == TransactionTypes.Invoice;

		protected virtual void SetBaseAL_TaxDate(ZDate value) => base.AL_TaxDate = value;

		public void SetTaxDateSafe(ZDate value) => AL_TaxDate = (AL_AT.IsValid ? (value.IsValid ? value : ZDate.Today) : ZDate.Empty);

		public override ZDate AL_TaxDate
		{
			get => base.AL_TaxDate;
			set
			{
				var oldRate = AL_TaxRateCalc;
				var oldExtraRate = AL_TaxExtraRateCalc;
				var oldExtraTaxRateType = TaxRate?.AT_ExtraTaxRateType ?? ZString.Empty;

				var hasChanged = base.AL_TaxDate != value;
				base.AL_TaxDate = value;

				if (hasChanged)
				{
					SetRates(oldRate, oldExtraRate, oldExtraTaxRateType);
				}
			}
		}

		protected bool AL_TaxDate_ReadOnly => !AL_AT.IsValid;

		void SetRates(ZDecimal oldRate, ZDecimal oldExtraRate, ZString oldExtraTaxRateType)
		{
			using (OnRateChangedSuspender.GetSuspender())
			{
				if (!AL_AT.IsValid || !AL_TaxDate.IsValid || TaxRate == null)
				{
					AL_TaxRateNumerator = 0;
					AL_TaxRateDenominator = 1;
					AL_TaxExtraRateNumerator = 0;
					AL_TaxExtraRateDenominator = 1;
				}
				else
				{
					var rate = TaxRate.GetRateComponents(AL_TaxDate);
					var extraRate = TaxRate.GetExtraRateComponents(AL_TaxDate);
					AL_TaxRateNumerator = rate.numerator;
					AL_TaxRateDenominator = rate.denominator;
					AL_TaxExtraRateNumerator = extraRate.numerator;
					AL_TaxExtraRateDenominator = extraRate.denominator;
				}

				if (oldRate != AL_TaxRateCalc || oldExtraRate != AL_TaxExtraRateCalc || TaxRate != null && oldExtraTaxRateType != TaxRate.AT_ExtraTaxRateType)
				{
					OnRateChanged();
				}
			}
		}

		public FunctionalitySuspender OnRateChangedSuspender => onRateChangedSuspender ?? (onRateChangedSuspender = new FunctionalitySuspender(OnRateChanged, doActionOnlyIfRequestedWhenSuspended: true));
		FunctionalitySuspender onRateChangedSuspender;

		void OnRateChanged()
		{
			if (!OnRateChangedSuspender.IsSuspended)
			{
				OnRateChangedCore();
			}
		}

		protected virtual void OnRateChangedCore()
		{
		}

		[ReadOnly(true)]
		public override ZInt AL_TaxRateNumerator
		{
			get => base.AL_TaxRateNumerator;
			set
			{
				if (base.AL_TaxRateNumerator != value)
				{
					base.AL_TaxRateNumerator = value;
					OnRateChanged();
				}
			}
		}

		[ReadOnly(true)]
		public override ZInt AL_TaxRateDenominator
		{
			get => base.AL_TaxRateDenominator;
			set
			{
				if (base.AL_TaxRateDenominator != value)
				{
					base.AL_TaxRateDenominator = value;
					OnRateChanged();
				}
			}
		}

		[ReadOnly(true)]
		public override ZInt AL_TaxExtraRateNumerator
		{
			get => base.AL_TaxExtraRateNumerator;
			set
			{
				if (base.AL_TaxExtraRateNumerator != value)
				{
					base.AL_TaxExtraRateNumerator = value;
					OnRateChanged();
				}
			}
		}

		[ReadOnly(true)]
		public override ZInt AL_TaxExtraRateDenominator
		{
			get => base.AL_TaxExtraRateDenominator;
			set
			{
				if (AL_TaxExtraRateDenominator != value)
				{
					base.AL_TaxExtraRateDenominator = value;
					OnRateChanged();
				}
			}
		}

		public override ZGuid AL_AG
		{
			get
			{
				return base.AL_AG;
			}
			set
			{
				base.AL_AG = value;

				SetAlternateGLAccountDissections();
			}
		}

		#endregion

		#region Calculated Properties

		public ZString JobNumber
		{
			get { return Job != null ? Job.JH_JobNum : ZString.Empty; }
		}

		public ZPropertyInfo JobNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JobNumber)); }
		}

		public ZString JobLocalReference
		{
			get { return Job != null ? Job.JH_JobLocalReference : ZString.Empty; }
		}

		public ZPropertyInfo JobLocalReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(JobLocalReference)); }
		}

		public virtual int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public virtual int CurrencyDecimals => this.TransactionCurrency?.Decimals ?? LocalDecimals;

		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		public int TaxRateDecimals => TaxRate != null ? TaxRate.TaxRateDecimals : 2;

		[DecimalPlaces(nameof(TaxRateDecimals))]
		public ZDecimal AL_TaxRateCalc => TaxRate != null ? AccTaxRate.GetRate(TaxRate.AT_Type, AL_TaxRateCalc_Raw) : 0;

		[DecimalPlaces(nameof(TaxRateDecimals))]
		public ZDecimal AL_TaxRateCalc_Raw => AccTaxRate.GetRateRaw(this);

		[DecimalPlaces(nameof(TaxRateDecimals))]
		public ZDecimal AL_TaxExtraRateCalc => AccTaxRate.GetExtraRate(this);

		public ZDecimal GetEffectiveExtraRate() => TaxRate != null ? AccTaxRate.GetEffectiveExtraRate(TaxRate.AT_Type, AL_TaxRateCalc, TaxRate.AT_ExtraTaxRateType, AL_TaxExtraRateCalc) : 0;

		public ZString MultiSubAccountTypeCode { get; set; }

		#region Tax Expense

		public bool IsTaxExpense => AL_TotalTaxExpenseAmount != 0m;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_TotalTaxExpenseAmount => GetTaxExpenses().Sum(x => x.TaxExpenseAmount);

		public (ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)[] GetTaxExpenses()
		{
			(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)[] taxExpenses;
			if (IsInDatabase)
			{
				taxExpenses = ((ITaxProcessorCrossAssembly)ObjectFactory.Get("ITaxProcessor")).GetTaxExpenses(Factory, PK);
			}
			else
			{
				taxExpenses = Array.Empty<(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)>();
			}
			return taxExpenses;
		}

		#endregion

		#endregion

		#region ReverseDateError

		protected ZString ResetReverseDateErrorKey => "AttemptToResetReverseDateWhenNotExpected_2";

		protected internal ZString ResetReverseDateErrorMessageTrap(ZDateTime newValue)
		{
			JobCharge relatedJobCharge = this.LoadRelatedJobCharge();
			var info = string.Format(Culture.Invariant, (NoResString)"{0} has Reverse Date {1} changed to {2}. {0} has the {3} type and PK: {4}.\r\n{0} other details: {5}\r\n",
					AL_LineType,
					AL_ReverseDateInfo.OriginalValue,
					newValue,
					this.GetType(),
					this.PK,
					this.GetTransactionLineInfo());

			if (relatedJobCharge != null)
			{
				info += string.Format(Culture.Invariant, (NoResString)"\r\n\r\nCharge extra details:\r\nInvoice Type: {0}", relatedJobCharge.JR_InvoiceType);
				if (relatedJobCharge.Job?.Parent is IJobInvoicingPlugIn)
				{
					info += string.Format(Culture.Invariant, (NoResString)"\r\nPlugin Type: {0}\r\nPlugin Consumer Type: {1}",
						relatedJobCharge.Job.Parent.GetType().Name,
						((IJobInvoicingPlugIn)relatedJobCharge.Job.Parent).InvoicingSupporter?.ConsumerType?.Code);
				}
				info += string.Format(Culture.Invariant, (NoResString)"\r\n\r\nRelated Job Charge details: {0}", relatedJobCharge.GetJobChargeInfo());
			}
			info += string.Format(Culture.Invariant, (NoResString)"\r\n\r\nStack Trace: {0}", System.Environment.StackTrace);
			return info;
		}

#if DEBUG
		public ZString ResetReverseDateErrorMessageTrap_ForTestOnly(ZDateTime newValue)
		{
			return ResetReverseDateErrorMessageTrap(newValue);
		}
#endif

		#endregion

		#region ISupportCriticalValidation Members

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return GetCriticalValidation(); }
		}

		protected virtual ICriticalValidation GetCriticalValidation()
		{
			return new AccTransactionLinesCriticalValidation(this);
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		protected virtual ZDecimal RoundAmountToLocalDecimals(ZDecimal value)
		{
			return Utilities.Round(value, LocalDecimals);
		}

		public bool CheckJobIsNotEmptyWhenChargeCodeRequiredIt()
		{
			bool result = true;

			if (AL_JH.IsEmpty &&
				ChargeCode != null &&
				(
					AL_LineType == TransactionLineTypes.Cost ||
					(AL_LineType == TransactionLineTypes.UnapprovedCost && (TransactionHeader == null || !TransactionHeader.IsCreatedByENett))
				) &&
				(!IsInDatabase || AL_LineTypeInfo.HasChanges || AL_ACInfo.HasChanges || AL_JHInfo.HasChanges))
			{
				result = AccChargeCode.IsValidInAP(ChargeCode.AC_ChargeType, false);
			}

			return result;
		}

		#region Collect information

		void CollectStackTraceWithOSAmountIncorrect(Action<ZStringBuilder> addNewValueAndOldValueInfo)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountIncorrectWhenPostingReceivableCharges, () =>
			{
				if (AL_LineType == TransactionLineTypes.Revenue && !IsInDatabase &&
					TransactionHeader != null && TransactionHeader.AH_RX_NKTransactionCurrency == AL_RX_NKTransactionCurrency && AL_RX_NKTransactionCurrency == Company.GC_RX_NKLocalCurrency &&
					AL_OSAmount != AL_LineAmount + AL_GSTVAT)
				{
					var info = new ZStringBuilder();
					addNewValueAndOldValueInfo(info);
					info.AppendLine(new StackTrace().ToString());
					return info.ToString();
				}

				return null;
			});
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AL_GC.IsEmpty)
			{
				AL_GC = GlbCompany.CurrentCompany.PK; //do this before base call to avoid creation of new GlbCompany
			}
			if (AL_GB.IsEmpty)
			{
				AL_GB = GlbBranch.CurrentBranch.PK;
			}
			if (AL_LineType.IsEmpty)
			{
				// AccTransactionLines is not valid with empty AL_LineType.
				AL_LineType = TransactionLineTypes.WIP;
				if (AL_AG.IsEmpty)
				{
					AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
				}
			}

			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				var accTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
				AL_AH = accTransactionHeader.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region IEInvoicingEligibilityLiteTransactionLine

		ZGuid IEInvoicingEligibilityLiteTransactionLine.ChargePK
			=> AL_AC.IsValid ? AL_AC : AL_AG;

		ZString IEInvoicingEligibilityLiteTransactionLine.ChargeCode
			=> ChargeCode?.AC_Code ?? ZString.Empty;

		ZString IEInvoicingEligibilityLiteTransactionLine.ChargeType
			=> ChargeCode?.AC_ChargeType ?? ZString.Empty;

		ZString IEInvoicingEligibilityLiteTransactionLine.TaxType
			=> TaxRate?.AT_Type ?? ZString.Empty;

		[DecimalPlaces(nameof(TaxRateDecimals))]
		ZDecimal IEInvoicingEligibilityLiteTransactionLine.TaxRate
			=> TaxRate?.GetRate(AL_TaxDate) ?? ZDecimal.Zero;

		[DecimalPlaces(nameof(CurrencyDecimals))]
		ZDecimal IEInvoicingEligibilityLiteTransactionLine.LineAmount
			=> AL_LineAmount;

		[DecimalPlaces(nameof(CurrencyDecimals))]
		ZDecimal IEInvoicingEligibilityLiteTransactionLine.OSAmount
			=> AL_OSAmount;

		[DecimalPlaces(nameof(CurrencyDecimals))]
		ZDecimal IEInvoicingEligibilityLiteTransactionLine.GSTVATAmount
			=> AL_GSTVAT;

		#endregion

		#region AccTransactionLineDissectionAttributes

		[ChildEditable(true)]
		public AccTransactionLineDissectionAttributeCollection AccTransactionLineDissectionAttributes
		{
			get
			{
				if (fAccTransactionLineDissectionAttributes == null)
				{
					fAccTransactionLineDissectionAttributes = new AccTransactionLineDissectionAttributeCollection(this);
					fAccTransactionLineDissectionAttributes.Load();
					RegisterEditableChildObject(fAccTransactionLineDissectionAttributes);
					SetAlternateGLAccountDissections();
				}
				return fAccTransactionLineDissectionAttributes;
			}
		}
		AccTransactionLineDissectionAttributeCollection fAccTransactionLineDissectionAttributes;

		void SetAlternateGLAccountDissections()
		{
			if (TransactionHeader != null
				&& TransactionHeader.AH_Ledger == LedgerTypes.General
				&& AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value
				&& GLHeader != null && (GLHeader.AlternateGLAccountDissections.Any() || AccTransactionLineDissectionAttributes.Any()))
			{
				var alternateGLAccountDissections = GLHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>();
				var lineDissectionAttributes = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>();
				var removingLineDissectionAttributes = new List<AccTransactionLineDissectionAttribute>();

				if (AccTransactionLineDissectionAttributes.Any())
				{
					foreach (var dissectionAttribute in lineDissectionAttributes)
					{
						if (alternateGLAccountDissections.All(x => x.ADC_Attribute != dissectionAttribute.ALD_Attribute))
						{
							removingLineDissectionAttributes.Add(dissectionAttribute);
						}
					}

					foreach (var removingLineDissection in removingLineDissectionAttributes)
					{
						AccTransactionLineDissectionAttributes.RemoveAndDelete(removingLineDissection);
					}
				}

				foreach (var dissection in alternateGLAccountDissections)
				{
					if (!lineDissectionAttributes.Any(x => x.ALD_Attribute == dissection.ADC_Attribute))
					{
						var newlineDissectionAttribute = AccTransactionLineDissectionAttributes.AddNew();
						newlineDissectionAttribute.ALD_Attribute = dissection.ADC_Attribute;
					}
				}
			}
		}

		#endregion
	}
}
