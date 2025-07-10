using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	class PatternMatchDeleter
	{
		internal PatternMatchDeleter(IPatternMatchDataManager dataManager)
		{
			this.dataManager = Argument.NotNull(dataManager, "OrgPatternMatchDataManager dataManager");
		}

		readonly IPatternMatchDataManager dataManager;

		internal void DeleteOldPatterns(IEnumerable<StringWithLanguage> deletedCompanyNames, IEnumerable<IMatchingAddress> deletedOrgAddresses, IEnumerable<IMatchingCusCode> deletedOrgCusCodes, bool deleteAllWithoutCusCode)
		{
			var companyNames = deletedCompanyNames.
				Select(
					companyName => OrgPatternMatchGenerationHelper
						.Get(companyName.LanguageCode, dataManager.Organisation.PortName, dataManager.Organisation.CountryName)
						.SuccinctCompanyName(companyName.Value, true)
					).ToList();

			var orgAddresses = deletedOrgAddresses.Select(adr => adr.PK).ToList();

			var orgCusCodes = deletedOrgCusCodes
				.Where(code => code.IsInDatabase && code.OK_CodeTypeOriginalValue.IsAcceptedForMatching())
				.Select(code => OrgPatternMatchGenerationHelper.SanitisedBusinessRegistrationNumber(code.OK_CustomsRegNoOriginalValue, OrgPatternMatchSchema.OS_BusinessRegNo.MaxLength))
				.ToList();

			if (companyNames.Count > 0 || orgAddresses.Count > 0 || orgCusCodes.Count > 0 || deleteAllWithoutCusCode)
			{
				dataManager.DeletePatternMatches(BuildDeletionQuery(companyNames, orgAddresses, orgCusCodes, deleteAllWithoutCusCode));
			}
		}

		ZQuery BuildDeletionQuery(List<string> companyNames, List<ZGuid> orgAddresses, List<string> orgCusCodes, bool deleteAllWithoutCusCode)
		{
			ZQuery query = new ZQuery();
			if (companyNames.Count > 0)
			{
				query.AddToFilter(JoinCondition.Or, OrgPatternMatchSchema.OS_FullCompanyName, companyNames);
			}

			if (orgAddresses.Count > 0)
			{
				query.AddToFilter(JoinCondition.Or, OrgPatternMatchSchema.OS_OA, orgAddresses);
			}

			if (orgCusCodes.Count > 0)
			{
				query.AddToFilter(JoinCondition.Or, OrgPatternMatchSchema.OS_BusinessRegNo, orgCusCodes);
			}

			if (deleteAllWithoutCusCode)
			{
				query.AddToFilter(JoinCondition.Or, OrgPatternMatchSchema.OS_BusinessRegNo, ZString.Empty);
			}

			query.AddToFilter(new ZQuery(OrgPatternMatchSchema.OS_OH, dataManager.Organisation.PK), JoinCondition.And);
			query.FetchOnlyFromLocalCache = !dataManager.Organisation.IsInDatabase;
			return query;
		}
	}
}
