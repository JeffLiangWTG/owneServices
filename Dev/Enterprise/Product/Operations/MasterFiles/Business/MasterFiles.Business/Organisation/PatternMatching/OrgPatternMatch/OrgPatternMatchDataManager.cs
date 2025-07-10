using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	class OrgPatternMatchDataManager : IPatternMatchDataManager
	{
		internal OrgPatternMatchDataManager(IMatchingOrganisation organisation, OrgPatternMatchCollection patternMatchCollection)
			: this(organisation, false, patternMatchCollection)
		{
		}

		internal OrgPatternMatchDataManager(IMatchingOrganisation organisation, ZBool allowPatternMatchesWithEmptyAddress, OrgPatternMatchCollection patternMatchCollection)
		{
			this.organisation = Argument.NotNull(organisation, "IMatchingOrganisation organisation");
			this.allowPatternMatchesWithEmptyAddress = allowPatternMatchesWithEmptyAddress;
			this.collection = Argument.NotNull(patternMatchCollection, "OrgPatternMatchCollection patternMatchCollection");

			this.factory = patternMatchCollection.Factory;
		}

		readonly IMatchingOrganisation organisation;
		readonly bool allowPatternMatchesWithEmptyAddress;

		readonly OrgPatternMatchCollection collection;
		readonly BusinessObjectFactory factory;

		public IMatchingOrganisation Organisation
		{
			get { return organisation; }
		}

		public ZBool AllowPatternMatchesWithEmptyAddress
		{
			get { return allowPatternMatchesWithEmptyAddress; }
		}

		public IOrgPatternMatch CreateNewPatternMatch()
		{
			return collection.AddNew();
		}

		public IEnumerable<IOrgPatternMatch> LoadPatternMatches(ZQuery query)
		{
			return factory.Load<OrgPatternMatch>(query);
		}

		public void DeletePatternMatches(ZQuery query)
		{
			foreach (var patternMatch in LoadPatternMatches(query))
			{
				patternMatch.Delete();
			}
		}

		public IEnumerable<IOrgPatternMatch> GetPatternMatchesAlreadyLoaded()
		{
			return collection.Cast<IOrgPatternMatch>().Where(match => !match.IsDeleted);
		}
	}
}
