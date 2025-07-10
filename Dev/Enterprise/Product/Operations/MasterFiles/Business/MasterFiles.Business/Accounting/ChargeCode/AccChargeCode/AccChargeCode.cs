using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[DebuggerDisplay("Charge Code ({" + AccChargeCode.Schema.AC_Code + "})")]
	[DescriptionProperty(AccChargeCode.Schema.AC_Desc)]
	[RestrictedFilteredItem]
	[UniversalCopyIgnoreElement(AccChargeCode.Schema.AC_GC)]
	public sealed class AccChargeCode : AutoAccChargeCode, IAccChargeCode, IDocManagerSupport, ITemplateCopyable, IAccChargeTypeOverride, IDataVersionLoggingSupported,
		IDuplicateValidationCollectionProvider<AccChargeComplianceDescription>, IEDocsParsingSupport, IAuditParent
	{
		public new abstract class Schema : AutoAccChargeCode.Schema
		{
			public const string AC_Calc_InputGSTVATRecoverablePercentage = "AC_Calc_InputGSTVATRecoverablePercentage";
		}

		public AccChargeCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AC_InputGSTVATRecoverable

		[ZUnbindableProperty()]
		[ReadOnlyMember(nameof(AC_InputGSTVATRecoverable_ReadOnly))]
		public override ZDecimal AC_InputGSTVATRecoverable
		{
			get { return base.AC_InputGSTVATRecoverable; }
			set
			{
				base.AC_InputGSTVATRecoverable = value;
				AC_Calc_InputGSTVATRecoverablePercentageInfo.RefreshBinding();
			}
		}

		[DecimalPlaces(2)]
		public ZDecimal AC_Calc_InputGSTVATRecoverablePercentage
		{
			get { return AC_InputGSTVATRecoverable * 100m; }
			set
			{
				if (AC_InputGSTVATRecoverable != value)
				{
					AC_InputGSTVATRecoverable = Enterprise.ZArchitecture.Core.Utilities.Round(value / 100m, AccChargeCodeSchema.AC_InputGSTVATRecoverable.Scale);
					AC_Calc_InputGSTVATRecoverablePercentageInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AC_Calc_InputGSTVATRecoverablePercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AC_Calc_InputGSTVATRecoverablePercentage, x => AC_InputGSTVATRecoverableInfo); }
		}

		bool AC_InputGSTVATRecoverable_ReadOnly
		{
			get { return !IsOverhead || IsGlobal || !GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		#endregion

		public static AccChargeCode CreateGlobalChargeCode(BusinessObjectFactory factory)
		{
			var globalChargeCode = factory.New<AccChargeCode>();
			using (globalChargeCode.SuspendSettingHasChanges())
			{
				globalChargeCode.AC_GC = ZGuid.Empty;
			}
			return globalChargeCode;
		}

		public static bool IsValidInConsolCosting(ZString chargeType)
		{
			bool result = true;

			if (chargeType == Constants.ChargeType.Overhead || chargeType == Constants.ChargeType.NonAccrual || chargeType == Constants.ChargeType.Revenue || chargeType == Constants.ChargeType.Comment)
			{
				result = false;
			}

			return result;
		}

		public static bool IsValidOnJob(ZString chargeType)
		{
			bool result = true;

			if (chargeType == Constants.ChargeType.Overhead || chargeType == Constants.ChargeType.NonAccrual)
			{
				result = false;
			}

			return result;
		}

		public static bool IsValidInAP(ZString chargeType, bool isJobEntered)
		{
			bool result = true;

			if (chargeType != Constants.ChargeType.Comment)
			{
				if (isJobEntered)
				{
					if (chargeType == Constants.ChargeType.Revenue)
					{
						result = false;
					}
				}
				else
				{
					if (chargeType != Constants.ChargeType.Overhead && chargeType != Constants.ChargeType.NonAccrual && chargeType != Constants.ChargeType.Comment)
					{
						result = false;
					}
				}
			}

			return result;
		}

		public static bool IsValidInAR(ZString chargeType)
		{
			bool result = true;

			if (chargeType != Constants.ChargeType.Revenue && chargeType != Constants.ChargeType.Comment)
			{
				result = false;
			}

			return result;
		}

		public bool IsReferencedByRegistry
		{
			get
			{
				using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
				{
					return registryDataAccessor.IsPKReferencedByRegistry(PK.ToGuid());
				}
			}
		}

		bool IsElectronicProcessingChargeCode
		{
			get
			{
				if (!IsGlobal && !IsLinkedToGlobalChargeCode)
				{
					return false;
				}

				var refElectronicProcessingChargeCodeGuid = ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargeCode;
				if (refElectronicProcessingChargeCodeGuid != Guid.Empty)
				{
					if (PK.Equals(refElectronicProcessingChargeCodeGuid))
					{
						return true;
					}

					if (IsLinkedToGlobalChargeCode)
					{
						var globalChargeCode = Factory.Load<AccChargeCode>(refElectronicProcessingChargeCodeGuid);
						return globalChargeCode?.AC_Code.Equals(AC_Code) ?? false;
					}
				}

				return false;
			}
		}

		bool IsCompanyGstRegistered
		{
			get { return Company != null && Company.GC_IsGSTRegistered; }
		}

		bool IsCompanyWHTRegistered
		{
			get { return Company != null && Company.GC_IsWHTRegistered; }
		}

		internal bool AC_AT_GSTRate_ReadOnly
		{
			get { return (AC_ChargeType == Constants.ChargeType.Comment && AC_AT_GSTRate.IsEmpty) || !IsCompanyGstRegistered || IsGlobal; }
		}

		internal bool AC_AW_WithholdingTaxRate_ReadOnly
		{
			get { return (AC_ChargeType == Constants.ChargeType.Comment && AC_AW_WithholdingTaxRate.IsEmpty) || IsGlobal || !IsCompanyWHTRegistered; }
		}

		internal bool AC_ChargeSubGroup_ReadOnly
		{
			get { return !GroupHasSubGroups || !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		bool AC_AR_SalesGroup_ReadOnly
		{
			get { return !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed; }
		}

		bool AC_AR_ExpenseGroup_ReadOnly
		{
			get { return !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed; }
		}

		internal bool AC_ChargeGroup_ReadOnly
		{
			get { return !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_IsGroupageCharge_ReadOnly
		{
			get { return !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_RateCalculator_ReadOnly
		{
			get { return !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_ShowOnQuotation_ReadOnly
		{
			get { return IsComment || !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_IsCommissionable_ReadOnly
		{
			get { return !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_DefaultCommissionProduct_ReadOnly
		{
			get { return !AC_IsCommissionable || !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_DefaultCommissionService_ReadOnly
		{
			get { return !AC_IsCommissionable || !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		internal bool AC_DefaultCommissionSubModule_ReadOnly
		{
			get { return !AC_IsCommissionable || !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Hint(this);
		}

		class Hint : EnterpriseBusinessObjectFetchStrategy
		{
			public Hint(AccChargeCode code)
				: base(code)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(AccChargeTaxOverrideSchema.AO_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(AccChargeTypeOverrideSchema.AN_AC_ChargeCode, BusinessObject.PK);
				Factory.AddFetchHint(AccChargeRevRecOverrideSchema.AE_AC, BusinessObject.PK);
				Factory.AddFetchHint(AccChargeGLPostingOverrideSchema.Y1_AC, BusinessObject.PK);
			}
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AC_GC = GlbCompany.CurrentCompany.PK;
			AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;
			AC_Calc_InputGSTVATRecoverablePercentage = 100m;
		}

		#endregion

		#region AC_Desc

		[TranslatableDataField(Schema.TableName, Schema.AC_Desc, DataXmlFilePaths.RefAccounting, MaxLength = Schema.AC_DescMaxLength, Type = typeof(AccChargeCode), Asmid = ResString.AssemblyId)]
		public override ZString AC_Desc { get => base.AC_Desc; set => base.AC_Desc = value; }

		[ResourceStringData("AccChargeCode|AC_DescMultilingual", Caption = "Description")]
		public MultilingualString AC_DescMultilingual => GetMultilingual(AC_DescInfo);

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString GetDescriptionOrLocalDescription(ZString description, bool isLocalClient)
		{
			if (ShouldDefaultLocalChargeDescription(isLocalClient))
			{
				return description == AC_Desc
					? AC_LocalLanguageDescription
					: description;
			}
			else
			{
				return description == AC_LocalLanguageDescription
					? AC_Desc
					: description;
			}
		}

		public string GetLocalLanguageDescription(bool isLocalClient) => ShouldDefaultLocalChargeDescription(isLocalClient)
			? AC_LocalLanguageDescription.ToString()
			: null;

		public bool ShouldDefaultLocalChargeDescription(bool isLocalClient) =>
			!AC_LocalLanguageDescription.IsEmpty
			&& ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault
			&& (isLocalClient || ObjectFactory.Get<IAccounting>().ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors);

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString GetDescriptionOrLocalLanguageDescription(bool isLocalClient)
		{
			return (isLocalClient || ObjectFactory.Get<IAccounting>().ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors) && !AC_LocalLanguageDescription.IsEmpty && ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault
				? AC_LocalLanguageDescription
				: AC_Desc;
		}

		public string GetMultilingualOrDefaultDescription(string description)
		{
			return GetMultilingualDescription(description)
				?? description;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public string GetMultilingualDescription(string description)
		{
			string multilingualDescription = null;

			if (description.StartsWith(AC_Desc, StringComparison.OrdinalIgnoreCase) && !ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault)
			{
				multilingualDescription = AC_DescMultilingual;
				if (description != AC_Desc.ToString())
				{
					multilingualDescription += description.Substring(AC_Desc.ToString().Length);
				}
			}

			return multilingualDescription;
		}

		#endregion

		#region AC_AT_GSTRate
		[List("Lookups.TaxRateCollection")]
		public override ZGuid AC_AT_GSTRate
		{
			get
			{
				return base.AC_AT_GSTRate;
			}
			set
			{
				base.AC_AT_GSTRate = value;
			}
		}
		#endregion

		#region AC_AW_WithholdingTaxRate
		[List("Lookups.WithholdingCollection")]
		public override ZGuid AC_AW_WithholdingTaxRate
		{
			get
			{
				return base.AC_AW_WithholdingTaxRate;
			}
			set
			{
				base.AC_AW_WithholdingTaxRate = value;
			}
		}
		#endregion

		#region AC_AR_SalesGroup
		[List("Lookups.SalesGroupCollection")]
		[ReadOnlyMember(nameof(AC_AR_SalesGroup_ReadOnly))]
		public override ZGuid AC_AR_SalesGroup
		{
			get
			{
				return base.AC_AR_SalesGroup;
			}
			set
			{
				base.AC_AR_SalesGroup = value;
			}
		}
		#endregion

		#region AC_AR_ExpenseGroup
		[List("Lookups.ExpenseGroupCollection")]
		[ReadOnlyMember(nameof(AC_AR_ExpenseGroup_ReadOnly))]
		public override ZGuid AC_AR_ExpenseGroup
		{
			get
			{
				return base.AC_AR_ExpenseGroup;
			}
			set
			{
				base.AC_AR_ExpenseGroup = value;
			}
		}
		#endregion

		#region AC_AG_RevenueAccount
		[List("Lookups.GLRevenueAccountCollection")]
		[ReadOnlyMember(nameof(AC_AG_RevenueAccount_ReadOnly))]
		public override ZGuid AC_AG_RevenueAccount
		{
			get
			{
				return base.AC_AG_RevenueAccount;
			}
			set
			{
				base.AC_AG_RevenueAccount = value;
			}
		}
		#endregion

		#region AC_AG_WIPAccount
		[List("Lookups.GLWIPAccountCollection")]
		[ReadOnlyMember(nameof(AC_AG_WIPAccount_ReadOnly))]
		public override ZGuid AC_AG_WIPAccount
		{
			get
			{
				return base.AC_AG_WIPAccount;
			}
			set
			{
				base.AC_AG_WIPAccount = value;
			}
		}
		#endregion

		#region AC_AG_RevenueClearingAccount
		[List("Lookups.GLRevenueClearingAccounts")]
		[ReadOnlyMember(nameof(AC_AG_RevenueClearingAccountReadOnly))]
		public override ZGuid AC_AG_RevenueClearingAccount
		{
			get
			{
				return base.AC_AG_RevenueClearingAccount;
			}
			set
			{
				base.AC_AG_RevenueClearingAccount = value;
			}
		}

		public bool AC_AG_RevenueClearingAccountReadOnly => !AC_ChargeType.ToString().In(Constants.ChargeType.Revenue, Constants.ChargeType.NonAccrual);

		#endregion

		#region AC_AG_CostClearingAccount
		[List("Lookups.GLCostClearingAccounts")]
		[ReadOnlyMember(nameof(AC_AG_CostClearingAccountReadOnly))]
		public override ZGuid AC_AG_CostClearingAccount
		{
			get
			{
				return base.AC_AG_CostClearingAccount;
			}
			set
			{
				base.AC_AG_CostClearingAccount = value;
			}
		}

		public bool AC_AG_CostClearingAccountReadOnly => !AC_ChargeType.ToString().In(Constants.ChargeType.Overhead, Constants.ChargeType.NonAccrual);

		#endregion

		#region AC_AG_CostAccount
		[List("Lookups.GLCostAccountCollection")]
		[ReadOnlyMember(nameof(AC_AG_CostAccount_ReadOnly))]
		public override ZGuid AC_AG_CostAccount
		{
			get
			{
				return base.AC_AG_CostAccount;
			}
			set
			{
				base.AC_AG_CostAccount = value;
			}
		}
		#endregion

		#region AC_AG_AccrualAccount

		[List("Lookups.GLAccrualAccountCollection")]
		[ReadOnlyMember(nameof(AC_AG_AccrualAccount_ReadOnly))]
		public override ZGuid AC_AG_AccrualAccount
		{
			get
			{
				return base.AC_AG_AccrualAccount;
			}
			set
			{
				base.AC_AG_AccrualAccount = value;
			}
		}

		#endregion

		#region AC_AG_DisbursementSurplusAccount

		[List("Lookups.GLDisbursementSurplusAccountCollection")]
		[ReadOnlyMember(nameof(AC_AG_DisbursementSurplusAccount_ReadOnly))]
		public override ZGuid AC_AG_DisbursementSurplusAccount { get => base.AC_AG_DisbursementSurplusAccount; set => base.AC_AG_DisbursementSurplusAccount = value; }

		#endregion

		#region AC_AG_DisbursementShortfallAccount

		[List("Lookups.GLDisbursementShortfallAccountCollection")]
		[ReadOnlyMember(nameof(AC_AG_DisbursementShortfallAccount_ReadOnly))]
		public override ZGuid AC_AG_DisbursementShortfallAccount { get => base.AC_AG_DisbursementShortfallAccount; set => base.AC_AG_DisbursementShortfallAccount = value; }

		#endregion

		#region AC_ChargeSubGroup
		[List("Lookups.ChargeSubGroupList")]
		public override ZString AC_ChargeSubGroup
		{
			get
			{
				return base.AC_ChargeSubGroup;
			}
			set
			{
				base.AC_ChargeSubGroup = value;
			}
		}
		#endregion

		#region AC_ChargeOtherGroups

		[List("Lookups.ChargeOtherGroupsList")]
		public override ZString AC_ChargeOtherGroups
		{
			[DebuggerStepThrough]
			get { return base.AC_ChargeOtherGroups; }
			[DebuggerStepThrough]
			set { base.AC_ChargeOtherGroups = value; }
		}

		#endregion

		#region AC_ChargeType

		[List("Lookups.AC_ChargeType_List")]
		public override ZString AC_ChargeType
		{
			get { return base.AC_ChargeType; }
			set
			{
				if (base.AC_ChargeType != value)
				{
					base.AC_ChargeType = value;

					AC_Calc_InputGSTVATRecoverablePercentage = 100m;

					if (!IsValidationSuspended)
					{
						Validation.ValidateAC_IATA_ChargeCodeMap();
					}

					if (IsComment)
					{
						AC_ShowOnQuotation = ZBool.False;
						AC_SuppressOnQuoteIfZero = ZBool.False;
					}

					if (IsMargin || IsRevenue || IsManualJobAccrual)
					{
						AC_IsCommissionable = ZBool.True;
					}

					if (AC_AG_CostClearingAccountReadOnly)
					{
						AC_AG_CostClearingAccount = ZGuid.Empty;
					}

					if (AC_AG_RevenueClearingAccountReadOnly)
					{
						AC_AG_RevenueClearingAccount = ZGuid.Empty;
					}

					ChargeTypeOverrides.RefreshBindingIncludingChildren();
					GLPostingOverrides.RefreshBindingIncludingChildren();
				}
				if (ShouldUpdateAC_MarginPercentage)
				{
					UpdateAC_MarginPercentage();
					ShouldUpdateAC_MarginPercentage = false;
				}
			}
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
		bool AC_MarginPercentage_ReadOnly
		{
			get
			{
				PropertyRequired marginPercentRequired = RequiredProperties(AC_ChargeType);
				return marginPercentRequired.IsValid && (IsDisbursement || !marginPercentRequired.MarginPercentage);
			}
		}

		bool AC_AG_RevenueAccount_ReadOnly
		{
			get
			{
				PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.RevenueAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		bool AC_AG_WIPAccount_ReadOnly
		{
			get
			{
				PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.WIPAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		bool AC_AG_CostAccount_ReadOnly
		{
			get
			{
				PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.CostAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		bool AC_AG_AccrualAccount_ReadOnly
		{
			get
			{
				PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.AccrualAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		bool AC_AG_DisbursementSurplusAccount_ReadOnly
		{
			get
			{
				PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.DisbursementSurplusAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		bool AC_AG_DisbursementShortfallAccount_ReadOnly
		{
			get
			{
				PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
				return (accountsRequired.IsValid && !accountsRequired.DisbursementShortfallAccount) || !ChargeCodesEditGLAccountSetupCheckPoint.IsAllowed;
			}
		}

		internal void UpdateChargeTypeDependentReadOnlyInfo()
		{
			PropertyRequired accountsRequired = RequiredProperties(HighestChargeType);
			AC_MarginPercentageInfo.RefreshBinding();

			if (accountsRequired.IsValid)
			{
				AC_AG_RevenueAccountInfo.RefreshBinding();
				AC_AG_WIPAccountInfo.RefreshBinding();
				AC_AG_CostAccountInfo.RefreshBinding();
				AC_AG_AccrualAccountInfo.RefreshBinding();
				AC_AG_DisbursementSurplusAccountInfo.RefreshBinding();
				AC_AG_DisbursementShortfallAccountInfo.RefreshBinding();

				if (!accountsRequired.RevenueAccount)
				{
					AC_AG_RevenueAccount = ZGuid.Empty;
				}

				if (!accountsRequired.WIPAccount)
				{
					AC_AG_WIPAccount = ZGuid.Empty;
				}

				if (!accountsRequired.CostAccount)
				{
					AC_AG_CostAccount = ZGuid.Empty;
				}

				if (!accountsRequired.AccrualAccount)
				{
					AC_AG_AccrualAccount = ZGuid.Empty;
				}

				if (!accountsRequired.DisbursementSurplusAccount)
				{
					AC_AG_DisbursementSurplusAccount = ZGuid.Empty;
				}

				if (!accountsRequired.DisbursementShortfallAccount)
				{
					AC_AG_DisbursementShortfallAccount = ZGuid.Empty;
				}
			}
			AC_AT_GSTRateInfo.RefreshBinding();
			AC_AW_WithholdingTaxRateInfo.RefreshBinding();
		}

		public ZString HighestChargeType
		{
			get
			{
				if (IsRevenue)
				{
					foreach (AccChargeTypeOverride typeOverride in ChargeTypeOverrides)
					{
						if (typeOverride.IsDisbursement || typeOverride.IsMargin)
						{
							return typeOverride.AN_ChargeType;
						}
					}
				}
				return AC_ChargeType;
			}
		}

		#endregion

		#region AC_IATA_ChargeCodeMap

		[List("Lookups.AC_IATACode_List")]
		public override ZString AC_IATA_ChargeCodeMap
		{
			get
			{
				return base.AC_IATA_ChargeCodeMap;
			}
			set
			{
				base.AC_IATA_ChargeCodeMap = value;
			}
		}

		#endregion

		#region AC_RateCalculator

		[List("Lookups.AC_RateCalculator_List")]
		public override ZString AC_RateCalculator
		{
			get { return base.AC_RateCalculator; }
			set
			{
				if (value != base.AC_RateCalculator)
				{
					base.AC_RateCalculator = value;
					AC_RateCalculatorDescInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region AC_RateCalculatorDesc

		public ZString AC_RateCalculatorDesc
		{
			get { return AC_RateCalculator.IsEmpty || AC_RateCalculatorInfo.HasErrors() ? "" : RateCalculatorDescriptions.GetDescriptionFromCode(AC_RateCalculator); }
		}

		public ZPropertyInfo AC_RateCalculatorDescInfo
		{
			get { return GetZPropertyInfo(nameof(AC_RateCalculatorDesc)); }
		}

		CodeDescriptionPairList RateCalculatorDescriptions => Factory.GetCachedValue("RateCalculatorDescriptions", GetRateCalculatorDescriptions);

		CodeDescriptionPairList GetRateCalculatorDescriptions()
		{
			var rateCalculatorDescriptions = new CodeDescriptionPairList();
			rateCalculatorDescriptions.AddPair("AGY", ResString.GetMultilingualString("RateCalculator.AGY", "Used to calculate customs agency charges and handle associated lines and tariff information."));
			rateCalculatorDescriptions.AddPair("CMB", ResString.GetMultilingualString("RateCalculator.CMB", "Allows for Base, Minimum, Per Unit and sliding charges (with possible accumulation) to be specified."));
			rateCalculatorDescriptions.AddPair("CBI", ResString.GetMultilingualString("RateCalculator.CBI", "Used to calculate Maintenance and Repair charges to be specified."));
			rateCalculatorDescriptions.AddPair("CST", ResString.GetMultilingualString("RateCalculator.CST", "Used to specify charges that are based on a matching Costing."));
			rateCalculatorDescriptions.AddPair("CTB", ResString.GetMultilingualString("RateCalculator.CTB", "Used to specify charges that are based on a matching Company Tariff."));
			rateCalculatorDescriptions.AddPair("CTG", ResString.GetMultilingualString("RateCalculator.CTG", "Used to handle various Port Transport weight breaks and varying volumetric conversion factors."));
			rateCalculatorDescriptions.AddPair("CTZ", ResString.GetMultilingualString("RateCalculator.CTZ", "Used to specify Port Transport rates that are based on Port Transport Zones (distance ranges)."));
			rateCalculatorDescriptions.AddPair("DIN", ResString.GetMultilingualString("RateCalculator.DIN", "Used to specify an interest charge on disbursement amounts. This can be based on the customer's credit terms, or on the number of outstanding days."));
			rateCalculatorDescriptions.AddPair("EQH", ResString.GetMultilingualString("RateCalculator.EQH", "Used to calculate a charge for special equipment required on a job."));
			rateCalculatorDescriptions.AddPair("FLT", ResString.GetMultilingualString("RateCalculator.FLT", "Used to handle fixed charges."));
			rateCalculatorDescriptions.AddPair("FPA", ResString.GetMultilingualString("RateCalculator.FPA", "Used to calculate a first unit charge and then a thereafter charge per unit."));
			rateCalculatorDescriptions.AddPair("FPU", ResString.GetMultilingualString("RateCalculator.FPU", "Used to handle a base charge as well as an additional per unit charge."));
			rateCalculatorDescriptions.AddPair("FRT", ResString.GetMultilingualString("RateCalculator.FRT", "Used to specify charges to be listed as an Inclusive Charge for Freight instead of creating a separate charge."));
			rateCalculatorDescriptions.AddPair("HCC", ResString.GetMultilingualString("RateCalculator.HCC", "Used to calculate a charge which is the highest amount of a set of charges."));
			rateCalculatorDescriptions.AddPair("HRC", ResString.GetMultilingualString("RateCalculator.HRC", "Used to calculate charges based on the highest rate of the weight, volume or minimum."));
			rateCalculatorDescriptions.AddPair("HRT", ResString.GetMultilingualString("RateCalculator.HRT", "Used to calculate a charge for each House bill Release Type."));
			rateCalculatorDescriptions.AddPair("IAT", ResString.GetMultilingualString("RateCalculator.IAT", "Used to calculate a charge taking into consideration the number of outer packages and weights - e.g. Italian Airport Tax."));
			rateCalculatorDescriptions.AddPair("IXC", ResString.GetMultilingualString("RateCalculator.IXC", "Used to calculate charges based on the Value of Goods. Most commonly used for the Italian export customs formalities charge."));
			rateCalculatorDescriptions.AddPair("MIN", ResString.GetMultilingualString("RateCalculator.MIN", "Used to calculate rates based on minimum per Job or Charge code."));
			rateCalculatorDescriptions.AddPair("MPU", ResString.GetMultilingualString("RateCalculator.MPU", "Used to calculate per unit charges and apply them if greater than the minimum."));
			rateCalculatorDescriptions.AddPair("NTE", ResString.GetMultilingualString("RateCalculator.NTE", "Used to specify free-text charges that cannot be automatically calculated."));
			rateCalculatorDescriptions.AddPair("PEB", ResString.GetMultilingualString("RateCalculator.PEB", "Used to calculate a charge which is a percentage of a group of charges, another charge or specific job values, where a different percentage is required based on a break value."));
			rateCalculatorDescriptions.AddPair("PER", ResString.GetMultilingualString("RateCalculator.PER", "Used to calculate a charge which is a percentage of a group of charges, another charge or specific job values."));
			rateCalculatorDescriptions.AddPair("PSR", ResString.GetMultilingualString("RateCalculator.PSR", "Used to calculate Profit Share /Rebate against the total Profit or Loss of a job."));
			rateCalculatorDescriptions.AddPair("SMB", ResString.GetMultilingualString("RateCalculator.SMB", "Used to calculate charges based on the day of month. Most commonly used for the Warehousing."));
			rateCalculatorDescriptions.AddPair("TME", ResString.GetMultilingualString("RateCalculator.TME", "Used to specify accumulated sliding charges based on a time period and units."));
			rateCalculatorDescriptions.AddPair("UNT", ResString.GetMultilingualString("RateCalculator.UNT", "Used to calculate a charge per unit."));
			rateCalculatorDescriptions.AddPair("VED", ResString.GetMultilingualString("RateCalculator.VED", "Used to calculate a pivot weight break and apply the volume equalized discount rate."));
			rateCalculatorDescriptions.AddPair("WLT", ResString.GetMultilingualString("RateCalculator.WLT", "Used to calculate storage rates based on the location type."));
			rateCalculatorDescriptions.AddPair("WPK", ResString.GetMultilingualString("RateCalculator.WPK", "Used to calculate warehouse package rates for all inner pack types."));

			return rateCalculatorDescriptions;
		}

		#endregion

		#region AC_SuppressOnQuoteIfZero_ReadOnly

		internal bool AC_SuppressOnQuoteIfZero_ReadOnly
		{
			get { return IsComment || !AC_ShowOnQuotation || !RatingAndQuotationsConfigurationCheckpoint.IsAllowed; }
		}

		#endregion

		#region AC_ChargeGroup
		[List("Lookups.ChargeGroupList")]
		public override ZString AC_ChargeGroup
		{
			get { return base.AC_ChargeGroup; }
			set
			{
				base.AC_ChargeGroup = value;
				if (!GroupHasSubGroups)
				{
					AC_ChargeSubGroup = "";
				}

				AC_IsGroupageCharge = IsConsolChargeGroup;
			}
		}

		#endregion

		#region AC_IsCommissionable

		public override ZBool AC_IsCommissionable
		{
			get { return base.AC_IsCommissionable; }
			set
			{
				if (base.AC_IsCommissionable != value)
				{
					base.AC_IsCommissionable = value;

					if (!AC_IsCommissionable)
					{
						AC_DefaultCommissionProduct = ZString.Empty;
						AC_DefaultCommissionService = ZString.Empty;
						AC_DefaultCommissionSubModule = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region AC_DefaultCommissionProduct

		[List("Lookups.CommissionProducts")]
		public override ZString AC_DefaultCommissionProduct
		{
			get { return base.AC_DefaultCommissionProduct; }
			set { base.AC_DefaultCommissionProduct = value; }
		}

		#endregion

		#region AC_DefaultCommissionService

		[List("Lookups.CommissionServices")]
		public override ZString AC_DefaultCommissionService
		{
			get { return base.AC_DefaultCommissionService; }
			set { base.AC_DefaultCommissionService = value; }
		}

		#endregion

		#region AC_DefaultCommissionSubModule

		[List("Lookups.CommissionSubModules")]
		public override ZString AC_DefaultCommissionSubModule
		{
			get { return base.AC_DefaultCommissionSubModule; }
			set { base.AC_DefaultCommissionSubModule = value; }
		}

		#endregion

		#region Charge Types / Groups

		bool IsConsolChargeGroup
		{
			get
			{
				return AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.Loading ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.Unloading;
			}
		}

		public bool IsCustomsCharge
		{
			get
			{
				return AC_ChargeGroup == ChargeCodeGroupList.Codes.Brokerage ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.BrokerageOnly ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.CustomsDuty ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.OriginBrokerage ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.OriginBrokerageOnly;
			}
		}

		public bool IsFOBCharge
		{
			get
			{
				return AC_ChargeGroup == ChargeCodeGroupList.Codes.Loading ||
					AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin;
			}
		}

		public bool IsMargin
		{
			get { return AC_ChargeType == Constants.ChargeType.Margin; }
		}

		public bool IsDisbursement
		{
			get { return AC_ChargeType == Constants.ChargeType.Disbursement; }
		}

		public bool IsFreight
		{
			get { return AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight; }
		}

		public bool IsRevenue
		{
			get { return AC_ChargeType == Constants.ChargeType.Revenue; }
		}

		public bool IsComment
		{
			get { return AC_ChargeType == Constants.ChargeType.Comment; }
		}

		public bool IsManualJobAccrual
		{
			get { return AC_ChargeType == Constants.ChargeType.ManualJobAccrual; }
		}

		public bool IsNonAccrual
		{
			get { return AC_ChargeType == Constants.ChargeType.NonAccrual; }
		}

		public bool IsOverhead
		{
			get { return AC_ChargeType == Constants.ChargeType.Overhead; }
		}

		public bool GroupHasSubGroups
		{
			get { return Lookups.ChargeSubGroupList.Count > 0; }
		}

		#endregion

		[List("Lookups.GoodServiceTypes")]
		public override ZString AC_GoodsServiceType
		{
			get { return base.AC_GoodsServiceType; }
			set { base.AC_GoodsServiceType = value; }
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region Required Fields Helper

		public PropertyRequired RequiredProperties(string chargeType)
		{
			PropertyRequired result = new PropertyRequired();
			result.IsValid = true;

			switch (chargeType)
			{
				case Constants.ChargeType.Margin:
					result.AccrualAccount = true;
					result.CostAccount = true;
					result.MarginPercentage = true;
					result.RevenueAccount = true;
					result.WIPAccount = true;
					break;
				case Constants.ChargeType.Disbursement:
					result.AccrualAccount = true;
					result.CostAccount = true;
					result.MarginPercentage = true;
					result.RevenueAccount = true;
					result.WIPAccount = true;
					result.DisbursementSurplusAccount = true;
					result.DisbursementShortfallAccount = true;
					break;
				case Constants.ChargeType.ManualJobAccrual:
					result.AccrualAccount = true;
					result.CostAccount = true;
					result.MarginPercentage = false;
					result.RevenueAccount = true;
					result.WIPAccount = true;
					break;

				case Constants.ChargeType.Revenue:
					result.AccrualAccount = false;
					result.CostAccount = false;
					result.MarginPercentage = false;
					result.RevenueAccount = true;
					result.WIPAccount = true;
					break;

				case Constants.ChargeType.Overhead:
					result.AccrualAccount = false;
					result.CostAccount = true;
					result.MarginPercentage = false;
					result.RevenueAccount = false;
					result.WIPAccount = false;
					break;

				case Constants.ChargeType.NonAccrual:
					result.AccrualAccount = false;
					result.CostAccount = true;
					result.MarginPercentage = false;
					result.RevenueAccount = true;
					result.WIPAccount = false;
					break;

				case Constants.ChargeType.Comment:
					result.AccrualAccount = false;
					result.CostAccount = false;
					result.MarginPercentage = false;
					result.RevenueAccount = false;
					result.WIPAccount = false;
					break;

				default:
					result.IsValid = false;
					break;
			}

			return result;
		}

		public struct PropertyRequired
		{
			public bool MarginPercentage;
			public bool RevenueAccount;
			public bool WIPAccount;
			public bool CostAccount;
			public bool AccrualAccount;
			public bool DisbursementSurplusAccount;
			public bool DisbursementShortfallAccount;

			public bool IsValid;
		}

		#endregion

		#region Related Business Objects

		#region RevenueRecOverrides

		[ChildEditable(true)]
		public AccChargeRevRecOverrideCollection RevenueRecOverrides
		{
			get
			{
				if (fRevenueRecOverrides == null)
				{
					fRevenueRecOverrides = new AccChargeRevRecOverrideCollection(this);
					fRevenueRecOverrides.Load();
					RegisterEditableChildObject(fRevenueRecOverrides);
				}
				return fRevenueRecOverrides;
			}
		}
		AccChargeRevRecOverrideCollection fRevenueRecOverrides;

		#endregion

		#region SupplyTypeOverrides

		[ChildEditable(true)]
		public AccChargeSupplyTypeOverrideCollection SupplyTypeOverrides
		{
			get
			{
				if (fSupplyTypeOverrides == null)
				{
					fSupplyTypeOverrides = new AccChargeSupplyTypeOverrideCollection(this);
					fSupplyTypeOverrides.Load();
					RegisterEditableChildObject(fSupplyTypeOverrides);
				}
				return fSupplyTypeOverrides;
			}
		}
		AccChargeSupplyTypeOverrideCollection fSupplyTypeOverrides;

		#endregion

		#region GLPostingOverrides

		[ChildEditable(true)]
		public AccChargeGLPostingOverrideCollection GLPostingOverrides
		{
			get
			{
				if (fGLPostingOverrides == null)
				{
					fGLPostingOverrides = new AccChargeGLPostingOverrideCollection(this);
					fGLPostingOverrides.Load();
					RegisterEditableChildObject(fGLPostingOverrides);
				}
				return fGLPostingOverrides;
			}
		}
		AccChargeGLPostingOverrideCollection fGLPostingOverrides;

		public struct GLPostingAccounts
		{
			public GLPostingAccounts(ZGuid accrualAccount, ZGuid costAccount, ZGuid revenueAccount, ZGuid wIPAccount, ZGuid costClearingAccount, ZGuid revenueClearingAccount)
				: this()
			{
				this.AccrualAccount = accrualAccount;
				this.CostAccount = costAccount;
				this.RevenueAccount = revenueAccount;
				this.WIPAccount = wIPAccount;
				this.CostClearingAccount = costClearingAccount;
				this.RevenueClearingAccount = revenueClearingAccount;
			}

			public ZGuid AccrualAccount { get; private set; }
			public ZGuid CostAccount { get; private set; }
			public ZGuid RevenueAccount { get; private set; }
			public ZGuid WIPAccount { get; private set; }
			public ZGuid CostClearingAccount { get; private set; }
			public ZGuid RevenueClearingAccount { get; private set; }
		}

		public GLPostingAccounts GetGLPostingAccounts(GlbDepartment department, OrgHeader header, ZString jobType, ZString direction, ZString transportMode, ZString consolContainerMode, ZString masterPaymentType, ZString housePaymentType)
		{
			var accChargeGlPostingOverride = FindGLPostingOverride(department, header, jobType, transportMode, direction, consolContainerMode, masterPaymentType, housePaymentType);

			return accChargeGlPostingOverride == null
				? new GLPostingAccounts(AC_AG_AccrualAccount, AC_AG_CostAccount, AC_AG_RevenueAccount, AC_AG_WIPAccount, AC_AG_CostClearingAccount, AC_AG_RevenueClearingAccount)
				: new GLPostingAccounts(
					GetGlPostingOverrideAccountOrDefault(accChargeGlPostingOverride.Y1_AG_ACR, AC_AG_AccrualAccount),
					GetGlPostingOverrideAccountOrDefault(accChargeGlPostingOverride.Y1_AG_CST, AC_AG_CostAccount),
					GetGlPostingOverrideAccountOrDefault(accChargeGlPostingOverride.Y1_AG_REV, AC_AG_RevenueAccount),
					GetGlPostingOverrideAccountOrDefault(accChargeGlPostingOverride.Y1_AG_WIP, AC_AG_WIPAccount),
					GetGlPostingOverrideAccountOrDefault(accChargeGlPostingOverride.Y1_AG_CST_Clearing, AC_AG_CostClearingAccount),
					GetGlPostingOverrideAccountOrDefault(accChargeGlPostingOverride.Y1_AG_REV_Clearing, AC_AG_RevenueClearingAccount));

			ZGuid GetGlPostingOverrideAccountOrDefault(ZGuid overrideAccount, ZGuid defaultAccount)
			{
				return overrideAccount != ZGuid.Empty ? overrideAccount : defaultAccount;
			}
		}

		AccChargeGLPostingOverride FindGLPostingOverride(GlbDepartment department, OrgHeader org, ZString jobType, ZString transportMode, ZString direction, ZString consolContainerMode, ZString masterPaymentType, ZString housePaymentType)
		{
			var categoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			if (org != null)
			{
				categoryClass = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.Cast<CodeDescriptionWithGroup>().FirstOrDefault(x => x.Code == org.CompanyData.OB_ARConsolidatedAccountingCategory)?.Group ?? categoryClass;
			}

			return GLPostingOverrides.GetGLPostingOverride(categoryClass, department, jobType, transportMode, direction, consolContainerMode, masterPaymentType, housePaymentType);
		}

		#endregion

		#region ChargeComplianceDescriptions

		[ChildEditable(true)]
		public AccChargeComplianceDescriptionCollection ChargeComplianceDescriptions
		{
			get
			{
				if (chargeComplianceDescriptions == null)
				{
					chargeComplianceDescriptions = new AccChargeComplianceDescriptionCollection(this);
					RegisterEditableChildObject(chargeComplianceDescriptions);
					chargeComplianceDescriptions.SetReadOnlyIncludingChildren(!Env.Security.ChargeCodesSellComplianceDescription.IsAllowed);
				}
				return chargeComplianceDescriptions;
			}
		}
		AccChargeComplianceDescriptionCollection chargeComplianceDescriptions;

		#endregion

		#region GST Tax Overrides

		[ChildEditable(true)]
		public AccChargeTaxOverrideCollection TaxOverrides
		{
			get
			{
				if (fTaxOverrides == null)
				{
					fTaxOverrides = new AccChargeTaxOverrideCollection(this);
					fTaxOverrides.Load();
					RegisterEditableChildObject(fTaxOverrides);
				}
				return fTaxOverrides;
			}
		}
		AccChargeTaxOverrideCollection fTaxOverrides;

		public ZBool HasTaxOverrides
		{
			get { return TaxOverrides.Count > 0; }
		}

		internal bool HasTaxOverrides_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo HasTaxOverridesInfo
		{
			get { return GetZPropertyInfo(nameof(HasTaxOverrides)); }
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public AccTaxRate GetGSTRate(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters, out ZGuid overrideInvTaxMsg)
		{
			var gstRateTuple = GetGSTRateTuple(parameters);
			overrideInvTaxMsg = gstRateTuple.OverrideInvTaxMsg;
			return gstRateTuple.TaxRate;
		}

		public (AccTaxRate TaxRate, ZGuid OverrideInvTaxMsg, AccChargeTaxOverride TaxOverride) GetGSTRateTuple(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters)
		{
			var overrideInvTaxMsg = ZGuid.Empty;
			if (this.AC_ChargeType == Constants.ChargeType.Comment)
			{
				return (null, overrideInvTaxMsg, null);
			}

			if (parameters.Organisation != null
				&& !IsGlobal
				&& (IsOrgPartOfInterOfficeBillingGroup(parameters)
					|| IsInterBranchPostingWithSameTaxRegistration(parameters)))
			{
				AccTaxRate notReportTaxRate = AccTaxRate.GetNOTREPORTTaxID(Factory, Company);
				if (notReportTaxRate != null)
				{
					overrideInvTaxMsg = ObjectFactory.Get<IAccounting>().GroupMemberBillingDefaultInvoiceTaxMessage;
					if (!overrideInvTaxMsg.IsValid)
					{
						overrideInvTaxMsg = notReportTaxRate.AT_A9_DefaultVatClass;
					}
					return (notReportTaxRate, overrideInvTaxMsg, null);
				}
			}

			parameters.Direction = this.RecalculateDirectionBasedOnOriginAndDestination(parameters);

			string key = GetCacheKey(parameters);
			if (GSTRateCache.ContainsKey(key))
			{
				var gstRate = GSTRateCache[key];
				overrideInvTaxMsg = gstRate.OverrideTaxInvMsg;
				return (gstRate.TaxRate, overrideInvTaxMsg, gstRate.TaxOverride);
			}

			AccTaxRate result = null;
			var taxRule = this.GetChargeTaxOverride(parameters);

			if (taxRule == null && TaxOverrideGroup != null)
			{
				taxRule = TaxOverrideGroup.GetChargeTaxOverride(parameters);
			}

			if (taxRule != null)
			{
				result = taxRule.TaxRate;
				overrideInvTaxMsg = taxRule.AO_A9_DefaultVATClass;
			}
			else if (!IsGlobal && Company.Country.IsPartOfEuropeanUnion)
			{
				RefCountry originCountry = parameters.Origin == null ? null : parameters.Origin.Country;
				RefCountry destinationCountry = parameters.Destination == null ? null : parameters.Destination.Country;
				result = GetGSTRateForCompanyInEuropeanUnion(parameters.CostOrSell, parameters.Organisation, originCountry, destinationCountry);
				if (result != null)
				{
					overrideInvTaxMsg = result.AT_A9_DefaultVatClass;
				}
			}

			if (result == null)
			{
				result = GSTRate;
				overrideInvTaxMsg = GSTRate != null ? GSTRate.AT_A9_DefaultVatClass : ZGuid.Empty;
			}

			GSTRateCache.Add(key, new TaxRateWithOverrideInvMsg(result, overrideInvTaxMsg, taxRule));

			return (result, overrideInvTaxMsg, taxRule);
		}

		bool IsOrgPartOfInterOfficeBillingGroup(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters) =>
			ObjectFactory.Get<IAccounting>().OverrideInterOfficeBillingTaxIDToNOTREPORT
			&& parameters.Organisation.IsInterOfficeBillingOrgNotReportableForTax
			&& !(ObjectFactory.Get<IAccounting>().OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy
				&& IsOrgProxyOfActiveBranch(parameters));

		bool IsInterBranchPostingWithSameTaxRegistration(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters) =>
			ObjectFactory.Get<IAccounting>().OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy
			&& IsOrgProxyOfActiveBranch(parameters)
			&& IsBranchTaxDetailsSameAsOrg(parameters);

		bool IsOrgProxyOfActiveBranch(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters) =>
			parameters.Branch?.Company?.ActiveBranches?.Any(b => b.GB_OH_OrgProxy == parameters.Organisation.PK) ?? false;

		bool IsBranchTaxDetailsSameAsOrg(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters) => !string.IsNullOrEmpty(parameters.Branch.OrgProxy?.RawTaxRegistrationNumber)
			&& !parameters.Organisation.RawTaxRegistrationNumber.IsEmpty
			&& new Regex(@"\s").Replace(parameters.Branch.OrgProxy.RawTaxRegistrationNumber.ToLower(), string.Empty)
				== new Regex(@"\s").Replace(parameters.Organisation.RawTaxRegistrationNumber.ToLower(), string.Empty);

		Dictionary<string, TaxRateWithOverrideInvMsg> GSTRateCache = new Dictionary<string, TaxRateWithOverrideInvMsg>();

		class TaxRateWithOverrideInvMsg
		{
			internal AccTaxRate TaxRate { get; set; }
			internal ZGuid OverrideTaxInvMsg { get; set; }
			internal AccChargeTaxOverride TaxOverride { get; set; }

			public TaxRateWithOverrideInvMsg(AccTaxRate taxRate, ZGuid overrideTaxInvMsg, AccChargeTaxOverride taxOverride)
			{
				this.TaxRate = taxRate;
				this.OverrideTaxInvMsg = overrideTaxInvMsg;
				this.TaxOverride = taxOverride;
			}
		}

#if DEBUG

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Adapter method for tests to avoid many test changes.")]
		public AccTaxRate GetGSTRateForTestOnly(ZString incoTerm, CostSell costOrSell, ZString jobType, Directions direction, ZString transportMode,
			OrgHeader organisation, ILocation origin, ILocation destination, GlbBranch branch, ILocation fixedPlaceOfSupply, ZString supplyType, out ZGuid overrideInvTaxMsg) =>
			GetGSTRateForTestOnly(incoTerm, costOrSell, jobType, direction, transportMode, organisation, origin, destination, branch, null, null, fixedPlaceOfSupply, supplyType, out overrideInvTaxMsg);

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Adapter method for tests to avoid many test changes.")]
		public AccTaxRate GetGSTRateForTestOnly(ZString incoTerm, CostSell costOrSell, ZString jobType, Directions direction, ZString transportMode,
			OrgHeader organisation, ILocation origin, ILocation destination, GlbBranch branch, ZString customsStatus, ZString communityTransitStatus, ILocation fixedPlaceOfSupply, ZString supplyType, out ZGuid overrideInvTaxMsg)
		{
			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = incoTerm,
				CostOrSell = costOrSell,
				JobType = jobType,
				Direction = direction,
				TransportMode = transportMode,
				Organisation = organisation,
				Origin = origin,
				Destination = destination,
				CustomsStatus = customsStatus,
				CommunityTransitStatus = communityTransitStatus,
				FixedPlaceOfSupply = fixedPlaceOfSupply,
				Branch = branch,
				SupplyType = supplyType,
			};
			return GetGSTRate(parameters, out overrideInvTaxMsg);
		}

		public void ClearGSTRateCacheForTesting()
		{
			GSTRateCache = new Dictionary<string, TaxRateWithOverrideInvMsg>();
		}
#endif

		string GetCacheKey(AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters)
		{
			return FormattableString.Invariant($"{parameters.IncoTerm}|{parameters.CostOrSell}|{parameters.JobType}|{parameters.Direction}|{parameters.TransportMode}|{parameters.Organisation?.OH_Code ?? ZString.Empty}|{parameters.Origin?.Code ?? ZString.Empty}|{parameters.Destination?.Code ?? ZString.Empty}|{parameters.Branch?.GB_Code ?? ZString.Empty}|{parameters.CustomsStatus}|{parameters.CommunityTransitStatus}|{parameters.FixedPlaceOfSupply?.Code ?? ZString.Empty}|{parameters.TransactionContext}|{parameters.SupplyType}");
		}

		internal AccTaxRate GetGSTRateForCompanyInEuropeanUnion(CostSell costOrSell, OrgHeader organisation, RefCountry origin, RefCountry destination)
		{
			string originCode = string.Empty;
			string destinationCode = string.Empty;
			string taxRegistrationCode = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry;

			RefCountry countryOfTaxRegistration = organisation != null && !organisation.RawTaxRegistrationNumber.IsEmpty ? organisation.CountryOfTaxRegistration : null;
			if (!IsGlobal)
			{
				if (countryOfTaxRegistration != null)
				{
					taxRegistrationCode = countryOfTaxRegistration.Code == Company.GC_RN_NKCountryCode ? EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry : countryOfTaxRegistration.IsPartOfEuropeanUnion ? EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry : EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry;
				}

				if (origin != null)
				{
					if (origin.Code == Company.GC_RN_NKCountryCode)
					{
						originCode = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry;
					}
					else
					{
						originCode = origin.IsPartOfEuropeanUnion ? EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry : EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry;
					}

					if (destination != null)
					{
						if (origin.IsPartOfEuropeanUnion)
						{
							if (destination.PK == origin.PK)
							{
								destinationCode = EUTaxIDDefaultingRule.OriginDestinationCode.SameAsOrigin;
							}
							else if (destination.Code == Company.GC_RN_NKCountryCode)
							{
								destinationCode = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry;
							}
							else
							{
								destinationCode = destination.IsPartOfEuropeanUnion ? EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry : EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry;
							}
						}
						else
						{
							destinationCode = destination.IsPartOfEuropeanUnion ? destination.Code == Company.GC_RN_NKCountryCode ? EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry : EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry : EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry;
						}
					}
				}
			}

			var jobDirectionCode = EUTaxIDDefaultingRule.GetJobDirectionCode(originCode, destinationCode);
			var cachedKey = $"{costOrSell}_{jobDirectionCode}_{taxRegistrationCode}";
			return Factory.GetCachedValue(cachedKey, () => { return AccChargeCodeRegistry.Instance.EUTaxIDDefaulting.Value.GetRate(costOrSell, jobDirectionCode, taxRegistrationCode); }, CacheStalenessPolicy.StaleOnFactorySave);
		}

		#endregion

		#region Charge Type Overrides

		[ChildEditable(true)]
		public AccChargeTypeOverrideCollection ChargeTypeOverrides
		{
			get
			{
				if (fChargeTypeOverrides == null)
				{
					fChargeTypeOverrides = new AccChargeTypeOverrideCollection(this);
					fChargeTypeOverrides.Load();
					RegisterEditableChildObject(fChargeTypeOverrides);
				}
				fChargeTypeOverrides.SetReadOnlyIncludingChildren(IsDeleted || AC_ChargeType == Constants.ChargeType.Overhead || AC_ChargeType == Constants.ChargeType.Comment || AC_ChargeType == Constants.ChargeType.NonAccrual);
				return fChargeTypeOverrides;
			}
		}
		AccChargeTypeOverrideCollection fChargeTypeOverrides;

		public ZBool HasTypeOverrides
		{
			get { return ChargeTypeOverrides.Count > 0; }
		}

		internal bool HasTypeOverrides_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo HasTypeOverridesInfo
		{
			get { return GetZPropertyInfo(nameof(HasTypeOverrides)); }
		}

		public IAccChargeTypeOverride GetChargeType(JobInvoicingConsumerType jobType, Directions direction)
		{
			return ChargeTypeOverrides.GetChargeType(jobType, direction);
		}

		#endregion

		#region BranchOverrides

		[ChildEditable(true)]
		public AccChargeBranchOverrideCollection BranchOverrides
		{
			get
			{
				if (branchOverrides == null)
				{
					branchOverrides = new AccChargeBranchOverrideCollection(this);
					RegisterEditableChildObject(branchOverrides);
				}
				return branchOverrides;
			}
		}
		AccChargeBranchOverrideCollection branchOverrides;

		public GlbBranch GetOverriddenBranch(JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, Func<ZString, OrgHeader> getOrganisationByBranchDefaultingRule)
		{
			GlbBranch resultBranch = null;
			var branchOverride = BranchOverrides.GetChargeBranchOverride(jobType, direction, transportMode);
			if (branchOverride != null)
			{
				resultBranch = branchOverride.GetBranch(getOrganisationByBranchDefaultingRule);
			}

			return resultBranch;
		}

		#endregion

		#region CreditorOverrides

		[ChildEditable(true)]
		public AccChargeCreditorOverrideCollection CreditorOverrides
		{
			get
			{
				if (creditorOverrides == null)
				{
					creditorOverrides = new AccChargeCreditorOverrideCollection(this);
					RegisterEditableChildObject(CreditorOverrides);
				}
				return creditorOverrides;
			}
		}
		AccChargeCreditorOverrideCollection creditorOverrides;

		#endregion

		#region Airline IATA Code overrides

		[ChildEditable(true)]
		public AccChargeCodeCarrierIataMappingCollection AccChargeCodeCarrierIataMappings
		{
			get
			{
				if (accChargeCodeCarrierIataMappings == null)
				{
					accChargeCodeCarrierIataMappings = new AccChargeCodeCarrierIataMappingCollection(this);
					RegisterEditableChildObject(accChargeCodeCarrierIataMappings);
				}

				return accChargeCodeCarrierIataMappings;
			}
		}
		AccChargeCodeCarrierIataMappingCollection accChargeCodeCarrierIataMappings;

		public ZString GetIATACodeWithFallback(ZGuid orgHeaderPK)
		{
			var accChargeCodeCarrierIataMapping = AccChargeCodeCarrierIataMappings.Cast<AccChargeCodeCarrierIataMapping>().FirstOrDefault(x => x.ACI_OH_Carrier == orgHeaderPK);
			if (accChargeCodeCarrierIataMapping != null && !accChargeCodeCarrierIataMapping.ACI_IATAChargeCodeMap.IsEmpty)
			{
				return accChargeCodeCarrierIataMapping.ACI_IATAChargeCodeMap;
			}

			if (!AC_IATA_ChargeCodeMap.IsEmpty)
			{
				return AC_IATA_ChargeCodeMap;
			}

			if (!IsGlobal && GlobalChargeCode != null)
			{
				var globalChargeCodeCarrierIataMapping = GlobalChargeCode.AccChargeCodeCarrierIataMappings.FirstOrDefault(x => x.ACI_OH_Carrier == orgHeaderPK);
				if (globalChargeCodeCarrierIataMapping != null && !globalChargeCodeCarrierIataMapping.ACI_IATAChargeCodeMap.IsEmpty)
				{
					return globalChargeCodeCarrierIataMapping.ACI_IATAChargeCodeMap;
				}

				return GlobalChargeCode.AC_IATA_ChargeCodeMap;
			}

			return ZString.Empty;
		}

		#endregion

		#region Universal Charge Code Mappings
		[ChildEditable(true)]
		public AccChargeCodeUniversalCodeMappingCollection UniversalChargeCodeMappingsCollection
		{
			get
			{
				if (fUniversalChargeCodeMappingsCollection == null)
				{
					fUniversalChargeCodeMappingsCollection = new AccChargeCodeUniversalCodeMappingCollection(this);
					fUniversalChargeCodeMappingsCollection.Load();
					RegisterEditableChildObject(fUniversalChargeCodeMappingsCollection);
				}
				return fUniversalChargeCodeMappingsCollection;
			}
		}
		AccChargeCodeUniversalCodeMappingCollection fUniversalChargeCodeMappingsCollection;

		public ZString UniversalChargeCodeMappings
		{
			get
			{
				return string.Join(", ", UniversalChargeCodeMappingsCollection.Select(m => m.AUP_Code));
			}
		}
		#endregion

		#region Place Of Supply Group

		// Unique index FK_UX__GRO_GC_GRO_GroupType_GRO_Code guarantees zero or one records.
		// That is, a Charge Code cannot be a member of multiple groups.
		public AccPOSChargeCodeGroup PlaceOfSupplyGroup
			=> Factory.LoadTop1<AccPOSChargeCodeGroupPivot>(new ZQuery(AccPOSChargeCodeGroupPivotViewSchema.GRP_MemberID, PK))?.ChargeCodeGroup;

		#endregion

		public ZString GetMappingForOrganisation(ZGuid organisationPK)
		{
			return OrgPatternMatchOverride.GetMappingForOrganisationByLocalCode(organisationPK, Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, AC_Code, Factory);
		}

		internal BusinessObjectFactory ReadOnlyFactory
		{
			get { return Factory.GetCachedReadOnlyFactory(); }
		}

		#region PlaceOfSupplyConfigurations

		[ChildEditable(true)]
		public AccPOSConfigurationCollection PlaceOfSupplyConfigurations
		{
			get
			{
				if (PlaceOfSupplyConfigurations_Internal == null)
				{
					var localPlaceOfSupplyConfigurations_Internal = IsGlobal
															? new AccPOSConfigurationCollection(Factory)
															: new AccPOSConfigurationCollection(this);
					localPlaceOfSupplyConfigurations_Internal.Load();
					PlaceOfSupplyConfigurations_Internal = localPlaceOfSupplyConfigurations_Internal;
					RegisterEditableChildObject(PlaceOfSupplyConfigurations_Internal);
				}
				return PlaceOfSupplyConfigurations_Internal;
			}
		}
		AccPOSConfigurationCollection PlaceOfSupplyConfigurations_Internal;

		#endregion

		#region GovtChargeCodeOverrides

		[ChildEditable(true)]
		public AccChargeGovtChargeCodeOverrideCollection GovtChargeCodeOverrides
		{
			get
			{
				if (govtChargeCodeOverride == null)
				{
					govtChargeCodeOverride = new AccChargeGovtChargeCodeOverrideCollection(this);
					govtChargeCodeOverride.Load();
					RegisterEditableChildObject(govtChargeCodeOverride);
				}
				return govtChargeCodeOverride;
			}
		}
		AccChargeGovtChargeCodeOverrideCollection govtChargeCodeOverride;

		public ZString GetFallbackGovtChargeCode(ConfigurationMatcherHelper.ConfigurationMatcherParameters parameters)
		{
			var matchResult = GovtChargeCodeOverrides.GetGovtChargeCode(Factory, PK, parameters);
			return matchResult == null
				? AC_GovtChargeCode
				: matchResult.ACG_GovtChargeCode;
		}

		#endregion

		#region ApportionmentMethodOverrides

		[ChildEditable(true)]
		public AccChargeApportionmentMethodOverrideCollection ApportionmentMethodOverrides
		{
			get
			{
				if (apportionmentMethodOverrides == null)
				{
					apportionmentMethodOverrides = new AccChargeApportionmentMethodOverrideCollection(this);
					apportionmentMethodOverrides.Load();
					RegisterEditableChildObject(apportionmentMethodOverrides);
				}
				return apportionmentMethodOverrides;
			}
		}
		AccChargeApportionmentMethodOverrideCollection apportionmentMethodOverrides;

		#endregion

		#endregion

		#region Saving

		protected override void RunPreSaveValidationCore()
		{
			this.ClearValueCachedForValidation();
			base.RunPreSaveValidationCore();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (ShouldDeleteClientSequenceSetups)
			{
				DeleteClientSequenceSetups();
			}
			if (OrganisationsDataRegistry.Instance.UpdateEDICodeMappingwhenChargeCodeIsRenamed.Value)
			{
				UpdateEDICodeMapping();
			}
		}

		void UpdateEDICodeMapping()
		{
			if ((ZString)AC_CodeInfo.OriginalValue != AC_Code)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.ChargeCodes);
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalCode, AC_CodeInfo.OriginalValue);
				OrgPatternMatchOverride[] matchOverrides = Factory.Load<OrgPatternMatchOverride>(query);
				foreach (OrgPatternMatchOverride matchOverride in matchOverrides)
				{
					matchOverride.OO_LocalCode = AC_Code;
				}
			}
		}

		bool ShouldDeleteClientSequenceSetups
		{
			get { return ZBool.True.Equals(AC_IsActiveInfo.OriginalValue) && !AC_IsActive; }
		}

		public void DeleteClientSequenceSetups()
		{
			ZQuery filter = new ZQuery(AccClientInvoiceOrderSchema.AI_AC, this.PK);
			AccClientInvoiceOrder[] setups = Factory.Load<AccClientInvoiceOrder>(filter);
			foreach (AccClientInvoiceOrder setup in setups)
			{
				setup.Delete();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (IsGlobal && HasChanges)
			{
				if (!IsInDatabase)
				{
					OnSavingNewGlobalChargeCode();
				}
				else
				{
					OnSavingExistingGlobalChargeCode();
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded && IsGlobal)
			{
				SnapshotGlobalChargeCodeCollections();

				if (childChargeCodes != null && childChargeCodes.Code != (ZString)this.AC_CodeInfo.OriginalValue)
				{
					childChargeCodes = null;
				}
			}
			base.OnFactorySaved(saveSucceeded);
		}

		#endregion

		#region Global Charge Code Saving

		#region Public Methods

		public void CopyToCompany(GlbCompany company)
		{
			CopyToCompanyCore(this, company.PK, company.GC_IsGSTRegistered);
		}

		#endregion

		#region Saving

		void OnSavingNewGlobalChargeCode()
		{
			try
			{
				CreateChargeCodesForNewGlobalChargeCode(CreateTrialRunChargeCode());
			}
			catch (ValidationOnLocalChargeCodeException ex)
			{
				ex.OnTrialRun = true;
				throw;
			}
			CreateChargeCodesForNewGlobalChargeCode(this);
		}

		void OnSavingExistingGlobalChargeCode()
		{
			try
			{
				UpdateChargeCodesForExistingGlobalChargeCode(CreateTrialRunChargeCode());
			}
			catch (ValidationOnLocalChargeCodeException ex)
			{
				ex.OnTrialRun = true;
				throw;
			}
			UpdateChargeCodesForExistingGlobalChargeCode(this);
		}

		AccChargeCode CreateTrialRunChargeCode()
		{
			var factoryForTrialRun = new BusinessObjectFactory();
			if (Factory.IsValidationSuspended)
			{
				factoryForTrialRun.SuspendValidation();
			}
			factoryForTrialRun.RefreshEnabled = false;
			return (AccChargeCode)factoryForTrialRun.ImportFromAnotherFactory(this);
		}

		#endregion

		#region Static Definitions

		static readonly Func<IBusinessObjectInternals, IBusinessObjectInternals, DataColumn, bool> globalToLocalCopyDecider = (IBusinessObjectInternals globalChargeCode, IBusinessObjectInternals localChargeCode, DataColumn column) =>
		{
			if (column.ColumnName == AccChargeCodeSchema.AC_MarginPercentage.Name)
			{
				object globalOriginalChargeType = globalChargeCode.GetColumnOriginalValue(AccChargeCodeSchema.AC_ChargeType.Name);
				object localOriginalChargeType = localChargeCode.GetColumnOriginalValue(AccChargeCodeSchema.AC_ChargeType.Name);

				if (!globalOriginalChargeType.Equals(localOriginalChargeType))
				{
					return false;
				}
			}

			object globalOriginal = globalChargeCode.GetColumnOriginalValue(column.ColumnName);
			object globalCurrent = globalChargeCode.GetValueFromRowSafely(column, DataRowVersion.Current);
			object localOriginal = localChargeCode.GetColumnOriginalValue(column.ColumnName);
			return !globalOriginal.Equals(globalCurrent) && globalOriginal.Equals(localOriginal);
		};

		static Func<IBusinessObjectInternals, IBusinessObjectInternals, DataColumn, bool> globalToLocalAirlineCopyDecider =>
			(IBusinessObjectInternals globalAirlineChargeCode, IBusinessObjectInternals localAirlineChargeCode, DataColumn column) =>
			{
				object globalOriginal = globalAirlineChargeCode.GetColumnOriginalValue(column.ColumnName);
				object globalCurrent = globalAirlineChargeCode.GetValueFromRowSafely(column, DataRowVersion.Current);
				object localOriginal = localAirlineChargeCode.GetColumnOriginalValue(column.ColumnName);

				var globalValueChanged = globalOriginal == null
					? globalOriginal != globalCurrent
					: !globalOriginal.Equals(globalCurrent);
				var hasSameOriginalValue = globalOriginal == null
					? globalOriginal == localOriginal
					: globalOriginal.Equals(localOriginal);
				return globalValueChanged && hasSameOriginalValue;
			};

		internal static HashSet<string> ColumnNamesToExcludeFromGlobalChargeCodeCopy
		{
			get
			{
				return new HashSet<string> {
					AccChargeCodeSchema.Constants.PK,
					AccChargeCodeSchema.Constants.AC_AT_GSTRate,
					AccChargeCodeSchema.Constants.AC_AW_WithholdingTaxRate,
					AccChargeCodeSchema.Constants.AC_GC,
					AccChargeCodeSchema.Constants.AC_AX_TaxOverrideGroup,
					AccChargeCodeSchema.Constants.AC_InputGSTVATRecoverable,
					AccChargeCodeSchema.Constants.AC_LocalLanguageDescription,
					AccChargeCodeSchema.Constants.AC_SystemCreateTimeUtc,
					AccChargeCodeSchema.Constants.AC_SystemCreateUser,
					AccChargeCodeSchema.Constants.AC_SystemLastEditTimeUtc,
					AccChargeCodeSchema.Constants.AC_SystemLastEditUser,
				};
			}
		}

		static HashSet<string> ColumnNamesToExcludeFromGlobalAirlineChargeCodeCopy
		{
			get
			{
				return new HashSet<string> {
					AccChargeCodeCarrierIataMappingSchema.Constants.PK,
					AccChargeCodeCarrierIataMappingSchema.Constants.ACI_AC_ChargeCode,
					AccChargeCodeCarrierIataMappingSchema.Constants.ACI_SystemCreateTimeUtc,
					AccChargeCodeCarrierIataMappingSchema.Constants.ACI_SystemCreateUser,
					AccChargeCodeCarrierIataMappingSchema.Constants.ACI_SystemLastEditTimeUtc,
					AccChargeCodeCarrierIataMappingSchema.Constants.ACI_SystemLastEditUser,
				};
			}
		}

		#endregion

		#region Copying

		static void CreateChargeCodesForNewGlobalChargeCode(AccChargeCode globalChargeCode)
		{
			var companiesWithoutChargeCode = new DynamicBusinessObjectCollection(globalChargeCode.Factory);

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@ChargeCode", globalChargeCode.AC_Code, AccChargeCodeSchema.AC_Code },
				{ "@True", new ZBool(true), GlbCompanySchema.GC_IsActive },
				{ "@DemoCompanyCode", new ZString(GlbCompany.DemoCompanyCode), GlbCompanySchema.GC_Code }
			};

			companiesWithoutChargeCode.Load(
						$@"SELECT 
							{GlbCompanySchema.Constants.PK}, 
							{GlbCompanySchema.Constants.GC_Code},
							{GlbCompanySchema.Constants.GC_RN_NKCountryCode}, 
							{GlbCompanySchema.Constants.GC_IsGSTRegistered}
						FROM 
							{GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName} 
						WHERE 
							{GlbCompanySchema.Constants.PK} NOT IN (
									SELECT 
										{AccChargeCodeSchema.Constants.AC_GC} 
									FROM 
										{AccChargeCodeSchema.Constants.SqlSchemaName}.{AccChargeCodeSchema.Constants.TableName} 
									WHERE 
										{AccChargeCodeSchema.Constants.AC_Code} = @ChargeCode 
										AND {AccChargeCodeSchema.Constants.AC_GC} IS NOT NULL) 
							AND {GlbCompanySchema.Constants.GC_IsActive} = @True 
							AND {GlbCompanySchema.Constants.GC_Code} <> @DemoCompanyCode"
					, sqlParams);

			foreach (DynamicBusinessObject company in companiesWithoutChargeCode)
			{
				CopyToCompanyCore(
					globalChargeCode,
					new ZGuid(company[GlbCompanySchema.Constants.PK]),
					new ZBool(company[GlbCompanySchema.Constants.GC_IsGSTRegistered]));
			}
		}

		static void CopyToCompanyCore(AccChargeCode globalChargeCode, ZGuid companyPK, ZBool companyIsGSTRegsitered)
		{
			var localChargeCode = (AccChargeCode)globalChargeCode.Clone(new BusinessObjectCloneArgs(ColumnNamesToExcludeFromGlobalChargeCodeCopy));
			localChargeCode.AC_GC = companyPK;

			if (companyIsGSTRegsitered && localChargeCode.AC_ChargeType != Core.Constants.ChargeType.Comment)
			{
				var taxRate = AccTaxRate.Helper.FindTaxRate(globalChargeCode.Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, companyPK.ToGuid());
				if (taxRate != null)
				{
					localChargeCode.AC_AT_GSTRate = taxRate.PK;
				}
			}

			foreach (var globalAirlineCode in globalChargeCode.AccChargeCodeCarrierIataMappings)
			{
				var localAirlineCode = (AccChargeCodeCarrierIataMapping)globalAirlineCode.Clone(new BusinessObjectCloneArgs(ColumnNamesToExcludeFromGlobalAirlineChargeCodeCopy));
				localChargeCode.AccChargeCodeCarrierIataMappings.Add(localAirlineCode);
			}

			foreach (var snapshotMap in GlobalChargeCodeSnapshotMap)
			{
				if (snapshotMap(globalChargeCode).Collection is BusinessObjectCollection bizoCollection)
				{
					bizoCollection.Load();
				}

				snapshotMap(localChargeCode).ImportSnapshot(snapshotMap(globalChargeCode).GetCurrentSnapshot());
			}
			CheckUpdatedChargeCodeIsValidDuringSave(localChargeCode);
		}

		static void UpdateChargeCodesForExistingGlobalChargeCode(AccChargeCode globalChargeCode)
		{
			foreach (AccChargeCode localChargeCode in globalChargeCode.ChildChargeCodes.ToArray())
			{
				if (!localChargeCode.ShouldNotChangeChargeCodeIfHasTransactionPostedInTheCompany())
				{
					CopyChargeCodeFieldsAndCollections(globalChargeCode, localChargeCode);
					CheckUpdatedChargeCodeIsValidDuringSave(localChargeCode);
				}
			}
		}

		static void CopyChargeCodeFieldsAndCollections(AccChargeCode sourceChargeCode, AccChargeCode destinationChargeCode)
		{
			destinationChargeCode.CopyPersistentValuesFrom(sourceChargeCode,
				new BusinessObjectCloneArgs(null, ColumnNamesToExcludeFromGlobalChargeCodeCopy, null, false, globalToLocalCopyDecider));
			CopyAirlineChargeCodeFields(sourceChargeCode, destinationChargeCode);

			foreach (var snapshotMap in GlobalChargeCodeSnapshotMap)
			{
				if (snapshotMap(sourceChargeCode).HasChanged)
				{
					var localCurrent = snapshotMap(destinationChargeCode).GetCurrentSnapshot();
					if (BusinessObjectCollectionCopier.AreEqual(localCurrent, snapshotMap(sourceChargeCode).GetLastSnapshot()))
					{
						snapshotMap(destinationChargeCode).ImportSnapshot(snapshotMap(sourceChargeCode).GetCurrentSnapshot());
					}
				}
			}
		}

		static void CopyAirlineChargeCodeFields(AccChargeCode sourceChargeCode, AccChargeCode destinationChargeCode)
		{
			var matchedAirlineCodePKs = new HashSet<ZGuid>();

			var airlineCodes = destinationChargeCode.AccChargeCodeCarrierIataMappings;
			foreach (var globalAirlineCode in sourceChargeCode.AccChargeCodeCarrierIataMappings)
			{
				var localAirlineCode = airlineCodes.FirstOrDefault(a => a.ACI_OH_Carrier == globalAirlineCode.ACI_OH_Carrier);
				if (localAirlineCode != null)
				{
					localAirlineCode.CopyPersistentValuesFrom(globalAirlineCode,
						new BusinessObjectCloneArgs(localAirlineCode.Factory, ColumnNamesToExcludeFromGlobalAirlineChargeCodeCopy, null, false, globalToLocalAirlineCopyDecider));
					matchedAirlineCodePKs.Add(localAirlineCode.PK);
				}
				else
				{
					var newLocalAirlineCode = (AccChargeCodeCarrierIataMapping)globalAirlineCode.Clone(new BusinessObjectCloneArgs(ColumnNamesToExcludeFromGlobalAirlineChargeCodeCopy));
					airlineCodes.Add(newLocalAirlineCode);
					matchedAirlineCodePKs.Add(newLocalAirlineCode.PK);
				}
			}

			for (var index = destinationChargeCode.AccChargeCodeCarrierIataMappings.Count - 1; index >= 0; index--)
			{
				if (!matchedAirlineCodePKs.Contains(destinationChargeCode.AccChargeCodeCarrierIataMappings[index].PK))
				{
					destinationChargeCode.AccChargeCodeCarrierIataMappings.Delete(destinationChargeCode.AccChargeCodeCarrierIataMappings[index]);
				}
			}
		}

		#endregion

		#region Validation Exception

		[NonSerializedClass]
		public class ValidationOnLocalChargeCodeException : ZCannotSaveException
		{
			public ValidationOnLocalChargeCodeException(string message) : base(message, Res.GetString("da3517fd-6df8-4146-9b1e-c02bd0cf0b8e", "Cannot Save")) { }
			public bool OnTrialRun { get; set; }
		}

		static void CheckUpdatedChargeCodeIsValidDuringSave(AccChargeCode localChargeCode)
		{
			using (localChargeCode.GetGlobalVsLocalValidationSuspender())
			{
				localChargeCode.RunPreSaveValidation();
			}

			if (localChargeCode.HasErrors())
			{
				var globalChargeCodeBreadCrumb = Res.GetString("4600f3d5-5677-488b-85a5-0979770c42c9", @"The invalid charge code '{0}' can be found in Maintain > Account > Global Charge Codes.
Please edit the charge code to correct the error shown above.
To highlight errors to be corrected choose ""File"" then ""Validate All"" after editing the charge code(s).", localChargeCode.AC_Code);

				throw new ValidationOnLocalChargeCodeException(
						Res.GetString("0efba433-654f-453e-9067-2b4b89696357", @"It is not possible to save the Global Charge Code, because the changes made to the Charge Code for company '{0}' would cause these errors:

{1}{2}",
						localChargeCode.Company.GC_Name,
						localChargeCode.GetErrors().ToMessageListString(),
						localChargeCode.Factory.HasContext(BusinessContext.SavingChargeCodeFromCompany) ? "\r\n\r\n" + globalChargeCodeBreadCrumb : null));
			}
		}

		#endregion

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsGlobal)
			{
				ChildChargeCodes.DeleteAll();
			}

			GovtChargeCodeOverrides.RemoveAndDeleteAll();
			ApportionmentMethodOverrides.RemoveAndDeleteAll();
			TaxOverrides.RemoveAndDeleteAll();
			ChargeTypeOverrides.RemoveAndDeleteAll();
			SupplyTypeOverrides.RemoveAndDeleteAll();
			DeleteClientSequenceSetups();
			UniversalChargeCodeMappingsCollection.RemoveAndDeleteAll();
			AccChargeCodeCarrierIataMappings.DeleteAll();
			ChargeComplianceDescriptions.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Global Charge Codes

		public ZBool IsGlobal
		{
			get
			{
				return AC_GC.IsEmpty;
			}
		}

		public AccChargeCode GlobalChargeCode
		{
			get
			{
				if (IsGlobal)
				{
					return null;
				}

				var code = AC_Code;
				if (codeForCachedGlobalChargeCode != code ||
					(cachedGlobalChargeCode != null && cachedGlobalChargeCode.AC_Code != code))
				{
					codeForCachedGlobalChargeCode = code;
					cachedGlobalChargeCode = GetGlobalChargeCodeByCode(Factory, code);
				}

				return cachedGlobalChargeCode;
			}
		}

		ZString codeForCachedGlobalChargeCode;
		AccChargeCode cachedGlobalChargeCode;

		#region IsLinkedToGlobalChargeCode

		[ReadOnlyMember(nameof(IsLinkedToGlobalChargeCode_ReadOnly))]
		public ZBool IsLinkedToGlobalChargeCode
		{
			get
			{
				return GlobalChargeCode != null;
			}
		}

		bool IsLinkedToGlobalChargeCode_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo IsLinkedToGlobalChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(IsLinkedToGlobalChargeCode)); }
		}

		#endregion

		#region IsDifferentToGlobalChargeCode

		public ZBool IsDifferentToGlobalChargeCode
		{
			get
			{
				if (!IsLinkedToGlobalChargeCode)
				{
					return false;
				}

				var globalChargeCode = GlobalChargeCode;
				var localRow = ((IBusinessObjectInternals)this).Row;
				var globalRow = ((IBusinessObjectInternals)globalChargeCode).Row;

				foreach (DataColumn column in localRow.Table.Columns)
				{
					if (!ColumnNamesToExcludeFromGlobalChargeCodeCopy.Contains(column.ColumnName) &&
						!(localRow[column]).Equals(globalRow[column, DataRowVersion.Original]))
					{
						return true;
					}
				}

				foreach (var snapshotMap in GlobalChargeCodeSnapshotMap)
				{
					if (!BusinessObjectCollectionCopier.AreEqual(snapshotMap(this).GetCurrentSnapshot(), snapshotMap(globalChargeCode).GetLastSnapshot()))
					{
						return true;
					}
				}

				return false;
			}
		}

		public ZPropertyInfo IsDifferentToGlobalChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(IsDifferentToGlobalChargeCode)); }
		}

		#endregion

		#region ShowDifferenceWarnings

		ZBool showDifferenceWarnings;

		public ZBool ShowDifferenceWarnings
		{
			get { return showDifferenceWarnings; }
			set
			{
				SetNonPersistentPropertyValue(ShowDifferenceWarningsInfo, ref showDifferenceWarnings, value);
			}
		}

		public ZPropertyInfo ShowDifferenceWarningsInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ShowDifferenceWarnings));
			}
		}

		#endregion

		#region AllowCodeToMatchExisting

		ZBool allowCodeToMatchExisting;

		public ZBool AllowCodeToMatchExisting
		{
			get { return allowCodeToMatchExisting; }
			set
			{
				SetNonPersistentPropertyValue(AllowCodeToMatchExistingInfo, ref allowCodeToMatchExisting, value);
			}
		}

		public ZPropertyInfo AllowCodeToMatchExistingInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AllowCodeToMatchExisting));
			}
		}

		#endregion

		public static AccChargeCode GetGlobalChargeCodeByCode(BusinessObjectFactory factory, ZString codeName)
		{
			ZQuery findGlobal = new ZQuery(AccChargeCodeSchema.AC_Code, codeName);
			findGlobal.AddToFilter(AccChargeCodeSchema.AC_GC, null);
			return factory.LoadTop1<AccChargeCode>(findGlobal);
		}

		#region ChildChargeCodes
		internal AccChargeCodesForGlobalCollection childChargeCodes;
		public AccChargeCodesForGlobalCollection ChildChargeCodes
		{
			get
			{
				if (childChargeCodes == null)
				{
					if (IsGlobal && !((ZString)AC_CodeInfo.OriginalValue).IsEmpty)
					{
						childChargeCodes = new AccChargeCodesForGlobalCollection(Factory, (ZString)AC_CodeInfo.OriginalValue);
						RegisterEditableChildObject(childChargeCodes);
					}
					else
					{
						childChargeCodes = new AccChargeCodesForGlobalCollection(Factory);
					}
				}
				return childChargeCodes;
			}
		}
		#endregion

		#region GlobalChargeCodeSnapshotMap
		static List<Func<AccChargeCode, BusinessObjectCollectionSnapshotManager>> globalChargeCodeSnapshotMap;
		public static List<Func<AccChargeCode, BusinessObjectCollectionSnapshotManager>> GlobalChargeCodeSnapshotMap
		{
			get
			{
				if (globalChargeCodeSnapshotMap == null)
				{
					globalChargeCodeSnapshotMap = new List<Func<AccChargeCode, BusinessObjectCollectionSnapshotManager>>()
					{
						c => c.ChargeTypeOverrides.SnapShotter,
						c => c.RevenueRecOverrides.SnapShotter,
						c => c.GLPostingOverrides.SnapShotter,
						c => c.ApportionmentMethodOverrides.SnapShotter,
						c => c.CreditorOverrides.SnapShotter
					};
				}
				return globalChargeCodeSnapshotMap;
			}
		}

		void SnapshotGlobalChargeCodeCollections()
		{
			GlobalChargeCodeSnapshotMap.ForEach(sc => sc(this).TakeSnapshot());
		}
		#endregion

		#region Global vs Local Validation Suspender

		FunctionalitySuspender GlobalVsLocalValidationSuspender
		{
			get { return globalVsLocalValidationSuspender ?? (globalVsLocalValidationSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender globalVsLocalValidationSuspender;

		public bool IsGlobalVsLocalValidationSupressed
		{
			get { return GlobalVsLocalValidationSuspender.IsSuspended; }
		}

		public IDisposable GetGlobalVsLocalValidationSuspender()
		{
			return GlobalVsLocalValidationSuspender.GetSuspender();
		}

		#endregion

		#endregion

		#region Security Checkpoints

		SecurityCheckpoint RatingAndQuotationsConfigurationCheckpoint
		{
			get
			{
				return IsGlobal ? Env.Security.GlobalChargeCodesRatingAndQuotationsConfiguration :
					IsLinkedToGlobalChargeCode ? Env.Security.ChargeCodesLTGRatingAndQuotationsConfiguration :
					Env.Security.ChargeCodesRatingAndQuotationsConfiguration;
			}
		}

		SecurityCheckpoint ChargeCodesEditGLAccountSetupCheckPoint
		{
			get
			{
				return IsGlobal ? Env.Security.GlobalChargeCodesEditGLAccountSetup :
					IsLinkedToGlobalChargeCode ? Env.Security.ChargeCodesLTGEditGLAccountSetup :
					Env.Security.ChargeCodesEditGLAccountSetup;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ChargeCode);
				}
				return docManagerInfo;
			}
		}
		internal DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			// Needs to be of type AccChargeCodeOnLoaded as this is what the AccChargeCode Form expects
			AccChargeCode copiedChargeCode = Factory.New<AccChargeCode>();
			copiedChargeCode.CopyPersistentValuesFrom(this);

			BusinessObjectCloneArgs taxOverrideArgs = new BusinessObjectCloneArgs(new string[] { AccChargeTaxOverrideSchema.Constants.AO_ParentID, AccChargeTaxOverrideSchema.Constants.AO_ParentTableCode });
			BusinessObjectCloneArgs typeOverridesArgs = new BusinessObjectCloneArgs(new string[] { AccChargeTypeOverrideSchema.Constants.AN_AC_ChargeCode });
			BusinessObjectCloneArgs govtChargeCodeOverridesArgs = new BusinessObjectCloneArgs(new string[] { AccChargeGovtChargeCodeOverrideSchema.Constants.ACG_AC });
			BusinessObjectCloneArgs glPostingOverrideArgs = new BusinessObjectCloneArgs(new string[] { AccChargeGLPostingOverrideSchema.Constants.Y1_AC });
			BusinessObjectCloneArgs revRecOverridesArgs = new BusinessObjectCloneArgs(new string[] { AccChargeRevRecOverrideSchema.Constants.AE_AC });
			BusinessObjectCloneArgs supplyTypeOverridesArgs = new BusinessObjectCloneArgs(new string[] { AccChargeSupplyTypeOverrideSchema.Constants.ACS_ParentID, AccChargeSupplyTypeOverrideSchema.Constants.ACS_ParentTableCode });
			BusinessObjectCloneArgs apportionmentMethodOverrideArgs = new BusinessObjectCloneArgs(new string[] { AccChargeApportionmentMethodOverrideSchema.Constants.AAM_AC });

			foreach (AccChargeTaxOverride taxOverride in TaxOverrides)
			{
				AccChargeTaxOverride copiedOverride = copiedChargeCode.TaxOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(taxOverride, taxOverrideArgs);
			}
			foreach (AccChargeRevRecOverride revRecOverride in RevenueRecOverrides)
			{
				AccChargeRevRecOverride copiedOverride = copiedChargeCode.RevenueRecOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(revRecOverride, revRecOverridesArgs);
			}
			foreach (AccChargeSupplyTypeOverride supplyTypeOverride in SupplyTypeOverrides)
			{
				var copiedOverride = copiedChargeCode.SupplyTypeOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(supplyTypeOverride, supplyTypeOverridesArgs);
			}
			foreach (AccChargeGLPostingOverride glOverride in GLPostingOverrides)
			{
				AccChargeGLPostingOverride copiedOverride = copiedChargeCode.GLPostingOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(glOverride, glPostingOverrideArgs);
			}
			foreach (AccChargeTypeOverride typeOverride in ChargeTypeOverrides)
			{
				AccChargeTypeOverride copiedOverride = copiedChargeCode.ChargeTypeOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(typeOverride, typeOverridesArgs);
			}
			foreach (AccChargeGovtChargeCodeOverride govtChargeCodeOverride in GovtChargeCodeOverrides)
			{
				AccChargeGovtChargeCodeOverride copiedOverride = copiedChargeCode.GovtChargeCodeOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(govtChargeCodeOverride, govtChargeCodeOverridesArgs);
			}
			foreach (AccChargeApportionmentMethodOverride apportionmentMethodOverride in ApportionmentMethodOverrides)
			{
				AccChargeApportionmentMethodOverride copiedOverride = copiedChargeCode.ApportionmentMethodOverrides.AddNew();
				copiedOverride.CopyPersistentValuesFrom(apportionmentMethodOverride, apportionmentMethodOverrideArgs);
			}
			return copiedChargeCode;
		}

		#region Loading

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (IsGlobal)
			{
				SnapshotGlobalChargeCodeCollections();
			}
		}

		#endregion

		public void OnLoadedGUIValidation()
		{
			base.OnLoaded();
			UpdateChargeTypeDependentReadOnlyInfo();
			if (AC_ChargeType == Core.Constants.ChargeType.Comment)
			{
				Validation.ValidateAC_ChargeType();
			}
		}

		#region Properties

		internal bool ShouldUpdateAC_MarginPercentage { get; set; }

		void UpdateAC_MarginPercentage()
		{
			if (RequiredProperties(AC_ChargeType).IsValid && RequiredProperties(AC_ChargeType).MarginPercentage)
			{
				AC_MarginPercentage = 100M;
			}
			else
			{
				AC_MarginPercentage = 0M;
			}
		}

		#endregion

		#region ReadOnly

		internal bool IsPostedTransactionLineReadOnlyInfoUpdated { get; set; }

		public void UpdatePostedTransactionLineReadOnlyInfo()
		{
			if (!IsPostedTransactionLineReadOnlyInfoUpdated)
			{
				IsPostedTransactionLineReadOnlyInfoUpdated = true;

				if (AC_ChargeType == Core.Constants.ChargeType.Comment)
				{
					SetReadOnlyIncludingChildren(true);
				}
			}
		}

		#endregion

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !IsComment && !IsElectronicProcessingChargeCode && !IsReferencedByRegistry; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result;
				if (IsComment)
				{
					result = ResString.GetMultilingualString("A30443A9-76C1-4ED0-A069-866CB20298C2", "The comment charge code '{0}' cannot be deleted", AC_Code);
				}
				else if (IsElectronicProcessingChargeCode)
				{
					result = ResString.GetMultilingualString("4D9121B2-CBD0-444C-9569-2CE711777BC4", "The Charge Code '{0}' is a system defined Charge Code used in Electronic Processing Fee management and cannot be deleted.", AC_Code);
				}
				else
				{
					result = ResString.GetMultilingualString("F6EB316D-9FF9-4B0B-8F18-44CD0F6666CD", "Cannot delete charge code '{0}' because it is being referenced by the following Registry items:\r\n\r\n{1}", AC_Code, new RegistryItemSetLocator().GetFormattedListOfRegistryItemsReferencingPK(PK.ToGuid()));
				}

				return result;
			}
		}

		#endregion

		#region IAccChargeTypeOverride

		ZGuid IAccChargeTypeOverride.AN_AC_ChargeCode
		{
			get { return PK; }
		}

		ZString IAccChargeTypeOverride.AN_ChargeType
		{
			get { return AC_ChargeType; }
		}

		ZString IAccChargeTypeOverride.AN_InvoiceType
		{
			get { return "DEF"; }
		}

		ZString IAccChargeTypeOverride.AN_JobDirection
		{
			get { return "ALL"; }
		}

		ZString IAccChargeTypeOverride.AN_JobType
		{
			get { return "ALL"; }
		}

		ZDecimal IAccChargeTypeOverride.AN_MarginPercentage
		{
			get { return AC_MarginPercentage; }
		}

		bool IAccChargeTypeOverride.HasOveriddenInvoiceType
		{
			get { return false; }
		}

		#endregion

		IReadOnlyList<IDuplicateValidationItem<AccChargeComplianceDescription>> IDuplicateValidationCollectionProvider<AccChargeComplianceDescription>.GetDuplicateValidationCollection() =>
			ChargeComplianceDescriptions.Cast<IDuplicateValidationItem<AccChargeComplianceDescription>>().ToList();

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("5BAAF904-C01C-485D-8BFA-B994DCCB2099", "Charge Code");
				if (!IsDeleted && !AC_Code.IsEmpty)
				{
					result += " (" + AC_Code + ")";
				}

				return result;
			}
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccChargeApportionmentMethodOverrideSchema.AAM_AC, null);
				yield return new AuditChildInfo(AccChargeBranchOverrideSchema.YA_AC_ChargeCode, null);
				yield return new AuditChildInfo(AccChargeCodeCarrierIataMappingSchema.ACI_AC_ChargeCode, null);
				yield return new AuditChildInfo(AccChargeCodeUniversalCodeMappingSchema.AUP_AC, null);
				yield return new AuditChildInfo(AccChargeComplianceDescriptionSchema.ADE_AC, null);
				yield return new AuditChildInfo(AccChargeCreditorOverrideSchema.ACC_AC_ChargeCode, null);
				yield return new AuditChildInfo(AccChargeGLPostingOverrideSchema.Y1_AC, null);
				yield return new AuditChildInfo(AccChargeGovtChargeCodeOverrideSchema.ACG_AC, null);
				yield return new AuditChildInfo(AccChargeRevRecOverrideSchema.AE_AC, null);
				yield return new AuditChildInfo(AccChargeSupplyTypeOverrideSchema.ACS_ParentID, null);
				yield return new AuditChildInfo(AccChargeTaxOverrideSchema.AO_ParentID, null);
				yield return new AuditChildInfo(AccChargeTypeOverrideSchema.AN_AC_ChargeCode, null);
			}
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (AC_GC.IsEmpty)
			{
				AC_GC = Env.CurrentCompany.PK; //do this before base call to avoid creation of new GlbCompany
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			AC_ChargeSubGroup = "";
			SetGLAccountDataForTesting(GLAccountForTesting);
		}

		#region GL Account For Testing

		public void SetGLAccountDataForTesting(AccGLHeader glAccount = null)
		{
			if (!IsInDatabase)
			{
				if (glAccount == null)
				{
					glAccount = GLAccountForTesting;
				}

				if (AC_AG_AccrualAccount.IsEmpty)
				{
					AC_AG_AccrualAccount = glAccount.PK;
				}
				if (AC_AG_CostAccount.IsEmpty)
				{
					AC_AG_CostAccount = glAccount.PK;
				}
				if (AC_AG_RevenueAccount.IsEmpty)
				{
					AC_AG_RevenueAccount = glAccount.PK;
				}
				if (AC_AG_WIPAccount.IsEmpty)
				{
					AC_AG_WIPAccount = glAccount.PK;
				}
			}
		}

		public AccGLHeader GLAccountForTesting
		{
			get { return gLAccountForTesting ?? (gLAccountForTesting = Factory.NewWithValidTestData<AccGLHeader>()); }
		}

		AccGLHeader gLAccountForTesting;

		#endregion

#endif
		#endregion
	}
}
