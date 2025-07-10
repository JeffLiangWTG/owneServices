using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ResourceStringData("AccPOSConfiguration", Caption = "Place Of Supply Configuration")]
	public class AccPOSConfiguration : AutoAccPOSConfigurationView, IJobConfiguration, IBusinessObjectLogging
	{
		public new class Schema : AutoAccPOSConfigurationView.Schema
		{
			public const string PSC_ChargeType = nameof(PSC_ChargeType);
			public const int PSC_ChargeTypeMaxLength = 3;

			public const string NR_UX__JCF_GC_JCF_Ledger_JCF_ParentId_JCF_ParentTableCode_JCF_JobType_JCF_IncoTerm_JCF_ServiceDirection_JCF_TransportMode = nameof(NR_UX__JCF_GC_JCF_Ledger_JCF_ParentId_JCF_ParentTableCode_JCF_JobType_JCF_IncoTerm_JCF_ServiceDirection_JCF_TransportMode);
		}

		public AccPOSConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var row = ((IBusinessObjectInternals)this).Row;
			row[AccPOSConfigurationViewSchema.Constants.PSC_ConfigType] = JobConfiguration.TypeCodes.PlaceOfSupply;
			row[AccPOSConfigurationViewSchema.Constants.PSC_JobType] = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			row[AccPOSConfigurationViewSchema.Constants.PSC_ServiceDirection] = Constants.FreightShipmentDirection.Code.All;
			row[AccPOSConfigurationViewSchema.Constants.PSC_TransportMode] = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			row[AccPOSConfigurationViewSchema.Constants.PSC_TaxRegistrationType] = ZString.Empty;
			row[AccPOSConfigurationViewSchema.Constants.PSC_IncoTerm] = ZString.Empty;
			row[AccPOSConfigurationViewSchema.Constants.PSC_SupplyType] = ZString.Empty;

			PSC_ChargeType = AccPOSChargeTypeList.Codes.CostAndRevenue;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			PSC_GC = Env.CurrentCompany.PK;
			PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.BillToPartyLocation;
		}
