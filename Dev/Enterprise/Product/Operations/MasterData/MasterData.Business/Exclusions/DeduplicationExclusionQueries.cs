using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public static class DeduplicationExclusionQueries
	{
		static ZQuery GetIgnoresAsMaster(ZDBOnlyQuery queryBase, BusinessObject bizo, string ignoreType, SchemaGuidColumn parentLookupColumn, SchemaGuidColumn childLookupColumn)
		{
			var queryForPig = new ZQuery(PatternMatchingResultSchema.PMT_Status, ignoreType);
			var conditionQuery = new ZQuery(PatternMatchingResultSchema.PMT_TargetTableCode, bizo.TablePrefix);

			conditionQuery.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix);

			var subQueryForIgnore = new ZDBOnlySubQuery(typeof(PatternMatchingResult), parentLookupColumn);
			subQueryForIgnore.AddToFilter(childLookupColumn, bizo.PK);
			subQueryForIgnore.AddToFilter(conditionQuery);
			subQueryForIgnore.AddToFilter(queryForPig);

			queryBase.AddSubQuery(subQueryForIgnore, JoinCondition.And);

			return queryBase;
		}

		public static ZQuery GetPermanentIgnoresAsMaster(ZDBOnlyQuery queryBase, BusinessObject bizo)
		{
			return GetIgnoresAsMaster(queryBase, bizo, PatternMatchingResult.StatusCodes.PermanentIgnore, PatternMatchingResultSchema.PMT_MasterPK, PatternMatchingResultSchema.PMT_TargetPK);
		}

		public static ZQuery GetTemporaryIgnoresAsMaster(ZDBOnlyQuery queryBase, BusinessObject bizo)
		{
			return GetIgnoresAsMaster(queryBase, bizo, PatternMatchingResult.StatusCodes.TemporaryIgnore, PatternMatchingResultSchema.PMT_MasterPK, PatternMatchingResultSchema.PMT_TargetPK);
		}

		public static ZQuery GetPermanentIgnoresAsTarget(ZDBOnlyQuery queryBase, BusinessObject bizo)
		{
			return GetIgnoresAsMaster(queryBase, bizo, PatternMatchingResult.StatusCodes.PermanentIgnore, PatternMatchingResultSchema.PMT_TargetPK, PatternMatchingResultSchema.PMT_MasterPK);
		}

		public static ZQuery GetTemporaryIgnoresAsTarget(ZDBOnlyQuery queryBase, BusinessObject bizo)
		{
			return GetIgnoresAsMaster(queryBase, bizo, PatternMatchingResult.StatusCodes.TemporaryIgnore, PatternMatchingResultSchema.PMT_TargetPK, PatternMatchingResultSchema.PMT_MasterPK);
		}

		public static ZQuery GetExlusionStatus(ZDBOnlyQuery queryBase, BusinessObject bizo)
		{
			var subQueryForExc = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_MasterPK);

			subQueryForExc.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix);
			subQueryForExc.AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.Excluded);
			queryBase.AddSubQuery(subQueryForExc, JoinCondition.And);

			return queryBase;
		}

		public static ZQuery GetRelatedPartyAsChildren(ZDBOnlyQuery queryBase, Guid masterPK)
		{
			var queryRelatedParty = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);

			queryRelatedParty.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, masterPK);
			queryBase.AddSubQuery(queryRelatedParty, JoinCondition.And);

			return queryBase;
		}

		public static ZQuery GetRelatedPartyAsParent(ZDBOnlyQuery queryBase, Guid masterPK)
		{
			var queryRelatedPartyReversed = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_RelatedParty);

			queryRelatedPartyReversed.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, masterPK);
			queryBase.AddSubQuery(queryRelatedPartyReversed, JoinCondition.And);

			return queryBase;
		}
	}
}
