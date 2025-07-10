using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveTransportationUnitProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemReceiveTransportationUnitProcessTaskCollection(WhsItemReceiveTransportationUnit header)
			: base(header)
		{
		}

		public new WhsItemReceiveTransportationUnitProcessTask this[int index]
		{
			get { return (WhsItemReceiveTransportationUnitProcessTask)Elements[index]; }
		}

		public new WhsItemReceiveTransportationUnitProcessTask AddNew()
		{
			return (WhsItemReceiveTransportationUnitProcessTask)base.AddNew();
		}
	}
}