using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchTransportationUnitProcessTaskCollection : ProcessTaskCollection
	{
		public WhsItemDispatchTransportationUnitProcessTaskCollection(WhsItemDispatchTransportationUnit header)
			: base(header)
		{
		}

		public new WhsItemDispatchTransportationUnitProcessTask this[int index]
		{
			get { return (WhsItemDispatchTransportationUnitProcessTask)Elements[index]; }
		}

		public new WhsItemDispatchTransportationUnitProcessTask AddNew()
		{
			return (WhsItemDispatchTransportationUnitProcessTask)base.AddNew();
		}
	}
}