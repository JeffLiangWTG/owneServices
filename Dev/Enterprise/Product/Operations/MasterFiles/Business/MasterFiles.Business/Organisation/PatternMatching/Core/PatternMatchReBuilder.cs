using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public class PatternMatchReBuilder
	{
		public PatternMatchReBuilder(IPatternMatchDataManager dataManager)
		{
			this.dataManager = Argument.NotNull(dataManager, "OrgPatternMatchDataManager dataManager");
		}

		readonly IPatternMatchDataManager dataManager;

		public void Generate(List<IMatchingAddress> addedAddressesOverride, List<OrganisationName> addedCompanyNamesOverride)
		{
			if (!dataManager.Organisation.OH_IsActive)
			{
				var query = new ZQuery(OrgPatternMatchSchema.OS_OH, dataManager.Organisation.PK);
				query.FetchOnlyFromLocalCache = !dataManager.Organisation.IsInDatabase;
				dataManager.DeletePatternMatches(query);
			}
			else
			{
				var originalOrganisationNamesExceptBrands = dataManager.Organisation.OrganisationNamesExceptBrands.ToList();
				var originalOrganisationNamesFromBrands = dataManager.Organisation.OrganisationNamesFromBrands.ToList();
				var originalOrganisationNamesAll = originalOrganisationNamesExceptBrands.Union(originalOrganisationNamesFromBrands);
				var originalAddresses = dataManager.Organisation.Addresses.ToList();
				var originalCusCodes = dataManager.Organisation.CustomsCodes.ToList();

				var newCompanyNames = addedCompanyNamesOverride ?? GetNewCompanyNames(dataManager.Organisation.PatternMatchRequiresFullRegen);
				var deletedCompanyNames = addedCompanyNamesOverride == null ? GetDeletedOrModifiedCompanyNamesAndBrands(dataManager.Organisation.PatternMatchRequiresFullRegen) : new List<OrganisationName>();

				var deletedOrgAddresses = addedAddressesOverride == null ? GetDeletedOrModifiedOrgAddresses(dataManager.Organisation.PatternMatchRequiresFullRegen) : new List<IMatchingAddress>();
				var newOrModifiedOrgAddresses = addedAddressesOverride ?? GetNewOrModifiedOrgAddresses(dataManager.Organisation.PatternMatchRequiresFullRegen);

				var deletedOrgCusCodes = GetDeletedOrgCusCodes(dataManager.Organisation.PatternMatchRequiresFullRegen);
				var newOrModifiedOrgCusCodes = GetNewOrModifiedOrgCusCodes(dataManager.Organisation.PatternMatchRequiresFullRegen);

				if (dataManager.Organisation.OH_RL_NKClosestPortInfoHasChanges)
				{
					HandleMainUNLOCOChange();
				}

				bool deleteMatchesWithoutCusCode = newOrModifiedOrgCusCodes.Any(code => code.OK_CodeType.IsAcceptedForMatching());
				CheckCusCodesForRegNumbersRegen(newOrModifiedOrgCusCodes, deletedOrgCusCodes);

				using (DisposableList disposableList = new DisposableList(originalAddresses.Count + 1))
				{
					disposableList.Add(dataManager.Organisation.CacheMainAddress());
					disposableList.AddRange(dataManager.Organisation.Addresses.Select(address => address.CachePortAndCountryNames()));

					new PatternMatchDeleter(dataManager).DeleteOldPatterns(deletedCompanyNames, deletedOrgAddresses, deletedOrgCusCodes, deleteMatchesWithoutCusCode);

					var addedMatches = new Dictionary<string, Dictionary<IMatchingAddress, object>>();

					var matchCreator = new PatternMatchCreator(dataManager);
					matchCreator.CreateMatchesForAddressesAndCompanyNames(newCompanyNames, originalAddresses, addedMatches);
					matchCreator.CreateMatchesForAddressesAndCompanyNames(originalOrganisationNamesAll, newOrModifiedOrgAddresses, addedMatches);
					matchCreator.CreateMatchesForCusCodes(newOrModifiedOrgCusCodes, addedMatches);
				}

				dataManager.Organisation.OriginalCollections.OrganisationNamesExceptBrands = originalOrganisationNamesExceptBrands;
				dataManager.Organisation.OriginalCollections.OrganisationNamesFromBrands = originalOrganisationNamesFromBrands;
				dataManager.Organisation.OriginalCollections.Addresses = originalAddresses;
				dataManager.Organisation.OriginalCollections.CustomsCodes = originalCusCodes;
			}

			dataManager.Organisation.PatternMatchRequiresRegen = false;
			dataManager.Organisation.PatternMatchRequiresFullRegen = false;
		}

		void CheckCusCodesForRegNumbersRegen(IEnumerable<IMatchingCusCode> newOrModifiedOrgCusCodes, IEnumerable<IMatchingCusCode> deletedCusCodes)
		{
			foreach (IMatchingCusCode code in newOrModifiedOrgCusCodes)
			{
				if (code.IsInDatabase && !code.OK_CustomsRegNo.IsEmpty && (code.OK_CustomsRegNoHasChanges || code.OK_CodeTypeHasChanges) && code.OK_CodeType.IsAcceptedForMatching())
				{
					string businessRegNo = OrgPatternMatchGenerationHelper.SanitisedBusinessRegistrationNumber(code.OK_CustomsRegNoOriginalValue, OrgPatternMatchSchema.OS_BusinessRegNo.MaxLength);
					foreach (var match in dataManager.LoadPatternMatches(new ZQuery(OrgPatternMatchSchema.OS_BusinessRegNo, businessRegNo)))
					{
						match.OS_BusinessRegNo = OrgPatternMatchGenerationHelper.SanitisedBusinessRegistrationNumber(code.OK_CustomsRegNo, OrgPatternMatchSchema.OS_BusinessRegNo.MaxLength);
					}
				}
			}

			if (!dataManager.Organisation.CustomsCodes.Any(cusCode => cusCode.OK_CodeType.IsAcceptedForMatching()))
			{
				foreach (IMatchingCusCode code in deletedCusCodes)
				{
					if (code.IsInDatabase && code.OK_CodeTypeOriginalValue.IsAcceptedForMatching())
					{
						string businessRegNo = OrgPatternMatchGenerationHelper.SanitisedBusinessRegistrationNumber(code.OK_CustomsRegNoOriginalValue, OrgPatternMatchSchema.OS_BusinessRegNo.MaxLength);
						foreach (var match in dataManager.LoadPatternMatches(new ZQuery(OrgPatternMatchSchema.OS_BusinessRegNo, businessRegNo)))
						{
							match.OS_BusinessRegNo = ZString.Empty;
						}
						break;
					}
				}
			}
		}

		void HandleMainUNLOCOChange()
		{
			ZQuery query = new ZQuery(OrgPatternMatchSchema.OS_OA, dataManager.Organisation.OriginalCollections.Addresses.Where(adr => !adr.IsDeleted && adr.OA_RL_NKRelatedPortCode.IsEmpty).Select(adr => adr.PK));
			query.FetchOnlyFromLocalCache = !dataManager.Organisation.IsInDatabase;
			foreach (OrgPatternMatch match in dataManager.LoadPatternMatches(query))
			{
				match.OS_UNLOCO = dataManager.Organisation.OH_RL_NKClosestPort;
			}
		}

		#region CompanyNames

		IEnumerable<OrganisationName> GetNewCompanyNames(bool getAll)
		{
			var allOrganisationNames = dataManager.Organisation.AllOrganisationNames();
			return getAll
				? allOrganisationNames
					.ToList()
				: allOrganisationNames
					.Except(dataManager.Organisation.OriginalCollections.AllOrganisationNames())
					.ToList();
		}

		IEnumerable<OrganisationName> GetDeletedOrModifiedCompanyNamesAndBrands(bool getAll)
		{
			var originalAllOrganisationNames = dataManager.Organisation.OriginalCollections.AllOrganisationNames();
			return getAll
				? originalAllOrganisationNames
				: originalAllOrganisationNames
					.Except(dataManager.Organisation.AllOrganisationNames())
					.ToList();
		}

		#endregion

		#region Org Addresses

		List<IMatchingAddress> GetNewOrModifiedOrgAddresses(bool getAll)
		{
			var originalAddresses = dataManager.Organisation.OriginalCollections.Addresses;
			if (originalAddresses != null)
			{
				var currentAddresses = dataManager.Organisation.Addresses;
				if (getAll)
				{
					return currentAddresses.Where(adr => adr.OA_IsActive).ToList();
				}

				var originalAddressesHashSet = new HashSet<IMatchingAddress>(originalAddresses);

				return currentAddresses
					.Where(adr => adr.OA_IsActive && (adr.HasChanges || adr.OA_RL_NKRelatedPortCodeInfoHasChanges || !originalAddressesHashSet.Contains(adr)))
					.ToList();
			}

			return new List<IMatchingAddress>();
		}

		List<IMatchingAddress> GetDeletedOrModifiedOrgAddresses(bool getAll)
		{
			var originalAddresses = dataManager.Organisation.OriginalCollections.Addresses;
			if (originalAddresses != null)
			{
				if (getAll)
				{
					return originalAddresses.ToList();
				}

				var orgWithAddressesNoAutoCreate = dataManager.Organisation as IOrgHeaderWithAddressesNoAutoCreate;
				var currentAddresses = orgWithAddressesNoAutoCreate != null ? orgWithAddressesNoAutoCreate.AddressesNoAutoCreate : dataManager.Organisation.Addresses;

				var currentAddressesHashSet = new HashSet<IMatchingAddress>(currentAddresses);

				return originalAddresses
					.Where(adr => adr.HasChanges || adr.OA_RL_NKRelatedPortCodeInfoHasChanges || !currentAddressesHashSet.Contains(adr))
					.ToList();
			}

			return new List<IMatchingAddress>();
		}

		#endregion

		#region Cus Codes

		List<IMatchingCusCode> GetNewOrModifiedOrgCusCodes(bool getAll)
		{
			if (dataManager.Organisation.OriginalCollections.CustomsCodes != null)
			{
				return getAll
					? dataManager.Organisation.CustomsCodes.ToList()
					: dataManager.Organisation.CustomsCodes.Where(code => code.HasChanges).ToList();
			}

			return new List<IMatchingCusCode>();
		}

		List<IMatchingCusCode> GetDeletedOrgCusCodes(bool getAll)
		{
			var originalCustomsCodes = dataManager.Organisation.OriginalCollections.CustomsCodes;
			if (originalCustomsCodes != null)
			{
				return getAll
					? originalCustomsCodes.ToList()
					: originalCustomsCodes.Except(dataManager.Organisation.CustomsCodes).ToList();
			}

			return new List<IMatchingCusCode>();
		}

		#endregion
	}
}
