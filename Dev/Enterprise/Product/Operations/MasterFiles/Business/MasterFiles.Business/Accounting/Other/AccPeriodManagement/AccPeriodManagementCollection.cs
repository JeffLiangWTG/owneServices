using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccPeriodManagementCollection : BusinessObjectCollection<AccPeriodManagement>
	{
		public AccPeriodManagementCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccPeriodManagementCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Env.CurrentCompany.PK);
			return result;
		}
	}
}
