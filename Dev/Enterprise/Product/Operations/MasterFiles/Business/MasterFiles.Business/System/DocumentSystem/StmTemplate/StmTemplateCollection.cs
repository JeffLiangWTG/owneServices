using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmTemplateCollection : BusinessObjectCollection<StmTemplate>
	{
		public StmTemplateCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public StmTemplateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();
			filter.AddToFilter(StmTemplateSchema.SO_TemplateType, SQLComparisonOperator.Equal, StmTemplateTypes.Codes.Document);
			return filter;
		}
	}
}