#endif

		#region Properties

		#region PSC_ChargeType

		[ResourceStringData("AccPOSConfiguration|PSC_ChargeType", Caption = "Charge Type")]
		[List("Lookups.ChargeTypeList")]
		[MaxLength(Schema.PSC_ChargeTypeMaxLength)]
		public ZString PSC_ChargeType
		{
			get
			{
				switch (PSC_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						return AccPOSChargeTypeList.Codes.Revenue;
					case LedgerTypes.AccountsPayable:
						return AccPOSChargeTypeList.Codes.Cost;
					case "":
						return AccPOSChargeTypeList.Codes.CostAndRevenue;
					default:
						return ChargeTypeInvalidData;
				}
			}
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(PSC_ChargeTypeInfo, value);
				switch (value)
				{
					case AccPOSChargeTypeList.Codes.Revenue:
						PSC_Ledger = LedgerTypes.AccountsReceivable;
						ChargeTypeInvalidData = ZString.Empty;
						break;
					case AccPOSChargeTypeList.Codes.Cost:
						PSC_Ledger = LedgerTypes.AccountsPayable;
						ChargeTypeInvalidData = ZString.Empty;
						break;
					case AccPOSChargeTypeList.Codes.CostAndRevenue:
						PSC_Ledger = ZString.Empty;
						ChargeTypeInvalidData = ZString.Empty;
						break;
					default:
						PSC_Ledger = InvalidLedger;
						ChargeTypeInvalidData = value;
						break;
				}
				PSC_ChargeTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePSC_ChargeType();
				}
			}
		}

		const string InvalidLedger = "..";

		ZString ChargeTypeInvalidData;

		public ZPropertyInfo PSC_ChargeTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(PSC_ChargeType)); }
		}

		#endregion

		#region PSC_JobType

		[ResourceStringData("AccPOSConfiguration|PSC_JobType", Caption = "Job Type")]
		[List("Lookups.JobTypeList")]
		public override ZString PSC_JobType
		{
			get => base.PSC_JobType;
			set
			{
				base.PSC_JobType = value;
				UpdatePSC_ServiceDirectionOnReadOnly();
				UpdatePSC_TransportModeOnReadOnly();
				UpdatePSC_IncoTermOnReadOnly();
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1307")]
		public JobInvoicingConsumerType JobType => PSC_JobType.IsEmpty || PSC_JobTypeInfo.HasErrors() ? null : Lookups.JobTypeList[PSC_JobType] as JobInvoicingConsumerType;

		#endregion

		#region PSC_IncoTerm

		[ResourceStringData("AccPOSConfiguration|PSC_IncoTerm", Caption = "Incoterms")]
		[List("Lookups.IncoTermList")]
		public override ZString PSC_IncoTerm { get => base.PSC_IncoTerm; set => base.PSC_IncoTerm = value; }

		protected bool PSC_IncoTerm_ReadOnly => Lookups.IncoTermList.Count == 0;

		void UpdatePSC_IncoTermOnReadOnly()
		{
			if (PSC_IncoTermInfo.ReadOnly)
			{
				PSC_IncoTerm = string.Empty;
			}
		}

		#endregion

		#region PSC_ServiceDirection

		[ResourceStringData("AccPOSConfiguration|PSC_ServiceDirection", Caption = "Service Direction", ShortCaption = "Direction")]
		[List("Lookups.ServiceDirectionList")]
		public override ZString PSC_ServiceDirection { get => base.PSC_ServiceDirection; set => base.PSC_ServiceDirection = value; }

		protected bool PSC_ServiceDirection_ReadOnly => !(JobType?.IsDirectionSupported ?? false);

		void UpdatePSC_ServiceDirectionOnReadOnly()
		{
			if (PSC_ServiceDirectionInfo.ReadOnly)
			{
				PSC_ServiceDirection = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			}
		}

		#endregion

		#region PSC_TransportMode

		[ResourceStringData("AccPOSConfiguration|PSC_TransportMode", Caption = "Transport Mode", ShortCaption = "Transport")]
		[List("Lookups.TransportModeList")]
		public override ZString PSC_TransportMode { get => base.PSC_TransportMode; set => base.PSC_TransportMode = value; }

		protected bool PSC_TransportMode_ReadOnly => !(JobType?.IsTransportModeSupported ?? false);

		void UpdatePSC_TransportModeOnReadOnly()
		{
			if (PSC_TransportModeInfo.ReadOnly)
			{
				PSC_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			}
		}

		#endregion

		#region PSC_TaxRegistrationType

		[ResourceStringData("AccPOSConfiguration|PSC_TaxRegistrationType", Caption = "Tax Registration Type", ShortCaption = "Tax Reg.")]
		[List("Lookups.TaxRegistrationTypeList")]
		public override ZString PSC_TaxRegistrationType { get => base.PSC_TaxRegistrationType; set => base.PSC_TaxRegistrationType = value; }

		#endregion

		#region PSC_NK_Branch

		[ResourceStringData("AccPOSConfiguration|PSC_NK_Branch", Caption = "Branch")]
		[List("Lookups.BranchList")]
		public override ZString PSC_NK_Branch { get => base.PSC_NK_Branch; set => base.PSC_NK_Branch = value; }

		#endregion

		#region PSC_SupplyType

		[ResourceStringData("AccPOSConfiguration|PSC_SupplyType", Caption = "Supply Type", ShortCaption = "Supply")]
		[List("Lookups.SupplyTypeList")]
		public override ZString PSC_SupplyType { get => base.PSC_SupplyType; set => base.PSC_SupplyType = value; }

		#endregion

		#region PSC_PlaceOfSupplyRule

		[ResourceStringData("AccPOSConfiguration|PSC_PlaceOfSupplyRule", Caption = "Place Of Supply Rule", ShortCaption = "Rule")]
		[List("Lookups.PlaceOfSupplyRuleList")]
		public override ZString PSC_PlaceOfSupplyRule { get => base.PSC_PlaceOfSupplyRule; set => base.PSC_PlaceOfSupplyRule = value; }

		#endregion

		#region Company

		public GlbCompany Company => Factory.Load<GlbCompany>(PSC_GC);

		#endregion

		#region ChargeCode

		public AccChargeCode ChargeCode
			=> PSC_ParentTableCode == AccChargeCodeSchema.Constants.Prefix ? Factory.Load<AccChargeCode>(PSC_ParentId) : null;

		#endregion

		#region ChargeCodeGroup

		public AccPOSChargeCodeGroup ChargeCodeGroup
			=> PSC_ParentTableCode == AccPOSChargeCodeGroupViewSchema.Constants.Prefix ? Factory.Load<AccPOSChargeCodeGroup>(PSC_ParentId) : null;

		#endregion

		#region Level

		public AccPOSConfigurationLevel Level => GetLevelByParentTableCode(PSC_ParentTableCode);

		AccPOSConfigurationLevel OriginalLevel => GetLevelByParentTableCode((ZString)PSC_ParentTableCodeInfo.OriginalValue);

		AccPOSConfigurationLevel GetLevelByParentTableCode(ZString parentTableCode)
		{
			switch (parentTableCode)
			{
				case "":
					return AccPOSConfigurationLevel.Company;
				case AccPOSChargeCodeGroupViewSchema.Constants.Prefix:
					return AccPOSConfigurationLevel.ChargeCodeGroup;
				case AccChargeCodeSchema.Constants.Prefix:
					return AccPOSConfigurationLevel.ChargeCode;
				default:
					return AccPOSConfigurationLevel.Null;
			}
		}

		public ZString LevelName => GetLevelNameByLevel(Level);

		public ZString OriginalLevelName => GetLevelNameByLevel(OriginalLevel);

		ZString GetLevelNameByLevel(AccPOSConfigurationLevel level)
		{
			switch (level)
			{
				case AccPOSConfigurationLevel.Company:
					return Res.GetString("34e989aa-4536-441e-b905-16621598a747", "Company");
				case AccPOSConfigurationLevel.ChargeCodeGroup:
					return Res.GetString("83dcc6f0-5fcf-4258-872d-5faf35e1cb15", "Charge Code Group");
				case AccPOSConfigurationLevel.ChargeCode:
					return Res.GetString("04aaf694-facf-4503-babf-1221163492fc", "Charge Code");
				default:
					return string.Empty;
			}
		}

		public ZPropertyInfo LevelNamePropertyInfo => GetZPropertyInfo(nameof(LevelName));

		public ZString LevelCode
			=> Level == AccPOSConfigurationLevel.Company ? Company.GC_Code
			: Level == AccPOSConfigurationLevel.ChargeCodeGroup ? ChargeCodeGroup.GRO_Code
			: Level == AccPOSConfigurationLevel.ChargeCode ? ChargeCode?.AC_Code ?? ZString.Empty
			: ZString.Empty;

		#endregion

		#region LookupKey

		public string LookupKey
			=> $"{Company.GC_Code}|{Level}:{LevelCode}|{PSC_JobType}|{PSC_ChargeType}|{PSC_IncoTerm}|{PSC_ServiceDirection}|{PSC_TransportMode}|{PSC_TaxRegistrationType}|{PSC_NK_Branch}|{PSC_SupplyType}";

		#endregion

		#endregion

		#region Overrides

		public override bool ReadOnly
		{
			get
			{
				var parentCollection = ((IBusinessObjectInternals)this).ParentCollections.OfType<AccPOSConfigurationCollection>().FirstOrDefault();
				if (parentCollection == null)
				{
					return base.ReadOnly;
				}

				if (parentCollection.ReadOnly)
				{
					return true;
				}

				return parentCollection.Level != Level;
			}

			set => base.ReadOnly = value;
		}

		public override string ToString() => LookupKey + " > " + PSC_PlaceOfSupplyRule;

		#endregion

		#region IsDuplicateOf()

		public bool IsDuplicateOf(AccPOSConfiguration other)
			=> PK != other.PK
			&& PSC_GC == other.PSC_GC
			&& PSC_ParentTableCode == other.PSC_ParentTableCode
			&& PSC_ParentId == other.PSC_ParentId
			&& PSC_ChargeType == other.PSC_ChargeType
			&& PSC_JobType == other.PSC_JobType
			&& PSC_IncoTerm == other.PSC_IncoTerm
			&& PSC_ServiceDirection == other.PSC_ServiceDirection
			&& PSC_TransportMode == other.PSC_TransportMode
			&& PSC_TaxRegistrationType == other.PSC_TaxRegistrationType
			&& PSC_NK_Branch == other.PSC_NK_Branch
			&& PSC_SupplyType == other.PSC_SupplyType;

		#endregion

		#region CompareByRulePriority[Asc|Desc]()

		/// <summary>
		/// Comparer to sort from most generic to most specific. Eg: Company ... Charge Code.
		/// </summary>
		public static int CompareByRulePriorityGenericToSpecific(AccPOSConfiguration left, AccPOSConfiguration right)
			=> CompareByRulePriority(left, right, 1);

		/// <summary>
		/// Comparer to sort from most specific to most generic. Eg: Charge Code ... Company.
		/// </summary>
		public static int CompareByRulePrioritySpecificToGeneric(AccPOSConfiguration left, AccPOSConfiguration right)
			=> CompareByRulePriority(left, right, -1);

		static int CompareByRulePriority(AccPOSConfiguration left, AccPOSConfiguration right, int factorForDirection)
		{
			if (Math.Abs(factorForDirection) != 1)
			{
				throw new ArgumentException("factorForDirection must be -1 or 1", nameof(factorForDirection));
			}
			if (left == null && right == null)
			{
				return 0;
			}
			else if (left == null)
			{
				return 1;
			}
			else if (right == null)
			{
				return -1;
			}

			var levelComparison = left.Level.CompareTo(right.Level) * factorForDirection;
			if (levelComparison != 0)
			{
				return levelComparison;
			}

			var jobTypeLeft = left.PSC_JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All ? 1 : 2;
			var jobTypeRight = right.PSC_JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All ? 1 : 2;
			var jobTypeComparison = jobTypeLeft.CompareTo(jobTypeRight) * factorForDirection;
			if (jobTypeComparison != 0)
			{
				return jobTypeComparison;
			}

			var chargeTypeLeft = left.PSC_ChargeType == AccPOSChargeTypeList.Codes.CostAndRevenue ? 1 : 2;
			var chargeTypeRight = right.PSC_ChargeType == AccPOSChargeTypeList.Codes.CostAndRevenue ? 1 : 2;
			var chargeTypeComparison = chargeTypeLeft.CompareTo(chargeTypeRight) * factorForDirection;
			if (chargeTypeComparison != 0)
			{
				return chargeTypeComparison;
			}

			var incoTermLeft = left.PSC_IncoTerm == ZString.Empty ? 1 : 2;
			var incoTermRight = right.PSC_IncoTerm == ZString.Empty ? 1 : 2;
			var incoTermComparison = incoTermLeft.CompareTo(incoTermRight) * factorForDirection;
			if (incoTermComparison != 0)
			{
				return incoTermComparison;
			}

			var directionLeft = left.PSC_ServiceDirection == Constants.FreightShipmentDirection.Code.All ? 1 : 2;
			var directionRight = right.PSC_ServiceDirection == Constants.FreightShipmentDirection.Code.All ? 1 : 2;
			var directionComparison = directionLeft.CompareTo(directionRight) * factorForDirection;
			if (directionComparison != 0)
			{
				return directionComparison;
			}

			var transportModeLeft = left.PSC_TransportMode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All ? 1 : 2;
			var transportModeRight = right.PSC_TransportMode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All ? 1 : 2;
			var transportModeComparison = transportModeLeft.CompareTo(transportModeRight) * factorForDirection;
			if (transportModeComparison != 0)
			{
				return transportModeComparison;
			}

			var taxRegistrationLeft = left.PSC_TaxRegistrationType == ZString.Empty ? 1 : 2;
			var taxRegistrationRight = right.PSC_TaxRegistrationType == ZString.Empty ? 1 : 2;
			var taxRegistrationComparison = taxRegistrationLeft.CompareTo(taxRegistrationRight) * factorForDirection;
			if (taxRegistrationComparison != 0)
			{
				return taxRegistrationComparison;
			}

			var branchLeft = left.PSC_NK_Branch == ZString.Empty ? 1 : 2;
			var branchRight = right.PSC_NK_Branch == ZString.Empty ? 1 : 2;
			var branchComparison = branchLeft.CompareTo(branchRight) * factorForDirection;
			if (branchComparison != 0)
			{
				return branchComparison;
			}

			var supplyTypeLeft = left.PSC_SupplyType == ZString.Empty ? 1 : 2;
			var supplyTypeRight = right.PSC_SupplyType == ZString.Empty ? 1 : 2;
			var supplyTypeComparison = supplyTypeLeft.CompareTo(supplyTypeRight) * factorForDirection;
			if (supplyTypeComparison != 0)
			{
				return supplyTypeComparison;
			}
			return 0;
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new PlaceOfSupplyUniqueIndexFailureHandler(this); }
		}

		public bool IsMovingFromAnotherPerson { get; set; }

		class PlaceOfSupplyUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public PlaceOfSupplyUniqueIndexFailureHandler(AccPOSConfiguration config)
			{
				Parent = config;
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return Schema.NR_UX__JCF_GC_JCF_Ledger_JCF_ParentId_JCF_ParentTableCode_JCF_JobType_JCF_IncoTerm_JCF_ServiceDirection_JCF_TransportMode; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var errorDetail = Res.GetString("8accf079-e881-4cf6-8476-70c643f05fc0", @"This Place of Supply Configuration has already been entered.

Company: '{0}', Level: '{1}', {1}: '{2}', Job Type: '{3}', Inco Term: '{4}', Service Direction: '{5}', Transport Mode: '{6}', Tax Registration: '{7}', Branch: '{8}', Supply Type: '{9}'.",
									Parent.Company.GC_Code,
									Parent.LevelName,
									Parent.LevelCode,
									Parent.PSC_JobType,
									Parent.PSC_IncoTerm,
									Parent.PSC_ServiceDirection,
									Parent.PSC_TransportMode,
									Parent.PSC_TaxRegistrationType,
									Parent.PSC_NK_Branch,
									Parent.PSC_SupplyType);
				notifier.ReportError(errorDetail, Res.GetString("6fb07b73-dd6a-49bd-bde0-9f9a422fb55b", "Duplicate Place of Supply Configuration."));
			}

			readonly AccPOSConfiguration Parent;
		}

		#endregion

		#region IJobConfiguration

		ZString IJobConfiguration.JobType => PSC_JobType;
		ZString IJobConfiguration.ServiceDirection => PSC_ServiceDirection;
		ZString IJobConfiguration.TransportMode => PSC_TransportMode;
		bool IJobConfiguration.IncludeOptionsForAllJobTypes => false;

		#endregion

		public override void OnSaving()
		{
			ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccountingLogHelper().AddLog(ChargeCode, this);

			base.OnSaving();
		}

		#region Logging

		public ZString GetLogReference()
		{
			return this.GetLogInfo(GetLogByStatus);
		}

		public string GetLogByStatus(string status, bool shouldCollectOrigin = false)
		{
			ZString originalChargeType = ChargeTypeInvalidData;
			if (shouldCollectOrigin)
			{
				switch ((ZString)PSC_LedgerInfo.OriginalValue)
				{
					case LedgerTypes.AccountsReceivable:
						originalChargeType = AccPOSChargeTypeList.Codes.Revenue;
						break;
					case LedgerTypes.AccountsPayable:
						originalChargeType = AccPOSChargeTypeList.Codes.Cost;
						break;
					case "":
						originalChargeType = AccPOSChargeTypeList.Codes.CostAndRevenue;
						break;
				}
			}

			var source = shouldCollectOrigin ? OriginalLevelName : LevelName;
			var jobType = shouldCollectOrigin ? PSC_JobTypeInfo.OriginalValue : PSC_JobType;
			var chargeType = shouldCollectOrigin ? originalChargeType : PSC_ChargeType;
			var incoTerm = shouldCollectOrigin ? PSC_IncoTermInfo.OriginalValue : PSC_IncoTerm;
			var direction = shouldCollectOrigin ? PSC_ServiceDirectionInfo.OriginalValue : PSC_ServiceDirection;
			var transportMode = shouldCollectOrigin ? PSC_TransportModeInfo.OriginalValue : PSC_TransportMode;
			var taxReg = shouldCollectOrigin ? PSC_TaxRegistrationTypeInfo.OriginalValue : PSC_TaxRegistrationType;
			var branch = shouldCollectOrigin ? PSC_NK_BranchInfo.OriginalValue : PSC_NK_Branch;
			var supplyType = shouldCollectOrigin ? PSC_SupplyTypeInfo.OriginalValue : PSC_SupplyType;
			var rule = shouldCollectOrigin ? PSC_PlaceOfSupplyRuleInfo.OriginalValue : PSC_PlaceOfSupplyRule;

			var optionalBranch = AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				? $", Branch:{branch}"
				: "";

			return $"Place of Supply Configuration {status}: Source:{source}, Job Type:{jobType}, Charge Type:{chargeType}, Inco Term:{incoTerm}, Direction:{direction}, Transport Mode:{transportMode}, Tax Reg:{taxReg}{optionalBranch}, Supply Type:{supplyType}, Rule:{rule}";
		}

		#endregion

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
			=> jobTypeDirectionAndTransportListProvider ??= new JobTypeDirectionAndTransportInfoProvider(() => PSC_JobTypeInfo, () => PSC_ServiceDirectionInfo, () => PSC_TransportModeInfo);
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;
	}
}
