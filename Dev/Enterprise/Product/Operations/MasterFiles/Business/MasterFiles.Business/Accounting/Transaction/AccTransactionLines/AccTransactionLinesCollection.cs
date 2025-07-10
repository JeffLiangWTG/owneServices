using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLinesCollection : BusinessObjectCollection<AccTransactionLines>
	{
		public AccTransactionLinesCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}

		public AccTransactionLinesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery relationshipFilter = base.CreateRelationshipFilter();
			relationshipFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			return relationshipFilter;
		}
	}
}