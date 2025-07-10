using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public interface ITaxFrameworkConfigurationHelper
	{
		CodeDescriptionPairList GetTaxAuthorities(ZString countryCode, ZString? taxAuthorityType = null);
		CodeDescriptionPairList GetTaxSystems(ZString countryCode, ZString taxSystemRegistrationLevel = default);
		TaxSystemsConfiguration GetTaxSystem(ZString taxSystemCode, BusinessObjectFactory factoryForCaching);
		ZBool HasAnyTaxConfigurationThatSupportsOrganisationRates(BusinessObjectFactory factory, GlbCompany company);
		ZBool HasAnyActiveAccTaxConfiguration(BusinessObjectFactory factory, GlbCompany glbCompany, ZString ledger);
		AccTaxConfigurationCollection GetTaxConfigurationThatSupportsOrganisationRates(BusinessObjectFactory factory, GlbCompany company);
		ZBool HasAnyAccTaxConfiguration(BusinessObjectFactory factory, GlbCompany glbCompany);
		AccTaxConfigurationCollection GetCompanyTaxConfigurations(BusinessObjectFactory factory, GlbCompany company, ZQuery additionalFilter = null);
		ZQuery GetCompanyTaxOverrideGroup(ZQuery taxOverrideGroupQuery, GlbCompany company);
		ZBool IsCompanyLevelTaxSystemConfigured(GlbCompany company);
		ZBool IsBranchLevelTaxSystemConfigured(GlbBranch branch);
	}

	public class TaxFrameworkConfigurationHelper : ITaxFrameworkConfigurationHelper
	{
		CodeDescriptionPairList ITaxFrameworkConfigurationHelper.GetTaxAuthorities(ZString countryCode, ZString? taxAuthorityType)
		{
			var taxAuthorities = new CodeDescriptionPairList();
			var taxAuthoritiesConfigurations = AccountingMasterFilesRegistry.Instance.TaxAuthorities.Value.Cast<TaxAuthoritiesConfiguration>().Where(x => x.Country == countryCode);

			if (taxAuthorityType.HasValue && !taxAuthorityType.Value.IsEmpty)
			{
				taxAuthoritiesConfigurations = taxAuthoritiesConfigurations.Where(x => x.TaxAuthorityType == taxAuthorityType.Value);
			}

			taxAuthorities.AddRange(taxAuthoritiesConfigurations.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Name.ToString())).ToList());

			return taxAuthorities;
		}

		CodeDescriptionPairList ITaxFrameworkConfigurationHelper.GetTaxSystems(ZString countryCode, ZString taxSystemRegistrationLevel)
		{
			var taxSystems = new CodeDescriptionPairList();
			var taxSystemsConfigurations = GetTaxSystemsEnumerable(countryCode, taxSystemRegistrationLevel);
			taxSystems.AddRange(taxSystemsConfigurations.Select(taxSystem => new CodeDescriptionPair(taxSystem.Code.ToString(), taxSystem.Name.ToString())).ToList());

			return taxSystems;
		}

		TaxSystemsConfiguration ITaxFrameworkConfigurationHelper.GetTaxSystem(ZString taxSystemCode, BusinessObjectFactory factoryForCaching) =>
			GetTaxSystemsByCode(factoryForCaching).TryGetValue(taxSystemCode, out TaxSystemsConfiguration taxSystemWithCode) ? taxSystemWithCode : null;

		AccTaxConfigurationCollection ITaxFrameworkConfigurationHelper.GetCompanyTaxConfigurations(BusinessObjectFactory factory, GlbCompany company, ZQuery additionalFilter)
		{
			Argument.NotNull(company, nameof(company));

			ZQuery query = GetCompanyAndBranchTaxConfigurationQuery(company);

			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}

			return new AccTaxConfigurationCollection(factory, query);
		}

		AccTaxConfigurationCollection ITaxFrameworkConfigurationHelper.GetTaxConfigurationThatSupportsOrganisationRates(BusinessObjectFactory factory, GlbCompany company)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(company, nameof(company));

			var allowedTaxRateSourceCodes = new HashSet<ZString> { TaxRateSources.OrganisationOnly.Code, TaxRateSources.OrganisationFallbackToTaxGroup.Code, TaxRateSources.OrganisationFallbackToTaxID.Code };

			var taxSystemCodes = AccountingMasterFilesRegistry.Instance.TaxSystems.Value
				.Cast<TaxSystemsConfiguration>()
				.Where(tsc => allowedTaxRateSourceCodes.Contains(tsc.TaxRateSource))
				.Select(tsc => tsc.Code)
				.ToList();

			var query = new ZQuery(AccTaxConfigurationSchema.ETC_TaxSystemCode, taxSystemCodes)
				.AddToFilter(AccTaxConfigurationSchema.ETC_IsActive, true);

			return ((ITaxFrameworkConfigurationHelper)this).GetCompanyTaxConfigurations(factory, company, query);
		}

		ZQuery ITaxFrameworkConfigurationHelper.GetCompanyTaxOverrideGroup(ZQuery taxOverrideGroupQuery, GlbCompany company)
		{
			taxOverrideGroupQuery.AddToFilter(AccTaxOverrideGroupSchema.AX_RN_NKCountry, company.GC_RN_NKCountryCode);

			var taxConfigSubQuery = new ZDBOnlySubQuery(typeof(AccTaxConfiguration), AccTaxConfigurationSchema.PK);
			taxConfigSubQuery.AddToFilter(GetCompanyAndBranchTaxConfigurationQuery(company));

			var taxOverrideGroupPivotSubQuery = new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupTaxConfigurationPivot), AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup);
			taxOverrideGroupPivotSubQuery.AddSubQuery(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_ETC_TaxConfiguration, taxConfigSubQuery, JoinCondition.And);

			var accTaxOverrideGroupQuery = new ZDBOnlyQuery(typeof(AccTaxOverrideGroup));
			accTaxOverrideGroupQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupTaxConfigurationPivot), AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup, true), JoinCondition.And);
			accTaxOverrideGroupQuery.AddSubQuery(taxOverrideGroupPivotSubQuery, JoinCondition.Or);
			taxOverrideGroupQuery.AddToFilter(accTaxOverrideGroupQuery);

			return taxOverrideGroupQuery;
		}

		ZBool ITaxFrameworkConfigurationHelper.HasAnyAccTaxConfiguration(BusinessObjectFactory factory, GlbCompany glbCompany)
		{
			Argument.NotNull(factory, nameof(factory));
			return factory.Exists(typeof(AccTaxConfiguration), ((ITaxFrameworkConfigurationHelper)this).GetCompanyTaxConfigurations(factory, glbCompany, null).CompleteFilter);
		}

		ZBool ITaxFrameworkConfigurationHelper.HasAnyActiveAccTaxConfiguration(BusinessObjectFactory factory, GlbCompany glbCompany, ZString ledger)
		{
			Argument.NotNull(factory, nameof(factory));

			var query = new ZQuery(AccTaxConfigurationSchema.ETC_Ledger, ledger)
				.AddToFilter(AccTaxConfigurationSchema.ETC_IsActive, true);

			return factory.Exists(typeof(AccTaxConfiguration), ((ITaxFrameworkConfigurationHelper)this).GetCompanyTaxConfigurations(factory, glbCompany, query).CompleteFilter);
		}

		ZBool ITaxFrameworkConfigurationHelper.HasAnyTaxConfigurationThatSupportsOrganisationRates(BusinessObjectFactory factory, GlbCompany company)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(company, nameof(company));

			return factory.Exists(typeof(AccTaxConfiguration), ((ITaxFrameworkConfigurationHelper)this).GetTaxConfigurationThatSupportsOrganisationRates(factory, company).CompleteFilter);
		}
		ZBool ITaxFrameworkConfigurationHelper.IsCompanyLevelTaxSystemConfigured(GlbCompany company)
		{
			Argument.NotNull(company, nameof(company));

			var country = AccTaxConfiguration.GetCountryFromCompany(company);
			var registrationLevel = TaxSystemRegistrationLevels.Company.Code;

			return IsTaxSystemConfigured(country, registrationLevel);
		}

		ZBool ITaxFrameworkConfigurationHelper.IsBranchLevelTaxSystemConfigured(GlbBranch branch)
		{
			Argument.NotNull(branch, nameof(branch));

			var country = AccTaxConfiguration.GetCountryFromBranch(branch);
			var registrationLevel = TaxSystemRegistrationLevels.Branch.Code;

			return IsTaxSystemConfigured(country, registrationLevel);
		}

		ZBool IsTaxSystemConfigured(ZString countryCode, ZString registrationLevel)
		{
			return GetTaxSystemsEnumerable(countryCode, registrationLevel).Any();
		}

		ZQuery GetCompanyAndBranchTaxConfigurationQuery(GlbCompany company)
		{
			var parentIdPKs = new List<ZGuid>(company.ActiveBranches.Select(x => x.PK));
			parentIdPKs.Add(company.PK);

			var query = new ZQuery(AccTaxConfigurationSchema.ETC_ParentId, parentIdPKs);

			return query;
		}

		IEnumerable<TaxSystemsConfiguration> GetTaxSystemsEnumerable(ZString countryCode, ZString taxSystemRegistrationLevel = default) =>
			AccountingMasterFilesRegistry.Instance.TaxSystems.Value.Cast<TaxSystemsConfiguration>()
			.Where(taxSystem => taxSystem.Country == countryCode
					&& (taxSystemRegistrationLevel.IsDefault || taxSystem.RegistrationLevel == taxSystemRegistrationLevel));

		Dictionary<ZString, TaxSystemsConfiguration> GetTaxSystemsByCode(BusinessObjectFactory factoryForCaching) =>
			Argument.NotNull(factoryForCaching, nameof(factoryForCaching)).GetCachedValue("TaxSystemsByCode",
				() => AccountingMasterFilesRegistry.Instance.TaxSystems.Value.Cast<TaxSystemsConfiguration>().ToDictionary(taxSystem => taxSystem.Code));
	}
}
