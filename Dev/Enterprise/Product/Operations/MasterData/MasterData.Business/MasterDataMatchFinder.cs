using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public static class MasterDataMatchFinder
	{
		public static IEnumerable<OrgHeader> GetOrgHeaderByEmailDomain(string emailAddress)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var hashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(emailAddress));
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				var patternMatchingDomainSubQuery = new ZDBOnlySubQuery(typeof(PatternMatchingDomain), PatternMatchingDomainSchema.PMD_OH, OrgHeaderSchema.PK);
				patternMatchingDomainSubQuery.AddToFilter(PatternMatchingDomainSchema.PMD_HashedValue, hashValue);
				query.AddSubQuery(patternMatchingDomainSubQuery, JoinCondition.And);
				return factory.Load<OrgHeader>(query);
			}
		}

		public static IEnumerable<GlbPerson> GetGlbPeronByEmailDomain(string emailAddress)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var hashValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(emailAddress));
				var query = new ZDBOnlyQuery(typeof(GlbPerson));
				var patternMatchingDomainSubQuery = new ZDBOnlySubQuery(typeof(PatternMatchingDomain), PatternMatchingDomainSchema.PMD_PER, GlbPersonSchema.PK);
				patternMatchingDomainSubQuery.AddToFilter(PatternMatchingDomainSchema.PMD_HashedValue, hashValue);
				query.AddSubQuery(patternMatchingDomainSubQuery, JoinCondition.And);
				return factory.Load<GlbPerson>(query);
			}
		}
	}
}
