using System.Collections.Generic;
using Enterprise.Freight.CFS.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Tracking.Business
{
	public class TrackingCFSShipmentFilterStripBusinessObject : ShipmentReceivalFilterStrip
	{
		protected override List<IFilterStripsHelper> AddCustomFilterStripsHelper()
		{
			return new List<IFilterStripsHelper>
			{
				new WorkflowFilterStripsHelper(typeof(TrackingCFSShipment), WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, Factory)
			};
		}
	}
}
