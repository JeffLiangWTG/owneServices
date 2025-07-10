using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RecentActivityDateFilter))]
	sealed class RecentActivityDateFilterTest : RecentActivityDateFilterTestCase<RecentActivityDateFilter>
	{
		#region Implementation

		protected override RecentActivityDateFilter GetNewModuleFilter()
		{
			var filter = new RecentActivityDateFilter("moo", OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			filter.SubGroup = new SalesRelationActivityFilterHelper.SalesRelationNodeSubGroup(OrgOpportunitySchema.Instance, typeof(OrgOpportunity));
			return filter;
		}

		#endregion
	}
}
