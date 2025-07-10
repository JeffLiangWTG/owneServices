using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	public static class MergeHelper
	{
		public static IEnumerable<OrgMergeParameterisedSqlProvider> GetAllParameterisedSqlProviders()
		{
			return new List<OrgMergeParameterisedSqlProvider>()
			{
				new MergeJobExchangeRatesSqlProvider()
			};
		}

		public static IOrderedEnumerable<CoreMergeAction> GetAllCoreActions(OrganisationMergeData mergeData)
		{
			return new List<CoreMergeAction>()
			{
				new ParameterisedSqlMergeAction(mergeData)
			}.OrderBy(a => a);
		}
	}
}
