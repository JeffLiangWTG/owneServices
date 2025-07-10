using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCFXUpliftConfiguration : AutoAccCFXUpliftConfigurationView, IJobConfiguration
	{
		public AccCFXUpliftConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
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
						ReferenceForAddingCFXUplift : ReferenceForEditingCFXUplift;
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
						deleteLog.SL_Reference = ReferenceForDeletingCFXUplift;
					}
				}
			}
		}

		ZString ReferenceForAddingCFXUplift => FormattableString.Invariant($@"Added {GetLevelNameWithParentCode()} CFX config for {GetLogOfCurrentValues()}");

		ZString ReferenceForEditingCFXUplift => FormattableString.Invariant($@"Changed {GetLevelNameWithParentCode()} CFX config for {GetLogOfOriginalValues()} changed to {GetLogOfCurrentValues()}");

		ZString ReferenceForDeletingCFXUplift => FormattableString.Invariant($@"Deleted {GetLevelNameWithParentCode()} CFX config for {GetLogOfOriginalValues()}");

		ZString GetLevelNameWithParentCode()
		{
			ZString parentCode;
			switch (Level)
			{
				case AccCFXConfigurationLevelEnum.Company:
					var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, JCF_GC));
					parentCode = company?.GC_Code ?? ZString.Empty;
					break;
				case AccCFXConfigurationLevelEnum.Branch:
					var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, JCF_ParentID));
					parentCode = branch?.GB_Code ?? ZString.Empty;
					break;
				case AccCFXConfigurationLevelEnum.Organisation:
					var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, JCF_ParentID));
					parentCode = orgHeader?.OH_Code ?? ZString.Empty;
					break;
				default:
					parentCode = ZString.Empty;
					break;
			}
			return parentCode.IsEmpty ? LevelName : ZString.Join(" ", new[] { LevelName, parentCode });
		}

		ZString GetLogOfCurrentValues()
		{
			var logBuilder = new ZStringBuilder();
			logBuilder.Append(FormattableString.Invariant($"{JCF_JobType}-{JCF_ServiceDirection}-{JCF_TransportMode}-{JCF_RX_NKCurrency}"));
			if (IsOriginAndDestinationEnabled)
			{
				logBuilder.Append(FormattableString.Invariant($"-{JCF_RN_NKOriginCountry}-{JCF_RN_NKDestinationCountry}"));
			}
			logBuilder.Append(FormattableString.Invariant($": CFX {JCF_CFXPercentage}% {JCF_CFXMinimum} Min."));
			if (!JCF_StartDate.IsEmpty)
			{
				logBuilder.Append(FormattableString.Invariant($" From {JCF_StartDate:dd/MM/yy} to {JCF_ExpiryDate:dd/MM/yy}"));
			}

			return logBuilder.ToString();
		}

		ZString GetLogOfOriginalValues()
		{
			var logBuilder = new ZStringBuilder();
			logBuilder.Append(FormattableString.Invariant($"{JCF_JobTypeInfo.OriginalValue}-{JCF_ServiceDirectionInfo.OriginalValue}-{JCF_TransportModeInfo.OriginalValue}-{JCF_RX_NKCurrencyInfo.OriginalValue}"));
			var originAndDestinationOriginallyEnabled =
				JCF_ServiceDirectionInfo.OriginalValue.ToString() == Constants.FreightShipmentDirection.Code.All &&
				jobTypesSupportedForOriginAndDestination.Contains(JCF_JobTypeInfo.OriginalValue.ToString());
			if (originAndDestinationOriginallyEnabled)
			{
				logBuilder.Append(FormattableString.Invariant($"-{JCF_RN_NKOriginCountryInfo.OriginalValue}-{JCF_RN_NKDestinationCountryInfo.OriginalValue}"));
			}
			logBuilder.Append(FormattableString.Invariant($": CFX {JCF_CFXPercentageInfo.OriginalValue}% {JCF_CFXMinimumInfo.OriginalValue} Min."));
			if (!JCF_StartDateInfo.OriginalValue.IsEmpty)
			{
				logBuilder.Append(FormattableString.Invariant($" From {JCF_StartDateInfo.OriginalValue:dd/MM/yy} to {JCF_ExpiryDateInfo.OriginalValue:dd/MM/yy}"));
			}

			return logBuilder.ToString();
		}

		#endregion

		#region Delete
		public override void Delete()
		{
			var isValidDeletion = Factory.HasContext(BusinessContext.PermittedToDeleteCFXUpliftConfig);
			var shouldPreventDelete = AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Value;
			var shouldReportError = AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Value;

			if (isValidDeletion || !IsInDatabase)
			{
				base.Delete();
			}
			else
			{
				if (shouldReportError)
				{
					var errorMessage = FormattableString.Invariant($"Trying to delete {LevelName} level CFX Configuration for {JCF_JobType}-{JCF_ServiceDirection}-{JCF_TransportMode}-{JCF_RX_NKCurrency}: CFX {JCF_CFXPercentage}% {JCF_CFXMinimum} Min.");
					ErrorReporter.ReportOnce("DeletingCFXConfiguration", errorMessage);
				}

				if (shouldPreventDelete)
				{
					var resonForCannotDelete = Res.GetString("1e2c834d-3b88-4827-9571-10834e82eb45", "Unable to delete {0} level CFX Configuration for {1}-{2}-{3}-{4}: CFX {5}% {6} Min. Please report this error to CargoWise Support.", LevelName, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_RX_NKCurrency, JCF_CFXPercentage, JCF_CFXMinimum);
					throw new CannotDeleteException(resonForCannotDelete);
				}
				else
				{
					base.Delete();
				}
			}
		}

		#endregion

		[List("Lookups.JobTypesList")]
		public override ZString JCF_JobType
		{
			get => base.JCF_JobType;
			set
			{
				base.JCF_JobType = value;
				ClearOriginAndDestinationIfDisabled();
			}
		}

		[ReadOnlyMember(nameof(IsOriginAndDestinationDisabled))]
		public override ZString JCF_RN_NKOriginCountry { get => base.JCF_RN_NKOriginCountry; set => base.JCF_RN_NKOriginCountry = value; }

		[ReadOnlyMember(nameof(IsOriginAndDestinationDisabled))]
		public override ZString JCF_RN_NKDestinationCountry { get => base.JCF_RN_NKDestinationCountry; set => base.JCF_RN_NKDestinationCountry = value; }

		ZBool IsOriginAndDestinationEnabled
			=> JCF_ServiceDirection == Constants.FreightShipmentDirection.Code.All &&
			   jobTypesSupportedForOriginAndDestination.Contains(JCF_JobType.ToString());

		ZBool IsOriginAndDestinationDisabled => !IsOriginAndDestinationEnabled;

		readonly string[] jobTypesSupportedForOriginAndDestination =
		{
			JobInvoicingConsumerTypes.Brokerage.Code,
			JobInvoicingConsumerTypes.Shipment.Code
		};

		void ClearOriginAndDestinationIfDisabled()
		{
			if (IsOriginAndDestinationEnabled)
			{
				return;
			}
			if (!JCF_RN_NKOriginCountry.IsEmpty)
			{
				JCF_RN_NKOriginCountry = ZString.Empty;
			}
			if (!JCF_RN_NKDestinationCountry.IsEmpty)
			{
				JCF_RN_NKDestinationCountry = ZString.Empty;
			}
		}

		[List("Lookups.DirectionsList")]
		public override ZString JCF_ServiceDirection
		{
			get => base.JCF_ServiceDirection;
			set
			{
				base.JCF_ServiceDirection = value;
				ClearOriginAndDestinationIfDisabled();
			}
		}

		[List("Lookups.TransportModesList")]
		public override ZString JCF_TransportMode { get => base.JCF_TransportMode; set => base.JCF_TransportMode = value; }

		[DecimalPlaces(2)]
		public override ZDecimal JCF_CFXPercentage { get => base.JCF_CFXPercentage; set => base.JCF_CFXPercentage = value; }

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JCF_CFXMinimum { get => base.JCF_CFXMinimum; set => base.JCF_CFXMinimum = value; }

		protected override ZString HumanReadableNameCore => Res.GetString("89bc8b58-5a34-4cf2-95c3-7a2f5f99ce94", "CFX Uplift");

		public AccCFXConfigurationLevelEnum Level => JCF_ParentTableCode.ToCFXLevel();

		public int LocalCurrencyDecimals => Company?.LocalCurrency?.Decimals ?? GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		public bool IsDateSpecific => JCF_StartDate.IsValid || JCF_ExpiryDate.IsValid;

		public bool IsDuplicateOf(AccCFXUpliftConfiguration cfxConfig)
		{
			return !IsDateSpecific && !cfxConfig.IsDateSpecific && Matches(cfxConfig);
		}

		public bool OverlapsWith(AccCFXUpliftConfiguration cfxConfig)
		{
			return IsDateSpecific && cfxConfig.IsDateSpecific
				&& Matches(cfxConfig)
				&& JCF_StartDate <= cfxConfig.JCF_ExpiryDate
				&& JCF_ExpiryDate >= cfxConfig.JCF_StartDate;
		}

		bool Matches(AccCFXUpliftConfiguration cfxConfig)
		{
			return JCF_GC == cfxConfig.JCF_GC
				&& JCF_ParentTableCode == cfxConfig.JCF_ParentTableCode
				&& JCF_ParentID == cfxConfig.JCF_ParentID
				&& JCF_JobType == cfxConfig.JCF_JobType
				&& JCF_RX_NKCurrency == cfxConfig.JCF_RX_NKCurrency
				&& JCF_ServiceDirection == cfxConfig.JCF_ServiceDirection
				&& JCF_TransportMode == cfxConfig.JCF_TransportMode
				&& JCF_RN_NKOriginCountry == cfxConfig.JCF_RN_NKOriginCountry
				&& JCF_RN_NKDestinationCountry == cfxConfig.JCF_RN_NKDestinationCountry;
		}

		public ZString LevelName
		{
			get
			{
				var captions = new[]
				{
					Res.GetString("c3019e85-fc90-4857-99d8-92b5f2b15d01", "Company"),
					Res.GetString("29ee4c19-7b03-4342-882c-e489db6cb4c3", "Branch"),
					Res.GetString("be028ecf-b8dc-4ff3-8b05-910cb4b35f19", "Organization")
				};

				return captions[(int)Level];
			}
		}

		public ZPropertyInfo LevelNamePropertyInfo => GetZPropertyInfo(nameof(LevelName));

		public override bool ReadOnly
		{
			get
			{
				var result = base.ReadOnly;

				var parentCollections = ((IBusinessObjectInternals)this).ParentCollections.OfType<AccCFXUpliftConfigurationCollection>();
				if (parentCollections != null)
				{
					result = parentCollections.Any(x => x.ReadOnly) || parentCollections.Any(x => x.Level != Level);
				}

				return result;
			}
			set => base.ReadOnly = value;
		}

		public override bool CanDelete => base.CanDelete && !ReadOnly;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (ReadOnly)
				{
					return ResString.GetMultilingualString("15978bf8-9628-41d3-b0f5-8b9334921b7d", @"This {0} level CFX Configuration for {1}-{2}-{3}-{4} is read only and can not be deleted", LevelName, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_RX_NKCurrency);
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;

			row[AccCFXUpliftConfigurationViewSchema.Constants.JCF_ConfigType] = JobConfiguration.TypeCodes.CFXUplift;
			row[AccCFXUpliftConfigurationViewSchema.Constants.JCF_Ledger] = "AR";
			row[AccCFXUpliftConfigurationViewSchema.Constants.JCF_JobType] = "ALL";
			row[AccCFXUpliftConfigurationViewSchema.Constants.JCF_RX_NKCurrency] = ZString.Empty;
		}

		GlbCompany Company => JCF_GC.IsValid ? Factory.Load<GlbCompany>(JCF_GC) : null;

		ZString IJobConfiguration.JobType => JCF_JobType;

		ZString IJobConfiguration.ServiceDirection => JCF_ServiceDirection;

		ZString IJobConfiguration.TransportMode => JCF_TransportMode;

		bool IJobConfiguration.IncludeOptionsForAllJobTypes => true;
	}
}
