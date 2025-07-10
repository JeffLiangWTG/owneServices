using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxConfiguration : AutoAccTaxConfiguration
	{
		public AccTaxConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ETC_CancellationPolicy = CancellationPolicyMethods.NoRestriction.Code;
			ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
		}

		[ReadOnly(true)]
		public override ZString ETC_Code { get => base.ETC_Code; set => base.ETC_Code = value; }

		[List("Lookups.TaxAuthorities")]
		public override ZString ETC_TaxAuthorityCode
		{
			get => base.ETC_TaxAuthorityCode;
			set
			{
				base.ETC_TaxAuthorityCode = value;
				OverrideCodeValue();
			}
		}

		[List("Lookups.TaxSystems")]
		public override ZString ETC_TaxSystemCode
		{
			get => base.ETC_TaxSystemCode;
			set
			{
				var hasChanges = base.ETC_TaxSystemCode != value;
				base.ETC_TaxSystemCode = value;
				OverrideCodeValue();
				Validation.ValidateETC_TaxRecordCreationTrigger();
				Validation.ValidateETC_TaxRealisationMethod();
				Validation.ValidateETC_RecoveryMethod();

				if (hasChanges && !ETC_TaxSystemCodeInfo.HasErrors())
				{
					using (SetGLAccountsSuspender.GetSuspender())
					{
						SetupSPRAPConfiguration();
					}
					SetGLAccounts();
				}
			}
		}

		public TaxSystemsConfiguration TaxSystem => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystem(ETC_TaxSystemCode, Factory);

		[List("Lookups.Ledger")]
		public override ZString ETC_Ledger
		{
			get => base.ETC_Ledger;
			set
			{
				var hasChanges = base.ETC_Ledger != value;
				base.ETC_Ledger = value;
				OverrideCodeValue();
				Validation.ValidateETC_TaxRecordCreationTrigger();
				Validation.ValidateETC_TaxRealisationMethod();
				Validation.ValidateETC_RecoveryMethod();
				Validation.ValidateETC_ThresholdAmount();

				if (hasChanges && !ETC_LedgerInfo.HasErrors())
				{
					using (SetGLAccountsSuspender.GetSuspender())
					{
						SetupSPRAPConfiguration();
					}
					SetGLAccounts();
				}
			}
		}

		public bool ETC_TaxAuthorityCode_ReadOnly => IsInDatabase;
		public bool ETC_TaxSystemCode_ReadOnly => IsInDatabase;
		public bool ETC_Ledger_ReadOnly => IsInDatabase;

		[List("Lookups.TaxRealisationMethods")]
		public override ZString ETC_TaxRealisationMethod
		{
			get => base.ETC_TaxRealisationMethod;
			set
			{
				var hasChanges = base.ETC_TaxRealisationMethod != value;
				base.ETC_TaxRealisationMethod = value;
				if (hasChanges && !ETC_TaxRealisationMethodInfo.HasErrors())
				{
					SetGLAccounts();
				}
			}
		}

		[ReadOnly(true)]
		public override ZGuid ETC_AG_LedgerControlAccount { get => base.ETC_AG_LedgerControlAccount; set => base.ETC_AG_LedgerControlAccount = value; }

		[List("Lookups.TaxRecordCreationTrigger")]
		public override ZString ETC_TaxRecordCreationTrigger { get => base.ETC_TaxRecordCreationTrigger; set => base.ETC_TaxRecordCreationTrigger = value; }

		[List("Lookups.CancellationPolicyMethods")]
		public override ZString ETC_CancellationPolicy { get => base.ETC_CancellationPolicy; set => base.ETC_CancellationPolicy = value; }

		[List("Lookups.RecoveryMethods")]
		public override ZString ETC_RecoveryMethod { get => base.ETC_RecoveryMethod; set => base.ETC_RecoveryMethod = value; }

		[List("Lookups.TaxAmountRoundingMethods")]
		public override ZString ETC_TaxAmountRounding { get => base.ETC_TaxAmountRounding; set => base.ETC_TaxAmountRounding = value; }

		[MaxLength(2)]
		[ReadOnly(true)]
		public override ZString ETC_RN_NKCountry
		{
			get => base.ETC_RN_NKCountry;
			set
			{
				base.ETC_RN_NKCountry = value;
				OverrideCodeValue();
			}
		}

		public override ZString ETC_ParentTableCode
		{
			get => base.ETC_ParentTableCode;
			set
			{
				base.ETC_ParentTableCode = value;
				OverrideCodeValue();
				Validation.ValidateETC_RN_NKCountry();
			}
		}

		public override ZGuid ETC_ParentId
		{
			get => base.ETC_ParentId;

			set
			{
				base.ETC_ParentId = value;
				OverrideCodeValue();
				Validation.ValidateETC_RN_NKCountry();
			}
		}

		[List("Lookups.ETC_ThresholdMethods")]
		public override ZString ETC_ThresholdMethod
		{
			get => base.ETC_ThresholdMethod;
			set
			{
				base.ETC_ThresholdMethod = value;
				if (value == ETC_ThresholdMethods.NoThreshold.Code)
				{
					ETC_ThresholdAmount = 0;
				}
				Validation.ValidateETC_ThresholdAmount();
			}
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public override ZDecimal ETC_ThresholdAmount { get => base.ETC_ThresholdAmount; set => base.ETC_ThresholdAmount = value; }

		public int LocalCurrencyDecimalPlaces => Company.GetLocalDecimals();

		public bool ETC_ThresholdAmount_ReadOnly => ETC_ThresholdMethod == ETC_ThresholdMethods.NoThreshold.Code;

		internal GlbCompany ParentCompany
		{
			get
			{
				if (!ETC_ParentId.IsEmpty && ETC_ParentTableCode == GlbCompanySchema.Constants.Prefix)
				{
					return Factory.Load<GlbCompany>(ETC_ParentId);
				}

				return null;
			}
		}

		internal GlbBranch ParentBranch
		{
			get
			{
				if (!ETC_ParentId.IsEmpty && ETC_ParentTableCode == GlbBranchSchema.Constants.Prefix)
				{
					return Factory.Load<GlbBranch>(ETC_ParentId);
				}

				return null;
			}
		}

		public GlbCompany Company => ParentCompany ?? ParentBranch?.Company;

		public void OverrideCodeValue()
		{
			var etc_code = new ZStringBuilder();
			if (!ETC_RN_NKCountry.IsEmpty)
			{
				etc_code.Append(ETC_RN_NKCountry);
			}
			var branch = ParentBranch;
			if (branch != null)
			{
				etc_code.Append(branch.GB_Code);
			}
			if (!ETC_TaxAuthorityCode.IsEmpty)
			{
				etc_code.Append(ETC_TaxAuthorityCode);
			}
			if (!ETC_TaxSystemCode.IsEmpty)
			{
				etc_code.Append(ETC_TaxSystemCode);
			}
			if (!ETC_Ledger.IsEmpty)
			{
				etc_code.Append(ETC_Ledger);
			}
			ZString format_etc_code = etc_code.ToStringWithDelimiterBetweenAppends("-").Trim();
			base.ETC_Code = format_etc_code.SubstringSafe(0, AccTaxConfigurationSchema.ETC_Code.MaxLength);
		}

		public override bool CanDelete => base.CanDelete && !(IsTaxConfigurationUsedInOrganisation || IsTaxConfigurationUsedInOverrideGroup);

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var reason = base.ReasonForNotAbleToDelete;
				if (IsTaxConfigurationUsedInOrganisation)
				{
					var codes = new ZStringBuilder(GetOrganisationCodesAssociatedWithThisTaxConfig());
					reason = ResString.GetMultilingualString("fb2f7366-1a3d-47c9-b249-c26bb18ba157",
						"The Tax Configuration cannot be deleted because it is used in Tax Configurations for Organizations: {0}.",
						codes.ToStringWithDelimiterBetweenAppends(", "));
				}
				else if (IsTaxConfigurationUsedInOverrideGroup)
				{
					reason = ResString.GetMultilingualString("26f3cf49-df06-471b-b9db-2ab7c63fb305",
						"The Tax Configuration cannot be deleted because it is configured for use against one or more Tax Configuration Override Groups in this record's login company. Please review and update the Tax Configuration Override Group records within that login company.");
				}
				return reason;
			}
		}

		IEnumerable<string> GetOrganisationCodesAssociatedWithThisTaxConfig()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var orgCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			var orgTaxConfigSubQuery = new ZDBOnlySubQuery(typeof(AccOrgTaxConfiguration), AccOrgTaxConfigurationSchema.OTC_OB);
			orgTaxConfigSubQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_ETC, PK);

			orgCompanyDataSubQuery.AddSubQuery(orgTaxConfigSubQuery, JoinCondition.And);
			query.AddSubQuery(orgCompanyDataSubQuery, JoinCondition.And);

			var orgHeaders = Factory.Load<OrgHeader>(query);

			return orgHeaders.Select(orgHeader => $"'{orgHeader.OH_Code}'");
		}

		bool IsTaxConfigurationUsedInOrganisation
		{
			get
			{
				return Factory.Exists(typeof(AccOrgTaxConfiguration), new ZQuery(AccOrgTaxConfigurationSchema.OTC_ETC, PK));
			}
		}

		bool IsTaxConfigurationUsedInOverrideGroup
		{
			get
			{
				return Factory.ExistsInDatabase(AutoAccTaxOverrideGroupTaxConfigurationPivot.Schema.TableName, new ZQuery(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_ETC_TaxConfiguration, PK));
			}
		}

		internal static ZString GetCountryFromCompany(GlbCompany company) => company?.GC_RN_NKCountryCode ?? ZString.Empty;

		internal static ZString GetCountryFromBranch(GlbBranch branch) => branch?.Company?.GC_RN_NKCountryCode ?? ZString.Empty;

		void SetGLAccounts()
		{
			if (SetGLAccountsSuspender.IsSuspended)
			{
				return;
			}

			GLAccRegTypes = GLAccountsProvider.GetApplicableGLAccounts(this);
			ETC_AG_LedgerControlAccount = GLAccountsProvider.GetRegistryValue(GLAccRegTypes.LedgerControlAccount);
			ETC_AG_TaxControlAccount = GLAccountsProvider.GetRegistryValue(GLAccRegTypes.TaxControlAccount);
			ETC_AG_TaxExpenseAccount = GLAccountsProvider.GetRegistryValue(GLAccRegTypes.TaxExpenseAccount);
			ETC_AG_TaxPendingControlAccount = GLAccountsProvider.GetRegistryValue(GLAccRegTypes.TaxPendingControlAccount);
		}

		FunctionalitySuspender SetGLAccountsSuspender => setGLAccountsSuspender ?? (setGLAccountsSuspender = new FunctionalitySuspender());
		FunctionalitySuspender setGLAccountsSuspender;

		internal (GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount) GLAccRegTypes
		{
			get
			{
				if (!glAccRegTypes.HasValue)
				{
					glAccRegTypes = GLAccountsProvider.GetApplicableGLAccounts(this);
				}
				return glAccRegTypes.Value;
			}
			private set
			{
				glAccRegTypes = value;
			}
		}

		(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? glAccRegTypes;

		public bool ETC_AG_TaxControlAccount_ReadOnly => IsReadOnly(GLAccRegTypes.TaxControlAccount);
		public bool ETC_AG_TaxExpenseAccount_ReadOnly => IsReadOnly(GLAccRegTypes.TaxExpenseAccount);
		public bool ETC_AG_TaxPendingControlAccount_ReadOnly => IsReadOnly(GLAccRegTypes.TaxPendingControlAccount);

		static bool IsReadOnly(GLAccountRegistryType type)
		{
			return new[] { GLAccountRegistryType.Error, GLAccountRegistryType.NotApplicable }.Contains(type);
		}

		void SetupSPRAPConfiguration()
		{
			if (this.IsSPR_AP_Configuration())
			{
				ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
				ETC_TaxRealisationMethod = TaxRealisationMethods.PostDateOfMatchTransaction.Code;
			}
		}

		IAccTaxConfigGLAccountsProvider GLAccountsProvider
		{
			get
			{
#if DEBUG
				if (GLAccountsProvider_ReplacementForTestOnly != null)
				{
					return GLAccountsProvider_ReplacementForTestOnly;
				}
#endif
				return new AccTaxConfigGLAccountsProvider();
			}
		}

#if DEBUG

		public IAccTaxConfigGLAccountsProvider GLAccountsProvider_ReplacementForTestOnly;
		public IAccTaxConfigGLAccountsProvider GLAccountsProvider_ExposedForTestOnly => GLAccountsProvider;

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (ETC_Ledger.IsEmpty)
			{
				ETC_Ledger = LedgerTypes.AccountsReceivable;
			}

			if (ETC_ParentTableCode.IsEmpty)
			{
				ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			}

			ETC_CancellationPolicy = CancellationPolicyMethods.NotAllowed.Code;
			ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
