using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public interface IMatchingOrganisation : IMatchingOrganisationCollections
	{
		ZGuid PK { get; }
		ZString OH_Code { get; }
		ZString OH_FullName { get; }
		ZString OH_Language { get; }
		ZString OH_RL_NKClosestPort { get; }
		ZBool OH_IsActive { get; }
		ZBool OH_RL_NKClosestPortInfoHasChanges { get; }

		IDisposable CacheMainAddress();
		ZString PortName { get; }
		MultilingualString CountryName { get; }

		ZBool PatternMatchRequiresFullRegen { get; set; }
		ZBool PatternMatchRequiresRegen { get; set; }
		PatternMatchingOriginalCollections OriginalCollections { get; }

		bool IsInDatabase { get; }
	}

	public interface IMatchingOrganisationCollections
	{
		IEnumerable<IMatchingAddress> Addresses { get; }
		IEnumerable<IMatchingCusCode> CustomsCodes { get; }
		IEnumerable<OrganisationName> OrganisationNamesExceptBrands { get; }
		IEnumerable<OrganisationName> OrganisationNamesFromBrands { get; }
	}

	public static class IMatchingOrganisationCollectionsExtensions
	{
		public static IEnumerable<OrganisationName> AllOrganisationNames(this IMatchingOrganisationCollections organisation)
		{
			var organisationNamesExceptBrands = organisation.OrganisationNamesExceptBrands ?? new List<OrganisationName>();
			var organisationNamesFromBrands = organisation.OrganisationNamesFromBrands ?? new List<OrganisationName>();
			return organisationNamesExceptBrands.Concat(organisationNamesFromBrands);
		}
	}
}
