using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public interface IPatternMatchDataManager
	{
		IMatchingOrganisation Organisation { get; }
		ZBool AllowPatternMatchesWithEmptyAddress { get; }

		IOrgPatternMatch CreateNewPatternMatch();
		void DeletePatternMatches(ZQuery query);
		IEnumerable<IOrgPatternMatch> GetPatternMatchesAlreadyLoaded();
		IEnumerable<IOrgPatternMatch> LoadPatternMatches(ZQuery query);
	}
}
