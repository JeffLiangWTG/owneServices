using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailProcessTask : ProcessTask, IGateTransportCFSDetailProcessTask
	{
		public GateTransportCFSDetailProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(GateTransportCFSDetail);

		public new GateTransportCFSDetail Parent => (GateTransportCFSDetail)base.Parent;
	}
}
