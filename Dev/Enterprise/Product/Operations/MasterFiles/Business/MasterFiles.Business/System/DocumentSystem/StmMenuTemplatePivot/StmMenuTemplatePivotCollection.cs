using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmMenuTemplatePivotCollection : BusinessObjectCollection<StmMenuTemplatePivot>
	{
		public StmMenuTemplatePivotCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public StmMenuTemplatePivotCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
