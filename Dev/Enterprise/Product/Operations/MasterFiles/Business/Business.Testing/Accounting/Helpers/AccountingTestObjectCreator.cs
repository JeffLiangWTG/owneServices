using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	public class AccountingTestObjectCreator
	{
		public readonly BusinessObjectFactory Factory;

		public AccountingTestObjectCreator(BusinessObjectFactory factory)
		{
			if (ValidTestDataGenerationException.ShouldThrow)
			{
				throw new ValidTestDataGenerationException("This class is for testing only.");
			}

			this.Factory = factory;
		}

		#region Tax Configuration

		public AccTaxConfiguration CreateTaxConfiguration(string ledger)
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = CountryCodes.Australia;
			taxConfiguration.ETC_Ledger = ledger;
			taxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			taxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			return taxConfiguration;
		}

		public AccTaxConfiguration CreateTaxConfiguration(GlbCompany parent, TaxAuthoritiesConfiguration taxAuthority, TaxSystemsConfiguration taxSystem, string ledger = "AR", bool active = true, string code = null, string desc = null, string taxRealisationMethod = null, AccGLHeader controlAccount = null, AccGLHeader taxRealisedControlAccount = null, AccGLHeader taxPrepaidPendingControlAccount = null, AccGLHeader taxExpenseAccount = null)
			=> CreateTaxConfigurationCore(parent, taxAuthority, taxSystem, ledger, active, code, desc, taxRealisationMethod, controlAccount, taxRealisedControlAccount, taxPrepaidPendingControlAccount, taxExpenseAccount);

		public AccTaxConfiguration CreateTaxConfiguration(GlbBranch parent, TaxAuthoritiesConfiguration taxAuthority, TaxSystemsConfiguration taxSystem, string ledger = "AR", bool active = true, string code = null, string desc = null, string taxRealisationMethod = null, AccGLHeader controlAccount = null, AccGLHeader taxRealisedControlAccount = null, AccGLHeader taxPrepaidPendingControlAccount = null, AccGLHeader taxExpenseAccount = null)
			=> CreateTaxConfigurationCore(parent, taxAuthority, taxSystem, ledger, active, code, desc, taxRealisationMethod, controlAccount, taxRealisedControlAccount, taxPrepaidPendingControlAccount, taxExpenseAccount);

		public AccTaxConfiguration CreateTaxConfiguration(GlbCompany parent, string ledger = "AR", bool active = true)
			=> CreateTaxConfigurationCore(parent, null, null, ledger, active);

		public AccTaxConfiguration CreateTaxConfiguration(GlbBranch parent, string ledger = "AR", bool active = true)
			=> CreateTaxConfigurationCore(parent, null, null, ledger, active);

		AccTaxConfiguration CreateTaxConfigurationCore(BusinessObject parent, TaxAuthoritiesConfiguration taxAuthority, TaxSystemsConfiguration taxSystem, string ledger = "AR", bool active = true, string code = null, string desc = null, string taxRealisationMethod = null, AccGLHeader controlAccount = null, AccGLHeader taxRealisedControlAccount = null, AccGLHeader taxPrepaidPendingControlAccount = null, AccGLHeader taxExpenseAccount = null)
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();

			if (code != null)
			{
				taxConfiguration.ETC_Code = code;
			}
			if (desc != null)
			{
				taxConfiguration.ETC_Description = desc;
			}

			taxConfiguration.ETC_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (parent is GlbCompany company)
			{
				taxConfiguration.ETC_ParentId = company.PK;
				taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			}
			else if (parent is GlbBranch branch)
			{
				taxConfiguration.ETC_ParentId = branch.PK;
				taxConfiguration.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			}
			else
			{
				throw new InvalidOperationException("Unknown parent object type");
			}
			if (taxSystem != null)
			{
				taxConfiguration.ETC_RN_NKCountry = taxSystem.Country;
				taxConfiguration.ETC_TaxSystemCode = taxSystem.Code;
			}
			if (taxAuthority != null)
			{
				taxConfiguration.ETC_TaxAuthorityCode = taxAuthority.Code;
			}
			taxConfiguration.ETC_Ledger = ledger;
			taxConfiguration.ETC_IsActive = active;
			taxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			taxConfiguration.ETC_TaxRealisationMethod = taxRealisationMethod ?? TaxRealisationMethods.PostDate.Code;
			if (controlAccount != null)
			{
				taxConfiguration.ETC_AG_LedgerControlAccount = controlAccount.PK;
			}
			if (taxPrepaidPendingControlAccount != null)
			{
				taxConfiguration.ETC_AG_TaxPendingControlAccount = taxPrepaidPendingControlAccount.PK;
			}
			if (taxRealisedControlAccount != null)
			{
				taxConfiguration.ETC_AG_TaxControlAccount = taxRealisedControlAccount.PK;
			}
			if (taxExpenseAccount != null)
			{
				taxConfiguration.ETC_AG_TaxExpenseAccount = taxExpenseAccount.PK;
			}

			return taxConfiguration;
		}
		#endregion

		#region Org Tax Configuration

		public AccOrgTaxConfiguration CreateOrgTaxConfiguration(AccTaxConfiguration taxConfiguration, OrgCompanyData companyData = null, bool active = true)
		{
			var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = taxConfiguration.ETC_Ledger;
			orgTaxConfiguration.OTC_ETC = taxConfiguration.PK;
			orgTaxConfiguration.OTC_OB = companyData?.PK ?? ZGuid.Empty;
			orgTaxConfiguration.OTC_IsActive = active;
			return orgTaxConfiguration;
		}

		public AccOrgTaxConfigurationTemplateItem AddOrgTaxConfigurationForTemplate(AccOrgTaxConfigurationTemplate parentTemplate, AccTaxConfiguration taxConfig)
		{
			var result = parentTemplate.AccOrgTaxConfigurations.AddNew();
			result.OTC_ETC = taxConfig.PK;
			return result;
		}

		public AccOrgTaxConfigurationTemplate CreateAccOrgTaxConfigurationTemplate(string code, bool isReceivable, ZGuid? companyPK, bool isActive = true)
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_Code = code;
			template.OCT_Description = code + " DESC";
			template.OCT_IsReceivable = isReceivable;
			template.OCT_IsActive = isActive;
			template.OCT_GC_Company = companyPK ?? GlbCompany.CurrentCompany.PK;
			return template;
		}

		public void LinkOrgHeaderWithTaxConfigurationTemplate(AccOrgTaxConfigurationTemplate parentTemplate, OrgHeader linkedOrg)
		{
			if (parentTemplate.OCT_IsReceivable)
			{
				linkedOrg.CompanyData.OB_OCT_ARTaxTemplate = parentTemplate.PK;
			}
			else
			{
				linkedOrg.CompanyData.OB_OCT_APTaxTemplate = parentTemplate.PK;
			}

			parentTemplate.LinkedOrganisations.Add(linkedOrg);
		}

		#endregion

		#region Tax Authority

		public TaxAuthoritiesConfiguration CreateTaxAuthority(ZString code, string name = "CW1 TAX AUTHORITY", string country = null, string taxAuthorityType = null)
		{
			var collection1 = new TaxAuthoritiesConfigurationCollection();
			var taxAuthoritiesConfiguration = collection1.AddNew();
			taxAuthoritiesConfiguration.Code = code;
			taxAuthoritiesConfiguration.Name = name;
			taxAuthoritiesConfiguration.Country = country ?? CountryCodes.Australia;
			taxAuthoritiesConfiguration.TaxAuthorityType = taxAuthorityType ?? TaxAuthorityTypeList.National.Code;

			return taxAuthoritiesConfiguration;
		}

		#endregion

		#region Tax System

		public TaxSystemsConfiguration CreateTaxSystem(string code, string adjustmentSign = null, bool includeInInvoiceTotal = true, string registrationLevel = null, string taxSuperType = null, string taxRateSource = null, string country = null, string name = "CW1 TAX SYSTEM")
		{
			var taxSystem = new TaxSystemsConfiguration();

			taxSystem.Code = code;
			taxSystem.Name = name;
			taxSystem.Country = country ?? CountryCodes.Australia;
			taxSystem.TaxAuthorityType = TaxAuthorityTypeList.National.Code;
			taxSystem.TaxSuperType = taxSuperType ?? TaxSuperTypeList.SalesTax.Code;
			taxSystem.RegistrationLevel = registrationLevel ?? TaxSystemRegistrationLevels.Company.Code;
			taxSystem.IncludeInInvoceTotal = includeInInvoiceTotal;
			taxSystem.AdjustmentSign = adjustmentSign ?? TaxCalculationAdjustmentSigns.Positive.Code;
			taxSystem.TaxBaseCalculationMethod = TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			taxSystem.TaxAmountCalculationMethod = TaxAmountCalculationMethods.BaseTimesRate.Code;
			taxSystem.ThresholdRule = ThresholdRulesForTaxCalculation.NoThreshold.Code;
			taxSystem.TaxRateSource = taxRateSource ?? TaxRateSources.OrganisationOnly.Code;

			return taxSystem;
		}

		#endregion

		#region Tax Override

		public void CreateTaxOverrides(AccChargeCode chargeCode, params Action<AccChargeTaxOverride>[] overrideSettings)
		{
			foreach (var overrideSetting in overrideSettings)
			{
				overrideSetting(PopulateTaxOverride(chargeCode.TaxOverrides.AddNew()));
			}
		}

		#endregion

		#region Tax Override Group

		public AccTaxOverrideGroup CreateTaxOverrideGroup(GlbCompany parent, AccTaxConfiguration taxConfiguration)
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = parent.GC_RN_NKCountryCode;
			if (taxConfiguration != null)
			{
				CreateTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroup, taxConfiguration);
			}
			return taxOverrideGroup;
		}

		#endregion

		#region Tax Override Group Tax Configuration Pivot

		public AccTaxOverrideGroupTaxConfigurationPivot CreateTaxOverrideGroupTaxConfigurationPivot(AccTaxOverrideGroup taxOverrideGroup, AccTaxConfiguration taxConfiguration, int rateNumerator = 0, int rateDenominator = 1, string serviceCode = "5.2020(809)", string serviceCodeDescription = "Legislation 5.2020(809)", ZGuid? taxIdPK = null, ZGuid? defaultVATClassPK = null)
		{
			var taxOverrideGroupTaxConfigurationPivot = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = rateNumerator;
			taxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = rateDenominator;
			taxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = serviceCode;
			taxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescription = serviceCodeDescription;
			taxOverrideGroupTaxConfigurationPivot.AXP_AT_TaxID = taxIdPK ?? ZGuid.Empty;
			taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClass = defaultVATClassPK ?? ZGuid.Empty;

			return taxOverrideGroupTaxConfigurationPivot;
		}

		#endregion

		#region Tax Override Group Charge Code Pivot

		public AccTaxOverrideGroupChargeCodePivot CreateTaxOverrideGroupChargeCodePivot(AccTaxOverrideGroup taxOverrideGroup, AccChargeCode chargeCode)
		{
			var taxOverrideGroupChargeCodePivot = Factory.New<AccTaxOverrideGroupChargeCodePivot>();
			taxOverrideGroupChargeCodePivot.ACP_AC_ChargeCode = chargeCode.PK;
			taxOverrideGroupChargeCodePivot.ACP_AX_TaxOverrideGroup = taxOverrideGroup.PK;

			return taxOverrideGroupChargeCodePivot;
		}

		#endregion

		#region Tax Override Group Rule

		public AccChargeTaxOverride PopulateTaxOverride(AccChargeTaxOverride taxOverride,
			ZGuid taxCodePK = default,
			ZGuid vatClassPK = default,
			string direction = AccChargeTaxOverride.ALL,
			string costSellAll = AccChargeTaxOverride.ALL,
			string incoTerm = AccChargeTaxOverride.ALL,
			string jobType = AccChargeTaxOverride.ALL,
			string origin = AccChargeTaxOverride.ALL,
			string destination = AccChargeTaxOverride.ALL)
		{
			taxOverride.AO_AT = taxCodePK;
			taxOverride.AO_A9_DefaultVATClass = vatClassPK;
			taxOverride.AO_Direction = direction;
			taxOverride.AO_CostSellAll = costSellAll;
			taxOverride.AO_IncoTerm = incoTerm;
			taxOverride.AO_JobType = jobType;
			taxOverride.AO_Origin = origin;
			taxOverride.AO_Destination = destination;
			return taxOverride;
		}

		public AccChargeTaxOverride CreateTaxOverrideRule(AccTaxOverrideGroup taxGroup,
			ZGuid taxCodePK,
			ZGuid vatClassPK = default,
			string direction = AccChargeTaxOverride.ALL,
			string costSellAll = AccChargeTaxOverride.ALL,
			string incoTerm = AccChargeTaxOverride.ALL,
			string jobType = AccChargeTaxOverride.ALL,
			string origin = AccChargeTaxOverride.ALL,
			string destination = AccChargeTaxOverride.ALL)
		{
			var taxOverride = taxGroup.TaxOverrides.AddNew();
			return PopulateTaxOverride(taxOverride, taxCodePK, vatClassPK, direction, costSellAll, incoTerm, jobType, origin, destination);
		}

		#endregion

		#region Accounting Exchange Rate Configuration

		public AccExchangeRateConfiguration CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum level, ZGuid parentPk, ZString ledger, ZString jobType, ZString serviceDirection, ZString transportMode, string invoiceCurrencyType = "", string[] currencyCodes = null)
		{
			var exRateConfig = Factory.New<AccExchangeRateConfiguration>();
			exRateConfig.JCE_GC = level == AccExRateConfigurationLevelEnum.System ? ZGuid.Empty : Env.CurrentCompanyPK;
			exRateConfig.JCE_JobType = jobType;
			exRateConfig.JCE_ServiceDirection = serviceDirection;
			exRateConfig.JCE_TransportMode = transportMode;
			exRateConfig.JCE_ParentTableCode = level.ToTablePrefix();
			exRateConfig.JCE_ParentID = parentPk;
			exRateConfig.JCE_Ledger = ledger;
			exRateConfig.JCE_Preference = "TDR";
			exRateConfig.JCE_InvoiceCurrencyType = invoiceCurrencyType;

			currencyCodes = currencyCodes?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
			if (currencyCodes.IsNullOrEmpty())
			{
				exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
			}
			else
			{
				exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

				foreach (var currencyCode in currencyCodes)
				{
					var curConfig = exRateConfig.CurrencyConfigurations.AddNew();
					curConfig.JCT_Code = currencyCode;
					curConfig.JCT_ExRateType = ExchangeRateTypes.Code.BuyRate;
				}
			}

			return exRateConfig;
		}

		public ExchangeRateCurrencyConfiguration CreateExchangeRateCurrencyConfiguration(AccExchangeRateConfiguration parent, string currencyCode = CurrencyCodes.Australia)
		{
			var currencyConfig = Factory.New<ExchangeRateCurrencyConfiguration>();
			currencyConfig.JCT_JCF_JobConfig = parent.PK;
			currencyConfig.JCT_Code = currencyCode;
			currencyConfig.JCT_ExRateType = ExchangeRateTypes.Code.BuyRate;

			return currencyConfig;
		}

		#endregion

		public TaxIdAndTaxMessageCombinationRulesConfiguration CreateTaxIdAndTaxMessageCombinationRulesConfiguration(params (string LineType, AccTaxRate TaxRate, AccInvMsg TaxMessage)[] settings)
		{
			var config = new TaxIdAndTaxMessageCombinationRulesConfiguration();

			foreach (var setting in settings)
			{
				config.TaxIdAndTaxMessageCombinationRulesCollection.Add(new TaxIdAndTaxMessageCombinationRules()
				{
					LineType = setting.LineType,
					TaxRate = setting.TaxRate.PK,
					TaxMessage = setting.TaxMessage.PK
				});
			}

			return config;
		}

		public AccTaxRate CreateTaxRate(string code, string taxSystemCode = null, string country = null, string type = null)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_RN_NKCountry = country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_Type = type ?? AccTaxRate.Types.NotReportable;
			if (!string.IsNullOrEmpty(taxSystemCode))
			{
				taxRate.AT_TaxSystemCode = taxSystemCode;
			}

			return taxRate;
		}

		public AccTaxRate CreateTaxRate_RateSource(ZString rateSource, string taxCode = null)
		{
			AccTaxRate result = Factory.NewWithValidTestData<AccTaxRate>();
			if (!string.IsNullOrEmpty(taxCode))
			{
				result.AT_Code = taxCode;
			}
			result.AT_RateSource = rateSource;

			return result;
		}

		public AccOrgTaxRate AddOrgTaxRateItem(AccOrgTaxConfiguration config, ZDate startDate, ZDate endDate, string source, (ZInt numerator, ZInt denominator) taxRate)
		{
			var item = config.TaxRates.AddNew();
			item.OTR_StartDate = startDate;
			item.OTR_EndDate = endDate;
			item.OTR_Source = source;
			item.OTR_RateNumerator = taxRate.numerator;
			item.OTR_RateDenominator = taxRate.denominator;

			return item;
		}

		public AccTaxConfiguration ConfigureTaxFrameworkAtBranchLevel(GlbBranch branch, string ledger = "AR", string taxSuperType = null, string taxRateSource = null, string taxSystemCountry = null, bool taxConfigIsActive = true, string taxSystemCode = "TS")
		{
			var taxAuthority = CreateTaxAuthority("TA");
			var taxSystem = CreateTaxSystem(taxSystemCode, country: taxSystemCountry ?? branch.BaseCountry.Code, taxSuperType: taxSuperType, taxRateSource: taxRateSource);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxConfig = CreateTaxConfiguration(branch, taxAuthority, taxSystem, ledger, taxConfigIsActive);

			Factory.Save();

			return taxConfig;
		}

		public AccTaxConfiguration ConfigureTaxFrameworkAtCompanyLevel(GlbCompany company, string ledger = "AR", string taxSuperType = null, string taxRateSource = null, string taxSystemCountry = null, bool taxConfigIsActive = true, string taxSystemCode = "TS")
		{
			var taxAuthority = CreateTaxAuthority("TA");
			var taxSystem = CreateTaxSystem(taxSystemCode, country: taxSystemCountry ?? company.Country.Code, taxSuperType: taxSuperType, taxRateSource: taxRateSource);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxConfig = CreateTaxConfiguration(company, taxAuthority, taxSystem, ledger, taxConfigIsActive);

			Factory.Save();

			return taxConfig;
		}

		public void SetupMinimumSettingsForTaxFramework(GlbCompany company, OrgHeader org, AccChargeCode chargeCode, string ledger = "AR", string taxSource = "TID", bool isJobRelated = true, string taxCode = null, int rateNumerator = 10, int rateDenominator = 1, string taxSuperType = null, string realisationMethod = null, AccGLHeader controlAccount = null, AccGLHeader taxRealisedControlAccount = null, AccGLHeader taxPrepaidPendingControlAccount = null, AccGLHeader taxExpenseAccount = null, bool isTaxConfigActive = true)
		{
			var companyData = org.GetCompanyDataForGlbCompany(company);
			var taxAuthority = CreateTaxAuthority("TA");
			var taxSystem = CreateTaxSystem("TS", country: company.Country.Code, taxSuperType: taxSuperType);

			var taxConfigCompany = CreateTaxConfiguration(company, taxAuthority, taxSystem, ledger, taxRealisationMethod: realisationMethod, controlAccount: controlAccount, taxRealisedControlAccount: taxRealisedControlAccount, taxPrepaidPendingControlAccount: taxPrepaidPendingControlAccount, taxExpenseAccount: taxExpenseAccount, active: isTaxConfigActive);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideGroup(company, taxConfigCompany);
			CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode);

			var taxID1 = CreateTaxRate_RateSource(taxSource, taxCode);
			taxID1.SetRate_ForTestOnly(rateNumerator, rateDenominator);

			if (isJobRelated)
			{
				CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
			}
			else
			{
				CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, jobType: AccountingMasterFilesConstants.JobTypes.NonJobRelated);
			}

			Factory.Save();
		}

		public OrgHeader CreateOrgHeader(string code, bool isCreditor, bool isDebtor, bool isAPGSTApplicable = true, bool isAPWHTApplicable = true, bool isARGSTApplicable = true, bool isARWHTApplicable = true)
		{
			var header = Factory.New<OrgHeader>();

			header.OH_Code = code;
			header.OH_FullName = "Test Org Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";

			header.OH_IsDebtor = isDebtor;
			header.OH_IsCreditor = isCreditor;
			header.CompanyData.SetARTaxApplicable(isARGSTApplicable);
			header.MiscServ.OM_ARWHTApplicable = isARWHTApplicable;
			header.CompanyData.SetAPTaxApplicable(isAPGSTApplicable);
			header.MiscServ.OM_APWHTApplicable = isAPWHTApplicable;

			return header;
		}

		#region Bank Account

		public AccBankAccount CreateBankAccount(string code, string desc, RefCurrency currency, AccGLHeader gLHeader, string accountType = "")
		{
			var bankAccount = Factory.New<AccBankAccount>();

			bankAccount.AB_AccountType = !string.IsNullOrEmpty(accountType) ? (ZString)accountType : bankAccount.AB_AccountType;
			bankAccount.AB_Code = code;
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_Desc = desc;
			bankAccount.AB_AG = gLHeader.PK;
			bankAccount.AB_BankName = desc;
			bankAccount.AB_BankAbbreviation = GetRandomString(3);
			bankAccount.AB_BSB = GetRandomString(10);
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			return bankAccount;
		}

		#endregion

		#region Random

		public static string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		static Random Generator
		{
			get { return generator ?? (generator = new Random()); }
		}
		[ThreadStatic]
		static Random generator;

		#endregion

		#region Reporting Book

		public AccAlternateChart CreateAlternateChart(string code, string description = "description", bool isFixedLength = true,
			 bool isGlobal = false, string balancesheetStyle = BalanceSheetStyleCode.ELA, string reportOrder = "PTB", Guid? secondReportStartAccount = null)
		{
			var chart = Factory.New<AccAlternateChart>();
			chart.AAC_Code = code;
			chart.AAC_Description = description;
			chart.AAC_IsFixedLength = isFixedLength;
			chart.AAC_IsGlobal = isGlobal;
			chart.AAC_BalanceSheetStyle = balancesheetStyle;
			chart.AAC_AGA_SecondReportStartAccount = secondReportStartAccount ?? ZGuid.Empty;
			chart.AAC_ReportOrder = reportOrder;
			return chart;
		}

		public AccAlternateChartFormat CreateAccAlternateChartFormat(AccAlternateChart chart, ZShort tier, string format,
			string description = "test", string separator = ".")
		{
			var chartFormat = chart.AlternateChartFormats.AddNew();
			chartFormat.ANF_Tier = tier;
			chartFormat.ANF_AAC_AlternateChart = chart.PK;
			chartFormat.ANF_Format = format;
			chartFormat.ANF_Description = description;
			chartFormat.ANF_Separator = separator;
			return chartFormat;
		}

		public AccReportingBook CreateAccReportingBook(string code, ZGuid chartPK, ZGuid companyOfPeriodPK, string presentationJournals = "", string description = "test", string currency = "", string rateType = "", bool isActive = true, bool isGlobal = true)
		{
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_Code = code;
			reportingBook.ARB_AAC_AlternateChart = chartPK;
			reportingBook.ARB_GC_CompanyOfPeriod = companyOfPeriodPK;
			reportingBook.ARB_IncludePresentationJournals = presentationJournals;
			reportingBook.ARB_Description = description;
			reportingBook.ARB_RX_NKCurrency = currency;
			reportingBook.ARB_IsActive = isActive;
			reportingBook.ARB_IsGlobal = isGlobal;

			return reportingBook;
		}

		public AccAlternateGLAccount CreateAccAlternateGlAccount(ZGuid chartPK, string accountNum, string accountType = Core.Constants.AccountType.BalanceSheetAccount,
			string drCR = "CR", int totalLevel = 1, string reportSection = "OV",
			int printSequence = 1, string description = "desc", ZGuid? alternateNum = null, ZGuid? percentNum = null, ZGuid? consolidate = null, ZGuid? totalReference = null)
		{
			var account = Factory.New<AccAlternateGLAccount>();
			account.AGA_AAC_AlternateChart = chartPK;
			account.AGA_AccountNum = accountNum;
			account.AGA_Description = accountNum;
			account.AGA_AccountType = accountType;
			account.AGA_DebitCredit = drCR;
			account.AGA_TotalLevel = totalLevel;
			account.AGA_ReportSection = reportSection;
			account.AGA_PrintSequence = printSequence;
			account.AGA_Description = description;
			account.AGA_AGA_AlternateNum = alternateNum ?? Guid.Empty;
			account.AGA_AGA_PercentNum = percentNum ?? Guid.Empty;
			account.AGA_AGA_ConsolidationNum = consolidate ?? Guid.Empty;
			account.AGA_AGA_HeaderDependsOnTotal = totalReference ?? Guid.Empty;
			return account;
		}

		public AccAlternateGLAccountAttribute CreateAccAlternateGlAccountAttribute(AccAlternateGLAccount account, ZGuid glHeaderPK, int sequence = 1, string attribute = "OCG", string attributeValue = "")
		{
			var attributeBO = account.AlternateGLAccountAttributes.AddNew();
			attributeBO.AAA_AAC_AlternateChart = account.AGA_AAC_AlternateChart;
			attributeBO.AAA_AG_GLHeader = glHeaderPK;
			attributeBO.AAA_Sequence = sequence;
			attributeBO.AAA_Attribute = attribute;
			attributeBO.AAA_Value = attributeValue;
			return attributeBO;
		}

		public AccAlternateGLAccountDissection CreateAccAlternateGLAccountDissection(AccGLHeader header, ZGuid chartPK, string attribute, bool isSeparateNumbering)
		{
			var dissection = header.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = chartPK;
			dissection.ADC_Attribute = attribute;
			dissection.ADC_SeparateNumbering = isSeparateNumbering;

			return dissection;
		}

		public AccTransactionLineDissectionAttribute CreateAccTransactionLineDissectionAttribute(AccTransactionLines line = null, string attribute = "", string attributeValue = "", Guid? attributeValueId = null)
		{
			var accTransactionLineDissectionAttribute = (AccTransactionLineDissectionAttribute)null;
			if (line != null)
			{
				accTransactionLineDissectionAttribute = line.AccTransactionLineDissectionAttributes.AddNew();
			}
			else
			{
				accTransactionLineDissectionAttribute = Factory.NewWithValidTestData<AccTransactionLineDissectionAttribute>();
			}

			if (!string.IsNullOrEmpty(attribute))
			{
				accTransactionLineDissectionAttribute.ALD_Attribute = attribute;
			}

			if (!string.IsNullOrEmpty(attributeValue))
			{
				accTransactionLineDissectionAttribute.ALD_AttributeValue = attributeValue;
			}

			if (attributeValueId != null)
			{
				accTransactionLineDissectionAttribute.ALD_AttributeValueID = attributeValueId ?? Guid.Empty;
			}

			return accTransactionLineDissectionAttribute;
		}

		#endregion

		public GlbBranch NonCurrentCompanyBranch
		{
			get
			{
				if (fNonCurrentCompanyBranch == null)
				{
					fNonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
				}

				return fNonCurrentCompanyBranch;
			}
		}
		GlbBranch fNonCurrentCompanyBranch;

		public GlbCompany NonCurrentCompany
		{
			get
			{
				if (fNonCurrentCompany == null)
				{
					fNonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbCompanySchema.GC_Code.Name });
				}

				return fNonCurrentCompany;
			}
		}
		GlbCompany fNonCurrentCompany;

		public AccGLHeader CreateAccGLHeader(string accountNum, string accountType = Core.Constants.AccountType.BalanceSheetAccount, string column = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, string cashFlowType = "", string units = "",
			string description = "Description", string debitCredit = Core.Constants.DebitCredit.Debit)
		{ 
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = accountNum;
			glHeader.AG_AccountType = accountType;
			glHeader.AG_Column = column;
			glHeader.AG_CashFlowType = cashFlowType;
			glHeader.AG_StatisticalUnits = units;
			glHeader.AG_Description = description;
			glHeader.AG_DebitCredit = debitCredit;
			return glHeader;
		}

		public AccAlternateChartCurrencyTranslation CreateAccAlternateChartCurrencyTranslation(AccAlternateChart chart, string currencyTranslationLevel = "", string exRateType = "", string accountType = "", Guid? alternateGLAccountPK = null)
		{
			var chartCurrencyTranslation = chart.AccAlternateChartCurrencyTranslations.AddNew();
			chartCurrencyTranslation.ART_Type = accountType;
			chartCurrencyTranslation.ART_AGA_AlternateAccount = alternateGLAccountPK ?? Guid.Empty;
			chartCurrencyTranslation.ART_CurrencyTranslationLevel = currencyTranslationLevel;
			chartCurrencyTranslation.ART_ExRateType = exRateType;
			return chartCurrencyTranslation;
		}

		public RefCurrency CreateRefCurrency(string code, bool isActive = true, string symbol = "", bool isSystem = true, string desc = "")
		{
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = code;
			currency.RX_IsActive = isActive;
			currency.RX_Symbol = symbol;
			currency.RX_IsSystem = isSystem;
			currency.RX_Desc = desc;
			return currency;
		}
	}
}
