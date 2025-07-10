using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemCycleCountLocationCreator : IWhsItemCycleCountLocationCreator
	{
		public IEnumerable<WhsItemCycleCountLocation> CreateCycleCountLocations(BusinessObjectFactory factory, IEnumerable<WhsItemCycleCountLocationInfo> cycleCountLocationInfos)
		{
			Argument.NotNull(factory, nameof(factory));

			var query = new ZDBOnlyQuery(typeof(WhsItemCycleCountLocation));
			query.AddToFilter(WhsItemCycleCountLocationSchema.WIC_EndTime, null);
			query.AddToFilter(JoinCondition.Or, WhsItemCycleCountLocationSchema.WIC_Status, new string[] { CycleCountLocationStatuses.Codes.Error, CycleCountLocationStatuses.Codes.ProcessVariance });

			var varianceSubQuery = new ZDBOnlySubQuery(typeof(WhsItemCycleCountLocationVariance), WhsItemCycleCountLocationVarianceSchema.WIV_WIC_CycleCountLocation);
			varianceSubQuery.AddToFilter(WhsItemCycleCountLocationVarianceSchema.WIV_Status, CycleCountVarianceStatuses.Codes.Open);
			query.AddSubQuery(varianceSubQuery, JoinCondition.Or);
			query.AddToFilter(WhsItemCycleCountLocationSchema.WIC_WL_Location, cycleCountLocationInfos.Select(c => c.LocationPK));

			var existingCycleCountLocPKs = factory.Load<WhsItemCycleCountLocation>(query).Select(c => c.WIC_WL_Location).ToHashSet();

			var createdCycleCounts = new List<WhsItemCycleCountLocation>();
			foreach (var cycleCountLocInfo in cycleCountLocationInfos.Where(cl => !existingCycleCountLocPKs.Contains(cl.LocationPK)).ToArray())
			{
				var newCount = factory.New<WhsItemCycleCountLocation>();
				newCount.WIC_WL_Location = cycleCountLocInfo.LocationPK;
				newCount.WIC_Priority = cycleCountLocInfo.Priority;
				createdCycleCounts.Add(newCount);
			}

			return createdCycleCounts;
		}
	}
}
