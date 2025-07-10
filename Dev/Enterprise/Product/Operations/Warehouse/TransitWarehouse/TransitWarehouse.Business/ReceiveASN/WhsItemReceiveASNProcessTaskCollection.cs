using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveASNProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemReceiveASNProcessTaskCollection(WhsItemReceiveASN receiveASN)
			: base(receiveASN)
		{
		}

		public new WhsItemReceiveASNProcessTask this[int index]
		{
			get { return (WhsItemReceiveASNProcessTask)Elements[index]; }
		}

		public new WhsItemReceiveASNProcessTask AddNew()
		{
			return (WhsItemReceiveASNProcessTask)base.AddNew();
		}
	}
}