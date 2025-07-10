using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingProfitShareRedistributionCollection : BusinessObjectCollection<ForwardingProfitShareRedistribution>
	{
		public ForwardingProfitShareRedistributionCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ForwardingProfitShareRedistributionCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(GetCurrentCompanyFilter());

			return query;
		}

		ZQuery GetCurrentCompanyFilter()
		{
			var query = new ZQuery();
			query.DefaultJoinCondition = JoinCondition.Or;
			query.AddToFilter(ProfitShareRedistributionSchema.PSR_GC_Company, DBNull.Value);
			query.AddToFilter(ProfitShareRedistributionSchema.PSR_GC_Company, GlbCompany.CurrentCompany.PK);

			return query;
		}
	}
}
