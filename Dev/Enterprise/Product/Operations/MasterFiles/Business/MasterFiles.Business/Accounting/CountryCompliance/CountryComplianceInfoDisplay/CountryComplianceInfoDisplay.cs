using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay
{
	public class CountryComplianceInfoDisplay : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CountryComplianceInfoDisplay() : this(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
		{
		}

		public CountryComplianceInfoDisplay(string countryCode) : base(new ReadOnlyBusinessObjectFactory())
		{
			CountryCode = countryCode;
		}

		#region CountryCode

		[List(nameof(Countries))]
		[ResourceStringData("C572CF40-05E3-460E-B54D-C079C1A60E9C", Caption = "Country/Region", ShortCaption = "Ctry/Rgn.")]
		[MaxLength(nameof(MaxCountryCodeLength))]
		[BusinessObjectTestExclude]
		public ZString CountryCode
		{
			get => countryCode;
			set
			{
				SetCountryCodeAndValidate(value);
				if (CountryCodeInfo.HasErrors())
				{
					SetCountryCodeAndValidate(lastValidCountryCode);
				}
				else
				{
					lastValidCountryCode = countryCode;
				}

				OrgCusCode.OK_RN_NKCodeCountry = CountryCode;
				LoadTaxes();
				LoadComplianceSubTypes();
				LoadComplianceSubTypeAttributionRules();
				LoadOrgCusCodeTypes();
				LoadTaxSystems();
				LoadTaxAuthorities();
				LoadCountrySpecificRegistryDefaultValue();
				RefreshBinding();
			}
		}
		ZString countryCode;
		ZString lastValidCountryCode;

		public ZPropertyInfo CountryCodeInfo => GetZPropertyInfo(nameof(CountryCode));

		void SetCountryCodeAndValidate(ZString value)
		{
			countryCode = value;
			CountryCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryCodeInfo);
			ListValidation.ErrorIfInvalidCode(CountryCodeInfo);
		}

		public RefCountryCollection Countries
		{
			get
			{
				var collection = new RefCountryCollection(Factory);
				collection.ApplySort(AutoRefCountry.Schema.RN_Desc, ListSortDirection.Ascending);

				return collection;
			}
		}

		int MaxCountryCodeLength => RefCountrySchema.RN_Code.MaxLength;

		#endregion

		#region Details

		[ResourceStringData("E8181237-6AB0-4D08-AA2A-CA5006134E08", Caption = "Is Supported By The License System")]
		public ZString IsSupportedForLicenceBuilder => GetValueSafe(() => Country.IsSupportedForLicenceBuilder(CountryCode));

		[ResourceStringData("1C202922-448D-408C-AD32-0425114E5AA5", Caption = "Consumption Tax Code")]
		public ZString ConsumptionTaxCode => GetValueSafe(() => Country.GetConsumptionTaxDescription(CountryCode));

		[ResourceStringData("41DA2CAA-0918-4017-AE42-8B8F0A9F53D5", Caption = "Consumption Tax Registration Code")]
		public ZString ConsumptionTaxRegistrationCode => GetValueSafe(() => Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode));

		[ResourceStringData("EF9A36A1-6453-4343-8AE2-E4D09DA2AFCB", Caption = "Consumption Tax Registration Code Description")]
		public ZString ConsumptionTaxRegistrationCodeDescription => GetValueSafe(() =>
		{
			// Get all codes, and grab the description from the consumption tax code directly from the codes list
			var codes = new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, CountryCode));
			return codes.GetDescriptionFromCode(Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode));
		});

		[ResourceStringData("4182819F-B0A8-4242-9061-DD63D99CB74F", Caption = "Local Business Reg No Code Type")]
		public ZString LocalBusinessRegNoCodeType => GetValueSafe(() => CountryComplianceInfo.GetLocalBusinessRegNoCodeType(CountryCode));

		[ResourceStringData("2D3F2849-FB12-4DD1-8E4A-50DECB72FFD2", Caption = "Is Reciprocal")]
		public ZString IsReciprocal => GetValueSafe(() => Country.IsReciprocal(CountryCode));

		[ResourceStringData("29D7F16F-14E2-45B6-A6CD-A66804A85F85", Caption = "Is Right Hand Side Address Country/Region")]
		public ZString IsRightHandSideAdressCountry => CountryComplianceInfo.GetIsRightHandSideAdressCountry(CountryCode).ToString();

		[ResourceStringData("83183B25-10BB-42C7-BB53-B60C78FF453D", Caption = "Default Value For Enable Government Charge Code Registry")]
		public ZString DefaultValueForEnableGovernmentChargeCodeRegistry => GetValueSafe(() => CountryComplianceFactory.GetIComplianceRegistryDefaultProvider(countryCode)?.GetDefaultValueForEnableGovernmentChargeCodeRegistry() ?? false);

		#endregion

		#region Taxes

		[ResourceStringData("3445AE6F-7EA3-4969-B2C5-9748F8E307DE", Caption = "Show For All Countries/Regions")]
		public ZBool ShowTaxIdsForAllCountries
		{
			get => showTaxIdsForAllCountries;
			set
			{
				showTaxIdsForAllCountries = value;
				LoadTaxes();
			}
		}
		ZBool showTaxIdsForAllCountries;

		void LoadTaxes()
		{
			LoadTaxIds();
			LoadTaxRates();
		}

		public TaxRateConfigurationCollection TaxIDs => taxIDs ?? (taxIDs = new TaxRateConfigurationCollection());
		TaxRateConfigurationCollection taxIDs;

		void LoadTaxIds() => TaxIDs.LoadTaxIds(ShowTaxIdsForAllCountries ? null : CountryCode.ToString());

		public ActiveBusinessObjectCollection<RefAccTaxRate> TaxRates => taxRates ?? (taxRates = new ActiveBusinessObjectCollection<RefAccTaxRate>(Factory));
		ActiveBusinessObjectCollection<RefAccTaxRate> taxRates;
		void LoadTaxRates()
		{
			TaxRates.AdditionalFilter = ShowTaxIdsForAllCountries ? new ZQuery() : new ZQuery(RefAccTaxRateSchema.ZAT_RN_NKCountry, CountryCode);
			TaxRates.ApplySort(RefAccTaxRateSchema.Constants.ZAT_RN_NKCountry, ListSortDirection.Ascending);
		}

		#endregion

		#region ComplinaceSubTypes

		[ResourceStringData("ECE71CE8-80BC-4FE0-8B27-ACB350510FA9", Caption = "Show For All Countries/Regions")]
		public ZBool ShowComplianceSubTypesForAllCountries
		{
			get => showComplianceSubTypesForAllCountries;
			set
			{
				showComplianceSubTypesForAllCountries = value;
				LoadComplianceSubTypes();
			}
		}
		ZBool showComplianceSubTypesForAllCountries;

		public ComplianceSubTypeDisplayCollection ComplianceSubTypes => complianceSubTypes ?? (complianceSubTypes = new ComplianceSubTypeDisplayCollection());
		ComplianceSubTypeDisplayCollection complianceSubTypes;

		void LoadComplianceSubTypes()
		{
			ComplianceSubTypes.LoadComplianceSubTypes(ShowComplianceSubTypesForAllCountries ? Countries.Select(x => x.RN_Code).ToArray() : new[] { CountryCode });
		}

		[ResourceStringData("ECE71CE8-80BC-4FE0-8B27-ACB350510FA9", Caption = "Show For All Countries/Regions")]
		public ZBool ComplianceSubTypeAttributionRulesForAllCountries
		{
			get => complianceSubTypeAttributionRulesForAllCountries;
			set
			{
				complianceSubTypeAttributionRulesForAllCountries = value;
				LoadComplianceSubTypeAttributionRulesShowAllCountry();
			}
		}
		ZBool complianceSubTypeAttributionRulesForAllCountries;

		public ComplianceSubTypeAttributionRuleConfigurationCollection ComplianceSubTypeAttributionRules
		{
			get
			{
				if (complianceSubTypeAttributionRules == null)
				{
					complianceSubTypeAttributionRules = new ComplianceSubTypeAttributionRuleConfigurationCollection();
					complianceSubTypeAttributionRules.SuspendValidation();
				}

				return complianceSubTypeAttributionRules;
			}
		}

		ComplianceSubTypeAttributionRuleConfigurationCollection complianceSubTypeAttributionRules;

		void LoadComplianceSubTypeAttributionRules()
		{
			if (!ComplianceSubTypeAttributionRulesForAllCountries)
			{
				ComplianceSubTypeAttributionRules.RemoveAll();
				LoadComplianceSubTypeAttributeRulesByCountry(CountryCode);
			}
		}

		void LoadComplianceSubTypeAttributionRulesShowAllCountry()
		{
			ComplianceSubTypeAttributionRules.RemoveAll();
			if (ComplianceSubTypeAttributionRulesForAllCountries)
			{
				foreach (var country in Countries)
				{
					LoadComplianceSubTypeAttributeRulesByCountry(country.RN_Code);
				}
			}
			else
			{
				LoadComplianceSubTypeAttributionRules();
			}
		}

		void LoadComplianceSubTypeAttributeRulesByCountry(ZString countryCode)
		{
			if (CountryComplianceFactory.GetIComplianceSubTypeRuleProvider(countryCode) != null)
			{
				CountryComplianceFactory.GetIComplianceSubTypeRuleProvider(countryCode).SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRules);
			}

			if (CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(countryCode) != null)
			{
				CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(countryCode).SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRules, null);
			}
		}

		[ResourceStringData("B1A22B21-71AB-4A80-807E-8D85B9157545", Caption = "Tax Registration Types")]
		public ZString ComplianceSubTypesTaxRegistrationTypes
		{
			get
			{
				var taxRegistrationTypes = ComplianceSubTypeCodesAndLists.Lists.GetTaxRegistrationTypeList(new ComplianceSubTypeAttributionRuleConfigurationLookups(new ComplianceSubTypeAttributionRuleConfiguration() { Country = CountryCode }), CountryCode);

				return string.Join(", ", taxRegistrationTypes.GetAllCodes());
			}
		}

		#endregion

		#region OrgCusCodes

		[ResourceStringData("A0B279A9-1517-4678-9277-153D643E12BF", Caption = "Code that cannot coexist in the country/region")]
		public ZString OrgCusCodesCannotCoexistInCountry
		{
			get
			{
				var codeSets = OrgCusCode.Validation.GetCodesCannotCoexistInParentCountryExposedForInternalDisplay();

				var stringBuilder = new ZStringBuilder();
				foreach (var codes in codeSets)
				{
					stringBuilder.AppendLine(string.Join(", ", codes));
				}

				return stringBuilder.ToString();
			}
		}

		OrgCusCode OrgCusCode
		{
			get
			{
				if (orgCusCode == null)
				{
					orgCusCode = Factory.New<OrgCusCode>();
					orgCusCode.SuspendValidation();
				}

				return orgCusCode;
			}
		}

		OrgCusCode orgCusCode;

		public OrgCusCodeTypeDisplayCollection OrgCusCodeTypes => orgCusCodeTypes ?? (orgCusCodeTypes = new OrgCusCodeTypeDisplayCollection());
		OrgCusCodeTypeDisplayCollection orgCusCodeTypes;

		void LoadOrgCusCodeTypes()
		{
			OrgCusCodeTypes.LoadTypes(OrgCusCode);
		}

		#endregion

		#region TaxSystems

		public TaxSystemsConfigurationCollection TaxSystems => taxSystems ?? (taxSystems = new TaxSystemsConfigurationCollection());
		TaxSystemsConfigurationCollection taxSystems;

		void LoadTaxSystems()
		{
			TaxSystems.RemoveAll();

			var registryDefaultValue = AccountingMasterFilesRegistry.Instance.TaxSystems.DefaultValue.OfType<TaxSystemsConfiguration>();
			TaxSystems.AddRange(registryDefaultValue.Where(x => showTaxSystemsForAllCountries || x.Country.EqualsIgnoringCase(CountryCode)));
		}

		[ResourceStringData("ECE71CE8-80BC-4FE0-8B27-ACB350510FA9", Caption = "Show For All Countries/Regions")]
		public ZBool ShowTaxSystemsForAllCountries
		{
			get => showTaxSystemsForAllCountries;
			set
			{
				showTaxSystemsForAllCountries = value;
				LoadTaxSystems();
			}
		}
		ZBool showTaxSystemsForAllCountries;

		#endregion

		#region TaxAuthorities 

		public TaxAuthoritiesConfigurationCollection TaxAuthorities => taxAuthorities ?? (taxAuthorities = new TaxAuthoritiesConfigurationCollection());
		TaxAuthoritiesConfigurationCollection taxAuthorities;

		void LoadTaxAuthorities()
		{
			TaxAuthorities.RemoveAll();

			var registryDefaultValue = AccountingMasterFilesRegistry.Instance.TaxAuthorities.DefaultValue.OfType<TaxAuthoritiesConfiguration>();
			TaxAuthorities.AddRange(registryDefaultValue.Where(x => showTaxAuthoritiesForAllCountries || x.Country.EqualsIgnoringCase(CountryCode)));
		}

		public ZBool ShowTaxAuthoritiesForAllCountries
		{
			get => showTaxAuthoritiesForAllCountries;
			set
			{
				showTaxAuthoritiesForAllCountries = value;
				LoadTaxAuthorities();
			}
		}
		ZBool showTaxAuthoritiesForAllCountries;

		#endregion

		#region CountrySpecificRegistryDefaultValues 

		//To add new registries, the need to be refactored as per: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/4341/Accounting-registry-with-country-specific-default-values
		public CountrySpecificRegistryDefaultValueCollection CountrySpecificRegistryDefaultValues => countrySpecificRegistryDefaultValues ?? (countrySpecificRegistryDefaultValues = new CountrySpecificRegistryDefaultValueCollection());
		CountrySpecificRegistryDefaultValueCollection countrySpecificRegistryDefaultValues;

		void LoadCountrySpecificRegistryDefaultValue()
		{
			CountrySpecificRegistryDefaultValues.LoadCountrySpecificRegistryDefaultValue(CountryCode);
		}

		#endregion

		ZString GetValueSafe<T>(Func<T> getValue)
		{
			try
			{
				return getValue()?.ToString() ?? "N/A";
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}
		}
	}
}
