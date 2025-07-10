using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemPackageStateProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemPackageStateProcessTaskCollection(WhsItemPackageState parent) : base(parent)
		{
		}

		public new WhsItemPackageStateProcessTask this[int index] => (WhsItemPackageStateProcessTask)Elements[index];

		public new WhsItemPackageStateProcessTask AddNew() => (WhsItemPackageStateProcessTask)base.AddNew();
	}
}
