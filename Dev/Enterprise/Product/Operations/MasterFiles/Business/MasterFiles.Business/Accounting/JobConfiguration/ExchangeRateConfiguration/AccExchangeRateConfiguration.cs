using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccExchangeRateConfiguration : AutoAccExchangeRateConfigurationView, IJobConfiguration
	{
		public AccExchangeRateConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoAccExchangeRateConfigurationView.Schema
		{
			public const string JCE_Calc_CurrencyType = "JCE_Calc_CurrencyType";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JCE_ConfigType = JobConfiguration.TypeCodes.ExchangeRate;
			JCE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			JCE_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			JCE_ServiceDirection = Constants.FreightShipmentDirection.Code.All;
			JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;
			CurrencyConfigurations.AddNew();
		}

		#region Auto Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();
			var autoCreatedLog = Logs.AutoCreatedLog;
			if (autoCreatedLog != null)
			{
				using (((IUpdateFieldsLock)autoCreatedLog).LockForUpdatingKeyFields())
				{
					autoCreatedLog.SL_Reference = autoCreatedLog.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystem.Code ?
						ReferenceForAddingExRate : ReferenceForEditingExRate;
				}
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			if (IsInDatabase && !IsDeleted)
			{
				var deleteLog = Logs.MostRecentLogByEventTime(AutoEvents.DeletedARecordInTheSystem);
				if (deleteLog != null)
				{
					using (((IUpdateFieldsLock)deleteLog).LockForUpdatingKeyFields())
					{
						deleteLog.SL_Reference = ReferenceForDeletingExRate;
					}
				}
			}
		}

		ZString ReferenceForAddingExRate => FormattableString.Invariant($@"Added {GetLevelNameWithParentCode()} Ex. Rate config for {ExRateConfigMessageID}.");

		ZString ReferenceForEditingExRate => FormattableString.Invariant($@"Changed {GetLevelNameWithParentCode()} Ex. Rate config for {ExRateConfigOriginalValueMessageID} changed to {ExRateConfigMessageID}.");

		ZString ReferenceForDeletingExRate => FormattableString.Invariant($@"Deleted {GetLevelNameWithParentCode()} Ex. Rate config for {ExRateConfigOriginalValueMessageID}.");

		ZString GetLevelNameWithParentCode()
		{
			ZString parentCode = default;
			switch (Level)
			{
				case AccExRateConfigurationLevelEnum.None:
				case AccExRateConfigurationLevelEnum.System:
					break;
				case AccExRateConfigurationLevelEnum.Company:
					var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, JCE_GC));
					parentCode = company?.GC_Code ?? ZString.Empty;
					break;
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					var debtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery(OrgDebtorGroupSchema.PK, JCE_ParentID));
					parentCode = debtorGroup?.OJ_Code ?? ZString.Empty;
					break;
				case AccExRateConfigurationLevelEnum.CreditorGroup:
					var creditorGroup = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery(OrgCreditorGroupSchema.PK, JCE_ParentID));
					parentCode = creditorGroup?.OG_Code ?? ZString.Empty;
					break;
				case AccExRateConfigurationLevelEnum.Debtor:
				case AccExRateConfigurationLevelEnum.Creditor:
					var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, JCE_ParentID));
					parentCode = orgHeader?.OH_Code ?? ZString.Empty;
					break;
			}
			return parentCode.IsEmpty ? LevelName : ZString.Join(" ", new[] { LevelName, parentCode });
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var isValidDeletion = Factory.HasContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig);
			var shouldPreventDelete = AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Value;
			var shouldReportError = AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Value;

			if (isValidDeletion || !IsInDatabase)
			{
				ClearCurrencyConfigurationsAndDelete();
			}
			else
			{
				if (shouldReportError)
				{
					var errorMessage = FormattableString.Invariant($"Trying to delete {LevelName} level Job Billing Exchange Rate Configuration for {ExRateConfigMessageID}.");
					ErrorReporter.ReportOnce("DeletingJobBillingExchangeRateConfiguration", errorMessage);
				}

				if (shouldPreventDelete)
				{
					var reasonForCannotDelete = Res.GetString("7a71f9ff-f84d-40ae-b60b-d169bba179d0", "Unable to delete {0} level Job Billing Exchange Rate Configuration for {1}. Please report this error to CargoWise Support.", LevelName, ExRateConfigMessageID);
					throw new CannotDeleteException(reasonForCannotDelete);
				}
				else
				{
					ClearCurrencyConfigurationsAndDelete();
				}
			}
		}

		void ClearCurrencyConfigurationsAndDelete()
		{
			if (CurrencyConfigurations != null)
			{
				CurrencyConfigurations.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		string ExRateConfigMessageID => GetExRateConfigMessageID(JCE_Ledger, JCE_JobType, JCE_ServiceDirection, JCE_TransportMode, JCE_InvoiceCurrencyType, JCE_Preference);

		string ExRateConfigOriginalValueMessageID => GetExRateConfigMessageID(JCE_LedgerInfo.OriginalValue.ToString(), JCE_JobTypeInfo.OriginalValue.ToString(), JCE_ServiceDirectionInfo.OriginalValue.ToString(),
			JCE_TransportModeInfo.OriginalValue.ToString(), JCE_InvoiceCurrencyTypeInfo.OriginalValue.ToString(), JCE_PreferenceInfo.OriginalValue.ToString());

		string GetExRateConfigMessageID(string ledger, string jobType, string serviceDirection, string transportMode, string invoiceCurrencyType, string preference)
			=> $"Ledger: {ledger}, JobType: {jobType}, ServiceDirection: {serviceDirection}, TransportMode: {transportMode}, InvoiceCurrencyType: {invoiceCurrencyType}, Preference: {preference}";

		#endregion

		[List("Lookups.LedgerList")]
		[ReadOnlyMember(nameof(JCE_Ledger_ReadOnly))]
		public override ZString JCE_Ledger
		{
			get => base.JCE_Ledger;
			set
			{
				base.JCE_Ledger = value;

				if (!IsInvoiceCurrencyTypeEnabled(value))
				{
					JCE_InvoiceCurrencyType = string.Empty;
				}

				ClearPromptOptionIfNecessary();
			}
		}

		protected bool JCE_Ledger_ReadOnly => (Level & (AccExRateConfigurationLevelEnum.System | AccExRateConfigurationLevelEnum.Company)) == 0 || IsSystemDefaultSavedInDB;

		[List("Lookups.JobTypeList")]
		[ReadOnlyMember(nameof(IsSystemDefaultSavedInDB))]
		public override ZString JCE_JobType
		{
			get => base.JCE_JobType;
			set
			{
				base.JCE_JobType = value;
				ClearPromptOptionIfNecessary();
			}
		}

		[List("Lookups.DirectionList")]
		[ReadOnlyMember(nameof(IsSystemDefaultSavedInDB))]
		public override ZString JCE_ServiceDirection { get => base.JCE_ServiceDirection; set => base.JCE_ServiceDirection = value; }

		[List("Lookups.TransportModeList")]
		[ReadOnlyMember(nameof(IsSystemDefaultSavedInDB))]
		public override ZString JCE_TransportMode { get => base.JCE_TransportMode; set => base.JCE_TransportMode = value; }

		[List("Lookups.PreferenceList")]
		public override ZString JCE_Preference
		{
			get => base.JCE_Preference;
			set
			{
				base.JCE_Preference = value;
				ClearPromptOptionIfNecessary();
			}
		}

		[List("Lookups.InvoiceCurrencyTypeList")]
		[ResourceStringData("f6047adb-b345-4d77-b618-cf499f015904", Caption = "Invoice Currency Type",
			FullDescription = "The value in this column determines whether the configuration applies to foreign currency invoices, local currency invoices or to both when set as blank.")]
		public override ZString JCE_InvoiceCurrencyType { get => base.JCE_InvoiceCurrencyType; set => base.JCE_InvoiceCurrencyType = value; }

		internal bool JCE_InvoiceCurrencyType_ReadOnly => !IsInvoiceCurrencyTypeEnabled(JCE_Ledger);

		bool IsInvoiceCurrencyTypeEnabled(string ledger) => Level != AccExRateConfigurationLevelEnum.System && ledger == LedgerTypes.AccountsReceivable;

		internal bool JCE_Prompt_ReadOnly => !IsPromptEnabled;

		bool IsPromptEnabled => (Level == AccExRateConfigurationLevelEnum.System || Level == AccExRateConfigurationLevelEnum.Company)
			&& JCE_JobType == JobInvoicingConsumerTypes.ShipmentCode
			&& JCE_Ledger.IsEmpty
			&& (JCE_Preference == Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate
			|| JCE_Preference == Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate
			|| JCE_Preference == Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate
			|| JCE_Preference == Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);

		void ClearPromptOptionIfNecessary()
		{
			if (JCE_Prompt && JCE_Prompt_ReadOnly)
			{
				JCE_Prompt = false;
			}
		}

		public ExchangeRateCurrencyConfiguration GetCurrencyConfig(ZString currencyCode, ZDate date)
		{
			ExchangeRateCurrencyConfiguration currencyConfig = null;

			if (IsCurrencyTypeAll)
			{
				currencyConfig = CurrencyConfigurations.GetRecord(ZString.Empty, date);
			}
			else if (IsCurrencyTypeCUR && !currencyCode.IsEmpty)
			{
				currencyConfig = CurrencyConfigurations.GetRecord(currencyCode, date);
			}

			return currencyConfig;
		}

		public AccExRateConfigurationLevelEnum Level
		{
			get
			{
				switch ((string)JCE_ParentTableCode)
				{
					case OrgHeaderSchema.Constants.Prefix:
						if (JCE_Ledger == LedgerTypes.AccountsReceivable)
						{
							return AccExRateConfigurationLevelEnum.Debtor;
						}
						else if (JCE_Ledger == LedgerTypes.AccountsPayable)
						{
							return AccExRateConfigurationLevelEnum.Creditor;
						}
						break;
					case OrgDebtorGroupSchema.Constants.Prefix:
						return AccExRateConfigurationLevelEnum.DebtorGroup;
					case OrgCreditorGroupSchema.Constants.Prefix:
						return AccExRateConfigurationLevelEnum.CreditorGroup;
					case "":
						return JCE_GC.IsEmpty ? AccExRateConfigurationLevelEnum.System : AccExRateConfigurationLevelEnum.Company;
				}

				return AccExRateConfigurationLevelEnum.None;
			}
		}

		public ZString LevelName => Level.GetLevelName();

		public ZPropertyInfo LevelNamePropertyInfo => GetZPropertyInfo(nameof(LevelName));

		#region JCE_Calc_CurrencyType

		[ResourceStringData("f24dc084-d847-47a5-b74d-659cecebf63b", Caption = "Currency Selection", FullDescription = "If this configuration applies only to specific currencies, then select 'CUR' and enter the relevant currencies in the Currencies grid. Otherwise, select 'ALL'.")]
		[List("Lookups.CurrencyTypeList")]
		[ReadOnlyMember(nameof(IsSystemDefaultSavedInDB))]
		public ZString JCE_Calc_CurrencyType
		{
			get
			{
				if (!isCurrencyTypeInitialized)
				{
					InitializeCurrencyType();
				}

				return calculatedCurrencyType;
			}

			set
			{
				var isValueUpdated = SetNonPersistentPropertyValue(JCE_Calc_CurrencyTypeInfo, ref calculatedCurrencyType, value);

				if (isValueUpdated)
				{
					UpdateCurrencyConfigurations();
				}
			}
		}

		ZString calculatedCurrencyType;

		bool isCurrencyTypeInitialized;

		public ZPropertyInfo JCE_Calc_CurrencyTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JCE_Calc_CurrencyType); }
		}

		void InitializeCurrencyType()
		{
			isCurrencyTypeInitialized = true;

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				if (!IsInDatabase)
				{
					calculatedCurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
				}
				else
				{
					calculatedCurrencyType = HasCurrency ? AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR : AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
				}
			}
		}

		#endregion

		#region CurrencyConfigurations

		[ChildEditable]
		public ExchangeRateCurrencyConfigurationCollection CurrencyConfigurations
		{
			get
			{
				if (currencyConfigurations == null)
				{
					currencyConfigurations = new ExchangeRateCurrencyConfigurationCollection(this);
					currencyConfigurations.Load();
					RegisterEditableChildObject(currencyConfigurations);
				}

				return currencyConfigurations;
			}
		}
		ExchangeRateCurrencyConfigurationCollection currencyConfigurations;

		void UpdateCurrencyConfigurations()
		{
			CurrencyConfigurations.RemoveAndDeleteAll();

			if (IsCurrencyTypeAll)
			{
				CurrencyConfigurations.AddNew();
			}
		}

		public bool IsCurrencyTypeAll => JCE_Calc_CurrencyType == AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;

		public bool IsCurrencyTypeCUR => JCE_Calc_CurrencyType == AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

		public bool HasCurrency => CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().Any(c => !c.JCT_Code.IsEmpty);

		#endregion

		public override bool CanDelete => base.CanDelete && !IsSystemDefaultSavedInDB && !ReadOnly;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsSystemDefaultSavedInDB)
				{
					return ResString.GetMultilingualString("632fd110-296f-4676-bd55-35f6be6b61a7", "A configuration for ALL job types is mandatory and cannot be deleted.");
				}
				else if (ReadOnly)
				{
					return ResString.GetMultilingualString("15978bf8-9628-41d3-b0f5-8b9334921b6d", @"This {0} level Job Billing Exchange Rate for {1} is read only and can not be deleted", LevelName, ExRateConfigMessageID);
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		public override bool ReadOnly
		{
			get
			{
				var result = base.ReadOnly;

				var parentCollections = ((IBusinessObjectInternals)this).ParentCollections.OfType<AccExchangeRateConfigurationCollection>();
				if (parentCollections != null)
				{
					result = parentCollections.Any(x => x.ReadOnly) || parentCollections.Any(x => x.Level != Level);
				}

				return result;
			}
			set => base.ReadOnly = value;
		}

		public bool IsDuplicateOf(AccExchangeRateConfiguration exRateConfig)
		{
			return IsTheSameConfigExceptCurrencyType(exRateConfig)
				&& JCE_Calc_CurrencyType == AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL
				&& JCE_Calc_CurrencyType == exRateConfig.JCE_Calc_CurrencyType;
		}

		public bool IsTheSameConfigWithCURCurrencyType(AccExchangeRateConfiguration exRateConfig)
		{
			return IsTheSameConfigExceptCurrencyType(exRateConfig)
				&& JCE_Calc_CurrencyType == AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR
				&& JCE_Calc_CurrencyType == exRateConfig.JCE_Calc_CurrencyType;
		}

		bool IsTheSameConfigExceptCurrencyType(AccExchangeRateConfiguration exRateConfig)
		{
			return JCE_GC == exRateConfig.JCE_GC
				&& JCE_ParentTableCode == exRateConfig.JCE_ParentTableCode
				&& JCE_ParentID == exRateConfig.JCE_ParentID
				&& JCE_Ledger == exRateConfig.JCE_Ledger
				&& JCE_JobType == exRateConfig.JCE_JobType
				&& JCE_ServiceDirection == exRateConfig.JCE_ServiceDirection
				&& JCE_TransportMode == exRateConfig.JCE_TransportMode
				&& JCE_InvoiceCurrencyType == exRateConfig.JCE_InvoiceCurrencyType;
		}

		public bool IsSystemDefaultSavedInDB => IsSystemDefault && IsInDatabase;

		bool IsSystemDefault =>
			Level == AccExRateConfigurationLevelEnum.System &&
			JCE_ConfigType == "ERT" &&
			JCE_JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All &&
			JCE_TransportMode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All &&
			JCE_Calc_CurrencyType == AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL &&
			JCE_ServiceDirection == Constants.FreightShipmentDirection.Code.All &&
			string.IsNullOrEmpty(JCE_Ledger) &&
			string.IsNullOrEmpty(JCE_InvoiceCurrencyType);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new AccExchangeRateConfigurationFetchStrategy(this);
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (!Globals.IsUserInteractive)
				{
					return base.UniqueIndexFailureHandlers;
				}

				var handlers = new List<IUniqueIndexFailureHandler>();
				handlers.Add(new ExchangeRateCurrencyConfigurationUniqueIndexFailureHandler());
				return handlers;
			}
		}

		public override void OnSaving()
		{
			if (currencyConfigurations.Count == 0)
			{
				throw new Exception(Res.GetString("61de6e99-0903-4d3e-9708-85941e188e7e", "Exchange Rate Configuration must have Currency Configuration"));
			}

			base.OnSaving();
		}

		#region IJobConfiguration members

		ZString IJobConfiguration.JobType => JCE_JobType;
		ZString IJobConfiguration.ServiceDirection => JCE_ServiceDirection;
		ZString IJobConfiguration.TransportMode => JCE_TransportMode;
		bool IJobConfiguration.IncludeOptionsForAllJobTypes => false;

		#endregion
	}
}
