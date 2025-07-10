using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AlternateChartofAccounts)]
	public class AccAlternateChartCollection : BusinessObjectCollection<AccAlternateChart>
	{
		public AccAlternateChartCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccAlternateChartCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AccAlternateChartCollection(BusinessObjectFactory factory, ZQuery sQLFilter, Guid companyPK) : base(factory, sQLFilter)
		{
			this.companyPK = companyPK;
		}

		readonly Guid companyPK = Env.CurrentCompany.PK;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery(AccAlternateChartSchema.AAC_GC_Company, null);
			if (companyPK == Guid.Empty)
			{
				query.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, GlbCompany.CurrentCompany.PK);
			}
			else
			{
				query.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, companyPK);
			}

			return query;
		}
	}
}
