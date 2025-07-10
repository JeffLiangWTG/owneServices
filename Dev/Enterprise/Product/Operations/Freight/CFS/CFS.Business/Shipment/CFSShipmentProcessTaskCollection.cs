using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentProcessTaskCollection : ProcessTaskCollection
	{
		public CFSShipmentProcessTaskCollection(CFSShipment shipment)
			: base(shipment)
		{
		}

		public new CFSShipment Parent
		{
			get { return (CFSShipment)base.Parent; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			// HACK: If the shipment is BOTH CFS and Forwarding registered, we have no way of knowing which ProcessTask type to load.
			//		 Currently we are hiding the Workflow Tab on the Shipment Receival Form so the workflow items will only be visible on the Forwarding Shipment screen.
			query.IsNoResultQuery = Parent.JS_IsForwardRegistered;
			// END HACK
			return query;
		}

		public new CFSShipmentProcessTask this[int index]
		{
			get { return (CFSShipmentProcessTask)Elements[index]; }
		}

		public new CFSShipmentProcessTask AddNew()
		{
			return (CFSShipmentProcessTask)base.AddNew();
		}
	}
}
