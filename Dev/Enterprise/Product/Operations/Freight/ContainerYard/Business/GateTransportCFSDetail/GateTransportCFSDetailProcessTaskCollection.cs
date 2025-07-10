using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailProcessTaskCollection : ProcessTaskCollection
	{
		public GateTransportCFSDetailProcessTaskCollection(GateTransportCFSDetail parent)
			: base(parent)
		{
		}

		public new GateTransportCFSDetail Parent => (GateTransportCFSDetail)base.Parent;

		public new GateTransportCFSDetailProcessTask this[int index] => (GateTransportCFSDetailProcessTask)Elements[index];

		public new GateTransportCFSDetailProcessTask AddNew() => (GateTransportCFSDetailProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new GateTransportCFSDetailProcessTaskCollection(Parent);
	}
}
