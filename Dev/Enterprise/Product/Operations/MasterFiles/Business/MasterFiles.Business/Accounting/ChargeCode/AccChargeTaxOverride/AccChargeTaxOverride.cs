using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTaxOverride : AutoAccChargeTaxOverride
	{
		public const string ALL = "ALL";
		public const string EuropeanUnion = "EUN";
		public const string EuropeanUnionExcludingLoginCountry = "EUX";
		public const string NotEuropeanUnion = "NEU";
		public const string AllCountriesExceptLoginCountry = "ALX";
		public const string SameCountryAndStateAsLineBranch = "BST";
		public const string SameCountryDifferentStateAsLineBranch = "BSX";
		public const string OtherTerritories = "OTR";

		public const string DirectionType_Other = "OTH";

		public AccChargeTaxOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AO_TaxRegCntryOrGroup

		[List("Lookups.TaxRegistrationLocations")]
		public override ZString AO_TaxRegCntryOrGroup
		{
			get { return base.AO_TaxRegCntryOrGroup; }
			set { base.AO_TaxRegCntryOrGroup = value; }
		}

		#endregion

		#region AO_CustomsStatus

		[List("Lookups.CustomsStatusList")]
		public override ZString AO_CustomsStatus
		{
			get
			{
				return base.AO_CustomsStatus;
			}
			set
			{
				base.AO_CustomsStatus = value;
			}
		}

		public bool AO_CustomsStatus_ReadOnly
		{
			get { return this.AO_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated; }
		}

		#endregion

		public GlbBranchDependentCollection Branches
		{
			get { return new GlbBranchDependentCollection(Factory); }
		}

		[List("Branches")]
		public override ZGuid AO_GB
		{
			get
			{
				return base.AO_GB;
			}
			set
			{
				base.AO_GB = value;
			}
		}

		#region AO_DebtorRole

		[List("Lookups.DebtorRoleList")]
		public override ZString AO_DebtorRole
		{
			get => base.AO_DebtorRole;
			set => base.AO_DebtorRole = value;
		}

		public bool AO_DebtorRole_ReadOnly
		{
			get
			{
				if (Factory.HasContext(AccTaxOverrideGroup.BusinessContext.TaxFramework))
				{
					return true;
				}
				return AO_JobType != JobInvoicingConsumerTypes.ShipmentCode || AO_CostSellAll != AccChargeTaxOverrideLookups.Revenue;
			}
		}
		#endregion

		#region Duplicate

		public bool IsDuplicate(AccChargeTaxOverride @override)
		{
			return AO_CostSellAll == @override.AO_CostSellAll &&
				AO_Direction == @override.AO_Direction &&
				AO_TransportMode == @override.AO_TransportMode &&
				AO_IncoTerm == @override.AO_IncoTerm &&
				AO_JobType == @override.AO_JobType &&
				AO_Origin == @override.AO_Origin &&
				AO_Destination == @override.AO_Destination &&
				AO_TaxRegCntryOrGroup == @override.AO_TaxRegCntryOrGroup &&
				AO_CustomsStatus == @override.AO_CustomsStatus &&
				AO_HomeCountryOrZone == @override.AO_HomeCountryOrZone &&
				AO_VATExemptOnExportCharges == @override.AO_VATExemptOnExportCharges &&
				AO_OrganisationCategory == @override.AO_OrganisationCategory &&
				AO_SplitPaymentVATOrganisation == @override.AO_SplitPaymentVATOrganisation &&
				AO_GB == @override.AO_GB &&
				AO_TransactionContext == @override.AO_TransactionContext &&
				AO_SupplyType == @override.AO_SupplyType &&
				AO_DebtorRole == @override.AO_DebtorRole;
		}

		#endregion

		#region Defaulting

		#region Properties

		#region AO_A9_DefaultVATClass

		protected bool AO_A9_DefaultVATClass_ReadOnly => !AO_CreateTaxRecord;

		void UpdateAO_A9_DefaultVATClassOnReadOnly()
		{
			if (AO_A9_DefaultVATClass_ReadOnly)
			{
				previousAO_A9_DefaultVATClassValue = AO_A9_DefaultVATClass;
				AO_A9_DefaultVATClass = ZGuid.Empty;
			}
			else
			{
				AO_A9_DefaultVATClass = previousAO_A9_DefaultVATClassValue;
			}
		}
		ZGuid previousAO_A9_DefaultVATClassValue;

		#endregion

		#region AO_AT

		[List("Lookups.TaxRates")]
		public override ZGuid AO_AT
		{
			get
			{
				return base.AO_AT;
			}
			set
			{
				base.AO_AT = value;
				Validation.ValidateAO_A9_DefaultVATClass();
			}
		}

		protected bool AO_AT_ReadOnly => !AO_CreateTaxRecord || AO_DefaultingRule == Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount;

		void UpdateAO_ATOnReadOnly()
		{
			if (AO_AT_ReadOnly)
			{
				previousAO_ATValue = AO_AT;
				AO_AT = ZGuid.Empty;
			}
			else
			{
				AO_AT = previousAO_ATValue;
			}
		}
		ZGuid previousAO_ATValue;

		#endregion

		#region AO_CostSellAll
		[List("Lookups.CostSellList")]
		public override ZString AO_CostSellAll
		{
			get
			{
				return base.AO_CostSellAll;
			}
			set
			{
				base.AO_CostSellAll = value;
				if (AO_CostSellAll != AccChargeTaxOverrideLookups.Revenue && AO_SplitPaymentVATOrganisation)
				{
					AO_SplitPaymentVATOrganisation = false;
				}
				if (AO_CostSellAll != AccChargeTaxOverrideLookups.Revenue && !AO_DebtorRole.IsEmpty)
				{
					AO_DebtorRole = ZString.Empty;
				}
			}
		}
		#endregion

		public override ZBool AO_CreateTaxRecord
		{
			get
			{
				return base.AO_CreateTaxRecord;
			}
			set
			{
				bool oldTaxIdIsReadOnly = AO_AT_ReadOnly;
				bool oldTaxMessageIsReadOnly = AO_A9_DefaultVATClass_ReadOnly;
				base.AO_CreateTaxRecord = value;

				if (oldTaxIdIsReadOnly != AO_AT_ReadOnly)
				{
					UpdateAO_ATOnReadOnly();
				}

				if (oldTaxMessageIsReadOnly != AO_A9_DefaultVATClass_ReadOnly)
				{
					UpdateAO_A9_DefaultVATClassOnReadOnly();
				}
			}
		}

		#region AO_Direction

		[List("Lookups.DirectionList")]
		public override ZString AO_Direction
		{
			get { return base.AO_Direction; }
			set
			{
				base.AO_Direction = value;

				if (!IsDefaultingSuspended)
				{
					using (GetDefaultingSuspender())
					{
						if (AO_Direction == Export)
						{
							AO_Origin = CurrentCountryCode;
							AO_Destination = ALL;
						}
						else if (AO_Direction == Import)
						{
							AO_Origin = ALL;
							AO_Destination = CurrentCountryCode;
						}
						else if (AO_Direction == Domestic)
						{
							AO_Origin = CurrentCountryCode;
							AO_Destination = CurrentCountryCode;
						}
						else
						{
							AO_Origin = ALL;
							AO_Destination = ALL;
						}
					}
				}
			}
		}

		public bool AO_Direction_ReadOnly => AO_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated || !(JobType?.IsDirectionSupported ?? false);

		#endregion

		#region AO_TransportMode

		[List("Lookups.TransportModeList")]
		public override ZString AO_TransportMode
		{
			get => base.AO_TransportMode;
			set => base.AO_TransportMode = value;
		}

		protected bool AO_TransportMode_ReadOnly => !IsTransportModeSupported;

		internal bool IsTransportModeSupported => AO_JobType != AccountingMasterFilesConstants.JobTypes.NonJobRelated && (JobType?.IsTransportModeSupported ?? false);

		void UpdateTransportModeIfIsTransportModeSupportedChange()
		{
			if (!IsTransportModeSupported)
			{
				transportModePreviousValue = AO_TransportMode;
				AO_TransportMode = defaultTransportMode;
			}
			else if (AO_TransportMode == defaultTransportMode && !transportModePreviousValue.IsEmpty)
			{
				AO_TransportMode = transportModePreviousValue;
			}
		}

		ZString transportModePreviousValue;

		string defaultTransportMode => Core.Constants.TransportModes.All;

		#endregion

		#region AO_OrganisationCategory

		[List("Lookups.OrganisationCategoryList")]
		public override ZString AO_OrganisationCategory
		{
			get
			{
				return base.AO_OrganisationCategory;
			}
			set
			{
				base.AO_OrganisationCategory = value;
			}
		}

		#endregion

		#region AO_SplitPaymentVATOrganisation

		public bool AO_SplitPaymentVATOrganisation_ReadOnly
		{
			get
			{
				return AO_CostSellAll != AccChargeTaxOverrideLookups.Revenue;
			}
		}

		#endregion

		#region AO_SupplyType

		[List("Lookups.SupplyTypes")]
		public override ZString AO_SupplyType
		{
			get => base.AO_SupplyType;
			set => base.AO_SupplyType = value;
		}

		#endregion

		#region AO_IncoTerm
		[List("Lookups.Incoterms")]
		public override ZString AO_IncoTerm
		{
			get
			{
				return base.AO_IncoTerm;
			}
			set
			{
				base.AO_IncoTerm = value;
			}
		}

		public bool AO_IncoTerm_ReadOnly
		{
			get { return this.AO_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated; }
		}
		#endregion

		#region AO_JobType
		[List("Lookups.JobTypes")]
		public override ZString AO_JobType
		{
			get
			{
				return base.AO_JobType;
			}
			set
			{
				if (value != base.AO_JobType)
				{
					var isTransportModeSupportedOldValue = IsTransportModeSupported;
					base.AO_JobType = value;

					if (value == AccountingMasterFilesConstants.JobTypes.NonJobRelated
						|| !(value.IsEmpty || value == ALL || (JobType?.IsDirectionSupported ?? false)))
					{
						AO_Direction = AccChargeTaxOverride.ALL;
						AO_IncoTerm = AccChargeTaxOverride.ALL;
						AO_CustomsStatus = ZString.Empty;
					}

					if (isTransportModeSupportedOldValue != IsTransportModeSupported)
					{
						UpdateTransportModeIfIsTransportModeSupportedChange();
					}

					if (value != JobInvoicingConsumerTypes.ShipmentCode && !AO_DebtorRole.IsEmpty)
					{
						AO_DebtorRole = ZString.Empty;
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1307")]
		public JobInvoicingConsumerType JobType
		{
			get
			{
				JobInvoicingConsumerType resultJobType = null;
				if (!base.AO_JobType.IsEmpty && !base.AO_JobTypeInfo.HasErrors())
				{
					resultJobType = Lookups.JobTypes[AO_JobType] as JobInvoicingConsumerType;
				}
				return resultJobType;
			}
		}

		#endregion

		#region AO_Origin
		[List("Lookups.Locations")]
		public override ZString AO_Origin
		{
			get { return base.AO_Origin; }
			set
			{
				base.AO_Origin = value;

				if (!IsDefaultingSuspended && AO_Direction.IsEmpty)
				{
					using (GetDefaultingSuspender())
					{
						if (AO_Origin == CurrentCountryCode)
						{
							AO_Direction = Export;

							if (AO_Destination.IsEmpty)
							{
								AO_Destination = ALL;
							}
						}
					}
				}
			}
		}

		protected bool AO_Origin_ReadOnly => AO_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated || AO_Direction_ReadOnly;

		#endregion

		#region AO_Destination
		[List("Lookups.Locations")]
		public override ZString AO_Destination
		{
			get { return base.AO_Destination; }
			set
			{
				base.AO_Destination = value;

				if (!IsDefaultingSuspended && AO_Direction.IsEmpty)
				{
					using (GetDefaultingSuspender())
					{
						if (AO_Destination == CurrentCountryCode)
						{
							AO_Direction = Import;

							if (AO_Origin.IsEmpty)
							{
								AO_Origin = ALL;
							}
						}
					}
				}
			}
		}

		protected bool AO_Destination_ReadOnly => AO_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated || AO_Direction_ReadOnly;

		#endregion

		[List("Lookups.Locations")]
		[ResourceStringData("327d0d80-8462-4456-9d9f-7258f5595dc7", Caption = "FPOS/AR/AP Location")]
		public override ZString AO_HomeCountryOrZone
		{
			get { return base.AO_HomeCountryOrZone; }
			set { base.AO_HomeCountryOrZone = value; }
		}

		#region AO_DefaultingRule

		[List("Lookups.DefaultingRuleList")]
		public override ZString AO_DefaultingRule
		{
			get
			{
				return base.AO_DefaultingRule;
			}
			set
			{
				bool oldTaxIdIsReadOnly = AO_AT_ReadOnly;
				base.AO_DefaultingRule = value;

				if (oldTaxIdIsReadOnly != AO_AT_ReadOnly)
				{
					UpdateAO_ATOnReadOnly();
				}
			}
		}

		public bool AO_DefaultingRule_ReadOnly => AO_TransactionContext != Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;

		void UpdateAO_DefaultingRuleOnly()
		{
			if (AO_DefaultingRule_ReadOnly)
			{
				previousAO_DefaultingRule = AO_DefaultingRule;
				AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable;
			}
			else
			{
				AO_DefaultingRule = previousAO_DefaultingRule.IsEmpty || previousAO_DefaultingRule == Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable ?
					(ZString)Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount : previousAO_DefaultingRule;
			}
		}
		ZString previousAO_DefaultingRule;

		#endregion

		#region AO_TransactionContext

		[List("Lookups.TransactionContextList")]
		public override ZString AO_TransactionContext
		{
			get
			{
				return base.AO_TransactionContext;
			}
			set
			{
				var oldDefaultingRuleReadOnly = AO_DefaultingRule_ReadOnly;
				base.AO_TransactionContext = value;

				if (oldDefaultingRuleReadOnly != AO_DefaultingRule_ReadOnly)
				{
					UpdateAO_DefaultingRuleOnly();
				}
			}
		}

		#endregion

		#endregion

		ZString Import
		{
			get { return OrgConstants.ServiceDirection.Code.Import; }
		}

		ZString Export
		{
			get { return OrgConstants.ServiceDirection.Code.Export; }
		}

		ZString Domestic
		{
			get { return OrgConstants.ServiceDirection.Code.Domestic; }
		}

		internal ZString CurrentCountryCode
		{
			get
			{
				if (ChargeCode != null)
				{
					return ChargeCode.Company != null ? ChargeCode.Company.GC_RN_NKCountryCode : ZString.Empty;
				}
				else
				{
					return TaxOverrideGroup != null ? TaxOverrideGroup.AX_RN_NKCountry : ZString.Empty;
				}
			}
		}

		#region Suspend Defaulting

		bool IsDefaultingSuspended;

		class DefaultingSuspender : Disposable
		{
			public DefaultingSuspender(AccChargeTaxOverride @override)
			{
				TaxOverride = @override;
				TaxOverride.IsDefaultingSuspended = true;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					TaxOverride.IsDefaultingSuspended = false;
				}
			}

			readonly AccChargeTaxOverride TaxOverride;
		}

		public IDisposable GetDefaultingSuspender()
		{
			return new DefaultingSuspender(this);
		}

		#endregion

		#endregion

		public AccChargeCode ChargeCode
		{
			get { return AO_ParentTableCode == ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(AccChargeCode.Schema.TableName) ? (AccChargeCode)Factory.Load(typeof(AccChargeCode), AO_ParentID) : null; }
		}

		public virtual AccTaxOverrideGroup TaxOverrideGroup
		{
			get { return AO_ParentTableCode == ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(AccTaxOverrideGroup.Schema.TableName) ? (AccTaxOverrideGroup)Factory.Load(typeof(AccTaxOverrideGroup), AO_ParentID) : null; }
		}

		public ZBool IsTaxFrameworkRelated => TaxOverrideGroup != null && TaxOverrideGroup.IsTaxFrameworkRelated;

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
			=> jobTypeDirectionAndTransportListProvider ??= new JobTypeDirectionAndTransportInfoProvider(() => AO_JobTypeInfo, () => AO_DirectionInfo, () => AO_TransportModeInfo);
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			AO_Direction = "ALL";
			if (AO_CostSellAll.IsEmpty)
			{
				AO_CostSellAll = "ALL";
			}
			if (AO_IncoTerm.IsEmpty)
			{
				AO_IncoTerm = "ALL";
			}
			if (AO_JobType.IsEmpty)
			{
				AO_JobType = "ALL";
			}
			AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
