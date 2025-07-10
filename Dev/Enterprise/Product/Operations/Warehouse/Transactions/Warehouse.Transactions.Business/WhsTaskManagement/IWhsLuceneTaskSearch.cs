using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using GlowIndexQueryService.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsLuceneTaskSearch
	{
		GlowIndexQueryResultCollection QueryLuceneForTasks(
			string reference,
			IGlbStaff staff,
			WhsRFRegistry userRegistry,
			WhsWarehouse warehouse,
			string[] formFlowTypesToConsider,
			Guid[] tasksToIgnore);
	}
}
