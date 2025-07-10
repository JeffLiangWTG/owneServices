using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class ScheduleChangeParentExtentions
	{
		public static ZQuery GetSailingFilter(this IEnumerable<JobScheduleChange> changes)
		{
			Dictionary<SchemaGuidColumn, List<ZGuid>> lookup = new Dictionary<SchemaGuidColumn, List<ZGuid>>();
			List<ZGuid> list;

			foreach (JobScheduleChange change in changes)
			{
				IScheduleChangeParent parent = change.Parent;

				if (parent != null)
				{
					if (!lookup.TryGetValue(parent.SailingRefColumn, out list))
					{
						list = new List<ZGuid>();
						lookup.Add(parent.SailingRefColumn, list);
					}

					list.Add(parent.PK);
				}
			}

			if (lookup.Count == 0)
			{
				return ZQuery.NoResultQuery;
			}
			else
			{
				ZQuery result = new ZQuery();
				result.DefaultJoinCondition = JoinCondition.Or;

				foreach (KeyValuePair<SchemaGuidColumn, List<ZGuid>> pair in lookup)
				{
					result.AddToFilter(pair.Key, pair.Value);
				}

				return result;
			}
		}
	}
}
