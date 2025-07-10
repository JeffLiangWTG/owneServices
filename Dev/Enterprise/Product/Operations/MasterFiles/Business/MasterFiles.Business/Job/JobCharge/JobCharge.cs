using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationInfoCollectorService;

namespace Enterprise.MasterFiles.Business
{
	[UniversalCopyIgnoreBusinessObject, DisableWorkflowSettingPropertiesAfterOnSaving]
	public abstract class JobCharge : AutoJobCharge, ISupportCriticalValidation, ICanApplyDataRefresh, IChargeWithChargeCode, IDataVersionLoggingSupported, IAutoRatingChargeInfo
	{
		#region Schema

		public new abstract class Schema : AutoJobCharge.Schema
		{
			public const string JR_IsApportioned = "JR_IsApportioned";
			public const string JR_OSCostGSTAmt_Calc = "JR_OSCostGSTAmt_Calc";
		}

		#endregion

		protected JobCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_OH_SellAccount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_OH_CostAccount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_GB), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_GE), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_E6), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_AC), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_RX_NKCostCurrency), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_RX_NKSellCurrency), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_AL_APLine), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_AL_ARLine), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_AT_CostGSTRate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_AT_SellGSTRate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_IsARCashAdvance), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_IsAPCashAdvance), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_CAL_ARLine), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JR_CAL_APLine), ConcurrencyPolicy.Strict);

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeConstructorStackTrace, () =>
			{
				return System.Environment.StackTrace;
			});

			CurrentJobChargeConstructorStackTrace = System.Environment.StackTrace;
		}

		protected string CurrentJobChargeConstructorStackTrace { get; }

		public static readonly TypeDecider TypeDecider = new JobChargeTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobChargeFetchStrategy(this);
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged
		{
			get { return AccountingMasterFilesRegistry.Instance.JobChargeDataVersionAutoLogging.Value; }
		}

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		public sealed override void OnSaving()
		{
			base.OnSaving();
			OnSavingCore();
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
			this.SetContext(BusinessContext.JobChargeAfterOnSaving);
		}

		protected virtual void OnSavingCore()
		{
			AddJobEditEvent();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				RemoveARAPLineValueHistory(this.PK);
			}
			this.RemoveContext(BusinessContext.JobChargeAfterOnSaving);
		}

		public override void Delete()
		{
			ReportUnexpectedDelete();

			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();

			if (IsInDatabase)
			{
				AddJobEditEvent();
			}

			if (!IsDeleted)
			{
				// We will use it for On Delete CriticalValidation check
				AddToAPLineValueHistory(JR_AL_APLine);
				AddToARLineValueHistory(JR_AL_ARLine);

				DeleteJobChargeTargetFromMemory();

				JobChargeAttributes.Load();
				JobChargeAttributes.RemoveAndDeleteAll();

				if (ARLine != null && ARLine.AL_LineType == TransactionLineTypes.Revenue && !ARLine.IsInDatabase)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(ARLine.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueTransactionLineWithoutJobCharge, () =>
					{
						return System.Environment.StackTrace;
					});
				}
			}

			try
			{
				var parentCollections = ((IBusinessObjectInternals)this).ParentCollections;
				Factory.SetContext(BusinessContext.DeletingJobCharge);
				base.Delete();

				if (!IsDeleting && IsDeleted && parentCollections.Where(x => !((IBusinessObjectCollectionInternals)x).MastersAreDeleted && x.Contains(PK)).Any()) //base.Delete skip collection with MastersAreDeleted. We must not access objects in such collections.
				{
					ErrorReporter.ReportOnce("Business_Object_Collections_With_Deleted_Charge_3", BuildMessageAboutDeletedChargeAndItsParentCollections(parentCollections));
				}
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeDeleteStackTrace, () =>
				{
					return System.Environment.StackTrace;
				});
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.DeletingJobCharge);
			}
		}

		void DeleteJobChargeTargetFromMemory()
		{
			var inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			inMemoryQuery.AddToFilter(JobChargeTargetSchema.JRT_JR, PK);
			var inMemoryJCTs = Factory.Load<JobChargeTarget>(inMemoryQuery);
			inMemoryJCTs.ForEach(x => x.Delete());
		}

		ZString BuildMessageAboutDeletedChargeAndItsParentCollections(BusinessObjectCollection[] parentCollections)
		{
			var errorMessageBuilder = new ZStringBuilder();
			errorMessageBuilder.Append((NoResString)"Deleted Charge:");
			errorMessageBuilder.Append(this.GetBusinessObjectGenericInfo());
			errorMessageBuilder.Append(this.GetJobChargeOriginalInfo());
			errorMessageBuilder.Append(CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PK, CriticalValidationInfoCollectorServiceKeyType.ChargeCollectionRemoveMethodInfo));
			errorMessageBuilder.Append((NoResString)"Before Delete:");
			errorMessageBuilder.Append(parentCollections.GetParentCollectionsInfo(this));
			errorMessageBuilder.Append((NoResString)"After Delete:");
			errorMessageBuilder.Append(this.GetParentCollectionsInfo());

			return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected override void DeleteForDataRefresh()
		{
			BeforeSuccessfulDeleting?.Invoke(this, EventArgs.Empty);

			if (!IsDeleted)
			{
				ReportUnexpectedDelete();

				if (ARLine != null && ARLine.AL_LineType == TransactionLineTypes.Revenue && !ARLine.IsInDatabase)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(ARLine.PK, CriticalValidationInfoCollectorServiceKeyType.DeletingChargeWithLinkedREVLineNotInDb, () =>
					{
						return System.Environment.StackTrace;
					});
				}
			}
			JobChargeAttributes.RemoveAndDeleteAll();
			base.DeleteForDataRefresh();
		}

		void ReportUnexpectedDelete()
		{
			// This context creation is temporary change to detect unexpected deleting of Job Charge.
			if (this.HasContext(BusinessContext.ReportDeletingCharges))
			{
				ErrorReporter.ReportOnce("Unexpected_Deleting_Of_JobCharge", string.Format(CultureInfo.InvariantCulture, "Deleting {0} when we do not expect it to be deleted.", this.GetType()));
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			BeforeSuccessfulDeleting?.Invoke(this, EventArgs.Empty);
			base.BeforeSuccessfulDelete();
		}

		public EventHandler BeforeSuccessfulDeleting;

		public override bool IsSavedByFactory
		{
			get
			{
				var result = base.IsSavedByFactory;
				return AnyOtherInstanceWithDifferentIsSavedByFactoryValue(result) ?   // Anybody with a different opinion about IsSavedByFactory?
					!result :   // There is a smarter subclass instance which has a different value for IsSavedByFactory, we will reverse our value to match that one.
					result;     // No other BizOs or all have the same IsSavedByFactory.
			}
		}

		bool AnyOtherInstanceWithDifferentIsSavedByFactoryValue(bool isSavedByFactory)
		{
			return this.GetType() == typeof(JobCharge) &&   // Applicable only for JobCharge instance. Should not do this for subclasses as it will be Stack Overflow!!!
							Factory.GetBizOsForPK(this.PK.ToGuid()).Any(x => x != this && x.IsSavedByFactory != isSavedByFactory);
		}

		public string GetIsSavedByFactoryEvaluationInfo()
		{
			var msgBuilder = GetIsSavedByFactoryEvaluationInfoCore();
			return msgBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual ZStringBuilder GetIsSavedByFactoryEvaluationInfoCore()
		{
			var msgBuilder = new ZStringBuilder();
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => base.IsSavedByFactory, "IsSavedByFactoryBase"));
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => AnyOtherInstanceWithDifferentIsSavedByFactoryValue(base.IsSavedByFactory), nameof(AnyOtherInstanceWithDifferentIsSavedByFactoryValue)));
			return msgBuilder;
		}

		protected string EvaluateIsSavedByFactoryAndCollectInfo(Func<bool> condition, string parameterName) => FormattableString.Invariant($"{parameterName}: {(condition?.Invoke() ?? false).ToYesNoString()}");

		public override bool HasChanges
		{
			get
			{
				return base.HasChanges;
			}
			set
			{
				var old = HasChanges;
				var oldIsSavedByFactory = IsSavedByFactory;
				base.HasChanges = value;

				if (old && !HasChanges && IsInDatabase && !IsDeleted && IsBizONeedRecordedForHasChange(ZPropertyInfoHash))
				{
					var message = string.Format(CultureInfo.CurrentCulture, (NoResString)"Resetting HasChanges on Charge in DB with real Changes.\r\nType Name: {0}\r\n{1}\r\nOld IsSavedByFactory: {2}\r\n{3}", GetType().Name, this.GetJobChargeInfo(), oldIsSavedByFactory, GetCallStacksForSelectedEditedProperties());
					ErrorReporter.ReportOnce("ChargesInDbModifiedWithoutHasChangesSet_5", message);
				}
				if (old != value && value && IsInDatabase && !IsDeleted && JR_GC != GlbCompany.CurrentCompany.PK)
				{
					CriticalValidationInfoCollectorService
					.GetOrCreateService(Factory)
					.AddInfoWhenAllowed(PK,
						CriticalValidationInfoCollectorServiceKeyType.JobChargeChangedInDifferentCompanies,
						() => System.Environment.StackTrace,
						CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE
					);
				}
			}
		}

		public bool HasChargeChanged()
		{
			var row = ((IBusinessObjectInternals)this).Row;
			var isRowStateUnchanged = row.RowState == System.Data.DataRowState.Unchanged;

			if (!isRowStateUnchanged)
			{
				var isOnlyDisplaySequenceChanged = true;

				if (row.Field<short>(JobChargeSchema.Constants.JR_DisplaySequence, DataRowVersion.Current) != row.Field<short>(JobChargeSchema.Constants.JR_DisplaySequence, DataRowVersion.Original))
				{
					foreach (DataColumn column in row.Table.Columns)
					{
						if (column.ColumnName != JobChargeSchema.Constants.JR_DisplaySequence && !row[column, DataRowVersion.Current].Equals(row[column, DataRowVersion.Original]))
						{
							isOnlyDisplaySequenceChanged = false;
							break;
						}
					}
				}
				else
				{
					isOnlyDisplaySequenceChanged = false;
				}

				return !isOnlyDisplaySequenceChanged;
			}
			else
			{
				return false;
			}
		}

		string GetCallStacksForSelectedEditedProperties()
		{
			var result = "";

			GetPropertiesReturningCallStacksWithKeys.ForEach(x =>
			{
				var propertyInfo = x.Item1;
				var key = x.Item2;

				if (propertyInfo.HasChanges)
				{
					result += CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PK, key);
				}
			});

			return result;
		}

		Tuple<ZPropertyInfo, CriticalValidationInfoCollectorServiceKeyType>[] GetPropertiesReturningCallStacksWithKeys => new[]
		{
			new Tuple<ZPropertyInfo,CriticalValidationInfoCollectorServiceKeyType>(JR_LocalSellAmtInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LocalSellAmtHasChangesAfterSaving),
			new Tuple<ZPropertyInfo,CriticalValidationInfoCollectorServiceKeyType>(JR_OSSellExRateInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OSSellExRateHasChangesAfterSaving),
			new Tuple<ZPropertyInfo,CriticalValidationInfoCollectorServiceKeyType>(JR_EstimatedCostInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_EstimatedCostHasChangesAfterSaving),
			new Tuple<ZPropertyInfo,CriticalValidationInfoCollectorServiceKeyType>(JR_LineCFXInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LineCFXHasChangesAfterSaving)
		};

		void RecordSetterCallStackAndValueChangesForProperty(ZPropertyInfo info, CriticalValidationInfoCollectorServiceKeyType key, CollectionFrequency frequency, IZType oldValue = null, IZType newValue = null)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, key, () =>
			{
				var sb = new ZStringBuilder();

				if (this.HasContext(BusinessContext.JobChargeAfterOnSaving) || this.HasContext(BusinessContext.PeriodicInvoicePosting))
				{
					sb.AppendLine(FormattableString.Invariant($"{info.Name} Setter Call Stack:"));
					sb.AppendLine(FormattableString.Invariant($"{new StackTrace()}"));
				}

				if (oldValue != null && newValue != null && !oldValue.Equals(newValue))
				{
					sb.AppendLine(FormattableString.Invariant($"{info.Name} Old Value = {oldValue}, New Value = {newValue}."));
				}

				return sb.Length == 0 ? null : sb.ToString();
			}, frequency);
		}

		public static bool IsBizONeedRecordedForHasChange(ZPropertyInfoHashtable infoTable)
		{
			return infoTable.Cast<ZPropertyInfo>().Any(info => info.IsPersistent && info.HasChanges && info.Name != JobChargeSchema.Constants.JR_DisplaySequence);
		}

		void AddJobEditEvent()
		{
			if (!IsDeleted && Job != null && !Job.IsDeleted)
			{
				Job.AddBillingJobEditEvent();
			}
		}

		public bool IsInDatabaseAndReadyForCostPosting
		{
			get { return IsInDatabase && !IsDeleted && Job != null && (Job.IsReadyForCostPosting || HasSecurityErrorChangeJobStatusForCostPosting); }
		}

		public bool IsInDatabaseAndReadyForRevenuePosting
		{
			get { return IsInDatabase && !IsDeleted && Job != null && (Job.IsReadyForRevenuePosting || HasSecurityErrorChangeJobStatusForRevenuePosting); }
		}

		public bool IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity => IsInDatabase && !IsDeleted && JobIsReadyForFinancialClosureWithoutModifySecurity;

		public bool JobIsReadyForFinancialClosureWithoutModifySecurity => Job != null && Job.IsReadyForFinancialClosureWithoutModifySecurity;

		public bool JobIsReadyForFinancialClosureWithoutPostSecurity => Job != null && Job.IsReadyForFinancialClosureWithoutPostSecurity;

