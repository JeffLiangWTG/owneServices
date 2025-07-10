using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Module
{
	public static class WorkflowModuleFilterQueryBuilder
	{
		public static ZDBOnlyQuery BuildQuery(Type businessObjectType, ZDBOnlySubQuery milestoneQuery, ZDBOnlySubQuery[] relatedParentSubQueries, SchemaColumn milestoneParentColumnOverride = null)
		{
			var resultQuery = new ZDBOnlyQuery(businessObjectType);

			if (!milestoneQuery.IsEmpty)
			{
				if (relatedParentSubQueries == null || relatedParentSubQueries.Length == 0)
				{
					AddQueriesWithColumnOverride(resultQuery, milestoneQuery, milestoneParentColumnOverride);
				}
				else
				{
					var milestoneParentQuery = (ZDBOnlySubQuery)relatedParentSubQueries[0].DeepClone();
					AddQueriesWithColumnOverride(milestoneParentQuery, milestoneQuery, milestoneParentColumnOverride);

					var lastSubQuery = milestoneParentQuery;
					for (int i = 1; i < relatedParentSubQueries.Length; i++)
					{
						var joiningQuery = (ZDBOnlySubQuery)relatedParentSubQueries[i].DeepClone();
						joiningQuery.AddSubQuery(lastSubQuery, JoinCondition.And);
						lastSubQuery = joiningQuery;
					}

					resultQuery.AddSubQuery(lastSubQuery, JoinCondition.And);
				}
			}

			return resultQuery;
		}

		static void AddQueriesWithColumnOverride(ZDBOnlyQuery parentQuery, ZDBOnlySubQuery milestoneQuery, SchemaColumn milestoneParentColumnOverride)
		{
			if (milestoneParentColumnOverride == null)
			{
				parentQuery.AddSubQuery(milestoneQuery, JoinCondition.And);
			}
			else
			{
				parentQuery.AddSubQuery(milestoneParentColumnOverride, milestoneQuery, JoinCondition.And);
			}
		}
	}
}
