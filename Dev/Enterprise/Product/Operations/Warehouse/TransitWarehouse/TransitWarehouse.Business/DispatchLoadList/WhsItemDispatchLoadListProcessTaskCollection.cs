using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchLoadListProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemDispatchLoadListProcessTaskCollection(WhsItemDispatchLoadList header)
			: base(header)
		{
		}

		public new WhsItemDispatchLoadListProcessTask this[int index]
		{
			get { return (WhsItemDispatchLoadListProcessTask)Elements[index]; }
		}

		public new WhsItemDispatchLoadListProcessTask AddNew()
		{
			return (WhsItemDispatchLoadListProcessTask)base.AddNew();
		}
	}
}