#if DEBUG
		internal
#endif
		bool HasSecurityErrorChangeJobStatusForCostPosting
		{
			get
			{
				string jobReadyForCostPostingMsg = null;
				string jobReadyForRevenueAndCostPostingMsg = null;

				foreach (var error in Job.JH_StatusInfo.GetErrors())
				{
					GetSecurityErrorMessage(JobHeaderStatus.JobReadyForCostPosting.CodeAndDescription, ref jobReadyForCostPostingMsg);
					GetSecurityErrorMessage(JobHeaderStatus.JobReadyForRevenueAndCostPosting.CodeAndDescription, ref jobReadyForRevenueAndCostPostingMsg);

					if (error.Message.Contains(jobReadyForCostPostingMsg) || error.Message.Contains(jobReadyForRevenueAndCostPostingMsg))
					{
						return true;
					}
				}

				return false;
			}
		}

#if DEBUG
		internal
#endif
		bool HasSecurityErrorChangeJobStatusForRevenuePosting
		{
			get
			{
				string jobReadyForRevenuePosting = null;
				string jobReadyForRevenueAndCostPosting = null;

				foreach (var error in Job.JH_StatusInfo.GetErrors())
				{
					GetSecurityErrorMessage(JobHeaderStatus.JobReadyForRevenuePosting.CodeAndDescription, ref jobReadyForRevenuePosting);
					GetSecurityErrorMessage(JobHeaderStatus.JobReadyForRevenueAndCostPosting.CodeAndDescription, ref jobReadyForRevenueAndCostPosting);

					if (error.Message.Contains(jobReadyForRevenuePosting) || error.Message.Contains(jobReadyForRevenueAndCostPosting))
					{
						return true;
					}
				}

				return false;
			}
		}

		void GetSecurityErrorMessage(string jobStatusCodeAndDesc, ref string result)
		{
			if (result == null)
			{
				result = new StringBuilder(Env.Security.ChangeStatusOfReadyToPostJobs.ErrorMessageForNotAllowed).AppendLine().AppendLine().
							Append(string.Format(CultureInfo.InvariantCulture, JobHeaderValidation.DisallowOverrideofFieldSecurityErrorMessage, jobStatusCodeAndDesc)).ToString();
			}
		}

		#region Related Business Objects

		#region Company

		public GlbCompany CalculatedCompany
		{
			get { return Branch != null ? Branch.Company : null; }
		}

		#endregion

		#region Attributes

		[ChildEditable(true)]
		public JobChargeAttribCollection JobChargeAttributes
		{
			get
			{
				if (jobChargeAttributes == null)
				{
					jobChargeAttributes = new JobChargeAttribCollection(this);
					jobChargeAttributes.Load();
					RegisterEditableChildObject(jobChargeAttributes);
				}
				return jobChargeAttributes;
			}
		}
		JobChargeAttribCollection jobChargeAttributes;

		#endregion

		#region Consol Cost

		public BusinessObject ParentConsolCost
		{
			get { return (BusinessObject)Factory.Load<IJobConsolCost>(JR_E6); }
		}

		public BusinessObject GatewayConsolCost
		{
			get { return (BusinessObject)Factory.Load<IJobConsolCost>(JR_E6_GatewaySellHeader); }
		}

		#endregion

		#endregion

		#region Properties

		#region JR_IsApportioned

		public ZBool JR_IsApportioned
		{
			get { return !JR_E6.IsEmpty; }
		}

		public ZPropertyInfo JR_IsApportionedInfo
		{
			get { return GetZPropertyInfo(Schema.JR_IsApportioned); }
		}

		#endregion

		#region JR_AgentDeclaredSellAmtLocal

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_AgentDeclaredSellAmtLocal
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(JR_AgentDeclaredSellAmt, JR_OSSellExRate); }
			set
			{
				RefCurrency currency = Factory.Load<RefCurrency>(PK);
				string currencyCode = currency != null ? currency.RX_Code : ZString.Empty;
				JR_AgentDeclaredSellAmt = (!JR_RX_NKSellCurrency.IsEmpty)
					? new ZDecimal(Env.CurrentCompany.ExchangeRate.LocalToForeign(value, JR_OSSellExRate, currencyCode))
					: value;
				JR_AgentDeclaredSellAmtLocalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JR_AgentDeclaredSellAmtLocalInfo
		{
			get { return GetZPropertyInfo(nameof(JR_AgentDeclaredSellAmtLocal)); }
		}

		#endregion

		#region JR_AgentDeclaredCostAmtLocal

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_AgentDeclaredCostAmtLocal
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(JR_AgentDeclaredCostAmt, JR_OSCostExRate); }
			set
			{
				RefCurrency currency = Factory.Load<RefCurrency>(PK);
				string currencyCode = currency != null ? currency.RX_Code : ZString.Empty;
				JR_AgentDeclaredCostAmt = (!JR_RX_NKCostCurrency.IsEmpty)
					? new ZDecimal(Env.CurrentCompany.ExchangeRate.LocalToForeign(value, JR_OSCostExRate, currencyCode))
					: value;
				JR_AgentDeclaredCostAmtLocalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JR_AgentDeclaredCostAmtLocalInfo
		{
			get { return GetZPropertyInfo(nameof(JR_AgentDeclaredCostAmtLocal)); }
		}

		#endregion

		#region JR_Calc_RelatedJobNumber

		public virtual ZString JR_Calc_RelatedJobNumber { get; set; }

		#endregion

		#region JR_Calc_LocalSellTaxAmt

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Calc_LocalSellTaxAmt
		{
			get
			{
				if (SellGSTRate != null)
				{
					return Utilities.Round(SellGSTRate.GetRate(JR_SellTaxDate) * JR_LocalSellAmt / 100m, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				}
				else
				{
					return 0;
				}
			}
		}

#if DEBUG

		ZDecimal JR_Calc_LocalCostTaxAmt_ForTestOnly
		{
			get
			{
				if (JR_AT_CostGSTRate.IsValid)
				{
					return CostGSTRate.GetRate(JR_CostTaxDate) * JR_LocalCostAmt / 100m;
				}
				else
				{
					return 0;
				}
			}
		}

#endif

		#endregion

		#region JR_LocalSellAmt

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JR_LocalSellAmt
		{
			get
			{
				return base.JR_LocalSellAmt;
			}
			set
			{
				var oldValue = JR_LocalSellAmt;
				decimal roundedValue = Utilities.Round(value, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				base.JR_LocalSellAmt = roundedValue;

				RecordSetterCallStackAndValueChangesForProperty(JR_LocalSellAmtInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LocalSellAmtHasChangesAfterSaving, CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount, () =>
				{
					if (IsRevenuePosted
					&& JR_LocalSellAmt != oldValue
					&& ARLine != null
					&& JR_LocalSellAmt != ARLine.AL_LineAmount)
					{
						return string.Format(CultureInfo.InvariantCulture, "\r\nJR_LocalSellAmt has been changed from {0} to {1} after the revenue posted.\r\n{2}", oldValue, JR_LocalSellAmt, System.Environment.StackTrace);
					}

					return null;
				});

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtChangeWhenPostingReceivableCharges, () =>
				{
					if (JR_LocalSellAmt != oldValue && Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel))
					{
						var info = new ZStringBuilder();
						info.AppendLine(System.FormattableString.Invariant($"JR_LocalSellAmt = {JR_LocalSellAmt}, JR_LocalSellAmt Old Value = {oldValue}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					}

					return null;
				});

				CheckLocalSellAmountDecimals();
			}
		}

		void CheckLocalSellAmountDecimals()
		{
			if (!ErrorReporter.HasBeenReported(CurrencySubUnitRatioErrorReportKey) &&
				Branch != null &&
				Branch.Company != null)
			{
				var companyLocalCurrencyForBranch = Branch.Company.LocalCurrency;
				if (companyLocalCurrencyForBranch != null &&
					JR_LocalSellAmt.DecimalPlaces > companyLocalCurrencyForBranch.Decimals)
				{
					var errorMsg = @$"Trying to update JR_LocalSellAmt with a value, of which the sub unit ratio is higher than expected.
Might be related to Critical Validation Error Type {nameof(CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11)}.

Additional Info:
Current Company Code: {Env.CurrentCompany.Code}.
Current Company Currency Code: {Env.CurrentCompany.LocalCurrency.Code}.

Job Charge's Company (JR_GC) Code:{Company?.GC_Code}.
Job Charge's Company (JR_GC) Currency Code:{Company?.GC_RX_NKLocalCurrency}.

Branch of Job Charge(JR_GB)'s Company Code:{Branch?.Company.GC_Code}.
Branch of Job Charge(JR_GB)'s Company Currency Code:{Branch?.Company.GC_RX_NKLocalCurrency}.

Company Currency Change Log:
{CompanyCurrencyChangeLogHelper.GetCurrentCompanyCurrencyChangeLog(Factory)}";

					ErrorReporter.ReportOnce(CurrencySubUnitRatioErrorReportKey, errorMsg);
				}
			}
		}

		const string CurrencySubUnitRatioErrorReportKey = "JobChargeLocalAmountDecimalsDoNotMatchCurrencySubUnitRatio";

		#endregion

		#region JR_OSSellGSTAmt_Calc

		public virtual ZDecimal JR_OSSellGSTAmt_Calc
		{
			get
			{
				return 0;
			}
		}

		public ZPropertyInfo JR_OSSellGSTAmt_CalcInfo
		{
			get { return GetZPropertyInfo(nameof(JR_OSSellGSTAmt_Calc)); }
		}

		#endregion

		#region JR_LocalCostAmt

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JR_LocalCostAmt
		{
			get
			{
				return base.JR_LocalCostAmt;
			}
			set
			{
				var oldValue = JR_LocalCostAmt;
				decimal roundedValue = Utilities.Round(value, GlbCompany.CurrentCompany.LocalCurrency.Decimals);

				var originalIgnoreValidationSuspended = IgnoreValidationSuspended;
				var isReopenValidation = (Factory.HasContext(BusinessContext.PostManagerCreatingTransaction) && ParentConsolCost != null && ParentConsolCost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting)) || originalIgnoreValidationSuspended;
				using (new DisposableAction(
					() => { IgnoreValidationSuspended = isReopenValidation; },
					() => { IgnoreValidationSuspended = originalIgnoreValidationSuspended; }
					))
				{
					base.JR_LocalCostAmt = roundedValue;
				}

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount, () =>
				{
					if (IsCostPosted
					&& JR_LocalCostAmt != oldValue
					&& APLine != null
					&& APLine.IsInDatabase
					&& JR_LocalCostAmt != -APLine.AL_LineAmount)
					{
						return string.Format(CultureInfo.InvariantCulture, "\r\nJR_LocalCostAmt has been changed from {0} to {1} after the cost posted.\r\n{2}", oldValue, JR_LocalCostAmt, System.Environment.StackTrace);
					}

					return null;
				});

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount, () =>
				{
					if (IsInDatabase
					&& JR_LocalCostAmt != oldValue
					&& (JR_LocalCostAmt > 0 && JR_OSCostAmt < 0) || (JR_LocalCostAmt < 0 && JR_OSCostAmt > 0))
					{
						return string.Format(CultureInfo.InvariantCulture, "\r\nJR_LocalCostAmt has been changed from {0} to {1}.\r\n{2}", oldValue, JR_LocalCostAmt, System.Environment.StackTrace);
					}

					return null;
				});
			}
		}

		#endregion

		#region JR_OSSellAmt

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public override ZDecimal JR_OSSellAmt
		{
			get
			{
				return base.JR_OSSellAmt;
			}
			set
			{
				decimal roundedValue = Utilities.Round(value, OSSellCurrencyDecimals);
				var oldValue = JR_OSSellAmt;
				base.JR_OSSellAmt = roundedValue;
				CollectDeveloperInfoInJR_OSSellAmtSetter(oldValue, value);
			}
		}

		void CollectDeveloperInfoInJR_OSSellAmtSetter(ZDecimal oldValue, ZDecimal newValue)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount, () =>
			{
				var chargeInfo = new JobChargeCriticalValidation.LineRelatedChargeInfo(JR_AL_ARLineInfo);

				if (IsRevenuePosted
				&& JR_OSSellAmt != oldValue
				&& ARLine != null
				&& chargeInfo.OSAmount != ARLine.AL_OSAmount)
				{
					return FormattableString.Invariant(
$@"{nameof(JR_OSSellGSTAmt_Calc)}: {JR_OSSellGSTAmt_Calc}
{nameof(ARLine.AL_OSAmount)}: {ARLine.AL_OSAmount}
{nameof(chargeInfo.OSAmount)}: {chargeInfo.OSAmount}
{nameof(JR_OSSellAmt)} has been changed from {oldValue} to {JR_OSSellAmt} after the revenue posted.

StackTrace:
{System.Environment.StackTrace}");
				}

				return null;
			});

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed, () =>
			{
				if (this.HasContext(BusinessContext.InvoicingPlugInGUI))
				{
					if (JR_RX_NKSellCurrency == Company.GC_RX_NKLocalCurrency && JR_OSSellExRate == 1m && newValue != JR_LocalSellAmt)
					{
						return new StackTrace().ToString();
					}
				}
				return null;
			});
		}

		#endregion

		#region JR_OSCostAmt

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_OSCostAmt
		{
			get
			{
				return base.JR_OSCostAmt;
			}
			set
			{
				var oldValue = JR_OSCostAmt;
				base.JR_OSCostAmt = Utilities.Round(value, OSCostCurrencyDecimals);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostAmountChanged, () =>
				{
					if (oldValue != JR_OSCostAmt)
					{
						return FormattableString.Invariant($@"{JobChargeSchema.JR_OSCostAmt.Name}: {JR_OSCostAmt}, {JobChargeSchema.JR_OSCostAmt.Name} old value: {oldValue}
{System.Environment.StackTrace}");
					}
					return null;
				});

				if (JR_OSCostAmt != oldValue && JR_OSCostAmt != 0 && JR_LocalCostAmt != 0 && Math.Sign(JR_OSCostAmt) != Math.Sign(JR_LocalCostAmt))
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount,
					() =>
					{
						return FormattableString.Invariant($@"{JobChargeSchema.JR_OSCostAmt.Name} has been changed from {oldValue} to {JR_OSCostAmt}.
{System.Environment.StackTrace}");
					});
				}
			}
		}

		#endregion

		#region JR_OSCostGSTAmt

		/// <summary>
		/// Set and Get Cost GST amount.
		/// It takes care of all pre-conditions that must be met before setting a non-zero amount to persistent JR_OSCostGSTAmt
		/// </summary>
		public abstract ZDecimal JR_OSCostGSTAmt_Calc { get; set; }

		public ZPropertyInfo JR_OSCostGSTAmt_CalcInfo
		{
			get { return GetZPropertyInfo(Schema.JR_OSCostGSTAmt_Calc); }
		}

		/// <summary>
		/// Do not set a non-zero amount to this persistent property directly.
		/// Use JR_OSCostGSTAmt_Calc instead to set and get Cost GST amount.
		/// JR_OSCostGSTAmt_Calc takes care of all pre-conditions that must be met before setting a non-zero amount to JR_OSCostGSTAmt
		/// </summary>
		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override sealed ZDecimal JR_OSCostGSTAmt
		{
			get
			{
				return base.JR_OSCostGSTAmt;
			}
			set
			{
				var oldValue = JR_OSCostGSTAmt;
				base.JR_OSCostGSTAmt = value;
				CollectDeveloperInfoInJR_OSCostGSTAmtSetter(oldValue,value);
			}
		}

		void CollectDeveloperInfoInJR_OSCostGSTAmtSetter(ZDecimal oldValue, ZDecimal newValue)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostGSTAmountChangedWhenCostPosted, () =>
			{
				if (IsCostPosted
				&& JR_OSCostGSTAmt != oldValue)
				{
					return FormattableString.Invariant($@"
Old value: {oldValue},
New Value: {JR_OSCostGSTAmt}

StackTrace:
{System.Environment.StackTrace}");
				}
				return null;
			});
		}

		#endregion

		#region JR_InvoiceType

		[List("Lookups+InvoiceTypeList")]
		public override ZString JR_InvoiceType
		{
			get { return base.JR_InvoiceType; }
			set
			{
				base.JR_InvoiceType = value;

				if (!JR_RX_NKSellInvoiceCurrency.IsEmpty &&
					(JR_InvoiceType.IsEmpty || !AllowsSellInvoiceCurrency))
				{
					JR_RX_NKSellInvoiceCurrency = ZString.Empty;
				}
			}
		}

		#endregion

		#region JR_DeclaredOSCostAmt

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_DeclaredOSCostAmt
		{
			get => base.JR_DeclaredOSCostAmt;
			set => base.JR_DeclaredOSCostAmt = value;
		}

		#endregion

		#region JR_LineCFX

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal JR_LineCFX
		{
			get => base.JR_LineCFX;
			set
			{
				var oldValue = JR_LineCFX;
				base.JR_LineCFX = value;
				RecordSetterCallStackAndValueChangesForProperty(JR_LineCFXInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LineCFXHasChangesAfterSaving, CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession, oldValue, value);
			}
		}

		#endregion

		#region JR_MarginPercentage

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal JR_MarginPercentage
		{
			get => base.JR_MarginPercentage;
			set => base.JR_MarginPercentage = value;
		}

		#endregion

		#region JR_MarginPercentage

		[DecimalPlaces(nameof(UnitDecimals))]
		public override ZDecimal JR_ProductQuantity
		{
			get => base.JR_ProductQuantity;
			set => base.JR_ProductQuantity = value;
		}

		#endregion

		#region JobChargeAttrib

		public ZString JobChargeAttrib_PartAttrib1 =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib1);

		public ZString JobChargeAttrib_PartAttrib2 =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib2);

		public ZString JobChargeAttrib_PartAttrib3 =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib3);

		public ZString JobChargeAttrib_SerialNumber =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.SerialNumber);

		public ZString JobChargeAttrib_Commodity =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Commodity);

		public ZString JobChargeAttrib_ContainerCode =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ContainerCode);

		public ZString JobChargeAttrib_ContainerNumber =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ContainerNumber);

		public ZString JobChargeAttrib_DocketReference =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.DocketReference);

		public ZString JobChargeAttrib_LocationDesc =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc);

		public ZString JobChargeAttrib_LocationType =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationType);

		public ZString JobChargeAttrib_Product =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Product);

		// TODO remove this property in 2 years time with WI00617632
		public ZString JobChargeAttrib_MinimumRateUsed =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.MinimumRateUsed);

		public ZDecimal JobChargeAttrib_ItemsToRate =>
			ZDecimal.ParseSafe(JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ItemsToRate), 0m);

		public ZDecimal JobChargeAttrib_UnroundedItemsToRate =>
			ZDecimal.ParseSafe(JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.UnroundedItemsToRate), 0m);

		public ZString JobChargeAttrib_ItemsToRateUnit =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ItemsToRateUnit);

		public ZString JobChargeAttrib_CartageZoneDescription =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.CartageZoneDescription);

		public ZString JobChargeAttrib_JobNumbersReference =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.JobNumbersReference);

		public ZString JobChargeAttrib_RateId =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.RateId);

		public ZString JobChargeAttrib_ServiceProviderPK =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ServiceProviderPK);

		public ZString JobChargeAttrib_ServiceId =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ServiceID);

		public ZString JobChargeAttrib_TransportProviderPK =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.TransportProviderPK);

		public ZString JobChargeAttrib_CalculatorDescription =>
			JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.CalculatorDescription);

		public IEnumerable<ZString> JobChargeAttrib_AllCalculatorDescriptions =>
			JobChargeAttributes.GetAllValuesFromName(JobChargeAttribTypeList.Codes.CalculatorDescription);

		public ZGuid JobChargeAttrib_CartageLegPK
		{
			get
			{
				ZGuid.TryParse(JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.CartageLegPK), out var result);
				return result;
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Requires English only description")]
		public ZString EnglishOnlyDescription
		{
			get
			{
				return ChargeCode != null && ChargeCode.AC_LocalLanguageDescription == JR_Desc ? ChargeCode.AC_Desc : JR_Desc;
			}
		}

		#region JR_OSCostExRate

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public override ZDecimal JR_OSCostExRate
		{
			get { return base.JR_OSCostExRate; }
			set
			{
				base.JR_OSCostExRate = Utilities.Round(value, ExchangeRateDecimalPlaces);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency, () =>
				{
					if ((!IsInDatabase || JR_RX_NKCostCurrencyInfo.HasChanges || JR_OSCostExRateInfo.HasChanges)
					&& JR_OSCostExRate != 0 && Company.GC_RX_NKLocalCurrency == JR_RX_NKCostCurrency && JR_OSCostExRate != 1m)
					{
						return new StackTrace().ToString();
					}
					return null;
				});
			}
		}

		#endregion

		#region JR_OSSellExRate

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public override ZDecimal JR_OSSellExRate
		{
			get { return base.JR_OSSellExRate; }
			set
			{
				var oldValue = base.JR_OSSellExRate;
				base.JR_OSSellExRate = Utilities.Round(value, ExchangeRateDecimalPlaces);

				CollectDeveloperInfoInJR_OSSellExRateSetter(oldValue, value);

				if (Factory.HasContext(BusinessContext.NonAccountingCode))
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK
						, CriticalValidationInfoCollectorServiceKeyType.OsSellExRateGetChangedDuringRunningNonAccountingCode
						, () => new StackTrace().ToString()
						, CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				}
			}
		}

		void CollectDeveloperInfoInJR_OSSellExRateSetter(ZDecimal oldValue, ZDecimal newValue)
		{
			RecordSetterCallStackAndValueChangesForProperty(JR_OSSellExRateInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OSSellExRateHasChangesAfterSaving, CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeSellCurrency, () =>
			{
				if ((!IsInDatabase || JR_RX_NKSellCurrencyInfo.HasChanges || JR_OSSellExRateInfo.HasChanges)
				&& JR_OSSellExRate != 0 && Company.GC_RX_NKLocalCurrency == JR_RX_NKSellCurrency && JR_OSSellExRate != 1m)
				{
					return new StackTrace().ToString();
				}
				return null;
			});

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed, () =>
			{
				if (this.HasContext(BusinessContext.InvoicingPlugInGUI))
				{
					if (JR_RX_NKSellCurrency == Company.GC_RX_NKLocalCurrency && newValue == 1m && JR_OSSellAmt != JR_LocalSellAmt)
					{
						return new StackTrace().ToString();
					}
				}
				return null;
			});

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateChangeWhenPostingReceivableCharges, () =>
			{
				if (JR_OSSellExRate != oldValue && Factory.HasContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel))
				{
					var info = new ZStringBuilder();
					info.AppendLine(System.FormattableString.Invariant($"{nameof(JR_OSSellExRate)} = {JR_OSSellExRate}, {nameof(JR_OSSellExRate)} Old Value = {oldValue}"));
					info.AppendLine(new StackTrace().ToString());
					return info.ToString();
				}

				return null;
			});

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount, () =>
			{
				if (JR_OSSellExRate < 0)
				{
					return System.Environment.StackTrace;
				}
				return null;
			});

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateNotEqualRelatedLineExRate, () =>
			{
				if (IsRevenuePosted
				&& JR_OSSellExRate != oldValue
				&& ARLine != null
				&& JR_OSSellExRate != ARLine.AL_ExchangeRate)
				{
					return FormattableString.Invariant(
$@"{nameof(JR_OSSellAmt)}: {JR_OSSellAmt}
{nameof(JR_OSSellGSTAmt_Calc)}: {JR_OSSellGSTAmt_Calc}
{nameof(ARLine.AL_OSAmount)}: {ARLine.AL_OSAmount}
{nameof(ARLine.AL_ExchangeRate)}: {ARLine.AL_ExchangeRate}
{nameof(JR_OSSellExRate)} has been changed from {oldValue} to {JR_OSSellExRate} after the revenue posted.

StackTrace:
{System.Environment.StackTrace}");
				}
				return null;
			});
		}

		#endregion

		#region JR_JH

		public override ZGuid JR_JH
		{
			get { return base.JR_JH; }
			set
			{
				var previousJR_JH = JR_JH;

				base.JR_JH = value;

				if (previousJR_JH != JR_JH)
				{
					SetDefaultsFromJobParent();
					SetProFormaRevenueAndCost();
				}

				if (Job != null && !Job.JH_IsActive)
				{
					ErrorReporter.ReportOnce("ChargeIsLinkedToInactiveJob", "Charge is linked to inactive job.");
				}

				if (Job != null && Job.JH_Status == JobHeaderStatus.Closed.Code)
				{
					IsCreatedOnClosedJob = true;

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeCreatedOnClosedJobStackTrace, () =>
					{
						return System.Environment.StackTrace;
					}, collectionFrequency: CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				}
			}
		}

		void SetDefaultsFromJobParent()
		{
			var job = Job;
			if (job != null)
			{
				var invoicingPlugIn = job.Parent as IJobInvoicingPlugIn;
				if (invoicingPlugIn != null)
				{
					invoicingPlugIn.InvoicingSupporter.SetDefaultsForNewCharge(this);

					if (JR_E6.IsEmpty)
					{
						JR_CostReference = invoicingPlugIn.InvoicingSupporter.OperationalJobRef;
					}
				}
			}
		}

		public void SetProFormaRevenueAndCost()
		{
			var job = Job;

			if (job != null)
			{
				using (new DisposableAction(() => this.SetContext(Context.SettingProFormaRevenueAndCost), () => this.RemoveContext(Context.SettingProFormaRevenueAndCost)))
				{
					JR_ProFormaCost = job.JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix;
					JR_ProFormaRevenue = job.JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix;
				}
			}
		}

		void ReportErrorWhenJR_ProFormaRevenueAndCostIsNotSetFromSetProFormaRevenueAndCost()
		{
			if (!this.HasContext(Context.SettingProFormaRevenueAndCost))
			{
				var message = "JR_ProFormaCost and JR_ProFormaRevenue can only be set from SetProFormaRevenueAndCost method";
				ErrorReporter.ReportDeveloperExceptionOnce("JobChargeJR_ProFormaRevenueOrCostIsNotSetFromSetProFormaRevenueAndCost", "", new DeveloperNotificationException(message));
			}
		}

		#endregion

		#region JR_ProFormaCost

		public override ZBool JR_ProFormaCost
		{
			get => base.JR_ProFormaCost;
			set
			{
				ReportErrorWhenJR_ProFormaRevenueAndCostIsNotSetFromSetProFormaRevenueAndCost();
				base.JR_ProFormaCost = value;
				CollectDeveloperInfoInJR_ProFormaCostSetter();
			}
		}

		void CollectDeveloperInfoInJR_ProFormaCostSetter()
		{
			if (Job != null)
			{
				GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaCostSetterCallStack, () =>
				{
					if (!this.HasContext(Context.SettingProFormaRevenueAndCost) && JR_ProFormaCost == (Job.JH_ParentTableCode != RatingHeaderSchema.Constants.Prefix))
					{
						return FormattableString.Invariant($@"
{nameof(JR_ProFormaCost)}: {JR_ProFormaCost}
Linked Job {nameof(Job.JH_ParentTableCode)}: {Job.JH_ParentTableCode}

StackTrace:
{System.Environment.StackTrace}");
					}

					return null;
				});
			}
		}

		#endregion

		#region JR_ProFormaRevenue

		public override ZBool JR_ProFormaRevenue
		{
			get => base.JR_ProFormaRevenue;
			set
			{
				ReportErrorWhenJR_ProFormaRevenueAndCostIsNotSetFromSetProFormaRevenueAndCost();
				base.JR_ProFormaRevenue = value;
				CollectDeveloperInfoInJR_ProFormaRevenueSetter();
			}
		}

		void CollectDeveloperInfoInJR_ProFormaRevenueSetter()
		{
			if (Job != null)
			{
				GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaRevenueSetterCallStack, () =>
				{
					if (!this.HasContext(Context.SettingProFormaRevenueAndCost) && JR_ProFormaRevenue == (Job.JH_ParentTableCode != RatingHeaderSchema.Constants.Prefix))
					{
						return FormattableString.Invariant($@"
{nameof(JR_ProFormaRevenue)}: {JR_ProFormaRevenue}
Linked Job {nameof(Job.JH_ParentTableCode)}: {Job?.JH_ParentTableCode} 

StackTrace:
{System.Environment.StackTrace}");
					}

					return null;
				});
			}
		}

		#endregion

		#region JR_OH_SellAccount

		public override ZGuid JR_OH_SellAccount
		{
			get { return base.JR_OH_SellAccount; }
			set
			{
				var oldValue = JR_OH_SellAccount;
				base.JR_OH_SellAccount = value;
				if (JR_OH_SellAccount != oldValue)
				{
					JR_OA_SellInvoiceAddress = ZGuid.Empty;
					JR_OC_SellInvoiceContact = ZGuid.Empty;

					if (IsSettingHasChangesSuspended && IsInDatabase && !Factory.HasContext(BusinessContext.SetDefaultsForJob))
					{
						var message = string.Format("Setting JR_OH_SellAccount when setting HasChanges is susupended.\r\nType Name: {0}\r\n{1}", GetType().Name, this.GetJobChargeInfo());
						ErrorReporter.ReportOnce("ChargesInDbModifiedWithoutHasChangesSet_5", message, new Exception(message));
					}

					CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(ARLine, oldValue, this);
				}
			}
		}

		#endregion

		#region JR_OH_CostAccount

		public override ZGuid JR_OH_CostAccount
		{
			get => base.JR_OH_CostAccount;
			set
			{
				var oldValue = JR_OH_CostAccount;
				base.JR_OH_CostAccount = value;
				if (JR_OH_CostAccount != oldValue)
				{
					CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(APLine, oldValue, this);
				}
			}
		}

		#endregion

		#region JR_GB

		public override ZGuid JR_GB
		{
			get
			{
				return base.JR_GB;
			}
			set
			{
				base.JR_GB = value;

				if (Branch != null)
				{
					this.JR_GC = Branch.GB_GC;
				}

				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateJR_GE();
				}
			}
		}

		#endregion

		#region CashAdvanceRequestLine
		public AccCashAdvanceRequestLine ARCashAdvanceRequestLine
		{
			get
			{
				return Factory.Load<AccCashAdvanceRequestLine>(JR_CAL_ARLine);
			}
		}

		public AccCashAdvanceRequestLine APCashAdvanceRequestLine
		{
			get { return Factory.Load<AccCashAdvanceRequestLine>(JR_CAL_APLine); }
		}

		#endregion

		#endregion

		#region Cost and Reveune Posted Properties

		/// <summary>
		/// Checking whether revenue is posted is an expensive operation.
		/// </summary>
		public bool IsRevenuePosted
		{
			get
			{
				var arLine = ARLine;
				return AccTransactionLines.IsRevenueLine(arLine) || IsRevenuePostedWithJobRevenueJournal(arLine);
			}
		}

		public static bool GetIsRevenuePosted(ZString type)
		{
			return AccTransactionLines.IsRevenueLine(type);
		}

		static bool IsRevenuePostedWithJobRevenueJournal(AccTransactionLines arLine)
		{
			var transactionHeader = arLine?.TransactionHeader;

			return transactionHeader != null
				&& transactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal;
		}

		public bool IsRevenuePostedWithManualJobRevenueJournal
		{
			get
			{
				var arLine = ARLine;
				return IsRevenuePostedWithJobRevenueJournal(arLine)
					&& arLine.TransactionHeader.AH_TransactionCategory.IsEmpty;
			}
		}

		public bool IsRevenuePostedWithAutoJobRevenueJournal
		{
			get
			{
				var arLine = ARLine;
				return IsRevenuePostedWithJobRevenueJournal(arLine)
					&& arLine.TransactionHeader.AH_TransactionCategory == Core.Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
			}
		}

		public bool IsApproved
		{
			get { return APLine != null && APLine.AL_LineType == TransactionLineTypes.Cost; }
		}

		public bool IsCostPostedWithAPTransaction
		{
			get { return AccTransactionLines.IsCostLine(APLine); }
		}

		public bool IsCostPostedWithJobRevenueJournal
		{
			get
			{
				return APLine != null
					&& APLine.TransactionHeader != null
					&& APLine.TransactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal;
			}
		}

		/// <summary>
		/// Checking whether cost is posted is an expensive operation.
		/// </summary>
		public bool IsCostPosted
		{
			get { return AccTransactionLines.IsCostLine(APLine) || AccTransactionLines.IsRevenueLine(APLine); }
		}

		public bool IsCFXPosted
		{
			get { return CFXLine != null && CFXLine.AL_LineType == TransactionLineTypes.Revenue; }
		}

		public bool IsRevenueInDatabase
		{
			get { return ARLine != null && ARLine.IsInDatabase; }
		}

		#endregion

		#region CFX related properties

		/// <summary>
		/// Consider checking BillInInvoiceCurrency property as an option when Sell Invoice should be posted in the JR_RX_NKSellInvoiceCurrency
		/// </summary>
		public bool BillInLocalCurrency
		{
			get { return GetIsBillingInLocalCurrency(JR_RX_NKSellInvoiceCurrency, Company?.GC_RX_NKLocalCurrency ?? ZString.Empty, JR_InvoiceType); }
		}

		public bool BillInInvoiceCurrency
		{
			get { return GetIsBillingInInvoiceCurrency(JR_RX_NKSellInvoiceCurrency, Company?.GC_RX_NKLocalCurrency ?? ZString.Empty, JR_InvoiceType); }
		}

		public bool BIllInInvoiceCurrencySameAsSellCurrency
		{
			get { return BillInInvoiceCurrency && JR_RX_NKSellCurrency == JR_RX_NKSellInvoiceCurrency; }
		}

		public bool BillInInvoiceCurrencyWithLocalSellCurrency
		{
			get { return BillInInvoiceCurrency && IsSellLocal; }
		}

		public static bool GetIsBillingInLocalCurrency(ZString sellInvoiceCurrencyCode, ZString localCurrencyCode, ZString invoiceType) => GetIsEmptyOrLocalCurrency(sellInvoiceCurrencyCode, localCurrencyCode) && GetIsSellInvoiceCurrencyAllowed(invoiceType);

		static bool GetIsBillingInInvoiceCurrency(ZString sellInvoiceCurrencyCode, ZString localCurrencyCode, ZString invoiceType) => !GetIsEmptyOrLocalCurrency(sellInvoiceCurrencyCode, localCurrencyCode) && GetIsSellInvoiceCurrencyAllowed(invoiceType);

		static bool GetIsSellInvoiceCurrencyAllowed(ZString invoiceType) => InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType);

		static bool GetIsEmptyOrLocalCurrency(ZString currencyCode, ZString localCurrencyCode) => currencyCode.IsEmpty || (currencyCode == localCurrencyCode);

		protected bool AllowsSellInvoiceCurrency => GetIsSellInvoiceCurrencyAllowed(JR_InvoiceType);

		public bool IsApplyCFX
		{
			get
			{
				return !(IsRevenuePosted && IsRevenueInDatabase) &&
					(IsSellForeign && AllowsSellInvoiceCurrency || BillInInvoiceCurrencyWithLocalSellCurrency) &&
					ObjectFactory.Get<IAccounting>().JobInvoicingCFXEnabled(JR_GC.ToGuid());
			}
		}

		#endregion

		#region JR_RX_NKSellCurrency

		public override ZString JR_RX_NKSellCurrency
		{
			get { return base.JR_RX_NKSellCurrency; }
			set
			{
				if (JR_RX_NKSellCurrency != value)
				{
					var oldCurrency = JR_RX_NKSellCurrency;

					base.JR_RX_NKSellCurrency = value;

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeSellCurrencyChangeWhenPostingOverseasAgentChargesFromConsol, () =>
					{
						var regisItem = ObjectFactory.Get<IAccounting>().Registry.ProfitShareChargeCode as ChargeCodeRegistryItem;

						if (!IsInDatabase && regisItem != null && JR_AC == regisItem.Value && Factory.HasContext(BusinessContext.PostingChargesFromConsol))
						{
							return GetCurrencyInfo();
						}

						return null;
					});

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeSellCurrencyChangeWhenPostingReceivableCharges, () =>
					{
						if (Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel))
						{
							return GetCurrencyInfo();
						}

						return null;
					});

					string GetCurrencyInfo()
					{
						var info = new ZStringBuilder();
						info.AppendLine(System.FormattableString.Invariant($"JR_RX_NKSellCurrency = {JR_RX_NKSellCurrency}, JR_RX_NKSellCurrency Old Value = {oldCurrency}"));
						info.AppendLine(new StackTrace().ToString());
						return info.ToString();
					}
				}
			}
		}
		#endregion

		#region LocalCurrencyDecimals

		public int LocalCurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region CompanyLocalCurrencyDecimals

		/// <summary>
		/// The number of decimal places for the Charge CalculatedCompany LocalCurrency.
		/// Fallback to GlbCompany.CurrentCompany when CalculatedCompany is null.
		/// </summary>
		public int CompanyLocalCurrencyDecimals
			=> (CalculatedCompany ?? GlbCompany.CurrentCompany).LocalCurrency.Decimals;

		#endregion

		#region OSCostCurrencyDecimals

		public int OSCostCurrencyDecimals => (CostCurrency == null) ? LocalCurrencyDecimals : CostCurrency.Decimals;

		#endregion

		#region OSSellCurrencyDecimals

		public int OSSellCurrencyDecimals => (SellCurrency == null) ? LocalCurrencyDecimals : SellCurrency.Decimals;

		#endregion

		#region OSSellInvoiceCurrencyDecimals

		public int OSSellInvoiceCurrencyDecimals => (SellInvoiceCurrency == null) ? LocalCurrencyDecimals : SellInvoiceCurrency.Decimals;

		#endregion

		#region UnitDecimals

		public int UnitDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits;

		#endregion

		#region WeightDecimals

		public int WeightVolumeDecimals => DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits;

		#endregion

		#region PercentageDecimals

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		#endregion

		#region ExchangeRateDecimalPlaces

		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion

		#region Sell foreign/local

		public ZBool IsSellForeign
		{
			get { return !JR_RX_NKSellCurrency.IsEmpty && Company != null && JR_RX_NKSellCurrency != Company.GC_RX_NKLocalCurrency; }
		}

		public ZBool IsSellInvoiceForeign
		{
			get { return SellInvoiceCurrency != null && !SellInvoiceCurrency.Code.IsNullOrEmpty() && Company != null && SellInvoiceCurrency.Code != Company.GC_RX_NKLocalCurrency; }
		}

		public ZBool IsSellLocal
		{
			get { return !JR_RX_NKSellCurrency.IsEmpty && Company != null && JR_RX_NKSellCurrency == Company.GC_RX_NKLocalCurrency; }
		}
		#endregion

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_AgentDeclaredCostAmt
		{
			get { return base.JR_AgentDeclaredCostAmt; }
			set { base.JR_AgentDeclaredCostAmt = value; }
		}

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public override ZDecimal JR_AgentDeclaredSellAmt
		{
			get { return base.JR_AgentDeclaredSellAmt; }
			set { base.JR_AgentDeclaredSellAmt = value; }
		}

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_EstimatedCost
		{
			get { return base.JR_EstimatedCost; }
			set
			{
				base.JR_EstimatedCost = value;
				var frequency = this.HasContext(BusinessContext.PeriodicInvoicePosting)
					? CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE
					: CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession;

				RecordSetterCallStackAndValueChangesForProperty(JR_EstimatedCostInfo, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_EstimatedCostHasChangesAfterSaving, frequency);
			}
		}

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public override ZDecimal JR_EstimatedRevenue
		{
			get { return base.JR_EstimatedRevenue; }
			set { base.JR_EstimatedRevenue = value; }
		}

		#region JR_AL_APLine

		public override ZGuid JR_AL_APLine
		{
			get
			{
				return base.JR_AL_APLine;
			}
			set
			{
				AddToAPLineValueHistory(JR_AL_APLine);
				var oldValue = JR_AL_APLine;
				base.JR_AL_APLine = value;
				if (JR_AL_APLine != oldValue)
				{
					CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(APLine, ZGuid.Empty, this);
				}
			}
		}

		#endregion

		#region JR_AL_ARLine

		public override ZGuid JR_AL_ARLine
		{
			get
			{
				return base.JR_AL_ARLine;
			}
			set
			{
				AddToARLineValueHistory(JR_AL_ARLine);
				var oldValue = JR_AL_ARLine;
				base.JR_AL_ARLine = value;
				if (oldValue != JR_AL_ARLine)
				{
					CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(ARLine, ZGuid.Empty, this);
				}
			}
		}

		#endregion

		#region AP/AR Line Value History

		public ZGuid[] GetAPLineValueHistory()
		{
			return APLineValueHistory.ToArray();
		}

		public ZGuid[] GetARLineValueHistory()
		{
			return ARLineValueHistory.ToArray();
		}

		protected ZGuid APLinePreviousValueFromHistory
		{
			get { return GetPreviousValueFromHistory(APLineValueHistory); }
		}

		protected ZGuid ARLinePreviousValueFromHistory
		{
			get { return GetPreviousValueFromHistory(ARLineValueHistory); }
		}

		Stack<ZGuid> APLineValueHistory
		{
			get { return APARLineValueHistoryService.GetOrCreateService(Factory).GetAPLineValueHistory(PK); }
		}

		Stack<ZGuid> ARLineValueHistory
		{
			get { return APARLineValueHistoryService.GetOrCreateService(Factory).GetARLineValueHistory(PK); }
		}

		void AddToAPLineValueHistory(ZGuid value)
		{
			AddToLineValueHistory(APLineValueHistory, value);
		}

		void AddToARLineValueHistory(ZGuid value)
		{
			AddToLineValueHistory(ARLineValueHistory, value);
		}

		void RemoveARAPLineValueHistory(ZGuid value)
		{
			APARLineValueHistoryService.GetService(Factory)?.RemoveHistory(value);
		}

		void AddToLineValueHistory(Stack<ZGuid> history, ZGuid value)
		{
			if (!value.IsEmpty && value.IsValid &&
				(history.Count == 0 || history.Peek() != value))
			{
				history.Push(value);
			}
		}

		ZGuid GetPreviousValueFromHistory(Stack<ZGuid> history)
		{
			return history.Count > 0 ? history.Peek() : ZGuid.Empty;
		}

		class APARLineValueHistoryService : IService
		{
			public static APARLineValueHistoryService GetOrCreateService(BusinessObjectFactory factory)
			{
				var result = factory.ServiceContainer.GetService<APARLineValueHistoryService>();
				if (result == null)
				{
					result = new APARLineValueHistoryService();
					factory.ServiceContainer.AddService(result);
				}
				return result;
			}

			public static APARLineValueHistoryService GetService(BusinessObjectFactory factory)
			{
				return factory.ServiceContainer.GetService<APARLineValueHistoryService>();
			}

			APARLineValueHistoryService()
			{
				APARLineValueHistory = new Dictionary<ZGuid, Tuple<Stack<ZGuid>, Stack<ZGuid>>>();
			}

			readonly Dictionary<ZGuid, Tuple<Stack<ZGuid>, Stack<ZGuid>>> APARLineValueHistory;

			internal Stack<ZGuid> GetAPLineValueHistory(ZGuid chargePK)
			{
				return GetHistory(chargePK).Item1;
			}

			internal Stack<ZGuid> GetARLineValueHistory(ZGuid chargePK)
			{
				return GetHistory(chargePK).Item2;
			}

			internal void RemoveHistory(ZGuid chargePK)
			{
				if (APARLineValueHistory.ContainsKey(chargePK))
				{
					APARLineValueHistory.Remove(chargePK);
				}
			}

			Tuple<Stack<ZGuid>, Stack<ZGuid>> GetHistory(ZGuid chargePK)
			{
				Tuple<Stack<ZGuid>, Stack<ZGuid>> history;
				if (!APARLineValueHistory.TryGetValue(chargePK, out history))
				{
					history = new Tuple<Stack<ZGuid>, Stack<ZGuid>>(new Stack<ZGuid>(), new Stack<ZGuid>());
					APARLineValueHistory.Add(chargePK, history);
				}

				return history;
			}
		}

		#endregion

		#region Tax Rate

		#region JR_AT_CostGSTRate

		public override ZGuid JR_AT_CostGSTRate
		{
			get { return base.JR_AT_CostGSTRate; }
			set
			{
				bool hasChanged = JR_AT_CostGSTRate != value;

				if (hasChanged && IsInDatabase
					&& IsCostPosted && APLine.IsInDatabase && !APLine.HasChanges)
				{
					ReportModifyingTaxRateOnPostedCharge();
				}

				base.JR_AT_CostGSTRate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJR_CostGovtChargeCode();
				}
			}
		}

		#endregion

		#region JR_AT_SellGSTRate

		public override ZGuid JR_AT_SellGSTRate
		{
			get { return base.JR_AT_SellGSTRate; }
			set
			{
				bool hasChanged = JR_AT_SellGSTRate != value;

				if (hasChanged && IsInDatabase
					&& IsRevenuePosted && ARLine.IsInDatabase && !ARLine.HasChanges)
				{
					ReportModifyingTaxRateOnPostedCharge();
				}

				base.JR_AT_SellGSTRate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJR_SellGovtChargeCode();
				}
			}
		}

		#endregion

		void ReportModifyingTaxRateOnPostedCharge()
		{
			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Modifying Tax Rate on Posted Charge.\r\nIssue#00885700\r\nType Name: {0}\r\nCharge={1}\r\nJob Consol Cost:{2}\r\nAPLine={3}\r\nARLine={4}",
				GetType().Name, this.GetJobChargeInfo(),
				ParentConsolCost != null ? ParentConsolCost.GetAllPropertyValues() : "NULL",
				APLine != null ? APLine.GetTransactionLineInfo() : "NULL",
				ARLine != null ? ARLine.GetTransactionLineInfo() : "NULL");

			var key = "JobCharge.ModifyingTaxRateOnPostedCharge";
			ErrorReporter.ReportOnce(key, message);
		}

		#endregion

		#region FindJobChargeAttrib

		public JobChargeAttrib FindJobChargeAttrib(ZString attribName)
		{
			return JobChargeAttributes.Cast<JobChargeAttrib>().FirstOrDefault(a => a.EC_Name == attribName);
		}

		#endregion

		#region Can Apply Data Refresh

		bool ICanApplyDataRefresh.CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher)
		{
			return ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(action, this, publisher, GetPropertiesWithStrictConcurrency());
		}

		protected ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			return new[] {
			JR_OH_SellAccountInfo,
			JR_OH_CostAccountInfo,
			JR_GBInfo,
			JR_GEInfo,
			JR_E6Info,
			JR_ACInfo,
			JR_RX_NKCostCurrencyInfo,
			JR_RX_NKSellCurrencyInfo,
			JR_AL_APLineInfo,
			JR_AL_ARLineInfo,
			JR_AT_CostGSTRateInfo,
			JR_AT_SellGSTRateInfo,
			JR_IsARCashAdvanceInfo,
			JR_IsAPCashAdvanceInfo,
			JR_CAL_ARLineInfo,
			JR_CAL_APLineInfo
		};
		}

		#endregion

		public virtual ZString JR_Calc_CostRatingBehavior { get; set; }

		public virtual ZString JR_Calc_SellRatingBehavior { get; set; }

		public abstract bool CanReautorate(CostSell costOrSell, params ZString[] adapterIDs);

		#region PlacesOfSupply

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlacesOfSupply))]
		[ResourceStringData("3e8f1003-b9d9-4f96-a046-e8a3e266ad99", ShortCaption = "Cost FPOS", Caption = "Cost Fixed Place of Supply")]
		public override ZString JR_CostPlaceOfSupply
		{
			get => base.JR_CostPlaceOfSupply;
			set
			{
				base.JR_CostPlaceOfSupply = value;
				JR_CostPlaceOfSupplyType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(Company ?? GlbCompany.CurrentCompany, JR_CostPlaceOfSupply);
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlaceOfSupplyTypes))]
		public override ZString JR_CostPlaceOfSupplyType
		{
			get => base.JR_CostPlaceOfSupplyType;
			set
			{
				base.JR_CostPlaceOfSupplyType = value;
				Validation.ValidateJR_CostPlaceOfSupply();
			}
		}

		public abstract ILocation CostPlaceOfSupplyLocation { get; }

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlacesOfSupply))]
		[ResourceStringData("a7b1eac9-7cb7-4ba0-bc75-3ef78ae509e7", ShortCaption = "Sell FPOS", Caption = "Sell Fixed Place of Supply")]
		public override ZString JR_SellPlaceOfSupply
		{
			get => base.JR_SellPlaceOfSupply;
			set
			{
				base.JR_SellPlaceOfSupply = value;
				JR_SellPlaceOfSupplyType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(Company ?? GlbCompany.CurrentCompany, JR_SellPlaceOfSupply);
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlaceOfSupplyTypes))]
		public override ZString JR_SellPlaceOfSupplyType
		{
			get => base.JR_SellPlaceOfSupplyType;
			set
			{
				base.JR_SellPlaceOfSupplyType = value;
				Validation.ValidateJR_SellPlaceOfSupply();
			}
		}

		internal bool IsCreatedOnClosedJob { get; private set; }

		public abstract ILocation SellPlaceOfSupplyLocation { get; }

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (JR_GB.IsEmpty)
			{
				JR_GB = GlbBranch.CurrentBranch.PK;  //do this before base call to avoid creation of new GlbCompany
				JR_GE = GlbDepartment.CurrentDepartment.PK;  //do this before base call to avoid creation of new GlbDepartment
			}
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			base.FillWithValidTestDataCore(kind, propertyPath);
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
		}

		public void SetAmountsFromLinkedLinesForTests()
		{
			if (ARLine != null)
			{
				int multiplier = ARLine.AL_LineType == TransactionLineTypes.WIP ? -1 : 1;
				JR_RX_NKSellCurrency = ARLine.AL_RX_NKTransactionCurrency;
				JR_AT_SellGSTRate = ARLine.AL_AT;
				JR_A9_SellVATClass = ARLine.AL_A9_VATClass;
				var osExTaxAmount = 0M;
				if (ARLine.AL_LineAmount + ARLine.AL_GSTVAT != 0)
				{
					osExTaxAmount = Utilities.Round(ARLine.AL_OSAmount * ARLine.AL_LineAmount / (ARLine.AL_LineAmount + ARLine.AL_GSTVAT), ARLine.TransactionCurrency != null ? ARLine.TransactionCurrency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				}
				JR_OSSellAmt = osExTaxAmount * multiplier;
				JR_OSSellExRate = ARLine.AL_ExchangeRate;
				JR_LocalSellAmt = ARLine.AL_LineAmount * multiplier;
			}
			if (APLine != null)
			{
				int multiplier = APLine.AL_LineType == TransactionLineTypes.Accrual ? -1 : 1;
				JR_RX_NKCostCurrency = APLine.AL_RX_NKTransactionCurrency;
				JR_AT_CostGSTRate = APLine.AL_AT;
				JR_A9_CostVATClass = APLine.AL_A9_VATClass;
				var osExTaxAmount = 0M;
				if (APLine.AL_LineAmount + APLine.AL_GSTVAT != 0)
				{
					osExTaxAmount = Utilities.Round(APLine.AL_OSAmount * APLine.AL_LineAmount / (APLine.AL_LineAmount + APLine.AL_GSTVAT), APLine.TransactionCurrency != null ? APLine.TransactionCurrency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				}
				JR_OSCostExRate = APLine.AL_ExchangeRate;
				JR_OSCostAmt = -osExTaxAmount * multiplier;
				JR_LocalCostAmt = -APLine.AL_LineAmount * multiplier;
				if (APLine.AL_LineType != TransactionLineTypes.Accrual)
				{
					JR_OSCostGSTAmt_Calc = -(APLine.AL_OSAmount - osExTaxAmount) * multiplier;
				}
			}
		}

		public void SetAmountsToLinkedLinesForTests()
		{
			if (ARLine != null)
			{
				int multiplier = ARLine.AL_LineType == TransactionLineTypes.WIP ? -1 : 1;
				ARLine.AL_OSAmount = JR_OSSellAmt;
				ARLine.AL_OSAmount = (JR_OSSellAmt + JR_OSSellGSTAmt_Calc) * multiplier;
				ARLine.AL_LineAmount = JR_LocalSellAmt * multiplier;
				ARLine.AL_AT = JR_AT_SellGSTRate;
				ARLine.AL_GSTVAT = JR_Calc_LocalSellTaxAmt * multiplier;
			}
			if (APLine != null)
			{
				int multiplier = APLine.AL_LineType == TransactionLineTypes.Accrual ? 1 : -1;
				APLine.AL_OSAmount = (JR_OSCostAmt + JR_OSCostGSTAmt_Calc) * multiplier;
				APLine.AL_LineAmount = JR_LocalCostAmt * multiplier;
				APLine.AL_AT = JR_AT_CostGSTRate;
				APLine.AL_GSTVAT = JR_Calc_LocalCostTaxAmt_ForTestOnly * multiplier;
			}
		}

		public void SetChargeValuesFromLinkedAPLineForTests()
		{
			var apLine = APLine;
			if (apLine == null)
			{
				return;
			}

			var currencyCode = apLine.AL_RX_NKTransactionCurrency;
			JR_RX_NKCostCurrency = currencyCode;
			JR_RX_NKSellCurrency = currencyCode;
			if (apLine.AL_AC.IsEmpty)
			{
				apLine.AL_AC = JR_AC;
			}
			else
			{
				JR_AC = apLine.AL_AC;
			}
			JR_GB = apLine.AL_GB;
			JR_GE = apLine.AL_GE;
			if (apLine.AL_JH.IsEmpty)
			{
				apLine.AL_JH = JR_JH;
			}
			else
			{
				JR_JH = apLine.AL_JH;
			}
			JR_A9_CostVATClass = apLine.AL_A9_VATClass;
			JR_AT_CostGSTRate = apLine.AL_AT;

			SetAmountsFromLinkedLinesForTests();
		}

		public void SetChargeValuesFromLinkedARLineForTests()
		{
			var arLine = ARLine;
			if (arLine == null)
			{
				return;
			}

			var currencyCode = arLine.AL_RX_NKTransactionCurrency;
			JR_RX_NKCostCurrency = currencyCode;
			JR_RX_NKSellCurrency = currencyCode;
			if (arLine.AL_AC.IsEmpty)
			{
				arLine.AL_AC = JR_AC;
			}
			else
			{
				JR_AC = arLine.AL_AC;
			}
			JR_GB = arLine.AL_GB;
			JR_GE = arLine.AL_GE;
			if (arLine.AL_JH.IsEmpty)
			{
				arLine.AL_JH = JR_JH;
			}
			else
			{
				JR_JH = arLine.AL_JH;
			}
			JR_A9_SellVATClass = arLine.AL_A9_VATClass;
			JR_AT_SellGSTRate = arLine.AL_AT;

			SetAmountsFromLinkedLinesForTests();
		}

		public void SetAPLineForcedForTest(ZGuid linePK)
		{
			var originalAPLine = APLine;
			if (originalAPLine != null)
			{
				var savedLineType = originalAPLine.AL_LineType;
				try
				{
					originalAPLine.AL_LineType = ""; // To go around the BaseCharge protection for posted AP Lines
					JR_AL_APLine = linePK;
				}
				finally
				{
					originalAPLine.AL_LineType = savedLineType;
				}
			}
			else
			{
				JR_AL_APLine = linePK;
			}
		}

		public void SetARLineForcedForTest(ZGuid linePK, bool withoutCriticalValidationCheck = false)
		{
			var originalARLine = ARLine;
			if (withoutCriticalValidationCheck)
			{
				base.JR_AL_ARLine = linePK;
			}
			else if (originalARLine != null)
			{
				var savedLineType = originalARLine.AL_LineType;
				try
				{
					originalARLine.AL_LineType = ""; // To go around the BaseCharge protection for posted AR Lines
					JR_AL_ARLine = linePK;
				}
				finally
				{
					originalARLine.AL_LineType = savedLineType;
				}
			}
			else
			{
				JR_AL_ARLine = linePK;
			}
		}

#endif

		#region ISupportCriticalValidation

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return new JobChargeCriticalValidation(this); }
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		#region IAutoRatingChargeInfo Members

		ICurrency IAutoRatingChargeInfo.CostCurrency => CostCurrency;
		ICurrency IAutoRatingChargeInfo.SellCurrency => SellCurrency;
		public ZBool IsApportioned => JR_IsApportioned;
		public ZDecimal CostAmount => JR_OSCostAmt;
		public ZDecimal AgentDeclaredCostAmount => JR_AgentDeclaredCostAmt;
		public ZDecimal SellAmount => JR_OSSellAmt;
		public ZDecimal LocalCostAmount => JR_LocalCostAmt;
		public ZDecimal LocalSellAmount => JR_LocalSellAmt;
		public ZString CostReference => JR_CostReference;
		public ZString SellReference => JR_SellReference;
		public ZString CostRatingBehavior => JR_Calc_CostRatingBehavior;
		public ZString SellRatingBehavior => JR_Calc_SellRatingBehavior;
		public ZGuid CostAccountPK => JR_OH_CostAccount;

		IAutoRatingChargeInfo IAutoRatingChargeInfo.ParentConsolCost => ParentConsolCost as IAutoRatingChargeInfo;

		public RateAttributeSet RateAttributes
		{
			get
			{
				var set = new RateAttributeSet();

				foreach (JobChargeAttrib attr in JobChargeAttributes)
				{
					set.Add(attr.EC_Name, attr.EC_Value, attr.EC_Amount);
				}

				return set;
			}
		}

		#endregion

		string IChargeWithChargeCode.AC_Code => ChargeCode?.AC_Code;
		string IChargeWithChargeCode.AC_Desc => ChargeCode?.AC_Desc;

		public static SchemaGuidColumn GetCashAdvanceRequestLineFKColumn(ZString ledgerType)
		{
			SchemaGuidColumn column = null;
			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				column = JobChargeSchema.JR_CAL_ARLine;
			}
			else if (ledgerType == LedgerTypes.AccountsPayable)
			{
				column = JobChargeSchema.JR_CAL_APLine;
			}
			return column;
		}

		public void ClearCashAdvanceRequestLineLink(ZString ledgerType)
		{
			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				JR_CAL_ARLine = ZGuid.Empty;
			}
			else if (ledgerType == LedgerTypes.AccountsPayable)
			{
				JR_CAL_APLine = ZGuid.Empty;
			}
		}

		public void RemoveCashAdvanceRequirement(ZString ledgerType)
		{
			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				JR_IsARCashAdvance = false;
			}
			else if (ledgerType == LedgerTypes.AccountsPayable)
			{
				JR_IsAPCashAdvance = false;
			}
		}

		enum Context
		{
			SettingProFormaRevenueAndCost
		}
	}
}
