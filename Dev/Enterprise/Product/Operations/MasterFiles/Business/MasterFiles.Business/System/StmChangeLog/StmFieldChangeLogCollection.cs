using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmFieldChangeLogCollection : NonPersistentBusinessObjectCollection<StmFieldChangeLog>
	{
		public StmFieldChangeLogCollection(StmChangeLog parent)
			: base(parent.Factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StmFieldChangeLog(Factory);
		}
	}
}
