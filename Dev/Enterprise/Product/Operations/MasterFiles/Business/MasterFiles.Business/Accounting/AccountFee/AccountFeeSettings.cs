using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccountFeeSettings : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccountFeeSettings(ZGuid companyPK, BusinessObject parent)
			: base(parent.Factory)
		{
			this.parent = parent;
			this.companyPK = companyPK;
		}

		#region Properties

		#region OverrideSettings
		[ReadOnlyMember(nameof(AccountFeeSettings_ReadOnly))]
		public ZBool OverrideSettings
		{
			get { return overrideSettings; }
			set
			{
				bool hasChanged = (overrideSettings != value);
				SetNonPersistentPropertyValue(OverrideSettingsInfo, ref overrideSettings, value);
				if (hasChanged)
				{
					PopulateFields(OverrideSettings);
				}
			}
		}
		ZBool overrideSettings;

		public ZPropertyInfo OverrideSettingsInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideSettings)); }
		}
		#endregion

		#region AAF_AG_GLAccount
		[RelatedBusinessObject("GLAccount")]
		[List("Lookups.GLAccounts")]
		[ReadOnlyMember(nameof(AccountFeeSettings_ReadOnly))]
		public ZGuid AAF_AG_GLAccount
		{
			get { return glAccount; }
			set
			{
				SetNonPersistentPropertyValue(AAF_AG_GLAccountInfo, ref glAccount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAAF_AG_GLAccount();
				}
			}
		}
		ZGuid glAccount;

		public ZPropertyInfo AAF_AG_GLAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AAF_AG_GLAccount)); }
		}

		public AccGLHeader GLAccount
		{
			get { return (AccGLHeader)Factory.Load(typeof(AccGLHeader), AAF_AG_GLAccount); }
		}
		#endregion

		#region AAF_Rule
		[List("Lookups.AccountFeeCalculationRuleList")]
		[ReadOnlyMember(nameof(AccountFeeSettings_ReadOnly))]
		public ZString AAF_Rule
		{
			get { return rule; }
			set
			{
				if (SetNonPersistentPropertyValue(AAF_RuleInfo, ref rule, value))
				{
					if (AAF_Rule == AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee)
					{
						AAF_FeeAmount = 0m;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateAAF_Rule();
					}
				}
			}
		}
		ZString rule;

		public ZPropertyInfo AAF_RuleInfo
		{
			get { return GetZPropertyInfo(nameof(AAF_Rule)); }
		}
		#endregion

		#region AAF_FeeAmount
		[DecimalPlaces(nameof(AAF_FeeAmountDecimalPlaces))]
		[ReadOnlyMember(nameof(AccountFeeSettings_ReadOnly))]
		public ZDecimal AAF_FeeAmount
		{
			get { return feeAmount; }
			set
			{
				SetNonPersistentPropertyValue(AAF_FeeAmountInfo, ref feeAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAAF_FeeAmount();
				}
			}
		}
		ZDecimal feeAmount;

		public ZPropertyInfo AAF_FeeAmountInfo
		{
			get { return GetZPropertyInfo(nameof(AAF_FeeAmount)); }
		}

		public int AAF_FeeAmountDecimalPlaces => FeeCurrency?.Decimals ?? LocalDecimals;

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region AAF_RX_NKFeeCurrency

		[RelatedBusinessObject("FeeCurrency")]
		[List("Lookups.FeeCurrencies")]
		public ZString AAF_RX_NKFeeCurrency
		{
			get { return currency; }
			set
			{
				SetNonPersistentPropertyValue(AAF_RX_NKFeeCurrencyInfo, ref currency, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAAF_RX_NKFeeCurrency();
				}
			}
		}
		ZString currency;

		public ZPropertyInfo AAF_RX_NKFeeCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(AAF_RX_NKFeeCurrency)); }
		}

		public RefCurrency FeeCurrency
		{
			get { return (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, AAF_RX_NKFeeCurrency); }
		}
		#endregion

		#region OverrideText

		public ZString OverrideText
		{
			get { return overrideText; }
			set
			{
				SetNonPersistentPropertyValue(OverrideTextInfo, ref overrideText, value);
			}
		}
		ZString overrideText;

		public ZPropertyInfo OverrideTextInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideText)); }
		}

		#endregion

		#region CurrentLevel
		AccountFeeSettingsLevel CurrentLevel
		{
			get
			{
				AccountFeeSettingsLevel level = AccountFeeSettingsLevel.None;

				if (parent is OrgCompanyData)
				{
					level = AccountFeeSettingsLevel.OrgHeader;
				}
				else if (parent is OrgDebtorGroup)
				{
					level = AccountFeeSettingsLevel.DebtorGroup;
				}
				else if (parent is GlbCompany)
				{
					level = AccountFeeSettingsLevel.Company;
				}

				return level;
			}
		}
		#endregion

		#region Company
		public GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = !companyPK.IsEmpty ? (GlbCompany)Factory.Load(typeof(GlbCompany), companyPK) : null;
				}
				return company;
			}
		}
		GlbCompany company;
		#endregion

		#endregion

		#region AccAccountFee

		AccAccountFee LoadSettingsExcludingCurrentLevel()
		{
			var allLines = LoadFromDB();
			var result = GetMostApplicableSettings(allLines, parent.PK, CurrentLevel, true, true);
			return result;
		}

		AccAccountFee LoadWithFallback(bool setOverrideText = true)
		{
			var allLines = LoadFromDB();
			var result = GetMostApplicableSettings(allLines, parent.PK, CurrentLevel, false, setOverrideText);
			return result;
		}

		AccAccountFee[] LoadFromDB()
		{
			var query = new ZQuery(AccAccountFeeSchema.AAF_GC_Company, companyPK);

			var companyQry = new ZQuery(AccAccountFeeSchema.AAF_OB_CompanyData, null);
			companyQry.AddToFilter(AccAccountFeeSchema.AAF_OJ_DebtorGroup, null);

			if (parent is OrgCompanyData)
			{
				ZQuery com = new ZQuery();

				var obQry = new ZQuery(AccAccountFeeSchema.AAF_OB_CompanyData, parent.PK);
				obQry.AddToFilter(AccAccountFeeSchema.AAF_OJ_DebtorGroup, null);
				com.AddToFilter(obQry, JoinCondition.Or);

				if (!(parent as OrgCompanyData).OB_OJ_ARDebtorGroup.IsEmpty)
				{
					var ojQry = new ZQuery(AccAccountFeeSchema.AAF_OJ_DebtorGroup, (parent as OrgCompanyData).OB_OJ_ARDebtorGroup);
					ojQry.AddToFilter(AccAccountFeeSchema.AAF_OB_CompanyData, null);
					com.AddToFilter(ojQry, JoinCondition.Or);
				}

				com.AddToFilter(companyQry, JoinCondition.Or);

				query.AddToFilter(com);
			}
			else if (parent is OrgDebtorGroup)
			{
				var ojQry = new ZQuery(AccAccountFeeSchema.AAF_OJ_DebtorGroup, parent.PK);
				ojQry.AddToFilter(AccAccountFeeSchema.AAF_OB_CompanyData, null);

				ZQuery com = new ZQuery(ojQry, JoinCondition.Or, companyQry);
				query.AddToFilter(com);
			}
			else if (parent is GlbCompany)
			{
				query.AddToFilter(companyQry);
			}
			else
			{
				throw new InvalidOperationException("unknown parent object type");
			}

			var allLines = parent.Factory.Load<AccAccountFee>(query);

			return allLines;
		}

		AccAccountFee GetMostApplicableSettings(AccAccountFee[] allApplicableSettings, ZGuid bizoPK, AccountFeeSettingsLevel level, bool excludeSettingsOfCurrentLevel, bool setOverrideText)
		{
			AccAccountFee result = null;
			ZGuid upperLevelPK = ZGuid.Empty;
			AccountFeeSettingsLevel upperLevel = AccountFeeSettingsLevel.None;

			switch (level)
			{
				case AccountFeeSettingsLevel.OrgHeader:
					result = allApplicableSettings.Where(x => x.AAF_OB_CompanyData == bizoPK && (!excludeSettingsOfCurrentLevel || !IsOverridden(x))).SingleOrDefault();
					if (!(parent as OrgCompanyData).OB_OJ_ARDebtorGroup.IsEmpty)
					{
						upperLevelPK = ((parent as OrgCompanyData).OB_OJ_ARDebtorGroup);
						upperLevel = AccountFeeSettingsLevel.DebtorGroup;
					}
					else
					{
						upperLevelPK = companyPK;
						upperLevel = AccountFeeSettingsLevel.Company;
					}
					break;
				case AccountFeeSettingsLevel.DebtorGroup:
					result = allApplicableSettings.Where(x => x.AAF_OJ_DebtorGroup == bizoPK && (!excludeSettingsOfCurrentLevel || !IsOverridden(x))).SingleOrDefault();
					upperLevelPK = companyPK;
					upperLevel = AccountFeeSettingsLevel.Company;
					break;
				case AccountFeeSettingsLevel.Company:
					result = allApplicableSettings.Where(x => x.AAF_GC_Company == bizoPK && x.AAF_OB_CompanyData.IsEmpty && x.AAF_OJ_DebtorGroup.IsEmpty && (!excludeSettingsOfCurrentLevel || !IsOverridden(x))).SingleOrDefault();
					break;
				default:
					break;
			}

			if (result == null && !upperLevelPK.IsEmpty)
			{
				result = GetMostApplicableSettings(allApplicableSettings, upperLevelPK, upperLevel, excludeSettingsOfCurrentLevel, setOverrideText);
			}
			else
			{
				if (setOverrideText)
				{
					SetOverrideText(result, GetLevelOfAvailableImmediateUpperLevelSettings(allApplicableSettings, result));
				}
			}

			return result;
		}

		AccountFeeSettingsLevel GetLevelOfAvailableImmediateUpperLevelSettings(AccAccountFee[] allApplicableSettings, AccAccountFee mostApplicableSettingsforCurrentLevel)
		{
			AccAccountFee availableImmediateUpperLevelSettings = null;

			if (IsOverridden(mostApplicableSettingsforCurrentLevel))
			{
				availableImmediateUpperLevelSettings = GetMostApplicableSettings(allApplicableSettings, parent.PK, CurrentLevel, true, false);
			}
			else
			{
				availableImmediateUpperLevelSettings = mostApplicableSettingsforCurrentLevel;
			}

			var immediateUpperLevel = AccountFeeSettingsLevel.None;

			if (availableImmediateUpperLevelSettings != null)
			{
				if (availableImmediateUpperLevelSettings.AAF_OJ_DebtorGroup != ZGuid.Empty)
				{
					immediateUpperLevel = AccountFeeSettingsLevel.DebtorGroup;
				}
				else if (availableImmediateUpperLevelSettings.AAF_OB_CompanyData == ZGuid.Empty
					&& availableImmediateUpperLevelSettings.AAF_OJ_DebtorGroup == ZGuid.Empty
					&& availableImmediateUpperLevelSettings.AAF_GC_Company != ZGuid.Empty)
				{
					immediateUpperLevel = AccountFeeSettingsLevel.Company;
				}
			}

			return immediateUpperLevel;
		}

		bool IsOverridden(AccAccountFee accFee)
		{
			return accFee != null && ((accFee.AAF_OB_CompanyData == parent.PK && parent is OrgCompanyData)
									|| (accFee.AAF_OJ_DebtorGroup == parent.PK && parent is OrgDebtorGroup)
										|| (accFee.AAF_GC_Company == parent.PK && parent is GlbCompany));
		}

		void SetOverrideText(AccAccountFee accFee, AccountFeeSettingsLevel overriddenLevel)
		{
			switch (overriddenLevel)
			{
				case AccountFeeSettingsLevel.DebtorGroup:
					OverrideText = Res.GetString("d2d1a28c-3609-4aed-876c-c71840107574", "Override Debtor Group Account Fee Settings");
					break;
				case AccountFeeSettingsLevel.Company:
					OverrideText = Res.GetString("ba755b9c-fd24-4710-b2f2-d9a89ff4212c", "Override Company Account Fee Settings");
					break;
				case AccountFeeSettingsLevel.None:
					OverrideText = accFee != null ? Res.GetString("428f8c1f-893b-461d-8c06-1058bd6860cd", "Account Fee Settings. (To remove this settings please un-tick)") : Res.GetString("1f16028f-da5e-417d-9940-1dd6d010e88e", "Set Account Fee");
					break;
			}
		}

		AccAccountFee Create()
		{
			var result = parent.Factory.New<AccAccountFee>();
			result.AAF_GC_Company = companyPK;
			if (parent is OrgDebtorGroup)
			{
				result.AAF_OJ_DebtorGroup = parent.PK;
			}
			if (parent is OrgCompanyData)
			{
				result.AAF_OB_CompanyData = parent.PK;
			}
			return result;
		}

		#endregion

		#region Lookups

		public AccAccountFeeLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		AccAccountFeeLookups GetNewLookups()
		{
			var accFee = new ReadOnlyBusinessObjectFactory().New<AccAccountFee>();
			accFee.AAF_GC_Company = companyPK;
			return new AccAccountFeeLookups(accFee);
		}

		AccAccountFeeLookups fLookups;

		#endregion

		#region Validation

		public AccountFeeSettingsValidation Validation
		{
			get { return accountFeeSettingsValidation = accountFeeSettingsValidation ?? GetNewValidation(); }
		}

		AccountFeeSettingsValidation accountFeeSettingsValidation;

		protected virtual AccountFeeSettingsValidation GetNewValidation()
		{
			return new AccountFeeSettingsValidation(this);
		}

		#endregion

		#region Functions

		public void ResetFields()
		{
			AAF_AG_GLAccount = ZGuid.Empty;
			AAF_Rule = ZString.Empty;
			AAF_FeeAmount = ZDecimal.Zero;
			AAF_RX_NKFeeCurrency = Company != null ? Company.GC_RX_NKLocalCurrency : ZString.Empty;
		}

		public void PopulateFields(bool? overridden = null)
		{
			using (SuspendSettingHasChanges())
			{
				AccAccountFee accFee = (overridden.HasValue && !overridden.Value) ? LoadSettingsExcludingCurrentLevel() : LoadWithFallback();
				if (accFee != null)
				{
					AAF_AG_GLAccount = accFee.AAF_AG_GLAccount;
					AAF_Rule = accFee.AAF_Rule;
					AAF_RX_NKFeeCurrency = accFee.AAF_RX_NKFeeCurrency;
					AAF_FeeAmount = accFee.AAF_FeeAmount;
					if (!overridden.HasValue)
					{
						overrideSettings = IsOverridden(accFee);
					}
				}
				else
				{
					ResetFields();
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!parent.IsDeleted)
			{
				var accFee = LoadWithFallback(setOverrideText: false);
				if (OverrideSettings)
				{
					if (!IsOverridden(accFee))
					{
						accFee = Create();
					}
					accFee.AAF_AG_GLAccount = AAF_AG_GLAccount;
					accFee.AAF_Rule = AAF_Rule;
					accFee.AAF_RX_NKFeeCurrency = AAF_RX_NKFeeCurrency;
					accFee.AAF_FeeAmount = AAF_FeeAmount;
				}
				else
				{
					if (IsOverridden(accFee))
					{
						accFee.Delete();
					}
				}
			}
		}

		public static bool HasAccountFeeSettings(ZGuid companyPK)
		{
			var factory = new ReadOnlyBusinessObjectFactory();

			var query = new ZDBOnlyQuery(typeof(AccAccountFee));
			query.AddToFilter(AccAccountFeeSchema.AAF_GC_Company, companyPK);

			var companySubQry = new ZDBOnlySubQuery(typeof(GlbCompany), AccAccountFeeSchema.AAF_GC_Company);
			companySubQry.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			query.AddSubQuery(companySubQry, JoinCondition.And);

			var debtorGroupQry = new ZDBOnlyQuery(typeof(AccAccountFee));
			debtorGroupQry.AddToFilter(AccAccountFeeSchema.AAF_OJ_DebtorGroup, null);

			var debtorGroupSubQry = new ZDBOnlySubQuery(typeof(OrgDebtorGroup), AccAccountFeeSchema.AAF_OJ_DebtorGroup);
			debtorGroupSubQry.AddToFilter(OrgDebtorGroupSchema.OJ_IsValid, true);
			debtorGroupQry.AddSubQuery(debtorGroupSubQry, JoinCondition.Or);

			query.AddToFilter(debtorGroupQry);

			var companyDataQry = new ZDBOnlyQuery(typeof(AccAccountFee));
			companyDataQry.AddToFilter(AccAccountFeeSchema.AAF_OB_CompanyData, null);

			var companyDataSubQry = new ZDBOnlySubQuery(typeof(OrgCompanyData), AccAccountFeeSchema.AAF_OB_CompanyData);
			var orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCompanyDataSchema.OB_OH);
			orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			companyDataSubQry.AddSubQuery(orgQuery, JoinCondition.And);
			companyDataQry.AddSubQuery(companyDataSubQry, JoinCondition.Or);

			query.AddToFilter(companyDataQry);

			return factory.ExistsInDatabase(AccAccountFeeSchema.Constants.TableName, query);
		}

		#endregion

		#region AccountFeeSettings ReadOnly

		bool AccountFeeSettings_ReadOnly => !Env.Security.OrgReceivablesModifyAccountFee.IsAllowed;

		#endregion

		readonly BusinessObject parent;
		readonly ZGuid companyPK;

		public enum AccountFeeSettingsLevel
		{
			None = 0,
			Company = 1,
			DebtorGroup = 2,
			OrgHeader = 3
		}
	}
}
